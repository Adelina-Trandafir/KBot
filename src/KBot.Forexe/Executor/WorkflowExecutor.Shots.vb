Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq

' =============================================================================
'  Shots - the screen captures the ALOP guide asks for (operator, 28.09.2026).
'
'  The guide (Surse/GHID UTILIZARE_ALOP_V2.pdf) wants pictures of FOREXE inside both
'  documents the operator signs:
'    * the document of fundamentare - the reservations, «imediat ce au fost
'      introduse/actualizate» (p.11-14; the example on p.41 is the Buget tab right
'      after the save);
'    * the ordonantare de plata - two pictures (p.21-22): «Captura cu receptii» (the
'      Receptii tab) and «Captura cu sectiunea Informatii complete contract».
'
'  Until now the operator took them by hand with PrtScr and pasted them into the
'  ordonantare («Lipeste captura», OrdAtasamentePage). K-BOT takes them itself, at the
'  moments the page script names (ForexeWatch.js section 14).
'
'  THE PICTURE IS NOT THE SCREEN. The page script is asked to take K-BOT out of it
'  first - the floating menu, the veil, the orange frame, the blocking box - and to
'  switch off dark mode, because an inverted page inside a signed document is not what
'  the guide shows. Whether the operator's own CSS rules stay on is their choice
'  (AppSettings.ForexeCapturaPaginaOriginala).
'
'  JPEG, not PNG (operator, 28.09.2026: «i don't need them in max quality, they will
'  reside in the pdf»). A full page comes out around a fifth of the PNG. What the XFA
'  cell of the final PDF is given is decided where the PDF is built, not here.
' =============================================================================
Partial Public Class WorkflowExecutor

    ''' <summary>The quality of a capture: enough to read the figures, a fifth of a PNG.</summary>
    Public Const ShotJpegQuality As Integer = 70

    ' FOREXE's own wording, copied from «adlop - Incarca Rezervare.wfl» (the selectors the
    ' robot itself clicks). The page's text carries diacritics and the match is on the page's
    ' text, so they stay literal here; the sidebar has a look-alike entry («Afisare informatii
    ' complete angajamente active»), which is why the text is matched whole.
    Private Const SelInfoComplete As String =
        "a:has-text('Afișează informații complete'), button:has-text('Afișează informații complete')"
    Private Const SelInfoCompletePage As String = "*:has-text('Informații complete contract')"
    Private Const SelInapoi As String = "button.btn-default:has-text('Înapoi'), a:has-text('Înapoi')"

    ''' <summary>
    ''' The page as it stands now, as JPEG bytes; Nothing when there is no page. The script
    ''' prepares the page first and puts it back afterwards, whatever the screenshot does.
    ''' </summary>
    Public Async Function CapturePageAsync(paginaOriginala As Boolean) As Task(Of Byte())
        If _page Is Nothing OrElse _page.IsClosed Then Return Nothing
        Dim gata As Boolean = Await BeginShotAsync(paginaOriginala)
        ' VB cannot Await in a Finally: the picture is taken into a variable, the page is put
        ' back, and only then is the answer given. The page MUST be put back either way -
        ' the sheet that hides K-BOT's menu would otherwise stay in it.
        Dim octeti As Byte() = Nothing
        Try
            octeti = Await _page.ScreenshotAsync(New PageScreenshotOptions With {
                .FullPage = True,
                .Type = ScreenshotType.Jpeg,
                .Quality = ShotJpegQuality})
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.CapturePageAsync", ex)
            _logger.LogWarning("[Capturi] Pagina nu a putut fi fotografiată: " & ex.Message)
        End Try
        If gata Then Await EndShotAsync()
        Return octeti
    End Function

    ''' <summary>
    ''' The guide's second reception picture (p.22): «Afișează informații complete» is pressed,
    ''' the wide table is scrolled to its RIGHT end - Recepții and Plăți live there, and it
    ''' opens showing its left end (operator, 28.09.2026) - photographed, and «Înapoi» brings
    ''' the angajament back. Nothing when the button is not on the page.
    ''' </summary>
    Public Async Function CaptureInfoCompleteAsync(paginaOriginala As Boolean) As Task(Of Byte())
        If _page Is Nothing OrElse _page.IsClosed Then Return Nothing
        Dim buton As ILocator = _page.Locator(SelInfoComplete).First
        If Await buton.CountAsync() = 0 Then
            _logger.LogDebug("[Capturi] «Afișează informații complete» nu e în pagină; sar peste captură.")
            Return Nothing
        End If

        ' K-BOT drives the page here, so the page is closed to the operator exactly as it is
        ' during a job: a click of theirs in the middle of this would leave them on another
        ' page with a picture of it. The veil hides nothing from the picture itself - the
        ' shot sheet takes it out (beginShot).
        Await SetWatchSuspendedAsync(True, "K-BOT fotografiază «Informații complete contract»")
        LockDockedInput(True)

        ' VB cannot Await in a Finally, and «Înapoi» must be pressed whatever happened in
        ' between: the operator has to find the angajament where they left it.
        Dim octeti As Byte() = Nothing
        Dim deschis As Boolean = False
        Try
            Await buton.ClickAsync(New LocatorClickOptions With {.Timeout = 10000})
            deschis = True
            Await _page.WaitForSelectorAsync(SelInfoCompletePage,
                                             New PageWaitForSelectorOptions With {.Timeout = 10000})
            Await WaitForAjaxQuietAsync()
            ' The table sits in a box of its own; the script pushes every scrollable box right.
            Dim mutate As Integer = Await ScrollRightAsync()
            If mutate = 0 Then
                _logger.LogDebug("[Capturi] Tabelul «Informații complete» nu s-a putut derula; fotografiez cum este.")
            End If
            octeti = Await CapturePageAsync(paginaOriginala)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.CaptureInfoCompleteAsync", ex)
            _logger.LogWarning("[Capturi] «Informații complete contract» nu a putut fi fotografiată: " & ex.Message)
        End Try
        If deschis Then Await GoBackFromInfoCompleteAsync()
        LockDockedInput(False)
        Await SetWatchSuspendedAsync(False)
        Return octeti
    End Function

    ''' <summary>
    ''' «Înapoi» after the picture. The operator must find the angajament where they left it,
    ''' so a failure here is said out loud rather than swallowed into the capture's result.
    ''' </summary>
    Private Async Function GoBackFromInfoCompleteAsync() As Task
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return
            Dim inapoi As ILocator = _page.Locator(SelInapoi).First
            If Await inapoi.CountAsync() = 0 Then Return
            Await inapoi.ClickAsync(New LocatorClickOptions With {.Timeout = 10000})
            Await WaitForAjaxQuietAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.GoBackFromInfoCompleteAsync", ex)
            _logger.LogWarning("[Capturi] Nu am putut reveni din «Informații complete contract»: " & ex.Message &
                               " Apăsați «Înapoi» în pagină.")
        End Try
    End Function

    ''' <summary>Wicket's spinner gone and a beat for the re-render; never throws.</summary>
    Private Async Function WaitForAjaxQuietAsync() As Task
        Try
            Await _page.WaitForFunctionAsync(
                "() => { const l = document.querySelector('#animlogo'); " &
                "return !l || getComputedStyle(l).display === 'none'; }",
                Nothing, New PageWaitForFunctionOptions With {.Timeout = 10000})
        Catch ex As Exception
            ' A page without the spinner (or one that keeps it on) is photographed anyway.
            GlobalErrorLog.Write("WorkflowExecutor.WaitForAjaxQuietAsync", ex)
        End Try
        Try
            Await Task.Delay(400)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.WaitForAjaxQuietAsync", ex)
        End Try
    End Function

    Private Async Function BeginShotAsync(paginaOriginala As Boolean) As Task(Of Boolean)
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return False
            Return Await _page.EvaluateAsync(Of Boolean)(
                "(o) => (window._kbotWatch && window._kbotWatch.beginShot) ? window._kbotWatch.beginShot(o) : false",
                paginaOriginala)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.BeginShotAsync", ex)
            Return False
        End Try
    End Function

    Private Async Function EndShotAsync() As Task
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return
            Await _page.EvaluateAsync(Of Object)(
                "() => { if (window._kbotWatch && window._kbotWatch.endShot) { window._kbotWatch.endShot(); } }")
        Catch ex As Exception
            ' The sheet that hides K-BOT would stay in the page: loud, it is visible.
            GlobalErrorLog.Write("WorkflowExecutor.EndShotAsync", ex)
            _logger.LogWarning("[Capturi] Pagina nu a putut fi pusă la loc după captură: " & ex.Message)
        End Try
    End Function

    Private Async Function ScrollRightAsync() As Task(Of Integer)
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return 0
            Return Await _page.EvaluateAsync(Of Integer)(
                "() => (window._kbotWatch && window._kbotWatch.scrollRight) ? window._kbotWatch.scrollRight() : 0")
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.ScrollRightAsync", ex)
            Return 0
        End Try
    End Function

    ''' <summary>
    ''' Forgets that this angajament's reservation session has its «before» picture, so the
    ''' next session takes a new one. Called when the session is taken into K-BOT.
    ''' </summary>
    Public Async Function ResetShotSessionAsync() As Task
        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return
            Await _page.EvaluateAsync(Of Object)(
                "() => { if (window._kbotWatch && window._kbotWatch.resetShots) { window._kbotWatch.resetShots(); } }")
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.ResetShotSessionAsync", ex)
            _logger.LogDebug("[Capturi] Nu am putut reporni fotografierea rezervărilor: " & ex.Message)
        End Try
    End Function

    ' =========================================================================
    '  The page asks for a picture (ForexeWatch.js section 14)
    ' =========================================================================
    ''' <summary>
    ''' Who takes the picture the page asks for: (tip) -> True when it was taken and kept.
    ''' Set by the runner from what the shell registered; Nothing = no pictures in this
    ''' session, and the page goes on without one.
    ''' </summary>
    Private _capturaProvider As Func(Of String, CancellationToken, Task(Of Boolean))

    Public Sub SetCapturaProvider(provider As Func(Of String, CancellationToken, Task(Of Boolean)))
        _capturaProvider = provider
    End Sub

    ' The page holds the operator's click until this answers (or its own timeout runs out),
    ' so this ALWAYS answers. Async Sub from the Playwright callback: log and swallow.
    Private Async Sub RaspundeLaCaptura(requestId As String, tip As String)
        Dim luata As Boolean = False
        Dim motiv As String = String.Empty
        Try
            If _capturaProvider Is Nothing Then
                motiv = "K-BOT nu face capturi în sesiunea asta."
            Else
                luata = Await _capturaProvider(tip, CancellationToken.None)
                If Not luata Then motiv = "captura nu a putut fi păstrată."
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.RaspundeLaCaptura", ex)
            motiv = ex.Message
        End Try

        If luata Then
            _logger.LogInfo("[Capturi] Am fotografiat pagina înainte de modificare.")
        Else
            _logger.LogWarning("[Capturi] Pagina merge mai departe fără captură: " & motiv)
        End If

        Try
            If _page Is Nothing OrElse _page.IsClosed Then Return
            Dim arg As String = New JObject(
                New JProperty("id", requestId),
                New JProperty("ok", luata),
                New JProperty("m", motiv)).ToString(Newtonsoft.Json.Formatting.None)
            Await _page.EvaluateAsync(Of Object)(
                "(a) => { const o = JSON.parse(a); if (window._kbotWatch && window._kbotWatch.setCaptura) { window._kbotWatch.setCaptura(o.id, o.ok, o.m); } }",
                arg)
        Catch ex As Exception
            ' The page navigated meanwhile: its own timeout has already let the click through.
            GlobalErrorLog.Write("WorkflowExecutor.RaspundeLaCaptura", ex)
            _logger.LogDebug("[Capturi] Răspunsul la captură nu a ajuns în pagină: " & ex.Message)
        End Try
    End Sub

End Class
