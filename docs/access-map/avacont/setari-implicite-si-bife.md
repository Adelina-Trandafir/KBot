# Fisa de citire: Setari implicite + bifele din panglica - AV01

> DECIZII operator (23.10.2026): setarile Avacont sunt **per utilizator**; «Stergere N.C.» (`DelNC`) si «Stergere
> clasificatii» (`DelCLSF`) devin **drept de rol**, nu bife in Setari. «Protectie operatii» (S6) nu devine setare: e o regula a ecranului Note (filtrare conturi dupa clasificatie), activa implicit.
> Rolurile exista deja (`Roluri`, `Roluri_Operatii` in `AVACONT_COMUN`) - de citit cum se ataseaza un drept nou.

Buton ribbon Access: `btnCFG` "Configurari Avacont" (USysRibbon.xml:155) -> `DoCmd.OpenForm "Setari implicite"`
(basRibbonCallbacks.bas.txt, `Case "btnCFG"`; refuza in unitate blocata). Formular: `Forms\Setari implicite_code.txt`
(738 linii), definitia in `Setari implicite_header.txt`. Cale export: `AVACONT\EXPORT_VBA`.
Decizia operatorului: in K-BOT NU exista fereastra "Setari implicite"; ce mai trebuie intra in fereastra Setari >
Aplicatie, pagina noua "Avacont" (langa "Documente" si "KBOT").
Marcaj: VERIFICAT = citit in fisierul numit; PRESUPUS = dedus, de confirmat. Nu s-a rulat nimic.

## 1. Cum functioneaza in Access

- VERIFICAT: valorile stau in tabela `Scheme` (NumeForm, C0-C9 Long implicit 0, T0-T3 Text) - `Surse\RawExport\tables\Scheme.md`.
  Tabela e in baza unitatii (`baza2026.accdb`; `Scheme` are 22 randuri, VERIFICAT), deci Access = **pe unitate (fisier unitate-an)**, comuna
  tuturor utilizatorilor. `mdl_TRECERE_AN.txt:183` (`A(21)=7 'scheme`) o copiaza la trecerea anului (PRESUPUS: asta o pastreaza).
- VERIFICAT: `Form_Load` (linii 418-498) citeste randurile `_`, `_MNC`, `_OP9`, `DPIFV`; daca `_` sau `_MNC` lipsesc le creeaza cu 0/False
  si `_MNC.T0 = "ORDER BY NC, DATA, DC"`. `btnSav_Click` (170-271) scrie inapoi (218-222) + UNIT, CAI, LEGUNIT.
- VERIFICAT: cele 4 bife din panglica scriu DIRECT in `Scheme`, fara buton Salvare: `OnActionCheckBox` (basRibbonCallbacks.bas.txt
  ~1420-1465) si `GetPressedCheckBox` (~1550-1582): cbxDelNC -> `_`.C1; cbxDelCLSF -> `_`.C3; cbxTextPart -> `_MNC`.C1; cbxProt -> `_MNC`.C5.
  Existente: cbxDelCT (`_`.C2, "Stergere conturi") si cbxCFPP ("Mod Noapte"; scrie de fapt randul `DarkMode`.C0, nu `_MNC`.C4) -
  AMBELE ELIMINATE de operator, nu se portau.
- VERIFICAT: etichetele panglicii (USysRibbon.xml:165-205): "Stergere N.C.", "Stergere conturi", "Stergere clasificatii",
  "Afiseaza cod partener", "Protectie operatii".

## 2. Fiecare setare din formular

Clasificare: **ISI** = deja in K-BOT; **NOU** = trebuie setare noua; **MORT** = nu o citeste nimeni; **ACCESS** = mecanism doar Access, se lasa.
Nota despre "cine citeste": cautat cu grep in `Forms\*_code.txt`, `Modules`, `Reports`, `queries` (VERIFICAT = gasit; "niciun cititor" = negasit,
PRESUPUS ca nu exista; Access poate citi si prin controale legate de query, nu exclus).

