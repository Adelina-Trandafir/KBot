# ADE10-03 — ADE.Migrator pe subunități

Implementează secțiunea 8 (migrator) din [planul subunităților](../../ADECHIT/plan_subunitati.md).

## Ce s-a schimbat și de ce

- Câmp nou **Subunitate** lângă DC destinație (propus din `Unitati.Denumire`, modificabil). Un nume
  nou creează subunitatea; numele unei subunități existente o alege (trebuie să fie activă și goală).
  Identitatea nu vine din calea MDB-ului.
- **Testează** verifică: tabelele și coloanele noi din AD_05, subunitatea (activă, fără rânduri, fără
  import, fără serie), faptul că hash-ul MDB nu a mai fost importat în DC, InnoDB. Înlocuiește verificarea
  „tabele goale pe tot DC-ul" cu cea pe subunitate: se poate importa B după A.
- **Migrează** (o singură tranzacție): creează sau ia subunitatea și îi blochează rândul din `AD_Lock`
  (același mecanism ca aplicația web), scrie fiecare tabel **fără cheile Access** (le alocă serverul) și
  rescrie toate referințele prin hartă — inclusiv rândurile construite de migrator (educatori, istoric
  copii). Lunile se scriu în ordinea IDL din Access; `LunaD.Ordine` = IDL Access. Harta se salvează în
  `AD_IdMap` (inserturi în loturi de 400). Seria și contorul intră în `AD_ReceiptConfig` al subunității;
  `Unitati_Chitante` nu mai este atinsă. `AD_Imports` primește subunitatea. Jurnalul SQL și confirmarea
  arată DC + subunitate.
- Rollback / COMMIT necunoscut se tratează ca înainte.

## Fișiere

`ADECHIT/ADE.Migrator/`: `AdeWriter.vb` (rescris), `AdeSchema.vb` (+Ordine, +BuiltRelations),
`AdePlan.vb` (+Ordine), `AdeSource.vb` (+UnitName), `AdeMigratorForm.vb`,
`AdeMigratorForm.Designer.vb`, `README.md`.

## Teste

`dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj`: 0 erori, 0 avertismente. Nicio migrare rulată.

## Rămâne neverificat / amânat

Migrarea efectivă a `baza_40.mdb` și `baza_47.mdb` (scenariile 1 și 9 și completările din secțiunea 12 a
planului); completarea sau înlocuirea unei subunități populate rămâne în afara primei versiuni.

## Corectură după review (ADE10-06)

Sub blocarea subunității, `EnsureSubunit` reverifică acum **toată** destinația (toate tabelele planului, `AD_Imports`, `AD_ReceiptConfig`), în aceeași
tranzacție, nu doar `AD_Imports`. Rânduri adăugate de aplicație între Testează și Migrează opresc migrarea cu mesaj. Build: 0 erori, 0 avertismente; nerulat.
