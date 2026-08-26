Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Singurul loc care va ști BaseUrl / token bearer / retry / timeout / JSON.
Public Interface IApiClient
    ''' <summary>
    ''' Trimite lista de angajamente la /api/forexe/angajamente/upsert.
    ''' Întoarce corpul brut al răspunsului. Hard-fail (Throw) la eroare.
    ''' </summary>
    Function UpsertAngajamenteAsync(dbName As String,
                                    rows As IReadOnlyList(Of Angajament),
                                    ct As CancellationToken) As Task(Of String)
    ''' <summary>
    ''' Aduce lista de angajamente pentru vederea-listă din MainForm (oglindește
    ''' Angajamente_SQL). Filtrează după COALESCE(IdUnitate,0)=idUnitate; doarAnulate
    ''' comută pe filtrul anulate/suspendat/ascuns. Hard-fail (Throw) la non-2xx;
    ''' fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                 ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament))

    ''' <summary>
    ''' Aduce arborele de angajamente pentru MainForm (GET /api/forexe/tree), filtrat
    ''' pe an + SS. includeHidden readuce angajamentele ASCUNS (opțiunea btnOpt).
    ''' Baza NU se trimite: serverul o ia din sesiune (o bază = o unitate). Hard-fail
    ''' (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                          ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo))

    ''' <summary>
    ''' Aduce sumarul unui angajament (GET /api/forexe/sumar): antetul + un rând per
    ''' indicator. FĂRĂ filtru SS — sumarul arată toți indicatorii. Baza NU se trimite:
    ''' serverul o ia din sesiune. Un cod necunoscut întoarce un SumarInfo cu Header
    ''' Nothing și zero rânduri (nu excepție). Hard-fail (Throw) la non-2xx; fără retry
    ''' pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo)

    ''' <summary>
    ''' Aduce rezervările unui angajament (GET /api/forexe/rezervari): un rând per
    ''' înregistrare FX_Rezervari. Baza NU se trimite: serverul o ia din sesiune. Un cod
    ''' necunoscut întoarce un RezervariInfo cu zero rânduri (nu excepție). Hard-fail
    ''' (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo)

    ''' <summary>
    ''' Aduce recepțiile unui angajament (GET /api/forexe/receptii): un rând per linie
    ''' FX_Receptii (cu antet + receptie părinte) plus lista de plăți a angajamentului.
    ''' Baza NU se trimite: serverul o ia din sesiune. Un cod necunoscut întoarce un
    ''' ReceptiiInfo cu zero rânduri (nu excepție). Hard-fail (Throw) la non-2xx; fără
    ''' retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo)

    ''' <summary>
    ''' Aduce plățile unui angajament (GET /api/forexe/plati): un rând per înregistrare
    ''' FX_Plati, cu extrasul bancar (FX_Extrase) purtat pe rând. Baza NU se trimite:
    ''' serverul o ia din sesiune. Un cod necunoscut întoarce un PlatiInfo cu zero rânduri
    ''' (nu excepție). Hard-fail (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo)

    ''' <summary>
    ''' Aduce documentul de fundamentare al unui angajament (GET /api/forexe/ddf): antet(e)
    ''' FX_DDF + revizii (fiecare cu SUM-ul real al secțiunii A) + liniile de secțiune A.
    ''' Un singur drum dus-întors pentru tot codul: vederea filtrează local. Baza NU se
    ''' trimite: serverul o ia din sesiune. Un cod necunoscut întoarce un DdfInfo cu listele
    ''' goale (nu excepție). Cu <paramref name="pentruGenerare"/> = True se cer și secțiunea B
    ''' și atașamentele (necesare generării PDF-ului, felia 05). Hard-fail (Throw) la non-2xx;
    ''' fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetDdfAsync(cod As String, ct As CancellationToken,
                         Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo)

    ''' <summary>
    ''' Aduce istoricul unui angajament (GET /api/forexe/istoric): un rând per înregistrare
    ''' FX_Istoric + ierarhia de clasificații pentru meniul de filtrare. Un singur drum
    ''' dus-întors pentru tot codul: vederea filtrează local. Baza NU se trimite: serverul o ia
    ''' din sesiune. Un cod necunoscut întoarce un IstoricInfo cu listele goale (nu excepție).
    ''' Hard-fail (Throw) la non-2xx; fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo)

    ''' <summary>
    ''' Aduce ordonanțările unui angajament (GET /api/forexe/ord): antetele FX_ORD (fiecare
    ''' cu SUM-ul real al liniilor) + liniile FX_ORD_TBL, plate. Un singur drum dus-întors
    ''' pentru tot codul: vederea construiește arborele din antete și filtrează liniile pe
    ''' IDORDP, local. Baza NU se trimite: serverul o ia din sesiune. Un cod necunoscut
    ''' întoarce un OrdInfo cu listele goale (nu excepție). Hard-fail (Throw) la non-2xx;
    ''' fără retry pe 401 (curge spre WithReauth).
    ''' </summary>
    Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo)

    ''' <summary>
    ''' Descarcă PDF-ul SEMNAT al unei revizii DDF (GET /api/forexe/ddf/pdf/{idrev}).
    '''
    ''' <paramref name="cachedSha"/> = suma fișierului din cache-ul local (gol când nu există):
    ''' se trimite ca <c>If-None-Match</c>, iar un 304 întoarce
    ''' <see cref="PdfDownloadStatus.NotModified"/> — cache-ul e bun, nu s-a transferat nimic.
    ''' Un 404 întoarce <see cref="PdfDownloadStatus.NotFound"/>, NU o excepție: „documentul nu
    ''' are PDF semnat" e o stare normală, iar apelantul cade pe regenerare.
    '''
    ''' Pe <see cref="PdfDownloadStatus.Content"/> octeții au trecut DEJA verificarea SHA-256
    ''' față de <c>ETag</c>-ul serverului — o nepotrivire aruncă <c>ApiException</c> cu motivul
    ''' <c>SHA_MISMATCH</c> în loc să întoarcă octeți în care nu se poate avea încredere.
    ''' Baza NU se trimite: serverul o ia din sesiune. Un 401 curge spre WithReauth.
    ''' </summary>
    Function DownloadDdfPdfAsync(idrev As Integer, cachedSha As String,
                                 ct As CancellationToken) As Task(Of PdfDownloadResult)

    ''' <summary>
    ''' Descarcă PDF-ul SEMNAT al unei ordonanțări (GET /api/forexe/ord/pdf/{idordp}).
    ''' Sora lui <see cref="DownloadDdfPdfAsync"/>, cu exact același contract.
    ''' </summary>
    Function DownloadOrdPdfAsync(idordp As Integer, cachedSha As String,
                                 ct As CancellationToken) As Task(Of PdfDownloadResult)

    ''' <summary>
    ''' Încarcă PDF-ul SEMNAT al unei revizii DDF (PUT /api/forexe/ddf/pdf/{idrev}),
    ''' înlocuind rândul existent dacă e cazul. Corpul sunt OCTEȚII BRUȚI — niciodată JSON,
    ''' niciodată base64.
    '''
    ''' <paramref name="shaPrecedent"/> = suma pe care apelantul a văzut-o ULTIMA DATĂ pentru
    ''' documentul acesta (gol / «-» când crede că nu există rând pe server). Dacă rândul de pe
    ''' server are altă sumă, răspunsul e 409 și NU se scrie nimic — nicio semnătură a altcuiva
    ''' nu se suprascrie în tăcere. Suma octeților trimiși se calculează intern (<c>PdfHash</c>),
    ''' deci apelantul nu o poate greși; numele fișierului îl derivă SERVERUL.
    '''
    ''' Se încarcă DOAR PDF-uri semnate. Cel nesemnat este un artefact derivat, care se
    ''' regenerează local. Hard-fail (Throw ApiException) la non-2xx; 401 curge spre WithReauth.
    ''' </summary>
    Function UploadDdfPdfAsync(idrev As Integer, continut As Byte(), shaPrecedent As String,
                               ct As CancellationToken) As Task(Of PutPdfResponse)

    ''' <summary>
    ''' Încarcă PDF-ul SEMNAT al unei ordonanțări (PUT /api/forexe/ord/pdf/{idordp}).
    ''' Sora lui <see cref="UploadDdfPdfAsync"/>, cu exact același contract.
    ''' </summary>
    Function UploadOrdPdfAsync(idordp As Integer, continut As Byte(), shaPrecedent As String,
                               ct As CancellationToken) As Task(Of PutPdfResponse)

    ''' <summary>
    ''' Trimite un Excel (base64) la server pentru conversie în JSON (/api/tools/process_excel).
    ''' Întoarce conținutul câmpului "data" din răspuns. Autorizare: bearer-ul sesiunii
    ''' curente (în ApiClient). Hard-fail (Throw ApiException) la non-2xx.
    ''' </summary>
    Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String)

    ''' <summary>
    ''' Trimite rezultatul unei prelucrări complete FOREXE la ingestie
    ''' (POST /api/forexe/prelucrare) și întoarce ce a răspuns serverul.
    '''
    ''' DOUĂ răspunsuri normale, amândouă fără excepție — de-asta întoarce un
    ''' <see cref="PrelucrareRaspuns"/> cu stare, nu un simplu rezultat:
    ''' <list type="bullet">
    ''' <item>200 — s-a scris. <c>Stare = Salvat</c>.</item>
    ''' <item>409 cu <c>reason = ALEGERE_UNITATE</c> — o clasificație se potrivește cu mai
    ''' multe unități, serverul a derulat tranzacția înapoi și NU a scris nimic.
    ''' <c>Stare = AlegereUnitate</c>, iar <c>AlegeriNecesare</c> poartă întrebările.
    ''' Apelantul întreabă operatorul și cheamă din nou cu ACEEAȘI sarcină, de data asta
    ''' cu <paramref name="alegeri"/> completat.</item>
    ''' </list>
    '''
    ''' Baza NU se trimite: serverul o ia din sesiune. Hard-fail (Throw ApiException) la
    ''' orice alt non-2xx; un 401 curge spre WithReauth (fără retry aici).
    ''' </summary>
    Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat,
                                    alegeri As IReadOnlyList(Of AlegereUnitate),
                                    ct As CancellationToken) As Task(Of PrelucrareRaspuns)

    ''' <summary>
    ''' FAZA UNU a ingestiei (felia 0048-03): cere serverului tabloul, fără să scrie nimic.
    '''
    ''' Serverul rulează pașii 1–7 într-o tranzacție, exact cum i-ar rula pe bune, și apoi
    ''' o derulează înapoi NECONDIȚIONAT. Ce se întoarce sunt recepțiile angajamentului,
    ''' instantaneele rămase neașezate și amprenta stării — nimic nu s-a scris.
    '''
    ''' <para>Poate răspunde tot cu 409 <c>ALEGERE_UNITATE</c>, ca
    ''' <see cref="TrimitePrelucrareAsync"/>: un angajament poate avea nevoie de DOUĂ
    ''' drumuri dus-întors înainte ca operatorul să vadă formularul de asociere. Atunci
    ''' <paramref name="alegeri"/> se completează și se cheamă din nou.</para>
    '''
    ''' <para>Rezultatul poartă starea: <c>Stare = Propunere</c> cu
    ''' <see cref="PrelucrareRaspuns.Propunere"/> completat, sau
    ''' <c>Stare = AlegereUnitate</c> cu întrebările. Un singur tip pentru amândouă,
    ''' fiindcă apelantul trebuie oricum să distingă între ele.</para>
    ''' </summary>
    Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                alegeri As IReadOnlyList(Of AlegereUnitate),
                                ct As CancellationToken) As Task(Of PrelucrareRaspuns)

    ''' <summary>
    ''' FAZA DOI a ingestiei (felia 0048-03): trimite deciziile operatorului și COMITE.
    '''
    ''' <paramref name="rezultat"/> trebuie să fie ACELAȘI payload pe care l-a văzut
    ''' propunerea — <c>RandIstoric</c> din decizii este indicele rândului în
    ''' <c>TabelIstoric</c> (F24), nu o cheie de bază de date. De-asta fișierul local
    ''' păstrează sarcina utilă exact cum a fost trimisă.
    '''
    ''' <para>Un 409 cu <c>reason = STARE_MODIFICATA</c> înseamnă că baza s-a schimbat între
    ''' cele două faze: nimic nu s-a scris, iar operatorul trebuie să descarce din nou.
    ''' Ajunge ca <see cref="ApiException"/> cu <c>Reason</c> completat, nu ca stare — spre
    ''' deosebire de ALEGERE_UNITATE, aici nu există nimic de răspuns, doar de reluat.</para>
    ''' </summary>
    Function SalveazaAsociereaAsync(rezultat As PrelucrareRezultat,
                                    amprenta As String,
                                    decizii As IReadOnlyList(Of DecizieAsociere),
                                    alegeri As IReadOnlyList(Of AlegereUnitate),
                                    ct As CancellationToken) As Task(Of PrelucrareRaspuns)

    Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T)
    Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse)
End Interface