### 2.1 Tab "Optiuni" (P1)

| Control (Scheme) | Sens | Implicit | Cine citeste | Clasa |
|---|---|---|---|---|
| DelCLSF (`_`.C3) = bifa ribbon cbxDelCLSF | permite stergerea clasificatiilor bugetare (cheltuieli si venituri) | 0 | VERIFICAT `Modules\mdl_Popup2022_code.txt:3454,3539` (stergere clasificatie; fara bifa -> eroare mesaj 25) | NOU |
| DelNC (`_`.C1) = cbxDelNC | activeaza butonul "Stergere Nota Contabila" | 0 | VERIFICAT `Forms\ModificareNC1280_code.txt:154` (`StergereNC_Click`, mesaj 33). In formular control marcat `Enabled = NotDefault` (PRESUPUS: dezactivat; se schimba doar din panglica) | NOU |
| DelCT (`_`.C2) | activeaza stergerea conturilor | 0 | niciun cititor gasit (doar panglica/formularul) | ELIMINAT (operator) |
| TextPart (`_MNC`.C1) = cbxTextPart | partenerii se afiseaza/aleg dupa COD (sursa CFPP) nu dupa nume | 0 | VERIFICAT `Note_code.txt:298,565,578`; `OP9_code.txt:218-220`; `OP_TREZO_code.txt:264-266`; `LFC_code.txt:269` (acolo implicit True daca lipseste) | NOU |
| Prot (`_MNC`.C5) = cbxProt | "Protectie operatii": lista de conturi restransa (`CTD.Tag` in loc de `CTC.Tag`) si `AFC = Not Protectie` la nota | 0, DAR fortat True la fiecare deschidere a Notei (`Note_code.txt:295`) | VERIFICAT `Note_code.txt:301,557-578`. Control `Enabled = NotDefault` in formular | NOU (cu intrebarea 3) |
| NDes (`_MNC`.C0) | pastreaza deschisa Nota dupa modificare | 0 | VERIFICAT `Note_code.txt:297,1081` (la iesire, daca nu e bifat, inchide formularul) | NOU |
| MNC_C6 (`_MNC`.C6) | starea initiala a panoului de informatii din subsolul "Modificare N.C." | 0 | VERIFICAT `ModificareNC1280_code.txt:206` + `basRibbonCallbacks.bas.txt:1645` (toggle `tgb_MNC_Info`). Eticheta formularului zice "Ascunde informatii", toggle-ul afiseaza cand e apasat - se contrazic | NOU (intrebarea 4) |
| MNC_T0 (`_MNC`.T0) | ordinea listei din "Modificare N.C.": `NC,DATA,DC` / `Suma,NC,Data` / `Data,NC,Suma` (RowSource Value List) | `ORDER BY NC, DATA, DC` | VERIFICAT `ModificareNC1280_code.txt:210` (RecordSource) si `Modules\mdl_ExportExcel_code.txt:1625` (export Excel, Tip=2). Control `Enabled = NotDefault` | NOU |
| Istoric (`_MNC`.C2) | pastreaza istoricul notelor | 0 | MORT: `Istoric_Click` spune ca optiunea e eliminata din 22/03/2019; cititorul e comentat (`Note_code.txt:299`) | MORT |
| CFPP (`_MNC`.C4) | "pastreaza bifat CFPP in Nota" | 0 | niciun cititor al `_MNC`.C4 | ELIMINAT (operator) |
| MNC_C3 (`_MNC`.C3) | "ascunde asocierile plati-facturi" | 0 | `Note_code.txt:300` il pune in `Legaturi`, variabila nefolosita nicaieri (grep). Mai mult: `btnSav_Click` nu-l scrie (lipseste din UPDATE-ul `_MNC`, linia 219) | MORT |
| C0_OP9 (`_OP9`.C0) | "activeaza modificarea conturilor in operatii" (avertisment de risc in tooltip) | 0 | niciun cititor (`_OP9` apare doar comentat in `mdl_2025_code.txt:698`) | MORT |
| C5_OP9 (`_OP9`.C5) | max. facturi / plata (lista 10;15;20;25;30) | 10 | niciun cititor | MORT |
| O_NrCarOP | max. caractere pe O.P. (35-255) | 70 | Se CITESTE din `DPIFV`.T1 (linia 493) dar se SCRIE in `OPTIUNI`.T1 (222) - nepotrivire. Export-ul XML are 70 fix: `Modules\ExportXML_code.txt:54,138` (`Mid(!Explicatie,1,70)`) | MORT |
| "A" ("GLOBAL Afiseaza erori din log.txt") | - | - | control dezactivat, nelegat, fara cod | MORT |
| Server/Port/User/Pass/SSL (`tblFTP`) | parametri FTP | - | incarcati in `Form_Load:573-579`, NESALVATI; `tblFTP` nu apare in alt cod (grep). Contine parola in clar | MORT / ACCESS (nu se porteaza) |

