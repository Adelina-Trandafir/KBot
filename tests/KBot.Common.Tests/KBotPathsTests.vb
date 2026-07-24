Option Strict On
Imports System
Imports System.IO
Imports Xunit
Imports KBot.Common

' Tests for KBotPaths (slice 0020-04). The config is a JSON file next to the executable; a
' missing or malformed file MUST fall back to the default and never throw at startup. Load(dir)
' bypasses the singleton cache so each test points at its own temp directory.
Public Class KBotPathsTests

    Private Shared Function TempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "kbot_paths_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(d)
        Return d
    End Function

    <Fact>
    Public Sub Load_MissingFile_ReturnsDefault()
        Dim d = TempDir()
        Try
            ' Niciun kbot_paths.json în director -> valoarea implicită.
            Dim p = KBotPaths.Load(d)
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, p.DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Load_ValidJson_ReadsDdfPdfRoot()
        Dim d = TempDir()
        Try
            File.WriteAllText(Path.Combine(d, KBotPaths.FileName),
                              "{ ""DdfPdfRoot"": ""D:\\alt\\loc\\DDF\\"" }")
            Dim p = KBotPaths.Load(d)
            Assert.Equal("D:\alt\loc\DDF\", p.DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Load_MalformedJson_FallsBackToDefault_DoesNotThrow()
        Dim d = TempDir()
        Try
            File.WriteAllText(Path.Combine(d, KBotPaths.FileName), "{ not valid json ")
            Dim p = KBotPaths.Load(d)      ' nu aruncă
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, p.DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Load_EmptyFile_FallsBackToDefault()
        Dim d = TempDir()
        Try
            File.WriteAllText(Path.Combine(d, KBotPaths.FileName), "")
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, KBotPaths.Load(d).DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Load_JsonWithoutDdfPdfRoot_KeepsDefault()
        Dim d = TempDir()
        Try
            File.WriteAllText(Path.Combine(d, KBotPaths.FileName), "{ ""Altceva"": 1 }")
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, KBotPaths.Load(d).DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Load_BlankDdfPdfRoot_KeepsDefault()
        Dim d = TempDir()
        Try
            File.WriteAllText(Path.Combine(d, KBotPaths.FileName), "{ ""DdfPdfRoot"": ""   "" }")
            Assert.Equal(KBotPaths.DefaultDdfPdfRoot, KBotPaths.Load(d).DdfPdfRoot)
        Finally
            Directory.Delete(d, True)
        End Try
    End Sub

    <Fact>
    Public Sub Default_MatchesTheOperatorConfiguredPath()
        ' Decizia 13: rădăcina implicită e C:\AVACONT\FOREXE\PDF\DDF\.
        Assert.Equal("C:\AVACONT\FOREXE\PDF\DDF\", KBotPaths.DefaultDdfPdfRoot)
    End Sub

End Class
