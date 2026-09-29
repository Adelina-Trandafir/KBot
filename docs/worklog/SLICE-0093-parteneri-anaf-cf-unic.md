# SLICE-0093 — Parteneri: filtrare, ANAF, banca din IBAN, cod fiscal unic

Cererea operatorului, 29.09.2026. Fereastra «Adăugare / editare parteneri» (felia 0087-02).

## Ce s-a schimbat și de ce

Cele zece puncte ale cererii:

1-5. **Lista** (`GET /api/forexe/nomenclatoare/parteneri`) întoarce doar partenerii cu
   `TRIM(Tip) = '1'`, `CodFiscal` completat, `COALESCE(Ascuns,0) = 0` și cod fiscal diferit de
   `AVACONT_COMUN.Unitati.CF` pentru `DC = db_name` al sesiunii. Partenerii cu același cod fiscal
   apar o singură dată: primul găsit, în ordinea `IdPartener` (cu codurile lui de angajament).
   Codurile fiscale se compară doar pe cifre (`anaf.normalize_cf`: «RO 123» = «123»), de aceea
   filtrul pe CF-ul unității și gruparea se fac în Python, nu în SQL.
6. **Tip** scos din fereastră (eticheta + combo + rândul din tabel); serverul scrie mereu `'1'`
   la salvare; lista de tipuri a dispărut din răspuns și din `ParteneriCatalog`.
7. **«Alte detalii»** — era chiar coloana `Parteneri.Adresa` (altă coloană nu există). Hotărârea
   operatorului: câmpul rămâne, redenumit **«Adresa»**.
8. **Banca** goală se ia din IBAN (caracterele 5-8 = codul `AVACONT_COMUN.BIC.Cod`): pe server în
   GET (afișare) și în POST (scriere), iar în fereastră la tastarea IBAN-ului — cât timp «Banca»
   e goală sau conține tot banca dedusă anterior.
9. **ANAF**: rută nouă `GET /api/forexe/nomenclatoare/parteneri/anaf/<cod_fiscal>`, care refolosește
   `routes/inregistrare/anaf.py` (v9, aceeași cerere ca `InformatiiFirmaOnline2`; `found` gol =
   necunoscut). În fereastră, la ieșirea din «Cod fiscal» sau Enter, dacă codul s-a schimbat,
   se completează denumirea și adresa (partener nou sau existent). Erorile ANAF (400 cod
   invalid / 404 negăsit / 502 ANAF căzut) apar ca mesaj.
10. **Un singur partener pe cod fiscal**: verificat în fereastră (pe lista încărcată) și pe server
    (pe TOȚI partenerii, inclusiv ascunși / alt Tip), la partener nou sau când codul fiscal se
    schimbă. Fără constrângere în MariaDB (datele din Access au dubluri); un partener vechi cu cod
    dublat rămâne editabil cât timp nu i se schimbă codul. Tot la salvare: codul fiscal devine
    obligatoriu și nu poate fi al unității (altfel partenerul ar dispărea din listă).

Filtrul «Arată partenerii ascunși» a fost scos: serverul nu mai trimite ascunșii.

## Fișiere atinse

- `PYTHON/routes/forexe/parteneri_edit.py` — filtre + grupare în GET, `bic` + `cf_unitate` în
  răspuns, `_comun_lookups`, `_bank_from_iban`, `_check_fiscal_code`, Tip = '1', ruta ANAF.
- `src/KBot.Domain/Nomenclatoare.vb` — `ParteneriCatalog`: `Tipuri` scos, `Bic` + `CfUnitate`
  adăugate; clasă nouă `PartenerAnaf`.
- `src/KBot.Api/INomenclatoareApi.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb` —
  `GetPartenerAnafAsync`; citirea `bic` / `cf_unitate`.
- `src/KBot.App/Views/Nomenclatoare/ParteneriForm.Designer.vb` — `lblTip`/`cmbTip`/`chkAscunsi`
  scoase, rândurile renumerotate (9), «Cod fiscal *», «Adresa», tooltip-uri.
- `src/KBot.App/Views/Nomenclatoare/ParteneriForm.vb` — căutarea ANAF, banca din IBAN,
  validarea codului fiscal la salvare.
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0090-0099.md`.

## Rezultate teste

La cererea operatorului: **niciun test rulat, niciun test scris**. `dotnet build
src\KBot.App\KBot.App.vbproj`: 0 avertismente, 0 erori. `py_compile` pe `parteneri_edit.py`: OK.
Fără git (operatorul face commit-ul).

## Neverificat / amânat

- Nimic rulat pe server sau pe ecran (ruta ANAF, filtrele, fereastra modificată).
- Partenerii ascunși / cu alt Tip / fără cod fiscal nu mai pot fi văzuți din K-BOT; un partener
  bifat «Ascuns» dispare la salvare.
- Un cod dublat al unui partener invizibil în fereastră se află abia la salvare (mesajul
  serverului îl numește și spune dacă e ascuns).
- ANAF întoarce diacritice cu sedilă; se scriu așa cum vin (ca la înregistrare, felia 0075).
