Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Xunit
Imports KBot.Common

''' <summary>
''' Decizia D-O — folderele sunt setări ale operatorului, iar <c>KBotPaths</c> e singurul
''' loc care rezolvă o cale.
'''
''' <para>
''' Testele lucrează pe un director TEMPORAR, dat explicit lui <c>Incarca</c>, deci nu ating
''' <c>%APPDATA%</c>-ul mașinii și nu contaminează singleton-ul procesului.
''' </para>
''' </summary>
Public Class SetariFoldereTests

    Private Shared Function TempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "kbot_set_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(d)
        Return d
    End Function

    Private Shared Sub Scrie(dir As String, json As String)
        File.WriteAllText(Path.Combine(dir, SetariFoldere.NumeFisier), json)
    End Sub

    ' ── Implicitele ──────────────────────────────────────────────────────

    <Fact>
    Public Sub FisierAbsent_DaImplicitele()
        ' Comportamentul de AZI e ce primește un operator care nu configurează nimic.
        Dim s As SetariFoldere = SetariFoldere.Incarca(TempDir())

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Logs"),
                     s.Cale(SetariFoldere.CheieLogs))
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Asociere"),
                     s.Cale(SetariFoldere.CheieAsociere))
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "WorkflowResults"),
                     s.Cale(SetariFoldere.CheieWorkflowResults))
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "TempPdf"),
                     s.Cale(SetariFoldere.CheieTempPdf))
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Workflows"),
                     s.Cale(SetariFoldere.CheieWorkflows))
        Assert.Empty(s.Probleme)
    End Sub

    <Fact>
    Public Sub FisierGol_DaImplicitele()
        Dim dir As String = TempDir()
        Scrie(dir, "")
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Logs"),
                     SetariFoldere.Incarca(dir).Cale(SetariFoldere.CheieLogs))
    End Sub

    <Fact>
    Public Sub CheieGoala_DaImplicitul()
        ' O valoare goală înseamnă «nu am configurat», nu «folderul rădăcină».
        Dim dir As String = TempDir()
        Scrie(dir, "{""Logs"": ""   ""}")
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Logs"),
                     SetariFoldere.Incarca(dir).Cale(SetariFoldere.CheieLogs))
    End Sub

    <Fact>
    Public Sub CheieLipsa_DaImplicitul_SiNuAtingeCelelalte()
        Dim dir As String = TempDir()
        Scrie(dir, "{""Logs"": ""D:\\Jurnale""}")
        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)

        Assert.Equal("D:\Jurnale", s.Cale(SetariFoldere.CheieLogs))
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Asociere"),
                     s.Cale(SetariFoldere.CheieAsociere))
    End Sub

    ' ── Rezolvarea ───────────────────────────────────────────────────────

    <Fact>
    Public Sub CaleAbsoluta_SeFoloseșteCaAtare()
        Dim dir As String = TempDir()
        Scrie(dir, "{""Asociere"": ""D:\\KBOT\\Asocieri""}")
        Assert.Equal("D:\KBOT\Asocieri",
                     SetariFoldere.Incarca(dir).Cale(SetariFoldere.CheieAsociere))
    End Sub

    <Fact>
    Public Sub CaleRelativa_SeRezolvaFataDeDirectorulAplicatiei()
        Dim dir As String = TempDir()
        Scrie(dir, "{""Asociere"": ""date\\asocieri""}")
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "date\asocieri"),
                     SetariFoldere.Incarca(dir).Cale(SetariFoldere.CheieAsociere))
    End Sub

    <Fact>
    Public Sub Bruta_SpuneCeAScrisOperatorul_NuCeIese()
        ' Formularul de setări are nevoie de asta: câmpul se arată GOL când operatorul nu
        ' a configurat nimic, nu preumplut cu implicitul rezolvat.
        Dim dir As String = TempDir()
        Scrie(dir, "{""Logs"": ""D:\\Jurnale""}")
        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)

        Assert.Equal("D:\Jurnale", s.Bruta(SetariFoldere.CheieLogs))
        Assert.Null(s.Bruta(SetariFoldere.CheieAsociere))
    End Sub

    <Fact>
    Public Sub CheieNecunoscutaInCod_Ridica()
        ' Fără no-op-uri tăcute: o cheie pe care nimeni nu a declarat-o e o greșeală de
        ' programare, nu o setare lipsă.
        Assert.Throws(Of ArgumentException)(
            Function() SetariFoldere.Incarca(TempDir()).Cale("NuExista"))
    End Sub

    ' ── Ce se spune despre un fișier stricat ─────────────────────────────

    <Fact>
    Public Sub JsonStricat_DaImplicitele_SiSpuneDeCe()
        Dim dir As String = TempDir()
        Scrie(dir, "{ asta nu e JSON")
        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Logs"),
                     s.Cale(SetariFoldere.CheieLogs))
        Assert.Single(s.Probleme)
        Assert.Contains(SetariFoldere.NumeFisier, s.Probleme(0))
    End Sub

    <Fact>
    Public Sub CheieNecunoscutaInFisier_SeIgnora_SiSeSpune()
        Dim dir As String = TempDir()
        Scrie(dir, "{""Logs"": ""D:\\J"", ""Altceva"": ""x""}")
        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)

        Assert.Equal("D:\J", s.Cale(SetariFoldere.CheieLogs))
        Assert.Contains(s.Probleme, Function(p) p.Contains("Altceva"))
    End Sub

    <Fact>
    Public Sub ValoareCareNuEText_CadePeImplicit_SiSeSpune()
        Dim dir As String = TempDir()
        Scrie(dir, "{""Logs"": 17}")
        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "Logs"),
                     s.Cale(SetariFoldere.CheieLogs))
        Assert.Contains(s.Probleme, Function(p) p.Contains("Logs"))
    End Sub

    <Fact>
    Public Sub Incarca_NuLogheazaNiciodata()
        ' Buclă: GlobalErrorLog scrie în folderul de jurnale, iar folderul de jurnale se
        ' află tocmai de aici. De-aia problemele se strâng într-o listă, nu se loghează.
        ' Testul e pe contract, nu pe implementare: un fișier stricat trebuie să iasă prin
        ' `Probleme`, nu prin altă parte.
        Dim dir As String = TempDir()
        Scrie(dir, "{ stricat")
        Assert.NotEmpty(SetariFoldere.Incarca(dir).Probleme)
    End Sub

    ' ── Validarea de pornire ─────────────────────────────────────────────

    <Fact>
    Public Sub Valideaza_CreeazaFoldereleInCareSeScrie()
        Dim dir As String = TempDir()
        Dim tinta As String = Path.Combine(TempDir(), "inca_inexistent")
        Scrie(dir, "{""Asociere"": " & Json(tinta) & "}")

        SetariFoldere.Incarca(dir).Valideaza()

        Assert.True(Directory.Exists(tinta))
    End Sub

    <Fact>
    Public Sub Valideaza_NuInventeazaUnFolderDoarCitit()
        ' `Workflows` nu se scrie de aplicație. Dacă lipsește, un folder gol creat de noi
        ' ar ascunde o instalare incompletă în loc să o arate.
        Dim dir As String = TempDir()
        Dim tinta As String = Path.Combine(TempDir(), "wfl_inexistent")
        Scrie(dir, "{""Workflows"": " & Json(tinta) & "}")

        SetariFoldere.Incarca(dir).Valideaza()

        Assert.False(Directory.Exists(tinta))
    End Sub

    <Fact>
    Public Sub Valideaza_OCaleImposibila_RidicaSiNumesteSetareaSiCalea()
        ' Niciodată o cădere tăcută pe implicit. Un caracter interzis în nume face calea
        ' imposibil de creat pe Windows, oricare ar fi drepturile.
        Dim dir As String = TempDir()
        Dim tinta As String = Path.Combine(TempDir(), "nu|se|poate")
        Scrie(dir, "{""Logs"": " & Json(tinta) & "}")

        Dim ex As SetariFoldereException = Assert.Throws(Of SetariFoldereException)(
            Sub() SetariFoldere.Incarca(dir).Valideaza())

        Assert.Equal(SetariFoldere.CheieLogs, ex.Setare.Cheie)
        Assert.Contains(SetariFoldere.CheieLogs, ex.Message)
        Assert.Contains("nu|se|poate", ex.Message)
        ' Mesajul spune și CE să facă, nu doar că ceva n-a mers.
        Assert.Contains(SetariFoldere.NumeFisier, ex.Message)
    End Sub

    ' ── Scrierea ─────────────────────────────────────────────────────────

    <Fact>
    Public Sub Salveaza_DusIntors()
        Dim dir As String = TempDir()
        SetariFoldere.Salveaza(New Dictionary(Of String, String) From {
            {SetariFoldere.CheieLogs, "D:\Jurnale"},
            {SetariFoldere.CheieAsociere, "  "}}, dir)

        Dim s As SetariFoldere = SetariFoldere.Incarca(dir)
        Assert.Equal("D:\Jurnale", s.Bruta(SetariFoldere.CheieLogs))
        ' O valoare goală NU se scrie: cheia absentă înseamnă deja «implicit».
        Assert.Null(s.Bruta(SetariFoldere.CheieAsociere))
        Assert.Empty(s.Probleme)
    End Sub

    <Fact>
    Public Sub Salveaza_NuScrieCheiNecunoscute()
        Dim dir As String = TempDir()
        SetariFoldere.Salveaza(New Dictionary(Of String, String) From {
            {"CevaInventat", "D:\x"}}, dir)

        Assert.Empty(SetariFoldere.Incarca(dir).Probleme)
    End Sub

    ' ── Lista în sine ────────────────────────────────────────────────────

    <Fact>
    Public Sub ToateSetarile_AuCheieUnicaImplicitSiDescriere()
        Assert.Equal(SetariFoldere.Toate.Count,
                     SetariFoldere.Toate.Select(Function(x) x.Cheie).Distinct().Count())
        For Each setare As SetariFoldere.Setare In SetariFoldere.Toate
            Assert.False(String.IsNullOrWhiteSpace(setare.Cheie))
            Assert.False(String.IsNullOrWhiteSpace(setare.Implicit))
            ' Descrierea ajunge sub ochii operatorului, deci nu poate lipsi.
            Assert.False(String.IsNullOrWhiteSpace(setare.Descriere))
        Next
    End Sub

    <Fact>
    Public Sub SetarileStauInAppData()
        Assert.Equal(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "AVACONT", "KBot", "settings.json"),
            SetariFoldere.CaleSetari())
    End Sub

    Private Shared Function Json(cale As String) As String
        Return """" & cale.Replace("\", "\\") & """"
    End Function
End Class
