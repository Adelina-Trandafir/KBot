# SLICE-0128-01 - Vederea «Receptii»: prima afisare si decalajul spre dreapta (05-06.11.2026)

Operator, 05.11.2026: in vederea «Receptii» a facturilor primite bara «Receptie noua / Receptii existente» aparea suprapusa la prima afisare si decalata spre dreapta. Formularul incorporat de atunci a fost eliminat in KE21 (vezi [KE21-02](SLICE-KE21-02-view-recepti-comun.md)); raman doua reparatii in controale generale:

1. **`KBotNavList`**: butoanele cu `AutoSize` erau masurate cu metricile unui parinte inca ascuns. Bara isi recalculeaza acum asezarea la `OnVisibleChanged`, `OnHandleCreated` si `OnDpiChangedAfterParent`.
2. **Capcana `KBotTableLayoutPanel`**: o coloana Absolute scrisa direct in `ColumnStyles` nu ramane 0 cat un copil ascuns cere lungime (tabelul o reface din «masura logica + surplus cerut de copil»). O coloana sau un rand care trebuie sa dispara se inchide cu `SetColumnWidth(0, 0)` + `SetColumnCollapsed(0, True)` (la fel `SetRowHeight` / `SetRowCollapsed`), niciodata scriind `ColumnStyles` / `RowStyles`.

Fisiere: `src/KBot.Controls/NavList/KBotNavList.vb`. Vazut de operator 06.11.2026 («arata bine»); fara teste.
