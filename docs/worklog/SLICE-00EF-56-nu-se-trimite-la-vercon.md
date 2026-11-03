# SLICE-00EF-56 — «NU SE TRIMITE»: linii ale facturii primite care nu ajung la Vercon (03.11.2026)

Cererea operatorului: la «Trimite la Vercon», pe anumite rânduri ale facturii să se poată alege «NU SE TRIMITE»; utilizatorul Vercon online să **nu vadă** produsele respective, nici în NIR, nici altundeva; ele rămân normal în e-Factura. Ajutor: 0000-149.

## What changed and why
- **Alegere:** opțiunea **«NU SE TRIMITE»** e în aceeași listă «Cont Vercon» (lângă «(fără cont)»), deci merge pe un rând, pe antetul unei facturi și din lista «Cont pentru toate liniile». Rezumatul de sub tabel are grupul «Nu se trimit». Când o factură trimisă așteaptă încă Administratorul, marcajul se schimbă din «Salvează conturile».
- **Date:** coloană nouă `EF_PrimiteVerconLinii.NuSeTrimite` (`sql/EV_04_primite_vercon_nu_se_trimite.sql`, `ADD COLUMN IF NOT EXISTS`, implicit 0). Rândul trimiterii rămâne (K-BOT arată linia, alegerea se poate schimba); `EF_PrimiteLinii` nu e atins, deci linia rămâne în e-Factura.
- **Server K-BOT** (`primite_vercon.py`, `primite_vercon_routes.py`): `conturi` primește `{IdLinie, IdMasina, NuSeTrimite?}`; o linie marcată nu are cont; citirea întoarce `NuSeTrimite` pe linie; cel puțin o linie trebuie trimisă (409 `NICIO_LINIE_TRIMISA`).
- **Server VERCON** — liniile marcate nu se citesc nicăieri: `efactura_queue.py` (numărul de linii din listă + liniile din «Adaugare NIR»), `efactura_association.py` (asocierea cu NIR: citire și salvare), `utils/vercon_efactura.py` (`sent_line_sql`, folosit și la bugetul de cantități din `progress` / `progress_states`, ca factura să poată ajunge PRELUATA fără liniile excluse).
- **Verificarea paginii web Vercon (JS), a doua trecere:** JS-ul nu citește liniile direct din `EF_*`, ci prin rutele de mai sus (`efactura-intake.js`, `efactura-association*.js`), deci liniile excluse nu ajung în el. Un loc le dădea totuși de gol și a fost reparat în Python (JS neatins); un al doilea, PDF-ul, rămâne cum era, hotărât de operator:
  - **Totalul facturii**: lista «Facturi din e-Factura» și antetul NIR-ului (`header.Valoare = detail.total`) arătau totalul întreg. Acum `efactura_queue.sent_total` scade valoarea cu TVA a liniilor excluse (regula `primite_ke.valori_cu_tva`, cu rotunjirea ei), în listă și în detaliu.
  - **PDF-ul facturii originale** (`efactura_receipts.invoice_pdf`, «Afișează eFactura» dintr-un NIR) e factura întreagă, cu liniile excluse. **Rămâne neschimbat (decizia operatorului, 03.11.2026):** utilizatorul Vercon îl poate deschide ca înainte și vede și liniile excluse. Un prim pas care îl bloca (403) a fost scos.
- **Client:** `KBot.Domain/EFacturaVercon.vb` (`NuSeTrimite` pe `EFacturaVerconLinie` și `EFacturaVerconContLinie`), `KBot.Api/ApiClient.EFactura.Vercon.vb`, `KBot.EFactura/Forms/VerconForm.vb`. FileVersion: Api 1.0.30.0, Domain 1.2.21.0, EFactura 1.14.0.0.
- **Ajutor:** `contabil/efactura/index.md` («Trimiterea la Vercon») și tutorialul `efactura-trimite-la-vercon.md` (pasul «Contul din Vercon»); etichete `00EF-56, 0000-149`; `help-version.txt`.

## Files touched
`sql/EV_04_primite_vercon_nu_se_trimite.sql` (nou), `PYTHON/routes/efactura/primite_vercon.py`, `primite_vercon_routes.py`, `PYTHON/routes/vercon/efactura_queue.py`, `efactura_association.py`, `PYTHON/utils/vercon_efactura.py`, `src/KBot.Domain/EFacturaVercon.vb`, `src/KBot.Api/ApiClient.EFactura.Vercon.vb`, `src/KBot.EFactura/Forms/VerconForm.vb`, trei `.vbproj`, două fișiere de ajutor, `help-version.txt`.

## Test results
`py_compile` pe cele 5 fișiere Python: ok. `dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Niciun test scris sau rulat (regula operatorului), nimic văzut pe ecran, nimic rulat pe server, `Check-Help.ps1` nerulat.

## Unverified / deferred
- **Ordinea pe server:** întâi SQL-ul (`EV_04`, pe `AVACONT_SURSA`, apoi sincronizarea schemei), abia apoi Python-ul. Cu Python nou și fără coloană, lista «Adaugare NIR» și asocierea dau eroare SQL. Nimic nu a fost pus pe server.
- **Totalul din Vercon** = totalul e-Facturii minus liniile excluse (cu TVA). Probă doar pe un cursor de test cu o factură de 178,50 (100 + 50 la 19%, a doua exclusă): 119,00. Nerulat pe date reale. O factură cu reduceri la nivel de document are o diferență care rămâne la ce rămâne trimis.
- **PDF-ul original rămâne deschis din Vercon, cu toate liniile** (decizia operatorului): liniile excluse se pot vedea acolo.
- Pe o trimitere veche (înainte de coloană) toate liniile merg, ca până acum. O trimitere cu asocieri parțiale în NIR nu mai poate schimba conturile (regula existentă), deci nici marcajul.
- Nu e nimic de «gri» pe rândurile excluse din tabel (doar textul din celulă și rezumatul); aspectul nu a fost văzut pe ecran. «NU SE TRIMITE» ca element al listei grupate (fără gestiune) nu a fost văzut.
- Pagina Vercon web nu a fost deschisă în browser: verificarea JS s-a făcut citind codul (`efactura-*.js`). PDF-ul nu a fost atins.
