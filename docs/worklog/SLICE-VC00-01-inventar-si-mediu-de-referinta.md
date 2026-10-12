# SLICE-VC00-01 — inventarul si mediul de referinta

Data: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md).

## Ce s-a schimbat si de ce

Inventar al celor 377 obiecte Access (tabele 109, interogari 80, formulare 53, rapoarte 61, module 44, clase 27, macro 3), fiecare cu o singura dispozitie (INCLUS-*, EXCLUS-EFACTURA / CODURI / SCHEMA-ACCESS / INFRA-ACCESS, TEMP, DERIVAT, DE-CLARIFICAT). Excluderea Coduri se propaga **tranzitiv** (potrivire stricta FROM/JOIN): 16 interogari, 2 tabele si 3 rapoarte de balanta (AFB, BF, furnizor) sunt legacy. Manifest al surselor cu SHA-256, dimensiuni, volume si configuratie (MF/OBINV/ZEC), plus lista lipsurilor (frontend, ribbon, `tmpOptiuniSFP`, bazele Materiale).

## Fisiere atinse

- `VERCON/INVENTAR_OBIECTE.md`, `VERCON/MANIFEST_SURSE.md`
- `VERCON/tools/inventar.js`, `inventar_trans.js`, `dump_schema.ps1`, `README.md`

## Rezultate / probe

- Hash-urile celor 4 baze `.accdb` sunt identice la inceput si la sfarsitul primei runde (runda 2: `tz_Alimente` deschis de doua procese MSACCESS ale utilizatorului, hash nerecalculat; celelalte trei identice); niciun proces MSACCESS; deschidere DAO `ReadOnly`, fara parola.
- `node tools/inventar.js`: 377 obiecte; 21 EXCLUS-CODURI, 13 EXCLUS-EFACTURA, 36 EXCLUS-SCHEMA-ACCESS, 61 EXCLUS-INFRA, 31 DE-CLARIFICAT (cifrele din fisierul generat).
- Nu s-au rulat teste de cod (nu exista cod).

## Nevalidat sau amanat

- Clasificarea este statica (nume + marcaje) si nu dovedeste utilizare in productie; 31 obiecte raman DE-CLARIFICAT.
- Frontendul `Vercon.accdb`, backendul anual `14	z_2024.accdb`, ribbonul, `ufTreeView`, datele `tmpOptiuniSFP`, RES/IMG/TMPLS nu sunt in director.
- Numele fisierelor nu stabilesc subunitatea/gestiunea/anul (D01).

## Revizie 11.10.2026 (dupa raspunsurile utilizatorului)

- Primite si salvate in `VERCON/REFERINTE/`: `tmpOptiuniSFP.xml/.xsd` (47 randuri) si ribbonul `ribbon_Vercon.xml` (copie fara atributele `tag` repetitive).
- Codul de intrare citit: `VaerCon.Intrare_Click` (Forms/VaerCon.txt:1697), `Form_Load`, `mdl_Autoexec.Setari`, `Main.Form_Load`, `basRibbonCallbacks`. Rezultate: gestiunea in Access = folderul `cale.Cale`; subunitatea nu exista in Access; `cale.accdb` are 7 randuri (folderele \ISJ2020 x2, \14 x2, \OI_MONNET, \AUTO, \MF; DC `00000000` peste tot).
- `tools/referenti.js` (cine refera fiecare obiect nelamurit) a redus DE-CLARIFICAT de la 31 la 10 (clasele/modulul ANAF, legate de D16) si a introdus NEREFERIT-NEPORTAT (8 obiecte fara nicio referinta in export). Inventarul se regenereaza cu `node tools/inventar.js` (blocul „overrides" din `inventar.js`).
- Nevalidat: continutul tabelului `Rbb` si legatura bazelor primite cu folderele din `cale`.

## Revizie runda 3 (11.10.2026)

Numerele D01/D16 de mai sus sunt istorice. Decis: atribuirea fisierelor o face operatorul in migrator; ANAF/`clsFirma*` nu se portează (INLOCUIT-PARTENERI-KBOT, 13 obiecte); DE-CLARIFICAT = 0.
