Option Strict On
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Changes K-BOT makes ONCE per machine, the first time a build carrying them starts (after an
''' update, before any window). Each action is recorded by name in
''' <see cref="KBotPaths.AppliedOneTimeActions"/> (<c>kbot_paths.json</c>, kept by the updater), so
''' it never runs again and a later choice of the operator in «Setări» stands. Run in the order
''' below: on a machine that has none of them, the last one decides.
''' </summary>
Friend NotInheritable Class OneTimeActions

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Slice 0078-05: the PDF engine goes from ActiveX (AcroPDF) to the hosted Adobe window, with
    ''' the settings proven on the signing bench (29.09.2026): «/n» automatic. The hosted window is
    ''' no longer positioned or trimmed (Ctrl+H only), so it no longer spoils the operator's Adobe,
    ''' and the scripts' errors seen on ActiveX do not occur there.
    ''' </summary>
    Friend Const HostedWindowEngine As String = "0078-05-fereastra-gazduita"

    ''' <summary>
    ''' Slice 0078-15 (operator, 07.10.2026): the PDF engine goes back to ActiveX, now the rebuilt viewer (src load,
    ''' size re-send, verified Read Mode, Save As trap). Silent: no message, the operator changes nothing. A machine
    ''' already on an ActiveX engine is left as it is.
    ''' </summary>
    Friend Const ActiveXEngine As String = "0078-15-activex"

    ''' <summary>Runs every action not applied yet. Startup: never throws, never stops the launch.</summary>
    Friend Shared Sub RunAll()
        RunHostedWindowEngine()
        RunActiveXEngine()
    End Sub

    Private Shared Sub RunHostedWindowEngine()
        Try
            Dim paths As KBotPaths = KBotPaths.Current
            If paths.HasApplied(HostedWindowEngine) Then Return

            Dim engineBefore As String = paths.AdobePreviewEngine
            Dim instanceBefore As String = paths.AdobeNewInstance
            paths.AdobePreviewEngine = AdobeViewerSettings.EngineToText(AdobePreviewEngine.WindowHost)
            paths.AdobeNewInstance = AdobeViewerSettings.NewInstanceToText(AdobeNewInstanceMode.Auto)
            paths.MarkApplied(HostedWindowEngine)
            ' A failed write keeps the change for this session and runs the action again next time.
            Dim saved As Boolean = paths.Save()

            AdobeHostLog.Write($"Acțiune unică «{HostedWindowEngine}»: motorul PDF «{engineBefore}» -> " &
                               $"«{paths.AdobePreviewEngine}», instanță nouă «{instanceBefore}» -> " &
                               $"«{paths.AdobeNewInstance}»" &
                               If(saved, ".", " — kbot_paths.json nu a putut fi scris; se reia la pornirea următoare."))
        Catch ex As Exception
            GlobalErrorLog.Write("OneTimeActions.RunHostedWindowEngine", ex)
        End Try
    End Sub

    Private Shared Sub RunActiveXEngine()
        Try
            Dim k_paths As KBotPaths = KBotPaths.Current
            If k_paths.HasApplied(ActiveXEngine) Then Return

            Dim k_before As String = k_paths.AdobePreviewEngine
            Dim k_current As AdobePreviewEngine = AdobeViewerSettings.ParseEngine(k_before).Value
            ' Both ActiveX engines use the rebuilt viewer; «ActiveX» in the settings combo is ActiveXReadMode.
            If k_current = AdobePreviewEngine.WindowHost Then
                k_paths.AdobePreviewEngine = AdobeViewerSettings.EngineToText(AdobePreviewEngine.ActiveXReadMode)
            End If
            k_paths.MarkApplied(ActiveXEngine)
            ' A failed write keeps the change for this session and runs the action again next time.
            Dim k_saved As Boolean = k_paths.Save()

            AdobeHostLog.Write($"Acțiune unică «{ActiveXEngine}»: motorul PDF «{k_before}» -> «{k_paths.AdobePreviewEngine}»" &
                               If(k_saved, ".", " — kbot_paths.json nu a putut fi scris; se reia la pornirea următoare."))
        Catch ex As Exception
            GlobalErrorLog.Write("OneTimeActions.RunActiveXEngine", ex)
        End Try
    End Sub

End Class
