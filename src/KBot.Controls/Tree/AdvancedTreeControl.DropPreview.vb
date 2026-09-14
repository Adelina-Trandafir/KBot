Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' UNDE AR CĂDEA RÂNDUL TRAS (previzualizarea aruncării).
'''
''' <para><b>Problema.</b> Într-un arbore unde locul unui rând NU e ales de operator — lanțul unei
''' recepții e ordonat după ora instantaneului, nu după unde nimerește mouse-ul — chenarul de pe
''' rândul de sub cursor spune o minciună: sugerează «aici, unde arăt eu». Operatorul află abia
''' după aruncare că rândul s-a dus în altă parte a lanțului, și nu are cum să lege ce-a văzut
''' de ce-a ieșit.</para>
'''
''' <para><b>Răspunsul.</b> Cât ține tragerea, arborele arată chiar rezultatul: rădăcina care ar
''' primi se aprinde întreagă, iar în ea apare un rând-fantomă exact pe locul pe care l-ar lua
''' rândul tras. Nu e o etichetă care descrie mutarea, e mutarea desenată.</para>
'''
''' <para><b>Cine hotărăște locul.</b> NU controlul. Ordinea aparține datelor (ora din
''' <c>DataH</c>, în cazul recepțiilor), iar arborele nu știe nimic despre ele. Gazda calculează
''' pozițiile și le trimite prin <see cref="SetDropPreview"/>; controlul doar le desenează. Aceeași
''' împărțire ca la vetourile tragerii: arborele arată, gazda hotărăște.</para>
'''
''' <para><b>Fantomele sunt rânduri adevărate, dar de o clipă.</b> Se bagă în
''' <c>Children</c>-ul rădăcinii, ca să treacă prin aceeași pictură, aceeași derulare și aceeași
''' măsurătoare ca restul — o pictură separată «pe deasupra» s-ar despărți de rânduri la prima
''' schimbare de înălțime sau de derulare. Ies TOATE pe același drum, prin
''' <see cref="ClearDropPreview"/>, chemat din <c>CancelDrag</c>: aruncare, ESC și ieșirea din
''' fereastră trec toate pe acolo.</para>
''' </summary>
Partial Public Class AdvancedTreeControl

    ' Rădăcina aprinsă și rândurile-fantomă băgate în ea. Toate Nothing/goale în afara tragerii.
    Private _previewRoot As TreeItem = Nothing
    Private ReadOnly _previewRows As New List(Of TreeItem)()

    ' Semnătura previzualizării de pe ecran. Fără ea, fiecare pixel de mișcare a mouse-ului ar
    ' desface și ar reface aceleași rânduri — adică o clipire continuă pe tot timpul tragerii.
    Private _previewKey As String = String.Empty

    ''' <summary>Rădăcina aprinsă acum de previzualizare, sau <c>Nothing</c>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DropPreviewRoot As TreeItem
        Get
            Return _previewRoot
        End Get
    End Property

    ''' <summary>
    ''' Arată rezultatul aruncării: <paramref name="root"/> aprinsă și câte un rând-fantomă pe
    ''' fiecare poziție din <paramref name="ghosts"/>.
    ''' </summary>
    ''' <param name="root">Rădăcina care ar primi rândurile. <c>Nothing</c> = stinge tot.</param>
    ''' <param name="ghosts">
    ''' Pozițiile, socotite în lanțul REZULTAT (cu fantomele în el), în ordine crescătoare. Așa
    ''' scrise, se bagă una după alta fără ca gazda să mai țină socoteala deplasărilor.
    ''' </param>
    Public Sub SetDropPreview(root As TreeItem, ghosts As IEnumerable(Of TreeDropGhost))
        Try
            If root Is Nothing OrElse ghosts Is Nothing Then
                ClearDropPreview()
                Return
            End If

            Dim lista As New List(Of TreeDropGhost)()
            For Each g As TreeDropGhost In ghosts
                If g IsNot Nothing Then lista.Add(g)
            Next
            If lista.Count = 0 Then
                ClearDropPreview()
                Return
            End If
            lista.Sort(Function(a, b) a.Index.CompareTo(b.Index))

            Dim cheie As New System.Text.StringBuilder()
            cheie.Append(If(root.Key, String.Empty))
            For Each g As TreeDropGhost In lista
                cheie.Append("|"c).Append(g.Index).Append(":"c).Append(If(g.Caption, String.Empty))
            Next
            Dim cheieNoua As String = cheie.ToString()
            If cheieNoua = _previewKey AndAlso _previewRoot Is root Then Return

            ScoateFantomele()

            For Each g As TreeDropGhost In lista
                Dim poz As Integer = Math.Max(0, Math.Min(g.Index, root.Children.Count))
                Dim fantoma As New TreeItem With {
                    .Key = "GHOST_" & Guid.NewGuid().ToString(),
                    .Caption = If(g.Caption, String.Empty),
                    .Level = root.Level + 1,
                    .Parent = root,
                    .Italic = True,
                    .IsDropGhost = True,
                    .LeftIconClosed = g.Icon,
                    .NodeForeColor = DragHighlightColor
                }
                root.Children.Insert(poz, fantoma)
                _previewRows.Add(fantoma)
            Next

            ' O rădăcină strânsă ar ține fantomele ascunse, adică ar arăta exact nimic.
            root.Expanded = True
            _previewRoot = root
            _previewKey = cheieNoua

            ' Rândurile s-au înmulțit: bara de derulare are altceva de măsurat.
            RefreshScrollVisibility()
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.SetDropPreview", ex)
        End Try
    End Sub

    ''' <summary>Stinge previzualizarea. Sigură de chemat oricând, chiar dacă nu era niciuna.</summary>
    Public Sub ClearDropPreview()
        Try
            If _previewRoot Is Nothing AndAlso _previewRows.Count = 0 Then Return
            ScoateFantomele()
            _previewRoot = Nothing
            _previewKey = String.Empty
            RefreshScrollVisibility()
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ClearDropPreview", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Rândul pe care gazda trebuie să-l vadă în locul unei fantome: fantoma NU e un nod al
    ''' datelor, dar locul ei spune «rădăcina asta».
    ''' </summary>
    Friend Shared Function RandReal(it As TreeItem) As TreeItem
        If it Is Nothing Then Return Nothing
        If it.IsDropGhost Then Return it.Parent
        Return it
    End Function

    Private Sub ScoateFantomele()
        For Each fantoma As TreeItem In _previewRows
            Dim parinte As TreeItem = fantoma.Parent
            If parinte Is Nothing Then Continue For
            ' Reconstruirea arborelui poate să fi golit deja lista; scoaterea unui rând care nu
            ' mai e acolo nu e o eroare, e chiar starea în care voiam să ajungem.
            For i As Integer = parinte.Children.Count - 1 To 0 Step -1
                If parinte.Children(i) Is fantoma Then parinte.Children.RemoveAt(i)
            Next
        Next
        _previewRows.Clear()
    End Sub

    ''' <summary>Uită previzualizarea fără s-o desfacă — pentru <c>Clear</c>, unde rândurile pier oricum.</summary>
    Friend Sub ForgetDropPreview()
        _previewRows.Clear()
        _previewRoot = Nothing
        _previewKey = String.Empty
    End Sub

    ''' <summary>
    ''' Aprinde toată rădăcina care ar primi — rândul ei și tot ce atârnă vizibil sub el.
    ''' <para>Un chenar doar pe rândul-antet s-ar citi ca «pe rândul ăsta»; ce se spune aici e
    ''' «în recepția asta», iar recepția e blocul întreg.</para>
    ''' </summary>
    Private Sub DrawDropPreviewRoot(g As Graphics)
        If _previewRoot Is Nothing Then Return

        Dim vizibile As List(Of TreeItem) = GetVisibleItems()
        Dim primul As Integer = -1
        Dim ultimul As Integer = -1
        For i As Integer = 0 To vizibile.Count - 1
            Dim it As TreeItem = vizibile(i)
            Dim radacina As TreeItem = it
            While radacina.Parent IsNot Nothing
                radacina = radacina.Parent
            End While
            If radacina IsNot _previewRoot Then Continue For
            If primul < 0 Then primul = i
            ultimul = i
        Next
        If primul < 0 Then Return

        Dim headerOff As Integer = TotalHeaderOffset
        Dim zonaNoduri As Integer = Math.Max(0, Me.Height - headerOff - FooterOffset)
        If zonaNoduri <= 0 Then Return

        Dim sus As Integer = -_vScroll.Value + PaddingTreeTopPx + headerOff + primul * _itemHeight
        Dim jos As Integer = sus + (ultimul - primul + 1) * _itemHeight

        Dim oldClip As Region = g.Clip.Clone()
        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        Try
            g.SetClip(New Rectangle(0, headerOff, Me.Width, zonaNoduri))
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim latime As Integer = Math.Max(1, Me.Width - If(_vScroll.Visible, _vScroll.Width, 0) - SX(2))
            Dim r As New Rectangle(SX(1), sus, latime - SX(2), Math.Max(_itemHeight, jos - sus) - 1)
            Dim culoare As Color = DragHighlightColor

            Using umplere As New SolidBrush(Color.FromArgb(24, culoare))
                g.FillRectangle(umplere, r)
            End Using
            Using pen As New Pen(culoare, CSng(SY(2)))
                pen.Alignment = PenAlignment.Inset
                pen.DashStyle = DashStyle.Dash
                g.DrawRectangle(pen, r)
            End Using
        Finally
            g.SmoothingMode = oldSmooth
            g.Clip = oldClip
        End Try
    End Sub
End Class

''' <summary>
''' Un rând-fantomă: pe ce poziție a rădăcinii ar cădea rândul tras și ce scrie pe el.
''' </summary>
''' <remarks>
''' <see cref="Index"/> se socotește în lanțul REZULTAT — cel cu fantomele în el. Când se trag
''' trei rânduri deodată, gazda dă cele trei poziții din lista finală, nu trei poziții din lista
''' de acum ajustate una câte una.
''' </remarks>
Public NotInheritable Class TreeDropGhost
    Public Sub New(index As Integer, caption As String)
        Me.Index = index
        Me.Caption = caption
    End Sub

    ''' <summary>Poziția între copiii rădăcinii, în lanțul rezultat.</summary>
    Public ReadOnly Property Index As Integer

    ''' <summary>Ce scrie pe rândul-fantomă — de obicei chiar eticheta rândului tras.</summary>
    Public ReadOnly Property Caption As String

    ''' <summary>Iconița din stânga. <c>Nothing</c> = fără.</summary>
    Public Property Icon As Image
End Class
