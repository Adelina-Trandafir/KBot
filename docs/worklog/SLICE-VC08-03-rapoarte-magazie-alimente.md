# VC08-03 — rapoarte Magazie–Alimente

13.10.2026. **IMPLEMENTAT_LOCAL, NERULAT.** Scop restrâns de utilizator: Materiale/Alimente acum, rapoarte OI/MF amânate. Excluderile Coduri rămân cele trei din catalog; nu au fost investigate.

## Familii și capturi

Cele 16 capturi din `VERCON/CAPTURI_RAPOARTE/Magazie-Alimente` au fost citite vizual. Machetele locale preiau orientarea, ordinea coloanelor, grupările, casetele și semnăturile de mai jos. Similaritatea finală nu este validată: nu au fost generate PDF-uri sau deschise pagini în browser.

| Familie / raport | Referință din capturi | Implementare locală |
|---|---|---|
| Balanță produse | `Magazie!Produse!balanta_produse.png` | Landscape, 11 coloane SI/intrări/ieșiri/SF, benzi pe cont, totaluri valorice |
| Fișă magazie și furnizor; FM1-u/FM2 | `Magazie!Produse!fisa_magazie.png` | Date produs/cont/UM/preț, sold inițial distinct, 7 coloane, total lunar și general |
| Inventar | `Magazie!Produse!lista_inventar.png` | Landscape, 14 coloane, antete pe trei niveluri, câmpuri faptice/diferențe necompletate, semnături |
| Stoc final | `Magazie!Produse!stoc_final_produse.png` | 5 coloane și totaluri pe cont; cantități zero omise |
| NIR / NIR_SFP / Storno | `Magazie!Intrari!NIR.png` | Antet cu șase câmpuri, furnizor/adresă/certificare/comisie, tabel recepționat și semnături. TVA disponibil ca binding, ascuns implicit ca în captură |
| Bon consum / retur / BUConsum | `Magazie!Iesiri!Bon_Consum.png` | Antet document, cantitate necesară/eliberată, cod/UM/preț/valoare, patru semnături; BUConsum_F are două exemplare |
| Centralizator BC | `Magazie!Iesiri!Centralizator_Bon_Consum.png` | Număr/data/valoare/primitor, grupare pe cont și total general |
| Consumuri produse / unități / Fișa evidenței | `Magazie!Iesiri!Consumuri_Produs.png` | Grupare cont sau primitor, produs/UM/variantă/preț. Nu se totalizează cantități cu UM diferite |
| Furnizori / conturi-furnizori / nou_Furnizori | `Magazie!Intrari!Centralizator_Intrari.png`, `Magazie!Produse!centralizator_conturi_furnizori.png` | Factură/data/NIR/cont/valoare, subtotaluri pe furnizor/cont și total general |
| Centralizator conturi | `Magazie!Produse!centralizator_conturi.png` | Cont și patru valori SI/intrări/ieșiri/SF |
| Comandă / referate / plan / situație intrări | `Magazie!Intrari!nota_comanda.png`, `Magazie!Produse!referat_achizitie.png`, `Magazie!Produse!plan_anual_achizitii.png`, `Magazie!Produse!situatie_intrari.png` | Coloane pe familie, CPV la plan, paragrafe și spațiu pentru referat, semnături. Comanda/Plan/Situație citesc intrări, referatele includ corecția storno |
| Lista zilnică și patru subreports | `Lista_Alimente.png` | Trei rezumate alăturate, aprobări, trei mese, alimente în două coloane, recapitulare economie/depășire, total și semnături; secțiune suplimentară pentru toate BC asociate |

## Surse SQL și configurare per unitate

Sursele SELECT implicite sunt cele 11 fișiere din `PYTHON/reports/vercon/queries`: metadata, ledger, receipts, consumption, committee, food_header, food_items, food_bons, food_consumption, food_history, settings. `report_data.py` descrie aliasurile obligatorii; se pot adăuga câmpuri. Sursele comune afectează toate rapoartele care le folosesc. Nu există SQL de raport în JavaScript.

