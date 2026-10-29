# SLICE-VC00-01…05 — Pregătirea documentară VERCON

Data comună etapelor: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md). Reunire documentară D003-33; reviziile și probele rămân la fiecare etapă.

## SLICE-VC00-01 — inventarul si mediul de referinta <a id="slice-vc00-01"></a>

### Ce s-a schimbat si de ce

Inventar al celor 377 obiecte Access (tabele 109, interogari 80, formulare 53, rapoarte 61, module 44, clase 27, macro 3), fiecare cu o singura dispozitie (INCLUS-*, EXCLUS-EFACTURA / CODURI / SCHEMA-ACCESS / INFRA-ACCESS, TEMP, DERIVAT, DE-CLARIFICAT). Excluderea Coduri se propaga **tranzitiv** (potrivire stricta FROM/JOIN): 16 interogari, 2 tabele si 3 rapoarte de balanta (AFB, BF, furnizor) sunt legacy. Manifest al surselor cu SHA-256, dimensiuni, volume si configuratie (MF/OBINV/ZEC), plus lista lipsurilor (frontend, ribbon, `tmpOptiuniSFP`, bazele Materiale).

### Fisiere atinse

`VERCON/INVENTAR_OBIECTE.md`, `VERCON/MANIFEST_SURSE.md`; `VERCON/tools/inventar.js`, `inventar_trans.js`, `dump_schema.ps1`, `README.md`

### Rezultate / probe

- Hash-urile celor 4 baze `.accdb` sunt identice la inceput si la sfarsitul primei runde (runda 2: `tz_Alimente` deschis de doua procese MSACCESS ale utilizatorului, hash nerecalculat; celelalte trei identice); niciun proces MSACCESS; deschidere DAO `ReadOnly`, fara parola.
- `node tools/inventar.js`: 377 obiecte; 21 EXCLUS-CODURI, 13 EXCLUS-EFACTURA, 36 EXCLUS-SCHEMA-ACCESS, 61 EXCLUS-INFRA, 31 DE-CLARIFICAT (cifrele din fisierul generat).
- Nu s-au rulat teste de cod (nu exista cod).

### Nevalidat sau amanat

- Clasificarea este statica (nume + marcaje) si nu dovedeste utilizare in productie; 31 obiecte raman DE-CLARIFICAT.
- Frontendul `Vercon.accdb`, backendul anual `14	z_2024.accdb`, ribbonul, `ufTreeView`, datele `tmpOptiuniSFP`, RES/IMG/TMPLS nu sunt in director.
- Numele fisierelor nu stabilesc subunitatea/gestiunea/anul (D01).

### Revizie 11.10.2026 (dupa raspunsurile utilizatorului)

