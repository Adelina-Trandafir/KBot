# SLICE-ADE0-09 — verificare selectivă a MDB-urilor pentru subunități

Data: 09.10.2026. Stare: **ANALIZAT LOCAL / DOCUMENTAT LOCAL**.

Utilizatorul a indicat `ADECHIT/ACCESS_SOURCES` pentru verificarea bazelor distincte,
cu instrucțiunea explicită de a nu le citi integral.

## Operații și rezultate

Inventariate `baza_40.mdb` și `baza_47.mdb`. Citire ACE OLE DB cu `Mode=Read`,
numărători, metadate și interogări TOP limitate, fără exportul tuturor rândurilor.
Nu s-a pornit MSACCESS. Au fost comparate 151 de coloane din 13 tabele relevante;
nu au fost găsite diferențe de nume/tip/lungime/nullable între aceste două surse.

Serii proprii: GR40 și G.R.; valori NUMAR: 6715 și 10199. DC-uri sursă diferite:
051_GR40 și 047_GR47. Destinația părinte rămâne de confirmat. Aceleași luni
calendaristice, ID-uri de grupe/taxe comune și intervale de plăți suprapuse.

Constatări relevante: baza_40 nu are chitanțe, dar are contor configurat;
baza_47 păstrează SS_Buget pentru 2020–2026, dar numai două luni în LunaD.
Referințe la luni lipsă: 295 în Retur din baza_40 și 35.834 în SS_Buget din baza_47.
Chitante și AlteDoc nu au IDL în aceste surse. Detaliile și scenariile suplimentare
sunt în secțiunea 12 din `ADECHIT/plan_subunitati.md`.

## Diagnostice și limite

Prima citire de metadate a returnat rezultate pentru ambele surse, dar procesul
shell a terminat cu cod de eroare nativ. Interogările ulterioare au fost executate
cu Windows PowerShell și au terminat cu succes. Prima verificare a referințelor
a întâlnit lipsa coloanei IDL; interogările au fost apoi limitate la tabelele unde
coloana există. Nu se declară reușite comenzile eșuate.

Nu s-au citit în masă date personale, nu s-au modificat MDB-urile, codul sau SQL-ul.
Nu s-au rulat teste ale aplicației, migrări ori operații pe server. Agregările
nu validează relațiile în întregime sau paritatea soldurilor. Fișierul vechi
baza2020_PP.mdb nu este prezent în inventarul curent; referințele istorice nu
sunt folosite ca dovadă a schemei actualelor MDB-uri.

Actualizate planul, indexul și registrul ADE0. Următoarea subfelie de analiză: ADE0-10.
