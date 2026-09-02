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

    ''' <summary>
    ''' Tabloul de asociere al unui angajament, citit DIRECT din bază (felia 0048-04).
    '''
    ''' Fără sarcină utilă și fără fază de propunere: operatorul deschide legăturile deja
    ''' scrise, oricând, fără să fi descărcat nimic. Ancora fiecărui instantaneu este
    ''' <c>IDRH</c>, cheia reală — nu indicele de rând din <c>TabelIstoric</c>, care are
    ''' sens doar cât timp există o sarcină utilă care să-l dea.
    '''
    ''' <para>Un angajament fără recepții întoarce 200 cu liste goale, nu 404.</para>
    ''' </summary>
    Function GetAsociereAsync(cod As String, ct As CancellationToken) As Task(Of AsociereStare)

    ''' <summary>
    ''' Aplică un set PARȚIAL de modificări peste legăturile R ▸ H (felia 0048-04).
    '''
    ''' O singură fază, o singură tranzacție: aici nu există nimic de derulat înapoi și
    ''' nimic de re-rulat. <paramref name="amprenta"/> rămâne, fiindcă două sesiuni pot
    ''' edita același angajament în același timp.
    '''
    ''' <para>Două refuzuri sosesc ca <see cref="ApiException"/> cu <c>Reason</c> completat:
    ''' <c>STARE_MODIFICATA</c> (baza s-a mișcat — se reîncarcă tabloul) și
    ''' <c>INSTANTANEU_BLOCAT</c> (o comandă atinge o legătură pe care o ordonanțare sau
    ''' plățile ulterioare au înghețat-o). În ambele cazuri nu s-a scris nimic.</para>
    ''' </summary>
    Function SalveazaLegaturiAsync(cod As String,
                                   amprenta As String,
                                   comenzi As IReadOnlyList(Of ComandaAsociere),
                                   ct As CancellationToken) As Task(Of AsociereRezultat)

    ' ── EDITORUL DE ORDONANTARE (felia 0049) ────────────────────────────────────────────
    ' Opt apeluri peste `routes/forexe/ord_edit.py`. Baza NU se trimite niciodata (serverul o
    ' ia din sesiune); un 401 curge spre WithReauth, fara retry in client.

    ''' <summary>
    ''' Cere graful PROPUS al unei ordonantari noi (POST /api/forexe/ord/genereaza). NIMIC nu
    ''' se scrie — portul lui <c>Genereaza_ORD</c>, mutat pe server fiindca interogarile lui
    ''' bat numai tabele care traiesc acum in MariaDB.
    '''
    ''' <para><paramref name="idPlataFx"/> completat = calea interactiva pentru O SINGURA
    ''' plata; <c>Nothing</c> = toate platile neordonantate ale zilei (VBA: <c>"*"</c>).</para>
    '''
    ''' <para>O zi fara plati neordonantate intoarce 404 cu motiv, deci ajunge aici ca
    ''' <see cref="ApiException"/> — NU un draft gol, care ar arata ca un document valid fara
    ''' nicio linie.</para>
    ''' </summary>
    Function GenereazaOrdAsync(cod As String, dataOrd As Date, idPlataFx As Integer?,
                               ct As CancellationToken) As Task(Of OrdDraft)

    ''' <summary>
    ''' Citeste graful unei ordonantari EXISTENTE, in forma editorului
    ''' (GET /api/forexe/ord/draft/{idordp}). Distinct de <see cref="GetOrdAsync"/>, care e
    ''' apelul vederii 0033 si nu intoarce cheile, codurile si legaturile de care are nevoie
    ''' editarea.
    ''' </summary>
    Function GetOrdDraftAsync(idordp As Integer, ct As CancellationToken) As Task(Of OrdDraft)

    ''' <summary>
    ''' Zilele cu plati neordonantate ale unui angajament (GET /api/forexe/ord/zile) — sursa
    ''' modului in lot. Fiecare zi spune si cate ordonantari ii trebuie (limita de 25 de
    ''' parteneri per document). <paramref name="luna"/> / <paramref name="an"/> sunt optionale.
    ''' </summary>
    Function GetOrdZileAsync(cod As String, luna As Integer?, an As Integer?,
                             ct As CancellationToken) As Task(Of OrdZileInfo)

    ''' <summary>
    ''' Numarul pe care l-ar primi ACUM o ordonantare noua (GET /api/forexe/ord/nr-urmator).
    '''
    ''' <para>E o PRESUPUNERE, nu o rezervare: numarul adevarat se aloca tot in tranzactia de
    ''' salvare, care e singurul loc unde doua salvari concurente se pot aseza la rand. Intre
    ''' intrebare si salvare altcineva poate salva primul, si atunci raspunsul de aici va fi
    ''' fost gresit — de asta interfata il arata ca pe o presupunere, nu ca pe un numar.</para>
    ''' </summary>
    Function GetOrdNrUrmatorAsync(ct As CancellationToken) As Task(Of Integer)

    ''' <summary>
    ''' Scrie TOT graful ordonantarii intr-o singura tranzactie (POST /api/forexe/ord/save) si
    ''' intoarce cheile reale plus hartile <c>TempId ▸ cheie</c>.
    '''
    ''' <para>Un refuz de validare soseste ca <see cref="ApiException"/> cu mesajul romanesc
    ''' al serverului, care enumera TOATE motivele deodata, nu primul.</para>
    '''
    ''' <para>Octetii atasamentelor NU pleaca de aici: un <c>IDORDATTP</c> trebuie sa existe
    ''' inainte ca ei sa poata atarna de el. Se urca dupa, cu
    ''' <see cref="PutOrdAtasamentAsync"/>, folosind harta din raspuns.</para>
    ''' </summary>
    Function SaveOrdAsync(draft As OrdDraft, ct As CancellationToken) As Task(Of OrdSaveRezultat)

    ''' <summary>
    ''' Sterge o ordonantare cu tot ce atarna de ea (DELETE /api/forexe/ord/{idordp}) si
    ''' intoarce cate randuri s-au dus, per tabela — inclusiv cate plati s-au intors in
    ''' rezerva de neordonantate si daca a plecat si PDF-ul stocat.
    ''' </summary>
    Function DeleteOrdAsync(idordp As Integer, ct As CancellationToken) As Task(Of OrdStergereRezultat)

    ''' <summary>
    ''' Descarca octetii imaginii unui atasament
    ''' (GET /api/forexe/ord/att/{idordattp}/imagine). Acelasi contract ca la PDF-uri: 304 pe
    ''' cache valid, 404 pe «nu are imagine» (stare normala, nu exceptie), iar octetii primiti
    ''' sunt deja verificati pe SHA-256 fata de ETag.
    ''' </summary>
    Function GetOrdAtasamentAsync(idordattp As Integer, cachedSha As String,
                                  ct As CancellationToken) As Task(Of PdfDownloadResult)

    ''' <summary>
    ''' Urca octetii imaginii unui atasament (PUT /api/forexe/ord/att/{idordattp}/imagine).
    ''' Numele fisierului SE TRIMITE (e alegerea operatorului, nu o conventie derivabila);
    ''' tipul MIME il deduce serverul din primii octeti. <paramref name="shaPrecedent"/> e
    ''' concurenta optimista: o suma diferita pe server da 409 si nu se scrie nimic.
    ''' </summary>
    Function PutOrdAtasamentAsync(idordattp As Integer, numeFisier As String,
                                  continut As Byte(), shaPrecedent As String,
                                  ct As CancellationToken) As Task(Of PutAtasamentResponse)

    ''' <summary>
    ''' Sterge octetii imaginii unui atasament, lasand randul de atasament pe loc
    ''' (DELETE /api/forexe/ord/att/{idordattp}/imagine).
    ''' </summary>
    Function DeleteOrdAtasamentAsync(idordattp As Integer, ct As CancellationToken) As Task

    Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T)
    Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse)
End Interface
