# Plan pentru plăți din Contări și operații contabile Avacont

`SLICE-04EF-01` acoperă plata directă din **FacturiCumparare → Contări** și pregătirea generării OP-ului asociat. Datele comune pentru plăți, OP și OPME țin de `SLICE-AV02-02`. Formularul Avacont va folosi același serviciu pentru OPuri2017 și va găzdui și OP9. Acest plan detaliază [etapa 2 a portării Avacont](PLAN_AVACONT.md#etapa-2---introducere-date-fara-salarii-burse-extrase).

## Plata directă din FacturiCumparare → Contări

1. Operatorul selectează facturi ale aceluiași furnizor și poate salva **doar evidențierea**, **doar plata** sau **evidențierea și plata**. La plată sunt eligibile doar facturile neachitate sau achitate parțial. Pentru o factură deja evidențiată, plata se poate salva fără un șablon de evidențiere.
2. Afișăm distinct **rămas de evidențiat**, **rest de plată** și **sumă de plată**. Restul se calculează din evidențierile existente și sumele alocate plăților, inclusiv cele importate. În salvarea combinată se iau în calcul și evidențierile care se postează în aceeași tranzacție.
3. Bifa **„Protectie contari Plati/Receptii”** din **Setări → Aplicație** pornește debifată în 2026. Adelina o poate bifa manual pentru utilizatorii aleși pentru probe. Pentru fiecare factură cu sumă de plată, serverul caută o recepție existentă, activă și asociată. Cu bifa debifată, lipsa recepției produce un avertisment și plata se poate salva. Cu bifa bifată, lipsa blochează salvarea plății. Evidențierea poate fi salvată separat. Regula pentru activarea automată la trecerea **2026 → 2027** este consemnată în [stadiul portării Avacont, la „Închidere anuală”](../worklog/state/KBOT_STATUS_AV.md#închidere-anuală).
4. Serverul recitește soldurile în tranzacția de salvare și verifică recepțiile și sumele alocate. Postează operația de plată, debitul, creditul și alocarea pe fiecare operație de evidențiere. Dacă este o salvare combinată, postează și evidențierile în aceeași tranzacție. O reluare a aceleiași cereri nu trebuie să dubleze plata. După salvare, Contări afișează operația și soldul actualizat.
5. După salvarea operației de plată, Contări oferă **Generează ordinul de plată** pentru operația salvată. Fluxul normal de emitere se aplică și plăților salvate fără recepție în 2026 cu bifa de protecție debifată. Când există recepția, OP-ul urmează recepției și documentului de ordonanțare aferent. Referința contabilă directă a OP-ului este exclusiv **operația de plată**. Un OP simplu poate rămâne independent sau poate fi adăugat ulterior într-un OPME. Fluxul din KBotForm care pornește de la extrase este separat.

În codul actual, `Salvează` cere un șablon de evidențiere și postează doar evidențierea. Se extind pagina, cererea de salvare și serviciul de postare existente. Asocierea facturii cu recepția se citește din `EF_PrimiteReceptii`, folosind regulile existente pentru recepțiile active și legăturile istorice prin `IDRH`. [Contări](../../src/KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb#L238) · [Salvarea pe server](../../PYTHON/routes/efactura/primite_contari.py#L401) · [Legăturile cu recepțiile](../../PYTHON/routes/efactura/primite_ke.py#L110)

### Regula OP · plată · operație contabilă

- **Un OP = o plată = o operație contabilă** (`401 = 770` în cazul clienților). O plată către același furnizor pe mai multe facturi dă **un singur OP** cu suma totală. Alocările pe facturi rămân linii separate în `PlatiFacturi`.
- **Plată suplimentară pe aceeași factură.** Cât timp OP-ul nu este blocat (vezi mai jos), operatorul poate fie să adauge o a doua operație de plată cu un al doilea OP independent, fie să completeze prima plată.
- **OP blocat** = confirmat prin `FX_Extrase` sau adăugat într-un OPME. De atunci, nici operația de plată, nici OP-ul nu se mai pot edita sau șterge.
- **Editare și ștergere** sunt permise doar pe un OP neblocat. La ștergerea unei plăți neblocate se șterge și ordinul de plată asociat.
- **Refolosirea numerelor.** Un OP șters eliberează numărul, iar următoarea emitere poate lua același `MAX + 1`. Asta este sigur, pentru că doar OP-urile neblocate se pot șterge, iar cele ajunse în extras sau OPME rămân cu numărul lor.

### Numerotarea OP

- Setarea **`PlajaComunaOPuri`** (True/False) hotărăște unde se caută ultimul număr de OP. **ON:** serverul caută cel mai mare `NrOP` din `OPuri` în toate subunitățile. **OFF:** caută doar în subunitatea curentă. Unele subunități pot avea setarea ON și altele OFF. Setarea este comună tuturor operatorilor, deci **nu** stă în `app_settings.json` (acela e per utilizator Windows).
- Setarea se păstrează în tabelul nou **`TZ_Settings`**, după modelul `VC_Settings`: loc unic al setărilor contabilității la nivel de bază de date (în `AVACONT_SURSA`). Are o coloană **`IdSubunitate` NULLABLE**: dacă e completată, setarea se referă la acea subunitate; dacă e `NULL`, se referă la toată unitatea. `PlajaComunaOPuri` se pune pe subunitate. Are `CreatedAt` și `UpdatedAt`, ca orice tabel nou, și colațiunea `utf8mb3_general_ci`. DDL-ul se livrează ca fișier SQL pentru aplicare de către Adelina. `Setari` (`Cheie`, `Valoare`) rămâne cum este: are doar citire în API. Cheia primară este un id propriu, pentru că `IdSubunitate` poate fi `NULL`. Unicitatea o dă o coloană generată `IdSubunitateCheie = IFNULL(IdSubunitate, 0)`, cu index unic pe (`SettingKey`, `IdSubunitateCheie`), astfel încât baza de date refuză singură două rânduri pentru aceeași setare și aceeași subunitate (sau două rânduri la nivel de unitate). Citirea: rândul subunității, iar dacă lipsește, rândul cu `NULL`, iar dacă lipsește și acela, valoarea implicită. Pentru `PlajaComunaOPuri` valoarea implicită este **FALSE** (OFF).
- **Numărul provizoriu.** Cât timp operatorul lucrează, numărul OP apare în fiecare DGV în care este folosit (grila OP din Contări, grila OP din OPuri2017, listele de OP). Serverul îl calculează ca `MAX(NrOP) + 1` conform `PlajaComunaOPuri` și îl trimite aplicației. Celula are tooltip: numărul final poate fi altul, dacă între timp altcineva a emis un OP conform aceleiași reguli.
- **Numărul final.** La salvare, serverul recitește `MAX(NrOP)` în tranzacție și alocă `MAX(NrOP) + 1`, astfel încât două emiteri simultane conform aceleiași reguli să primească numere distincte. Serverul întoarce numărul alocat în răspunsul de salvare, iar aplicația înlocuiește numărul provizoriu din grilă cu cel real, ca operatorul să vadă numărul sub care s-a emis OP-ul.

[Numerotarea comună prin LEGUNIT.OP în Access](../../AVACONT/EXPORT_VBA/Modules/mdl_2025_code.txt#L285)

## Date comune și migrarea din Access

| Sursă Access | Rol în K-BOT |
| --- | --- |
| `PlatiFacturi` | O alocare pe factură. `IdPlata` indică operația de plată. `IdFactura` sau `IdFacturaAA` indică operația de evidențiere din anul curent sau din anul anterior. `Platit` este suma alocată, distinctă de totalul OP. Pentru e-facturi, `EFT_O` leagă factura primită de evidențiere. |
| `OPuri` | Un ordin individual, legat de operația de plată prin `Oper`. Păstrează numărul, data, suma, explicația și datele de emitere. Legătura cu un lot OPME este opțională. |
| `OPME` | Antetul lotului de OP-uri. `NrRec` este numărul ordinelor din lot. Recipisa se păstrează separat în K-BOT. Exportul Access grupează ordinele prin `IDOPME` în XML `f1129`. |

- **Istoricul se importă ca atare.** Se păstrează sumele, diferențele, datele, numerele de document și valorile originale ale cheilor Access. Migratorul mapează legăturile către cheile K-BOT în contextul unității și anului sursă. Semnalează nepotrivirile fără să recalculeze sau să rescrie datele sursă. Migratorul nu verifică existența recepției și nu aplică protecția din Contări.
- În copia analizată, cele 7 alocări și cele 7 OP-uri se potrivesc prin `PlatiFacturi.IdPlata = OPuri.Oper`, cu sume concordante. Niciun `PlatiFacturi.IDOP` nu găsește OP-ul, iar numărul OP coincide doar în 3 dintre cele 7 cazuri. O alocare trimite la o factură din anul anterior. Cele 7 OP-uri sunt independente. La migrare, folosim operația de plată pentru a asocia un OP când potrivirea este unică. Cazurile ambigue se raportează. Pentru facturile migrate, restul de plată devine utilizabil după reconcilierea alocărilor istorice.
- **`PlajaComunaOPuri` la migrare.** `AVC.Migrator` o citește din Access, din `Scheme`, rândul `NumeForm = '_OP9'`, coloana `C6`. Ea se copiază în `LEGUNIT.OP` ([ACTUALIZEAZA_LEGUNIT_2025_OP](../../AVACONT/EXPORT_VBA/tabledata/RUNONCE_LOCAL.csv)), iar `ObtineIDURI` caută ultimul `NrOP` în fișierele cu `LEGUNIT.OP = True` ([mdl_2025_code.txt#L353](../../AVACONT/EXPORT_VBA/Modules/mdl_2025_code.txt#L353)). Valoarea se scrie în `TZ_Settings`, pe subunitatea portată din unitatea Access respectivă (`TZ_Subunitati.IdUnitateAccess`). Lipsa rândului sau `C6 = 0` înseamnă OFF, adică implicitul. *De verificat la migrare:* în Access plaja comună se întindea doar peste fișierele marcate în `LEGUNIT`. În K-BOT, ON caută în toate subunitățile. Migratorul raportează cazurile în care cele două nu coincid.
- Modelul comun păstrează separarea dintre evidențiere, alocarea plății, OP și lotul OPME. DDL-ul se livrează ca fișier SQL pentru aplicare de către Adelina.

[Calculul Access pentru facturi evidențiate](../access-map/efactura/queries.md#qef_pl-append) · [Salvarea alocărilor OPuri2017](../../AVACONT/EXPORT_VBA/queries/SAV_PlatiFacturi_2017.sql) · [Datele de emitere OP](../../AVACONT/EXPORT_VBA/queries/SAV_OP_2017.sql) · [Exportul OPME](../../AVACONT/EXPORT_VBA/Modules/ExportXML_code.txt#L81)

## Formularul Avacont pentru operații contabile

Formularul se numește **`OperatiiSabloane`**. Un formular principal găzduiește paginile **Operații diverse** și **Facturi și plăți**. Are `KBotNavList` vertical în stânga, după modelul **Șabloane**, și pagina activă în dreapta. Folosește anul și subunitatea de lucru și tratează editările nesalvate la schimbarea paginii sau la închidere. **Salarii** se adaugă în pasul următor ca a treia pagină. Grilele și arborii folosesc aspectul implicit al controalelor comune și sortează după tipul real al valorii. [Modelul Șabloane](../../src/KBot.App/Avacont/Configurari/Sabloane/Views/SabloaneForm.vb)

### Operații diverse · OP9

- Lista de șabloane `Tip=A` este căutabilă. Antetul conține nota contabilă, numărul, felul și data documentului, partenerul unde este necesar și suma. Nota se alege dintre cele existente sau se introduce un număr nou.
- Suma din antet se copiază pe fiecare linie a șablonului și poate fi ajustată pe rând. Grila arată jurnalul, clasificația, explicația, conturile și partenerii pe debit și credit. Liniile nenule se postează împreună prin serviciul existent, câte un document `Fel=A` și o operație `Tip=A` cu debit și credit pentru fiecare linie. Se validează conturile, data de lucru și echilibrul înainte de salvare.

[Rutina OP9 din Access](../../AVACONT/EXPORT_VBA/Forms/OP9_code.txt#L338) · [Serviciul de postare](../../PYTHON/routes/avacont/introducere_date/operatii/postare.py)

### Facturi și plăți · OPuri2017

- Reutilizează **pagina Contări din FacturiCumparare** (`FacturiCumparareContarePage`), **fără arborele de facturi**. Pagina găzduită în Avacont adaugă selectorul de partener, iar documentele se introduc sau se selectează direct în grilă. Păstrează selectorul de clasificație, listele de șabloane `Tip=M` cu `Fel=E` sau `Fel=P`, datele beneficiarului și grila OP.
- **Facturi noi** permite introducerea manuală și, opțional, plata. **Facturi din anul curent** și **Facturi din anul anterior** permit selectarea facturilor deja evidențiate și neachitate integral. Pentru anul anterior se folosesc legăturile istorice și soldul reportat. Suma de plată poate acoperi integral sau parțial restul disponibil.
- Evidențierea și plata folosesc serviciul contabil comun cu FacturiCumparare. Verificarea recepției prin bifă este stabilită pentru salvarea plăților noi din FacturiCumparare → Contări.

[OPuri2017 din Access](../../AVACONT/EXPORT_VBA/Forms/OPuri2017_code.txt#L650) · [Pagina Contări](../../src/KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb)

## Ordine de lucru propusă

1. **`SLICE-AV02-02`** — modelul comun, `TZ_Settings`, DDL-ul și maparea istoricului Access pentru alocări, OP și OPME.
2. **`SLICE-04EF-01`** — bifa, salvarea directă din FacturiCumparare → Contări, afișarea plății salvate și acțiunea de generare a OP-ului asociat. [Worklog](../worklog/SLICE-04EF-01-plati-contari.md)
3. **Avacont, etapa 2** — formularul `OperatiiSabloane` cu navigare verticală și paginile OP9 și OPuri2017. În etapa următoare se adaugă pagina Salarii.
