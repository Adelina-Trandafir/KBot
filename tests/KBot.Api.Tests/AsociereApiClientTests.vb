Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Offline tests for the two-phase ingest calls (slice 0048-03). A stub HttpMessageHandler
' captures the request body and returns a configured response -- no network, no server.
'
' What these pin is the WIRE, in both directions:
'   * the proposal body comes back as domain POCOs, dates and all;
'   * the save body goes out under the exact JSON keys prelucrare_asociere.py reads.
'
' The single most load-bearing assertion is that `mod` reaches the server. A save that
' silently travelled as a proposal would return 200, write nothing, and look like success.
Public Class AsociereApiClientTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastBody As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastBody = If(request.Content IsNot Nothing,
                          request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult(),
                          Nothing)
            Return Task.FromResult(New HttpResponseMessage(Status) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class

    Private Shared Function NewClient(handler As StubHandler) As ApiClient
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        Dim session As New SessionContext() With {.Token = "tok-opaque-123"}
        Return New ApiClient(http, New ApiOptions(), session)
    End Function

    Private Shared Function Pachet() As PrelucrareRezultat
        Dim r As New PrelucrareRezultat() With {
            .CodAngajament = "AAB37CNBK95",
            .Moment = New Date(2026, 8, 26, 10, 0, 0),
            .Workflow = "adlop - Prelucrare Completa.wfl"}
        r.Scalari("DescriereAngajament") = "2026 - NOVA WATER"
        Return r
    End Function

    ' Corpul propunerii, in forma exacta pe care o scrie routes/forexe/prelucrare.py:
    ' datele ies prin `default=str`, deci "2026-02-11 00:00:00", nu ISO cu «T».
    Private Const CORP_PROPUNERE As String = "{
  ""cod"": ""AAB37CNBK95"",
  ""faza"": ""propunere"",
  ""amprenta"": ""a1b2c3"",
  ""receptii"": [
    {""idrr"": 271, ""data_r"": ""2026-02-11 00:00:00"", ""suma_antet"": 510.0,
     ""descriere"": ""PLATA FACT."", ""sters"": false, ""reconstituit"": false,
     ""rhr"": [{""cod_indicator"": ""AAB"", ""cod_ai"": ""AAB37CNBK95-AAB"",
                ""cod_ssi"": ""02E650301200301"", ""credit_bugetar"": 10502.19,
                ""valoare"": 510.0, ""valoare_n"": 0.0}]}
  ],
  ""instantanee"": [
    {""rand_istoric"": 9, ""data_h"": ""2026-02-10 22:46:54"", ""descriere"": ""PLATA FACT."",
     ""total"": 510.0, ""stergere"": false, ""sugestie_idrr"": 271,
     ""sugestie_automata"": true,
     ""linii"": [{""cod_indicator"": ""AAB"", ""cod_ai"": ""AAB37CNBK95-AAB"",
                  ""cod_ssi"": ""02E650301200301"", ""id_clsf"": 1204, ""valoare"": 510.0}]},
    {""rand_istoric"": 41, ""data_h"": ""2026-05-28 20:11:34"", ""descriere"": ""Plata ces"",
     ""total"": 7150.0, ""stergere"": true, ""sugestie_idrr"": null,
     ""sugestie_automata"": false, ""linii"": []}
  ],
  ""are"": {""Receptii"": true, ""Plati"": false},
  ""scrise"": {""FX_Istoric"": 44},
  ""avertismente"": [""Pasul 8 nu s-a executat.""]
}"

    ' ── Faza unu ────────────────────────────────────────────────────────────────
    <Fact>
    Public Async Function Propunerea_Se_Citeste_Intreaga() As Task
        Dim h As New StubHandler() With {.ResponseBody = CORP_PROPUNERE}
        Dim raspuns As PrelucrareRaspuns =
            Await NewClient(h).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)

        Assert.Equal(PrelucrareStare.Propunere, raspuns.Stare)
        Dim p As PrelucrarePropunere = raspuns.Propunere
        Assert.NotNull(p)
        Assert.Equal("a1b2c3", p.Amprenta)
        Assert.Equal("AAB37CNBK95", p.CodAngajament)

        Assert.Single(p.Receptii)
        Assert.Equal(271, p.Receptii(0).Idrr)
        Assert.Equal(New Date(2026, 2, 11), p.Receptii(0).DataR)
        Assert.Equal(510.0, p.Receptii(0).SumaAntet)
        Assert.Single(p.Receptii(0).Rhr)
        Assert.Equal(10502.19, p.Receptii(0).Rhr(0).CreditBugetar)

        Assert.Equal(2, p.Instantanee.Count)
        Assert.True(p.Are("Receptii"))
        Assert.Equal(44, p.Scrise("FX_Istoric"))
        Assert.Single(p.Avertismente)
    End Function

    <Fact>
    Public Async Function Ora_Instantaneului_Supravietuieste_La_Secunda() As Task
        ' Vetoul de data (F13) compara TIMESTAMP-uri COMPLETE, fiindca operatorii salveaza
        ' aceeasi receptie de mai multe ori intr-un minut. O data trunchiata la zi in
        ' drumul asta ar dezarma tacut chiar paza contra asocierii gresite.
        Dim h As New StubHandler() With {.ResponseBody = CORP_PROPUNERE}
        Dim raspuns As PrelucrareRaspuns =
            Await NewClient(h).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)
        Assert.Equal(New Date(2026, 2, 10, 22, 46, 54), raspuns.Propunere.Instantanee(0).DataH)
    End Function

    <Fact>
    Public Async Function Sugestia_Lipsa_Devine_Zero_Si_Nu_Automata() As Task
        ' Serverul trimite null cand trecerea automata nu a avut raspuns. Zero ar fi o
        ' receptie; steagul e cel care spune care e care.
        Dim h As New StubHandler() With {.ResponseBody = CORP_PROPUNERE}
        Dim raspuns As PrelucrareRaspuns =
            Await NewClient(h).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)
        Dim fara As InstantaneuPropus = raspuns.Propunere.Instantanee(1)
        Assert.Equal(0, fara.SugestieIdrr)
        Assert.False(fara.SugestieAutomata)
        Assert.True(fara.Stergere)
    End Function

    <Fact>
    Public Async Function Propunerea_Trimite_Modul_Si_Nimic_Din_Faza_Doi() As Task
        Dim h As New StubHandler() With {.ResponseBody = CORP_PROPUNERE}
        Await NewClient(h).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Assert.Equal("propunere", doc.RootElement.GetProperty("mod").GetString())
            ' `amprenta` si `decizii` apartin doar salvarii si nu au ce cauta aici.
            Assert.False(doc.RootElement.TryGetProperty("amprenta", Nothing))
            Assert.False(doc.RootElement.TryGetProperty("decizii", Nothing))
        End Using
    End Function

    <Fact>
    Public Async Function Un_409_De_Alegere_Vine_Ca_Stare_Nu_Ca_Exceptie() As Task
        ' ALEGERE_UNITATE se poate declansa SI in faza intai: un angajament poate avea
        ' nevoie de doua drumuri dus-intors inainte ca operatorul sa vada formularul.
        Dim h As New StubHandler() With {
            .Status = HttpStatusCode.Conflict,
            .ResponseBody = "{""error"": ""Alegeți unitatea."", ""reason"": ""ALEGERE_UNITATE""," &
                            """cod"": ""AAB37CNBK95"", ""alegeri_necesare"": [" &
                            "{""ss"": ""02E"", ""clsfe"": ""200101"", ""clsf"": ""02E- 65."", " &
                            """cod_indicator"": ""AAB"", ""indicatori"": [""AAB""]," &
                            """unitati"": [{""id_unitate"": 76, ""detalii"": ""ENERGETIC ISJ""}]}]}"}
        Dim raspuns As PrelucrareRaspuns =
            Await NewClient(h).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)
        Assert.Equal(PrelucrareStare.AlegereUnitate, raspuns.Stare)
        Assert.Single(raspuns.AlegeriNecesare)
        Assert.Null(raspuns.Propunere)
    End Function

    ' ── Faza doi ────────────────────────────────────────────────────────────────
    Private Shared Function Decizii() As List(Of DecizieAsociere)
        Return New List(Of DecizieAsociere) From {
            New DecizieAsociere() With {
                .RandIstoric = 9, .DataH = New Date(2026, 2, 10, 22, 46, 54),
                .Actiune = ActiuneAsociere.Asociat, .Idrr = 271},
            New DecizieAsociere() With {
                .RandIstoric = 21, .DataH = New Date(2026, 3, 30, 22, 22, 23),
                .Actiune = ActiuneAsociere.Ignorat},
            New DecizieAsociere() With {
                .RandIstoric = 31, .DataH = New Date(2026, 1, 5, 9, 0, 0),
                .Actiune = ActiuneAsociere.Reconstituire, .ReceptieNoua = "R1"},
            New DecizieAsociere() With {
                .RandIstoric = 38, .DataH = New Date(2026, 3, 1, 10, 0, 0),
                .Actiune = ActiuneAsociere.Stergere, .ReceptieNoua = "R1"}}
    End Function

    <Fact>
    Public Async Function Salvarea_Trimite_Cheile_Pe_Care_Serverul_Le_Citeste() As Task
        Dim h As New StubHandler() With {
            .ResponseBody = "{""cod"": ""AAB37CNBK95"", ""faza"": ""salvare""," &
                            """are"": {}, ""scrise"": {}, ""avertismente"": []}"}
        Await NewClient(h).SalveazaAsociereaAsync(Pachet(), "a1b2c3", Decizii(), Nothing,
                                                  CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Dim root As JsonElement = doc.RootElement
            Assert.Equal("salvare", root.GetProperty("mod").GetString())
            Assert.Equal("a1b2c3", root.GetProperty("amprenta").GetString())

            Dim d As JsonElement = root.GetProperty("decizii")
            Assert.Equal(4, d.GetArrayLength())

            ' asociat: `idrr`, fara eticheta.
            Assert.Equal(9, d(0).GetProperty("rand_istoric").GetInt32())
            Assert.Equal("asociat", d(0).GetProperty("actiune").GetString())
            Assert.Equal(271, d(0).GetProperty("idrr").GetInt32())
            Assert.False(d(0).TryGetProperty("receptie_noua", Nothing))

            ' ignorat: NICIUNA dintre cele doua tinte. Un `idrr: 0` ar fi citit ca
            ' «receptia zero», nu ca «niciuna» — de-asta campul e nulabil pe fir.
            Assert.Equal("ignorat", d(1).GetProperty("actiune").GetString())
            Assert.False(d(1).TryGetProperty("idrr", Nothing))
            Assert.False(d(1).TryGetProperty("receptie_noua", Nothing))

            ' reconstituire si stergere: eticheta, fara idrr.
            Assert.Equal("reconstituire", d(2).GetProperty("actiune").GetString())
            Assert.Equal("R1", d(2).GetProperty("receptie_noua").GetString())
            Assert.False(d(2).TryGetProperty("idrr", Nothing))
            Assert.Equal("stergere", d(3).GetProperty("actiune").GetString())
            Assert.Equal("R1", d(3).GetProperty("receptie_noua").GetString())
        End Using
    End Function

    <Fact>
    Public Async Function Data_Deciziei_Pleaca_Cu_Secunde_Si_Fara_Locale() As Task
        ' Serverul compara `data_h` cu randul aflat la acel indice, la SECUNDA. O data
        ' trimisa in formatul masinii (sau fara ora) ar face fiecare salvare sa cada cu
        ' «fișier învechit», si nimeni nu ar sti de ce.
        Dim h As New StubHandler() With {
            .ResponseBody = "{""cod"": ""x"", ""are"": {}, ""scrise"": {}, ""avertismente"": []}"}
        Await NewClient(h).SalveazaAsociereaAsync(Pachet(), "a1b2c3", Decizii(), Nothing,
                                                  CancellationToken.None)
        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Assert.Equal("2026-02-10T22:46:54",
                         doc.RootElement.GetProperty("decizii")(0).GetProperty("data_h").GetString())
        End Using
    End Function

    <Fact>
    Public Async Function Salvarea_Fara_Amprenta_Este_Refuzata_Inainte_De_Retea() As Task
        Dim h As New StubHandler()
        Await Assert.ThrowsAsync(Of ArgumentException)(
            Function() NewClient(h).SalveazaAsociereaAsync(Pachet(), "  ", Decizii(), Nothing,
                                                           CancellationToken.None))
        Assert.Null(h.LastBody)      ' nici macar nu s-a trimis
    End Function

    <Fact>
    Public Async Function Stare_Modificata_Ajunge_Ca_Exceptie_Cu_Motiv() As Task
        ' Spre deosebire de ALEGERE_UNITATE, aici nu exista nimic de raspuns: baza s-a
        ' miscat, nu s-a scris nimic, si singurul drum inainte e o descarcare noua.
        Dim h As New StubHandler() With {
            .Status = HttpStatusCode.Conflict,
            .ResponseBody = "{""error"": ""Angajamentul s-a modificat.""," &
                            """reason"": ""STARE_MODIFICATA"", ""cod"": ""AAB37CNBK95""}"}
        Dim ex As ApiException = Await Assert.ThrowsAsync(Of ApiException)(
            Function() NewClient(h).SalveazaAsociereaAsync(Pachet(), "a1b2c3", Decizii(),
                                                           Nothing, CancellationToken.None))
        Assert.Equal(PrelucrarePropunere.MotivStareModificata, ex.Reason)
        Assert.Contains("modificat", ex.Message)
    End Function
End Class
