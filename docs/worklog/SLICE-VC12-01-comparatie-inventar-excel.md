# SLICE-VC12-01 — Comparație inventar Excel cu VERCON

Data: 06.11.2026.

## Scop și stare

Implementare în surse a funcției KBOTADMIN **Publicare → VERCON → Compară inventar Excel**. Pregătește revizuirea pozițiilor înaintea unui eventual import. Nu implementează și nu execută importul. Nu modifică datele sau schema MariaDB.

Fișierul furnizat, `INVENTAR-31 DEC 2025.xlsx`, a fost citit pentru analiza machetei, fără modificare. Foaia `scoala 2025` are 583 de poziții: 8 MF și 575 OI. Are antete repetate, totaluri, declarații și semnături; acestea nu sunt poziții de comparat. Fișierul sursă și datele sale nu sunt copiate în proiect.

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Sunt înregistrate toate cele 12 fișiere proprii din lista de mai jos, inclusiv documentația, cu motivul acestui slice. Agentul nu a compilat, testat, executat SQL, publicat sau executat Git. Citirea sursei și analiza codului au fost folosite pentru proiectare.

## Utilizare

1. În KBOTADMIN selectați o singură bază în arbore. Deschideți **Publicare → VERCON → Compară inventar Excel**.
2. **Alege Excel…**, alegeți fișierul. Selectați foaia; `scoala 2025` este preferată când există. **Citește foaia**.
3. **Citește destinațiile**, apoi alegeți **Gestiune / Subgestiune / Cont**, conform terminologiei cerute de Adelina. Acestea se mapează pe `VC_Subunitati.SubunitId`, `VC_Gestiuni.GestiuneId`, respectiv `VC_Masini.IdMasina` (categoria/contul produsului, nu locul sau primitorul din registrul OI).
4. **Compară**. Din Excel se folosesc numai pozițiile MF sau OI de tipul subgestiunii selectate. Sumarul arată și numărul pozițiilor de alt tip omise.
5. Selectați un rând din grila principală pentru primii cinci candidați, prețurile, diferențele și explicațiile. Filtrele și sortarea sunt disponibile în coloane. **Salvează CSV…** păstrează toate rezultatele comparației, cu până la cinci candidați per rând, indiferent de filtrele vizuale.

## Date și potrivire

- Reader XLSX local prin ZIP/XML .NET, fără Excel instalat, ACE sau biblioteci noi. Este dedicat machetei inventarului furnizat: număr curent A, denumire B, cod C, UM D, cantitate scriptică F, preț unitar I, tipul secțiunii din antetul K/L. Alte machete necesită adaptare explicită. Numărul fizic al rândului rămâne referința rezultatului. Nu se recalculează formule; celulele cu formule folosesc valoarea memorată în XLSX. Lipsa prețului memorat apare ca preț incomplet.
- Se compară exclusiv rândurile existente în `VC_ProduseP` dintr-un singur cont, în aceeași bază/subunitate/gestiune, într-o tranzacție numai-citire. `VC_Produse` furnizează denumirea, UM și contul prin JOIN; produsele fără rând în `VC_ProduseP` sunt excluse. Variantele inactive rămân marcate în explicație. Această limitare corectează implementarea inițială, conform cererii Adelinei din [SLICE-VC12-03](SLICE-VC12-03-comparatie-exclusiv-produsep.md). Nu se caută implicit în alte conturi sau gestiuni.
- Denumirile sunt comparate fără diacritice, majuscule sau punctuație, folosind asemănarea caracterelor în ordinea originală și cu cuvintele reordonate. Nu se folosește un serviciu AI extern. Abrevierile sunt evaluate textual, fără un dicționar inventat de sinonime.
- Codul/numărul de inventar identic are prioritate la ordonarea candidaților; zerourile inițiale și punctuația codului sunt păstrate. Coduri diferite, UM diferite/incomplete, modele sau dimensiuni numerice diferite și inactivitatea rămân vizibile.
- Scorul denumirii este 0–100. Dacă ambele prețuri sunt cunoscute, scorul total este 80% scor denumire + 20% apropiere relativă a prețurilor. Fără ambele prețuri se afișează scorul numelui. **Scorul este orientativ, nu o probabilitate.** Prețul singur nu produce candidat: este necesar cod identic sau scor nume de minimum 40.
- Prețurile sunt `Decimal`; sunt acceptate virgulă/punct și separatoare mixte. VERCON folosește direct `VC_ProduseP.PUTVA` (preț cu TVA), fără multiplicare cu TVA ori cu scara gestiunii. Egalitatea de preț are toleranță absolută 0,01 lei. Diferența afișată este VERCON minus Excel.
- Concordanța puternică cere scor nume de minimum 90, preț egal, UM egală, produs/variantă activă și lipsa conflictelor de cod/model; rămâne „de revizuit”. Candidații apropiați la mai puțin de 5 puncte, codurile duplicate și același candidat principal pentru mai multe poziții sursă sunt marcate ambiguu. Nu se atribuie automat o variantă unui rând și nu se declară automat produs nou.
- Cantitatea scriptică din Excel este afișată pentru revizuire. Nu se calculează stocul VERCON la 31.12.2025, nu se compară cantitățile cu registrul de folosință și nu se deduce existența în stoc din existența în nomenclator. Orice astfel de extindere și importul necesită un scop discutat separat.
- Textul se ordonează în limba română, numerele ca numere, cu sensul ales păstrat la reumplerea grilelor. Cultura este limitată la grilele acestei funcții. Ordinea implicită a candidaților este rangul calculat de potrivire, cu departajare alfabetică/numerică; ID-urile nu sunt chei de sortare implicite.

## Transport și limite

Excelul rămâne local. Numai pozițiile de bunuri sunt trimise ca JSON prin SFTP, cu drepturi doar pentru utilizator, la `/tmp/kbot-inventory-<guid>.json`; numele persoanelor și semnăturile nu intră în payload. CLI acceptă strict această cale temporară, verifică tipul/proprietarul/dimensiunea fișierului și îl șterge la consum. Clientul cere curățarea și dacă apelul CLI eșuează. O eroare de curățare este raportată.

Limită: 5000 poziții / 2 MB per comparație; componentele XML citite au limită 25 MB. Nu sunt introduse fișiere de configurare sau tabele noi. Export CSV UTF-8 cu BOM, separator `;`, prețuri VERCON numerice cu virgulă și protecție pentru interpretarea textelor drept formule. Exportul se produce numai la alegerea explicită din interfață.

## Fișiere proprii ale taskului

- `KBotAdmin/MainForm.vb`
- `KBotAdmin/MainForm.Designer.vb`
- `KBotAdmin/Server/InventoryExcel.vb`
- `KBotAdmin/Views/InventoryCompareView.vb`
- `KBotAdmin/Views/InventoryCompareView.Designer.vb`
- `KBotAdmin/Views/InventoryCompareView.Export.vb`
- `PYTHON/routes/admin_cli/admin_cli.py`
- `PYTHON/routes/admin_cli/vercon_inventory.py`
- `PYTHON/routes/admin_cli/inventory_similarity.py`
- `docs/worklog/SLICE-VC12-01-comparatie-inventar-excel.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

Pentru utilizarea funcției trebuie compilat KBOTADMIN și publicate modulele Python aferente prin fluxul autorizat. Nici compilarea, nici publicarea nu au fost executate în acest task.
