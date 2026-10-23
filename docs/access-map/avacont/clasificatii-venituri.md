# Fisa de citit: Clasificatii Venituri (Avacont -> K-BOT)

> DECIZIE operator (23.10.2026): Clasificatiile de venituri se fac **identic cu clasificatiile normale**. Consecinta
> (VERIFICAT, `MariaDB_Schema/AVACONT_SURSA.sql`): cheltuielile au `Clasificatii` (pe unitate, cu Sector/Sursa/SS) +
> `Clasificatii_Buget` (sume `double`, `An`, `DataInceput`, o linie = o versiune de buget, unic pe clasificatie+an+data) +
> `Clasificatii_Rectificari` (sume `double`, `Data`, `Document`, unic pe clasificatie+data+document, legatura `RESTRICT`).
> Venitul are azi `Clasificatii_Venituri` cu `Trim1..4` ca `int`, fara An, fara versiuni, fara IdUnitate. Deci: tabel de
> buget separat pe versiuni, sume zecimale, modificare/stergere ca la cheltuieli (cu aceleasi interdictii). Atentie:
> `extrase.py` citeste `Clasificatii_Venituri` - orice schimbare de coloane cere verificarea lui inainte (DDL in `sql/`,
> aplicat de operator). Lista exacta de coloane se stabileste in felia de proiectare, nu aici.

Cod: VERIFICAT = citit intr-un fisier numit; PRESUPUS = dedus, de confirmat.
Cale export Access: `AVACONT\EXPORT_VBA` (prescurtat `EV`). Schema: `MariaDB_Schema\*.sql`.
Nu contine cod. Nimic din K-BOT nu a fost modificat pentru aceasta fisa.

## 1. Ce face operatorul in fereastra Access

Intrare: butonul de panglica `btnClsfV` "Calsificatii Venituri" (VERIFICAT, `AVACONT\USysRibbon.xml`
r.105; vizibilitate/activare prin `GetVisible`/`GetEnabled` - regula de drepturi PRESUPUS, nu citita).
Formularul `ClasificatiiBugetareV` (VERIFICAT, `EV\Forms\ClasificatiiBugetareV_code.txt`):

- **Arbore (stanga, 42% din latime)**: 3 nivele Capitol > Subcapitol > Paragraf. Radacina = "C - DENUMIRE"
  (taiata la 50 caractere), subcapitol "S - DEN" (50), paragraf "C.S.P - DEN" (40); numele intreg e in
  tooltip. Datele vin din `qClsfV2025` (VERIFICAT, `EV\queries\qClsfV2025.sql`).
- **Cautare**: lista "Cautare" (C_Capitol) cu `< TOATE CLASIFICATIILE >` + fiecare `C.S.P` existent;
  la alegere arborele se pozitioneaza pe nodul respectiv (`FindNode`). VERIFICAT (code r.34-44, header r.1002).
- **Click pe nod**: umple Capitol/Subcapitol/Paragraf/IdClsfV/Denumire (campuri ascunse in footer) si
  reincarca cele doua grile din dreapta. Grilele se filtreaza cu `Like` pe aceste campuri, deci pe un
  nod de capitol/subcapitol arata TOATE randurile de sub el (`*`), dar sunt dezactivate (read-only);
  doar pe nod de paragraf (nivel 3) sunt editabile. VERIFICAT (code r.312-355; `ClasificatiiV_header` r.61,
  `RectificariV_header` r.31).
- **Grila "Buget Anual"** (`ClasificatiiV`): coloane Clsf (calculat C.S.P), Trim1..Trim4, Total
  (= suma celor 4, calculat in forma). Se editeaza direct. VERIFICAT (`ClasificatiiV_header` r.557-753).
- **Grila "Rectificari bugetare"** (`RectificariV`): Trim1..Trim4, Document, Data, Total calculat, buton
  de stergere pe rand (`DelRec`, cu intrebare de confirmare MsBox 10473). VERIFICAT (`RectificariV_code`,
  `_header` r.262-864).
- **Click dreapta pe arbore** (`Popup_Clasificatii_Venituri`, VERIFICAT `EV\Modules\mdl_Popup2022_code.txt`
  r.3479-3548): "Adauga clasificatie", "Modifca clasificatie", "Sterge clasificatie". In practica doar
  "Adauga" merge (vezi sectiunea 3).
