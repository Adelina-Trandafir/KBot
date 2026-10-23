# Fisa de citire: Sablon materiale / servicii / diverse (btnOCI -> form Operatii2020)

Scop: ce trebuie sa supravietuiasca din sablonul de operatii contabile din Access in K-BOT, ce se arunca,
ce a mers prost. Document de specificatie, fara cod. Autor: analiza 23.10.2026.

Legenda: VERIFICAT (fisier) = citit direct in export. PRESUPUS = dedus, de confirmat cu operatorul.
Prescurtare: EV = AVACONT\EXPORT_VBA ; Forms\X = EV\Forms\X_code.txt sau X_header.txt.

Decizie operator (data in cerere): se pastreaza LOGICA contabila, NU metoda de calcul din Access
si NU defectele ei. Fara mecanismul de substituire 'XXX' pentru partener. Fara copie 1:1.

## 0. Corectii fata de analiza anterioara

- CORECTIE (23.10.2026, dupa citirea `baza2026.accdb`): DefaNote si DefaNoteA NU sunt tabele legate; sunt
  tabele LOCALE in `AVACONT\DATABASES\baza2026.accdb` si lipseau doar din `EV\tables` (acel export vine din alt
  fisier). Structura reala e in sectiunea 0.1.
- DefaNoteA.CodPartener are TREI continuturi, dupa Tip (VERIFICAT pe date, `baza2026.accdb`, 89 de antete):
  Tip='C' (41): cheia clasificatiei cu 'XX.XX' in loc de subcapitol, ex. `65.01.XX.XX.57.02.02`;
  Tip='A' (24): jumatate cheie de clasificatie, jumatate marcajul `XXX`;
  Tip='M' (24): un COD DE PARTENER real din 3 caractere (`024`, `028`, `035`, `041`...).
  VERIFICAT in cod: Forms\Operatii2020 Sav_Click scrie `IIf(C_CLSF, mcTree.ActiveNode.ControlTipText, "XXX")`;
  Forms\OPuri2017_header LstEv filtreaza `CodPartener = forms!opuri2017!ClsfX`.
  Concluzie: coloana e SUPRAINCARCATA (cheie clasificatie / partener / marcaj). In K-BOT se separa in coloane cu
  sens clar. (Operatorul a confirmat ca ea tine cheia clasificatiei; pentru Tip 'M' datele arata partener.)
- Fel (E/P/A) este in DefaNoteA si decide in ce lista apare sablonul. VERIFICAT: Forms\Operatii2020_header,
  TIPOP: E = Evidentiere operatie contabila, P = Plata operatie contabila, A = Alte operatii contabile.
- Consumatorii NU folosesc toti aceeasi cheie de legatura header-linie-factura: vezi sectiunea 3.
- Codul "Preluare ani anteriori" (Operatii2020_Preluare*) este in intregime comentat in export.
  VERIFICAT: Forms\Operatii2020_Preluare_code.txt are 5 linii necomentate (doar antet). Nu e functie vie.

## 0.1 Structura reala si continut (VERIFICAT: `baza2026.accdb`, citita pe o copie, read-only)

