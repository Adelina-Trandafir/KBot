# SLICE-0041 — Stocarea PDF-urilor semnate pe server (`FX_DDF_PDF` / `FX_ORD_PDF`)

Data: 2026-08-17 · Plan: „Slice 00XX: Server-side PDF storage" (dat în conversație)
Număr de felie: **0041** — cel declarat de `KBOT_STATUS.md` ca «Next free» (gaura 0038/0039
rămâne neatinsă; 0038 e deja referit în comentariile din `KBot.Controls/Tree`).

---

## 1. Ce s-a schimbat și de ce

PDF-urile **semnate** (revizii DDF, ordonanțări ORD) nu mai trăiesc doar pe discul unui
operator: se stochează în MariaDB, o tabelă per familie de document. Discul devine **cache**.

Trei reguli au condus toată implementarea, fiindcă un PDF semnat digital trebuie să se întoarcă
de pe server **bit cu bit**:

1. **Octeți bruți pe fir.** `application/octet-stream` la încărcare, `application/pdf` la
   descărcare. Niciodată JSON, niciodată base64, niciun pas prin mod text.
2. **SHA-256 verificat la amândouă capetele, în amândouă sensurile.** La încărcare serverul
   recalculează suma peste corpul primit și respinge cu 400 la nepotrivire (nu scrie nimic);
   la descărcare trimite suma stocată ca `ETag`, iar clientul o recalculează peste octeții
   primiți și refuză să scrie cache-ul dacă nu corespunde.
3. **Concurență optimistă.** `X-Sha-Precedent` poartă suma pe care clientul a văzut-o ultima
   dată (`-` = «cred că nu există rând»). Dacă rândul de pe server are altă sumă → 409, nu se
   scrie nimic. Nicio semnătură a altcuiva nu se suprascrie tăcut.

Decizii de plan reținute ca atare: se stochează **doar semnate** (cele nesemnate sunt artefacte
derivate, se regenerează local), **fără istoric** (cheie unică → re-semnarea înlocuiește rândul),
**fără coloană `Semnat`** (existența rândului ESTE semnalul), **`LONGBLOB` nu `MEDIUMBLOB`**
(plafonul de 16 MB al lui MEDIUMBLOB cade exact pe estimarea maximă), **fără compresie de blob**
în v1.

