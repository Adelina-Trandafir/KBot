# Possible future directions

Decisions deliberately deferred from a slice, with their reason and the question needed to resume. These are historical findings, not a new execution brief. Entries through `FX_ORD_ATT.Imagine` come from slice 0049 (ORD editor); the next four from 0051 (DDF editor), and the last from 0052.

## Per-year reset of `FX_ORD.NrORD` and `FX_DDF.CUAL`

Deferred by the operator on 01.09.2026. Access's one-database-per-year layout implicitly reset `DMax("NrORD", "FX_ORD")` and `DMax("CUAL", "FX_DDF", "DC='…'")`; MariaDB spans years and both counters run continuously.

The operator wants annual resets in a separate slice because the change covers DDF and ORD. First decide the governing year: `YEAR(DataORD)` / `YEAR(DataCreare)` or session `AN` from `Unitati_Ani`. Back-dated documents can make them disagree; the wrong choice creates duplicate numbers within a year. Existing rows keep their numbers.

0049 decision D8 allocates `NrORD = MAX(NrORD) + 1` over the whole table inside the save transaction. It drops the Access `DC='…'` predicate because one database is now one unit and the predicate selected everything.

## The 25-partner cap on an ordonanțare

`qFX_ORD_BASE` has `TOP 25` inside the saved query; `Populeaza_PartSel` repeats it. 0049 preserves `LIMIT 25`, the same `ORDER BY` (which partners are picked is a business rule), and the warning when a day needs multiple documents.

**Undecided:** was 25 a printed-form capacity or a JET query limit? The former stays; the latter may be an artefact. Keep the cap pending that answer: silently enlarging the document could send an incorrect form to the treasury.

## `FX_ORD.CUAL` is `varchar`, `FX_DDF.CUAL` is `int`

Also recorded in `SLICE-0045-01`, item 3. 0049 copies DDF CUAL into ORD by int → text conversion, safe in that direction if the stored text always represents the DDF integer.

**Unverified on live data:** does any unit store a non-integer `FX_ORD.CUAL`? Such a value may carry another meaning that must be understood before reconciling the columns.

## `Explicatie` on `FX_ORD_TBL`: computed or edited?

Access's `frmFX_ORD_TBL` and 0049 expose an editable textbox. Generation computes its initial value from the payment description or `Incarca_Explicatii_Incasari` for negative amounts.

**Undecided:** does the operator edit it? If yes, the computation is a default; if always computed, editing permits divergence between document and data.

## `Incarca_Explicatii_Incasari` filters on a contract code that cannot exist

Found in 0049 and ported faithfully, pending an operator decision. The VBA reads:

```vba
WHERE FX_Extrase.NrDoc IN (SELECT NrDoc FROM FX_Extrase
                            WHERE CodContract='<cod>' AND Left(nrdoc,5)='00000')
  AND FX_Extrase.CodContract='ERRRRRRRRRR'
```

No contract is called `ERRRRRRRRRR`: the dictionary always returns empty, yielding `"LIPSA EXPLICATIE"` as every incasare's explanation and `"INCASARE"` as its justifying document. It resembles a leftover debugging sentinel.

0049 follows D18 from 0048-01 (port, record, no silent repair) and isolates it as `EXPLICATII_FILTRU_CODCONTRACT` in `routes/forexe/ord_edit.py`. If the operator confirms the defect, removing the second predicate is the fix.

<a id="bic-has-no-mariadb-successor"></a>

## BIC — initial diagnosis superseded in 0049-02

