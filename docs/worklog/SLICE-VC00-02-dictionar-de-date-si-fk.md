# SLICE-VC00-02 — dictionarul de date si graful FK

Data: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md).

## Ce s-a schimbat si de ce

Dictionar al tabelelor business din TZ_SCHEMA cu clasa fiecarei coloane (CHEIE, TEXT, VALOARE-INT, COTA, NUMAR, DATA, BOOL) si tipul tinta propus; matricea celor **26 relatii business** cu tratamentul tinta (RESTRICT/inactivare, FK de context, fara CASCADE DELETE peste istoric); atributele DAO brute din bazele reale (inclusiv 8192 si 4353, neinterpretate); legaturile logice fara relatie declarata; abaterile fiecarei baze reale fata de TZ_SCHEMA (OI: lipsesc `Alimente`, `ProduseM`, `UNIT`, `ListaAlimente.Comentarii`, `Produse.DNF`, `ProduseP.COD`; MF: `ProduseM.ValoareLunara` in plus).

## Fisiere atinse

- `VERCON/DICTIONAR_DATE.md`, `VERCON/tools/dictionar.js`, `dump_schema.ps1`

## Rezultate / probe

- Generat fara erori; clase: CHEIE 81, TEXT 76, VALOARE-INT 43, DATA 19, INT 17, BOOL 4, NUMAR 1, CONFIG 1, COTA 1; zero coloane DE-CLARIFICAT.
- Relatii: 29 in TZ_SCHEMA = 26 business + 3 metadate; distributia atributelor pe cele 3 baze in document.

## Nevalidat sau amanat

- Tipurile tinta sunt propuneri; DDL = VC03-02. Politica FK finala pe relatie = D05. Atributul 8192 nu are semnificatie confirmata. Prima redactare a scriptului a esuat la parsarea CRLF; corectat inainte de a folosi iesirea.

## Revizie runda 3 (11.10.2026)

Tipul VALOARE-INT este acum `INT scalat 10^SCARA` (SCARA pe gestiune: Alimente 4, MF 2, OI 3), valori stocate exact; dictionarul regenerat. Politica FK (fosta D05) ramane de fixat la VC03-02.
