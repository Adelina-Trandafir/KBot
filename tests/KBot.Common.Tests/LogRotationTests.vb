Option Strict On
Imports System.IO
Imports System.Text
Imports KBot.Common
Imports Xunit

''' <summary>
''' Testele 7–9 din planul feliei 0031: cititorul și rotația.
''' Fiecare test lucrează într-un director temporar propriu și îl șterge la final.
''' </summary>
Public Class LogRotationTests
    Implements IDisposable

    Private ReadOnly _dir As String

    Public Sub New()
        _dir = Path.Combine(Path.GetTempPath(), "kbot_log_tests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_dir)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            If Directory.Exists(_dir) Then Directory.Delete(_dir, recursive:=True)
        Catch ex As IOException
            ' Curățenia de test nu are voie să pice testul; un fișier încă deschis se va șterge
            ' oricum la următoarea golire a directorului temporar.
            Diagnostics.Trace.WriteLine("Curățenie de test eșuată: " & ex.Message)
        End Try
    End Sub

    Private Function PathIn(name As String) As String
        Return Path.Combine(_dir, name)
    End Function

    ' ── Testul 7: fișier prea mare -> trunchiere, prima linie parțială aruncată, BOM sărit ──

    <Fact>
    Public Sub Cititorul_FișierPesteFereastră_TaieȘiAruncăPrimaLinieParțială()
        Dim logFile As String = PathIn("mare.log")
        Dim sb As New StringBuilder()
        For i As Integer = 0 To 999
            sb.AppendLine("linia " & i.ToString("0000") & " cu ceva text ca să umplem fișierul")
        Next
        File.WriteAllText(logFile, sb.ToString(), New UTF8Encoding(True))

        Dim total As Long = New FileInfo(logFile).Length
        ' Fereastră mai mică decât fișierul, ca să cadă în mijlocul unei linii.
        Dim result As LogReadResult = LogFileReader.ReadTail(logFile, total \ 2)

        Assert.True(result.WasTruncated)
        Assert.Equal(total, result.FileLengthBytes)
        ' Prima linie păstrată e ÎNTREAGĂ: nu începe cu o bucată de linie tăiată.
        Dim firstLine As String = result.Text.Split(ControlChars.Lf)(0)
        Assert.StartsWith("linia ", firstLine)
        ' Și e chiar coada fișierului.
        Assert.Contains("linia 0999", result.Text)
    End Sub

    <Fact>
    Public Sub Cititorul_SareBomUlCândCitșteDeLaÎnceput()
        Dim logFile As String = PathIn("cu_bom.log")
        File.WriteAllText(logFile, "prima linie", New UTF8Encoding(True))

        Dim result As LogReadResult = LogFileReader.ReadTail(logFile)

        Assert.False(result.WasTruncated)
        ' Fără sărirea BOM-ului, textul ar începe cu U+FEFF și orice antet ar rata potrivirea.
        Assert.Equal("prima linie", result.Text)
        Assert.False(result.Text.Contains(ChrW(&HFEFF)))
    End Sub

    ' ── Testul 8: fișier ținut DESCHIS de un StreamWriter cu AutoFlush ───────

    <Fact>
    Public Sub Cititorul_CiteșteUnFișierȚinutDeschisDeRunLogger()
        ' Cazul RunLogger: jurnalul rulării CURENTE e deschis cu AutoFlush cât timp rulează
        ' bancul. Un File.ReadAllText ar pica aici — de asta cititorul cere FileShare.ReadWrite.
        Dim logFile As String = PathIn("test_deschis.log")
        Using writer As New StreamWriter(logFile, append:=False,
                                         encoding:=New UTF8Encoding(True)) With {.AutoFlush = True}
            writer.WriteLine("=== rulare în curs ===")
            writer.WriteLine("[PASSED] Test1  (1 ms)  ok")

            Dim result As LogReadResult = LogFileReader.ReadTail(logFile)

            Assert.Contains("rulare în curs", result.Text)
            Assert.Contains("[PASSED]", result.Text)
        End Using
    End Sub

    ' ── Testul 9: rotația ────────────────────────────────────────────────────

    <Fact>
    Public Sub Rotația_SubLimită_NuAtingeNimicȘiÎntoarceFalse()
        Dim logFile As String = PathIn("mic.log")
        File.WriteAllText(logFile, "conținut scurt")
        Dim before As Byte() = File.ReadAllBytes(logFile)

        Dim rolled As Boolean = LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=5)

        Assert.False(rolled)
        Assert.Equal(before, File.ReadAllBytes(logFile))
        Assert.False(File.Exists(logFile & ".1"))
    End Sub

    <Fact>
    Public Sub Rotația_FișierLipsă_ÎntoarceFalseFărăSăArunce()
        Assert.False(LogRotation.Roll(PathIn("nu_există.log"), maxBytes:=10L, backupCount:=5))
    End Sub

    <Fact>
    Public Sub Rotația_PesteLimită_MutăFișierulViuÎnPunctUnu()
        Dim logFile As String = PathIn("mare.log")
        File.WriteAllText(logFile, New String("x"c, 2048))

        Dim rolled As Boolean = LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=5)

        Assert.True(rolled)
        ' Fișierul viu NU mai există: apelantul îl recreează prin adăugarea lui obișnuită.
        Assert.False(File.Exists(logFile))
        Assert.True(File.Exists(logFile & ".1"))
        Assert.Equal(2048, File.ReadAllText(logFile & ".1").Length)
    End Sub

    <Fact>
    Public Sub Rotația_CinciGenerații_SeDeplaseazăȘiCeaMaiVecheIese()
        Dim logFile As String = PathIn("rotit.log")
        ' Generațiile existente, fiecare cu un conținut recunoscibil.
        For generation As Integer = 1 To 5
            File.WriteAllText(logFile & "." & generation.ToString(), "gen" & generation.ToString())
        Next
        File.WriteAllText(logFile, New String("v"c, 2048))

        Dim rolled As Boolean = LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=5)

        Assert.True(rolled)
        ' viu -> .1, .1 -> .2, … , .4 -> .5, iar vechiul .5 a IEȘIT din istoric.
        Assert.Equal(New String("v"c, 2048), File.ReadAllText(logFile & ".1"))
        Assert.Equal("gen1", File.ReadAllText(logFile & ".2"))
        Assert.Equal("gen2", File.ReadAllText(logFile & ".3"))
        Assert.Equal("gen3", File.ReadAllText(logFile & ".4"))
        Assert.Equal("gen4", File.ReadAllText(logFile & ".5"))
        ' NICIODATĂ o a șasea generație.
        Assert.False(File.Exists(logFile & ".6"))
    End Sub

    <Fact>
    Public Sub Rotația_RespectăUnBackupCountMaiMic()
        Dim logFile As String = PathIn("doi.log")
        File.WriteAllText(logFile & ".1", "gen1")
        File.WriteAllText(logFile & ".2", "gen2")
        File.WriteAllText(logFile, New String("v"c, 2048))

        Assert.True(LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=2))

        Assert.Equal(New String("v"c, 2048), File.ReadAllText(logFile & ".1"))
        Assert.Equal("gen1", File.ReadAllText(logFile & ".2"))
        Assert.False(File.Exists(logFile & ".3"))
    End Sub

    <Fact>
    Public Sub Rotația_FișierBlocat_ÎntoarceFalse_NuAruncăȘiNuPierdeDate()
        Dim logFile As String = PathIn("blocat.log")
        File.WriteAllText(logFile, New String("x"c, 2048))

        ' Blocare EXCLUSIVĂ: redenumirea nu are cum să reușească.
        Using locked As New FileStream(logFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            Dim rolled As Boolean = LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=5)

            Assert.False(rolled)
        End Using

        ' Datele sunt intacte: rotația eșuată NU costă nimic.
        Assert.True(File.Exists(logFile))
        Assert.Equal(2048, File.ReadAllText(logFile).Length)
        Assert.False(File.Exists(logFile & ".1"))
    End Sub

    <Fact>
    Public Sub Rotația_BackupCountSubUnu_RefuzăSăDistrugăIstoricul()
        Dim logFile As String = PathIn("zero.log")
        File.WriteAllText(logFile, New String("x"c, 2048))

        Assert.False(LogRotation.Roll(logFile, maxBytes:=1024L, backupCount:=0))

        Assert.True(File.Exists(logFile))
        Assert.Equal(2048, File.ReadAllText(logFile).Length)
    End Sub

End Class
