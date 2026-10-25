# SLICE-D001-03 — Absorbția planurilor redundante și a predărilor istorice

Data: 24.10.2026. Bilanț: 4 fișiere retrase, 1 jurnal nou; net 3 fișiere și 321 de rânduri eliminate. Reorganizare documentară; afirmațiile istorice rămân datate, fără certificare nouă a codului sau a serverului.

## What changed and why

| Fișier retras | Informația păstrată și destinația |
|---|---|
| `docs/KBOT_Tier1_Plan.md` | Redirecționarea este înlocuită de planul [Tier 1](KBOT_Tier1_Plan.md); nu exista un contract distinct în copie. |
| `docs/PLAN_MainForm_Scaffolding.md` | Constructorul cu cinci parametri și trecerea de la șase la zece vederi sunt consemnate în [planul MainForm](PLAN_MainForm_Scaffolding.md), împreună cu referința la 0008. |
| `docs/HANDOFF_TreeDataApi.md` | Bilanțul Tier1 și capcana config-ului fictiv sunt în planul Tier1; livrarea MainForm este în planul MainForm; contractul compus al arborelui și corectarea afirmațiilor greșite sunt în [0008](SLICE-0008-tree-data-api.md) și [planul său](PLAN_TreeDataApi.md). |
| `status_migrare6.md` | Logica pașilor, abaterile, probele și limitele sunt deja în jurnalele 0020-01…05. Commiturile și reperul rezumatului sunt în [starea 0020](state/KBOT_STATUS_0020-0029.md#slice-0020); condițiile semnării amânate sunt păstrate în [0020-05](SLICE-0020-01-ddf-endpoint.md#slice-0020-05). |

Trimiterile de navigare folosesc acum planurile din worklog. Referințele la căile retrase din tabelul istoric Files touched al lui 0008 sunt păstrate ca evidență a intervenției din 2026, cu o notă explicită că nu sunt destinații curente. Nu se copiază în contractul actual afirmațiile respinse despre Salarii, IDORD, scoping sau CODPARTENER. Sursele originale rămase explică de ce au fost respinse.

## Files touched

Planurile Tier1/MainForm/TreeDataApi și jurnalul 0008; stările 0000–0009 și 0020–0029; jurnalul 0020-05; indexul global și registrul D; acest jurnal. Cele patru fișiere din tabel sunt eliminate după transfer.

## Test results

Numai citire, căutarea trimiterilor și analiza documentară sursă/destinație. Nu s-au rulat teste, builduri, verificatoare, aplicații, Git, SQL sau acces MariaDB. Rezultatele de test din documentele absorbite sunt istorice; nu au fost rerulate.

## Unverified / deferred

Modificări locale, fără commit/push. AVACONT, documentația activă AV/AVACONT, fișierele CLAUDE și ajutorul runtime rămân excluse. Navigarea și mecanismele runtime nu au fost executate. Restul consolidării continuă în D001-04.