### 2.2 Tab "Date unitate" (P4) - subformular `UNIT subform`, tabela `UNIT` (1 rand)

| Camp | Cine citeste | Clasa |
|---|---|---|
| NumeUnitate, CUI, Adresa, Judetul, Orasul | VERIFICAT peste tot (`mdl_EFactura_code.txt:1075`, `OPME_code.txt:100-102`, `qFacturi_Vanzare.sql`) | ISI: `Unitati.NumeUnitate/CF` + `AVACONT_COMUN.Unitati_Date` (Denumire, CodFiscal, Adresa, Orasul, Judetul, Mail, Telefon) - `MariaDB_Schema\AVACONT_COMUN.sql` (tabela la linia ~19694); fereastra `src\KBot.EFactura\Forms\DateUnitateForm.vb` (butonul "Date unitate"), rute `/api/efactura/furnizor` (`PYTHON\routes\efactura\factura_routes.py`) |
| Sector + Sursa (`SectorSursa`), CodProgram | VERIFICAT `Forms\Form2_code.txt:63`, `Modules\mdl_FX_Tasks_Send_code.txt:17`, `queries\SAV_OPER_EF.sql` | ISI: `Unitati_Ani(DC,AN,SS,CodProgram)` si `CAI.Sursa/CodProgram` (schema); PRESUPUS: se seteaza din administrare, nu de operator |
| Director, Contabil | VERIFICAT in rapoarte: `Reports\Balanta 5 Egalitati_code.txt:233-234` (+3 baze de balanta) | NOU, pe unitate (intrebarea 5). PRESUPUS: rolurile exista in `Unitati_Utilizatori.Rol`, dar numele de pe hartie nu e acolo |
| SursaCont | VERIFICAT `Forms\OPuri_code.txt:266,892`, `Note_OPuri_code.txt:48` (IBAN trezorerie = `RO00TREZ` + SursaCont) | NOU, pe unitate (viitor ecran OP) |
| Gestionar, Secretar | niciun cititor gasit | MORT |
| Alte (-1) | `Note_code.txt:277`: daca <> -1 verifica expirarea abonamentului (`Scheme` rand `SR`, linia 280) | ACCESS (licenta) |
| AlteDetalii | `Balanta_code.txt:757` (eticheta unitatii, din clasa `cUnit`) | ISI: `CAI.AlteDetalii` / `000_DEMO.Unitati.Detalii` (PRESUPUS ca acelasi sens) |
| 4 semnaturi (butoane Director/Contabil/Comp. Specialitate/CFD) | tabela `Smn(Den,ATT)` = CALE imagine copiata in `CPP\IMG\<EXT>`; citita de rapoarte (`Reports\OrdonantarePlata1280_header.txt:2241` `DLookUp("Att","Smn",...)`) | ACCESS ca mecanism (cale pe PC). Nevoia (imagine semnatura pe rapoarte) = NOU doar daca se portaza rapoartele; semnarea DDF/ORD din K-BOT e digitala (`FX_PDF_SEMNATURI`) |
| Salvarea scrie si `CAI` (NumeUnitate, AlteDetalii, SURSA), `CALE`, `LEGUNIT` | `btnSav_Click:204-215` | ACCESS |

