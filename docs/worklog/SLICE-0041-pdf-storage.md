# SLICE-0041 — Stocarea PDF-urilor semnate pe server (`FX_DDF_PDF` / `FX_ORD_PDF`)

Istoric al implementării din 17.08.2026: „încărcarea nu are apelant” și pașii de instalare sunt stări de la acel reper; nu se execută prin consolidare. Contractul octeților, sumelor și concurenței rămâne în corp.

Data: 2026-08-17 · Plan: „Slice 00XX: Server-side PDF storage" (dat în conversație) Număr de felie: **0041** — cel declarat de `KBOT_STATUS.md` ca «Next free» (gaura 0038/0039 rămâne neatinsă; 0038 e deja referit în comentariile din `KBot.Controls/Tree`).

---

## 1. Ce s-a schimbat și de ce

PDF-urile **semnate** (revizii DDF, ordonanțări ORD) nu mai trăiesc doar pe discul unui operator: se stochează în MariaDB, o tabelă per familie de document. Discul devine **cache**.

Trei reguli au condus toată implementarea, fiindcă un PDF semnat digital trebuie să se întoarcă de pe server **bit cu bit**:

1. **Octeți bruți pe fir.** `application/octet-stream` la încărcare, `application/pdf` la descărcare. Niciodată JSON, niciodată base64, niciun pas prin mod text.
2. **SHA-256 la ambele capete/sensuri:** serverul recalculează suma corpului primit, respinge nepotrivirea cu 400 fără scriere; la descărcare trimite suma stocată ca ETag. Clientul recalculează octeții primiți și scrie cache-ul numai dacă suma corespunde.
3. **Concurență optimistă:** X-Sha-Precedent conține ultima sumă văzută de client (`-` = rând presupus absent). Altă sumă pe server → 409 fără scriere, protejând semnătura altcuiva de suprascriere tăcută.

Decizii de plan: numai semnate (nesemnatele se regenerează local), fără istoric (cheia unică permite înlocuirea la re-semnare), fără coloană Semnat (rândul e semnalul), LONGBLOB (MEDIUMBLOB plafonat la 16 MB coincide cu estimarea maximă), fără compresie blob în v1.

