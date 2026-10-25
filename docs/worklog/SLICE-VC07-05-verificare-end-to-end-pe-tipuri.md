# SLICE-VC07-05 — verificarea end-to-end pe tipuri

Stare: **PLANIFICAT** (matricea scrisa, **nimic executat**). Cererea operatorului, 12.10.2026 («continuă cu VC07»).

## Ce s-a facut
- Matricea flux → rezultat este in [PROBE_VC07](../../VERCON/PROBE_ACCES.md#probe-vc07): recepție→consum/listă, predare→transfer/casare, retur/storno, pe fiecare tip si pe doua gestiuni de acelasi tip, inclusiv scenarii de rollback la actualizarea registrelor si de acces exclusiv.
- Garantii verificabile prin cod (citite, nerulate): niciun efect dublat intre stoc si OI (predarea scade stocul **numai** prin BC-ul creat, detinerea vine din registru); meniul si API-ul refuza functiile straine tipului (`operations.type_allows`, `capabilities.FORMS`); toate scrierile unui document sunt intr-o singura tranzactie.
- Scenariile fara date reale sunt marcate **sintetice**, nu paritate pe productie.

## Neverificat
Tot: nu s-a rulat niciun scenariu. Rularea lor este testarea operatorului.
