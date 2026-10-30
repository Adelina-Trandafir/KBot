# SLICE-AV01-14 - SablonOperatii: the operation templates (operator, 27.10.2026)

## What changed and why
The Access button OCI (form `Operatii2020`, tables `DefaNoteA` + `DefaNote`) is ported as **SablonOperatii**. The salary templates (OCS) are **SablonSalarii** and come in the
next slice. Both live in ONE window, «Sabloane» (ribbon Date > «Sabloane»), with a vertical bar like the rest of the application: «Operatii cu clasificatie» (type M) and
«Operatii diverse» (type A); the salary page (AV01-15) is the third item of the bar.

Decisions of the operator (27.10.2026): type **M** = an operation with a classification and a partner on ONE side (debit or credit), fixed or chosen at use; type **A** = any other
operation, it may have no partner and no classification. The Access types `C` and `M` both become `M`. Any account may take a partner; no separate role for the accounts;
inactive accounts are out until the end of AVACONT.

1. **DDL** `sql/AVACONT/Configurari/SablonOperatii/AV01_14_sablon_operatii.sql` (on `AVACONT_SURSA`, then schema sync): `TZ_SablonOperatii` (journal, type, kind E/P/A, explanation, department,
   payment text, key) and `TZ_SablonOperatiiLinie` (form, BASE debit / credit account, partner mode NIMIC / OPERATIE / FIX + fixed partner, flags PA / Ordonantare / Chelt). The sector+source
   of a template is the one of its journal. Lines go with their template (CASCADE); the journal, accounts and partners are RESTRICT. No `Activ` column (deleting a template that
   operations use will be blocked by the foreign key those operations bring in stage 2).
2. **Server** `PYTHON/routes/avacont/configurari/sablon_operatii/sablon_operatii.py`: `GET` and `POST /api/avacont/sabloane-operatii` (one transaction; the lines of a template are replaced on save).
   Validation with Romanian messages: key required for M, partner on one side only for M, flags only for M (PA only with E, Ordonantare only with P), FelOP only with P, form NN.NN unique per
   template (lines without a form may repeat), at least one line, references exist.
3. **Client** `KBot.Domain/Avacont/Configurari/SablonOperatii/SablonOperatiiInfo.vb`, `KBot.Api/.../ISablonOperatiiApi.vb` + `ApiClient.SablonOperatii.vb`.
4. **Window** `Avacont/Configurari/Sabloane/` (`SabloaneForm`, `ISablonPagina`, `KbotForm.Sabloane.vb`) and `Avacont/Configurari/SablonOperatii/Views/SablonOperatiiView` (one class, bound
   with type M or A). The look follows the Access screens (operator's screenshots): the tree of operations on the left (type M: Title > Article > Paragraph from the classification key,
   names from `DefaClsfE`, first row «Adauga operatie», evidence blue / payment green; type A: one flat list under «Adauga operatie diversa»), the header of the operation and ONE of its
   lines on the right, a pager over the lines. Machete `sabloane`, ribbon button «Sabloane», page switching and closing ask about unsaved changes like «Clasificatii».
5. **Migration** `AVC.Migrator`: step `SablonOperatii` (after Subunitati and Jurnale): reads `DefaNoteA` + `DefaNote`, base account = first seven digits of the Access symbol, `XXX` -> the partner
   of the operation, a code -> a fixed partner, key of type C = the header's `CodPartener`, otherwise from the first line's classification through `Clasificatii.IdClsfAcc`. A template with
   anything the target would refuse is left out whole and named in the verification; an existing template is not overwritten.
6. **Help** 0000-134 (`contabil/nomenclatoare/sabloane.md`, label `AV01-14`).

## Files touched
`sql/.../AV01_14_sablon_operatii.sql`; `PYTHON/routes/avacont/__init__.py`, `.../sablon_operatii/{__init__,sablon_operatii}.py`; `SablonOperatiiInfo.vb`, `ISablonOperatiiApi.vb`, `ApiClient.SablonOperatii.vb`;
`SabloaneForm.vb` + `.Designer.vb`, `ISablonPagina.vb`, `KbotForm.Sabloane.vb`, `SablonOperatiiView.vb` + `.Designer.vb`, `KbotForm.Machete.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb`;
`AVC.Migrator/AvcStep.vb`, `AvcCatalog.vb`, `Configurari/SablonOperatii/AvcSablonOperatiiStep.vb`; help `sabloane.md`, `nomenclatoare/index.md`; docs `proiect-de-date-etapa-1.md`, `KBOT_STATUS_AV.md`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.App`, `AVC.Migrator`: 0 warnings, 0 errors. `py_compile` of the Python files: ok. No tests run (operator rule); nothing seen on screen; the DDL, the server
routes and the migration step were never run.

## Unverified / deferred
- **DDL not run.** The window cannot work until `AV01_14` is run on `AVACONT_SURSA` and the schema synced.
- Nothing on screen: the layout (96 dpi designer coordinates, row heights of the right-hand table), the tree captions with `‹ ›`, the colours of the operation nodes, `KBotComboBox` typing on the
  account lists (about 1000 items) and the partner lists were never seen.
- The key (`Cheie`) format `65.01.XX.XX.57.02.02` = `Clasificatii.ClsfX` (read from `MariaDB_Schema`); the tree reads Title / Article / Paragraph from its segments 5, 6, 7 and the names from
  `DefaClsfE` (`NN0000`, `NNNN00`, `NNNNNN`) - assumed from the data, not checked against a live database. A paragraph `00` hangs the operations straight under the article.
- The Access column names `DefaNoteA` (`IDA`, `Explicatie`, `Tip`, `Jurnal`, `Fel`, `Compartiment`, `CodPartener`, `FelOP`) and `DefaNote` (`IDA`, `Forma`, `Clsf`, `ContDebit`, `PartDebit`, `ContCredit`,
  `PartCredit`, `Ordonantare`, `PA`, `Chelt`) come from the reading sheet `sablon-operatii-oci.md`, not from a run against the file.
- Old type M (24 templates, a partner code in the header): the key is derived from the line classification; if the lines do not lead to a classification the template is left out and named.
- Kind of an old type C / M template other than E or P, and the migration of templates whose journal is not in `TZ_Jurnale`, leave the template out (named in the verification).
- Open: whether a type-A template may still carry a classification key (the window keeps it empty); whether lines of type A need a form (they have none).
