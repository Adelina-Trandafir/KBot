# Fisa de citire: Modificare / Stergere operatii contabile (ModificareNC1280) - AV02-05

Buton ribbon Access: `btnMNC_LST` (tab «Introducere / Modificare», grupul «Modificare note»). Formular: `ModificareNC1280`
(«Modificare / Ștergere operații contabile»), cu sub-formularul lista `MNotaC1`, panoul de informatii `MNotaC11`,
filtrul plutitor `frmFiltru` si ribbon-ul propriu «MNC». Stare in K-BOT: **LIPSESTE**.
Marcaj: VERIFICAT = citit in fisierul numit; PRESUPUS = dedus; NEEXPORTAT = nu exista in exporturi.

Plan: [`docs/PLANS/PLAN_AV02_05_MODIFICARE_OPERATII.md`](../../PLANS/PLAN_AV02_05_MODIFICARE_OPERATII.md).

## 0. Decizii deja luate (Adelina, 09.11.2026)

- Felia **AV02-05**; folder `Avacont/ModificareDate/Operatii/` (App, Api, Domain), `PYTHON/routes/avacont/modificare_date/operatii/`.
- **Pagina in AvacontMain** (nu fereastra modala); ribbon-ul «MNC» devine grupul din dreapta al paginii.
- **Fara «Modifica operatie» acum**: vine odata cu portarea formularului `Note`. Acum: lista + stergere.
- **Listarile** (nota contabila in 4 variante, fisa cont debit / credit): **amanate la etapa 6** (rapoarte).
  Ordonantare / Propunere / Angajament bugetar-individual-legal: **ELIMINAT** (decizia 23.10.2026, regulile ALOP).
- **Coloanele din dreapta**: CH si CF raman bife; PA si FX ies. **«Ord» = starea documentului de ordonantare**
  (nu bifa). **Coloana noua «R»** = are receptie (da / nu). Detalii la 2.3.

## 0.1 Raspunsurile Adelinei (10.11.2026) - fisa CONFIRMATA

1. **«Ord» = ICONITE, nu cuvinte**: cate o iconita pe etapa (generat, semnat AB, semnat CD, ordonator); gol = fara ordonantare.
2. **«R» = ICONITE**: una pentru TOATE facturile cu receptie, alta (vizibil diferita) pentru PARTIAL; niciuna = fara iconita.
3. **Mai multe filtre deodata, cate unul pe coloana: DA.** Se face in `KBotDataView`, ca sa poata orice tabel.
4. **Operatie cu ordonantare generata: stergerea REFUZATA.**
5. **Soldul partenerului din panou include soldul initial.**
6. **Ordinea listei si starea panoului: tinute minte per utilizator.**
7. **Listele «operatii de inchidere», «salarii», «Centralizator»: mai tarziu.**
8. **Butonul NU merge in Configurari.** Tab NOU **«Introducere date»** cu grupul **«Introducere Date»** (acolo se MUTA
   butonul «Operatii» din Configurari > «Date») si grupul **«Modificare Date»** (butonul nou «Modificare operatii»).
- **Cascada in Access: VERIFICAT de operator** - legaturile `Oper -> Debit / Credit` sunt cu stergere si actualizare in
  cascada, deci in Access se sterge TOT. In K-BOT legaturile sunt `RESTRICT`: serverul sterge liniile explicit.

## 1. Ce vede si ce face operatorul in Access

VERIFICAT (`Forms/ModificareNC1280.txt`, `ModificareNC1280_code.txt`, `Forms/MNotaC1*.txt`, `queries/qMNC2025.sql`,
`queries/MNotaC1.sql`):

- O lista cu toate operatiile anului, cate un rand pe operatie: **NC, Doc., Data, Clasificatie, Fel, Jurn., Explicatie,
  Cont Debit, Cont Credit, Suma oper**, apoi bifele **CH, PA, FX, OR, CF**, plus o iconita la inceputul randului dupa felul
  documentului (P, E, T, I, A).
  - «Data» este **data documentului**, nu data operatiei.
  - Contul apare ca `cont` sau `cont.Pcodpartener` cand are partener.
  - Suma: cea mai mica dintre debit si credit (operatie simpla pe ambele parti), altfel suma partii completate.
