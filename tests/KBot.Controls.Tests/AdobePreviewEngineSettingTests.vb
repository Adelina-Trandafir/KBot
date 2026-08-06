Option Strict On
Imports Xunit
Imports KBot.Controls

''' <summary>
''' The «Motor previzualizare» setting: which surface renders the DDF document.
'''
''' The rule that matters most here is the FALLBACK. A broken or unrecognised value must land on the
''' hosted window, not on ActiveX — even though the ActiveX route measures better on the bench —
''' because the hosted window is the only one that has ever run inside the application. A corrupt
''' settings file must not silently switch the operator onto an engine they did not ask for.
''' </summary>
Public Class AdobePreviewEngineSettingTests

    <Theory>
    <InlineData("Fereastra")>
    <InlineData("fereastră")>
    <InlineData("window")>
    <InlineData("WindowHost")>
    Public Sub WindowSpellings_AllResolveToTheHostedWindow(stored As String)
        Dim read = AdobeViewerSettings.ParseEngine(stored)
        Assert.Equal(AdobePreviewEngine.WindowHost, read.Value)
        Assert.False(read.HasWarning)
    End Sub

    <Theory>
    <InlineData("ActiveX")>
    <InlineData("activex")>
    <InlineData("AcroPDF")>
    <InlineData("acro")>
    Public Sub ActiveXSpellings_AllResolveToTheControl(stored As String)
        Dim read = AdobeViewerSettings.ParseEngine(stored)
        Assert.Equal(AdobePreviewEngine.ActiveX, read.Value)
        Assert.False(read.HasWarning)
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("   ")>
    <InlineData(Nothing)>
    Public Sub AMissingValue_MeansTheHostedWindow_Silently(stored As String)
        ' Absent is not a mistake — it is simply an installation that never chose. No warning.
        Dim read = AdobeViewerSettings.ParseEngine(stored)
        Assert.Equal(AdobePreviewEngine.WindowHost, read.Value)
        Assert.False(read.HasWarning)
    End Sub

    <Fact>
    Public Sub AnUnrecognisedValue_FallsBackToTheWindow_AndSaysSo()
        Dim read = AdobeViewerSettings.ParseEngine("chromium")

        Assert.Equal(AdobePreviewEngine.WindowHost, read.Value)
        Assert.True(read.HasWarning)
        ' The message must name the setting and the offending value, or it cannot be acted on.
        Assert.Contains(KBotPathsKeys.AdobePreviewEngine, read.Warning)
        Assert.Contains("chromium", read.Warning)
    End Sub

    <Fact>
    Public Sub EngineTextRoundTrips()
        For Each e As AdobePreviewEngine In New AdobePreviewEngine() {
            AdobePreviewEngine.WindowHost, AdobePreviewEngine.ActiveX}
            Dim text As String = AdobeViewerSettings.EngineToText(e)
            Assert.Equal(e, AdobeViewerSettings.ParseEngine(text).Value)
        Next
    End Sub

    <Fact>
    Public Sub TheDefaultStoredValue_IsTheWindow()
        ' KBotPaths and the parser must agree, or a fresh install would warn about its own default.
        Dim read = AdobeViewerSettings.ParseEngine(KBot.Common.KBotPaths.DefaultAdobePreviewEngine)
        Assert.Equal(AdobePreviewEngine.WindowHost, read.Value)
        Assert.False(read.HasWarning)
    End Sub

    <Fact>
    Public Sub BothEngines_HaveDistinctRomanianLabels()
        Dim a As String = AdobeViewerSettings.EngineLabel(AdobePreviewEngine.WindowHost)
        Dim b As String = AdobeViewerSettings.EngineLabel(AdobePreviewEngine.ActiveX)
        Assert.NotEqual(a, b)
        Assert.False(String.IsNullOrWhiteSpace(a))
        Assert.False(String.IsNullOrWhiteSpace(b))
    End Sub

End Class
