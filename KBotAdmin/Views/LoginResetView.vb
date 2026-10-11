Option Strict On
Imports System.Globalization

''' <summary>
''' The failed-login counters of the operators and their reset (slice ME01), as AvacontPush did through the
''' admin API. The counters live in the memory of the running server, so SSH cannot reach them. The API key is
''' read once over SSH from the server's config.py and never saved or logged.
''' </summary>
Public Class LoginResetView
    Implements IAdminView

    Private _context As AdminContext
    Private _apiKey As String = String.Empty

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' Not tied to the chosen database: every operator on every unit is listed.
    End Sub

    Private Async Function EnsureKeyAsync() As Task
        If _apiKey <> String.Empty Then Return
        Dim k_result As SshResult = Await _context.Client.StreamAsync(LoginApi.ReadKeyCommand(_context.Client.Settings), Nothing)
        Dim k_key As String = k_result.StdOut.Trim()
        If k_result.ExitStatus <> 0 OrElse k_key = String.Empty Then
            Throw New AdminException("Tokenul de administrare (ADMIN_TOKEN) nu a putut fi citit din config.py de pe server.")
        End If
        _apiKey = k_key
    End Function

    Private Function Api() As LoginApi
        Return New LoginApi(_context.Client.Settings.ApiBaseUrl, _apiKey)
    End Function

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("LoginResetView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citesc utilizatorii...")
                Await EnsureKeyAsync()
                Dim k_users As List(Of LoginUser) = Await Api().GetUsersAsync()
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_user As LoginUser In k_users
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row.Tag = k_user
                        k_row("sel") = False
                        k_row("un") = k_user.Un
                        k_row("dc") = k_user.Dc
                        k_row("unit") = k_user.UnitName
                        k_row("rol") = k_user.Rol
                        k_row("last") = k_user.LastSs
                        k_row("fails") = If(k_user.Fails > 0, k_user.Fails.ToString(CultureInfo.InvariantCulture), String.Empty)
                        k_row("blocked") = If(k_user.Blocked > 0, "da (" & CInt(Math.Ceiling(k_user.Blocked / 60.0)).ToString(CultureInfo.InvariantCulture) & " min)", String.Empty)
                    Next
                Finally
                    grid.EndUpdate()
                End Try
                Await ShowPasswordStateAsync()
                Dim k_blocked As Integer = k_users.Where(Function(k_u) k_u.Blocked > 0).Select(Function(k_u) k_u.Un).Distinct().Count()
                lblState.Text = k_users.Count.ToString(CultureInfo.InvariantCulture) & " rânduri citite, " & k_blocked.ToString(CultureInfo.InvariantCulture) & " utilizatori blocați."
                _context.Say(lblState.Text)
            End Function)
    End Function

    ' Fills the «Parola» column from the server (Conturi_Securitate). A server without the table says so
    ' and the column stays empty; the rest of the page still works.
    Private Async Function ShowPasswordStateAsync() As Task
        Try
            Dim k_state As PasswordStateResult = Await _context.Client.CallAsync(Of PasswordStateResult)("password-state")
            Dim k_by As New Dictionary(Of String, PasswordStateRow)(StringComparer.OrdinalIgnoreCase)
            For Each k_acc As PasswordStateRow In k_state.Accounts
                k_by(k_acc.Un) = k_acc
            Next
            For k_i As Integer = 0 To grid.Rows.Count - 1
                Dim k_un As String = Convert.ToString(grid("un", k_i), CultureInfo.InvariantCulture).Trim()
                Dim k_row As PasswordStateRow = Nothing
                Dim k_text As String = String.Empty
                If k_by.TryGetValue(k_un, k_row) Then
                    If k_row.MustChange Then
                        k_text = "de schimbat"
                    ElseIf Not String.IsNullOrEmpty(k_row.ChangedAt) Then
                        k_text = "schimbată " & k_row.ChangedAt.Substring(0, Math.Min(10, k_row.ChangedAt.Length))
                    End If
                End If
                grid("pwd", k_i) = k_text
            Next
        Catch ex As AdminException
            _context.Say("Starea parolelor nu a putut fi citită: " & ex.Message)
        End Try
    End Function

    Private Function SelectedAccounts() As List(Of String)
        Dim k_names As New List(Of String)()
        For k_i As Integer = 0 To grid.Rows.Count - 1
            If Convert.ToBoolean(grid("sel", k_i), CultureInfo.InvariantCulture) Then
                Dim k_un As String = Convert.ToString(grid("un", k_i), CultureInfo.InvariantCulture).Trim()
                If k_un <> String.Empty AndAlso Not k_names.Contains(k_un, StringComparer.OrdinalIgnoreCase) Then k_names.Add(k_un)
            End If
        Next
        Return k_names
    End Function

    Private Async Sub BtnMustChange_Click(k_sender As Object, e As EventArgs) Handles btnMustChange.Click
        Try
            Dim k_names As List(Of String) = SelectedAccounts()
            If k_names.Count = 0 Then
                KBotMessage.Show("Bifați cel puțin un utilizator.", "Parola", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Următorii utilizatori vor trebui să-și schimbe parola la următoarea conectare, iar sesiunile lor deschise se închid:" &
                                 vbLf & String.Join(vbLf, k_names) & vbLf & vbLf & "Continuați?", "Schimbarea parolei") Then Return
            Await UiRun.Guard("LoginResetView.BtnMustChange_Click", _context,
                Async Function()
                    For Each k_un As String In k_names
                        Await _context.Client.CallRawAsync("must-change", "--un", k_un)
                        _context.Say("Schimbare obligatorie a parolei: " & k_un)
                    Next
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.BtnMustChange_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnClearMustChange_Click(k_sender As Object, e As EventArgs) Handles btnClearMustChange.Click
        Try
            Dim k_names As List(Of String) = SelectedAccounts()
            If k_names.Count = 0 Then
                KBotMessage.Show("Bifați cel puțin un utilizator.", "Parola", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Se anulează obligația de a schimba parola pentru:" & vbLf & String.Join(vbLf, k_names) &
                                 vbLf & vbLf & "Continuați?", "Anulare obligație") Then Return
            Await UiRun.Guard("LoginResetView.BtnClearMustChange_Click", _context,
                Async Function()
                    For Each k_un As String In k_names
                        Await _context.Client.CallRawAsync("clear-must-change", "--un", k_un)
                        _context.Say("Obligația de schimbare a parolei anulată: " & k_un)
                    Next
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.BtnClearMustChange_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnMustChangeAll_Click(k_sender As Object, e As EventArgs) Handles btnMustChangeAll.Click
        Try
            If Not UiRun.Confirm("TOȚI utilizatorii legați de o unitate vor trebui să-și schimbe parola la următoarea conectare, " &
                                 "iar toate sesiunile deschise se închid." & vbLf & vbLf &
                                 "Fiecare are nevoie de e-mailul contului pentru codul de confirmare. Continuați?", "Schimbarea parolei pentru toți") Then Return
            Await UiRun.Guard("LoginResetView.BtnMustChangeAll_Click", _context,
                Async Function()
                    Await _context.Client.CallRawAsync("must-change-all")
                    _context.Say("Schimbare obligatorie a parolei: toate conturile.")
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.BtnMustChangeAll_Click", ex)
        End Try
    End Sub

    Private Sub Grid_RowFormatting(k_sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Try
            Dim k_user As LoginUser = TryCast(e.Row.Tag, LoginUser)
            If k_user IsNot Nothing AndAlso k_user.Blocked > 0 Then e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.Grid_RowFormatting", ex)
        End Try
    End Sub

    Private Async Sub BtnLoad_Click(k_sender As Object, e As EventArgs) Handles btnLoad.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.BtnLoad_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnReset_Click(k_sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            Dim k_names As New List(Of String)()
            For k_i As Integer = 0 To grid.Rows.Count - 1
                If Convert.ToBoolean(grid("sel", k_i), CultureInfo.InvariantCulture) Then
                    Dim k_un As String = Convert.ToString(grid("un", k_i), CultureInfo.InvariantCulture).Trim()
                    If k_un <> String.Empty AndAlso Not k_names.Contains(k_un, StringComparer.OrdinalIgnoreCase) Then k_names.Add(k_un)
                End If
            Next
            If k_names.Count = 0 Then
                KBotMessage.Show("Bifați cel puțin un utilizator.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Se resetează încercările de login pentru:" & vbLf & String.Join(vbLf, k_names) & vbLf & vbLf & "Continuați?", "Resetare login") Then Return
            Await UiRun.Guard("LoginResetView.BtnReset_Click", _context,
                Async Function()
                    _context.Say("Se resetează încercările de login...")
                    Await EnsureKeyAsync()
                    Dim k_api As LoginApi = Api()
                    For Each k_un As String In k_names
                        Dim k_cleared As Integer = Await k_api.ResetAsync(k_un)
                        _context.Say("Resetat: " & k_un & " (" & k_cleared.ToString(CultureInfo.InvariantCulture) & " contoare șterse)")
                    Next
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginResetView.BtnReset_Click", ex)
        End Try
    End Sub

End Class
