# SLICE-VC07-02 — OI predare și registrul deținătorului

Starea inițială comună VC07-02/03: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»).

## Modelul (citit din formularele Access `OI_Predare`, `OI_Transfer`, `OI_Casare` si `mdl_OI_Produse`)
- Un document OI este un **grup de randuri** cu acelasi id (`IDBP`/`IDBT`/`IDBC`), fara tabel de antet; pagina il vede ca pe un document. Revizia documentului = `<cel mai mare id de linie>-<suma reviziilor liniilor>` (se schimba la orice rescriere/anulare).
- **Registrul** `VC_OI_Produse` spune cine detine ce: rand pozitiv = lot primit de un detinator (predare sau latura de primire a unui transfer), rand negativ = iese din lot (latura de predare a transferului, casare). Lot = (id-ul documentului cu care a intrat la detinator, produs). Registrul se rescrie impreuna cu documentul.
- **Predarea** scade stocul de magazie prin **BC-ul pe care il creeaza** (ca in Access: `Iesiri_H` + `Iesiri`, numar = numarul predarii) si creste detinerea prin registru; motorul de stoc vede doar BC-ul, registrul doar detinerea → **nu se aduna** (fara dublarea consumului).

## Ce s-a facut (`oi.py`, rute prin `doc_routes.py`, `docs.js`)
- **BP**: detinator (primitor), cont de intrare, cont primitor, numar automat/manual, data, explicatii; linii = varianta + cantitate la pretul variantei; creeaza BC + `VC_OI_Predare` + randuri de registru intr-o tranzactie; stoc negativ interzis (aceeasi verificare pe zile ca la VC06). Editare: pretul unui produs deja pe document ramane cel de la predare.
- **Id-uri noi** dintr-un singur contor pe gestiune (`VC_Numbering`, tip `OIID`), mereu peste orice id deja folosit in cele patru coloane → nu se ciocnesc loturi de tipuri diferite.
- **Editare/anulare**: refuzate (`HAS_OUTPUTS`) daca din bun s-au facut transferuri sau casari; anularea marcheaza liniile + BC-ul, sterge randurile derivate din registru si reface stocul.
- Ecran: grila BC (NIR), arbore **detinator → data → BP + total**.

## Presupuneri si neverificat
- ~~Nu se verifica daca produsul apartine contului de intrare~~ — **corectat, vezi mai jos**: se verifica. Risc ramas: un produs migrat fara cont (categorie goala) nu poate fi pus pe niciun document pana nu i se atribuie contul in nomenclator.
- Aceleasi machete pentru MF (capabilitati `oi_*` valabile pe OI si MF).
- Randurile de registru ale documentelor migrate se citesc ca atare; coliziuni de id intre tipuri vechi (IDBP=IDBT=5) nu sunt rezolvate pentru istoric.
- Nimic rulat.

## Corectie 12.10.2026 — contul de intrare
Eticheta era inversata fata de Access: **«Cont de intrare» este `MasinaP`** (contul in care sunt produsele in magazie; filtreaza produsele si este contul BC-ului creat), iar **«Cont primitor» este `Masina`** (contul la detinator, scris in registru). Pagina are acum etichetele corecte, ofera numai produsele contului de intrare, iar serverul refuza un produs din alt cont (`Produsul nu face parte din contul ales`). Nerulat.

## SLICE-VC07-03 — OI transfer și casare <a id="slice-vc07-03"></a>

Modelul este cel din [VC07-02](SLICE-VC07-02-oi-predare-si-registrul-detinatorului.md).

### Ce s-a facut (`oi.py`, `docs.js`)
- **Transfer (BT)**: predator, primitor (altcineva), numar, data, explicatii. Ecranul arata ce detine predatorul, **pe loturi** (produs, lot, cantitate detinuta, pretul lotului); operatorul scrie cantitatea de transferat. Serverul verifica: lotul este al predatorului, cantitatea nu depaseste ce detine (adunata pe lot), data nu e inaintea intrarii lotului la predator, pretul este cel al lotului. Se scriu **atomic** liniile `VC_OI_Transfer` si **ambele laturi** ale registrului (+ primitor, − predator): net zero.
- **Casare (BS)**: detinator, numar, data, explicatii; aceleasi reguli pe loturi; scrie liniile `VC_OI_Casare` si randuri negative in registru. Pretul ramane pozitiv, semnul apartine miscarii: casare 2 × 100 → cantitate −2, valoare −200 in registru.
- **Editare/anulare**: la editare lotul documentului propriu este eliberat inainte de validare (cantitatea eligibila include ce a luat documentul); refuz `HAS_OUTPUTS` daca din bunul primit prin transfer s-a mai transferat/casat; anularea sterge randurile derivate si restabileste detinerea predatorului.
- Arbore **detinator → data → BT/BS + total**.

### Presupuneri si neverificat
- Valoarea randului negativ din registru la transfer este stocata cu semn (negativa), spre deosebire de Access (care punea valoare pozitiva pe randul negativ); suma valorilor din registru ramane corecta.
- Eligibilitatea nu depinde de data documentului in afara regulii «nu inainte de intrarea lotului»; capetele de interval ale rapoartelor sunt VC08.
- Stare an: OI/MF au un singur exercitiu mereu deschis; o perioada inchisa ar refuza scrierea (`PERIOD_CLOSED`).
- Nimic rulat.