### 2.3 Tab "Clasificatii" (P5) si "Conturi" (P9)

- P5: 5 subformulare (`DefaSector`, `DefaSursa`, `DefaClsfF`, `DefaClsfE`, `DefaTitlu`) = editare de NOMENCLATOR global, nu setare.
  ISI ca date: `AVACONT_COMUN.DefaSursaSector`, `DefaClsfE/F/V`, `DefaTitlu`, `DefaSS/SSS`, `DefaArticol`, `DefaProgram` (VERIFICAT schema SQL);
  K-BOT le citeste (`PYTHON\routes\inregistrare\nomenclatoare.py`), nu le editeaza. Editarea = administrare (PRESUPUS aplicatia Admin). NU intra in pagina Avacont.
- P9: `CautCont` + `DefaCont subform` = plan de conturi. `Cont3` (bifa "conturi sintetice din 3 caractere") filtreaza doar combo-ul `CautCont`
  din acelasi formular (header linia 2333) -> MORT. Plan de conturi: NU exista in MariaDB (`MariaDB_Schema` fara `DefaCont`), vezi `conturi-plan-de-conturi.md`. Nu e setare.

### 2.4 Tab "E-FACTURA"

| Camp (Scheme `DPIFV`) | Sens / cititor | Clasa |
|---|---|---|
| DIPFV_T3 seria | `mdl_EFactura_code.txt:1080`, `EFACTURA_ADD_code.txt:192`; blocat daca exista facturi (`DIPFV_T3_BeforeUpdate`) | ISI: `Unitati_Date.SerieFactura`, regula "blocat dupa prima factura" in `PYTHON\routes\efactura\facturi.py` (409 SERIE_BLOCATA) |
| DIPFV_C5 numar initial | `mdl_Functii_Generale_code.txt:1870` | ISI: `Unitati_Date.NumarInitial` (implicit 1) |
| DIPFV_C2 afiseaza lista facturilor primite inainte de descarcare | `mdl_EFactura_code.txt:57` (`afiseazaTMPEF`) | ISI ca stocare: `Unitati_Date.AfiseazaPrimiteNoi` (DateUnitateForm). Cititor in fluxul K-BOT de primite: negasit prin grep (intrebarea 8) |
| DIPFV_T1 autorizatie (ascuns), DIPFV_T2 cont bancar (ascuns) | niciun cititor al `DPIFV`.T1/T2 | MORT; contul bancar = ISI in `Unitati_Conturi` (rute `/api/efactura/furnizor/conturi`, fereastra "Conturi Unitate") |
| Token / RefreshKey / TokenExpiry, "Obtine Cod unic" (`EF.EXE`), "Elimina Cod unic" | registry `HKCU\...\AVACONT\<DC>\Tokens` (`GetToken_Click:664-719`) | ACCESS ca mecanism; ISI ca functie: `AVACONT_COMUN.EF_Token` + rute `/api/efactura/token/start|cod|stare` (`token_routes.py`). "Elimina" nu are ruta in K-BOT (VERIFICAT: doar cele 3 rute) |
| WithVercon, bActV, bDezV | `bActV_Click` = "NU E IMPLEMENTAT (05.02.2025)", `bDezV` comentat | MORT |

### 2.5 Tab "SUBUNITATI"

- VERIFICAT: subformular `SUBUNITATI` pe `tmpLEGUNIT` -> `LEGUNIT(IdUnitate,Cale,Buget,SclavBuget,ParinteBuget,OP,EF)` + `CAI.IdUnitateParinte/UnitatiSclav`
  (`btnSav_Click:206-215`). Sens: plaja comuna OP, plaja comuna E-FACTURA, buget comun / extern / local intre FISIERE .accdb separate.
  Randul `Scheme` `Buget` (C1,C2) il citesc `ClasificatiiBugetare_code.txt:52`, `tmpLEGUNIT_code.txt:47`. -> **ACCESS** (K-BOT are o singura baza MariaDB per unitate);
  nu se porteaza. `AD_Subunits` / `VC_Subunitati` din K-BOT sunt alt lucru (PRESUPUS).

