# SLICE-ME01-22 - Arborele principal din KBotAdmin: fara bife, cu cautare mereu vizibila (06.11.2026)

Cererea operatorului (KBotAdmin): (1) bifele dispar din arborele principal; (2) cautarea din `AdvancedTreeControl` este vizibila tot timpul, iar cautarea merge dupa cod, dupa denumirea unitatii si dupa conturile asociate. Owner's tool only (KBotAdmin, never published).

## What changed
- **Fara bife** (`MainForm.Designer.vb`, `MainForm.vb`): `treeDb.CheckBoxes` scos, `HasCheckBox` scos de pe radacina, baze, conturi si utilizatori; caseta «Selectează tot» (`chkAll`) scoasa cu tot cu randul ei din `tlyLeft` (3 randuri -> 2). Dispar `CheckedLeaves`, `PublishChecks`, `TreeDb_NodeChecked`, `ChkAll_CheckedChanged`, `_syncingAll`. `AdminContext.CheckedDatabases` / `CheckedAccounts` scoase (un singur consumator: `OnceQueryView`).
- **Cautare mereu vizibila** (`treeDb.SearchShow = True`, fara iconita de toggle in antet, deci banda e permanenta; text de ajutor in caseta «… cod, denumire sau cont (minim 3 caractere) …»). Cautarea controlului cauta in legenda randului: codul si denumirea bazei sunt in legenda randului de baza, contul in legenda frunzei (baza apare ca parinte al contului gasit). In modul «Utilizatori» frunza are codul bazei in legenda si denumirea ei in `SearchExtra`. Dupa fiecare reconstruire a arborelui (`BuildDbTree`) se cheama `treeDb.RerunSearch()`, ca un text deja tastat sa filtreze si randurile noi.
- **«Interogari unice»** (`OnceQueryView`): fara bife, bazele de rulat vin din arbore: baza aleasa -> doar ea; «TOATE BAZELE» -> toate bazele serverului; un utilizator ales (fara baza) -> nimic, cu mesaj «Alegeți în arborele din stânga o bază de date sau «TOATE BAZELE».». Confirmarea existenta arata numarul de unitati. Texte si tooltip-uri actualizate (inclusiv tooltip-ul butonului ribbon «Unice»).

## Files touched
`KBotAdmin/MainForm.Designer.vb`, `KBotAdmin/MainForm.vb`, `KBotAdmin/Core/AdminContext.vb`, `KBotAdmin/Views/OnceQueryView.vb`, `KBotAdmin/Views/OnceQueryView.Designer.vb`, `KBotAdmin/KBotAdmin.vbproj` (FileVersion 1.2.0.14); status.

## Test results
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 avertismente, 0 erori. Nicio rulare, niciun test.

## Unverified / deferred
- **Nimic vazut pe ecran**: banda de cautare (inaltime, tema, text de ajutor), arborele fara bife, raportul dintre `tlyLeft` si arbore dupa scoaterea casetei.
- **Cautarea controlului cere minim 3 caractere** (valoare fixa in control): un cod scurt de baza (sub 3 caractere) nu se gaseste. Nemodificat, controlul e comun.
- **Comportament schimbat**: «Interogari unice» nu mai poate rula pe un set ales de baze; doar pe una sau pe toate. Daca e nevoie de un set, trebuie o alta forma de alegere (de ex. lista proprie a paginii).
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).
