# SLICE-ME01-29 - «Comenzi»: Golire date contabile Avacont, ca migrarea sa se poata relua de la zero (09.11.2026)

Cererea operatorului: in KBotAdmin, la «Comenzi», sub AVACONT, o comanda care sterge tot din tabelele `TZ_` si din cele conexe, ca migrarea sa se poata face din nou. Fluxul ei (09.11.2026): goleste cu comanda, apoi migreaza de la inceput cu AVC.Migrator; nu se mai ruleaza migrari consecutive.

## What it is
- **Verb nou `avacont_golire`** (`PYTHON/routes/admin_cli/avacont_wipe.py`, inregistrat in `admin_cli.py`: `COMMANDS`, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`, `_c_avacont_golire`). `writes=True`, pe **o singura baza**, modelat pe `vercon_golire` (ME01-16).
- **Ce goleste**: toate tabelele de baza ale bazei al caror nume incepe cu `TZ_` (note, documente, operatii, debit/credit, plan de conturi, solduri, jurnale, sabloane, subunitati, OP-uri, OPME, plati pe facturi, setari) si `EF_PrimiteOperatii` (legaturile facturilor primite cu operatiile; fara operatii ar arata spre nimic; se pun la loc de pasul «E-Factura» al AVC.Migrator).
- **Ce NU atinge**: `Parteneri`, `Clasificatii`, `FX_`, `AD_`, `VC_` si restul tabelelor `EF_` (facturile, liniile, mesajele). Verificat in dump-ul schemei: nicio tabela din afara `TZ_` nu are cheie straina spre `TZ_`.
- **Refuza** `AVACONT_SURSA` si `AVACONT_COMUN` (tin doar structura) si o baza fara tabele `TZ_`.
- **Confirmare**: valoarea tastata («Numele bazei (confirmare)») trebuie sa fie numele bazei alese, verificata in client si pe server; in client o fereastra spune ca nu se poate anula.
- **Cum**: `FOREIGN_KEY_CHECKS=0` pe sesiune, `TRUNCATE TABLE` pe fiecare tabela (DDL, se comite singur, reseteaza `AUTO_INCREMENT`), apoi `FOREIGN_KEY_CHECKS=1`. Rezultat: un bloc de sumar si un bloc pe tabele (randuri inainte / dupa). Intrare in `Jurnal` (`avacont_golire`).
- **Client** (`KBotAdmin/Views/CommandsView.vb`): comanda e in grupa AVACONT (`CommandApps`), cu explicatie proprie in `Expectations` si cu aceeasi confirmare ca la golirea VERCON; `KBotAdmin` 1.2.0.17.

## Files touched
`PYTHON/routes/admin_cli/avacont_wipe.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py`; `KBotAdmin/Views/CommandsView.vb`, `KBotAdmin/KBotAdmin.vbproj`; `src/AVC.Migrator/README.md`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build KBotAdmin/KBotAdmin.vbproj`: 0 erori. Python: `py_compile` pe cele doua fisiere. Comanda nu a fost rulata pe nicio baza.

## Unverified / deferred
- Nerulata; `TRUNCATE` nu se poate anula, copia bazei se face inainte.
- Goleste si `TZ_Subunitati` si `TZ_Settings`: subunitatile se recreeaza din pasul «Subunitati» al migratorului, setarile K-BOT (de ex. `PlajaComunaOPuri`) se refac de mana.
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).
