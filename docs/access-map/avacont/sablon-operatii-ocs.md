# Fisa de citire: Sablon operatii salarii (btnOCS -> form OCS)

Scop: ce trebuie sa supravietuiasca din sablonul de contari salarii din Access in K-BOT, ce se arunca, ce a
mers prost. Document de specificatie, fara cod. Analiza 23.10.2026.

Legenda: VERIFICAT (fisier) = citit direct in export. PRESUPUS = dedus, de confirmat cu operatorul.
Prescurtare: EV = AVACONT\EXPORT_VBA ; Forms\X = EV\Forms\X_code.txt sau X_header.txt.

Decizie operator: se pastreaza LOGICA contabila, NU metoda de calcul din Access si NU defectele ei.
Fara copie 1:1.

## 0. Corectii fata de analiza anterioara

- Numele "VA/PC/PN" nu inseamna ce parea: VA = Retineri, PC = Viramente, PN = Plati cu numerar; articolul
  de baza (DefaSalarii) = Angajamente salarii. VERIFICAT: Forms\OCS_header, etichetele L1..L4
  ("> Angajamente salarii", "> Retineri", "> Viramente", "> Plati cu numerar") legate de subformularele
  DefaSalariiAN / VA / PC / PN (Forms\OCS_code Art_Click).
- CORECTIE (23.10.2026): tabelele sablonului (DefaSalarii 16 randuri, DefaSalariiVA 73, DefaSalariiPC 88,
  DefaSalariiPN 18) sunt LOCALE in `AVACONT\DATABASES\baza2026.accdb`, nu legate; lipseau doar din `EV\tables`.
  Structura reala (VERIFICAT, copie read-only): DefaSalarii: ID, IDS (PK), Clsf, Explicatie, ContD, PartD, ContC,
  PartC, Cap, Proiect, Ascuns, DTQ. VA: IDVA (PK), IDD (FK->DefaSalarii.IDS), Explicatie, ContD/PartD/ContC/PartC,
  Cap, CM, Ascuns, ContB. PC: IDPC (PK), IDD, IDVAA (legatura la VA), Explicatie, conturi, Cap, CM, Ascuns.
  PN: IDPN (PK), IDD, Articol, Explicatie, conturi, GRUPA, Cap, Ascuns. Relatiile sunt cu Update+Delete in cascada
  (stergerea unui articol sterge VA/PC/PN; stergerea unui cont din PlanCont sterge randurile care il folosesc).
  Conturile stau rezolvate (ex. `6410000`, `4210000`, `7700000`) cu partener fix (`001`, `002`, `005`...).
- Consumatorii reali sunt Salarii_Nou_Ang + Salarii_Nou_EvPcPn (la deschiderea unei luni noi, Forms\Salarii2015
  Form_Open/Nou) si Forms\SalariiSavDoc2019 SavSalarii (salvare). Edu-variantele (Salarii_Nou_EDU_*) fac aceeasi
  treaba pentru ecranul Salarii2026. VERIFICAT: Forms\Salarii2015_code.txt liniile ~80-95; Salarii2026_code.txt ~41-57.

## 1. Structura sablonului (VERIFICAT: Forms\OCS, DefaSalarii*)

Un ARTICOL (DefaSalarii) = o clasificatie economica din stat (ex. salarii de baza), pentru un capitol (Cap).
Sub el patru straturi, toate cu aceleasi patru campuri contabile (ContD, PartD, ContC, PartC) + Explicatie + Cap:

1. Angajament (articolul insusi, 1 rand): debit = cheltuiala de personal, credit = datorie catre personal.
   Se salveaza ca document de tip ANG. Seteaza Angajament + Propunere.
2. Retineri (VA, 0..n pe articol): sumele retinute din salariu (taxe, contributii, popriri etc.).
   Debit = exact contul/partenerul de credit al articolului (se copiaza automat la inserare); credit = tertul
   catre care se datoreaza retinerea. Toate cele 4 campuri obligatorii. VERIFICAT: DefaSalariiVA Form_BeforeInsert
   (ContD = Ch1!ContC, PartD = Ch1!PartC) si Form_BeforeUpdate (MsBox 10427 daca lipseste ceva).
