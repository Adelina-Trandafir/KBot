Option Strict On
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Changes K-BOT makes ONCE per machine, the first time a build carrying them starts (after an
''' update, before any window). Each action is recorded by name in
''' <see cref="KBotPaths.AppliedOneTimeActions"/> (<c>kbot_paths.json</c>, kept by the updater), so
''' it never runs again and a later choice of the operator in «Setări» stands.
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

    ''' <summary>Runs every action not applied yet. Startup: never throws, never stops the launch.</summary>
    Friend Shared Sub RunAll()
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
            GlobalErrorLog.Write("OneTimeActions.RunAll", ex)
        End Try
    End Sub

End Class
