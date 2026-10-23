# ADE10-07 — ADE.Migrator: chitanțe dublate corectate de mână, copii reconstituiți din SS_Buget

Data: 2026-10-21.

## Ce s-a schimbat și de ce

Două blocaje frecvente la migrare nu aveau altă ieșire decât corectarea sursei Access:

1. **`SS_Buget.IDP` arată către un copil care nu mai există în `Platitori`** (ex.: 1154). Acum planul reconstituie copilul
   din situațiile lunare: numele, CNP-ul, grupa și «plecat» din ULTIMA situație (după an, apoi IDL), plus un rând în
   `Platitori_sub` (doar numele copilului, fără adresă sau contact: Access nu le păstrează în `SS_Buget`). Apare o notă
   cu IDP-urile reconstituite. Pentru acest pas nu există opțiune: se aplică întotdeauna. Efect lateral dorit:
   `Prezenta`, `Plati` etc. care arătau către același IDP găsesc acum părintele.
2. **Chitanțe cu aceeași serie și același număr** (cheia `uq_Sub_Serie_Numar`). Butonul nou **«Chitanțe dublate…»** (activ
   când există dubluri) deschide o fereastră cu lista chitanțelor în dublu: numărul se schimbă de mână în coloana
   «Număr nou», iar după fiecare schimbare toată lista se verifică din nou («Încă în dublu cu IDC …», «Schimbat»,
   «Rezolvat»). Butonul **«Taie cifra miilor la cele mai vechi»**: în fiecare grup de numere identice, toate în afară
   de cea mai nouă (după dată, apoi IDC) pierd cifra miilor (1234 → 234; 19387 → 1387), numai dacă numărul nou este liber
   în serie; celelalte rămân și sunt numărate. **«Anulează schimbările»** pune numerele din Access la loc. **«Gata»**
   refă planul cu numerele schimbate (nu și Access-ul), apoi trebuie repetat «Testează». Numerele schimbate sunt
   uitate la o nouă citire a MDB-ului.

**Regula «adaugă .1» NU s-a făcut:** `AD_Chitante.Numar` este `int(11)`, deci «1234.1» nu încape; ar cere o coloană nouă
(de ex. `NumarSub`, ca la NIR în VERCON), cheia unică extinsă și schimbări în aplicația web (afișare, căutare, următorul număr).
Nu am făcut DDL.

## Fișiere

`ADECHIT/ADE.Migrator/AdePlan.vb` (reconstituire, aplicare numere, mesaj), `AdeReceiptNumbers.vb` (nou: analiza și regula),
`AdeReceiptsForm.vb` + `AdeReceiptsForm.Designer.vb` (nou: fereastra), `AdeMigratorForm.vb` + `.Designer.vb` (butonul,
refacerea planului), `README.md`.

## Verificări

`dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: reușit, 0 avertismente, 0 erori. Fără teste, fără rularea
migratorului, fără deschiderea ferestrei (Designer-ul scris de mână, niciodată deschis în Visual Studio), fără MariaDB.

## Nerezolvat / neverificat

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- Fereastra nu a fost văzută pe ecran; aspectul coloanelor, comportamentul editării în `KBotDataView` și butonul nou din
  `tlpActiune` rămân de probat.
- Copiii reconstituiți primesc `SI = 0` și fără `DataIntrare`; `Platitori_sub.Activ` = nu «plecat». De confirmat dacă e bine.
- Dacă o chitanță dublă nu are serie sau număr, nu e listată (cheia unică permite NULL).
- Regula «.1» așteaptă decizia pentru schimbarea schemei.
- Ajutorul pentru utilizator (dacă migratorul are) nu e actualizat.
