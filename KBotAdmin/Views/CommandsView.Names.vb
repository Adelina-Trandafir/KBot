Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' Command names, block titles, the list roots and the «what to expect» texts of the command list (slice ME06-02: split out of CommandsView.vb).
''' </summary>
Partial Public Class CommandsView

    Private Const MaxColumnWidth As Integer = 60

    ' Repair buttons use the count in each check's summary, tied to the selected command/database.
    Private Const SignCheckName As String = "oi_semne"
    Private Const SignRepairName As String = "oi_semne_repara"
    Private Const SignBlockTitle As String = "Semne diferite"
    Private Const RepairableColumn As String = "cu cantitate negativa"
    Private Const StornoCheckName As String = "storno_preturi"
    Private Const StornoRepairName As String = "storno_preturi_repara"
    Private Const StornoBlockTitle As String = "Preturi storno si retur VERCON - sumar"

    ' The FX_DDF check finds, the «Explică» button (and the code box) explains ONE code. Both only read.
    Private Const DdfCheckName As String = "ddf_antete"
    Private Const DdfExplainName As String = "ddf_antete_explica"
    Private Const DdfRepairName As String = "ddf_antete_repara"
    Private Const DdfBlockTitle As String = "Documente de fundamentare (FX_DDF) - sumar"
    Private Const DdfCountColumn As String = "cazuri"
    Private Const DdfConclusionTitle As String = "Concluzie pentru "
    Private Const DdfRepairableMark As String = "REPARABIL"

    ' The reception-chain check finds, the «Repară recepțiile» button repairs ONE code (two steps, no explain in between:
    ' the check's detail already says what each repairable case will do).
    Private Const LanturiCheckName As String = "receptii_lanturi"
    Private Const LanturiRepairName As String = "receptii_lanturi_repara"
    Private Const LanturiBlockTitle As String = "Lanturi de receptii (instantanee) - sumar"
    Private Const LanturiRepairableColumn As String = "reparabile"

    ' Duplicate Clasificatii: the check lists them, «Repara» deletes the ones no other table holds (the repair command only exists behind the button).
    Private Const ClsfDupCheckName As String = "clasificatii_dubluri"
    Private Const ClsfDupRepairName As String = "clasificatii_dubluri_repara"
    Private Const ClsfDupBlockTitle As String = "Clasificatii dublate - sumar"

    ' Bank statements the XML of FX_Extrase_F holds but FX_Extrase_H / FX_Extrase do not (slice ME04-03): the check lists them, «Repară» writes them.
    Private Const ExtrCheckName As String = "extrase_lipsa"
    Private Const ExtrRepairName As String = "extrase_lipsa_repara"
    Private Const ExtrBlockTitle As String = "Extrase lipsa - sumar"

    ' The wipe of a wrongly migrated VERCON database (slice ME01-16): the typed value must be the database name.
    Private Const WipeCommandName As String = "vercon_golire"
    ' Archives and deletes the documents annulled before VC14-04 (they leave the trees, their numbers are free); the typed value must be the database name.
    Private Const ArchiveCancelledName As String = "vercon_arhiva_anulate"
    ' The same wipe for the accounting data of Avacont (TZ_ + the links of the invoices to their operations): the typed value must be the database name.
    Private Const AvacontWipeCommandName As String = "avacont_golire"
    Private Const DeleteBcName As String = "vercon_bc_sterge"
    Private Const PreviewDeleteBcName As String = "vercon_bc_analiza_stergerii"
    Private Const DeletedBcTitle As String = "Bon de consum șters"
    ' SLICE-VC22-01: complete deletion of the PTC bons (BP/BT/BS + OI register), the consumption bons and the NIR of ONE OI/MF gestiune. «Rulează» on
    ' the analysis command (the only one listed) analyses; when the server finds nothing against the deletion it returns a confirmation token and the
    ' same button becomes «Rulează ștergerea», which confirms and sends that token to the deletion command (hidden). Any change of command, database or
    ' typed value puts the button back to «Rulează».
    Private Const OiWipeAnalysisName As String = "vercon_oi_analiza_golirii"
    Private Const OiWipeName As String = "vercon_oi_golire"
    Private Const OiWipeRunText As String = "Rulează ștergerea"

    ' Deleting ONE angajament with everything the FX_ tables hold (EF_ untouched); the typed value is the code.
    ' The analysis (read only) finds what would go; «Șterge angajamentul» deletes it for that code (the delete command only exists behind the button).
    Private Const AngAnalysisName As String = "angajament_analiza"
    Private Const DeleteAngName As String = "angajament_stergere"
    Private Const AngAnalysisBlockTitle As String = "Stergere angajament - analiza"
    Private Const AngAnalysisCountColumn As String = "randuri de sters"

    ' Commands that only exist behind a button, never in the list.
    Private Shared ReadOnly HiddenCommands As New HashSet(Of String)(StringComparer.Ordinal) From {DdfExplainName, DdfRepairName, ExtrRepairName, LanturiRepairName, ClsfDupRepairName, DeleteAngName, PreviewDeleteBcName, MoveAnalysisName, MoveName, OiWipeName, PartnerListName}

    ' ADECHIT (slice ME05-01): moving ONE whole payment to another child. «Rulează» on the list command takes the source child (box 1) and the receiving
    ' child (box 2) and lists the source child's payments; choosing one runs the analysis (shows what would be done, writes nothing); «Mută plata» only
    ' exists after an analysis that found nothing against the move, tied to the exact payment and child analysed. The analysis and the move are hidden commands.
    Private Const MoveListName As String = "adechit_plata_lista"
    Private Const MoveListTitle As String = "Mutare plata intre copii - plati"
    Private Const MoveAnalysisName As String = "adechit_plata_analiza"
    Private Const MoveName As String = "adechit_plata_muta"
    Private Const MoveBlockTitle As String = "Mutare plata intre copii - analiza"
    Private Const MoveBlockedMark As String = "NU SE POATE MUTA"

    ' ADECHIT: everything the unit database holds about ONE child, asked by CNP (slice ME03-01); read only, several blocks.
    Private Const AdechitChildName As String = "adechit_copil"

    ' AVACONT: the invoices and payments of ONE CodFiscal per SS, from the accounting operations (slice ME06-01). The CodFiscal is not typed: it is chosen in
    ' cmbPartener, filled for the selected database by the hidden list command (one partner per CodFiscal).
    Private Const PartnerInvoicesName As String = "facturi_plati_cui"
    Private Const PartnerListName As String = "facturi_plati_cui_parteneri"
    Private Const PartnerListTitle As String = "Parteneri cu CodFiscal"

    ' The roots of the command list, in the order they are shown. A command is listed under the application it works for; the server
    ' does not say which, so the table below does (a command it does not name goes under NoAppRoot).
    Private Const NoAppRoot As String = "_FARA APLICATIE"
    Private Shared ReadOnly AppRoots As String() = {"VERCON", "FOREXE", "AVACONT", "ADECHIT", NoAppRoot}
    Private Shared ReadOnly CommandApps As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"profil_produs", "VERCON"},
        {AdechitChildName, "ADECHIT"},
        {MoveListName, "ADECHIT"},
        {MoveAnalysisName, "ADECHIT"},
        {MoveName, "ADECHIT"},
        {WipeCommandName, "VERCON"},
        {ArchiveCancelledName, "VERCON"},
        {DeleteBcName, "VERCON"},
        {OiWipeAnalysisName, "VERCON"},
        {OiWipeName, "VERCON"},
        {AvacontWipeCommandName, "AVACONT"},
        {PartnerInvoicesName, "AVACONT"},
        {SignCheckName, "VERCON"},
        {SignRepairName, "VERCON"},
        {StornoCheckName, "VERCON"},
        {StornoRepairName, "VERCON"},
        {"fisa_angajament", "FOREXE"},
        {AngAnalysisName, "FOREXE"},
        {DeleteAngName, "FOREXE"},
        {DdfCheckName, "FOREXE"},
        {ExtrCheckName, "FOREXE"},
        {LanturiCheckName, "FOREXE"}
    }

    ' What to expect from each command, by the name the server gives it (verb «commands»). A command the server adds later and this list
    ' does not know falls back to the generic sentence below.
    Private Shared ReadOnly Expectations As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {DeleteBcName, "Alegeți o bază și scrieți numărul bonului (de exemplu BC 123) sau ID-ul afișat în rezultat. «Rulează» caută bonurile cu acel număr și le pune într-o listă (data, gestiunea, exercițiul, contul, valoarea) sub bara de comenzi; nu șterge nimic. Alegeți din listă bonul dorit și apăsați «Șterge bonul»: se șterge DOAR el. După confirmare, antetul și pozițiile se șterg definitiv, într-o singură tranzacție, cu înregistrare în Jurnal. Bonul poate fi activ sau anulat. Dacă are retur, este asociat unei liste de alimente, provine dintr-un bon de predare sau exercițiul este închis, ștergerea este oprită."},
        {OiWipeAnalysisName, "Pentru o gestiune OI sau MF: butonul «Rulează» face mai întâi ANALIZA (doar citește) și numără, tabelă cu tabelă, ce s-ar șterge: bonurile PTC (predare BP, transfer BT, casare BS) cu registrul OI, TOATE consumurile (BC și retururile lor) și NIR-urile cu pozițiile, storno-urile, legăturile cu facturile și legăturile E-Factura. În casetă scrieți anii: TOATE, 2023, 2022,2023 sau 2020-2023 (anul din data documentului). Dacă baza are mai multe gestiuni OI/MF, scrieți IdGestiune;ani sau numele gestiunii;ani (valoarea * le listează). Dacă totul e în regulă, butonul devine «Rulează ștergerea»; apăsat, cere confirmare și șterge DEFINITIV, într-o singură tranzacție, cu Jurnal. Facturile, nomenclatoarele și Auditul nu se ating; legăturile NIR-urilor cu E-Factura pleacă odată cu ele; contoarele de numerotare ale documentelor dispărute revin la 0. Dacă analiza găsește un blocaj (documente BU, retururi sau storno din anii păstrați care arată spre ce se șterge), butonul rămâne «Rulează» și nu se poate șterge. Schimbarea anilor sau a comenzii readuce butonul la «Rulează». NU SE POATE ANULA: faceți mai întâi o copie a bazei. Dacă păstrați o parte din ani, stocul nu se mai verifică după ștergere."},
        {LanturiCheckName, "Caută în baza aleasă recepțiile al căror ultim instantaneu nu are valoarea recepției (aceleași cazuri pe care le oprește garda din fereastra de asociere) și le desparte pe cauze: instantaneu așezat pe altă recepție (numele lui e numele altei recepții a angajamentului), recepție rescrisă de gemena ei din aceeași zi (are valorile gemenei, iar lanțul ei, neatins, spune altceva) și alte cazuri, care doar se listează. Rezultatul: un sumar cu câte cazuri sunt din fiecare fel și câte sunt reparabile automat, apoi o listă cu fiecare caz și cu ce s-ar face. Dacă există cazuri reparabile apar caseta pentru codul angajamentului și butonul «Repară recepțiile»: scrieți un cod din listă și apăsați-l. Repararea mută instantaneele pe recepția lor sau pune recepția la valorile din lanțul ei, recalculează diferențele și mută cifrele salvate (total recepții, rămas) ale ordonanțărilor afectate, ca ele să se listeze din nou, nu să se genereze din nou; într-o singură tranzacție, cu Jurnal. Verificarea doar citește și cere o singură bază."},
        {DdfCheckName, "Caută în baza aleasă documentele de fundamentare care par greșite: coduri cu mai multe antete FX_DDF (editorul refuză atunci să salveze reviziile), angajamente care arată spre un document care nu e al lor, numere de revizie folosite de două ori în același document și revizii care poartă alt cod decât documentul lor. Rezultatul: un sumar cu câte cazuri sunt din fiecare fel și o listă cu fiecare caz. Sumar cu zero peste tot înseamnă că nu e nimic de semnalat. Dacă apar cazuri, apar caseta pentru codul angajamentului și butonul «Explică»: scrieți un cod din listă și vedeți ce are baza pentru el și ce ar strica ștergerea fiecărui antet (reviziile, rezervările și celelalte rânduri care ar dispărea odată cu el). Verificarea doar citește și cere o singură bază."},
        {ExtrCheckName, "Recitește XML-ul păstrat în FX_Extrase_F pentru fiecare fișier, cu aceleași reguli ca importul, și caută ce ar fi trebuit să scrie importul și nu a scris: antete de cont (FX_Extrase_H) și operațiuni (FX_Extrase). O operațiune lipsește când cheia ei (data documentului, numărul, plătitorul, sumele și referința) nu există nicăieri în FX_Extrase; aceeași operațiune găsită în alt fișier nu se numără. Rezultatul: un sumar (fișiere citite, fișiere fără XML sau cu XML invalid, antete lipsă, operațiuni lipsă) și lista fiecărui caz, cu fișierul, contul, data, numărul documentului, plătitorul, suma și referința. Dacă există cazuri apare butonul «Repară», care scrie DOAR ce lipsește, sub fișierul din care vine, cu funcțiile importului (unitate, clasificație, partener, HASH), într-o singură tranzacție, cu Jurnal; FX_Extrase_F și ce există deja nu se ating. La sfârșit arată ce mai găsește verificarea. Cere o singură bază."},
        {StornoCheckName, "Verifică prețurile cu TVA ale tuturor liniilor storno și bonurilor de retur din baza selectată, în toate gestiunile și exercițiile. Arată sumarul pe gestiune și tip, apoi fiecare diferență cu produsul, documentul, prețul actual și cel propus. Sunt reparabile liniile migrate, nemodificate și active, la care prețul întreg corespunde rotunjirii Access a prețului variantei din aceeași gestiune. Celelalte diferențe rămân pentru analiză. Dacă există cazuri reparabile, apare butonul «Repară», care le corectează pe toate în baza selectată. Verificarea doar citește și cere o singură bază."},
        {StornoRepairName, "Repară toate cazurile eligibile de preț rotunjit la storno și bonuri de retur în baza selectată. Reface verificarea înainte să scrie, recuperează prețul cu TVA din varianta aceleiași gestiuni și actualizează reviziile liniilor și documentelor. Nu adaugă încă o dată TVA și păstrează cantitățile și valorile din antete. Corecțiile au audit cu prețul vechi/nou și o înregistrare în Jurnal; operația este într-o singură tranzacție pentru ambele tipuri. La sfârșit arată liniile corectate și diferențele rămase pentru analiză. Nu se rulează pe <TOATE BAZELE>."},
        {ClsfDupCheckName, "Caută în tabela Clasificatii a bazei alese rândurile repetate (aceeași unitate și același cod Capitol.Subcapitol.Articol.Alineat), de exemplu după ce clasificațiile au fost transferate de mai multe ori. Pentru fiecare rând arată de ce alte tabele este legat (orice coloană IdClsf și orice cheie străină spre Clasificatii) și ce s-ar face cu el: «RAMANE - legat», «RAMANE - nelegat, pastrat din grup», sau «SE STERGE». O legătură doar din Clasificatii_Rectificari sau Clasificatii_Buget nu oprește ștergerea (rândul se șterge odată cu rândurile lui de acolo); o legătură din FX_, EF_ sau orice altă tabelă o oprește. Rezultatul: un sumar (câte grupuri, câte rânduri sunt reparabile, adică nelegate de nimic) și lista rândurilor. Dacă există rânduri reparabile apare butonul «Repară», care șterge DOAR dublurile nelegate, într-o singură tranzacție, cu Jurnal; un grup fără niciun rând legat păstrează unul. Grupurile cu mai multe rânduri legate nu se ating, se analizează de mână. Verificarea doar citește și se poate rula și pe <TOATE BAZELE>; repararea merge pe o bază anume."},
        {AngAnalysisName, "Arată, pentru codul scris în casetă, ce angajament este (denumirea, descrierea, obiectul și partenerul documentului de fundamentare, starea) și ce s-ar șterge din baza aleasă, tabelă cu tabelă: indicatorii și istoricul, documentele de fundamentare cu reviziile, PDF-urile și rezervările lor, recepțiile, plățile și operațiunile, ordonanțările cu părțile și PDF-urile lor, numerele blocate și corecțiile lui din notele CAB (nota însăși rămâne dacă o mai folosește alt angajament). Spune și ce rămâne: tabelele EF_ (facturile primite) nu se ating; singura excepție inevitabilă sunt legăturile factură–document de fundamentare alese de mână, care pleacă odată cu documentul și rămân scrise în Jurnal; Clasificațiile, partenerii, legăturile Parteneri_Angajamente și extrasele rămân. Analiza doar citește. Dacă codul există, apare butonul «Șterge angajamentul», care cere confirmare cu denumirea și șterge exact ce ați văzut, într-o singură tranzacție (dacă ceva nu merge, nu se șterge nimic); nu se poate anula. Cere o singură bază."},
        {PartnerInvoicesName, "Alegeți baza în arbore și furnizorul în lista care apare (partenerii bazei, unul pe CodFiscal; scrieți o parte din denumire sau din CodFiscal ca să îl găsiți). Rezultatul: partenerii găsiți cu acel CodFiscal (câte unul în fiecare unitate); sumarul pe SS și an (facturi, valoare, plătit, rest, plăți); facturile (evidențe Tip E) cu plătit și rest, «in lista» arătând dacă apar în «Facturi și plăți»; plățile (Tip P) cu numărul ordinului; și alte operații ale partenerului, cu alt Tip, care nu intră în «Facturi și plăți». Se citește, nu se schimbă nimic."},
        {"ping", "Un singur rând pe bază: versiunea MariaDB, contul cu care s-a conectat serverul, charset-ul și collation-ul bazei și timpul de răspuns. Normal: charset utf8mb3, collation utf8mb3_general_ci, un timp de răspuns de câteva zeci de milisecunde. Altceva, sau sute de milisecunde, merită verificat."},
        {"tables", "Lista tuturor tabelelor bazei, cele mai mari primele, cu mărimea în MB, motorul și collation-ul. Numărul de rânduri e doar estimat de server, nu exact. Se citește, nu se schimbă nimic."},
        {"counts", "Numărul exact de rânduri al fiecărei tabele. Tabelele de peste 3 milioane de rânduri rămân pe estimare (nota de sub rezultat spune câte). Pe baze mari poate dura câteva secunde."},
        {"processlist", "Conexiunile deschise acum pe bază: cine, în ce stare, de câte secunde și ce comandă rulează. Normal: puține rânduri. Multe rânduri cu timp mare arată conexiuni rămase agățate."},
        {"journal", "Ultimele 50 de intrări din Jurnal pentru baza aleasă (cont, acțiune, țintă, rezultat), cele mai noi primele."},
        {"activity", "Pentru fiecare cont al bazei: ultima acțiune și numărul de acțiuni din Jurnal. Un cont care lipsește din listă nu a făcut nimic înregistrat."},
        {"charset", "Rezultatul bun este LISTA GOALĂ (nota spune că totul este utf8mb3_general_ci). Fiecare rând apărut este o coloană cu alt collation și trebuie corectată."},
        {"analyze", "Rulează ANALYZE TABLE pe fiecare tabelă: se reînnoiesc doar statisticile, datele rămân neschimbate. Fiecare tabelă ar trebui să răspundă «OK». Cere confirmare, fiindcă scrie în statistici; pe baze mari poate dura."},
        {"fisa_angajament", "Un tabel cu tot ce știe baza despre angajamentul scris: întâi rândurile DIAG (verificările calculate), apoi rândurile brute din toate tabelele. Cere o bază anume și codul angajamentului; nu merge pe <TOATE BAZELE>. Rândurile nu se trunchiază, ca rezultatul să poată fi copiat."},
        {"oi_semne_repara", "Corectează în registrul OI al bazei alese rândurile cu cantitate negativă și valoare pozitivă: valoarea devine negativă, ca cantitatea. Rândurile cu cantitate pozitivă și valoare negativă nu se ating, rămân în rezultat. SCRIE în baza aleasă, cere confirmare, notează în Jurnal și arată apoi ce a mai rămas. Merge doar pe o bază anume. În mod obișnuit nu se alege de aici: butonul «Repară» apare singur după «Semne greșite»."},
        {"oi_semne", "Caută în registrul OI (VC_OI_Produse) rândurile la care cantitatea și valoarea au semne diferite, de exemplu cantitate -1 și valoare +184,99. Rezultatul: pe fiecare gestiune, câte rânduri sunt, la câte produse, pe ce fel de bon (predare, transfer, casare) și primele numere IDOI. Lista goală înseamnă că nu e nimic de corectat. Se poate rula pe <TOATE BAZELE>; o bază fără tabelele VERCON o spune și trece mai departe. Se citește, nu se schimbă nimic."},
        {WipeCommandName, "Șterge TOATE rândurile din tabelele VC_ ale bazei alese (documente, produse, gestiuni, exerciții, subunități, setări, numerotare, markerii de import) și din tabelele de trimitere spre Vercon ale facturilor primite (EF_PrimiteVercon*, EF_VERCON_*), ca baza să poată fi migrată din nou. Structura tabelelor rămâne. Partenerii, FX_, AD_ și TZ_ nu se ating. Cere o bază anume (nu <TOATE BAZELE>) și numele ei scris în casetă, ca să nu se golească baza greșită. Rezultatul: câte rânduri au fost în fiecare tabelă și câte au rămas (trebuie să fie 0 peste tot). NU SE POATE ANULA: faceți mai întâi o copie a bazei dacă mai e nevoie de date."},
        {AvacontWipeCommandName, "Șterge TOATE rândurile din tabelele TZ_ ale bazei alese (note, documente, operații, debit/credit, plan de conturi, solduri inițiale, jurnale, șabloane de operații și de salarii, subunități, OP-uri, OPME, plăți pe facturi, setări) și din tabela EF_PrimiteOperatii (legăturile facturilor primite cu operațiile), ca datele contabile să poată fi migrate din nou de la zero. Structura rămâne. Partenerii, clasificațiile, FX_, AD_, VC_ și restul tabelelor EF_ (facturile primite, liniile și mesajele lor) nu se ating. Nu merge pe AVACONT_SURSA și AVACONT_COMUN, care țin doar structura. Cere o bază anume și numele ei scris în caseta de mai sus. Rezultatul: câte rânduri au fost în fiecare tabelă și câte au rămas (trebuie să fie 0 peste tot). După golire: rulați migrarea (Subunități, Plan de conturi, Jurnale, Șabloane, Operații, Plăți) și importul E-Factura din Access, care pune la loc legăturile facturilor. NU SE POATE ANULA: faceți mai întâi o copie a bazei."},
        {"profil_produs","Cere o bază anume și IdProdus (numărul variantei produsului din gestiune, nu codul din nomenclator). Rezultatul, de sus în jos: produsul (denumire, cod, preț, câte gestiuni îl au); în ce tabele VC_ apare și câte rânduri; apoi, pe fiecare gestiune, observațiile despre registrul OI (liniile de bon care nu-și găsesc contul în registru, adică exact cazul OI_ACCOUNT_ORIGIN), stocul OI pe deținător și, la urmă, fiecare rând din fiecare tabelă, cu antetul bonului sau NIR-ului lângă el. Se citește, nu se schimbă nimic. Rezultatul se copiază cu butonul de copiere și se poate lipi în conversație."}
    }

End Class
