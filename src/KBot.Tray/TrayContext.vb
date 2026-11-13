Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The life of the tray application: the icon in the notification area, its menu (a right click; «Închide» is the last
''' item), the timer that asks <see cref="TrayChecker"/> whether there are documents to sign, and the message above the clock
''' (<see cref="DocumentsToast"/>) with the list. A click on a row opens the signing window itself (<see cref="SignerWindows"/>); the
''' tray updates itself (<see cref="TrayUpdater"/>). Slice TRAY-02: the signer edition has no K-BOT next to it.
'''
''' <para>The icon and the menu are built here, in code: this is not a form, so there is no designer file to declare them in
''' (KBotAdmin declares its own on its main form).</para>
'''
''' <para>Every <see cref="CheckInterval"/> the server is asked. While there is nothing to ask with (K-BOT has not been signed
''' in with «Ține minte parola» on this Windows session) or the last answer failed, it looks again every
''' <see cref="RetryInterval"/>: reading the registry costs nothing and the user should not wait ten minutes after signing in.</para>
''' </summary>
Friend NotInheritable Class TrayContext
    Inherits ApplicationContext

    Private Shared ReadOnly CheckInterval As TimeSpan = TimeSpan.FromMinutes(10)
    Private Shared ReadOnly RetryInterval As TimeSpan = TimeSpan.FromMinutes(1)
    Private Shared ReadOnly FirstCheckDelay As TimeSpan = TimeSpan.FromSeconds(20)

    Private ReadOnly _appDir As String
    Private ReadOnly _checker As New TrayChecker()
    Private ReadOnly _notify As New NotifyIcon()
    Private ReadOnly _menu As New ContextMenuStrip()
    Private ReadOnly _itemOpen As New ToolStripMenuItem("Documente de semnat")
    Private ReadOnly _itemCheck As New ToolStripMenuItem("Verifică acum")
    Private ReadOnly _itemProbe As New ToolStripMenuItem("Probă: deschide fereastra de semnare")
    Private ReadOnly _itemSignIn As New ToolStripMenuItem("Autentificare...")
    Private ReadOnly _itemAutostart As New ToolStripMenuItem("Pornește cu Windows")
    Private ReadOnly _itemExit As New ToolStripMenuItem("Închide")
    Private ReadOnly _timer As New System.Windows.Forms.Timer()
    Private ReadOnly _toast As New DocumentsToast()
    Private ReadOnly _signer As SignerWindows
    Private ReadOnly _updater As TrayUpdater

    Private _icon As Icon
    Private _busy As Boolean
    Private _last As TrayCheckResult = New TrayCheckResult(TrayStatus.NoCredentials)
    Private _closing As Boolean
    Private _prompting As Boolean
    Private _promptedOnce As Boolean
    Private _submittedFromPrompt As Boolean
    Private _resultFromPrompt As Boolean
    Private _exitEvent As EventWaitHandle
    Private _exitWait As RegisteredWaitHandle

    Friend Sub New(k_appDir As String)
        _appDir = k_appDir

        _menu.Items.AddRange(New ToolStripItem() {_itemOpen, _itemCheck, _itemSignIn, _itemProbe, New ToolStripSeparator(), _itemAutostart, New ToolStripSeparator(), _itemExit})
        _itemAutostart.CheckOnClick = False
        AddHandler _itemOpen.Click, AddressOf ItemOpen_Click
        AddHandler _itemCheck.Click, AddressOf ItemCheck_Click
        AddHandler _itemProbe.Click, AddressOf ItemProbe_Click
        AddHandler _itemSignIn.Click, AddressOf ItemSignIn_Click
        AddHandler _itemAutostart.Click, AddressOf ItemAutostart_Click
        AddHandler _itemExit.Click, AddressOf ItemExit_Click
        AddHandler _menu.Opening, AddressOf Menu_Opening

        _icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath)
        _notify.Icon = _icon
        _notify.Text = "K-BOT — documente de semnat"
        _notify.ContextMenuStrip = _menu
        AddHandler _notify.MouseUp, AddressOf Notify_MouseUp
        _notify.Visible = True

        AddHandler _toast.DocumentChosen, AddressOf Toast_DocumentChosen
        AddHandler _toast.ListChosen, AddressOf Toast_ListChosen

        _signer = New SignerWindows(_checker)
        _updater = New TrayUpdater(k_appDir)
        _updater.CleanUp()

        Autostart.EnsureOnFirstRun(k_appDir)
        WatchExitEvent()

        AddHandler _timer.Tick, AddressOf Timer_Tick
        _timer.Interval = CInt(FirstCheckDelay.TotalMilliseconds)
        _timer.Start()
    End Sub

    ' ── The check ────────────────────────────────────────────────────────────────

    Private Sub Timer_Tick(k_sender As Object, e As EventArgs)
        CheckNow()
    End Sub

    ' UI boundary (timer / menu): started without await, logs and swallows.
    Private Async Sub CheckNow()
        If _busy OrElse _closing OrElse _signer.IsOpen Then Return
        _busy = True
        Try
            _timer.Stop()
            ' Slice TRAY-02: a newer tray on the server replaces this one (at most every six hours); the new one starts itself.
            If Await _updater.TryUpdateAsync(CancellationToken.None) Then
                _updater.Restart()
                _closing = True
                _notify.Visible = False
                ExitThread()
                Return
            End If
            _resultFromPrompt = _submittedFromPrompt
            _submittedFromPrompt = False
            Dim k_result As TrayCheckResult = Await _checker.CheckAsync(CancellationToken.None)
            If _closing Then Return
            _last = k_result
            Apply(k_result)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.CheckNow", ex)
        Finally
            _busy = False
            If Not _closing Then
                Dim k_wait As TimeSpan = If(_last.Status = TrayStatus.Listed OrElse _last.Status = TrayStatus.NoList, CheckInterval, RetryInterval)
                ' A pair was typed while this check was still running: look again at once.
                If _submittedFromPrompt Then k_wait = TimeSpan.FromSeconds(1)
                _timer.Interval = CInt(k_wait.TotalMilliseconds)
                _timer.Start()
            End If
        End Try
    End Sub

    Private Sub Apply(k_result As TrayCheckResult)
        TrayLog.Write("check", $"result: {k_result.Status}, documents={k_result.Documents.Count}, units not read={k_result.UnreadUnits.Count}")
        Select Case k_result.Status
            Case TrayStatus.Listed
                If k_result.Documents.Count = 0 Then
                    _toast.Hide()
                    SetTip("Nu aveți documente de semnat.")
                Else
                    SetTip(If(k_result.Documents.Count = 1, "Aveți 1 document de semnat.", $"Aveți {k_result.Documents.Count} documente de semnat."))
                    _toast.ShowDocuments(k_result.Documents, k_result.UnreadUnits.Count)
                End If
            Case TrayStatus.NoList
                SetTip("Nu aveți documente de semnat pentru rolul dvs.")
            Case TrayStatus.NoCredentials
                SetTip("Faceți clic pe iconiță ca să vă autentificați.")
                ' Asked once on its own (at the start of Windows there is nothing else to go by); after a Cancel only the icon or the menu ask.
                If Not _promptedOnce Then PromptSignIn(String.Empty)
            Case TrayStatus.Rejected
                SetTip("Autentificarea nu a reușit. Faceți clic pe iconiță.")
                ' Only a pair typed here is answered with the window again; a stale pair of K-BOT's is just left aside.
                If _resultFromPrompt Then PromptSignIn(k_result.Message)
            Case TrayStatus.PasswordChangeRequired
                SetTip("Parola trebuie schimbată înainte de autentificare. Contactați administratorul.")
            Case Else
                SetTip("Verificarea documentelor de semnat nu a reușit. Reîncerc.")
        End Select
    End Sub

    ' NotifyIcon.Text takes 127 characters at most.
    Private Sub SetTip(k_text As String)
        Dim k_full As String = "K-BOT — " & k_text
        _notify.Text = If(k_full.Length > 127, k_full.Substring(0, 124) & "...", k_full)
    End Sub

    ' ── Opening K-BOT ────────────────────────────────────────────────────────────

    Private Sub Toast_DocumentChosen(k_document As DdfDeSemnat)
        OpenSigning(k_document.Idrev)
    End Sub

    Private Sub Toast_ListChosen()
        OpenSigning(0)
    End Sub

    ''' <summary>
    ''' Opens the signing window (the list of documents, with the revision picked when <paramref name="k_idrev"/> is not 0). It is modal:
    ''' the tray reads nothing and signs nothing out underneath it; when it closes the list is read again at once, because the user
    ''' has probably signed something. UI boundary: logs and swallows.
    ''' </summary>
    Private Sub OpenSigning(k_idrev As Integer)
        Try
            _toast.Hide()
            _signer.Open(k_idrev)
            If Not _closing Then CheckNow()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.OpenSigning", ex)
        End Try
    End Sub

    Private Sub Notify_MouseUp(k_sender As Object, e As MouseEventArgs)
        Try
            ' The right click opens the menu by itself (WinForms); the left one shows the list.
            If e.Button <> MouseButtons.Left Then Return
            If _last.Status = TrayStatus.NoCredentials OrElse _last.Status = TrayStatus.Rejected Then
                PromptSignIn(String.Empty)
            ElseIf _last.Status = TrayStatus.Listed Then
                _toast.ShowDocuments(_last.Documents, _last.UnreadUnits.Count)
            Else
                CheckNow()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.Notify_MouseUp", ex)
        End Try
    End Sub

    ' ── The menu ─────────────────────────────────────────────────────────────────

    Private Sub Menu_Opening(k_sender As Object, e As System.ComponentModel.CancelEventArgs)
        ' A tray started from a build folder never touches the Windows Run key (see Autostart.IsDevelopmentFolder).
        _itemAutostart.Enabled = Not Autostart.IsDevelopmentFolder(_appDir)
        ' The probe is only for the developer's build folder: it opens the signing window even when there is nothing to sign.
        _itemProbe.Visible = Autostart.IsDevelopmentFolder(_appDir)
        _itemAutostart.Checked = Autostart.IsOn()
    End Sub

    Private Sub ItemOpen_Click(k_sender As Object, e As EventArgs)
        If _last.Status = TrayStatus.Listed Then
            OpenSigning(0)
        ElseIf _last.Status = TrayStatus.NoCredentials OrElse _last.Status = TrayStatus.Rejected Then
            PromptSignIn(String.Empty)
        Else
            CheckNow()
        End If
    End Sub

    Private Sub ItemSignIn_Click(k_sender As Object, e As EventArgs)
        PromptSignIn(String.Empty)
    End Sub

    ''' <summary>
    ''' The tray's own sign-in window. The pair is kept in this process only (encrypted, DPAPI) and the check runs at once. UI boundary:
    ''' logs and swallows.
    ''' </summary>
    Private Sub PromptSignIn(k_error As String)
        If _prompting OrElse _closing Then Return
        _prompting = True
        _promptedOnce = True
        Try
            _toast.Hide()
            Dim k_user As String = Nothing, k_pass As String = Nothing
            If Not SessionCredentials.TryLoadForWindowsSession(k_user, k_pass) Then k_user = LastLoginStore.Load().Username
            Using k_form As New TrayLoginForm(k_user, k_error, _checker.Auth)
                If k_form.ShowDialog() <> DialogResult.OK Then Return
                SessionCredentials.Remember(k_form.UserName, k_form.Password, interactive:=True)
            End Using
            _submittedFromPrompt = True
            CheckNow()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.PromptSignIn", ex)
        Finally
            _prompting = False
        End Try
    End Sub

    Private Sub ItemProbe_Click(k_sender As Object, e As EventArgs)
        TrayLog.Write("probe", "the signing window was asked for from the menu (developer's build)")
        OpenSigning(0)
    End Sub

    Private Sub ItemCheck_Click(k_sender As Object, e As EventArgs)
        CheckNow()
    End Sub

    Private Sub ItemAutostart_Click(k_sender As Object, e As EventArgs)
        Try
            Autostart.SetOn(Not Autostart.IsOn(), _appDir)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.ItemAutostart_Click", ex)
            KBotMessage.Show("Pornirea cu Windows nu a putut fi schimbată. Detalii în jurnalul de erori.", "K-BOT — documente de semnat",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub ItemExit_Click(k_sender As Object, e As EventArgs)
        ExitAsync()
    End Sub

    ' ── K-BOT asks the tray to end (before an update) ─────────────────────────────

    ' Waits for the event K-BOT sets before an update; an event, not a timer. The callback runs on a pool thread.
    Private Sub WatchExitEvent()
        _exitEvent = New EventWaitHandle(False, EventResetMode.AutoReset, TrayRequest.ExitEventName)
        ' The menu's handle is made now so that the callback can post to the UI thread.
        Dim k_handle As IntPtr = _menu.Handle
        _exitWait = ThreadPool.RegisterWaitForSingleObject(_exitEvent, AddressOf ExitEvent_Signaled, Nothing, Timeout.Infinite, True)
    End Sub

    Private Sub ExitEvent_Signaled(k_state As Object, k_timedOut As Boolean)
        Try
            If k_timedOut OrElse _closing Then Return
            _menu.BeginInvoke(New Action(AddressOf ExitAsync))
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.ExitEvent_Signaled", ex)
        End Try
    End Sub

    ' UI boundary: logs and swallows. The server session is ended first (best effort), then the program ends.
    Private Async Sub ExitAsync()
        If _closing Then Return
        Try
            _closing = True
            _timer.Stop()
            _notify.Visible = False
            _toast.Hide()
            Await _checker.LogoutAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayContext.ExitAsync", ex)
        Finally
            ExitThread()
        End Try
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then
            _exitWait?.Unregister(Nothing)
            _exitEvent?.Dispose()
            _timer.Dispose()
            _toast.Dispose()
            _notify.Dispose()
            _menu.Dispose()
            _checker.Dispose()
            _updater.Dispose()
            _icon?.Dispose()
        End If
        MyBase.Dispose(k_disposing)
    End Sub
End Class