DefaNoteA (89 randuri): IDA (Long, PK), Explicatie, Tip, Jurnal, Fel, Compartiment, CodPartener, FelOP (toate Text 255),
DTQ (Date, implicit Now()). Tip: A=24, C=41, M=24. DefaNote (92 linii): ID (Long, PK), IDA (Long, FK->DefaNoteA),
Forma (Text), Tip (Long: valori 1, 2, 4), Clsf (Long, FK->Clasificatii.IDClsf), Jurnal (FK->Jurnale.CodJurnal),
Explicatie, ContDebit, PartDebit, ContCredit, PartCredit (Text), Ordonantare, PA, Chelt (Da/Nu), Simpla (Long), Fel, DTQ.
Relatiile au Update+Delete in CASCADA: stergerea unui cont din PlanCont sau a unui jurnal ar sterge liniile de sablon.
- ContDebit/ContCredit contin SimbolCont REZOLVAT (22 de caractere cu clasificatie, 10 fara), de ex.
  `635010001A650402594000` = cont + sector `01` + sursa `A` + ClsfF + ClsfE. Deci sablonul are sectorul si sursa
  unitatii "coapte" in cont; nu se poate reutiliza la alt sector/sursa. (Raspunde intrebarii "cont rezolvat sau
  cont de baza + regula": Access il stocheaza rezolvat.)
- 54 din 92 de linii folosesc marcajul `XXX` in PartDebit/PartCredit (partener cerut la utilizare); 44 au partener
  fix la debit, 46 la credit. Forma este doar `04.02` (91 linii) si `04.03` (1 linie) in aceasta baza.
- Tip de antet 'M' (24 de antete, cu cod de partener fix, Jurnal J0D): sensul literei nu e documentat in cod;
  PRESUPUS "materiale / furnizor fix". De confirmat de operator.

## 1. Ce este sablonul (VERIFICAT: Forms\Operatii2020, Operatii2020_S)

Un sablon = un ANTET (DefaNoteA) + N LINII (DefaNote). Antetul spune "ce operatie este" si pentru
ce clasificatie; liniile spun ce conturi se misca, pentru fiecare forma (subcapitol) a clasificatiei.

Antet (o singura data pe sablon):
- Explicatie (text operatie), Jurnal (cod din tabelul Jurnale, fara 'JI'), Compartiment (text liber cu
  lista din valorile deja folosite), Fel (E/P/A), FelOP (text "fel factura", doar pentru P).
- Tip: 'C' = legat de o clasificatie (ecran "Evidentiere / Plati"), 'A' = diverse (fara clasificatie fixa).
- CodPartener = cheia clasificatiei (vezi sectiunea 0).
- IDA = numar manual: `DMax(IDA)+1` (VERIFICAT, Sav_Click).

Linie (repetata pe sablon):
- Forma (= Clasificatii.Subcapitol), Clsf (= IDClsf concret), ContDebit, PartDebit, ContCredit, PartCredit,
  Ordonantare, PA, Chelt (3 bifari), plus Jurnal si Explicatie copiate din antet pe fiecare linie.
- Pentru Tip='C' liniile se genereaza automat cate una pe forma existenta a clasificatiei
  (`Clasificatii WHERE ClsfX Like cheie`), operatorul doar completeaza conturile. Pentru 'A' operatorul
  adauga liniile liber, cu forma si clasificatie la alegere. VERIFICAT: Operatii2020 mcTree_Click.

## A. Reguli de afaceri care trebuie sa supravietuiasca

A1. Conturile de cheltuiala/venit legate de buget se compun din cont de baza + capitol + forma + articol/alineat.
    VERIFICAT: EV\Modules\mdl_Functii2022_code.txt MakeCont: baza + sector/sursa + 2 cifre capitol + forma
    + ultimele 8 cifre ale articolului (+ analitic dupa '.'), cu 3 variante de rezerva, altfel '#'.
    Lista de conturi oferita pe linie e filtrata dupa clasificatie: ClsfF (capitol+forma) si ClsfE
    (articol+alineat) trebuie sa se potriveasca simbolului contului din PlanCont; clasa 8 exclusa.
    VERIFICAT: Forms\Operatii2020_S_header, RowSource ContDebit/ContCredit.
A2. Un sablon de evidentiere/plata trebuie sa existe pentru FIECARE forma a clasificatiei folosite pe factura.
    VERIFICAT: EV\Modules\mdl_Salvare_NoteContabile_code.txt SavEvPlop: mesaj "Operatia selectata nu este
    configurata pentru forma X", rollback.
A3. Evidentiere (Fel=E): se creeaza Oper cu Angajament = PA, Propunere = PA, Ordonantare = 0, Chelt din linie.
    VERIFICAT: SavEvPlop + queries\SAV_OPER_EV_2017.sql.
A4. Plata (Fel=P): Oper cu Angajament = 0, Propunere = 0, Ordonantare = linie.Ordonantare, Chelt din linie.
    VERIFICAT: SavEvPlop (blocul P1) + queries\SAV_OPER_PL_2017.sql.
A5. Sensul practic al bifarilor (stadiile executiei bugetare):
    PA = Propunere de angajament SI angajament (se scriu amandoua cu aceeasi valoare) - doar la evidentiere;
    Ordonantare = ordonantare la plata, primeste numar NrOpe urmator - doar la plata;
    Chelt = linia numara ca cheltuiala efectiva (flag pe Oper). Regula: nici una nu e permisa fara clasificatie.
    VERIFICAT: Forms\Operatii2020_S (Form_BeforeUpdate, Ordonantare/PA_BeforeUpdate); Forms\OP9
    Sav_Mat2015 (NrAng la PA, NrOpe la Ordonantare). Semantica juridica "Chelt" = PRESUPUS (nu e definita in cod).
A6. Debit si credit au aceeasi suma (suma documentului), cate un singur rand Debit si un singur Credit pe operatie.
    VERIFICAT: SavEvPlop, Sav_Mat2015.
A7. Contul si partenerul trebuie sa existe (PlanCont, Parteneri) inainte de salvare; altfel eroare si rollback.
    VERIFICAT: SavEvPlop (EROARE! LIPSESTE CONTUL / PARTENERUL).
A8. Angajament legal (ALG) NU vine din sablon, vine din document/formular; daca ALG este bifat se scrie si Cont8067.
    VERIFICAT: SavEvPlop, Sav_Mat2015.
A9. Linie valida = ContDebit si ContCredit completate; sablonul fara nicio linie valida nu se pastreaza.
    VERIFICAT: Operatii2020 Sav_Click (INSERT ... WHERE ContDebit/ContCredit nenule), Form_Load (sterge antete fara linii).
A10. FelOP: text adaugat la explicatia ordinului de plata ("F <nr facturi> C <cod client> <FelOP>").
    VERIFICAT: Forms\OPuri2017_code.txt linia ~1017 + FelOp_AfterUpdate (actualizeaza antetul P).
A11. Compartiment si Jurnal din antet ajung pe Oper (Comp, CodJurnal). VERIFICAT: SAV_OPER_*.sql.
A12. Pentru operatii 'A' (diverse) numerotarea NrAng/NrOpe incepe de la ultimul numar existent + 1.
    VERIFICAT: Forms\OP9 mcTree_Click (IDURI.NrAng/NrOpe).

## B. Mecanisme Access care se arunca

B1. Partener 'XXX' (PartDebit/PartCredit): inlocuit la salvare cu partenerul ales pe ecran
    (`Replace(...,"XXX",forms!opuri2017!part)`). VERIFICAT: SAV_Debit_EV_2017.sql, OP9 PART_AfterUpdate.
    DE INLOCUIT cu regula explicita pe linie (sectiunea F).
B2. Tabele de lucru: Operatii2020 (copie de editare), tmpClsf4 (arbore clasificatii), tmpDefaNote (ecranul
    OP9/OP_TREZO), tmpDefaNote_T / tmpDefaNoteA_T / tmpDefaNote_BC (preluare/backup). VERIFICAT: EV\tables.
    Nu se porteaza; editarea se face in memorie, salvarea intr-o tranzactie.
B3. Formulare ca parametri: `Forms!opuri2017!LstEv`, `forms!op9!datadc` in interogari. VERIFICAT: queries\SAV_*.sql.
B4. Cheie IDA/ID prin `DMax+1` si `DMax(ID)+IdClsf` (concurenta si coliziuni). VERIFICAT: Operatii2020_code.
B5. Stergere antet+linii fizic, doar cu jurnal de actiuni (Actiuni), fara verificare de utilizare.
    VERIFICAT: EV\Modules\mdl_Popup2022_code.txt PP_fOperatii2020 (Case 3).
B6. Preluare intre baze prin `IN '' [MS Access;...]` cu parola in clar (comentat). Nu se porteaza.
    Echivalent nou: copiere sablon intre unitati din aplicatie, cu validari (vezi D4).
B7. Legatura vizuala prin arbore clsTreeNode cu Tag numeric -9/-3/1001/1002 care codifica starea.
    VERIFICAT: Operatii2020_code mcTree_Click.

## C. Defecte observate

C1. Cheia de legare difera: SavEvPlop cauta linia dupa (IDA, Forma); SAV_*_2017 dupa Clsf; SAV_*(E-factura) si
    Burse dupa Clsf SI Forma. VERIFICAT: SAV_Debit_EV_2017 vs SAV_Debit_EV vs SAV_OPER_EV_Burse.
C2. Document (Documente) se insereaza pentru toate facturile noi indiferent daca exista linie de sablon;
    lipsa liniei da document fara Oper. VERIFICAT: SAV_Document_EV_2017.sql (fara join pe DefaNote).
C3. Oper.Tip = "E" fix si la plata; la burse "EB"; la diverse "A"; la salarii 10. Nu vine din sablon.
    VERIFICAT: SAV_OPER_PL_2017.sql, SAV_OPER_EV_Burse.sql, OP9 Sav_Mat2015, SalariiSavDoc2019.
C4. Coloane care mint: CodPartener (= cheia clasificatiei), Tip pe linie (2 sau 4 = stare de editare, nu
    afacere; "If Tip = 2 Then Exit Sub" sare validarea). VERIFICAT: Operatii2020_S_code Form_BeforeUpdate.
C5. Jurnal si Explicatie dublate pe fiecare linie, setate in bloc din antet; VBA si SQL citesc din locuri
    diferite (SavEvPlop din antet, SAV_OPER_*.sql din linie). VERIFICAT: Operatii2020 CODJ/DOC_AfterUpdate.
C6. Validare "macar un partener" pe linie: PartDebit sau PartCredit nenul. Un sablon fara parteneri
    nu se poate salva. VERIFICAT: Operatii2020_S Form_BeforeUpdate (MsBox 86). Regula incompleta pentru
    operatii intre conturi fara tert (PRESUPUS: se ocoleste cu 'XXX').
C7. OP9: `IDD = IDD + I` creste cumulat (1,3,6,...) - goluri in numerotare. VERIFICAT: Forms\OP9 Sav_Mat2015.
C8. SQL concatenat cu valori din text (Doc, CODJ) cu replace de ghilimele; risc de injectie/rupere.
    VERIFICAT: Operatii2020 Sav_Click, DOC_AfterUpdate.
C9. Clsf_Oper2020_2022 si Clsf_Oper2020_2025 (interogari gemene) alimenteaza arborele; ambele pe DLookup per rand.
    VERIFICAT: EV\queries.

## D. Coloane citite efectiv (grep in EV\queries, Forms, Modules)

Citite de consumatori (SavEvPlop, SAV_*, OP9, OP_TREZO, Burse):
- Antet: IDA, Explicatie, Jurnal, Fel, Compartiment, CodPartener (=cheie clsf), FelOP, Tip (OP9, OP_TREZO filtreaza Tip='A').
- Linie: IDA, Forma, Clsf, ContDebit, PartDebit, ContCredit, PartCredit, Ordonantare, PA, Chelt, Jurnal, Explicatie.
VERIFICAT: grep `DefaNote(A)?\.` in EV (ex.: 59x IDA, 40x Clsf, 33x Forma, 15x PA).
Moarte sau doar copiate:
- Linie: Simpla (copiat in tmpDefaNote, dar Oper.Simpla = 0 peste tot), Fel, DTQ, Tip (starea de editare), ID.
- Antet: DTQ.
  VERIFICAT: Simpla "0 AS Simpla" in toate SAV_*; Sav_Mat2015 `!Simpla = 0`. Fel/DTQ doar in Preluare comentat.

## E. Fluxul de aplicare (ce are nevoie sablonul ca intrari)

E1. Evidentiere factura (Fel=E): intrari = lista facturi noi (Forma, IdClsf, Suma, data, numar document, NC),
    partener ales, ALG/DataALG/SumaALG, sablonul ales. Pas: pentru fiecare factura -> linia (sablon, forma) ->
    Document + Oper + Debit + Credit (+Cont8067). VERIFICAT: SavEvPlop, SaveEFactura.
E2. Plata (Fel=P): intrari = plati (IDO, valoare, data, nr OP, forma, IdClsf), partener, FelOP. Idem + PlatiFacturi
    + OPuri (ordin de plata). VERIFICAT: SavEvPlop blocul P1.
E3. Diverse (OP9) si Trezorerie (OP_TREZO): intrari = sablon (Tip=A), suma, data, numar si fel document, NC, partener
    (daca linia are 'XXX'). Pas: linii -> Document(Fel='A') + Oper(Tip='A') + Debit + Credit. VERIFICAT: OP9 Sav_Mat2015,
    OP_TREZO_code, queries\q_OP_TREZO.sql.
E4. Burse: acelasi mecanism cu (IDA, Clsf, Forma) si Tip='EB'. VERIFICAT: SAV_OPER_EV_Burse.sql, qBURSE_PL_Tip11.sql.
E5. Tranzactie unica pe toata salvarea cu rollback total. VERIFICAT: SavEvPlop (Wrk.BeginTrans/Rollback).

## F. Model de date propus (PROPUNERE, nu decizie)

Per unitate (IdUnitate), fara coloana de an: sablonul nu depinde de exercitiu; clasificatia se leaga prin cheie
(capitol, articol, alineat), nu prin IdClsf. Daca clasificatiile se schimba pe an, vezi intrebarea 6.

- sablon_op (antet): id, id_unitate, denumire, faza (EVIDENTIERE | PLATA | ALTELE), mod (CLASIFICATIE | LIBER),
  cod_jurnal (FK Jurnale), compartiment, text_fel_op, capitol, articol, alineat (null la mod=LIBER), activ, creat/modificat.
- sablon_op_linie: id, id_sablon, ordine, forma (null = valabila pentru toate formele), id_clasificatie (null permis),
  cont_baza_debit, cont_baza_credit, cont_debit_final, cont_credit_final (final null = se compune la aplicare),
  sursa_tert_debit (NICIUNUL | PARTENER_OPERATIE | FIX), id_partener_debit_fix, idem credit,
  flag_pa, flag_ordonantare, flag_cheltuiala.
- Reguli de integritate in tabela/aplicatie: unic (id_sablon, forma); flag_* doar cu clasificatie; faza=EVIDENTIERE
  nu permite flag_ordonantare; faza=PLATA nu permite flag_pa; cont si partener fix cu FK.
- Parteneri: tabela exista in MariaDB (000_DEMO.sql: Parteneri, unica (IdUnitate, CodPartener)) si fereastra
  ParteneriForm exista (src\KBot.App\Views\Nomenclatoare\ParteneriForm.vb). Clasificatii exista (000_DEMO.sql, cu
  coloanele generate ClsfX, ClsfF, ClsfE, SS). VERIFICAT. Nu exista nicio tabela de conturi/jurnale/sabloane
  in MariaDB_Schema\000_DEMO.sql (CREATE TABLE cu Plan, Jurnal, DefaNote, Salar: niciuna). VERIFICAT.
  Existenta planului de conturi si a jurnalelor: vezi celelalte fise din acest folder (conturi-plan-de-conturi.md, jurnale.md).
- Audit: tabel de actiuni existent in K-BOT (de confirmat), in locul Actiuni din Access.

## G. Intrebari pentru operator (max 8)

1. Pentru fiecare linie, tertul (partener) are doar trei cazuri: niciunul, "partenerul operatiei" (cel ales la aplicare),
   sau un partener fix? Exista un al patrulea (ex. tert din document)?
2. Contul final se salveaza gata compus (un rand pe forma, ca acum) sau se salveaza contul de baza si se compune la
   aplicare cu regula MakeCont? Prima varianta e simpla dar se desincronizeaza cand se adauga o forma noua.
3. Ce inseamna exact "Cheltuiala" (Chelt) in rapoartele voastre: apare pe evidentiere, pe plata, sau pe ambele?
   Azi se copiaza din linie fara regula.
4. Sablonul se copiaza intre unitati (cum incerca "Preluare ani anteriori")? Daca da, vrem validari (jurnal, cont,
   partener, forma lipsa) cu raport de erori inainte de copiere?
5. Jurnal si Explicatie pot diferi pe linii in acelasi sablon, sau raman la nivel de antet?
6. Clasificatiile bugetare isi schimba codurile de la un an la altul? Daca da, sablonul trebuie legat de cod sau
   de IdClsf, si cum se repara dupa schimbare?
7. Cand lipseste linia de sablon pentru o forma, blocam salvarea (cum face SavEvPlop) sau permitem salvare fara
   operatie contabila (cum face accidental varianta 2017)?
8. Stergerea unui sablon deja folosit (Oper.IDA il refera): interzisa, sau dezactivare (activ=false)?