The initial “missing table” diagnosis proposed migration or manually typed bank names. [0049-02](worklog/SLICE-0049-ord-edit-form.md#slice-0049-02) records the correction: the operator located AVACONT_COMUN.BIC, the information_schema probe was removed, and failed SELECT / missing rights became the relevant case. The original diagnosis/proposal remains in that history; live access and the SELECT grant were unverified.

## `FX_ORD_ATT.Imagine` — is it really dead?

0049 decision D9: `FX_ORD_ATT.Imagine` is base64 `longtext`. In the demo dump the table is empty (`AUTO_INCREMENT = 1`), while `FX_ORD_DOC` is at 719 and `FX_ORD_TBL` at 891. This suggested the screenshot feature was unused. 0049 stores attachment bytes in the new `FX_ORD_ATT_IMG` blob table and leaves Imagine unread and unwritten.

**Demo evidence only.** The proposed live check, not executed here, is:

```sql
SELECT COUNT(*) AS randuri,
       SUM(CASE WHEN Imagine IS NOT NULL AND Imagine <> '' THEN 1 ELSE 0 END) AS cu_imagine
  FROM <BAZA>.FX_ORD_ATT;
```

Any live rows with content require moving their bytes before ignoring the column. `FX_ORD_DOC`'s separate base64-in-`DocJust` attachments remain unchanged: that mechanism was live (719 rows) and was ported as-is. Unifying the two mechanisms is a separate question.

<a id="kbotcombobox-cannot-be-editable--closed-by-slice-0051-02"></a>

## Editable combo — control delivered, DDF integration corrected

[0051-02](worklog/SLICE-0051-02-combo-editabil.md) records Editable, LimitToList and native EDIT theming; [0051-03](worklog/SLICE-0051-ddf-edit-form.md#slice-0051-03) replaces the txtComp/cmbComp workaround and connects typed text to saving. The draft's WinForms reflection / Control.WmCtlColor attribution remains a historical discrepancy in 0051-02; the implementation record describes the explicit WndProc response. Public contract: [KBotComboBox](../src/KBot.Controls/Combo/KBotComboBox.md). Live DDF saving remained unproven.

## The FOREXE upload of a fundamentation document

0051 plan §10.4: Access saved the document and the operator uploaded it manually through FOREXE; 0051 ports saving only. `src/KBot.App/Views/Ddf/IDdfForexeUpload.vb` is a one-method seam whose implementation throws `NotImplementedException`. Playwright work belongs in `KBot.Forexe` beside `ForexeRunner`.

**Undecided:** upload synchronously from the save handler, blocking until the portal answers, or queue it like `ExcelJob`?

<a id="rezervariview-has-no-refresh-entry-point"></a>

## Rezervări refresh — concrete gap closed, common contract undecided

[0051](worklog/SLICE-0051-ddf-edit-form.md#slice-0051-baza), Known gaps, records Reincarca and DupaScriereaDdf after saving: the active view refreshes; an inactive one reloads on activation. The stale-row gap is closed. A common refresh interface, replacing one-off methods added for each write path, remains undecided.

## RULE 0 across the older files

RULE 0 reserves diacritics for operator-visible strings and says older conflicting code gets swept. The pre-0051 tree still has Romanian comments with diacritics: `ApiClient.vb`, `IApiClient.vb`, `MainForm.vb` and `DdfView.vb` alone contain several hundred lines.

0051 left them untouched because an unrelated 400-line comment sweep would bury the feature diff. **Undecided:** sweep them together in a separate commit, then mechanically detect diacritics outside string literals?

## Nothing guards the `AutoScaleDimensions` stamp at build time

A form's font and `AutoScaleDimensions` are one measurement: WinForms scales child rectangles and ClientSize by their ratio. A stamp from another font or DPI silently resizes the launched window. 0052 found all application windows at 90% authored width and 88% height, with no designer indication or build diagnostic.

Visual Studio re-stamps and rewrites coordinates when a form is saved at another display scaling. That is internally correct but permits drift across machines/sessions. Commit `ab69142` is titled “designer re-save at 150% DPI”; seven files still carried comments claiming 96 dpi despite their stamps.

**Undecided:** a build check accepting only Calibri 9 stamps `(6F, 14F)` at 96 dpi, `(8F, 18F)` at 120 and `(9F, 22F)` at 144. It would detect the observed wrong-font stamp; it cannot prove coordinates match the stamp and would not catch every mismatch.
