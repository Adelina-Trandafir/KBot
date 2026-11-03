# SLICE-VC06-36 — Denumirea din factura primită: selectabilă și copiabilă

Data: 03.11.2026.

## Cerere
Caseta cu denumirea produsului din eFactura (coloana «Din factura primită») să poată fi deschisă cu mausul, fără a fi editabilă, ca să se poată copia selectiv din ea.

## Ce s-a schimbat
- `PYTHON/static/js/dgv/editing.js`: opțiune nouă de coloană `readOnlyEditor` — la clic se deschide caseta de text cu `readOnly`; valoarea nu se schimbă, deci nu se apelează `onCellSave`. `_canTraverse` (= `canEdit` fără coloanele `readOnlyEditor`) înlocuiește `canEdit` la deplasarea cu Tab/Enter/săgeți și la alegerea primei celule de editat, ca acea coloană să nu fie oprire.
- `PYTHON/static/js/dgv/datagrid.js`: Enter fără coloană activă folosește `_canTraverse`.
- `PYTHON/static/js/vercon/docs.js`: coloana `ef_name` primește `readOnlyEditor: true` și `editable` pe rândurile NIR-ului în editare (nu mai este în `lineSpecs` ca editabilă, deci nu intră în validare sau salvare).
- Ajutor: `PYTHON/static/help/vercon.json`, `nir-efactura` (tag VC06-36).

## Teste

Testat de Adelina în aplicație: merge. Fișierele pentru server sunt înregistrate (SLICE-VC06-36). Niciun test automat.

## De știut

- La clic, tot textul e selectat de la început (comportamentul editorului); selecția parțială cere un al doilea clic sau tragere.
- Grila comună `dgv` e folosită și de ADECHIT; fără `readOnlyEditor` nu se schimbă nimic.