- Ordinea implicita: `NC, Data, Doc` (setarea `MNC_T0`, alte variante `Suma,NC,Data` / `Data,NC,Suma`).
- **Antetul coloanelor sunt butoane**:
  - clic stanga = ordonare: crescator (verde) -> descrescator (portocaliu) -> scos; mai multe coloane, in ordinea clicurilor;
  - clic dreapta = filtru plutitor sub coloana: lista de valori (NC, Doc, Clasificatie, Fel, Jurnal, Cont debit,
    Cont credit), text exact sau «CONTINE» (Explicatie), interval de date «DE LA / PANA LA» (Data), lista de sume sau
    interval (Suma). **Un singur filtru o data**: fiecare il inlocuieste pe cel dinainte. «Reseteaza» il scoate.
  - butonul din dreapta antetului («Curata») scoate ordonarea si filtrul.
- **Tastatura** in lista: Enter = modifica randul; Sus / Jos = randul anterior / urmator; Shift+Sus / Shift+Jos = o pagina;
  literele O (Data), C (Cont debit), E (Explicatie), F (Fel), J (Jurnal), L (Clasificatie), N (NC), D (Doc), S (Suma)
  deschid filtrul coloanei. Cont credit nu are tasta.
- Randul ales e marcat cu «►» pe fundal verde deschis. Dublu-clic = modifica.
- **Subsol**: «IESIRE», «Sterge Nota» (ascuns), «Sterge Document», «Sterge operatie», «Modifica operatie».
- **Panoul «Informatii suplimentare»** (`MNotaC11`, fereastra plutitoare, pornit / oprit din ribbon sau clic dreapta pe rand):
  IdDocument, IdOperatie, tipul operatiei, luna si valoarea Forexe, factura platita (INTEGRAL / PARTIAL), ordinul de plata
  (numar / data), lista facturilor platite de o plata, denumirea partenerului, soldul partenerului pana la data
  documentului (debit, credit, sold, tip) si soldul anual.

## 2. Propunere, element cu element

### 2.1 Lista

| Element Access | Eticheta | In K-BOT |
|---|---|---|
| Coloanele NC, Doc., Data, Clasificatie, Fel, Jurnal, Explicatie, Cont debit, Cont credit, Suma | **PORT** | aceleasi, din `TZ_NoteC` / `TZ_Documente` / `TZ_Oper` / `TZ_Debit` / `TZ_Credit` + `TZ_PlanCont`, `Parteneri`, `Clasificatii`, `TZ_Jurnale`; Data = data documentului, ca in Access |
| Iconita dupa felul documentului | **PORT** | iconita dupa `TZ_Documente.Fel` |
| Bifele CH (`Chelt`) si CF (`Cfpp`) | **PORT** | bife (doar citire) |
| Bifa PA (`Oper.Angajament`) | **ELIMINAT** | decizia 09.11.2026 |
| Bifa FX (`Cont8067`) | **ELIMINAT** | `Cont8067` nu exista in K-BOT (decizia 23.10.2026) |
| Bifa OR (`Oper.Ordonantare`) | **PORT, schimbat** | devine coloana «Ord» = starea ordonantarii (2.3) |
| — | **NOU** | coloana «R» = are receptie (2.3) |
| Un rand pe operatie | **PORT, reparat** | Access dubla randul cand o factura era platita in doua plati (`qMNC2025` fara grupare); in K-BOT serverul da un singur rand pe operatie |
| Ordinea implicita `NC, Data, Doc` | **PORT** | |
| Setarea `MNC_T0` (alta ordine implicita) | **DE DECIS** | propunere: tinuta minte per utilizator (setarile locale) pana exista pagina Setari «Avacont» |
| Verificarea de la deschidere «Exista erori in baza de date! ... #EROARE CONT#» | **ELIMINAT** | in K-BOT cheile straine nu lasa operatii fara cont sau document |

