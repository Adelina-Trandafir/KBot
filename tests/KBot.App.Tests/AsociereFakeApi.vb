Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Clientul API de proba folosit de AsociereFormTests (felia 0048-04).
'
' Traieste in fisierul lui fiindca IApiClient are multi membri si niciunul dintre ei nu
' spune nimic despre felia asta: DOUA metode raspund - citirea tabloului si salvarea
' legaturilor - iar restul ridica NotSupportedException, ca o cerere neasteptata sa cada
' zgomotos in loc sa se strecoare ca un raspuns gol.
Friend NotInheritable Class AsociereFakeApi
    Implements IApiClient

    ' Felia 0048-02: ingestia FOREXE. Nefolosita de vederile astea.
    Public Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat,
                                           alegeri As IReadOnlyList(Of AlegereUnitate),
                                           ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
        Implements IApiClient.TrimitePrelucrareAsync
        Throw New NotSupportedException()
    End Function

    Public Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                       alegeri As IReadOnlyList(Of AlegereUnitate),
                                       ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
        Implements IApiClient.CerePropunereAsync
        Throw New NotSupportedException()
    End Function

    Public Function SalveazaAsociereaAsync(rezultat As PrelucrareRezultat,
                                           amprenta As String,
                                           decizii As IReadOnlyList(Of DecizieAsociere),
                                           alegeri As IReadOnlyList(Of AlegereUnitate),
                                           ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
        Implements IApiClient.SalveazaAsociereaAsync
        Throw New NotSupportedException()
    End Function

    ' Tabloul pe care il intoarce citirea. Testul il pune inainte de a arata formularul.
    Public Stare As AsociereStare
    Public ReadOnly CoduriCerute As New List(Of String)()

    Public Function GetAsociereAsync(cod As String, ct As CancellationToken) _
        As Task(Of AsociereStare) Implements IApiClient.GetAsociereAsync
        CoduriCerute.Add(cod)
        Return Task.FromResult(Stare)
    End Function

    ' Ce a primit ultima salvare -- testul se uita EXACT la ce a plecat pe fir.
    Public Salvari As New List(Of List(Of ComandaAsociere))()
    Public AmprentaPrimita As String

    Public Function SalveazaLegaturiAsync(cod As String,
                                          amprenta As String,
                                          comenzi As IReadOnlyList(Of ComandaAsociere),
                                          ct As CancellationToken) As Task(Of AsociereRezultat) _
        Implements IApiClient.SalveazaLegaturiAsync
        AmprentaPrimita = amprenta
        Salvari.Add(New List(Of ComandaAsociere)(comenzi))
        Return Task.FromResult(New AsociereRezultat() With {.CodAngajament = cod, .Amprenta = "dupa"})
    End Function

    ' Editorul de legaturi NU cere receptiile: are ruta lui. Daca ajunge aici, ceva s-a legat
    ' gresit, si e mai bine sa se vada decat sa se intoarca o lista goala.
    Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) _
        As Task(Of ReceptiiInfo) Implements IApiClient.GetReceptiiAsync
        Throw New NotSupportedException()
    End Function

    ' --- restul contractului: nefolosit aici ---
    Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
        Implements IApiClient.GetRezervariAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
        Implements IApiClient.GetPlatiAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
        Implements IApiClient.GetSumarAsync
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

    Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
        Implements IApiClient.GetDdfAsync
        Throw New NotSupportedException()
    End Function

    Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
        Implements IApiClient.GetIstoricAsync
        Throw New NotSupportedException()
    End Function

    ' Felia 0033: vederea ORD nu e exercitată de acest dublu — contractul cere metoda,
    ' deci o refuzăm zgomotos, ca pe celelalte neatinse.
    Public Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo) _
        Implements IApiClient.GetOrdAsync
        Throw New NotSupportedException()
    End Function

    ' Felia 0041: rutele de PDF semnat nu sunt exercitate de acest dublu — contractul cere
    ' metodele, deci le refuzăm zgomotos, ca pe celelalte neatinse.
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
