Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Dialogul care întreabă operatorul CĂREI unități îi aparține o clasificație, atunci când
''' perechea (SS, ClsfE) se potrivește cu mai multe. Portul modern al formularului modal
''' <c>FX_Unitate</c> din Access, deschis de <c>Obtine_IdUnitate_Din</c>.
''' </summary>
''' <remarks>
''' <para>O întrebare per dialog. Când o singură salvare are mai multe clasificații
''' ambigue, <c>PrelucrareCoordinator</c> deschide dialogul de mai multe ori la rând și
''' îi spune fiecăruia al câtelea este (<c>1 din 3</c>) — mai simplu, și mai ușor de urmărit
''' pentru operator, decât un formular care le-ar ține pe toate deodată.</para>
''' <para>Bifa «Nu mă mai întreba» este per COMBINAȚIE, nu globală: răspunsul se reține
''' pentru perechea asta, iar o pereche nouă se întreabă din nou.</para>
''' </remarks>
Public NotInheritable Class AlegereUnitateForm

    Private Const ColUnitate As String = "unitate"
    Private Const ColSursa As String = "sursa"
    Private Const ColProgram As String = "program"
    Private Const ColCod As String = "cod"

    Private ReadOnly _necesara As AlegereNecesara
    Private ReadOnly _codAngajament As String
    Private ReadOnly _pozitie As Integer
    Private ReadOnly _total As Integer

    ''' <summary>
    ''' Răspunsul operatorului, sau Nothing dacă a renunțat. Se citește după
    ''' <c>ShowDialog</c>.
    ''' </summary>
    Public ReadOnly Property Rezultat As AlegereUnitate

    ''' <param name="necesara">Întrebarea, așa cum a trimis-o serverul.</param>
    ''' <param name="codAngajament">Angajamentul în curs de salvare.</param>
    ''' <param name="pozitie">A câta întrebare este (1-based).</param>
    ''' <param name="total">Câte întrebări sunt în total.</param>
    Public Sub New(necesara As AlegereNecesara, codAngajament As String,
                   pozitie As Integer, total As Integer)
        ArgumentNullException.ThrowIfNull(necesara)
        _necesara = necesara
        _codAngajament = If(codAngajament, String.Empty)
        _pozitie = pozitie
        _total = total
        InitializeComponent()
    End Sub

    Private Sub AlegereUnitateForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            capBar.IconImage = My.Resources.kbot_64
            Pregateste()
        Catch ex As Exception
            ' Frontieră UI (Load): logăm și înghițim — o excepție aici ar dărâma procesul.
            GlobalErrorLog.Write("AlegereUnitateForm.AlegereUnitateForm_Load", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Umple formularul din întrebare. Separat de <c>Load</c> și <c>Friend</c> ca să se
    ''' poată verifica fără a deschide o fereastră: <c>Load</c> se ridică doar la
    ''' <c>Show</c>/<c>ShowDialog</c>, iar un test care ar arăta ferestre pe ecranul
    ''' operatorului e exact ce nu vrem în suita de teste.
    ''' </summary>
    Friend Sub Pregateste()
        Me.KeyPreview = True                ' fără chenar nativ => Escape ține locul lui X
        AcceptButton = btnAlege
        CancelButton = btnRenunta

        If _total > 1 Then
            lblTitle.Text = $"Alegeți unitatea ({_pozitie} din {_total})"
        End If

        lblAngajament.Text = If(String.IsNullOrEmpty(_codAngajament), "—", _codAngajament)
        lblIndicator.Text = TextIndicatori()
        lblClsf.Text = If(String.IsNullOrEmpty(_necesara.Clsf), _necesara.ClsfE, _necesara.Clsf)

        UmpleGrila()
    End Sub

    ''' <summary>
    ''' «AAB» sau «AAB, AAC și încă 3» — toți indicatorii care folosesc perechea, fiindcă
    ''' răspunsul se aplică tuturor, nu doar celui care a declanșat întrebarea.
    ''' </summary>
    Private Function TextIndicatori() As String
        Dim toti As List(Of String) = _necesara.Indicatori
        If toti Is Nothing OrElse toti.Count = 0 Then
            Return If(String.IsNullOrEmpty(_necesara.CodIndicator), "—", _necesara.CodIndicator)
        End If
        If toti.Count <= 3 Then Return String.Join(", ", toti)
        Return String.Join(", ", toti.GetRange(0, 3)) & $" și încă {toti.Count - 3}"
    End Function

    ' Un rând per unitate posibilă. Prima e preselectată doar ca punct de plecare al
    ' tastaturii — butonul cere oricum o alegere explicită (vezi btnAlege_Click).
    Private Sub UmpleGrila()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each u As UnitateCandidat In _necesara.Unitati
                Dim rand As KBotDataRow = grid.AddRow()
                rand(ColUnitate) = If(String.IsNullOrEmpty(u.Detalii), "(fără nume)", u.Detalii)
                rand(ColSursa) = u.SursaSector
                rand(ColProgram) = u.CodProgram
                rand(ColCod) = u.IdUnitate.ToString(Globalization.CultureInfo.InvariantCulture)
            Next
        Finally
            grid.EndUpdate()
        End Try
        If _necesara.Unitati.Count > 0 Then grid.CurrentRowIndex = 0
    End Sub

    ' Dublu-click pe un rând = alegerea lui. Aceeași cale ca butonul.
    Private Sub grid_CellDoubleClick(sender As Object, e As KBotCellEventArgs) Handles grid.CellDoubleClick
        Try
            Confirma()
        Catch ex As Exception
            GlobalErrorLog.Write("AlegereUnitateForm.grid_CellDoubleClick", ex)
        End Try
    End Sub

    Private Sub btnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            Confirma()
        Catch ex As Exception
            GlobalErrorLog.Write("AlegereUnitateForm.btnAlege_Click", ex)
        End Try
    End Sub

    Private Sub btnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            Renunta()
        Catch ex As Exception
            GlobalErrorLog.Write("AlegereUnitateForm.btnRenunta_Click", ex)
        End Try
    End Sub

    ''' <summary>Inchide fara raspuns. Butonul si Escape trec amandoua pe aici.</summary>
    Friend Sub Renunta()
        _Rezultat = Nothing
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ''' <summary>
    ''' Închide cu răspunsul rândului selectat. Fără selecție NU se închide: a ghici pentru
    ''' operator ar readuce exact defectul pe care dialogul îl repară.
    ''' </summary>
    Friend Sub Confirma()
        Dim idx As Integer = grid.CurrentRowIndex
        If idx < 0 OrElse idx >= _necesara.Unitati.Count Then
            ntfError.Show("Selectați o unitate din listă.", NoticeKind.Warning)
            Return
        End If
        ntfError.Clear()

        Dim ales As UnitateCandidat = _necesara.Unitati(idx)
        _Rezultat = New AlegereUnitate() With {
            .Ss = _necesara.Ss,
            .ClsfE = _necesara.ClsfE,
            .IdUnitate = ales.IdUnitate,
            .Retine = chkRetine.Checked
        }
        DialogResult = DialogResult.OK
        Close()
    End Sub

    ' Fără chenar nativ => fără buton X. Escape renunță, ca la LoginForm.
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            If e.KeyCode = Keys.Escape Then Renunta()
        Catch ex As Exception
            GlobalErrorLog.Write("AlegereUnitateForm.OnKeyDown", ex)
        End Try
    End Sub

    ' Culorile theme-aware. Rulează DUPĂ structura temei și la fiecare comutare de schemă.
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p = ThemeManager.Current.Palette

            ' Fundalul formularului ESTE conturul de 1px: se vede prin Padding(1).
            BackColor = p.BorderColor

            ' Etichetele-titlu de câmp și intro sunt secundare -> text estompat.
            For Each l As Label In {lblIntro, lblCapAngajament, lblCapIndicator, lblCapClsf}
                l.ForeColor = p.TextDimColor
            Next

            btnAlege.BackColor = p.AccentColor
            btnAlege.ForeColor = p.AccentTextColor
            btnAlege.FlatAppearance.BorderColor = p.AccentColor
            btnRenunta.BackColor = Color.Transparent
            btnRenunta.ForeColor = p.TextColor
            btnRenunta.FlatAppearance.BorderColor = p.BorderColor
        Catch ex As Exception
            ' Frontieră UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("AlegereUnitateForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