- Primite si salvate in `VERCON/REFERINTE/`: `tmpOptiuniSFP.xml/.xsd` (47 randuri) si ribbonul `ribbon_Vercon.xml` (copie fara atributele `tag` repetitive).
- Codul de intrare citit: `VaerCon.Intrare_Click` (Forms/VaerCon.txt:1697), `Form_Load`, `mdl_Autoexec.Setari`, `Main.Form_Load`, `basRibbonCallbacks`. Rezultate: gestiunea in Access = folderul `cale.Cale`; subunitatea nu exista in Access; `cale.accdb` are 7 randuri (folderele \ISJ2020 x2, \14 x2, \OI_MONNET, \AUTO, \MF; DC `00000000` peste tot).
- `tools/referenti.js` (cine refera fiecare obiect nelamurit) a redus DE-CLARIFICAT de la 31 la 10 (clasele/modulul ANAF, legate de D16) si a introdus NEREFERIT-NEPORTAT (8 obiecte fara nicio referinta in export). Inventarul se regenereaza cu `node tools/inventar.js` (blocul „overrides" din `inventar.js`).
- Nevalidat: continutul tabelului `Rbb` si legatura bazelor primite cu folderele din `cale`.

### Revizie runda 3 (11.10.2026)

Numerele D01/D16 de mai sus sunt istorice. Decis: atribuirea fisierelor o face operatorul in migrator; ANAF/`clsFirma*` nu se portează (INLOCUIT-PARTENERI-KBOT, 13 obiecte); DE-CLARIFICAT = 0.

## SLICE-VC00-02 — dictionarul de date si graful FK <a id="slice-vc00-02"></a>

### Ce s-a schimbat si de ce

Dictionar al tabelelor business din TZ_SCHEMA cu clasa fiecarei coloane (CHEIE, TEXT, VALOARE-INT, COTA, NUMAR, DATA, BOOL) si tipul tinta propus; matricea celor **26 relatii business** cu tratamentul tinta (RESTRICT/inactivare, FK de context, fara CASCADE DELETE peste istoric); atributele DAO brute din bazele reale (inclusiv 8192 si 4353, neinterpretate); legaturile logice fara relatie declarata; abaterile fiecarei baze reale fata de TZ_SCHEMA (OI: lipsesc `Alimente`, `ProduseM`, `UNIT`, `ListaAlimente.Comentarii`, `Produse.DNF`, `ProduseP.COD`; MF: `ProduseM.ValoareLunara` in plus).

### Fisiere atinse

- `VERCON/DICTIONAR_DATE.md`, `VERCON/tools/dictionar.js`, `dump_schema.ps1`

### Rezultate / probe

- Generat fara erori; clase: CHEIE 81, TEXT 76, VALOARE-INT 43, DATA 19, INT 17, BOOL 4, NUMAR 1, CONFIG 1, COTA 1; zero coloane DE-CLARIFICAT.
- Relatii: 29 in TZ_SCHEMA = 26 business + 3 metadate; distributia atributelor pe cele 3 baze in document.

### Nevalidat sau amanat

- Tipurile tinta sunt propuneri; DDL = VC03-02. Politica FK finala pe relatie = D05. Atributul 8192 nu are semnificatie confirmata. Prima redactare a scriptului a esuat la parsarea CRLF; corectat inainte de a folosi iesirea.

### Revizie runda 3 (11.10.2026)

Tipul VALOARE-INT este acum `INT scalat 10^SCARA` (SCARA pe gestiune: Alimente 4, MF 2, OI 3), valori stocate exact; dictionarul regenerat. Politica FK (fosta D05) ramane de fixat la VC03-02.

## SLICE-VC00-03 — contractul de calcul, scalarea INT si etaloanele <a id="slice-vc00-03"></a>

### Ce s-a schimbat si de ce

Contract numeric: `Z` zecimale pe subunitate, imuabil dupa prima scriere, cota x10000, rotunjire pe linie HALF_EVEN, limita INT cu semnalare inainte de import. Descoperiri din date: `Intrari.TVA` este suma TVA (nu procent) si este inconsistenta pe 114 linii Alimente; totalurile de document depasesc INT la 4 zecimale, deci antetul stocheaza `AjustareAntet = H - S`. Formulele QBC, Balanta22, OI_Balanta/OI_Union_Balanta (N07), Amortizare, fisa pe furnizor (N09) documentate cu sursa si contract; tabel de etaloane E01-E12; reguli N01/N02 cu exemplele MF recalculate (cota 0 pe toate liniile).

### Fisiere atinse

`VERCON/CONTRACT_CALCULE.md`; `VERCON/tools/oracle_probe.ps1`, `oracle_probe2.ps1`, `oracle_probe3.ps1`, `tools/out/oracle_probe*.txt`

### Rezultate / probe

- Probe agregate read-only pe 3 baze (rezultatele brute in `tools/out`): relatia TVA/pret/valoare, intervale si zecimale necesare pe fiecare coloana, antet vs linii, preturi negative (0 in toate).
- Concluzii cheie: Alimente Zreq=4, MF Zreq=2 (total max 1.262.155,34), OI Zreq=4 (total max 1.091.160,61).

### Nevalidat sau amanat

- Nu exista cod de motor; nimic nu a fost comparat pe un motor.
- HALF_EVEN neverificat pe date (VC08-05). Etaloanele E01, E04-E10 sunt sintetice (bazele nu au retururi/casari/transferuri).
- Doar formulele din VC00-03 au fost urmarite, nu toate cele ~70 de interogari.
- `AjustareAntet` este o propunere (D14), reversibila pana la DDL.

### Revizie 11.10.2026 (decizia utilizatorului: zecimale = `NR.ZEC`)

- Contractul §1 rescris: `NR.ZEC` (fallback 4, confirmat in `modStartup.Zec`); **retrasa** propunerea `AjustareAntet`. **Superata de runda 3 (11.10.2026):** `ZEC` si `SCARA` pe gestiune, valori stocate exact, afisare rotunjita in sus; vezi CONTRACT_CALCULE §1.
- Proba noua `tools/oracle_probe4.ps1`: cu `Z=NR.ZEC` nu exista depasiri de INT; pierderi doar in OI: 5 `Intrari.Pret`, 5 `Intrari.PUTVA`, 5 `ProduseP.PUTVA`, 8 `Iesiri.ValoareCUTVA` cu a treia zecimala, diferenta exact 0,005 (decis: stocate exact, scara OI 3, afisate rotunjite in sus).
- `Intrari.TVA` = suma TVA calculata: confirmat de utilizator.
- Etaloane noi E13–E15. Nevalidat: modul de rotunjire pe date reale.

## SLICE-VC00-04 — schitele machetelor si rapoartelor <a id="slice-vc00-04"></a>

### Ce s-a schimbat si de ce

Parser al exporturilor FORMS/REPORTS (sectiuni, controale, pozitii in twips, Caption/ControlSource/SourceObject, linii sursa) si randare PNG cu System.Drawing. 101 schite (43 formulare + 58 rapoarte) pentru obiectele INCLUS-* si DE-CLARIFICAT, plus `VaerCon`/`Main` ca referinta pentru selectorul de context. Index, trasabilitate pe fiecare control si tabelele de capabilitate pe tip (MAT/OI/MF/AL) in MAPARE_ACCESS_WEB si RAPOARTE (dependente de date transitive, parametri `Forms!`/`TempVars`).

### Fisiere atinse

`VERCON/SCHITE/*.png` (101), `VERCON/SCHITE/INDEX.md`, `VERCON/SCHITE/trasabilitate/*.md`; `VERCON/MAPARE_ACCESS_WEB.md`, `VERCON/RAPOARTE.md`; `VERCON/tools/schite.js`, `schite_render.ps1`, `mapare.js`

### Rezultate / probe

- Generare fara erori (0 esecuri de parsare); `Main` are 0 controale in export (ribbon).
- Verificate vizual 4 imagini (BR, FC, NIR, OI_Predare): lizibile, recognoscibile fata de structura exportului; suprapunerile din FC sunt stari alternative de vizibilitate din Access.

### Nevalidat sau amanat

- Restul de 97 imagini nu au fost inspectate una cate una.
- Schitele nu reprezinta functionalitate implementata si nu sunt designul web final.
- Vizibilitatea setata din VBA, ribbonul, imaginile si ActiveX nu sunt recuperabile din export.
- Rapoartele BF/AFB/furnizor nu au schita (exclus tranzitiv prin Coduri).

### Revizie 11.10.2026

- **Toate cele 101 imagini au fost privite individual** (initial doar 4; esantionarea nu avea justificare tehnica). A gasit: (a) defect de randare — controale fara `Height` explicit desenate cu dimensiune 0 (`Fisa_Produsului` aparea fara date) → geometria foloseste acum `LayoutCached*`; (b) text cu `\015\012` literal → curatat; (c) pagini de tab nedesenate (`SETARI`) → desenate; (d) latime minima pentru titluri.
- Dupa corectie: 26 imagini reprivite individual; restul sunt acoperite de verificarea automata (23 controale vizibile cu dimensiune 0 in export, listate in INDEX).
- Defecte ale sursei vazute pe schite: DECIZII_DESCHISE F14. Meniul SFP integrat in RAPOARTE.md; ribbonul si regulile `Form_Open` in MAPARE_ACCESS_WEB.md.

## SLICE-VC00-05 — registrul deciziilor si planul executabil <a id="slice-vc00-05"></a>

### Ce s-a schimbat si de ce

Registru care separa deciziile utilizatorului (neredeschise) de constatarile noi F1-F9 si de propunerile modelului (D03, D14, HALF_EVEN, domeniul Z, tratamentul coloanei TVA); starea D01-D12 dupa VC00; backlog independent de deciziile deschise. Registru de status separat `VERCON_STATUS.md` (pe modelul ADECHIT_STATUS) si trimitere in `KBOT_STATUS.md`.

### Fisiere atinse

- `VERCON/DECIZII_DESCHISE.md`, `VERCON/VERCON_STATUS.md`, `docs/worklog/KBOT_STATUS.md` (trimitere VERCON), cele 5 worklogs VC00

### Rezultate / probe

- Verificare documentara: toate linkurile relative din documentele noi verificate cu script (vezi sectiunea de verificare din status).

### Nevalidat sau amanat

- (Stare initiala; dupa runda 3 nu mai exista intrebari deschise cerute utilizatorului, vezi sectiunea de mai jos.)
- Documentul PLAN_IMPLEMENTARE.md nu a fost rescris; constatarile care il corecteaza (balante BF/AFB exclus tranzitiv; antet ca ajustare) sunt in DECIZII_DESCHISE.

### Revizie 11.10.2026

Registrul rescris: F2 retras (totalurile incap cu `Z=NR.ZEC`), D14 eliminat, D03 decis pe `Z = NR.ZEC`, D01/D10 reduse, D15 (ZEC in subunitate, rotunjirea celor 13 valori) si D16 (verificare ANAF in Furnizori) adaugate. Planul principal are corectiile in checkpoint.

### Revizie runda 3 (11.10.2026)

Registrul `DECIZII_DESCHISE.md` rescris dupa raspunsurile Q1-Q12; `CONTRACT_CALCULE.md`, `MAPARE_ACCESS_WEB.md`, `RAPOARTE.md`, `MANIFEST_SURSE.md`, `DICTIONAR_DATE.md`, `INVENTAR_OBIECTE.md` si `VERCON_STATUS.md` aliniate la sectiunea autoritara din `PLAN_IMPLEMENTARE.md`. Stare: documentat / verificat local (linkuri); nimic validat pe server.
