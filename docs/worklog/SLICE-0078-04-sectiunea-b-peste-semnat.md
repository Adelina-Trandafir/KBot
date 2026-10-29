# SLICE-0078-04 — A doua semnătură pe DDF (Secțiunea B după A) + proba pe banc

Regrupare documentară D001-05 (25.10.2026): continuarea [0078-06](#slice-0078-06) este inclusă mai jos cu textul, probele și limitele sale istorice. Referințele «§» se citesc în intervenția care le conține.

Data: 28.09.2026. Cererea operatorului: în DDF, după ce Secțiunea A e semnată și salvată, Secțiunea B se poate completa și semna în continuare (verificat de operator în Adobe Reader deschis de mână). Două probleme:

1. În K-BOT: semnat A, salvat, redeschis (clic pe revizie în arborele DDF, fișier din «Fișiere», «Generează PDF final» din Rezervări), semnat B → la salvare, o eroare de K-BOT despre versiuni care nu se potrivesc.
2. Pe bancul de semnare: o probă «DDF nou (fără rezervare) → semnat doar A → salvat → încărcată Secțiunea B → redeschis → se mai poate semna B?».

Număr: **0078-04** (propus, sub-felie a 0078; operatorul nu l-a contestat, dar nici nu l-a confirmat explicit).

---

## 1. Ce s-a schimbat și de ce

### 1.1 Eroarea la salvarea semnăturii B (IPOTEZĂ din cod, nu dovedită din jurnale)

Jurnalele / textul exact al erorii **nu** au fost văzute (sunt pe alt PC; operatorul le trimite). Diagnosticul de mai jos e o citire a codului.

Fiecare încărcare trimite serverului `X-Sha-Precedent` = suma versiunii pe care clientul crede că o are serverul; dacă serverul are altceva → **409** și mesajul «Între timp altcineva a salvat pe server o altă versiune semnată a acestui document…» (`PdfSigningSession.UploadAsync`).

`DdfView.EnsureSigning`, ca `OrdView` și `NoteCabView`, păstra sesiunea dacă revizia și calea erau aceleași, fără compararea sumei serverului. Calea cache-ului rămânea aceeași:

- semnat A → sesiunea încarcă, `ServerSha = shaA`;
- «Generează PDF final» încarcă PDF-ul final peste el → pe server `shaFinal`; vederea se reîncarcă (`Reincarca`), rândul reviziei are `shaFinal`, fișierul din cache e înlocuit cu cel de pe server — dar sesiunea VECHE (cu `shaA`) e păstrată, fiindcă calea e aceeași;
- semnat B → încărcare cu precedent `shaA`, serverul are `shaFinal` → **409**.

Reparat în cele trei vederi prin `PdfSigningSession.Matches(kind, id, path, serverSha)`: suma serverului trebuie să coincidă și ea, altfel sesiunea se recreează cu baza câmpurilor semnate din fișierul nou. După încărcarea proprie, `r.PdfSha256 = NewSha` păstrează sesiunea curentă.

### 1.2 Mesajul fals «nu era identic cu originalul» după «Generează PDF final»

`GenereazaPdfFinalAsync` încărca PDF-ul final, dar lăsa în cache-ul local copia intermediară semnată pe A. La următoarea deschidere, `PdfCache` găsea sume diferite, descărca și arăta «Fișierul semnat de pe acest calculator nu era identic cu originalul de pe server». Acum PDF-ul încărcat e scris și în cache (`SignedPdfFiles.WriteCache`, calea `DdfPdfLocator.ExpectedPath`); eșecul nu e fatal (logat).

### 1.3 Proba pe banc: Secțiunea B scrisă ÎN documentul semnat pe A

`KBot.Xfa/XfaSignedDocument.vb` (NOU):
- `ReadFormData(bytes)` — datele formularului XFA (`form1` de sub `xfa:data`);
- `FillIncremental(in, out, xml)` — aceleași reguli de completare ca generarea (`AdobeUtils.ModifyXfaFromXml`, devenit `Friend`), în mod **append**, fără machetă și fără atașamente: octeții semnați rămân neatinși, schimbarea e o revizie nouă după ei;
- `CheckSignatures(bytes)` — pentru fiecare semnătură: revizia, dacă acoperă tot documentul, integritatea (`PdfPKCS7.Verify`).

**Banc nou** (a doua trecere, la cererea operatorului: «nu am niciun DDF doar cu Secțiunea A»): DevHarness ▸ «Adobe/PDF» ▸ «DDF — Secțiunea A semnată, apoi Secțiunea B inserată și semnată (doar local)» (`DdfSectiuneaBHarnessForm`). **Nimic nu se încarcă pe server** (fără sesiune de semnare; serverul e doar CITIT); toate fișierele stau în `Temp\PDF` (`TempPdfStore`, golit la pornire).
1. «Autentificare…», **cod angajament + IDREV** (serverul citește DDF-ul după cod — `GET /api/forexe/ddf?cod=` —, nu există rută după IDREV; n-am schimbat serverul), «Generează DDF doar cu Secțiunea A»: generarea REALĂ a documentului intermediar (`DdfPdfGenerator`, `DdfPdfMode.Interim`, ca «Generează» din vederea DDF) ▸ `BANC_DDF_<idrev>_A.pdf`, afișat în vizualizatorul real (motorul din «Setări»). Operatorul semnează A și salvează — capcana scrie înapoi în același fișier.
2. «Deschide un PDF salvat…» — redeschide PE LOC (fără copie) orice PDF din `Temp\PDF`.
3. «Inserează Secțiunea B»: rândurile Secțiunii B ale reviziei de pe server când există (după trimitere; construite cu `DdfXmlBuilder.BuildFormXml` și tăiate la `SubformSectiuneaB`, deci restul formularului semnat nu se rescrie); altfel — angajament nou, netrimis, fără rezervare — din rândurile Secțiunii A ale documentului + codul și indicatorul scrise (program și SSI ca în A, anterior 0, influența = valoarea curentă). Bifa opțiunii 1 (`CheckBox9`). Documentul e eliberat din Adobe, scris incremental într-un fișier NOU `…_B.pdf` (sursa `_A` rămâne), se verifică că octeții semnați au rămas identici (prefix), se loghează semnăturile + integritatea și se deschide `…_B.pdf`.
4. Operatorul verifică în panoul de semnături Adobe dacă A e încă valabilă, semnează B și salvează (tot local); «Verifică semnăturile» recitește oricând.

Bancul vechi «Semnare PDF» a rămas neschimbat, în afară de citirea semnăturilor: ambele bancuri folosesc `PdfSignatureReport` (câmpuri, roluri, semnatar + integritatea și «acoperă tot documentul»).


### 1.4 A treia trecere (28.09.2026, după prima probă a operatorului pe banc)

Ce a raportat operatorul: (a) fără Secțiunea B completată, butonul «Validează» din PDF nu activează câmpul de semnătură; (b) «Inserează Secțiunea B» strică atașamentul NOTAFD.xml (completarea de mână în Adobe nu îl strică); (c) Acrobat cade: `Access violation c0000005` în `DigSig.api` (`DigSig!ObsoleteProc+0xb523f`, citire de la adresa 0), pe firul de mesaje al ferestrei.

- **Documentul intermediar are acum Secțiunea B PROVIZORIE** (cererea operatorului): un rând per rând al Secțiunii A, cod angajament `__________` (10 × `_`), indicator `___` (3 × `_`), program `0000000000` (10 × `0`) în Secțiunea A ȘI în B (formular + NOTAFD), bifa opțiunii 1 pusă. Valorile din rând: SSI = textul din Cell3 al Secțiunii A, anterior = `ValPrec`, influența = `ValCur` (presupunere: formularul cere doar B completată pentru «Validează»). Se aplică în `DdfXmlBuilder` (modul `Interim`), deci ȘI la «Generează» din vederea DDF pentru o revizie netrimisă, nu doar pe banc. Documentul final e neschimbat.
- **Inserarea rescrie DOAR pachetul `datasets`.** Verificat în cod: prima variantă trecea prin `XfaForm.SetXfa` al iText, care rescrie și pachetul `template` (macheta, cu scripturile ei) din DOM. După o semnătură asta e o schimbare a FORMULARULUI, nu o completare; Adobe, la tastare, schimbă doar `datasets`. `XfaSignedDocument.FillIncremental` găsește fluxul `datasets` din `AcroForm/XFA`, îl rescrie (`PRStream.SetData`) și îl marchează singurul obiect nou (`PdfStamper.MarkUsed`, mod append). **Ipoteză, neverificată:** macheta rescrisă e cauza atașamentului stricat și a căderii din DigSig. Nu am văzut fișierul `_B.pdf` stricat, nici când exact a căzut Acrobat.

Build `KBot.App` (ieșire în scratchpad — `bin\Debug` era blocat de KBot.App pornit și de Visual Studio): **0 erori, 0 avertismente**.


### 1.5 A patra trecere — dovezi din `adobe_preview.log` și din PDF-urile din `C:\KBOT\Temp\PDF` (28.09.2026)

Operatorul: după «Inserează Secțiunea B», B nu are codul angajamentului și apar erori JS. Citite (cu acordul lui): `adobe_preview.log`, `BANC_DDF_42_A.pdf`, `BANC_DDF_42_B.pdf`, `BANC_DDF_42_B.xml`.

**Verificat:**
- Generarea intermediară a pus B provizorie (datele de la offset 264348: `__________`, `___`, `0000000000`). La «Validează», formularul: *«Cod angajament trebuie sa aiba lungimea de 11 caractere… sectiunea B randul 1»* (21:26:38) — operatorul a corectat de mână la 11. Reparat: `InterimCodAngajament` = 11 × `_`.
- **Inserarea NU a scris nimic.** Partea adăugată la `_B.pdf` (3.888 octeți) conține doar obiectele 12 (XMP) și 29 (Info) + xref; datele sunt identice cu A. În modul append iText scrie doar obiectele marcate prin REFERINȚA indirectă; `MarkUsed(stream)` nu marca nimic. Reparat: `MarkUsed(referința)` + `VerifyDatasets` (fișierul scris e recitit; dacă datele nu sunt cele noi, excepție — nu mai «reușește» în gol).
- Inserarea a luat calea «rânduri de pe server» (IDREV 42 are Secțiunea B pe server: `AAB2MBD8E3F` / `AAB`), iar programul venea din `CodProgram` al sesiunii bancului — gol → `Cell3` gol. Reparat pe banc: programul lui B = programul din Secțiunea A a documentului semnat (`0000000000`), altfel sesiunea.
- Fișierul nu are drepturi de utilizare (`UR3`), dar are un lacăt `/FieldMDP` (semnătura A blochează câmpuri).

**Neexplicat:** «GeneralErrorOperation failed» (în rafală) la deschiderea `_B.pdf` și `_B_B.pdf` — deși datele lor erau IDENTICE cu A; singura diferență: Info + XMP rescrise de iText. `_A.pdf` redeschis la 21:27:16 n-a dat erori, dar probabil a fost servit din documentul deja deschis în Acrobat (fără scripturi de inițializare). Proba care desparte cauzele: deschiderea PROASPĂTĂ a lui `_A.pdf` semnat (fără nicio atingere iText după semnare). Dacă și ea dă erorile, vin din formular + lacătul FieldMDP, nu din inserare.

**Aceeași zi, după reluare.** Două erori la «Inserează», ambele REALE:
- `NullReferenceException` la `stamper.Close()`. (Am susținut greșit că e prinsă în iTextSharp, fiindcă lipsea din jurnal — dar sesiunea de depanare nu rulase până la capăt, deci lipsa din jurnal nu dovedea nimic.) Cauza, după logica sursei iText 5: în modul append, `Close()` scrie fiecare obiect marcat cu propriul `IndRef`, completat de cititor doar DUPĂ ce stamperul l-a făcut «appendable»; fluxul `datasets` era citit ÎNAINTE de stamper → `IndRef` gol. Reparat: fluxul se citește după crearea stamperului + `IndRef` pus explicit. Nevăzut încă rulând.
- `IOException` în `VerifyDatasets` (`harness_errors.log`, 21:39:57): `PdfReader(cale)` deschide cu `FileShare.Read`, iar Acrobat ține `BANC_DDF_42_B.pdf` din proba anterioară (nume refolosit). Reparat: recitire partajată; fiecare inserare scrie un nume NOU (`…_B_HHmmss.pdf`).


### 1.6 PROBAT: fluxul complet A → B → Ordonator (28.09.2026, 21:48–21:54, IDREV 42 / AAB2MBD8E3F)

Jurnalul bancului (lipit de operator) arată fluxul mers cap-coadă; toate trei semnăturile cu integritate OK, ultima acoperă tot documentul. **Cum se face un DDF semnat pe secțiuni:**

1. **Intermediar** (revizie netrimisă): Secțiunea A reală + **Secțiunea B provizorie**, bifată (`CheckBox9 = 1`), un rând B per rând A: cod angajament **11 × `_`** (formularul refuză altă lungime), indicator **3 × `_`**, program **`0000000000`** în A ȘI în B.
2. Operatorul apasă **«Validează»** din PDF: «Nu sunt erori la sectiunea A» ▸ «Completati sectiunea B, validati_o…» ▸ «Nu sunt erori la sectiunea B» ▸ «Validarea s-a terminat cu succes! A fost atasat fisierul NOTAFD.xml.» — abia atunci câmpul de semnătură se activează (cu B goală, niciodată).
3. Semnează A (dialogurile Adobe «Token Logon», «Signing»), capcana «Salvare ca» suprascrie fișierul.
4. După trimitere: **Secțiunea B reală scrisă ÎN documentul semnat pe A** (actualizare incrementală, doar pachetul `datasets`), într-un fișier NOU; rândurile de pe server, programul din Secțiunea A a documentului. Semnătura A: integritate OK.
5. Deschis, semnat B, salvat; apoi Ordonatorul — toate în același fișier.

**Regulile iText în mod append** (`XfaSignedDocument.FillIncremental`): doar fluxul `datasets` (nu `XfaForm.SetXfa`, care rescrie și macheta); fluxul se citește DUPĂ crearea `PdfStamper` (altfel `IndRef` gol → `NullReferenceException` în `Close()`); `MarkUsed(referința)`; recitire partajată + comparare; nume nou la fiecare scriere (Acrobat blochează și servește din memorie un nume deschis).

**Nerezolvat:**
- **Erorile JS «GeneralErrorOperation failed»** la deschiderea unui DDF SEMNAT în K-BOT (motor ActiveX, mod citire), dar nu în Adobe deschis direct. Sursa, citită din machetă (`BANC_DDF_42_A.pdf`, pachetul `template`): scripturile `initialize` ale fiecărui câmp de semnătură apelează `event.target.getField(…).signatureInfo()` la fiecare deschidere; documentul nesemnat se deschide curat. **Ipoteză:** în controlul AcroPDF documentul e «extern» și `signatureInfo()` eșuează (tot DigSig a căzut și mai devreme). Proba: aceleași fișiere cu motorul «fereastră găzduită» (WindowHost). Efect probabil: câmpurile de co-semnătură (`SignatureField11…16`, `21…26`) nu se deschid.
- **NOTAFD.xml** e atașat de «Validează»; după inserarea lui B nu s-a mai validat, deci atașamentul conține probabil tot B provizorie. De verificat și de hotărât dacă B se revalidează înainte de semnare.
- **«Generează PDF final»** regenerează încă documentul și pierde semnătura A — de mutat pe inserarea incrementală.
---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/PdfSigningSession.vb` | `Matches(kind, id, path, serverSha)` |
| `src/KBot.App/Views/DdfView.vb`, `OrdView.vb`, `NoteCabView.vb` | `EnsureSigning` compară și suma de pe server |
| `src/KBot.App/KbotForm.DdfSendMenu.vb` | PDF-ul final scris și în cache după încărcare |
| `src/KBot.Xfa/XfaSignedDocument.vb` | **NOU** |
| `src/KBot.Xfa/AdobeUtils.vb` | `ModifyXfaFromXml`, `ProcessXmlNodes` Private ▸ Friend |
| `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` | modul `Interim`: Secțiunea B + program provizorii, bifa opțiunii 1 |
| `src/KBot.App/HarnessTests/DdfSectiuneaBHarnessForm.vb` + `.Designer.vb`, `DdfSectiuneaBHarnessTest.vb` | **NOI** — bancul Secțiunii B |
| `src/KBot.App/HarnessTests/PdfSignatureReport.vb` | **NOU** — liniile de semnături comune celor două bancuri |
| `src/KBot.App/HarnessTests/PdfSigningHarnessForm.vb` | `LogSignatures` folosește `PdfSignatureReport` |

FileVersion: `KBot.Xfa` 1.4.0.0 ▸ **1.5.0.0**, `KBot.App` 1.1.0.1 ▸ **1.1.0.2**.

## 3. Rezultatele testelor

- `dotnet build src\KBot.Xfa\KBot.Xfa.vbproj`: **0 erori, 0 avertismente**.
- `dotnet build src\KBot.App\KBot.App.vbproj` (cu lanțul): **0 erori, 0 avertismente**.
- Nicio suită rulată (regula casei). Nimic rulat pe ecran.

## 4. Rămas NEVERIFICAT / amânat

1. **Cauza erorii de la 1.1 e o ipoteză.** Textul exact al erorii și jurnalele (`mesaje_operator.log`, `adobe_preview.log`, `harness_errors.log` de pe PC-ul unde s-a întâmplat; pe server linia `[forexe.pdf] … conflict (precedent=… asteptat=…)`) nu au fost văzute. Dacă mesajul a fost altul, cauza e alta.
2. **Pierderea semnăturii A în fluxul real.** «Generează PDF final» REGENEREAZĂ documentul din machetă și îl încarcă nesemnat peste cel semnat pe A — semnătura A se pierde. Dacă proba de pe banc arată că Adobe acceptă Secțiunea B scrisă incremental peste A, fluxul real trebuie mutat pe `XfaSignedDocument.FillIncremental` (date din server, nu din banc). Nefăcut: întâi rezultatul probei.
3. **Ce spune Adobe** despre A după scrierea incrementală (valabilă / «modificat după semnare») depinde de blocarea pusă de câmpul de semnătură A (FieldMDP) și de drepturile de utilizare ale machetei — iText vede doar integritatea octeților. Doar ecranul dă răspunsul.
4. **NOTAFD.xml** (atașamentul) nu e actualizat de probă: rămâne cu Secțiunea B goală.
5. **Presupunere pe banc:** Cell3 din Secțiunea A e codul SSI pe care Secțiunea B îl vrea în Cell4.
6. Calea «fișier din pagina Fișiere» care NU e fișierul canonic al reviziei (de ex. o copie din `Temp\PDF`) se deschide fără sesiune de semnare — o semnătură făcută acolo nu se încarcă. Nemodificat.
7. **Căderea Acrobat în DigSig** — nu se știe la ce pas (deschiderea `_B.pdf`, «Validează», semnarea B). De reluat proba după trecerea 1.4; dacă tot cade, e nevoie de pasul exact și de fișierul `_B.pdf`.
8. **Programul `0000000000` în Secțiunea A** rămâne și în documentul semnat pe B (inserarea schimbă doar B). Neclar dacă după trimitere programul real trebuie pus și în A — de hotărât de operator.
9. **Rândurile provizorii ale Secțiunii B** (unul per rând din A, valorile din A) sunt o presupunere; dacă «Validează» cere altă formă (de ex. totaluri pe SSI), se schimbă `InterimSectionB`.

---

<a id="slice-0078-06"></a>

## SLICE 0078-06 — Secțiunea B inserată în DDF-ul semnat pe A, în fluxul real (angajament nou)

Data: 29.09.2026. Cererea operatorului: calea probată pe banc în 0078-04/05 (A reală + B provizorie ▸ semnat A ▸ B reală scrisă incremental în fișierul semnat ▸ semnat B) se aplică la crearea unui angajament nou (cod «!…»):

1. «Generează» în pagina «Document» a DDF-ului, pentru revizia netrimisă: ambele secțiuni; B cu valorile provizorii pe care le are MariaDB (codul «!…»).
2. Utilizatorul care a generat semnează A.
3. Alt utilizator îl trimite în FOREXE (Creare Angajament); codurile reale se salvează în MariaDB.
4. La deschiderea DDF-ului în vederea «Doc. Fund.»: documentul cu A semnată + Secțiunea B nouă inserată, ca pe bancul `DdfSectiuneaBHarnessForm`.
5. Semnează B (nu e obligatoriu) ▸ se salvează pe server; fără semnătură nu se salvează nimic.

Număr: **0078-06** (propus, sub-felie a 0078, ca 0078-04/05; neconfirmat de operator).

Hotărâri ale operatorului (întrebate, 29.09.2026):
- «Generează PDF final» nu mai regenerează un PDF semnat pe A: A+B semnate ▸ doar trece revizia la etapa 3; doar A ▸ se deschide vederea DDF cu B inserată, iar etapa 3 se pune când semnătura B ajunge pe server.
- Inserarea pune ȘI capturile FOREXE (Table4), nu doar rândurile B.

---

### 1. Ce s-a schimbat și de ce

#### 1.1 Documentul intermediar: codul provizoriu din MariaDB

`DdfXmlBuilder.InterimSectionB` (mod `Interim`): codul angajamentului din B = codul revizuit din antet (`FX_DDF.CodAngajament`, la angajament nou «!» + 10 caractere = 11) când are exact 11 caractere, altfel `___________`. Indicatorul = `FX_DDF_REV_SB.CodIndicator` (potrivit după poziție, doar când A și B au același număr de rânduri) când are exact 3 caractere, altfel `___`.

**Verificat în machetă** (scripturile din `C:\KBOT\Temp\PDF\DDF_NR_12_REV_1_AAB2KXS6FKM.PDF`): «Validează» cere cod angajament = **11** caractere și indicator = **3** caractere în Secțiunea B. Indicatorul provizoriu al K-BOT e «!» + 3 = **4** caractere ▸ formularul l-ar refuza, deci în practică indicatorul rămâne `___`. Programul rămâne `0000000000` (A și B), SSI și sumele ca înainte.

#### 1.2 Vederea DDF: Secțiunea B scrisă în documentul semnat pe A

`DdfView.EnsureSignedPdfAsync`, după ce copia de pe server e verificată în cache (`PdfCache` ▸ `Gata`), apelează `SectiuneaBInseratAsync`. Inserarea are loc când (`DdfSectionBInsert.IsCandidate`):
- revizia e trimisă (`StareTrimitere` 2 sau 3);
- PDF-ul de pe server e semnat pe A și NU pe B (`FX_DDF_REV.Semnatura`);
- angajamentul are deja codul FOREXE (nu începe cu «!»);
- și (`ServerRowsReady`) toate rândurile `FX_DDF_REV_SB` ale reviziei au cod de 11 și indicator de 3 caractere, fără «!» și fără `_`. Altfel: un mesaj (o dată pe revizie) că B nu are încă codurile, și se arată documentul semnat pe A.

Inserarea (`DdfSectionBInsert.InsertAsync`, clasă NOUĂ, același drum ca bancul):
- datele reviziei cu `pentru_generare=1`; rândurile B de pe server; capturile FOREXE ale reviziei (`DdfPdfGenerator.CapturileAsync`, devenit `Friend`);
- XML-ul = `DdfXmlBuilder.BuildFormXml` tăiat la `SubformSectiuneaB` (rânduri + `CheckBox9` + `Subform51/Table4`); programul lui B = programul din Secțiunea A a documentului semnat;
- `XfaSignedDocument.FillIncremental` (doar `datasets` + NOTAFD.xml) într-un fișier NOU din zona de lucru: `<nume canonic>_B_HHmmss.pdf`; sursa se citește partajat și nu se atinge;
- verificare: octeții semnați = prefixul fișierului nou, altfel excepție (nu se arată nimic care ar fi pierdut semnătura A); se numără capturile ajunse în Table4.
- Mesaj către operator: B pusă, semnați B, fără semnătură nu se salvează nimic (+ avertisment dacă au intrat mai puține capturi decât s-au trimis). Linia NOTAFD în `mesaje_operator.log`.
- Fișierul făcut se refolosește cât timp suma de pe server e aceeași (`_inserariB`), deci un al doilea click nu mai face alt fișier.

Fișierul nou devine calea rezolvată a reviziei ▸ `EnsureSigning` pornește sesiunea de semnare pe el, cu precedentul = suma de pe server și cache-ul = calea canonică. Semnat B ▸ sesiunea încarcă (și scrie cache-ul). Nesemnat ▸ nimic pe server.

#### 1.3 După încărcarea semnăturii B: etapa 3

`DdfView.OnSigningCompleted`: încărcare reușită, documentul era cel inserat, rolurile conțin B, revizia e la etapa 2 ▸ comanda nouă `DdfActiune.FinalizeazaPdf` către shell (`KbotForm.FinalizeazaPdfDupaSemnareBAsync`): recitește revizia; dacă e la etapa 2 cu A și B semnate, o trece la etapa 3 (`SeteazaStareTrimitereDdfAsync(FinalPdf)`) și reîncarcă. **Excepție:** Rev 0 a unui angajament creat în K-BOT (`FX_DDF.Manual`) cât timp FOREXE nu e «În derulare» — acolo «Definitivează» / «Derulează» vin întâi (varianta a); etapa se mută mai târziu din «Generează PDF final».

#### 1.4 «Generează PDF final» nu mai regenerează un PDF semnat

`KbotForm.GenereazaPdfFinalAsync` (meniul Rezervări și trimiterea unei revizii de angajament existent) citește revizia proaspăt:
- semnată A și B ▸ doar etapa 3;
- semnată doar A ▸ mesaj «nu se mai generează din nou…» și apelantul deschide vederea DDF, care inserează B (1.2); etapa 3 la încărcarea semnăturii B (1.3);
- nesemnată ▸ ca înainte (regenerare completă + încărcare nesemnată + etapa 3).

#### 1.5 Bancul

`DdfSectiuneaBHarnessForm` folosește acum `DdfSectionBInsert.BuildFillXml` și `SectionAProgram` (o singură construcție a lui B pentru banc și producție; bancul nu trimite capturi).

### 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/Ddf/DdfSectionBInsert.vb` | **NOU** — condițiile, XML-ul lui B, inserarea în zona de lucru |
| `src/KBot.App/Views/DdfView.vb` | `SectiuneaBInseratAsync`, `IsInsertedFile`, `_inserariB`, cererea `FinalizeazaPdf` după încărcarea B |
| `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` | B provizorie cu codul / indicatorul din MariaDB când au lungimea cerută (`InterimValue`) |
| `src/KBot.App/Views/Ddf/DdfPdfGenerator.vb` | `CapturileAsync` Private ▸ Friend |
| `src/KBot.App/Views/Ddf/DdfComanda.vb` | `DdfActiune.FinalizeazaPdf` |
| `src/KBot.App/KbotForm.Ddf.vb` | ramura `FinalizeazaPdf` |
| `src/KBot.App/KbotForm.DdfSendMenu.vb` | `GenereazaPdfFinalAsync` fără regenerare pe PDF semnat; `FinalizeazaPdfDupaSemnareBAsync` |
| `src/KBot.App/HarnessTests/DdfSectiuneaBHarnessForm.vb` | folosește constructorul comun al lui B |

FileVersion: `KBot.App` rămâne **1.1.0.2** (crescut în 0078-04; 0078-05 îl nota «nelivrat» — nu am putut verifica dacă între timp s-a livrat; dacă da, de crescut la publicare).

### 3. Rezultatele testelor

- `dotnet build src\KBot.App\KBot.App.vbproj` (ieșire în afara `bin\`): **0 erori, 0 avertismente**.
- Nicio suită rulată, nimic pornit pe ecran (regula casei).

### 4. Rămas NEVERIFICAT / amânat

1. **Nimic rulat.** Fluxul real (generare ▸ semnat A ▸ trimitere ▸ inserare în vedere ▸ semnat B ▸ încărcare ▸ etapa 3) nu a fost văzut; bancul a probat doar pașii de fișier.
2. **Codul «!…» în Secțiunea B provizorie**: are lungimea 11, dar nu știu dacă «Validează» / XSD-ul NOTAFD acceptă «!» (am văzut doar verificarea de lungime). Dacă refuză, `InterimValue` trebuie să cadă pe `___________`.
3. **Capturile în Table4 pe documentul semnat**: `AdobeUtils.ProcessXmlNodes` caută `Table4` în datele documentului; dacă documentul semnat nu are nodul, capturile nu intră (mesajul spune câte au intrat). Nu știu nici dacă Adobe desenează imaginile venite într-o actualizare incrementală, nici cât crește fișierul.
4. **Angajament nou, B semnată înainte de «Derulează»**: capturile făcute la «Definitivează» / «Derulează» NU mai intră în PDF (după semnătura B documentul nu se mai completează); «Generează PDF final» doar mută etapa.
5. **Programul `0000000000`** rămâne în A și B (ca pe banc).
6. **Două mesaje la trimiterea unei revizii de angajament existent** (cel din «Generează PDF final» și cel al inserării din vedere). Acceptat; de redus dacă deranjează.
7. Dacă semnătura B e încărcată din alt loc decât documentul inserat (de ex. fișier ales din pagina «Fișiere»), comanda `FinalizeazaPdf` nu pleacă; etapa se mută din «Generează PDF final».

### 5. Jurnalul din teren, 29.09.2026 15:15–15:19 (KBot 1.1.0.3, lipit de operator)

- **Eroare reală, reparată:** `NullReferenceException` în `ReaderHostPreview.pnlHost_SizeChanged` (de două ori, la prima deschidere a vederii). `pnlHost.SizeChanged` se ridică în `InitializeComponent`, înainte ca constructorul să creeze `_host`. Acum `_host?.Relayout()`. Eroarea era prinsă și logată (fără efect pe ecran), dar umplea `harness_errors.log`. Build `KBot.App`: 0 erori, 0 avertismente. FileVersion: 1.1.0.3 e deja livrat ▸ de crescut la următoarea publicare.
- **Inserarea lui B nu apare în jurnal:** documentele deschise (DDF 96 = REV 0, DDF 97 = REV 1, `AAB2DF4BC3P`) sunt `C:\KBOT\Temp\PDF\DDF_NR_1_REV_<n>_AAB2DF4BC3P.PDF` — numele canonic din zona de lucru, adică documentul GENERAT al unei revizii fără PDF semnat pe server; un document cu B inserată ar fi `…_B_HHmmss.pdf`. Nicio salvare / semnătură în acest interval. Ce a făcut operatorul și ce aștepta — de aflat, nu dedus.
