Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Bancul de probă al lui <see cref="CustomPopup"/> — echivalentul lui <c>TreePlaygroundForm</c>
''' pentru meniu. Deschide același meniu în cele trei feluri (sub buton, la cursor, clic dreapta
''' oriunde), cu conținutul comutabil din casetele de sus, și scrie în jurnal ce a ales operatorul.
'''
''' Ce trebuie VĂZUT aici, fiindcă niciun test fără ecran nu poate spune:
''' <list type="bullet">
''' <item>literele de acces se subliniază, iar tasta alege rândul (sau, la mai multe potriviri,
''' doar mută evidențierea);</item>
''' <item>meniul se deschide DEJA pe rândul cerut, iar Enter îl confirmă fără nicio altă atingere;</item>
''' <item>culorile se schimbă odată cu schema — inclusiv rândul evidențiat și separatorii;</item>
''' <item>meniul se răstoarnă când e deschis lângă marginea ecranului și se închide la clic în afară.</item>
''' </list>
''' </summary>
Public NotInheritable Class PopupPlaygroundForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private ReadOnly _icons As New List(Of Image)()

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current   ' de restaurat la închidere (proba nu repersistă alegerea)
        InitializeComponent()
        BuildIcons()
    End Sub

    ' Restaurează schema activă dinainte de probă și eliberează pictogramele desenate aici
    ' (meniul nu le deține niciodată — vezi CustomPopupItem.Image).
    Private Sub OnClosedCleanup(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
            ThemeManager.SetScheme(_originalScheme)
        End If
        For Each img As Image In _icons
            img.Dispose()
        Next
        _icons.Clear()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        If lblActive IsNot Nothing Then lblActive.Text = "activ: " & ThemeManager.Current.Name
    End Sub

    ' ── Conținutul meniului ──────────────────────────────────────────────────────

    ''' <summary>
    ''' Meniul de probă, construit după casetele de sus. Textele poartă litera de acces («&amp;»),
    ''' iar primele două rânduri sunt amândouă pe S dinadins: așa se vede că a doua apăsare de S
    ''' MUTĂ evidențierea în loc să aleagă, și că Enter e cel care confirmă.
    ''' </summary>
    Private Function BuildItems() As List(Of CustomPopupItem)
        Dim cuIcoane As Boolean = chkImagini.Checked
        Dim items As New List(Of CustomPopupItem)()

        items.Add(New CustomPopupItem("save", "&Salvează", If(cuIcoane, _icons(0), Nothing)))
        items.Add(New CustomPopupItem("saveall", "&Salvează tot", If(cuIcoane, _icons(1), Nothing)))
        items.Add(New CustomPopupItem("open", "&Deschide angajamentul…", If(cuIcoane, _icons(2), Nothing)))
        If chkSeparatori.Checked Then items.Add(CustomPopupItem.Separator())
        items.Add(New CustomPopupItem("print", "&Tipărește", If(cuIcoane, _icons(3), Nothing)))
        items.Add(New CustomPopupItem("export", "&Exportă în Excel"))
        If chkDezactivat.Checked Then
            items.Add(New CustomPopupItem("lock", "&Blochează perioada") With {.Enabled = False})
        End If
        If chkSeparatori.Checked Then items.Add(CustomPopupItem.Separator())
        items.Add(New CustomPopupItem("cancel", "&Renunță"))

        If chkMulte.Checked Then
            For i As Integer = 1 To 40
                items.Add(New CustomPopupItem("r" & i, "Rând de probă " & i))
            Next
        End If
        Return items
    End Function

    ' Drumul comun: construiește meniul, îl leagă la jurnal și îl lasă să se arate.
    ' NU se pune în «Using»: fiind arătat nemodal, WinForms îl eliberează singur la închidere.
    Private Function NewPopup() As CustomPopup
        Dim cheie As String = If(chkSelectie.Checked, "print", Nothing)
        Dim p As New CustomPopup(BuildItems(), cheie)
        AddHandler p.ItemClicked, AddressOf OnPopupItemClicked
        AddHandler p.FormClosed, AddressOf OnPopupClosed
        Return p
    End Function

    Private Sub OnPopupItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Note("ales: «" & PopupMnemonic.Strip(e.Item.Text) & "» (cheie=" & e.Item.Key & ", poziția " & e.Index & ")")
    End Sub

    Private Sub OnPopupClosed(sender As Object, e As FormClosedEventArgs)
        Dim p As CustomPopup = TryCast(sender, CustomPopup)
        If p IsNot Nothing AndAlso p.ClickedItem Is Nothing Then Note("respins (Esc sau clic în afară)")
    End Sub

    ' ── Deschiderile ─────────────────────────────────────────────────────────────

    Private Sub OnSubButon(sender As Object, e As EventArgs) Handles btnSubButon.Click
        Try
            NewPopup().ShowBelow(btnSubButon)
        Catch ex As Exception
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    Private Sub OnLaCursor(sender As Object, e As EventArgs) Handles btnLaCursor.Click
        Try
            NewPopup().ShowAtCursor(Me)
        Catch ex As Exception
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Clic dreapta oriunde pe fereastră sau pe jurnal — cazul clasic al meniului contextual.</summary>
    Private Sub OnRightClick(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp, lstLog.MouseUp
        Try
            If e.Button <> MouseButtons.Right Then Return
            NewPopup().ShowAtCursor(Me)
        Catch ex As Exception
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────────────

    Private Sub OnClassic(sender As Object, e As EventArgs) Handles btnClassic.Click
        Switch(BuiltInSchemes.Classic())
    End Sub

    Private Sub OnDark(sender As Object, e As EventArgs) Handles btnDark.Click
        Switch(BuiltInSchemes.Dark())
    End Sub

    Private Sub OnModern(sender As Object, e As EventArgs) Handles btnModern.Click
        Switch(BuiltInSchemes.Modern())
    End Sub

    Private Sub Switch(scheme As ThemeScheme)
        Note("comută schema → " & scheme.Name)
        ThemeManager.SetScheme(scheme)
    End Sub

    ' ── Ajutoare ─────────────────────────────────────────────────────────────────

    Private Sub Note(text As String)
        _log(text)
        lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") & "  " & text)
    End Sub

    ' Patru pictograme desenate în cod: bancul de probă n-are nevoie de resurse ca să arate că
    ' banda din stânga funcționează, iar culorile lor sunt independente de schemă dinadins —
    ' o pictogramă a apelantului nu se re-colorează, doar se șterge când rândul e dezactivat.
    Private Sub BuildIcons()
        Dim culori As Color() = {Color.FromArgb(0, 122, 204), Color.FromArgb(0, 153, 51),
                                 Color.FromArgb(225, 140, 0), Color.FromArgb(190, 30, 30)}
        For Each c As Color In culori
            Dim bmp As New Bitmap(16, 16)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Using b As New SolidBrush(c)
                    g.FillEllipse(b, 2, 2, 12, 12)
                End Using
                Using p As New Pen(Color.FromArgb(120, Color.Black))
                    g.DrawEllipse(p, 2, 2, 12, 12)
                End Using
            End Using
            _icons.Add(bmp)
        Next
    End Sub

End Class
