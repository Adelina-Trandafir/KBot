---
id: tur-parteneri
title: Fereastra «Parteneri»
part: contabil
topic: contabil.nomenclatoare.parteneri
---
## Lista partenerilor
<!-- slice: 0087, 0093, 0000-20 -->
target: ParteneriForm.tree
goto: menu:parteneri
Partenerii unității care au cod fiscal, câte unul pe cod fiscal. Lupa din capul listei caută după o parte din cod sau din denumire; bifa «Ascunde partenerii fără activitate» lasă doar partenerii care apar pe un document (DDF sau ORD).

## Codul fiscal
<!-- slice: 0087, 0000-20 -->
target: ParteneriForm.txtCodFiscal
La Enter sau la ieșirea din câmp, K-BOT întreabă ANAF și completează singur denumirea și adresa. Codul fiscal e obligatoriu, nu poate fi al unității și nu pot exista doi parteneri cu același cod.

## IBAN și banca
<!-- slice: 0087, 0000-20 -->
target: ParteneriForm.txtIban
Banca se completează singură din IBAN, cât timp câmpul ei e gol.

## Coduri angajament
<!-- slice: 0087, 0093, 0000-20 -->
target: ParteneriForm.gridCoduri
Clasificația, contul bancar asociat, codul angajamentului și codul indicatorului. + din subsolul tabelului adaugă un rând, completat direct în tabel; ✕ de pe un rând îl scoate.

## Butoanele ferestrei
<!-- slice: 0087, 0000-20 -->
target: ParteneriForm.tlySubsol
Adăugare, Salvare, Ștergere, Renunță și Ieșire.
