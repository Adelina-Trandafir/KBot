Option Strict On
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Subunitate» (slice AV01-04): one subordinate unit with its own accounting. The operator types the name and the
''' chapter (65.01), picks the sector+source (01A ...) and the salary decimals (0 / 2, the old Access flag Zec2).
''' The first subunit of a unit is always the default one. The dialog SAVES by itself: it stays open while the
''' server refuses, and closes with OK once saved (<see cref="SavedId"/>).
''' </summary>
Public Class SubunitateEditForm

    Private Shared ReadOnly CapitolRegex As New Regex("^\d{2}\.\d{2}$", RegexOptions.CultureInvariant)

    Private ReadOnly _api As IConturiApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _subunitate As SubunitateInfo
    Private ReadOnly _first As Boolean
    Private _busy As Boolean

    ''' <summary>The number of the subunit once saved; 0 until then.</summary>
    Public Property SavedId As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_subunitate">The subunit to edit; Nothing = a new one.</param>
    ''' <param name="k_surse">Every sector+source the operator may pick.</param>
    ''' <param name="k_first">True when the unit has no subunit yet: the new one is the default one.</param>
    Public Sub New(k_api As IConturiApi, k_gate As ReauthGate, k_subunitate As SubunitateInfo,
                   k_surse As IReadOnlyList(Of ClsfCont), k_first As Boolean)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_surse)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _subunitate = k_subunitate
        _first = k_first

        cboSs.CaptionSelector = Function(k_item) CType(k_item, ClsfCont).Cod & " — " & CType(k_item, ClsfCont).Denumire
        cboSs.BeginUpdate()
        For Each k_s As ClsfCont In k_surse
            cboSs.Items.Add(k_s)
        Next
        cboSs.EndUpdate()
        cboZecimale.Items.Add("0 zecimale")
        cboZecimale.Items.Add("2 zecimale")
        cboZecimale.SelectedIndex = 0
        Show(k_subunitate)
    End Sub

    ' Fills the fields from the subunit (Nothing = a new one).
    Private Overloads Sub Show(k_sub As SubunitateInfo)
        If k_sub Is Nothing Then
            capBar.Text = "Subunitate nouă"
            Text = "Subunitate nouă"
            chkImplicita.Checked = _first
            chkImplicita.Enabled = Not _first
            chkActiv.Checked = True
            Return
        End If
        capBar.Text = "Subunitate — " & k_sub.Denumire
        Text = capBar.Text
        txtDenumire.Text = k_sub.Denumire
        txtCapitol.Text = k_sub.Capitol
        For k_i As Integer = 0 To cboSs.Items.Count - 1
            If String.Equals(CType(cboSs.Items(k_i), ClsfCont).Cod, k_sub.SursaSector, StringComparison.Ordinal) Then
                cboSs.SelectedIndex = k_i
                Exit For
            End If
        Next
        cboZecimale.SelectedIndex = If(k_sub.Zecimale = 2, 1, 0)
        chkImplicita.Checked = k_sub.Implicita
        ' The default one stays default until another is marked (the server refuses an unmark).
        chkImplicita.Enabled = Not k_sub.Implicita
        chkActiv.Checked = k_sub.Activ
    End Sub

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _busy Then Return
            Dim k_denumire As String = txtDenumire.Text.Trim()
            Dim k_capitol As String = txtCapitol.Text.Trim()
            Dim k_problem As String = Nothing
            If k_denumire.Length = 0 Then
                k_problem = "Introduceți denumirea subunității."
                txtDenumire.Focus()
            ElseIf Not CapitolRegex.IsMatch(k_capitol) Then
                k_problem = "Capitolul trebuie să aibă forma 65.01 (două cifre, punct, două cifre)."
                txtCapitol.Focus()
            ElseIf cboSs.SelectedItem Is Nothing Then
                k_problem = "Alegeți sectorul și sursa."
                cboSs.Focus()
            End If
            If k_problem IsNot Nothing Then
                KBotMessage.Show(Me, k_problem, "Subunitate", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim k_toSave As New SubunitateInfo() With {
                .IdSubunitate = If(_subunitate Is Nothing, 0, _subunitate.IdSubunitate),
                .Denumire = k_denumire, .Capitol = k_capitol,
                .SursaSector = CType(cboSs.SelectedItem, ClsfCont).Cod,
                .Zecimale = If(cboZecimale.SelectedIndex = 1, 2, 0),
                .Implicita = chkImplicita.Checked, .Activ = chkActiv.Checked}
            SetBusy(True, "Se salvează subunitatea…")
            Dim k_id As Integer
            Try
                k_id = Await _gate.RunAsync(Function() _api.SaveSubunitateAsync(k_toSave, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            SavedId = k_id
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ApiException
            GlobalErrorLog.Write("SubunitateEditForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Subunitate", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("SubunitateEditForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Subunitatea nu a putut fi salvată. Detalii în jurnalul de erori.",
                                 "Subunitate", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        btnSalveaza.Enabled = Not k_busy
        If k_status IsNot Nothing Then lblStare.Text = k_status
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            BackColor = k_scheme.Palette.BorderColor
            lblStare.ForeColor = k_scheme.Palette.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            ButtonStyles.ApplySecondary(btnRenunta, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SubunitateEditForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