3. Viramente (PC, 0..n): plata retinerii catre terti. Se creeaza automat odata cu fiecare retinere:
   debit = creditul retinerii, credit = cont de trezorerie '7700000', fara partener credit; text
   "Achitat <tert>". Se pastreaza legatura PC -> VA (IDVAA). VERIFICAT: DefaSalariiVA Form_BeforeUpdate (S = INSERT ... '7700000').
   Seteaza Ordonantare=True.
4. Plati cu numerar (PN, de regula 2 randuri pe articol): salarii platite in numerar (ridicare din trezorerie +
   plata). Ordonantare setat per rand. VERIFICAT: Forms\Salarii2026_code.txt Actualizeaza_Numarer
   ("Config. pentru numerar nu este corecta" daca numarul de randuri nu e exact 2).

Randul Angajament nu are linie cu sume proprii in sablon: suma vine din statul de plata la aplicare.

## A. Reguli de afaceri care trebuie sa supravietuiasca

A1. Conturile se salveaza in forma de baza (max. 7 caractere, optional '.analitic') si se compun la aplicare cu
    capitolul, forma si articolul lunii: MakeCont(baza, Cap, Forma, Art). VERIFICAT: queries\Salarii_Nou_Ang.sql
    si Salarii_Nou_EvPcPn.sql (MakeCont pe ContD si ContC); EV\Modules\mdl_Functii2022_code.txt.
    Lista de conturi din sablon se construieste din PlanCont taind sufixul capitol+forma. VERIFICAT: queries\qOCS_Cont.sql.
A2. Fiecare retinere trebuie sa aiba virament asociat; fiecare virament trebuie sa aiba retinere sau angajament
    asociat. Violarea doar se afiseaza ca avertisment rosu (LF2, LF3), nu blocheaza.
    VERIFICAT: DefaSalariiVA Form_Current ("Lipseste operatia de plata asociata"); DefaSalariiPC Form_Current.
A3. Virament = plata a unei datorii deja inregistrate: debitul viramentului = creditul (cont+partener) retinerii
    sau al articolului. VERIFICAT: DefaSalariiPC Form_Current (compara ContD/PartD cu ContC/PartC din VA sau articol);
    Form_BeforeUpdate VA (actualizeaza PC cand se schimba contul retinerii).
A4. Reconciliere (varianta EDU, ecranul Salarii2026): totalul articolului trebuie sa fie egal cu totalul retinerilor/viramentelor; abaterile se
    coloreaza (negru ok, albastru lipsa plata, verde lipsa evidentiere, rosu ambele).
    VERIFICAT: Forms\Salarii2026_code.txt Poptree_AsocEdusal_Sub (RC!TOTAL vs totalpc vs totalva).
A5. Stari la salvare, pe fiecare tip (ANG, VA, PC, PN):
      ANG: Angajament=True, Propunere=True, Ordonantare=False, NrAng+1 pe rand;
      VA : toate False (doar evidentiere retinere);
      PC : Ordonantare=True, NrOpe+1;
      PN : Ordonantare = valoarea randului; NrOpe+1 doar daca Ordonantare.
    Chelt=False peste tot. Oper.Tip = 10, Oper.Fel = tipul, Oper.IDX = id rand sablon.
    VERIFICAT: Forms\SalariiSavDoc2019_code.txt SavSalarii (Switch pe Tip).
A6. Un document pe tip (Documente.Fel = ANG/VA/PC/PN), cu numar NC, numar document, data, jurnal, compartiment
    introduse de operator la salvare (NU vin din sablon). VERIFICAT: SalariiSavDoc2019 Form_Load/SavSalarii.
