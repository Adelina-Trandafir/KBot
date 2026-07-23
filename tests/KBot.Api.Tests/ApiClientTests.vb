Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Offline tests for ApiClient.GetAngajamenteAsync. A stub HttpMessageHandler captures
' the request (URI / Authorization) and returns a configured response — no network.
Public Class ApiClientTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastRequestUri As Uri
        Public Property LastMethod As HttpMethod
        Public Property LastAuthorization As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
            LastMethod = request.Method
            LastAuthorization = If(request.Headers.Authorization IsNot Nothing,
                                   request.Headers.Authorization.ToString(), Nothing)
            Return Task.FromResult(New HttpResponseMessage(Status) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class

    Private Shared Function NewClient(handler As StubHandler, ByRef session As SessionContext) As ApiClient
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        session = New SessionContext() With {.Token = "tok-opaque-123"}
        Return New ApiClient(http, New ApiOptions(), session)
    End Function

    Private Const TwoRows As String =
        "{""db_name"":""000_DEMO"",""count"":2,""rows"":[" &
        "{""Cod"":""C1"",""Descriere"":""D1"",""Stare"":""În derulare"",""IDDF"":30," &
        "  ""Surse"":""02A;02B"",""Incarcat"":true,""Preluat"":true," &
        "  ""Ascuns"":false,""DataCreare"":""2026-01-18T00:00:00""}," &
        "{""Cod"":""C2"",""Descriere"":""D2"",""Stare"":""Anulat"",""IDDF"":null," &
        "  ""Surse"":null,""Incarcat"":false,""Preluat"":false," &
        "  ""Ascuns"":true,""DataCreare"":null}]}"

    <Fact>
    Public Async Function GetAngajamente_BuildsUrl_SendsBearer() As Task
        Dim h As New StubHandler With {.ResponseBody = TwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/angajamente", h.LastRequestUri.AbsolutePath)
        Assert.Contains("db_name=000_DEMO", h.LastRequestUri.Query)
        Assert.Contains("id_unitate=0", h.LastRequestUri.Query)
        Assert.Contains("doar_anulate=0", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetAngajamente_DoarAnulate_SetsFlag() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""db_name"":""000_DEMO"",""count"":0,""rows"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetAngajamenteAsync("000_DEMO", 7, True, CancellationToken.None)

        Assert.Contains("id_unitate=7", h.LastRequestUri.Query)
        Assert.Contains("doar_anulate=1", h.LastRequestUri.Query)
    End Function

    <Fact>
    Public Async Function GetAngajamente_Deserializes_AllFields_And_NullSurse() As Task
        Dim h As New StubHandler With {.ResponseBody = TwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim rows = Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None)

        Assert.Equal(2, rows.Count)

        Dim r0 = rows(0)
        Assert.Equal("C1", r0.CodAngajament)
        Assert.Equal("În derulare", r0.Stare)
        Assert.Equal("02A;02B", r0.Surse)
        Assert.True(r0.IDDF.HasValue)
        Assert.Equal(30, r0.IDDF.Value)
        Assert.True(r0.Incarcat)
        Assert.True(r0.Preluat)
        Assert.False(r0.Ascuns)
        Assert.True(r0.DataCreare.HasValue)
        Assert.Equal(New Date(2026, 1, 18), r0.DataCreare.Value)

        Dim r1 = rows(1)
        Assert.Equal("C2", r1.CodAngajament)
        Assert.Null(r1.Surse)                  ' orphan: Surse = null
        Assert.False(r1.IDDF.HasValue)
        Assert.True(r1.Ascuns)
        Assert.False(r1.DataCreare.HasValue)
    End Function

    <Fact>
    Public Async Function GetAngajamente_Non2xx_ThrowsWithStatus() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "{""error"":""boom""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
    End Function

    <Fact>
    Public Async Function GetAngajamente_401_ThrowsWithStatus_ForReauth() As Task
        ' A 401 must surface as ApiException(401) with no retry so MainForm.WithReauth
        ' can catch it and re-login.
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""expired""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
    End Function

    <Fact>
    Public Async Function ApiClient_ReadsTokenAtCallTime_NotAtConstruction() As Task
        ' Regression guard for H1: ApiClient must read _session.Token PER REQUEST, not
        ' capture it at construction. Build with an EMPTY session (no token), populate
        ' the token AFTERWARDS (as LoginForm.Populate does), then issue a request — the
        ' outgoing Authorization must carry the token set after construction.
        Dim h As New StubHandler With {.ResponseBody = "{""db_name"":""000_DEMO"",""count"":0,""rows"":[]}"}
        Dim http As New HttpClient(h) With {.BaseAddress = New Uri("http://localhost/")}
        Dim session As New SessionContext()        ' fără token la construcție
        Dim client As New ApiClient(http, New ApiOptions(), session)

        session.Token = "fresh-token-after-login"  ' populat DUPĂ construcție, ca la login

        Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None)

        Assert.Equal("Bearer fresh-token-after-login", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetAngajamente_Non2xx_ParsesErrorMessageAndReason() As Task
        ' The client must surface the server's Romanian message + machine reason, never
        ' the raw JSON body.
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Sesiune necunoscută. Autentificați-vă din nou."",""reason"":""TOKEN_UNKNOWN""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
        Assert.Equal("TOKEN_UNKNOWN", ex.Reason)
        Assert.Equal("Sesiune necunoscută. Autentificați-vă din nou.", ex.Message)
    End Function

    ' --- GetTreeAsync (slice 0008/0009): the MainForm tree read. Maps the wire row to
    ' AngajamentTreeInfo inside the client (no BuildTreeInfo in MainForm), so these
    ' assertions cover BOTH the query shape and the row mapping incl. all nine flags. ---

    Private Const TreeOneRow As String =
        "{""db_name"":""000_DEMO"",""count"":1,""rows"":[{" &
        """CodAngajament"":""A100"",""IDDF"":42,""Descriere"":""Angajament X""," &
        """Stare"":""În derulare"",""DataCreare"":""2026-02-03T00:00:00""," &
        """DataDefinitivare"":""2026-05-06T00:00:00""," &
        """Incarcat"":true,""Preluat"":false,""Salarii"":true,""Ascuns"":false," &
        """Surse"":""02A;02B""," &
        """AreIndicatori"":true,""AreIstoric"":false,""AreRevizii"":true," &
        """AreRezervari"":false,""AreReceptii"":true,""ArePlati"":false," &
        """AreDDF"":true,""ArePartener"":false,""AreOrd"":true}]}"

    <Fact>
    Public Async Function GetTree_BuildsUrl_SendsBearer_EscapesSs() As Task
        Dim h As New StubHandler With {.ResponseBody = TreeOneRow}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        ' SS cu spațiu -> dovada că folosim Uri.EscapeDataString, nu concatenare crudă.
        Await client.GetTreeAsync(2026, "02 A", True, CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/tree", h.LastRequestUri.AbsolutePath)
        Assert.Contains("an=2026", h.LastRequestUri.Query)
        Assert.Contains("ss=02%20A", h.LastRequestUri.Query)
        Assert.Contains("include_hidden=1", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetTree_IncludeHiddenFalse_SetsZero() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""db_name"":""000_DEMO"",""count"":0,""rows"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetTreeAsync(2026, "02A", False, CancellationToken.None)

        Assert.Contains("include_hidden=0", h.LastRequestUri.Query)
    End Function

    <Fact>
    Public Async Function GetTree_Deserializes_AllFields_AndNineFlags() As Task
        Dim h As New StubHandler With {.ResponseBody = TreeOneRow}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim rows = Await client.GetTreeAsync(2026, "02A", False, CancellationToken.None)

        Assert.Single(rows)
        Dim r = rows(0)
        Assert.Equal("A100", r.CodAngajament)
        Assert.Equal("A100", r.NodeKey)
        Assert.Equal("Angajament X", r.Descriere)
        Assert.Equal("Angajament X", r.Caption)
        Assert.Equal("În derulare", r.Stare)
        Assert.True(r.IDDF.HasValue)
        Assert.Equal(42L, r.IDDF.Value)               ' Integer pe wire -> Long? pe POCO
        Assert.True(r.DataCreare.HasValue)
        Assert.Equal(New Date(2026, 2, 3), r.DataCreare.Value)
        Assert.True(r.DataDefinitivare.HasValue)
        Assert.Equal(New Date(2026, 5, 6), r.DataDefinitivare.Value)
        Assert.True(r.EIncarcat)
        Assert.False(r.EPreluat)
        Assert.True(r.Salarii)
        Assert.False(r.Ascuns)
        Assert.Equal("02A;02B", r.Surse)

        ' Cele nouă flag-uri, 1:1 — inclusiv puntea AreOrd (JSON) -> AreORD (POCO).
        Assert.True(r.AreIndicatori)
        Assert.False(r.AreIstoric)
        Assert.True(r.AreRevizii)
        Assert.False(r.AreRezervari)
        Assert.True(r.AreReceptii)
        Assert.False(r.ArePlati)
        Assert.True(r.AreDDF)
        Assert.False(r.ArePartener)
        Assert.True(r.AreORD)
    End Function

    <Fact>
    Public Async Function GetTree_Non2xx_ThrowsWithStatus() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "{""error"":""Eroare la citirea arborelui: boom""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetTreeAsync(2026, "02A", False, CancellationToken.None))
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
    End Function

    <Fact>
    Public Async Function GetAngajamente_403ContextMismatch_CarriesStatusAndReason() As Task
        ' CONTEXT_MISMATCH = token VIU pe alt context; serverul îl dă cu 403, NU cu 401
        ' (guard.reject / auth_periods). Contractul de care depinde MainForm.WithReauth:
        ' un 403 trebuie să ajungă cu status 403 + reason, ca să fie oprit scurt în loc
        ' să intre în calea de re-login (unde nu s-ar repara niciodată).
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Forbidden,
            .ResponseBody = "{""error"":""Acces interzis pentru această unitate."",""reason"":""CONTEXT_MISMATCH""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.Equal(403, ex.StatusCode.Value)
        Assert.Equal("CONTEXT_MISMATCH", ex.Reason)
        Assert.Equal("Acces interzis pentru această unitate.", ex.Message)
    End Function

    ' --- GetSumarAsync (slice 0011): vederea Sumar. Contractul de fir e snake_case
    ' (spre deosebire de /tree, care e PascalCase), iar antetul poate lipsi cu totul.
    ' Testele acoperă forma URL-ului, maparea wire -> POCO și cele două cazuri „gol”. ---

    Private Const SumarOneRow As String =
        "{""header"":{""cod_angajament"":""A100"",""data_fx"":""2026-03-05""," &
        """data_creare"":""2026-03-01"",""data_definitivare"":null," &
        """descriere"":""Angajament X"",""stare"":""În derulare""," &
        """incarcat"":true,""preluat"":false}," &
        """rows"":[{""clsf"":""65.02.04.02.20.01.03"",""cod_indicator"":""IND-A""," &
        """partener"":""Furnizor SRL"",""total_rezervari"":150.0,""total_receptii"":10.0," &
        """total_plati"":25.0,""total_revizii"":7.5,""total_ordonantari"":3.25}]}"

    <Fact>
    Public Async Function GetSumar_BuildsUrl_SendsBearer_EscapesCod() As Task
        Dim h As New StubHandler With {.ResponseBody = SumarOneRow}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        ' Cod cu spațiu -> dovada că folosim Uri.EscapeDataString, nu concatenare crudă.
        Await client.GetSumarAsync("A 100", CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/sumar", h.LastRequestUri.AbsolutePath)
        Assert.Contains("cod=A%20100", h.LastRequestUri.Query)
        ' Fără filtru SS: sumarul arată TOȚI indicatorii angajamentului (decizia 1).
        Assert.DoesNotContain("ss=", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetSumar_BlankCod_ThrowsBeforeAnyRequest() As Task
        Dim h As New StubHandler With {.ResponseBody = SumarOneRow}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function() Await client.GetSumarAsync("   ", CancellationToken.None))
        Assert.Null(h.LastRequestUri)      ' nu s-a trimis nimic pe fir
    End Function

    <Fact>
    Public Async Function GetSumar_Deserializes_HeaderAndRow() As Task
        Dim h As New StubHandler With {.ResponseBody = SumarOneRow}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetSumarAsync("A100", CancellationToken.None)

        Assert.NotNull(data.Header)
        Assert.Equal("A100", data.Header.CodAngajament)
        Assert.Equal(New Date(2026, 3, 5), data.Header.DataFX.Value)
        Assert.Equal(New Date(2026, 3, 1), data.Header.DataCreare.Value)
        Assert.False(data.Header.DataDefinitivare.HasValue)     ' null pe fir -> Nothing
        Assert.Equal("Angajament X", data.Header.Descriere)
        Assert.Equal("În derulare", data.Header.Stare)
        Assert.True(data.Header.Incarcat)
        Assert.False(data.Header.Preluat)

        Assert.Single(data.Rows)
        Dim r = data.Rows(0)
        Assert.Equal("65.02.04.02.20.01.03", r.Clsf)
        Assert.Equal("IND-A", r.CodIndicator)
        Assert.Equal("Furnizor SRL", r.Partener)
        Assert.Equal(150.0, r.TotalRezervari)
        Assert.Equal(10.0, r.TotalReceptii)
        Assert.Equal(25.0, r.TotalPlati)
        Assert.Equal(7.5, r.TotalRevizii)
        Assert.Equal(3.25, r.TotalOrdonantari)
    End Function

    <Fact>
    Public Async Function GetSumar_NullHeader_IsEmptyNotException() As Task
        ' Un angajament fără indicatori e legitim: serverul dă 200 cu header null.
        ' Clientul trebuie să întoarcă un SumarInfo gol, NU să arunce.
        Dim h As New StubHandler With {.ResponseBody = "{""header"":null,""rows"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetSumarAsync("NUEXISTA", CancellationToken.None)

        Assert.NotNull(data)
        Assert.Null(data.Header)
        Assert.Empty(data.Rows)
    End Function

    <Fact>
    Public Async Function GetSumar_NullTextFields_BecomeEmptyStrings() As Task
        ' Clsf e null pentru un indicator fără clasificație (ramura LEFT JOIN). Randul
        ' TREBUIE să existe, iar textele null să ajungă String.Empty, nu Nothing —
        ' altfel grila ar picta o celulă nulă.
        Dim h As New StubHandler With {
            .ResponseBody = "{""header"":null,""rows"":[{""clsf"":null," &
                            """cod_indicator"":""IND-B"",""partener"":null," &
                            """total_rezervari"":0.0,""total_receptii"":0.0," &
                            """total_plati"":0.0,""total_revizii"":0.0," &
                            """total_ordonantari"":0.0}]}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetSumarAsync("A100", CancellationToken.None)

        Assert.Single(data.Rows)
        Assert.Equal(String.Empty, data.Rows(0).Clsf)
        Assert.Equal(String.Empty, data.Rows(0).Partener)
        Assert.Equal("IND-B", data.Rows(0).CodIndicator)
        Assert.Equal(0.0, data.Rows(0).TotalRezervari)
    End Function

    <Fact>
    Public Async Function GetSumar_Non2xx_ParsesRomanianErrorAndReason() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Sesiune necunoscută. Autentificați-vă din nou."",""reason"":""TOKEN_UNKNOWN""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetSumarAsync("A100", CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
        Assert.Equal("TOKEN_UNKNOWN", ex.Reason)
        Assert.Equal("Sesiune necunoscută. Autentificați-vă din nou.", ex.Message)
    End Function

    ' --- GetRezervariAsync (slice 0014): vederea Rezervări. Contract de fir snake_case
    ' (ca /sumar), listă plată de rânduri (fără antet). Testele acoperă forma URL-ului,
    ' maparea wire -> POCO, tipul derivat și cazul „gol”. ---

    Private Const RezervariTwoRows As String =
        "{""rows"":[" &
        "{""idrz"":960001,""cod_indicator"":""IND-A"",""clsf"":""65.02.04.02.20.01.03""," &
        """denumire"":""Cheltuieli"",""data_rezervare"":""2026-01-17""," &
        """r_credit_bug"":112100.0,""r_initiala"":3065.12,""r_valoare"":3065.12," &
        """r_definitiva"":0.0,""e_initiala"":true,""e_marire"":false," &
        """e_micsorare"":false,""are_ddf"":false}," &
        "{""idrz"":960003,""cod_indicator"":""IND-A"",""clsf"":null,""denumire"":null," &
        """data_rezervare"":""2026-02-07"",""r_credit_bug"":112100.0,""r_initiala"":6704.55," &
        """r_valoare"":-23.0,""r_definitiva"":0.0,""e_initiala"":false,""e_marire"":false," &
        """e_micsorare"":true,""are_ddf"":true}]}"

    <Fact>
    Public Async Function GetRezervari_BuildsUrl_SendsBearer_EscapesCod() As Task
        Dim h As New StubHandler With {.ResponseBody = RezervariTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetRezervariAsync("A 100", CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/rezervari", h.LastRequestUri.AbsolutePath)
        Assert.Contains("cod=A%20100", h.LastRequestUri.Query)
        Assert.DoesNotContain("ss=", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetRezervari_BlankCod_ThrowsBeforeAnyRequest() As Task
        Dim h As New StubHandler With {.ResponseBody = RezervariTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function() Await client.GetRezervariAsync("   ", CancellationToken.None))
        Assert.Null(h.LastRequestUri)      ' nu s-a trimis nimic pe fir
    End Function

    <Fact>
    Public Async Function GetRezervari_Deserializes_RowsAndDerivedType() As Task
        Dim h As New StubHandler With {.ResponseBody = RezervariTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetRezervariAsync("A100", CancellationToken.None)

        Assert.Equal(2, data.Rows.Count)
        Dim first = data.Rows(0)
        Assert.Equal(960001, first.Idrz)
        Assert.Equal("IND-A", first.CodIndicator)
        Assert.Equal("65.02.04.02.20.01.03", first.Clsf)
        Assert.Equal("Cheltuieli", first.Denumire)
        Assert.Equal(New Date(2026, 1, 17), first.DataRezervare)
        Assert.Equal(112100.0, first.RCreditBug)
        Assert.Equal(3065.12, first.RInitiala)
        Assert.Equal(3065.12, first.RValoare)
        Assert.True(first.EInitiala)
        Assert.False(first.AreDDF)
        ' Tip derivat + valoarea afișată a operației (= R_Initiala pentru inițială).
        Assert.Equal(RezervareTip.Initiala, first.Tip)
        Assert.Equal(3065.12, first.ValoareOperatie)

        Dim second = data.Rows(1)
        Assert.Equal(RezervareTip.Micsorare, second.Tip)
        Assert.Equal(-23.0, second.RValoare)
        Assert.Equal(-23.0, second.ValoareOperatie)   ' non-inițială -> R_Valoare
        Assert.True(second.AreDDF)
    End Function

    <Fact>
    Public Async Function GetRezervari_NullTextFields_BecomeEmptyStrings() As Task
        ' clsf/denumire null pentru o rezervare al cărei indicator nu are clasificație
        ' (ramura LEFT JOIN). Rândul TREBUIE să existe, iar textele null -> String.Empty.
        Dim h As New StubHandler With {.ResponseBody = RezervariTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetRezervariAsync("A100", CancellationToken.None)

        Assert.Equal(String.Empty, data.Rows(1).Clsf)
        Assert.Equal(String.Empty, data.Rows(1).Denumire)
    End Function

    <Fact>
    Public Async Function GetRezervari_EmptyRows_IsEmptyNotException() As Task
        ' Un angajament fără rezervări e legitim: serverul dă 200 cu rows []. Clientul
        ' trebuie să întoarcă un RezervariInfo gol, NU să arunce.
        Dim h As New StubHandler With {.ResponseBody = "{""rows"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetRezervariAsync("NUEXISTA", CancellationToken.None)

        Assert.NotNull(data)
        Assert.Empty(data.Rows)
    End Function

    <Fact>
    Public Async Function GetRezervari_Non2xx_ParsesRomanianErrorAndReason() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Sesiune necunoscută. Autentificați-vă din nou."",""reason"":""TOKEN_UNKNOWN""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetRezervariAsync("A100", CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
        Assert.Equal("TOKEN_UNKNOWN", ex.Reason)
    End Function

    ' --- GetReceptiiAsync (slice 0015): vederea Recepții. Contract de fir snake_case
    ' (ca /sumar, /rezervari), envelope cu DOUA liste (receptii + plati). Testele acoperă
    ' forma URL-ului, maparea wire -> POCO (inclusiv nullable idr/nrcrt), și cazul „gol". ---

    Private Const ReceptiiTwoRows As String =
        "{""cod"":""A100"",""receptii"":[" &
        "{""idrr"":970001,""nrcrt_r"":1,""data_r"":""2026-01-19"",""suma_antet"":2864.12," &
        """incarcat"":false,""preluat"":true,""idrh"":980001,""nrcrt_h"":1," &
        """data_h"":""2026-01-19"",""total"":2864.12,""difh"":2864.12,""sters_h"":false," &
        """descriere_h"":""Plata factura"",""idr"":990001,""id_clsf"":75," &
        """cod_indicator"":""IND-A"",""clsf"":""65.02.04.02.20.01.03""," &
        """denumire"":""Cheltuieli"",""nrcrt_ind"":1," &
        """valoare"":2864.12,""dif"":2864.12}," &
        "{""idrr"":970002,""nrcrt_r"":2,""data_r"":""2026-02-16"",""suma_antet"":3480.43," &
        """incarcat"":true,""preluat"":false,""idrh"":980002,""nrcrt_h"":null," &
        """data_h"":null,""total"":3480.43,""difh"":616.31,""sters_h"":false," &
        """descriere_h"":null,""idr"":null,""id_clsf"":0," &
        """cod_indicator"":null,""clsf"":null,""denumire"":null,""nrcrt_ind"":null," &
        """valoare"":0.0,""dif"":0.0}]," &
        """plati"":[{""data_plata"":""2026-01-25"",""suma"":1000.0}," &
        "{""data_plata"":""2026-02-20"",""suma"":500.0}]}"

    <Fact>
    Public Async Function GetReceptii_BuildsUrl_SendsBearer_EscapesCod() As Task
        Dim h As New StubHandler With {.ResponseBody = ReceptiiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetReceptiiAsync("A 100", CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/receptii", h.LastRequestUri.AbsolutePath)
        Assert.Contains("cod=A%20100", h.LastRequestUri.Query)
        Assert.DoesNotContain("ss=", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetReceptii_BlankCod_ThrowsBeforeAnyRequest() As Task
        Dim h As New StubHandler With {.ResponseBody = ReceptiiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function() Await client.GetReceptiiAsync("   ", CancellationToken.None))
        Assert.Null(h.LastRequestUri)      ' nu s-a trimis nimic pe fir
    End Function

    <Fact>
    Public Async Function GetReceptii_Deserializes_RowsPlati_AndNullables() As Task
        Dim h As New StubHandler With {.ResponseBody = ReceptiiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetReceptiiAsync("A100", CancellationToken.None)

        Assert.Equal("A100", data.Cod)
        Assert.Equal(2, data.Receptii.Count)

        Dim first = data.Receptii(0)
        Assert.Equal(970001, first.Idrr)
        Assert.Equal(1, first.NrCrtR.Value)
        Assert.Equal(New Date(2026, 1, 19), first.DataR.Value)
        Assert.Equal(2864.12, first.SumaAntet)
        Assert.False(first.Incarcat)
        Assert.True(first.Preluat)
        Assert.Equal(980001, first.Idrh)
        Assert.Equal(2864.12, first.Difh)
        Assert.Equal("Plata factura", first.DescriereH)
        Assert.Equal(990001, first.Idr.Value)
        Assert.Equal("65.02.04.02.20.01.03", first.Clsf)
        Assert.Equal("Cheltuieli", first.Denumire)
        Assert.Equal(1, first.NrCrtInd.Value)

        ' Al doilea rând: antet fără linii (idr null), nrcrt_h/data_h/descriere_h null.
        Dim second = data.Receptii(1)
        Assert.True(second.Incarcat)
        Assert.False(second.Preluat)
        Assert.False(second.NrCrtH.HasValue)
        Assert.False(second.DataH.HasValue)
        Assert.False(second.Idr.HasValue)
        Assert.False(second.NrCrtInd.HasValue)

        ' Plăți: două rânduri, ordonate, mapate în ReceptiePlata.
        Assert.Equal(2, data.Plati.Count)
        Assert.Equal(New Date(2026, 1, 25), data.Plati(0).DataPlata.Value)
        Assert.Equal(1000.0, data.Plati(0).Suma)
        Assert.Equal(500.0, data.Plati(1).Suma)
    End Function

    <Fact>
    Public Async Function GetReceptii_NullTextFields_BecomeEmptyStrings() As Task
        ' clsf/cod_indicator/descriere_h null pentru un antet fără linii (ramura LEFT JOIN).
        ' Rândul TREBUIE să existe, iar textele null -> String.Empty.
        Dim h As New StubHandler With {.ResponseBody = ReceptiiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetReceptiiAsync("A100", CancellationToken.None)

        Assert.Equal(String.Empty, data.Receptii(1).Clsf)
        Assert.Equal(String.Empty, data.Receptii(1).Denumire)
        Assert.Equal(String.Empty, data.Receptii(1).CodIndicator)
        Assert.Equal(String.Empty, data.Receptii(1).DescriereH)
    End Function

    <Fact>
    Public Async Function GetReceptii_Empty_IsEmptyNotException() As Task
        ' Un angajament fără recepții e legitim: serverul dă 200 cu receptii []. Clientul
        ' trebuie să întoarcă un ReceptiiInfo gol, NU să arunce.
        Dim h As New StubHandler With {.ResponseBody = "{""cod"":""NUEXISTA"",""receptii"":[],""plati"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetReceptiiAsync("NUEXISTA", CancellationToken.None)

        Assert.NotNull(data)
        Assert.Empty(data.Receptii)
        Assert.Empty(data.Plati)
        Assert.Equal("NUEXISTA", data.Cod)
    End Function

    <Fact>
    Public Async Function GetReceptii_Non2xx_ParsesRomanianErrorAndReason() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Sesiune necunoscută. Autentificați-vă din nou."",""reason"":""TOKEN_UNKNOWN""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetReceptiiAsync("A100", CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
        Assert.Equal("TOKEN_UNKNOWN", ex.Reason)
    End Function

    ' --- GetPlatiAsync (slice 0017): vederea Plăți. Contract de fir snake_case, extrasul
    ' bancar FLAT pe rand -> se pliaza intr-un ExtrasBancar (Nothing cand idfxe e null). ---

    Private Const PlatiTwoRows As String =
        "{""cod"":""A100"",""plati"":[" &
        "{""id_plata_fx"":9900171,""id_clsf"":75,""cod_ai"":""AI-A"",""cod_indicator"":""IND-A""," &
        """nr_op"":""39"",""data_plata"":""2026-01-31"",""suma"":1331.0,""tip"":""PLATA""," &
        """incarcat"":true,""preluat"":false,""referinta_trezor"":""TZ001"",""clsf"":""65.02""," &
        """denumire"":""Cheltuieli"",""clsf_plata"":""65.02"",""are_ord"":true," &
        """idfxe"":9900171,""data_banca"":""2026-01-31"",""data_doc"":""31.01.2026""," &
        """nr_doc_extras"":""0100"",""referinta"":""TZ001"",""platitor_nume"":""FURNIZOR SRL""," &
        """platitor_cui"":""123"",""platitor_iban"":""RO00"",""suma_debit"":1331.0," &
        """suma_credit"":0.0,""explicatii"":""Explicație""}," &
        "{""id_plata_fx"":9900172,""id_clsf"":0,""cod_ai"":null,""cod_indicator"":null," &
        """nr_op"":""85"",""data_plata"":""2026-02-04"",""suma"":-23.0,""tip"":""INCASARE""," &
        """incarcat"":false,""preluat"":true,""referinta_trezor"":""TZ002"",""clsf"":null," &
        """denumire"":null,""clsf_plata"":""66.01"",""are_ord"":false," &
        """idfxe"":null,""data_banca"":null,""data_doc"":null,""nr_doc_extras"":null," &
        """referinta"":null,""platitor_nume"":null,""platitor_cui"":null,""platitor_iban"":null," &
        """suma_debit"":0.0,""suma_credit"":0.0,""explicatii"":null}]}"

    <Fact>
    Public Async Function GetPlati_BuildsUrl_SendsBearer_EscapesCod() As Task
        Dim h As New StubHandler With {.ResponseBody = PlatiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetPlatiAsync("A 100", CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/plati", h.LastRequestUri.AbsolutePath)
        Assert.Contains("cod=A%20100", h.LastRequestUri.Query)
        Assert.DoesNotContain("ss=", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetPlati_BlankCod_ThrowsBeforeAnyRequest() As Task
        Dim h As New StubHandler With {.ResponseBody = PlatiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function() Await client.GetPlatiAsync("   ", CancellationToken.None))
        Assert.Null(h.LastRequestUri)      ' nu s-a trimis nimic pe fir
    End Function

    <Fact>
    Public Async Function GetPlati_Deserializes_Rows_Extras_AndNullables() As Task
        Dim h As New StubHandler With {.ResponseBody = PlatiTwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetPlatiAsync("A100", CancellationToken.None)

        Assert.Equal("A100", data.Cod)
        Assert.Equal(2, data.Plati.Count)

        ' Primul rând: ordonantat, cu extras bancar pliat.
        Dim first = data.Plati(0)
        Assert.Equal(9900171, first.IdPlataFX)
        Assert.Equal(New Date(2026, 1, 31), first.DataPlata.Value)
        Assert.Equal(1331.0, first.Suma)
        Assert.Equal("PLATA", first.Tip)
        Assert.True(first.Incarcat)
        Assert.False(first.Preluat)
        Assert.True(first.AreOrd)
        Assert.Equal("65.02", first.Clsf)
        Assert.Equal("Cheltuieli", first.Denumire)
        Assert.NotNull(first.Extras)
        Assert.Equal(9900171, first.Extras.Idfxe)
        Assert.Equal("FURNIZOR SRL", first.Extras.PlatitorNume)
        Assert.Equal("31.01.2026", first.Extras.DataDoc)     ' TEXT, nu ISO
        Assert.Equal(1331.0, first.Extras.SumaDebit)

        ' Al doilea rând: fără extras (idfxe null -> Extras Nothing), clsf null -> gol,
        ' fallback pe clsf_plata; INCASARE ne-ordonantată.
        Dim second = data.Plati(1)
        Assert.Equal(9900172, second.IdPlataFX)
        Assert.Null(second.Extras)
        Assert.Equal(String.Empty, second.Clsf)
        Assert.Equal("66.01", second.ClsfPlata)
        Assert.Equal("66.01", second.ClsfEfectiv)            ' fallback pe clsf_plata
        Assert.True(second.EsteIncasare)
        Assert.False(second.AreOrd)
    End Function

    <Fact>
    Public Async Function GetPlati_Empty_IsEmptyNotException() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""cod"":""NUEXISTA"",""plati"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim data = Await client.GetPlatiAsync("NUEXISTA", CancellationToken.None)

        Assert.NotNull(data)
        Assert.Empty(data.Plati)
        Assert.Equal("NUEXISTA", data.Cod)
    End Function

    <Fact>
    Public Async Function GetPlati_Non2xx_ParsesRomanianErrorAndReason() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Sesiune necunoscută. Autentificați-vă din nou."",""reason"":""TOKEN_UNKNOWN""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetPlatiAsync("A100", CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
        Assert.Equal("TOKEN_UNKNOWN", ex.Reason)
    End Function

End Class
