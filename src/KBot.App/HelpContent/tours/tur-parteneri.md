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
Partenerii unității care au cod fiscal, câte unul pe cod fiscal. Bifa «Ascunde partenerii fără activitate» lasă doar partenerii care apar pe un document (DDF sau ORD).

## Listă › Lupa
<!-- slice: 0087, 0000-23 -->
target: ParteneriForm.tree
part: header.search
Caută după o parte din cod sau din denumire. Esc golește căutarea și închide banda.

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
Clasificația, contul bancar asociat, codul angajamentului și codul indicatorului. ✕ de pe un rând îl scoate.

## Coduri › Adaugă un cod
<!-- slice: 0087, 0000-23 -->
target: ParteneriForm.gridCoduri
part: footer.right
Semnul + adaugă un rând nou, pe care îl completezi direct în tabel.

## Butoanele ferestrei
<!-- slice: 0087, 0000-20 -->
target: ParteneriForm.tlySubsol
Adăugare, Salvare, Ștergere, Renunță și Ieșire.
