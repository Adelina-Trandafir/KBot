Option Strict On
Imports System
Imports System.IO
Imports System.Text.Json
Imports Xunit
Imports KBot.App
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

' Tests for ForexeRunDump (slice 0054) — the black box of the downloader. The point of the
' class is that a run which brought back NOTHING still leaves something to read, so most of
' these check exactly that: no result at all, a failed result, a result whose table is
' missing. The happy path is checked too, because the dump must not lose what did arrive.
'
' The dump writes next to the test assembly (WorkflowResults\Runs\), so every test deletes
' its own folder afterwards.
Public Class ForexeRunDumpTests

    Private Shared Function Sesiune() As SessionContext
        Return New SessionContext With {
            .DbName = "000_TEST",
            .An = 2026,
            .SectorSursa = "02A"
        }
    End Function

    ' A result with one two-row table and one scalar, as the runner would hand it over.
    Private Shared Function Rezultat(succes As Boolean) As JobResult
        Dim tabel As New TabelRezultat From {
            New RandTabel From {{"Cod", "AAB2MAACHXB"}, {"Suma", "100"}},
            New RandTabel From {{"Cod", "AAB362H6KTM"}, {"Suma", "250"}}
        }
        Dim r As New JobResult With {
            .Success = succes,
            .Message = If(succes, "'Prelucrare Completa' rulat.", "sesiune pierduta")
        }
        r.Tables("LISTA") = tabel
        r.Data("LISTA") = "[{""Cod"":""AAB2MAACHXB""}]"
        r.Data("TotalPlati") = "350"
        Return r
    End Function

    Private Shared Function CitesteInfo(folder As String) As JsonElement
        Return JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "run.json"))).RootElement
    End Function

    Private Shared Sub Curata(folder As String)
        If Not String.IsNullOrEmpty(folder) AndAlso Directory.Exists(folder) Then
            Directory.Delete(folder, recursive:=True)
        End If
    End Sub

    <Fact>
    Public Sub Save_FaraRezultat_ScrieTotusiRunJson()
        Dim dump As New ForexeRunDump("PrelucrareCompleta", "AAB2MAACHXB", Sesiune())
        dump.Note("motiv", "O alta operatie FOREXE era deja in curs.")
        Dim folder As String = dump.Save("ocupat", Nothing)
        Try
            Assert.False(String.IsNullOrEmpty(folder))
            Assert.True(File.Exists(Path.Combine(folder, "run.json")))
            ' Nimic nu a rulat: nu se inventeaza fisiere de date.
            Assert.False(File.Exists(Path.Combine(folder, "raw.json")))
            Assert.False(File.Exists(Path.Combine(folder, "tables.json")))

            Dim info As JsonElement = CitesteInfo(folder)
            Assert.Equal("ocupat", info.GetProperty("Outcome").GetString())
            Assert.Equal("AAB2MAACHXB", info.GetProperty("Code").GetString())
            Assert.Equal("000_TEST", info.GetProperty("DbName").GetString())
            Assert.Equal(2026, info.GetProperty("FiscalYear").GetInt32())
            Assert.Equal("O alta operatie FOREXE era deja in curs.",
                         info.GetProperty("Notes").GetProperty("motiv").GetString())
            Assert.False(info.GetProperty("Success").GetBoolean())
        Finally
            Curata(folder)
        End Try
    End Sub

    <Fact>
    Public Sub Save_RezultatEsuat_PastreazaBrutulSiMesajul()
        Dim dump As New ForexeRunDump("PrelucrareCompleta", "AAB2MAACHXB", Sesiune())
        Dim folder As String = dump.Save("esuat", Rezultat(succes:=False))
        Try
            Dim info As JsonElement = CitesteInfo(folder)
            Assert.Equal("esuat", info.GetProperty("Outcome").GetString())
            Assert.False(info.GetProperty("Success").GetBoolean())
            Assert.Equal("sesiune pierduta", info.GetProperty("Message").GetString())
            ' Un esec are cu atat mai multa nevoie de variabilele brute: acolo se vede ce a
            ' apucat robotul sa citeasca inainte sa cada.
            Assert.True(File.Exists(Path.Combine(folder, "raw.json")))
            Assert.Contains("TotalPlati", File.ReadAllText(Path.Combine(folder, "raw.json")))
        Finally
            Curata(folder)
        End Try
    End Sub

    <Fact>
    Public Sub Save_RezultatReusit_ScrieBrutTabeleSiMapat()
        Dim dump As New ForexeRunDump("PrelucrareCompleta", "AAB2MAACHXB", Sesiune())
        dump.NoteRequest(New JobRequest With {
            .WorkflowName = "Prelucrare Completa",
            .WflPath = "C:\AVACONT\FOREXE\WFL\prelucrare.wfl",
            .Parameters = New Dictionary(Of String, String) From {{"COD", "AAB2MAACHXB"}}
        })
        Dim pachet As New PrelucrareRezultat With {.CodAngajament = "AAB2MAACHXB"}
        Dim folder As String = dump.Save("ok", Rezultat(succes:=True), pachet)
        Try
            Assert.True(File.Exists(Path.Combine(folder, "raw.json")))
            Assert.True(File.Exists(Path.Combine(folder, "tables.json")))
            Assert.True(File.Exists(Path.Combine(folder, "mapped.json")))

            Dim info As JsonElement = CitesteInfo(folder)
            Assert.True(info.GetProperty("Success").GetBoolean())
            Assert.Equal(2, info.GetProperty("TableRowCounts").GetProperty("LISTA").GetInt32())
            Assert.Equal("Prelucrare Completa", info.GetProperty("WorkflowName").GetString())
            Assert.Equal("AAB2MAACHXB", info.GetProperty("Parameters").GetProperty("COD").GetString())
            ' Cheia care a devenit tabel NU se numara si printre scalari.
            Dim scalari As String = info.GetProperty("ScalarKeys").ToString()
            Assert.Contains("TotalPlati", scalari)
            Assert.DoesNotContain("LISTA", scalari)
        Finally
            Curata(folder)
        End Try
    End Sub

    ' A nested cell (ListaReceptii.Detaliu) used to kill the whole dump: System.Text.Json
    ' walked CelulaTabel's public properties and hit `Text`, which throws by design on
    ' anything but a scalar. The run that most needs a black box -- the one with nested
    ' rows -- was the one that left none. It goes through TabeleJson.Catre now.
    <Fact>
    Public Sub Save_CuCelulaImbricata_ScrieTabeleleCaArbore()
        Dim detaliu As CelulaTabel = CelulaTabel.DinLista({
            CelulaTabel.DinObiect({New KeyValuePair(Of String, CelulaTabel)(
                "Suma", CelulaTabel.DinText("100"))}),
            CelulaTabel.DinObiect({New KeyValuePair(Of String, CelulaTabel)(
                "Suma", CelulaTabel.DinText("250"))})})
        Dim r As New JobResult With {.Success = True, .Message = "ok"}
        r.Tables("ListaReceptii") = New TabelRezultat From {
            New RandTabel From {{"Cod", "AAB2MAACHXB"}, {"Detaliu", detaliu}}}

        Dim dump As New ForexeRunDump("PrelucrareCompleta", "AAB2MAACHXB", Sesiune())
        Dim folder As String = dump.Save("ok", r)
        Try
            Dim cale As String = Path.Combine(folder, "tables.json")
            Assert.True(File.Exists(cale))
            Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(cale))
                Dim rand As JsonElement = doc.RootElement.GetProperty("ListaReceptii")(0)
                Assert.Equal("AAB2MAACHXB", rand.GetProperty("Cod").GetString())
                ' A real JSON array, not a string that happens to contain JSON.
                Dim d As JsonElement = rand.GetProperty("Detaliu")
                Assert.Equal(JsonValueKind.Array, d.ValueKind)
                Assert.Equal(2, d.GetArrayLength())
                Assert.Equal("250", d(1).GetProperty("Suma").GetString())
            End Using
        Finally
            Curata(folder)
        End Try
    End Sub

    <Fact>
    Public Sub Folder_PoartaCodulSiOperatia()
        Dim dump As New ForexeRunDump("PrelucrareCompleta", "AAB2MAACHXB", Sesiune())
        Dim nume As String = Path.GetFileName(dump.Folder)
        Assert.EndsWith("_PrelucrareCompleta_AAB2MAACHXB", nume)
        Assert.StartsWith(ForexeRunDump.RunsFolder, dump.Folder)
        ' Numele se compune, dar folderul se creeaza abia la Save.
        Assert.False(Directory.Exists(dump.Folder))
    End Sub

    <Fact>
    Public Sub Save_ListaFaraCod_NuLasaLiniuteInNume()
        Dim dump As New ForexeRunDump("ListaAngajamente", String.Empty, Sesiune())
        Dim nume As String = Path.GetFileName(dump.Folder)
        Assert.EndsWith("_ListaAngajamente", nume)
        Assert.DoesNotContain("__", nume)
    End Sub

End Class
