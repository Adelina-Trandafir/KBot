# K-BOT — STATUS, slices 0100–0109

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0100

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0100 | **Descărcări multiple: mai multe angajamente deodată, câte un tab FOREXE fiecare (cererea operatorului, 01.10.2026)** — robotul (`RunJobsParallelAsync`: coadă FIFO, cel mult 10 taburi, o eroare nu le oprește pe celelalte, la sfârșit rămâne un singur tab — al ultimei descărcări fără eroare), coordonatorul (`DownloadNodesParallelAsync`), shell-ul (fereastra «Actualizează angajamente», actualizarea la conectare, întrebarea despre angajamentele noi, ingestia una câte una), `FX_Angajamente.DataActualizare`, setări (avansate) + pagina «Aplicație» regrupată | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate** | `SLICE-0100-multithreading-descarcari.md` | Ordinea de deploy: întâi `sql/0100_fx_angajamente_data_actualizare.sql`, apoi `prelucrare.py` + `tree.py`. Ajutorul: 0000-34. |

### Current focus

- Branch `SLICE-0100-Multithreading`. Nimic din partea browserului (taburi în același context, adoptarea tabului păstrat)
  nu a fost văzut rulând.

### Open threads

- **0100 — partea cu taburile e nedovedită** (tab nou fără altă autentificare, taburi din fundal, fereastra andocată
  care arată tabul principal, `AdoptTabAsync`). De probat pe un cont real cu 2–3 angajamente, apoi 10.
- **0100 — DDL înainte de server.** Până la aplicare, arborele întoarce `DataActualizare = null` și salvarea nu scrie data.
- **0100 — fără limită de timp pe lucrare**: o descărcare se oprește prin așteptările din workflow (× multiplicatorul).
- **0100 — `tests/KBot.App.Tests` nu compila deja înainte** (`capturiApi`, indexer `JobRequest`).