### 2.2 Ordonare, filtre, tastatura

| Element Access | Eticheta | In K-BOT |
|---|---|---|
| Clic pe antet = ordonare crescator / descrescator / scos, pe mai multe coloane | **PORT** | |
| Clic dreapta pe antet = filtru (lista de valori, «contine», interval de date, interval de sume) | **PORT** | |
| Un singur filtru o data | **DE DECIS** | propunere: cate un filtru pe fiecare coloana, toate aplicate impreuna; «Curata» le scoate pe toate |
| Filtrul pe Cont debit / Cont credit ofera doar simbolul contului (nu se potriveste cu randurile cu partener) | **PORT, reparat** | valorile oferite = exact ce se vede in coloana |
| Ordonare / filtru pe coloanele FX si CF (in Access dau eroare) | **PORT, reparat** | merg pe CH, CF, Ord, R ca pe oricare alta |
| «Curata» (ordonare + filtru) | **PORT** | buton in ribbon; reface si culorile antetului (Access le lasa) |
| Ribbon «Ordonare implicita» (`btn_MNC_S1`) | **PORT** | |
| Ribbon «Ordonare suma» (`btn_MNC_S2`) | **ELIMINAT** | in Access nu face nimic |
| Tastele O C E F J L N D S, Sus / Jos, Shift+Sus / Shift+Jos | **PORT** | Enter si dublu-clic nu fac nimic acum (fara modificare) |
| Tasta P (cautare partener, neimplementata) | **ELIMINAT** | |
| Butoanele «Sari la primul / ultimul rand» | **ELIMINAT** | Ctrl+Home / Ctrl+End in lista |
| `Filtru_b_Click` (deschide formularul «Filtru», care nu exista) | **ELIMINAT** | |

### 2.3 Coloanele «Ord» si «R» (cerinte noi, 09.11.2026)

- **«Ord»** - starea documentului de ordonantare legat de operatie:
  - gol = operatia nu are ordonantare (`TZ_Oper.IdOrdTbl` gol);
  - «Generat» = exista randul in `FX_ORD_TBL` (ordonantarea e facuta, nesemnata);
  - apoi etapa semnaturii, din `FX_ORD.Semnatura`.
  - **De confirmat - numele semnaturilor:** la ordonantare rolurile salvate sunt **`AB`, `CD`, `Ordonator`**
    (VERIFICAT `PYTHON/routes/forexe/pdf.py:125`; la DDF sunt `A`, `B`, `Ordonator`). Propunere de afisare:
    «Generat» -> «Semnat AB» -> «Semnat CD» -> «Ordonator» (se arata ultima etapa atinsa). Spune daca vrei alte cuvinte
    (de ex. «Semnat A» / «Semnat B»).
- **«R»** - da / nu: factura de care tine operatia are receptie.
  - operatie de evidentiere: factura prin `EF_PrimiteOperatii.IdOperatie` -> `IdPrimita`;
  - operatie de plata: facturile platite prin `TZ_PlatiFacturi.IdPlata` -> `IdPrimita`;
  - «are receptie» = aceeasi regula ca la «Protectie contari Plati/Receptii» (`plati.are_receptie`, `plati.py:161`).
  - **DE DECIS:** o plata care plateste mai multe facturi, din care doar unele au receptie - propunere: «R» doar cand
    TOATE au. O operatie fara factura (nota manuala, operatie diversa, factura tastata de mana) = «R» gol.

### 2.4 Panoul de informatii