A7. Randul intra la salvare doar daca e bifat (S=True) si suma de plata <> 0. Conturile si partenerii
    se valideaza contra PlanCont si Parteneri INAINTE de tranzactie, cu lista completa a celor lipsa.
    VERIFICAT: SavSalarii (primele 4 verificari, mesajele 80 si 81).
A8. Angajament legal (ALG): se bifeaza la angajament; la PC/PN doar pentru plati din anul precedent.
    Daca ALG, se scrie si Cont8067 cu suma randului. VERIFICAT: SalariiSavDoc2019 Form_Load + SavSalarii.
A9. Randurile cu CM (concediu medical, cont 423) se calculeaza diferit de restul statului; CM se bifeaza doar pe
    randurile legate de concediu medical. VERIFICAT: tooltip Forms\OCS_header (control "CM"). Cum se calculeaza
    diferit = PRESUPUS (nu e in Forms\OCS; vezi Salarii2015, neinvestigat aici).
A10. Idempotenta: la deschiderea unei luni, randurile de sablon deja incarcate (IDTBL) nu se dubleaza;
    randurile de sablon cu cont sau partener lipsa (VA: toate 4; PC: ContD, ContC, PartD; PN: ContD, ContC) nu se
    incarca. VERIFICAT: queries\Salarii_Nou_EvPcPn.sql (conditiile WHERE).

## B. Mecanisme Access care se arunca

B1. Copierea prin tabel temporar tmpDefaSalarii ("Copiaza randurile curente" / "Lipeste"): se salveaza
    ID-urile prin DMax+(ID-DMin)+1. VERIFICAT: Forms\OCS_code bCh2_Click, bCh4_Click. Inlocuit cu "Duplica".
B2. TmpSalariiX ca tabel de lucru la aplicare, plus Salarii/SalariiH/tmp_CSAV. Se inlocuiesc cu un obiect in memorie
    si o singura tranzactie. VERIFICAT: EV\tables\TmpSalariiX.md.
B3. Soft-delete prin Ascuns pe cele 4 tabele + stergere fizica daca nu e folosit (Oper WHERE Fel=.. AND IDX=..).
    VERIFICAT: Forms\DefaSalariiAN bDEL_Click, DefaSalariiVA/PC/PN bDEL_Click. Inlocuit cu activ + restrictie FK.
B4. Parametri din formulare in interogari: `forms!Salarii2015!cap/forma/la`, `Forms!OCS!Art`. VERIFICAT: Salarii_Nou_*.sql.
B5. Indicatori text in loc de tipuri: Tbl = 'A','V','C','N'; Fel = 'ANG','VA','PC','PN'; Articol='-1' comparat ca text.
    VERIFICAT: Salarii_Nou_EvPcPn.sql; Salarii2026_code Actualizeaza_Numarer.
B6. Legaturi prin potrivire de conturi (nu prin cheie): la duplicarea articolului, PC se relega de VA prin
    "PC.ContD=VA.ContC si PC.PartD=VA.PartC". VERIFICAT: DefaSalariiAN bCopy_Click (UPDATE ... SET IDVAA).
B7. Dublu id: DefaSalarii are ID si IDS cu aceeasi valoare; subformularele filtreaza dupa ID, interogarile dupa IDS.
    VERIFICAT: Forms\OCS mPick_ClsfClick (INSERT ID=IDS), DefaSalariiAN RecordSource vs Salarii_Nou_Ang.sql.

## C. Defecte si riscuri observate

C1. Form_Close sterge ORICE articol fara randuri in viramente: `DELETE FROM DefaSalarii WHERE IDS NOT IN (SELECT IDD
    FROM DefaSalariiPC)`. VERIFICAT: Forms\OCS_code Form_Close.
    CERINTA OPERATORULUI (23.10.2026), care transforma defectul intr-o regula: **un articol de sablon salarii NU se
    salveaza fara cel putin un rand PC (viramente)**. Era metoda aleasa in Access ca sa nu mai arate mesaj; in K-BOT
    regula se aplica la SALVARE (validare in serviciu + mesaj clar), nu prin stergere la inchidere. Date: in
    `baza2026.accdb` toate cele 16 articole au cel putin un rand PC (0 fara), deci datele existente respecta regula.
    Intrebare ramasa: «minim un rand PC» conteaza si randurile cu `Ascuns = True`? (Access le numara.)
