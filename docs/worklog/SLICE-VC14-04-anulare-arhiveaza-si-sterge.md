# SLICE-VC14-04 — Anularea arhivează și șterge documentul

12.11.2026 — La cererea Adelinei: NIR-urile, bonurile (consum, retur, predare/transfer/casare) și listele de alimente anulate nu mai rămân în tabele ca «anulat». Se păstrează undeva, **numărul lor se deblochează** și **nu mai apar deloc în arbori**.

## Ce se întâmplă acum la «Anulează»

1. Verificările rămân exact cum erau (are storno, are retur, e într-o listă, stocul rămâne pozitiv, regula ultimului document, perioada închisă, BU legat).
2. La sfârșitul lor, `archive.purge` copiază documentul (antet, linii și tot ce atârnă de el) ca JSON în **`VC_ArhivaAnulate`**, apoi șterge rândurile din tabelele VC_ (copiii înaintea părinților, ca să țină cheile străine).
3. Contorul de numerotare (`VC_Numbering`) coboară la cel mai mare număr rămas în uz → numărul eliberat este iar următorul automat. Un număr din mijloc este liber pentru introducere manuală imediat ce rândul lui a dispărut. NIR/FCS nu au contor (seria pe an se calculează din rânduri), deci ștergerea singură eliberează numărul.
4. Macheta se golește după anulare (nu mai rămâne pe un document «Anulat»); mesajul spune că numărul se poate folosi din nou.

Ce intră în arhivă, pe familii: **NIR** (VC_NIR, VC_Intrari, repartizările pe factură, legătura cu factura, calculul NIR, alocările și trimiterile EF spre Vercon); **FCS** (VC_NIR + VC_IntrariStorno); **BC / BR** (VC_Iesiri_H + VC_Iesiri / VC_IesiriRetur + legăturile cu lista); **lista de alimente** (VC_LABC + mesele + rezumatul + legăturile); **BP / BT / BS** (rândurile documentului + registrul; BP ia cu el și BC-ul lui); **BU** (legătura și liniile ei, plus NIR-ul și BP-ul din destinație, bonurile și BT-ul din sursă).

## Ascundere completă

Arborii nu mai citesc documentele cu `Anulat=1`: NIR (`receipt_tree`, `list_documents`), BC/BR, liste, BP/BT/BS, BU. Seria NIR/FCS și «următorul număr» al BC, BR, liste și BP/BT/BS ignoră și ele rândurile rămase anulate — corect chiar înainte de curățarea unei baze vechi.

## Documentele deja anulate

**Varianta SQL (pe toate bazele):** `sql/VC14_04_MUTA_ANULATE_IN_ARHIVA.sql` (35 KB, o singură interogare; KBotAdmin urcă pe server ca fișier un text peste 30000 de octeți, limita crește la 100000, cât are runnerul), pentru KBotAdmin ▸ Interogări unice ▸ TOATE bazele (întâi «view», apoi rulare; o bază care a rulat textul este sărită). Singură: creează `VC_ArhivaAnulate` dacă lipsește, copiază documentele cu `Anulat=1` ca JSON (coloanele luate din schema reală), șterge exact ce a copiat (fiecare DELETE cere rândul din arhivă), apoi coboară numărătorile. Într-o tranzacție pe bază: dacă pică o instrucțiune, datele bazei rămân neschimbate. Diferențe față de comanda Python: NU mută un NIR cu storno încă activ și un BC cu retur încă activ; BC-ul unui BP se arhivează ca document separat. Generat de un program mic din `MariaDB_Schema/AVACONT_SURSA.sql`, deci rulează pe baze sincronizate cu acea schemă (cu `NumarSub` pe VC_NIR).

**Varianta Python:** Comandă nouă în KBotAdmin ▸ Comenzi ▸ VERCON: **«Arhivare documente anulate VERCON»** (`vercon_arhiva_anulate`, o singură bază, numele bazei scris ca confirmare). Trece prin BU, BP, BT, BS, liste, BR, BC, FCS, NIR (copiii înaintea părinților), câte un document pe commit; ce nu se poate muta rămâne neatins și apare în rezultat cu motivul. **Nu am rulat-o.**

## De aplicat de mână (înainte de publicarea codului)

`sql/VC14_04_ARHIVA_ANULATE.sql` pe AVACONT_SURSA și pe fiecare bază VERCON (tabelă nouă, `CreatedAt`/`UpdatedAt`, utf8mb3, fără chei străine). Fără ea, anularea refuză cu mesaj clar (`ARCHIVE_MISSING`) în loc să șteargă fără să păstreze.

## Fișiere

- `sql/VC14_04_ARHIVA_ANULATE.sql` (nou), `sql/VC14_04_MUTA_ANULATE_IN_ARHIVA.sql` (nou, generat), `KBotAdmin/Server/OnceQuery.vb`, `KBotAdmin/Views/OnceQueryView.vb` + `.Designer.vb` (text peste 30000 de octeți urcat ca fișier, limita 100000; KBotAdmin compilat curat, nerulat)
- `PYTHON/routes/vercon/archive.py` (nou)
- `PYTHON/routes/vercon/documents.py`, `oi.py`, `lista.py`, `bu.py`, `doc_routes.py`, `bu_routes.py`, `receipt_tree.py`
- `PYTHON/routes/vercon/receipt_number.py`, `oi_number.py`, `consumption_number.py`, `numbering_overview.py`
- `PYTHON/routes/admin_cli/vercon_arhiva_anulate.py` (nou), `admin_cli.py`
- `KBotAdmin/Views/CommandsView.vb` (comanda apare sub VERCON)
- `PYTHON/static/js/vercon/docs.js`, `lista.js`, `bu.js`
- `PYTHON/static/help/vercon.json` (ajutor: documentul anulat dispare, numărul se refolosește)

## Verificat / neverificat

- Verificat: `py_compile` curat pe toate fișierele Python atinse, importul modulelor merge, `node --check` curat pe cele trei JS. Interogările și ordinea ștergerilor au fost scrise după schema reală din `MariaDB_Schema/AVACONT_SURSA.sql` (11.11.2026).
- **Nerulat:** nicio anulare pe o bază reală, nicio rulare a comenzii KBotAdmin, nimic văzut pe ecran, KBotAdmin necompilat (o singură constantă și o intrare în dicționar), nepublicat. Fără teste (regula «niciodată teste neîntrebat»).
- Presupus, de verificat la prima rulare: coloanele `Anulat` / `MotivAnulare` / `AnulatDe` / `AnulatLa` sunt pe toate tabelele atinse (confirmat în schema din 11.11.2026); tabelele `EF_VERCON_ALOCARI` și `EF_PrimiteVerconNir` pot lipsi într-o bază veche (codul le sare).
- **Scriptul SQL nu a fost rulat**: pe acest calculator nu există MariaDB, deci sintaxa nu e verificată pe server (REGEXP_REPLACE, JSON_OBJECT și DELETE cu alias cer MariaDB 10.2+). Se rulează întâi ca «view» și pe o singură bază (000_DEMO).
- Deschis: copiile din `VC_ArhivaAnulate` nu au încă o machetă de citire sau de restaurare — sunt doar păstrate.
