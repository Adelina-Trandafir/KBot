# Planul de portare AVACONT (Access) -> aplicatia principala K-BOT

> **Starea executiei NU e aici.** Registrul feliilor `AVnn-nn`, ce e inchis si ce urmeaza: `docs/worklog/state/KBOT_STATUS_AV.md` (rezumat: randul `AV` din `docs/worklog/KBOT_STATUS.md`). Partea de E-Factura -> contabilitate (etapa 2, «Contari») se urmareste in familiile 01EF / 03EF / 04EF.

## Context

Aplicatia principala (`AvacontMain`) devine Avacont: contine nucleul contabil si, alaturi, KBOT (FOREXE/angajamente), E-Factura, Vercon, ADECHIT. Butoanele de ribbon care raman se muta in taburile existente, la fel ca in Avacont. NU exista o sectiune "AVACONT".

**Regula de baza (cerinta operatorului): NU portam tot.** Access are 341 formulare, 430 rapoarte, 545 interogari, 275 tabele, dar ribbon-ul expune ~45 de intrari, iar o parte din ele sunt deja rezolvate in KBOT sau sunt moarte. Se portheaza doar ce e accesibil din ribbon, in ordinea etapelor, dupa confirmarea operatorului.

Decizii luate (23.10.2026, revizuite dupa comentariile operatorului):
- Etapa 1: **se portheaza fereastra Clasificatii Venituri**. **NU se portheaza** (confirmat explicit de operator): Adauga/Incarca/Transfera unitate (le face aplicatia de administrare), Backup online + Descarca actualizare, bifele "Stergere conturi" si "Mod Noapte".
- Datele contabile: tabele cu prefix `TZ_` in baza fiecarei unitati, cu coloana `An` (ca Vercon cu `VC_`). Nu se creeaza o baza pe an.
- Date vechi: doar anul curent + solduri initiale; anii trecuti nu se migreaza.
- Familia de felii: **`AVnn-nn`** (nu `00AV-NN`); se verifica totusi in cod/worklog ca numerele `AVnn` nu sunt deja folosite.
- Serviciu unic de postare pe server (Flask) pentru Documente/Oper/Debit/Credit/NoteC; view-urile WinForms doar apeleaza API. (aprobat)
- Se pastreaza **logica** din Access, nu **modul de calcul** si nici neajunsurile lui (ex. partenerul "XXX"). Se refac in stilul KBOT.
- Se ramane doar cu ce e vizibil acum in ribbon; nu se mai cere nimic suplimentar despre formulare "moarte".
- NU se portheaza: Contracte (8067), Ajustari valori 8067, Preluare date TREZO (Extrasele KBOT importa deja din Excel).
- Bifele/numerele vechi **Ord, Ang, FX, NrAng, NrOrd, NrProp NU se mai portheaza**: legatura cu FOREXE urmeaza regulile ALOP (manual: `Surse/GHID UTILIZARE_ALOP_V2.pdf`, de citit inainte de etapa 2).

## Ce exista deja (nu se reface)

- Fereastra `AvacontMain` cu ribbon (Configurari, Angajamente, E-Factura, Extrase, VERCON, ADECHIT); selectoare unitate/an/SS in bara (`KbotForm.Units.vb`, `KbotForm.Periods.vb`); `SessionContext`.
- Clasificatii bugetare (`Views\Nomenclatoare\ClasificatiiForm`), Parteneri (`ParteneriForm`).
- E-Factura emisa/primita (`KBot.EFactura`, `PYTHON/routes/efactura`), "Trimite la Vercon".
- Vercon web (NIR, bon consum, lista alimente, OI/MF, rapoarte) = tabul Gestiuni/Materiale din Access este acoperit functional.
- Setari / date unitate / conturi bancare unitate / istoric versiuni / updater.
- Lipsesc total din KBOT (nici schema, nici rute): plan de conturi, note contabile, operatii, documente, sabloane, salarii, burse, balanta, jurnale, registre.

## Metoda pentru fiecare etapa (anti-"portam tot")