PDF-urile nesemnate folosesc separat `<AppDir>\TempPdf\`, golit la fiecare pornire. Cache-ul semnat nu se golește în bloc: se înlocuiește numai la nepotrivirea sumei.

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

`LEFT JOIN` nu multiplică documentele: cheia părinte este UNIQUE în tabela PDF, deci are cel mult un rând (aceeași justificare ca `FX_ORD_PART` în `ord.py`).

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

Ce **NU** s-a schimbat, deși ar fi fost tentant: convențiile de nume și de folder ale PDF-urilor (`DdfPdfLocator` / `OrdPdfLocator` sunt neatinse), enumerarea de disc din `DdfFileBrowser` (repointarea ei e o decizie separată), și câmpul `pdf` (vechiul `CalePDF`) din ruta ORD.

---

## 3. Abateri de la plan (toate deliberate, niciuna tăcută)

1. **MAX_CONTENT_LENGTH globală:** planul cerea 17 MB; main.py avea None «pentru imagini mari». Adelina, avertizată că plafonul atinge toate rutele, a ales 17 MB global + handler 413 cu plafon și remediu. Consecință de acceptat conștient: atașamentele base64, capturile și uploadul FTP peste 17 MB primesc 413; capturile trebuie comprimate la generare.
2. **Handler Flask catch-all absent**, deși planul §4/§5 îl presupunea. Werkzeug respinge înaintea rutei; fără handler clientul primea HTML implicit, operatorul mesaj gol. Adăugat handler 413 cu JSON românesc și reason: PAYLOAD_TOO_LARGE.
3. **`_iso_dt` în loc de `_iso`** pentru `DataModif`: `_iso` taie ora (`.date()`), ceea ce ar face două semnări din aceeași zi să arate identic.
4. **Plafon și în rută:** MAX_PDF_BYTES din pdf.py produce propriul 413. Redundant cu limita globală actuală, dar independent de o configurare ce poate fi schimbată.
5. **DDL-ul e livrat NEVERIFICAT**, cu proba în cap. Vezi §4.

---

## 4. Ce a rămas NEVERIFICAT / amânat

* **DDL — tipurile cheilor părinte.** Planul cere confirmarea numelui **și a tipului** PK-urilor `FX_DDF_REV` / `FX_ORD` printr-o probă `information_schema` pe o bază vie. Proba **NU s-a putut face**: fără acces la o bază reală, iar în depozit nu există niciun dump DDL MariaDB. Ce se știe și de unde: **numele** `IDREV` e coroborat de SQL-ul rutei care rulează azi (`routes/forexe/ddf.py`, `r.IDREV`) și de FK-ul citat acolo (`FX_DDF_REV_SA_ibfk_4`); numele `IDORDP` e coroborat de nota «capcana 1» din `routes/forexe/ord.py`. **Tipurile nu sunt verificate** — un FK cu tip nepotrivit (INT vs INT UNSIGNED) cade la `CREATE TABLE` cu errno 150. Fișierul poartă un antet `!!! NEVERIFICAT !!!` cu interogarea exactă de rulat întâi. Decizia Adelinei (2026-08-17): se livrează așa, marcat.
* **Valorile de desfășurare cerute de plan §5 nu s-au putut citi** (nu s-a atins VPS-ul): `nginx client_max_body_size` (trebuie pus pe **20m**, altfel default-ul de 1 MB respinge orice încărcare cu un 413 care nu ajunge niciodată la Flask) și `max_allowed_packet` (se vrea ≥ 32M; proba `SHOW VARIABLES LIKE 'max_allowed_packet'` e în antetul fișierului SQL). **Amândouă rămân de făcut de operator, iar valoarea curentă a lui `max_allowed_packet` nu a fost înregistrată — nu am putut.**
* **Nimic din felia asta n-a atins o bază vie și nimic n-a fost văzut pe ecran.** Cele patru rute n-au rulat niciodată împotriva unei tabele reale; fluxul de vedere n-a fost privit.
* **Gunicorn cu un singur worker:** o încărcare de 16 MB ocupă worker-ul pe toată durata ei. Acceptat la scara curentă — documentat, nu „rezolvat".
* **Copiile de siguranță** per unitate cresc cu totalul PDF-urilor stocate. Notat, fără acțiune.
* **Încărcarea nu are încă apelant.** `UploadDdfPdfAsync` / `UploadOrdPdfAsync` există ca primitive de transport; cine le cheamă vine cu **felia 0021** (semnarea), care va face apoi și propriul `UPDATE` pe `FX_DDF_REV.Semnatura`. Ruta de aici **nu atinge** coloana aceea.
* **`FX_ORD.CalePDF`** (câmpul `pdf` din ruta ORD) devine caduc odată cu stocarea pe server. Scoaterea lui e o decizie separată a Adelinei — trecută la fire deschise în STATUS.
* **`DdfFileBrowser`** enumeră mai departe discul. Repointarea lui pe `PdfSha256` e o continuare, nu parte din 0041.
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

**Cele 17 căderi sunt PREEXISTENTE, nu ale acestei felii.** Verificat empiric, nu presupus: cu modificările puse la o parte (`git stash`), aceleași proiecte dau **exact aceleași cifre** (13 / 1 / 3). Cauza majorității e `RevizieRow.EtichetaRevizie`, comentat într-o felie anterioară (`Return $"{data}" '$"{numar} - {data}"`) fără ca testele să fie actualizate; restul sunt în `DdfXfaParser`, `XfaXmlPreview`, `MainFormNavItems` și `IstoricView`. **Nu s-au atins** — sunt o datorie separată, care merită propria felie.

Teste noi: **niciunul**. Regula casei spune „fără teste automate implicit"; candidatul real (matricea sumă/concurență pe rutele PUT) cere confirmarea explicită a Adelinei înainte să fie scris.

---

## 6. Ce trebuie să facă operatorul înainte ca felia să funcționeze

1. Rulează proba din antetul lui `sql/0041_fx_pdf_tables.sql` pe o bază de unitate și confirmă numele + tipul PK-urilor. Potrivește tipurile din fișier cu ce răspunde ea.
2. Aplică DDL-ul pe **fiecare** bază de unitate.
3. `nginx`: `client_max_body_size 20m;` pe blocul serverului K-BOT.
4. Verifică `max_allowed_packet` ≥ 32M pe VPS.
5. Repornește serverul Flask (rutele noi + noul `MAX_CONTENT_LENGTH`).
