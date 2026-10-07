Option Strict On
Imports KBot.Common

' Save after signature (Ctrl+S), the ActiveX side of PdfSigningSession.SaveAfterSignature (slice 0078-15, operator
' 07.10.2026: OPTIONAL, to be taken out easily if it does not work).
'
' Why (slice 0078-05, proven on the hosted window 28-29.09.2026): after a signature Adobe runs the form's postSign
' scripts, which change the document AFTER it was saved -- the change that unlocks the next signer (A -> B ->
' Ordonator). The signing session asks the viewer to save once more and waits for the file to change before it
' uploads. The hosted window sends Ctrl+S; until now the ActiveX viewer answered «cannot».
'
' How: RequestSave flags it; the attempt goes through the same posted path as Ctrl+H / Ctrl+2 (TrySendReadMode), with
' the same conditions (form enabled and in the foreground, no script-alert burst, the keyboard focus really inside the
' control). A condition that fails leaves it pending; the next window event / foreground change retries -- the small
' watch is started again for it when it had already stopped after Read Mode. The Save As that may follow is the trap's.
' SaveKeysSent / SaveNotSent tell the caller (ReaderHostPreview -> the signing session), as AdobeReaderHost does.
Partial Public NotInheritable Class AcroPdfViewer

    Private Const VK_S As UShort = &H53US

    Private _savePending As Boolean

    ''' <summary>True = <see cref="RequestSave"/> sends Ctrl+S; False (default) = it answers False, as before.</summary>
    Public Property SaveAfterSignatureEnabled As Boolean

    ''' <summary>Raised (UI thread) when Ctrl+S went out to the document.</summary>
    Public Event SaveKeysSent As Action
    ''' <summary>Raised (UI thread) when Ctrl+S could not be sent. Argument = Romanian sentence for the operator.</summary>
    Public Event SaveNotSent As Action(Of String)

    ''' <summary>
    ''' Asks Adobe to save the document (Ctrl+S), as soon as the keys can go. True = asked (it is pending or sent); False
    ''' = the option is off or there is no document.
    ''' </summary>
    Public Function RequestSave() As Boolean
        Try
            If Not SaveAfterSignatureEnabled OrElse _host Is Nothing OrElse Not _host.IsHandleCreated OrElse
               String.IsNullOrEmpty(_loadedPath) Then Return False
            _savePending = True
            _readModeWait = Nothing
            Report("AcroPDF: salvare cerută de K-BOT după semnătură — Ctrl+S în așteptare.")
            Check($"+{Elapsed()} ms CTRL+S requested")                                ' ACTIVEX-CHECK
            ' The small watch stops once Read Mode is done: it is needed again for the retries.
            If Not DetailedWatch AndAlso _lightObject = IntPtr.Zero Then StartLightWatch(k_keepPageSeen:=True)
            ReadModeOnEvent()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.RequestSave", ex)
            Return False
        End Try
    End Function

    ' One attempt, from TrySendReadMode (wrapped there): the conditions, the focus, then Ctrl+S once.
    Private Sub SendSaveStep(k_layout As AdobeLayout)
        Dim k_focus As IntPtr = FocusForKeys(k_layout)
        If k_focus = IntPtr.Zero Then Return
        Dim k_error As Integer = 0
        Dim k_ok As Boolean = SendCtrlChord(VK_S, k_error)
        _savePending = False
        Check($"+{Elapsed()} ms >>>>> CTRL+S sent: ok={k_ok}" & If(k_error <> 0, $", Win32 error {k_error}", "") &   ' ACTIVEX-CHECK
              $"; focus {Describe(k_focus)} <<<<<")
        If k_ok Then
            Report("AcroPDF: Ctrl+S trimis documentului (salvare după semnătură).")
            RaiseEvent SaveKeysSent()
        Else
            RaiseEvent SaveNotSent($"K-BOT nu a putut cere salvarea documentului după semnătură (eroarea {k_error})." &
                                   Environment.NewLine & "Apăsați Ctrl+S în document.")
        End If
    End Sub

End Class