În **KbotAdmin → Rapoarte VERCON**, se alege o unitate din arbore, Citește lista, definiția, Deschide. Interogările sunt editabile ca SELECT; antetul/subsolul și identitatea au câmpuri dedicate. Salvarea SQL face validare de structură și **nu execută interogarea**. La generare, serverul înlocuiește `{{TABLE:Nume}}` cu relații delimitate la contextul autorizat. Nu sunt permise nume brute de tabele, mutații, alte baze sau parametri de context introduși de client.

Datele locale sunt în `PYTHON/instance/vercon-reports`, sau calea `VERCON_REPORT_CONFIG_DIR`. Configurarea se păstrează per bază/unitate și este exclusă de la push. Salvarea atomică verifică versiunea citită și păstrează versiunea anterioară în `history`. Snapshoturile de raport sunt comune workerilor, legate de utilizator/unitate/subunitate/gestiune/exercițiu, valabile 10 minute. Ecranul, PDF și Excel folosesc același model salvat; lățimile Excel sunt recitite la export.

## Editor vizual

`VerconReportDesignerForm.vb` și `.Designer.vb`: selectare/tragere texte, redimensionare lățime cu marginea dreaptă, adăugare/eliminare/reordonare coloane, binding/etichetă/tip/font/text îngroșat, titlu și orientare. Benzile sunt antet/subsol și texte înainte/după corp; pentru unitate doar antet/subsol. Lista de alimente expune șase tabele separate în selectorul Secțiune. Aplică în ciornă nu salvează pe server; butonul Salvează din pagina de administrare salvează definiția. Nu se editează manual JSON.

Previzualizarea editorului este schematică, fără date reale și fără paginare identică PDF. Adăugarea/eliminarea/reordonarea coloanelor șterge antetele combinate din tabelul modificat, pentru a nu păstra spanuri greșite. Editorul nu implementează expresii Access, designer de grupări/totaluri, imagini, subrapoarte arbitrare sau mutarea fiecărui element fix al corpului. Titlul și coloanele/textele se pot edita acum; pentru paritate completă Access rămâne followup-ul vizual cerut. Îngroșarea PDF folosește accentuare sintetică a fontului Unicode livrat; Excel folosește bold nativ.

## Excel comun tuturor aplicațiilor

`PYTHON/reporting/excel_widths.py` conține cheia coloanei, citirea din `AVACONT_COMUN.RAPOARTE_EXCEL_COLOANE`, adaptorul cursor și politica de lățimi. ADECHIT și VERCON îl folosesc. Algoritmul inițial ADECHIT este păstrat, inclusiv prioritate/maxim/fallback. Importurile istorice ADECHIT sunt reexportate; VERCON nu depinde de pachetul ADECHIT.

Contract VERCON: `NUME_RAPORT = VERCON_` + ID majuscul, caracterele speciale devin `_` (`FM1-u` → `VERCON_FM1_U`). Cheile sunt câmpuri majuscule, duplicările au `_2`, `_3` etc. (`QUANTITY`, `QUANTITY_2`). Configurația Excel nu este lățimea relativă din macheta PDF. Exemple SQL sunt pregătite în `sql/VC08_03_rapoarte_excel_coloane.sql`, fără execuție și fără DDL.

## Limite și continuare

Nicio verificare executată: fără teste, build .NET, import Python, browser, generare PDF/Excel sau acces MariaDB. Nu există rezultate de paritate pe date reale. Dependențele de server sunt în `PYTHON/requirements-reporting.txt`. Nu s-au creat/modificat scheme. Stocul este evaluat la prețul curent al variantei; liniile NIR/BC păstrează valoarea istorică, BR/Storno derivă explicit valoarea din cantitate și preț. Fișa pe furnizor selectează variantele acelui furnizor, păstrează toate mișcările lor și nu inventează alocare FIFO.

Rămân acceptarea vizuală și corective followup după rularea autorizată de utilizator. VC08-04 (închidere/redeschidere) și VC08-05 (comparator real Access/import/web) nu sunt implementate prin acest slice; VC08 în ansamblu nu este declarat încheiat.
