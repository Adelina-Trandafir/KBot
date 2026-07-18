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

End Class
