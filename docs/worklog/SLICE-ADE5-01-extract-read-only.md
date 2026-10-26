# SLICE-ADE5-01…03 — Extractul read-only, importul și reconcilierea inițială

Pipeline-ul inițial din 08.10 păstrează 5.247 rânduri și cele 745 legături istorice raportate separat. [ADE5-04…14/ADE10-07](SLICE-ADE5-04-ade-migrator.md) îl înlocuiește cu utilitarul direct; rezultatele SQLite și confirmarea ulterioară a operatorului rămân probe distincte de implementarea acelui utilitar.

- [ADE5-01](#slice-ADE5-01)
- [ADE5-02](#slice-ADE5-02)
- [ADE5-03](#slice-ADE5-03)

## SLICE-ADE5-01 — extract Access read-only <a id="slice-ADE5-01"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). `extract_access.ps1` citește 14 tabele și CFGs/CH prin ACE în mod read-only. Extract real: 5.247 rânduri. Nu a fost oprit și nu rula niciun proces MSACCESS.

## SLICE-ADE5-02 — import repetabil <a id="slice-ADE5-02"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Importul validează tipuri/relații, păstrează ID-urile, folosește hash și jurnal, nu suprascrie configurația existentă și păstrează proveniența M05. Câmpurile de reduceri excluse sunt ignorate explicit.

## SLICE-ADE5-03 — reconciliere <a id="slice-ADE5-03"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Importul extractului real într-un SQLite gol și reconcilierea au raportat zero diferențe. Sunt raportate separat 745 legături istorice către luni/prezențe lipsă; datele nu sunt șterse. Importul pe server rămâne la utilizator.
