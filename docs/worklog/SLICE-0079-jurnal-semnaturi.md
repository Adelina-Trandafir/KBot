# Felia 0079 — Jurnalul semnăturilor (FX_PDF_SEMNATURI)

**Data:** 23.09.2026 · **Cerută de:** operator, după primul test reușit al feliei 0078 pe banc.

## 1. Cererea

«Pe MariaDB să salvez pentru fiecare semnătură momentul, locul (IP-ul și eventual locul fizic
după IP), informațiile calculatorului și contul conectat.»

Deciziile operatorului (23.09.2026):

| Întrebare | Răspuns |
|---|---|
| Locul fizic după IP | **Doar IP-ul** se păstrează; nicio căutare de localitate acum. |
| Cine citește semnatarul și data din PDF | **K-BOT le trimite** (iTextSharp, deja pe client). |
| Unde stă tabelul | **În fiecare bază de unitate**, lângă `FX_DDF_PDF` / `FX_ORD_PDF`. |

## 2. Ce s-a construit

**Tabelul** `sql/0079_fx_pdf_semnaturi.sql` — `FX_PDF_SEMNATURI`, un rând per semnătură:

| Coloană | Sursa |
|---|---|
| `IDREV` / `IDORDP` (exact unul; FK cu ștergere în cascadă) | documentul |
| `Camp`, `Rol`, `Semnatar`, `DataSemnaturii` | PDF-ul, citit de K-BOT |
| `DataInregistrarii`, `Sha256Pdf` | serverul |
| `UN` | serverul — contul sesiunii bearer, NU ce spune clientul |
| `IpPublic` | serverul — adresa cererii (ProxyFix) |
| `IpLocal`, `NumeCalculator`, `UtilizatorWindows`, `SistemOperare`, `VersiuneKbot` | calculatorul operatorului |

**Doar semnăturile NOI.** Câmpurile deja semnate când documentul a fost deschis sunt ale altcuiva;
dacă ar fi trimise din nou, ar primi contul, IP-ul și calculatorul operatorului de acum.
`SignatureRecords.Added` scade din fișierul salvat mulțimea de câmpuri citită la pornirea
sesiunii de semnare. Copia păstrată în `PdfDeIncarcat` își ține propriile înregistrări în
`.json` (reîncercarea are loc mai târziu, când nimeni nu mai știe cum arăta fișierul înainte).

**Fără dubluri.** Serverul sare peste un rând pe care îl are deja (același document, câmp și dată
a semnăturii), deci o reîncercare după un răspuns pierdut nu dublează nimic.

**Aceeași tranzacție** ca PDF-ul și `Semnatura`: nu există PDF încărcat fără înregistrarea lui.

**Semnatar și dată:** `PdfPKCS7.SignName` (sau CN-ul certificatului); data = marca de timp a
autorității, dacă există, altfel ceasul semnatarului (`/M`). Se trimit cu fus orar; serverul le
scrie în ora lui locală, ca toate coloanele `NOW()`.

**Pe fir:** `X-Semnaturi` (lista) și `X-Statie` (calculatorul) — base64 de JSON UTF-8, fiindcă un
antet HTTP poartă doar ASCII, iar numele semnatarului poate avea diacritice. Răspunsul PUT are în
plus `semnaturi_inregistrate`.

**Ordinea e liberă:** ruta sondează tabelul la fiecare încărcare. Până rulează DDL-ul pe o bază,
încărcarea merge ca în 0078, iar înregistrările sunt doar trecute în jurnal ca sărite.

## 3. Fișiere

| Fișier | Ce |
|---|---|
| `sql/0079_fx_pdf_semnaturi.sql` | **NOU** — tabelul. |
| `PYTHON/routes/forexe/pdf.py` | `_parse_audit`, `_has_sign_log`, `_record_signatures`, pasul 8 din `_incarca`. |
| `PYTHON/tests/test_pdf_semnaturi.py` | **NOU** — scris, **nerulat**. |
| `src/KBot.Api/PdfSignatureRecord.vb`, `StationInfo.vb` | **NOI** — o înregistrare; datele calculatorului. |
| `src/KBot.Api/IApiClient.vb`, `ApiClient.vb` | Parametrul `semnaturi` la cele două încărcări; antetele `X-Semnaturi` / `X-Statie`. |
| `src/KBot.Xfa/PdfSignatures.vb` | `PdfSignatureDetail` (câmp, rol, semnatar, dată); `PdfSignatureInfo.Details`, `RoleName`. |
| `src/KBot.App/Views/SignatureRecords.vb` | **NOU** — «doar cele noi» + unirea cu copia păstrată. |
| `src/KBot.App/Views/PdfSigningSession.vb`, `PendingPdfUploads.vb` | Trimit / păstrează înregistrările. |
| `src/KBot.App/HarnessTests/PdfSigningHarnessForm.vb` | Bancul listează semnatarul și data fiecărui câmp. |
| `tests/KBot.App.Tests/*` (9 dubluri `IApiClient`) | Parametrul `semnaturi`. |

FileVersion: neschimbate față de 0078 — aceleași proiecte, încă necomise (Api 1.0.8, Xfa 1.3, App 1.0.38.0).

## 4. Rezultate

- `dotnet build` pe `KBot.Xfa`, `KBot.Api`, `KBot.App`: **0 erori, 0 avertismente**.
- Python: `ast.parse` curat pe `pdf.py`.
- **Nimic rulat pe server sau pe ecran; nicio suită rulată.**

## 5. De făcut de operator

1. `sql/0079_fx_pdf_semnaturi.sql` pe `AVACONT_SURSA` și pe fiecare bază de unitate.
2. `PYTHON/routes/forexe/pdf.py` pe VPS, repornire gunicorn.
3. Pe banc: semnați un câmp nou și încărcați. Bancul arată semnatarul și data fiecărui câmp.
   Apoi `SELECT * FROM FX_PDF_SEMNATURI ORDER BY IdSemnatura DESC` — un rând pentru câmpul nou,
   niciunul pentru câmpurile semnate dinainte.

## 6. Neverificat

- `SignDate` / `TimeStampDate` din iTextSharp pe semnăturile reale AD.CREDIT (fus orar, marcă de timp).
- `IpPublic` depinde de ProxyFix pe VPS (același mecanism ca `FX_LoginLog`).
- Semnăturile făcute înainte de K-BOT (Access) nu au rând — nu se poate ști cine le-a încărcat.