C2. Salarii_Nou_Ang nu are WHERE: incarca TOATE articolele (si cele Ascuns, si cele de alt capitol). Clasificatia
    se cauta pe cap/forma/articol; daca nu exista, IdClsf = 0. VERIFICAT: queries\Salarii_Nou_Ang.sql.
C3. Ascuns respectat inconsecvent: filtrat la VA si PN, nefiltrat la PC si la articol. VERIFICAT: Salarii_Nou_EvPcPn.sql.
C4. BCM_Click: `UPDATE DefaSalariiVA SET CM=False` fara WHERE - sterge CM pe toate articolele, apoi seteaza unul.
    Idem la PC. VERIFICAT: DefaSalariiVA_code, DefaSalariiPC_code.
C5. Contul de trezorerie diferit: '7700000' (7 cifre) la crearea PC, dar PN seteaza Articol=(ContC="770000")
    (6 cifre) - conditia nu se poate potrivi cu valoarea creata. VERIFICAT: DefaSalariiVA_code linia 125 vs DefaSalariiPN_code
    Form_BeforeUpdate. Efectul asupra "Articol" in PN = PRESUPUS: indicator mereu fals.
C6. Pentru PC mesajul de blocare cere doar PartD, ContC, ContD (nu PartC); la VA toate 4. Regula diferita fara motiv scris.
    VERIFICAT: DefaSalariiPC Form_BeforeUpdate vs DefaSalariiVA Form_BeforeUpdate.
C7. Randuri PN: exact 2 asteptate in alt modul, nimic nu impune asta in sablon. VERIFICAT: Salarii2026 Actualizeaza_Numarer.
C8. Sablonul nu verifica ca tertii (PartD, PartC) exista in Parteneri; verificarea se
    face abia la salvarea lunii (A7). VERIFICAT: SavSalarii.
C9. Cont8067 si NC se creeaza in aceeasi tranzactie dar IDD/IDO se iau din ObtineIDURI (DMax) - concurenta.
    VERIFICAT: SalariiSavDoc2019 SavSalarii (IDURI.IDD + 1 ...).

## D. Coloane citite efectiv

Articol (DefaSalarii): ID/IDS, Clsf (articol), Explicatie, ContD, PartD, ContC, PartC, Cap, Ascuns - citite
(Salarii_Nou_Ang.sql, OCS_header Art). Proiect: listat in Art dar MakeCont primeste mereu 0 (PRESUPUS mort).
SRT si Sef: copiate la duplicare (DefaSalariiAN bCopy_Click), neutilizate altundeva in EV (grep SRT/Sef gaseste doar
balante). PRESUPUS mort.
VA / PC / PN: IDVA/IDPC/IDPN, IDD, Explicatie, ContD, PartD, ContC, PartC, Cap, CM, Ascuns - citite
(Salarii_Nou_EvPcPn.sql). IDVAA doar la PC (legatura cu VA, reconciliere). CM la PN: copiat in Articol de OCS
(bCh4_Click) - confuz; PN.CM nu exista, PN citeste 0 AS CM. VERIFICAT: Salarii_Nou_EvPcPn.sql (PN: `0 AS CM`).

## E. Fluxul de aplicare (intrari necesare)

