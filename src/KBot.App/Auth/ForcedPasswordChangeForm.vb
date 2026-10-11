Option Strict On
Imports System.Collections.Generic
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Mandatory change of the INITIAL password (security plan S1). Opened by the login window when the
''' server answers «password change required»; nothing else of the application opens until it
''' returns OK. The window talks to the server on the short-lived restricted token only.
''' </summary>
''' <remarks>
''' The requirements are the server's own (<see cref="IAuthApi.GetPasswordPolicyAsync"/>), listed
''' with what is met and what is missing while the operator types; the server judges again before
''' it changes anything and a refusal carries the current policy back. Closing the window (Escape,
''' the left button) gives <see cref="DialogResult.Cancel"/>: no change was made and the caller
''' goes back to the credentials.
''' </remarks>
Public NotInheritable Class ForcedPasswordChangeForm

    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _token As String
    Private ReadOnly _currentPassword As String
    Private _policy As PasswordPolicyInfo
    Private _codeSent As Boolean

    ''' <summary>False when the legacy (Access) server could not take the new password too.</summary>
    Public Property LegacyUpdated As Boolean

    Public Sub New(k_authApi As IAuthApi, k_restrictedToken As String, k_currentPassword As String)
        ArgumentNullException.ThrowIfNull(k_authApi)
        If String.IsNullOrEmpty(k_restrictedToken) Then Throw New ArgumentException("Token lipsa.", NameOf(k_restrictedToken))
        _authApi = k_authApi
        _token = k_restrictedToken
        _currentPassword = k_currentPassword
        InitializeComponent()
    End Sub

    Private Async Sub Form_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Try
            Await LoadPolicyAsync()
            Await SendCodeAsync()
        Catch ex As Exception
            ' UI boundary (async Sub): log and swallow.
            GlobalErrorLog.Write("ForcedPasswordChangeForm.Form_Shown", ex)
        End Try
    End Sub

    Private Async Function LoadPolicyAsync() As Task
        Try
            _policy = Await _authApi.GetPasswordPolicyAsync(_token, CancellationToken.None)
            RefreshRequirements()
        Catch ex As ApiException
            ntfEroare.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.LoadPolicyAsync", ex)
            ntfEroare.Show("Cerințele de parolă nu au putut fi citite. Verificați rețeaua.", NoticeKind.Error)
        End Try
    End Function

    Private Async Function SendCodeAsync() As Task
        SetBusy(True)
        Try
            Dim k_info As PasswordCodeInfo = Await _authApi.RequestPasswordCodeAsync(_token, _currentPassword, CancellationToken.None)
            _codeSent = True
            Dim k_minutes As Integer = Math.Max(1, k_info.ExpiresInSeconds \ 60)
            lblStare.Text = $"Codul de confirmare a fost trimis la {k_info.EmailMasked}. Este valabil {k_minutes} minute."
            ntfEroare.Clear()
            txtCod.FocusInput()
        Catch ex As ApiException
            lblStare.Text = "Codul de confirmare nu a putut fi trimis."
            ntfEroare.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.SendCodeAsync", ex)
            lblStare.Text = "Codul de confirmare nu a putut fi trimis."
            ntfEroare.Show("Codul nu a putut fi cerut. Verificați rețeaua.", NoticeKind.Error)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Async Sub BtnRetrimite_Click(sender As Object, e As EventArgs) Handles btnRetrimite.Click
        Try
            Await SendCodeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.BtnRetrimite_Click", ex)
        End Try
    End Sub

    Private Sub TxtNoua_TextChanged(sender As Object, e As EventArgs) Handles txtNoua.TextChanged
        Try
            RefreshRequirements()
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.TxtNoua_TextChanged", ex)
        End Try
    End Sub

    ' One line per requirement, what is met first marked with a tick and what is missing with a cross.
    Private Sub RefreshRequirements()
        If _policy Is Nothing Then Return
        Dim k_text As New StringBuilder()
        For Each k_req As PasswordRequirement In PasswordPolicyEvaluator.Evaluate(txtNoua.Text, _policy)
            k_text.Append(If(k_req.Met, ChrW(&H2714), ChrW(&H2718))).Append(" ").AppendLine(k_req.Text)
        Next
        If Not _policy.PluginActive Then
            k_text.AppendLine("Serverul nu are o politică de parolă activă; se aplică doar lungimea minimă.")
        End If
        lblCerinte.Text = k_text.ToString().TrimEnd()
    End Sub

    Private Async Sub BtnSchimba_Click(sender As Object, e As EventArgs) Handles btnSchimba.Click
        Try
            Dim k_code As String = txtCod.Text.Trim()
            Dim k_new As String = txtNoua.Text
            If k_code.Length = 0 Then
                ntfEroare.Show(If(_codeSent, "Introduceți codul primit pe e-mail.", "Cereți mai întâi codul de confirmare."), NoticeKind.Error)
                Return
            End If
            If k_new.Length = 0 Then
                ntfEroare.Show("Introduceți parola nouă.", NoticeKind.Error)
                Return
            End If
            If Not String.Equals(k_new, txtConfirmare.Text, StringComparison.Ordinal) Then
                ntfEroare.Show("Parola nouă și confirmarea ei nu coincid.", NoticeKind.Error)
                Return
            End If
            If String.Equals(k_new, _currentPassword, StringComparison.Ordinal) Then
                ntfEroare.Show("Parola nouă trebuie să difere de cea inițială.", NoticeKind.Error)
                Return
            End If

            ntfEroare.Clear()
            SetBusy(True)
            Try
                Dim k_result As PasswordChangeResult = Await _authApi.ChangePasswordAsync(
                    _token, _currentPassword, k_code, k_new, CancellationToken.None)
                LegacyUpdated = k_result.LegacyUpdated
                DialogResult = DialogResult.OK
                Close()
            Catch ex As ApiException
                If ex.PasswordFailure IsNot Nothing Then
                    ' The policy moved, or an extra rule refused it: show the CURRENT requirements.
                    If ex.PasswordFailure.Policy IsNot Nothing Then _policy = ex.PasswordFailure.Policy
                    RefreshRequirements()
                    Dim k_lines As New StringBuilder(ex.Message)
                    For Each k_v As PasswordViolation In ex.PasswordFailure.Violations
                        k_lines.AppendLine().Append("• ").Append(k_v.Message)
                    Next
                    ntfEroare.Show(k_lines.ToString(), NoticeKind.Error)
                Else
                    ntfEroare.Show(ex.Message, NoticeKind.Error)
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("ForcedPasswordChangeForm.BtnSchimba_Click", ex)
                ntfEroare.Show("Parola nu a putut fi schimbată. Verificați rețeaua.", NoticeKind.Error)
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.BtnSchimba_Click.outer", ex)
        End Try
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        Try
            tlyCampuri.Enabled = Not k_busy
            btnSchimba.Enabled = Not k_busy
            btnRetrimite.Enabled = Not k_busy
            UseWaitCursor = k_busy
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.SetBusy", ex)
        End Try
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            txtNoua.Text = String.Empty
            txtConfirmare.Text = String.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.Form_FormClosed", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            If k_scheme Is Nothing Then Return
            Dim k_p As ThemePalette = k_scheme.Palette
            tlyMain.BackColor = k_p.SurfaceAltColor
            tlyCampuri.BackColor = k_p.SurfaceAltColor
            tlySubsol.BackColor = k_p.SurfaceAltColor
            For Each k_caption As Label In New Label() {lblIntro, lblStare, lblCod, lblNoua, lblConfirmare, lblCerinte}
                k_caption.ForeColor = k_p.TextDimColor
                k_caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnIesire, k_scheme)
            ButtonStyles.ApplySecondary(btnRetrimite, k_scheme)
            ButtonStyles.ApplyPrimary(btnSchimba, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ForcedPasswordChangeForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
