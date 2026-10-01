Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The shell's year / SS / CodProgram choice (slice 0086 split out of KbotForm.vb; slice 0097-03
''' moved the year and SS drop-downs from the header band into the caption bar, where they sit
''' beside the unit selector). The catalogue comes from /api/auth/periods; the chosen period is
''' fixed on the session, where JobBuilder reads it, and the SS is remembered on the server.
''' </summary>
Partial Public Class KbotForm

    ' The year / SS chosen in the caption bar (Nothing / empty = not read yet).
    Private Function AnulAles() As Integer?
        Dim key As String = capBar.GetSelectorKey(KBotCaptionBar.SelectorYear)
        Dim an As Integer
        Return If(Integer.TryParse(key, an), CType(an, Integer?), Nothing)
    End Function

    Private Function SsAles() As String
        Return capBar.GetSelectorKey(KBotCaptionBar.SelectorSector)
    End Function

    ''' <summary>
    ''' Fetches the year / SS / CodProgram catalogue of the current database and fills the
    ''' caption bar's selectors. The default year is the highest; the SS starts from LastSS (when
    ''' valid in that year), otherwise the first one. A read failure does not block the window --
    ''' it only leaves the selectors out of the bar.
    ''' </summary>
    Private Async Function LoadPeriodsAsync() As Task
        Try
            Try
                _periods = Await _authApi.GetPeriodsAsync(_session.Token, _session.DbName, CancellationToken.None)
            Catch ex As Exception
                ' The window is not blocked, but nothing is swallowed silently: the operator is
                ' told WHY the year/SS selectors are missing (the server's Romanian message, not
                ' raw JSON). The full detail goes to the shell's error log, not the FOREXE console.
                GlobalErrorLog.Write("MainForm.LoadPeriodsAsync.GetPeriods", ex)
                capBar.ClearSelector(KBotCaptionBar.SelectorYear)
                capBar.ClearSelector(KBotCaptionBar.SelectorSector)
                KBotMessage.Show(Me,
                    "Nu s-au putut citi perioadele (an/SS): " & ex.Message,
                    "Perioade", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try

            If _periods Is Nothing OrElse _periods.Count = 0 Then
                ' The unit has no configured periods -- not an error, a missing configuration.
                ' The bar without the selectors shows it; it is not the FOREXE console's business.
                capBar.ClearSelector(KBotCaptionBar.SelectorYear)
                capBar.ClearSelector(KBotCaptionBar.SelectorSector)
                Return
            End If

            Dim years As List(Of Integer) = _periods.Select(Function(p) p.AN).Distinct().OrderByDescending(Function(y) y).ToList()
            ' The highest year is the first one, and the one selected.
            capBar.SetSelectorItems(KBotCaptionBar.SelectorYear,
                years.Select(Function(y) New KeyValuePair(Of String, String)(CStr(y), CStr(y))),
                CStr(years(0)))

            LoadSsForSelectedYear()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadPeriodsAsync", ex)
            Throw
        End Try
    End Function

    ' Fills the SSs of the selected year; preselects LastSS when it exists in that year.
    Private Sub LoadSsForSelectedYear()
        Try
            Dim anAles As Integer? = AnulAles()
            If _periods Is Nothing OrElse Not anAles.HasValue Then Return
            Dim an As Integer = anAles.Value
            Dim ssList As List(Of String) = _periods.Where(Function(p) p.AN = an).
                                                     Select(Function(p) p.SS).Distinct().ToList()

            If ssList.Count = 0 Then
                capBar.ClearSelector(KBotCaptionBar.SelectorSector)
            Else
                Dim idx As Integer = If(String.IsNullOrEmpty(_session.LastSS), -1, ssList.IndexOf(_session.LastSS))
                capBar.SetSelectorItems(KBotCaptionBar.SelectorSector,
                    ssList.Select(Function(s) New KeyValuePair(Of String, String)(s, s)),
                    ssList(If(idx >= 0, idx, 0)))
            End If

            ApplySelectedPeriod(persist:=False)   ' the value already remembered is not saved again
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.LoadSsForSelectedYear", ex)
            Throw
        End Try
    End Sub

    ' Fixes the period on the session from the current selection; optionally remembers it on the server.
    Private Sub ApplySelectedPeriod(persist As Boolean)
        Try
            Dim anAles As Integer? = AnulAles()
            Dim ss As String = SsAles()
            If _periods Is Nothing OrElse Not anAles.HasValue OrElse String.IsNullOrEmpty(ss) Then Return
            Dim an As Integer = anAles.Value
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

    ' The operator picked another year or SS in the caption bar. A year change rebuilds the
    ' year's SSs (which fixes the period) and RE-READS the tree: the year is a server-side filter,
    ' so the old data is no longer valid. The same for the SS (a server-side filter, through EXISTS
    ' on FX_Indicatori.SS). The unit selector is handled in KbotForm.Units.vb.
    ' Nothing here can fail on the server before the tree read, so the selector moves at once.
    Private Async Sub CapBar_PeriodChanged(sender As Object, e As CaptionSelectorChangedEventArgs) Handles capBar.SelectorChanged
        Try
            If e Is Nothing OrElse _schimbaUnitatea Then Return
            Select Case e.Selector
                Case KBotCaptionBar.SelectorYear
                    capBar.SetSelectorKey(KBotCaptionBar.SelectorYear, e.Key)
                    LoadSsForSelectedYear()
                    Await LoadTreeAsync()
                Case KBotCaptionBar.SelectorSector
                    capBar.SetSelectorKey(KBotCaptionBar.SelectorSector, e.Key)
                    ApplySelectedPeriod(persist:=True)
                    Await LoadTreeAsync()
            End Select
        Catch ex As Exception
            ' UI boundary: a handler cannot re-throw (it would bring the process down) -- log and swallow.
            GlobalErrorLog.Write("MainForm.CapBar_PeriodChanged", ex)
        End Try
    End Sub
End Class
