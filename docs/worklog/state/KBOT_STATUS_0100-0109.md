# K-BOT — STATUS, slices 0100–0109

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0100

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0100 | **Descărcări multiple: mai multe angajamente deodată, câte un tab FOREXE fiecare (cererea operatorului, 01.10.2026)** — robotul (`RunJobsParallelAsync`: coadă FIFO, cel mult 10 taburi, o eroare nu le oprește pe celelalte, la sfârșit rămâne un singur tab — al ultimei descărcări fără eroare), coordonatorul (`DownloadNodesParallelAsync`), shell-ul (fereastra «Actualizează angajamente», actualizarea la conectare, întrebarea despre angajamentele noi, ingestia una câte una), `FX_Angajamente.DataActualizare`, setări (avansate) + pagina «Aplicație» regrupată | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate** | `SLICE-0100-multithreading-descarcari.md` | Ordinea de deploy: întâi `sql/0100_fx_angajamente_data_actualizare.sql`, apoi `prelucrare.py` + `tree.py`. Ajutorul: 0000-34. |

| 0100-02 | **`Setari`: serverul decide multi-thread (cererea operatorului, 01.10.2026)** — tabel `Setari` pe fiecare bază (`Multithread` 0/1, `Multithread_Max`), `GET /api/setari`, `ServerSettings`, pagina nouă «Descărcări multiple» în Setări (ascunsă cât `Multithread = 0`), grupul scos din «Aplicație»; opțiunile avansate nu mai contează | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate; ajutorul: 0000-36 (fără capturi)** | `SLICE-0100-02-setari-server-multithread.md` | Ordinea: `sql/0100_02_setari.sql` (AVACONT_SURSA), `sql/0100_02_setari_toate_bazele.sql` (toate unitățile), apoi `setari.py`. |
| 0101 | **Lanțuri de recepții care nu se închid: roșu în fereastra de asociere și în arborele principal; «+» pe lună fără mesaj de succes (cererea operatorului, 02.10.2026)** — `AsociereForm` (R și ultimul H în roșu, peste orice altă regulă), `GET /api/forexe/tree` (coloana `LantNeinchis`), arborele din `KbotForm` (rând roșu + motivul în tooltip), generarea în lot din Plăți fără caseta de după | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **`tree.py` nedeployat; ajutorul: 0000-37** | `SLICE-0101-lanturi-neinchise-si-ord-lot.md` | Fără DDL. SQL-ul testat de operator pe `014_SCSV`: 5 din 39 angajamente semnalate. `tests/test_forexe_tree.py` neatins și nerulat (coloană nouă pe rând). |
| 0101-01 | **ORD și DDF: bara cu pagini rămâne pe o lună / «Toate…»; pagina «Documente» are lista de tipărire (cererea operatorului, 02.10.2026)** — pe o lună sau pe rădăcină «Vizualizare» arată din nou datele (rândurile tuturor ordonanțărilor / reviziilor de sub ea), iar pagina «Document» / «Document PDF» se numește «Documente» și arată lista de tipărire din 0099; pe o frunză rămâne documentul; `OrdView` + `DdfView` (`IsListNode` / `IsListTab`), `KBotNavList.SetItemText` | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **ajutorul: 0000-38** | `SLICE-0101-01-documente-pe-luna-ord-ddf.md` | Fără server, fără DDL. Neverificat: trecerea frunză (pagina Document, cu Adobe) → lună și înapoi. |

### Current focus

- Branch `SLICE-0100-Multithreading`. Nimic din partea browserului (taburi în același context, adoptarea tabului păstrat)
  nu a fost văzut rulând.

### Open threads

- **0101 — nimic văzut pe ecran** (rândurile roșii din arbore și din fereastra de asociere, tooltipul nou; lizibilitatea roșului pe un rând SELECTAT). `tree.py` trebuie pus pe server; până atunci clientul nou nu arată roșu. Ajutorul e făcut în 0000-37, fără capturi noi.
- **0101-01 — nimic văzut pe ecran** (numele filei «Documente», trecerea între listă și pagina de date, trecerea frunză → lună cu un document deschis în Adobe). Ajutorul: 0000-38, fără capturi de refăcut (cele patru poze ale listei nu fuseseră făcute).
- **0100 — partea cu taburile e nedovedită** (tab nou fără altă autentificare, taburi din fundal, fereastra andocată
  care arată tabul principal, `AdoptTabAsync`). De probat pe un cont real cu 2–3 angajamente, apoi 10.
- **0100 — DDL înainte de server.** Până la aplicare, arborele întoarce `DataActualizare = null` și salvarea nu scrie data.
- **0100 — fără limită de timp pe lucrare**: o descărcare se oprește prin așteptările din workflow (× multiplicatorul).
- **0100-02 — ajutorul e făcut (0000-36), capturile nu**: `setari`, `avansat-activare`, `setari-descarcari-multiple` (roșii) și `arbore-meniu-actualizare`, `actualizare-multipla` (lipsă) cer o unitate cu `Multithread = 1`.
- **0100-02 — DDL înainte de server**; `Setari` lipsă = multi-thread oprit peste tot.
- **0100 — `tests/KBot.App.Tests` nu compila deja înainte** (`capturiApi`, indexer `JobRequest`).
