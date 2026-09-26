Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The shell's year / SS / CodProgram combos (slice 0086 split out of KbotForm.vb). The
''' catalogue comes from /api/auth/periods; the chosen period is fixed on the session, where
''' JobBuilder reads it, and the SS is remembered on the server.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' Fetches the year / SS / CodProgram catalogue of the current database and fills the
    ''' combos. The default year is the highest; the SS starts from LastSS (when valid in that
    ''' year), otherwise the first one. A read failure does not block the window -- it only
    ''' disables the combos.
    ''' </summary>
    Private Async Function LoadPeriodsAsync() As Task
        Try
            Try
                _periods = Await _authApi.GetPeriodsAsync(_session.Token, _session.DbName, CancellationToken.None)
            Catch ex As Exception
                ' The window is not blocked, but nothing is swallowed silently: the operator is
                ' told WHY the year/SS combos are disabled (the server's Romanian message, not
                ' raw JSON). The full detail goes to the shell's error log, not the FOREXE console.
                GlobalErrorLog.Write("MainForm.LoadPeriodsAsync.GetPeriods", ex)
                cboAn.Enabled = False
                cboSs.Enabled = False
                KBotMessage.Show(Me,
                    "Nu s-au putut citi perioadele (an/SS): " & ex.Message,
                    "Perioade", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try

            If _periods Is Nothing OrElse _periods.Count = 0 Then
                ' The unit has no configured periods -- not an error, a missing configuration.
                ' The disabled combos show it; it is not the FOREXE console's business.
                cboAn.Enabled = False
                cboSs.Enabled = False
                Return
            End If

            _suppressPeriodEvents = True
            Dim years = _periods.Select(Function(p) p.AN).Distinct().OrderByDescending(Function(y) y).ToList()
            cboAn.DataSource = years
            cboAn.SelectedIndex = 0            ' the highest year
            _suppressPeriodEvents = False

            LoadSsForSelectedYear()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadPeriodsAsync", ex)
            Throw
        End Try
    End Function

    ' Fills the SSs of the selected year; preselects LastSS when it exists in that year.
    Private Sub LoadSsForSelectedYear()
        Try
            If _periods Is Nothing OrElse cboAn.SelectedItem Is Nothing Then Return
            Dim an As Integer = CInt(cboAn.SelectedItem)
            Dim ssList = _periods.Where(Function(p) p.AN = an).
                                  Select(Function(p) p.SS).Distinct().ToList()

            _suppressPeriodEvents = True
            cboSs.DataSource = ssList
            Dim idx As Integer = If(String.IsNullOrEmpty(_session.LastSS), -1, ssList.IndexOf(_session.LastSS))
            cboSs.SelectedIndex = If(idx >= 0, idx, If(ssList.Count > 0, 0, -1))
            _suppressPeriodEvents = False

            ApplySelectedPeriod(persist:=False)   ' the value already remembered is not saved again
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadSsForSelectedYear", ex)
            Throw
        End Try
    End Sub

    ' Fixes the period on the session from the current selection; optionally remembers it on the server.
    Private Sub ApplySelectedPeriod(persist As Boolean)
        Try
            If _periods Is Nothing OrElse cboAn.SelectedItem Is Nothing OrElse cboSs.SelectedItem Is Nothing Then Return
            Dim an As Integer = CInt(cboAn.SelectedItem)
            Dim ss As String = CStr(cboSs.SelectedItem)
            Dim row As PeriodInfo = _periods.FirstOrDefault(Function(p) p.AN = an AndAlso p.SS = ss)
            If row Is Nothing Then Return

            ' CodProgram no longer has its own label in the footer (the FOREXE band took its
            ' place, slice 0034) -- it stays on the session, where JobBuilder reads it.
            _session.SetPeriod(an, ss, row.CodProgram)
            If persist Then PersistLastSs(ss)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ApplySelectedPeriod", ex)
            Throw
        End Try
    End Sub

    ' Fire-and-forget: remembering the SS must not block the user; a failure is logged.
    ' Async Sub with try/catch = cannot bring the application down.
    Private Async Sub PersistLastSs(ss As String)
        Try
            Await _authApi.SaveLastSsAsync(_session.Token, ss, CancellationToken.None)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PersistLastSs", ex)
        End Try
    End Sub

    ' A year change rebuilds the year's SSs (which fixes the period) and RE-READS the tree:
    ' the year is a server-side filter, so the old data is no longer valid.
    Private Async Sub CboAn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAn.SelectedIndexChanged, cboAn.SelectedIndexChanged
        Try
            If _suppressPeriodEvents Then Return
            LoadSsForSelectedYear()
            Await LoadTreeAsync()
        Catch ex As Exception
            ' UI boundary: a handler cannot re-throw (it would bring the process down) -- log and swallow.
            GlobalErrorLog.Write("MainForm.cboAn_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The same for the SS (a server-side filter, through EXISTS on FX_Indicatori.SS).
    Private Async Sub CboSs_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSs.SelectedIndexChanged, cboSs.SelectedIndexChanged
        Try
            If _suppressPeriodEvents Then Return
            ApplySelectedPeriod(persist:=True)
            Await LoadTreeAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.cboSs_SelectedIndexChanged", ex)
        End Try
    End Sub
End Class
