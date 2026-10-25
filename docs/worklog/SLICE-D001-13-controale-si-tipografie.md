# SLICE-D001-13 — Controale, editor și tipografie

Data: 25.10.2026. Aplicat local, numai documentație.

## What changed and why

Opt secțiuni generale și nota finală despre font sunt absorbite în documentele existente. API-urile TextBox/CheckBox/Combobox/TreeView/Menu/ModalFrame/debug nu mai sunt copiate în jurnalul general; clauzele unice de integrare, etapele NIR și riscurile/probele sunt păstrate separat. Introducerea rescrierii VERCON rămâne în jurnalul general.

Variantele succesive sunt explicite: roz inițial → validare la părăsire, furnizori după ultima recepție → alfabet, modal/TVA blocată față de reviziile ulterioare, Adăugare din meniu față de Intrări/consultare. Reperele CSS păstrează ordinea, dimensiunile și probele efectuate atunci.

Inventarul tipografic are 167 de intrări, 131 mapări distincte. Cele 36 de rânduri identice sunt grupate, cu toate pozițiile originale: nu se pierde nici multiplicitatea, nici ordinea reconstruibilă. Baza 14px rămâne reperul migrării, iar formula/panoul din 23.10 și scoaterea finală a butonului Aa sunt consemnate ca revizii separate.

## Files touched

Rânduri eliminate (minus = adăugate).

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md` | -25 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -11 |
| `docs/worklog/SLICE-VC02-02-meniuri-capabilitati-coloane.md` | -8 |
| `docs/worklog/SLICE-VC02-01-alegerea-contextului.md` | -4 |
| `docs/worklog/SLICE-VC08-01-motor-stoc-si-balante.md` | -8 |
| `VERCON/TIPOGRAFIE_MIGRARE.md` | 24 |
| `implementare_vercon_v2.md` | 106 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-13-controale-si-tipografie.md` | -38 |

Bilanț: **10 căi, 34 rânduri mai puțin; un jurnal nou**. Cumul D001-01…13: **48 fișiere și 4528 rânduri mai puțin**.

## Test results

Numai lectură/comparație documentară. Fără teste, builduri, verificatoare, aplicații, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

## Unverified / deferred

Probe și riscuri istorice nu sunt recertificate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-14.
