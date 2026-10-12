# SLICE-VC07-03 — OI transfer și casare

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»). Modelul este cel din [VC07-02](SLICE-VC07-02-oi-predare-si-registrul-detinatorului.md).

## Ce s-a facut (`oi.py`, `docs.js`)
- **Transfer (BT)**: predator, primitor (altcineva), numar, data, explicatii. Ecranul arata ce detine predatorul, **pe loturi** (produs, lot, cantitate detinuta, pretul lotului); operatorul scrie cantitatea de transferat. Serverul verifica: lotul este al predatorului, cantitatea nu depaseste ce detine (adunata pe lot), data nu e inaintea intrarii lotului la predator, pretul este cel al lotului. Se scriu **atomic** liniile `VC_OI_Transfer` si **ambele laturi** ale registrului (+ primitor, − predator): net zero.
- **Casare (BS)**: detinator, numar, data, explicatii; aceleasi reguli pe loturi; scrie liniile `VC_OI_Casare` si randuri negative in registru. Pretul ramane pozitiv, semnul apartine miscarii: casare 2 × 100 → cantitate −2, valoare −200 in registru.
- **Editare/anulare**: la editare lotul documentului propriu este eliberat inainte de validare (cantitatea eligibila include ce a luat documentul); refuz `HAS_OUTPUTS` daca din bunul primit prin transfer s-a mai transferat/casat; anularea sterge randurile derivate si restabileste detinerea predatorului.
- Arbore **detinator → data → BT/BS + total**.

## Presupuneri si neverificat
- Valoarea randului negativ din registru la transfer este stocata cu semn (negativa), spre deosebire de Access (care punea valoare pozitiva pe randul negativ); suma valorilor din registru ramane corecta.
- Eligibilitatea nu depinde de data documentului in afara regulii «nu inainte de intrarea lotului»; capetele de interval ale rapoartelor sunt VC08.
- Stare an: OI/MF au un singur exercitiu mereu deschis; o perioada inchisa ar refuza scrierea (`PERIOD_CLOSED`).
- Nimic rulat.
