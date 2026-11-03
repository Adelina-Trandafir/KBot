# SLICE-ME01-15 - «Comenzi»: lista de comenzi grupata pe aplicatii (03.11.2026)

Cererea operatorului: in KBotAdmin, pagina «Comenzi», comboul cu comenzi devine un combo cu arbore, comenzile separate pe aplicatii (VERCON, FOREXE, AVACONT, ADECHIT) si un root `_FARA APLICATIE`; la deschidere root-urile sunt collapsed. Ultimul sub-numar folosit era ME01-14, deci ME01-15.

## What it is
- **Nu exista un control nou de arbore.** `KBotComboBox` stie deja grupuri care se inchid (slice 00EF-46: `IKBotComboGroupedItem` / `KBotComboGroupedItem`, un rand de antet cu triunghi pe grup). Comanda se alege tot din `cmbCommand`, iar `SelectedIndex` si `_commands` raman aliniate cu `Items`.
- **`KBotComboBox.GroupsStartCollapsed`** (nou, `Combo/KBotComboBox.DropDown.vb`, implicit False, `DefaultValue(False)`): lista INTREAGA (sageata, F4) se deschide cu toate grupurile inchise si fara rand evidentiat (un rand dintr-un grup inchis nu se vede). O cautare tastata isi arata rezultatele deschise: pliurile ramase de la o deschidere anterioara se uita. `KBotComboList.ShowRows` primeste `k_foldAll` (optional, implicit False: comportamentul vechi, grupul randului evidentiat se deschide).
- **`CommandsView`**: itemii sunt `KBotComboGroupedItem(titlu [+ «  (scrie)»], aplicatie, "", CommandInfo)`. Ordinea radacinilor: VERCON, FOREXE, AVACONT, ADECHIT, `_FARA APLICATIE`; in grup, comenzile dupa titlu (ro-RO). Un grup fara comenzi nu apare (acum AVACONT si ADECHIT nu au nicio comanda).
- **Tabelul comanda → aplicatie** e in client (`CommandApps`), nu pe server: VERCON = `profil_produs`, `oi_semne`, `oi_semne_repara`, `storno_preturi`, `storno_preturi_repara`; FOREXE = `fisa_angajament`, `ddf_antete`, `receptii_lanturi`; restul (`ping`, `tables`, `counts`, `processlist`, `journal`, `activity`, `charset`, `analyze`) sunt `_FARA APLICATIE`, ca orice comanda noua a serverului pe care tabelul nu o cunoaste.
- Tooltip-ul comboului (Designer) spune acum ca grupurile se deschid / se inchid la clic.
- KBot.Controls 1.83.0.0, KBotAdmin 1.2.0.4. `KBotComboBox.md` actualizat. Ajutor: niciunul (KBotAdmin nu e pentru clienti; nicio fereastra K-BOT nu foloseste optiunea).

## Files touched
`KBot.Controls/Combo/KBotComboBox.DropDown.vb`, `KBotComboList.vb`, `KBotComboBox.md`; `KBot.Controls.vbproj`; `KBotAdmin/Views/CommandsView.vb`, `CommandsView.Designer.vb`; `KBotAdmin.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build KBotAdmin/KBotAdmin.vbproj` (cu KBot.Controls, KBot.Theming): 0 avertismente, 0 erori. Nimic rulat, nici un test. Versiunile au fost ridicate dupa build.

## Unverified / deferred
- **Nevazut pe ecran**: ca lista se deschide cu cele grupuri inchise, ca un clic / Enter pe antet il deschide, ca alegerea unei comenzi pune titlul ei pe fata comboului, ca trecerea la alta comanda schimba explicatia si caseta de parametru ca inainte.
- Tabelul comanda → aplicatie e o decizie a clientului, **neconfirmata de operator**; serverul (`PYTHON/routes/admin_cli/admin_cli.py`) nu trimite aplicatia si nu a fost atins.
- Dupa alegerea unei comenzi dintr-un grup, la urmatoarea deschidere grupurile sunt iar inchise (cerinta), deci comanda aleasa nu se vede in lista pana nu se deschide grupul ei.