### 2.6 Randuri `Scheme` citite dar NEeditate in acest formular (doar de stiut)

`Taskbar`.C0 (`cFrms_code.txt:646`), `SR`.T0-T2 (licenta, `Note_code.txt:280`), `Vercon`.C0 (`ZEC()`, implicit 4, `mdl_Functii_Generale_code.txt:845`),
`OPTIUNI`.C1 (`NoVisWindow`, `mdl_Autoexec_code.txt:80`), `DarkMode`.C0 (panglica). Nu intra in pagina Avacont.

## 3. Setari noi propuse (pagina Avacont)

Toate bifele/combo-urile salveaza la schimbare (ca restul paginii), nu cu buton. Nume cheie = proprietatea din `AppSettings` (ASCII, engleza),
cheia JSON e acelasi nume. Scop implicit propus: **pe utilizator** (`app_settings.json`) - vezi intrebarea 1.

| Cheie propusa | Tip | Implicit | Din | Ecran consumator (viitor) |
|---|---|---|---|---|
| `AvacontAllowDeleteVoucher` | Boolean | False | DelNC | Modificare Note contabile (butonul de stergere) |
| `AvacontAllowDeleteClassification` | Boolean | False | DelCLSF | `src\KBot.App\Views\Nomenclatoare\ClasificatiiForm.vb` (are deja coloana de stergere `ColDelete`) + venituri |
| `AvacontShowPartnerCode` | Boolean | False | TextPart | Note contabile, OP9 / OP Trezo, LFC |
| `AvacontAccountProtection` | Boolean | False | Prot | Note contabile (intrebarea 3: poate ramane doar comutator de sesiune) |
| `AvacontKeepVoucherOpen` | Boolean | False | NDes | Note contabile |
| `AvacontVoucherInfoVisible` | Boolean | False | MNC_C6 | Modificare N.C. (panou informatii) |
| `AvacontVoucherSort` | text enum: `NcDateDc` / `AmountNcDate` / `DateNcAmount` | `NcDateDc` | MNC_T0 | Modificare N.C. + export Excel. Se tine COD-ul, NU textul SQL `ORDER BY` (codul il traduce) |

Date PE UNITATE (nu merg in `app_settings.json`; intrebarea 5): nume Director / Contabil pentru rapoarte, `SursaCont` (trezorerie), imagini de semnatura.
Locul firesc: coloane noi in `AVACONT_COMUN.Unitati_Date` editate din `DateUnitateForm` - PRESUPUS, nu pe aceasta pagina.

## 4. Mecanismul K-BOT existent (VERIFICAT) si cum se adauga pagina (PROPUNERE)

Cum stau pe disc paginile de azi (`src\KBot.App\Setari\SetariAplicatieView.vb`, comentariul clasei):
- "Generale" + "KBOT" + `Excel ribbon`: `src\KBot.Common\AppSettings.vb` -> `%APPDATA%\AVACONT\KBot\app_settings.json`, **per utilizator Windows**, nu pe unitate si fara API
  (`Save()` scrie fisierul, face instanta `Current` si ridica `AppSettings.Changed`; lipsa/gol = implicit; fisier stricat = implicit + `harness_errors.log`).
- "Documente": motorul PDF in `kbot_paths.json` (**per masina**, `AdobeViewerSettings.Persist`), restul in `app_settings.json`; bifele Adobe in registry Adobe.
- Foldere: `settings.json` (`SetariFoldere`). Sumar: `docs\SETARI_UTILIZATOR.md` sect. 6 / 6.1.
- Pe unitate, serverul are doar `Setari(Cheie,TextVizibil,Valoare,Tip)` din fiecare baza (`MariaDB_Schema\000_DEMO.sql`), cu **GET `/api/setari` doar citire**
  (`PYTHON\routes\setari.py`; randurile le pune administratorul prin SQL, `sql\0100_02_setari.sql`). Nu exista PUT -> o setare pe unitate cere ruta noua.
