# SLICE-0078-17 — Încercare: semnarea în afara Adobe, cu iText 9 (03.11.2026) — oprită, fără rezultat <a id="slice-0078-17"></a>

Data: 03.11.2026 · Continuă [SLICE-0078-01](SLICE-0078-01-semnare-pdf.md#slice-0078-01) (semnarea DDF / ORD în Adobe). Număr: **0078-17**, confirmat de operator (0078-16 era deja folosit de jurnalul ActiveX pe culoare).

## 1. Ce s-a încercat și de ce

Întrebarea operatorului: pot WebView2 + iText 9 (mod append, certificatul de pe token din depozitul Windows) să înlocuiască Adobe la afișarea și semnarea unui formular XFA (DDF / Document de fundamentare)? Încercarea a fost un proiect separat, `tools\XfaSignTrial` (WinForms, iText 9.5.0, `itext.bouncy-castle-adapter`, WebView2), în afara `KBot.sln`, fără nicio referință la K-BOT. **Nimic din proiectul principal nu s-a schimbat**; folderul a fost șters la cererea operatorului (oprită: prea mult timp față de șansele de reușită). Fluxul în șapte pași 0124 și semnarea prin Adobe rămân neatinse.

## 2. Ce s-a aflat

**Verificat** (văzut de operator în Adobe sau în octeții fișierelor):
- **Tokenul și iText 9 funcționează pe un PDF obișnuit:** același certificat Podeanu (PIN prin CSP-ul tokenului) a semnat un PDF simplu; Adobe l-a arătat valid, semnătură electronică calificată (eIDAS), «document nemodificat». Deci certificatul, lanțul, PIN-ul și containerul CMS nu sunt problema.
- **Modul append păstrează totul:** rezultatul începe cu octeții originalului, `/XFA` rămâne, integritatea semnăturii e bună în citirea iText.
- **Un formular XFA dinamic nesemnat NU are câmpuri de semnătură în stratul PDF:** `/Fields[]` e gol. Câmpurile există doar în șablonul XFA (15: A `SignatureField1`, `11`–`16`; B `2`, `21`–`26`; Ordonator `3`), cu nume complete de forma `form1[0].SubformSemnaturaA[0].SignatureField1[0]`. Adobe adaugă câmpul în stratul PDF abia când îl semnează, iar arborele final conține DOAR câmpurile semnate.
- **Cum scrie Adobe câmpul:** lanț de câmpuri-părinte (`form1[0]` → `SubformSemnatura…[0]`) și câmpul cu `/FT/Sig`, widget cu `Rect[0 0 0 0]`, `/TM` = numele complet XFA. Un astfel de câmp creat de noi (aceeași structură) + semnătură iText în el **este recunoscut de Adobe**: apare sub numele corect, iar celelalte 14 apar ca «Unsigned Signature Fields».
- **Semnăturile Adobe din DDF** au `adbe.pkcs7.detached`, nume de creator, `/Reference` de tip FieldMDP (`/Action /Include /Fields [...]`, derivat din manifestul `semnatura1`, ~70 de câmpuri) și `DigestValue`. Ale noastre n-aveau `/Reference`.
- **Succesiunea semnărilor stă în scripturile formularului**, nu în PDF: `initialize` / `postSign` citesc `signatureInfo().status` și schimbă `access` (open / readOnly); starea se salvează în pachetul `<form>`. iText nu rulează aceste scripturi (șablonul exportat e în `Surse\doc_fund_xdp_full.xml`).

**Nerezolvat (capcana):** pe un formular XFA nesemnat, Adobe nu reușește să valideze semnătura noastră și arată «An error occurred while attempting to validate this signature» / «Signed by Unknown» / detaliu «Adobe Acrobat error. Expected a dict object.». Apare identic cu `ETSI.CAdES.detached` și cu `adbe.pkcs7.detached`, cu nume de creator completat și fără editarea structurii etichetate. **Cauza nu s-a aflat.**

**Capcane de reținut:**
- La un PDF etichetat (tagged), `PdfPage.AddAnnotation(widget)` din iText modifică și arborele de structură (`/StructParent`); câmpurile Adobe nu au asta. Variantă: `AddAnnotation(-1, widget, False)`.
- iText 9 cere pachetul separat `itext.bouncy-castle-adapter`; fără el orice apel de certificat dă `NotSupportedException`.
- În VB, `Math.Abs` e umbrit de spațiul de nume `iText.Commons.Bouncycastle.Math`: se scrie `System.Math.Abs`.
- Fereastra Adobe «Font Capture: Windows - Application Error 0xc06d007e» apare și pe PDF-uri simple semnate: ține de instalarea Adobe, nu de semnare.
- **Licență:** iText 9.5 este AGPL (ca și iTextSharp 5.5 folosit deja de K-BOT); nu intră în K-BOT fără licență comercială.

## 3. Fișiere atinse

Niciunul în proiectul principal (proiectul `tools\XfaSignTrial` a fost creat și șters). Documentație: acest jurnal, rândul din `state/KBOT_STATUS_0070-0079.md`, linia din `KBOT_STATUS.md`.

## 4. Rezultatele testelor

Niciun test scris sau rulat. Încercarea a fost rulată de mână de operator; rezultatele de mai sus sunt din ecranele și jurnalul lui. Nimic de construit în proiectul principal.

## 5. Rămas NEVERIFICAT / amânat

- Ce arată panoul WebView2 pentru XFA (nu a fost raportat; așteptat gol/«Please wait», fiindcă vizualizatorul Chromium nu rulează XFA).
- Semnarea cu iText într-un document salvat de Adobe (`DDF_NR_7_REV_0`, `SignatureField22`).
- Câmpul simplu iText pe formularul nesemnat.
- Adăugarea `/Reference` FieldMDP.
- Dacă `postSign` ar fi deblocat câmpul următor (din proiectare, nu: iText nu rulează scripturile).