Intrari: unitatea, capitol (Cap), forma (Forma), luna/an (LA), statul de plata (sume pe articol si pe tert),
optiunea "Zec2" (rotunjire cu 2 zecimale, PRESUPUS dupa nume), apoi la salvare pe fiecare tip: NC, numar document, data,
fel document, jurnal, compartiment, ALG + data ALG. VERIFICAT: Salarii2015 (CAP/Forma/LA), SalariiSavDoc2019 Form_Load.
Pasi: 1) pentru fiecare articol: cont compus + IdClsf din (cap, forma, articol); 2) pentru fiecare VA/PC/PN al articolului
aceeasi compunere; 3) se atasaza sumele din stat; 4) operatorul bifeaza ce salveaza; 5) validare conturi/parteneri;
6) un Document pe tip + cate un Oper+Debit+Credit pe rand bifat, suma = DePlata; 7) Cont8067 daca ALG. Tot intr-o tranzactie.

## F. Model de date propus (PROPUNERE)

Per unitate (id_unitate). Fara coloana de an: nu depinde de exercitiu; luna/anul apar doar la aplicare.
- sablon_sal_articol: id, id_unitate, capitol, articol (sau id_clasificatie economica), denumire, cont_baza_debit,
  tert_debit, cont_baza_credit, tert_credit, activ.
- sablon_sal_linie: id, id_articol, tip (RETINERE | VIRAMENT | NUMERAR), id_parinte (FK la retinere pentru
  VIRAMENT; null altfel), denumire, cont_baza_debit, tert_debit, cont_baza_credit, tert_credit, este_cm, ordonantare
  (doar NUMERAR/VIRAMENT), activ.
- Reguli: tert_* referinta la Parteneri (FK pe IdPartener), nu text. Debit retinere = credit articol (impus de aplicatie,
  nu copiat). Virament are cont_credit de trezorerie dintr-o setare unica per unitate (nu in fiecare rand).
  Un singur "este_cm" per articol si tip. Duplicare articol = comanda aplicatie, nu tabel temporar.
- Parteneri: tabel `Parteneri` si ParteneriForm exista deja (000_DEMO.sql; src\KBot.App\Views\Nomenclatoare\ParteneriForm.vb).
  Clasificatii exista in MariaDB (000_DEMO.sql). Nu exista in MariaDB_Schema (000_DEMO.sql) tabele de sabloane
  salarii, plan de conturi sau jurnale. VERIFICAT.
- Aplicarea: serviciu care produce operatii in memorie (documente + randuri) si le salveaza in tranzactie.

## G. Intrebari pentru operator (max 8)

(Decizii 23.10.2026: sablon pe cont de baza, completat automat; parteneri pe linie = niciunul / cel al operatiei / fix;
subunitate = alta unitate subordonata; `Zec2` -> setare pe subunitate; articol nesalvat fara minim un rand PC.
`Chelt` e fals mereu la postarea salariilor - `SalariiSavDoc2019_code.txt`, linia 376.)

1. (REZOLVAT, 23.10.2026) «Zec2» NU era rotunjire. Salariile bugetarilor se afiseaza si se introduc FARA zecimale;
   unele proiecte finantate din UE cer salariile cu 2 zecimale, iar bifa «Zec2» comuta asta. Operatorul propune sa se
   lege direct de **subunitate** (nu bifa pe ecran). Corectie fata de presupunerea din sectiunea E ("rotunjire").
2. Cum se calculeaza diferit randurile CM (423) si de unde vin sumele (stat extern, Excel)?
3. Contul de trezorerie pentru viramente: este mereu 7700000 sau difera pe unitate/an? Poate fi setare unica?
4. In PN cele doua randuri, ce reprezinta fiecare (ridicare numerar si plata)? Pot fi generate automat?
5. (REZOLVAT, 23.10.2026) Capitolul il hotaraste SUBUNITATEA (65.01 = 01A, 65.02 = 02A, 65.10 = 02E, 65.11 = 01A):
   articolele de sablon salarii se definesc per subunitate, fiecare cu capitolul ei.
6. Se doreste blocare (nu doar avertisment rosu) cand retinerea nu are virament asociat?
7. Jurnalul si compartimentul pentru salarii: raman introduse la salvare sau se leaga de articol?
8. Stergerea unui articol deja folosit in luni salvate: interzisa, sau dezactivare?