- **Adauga** = fereastra `AddClsf_Venituri` (VERIFICAT `EV\Forms\AddClsf_Venituri_code.txt`): 3 liste
  in cascada Capitol -> Subcapitol -> Paragraf (din `DefaClsfV`, doar din lista, nu se poate lasa gol),
  Denumirea se completeaza automat cu `UCase` din nomenclator la alegerea paragrafului; butoane
  "Salveaza" / "Renunta". La deschidere se preselecteaza ce e deasupra nodului ales (pe nod capitol se
  alege tot; pe nod subcapitol se blocheaza capitolul; pe nod paragraf se blocheaza C si S).
- **Exporta Excel** (`ExportaExcel`): vezi sectiunea 3 - nu functioneaza.
- Subsol: "Iesire"; text-ajutor "Dublu-click deschide ramura; Click-dreapta afiseaza optiuni" (header r.1514).

## 2. Date: Access -> MariaDB

| Access (VERIFICAT unde e indicat) | MariaDB (VERIFICAT `MariaDB_Schema\000_DEMO.sql` r.691-722; `AVACONT_SURSA.sql`) |
|---|---|
| `ClasificatiiV`: IdClsfV, Capitol, SubCapitol, Paragraf, Denumire, Trim1..Trim4 (coloane citite din `RecordSource`, `AddClsf_Venituri_code`; tabela NU e in `EV\tables`; este LOCALA in `baza2026.accdb`, cu 0 randuri: IdClsfV Long (PK, fara autonumerotare), Trim1..Trim4 Long; `RectificariV` are Trim1..4 Double, 0 randuri; `DefaClsfV` nu exista in acel fisier) | `Clasificatii_Venituri`: aceleasi 9 coloane. Fara IdUnitate, fara An |
| `RectificariV`: ID, IdClsfV, Trim1..Trim4, Document, Data (din `RectificariV_header`/`_code`) | `Clasificatii_Venituri_Rectificari`: ID, IdClsfV, Trim1..4, Document, Data + DTQ, DTI (noi) |
| `DefaClsfV` (VERIFICAT `EV\tables\DefaClsfV.md`): ID, C, S, P, D, CV ('VN' filtrat in query) | `AVACONT_COMUN.DefaClsfV`: ClsfV (cod 6 caractere, PK), DENUMIREA. 1429 randuri: 69 capitole (xx0000), 748 subcapitole (xxyy00), 612 paragrafe |
| Total = Trim1+..+Trim4, calculat in forma | nu exista coloana; se calculeaza (spre deosebire de `Clasificatii_Buget.TOTAL`, generata) |
| Migrare: `ClasificatiiV`->`Clasificatii_Venituri`, `RectificariV`->`..._Rectificari` (VERIFICAT `src\KBot.Migrator\Transfer\TableMaps.vb` r.144-205); IdClsfV si ID pastrate. Un singur fisier Access are date | `FX_Extrase_H.IdClsfV` (r.1349 din `000_DEMO.sql`) si `extrase.py` citesc `Clasificatii_Venituri` nequalificat (VERIFICAT) |

**Goluri / diferente (toate VERIFICAT din DDL, afara de cele marcate)**
1. `Trim1..4` din `Clasificatii_Venituri` sunt `int` (fara zecimale); in rectificari sunt `varchar(255)` (text!). Tipul Access nu e exportat (PRESUPUS Currency/Double, suma de bani).
2. Nu exista UNIQUE pe (Capitol, SubCapitol, Paragraf); coloanele sunt NULL-abile (Access impunea unicitatea doar prin lista).
3. Nu exista UNIQUE pe rectificari (Clasificatii_Rectificari are (IdClsf, Data, Document)).
4. Nu exista `An`: bugetul venituri e un singur set de 4 trimestre, nu versiuni pe an cum are `Clasificatii_Buget` (An + DataInceput).
5. Nu exista `IdUnitate`: tabela tine clasificatiile intregii directii (VERIFICAT `extrase.py` r.79-86; `TableMaps.vb`).
6. `IdClsfV`: `000_DEMO.sql` il are AUTO_INCREMENT, `AVACONT_SURSA.sql` il are simplu; comentariul din `AutoIncrementStep.vb` spune ca serverul e "plain". Starea reala = de aflat (Access folosea `DMax+1`).
7. `DefaClsfV` MariaDB nu are coloana `CV`; sensul lui 'VN' nu e documentat (PRESUPUS "venituri", nelamurit). Descompunere cod 6 = C(2)+S(2)+P(2) (PRESUPUS din lungimea 6 si masca Access "AA").
8. Rectificari: FK spre `Clasificatii_Venituri` este RESTRICT (nu se poate sterge o clasificatie cu rectificari).

## 3. Reguli de PASTRAT si mecanisme Access de ELIMINAT

