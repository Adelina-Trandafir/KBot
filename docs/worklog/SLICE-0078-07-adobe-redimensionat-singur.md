# SLICE 0078-07 — Fereastra Adobe găzduită care se mărește singură

Data: 29.09.2026. Pe PC-ul unui client (KBot 1.1.0.4, `adobe_preview.log` + captura arborelui de
ferestre, lipite de operator): K-BOT a pus fereastra Adobe la mărimea panoului (15:25:44.829
«MUTAT 0,0 1920x1020 -> 0,0 654x586», a doua trecere «NESCHIMBAT»), a trimis Ctrl+H (15:25:45.455),
iar în captură: panoul `0x000D07B4` = **807×772**, fereastra `AcrobatSDIWindow` `0x00050846` =
**1919×1079** — tăiată, fără partea dreaptă și de jos. Număr: **0078-07** (propus).

**Verificat din captură:** panoul a crescut după așezare (654×586 ▸ 807×772; controalele ascunse
«Se încarcă documentul…» au rămas la mărimea veche). **Nu se știe** dacă fereastra Adobe a ajuns
la ecran întreg înainte sau după ce K-BOT a readus-o la noua mărime a panoului: redimensionarea la
schimbarea panoului nu scria nimic în jurnal. **Ipoteză:** fereastra venea MAXIMIZATĂ din Adobe-ul
operatorului (predată unei instanțe existente, «NU a fost creată de K-BOT»), iar K-BOT nu scoate
marcajul «maximizată» — Adobe se reface la ecran întreg la o reașezare proprie (probabil Ctrl+H).

## 1. Ce s-a schimbat

- **`AdobeSizeWatcher` (NOU, KBot.Controls/Adobe):** cârlig WinEvent `EVENT_OBJECT_LOCATIONCHANGE`
  pe procesul Adobe (în afara procesului — nimic injectat în Adobe), filtrat la fereastra găzduită;
  o rafală de evenimente = un singur `Changed`, la 100 ms după ultimul.
- **`AdobeReaderHost`:** după a doua trecere pornește urmărirea. Când Adobe își schimbă singur
  fereastra și ea nu mai e cât panoul: linia «Adobe și-a schimbat singur fereastra: <dreptunghi>
  (panoul: …; marcată «maximizată»: da/nu)», apoi o readuce (linia «Poziție (readusă la panou)»).
  Dacă Adobe o schimbă de peste 5 ori în 3 s, K-BOT se oprește (mesaj în jurnal) până la
  următoarea redimensionare a panoului. Oprit la `Detach`, la eliberarea pe ecran întreg, la `Dispose`.
- **Redimensionarea panoului** (`Relayout`) scrie acum în jurnal când fereastra NU a ajuns la
  mărimea cerută («Poziție (panoul s-a redimensionat)»); când a ajuns, tace (fără zgomot la tragerea
  separatorului).
- Stilul ferestrei Adobe NU se schimbă (marcajul «maximizată» doar se citește, pentru jurnal):
  regula 0078-05 — nimic din ce ar ține minte Adobe-ul operatorului.
- **Ctrl+2 după Ctrl+H** (cererea operatorului, 29.09.2026): `ArmReadMode` pune în coadă Ctrl+H și
  apoi Ctrl+2 — aceleași condiții, același focus, trimise în același lot, în ordinea asta; în jurnal
  «Ctrl+2 trimis documentului.». În Acrobat, Ctrl+2 = potrivire la lățime (din cunoștințe generale,
  neverificat pe versiunea clientului). Dacă nu poate fi trimis: «Apăsați Ctrl+2 în document…».
- (din 0078-06 §5) `ReaderHostPreview.pnlHost_SizeChanged`: `_host?.Relayout()`.

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/Adobe/AdobeSizeWatcher.vb` | **NOU** |
| `src/KBot.Controls/Adobe/AdobeReaderHost.vb` | urmărirea, `OnAdobeResized`, `Fill(…, onlyWhenWrong)`, `Relayout` cu jurnal |
| `src/KBot.Controls/Adobe/AdobeNativeMethods.vb` | `WS_MAXIMIZE` |
| `src/KBot.App/Views/Ddf/ReaderHostPreview.vb` | `_host?.Relayout()` |

FileVersion: Controls 1.55 (necomis, posibil deja în 1.1.0.4) ▸ **1.56**. App: 1.1.0.4 e livrat ▸
de crescut la publicare (`push-update.ps1` întreabă).

## 3. Rezultatele testelor

`dotnet build src\KBot.App\KBot.App.vbproj` (cu Controls): **0 erori, 0 avertismente**. Nimic rulat.

## 4. Rămas NEVERIFICAT

1. Nerulat. Dacă ipoteza e bună, jurnalul următor de pe acel PC arată, după «Ctrl+H trimis», linia
   «Adobe și-a schimbat singur fereastra: 0,0 1919x1079 … «maximizată»: da» și readucerea.
2. Dacă Adobe se luptă (linia «de N ori în 3 s»), readucerea nu ajunge: rămâne varianta scoaterii
   marcajului «maximizată» la încorporare, cu riscul ca Adobe-ul operatorului să țină minte fereastra
   mică (de hotărât de operator).
3. Ctrl+2 se trimite o singură dată, la deschidere: după o readucere la panou (sau o redimensionare
   a panoului) Adobe poate păstra zoom-ul vechi, nepotrivit la noua lățime.
4. Readucerea poate retrimite Adobe-ului o reașezare care anulează modul citire (Ctrl+H) — de văzut.

> 30.09.2026: `AdobeSizeWatcher` a fost redenumit `AdobeWindowWatcher` și lărgit în 0078-08 (vezi `SLICE-0078-08-arbore-blocat-cat-se-deschide-pdf.md`).
