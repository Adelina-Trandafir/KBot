Option Strict On
Imports System.IO
Imports KBot.Common
Imports KBot.Controls
Imports Xunit

' Slice 0024 — reading and writing the two Adobe settings.
'
' THE RULE UNDER TEST: a stored value that cannot be interpreted falls back to «Auto» WITH A
' WARNING, and never throws. A broken settings file must not be able to stop a document from
' opening — the operator would have no way to connect the two.
'
' KBotPaths.Load/Save take an explicit directory precisely so this can run against a temp folder
' without touching the singleton or the installed application.
Public Class AdobeViewerSettingsTests

    ' ══ parsing the profile ════════════════════════════════════════════════════
    <Theory>
    <InlineData("Auto")>
    <InlineData("auto")>
    <InlineData("Automat")>
    Public Sub AutoText_ParsesToAuto(stored As String)
        Dim r = AdobeViewerSettings.ParseMode(stored)
        Assert.Equal(AdobeViewerMode.Auto, r.Value)
        Assert.False(r.HasWarning)
    End Sub

    <Theory>
    <InlineData("Modern")>
    <InlineData("MODERN")>
    <InlineData(" modern ")>
    Public Sub ModernText_ParsesToModern(stored As String)
        Assert.Equal(AdobeViewerMode.Modern, AdobeViewerSettings.ParseMode(stored).Value)
    End Sub

    <Theory>
    <InlineData("Classic")>
    <InlineData("Clasic")>
    Public Sub ClassicText_ParsesToClassic(stored As String)
        ' Both spellings: the file stores the English word, the operator may well type the Romanian.
        Assert.Equal(AdobeViewerMode.Classic, AdobeViewerSettings.ParseMode(stored).Value)
    End Sub

    <Theory>
    <InlineData(Nothing)>
    <InlineData("")>
    <InlineData("   ")>
    Public Sub AMissingValue_IsAutoWithoutAWarning(stored As String)
        ' Missing is the DEFAULT, not a mistake — warning about it would train the operator to
        ' ignore warnings.
        Dim r = AdobeViewerSettings.ParseMode(stored)
        Assert.Equal(AdobeViewerMode.Auto, r.Value)
        Assert.False(r.HasWarning)
    End Sub

    <Fact>
    Public Sub AnInvalidValue_IsAutoWITHAWarningThatNamesTheKeyAndTheValue()
        Dim r = AdobeViewerSettings.ParseMode("turbo")
        Assert.Equal(AdobeViewerMode.Auto, r.Value)
        Assert.True(r.HasWarning)
        Assert.Contains("AdobeViewerMode", r.Warning)
        Assert.Contains("turbo", r.Warning)
    End Sub

    ' ══ parsing «instanță nouă» ════════════════════════════════════════════════
    <Theory>
    <InlineData("Auto", AdobeNewInstanceMode.Auto)>
    <InlineData("Da", AdobeNewInstanceMode.Da)>
    <InlineData("yes", AdobeNewInstanceMode.Da)>
    <InlineData("1", AdobeNewInstanceMode.Da)>
    <InlineData("Nu", AdobeNewInstanceMode.Nu)>
    <InlineData("false", AdobeNewInstanceMode.Nu)>
    <InlineData("0", AdobeNewInstanceMode.Nu)>
    Public Sub NewInstanceText_Parses(stored As String, expected As AdobeNewInstanceMode)
        Dim r = AdobeViewerSettings.ParseNewInstance(stored)
        Assert.Equal(expected, r.Value)
        Assert.False(r.HasWarning)
    End Sub

    <Fact>
    Public Sub AnInvalidNewInstanceValue_IsAutoWithAWarning()
        Dim r = AdobeViewerSettings.ParseNewInstance("poate")
        Assert.Equal(AdobeNewInstanceMode.Auto, r.Value)
        Assert.True(r.HasWarning)
        Assert.Contains("AdobeNewInstance", r.Warning)
    End Sub

    ' ══ text round trip ════════════════════════════════════════════════════════
    <Theory>
    <InlineData(AdobeViewerMode.Auto)>
    <InlineData(AdobeViewerMode.Modern)>
    <InlineData(AdobeViewerMode.Classic)>
    Public Sub EveryMode_SurvivesTextRoundTrip(mode As AdobeViewerMode)
        Assert.Equal(mode, AdobeViewerSettings.ParseMode(AdobeViewerSettings.ModeToText(mode)).Value)
    End Sub

    <Theory>
    <InlineData(AdobeNewInstanceMode.Auto)>
    <InlineData(AdobeNewInstanceMode.Da)>
    <InlineData(AdobeNewInstanceMode.Nu)>
    Public Sub EveryNewInstanceMode_SurvivesTextRoundTrip(mode As AdobeNewInstanceMode)
        Assert.Equal(mode, AdobeViewerSettings.ParseNewInstance(AdobeViewerSettings.NewInstanceToText(mode)).Value)
    End Sub

    <Fact>
    Public Sub TheComboLabels_AreRomanian()
        Assert.Equal("Automat", AdobeViewerSettings.ModeLabel(AdobeViewerMode.Auto))
        Assert.Equal("Modern", AdobeViewerSettings.ModeLabel(AdobeViewerMode.Modern))
        Assert.Equal("Clasic", AdobeViewerSettings.ModeLabel(AdobeViewerMode.Classic))
        Assert.Equal("Automat", AdobeViewerSettings.NewInstanceLabel(AdobeNewInstanceMode.Auto))
        Assert.Equal("Da", AdobeViewerSettings.NewInstanceLabel(AdobeNewInstanceMode.Da))
        Assert.Equal("Nu", AdobeViewerSettings.NewInstanceLabel(AdobeNewInstanceMode.Nu))
    End Sub

    ' ══ file round trip ════════════════════════════════════════════════════════
    <Fact>
    Public Sub SavingThenLoading_KeepsBothSettings()
        Dim dir As String = NewTempDir()
        Try
            Dim p As New KBotPaths() With {
                .AdobeViewerMode = AdobeViewerSettings.ModeToText(AdobeViewerMode.Modern),
                .AdobeNewInstance = AdobeViewerSettings.NewInstanceToText(AdobeNewInstanceMode.Da)}
            Assert.True(p.Save(dir))

            Dim back = KBotPaths.Load(dir)
            Assert.Equal(AdobeViewerMode.Modern, AdobeViewerSettings.ParseMode(back.AdobeViewerMode).Value)
            Assert.Equal(AdobeNewInstanceMode.Da, AdobeViewerSettings.ParseNewInstance(back.AdobeNewInstance).Value)
            ' …and the setting that was already there is not lost by writing the new ones.
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, back.DdfPdfRoot)
        Finally
            CleanUp(dir)
        End Try
    End Sub

    <Fact>
    Public Sub AFileWithoutTheAdobeKeys_LoadsTheDefaults()
        ' Every installation upgrading from before slice 0024 has exactly this file.
        Dim dir As String = NewTempDir()
        Try
            File.WriteAllText(Path.Combine(dir, KBotPaths.FileName),
                              "{ ""DdfPdfRoot"": ""D:\\PDF\\DDF\\"" }")
            Dim p = KBotPaths.Load(dir)
            Assert.Equal("D:\PDF\DDF\", p.DdfPdfRoot)
            Assert.Equal(AdobeViewerMode.Auto, AdobeViewerSettings.ParseMode(p.AdobeViewerMode).Value)
            Assert.Equal(AdobeNewInstanceMode.Auto, AdobeViewerSettings.ParseNewInstance(p.AdobeNewInstance).Value)
        Finally
            CleanUp(dir)
        End Try
    End Sub

    <Fact>
    Public Sub ABrokenFile_LoadsDefaultsInsteadOfThrowing()
        Dim dir As String = NewTempDir()
        Try
            File.WriteAllText(Path.Combine(dir, KBotPaths.FileName), "{ nu e JSON")
            Dim p = KBotPaths.Load(dir)
            Assert.Equal(KBotPaths.DefaultAdobeViewerMode, p.AdobeViewerMode)
            Assert.Equal(KBotPaths.DefaultAdobeNewInstance, p.AdobeNewInstance)
        Finally
            CleanUp(dir)
        End Try
    End Sub

    <Fact>
    Public Sub AnInvalidStoredValue_SurvivesTheLoadAndIsRejectedAtParse()
        ' KBotPaths keeps the text verbatim (it cannot know the vocabulary — the enum lives in
        ' KBot.Controls); the fallback + warning happen where the meaning is known.
        Dim dir As String = NewTempDir()
        Try
            File.WriteAllText(Path.Combine(dir, KBotPaths.FileName),
                              "{ ""AdobeViewerMode"": ""turbo"" }")
            Dim p = KBotPaths.Load(dir)
            Assert.Equal("turbo", p.AdobeViewerMode)
            Dim r = AdobeViewerSettings.ParseMode(p.AdobeViewerMode)
            Assert.Equal(AdobeViewerMode.Auto, r.Value)
            Assert.True(r.HasWarning)
        Finally
            CleanUp(dir)
        End Try
    End Sub

    Private Shared Function NewTempDir() As String
        Dim dir As String = Path.Combine(Path.GetTempPath(), "kbot_adobe_settings_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(dir)
        Return dir
    End Function

    Private Shared Sub CleanUp(dir As String)
        Try
            If Directory.Exists(dir) Then Directory.Delete(dir, True)
        Catch
            ' A leftover temp folder is not worth failing a test over.
        End Try
    End Sub

End Class
