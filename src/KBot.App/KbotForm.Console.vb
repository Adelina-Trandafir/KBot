Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' FOREXE: console, history, connect and synchronise (slice 0086 split out of KbotForm.vb).
''' </summary>
''' <remarks>
''' Connecting, the certificate choice, progress, state and cancelling all moved into
''' ForexeController (slice 0034). The shell keeps only the console window (created once,
''' hidden on close), the history window, the footer band's buttons, and the upsert step,
''' which is its own -- the coordinator brings the data, the server receives it on the
''' existing path (WithReauth).
''' </remarks>
Partial Public Class KbotForm

    ''' <summary>
    ''' Creates the FOREXE console when it does not exist. Called from <c>MainForm_Load</c>,
    ''' BEFORE the logger is built: its box is the logger's target for the whole life of the
    ''' application. Closing the window hides it (see <c>ForexeConsoleForm.OnFormClosing</c>),
    ''' so the instance stays valid and is never re-created.
    ''' </summary>
    Private Sub EnsureConsole()
        Try
            If _console IsNot Nothing AndAlso Not _console.IsDisposed Then Return
            _console = New ForexeConsoleForm()
            _console.Bind(_controller)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.EnsureConsole", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Creates the FOREXE history window on first request (slice 0040). Like the console, it is
    ''' built ONCE and hidden on close -- but, unlike the console, it is not needed at start-up:
    ''' nobody holds a reference into it, the history lives in <c>JobHistoryManager</c>, and the
    ''' window only reads it.
    ''' </summary>
    Private Sub EnsureIstoricForexe()
        Try
            If _istoricForexe IsNot Nothing AndAlso Not _istoricForexe.IsDisposed Then Return
            _istoricForexe = New ForexeHistoryForm()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.EnsureIstoricForexe", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' «Conectare» -- asks the coordinator for a FOREXE session (the certificate is chosen in
    ''' the coordinator's dialog).
    ''' </summary>
    Private Async Sub ConectareForexe(sender As Object, e As EventArgs) Handles forexeFooter.ConectareForexeRequested
        Try
            Await _controller.ConnectAsync()
        Catch ex As Exception
            ' UI boundary (async Sub): cannot re-throw -- log it and say why.
            GlobalErrorLog.Write("MainForm.btnConectare_Click", ex)
            KBotMessage.Show(Me, "Conectarea la FOREXE a eșuat: " & ex.Message, "FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>«Conectare» with the certificate used last time, without the choice dialog.</summary>
    Private Async Sub forexeFooter_ConectareForexeDefaultRequested(sender As Object, e As EventArgs) Handles forexeFooter.ConectareForexeDefaultRequested
        Try
            Await _controller.ConnectAsync(forexeFooter.LastUsedCertificate)
        Catch ex As Exception
            ' UI boundary (async Sub): cannot re-throw -- log it and say why.
            GlobalErrorLog.Write("MainForm.btnConectare_Click", ex)
            KBotMessage.Show(Me, "Conectarea la FOREXE a eșuat: " & ex.Message, "FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' The expand button of the footer band: shows the console (modeless, owned by the shell).
    Private Sub ForexeFooter_ExpandRequested(sender As Object, e As EventArgs) Handles forexeFooter.ExpandRequested
        Try
            EnsureConsole()
            If Not _console.Visible Then _console.Show(Me)
            If _console.WindowState = FormWindowState.Minimized Then _console.WindowState = FormWindowState.Normal
            _console.BringToFront()
            _console.Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.forexeFooter_ExpandRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The «Istoric» button of the footer band (slice 0040): shows the FOREXE action history,
    ''' modeless, owned by the shell -- exactly like the console. It reloads on EVERY opening:
    ''' the window lives as long as the application, and more jobs ran between two openings.
    ''' </summary>
    Private Sub ForexeFooter_HistoryRequested(sender As Object, e As EventArgs) Handles forexeFooter.HistoryRequested
        Try
            EnsureIstoricForexe()
            If Not _istoricForexe.Visible Then _istoricForexe.Show(Me)
            If _istoricForexe.WindowState = FormWindowState.Minimized Then _istoricForexe.WindowState = FormWindowState.Normal
            _istoricForexe.Reincarca()
            _istoricForexe.BringToFront()
            _istoricForexe.Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.forexeFooter_HistoryRequested", ex)
        End Try
    End Sub

    Private Async Sub forexeFooter_ShowBrowserRequested(sender As Object, e As EventArgs) Handles forexeFooter.ShowBrowserRequested
        Try
            If _controller Is Nothing Then Return
            ' Slice 0074: the place to look at the page is the «Browser FOREXE» view, not a
            ' window of its own. Connected, the button goes there (and back to «Sumar» when
            ' it is already there, which is the «hide» half of the old toggle); the console's
            ' own button still opens the recorder in view mode.
            If _controller.IsConnected Then
                navViews.SelectedKey = If(navViews.SelectedKey = "browser", "sumar", "browser")
                Return
            End If
            Await _controller.ToggleBrowserAsync()
        Catch ex As Exception
            ' UI boundary (async Sub): cannot re-throw -- log it and say why.
            GlobalErrorLog.Write("KbotForm.forexeFooter_ShowBrowserRequested", ex)
            KBotMessage.Show(Me, "Browserul nu a putut fi adus în față: " & ex.Message, "Consolă FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Synchronise = download the list (through the coordinator) + upsert to
    ''' <c>/api/forexe/angajamente/upsert</c>, with <c>WithReauth</c> on the HTTP call. It is
    ''' the flow of the old <c>btnSinc</c>. With no DbName (no login, possible only in the Debug
    ''' harness) it stops after the download -- the data is saved locally by the coordinator
    ''' anyway. Unreachable from the shell since 20.09.2026 (see <c>CapBar_OptionButtonClick</c>).
    ''' </summary>
    Private Async Function SincronizeazaAsync() As Task
        busyBar.Running = True
        _cts = New CancellationTokenSource()
        Try
            ' The same offer as on an angajament download (slice 0058): when the list is already
            ' in memory from this session, the operator picks between it and a fresh download.
            Dim mapate As List(Of Angajament) = IntreabaDacaRefolosescLista()
            If mapate Is Nothing Then mapate = Await _controller.DownloadListaAsync()
            If mapate Is Nothing Then
                ' The robot already said why: the coordinator put its line in the FOREXE band's
                ' state, and the download steps are on the console. Nothing to add here.
                Return
            End If

            ' Guard: without DbName (filled at login) the unit's database cannot be targeted.
            If String.IsNullOrEmpty(_session.DbName) Then
                KBotMessage.Show(Me,
                    "Lista a fost descărcată și salvată local, dar nu poate fi trimisă pe server: " &
                    "sesiunea nu are baza unității (necesită login).",
                    "Sincronizare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' The upsert is HTTP, not the robot: its result is told to the operator, not the FOREXE console.
            Dim resp As String = Await WithReauth(Function() _apiClient.UpsertAngajamenteAsync(_session.DbName, mapate, _cts.Token))
            KBotMessage.Show(Me,
                $"Sincronizare reușită: {mapate.Count} angajamente trimise în «{_session.DbName}».{Environment.NewLine}{resp}",
                "Sincronizare", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SincronizeazaAsync", ex)
            Throw
        Finally
            busyBar.Running = False
        End Try
    End Function
End Class
