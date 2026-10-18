# SLICE-VC06-04 — BR și FCS

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Retur BC (BR)** (`create_br/edit_br`): se alege un BC existent; produsele lui apar cu cantitatea de pe bon, ce s-a returnat deja si ce a ramas; se returneaza partial si de mai multe ori. Antet nou in `VC_Iesiri_H` (`Retur=1`, `IdRetur` = bonul sursa, primitor/categorie mostenite, numar `BX<n>` ca in Access), linii in `VC_IesiriRetur` la **pretul bonului**. Cantitate peste cea ramasa sau data inaintea bonului → refuz. Mareste stocul.
- **Storno recepție (FCS)** (`create_storno/edit_storno`): se alege un NIR; produsele cu cantitatea primita, deja stornata si ramasa; storno partial/multiplu. Antet nou in `VC_NIR` (`Storno=1`, `IdStorno` = NIR-ul sursa, furnizor mostenit, numarul stocat **negativ**, contor separat), linii in `VC_IntrariStorno` la **pretul din recepție**. Scade stocul de la data documentului storno (un storno ulterior nu schimba disponibilul unei date anterioare).
- Pretul nu poate fi negativ (vine din documentul sursa, care are pret ≥ 0); eroarea nu lasa document partial (o singura tranzactie).
- Pagina: alegerea sursei, apoi grila cu «De stornat / De returnat»; produsele fara cantitate nu intra in document.

## Fisiere
`PYTHON/routes/vercon/`: `documents.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`, `app.js`.

## Presupuneri si neverificat
- Pretul de storno/retur este pretul **documentului sursa**, nu `ProduseP.PUTVA` actual (liniile isi pastreaza valorile proprii, Q9). Daca o recepție are doua linii ale aceleiasi variante la preturi diferite, se foloseste cel mai mic.
- Retururile/storno nu ating registrul OI/MF (VC07). Efectul unui BR/FCS pe MF/OI nu este definit (D04/D10 deschise): FCS este permis pe toate tipurile, BR doar pe Materiale/Alimente.
- Nimic rulat.

## Extensie 18.10.2026 — documente migrate fara sursa

Stare: **IMPLEMENTAT ÎN SURSE, NERULAT**. Cererea operatorului: implementarea
`VERCON/CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md`, sectiunile 3 si 3b.

### Ce s-a schimbat si de ce

- BR cu `IdRetur IS NULL`: indicator explicit in lista si antet; alegerea BC
  este disponibila in consultare doar pentru returul activ fara sursa.
  Actiunea dedicata valideaza fiecare varianta si suma cantitatilor sale fata
  de `return_eligible(..., exclude=doc_id)`. Refuzurile sunt in romana, la
  campul `source`, cu lista produselor incompatibile. Asocierea modifica
  numai `IdRetur` si `Version`, cu audit `br.link`, fara controlul cronologiei
  stocului. Modificarea obisnuita cere explicit asocierea prealabila.
- FCS: lista liniilor `VC_IntrariStorno.IdIntrare IS NULL`, inclusiv cele
  anulate, cu produs, cantitate, pret si data salvarii. Asocierea individuala
  cere o FCS neanulata din acelasi context, cu receptie sursa valida, si
  verifica disponibilul prin regulile storno existente. Cantitatile deja
  legate de FCS aleasa sunt incluse in consumul disponibilului. Se pastreaza
  cantitatea si pretul liniei migrate. Versiunile liniei si antetului cresc;
  se aplica `check_timeline` si audit `storno.link` in aceeasi tranzactie.
  Anularea separata a liniei orfane cere motiv si lasa linia vizibila.
- Rutele pastreaza drepturile BR/FCS, contextul rezolvat de server, tokenul
  blocarii si perioada deschisa. Cheia cererii permite reluarea aceleiasi
  operatii dupa pierderea raspunsului. UI blocheaza schimbarea contextului
  cat timp operatia este in curs sau rezultatul ramane neconfirmat.
- Ajutorul web explica ambele fluxuri si avertizeaza asupra coliziunilor
  identificatorilor OI migrati. Numerotarea OI nu a fost schimbata.

### Fisiere atinse

- `PYTHON/routes/vercon/documents.py`, `doc_routes.py`, `migration_links.py` (nou).
- `PYTHON/static/js/vercon/docs.js`, `orphan-storno.js` (nou), `app.js` (ajutor).
- `PYTHON/static/css/vercon.css`, `vercon-migration.css` (nou).
- `implementare_vercon_v2.md`, `VERCON/VERCON_STATUS.md`, acest worklog.

### Rezultate si limite

Doar analiza de design a surselor si a integrarii cu protocolul/repository-ul
existente; structura coloanelor a fost citita din dumpul local
`MariaDB_Schema/AVACONT_SURSA.sql`. Nu s-au rulat teste, compilari, verificari
de sintaxa, browser, HTTP, SQL sau alte verificari automate. Nicio schema
modificata si nicio creare automata de tabele. Nedeployat, neacceptat vizual.
Anularea BR nu solicita sursa in codul existent; regulile de stoc raman
aplicabile, inclusiv la anulare. Scenariile de acceptare, toate NERULATE,
sunt descrise in intrarea din 18.10.2026 din `implementare_vercon_v2.md`.
Decizia numerotarii OI/MF ramane operatorului. Modificarile sunt in arborele
local de lucru; nu s-a facut commit sau push.