**Pastrat (VERIFICAT in `AddClsf_Venituri_code/_header`, `RectificariV_code`, `ClasificatiiBugetareV_code`):**
- Capitol, Subcapitol, Paragraf, Denumire obligatorii la salvare (mesaj 10482).
- Coduri alese DOAR din nomenclator `DefaClsfV` (limita la lista); subcapitolul filtrat dupa capitol, paragraful filtrat dupa C si S.
- Un paragraf deja existent nu mai apare in lista de adaugare => unicitate C+S+P (de impus si in baza de date).
- Denumire implicita = numele din nomenclator, majuscule.
- Buget si rectificari se editeaza doar pe nod de paragraf; pe capitol/subcapitol sunt doar vizualizare (suma/lista).
- Trimestrele null devin 0 la salvare (rectificari: `Form_BeforeUpdate`).
- Rectificarea primeste automat `IdClsfV` al paragrafului selectat; stergerea cere confirmare.
- Total = Trim1+Trim2+Trim3+Trim4, nestocat.
- Arborele trimite spre alte module `IdClsfV` (extrase) - cheia nu se schimba.

**Eliminat (mecanisme Access):** arbore `clsTreeView`/`subFormVBNET`; meniul `CommandBars("ppMenu")`; pozitionare
manuala `PozCtl`, `Form_Resize`, `GetAsyncKeyState`, timer; `ADD_TRACE`/`REM_TRACE`/`MsBox(id)`/`DebugPrint`;
campuri ascunse in footer (Capitol, Subcapitol, Paragraf, IdClsfV, CLSF, CIA, Cio, CIP, `EndLoad`);
filtrare subformuri prin `Like Forms!...`; `Scheme.NumeForm='Buget'` (doar culoare/titlu "unitAsoc", fara efect de date).

**Cod mort sau defect in Access (VERIFICAT) - NU se porteaza ca atare:**
- `PrelBug_Click` si `GolBug_Click` lucreaza pe tabela `Clasificatii` (cheltuieli), nu pe venituri, si nu au butoane in header (0 aparitii). `PrelBug` deschide `.accdb` anterior cu parola in clar.
- "Modifca"/"Sterge" din meniu sunt activate doar daca `NodeTag = "5"`, dar tag-urile reale sunt "C", "C_S", "C_S_P" => niciodata active; ramura de stergere sterge din `Clasificatii` si citeste `Forms!clasificatiibugetare` (alta forma). Deci modificare/stergere venituri NU exista de fapt.
- `ExportaExcel_Click`: linia ce completa `txt_Export_Excel` este comentata (`'txt_Export_Excel = Export_Clasificatii_Excel`), deci deschide un fisier gol.
- `qClsfV2025`: numele nodurilor de capitol/subcapitol este `Nz([Denumire],[D])`, iar `Denumire` e cea a randului din `ClasificatiiV` => primul paragraf imprumuta numele capitolului/subcapitolului (PRESUPUS bug; intentia pare numele din `DefaClsfV`). Cu `WHERE DCV.P="00"` nivelurile 1 si 2 pierd randurile fara intrare in nomenclator.
- `AddClsf_Venituri`: parametrul "modificare" (`E`) e citit si neutilizat; "Salveaza" face upsert pe C+S+P, dar lista exclude paragrafele existente => editarea nu e atinsa.
- Mesaj de eroare Access: "XX.XX sau XX" copiat de la clasificatiile de cheltuieli (nepotrivit).

## 4. Ce se reutilizeaza din `ClasificatiiForm` (K-BOT) si ce difera

Existent (VERIFICAT `src\KBot.App\Views\Nomenclatoare\ClasificatiiForm.vb`, `ClasificatiiAddForm.vb`,
`PYTHON\routes\forexe\clasificatii_edit.py`, `docs\MAPARE_NOMENCLATOARE.md` r.55 "D5 venituri: not in this slice"):

| Reutilizabil | Difera pentru venituri |
|---|---|
| Schelet fereastra: arbore `AdvancedTreeControl` (cheie `C|..`,`S|..`,`L|id`), grile `KBotDataView` (buget, rectificari, `gridTotal`), subsol cu "+"/"x", flux Salveaza / dirty / inchide, mod "rezumat" read-only pe noduri de deasupra frunzei (= comportamentul Access pe capitol/subcapitol) | 3 nivele (Capitol/Subcapitol/Paragraf) in loc de 4 (Capitol/Subcapitol/Articol/Alineat) si fara gruparea pe `Ss` (sursa-sector) |
| Tipar API: `INomenclatoareApi` + `ReauthGate`; Flask `@require_session`, `get_kbot_connection`, `_Refused`, `_json_utf8`, tranzactie cu rollback | Fara `an` si fara `IdUnitate` in apeluri (tabela e pe baza, nu pe unitate); o singura cheie `IdClsfV` (nu `IdClsf`) |
| Validari de grila: document obligatoriu, data valida, sume numerice (`ReadCorrections`, `_save_budget`) | Fara versiuni de buget (`DataInceput`) si fara regula "data in anul `an`" decat daca se decide la Q1; sume pe tabela alta (`Clasificatii_Venituri_Rectificari`) |
| Mod de introducere nomenclatoare prin dictionare (`read_names`) | Un singur dictionar: `AVACONT_COMUN.DefaClsfV` (cod 6 -> C/S/P), nu 4 dictionare |
| Fereastra de adaugare `ClasificatiiAddForm` ca idee | Aici: cascada simpla C -> S -> P, o clasificatie odata, nu produs cartezian F x E x SS |

