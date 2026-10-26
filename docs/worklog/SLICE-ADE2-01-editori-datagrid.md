# SLICE-ADE2 — Editare DataGrid: contract, salvare și probă inițială

Etape ADE2-01…03 din 08.10.2026. TESTAT, RULAT, PUBLICAT conform operatorului la 23.10.2026. Mențiunea inițială despre regresiile rămase nu consemnează o probă ulterioară; [registrul ADE2](state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2) păstrează confirmarea matricei complete. Interacțiunile adăugate ulterior sunt în [ADE6](SLICE-ADE6-01-cataloage.md), inclusiv clic PC, Enter-next și Escape numai în editor.

- [ADE2-01](#slice-ADE2-01)
- [ADE2-02](#slice-ADE2-02)
- [ADE2-03](#slice-ADE2-03)

## SLICE-ADE2-01 — editori DataGrid <a id="slice-ADE2-01"></a>

Editorii text/număr/dată/listă sunt opt-in. Selecția nu mai rerandează ținta între cele două clicuri; regresia descoperită în proba browser a fost reparată.

## SLICE-ADE2-02 — salvare și validare <a id="slice-ADE2-02"></a>

Enter/Tab/Escape, salvarea asincronă, conflictul de versiune, mesajele de eroare și emiterea evenimentului sunt implementate. Testele API verifică refuzurile.

## SLICE-ADE2-03 — probă browser <a id="slice-ADE2-03"></a>

Pe localhost:5050, `ZilePrezenta` s-a salvat 10→12 prin dublu clic + Enter. „Situație” a rămas fără editor la Enter și dublu clic. Mai rămân regresiile extinse pe seturi virtualizate mari și Combobox.
