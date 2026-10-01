# K-BOT — STATUS, slices 0100–0109

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0100

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0100 | **Descărcări multiple: mai multe angajamente deodată, câte un tab FOREXE fiecare (cererea operatorului, 01.10.2026)** — robotul (`RunJobsParallelAsync`: coadă FIFO, cel mult 10 taburi, o eroare nu le oprește pe celelalte, la sfârșit rămâne un singur tab — al ultimei descărcări fără eroare), coordonatorul (`DownloadNodesParallelAsync`), shell-ul (fereastra «Actualizează angajamente», actualizarea la conectare, întrebarea despre angajamentele noi, ingestia una câte una), `FX_Angajamente.DataActualizare`, setări (avansate) + pagina «Aplicație» regrupată | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate** | `SLICE-0100-multithreading-descarcari.md` | Ordinea de deploy: întâi `sql/0100_fx_angajamente_data_actualizare.sql`, apoi `prelucrare.py` + `tree.py`. Ajutorul: 0000-34. |

| 0100-02 | **`Setari`: serverul decide multi-thread (cererea operatorului, 01.10.2026)** — tabel `Setari` pe fiecare bază (`Multithread` 0/1, `Multithread_Max`), `GET /api/setari`, `ServerSettings`, pagina nouă «Descărcări multiple» în Setări (ascunsă cât `Multithread = 0`), grupul scos din «Aplicație»; opțiunile avansate nu mai contează | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate; ajutorul și capturile NEFĂCUTE aici (ajutorul 0100 e pe `master`)** | `SLICE-0100-02-setari-server-multithread.md` | Ordinea: `sql/0100_02_setari.sql` (AVACONT_SURSA), `sql/0100_02_setari_toate_bazele.sql` (toate unitățile), apoi `setari.py`. |

### Current focus

- Branch `SLICE-0100-Multithreading`. Nimic din partea browserului (taburi în același context, adoptarea tabului păstrat)
  nu a fost văzut rulând.

### Open threads

- **0100 — partea cu taburile e nedovedită** (tab nou fără altă autentificare, taburi din fundal, fereastra andocată
  care arată tabul principal, `AdoptTabAsync`). De probat pe un cont real cu 2–3 angajamente, apoi 10.
- **0100 — DDL înainte de server.** Până la aplicare, arborele întoarce `DataActualizare = null` și salvarea nu scrie data.
- **0100 — fără limită de timp pe lucrare**: o descărcare se oprește prin așteptările din workflow (× multiplicatorul).
- **0100-02 — ajutor de actualizat** (pe `master`, 0000-34): `contabil.forexe.descarcare-multipla` (secțiunea «Cum o pornești»: pagina nouă, apare doar cu `Multithread = 1` pe server, nu mai cere opțiunile avansate; limita de taburi vine de la server), `contabil.setari` (grupul a ieșit din «Aplicație»; pagina nouă în listă), `avansat` (pasul 3 nu mai pomenește grupul). **Textul exact e pregătit în `docs/worklog/SLICE-0000-36-DRAFT-ajutor-setari-multithread.md`, de aplicat pe `master`.** Capturi: `setari-descarcari-multiple` (alt ecran acum), `setari` și `avansat-activare` (de refăcut).
- **0100-02 — DDL înainte de server**; `Setari` lipsă = multi-thread oprit peste tot.
- **0100 — `tests/KBot.App.Tests` nu compila deja înainte** (`capturiApi`, indexer `JobRequest`).
