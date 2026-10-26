# SLICE-0008-03 — `KBotDataView.HeaderClick`

Cererea operatorului, 12.10.2026. Modificarea controlului `KBotDataView` cerută de fereastra «Parteneri ANAF» din VRC.Migrator (VERCON) este înregistrată aici, în ultima felie care a atins controlul (0008-02), ca sub-felie nouă.

## What changed and why

La click pe titlul unei coloane SS (01A, 02A, ...) din fereastra «Parteneri ANAF» trebuie bifate sau debifate toate casetele coloanei. Grila nu avea niciun eveniment pentru click pe antet.

- `src/KBot.Controls/DataView/KBotDataView.vb`: `Public Event HeaderClick As EventHandler(Of KBotCellEventArgs)`.
- `src/KBot.Controls/DataView/KBotDataView.Input.vb` (`OnMouseUp`): dacă antetul e vizibil și punctul e în banda de antet, se ridică `HeaderClick` cu cheia coloanei și `RowIndex = -1`, apoi se iese (nicio celulă nu se acționează). O tragere de margine (redimensionare) iese mai devreme din `OnMouseUp`, deci nu ridică evenimentul.
- Consumator: `VERCON/VRC.Migrator/VrcPartnersForm.vb` (`dgvFurnizori_HeaderClick`): pe coloanele cu cheia `ss_*` bifează toate rândurile, sau le debifează dacă sunt deja toate bifate.

Nicio grilă existentă din K-BOT nu ascultă evenimentul, deci comportamentul lor nu se schimbă.

## Test results

- `dotnet build VERCON\VRC.Migrator\VRC.Migrator.vbproj` (compilează și `KBot.Controls`): 0 erori, 0 avertismente.
- Nu s-au scris și nu s-au rulat teste (regula operatorului).

## Left unverified or deferred

- Nimic văzut pe ecran: click pe titlu, bifarea coloanei, comportamentul după o redimensionare.
- Un click pe o pictogramă din antet (filtru / pictogramă din dreapta) ridică și `HeaderClick`; nimeni nu îl ascultă încă.
- Ajutorul nu se schimbă: VRC.Migrator nu e în ajutorul K-BOT.
