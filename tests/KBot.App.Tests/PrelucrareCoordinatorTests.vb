Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.App
Imports KBot.Common
Imports KBot.Domain

' Tests for PrelucrareCoordinator — the loop "ask, and if the server answers that a
' classification matches several units, ask the operator and send the SAME payload again
' with the answers attached".
'
' They run on CerePropunereAsync (phase one). Until slice 0055 they ran on TrimiteAsync, the
' phaseless send, which was retired with IApiClient.TrimitePrelucrareAsync: it left «mod» out
' of the request, the server read that as «propunere», rolled everything back, and the client
' still reported PrelucrareStare.Salvat. The ask-loop itself is unchanged — it is the same
' loop in both methods, which is why the whole file moved over without losing a case.
'
' No windows: the coordinator takes the question-asker as a delegate, so these tests hand
' it a stub instead of AlegereUnitateForm. That seam is the reason the loop is testable at
' all — ShowDialog is modal and would block a test run.
Public Class PrelucrareCoordinatorTests

    ' Records every attempt (the choices it was given) and hands back the queued responses
    ' in order, so a test scripts the whole conversation up front.
    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly Attempts As New List(Of List(Of AlegereUnitate))()
        Public ReadOnly Queue As New Queue(Of PrelucrareRaspuns)()

        ' Faza UNU. Records every attempt (the choices it was given) and hands back the
        ' queued responses in order, so a test scripts the whole conversation up front.
        ' Copy, not reference: the coordinator keeps adding to the same list between rounds,
        ' so a stored reference would show the FINAL contents on every recorded attempt.
        Public Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                           alegeri As IReadOnlyList(Of AlegereUnitate),
                                           ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.CerePropunereAsync
            Attempts.Add(New List(Of AlegereUnitate)(alegeri))
            Return Task.FromResult(Queue.Dequeue())
        End Function

        ''' <summary>Deciziile cu care s-a chemat salvarea, in ordine.</summary>
        Public ReadOnly Salvari As New List(Of List(Of DecizieAsociere))()
        Public Property AmprentaPrimita As String

        Public Function SalveazaAsociereaAsync(rezultat As PrelucrareRezultat,
                                               amprenta As String,
                                               decizii As IReadOnlyList(Of DecizieAsociere),
                                               alegeri As IReadOnlyList(Of AlegereUnitate),
                                               ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.SalveazaAsociereaAsync
            AmprentaPrimita = amprenta
            Salvari.Add(New List(Of DecizieAsociere)(decizii))
            Attempts.Add(New List(Of AlegereUnitate)(alegeri))
            Return Task.FromResult(Queue.Dequeue())
        End Function

        ' --- restul contractului: nefolosit aici ---
        Public Function GetAsociereAsync(cod As String, ct As CancellationToken) _
            As Task(Of AsociereStare) Implements IApiClient.GetAsociereAsync
            Throw New NotSupportedException()
        End Function

        Public Function SalveazaLegaturiAsync(cod As String,
                                              amprenta As String,
                                              comenzi As IReadOnlyList(Of ComandaAsociere),
                                              ct As CancellationToken) As Task(Of AsociereRezultat) _
            Implements IApiClient.SalveazaLegaturiAsync
            Throw New NotSupportedException()
        End Function

        Public Function UpsertAngajamenteAsync(dbName As String, rows As IReadOnlyList(Of Angajament),
                                               ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.UpsertAngajamenteAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                            ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) _
            Implements IApiClient.GetAngajamenteAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                     ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) _
            Implements IApiClient.GetTreeAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
            Implements IApiClient.GetSumarAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
            Implements IApiClient.GetIstoricAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo) _
            Implements IApiClient.GetOrdAsync
            Throw New NotSupportedException()
        End Function
        Public Function DownloadDdfPdfAsync(idrev As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadDdfPdfAsync
            Throw New NotSupportedException()
        End Function
        Public Function DownloadOrdPdfAsync(idordp As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadOrdPdfAsync
            Throw New NotSupportedException()
        End Function
        Public Function UploadDdfPdfAsync(idrev As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadDdfPdfAsync
            Throw New NotSupportedException()
        End Function
        Public Function UploadOrdPdfAsync(idordp As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadOrdPdfAsync
            Throw New NotSupportedException()
        End Function
        Public Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.ProcessExcelAsync
            Throw New NotSupportedException()
        End Function
        Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) _
            Implements IApiClient.GetAsync
            Throw New NotSupportedException()
        End Function
    
    ' ── Editorul de ordonanțare (felia 0049) ────────────────────────────────────────────
    ' Cioturi: dublura asta nu exersează scrierea ORD. Aruncă, nu întoarce gol — un dublu
    ' care ar răspunde tăcut ar face un test să treacă pe un drum pe care nimeni nu l-a scris.
    Public Function GenereazaOrdAsync(cod As String, dataOrd As Date, idPlataFx As Integer?,
                                      ct As CancellationToken) As Task(Of OrdDraft) _
        Implements IApiClient.GenereazaOrdAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetOrdDraftAsync(idordp As Integer, ct As CancellationToken) As Task(Of OrdDraft) _
        Implements IApiClient.GetOrdDraftAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetOrdZileAsync(cod As String, luna As Integer?, an As Integer?,
                                    ct As CancellationToken) As Task(Of OrdZileInfo) _
        Implements IApiClient.GetOrdZileAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetOrdNrUrmatorAsync(ct As CancellationToken) As Task(Of Integer) _
        Implements IApiClient.GetOrdNrUrmatorAsync
        Throw New NotSupportedException()
    End Function

    Public Function SaveOrdAsync(draft As OrdDraft, ct As CancellationToken) As Task(Of OrdSaveRezultat) _
        Implements IApiClient.SaveOrdAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteOrdAsync(idordp As Integer, ct As CancellationToken) As Task(Of OrdStergereRezultat) _
        Implements IApiClient.DeleteOrdAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetOrdAtasamentAsync(idordattp As Integer, cachedSha As String,
                                         ct As CancellationToken) As Task(Of PdfDownloadResult) _
        Implements IApiClient.GetOrdAtasamentAsync
        Throw New NotSupportedException()
    End Function

    Public Function PutOrdAtasamentAsync(idordattp As Integer, numeFisier As String,
                                         continut As Byte(), shaPrecedent As String,
                                         ct As CancellationToken) As Task(Of PutAtasamentResponse) _
        Implements IApiClient.PutOrdAtasamentAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteOrdAtasamentAsync(idordattp As Integer, ct As CancellationToken) As Task _
        Implements IApiClient.DeleteOrdAtasamentAsync
        Throw New NotSupportedException()
    End Function

    ' ── Slice 0051: the DDF editor. Compilation bookkeeping, NOT coverage. ───────────────
    ' These sixteen members exist so this double still satisfies `IApiClient`. Nothing here
    ' is exercised by any test, and `NotSupportedException` says so out loud -- a stub that
    ' returned a plausible empty value would let a test pass while testing nothing.

    Public Function GenereazaDdfAsync(cod As String, rev0 As Boolean, ct As CancellationToken) _
        As Task(Of DdfDraft) Implements IApiClient.GenereazaDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetDdfDraftAsync(iddf As Integer, idrev As Integer, ct As CancellationToken) _
        As Task(Of DdfDraft) Implements IApiClient.GetDdfDraftAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetDdfClasificatiiAsync(cod As String, manual As Boolean, titlu As String,
                                            ct As CancellationToken) _
        As Task(Of List(Of DdfClasificatie)) Implements IApiClient.GetDdfClasificatiiAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetDdfParteneriAsync(cod As String, ct As CancellationToken) _
        As Task(Of List(Of DdfPartener)) Implements IApiClient.GetDdfParteneriAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetDdfCompAsync(ct As CancellationToken) _
        As Task(Of List(Of String)) Implements IApiClient.GetDdfCompAsync
        Throw New NotSupportedException()
    End Function

    Public Function SaveDdfAsync(draft As DdfDraft, ct As CancellationToken) _
        As Task(Of DdfSaveRezultat) Implements IApiClient.SaveDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteDdfRevizieAsync(idrev As Integer, ct As CancellationToken) _
        As Task(Of DdfStergereRezultat) Implements IApiClient.DeleteDdfRevizieAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteDdfAsync(iddf As Integer, ct As CancellationToken) _
        As Task(Of DdfStergereRezultat) Implements IApiClient.DeleteDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteDdfLunaAsync(iddf As Integer, an As Integer, luna As Integer,
                                       ct As CancellationToken) _
        As Task(Of DdfStergereRezultat) Implements IApiClient.DeleteDdfLunaAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetDdfFisierAsync(idrevatt As Integer, cachedSha As String,
                                      ct As CancellationToken) _
        As Task(Of PdfDownloadResult) Implements IApiClient.GetDdfFisierAsync
        Throw New NotSupportedException()
    End Function

    Public Function PutDdfFisierAsync(idrevatt As Integer, numeFisier As String,
                                      continut As Byte(), shaPrecedent As String,
                                      ct As CancellationToken) _
        As Task(Of PutDdfFisierResponse) Implements IApiClient.PutDdfFisierAsync
        Throw New NotSupportedException()
    End Function

    Public Function DeleteDdfFisierAsync(idrevatt As Integer, ct As CancellationToken) _
        As Task Implements IApiClient.DeleteDdfFisierAsync
        Throw New NotSupportedException()
    End Function

    Public Function RezervaNumarDdfAsync(tip As String, cod As String, dc As String,
                                         ct As CancellationToken) _
        As Task(Of DdfNumarLock) Implements IApiClient.RezervaNumarDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function SchimbaNumarDdfAsync(idLock As Integer, valoare As Integer,
                                         ct As CancellationToken) _
        As Task(Of DdfNumarLock) Implements IApiClient.SchimbaNumarDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function PrelungesteNumarDdfAsync(idLock As Integer, ct As CancellationToken) _
        As Task(Of DdfNumarLock) Implements IApiClient.PrelungesteNumarDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function ElibereazaNumarDdfAsync(idLock As Integer, ct As CancellationToken) _
        As Task Implements IApiClient.ElibereazaNumarDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest,
                                                          ct As CancellationToken) As Task(Of TResponse) _
            Implements IApiClient.PostAsync
            Throw New NotSupportedException()
        End Function
    End Class

    Private Shared Function Pachet() As PrelucrareRezultat
        Return New PrelucrareRezultat() With {.CodAngajament = "AAB37CNBK95"}
    End Function

    ' Raspunsul care incheie bucla: faza intai s-a terminat si a produs tabloul.
    Private Shared Function Propunerea() As PrelucrareRaspuns
        Return New PrelucrareRaspuns() With {
            .Stare = PrelucrareStare.Propunere,
            .CodAngajament = "AAB37CNBK95",
            .Propunere = New PrelucrarePropunere() With {
                .CodAngajament = "AAB37CNBK95", .Amprenta = "a1b2"}}
    End Function

    Private Shared Function Intreaba(ParamArray perechi As String()) As PrelucrareRaspuns
        Dim r As New PrelucrareRaspuns() With {
            .Stare = PrelucrareStare.AlegereUnitate, .CodAngajament = "AAB37CNBK95"}
        For Each clsfE As String In perechi
            Dim q As New AlegereNecesara() With {.Ss = "02E", .ClsfE = clsfE, .CodIndicator = "AAB"}
            q.Unitati.Add(New UnitateCandidat() With {.IdUnitate = 75, .Detalii = "SC29 LOCAL"})
            q.Unitati.Add(New UnitateCandidat() With {.IdUnitate = 76, .Detalii = "ENERGETIC ISJ"})
            r.AlegeriNecesare.Add(q)
        Next
        Return r
    End Function

    ' Un operator care alege mereu a doua unitate, fără să bifeze.
    Private Shared Function AlegeAlDoilea(retine As Boolean) _
        As Func(Of AlegereNecesara, String, Integer, Integer, AlegereUnitate)
        Return Function(q, cod, poz, total) New AlegereUnitate() With {
            .Ss = q.Ss, .ClsfE = q.ClsfE, .IdUnitate = q.Unitati(1).IdUnitate, .Retine = retine}
    End Function

    ' ── fără întrebare ────────────────────────────────────────────────────
    <Fact>
    Public Async Function FaraAmbiguitate_UnSingurDrumDusIntors() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Propunerea())
        Dim intrebat As Integer = 0
        Dim c As New PrelucrareCoordinator(api,
            Function(q, cod, poz, total)
                intrebat += 1
                Return Nothing
            End Function)

        Dim r = Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.Equal("AAB37CNBK95", r.CodAngajament)
        Assert.Single(api.Attempts)
        Assert.Empty(api.Attempts(0))
        Assert.Equal(0, intrebat)
    End Function

    ' ── întrebare, răspuns, retrimitere ───────────────────────────────────
    <Fact>
    Public Async Function Ambiguitate_IntreabaApoiRetrimiteAceleasiDateCuAlegerea() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        api.Queue.Enqueue(Propunerea())
        Dim c As New PrelucrareCoordinator(api, AlegeAlDoilea(retine:=False))

        Dim r = Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.Equal("AAB37CNBK95", r.CodAngajament)
        Assert.Equal(2, api.Attempts.Count)
        Assert.Empty(api.Attempts(0))                       ' prima încercare, fără alegeri
        Dim a = Assert.Single(api.Attempts(1))
        Assert.Equal("02E", a.Ss)
        Assert.Equal("200101", a.ClsfE)
        Assert.Equal(76, a.IdUnitate)
        Assert.False(a.Retine)
    End Function

    <Fact>
    Public Async Function BifaAjungePeFir() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        api.Queue.Enqueue(Propunerea())
        Dim c As New PrelucrareCoordinator(api, AlegeAlDoilea(retine:=True))

        Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.True(api.Attempts(1)(0).Retine)
    End Function

    ' Serverul adună TOATE perechile într-o singură trecere: două întrebări, un singur
    ' drum înapoi.
    <Fact>
    Public Async Function DouaPerechi_SeIntreabaAmandouaInaintaDeRetrimitere() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101", "200301"))
        api.Queue.Enqueue(Propunerea())
        Dim vazute As New List(Of String)()
        Dim c As New PrelucrareCoordinator(api,
            Function(q, cod, poz, total)
                vazute.Add($"{q.ClsfE} {poz}/{total}")
                Return New AlegereUnitate() With {.Ss = q.Ss, .ClsfE = q.ClsfE, .IdUnitate = 75}
            End Function)

        Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.Equal(New String() {"200101 1/2", "200301 2/2"}, vazute)
        Assert.Equal(2, api.Attempts.Count)
        Assert.Equal(2, api.Attempts(1).Count)
    End Function

    ' Codul angajamentului ajunge în dialog — el e primul lucru din antetul întrebării.
    <Fact>
    Public Async Function CodulAngajamentuluiAjungeInIntrebare() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        api.Queue.Enqueue(Propunerea())
        Dim codVazut As String = Nothing
        Dim c As New PrelucrareCoordinator(api,
            Function(q, cod, poz, total)
                codVazut = cod
                Return New AlegereUnitate() With {.Ss = q.Ss, .ClsfE = q.ClsfE, .IdUnitate = 75}
            End Function)

        Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.Equal("AAB37CNBK95", codVazut)
    End Function

    ' ── renunțarea ────────────────────────────────────────────────────────
    <Fact>
    Public Async Function Renuntare_NuRetrimiteNimic() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        Dim c As New PrelucrareCoordinator(api, Function(q, cod, poz, total) Nothing)

        Dim r = Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None)

        ' Nothing = s-a renunțat; ultimul răspuns al serverului a fost 409, deci nimic scris.
        Assert.Null(r)
        Assert.Single(api.Attempts)
    End Function

    ' O renunțare la a doua întrebare oprește tot: o alegere lipsă nu se poate compensa.
    <Fact>
    Public Async Function RenuntareLaADouaIntrebare_OpresteTot() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101", "200301"))
        Dim c As New PrelucrareCoordinator(api,
            Function(q, cod, poz, total)
                If poz = 2 Then Return Nothing
                Return New AlegereUnitate() With {.Ss = q.Ss, .ClsfE = q.ClsfE, .IdUnitate = 75}
            End Function)

        Assert.Null(Await c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None))
        Assert.Single(api.Attempts)
    End Function

    ' ── bucla ─────────────────────────────────────────────────────────────
    <Fact>
    Public Async Function ServerCareIntreabaLaNesfarsit_SeOpresteCuOEroareLimpede() As Task
        Dim api As New FakeApiClient()
        For i As Integer = 1 To PrelucrareCoordinator.MaxRunde
            api.Queue.Enqueue(Intreaba("200101"))
        Next
        Dim c As New PrelucrareCoordinator(api, AlegeAlDoilea(retine:=False))

        Dim ex = Await Assert.ThrowsAsync(Of InvalidOperationException)(
            Function() c.CerePropunereAsync(Pachet(), New List(Of AlegereUnitate)(), CancellationToken.None))
        Assert.Contains("nu s-a scris nimic", ex.Message)
        Assert.Equal(PrelucrareCoordinator.MaxRunde, api.Attempts.Count)
    End Function

    <Fact>
    Public Async Function PachetLipsa_Arunca() As Task
        Dim c As New PrelucrareCoordinator(New FakeApiClient())
        Await Assert.ThrowsAsync(Of ArgumentNullException)(
            Function() c.CerePropunereAsync(Nothing, New List(Of AlegereUnitate)(), CancellationToken.None))
    End Function

    ' ── faza a doua: aceeași buclă, fiindcă 409 se poate declanșa și la salvare ─────────
    '
    ' Cazul ăsta e chiar defectul pentru care s-a retras TrimiteAsync, în alt loc: un răspuns
    ' care întreabă, luat drept răspuns care confirmă. Serverul rulează pașii DIN NOU la
    ' salvare, deci o clasificație nouă apărută între faze cere iar o alegere.
    <Fact>
    Public Async Function Salvarea_Intreaba_Apoi_Retrimite_Cu_Alegerea() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        api.Queue.Enqueue(Propunerea())          ' al doilea răspuns: 200, s-a comis
        Dim c As New PrelucrareCoordinator(api, AlegeAlDoilea(retine:=False))
        Dim alegeri As New List(Of AlegereUnitate)()

        Dim r = Await c.SalveazaAsync(Pachet(), "a1b2", New List(Of DecizieAsociere)(),
                                      alegeri, CancellationToken.None)

        Assert.NotNull(r)
        Assert.NotEqual(PrelucrareStare.AlegereUnitate, r.Stare)
        Assert.Equal(2, api.Attempts.Count)
        Assert.Empty(api.Attempts(0))
        Assert.Equal(76, Assert.Single(api.Attempts(1)).IdUnitate)
        ' Alegerea rămâne în lista apelantului: aceeași care a trecut prin faza întâi.
        Assert.Single(alegeri)
        Assert.Equal("a1b2", api.AmprentaPrimita)
    End Function

    <Fact>
    Public Async Function Salvarea_FaraIntrebare_MergeDinPrimaIncercare() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Propunerea())
        Dim c As New PrelucrareCoordinator(api, Function(q, cod, poz, total) Nothing)

        Dim r = Await c.SalveazaAsync(Pachet(), "a1b2", New List(Of DecizieAsociere)(),
                                      New List(Of AlegereUnitate)(), CancellationToken.None)

        Assert.NotNull(r)
        Assert.Single(api.Attempts)
    End Function

    <Fact>
    Public Async Function Renuntarea_La_Salvare_NuRetrimiteNimic() As Task
        Dim api As New FakeApiClient()
        api.Queue.Enqueue(Intreaba("200101"))
        Dim c As New PrelucrareCoordinator(api, Function(q, cod, poz, total) Nothing)

        ' Nothing = s-a renunțat; ultimul răspuns al serverului a fost 409, deci nimic scris.
        Assert.Null(Await c.SalveazaAsync(Pachet(), "a1b2", New List(Of DecizieAsociere)(),
                                          New List(Of AlegereUnitate)(), CancellationToken.None))
        Assert.Single(api.Attempts)
    End Function

    <Fact>
    Public Async Function Salvarea_FaraDecizii_Arunca() As Task
        Dim c As New PrelucrareCoordinator(New FakeApiClient())
        Await Assert.ThrowsAsync(Of ArgumentNullException)(
            Function() c.SalveazaAsync(Pachet(), "a1b2", Nothing, New List(Of AlegereUnitate)(),
                                       CancellationToken.None))
    End Function

End Class
