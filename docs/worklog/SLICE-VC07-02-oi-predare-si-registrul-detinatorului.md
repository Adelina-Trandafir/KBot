# SLICE-VC07-02 — OI predare și registrul deținătorului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»).

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
- Nu se verifica daca produsul apartine contului de intrare ales (categoria produselor migrate poate fi goala/diferita); filtrul ramane de ales de operator.
- Aceleasi machete pentru MF (capabilitati `oi_*` valabile pe OI si MF).
- Randurile de registru ale documentelor migrate se citesc ca atare; coliziuni de id intre tipuri vechi (IDBP=IDBT=5) nu sunt rezolvate pentru istoric.
- Nimic rulat.