- Cele 3 pagini sunt file ale unui `KBotNavList` orizontal `navPagini` (Key `generale` / `documente` / `kbot`), fiecare cu un panou `KBotTableLayoutPanel`.

Pasi propusi (copiat dupa pagina "KBOT", felia 0777):
1. `AppSettings.vb`: proprietatile din sect. 3 (cu valoare implicita) + `ToDto` (~l.632-720) + `FromDto` (l.724) + clasa `AppSettingsDto` (~l.824).
2. `SetariAplicatieView.Designer.vb`: `KBotNavItem4` (Key `avacont`, Text `Avacont`) in `navPagini.Items`; panou `tlyPaginaAvacont` (AutoScroll, Visible False, ca `tlyPaginaKbot`)
   cu titlu, grup "Note contabile" si grup "Stergeri" (checkbox-uri) + combo pentru ordine. Tot in Designer (regula formelor).
3. `SetariAplicatieView.vb`: `PAGE_AVACONT`; `Case` in `NavPagini_SelectionChanged` + adaugat in tabloul `{tlyGenerale, ...}` (linia 69, altfel pagina ramane vizibila);
   `IncarcaAvacont()` apelat din `Activated`, `UrmeazaSetarile` si calea de eroare din `SalveazaComutator`; handlere cu `SalveazaComutator(Sub(s) s.X = ..., mesaj)`;
   `ApplyTheme` (panoul si etichetele). Fara culori fixe; nume cu prefix `k_` la parametri/locale noi.
4. Consumatorii citesc `AppSettings.Current.X` la folosire si urmaresc `AppSettings.Changed` (pattern `AvacontMain`).
5. Ajutor: sectiunea din `src\KBot.App\HelpContent\contabil\setari.md` cu eticheta `<!-- slice: NNNN -->` + `docs\SETARI_UTILIZATOR.md` 6.1 (regula CLAUDE.md); felie `0000-NN`; `Check-Help.ps1`.
6. Teste: doar la cererea explicita a operatorului.
Pagina e utila abia cand exista ecranele consumatoare (Note, Modificare N.C., OP, LFC lipsesc in K-BOT azi - `conturi-plan-de-conturi.md`); pana atunci cheile ar fi nefolosite.

## 5. Intrebari pentru operator (max 8)

1. Scop: **pe utilizator** (`app_settings.json`, simplu, ca "KBOT") sau **pe unitate** (ruta PUT noua + `Setari`)? In Access bifele erau comune tuturor celor ce lucreaza pe unitate.
2. DelNC si DelCLSF sunt lacate de siguranta: raman bife sau le inlocuiesc drepturile pe roluri (`Roluri_Operatii`)?
3. Prot ("Protectie operatii") e fortat True la fiecare deschidere a Notei (`Note_code.txt:295`): il pastram persistent sau doar comutator de sesiune pe ecranul Note?
4. MNC_C6: eticheta zice "ascunde informatii", toggle-ul panglicii le AFISEAZA cand e apasat. Care e sensul dorit?
5. Director / Contabil / SursaCont / imagini semnatura: se portaza (rapoarte, OP)? Daca da, coloane in `Unitati_Date` sau luate din `Unitati_Utilizatori`?
6. Facem pagina acum sau cand apar ecranele consumatoare? Si ramane vizibila mereu sau sub "optiuni avansate" (ca "Documente")?
7. Confirmati ca se lasa: MORT (Istoric, MNC_C3, C0/C5_OP9, O_NrCarOP, "A", FTP, Cont3, Gestionar, Secretar, WithVercon/bActV) si ACCESS (CAI/CALE/LEGUNIT, SUBUNITATI, plaje comune, token in registry)?
8. `AfiseazaPrimiteNoi` e salvat de `DateUnitateForm` dar nu l-am gasit citit in fluxul de facturi primite: ecranul de primite trebuie sa-l citeasca?