Fara echivalent la venituri: "Arata doar folosite in FOREXE", "Arata toate" (miscare in an), "Verifica buget", "Trimite in Access".

## 5. Lista de elemente

| Eticheta | Element |
|---|---|
| EXISTA | Tabelele `Clasificatii_Venituri`, `Clasificatii_Venituri_Rectificari`, nomenclatorul `AVACONT_COMUN.DefaClsfV`; migrarea Access->MariaDB (TableMaps); citirea in `extrase.py` |
| PORT | Fereastra noua (view in `KBot.App\Views\Nomenclatoare`) cu arbore 3 nivele, cautare C.S.P, grila buget (4 trimestre + total) si grila rectificari (Document, Data, 4 trimestre, total, stergere rand) |
| PORT | Rute Flask noi (fisier propriu, ex. `clasificatii_venituri_edit.py`): arbore, citire buget+rectificari, salvare tranzactionala, nomenclator DefaClsfV, adaugare C/S/P; metode noi in `INomenclatoareApi` |
| PORT | Fereastra "Adauga clasificatie venit" (cascada C -> S -> P din DefaClsfV, denumire implicita) |
| PORT | Intrare de meniu pentru "Clasificatii venituri" (nu "Calsificatii") si pagina de ajutor `0000-NN` cu eticheta de slice |
| PORT | UNIQUE (Capitol, SubCapitol, Paragraf) + NOT NULL pe coloane; UNIQUE sau cheie naturala pe rectificari (DDL de pregatit, operatorul il aplica) |
| DE DECIS | Tip sume (int / varchar -> DECIMAL) si, daca da, migrarea datelor existente |
| DE DECIS | Coloana `An` (+ versiuni de buget) sau un singur set de trimestre pe baza |
| DE DECIS | Modificare (doar Denumire?) si stergere clasificatie (interzisa daca exista rectificari sau `FX_Extrase_H.IdClsfV`) |
| DE DECIS | Generare `IdClsfV` (AUTO_INCREMENT sau MAX+1) |
| DE DECIS | Sursa numelor la capitol/subcapitol (DefaClsfV, nu Denumirea frunzei) si filtrul 'VN' |
| DE DECIS | Export Excel (nefunctional in Access) si copierea bugetului din anul anterior (`PrelBug`) |
| ELIMINAT | `clsTreeView`, `CommandBars`, `PozCtl`/resize/timer, trace-uri, campuri ascunse, `Scheme` ("unitAsoc") |
| ELIMINAT | `GolBug_Click`, `PrelBug_Click` (pe tabela gresita, parola in clar), ramura de stergere gresita a meniului, `ExportaExcel_Click` actual |

## 6. Intrebari deschise pentru operator (max 6)

1. **An / versiuni**: bugetul de venituri ramane un singur set de 4 trimestre pe baza (cum era in `baza2026.accdb`), sau adaugam `An` (si eventual versiuni cu data de inceput ca la cheltuieli)?
2. **Tip sume**: trimestrele sa devina numere cu zecimale (`int` pierde bani, rectificarile sunt text)? Se aplica DDL in `sql\` si apoi pe server de catre dumneavoastra.
3. **Modificare si stergere**: in Access nu functionau. Permitem modificarea denumirii si stergerea (cu interdictie daca exista rectificari sau extrase legate prin `IdClsfV`)?
4. **IdClsfV**: pe serverul real este AUTO_INCREMENT sau simplu? Daca e simplu, noul id se ia MAX+1 pe server (cum facea `DMax+1`).
5. **Denumiri in arbore**: la capitol/subcapitol folosim numele din `DefaClsfV` (recomandat) in loc de Denumirea primului paragraf, cum face azi Access? Ce inseamna filtrul 'VN'?
6. **Extra**: vreti export Excel al listei si copierea bugetului din anul anterior (ambele erau defecte/moarte in Access)?
