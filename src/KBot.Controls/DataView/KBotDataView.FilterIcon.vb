Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' PICTOGRAMA DE FILTRARE din antetul <see cref="KBotDataView"/> (slice 0028-03) — butonul care
''' desfășoară meniul de sortare și filtrare al coloanei, ca săgeata din foaia de date Access.
'''
''' <para><b>Se hotărăște PE COLOANĂ, în designer.</b> Steagul și înfățișarea butonului stau pe
''' <see cref="KBotDataColumn"/> (<c>ShowColumnFilter</c>, <c>ColumnFilterIcon</c>,
''' <c>ColumnFilterIconSize</c>, <c>ColumnFilterHoverColor</c>), lângă celelalte pictograme de
''' antet ale coloanei; aici rămâne doar ce ține de GRILĂ — așezarea, hit-testul, pictarea și
''' deschiderea meniului. Pe coloanele <see cref="KBotColumnType.Button"/> și
''' <see cref="KBotColumnType.ProgressBar"/> filtrarea nu se poate aprinde deloc (vezi
''' <see cref="KBotDataColumn.ShowColumnFilter"/>).</para>
'''
''' <para><b>Stă mereu în același loc</b> (capătul din dreapta al celulei de antet), iar
''' <see cref="KBotDataColumn.HeaderRightIcon"/> se mută la stânga lui. Vezi
''' <c>ComputeHeaderCellLayout</c>: nici unul, nici celălalt nu se sacrifică la îngustare, fiindcă
''' amândouă se apasă.</para>
'''
''' <para><b>Fără imagine dată, se DESENEAZĂ.</b> <c>ColumnFilterIcon</c> lăsat gol înseamnă o
''' pâlnie trasată din culoarea temei — plină și în accent cât timp coloana chiar are un filtru
''' așezat, goală altfel. Așa antetul spune dintr-o privire care coloane sunt filtrate, ceea ce e
''' jumătate din rostul semnului; și așa controlul nu are nevoie de nicio resursă imagine ca să
''' arate corect în orice schemă.</para>
''' </summary>
Partial Class KBotDataView

    ' Coloana al cărei buton de filtru e sub cursor (pe CHEIE, ca la pictogramele de antet:
    ' o coloană poate fi înlocuită în colecție între două mișcări de mouse).
    Private _hotFilterKey As String = Nothing

    ''' <summary>
    ''' Meniul de filtrare al unei coloane e pe cale să se deschidă. Gazda poate să-l oprească
    ''' (<c>Cancel</c>) — de pildă o vedere care își filtrează singură datele pe server.
    ''' </summary>
    Public Event ColumnFilterOpening As EventHandler(Of KBotColumnFilterOpeningEventArgs)

    ''' <summary>Vreo coloană poartă butonul de filtrare? (Hit-testul se oprește din asta.)</summary>
    Friend Function AnyColumnFilterShown() As Boolean
        For Each col In _columns
            If col.FilterEnabled Then Return True
        Next
        Return False
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' GEOMETRIE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Mărimea butonului de filtrare pe o anumită coloană — goală dacă acea coloană nu-l poartă
    ''' (steagul stins, tip interzis, sau coloană lipsă).
    ''' </summary>
    Friend Function FilterIconSizeFor(col As KBotDataColumn) As Size
        If col Is Nothing OrElse Not col.FilterEnabled Then Return Size.Empty
        Return col.ColumnFilterIconSize
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTARE (acoperită tranzitiv de Try-ul din OnPaint)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Desenează pictograma de filtrare a unei coloane. Fundalul de hover merge dedesubt, ca la
    ''' pictograma din dreapta: e un buton, deci trebuie să se vadă că răspunde.
    ''' </summary>
    Private Sub DrawColumnFilterIcon(g As Graphics, col As KBotDataColumn, iconRect As Rectangle)
        If iconRect.IsEmpty Then Return

        If IsFilterIconHot(col) Then
            Using b As New SolidBrush(FilterIconHoverResolved(col))
                Using path As GraphicsPath = RoundedRect(Rectangle.Inflate(iconRect, 3, 3), ScaleDpi(3))
                    g.FillPath(b, path)
                End Using
            End Using
        End If

        If col.ColumnFilterIcon IsNot Nothing Then
            g.DrawImage(col.ColumnFilterIcon, iconRect)
            Return
        End If

        DrawFunnel(g, iconRect, HasColumnFilter(col.Key))
    End Sub

    ''' <summary>
    ''' Pâlnia desenată — semnul implicit. PLINĂ, în accent, când coloana chiar e filtrată; doar
    ''' conturată, în culoarea antetului, altfel. Diferența e citită dintr-o privire pe un antet cu
    ''' zece coloane, ceea ce o listă de proprietăți nu e.
    ''' </summary>
    Private Sub DrawFunnel(g As Graphics, r As Rectangle, filtrat As Boolean)
        ' Se desenează într-un pătrat centrat, ca pâlnia să nu se deformeze pe o mărime ne-pătrată.
        Dim latura As Integer = Math.Max(6, Math.Min(r.Width, r.Height))
        Dim x As Integer = r.Left + (r.Width - latura) \ 2
        Dim y As Integer = r.Top + (r.Height - latura) \ 2

        ' Punctele pâlniei, în fracțiuni de latură: gura sus, gâtul la mijloc, tija în jos.
        Dim gura As Single = latura * 0.1F
        Dim gat As Single = latura * 0.45F
        Dim baza As Single = latura * 0.9F
        Dim mijloc As Single = x + latura / 2.0F
        Dim tija As Single = latura * 0.11F

        Dim puncte As PointF() = {
            New PointF(x + gura, y + gura),
            New PointF(x + latura - gura, y + gura),
            New PointF(mijloc + tija, y + gat),
            New PointF(mijloc + tija, y + baza),
            New PointF(mijloc - tija, y + baza - latura * 0.12F),
            New PointF(mijloc - tija, y + gat)}

        Dim vechiu As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            If filtrat Then
                Using b As New SolidBrush(_cHeaderBaseline)
                    g.FillPolygon(b, puncte)
                End Using
            Else
                Using p As New Pen(HeaderForeResolved(), 1.2F)
                    g.DrawPolygon(p, puncte)
                End Using
            End If
        Finally
            g.SmoothingMode = vechiu
        End Try
    End Sub

    ' Coloana e cea survolată? Comparație pe cheie (vezi _hotFilterKey).
    Private Function IsFilterIconHot(col As KBotDataColumn) As Boolean
        If _hotFilterKey Is Nothing OrElse col Is Nothing Then Return False
        Return String.Equals(_hotFilterKey, col.Key, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Culoarea de hover a butonului de filtrare: cea fixată pe coloană, altfel o spălare din
    ''' culoarea de text a antetului — adică din TEMĂ, ca peste tot.
    ''' </summary>
    Friend Function FilterIconHoverResolved(col As KBotDataColumn) As Color
        If col IsNot Nothing AndAlso col.ColumnFilterHoverColor <> Color.Empty Then
            Return col.ColumnFilterHoverColor
        End If
        Return Color.FromArgb(40, HeaderForeResolved())
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' HIT-TEST + HOVER (chemate din partiala .Input)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Coloana a cărei pictogramă de filtrare e sub punct (Nothing = niciuna), plus dreptunghiul
    ''' ei. Banda înghețată se caută prima, fiindcă ea se pictează PESTE cea derulată.
    ''' </summary>
    Friend Function FilterIconTarget(pt As Point, ByRef iconRect As Rectangle) As KBotDataColumn
        iconRect = Rectangle.Empty
        If Not AnyColumnFilterShown() Then Return Nothing
        If Not _showHeader OrElse _headerHeight <= 0 Then Return Nothing
        If pt.Y < 0 OrElse pt.Y >= _headerHeight Then Return Nothing

        For Each cl In _frozenLayout
            Dim r As Rectangle = HeaderLayoutFor(cl.Column, New Rectangle(cl.X, 0, cl.Column.Width, _headerHeight)).FilterIcon
            If Not r.IsEmpty AndAlso r.Contains(pt) Then
                iconRect = r
                Return cl.Column
            End If
        Next

        ' Sub banda înghețată nu se mai caută: acolo coloanele derulate sunt acoperite.
        If pt.X < _frozenBandWidth OrElse pt.X >= ViewportWidth() Then Return Nothing

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            Dim r As Rectangle = HeaderLayoutFor(cl.Column,
                New Rectangle(_frozenBandWidth + cl.X - hOffset, 0, cl.Column.Width, _headerHeight)).FilterIcon
            If Not r.IsEmpty AndAlso r.Contains(pt) Then
                iconRect = r
                Return cl.Column
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Actualizează hover-ul pictogramei de filtrare. True dacă punctul e chiar peste una —
    ''' atunci apelantul pune cursorul de mână și nu mai caută nimic altceva acolo.
    ''' </summary>
    Friend Function UpdateFilterIconHover(pt As Point) As Boolean
        Dim r As Rectangle = Rectangle.Empty
        Dim col As KBotDataColumn = FilterIconTarget(pt, r)
        Dim cheie As String = If(col Is Nothing, Nothing, col.Key)
        If Not String.Equals(cheie, _hotFilterKey, StringComparison.Ordinal) Then
            _hotFilterKey = cheie
            Invalidate()
        End If
        Return col IsNot Nothing
    End Function

    ''' <summary>Stinge hover-ul pictogramei de filtrare (cursorul a plecat din control).</summary>
    Friend Sub ClearFilterIconHover()
        If _hotFilterKey Is Nothing Then Return
        _hotFilterKey = Nothing
        Invalidate()
    End Sub

    ''' <summary>
    ''' Deschide meniul de filtrare al unei coloane, ca și cum s-ar fi apăsat pictograma ei.
    ''' Public: o gazdă poate lega aceeași comandă de un buton propriu sau de o tastă.
    ''' </summary>
    Public Sub ShowColumnFilterMenu(colKey As String)
        Try
            Dim col As KBotDataColumn = Column(colKey)      ' cheie necunoscută => ArgumentException
            RecalcColumnLayout()
            Dim r As Rectangle = DebugFilterIconRect(colKey)
            ' Fără pictogramă pe ecran (coloana e ascunsă, derulată afară sau exclusă), meniul se
            ' deschide sub colțul din stânga-sus al antetului: tot trebuie să se vadă undeva.
            If r.IsEmpty Then r = New Rectangle(0, 0, 1, Math.Max(1, _headerHeight))
            OpenColumnFilterMenu(col, r)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ShowColumnFilterMenu", ex)
            Throw
        End Try
    End Sub

    ' Construiește și arată meniul. Boundary UI (vine dintr-un click): loghează + înghite — o
    ' fereastră care nu s-a putut deschide nu are voie să dărâme grila de sub ea.
    Private Sub OpenColumnFilterMenu(col As KBotDataColumn, iconRect As Rectangle)
        Try
            Dim args As New KBotColumnFilterOpeningEventArgs(col.Key, iconRect)
            RaiseEvent ColumnFilterOpening(Me, args)
            If args.Cancel Then Return

            Dim meniu As New KBotFilterPopup(col.Key,
                                             If(col.HeaderText, col.Key),
                                             col.ValueType,
                                             DistinctDisplayValues(col.Key),
                                             ColumnFilter(col.Key),
                                             SortDirectionFor(col.Key))

            AddHandler meniu.FilterAccepted,
                Sub(s As Object, e As KBotFilterAcceptedEventArgs) SetColumnFilter(e.Filter)
            AddHandler meniu.SortRequested,
                Sub(s As Object, e As KBotSortRequestedEventArgs) ApplySort(e.ColumnKey, e.Direction)

            ' Arătat NEMODAL, deci WinForms îl eliberează singur la închidere — vezi CustomPopup:
            ' un «Using» aici l-ar distruge înainte să-l vadă cineva.
            meniu.ShowBelow(Me, iconRect)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OpenColumnFilterMenu", ex)
        End Try
    End Sub

    ' Sensul de sortare al unei coloane ANUME: None dacă sortarea stă pe altă coloană — meniul
    ' arată bifa numai pe coloana lui, nu pe cea sortată din altă parte.
    Private Function SortDirectionFor(colKey As String) As KBotSortDirection
        If Not String.Equals(SortColumnKey, colKey, StringComparison.Ordinal) Then Return KBotSortDirection.None
        Return SortDirection
    End Function

    ''' <summary>
    ''' Dreptunghiul pictogramei de filtrare a unei coloane, în coordonate client (gol = nu se
    ''' vede). Friend: poarta de verificare headless a testelor — hit-testul nu se poate proba cu
    ''' mouse-ul.
    ''' </summary>
    Friend Function DebugFilterIconRect(colKey As String) As Rectangle
        RecalcColumnLayout()
        For Each cl In _frozenLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return HeaderLayoutFor(cl.Column, New Rectangle(cl.X, 0, cl.Column.Width, _headerHeight)).FilterIcon
            End If
        Next
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return HeaderLayoutFor(cl.Column,
                    New Rectangle(_frozenBandWidth + cl.X - hOffset, 0, cl.Column.Width, _headerHeight)).FilterIcon
            End If
        Next
        Return Rectangle.Empty
    End Function

    ''' <summary>
    ''' Apăsare peste pictograma de filtrare. True dacă a fost consumată — atunci grila nu mai
    ''' pornește o redimensionare și nu mai mută selecția.
    ''' </summary>
    Friend Function HandleFilterIconMouseDown(pt As Point) As Boolean
        Dim r As Rectangle = Rectangle.Empty
        Dim col As KBotDataColumn = FilterIconTarget(pt, r)
        If col Is Nothing Then Return False
        ' În designer pictograma se VEDE, dar nu deschide nimic: un meniu deschis peste suprafața
        ' de design ar fura click-urile de la unealta de proiectare.
        If Not KBotDesignTime.IsDesignTime(Me) Then OpenColumnFilterMenu(col, r)
        Return True
    End Function

End Class

''' <summary>
''' Argumentele lui <c>KBotDataView.ColumnFilterOpening</c> (slice 0028-03): cheia coloanei,
''' dreptunghiul pictogramei apăsate (ca gazda să-și poată așeza propriul meniu sub ea) și
''' portița de anulare.
''' </summary>
Public Class KBotColumnFilterOpeningEventArgs
    Inherits CancelEventArgs

    Public Sub New(columnKey As String, iconRectangle As Rectangle)
        Me.ColumnKey = columnKey
        Me.IconRectangle = iconRectangle
    End Sub

    ''' <summary>Cheia coloanei al cărei meniu de filtrare se deschide.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>Dreptunghiul pictogramei apăsate, în coordonate CLIENT ale grilei.</summary>
    Public ReadOnly Property IconRectangle As Rectangle

End Class
