# SLICE-VC00-03 — contractul de calcul, scalarea INT si etaloanele

Data: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md).

## Ce s-a schimbat si de ce

Contract numeric: `Z` zecimale pe subunitate, imuabil dupa prima scriere, cota x10000, rotunjire pe linie HALF_EVEN, limita INT cu semnalare inainte de import. Descoperiri din date: `Intrari.TVA` este suma TVA (nu procent) si este inconsistenta pe 114 linii Alimente; totalurile de document depasesc INT la 4 zecimale, deci antetul stocheaza `AjustareAntet = H - S`. Formulele QBC, Balanta22, OI_Balanta/OI_Union_Balanta (N07), Amortizare, fisa pe furnizor (N09) documentate cu sursa si contract; tabel de etaloane E01-E12; reguli N01/N02 cu exemplele MF recalculate (cota 0 pe toate liniile).

## Fisiere atinse

- `VERCON/CONTRACT_CALCULE.md`
- `VERCON/tools/oracle_probe.ps1`, `oracle_probe2.ps1`, `oracle_probe3.ps1`, `tools/out/oracle_probe*.txt`

## Rezultate / probe

- Probe agregate read-only pe 3 baze (rezultatele brute in `tools/out`): relatia TVA/pret/valoare, intervale si zecimale necesare pe fiecare coloana, antet vs linii, preturi negative (0 in toate).
- Concluzii cheie: Alimente Zreq=4, MF Zreq=2 (total max 1.262.155,34), OI Zreq=4 (total max 1.091.160,61).

## Nevalidat sau amanat

- Nu exista cod de motor; nimic nu a fost comparat pe un motor.
- HALF_EVEN neverificat pe date (VC08-05). Etaloanele E01, E04-E10 sunt sintetice (bazele nu au retururi/casari/transferuri).
- Doar formulele din VC00-03 au fost urmarite, nu toate cele ~70 de interogari.
- `AjustareAntet` este o propunere (D14), reversibila pana la DDL.

## Revizie 11.10.2026 (decizia utilizatorului: zecimale = `NR.ZEC`)

- Contractul §1 rescris: `NR.ZEC` (fallback 4, confirmat in `modStartup.Zec`); **retrasa** propunerea `AjustareAntet`. **Superata de runda 3 (11.10.2026):** `ZEC` si `SCARA` pe gestiune, valori stocate exact, afisare rotunjita in sus; vezi CONTRACT_CALCULE §1.
- Proba noua `tools/oracle_probe4.ps1`: cu `Z=NR.ZEC` nu exista depasiri de INT; pierderi doar in OI: 5 `Intrari.Pret`, 5 `Intrari.PUTVA`, 5 `ProduseP.PUTVA`, 8 `Iesiri.ValoareCUTVA` cu a treia zecimala, diferenta exact 0,005 (decis: stocate exact, scara OI 3, afisate rotunjite in sus).
- `Intrari.TVA` = suma TVA calculata: confirmat de utilizator.
- Etaloane noi E13–E15. Nevalidat: modul de rotunjire pe date reale.