| Element Access | Eticheta | In K-BOT |
|---|---|---|
| Panoul pornit / oprit din ribbon (`tgb_MNC_Info`) | **PORT** | panou in subsolul paginii (nu fereastra plutitoare), urmeaza randul ales |
| Starea panoului la deschidere (`MNC_C6`) | **DE DECIS** | propunere: tinuta minte per utilizator (Access nu o aplica si nu o salva) |
| IdDocument, IdOperatie | **ELIMINAT** | numere interne |
| Tipul operatiei (felul documentului sau «MANUALA») | **PORT** | |
| Luna / valoarea Forexe | **ELIMINAT** | `Cont8067` |
| Factura platita INTEGRAL / PARTIAL (la evidentieri) | **PORT** | din `TZ_PlatiFacturi` |
| Ordinul de plata (numar / data) si facturile platite (la plati) | **PORT** | din `TZ_OPuri` / `TZ_PlatiFacturi`; in Access lista urma alt rand decat cel ales - reparat |
| Partenerul, soldul lui pana la data documentului si soldul anual | **PORT** | **DE DECIS:** Access aduna doar operatiile; propunere: plus soldul initial al partenerului (`TZ_SoldInitial` / ParteneriSI) |

### 2.5 Stergere

| Element Access | Eticheta | In K-BOT |
|---|---|---|
| «Sterge operatie» (`STO`) | **PORT** | buton in ribbon + tasta Delete |
| «Sterge Document» (`STD`) | **PORT, reparat** | aceleasi verificari ca la operatie, pentru fiecare operatie a documentului (Access nu verifica plata si nota de inchidere) |
| «Sterge Nota» (`StergereNC`, ascuns; «AM ELIMINAT ACEASTA OPTIUNE!») | **ELIMINAT** | |
| Bifa `DelNC` / «Stergere N.C.» | **ELIMINAT** | decizia 23.10.2026 a facut-o drept de rol; fara «Sterge Nota» nu mai are ce porni |
| Jurnalul de actiuni (`Actiuni`, mesajul 234) | **PORT, altfel** | fiecare stergere se scrie in jurnalul de actiuni al serverului (`PYTHON/routes/actiuni.py`), nu intr-un tabel nou |

**Reguli de stergere** (serverul hotaraste; butonul se stinge, cu tooltip, cand randul e blocat):

1. **Refuzat** cand data e intr-o perioada inchisa: pana la ultima nota de inchidere (`postare.data_ultimei_inchideri`) sau
   an anterior dupa luna ianuarie (ca `EInchis`, VERIFICAT `Modules/mdl_Functii2022_code.txt:10`).
2. **Refuzat** pentru operatiile de inchidere (`Fel = 'I'`): «nu se sterg in felul acesta».
3. **Refuzat** pentru o singura operatie dintr-o nota de inchidere (numarul documentului incepe cu «CCV»; mesajul 10290).
4. **Refuzat** cand factura evidentiata are plati (mesajul 10340 «Operatia selectata este platita! NU SE POATE STERGE!»).
5. **Refuzat** cand ordinul de plata e blocat: intr-un OPME sau confirmat de extras (`plati.motiv_blocare`).
6. **DE DECIS:** operatie cu ordonantare generata (`IdOrdTbl` plin) - propunere: **refuzat** (Access nu stia de ea).
7. O plata care a platit facturi **intreaba intai**, cu lista facturilor care devin neplatite (mesajul 10350).
8. Altfel intreaba «Doriti stergerea operatiei selectate?» cu documentul, data, conturile si suma (mesajul 36, in Access
   cu campuri goale - reparat).
9. Ce se sterge (o tranzactie): la plata ca in «Sterge plata» (`plati.sterge_plata`: alocarile, ordinul, liniile, operatia);
   la evidentiere legatura cu factura (`EF_PrimiteOperatii`) si factura redevine necontata; apoi liniile debit / credit,
   operatia, documentul ramas gol, nota ramasa goala. Access sterge tot documentul la E/P chiar daca avea alte operatii -
   **reparat**: documentul se sterge doar gol.

### 2.6 Ribbon «MNC»

NEEXPORTAT: randul «MNC» din `USysRibbons` (etichete si asezare). Butoanele sunt reconstituite din
`Surse/RawExport/Modules/basRibbonCallbacks.bas.txt:1117-1309, 1622-1645` (VERIFICAT).

