# SLICE-00EF-36 - Received invoices: the view «XML Factura» with the new control XmlViz (15.10.2026)

Slice number: the operator wrote «SLICE-01EF» for the work that imports from the editor; there is no 01EF slice anywhere, so it is
read as the E-Factura slice (00EF), next free number 00EF-36 (to be corrected if 01EF was meant). The file name keeps «in-editor» from
the first version.

## What changed and why
**First version (same day, REVERTED at the operator's word «NU e bine»)**: the FOREXE workflow editor was imported into
`KBot.Forexe.Editor` and opened in a window of its own with a button of the bar. Taken out again: the ten imported files,
`wfl_help.md`, the project / README edits and the launcher are gone; `src/KBot.Forexe.Editor` is as in HEAD (an empty project).
**Now**: a new control, written from the ideas of that editor (the map, the colouring), made for the e-invoice:
1. **`XmlViz`** (`KBot.Controls/Xml/XmlViz.vb` + designer): left a MAP (`AdvancedTreeControl`), right the text (a read-only
   `RichTextBox`). No help panel, no validation, no editing, no autocomplete, no window of its own.
   - **Map**: one node per element, named WITHOUT prefix (`cbc:IssueDate` -> «IssueDate»; the prefix only says value / aggregate, which the
     expander and the bold name already say); a value element shows its text on the right (`~~~`, cut at 60 characters). A click selects
     the element in the text.
   - **Text**: written by the control from the parsed XML (two spaces per level, one element per line) as RTF, so the position of every
     element is known and the colours cost one assignment. Colours are the theme palette's (punctuation, element, attribute name,
     attribute value, text, comment) and follow a theme change. A value longer than 400 characters (the base64 of an attachment)
     is cut with the count of what was left out. A text that is not well-formed is shown as it came, with the reason.
   - **Romanian names (follow-up, same day)**: the map shows the names in Romanian (`XmlVizLabels`, ~180 UBL / CIUS-RO names, looked up by
     parent + name first: `ID` = «Număr factură» under the root, «IBAN» under `PayeeFinancialAccount`, «Nr. linie» under a line;
     `Party` = «Date furnizor» / «Date client» by its parent); an element not in the list keeps its name without prefix; the
     file's own name (with prefix) is in the tooltip of the node. The text on the right is untouched. Not used: the XSD (it has no
     Romanian). The list was written from the UBL 2.1 / CIUS-RO element names by hand: **to be read by someone who knows the
     business terms**, a wrong word is a one-line fix in `XmlVizLabels.vb`.
2. **The view**: item `xml` «XML Factura» (`Align = Far`) in `navDetaliu` of `PrimiteView`, visible while an invoice is shown; page
   `pgXml` (`XmlViz`) in `pnlPages`; the XML (`GetPrimitaXmlAsync`, UTF-8) is read when the view is shown, once per invoice
   (`RefreshXml`, with a stale-answer guard).

## Files touched
`src/KBot.Controls/Xml/XmlViz.vb`, `XmlViz.Designer.vb` (new), `src/KBot.EFactura/Views/Primite/PrimiteView.vb`,
`PrimiteView.Designer.vb`, `contabil/efactura/index.md` (0000-81). Nothing else remains of the first version.

## Test results
`dotnet build src/KBot.App` (builds Controls and EFactura): 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

## Unverified / deferred
- Nothing seen: the colours in the three themes, the map on a real invoice (hundreds of nodes), `RightTextWidth` of the map for the
  values, the click-to-select (it relies on the RichTextBox counting a paragraph break as ONE character).
- The XSD was not needed: the rule «prefix = name without it» is enough. With the XSD the map could show the Romanian / standard labels
  of the business terms (BT-xx); not done, not asked.
- `XmlViz.md` / `CONTROLS.md` line: not written. No search box in the map, no way to resize the map / text split.
- The edit of an attachment's base64 is NOT shown (cut); «Salvează ca XML» of the tree menu still gives the exact file.

## Follow-up (15.10.2026): the map starts expanded at the root only
`XmlViz`: `pExpanded` is true only for the root element (depth 0); every other node starts collapsed.
