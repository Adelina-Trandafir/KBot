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

    ' The key of the «Toate» choice of the source selector: the same «*» the tree request sends for every source.
    Private Const SS_TOATE As String = ApiClient.TreeAllSources

    Private Function SsToate() As Boolean
        Return String.Equals(SsAles(), SS_TOATE, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Moves the source selector to «Toate» without raising its event (it stays visible) and tells the session,
    ''' so the windows that follow the period hear it. The caller reloads the tree. No source choices = nothing to do.
    ''' </summary>
    Private Sub PuneSursaPeToate()
        Try
            If String.IsNullOrEmpty(SsAles()) Then Return
            capBar.SetSelectorKey(KBotCaptionBar.SelectorSector, SS_TOATE)
            ApplySelectedPeriod(persist:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PuneSursaPeToate", ex)
            Throw
        End Try
    End Sub

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
                Return
            End If

            Dim idx As Integer = If(String.IsNullOrEmpty(_session.LastSS), -1, ssList.IndexOf(_session.LastSS))
            Dim k_concrete As String = ssList(If(idx >= 0, idx, 0))
            ' «Toate» stays «Toate» across a year change; on the first read it is the choice when the tree is
            ' sorted by date (that sort always showed every source). Otherwise LastSS / the first source.
            Dim k_current As String = SsAles()
            Dim k_toate As Boolean = String.Equals(k_current, SS_TOATE, StringComparison.Ordinal) OrElse
                                     (String.IsNullOrEmpty(k_current) AndAlso AppSettings.Current.TreeSortIsDate)
            Dim k_items As New List(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)(SS_TOATE, "Toate")}
            k_items.AddRange(ssList.Select(Function(k_ss) New KeyValuePair(Of String, String)(k_ss, k_ss)))
            capBar.SetSelectorItems(KBotCaptionBar.SelectorSector, k_items, If(k_toate, SS_TOATE, k_concrete))

            ' The session always holds a CONCRETE source and program of the year (the robot and the documents need
            ' one); «Toate» only lifts the filter. Not saved as LastSS: the value already remembered stays.
            Dim k_row As PeriodInfo = _periods.FirstOrDefault(Function(p) p.AN = an AndAlso p.SS = k_concrete)
            If k_row IsNot Nothing Then _session.SetPeriod(an, k_concrete, k_row.CodProgram, k_toate)
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
            If SsToate() Then
                ' «Toate»: the concrete source and program stay (LoadSsForSelectedYear fixed them for this year);
                ' the session only learns the list is not narrowed. Nothing is remembered as LastSS.
                If Not String.IsNullOrEmpty(_session.SectorSursa) Then
                    _session.SetPeriod(an, _session.SectorSursa, _session.CodProgram, k_toateSursele:=True)
                End If
                Return
            End If
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
                Case KBotCaptionBar.SelectorSubunit
                    ' Slice AV01-06: the «Plan de conturi» window moves the selector itself once the pick went through.
                    OnSubunitPicked(e.Key)
            End Select
        Catch ex As Exception
            ' UI boundary: a handler cannot re-throw (it would bring the process down) -- log and swallow.
            GlobalErrorLog.Write("MainForm.CapBar_PeriodChanged", ex)
        End Try
    End Sub
End Class