| Buton Access | Eticheta | In K-BOT |
|---|---|---|
| `tgb_MNC_Info` - panoul de informatii | **PORT** | «Informatii» (apasat / neapasat) |
| `btn_MNC_OA1` - lista implicita | **PORT** | lista normala a paginii |
| `btn_MNC_OA2` - operatii de inchidere | **DE DECIS** | propunere: nu acum (etapa 7, inchiderea); SQL-ul NEEXPORTAT |
| `btn_MNC_OA3` - operatii salarii | **DE DECIS** | propunere: nu acum (etapa 3, salariile) |
| `btn_MNC_OA4` - Centralizator | **DE DECIS** | SQL-ul NEEXPORTAT, nu stim ce arata; propunere: nu acum |
| `btn_MNC_S1` / `S2` - ordonare | PORT / ELIMINAT | vezi 2.2 |
| `btn_MNC_NC1RP`, `NC1RL`, `NC2R`, `NC3R` - nota contabila | **AMANAT** | etapa 6 |
| `btn_MNC_Debit`, `btn_MNC_Credit` - fisa contului | **AMANAT** | etapa 6 |
| `btn_MNC_ORD`, `btn_MNC_PROP`, `mnu_sbt21` (`AB`, `AI`, `AL`) | **ELIMINAT** | decizia 23.10.2026 (ALOP) |
| `btn_MNC_XLS1` - Excel, lista curenta | **PORT, reparat** | exporta **ce se vede** (filtre + ordonare aplicate); Access exporta lista de la deschidere si scria NC peste randul de titlu |
| `btn_MNC_XLS2` - Excel, tot anul | **PORT** | |
| `cbxSterg_NC` | **ELIMINAT** | cod mort |

Butonul principal (care deschide pagina) - **DE DECIS**: propunere **Configurari > «Date»**, langa «Operatii»,
text «Modificare operatii».

## 3. Mecanisme Access de lasat

- Lista construita din tabelul `[~SQL]` (randurile 40000-40003, NEEXPORTATE) + `Scheme`; filtrele si ordonarea puse pe
  formular, campurile ascunse `SQL_Main` / `SQL_Filter` / `SQL_Sort`.
- `ControlTipText` folosit ca cod de asezare («ordine_procent»), `Tag` ca nume de camp, caseta ascunsa `Rd` ca rand ales.
- Interogarile si rapoartele care citesc direct formularul (`Forms!ModificareNC1280!MNotaC1!...`).
- Mesajele ridicate cu `Err.Raise` se vad doar daca tabelul `B` e adevarat (NEEXPORTAT) - in K-BOT orice refuz se vede.
- La esec, `DELETE Actiuni WHERE IdAct = DMax(...)` stergea ultimul rand de jurnal, care putea fi al altcuiva.
- La modificare, Access doar avertiza pentru perioada inchisa (138) si nota de inchidere (318) si deschidea totusi `Note`.
  Se noteaza pentru portarea formularului `Note`: acolo **refuzat**, ca la stergere.

## 4. Neverificat

- Randul «MNC» din `USysRibbons`, randurile `[~SQL]` 40000-40003 si 3403, tabelul `B`: NEEXPORTATE.
- (Cascada `Oper -> Debit / Credit`: lamurita de operator, vezi 0.1.)

## 5. Intrebari pentru Adelina (RASPUNSE 10.11.2026, vezi 0.1)

1. «Ord»: cuvintele etapelor (2.3) - «Generat / Semnat AB / Semnat CD / Ordonator»?
2. «R» la o plata cu mai multe facturi: doar cand toate au receptie?
3. Mai multe filtre deodata (cate unul pe coloana) sau unul singur, ca in Access?
4. Operatie cu ordonantare generata: stergerea refuzata?
5. Soldul partenerului din panou: cu soldul initial sau doar operatiile, ca in Access?
6. Ordinea implicita si starea panoului: tinute minte per utilizator pana la pagina Setari «Avacont»?
7. Listele «operatii de inchidere», «salarii», «Centralizator»: nu acum?
8. Butonul principal in Configurari > «Date», langa «Operatii», cu textul «Modificare operatii»?