Zona nesemnată e nouă și separată: `<AppDir>\TempPdf\`, golită la fiecare pornire. Regula
„temporar, șters la fiecare deschidere" trăiește **exclusiv** acolo — cache-ul semnat nu se
golește niciodată în bloc, se înlocuiește doar când suma nu mai corespunde.

---

## 2. Fișiere atinse

### DDL (de aplicat manual, pe FIECARE bază de unitate)
| Fișier | Ce e |
|---|---|
| `sql/0041_fx_pdf_tables.sql` | **NOU.** `FX_DDF_PDF` (cheie `IDREV`) + `FX_ORD_PDF` (cheie `IDORDP`), LONGBLOB, `UNIQUE` pe cheia părinte, FK `ON DELETE CASCADE`. Marcat **NEVERIFICAT** — vezi §4. |

### Server (`PYTHON/`)
| Fișier | Ce s-a făcut |
|---|---|
| `routes/forexe/pdf.py` | **NOU.** Cele patru rute. Logica stă o singură dată, parametrizată pe un dicționar (tabelă / coloană-cheie / tabelă părinte) — rutele DDF și ORD sunt identice în afara acestor trei nume. |
| `routes/forexe/__init__.py` | Import + comentariu de inventar pentru `pdf.py`. |
| `routes/forexe/ddf.py` | `LEFT JOIN FX_DDF_PDF` → `pdf_sha256` / `pdf_dimensiune` / `pdf_data_modif` pe fiecare rând de revizie. Aditiv; nimic existent nu s-a schimbat. Adăugat `_iso_dt` (ISO **cu oră** — două semnări din aceeași zi trebuie să se deosebească). |
| `routes/forexe/ord.py` | Idem, `LEFT JOIN FX_ORD_PDF` pe antete. Coloanele noi stau **înaintea** fragmentului opțional `{cale_pdf}`, ca pozițiile fixe din despachetare să nu depindă de proba de schemă. |
| `main.py` | `MAX_CONTENT_LENGTH` 17 MB + handler 413 (§3). |

`LEFT JOIN` și nu subinterogare: cheia părinte e **UNIQUE** în tabela de PDF-uri, deci cel mult
un rând per document — fan-out imposibil. (Aceeași justificare ca `FX_ORD_PART` în `ord.py`.)

### Client (`src/`)
| Fișier | Ce s-a făcut |
|---|---|
| `KBot.Common/PdfHash.vb` | **NOU.** SHA-256 hex minuscule: `Compute` (octeți), `ComputeFile` (flux; `Nothing` când fișierul lipsește = „n-am cache"), `AreEqual`. O singură implementare pentru amândouă sensurile. |
| `KBot.Common/TempPdfStore.vb` | **NOU.** `<AppDir>\TempPdf\` + `Wipe()` care **nu aruncă niciodată** (fișier ținut deschis de Adobe → se sare cu avertisment, pornirea continuă). |
| `KBot.Api/PdfDownloadResult.vb` | **NOU.** `Content` / `NotModified` / `NotFound` ca tip explicit — un `Byte()` care poate fi `Nothing` din două motive diferite ar amesteca „nu are PDF" cu „cache-ul e bun". Plus `PutPdfResponse`. |
| `KBot.Api/ApiClient.vb` + `IApiClient.vb` | Patru metode noi (download/upload × DDF/ORD) peste două helper-e private. Maparea celor trei câmpuri noi în POCO-uri. |
| `KBot.Api/UpsertAngajamenteRequest.vb` | Câmpurile de fir `pdf_sha256` / `pdf_dimensiune` / `pdf_data_modif` pe `GetDdfRevizieRow` și `GetOrdHeaderRow`. |
| `KBot.Domain/DdfInfo.vb`, `OrdInfo.vb` | `RevizieRow` / `OrdHeaderRow`: `PdfSha256`, `PdfDimensiune`, `PdfDataModif` + `ArePdfSemnat`. |
| `KBot.App/Views/PdfCache.vb` | **NOU.** Rezolvarea cache-or-download, comună celor două vederi. Scrie prin `.part` + `File.Move(overwrite)` — o întrerupere la jumătate ar lăsa altfel un PDF trunchiat purtând numele unui document semnat valid. |
| `KBot.App/Views/DdfView.vb` | `EnsureSignedPdfAsync` pe click de frunză (stale-guard pe `Idrev`); generarea NESEMNATĂ scrie acum în `TempPdf\`, nu în cache-ul persistent. |
| `KBot.App/Views/OrdView.vb` | `EnsureSignedPdfAsync`, sora celei de mai sus. |
| `KBot.App/Program.vb` | `TempPdfStore.Wipe()` la pornire, după `ThemeManager.Initialize()`. |
| `tests/KBot.App.Tests/*.vb` (7 fișiere) | Cele patru metode noi adăugate ca `Throw New NotSupportedException()` în dublurile `IApiClient` — contractul le cere, dar niciun test nu le exercită. |

Ce **NU** s-a schimbat, deși ar fi fost tentant: convențiile de nume și de folder ale
PDF-urilor (`DdfPdfLocator` / `OrdPdfLocator` sunt neatinse), enumerarea de disc din
`DdfFileBrowser` (repointarea ei e o decizie separată), și câmpul `pdf` (vechiul `CalePDF`) din
ruta ORD.

---

## 3. Abateri de la plan (toate deliberate, niciuna tăcută)

1. **`MAX_CONTENT_LENGTH` — globală, cum a cerut operatorul.** Planul cerea 17 MB; `main.py`
   avea deliberat `None` («pentru imagini mari»). I s-a semnalat Adelinei că o limită globală
   atinge **toate** rutele, nu doar cele de PDF, iar decizia a fost: **globală, dar cu un mesaj
   clar când plafonul nu e de ajuns**. Deci: 17 MB global + handler `413` care spune plafonul și
   ce e de făcut. **Consecință de acceptat conștient:** încărcările mari existente (atașamente
   base64, capturi de ecran, upload FTP) primesc de acum 413 peste 17 MB. Capturile trebuie
   comprimate la generare.
2. **Nu exista niciun „catch-all Flask error handler".** Planul (§4, §5) presupunea unul.
   `main.py` n-avea niciunul, iar Werkzeug taie cererea prea devreme ca vreo rută să o vadă —
   fără handler, clientul ar fi primit pagina HTML implicită și operatorul un mesaj gol. S-a
   adăugat handler-ul de 413, cu corp JSON românesc și `reason: PAYLOAD_TOO_LARGE`.
3. **`_iso_dt` în loc de `_iso`** pentru `DataModif`: `_iso` taie ora (`.date()`), ceea ce ar
   face două semnări din aceeași zi să arate identic.
4. **Plafonul se verifică și în rută**, nu doar global: `MAX_PDF_BYTES` în `pdf.py` dă un 413 cu
   mesaj propriu. Redundant azi (limita globală taie mai devreme), dar ruta nu are voie să
   depindă de o valoare de config pe care altcineva o poate schimba.
5. **DDL-ul e livrat NEVERIFICAT**, cu proba în cap. Vezi §4.

---

## 4. Ce a rămas NEVERIFICAT / amânat

* **DDL — tipurile cheilor părinte.** Planul cere confirmarea numelui **și a tipului** PK-urilor
  `FX_DDF_REV` / `FX_ORD` printr-o probă `information_schema` pe o bază vie. Proba **NU s-a
  putut face**: fără acces la o bază reală, iar în depozit nu există niciun dump DDL MariaDB.
  Ce se știe și de unde: **numele** `IDREV` e coroborat de SQL-ul rutei care rulează azi
  (`routes/forexe/ddf.py`, `r.IDREV`) și de FK-ul citat acolo (`FX_DDF_REV_SA_ibfk_4`); numele
  `IDORDP` e coroborat de nota «capcana 1» din `routes/forexe/ord.py`. **Tipurile nu sunt
  verificate** — un FK cu tip nepotrivit (INT vs INT UNSIGNED) cade la `CREATE TABLE` cu
  errno 150. Fișierul poartă un antet `!!! NEVERIFICAT !!!` cu interogarea exactă de rulat
  întâi. Decizia Adelinei (2026-08-17): se livrează așa, marcat.
* **Valorile de desfășurare cerute de plan §5 nu s-au putut citi** (nu s-a atins VPS-ul):
  `nginx client_max_body_size` (trebuie pus pe **20m**, altfel default-ul de 1 MB respinge orice
  încărcare cu un 413 care nu ajunge niciodată la Flask) și `max_allowed_packet` (se vrea
  ≥ 32M; proba `SHOW VARIABLES LIKE 'max_allowed_packet'` e în antetul fișierului SQL).
  **Amândouă rămân de făcut de operator, iar valoarea curentă a lui `max_allowed_packet` nu a
  fost înregistrată — nu am putut.**
* **Nimic din felia asta n-a atins o bază vie și nimic n-a fost văzut pe ecran.** Cele patru
  rute n-au rulat niciodată împotriva unei tabele reale; fluxul de vedere n-a fost privit.
* **Gunicorn cu un singur worker:** o încărcare de 16 MB ocupă worker-ul pe toată durata ei.
  Acceptat la scara curentă — documentat, nu „rezolvat".
* **Copiile de siguranță** per unitate cresc cu totalul PDF-urilor stocate. Notat, fără acțiune.
* **Încărcarea nu are încă apelant.** `UploadDdfPdfAsync` / `UploadOrdPdfAsync` există ca
  primitive de transport; cine le cheamă vine cu **felia 0021** (semnarea), care va face apoi și
  propriul `UPDATE` pe `FX_DDF_REV.Semnatura`. Ruta de aici **nu atinge** coloana aceea.
* **`FX_ORD.CalePDF`** (câmpul `pdf` din ruta ORD) devine caduc odată cu stocarea pe server.
  Scoaterea lui e o decizie separată a Adelinei — trecută la fire deschise în STATUS.
* **`DdfFileBrowser`** enumeră mai departe discul. Repointarea lui pe `PdfSha256` e o
  continuare, nu parte din 0041.
* **Compresia blobului** rămâne amânată până la o măsurătoare pe fișiere semnate reale.

---

## 5. Rezultatele testelor

**Python** (`PYTHON/.venv`, suita offline):
```
75 passed, 15 skipped in 21.92s     # 0 fail / 0 error
```
Byte-compilare curată pentru `pdf.py`, `ddf.py`, `ord.py`, `forexe/__init__.py`, `main.py`.

**.NET** — `dotnet build KBot.sln`: **0 erori, 0 avertismente**.

Proiectele de test atinse (rulate individual, NU pe soluție — regula casei despre DevHarness):

| Proiect | Rezultat |
|---|---|
| `KBot.Common.Tests` | 58 passed / 0 failed |
| `KBot.Api.Tests` | 67 passed / **1 failed** |
| `KBot.App.Tests` | 148 passed / **13 failed** |
| `KBot.Domain.Tests` | 14 passed / **3 failed** |

**Cele 17 căderi sunt PREEXISTENTE, nu ale acestei felii.** Verificat empiric, nu presupus: cu
modificările puse la o parte (`git stash`), aceleași proiecte dau **exact aceleași cifre**
(13 / 1 / 3). Cauza majorității e `RevizieRow.EtichetaRevizie`, comentat într-o felie
anterioară (`Return $"{data}" '$"{numar} - {data}"`) fără ca testele să fie actualizate;
restul sunt în `DdfXfaParser`, `XfaXmlPreview`, `MainFormNavItems` și `IstoricView`.
**Nu s-au atins** — sunt o datorie separată, care merită propria felie.

Teste noi: **niciunul**. Regula casei spune „fără teste automate implicit"; candidatul real
(matricea sumă/concurență pe rutele PUT) cere confirmarea explicită a Adelinei înainte să fie
scris.

---

## 6. Ce trebuie să facă operatorul înainte ca felia să funcționeze

1. Rulează proba din antetul lui `sql/0041_fx_pdf_tables.sql` pe o bază de unitate și
   confirmă numele + tipul PK-urilor. Potrivește tipurile din fișier cu ce răspunde ea.
2. Aplică DDL-ul pe **fiecare** bază de unitate.
3. `nginx`: `client_max_body_size 20m;` pe blocul serverului K-BOT.
4. Verifică `max_allowed_packet` ≥ 32M pe VPS.
5. Repornește serverul Flask (rutele noi + noul `MAX_CONTENT_LENGTH`).