1. **Sub-felie de citire** (fara cod): pentru fiecare formular din ribbon se citeste codul Access si se scrie o fisa scurta in `docs/access-map/avacont/<nume>.md` (ce face, tabele, ce parti sunt moarte). Model: `docs/access-map/efactura/`.
2. Fiecare element primeste o eticheta: **PORT / EXISTA / ELIMINAT / DE DECIS**. Operatorul confirma lista inainte sa inceapa codul.
3. Abia apoi: DDL (`sql/`), API Flask, client (`KBot.Api` + `KBot.Domain`), view in `KBot.App\Views\`, buton in ribbon, ajutor `0000-NN`.
4. Familie de felii: `AVnn-nn` (ex. `AV01-01` = etapa 1, felia 1); documentata in `docs/worklog/` si `KBOT_STATUS.md`.

Conventii obligatorii (CLAUDE.md): fara diacritice in cod, Romana doar in text vazut de operator, `k_` pe parametri/locale la risc, Try/Catch + `GlobalErrorLog`, `KBotMessage.Show`, controale din `KBot.Controls` (KBotDataView etc.), toate controalele in `.Designer.vb`, `AutoScaleMode.Dpi`, utf8mb3_general_ci pentru DDL nou, citit `MariaDB_Schema/` inainte de orice coloana, fara teste/rulari, fara scrieri git, fara deploy pe server (DDL scris in `sql/`, aplicat de operator). Se numara randurile in jurul fiecarui JOIN.

## Etapa 0 - Fundatie (scurta, inainte de etapa 1)

- Conventia `TZ_`: prefix + `An` + chei; ce e per-unitate fara an (configurari) vs per-an (operatii). De confirmat unde stau planul de conturi si sabloanele: in baza unitatii, fara `An`, ca in Access (DefaCont/DefaNote sunt configurari, nu operatii) - de verificat in sursa.
- Extinderea `src\KBot.Migrator\Transfer\TableMaps.vb` pentru tabelele noi (doar anul curent + solduri initiale + configurari).
- Gol de arhitectura: in Access, scrierea notelor (`mdl_Salvare_NoteContabile`: `SavEvPlop`, `SavOPuri2017_2022`, `SaveBurse`, `SaveEFactura`, `OP9.Sav_Mat2015`, `SavSalarii`) e codul critic. **Propunere: un singur serviciu de postare pe server (Flask), view-urile WinForms doar apeleaza API** - o singura sursa de adevar pentru Documente/Oper/Debit/Credit/NoteC.

## Etapa 1 - Configurare (cu sabloane)

| Element ribbon | Stare | Actiune |
|---|---|---|
| Conturi (`AdaugCont`, DefaCont/PlanCont, 762 linii) | lipseste | PORT: tabel + fereastra plan de conturi |
| Clasificatii | EXISTA | doar verificat lipsuri fata de Access (AddClsf, Credite, Buget, Rectificari) |
| Clasificatii Venituri (`ClasificatiiBugetareV`, ClasificatiiV, RectificariV, AddClsf_Venituri) | tabelele exista, FEREASTRA lipseste | PORT: fereastra de editare, dupa modelul `ClasificatiiForm` (tabelele `Clasificatii_Venituri` + rectificari exista deja in MariaDB) |
| Jurnale (`Jurnale`, 21 controale) | lipseste | PORT (mic) |
| Parteneri | PARTIAL: ecran de furnizori exista (in flux 0121-0123), analiticele nu | partener = furnizor SAU analitic (decis 23.10.2026); fisa `parteneri-ca-analitic.md`; plus ParteneriSI (solduri initiale) |
| Sablon materiale/servicii/diverse (`Operatii2020`, `DefaNoteA`/`DefaNote`) | lipseste | PORT cu **logica** pastrata (antet + linii cont debit/credit pe clasificatie, explicatii, tipuri), dar proiectat nou: fara mecanismul partenerului "XXX" si fara modul de calcul din Access. Fisa de citire extrage regulile de business; operatorul valideaza designul inainte de cod |
| Sablon salarii (`OCS`, `DefaSalarii` + `VA`/`PC`/`PN`) | lipseste | PORT, aceeasi abordare (logica, nu mecanismul); se foloseste in etapa 3, configurarea e acum |
| Configurari Avacont (`Setari implicite`) | NU formular separat | setarile necesare merg in **Setari > Aplicatie > pagina «Avacont»**, langa «Documente» si «KBOT» (`src/KBot.App/Setari/SetariAplicatieView`, nav cu `KBotNavItem`). Datele unitatii / E-Factura / conturi exista deja (`unitate_date`, `ef_token`, `unitate_conturi`); se muta doar ce lipseste |
| Bife: Stergere N.C., Stergere clasificatii, Afiseaza cod partener, Protectie operatii | lipseste | PORT ca setari in pagina «Avacont» (consumate in etapa 2: ModificareNC, stergere clasificatie, Note/OP9) |
| Bife: Stergere conturi, Mod Noapte | ELIMINAT | confirmat: nu se portheaza |
| Adauga / Incarca / Transfera date unitate | ELIMINAT | confirmat: raman in aplicatia de administrare |
| Backup online / Descarca actualizare | ELIMINAT | confirmat: Updater-ul KBOT si serverul acopera |

Ordine: Conturi -> Jurnale -> Clasificatii Venituri -> Sablon OCI -> Sablon OCS -> pagina Setari «Avacont» + bife. Livrabil: butoane in taburile existente (Configurari), fara sectiune AVACONT; migrare configurari din `baza2026.accdb`.

## Etapa 2 - Introducere date (fara salarii, burse, extrase)

[Plan detaliat pentru plăți din Contări, OP9 și OPuri2017](PLAN_04EF_01_PLATI_CONTARI_OP9_OPURI2017.md).

Cele mai mari formulare: Note (1659 linii), EFactura_2025 (1378), OPuri2017 (1219), OP9 (734), OPuri (984).

- Nucleu: tabele `TZ_Documente`, `TZ_Oper`, `TZ_Debit`, `TZ_Credit`, `TZ_NoteC` + solduri initiale; serviciul de postare.
- Introducere: Note manuale (`Note`), Operatii diverse (`OP9`), Evidentieri/Plati (`OPuri2017` + `OPuri2017_Salvare` caz `SavEvPlop`).
- **Fara** bifele Ord/Ang/FX si fara NrAng/NrOrd/NrProp: legatura contabilitate <-> FOREXE se proiecteaza dupa regulile ALOP din `Surse/GHID UTILIZARE_ALOP_V2.pdf`. **Pas obligatoriu inainte de cod: citirea ghidului** si o fisa scurta "ce cere ALOP de la o nota contabila".
- **E-Factura -> contabilitate:** macheta E-Factura exista deja (primite, copac KE, legare la receptii). Se adauga doar view-ul de contabilitate «Contari» (portat din `EFactura_2025` / `SaveEFactura`, pe `EF_PrimiteOperatii`), **fara partea de copac** (exista deja in macheta), dar cu optiunile din Access legate de copac care azi lipsesc in macheta existenta (se listeaza in fisa de citire).
- Modificare: evidentieri/plati (`MF2019`), operatii diverse (`ModificareNC1280`), numere documente (`MODNRNC`).
- Alte date: Ordine de plata (`OPuri`) si multiple (`OPME`).
- EXISTA: "Adauga factura vanzare" (E-Factura emisa) - nu se refac `EFACTURA_ADD`.
- NU se face: "Preluare date TREZO" (`OP_TREZO`) - Extrasele KBOT importa deja din Excel. Scrierea in `Cont8067` este scoasa odata cu Contracte/Ajustari 8067 (nu exista tabel `Cont8067` in KBOT). "Gestiuni -> Note contabile" (`TransferVercon`): formularul lipseste din export; se discuta in etapa 7.

## Etapa 3 - Salarii

`Salarii2015` (introducere, `TmpSalariiX` -> `SalariiH`/`Salarii`/`VA`/`PC`/`PN`), `Salarii2019` + `SalariiSavDoc2019` (modificare si postare note). Foloseste sabloanele din etapa 1 si serviciul de postare din etapa 2. `Salarii2026` vs vechi: se citeste ce e activ inainte de port.

## Etapa 4 - Burse si extrase

Burse (`Burse`, `_Fact`, `_Plati`, `SaveBurse`); Extrase: K-BOT are deja pagina "Extrase" (importa din Excel) - se verifica doar ce lipseste fata de Access pentru contari. TREZO nu se portheaza. `Burse_PickIBAN` fara referinte (candidat eliminat).

## Etapa 5 - Extrabilant (FARA FOREXE, FARA ALOP vechi)

Active fixe in folosinta (8030), Dispozitii bugetare (8062), Operatii conturi 8XXX. **Exclus:** Contracte (8067), Ajustari valori 8067, "Buget ForexeBug", orice cod ALOP/FOREXE vechi din Access (FOREXE ramane in KBOT-ul existent).

## Etapa 6 - Rapoarte

Doar rapoartele chemate de butoanele: O.M.F. 1792, Situatii bugetare, Balanta contabila (partea FOREXE exclusa), Fise conturi, Note contabile, Registre/Jurnale. Din 430 de rapoarte se portheaza doar cele din aceste lanturi (se trasaza apelurile in sub-felia de citire). Strategie de randare (grila + export/print/PDF) de decis dupa modelul rapoartelor Vercon/ADECHIT.

## Etapa 7 - Alte treburi

Inchidere conturi Primarie, Inchidere anuala, Istoric operatiuni, Descarca documente, Indexare/reparare (`btn_Index_*` - probabil inutile pe server, de decis), PDF417/cod bare, RustDesk, legatura Vercon->NC. Cluster-ul ADECHIT din Access (Grupe, Prezenta, Plati...) NU se portheaza aici - exista ADECHIT separat.

## Candidate moarte (nu se porteaza fara confirmare)

Cod comentat integral: `Btns_old`, `Operatii2020_Preluare(_E)`, `frmFX_MAIN_ASOC`, `frmFX_MAIN_ORD`, `frmTrace`, `cBtns`, `cFTP`, `mdl_FTP`, `mdl_Ordonantare2026`, `mdl_Zip_Unzip`. Fara referinte: `Copy Of Btns`, `DefaNoteB`, `DefaNumerar`, `Burse_PickIBAN`, `OPuri2017_Salvare` (de verificat: apelat prin nume), `SQL_8XXX`, familia `frmFX_*_OLD`/`*_NEFOLOSIT`. Tinte rupte in ribbon: `TransferVercon`, `OPN`, `Operatii2020N`. Nota tehnica: callback-urile din `AVACONT/EXPORT_VBA/Modules/basRibbonCallbacks_code.txt` sunt un schelet; maparea reala e in `Surse/RawExport/Modules/basRibbonCallbacks.bas.txt`. Nu se cere nimic suplimentar operatorului: se lucreaza doar cu ce e vizibil acum in ribbon.

## Fisiere critice

- Sursa Access: `AVACONT/USysRibbon.xml`, `AVACONT/EXPORT_VBA/{Forms,Modules,Queries,Reports,tables}`, `Surse/RawExport/Modules/basRibbonCallbacks.bas.txt`, `mdl_Salvare_NoteContabile_code.txt`, `AVACONT/DATABASES/baza2026.accdb`.
- KBOT de extins: `src/KBot.App/AvacontMain.Designer.vb` + `AvacontMain.vb` (`Ribbon_ItemClicked`), `KbotForm.Machete.vb` (`MachetaType`), `Views\Nomenclatoare\`, `src/KBot.Migrator/Transfer/TableMaps.vb`, `PYTHON/routes/` (rute noi), `sql/`.
- Modele de urmat: `docs/access-map/efactura/`, `VERCON/PLAN_IMPLEMENTARE.md`, `VERCON/MAPARE_ACCESS_WEB.md`, `docs/MAPARE_NOMENCLATOARE.md`, `ADECHIT/PLAN_IMPLEMENTARE.md`.

## Verificare

Fara rulari si fara teste (regula operatorului). Pentru fiecare felie: build curat al proiectelor atinse, dosar de felie in `docs/worklog/`, rand in `KBOT_STATUS.md`, ajutor `0000-NN` cu eticheta feliei, DDL in `sql/` aplicat de operator. Comparatie cifre: dupa fiecare postare, totalurile din K-BOT se compara de operator cu aceeasi nota din Access pe `baza2026.accdb`.

## Prima actiune dupa aprobare

Etapa 0 + sub-felia de citire a etapei 1 (fise Conturi, Jurnale, Clasificatii Venituri, Sablon OCI, Sablon OCS, Setari implicite -> pagina «Avacont», cele 4 bife ramase), cu lista PORT/EXISTA/ELIMINAT/DE DECIS de confirmat. Cod abia dupa confirmare.
