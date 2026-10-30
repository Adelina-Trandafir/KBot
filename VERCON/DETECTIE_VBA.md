# VERCON — analiză și plan de implementare

## Scop și mod de lucru

Analiză solicitată la 11.10.2026 pentru `VERCON_ACCESS`, consemnată incremental pentru predarea către alt model. Etapa documentează observații/propuneri din surse, fără implementare în aplicație.

Etichete: **confirmat** = vizibil în fișiere; **ipoteză** = necesită verificare suplimentară; **propunere** = pas recomandat, încă neimplementat.

## Starea analizei / punct de reluare

- [x] Etapa 0: verificare director și inventar inițial.
- [x] Etapa 1: completitudinea exportului și arhitectura (limitele sunt precizate mai jos).
- [x] Etapa 2: date, fluxuri și reguli de gestiune (analiză focalizată + verificări agregate).
- [x] Etapa 3: integrări, actualizare, siguranță și riscuri tehnice (analiză statică).
- [x] Etapa 4: priorități și plan verificabil de implementare.

**Starea finală a rundei inițiale:** analiza inițială încheiată, implementare neîncepută. Reluare recomandată: P0-01, apoi P0-02/P0-03. Bifele atestă analiza, fără repararea problemelor.

### Etapa 0 — inventar inițial

**Confirmat:** directorul conține un export Microsoft Access: module VBA, clase, formulare, rapoarte, interogări SQL, descrieri de tabele și trei baze `.accdb` pentru alimente, mijloace fixe și obiecte de inventar. Există metadate de export: `INDEX.md`, `relationships.md`, `vba-references.md`, `database-properties.md`, `excluded.md`, `export.log`.

**Confirmat:** `PLAN_IMPLEMENTARE.md` nu exista la început. Nu am găsit `AGENTS.md` în repo prin inventarul inițial și nici în directoarele părinte verificate. Repo-ul are multe modificări preexistente în alte directoare; acestea nu fac parte din această analiză și nu trebuie suprascrise.

**Urmează:** citirea metadatelor, identificarea punctului de pornire, a legăturii frontend/backend și a fluxurilor principale. Detaliile financiare și fiscale observate vor fi descrise ca comportament al codului, nu ca validare legală a regulilor.

### Etapa 1 — arhitectură și completitudinea materialului

**Confirmat:** `INDEX.md`/`export.log`, export `C:\vercon\Vercon.accdb` la 11.10.2026, 14:54: 109 tabele, 80 queries, 53 formulare, 61 rapoarte, 44 module, 27 clase, 3 macrocomenzi = 377 obiecte, 0 erori/excluderi raportate. Dovedește exportul raportat reușit, fără garanția reconstrucției unei aplicații executabile complete.

**Confirmat:** 40/109 tabele sunt legate: 5 spre `cale.accdb`, 35 spre `14\tz_2024.accdb`; restul, locale frontend. `INDEX.md` nu inventariază `Databases`: inițial trei baze de gestiune, apoi și `Databases/cale.accdb` (modificat 15:34). Lipsesc frontendul original/backendul anual din căile legate; cele trei baze denumite pe domenii au date efective (etapa 2C).

**Confirmat:** aplicația este un frontend Access cu backend-uri selectate dinamic. `mdl_Autoexec.bas.txt:Aexec` inițializează componente, loguri și FTP, încearcă actualizarea șabloanelor/imaginilor/frontendului, șterge legăturile, repară/conectează `cale.accdb`, citește separatoarele regionale și deschide `VaerCon`. `mdl_LegTabele.bas.txt:CnnTbl_2022` leagă tabelele din `_TBLS_` după `DB_INDEX`. Există infrastructură proprie de descriere/reparare a schemei (`_TBLS_`, `_Struct_`, `_IDX_`, `_REL_`, `SQL_FIX_BD`, `clsDatabase`).

**Limită:** macrocomanda exportată `Autoexec_`, nu `AutoExec`, conține `Productie(true)`/`AExec()`, fără pornire automată confirmată. `modStartup:StartUp` verifică/creează `ufTreeView`, ale cărui fișiere MSForms lipsesc din inventar. Ribbonul `Vercon` este declarat în proprietățile bazei, fără export `USysRibbons`/XML. De identificat pornirea reală/resursele UI lipsă.

**Confirmat:** `vba-references.md` nu raportează referințe defecte pe mașina de export. Referințele includ DAO, ADO/ADOX, Office, Scripting, VBIDE, MSForms și `WinSCPnet` din `C:\AVACONT\RES\WinScpNet.tlb`. Codul poate extrage `RegExpWrapper32.dll`/`RegExpWrapper64.dll` din `tblFiles`. Nu rezultă că aceste dependențe sunt disponibile pe orice stație; descrierile de tabele nu includ payloadurile binare.

**Confirmat:** `relationships.md` conține numai 6 relații ale metadatelor locale; tabelele legate (ex. `Intrari`) descriu și indici FOREIGN business. Absența din acest fișier nu dovedește lipsa integrității backendului.

**Impresie inițială:** aplicație matură de gestiune, cu multe funcții utile și infrastructură proprie, dar strâns dependentă de Access, starea formularelor, fișiere externe și configurația stației. O rescriere ori integrare cere mai întâi recuperarea regulilor existente și a unui mediu de referință reproductibil.

**Punct de reluare:** citite metadatele, `mdl_Autoexec`, `modStartup`, începutul `mdl_LegTabele`; inventariate procedurile principale din `VaerCon`, `FC`, `BC`, `BR`, OI și `clsDatabase`. Urmează analiza salvării documentelor, stocului, închiderii anului și a integrărilor. Nu s-a rulat aplicația și nu s-au deschis bazele binare.

### Etapa 2A — modelul de date și fluxurile principale

| Domeniu | Surse și comportament observat |
|---|---|
| Unitate/an de lucru | `cale` + `Caletemp`, selectate în `Forms/VaerCon.txt:1697`. Calea anuală este construită din directorul aplicației, unitate și `AnDate`. |
| Produse și variante | `Produse` = nomenclator/categorie (`IdMasina`, denumire, UM, CPV, CC); `ProduseP` = identitate distinctă `IdProdus`, preț cu TVA și eventual cod de inventar. `ID` și `IdProdus` nu sunt interschimbabile. |
| Recepție/NIR | `FC`, `TmpFact`, `NIR` (antet), `Intrari` (linii), `Furnizori`, `ComisieNIR`. Salvarea din `FC:4229` creează variante noi, antet și linii, iar pentru MF scrie și `ProduseM`. |
| Consum/retur/storno | `BC`, `BR`, `FCS`; antete în `Iesiri_H`, linii în `Iesiri`/`IesiriRetur`, storno intrări în `IntrariStorno`. Bonurile și listele de alimente sunt legate prin `LegBCLA`. |
| Obiecte de inventar/MF | `OI_Predare`, `OI_Transfer`, `OI_Casare`, registrul `OI_Produse`; transferul scrie cantități pozitive pentru primitor și negative pentru vechiul deținător (`OI_Transfer:2193`). `ProduseM` păstrează data intrării, durata și valoarea lunară. |
| Alimente | `ListaAlimente`, `LABC`, `Retetar`, `FeluriMancare`, `MicDejun`/`Dejun`/`Cina`, legături cu bonul de consum; există salvare tranzacțională în `ListaAlimente:2311`. |
| Situații și închidere an | `SFP`, `tmpSFP`, `tmp_Balanta`, variante de balanță, fișă de magazie, inventar, amortizare; `IA:905` creează anul următor prin copiere și curățare a bazei. |

**Confirmat:** `Masini`/`IdMasina` înseamnă și cont/categorie de gestiune (mesaje/selecții `FC`), nu numai vehicule; schema viitoare păstrează semantica, fără traducerea mecanică a numelor.

**Confirmat:** queries folosesc `Forms!...`/`TempVars!Zec`, iar `RC2QC` completează parametrii prin `Eval`. UI/temporarele participă la calcul; mutarea backendului cere parametri expliciți unitate/an/categorie/data/document curent/precizie.

**Confirmat:** `QBC` = intrări + retururi − ieșiri − cantitățile bonului în lucru − storno; exclude bonul curent din ieșiri și oferă numai stoc pozitiv. Primele trei ramuri au filtru de dată, storno nu (`queries/QBC.sql:16`). **Risc de verificat:** storno ulterior poate modifica disponibilul unei date anterioare.

**Confirmat:** `Balanta22` calculează stoc inițial, intrări, ieșiri și stoc final din `QSFP_BL`, pe `IdProdus`, categorie și interval. `Amortizare` folosește `DateDiff("m",...)`, durată normală, durată folosită și valoarea lunară rotunjită la 2 zecimale. În `OI_Union_Balanta`, predările includ capetele intervalului, dar casările din interval folosesc `>`/`<`. **Risc concret:** casările chiar în prima/ultima zi pot fi omise din rulaj; necesită reconciliere cu raportul folosit efectiv.

**Confirmat:** există două generații de cod. `mdl_EditData:UpDateP` și `IntrariIesiriTotal`, precum și `Balanta`/`BC_Produs_FIFO`, folosesc `Coduri`, `IntrariIesiri`, `Intrari.DenumireProdus` etc., absente din schema exportată actuală. `QBC`/`Balanta22` folosesc modelul `Produse` + `ProduseP`. Setarea `FIFO` încă apare în formularul `BC`. **Nu este demonstrat** că rutinele vechi sunt apelate în exploatarea curentă; trebuie urmărite apelurile înainte de reparare sau eliminare. Nu trebuie portat automat tot codul exportat.

### Etapa 2B — constatări tehnice cu impact asupra datelor

1. **Identificatori concurenți:** `FC:4027`, `BC:2332`, `BR:2168`, `OI_Predare:2437`, `OI_Transfer:2157`, `OI_Casare:2081` aleg `DMax + 1` înaintea salvării. PK împiedică unele duplicate, dar doi utilizatori pot alege același ID și unul eșuează. Propusă alocare atomică backend, cu număr afișat distinct de cheia tehnică; nu dovedește utilizarea multiuser actuală.
2. **Tranzacții neuniforme:** FC/BC/BR/OI/ListaAlimente folosesc begin/commit/rollback, de păstrat. În `FC:4303`, commit precede codurile MF/ștergerea variantelor fără intrări (`4308–4310`): eșecul ulterior lasă documentul salvat. Multe `Execute` nu folosesc `dbFailOnError`; de verificat linia respinsă, fără presupunerea rollbackului oricărei erori.
3. **Ștergere BC:** `BC:2467` poate șterge `LABC`/`LegBCLA`/antet în afara tranzacției; liniile pot depinde de cascadele backendului, incomplet exportate. Propusă operație atomică după verificarea relațiilor reale.
4. **Validare NIR greșită:** `FC:4816–4818` repetă `DataNIR` cu mesaj despre valoarea facturii. De validat `Valoare` și selecția/liniile nevide. `VerificariSalvare` permite explicit diferența sumă calculată/valoare introdusă după confirmarea UI; regula se păstrează ori se schimbă conștient.
5. **Închidere an vulnerabilă:** `IA:905` poate suprascrie `tz_<an>.accdb`, apoi șterge/inserează și înscrie anul în `cale`, fără tranzacție/global rollback sau publicarea finală a copiei validate. Propus fișier temporar, verificare solduri, backup, înlocuire după validare. Fără probarea rutinei pe originale.
6. **Precizie/parsare pe stație:** predominant `Double`, rotunjire prin `Zec()` (implicit 4), unele calcule MF la 2; SQL concatenează `CDate('...')`/texte. Apostrofurile pot rupe SQL (`BC:2438–2441`, `FC:4522`), datele depind de format. Propuse QueryDef parametrizat și politică numerică validată pe exemple existente.

**Punct de reluare:** fluxurile au fost analizate static în procedurile principale; nu s-au verificat încă toate ramurile de storno/retur, liste de alimente și rapoarte. Urmează integrările și verificarea read-only a celor trei `.accdb`, dacă motorul DAO este disponibil. Se vor nota doar schema și numărători, nu datele personale sau parolele.

### Etapa 2C — verificare read-only a bazelor reale

Motorul `DAO.DBEngine.120` este disponibil. Cele patru baze au fost deschise cu `ReadOnly=True`, fără lansarea Access și fără executarea VBA/macrocomenzilor. Au fost citite schema, relațiile și numărători agregate; nu au fost afișate înregistrări cu date personale și nu s-au efectuat corecții.

| Bază | Date observate | Observație |
|---|---|---|
| `cale.accdb` | 7 înregistrări `cale`, 1 în `Ver` | Are doar aceste două tabele de aplicație; exportul frontendului arată și `EF`/`EFS`/`EFT` legate către cale. Disponibilitatea acelor tabele trebuie reconciliată cu schema țintă și migrările. |
| `tz_Alimente.accdb` | 192 NIR, 1.671 intrări, 127 bonuri, 3.196 ieșiri, 127 liste de alimente, 2.304 variante de produs | Date NIR de la 31.12.2025 până la 30.09.2026. Nu este un șablon gol. |
| `tz_Mijloace_Fixe.accdb` | 2 NIR, 156 intrări, 7 bonuri, 154 ieșiri/predări, 12 înregistrări `ProduseM` | Date NIR de la 31.12.2022 până la 15.03.2024. |
| `tz_Obiecte_Inventar.accdb` | 4 NIR, 1.137 intrări, 96 bonuri, 1.529 ieșiri/predări, 1.144 variante | Date NIR de la 31.05.2022 până la 10.06.2022. |

**Confirmat:** verificările pentru intrări fără NIR/variantă, ieșiri fără antet/variantă, variante fără produs, OI fără variantă/client și antete fără linii (NIR ne-storno/bon ne-retur) au dat 0 în toate cele trei baze. Nu reprezintă un audit complet al integrității sau al soldurilor.

**Confirmat:** backendurile au 29/30/29 relații. Relațiile `NIR -> Intrari`, `Iesiri_H -> Iesiri`, `Produse -> ProduseP` și multe relații către OI au atributul 4352 = 256 + 4096 (cascade update + cascade delete). `Iesiri_H -> LegBCLA` are atributul 8192, a cărui semnificație nu este inclusă în enumerarea DAO publică consultată; nu o clasific drept relație neimpusă sau LEFT JOIN. Aceasta corectează interpretarea provizorie din prima redactare. Flagul documentat pentru lipsa integrității este 2, iar pentru LEFT JOIN este 16777216. [Referință Microsoft](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/relationattributeenum-enumeration-dao). Verificați efectul complet al cascadării pe o copie înainte de intervenție.

**Confirmat:** există variații de schemă între baze:

- MF are `ProduseM.ValoareLunara` în plus față de descrierea exportată (`ValoareLuna` există și ea).
- OI nu are `Produse.DNF`, `ProduseP.COD`, `ListaAlimente.Comentarii`; nici tabelele `Alimente`, `ProduseM`, `UNIT`, `Ver` nu au apărut în inventarul read-only. Aceste diferențe pot indica o generație anterioară, nu neapărat corupție.
- Metadatele backendurilor diferă de cele ale frontendului (`_IDX_` fără `IdStruct`/`OK`; `_REL_` cu `ID`/`Ref`, fără `IDR`/`IdStruct`/`Attrib`; `_Struct_` fără `Column_DefaultValue`). Nu tratați automat schema metadatelor locale ca schema tuturor backendurilor.
- Niciun backend citit nu are `Coduri` sau `IntrariIesiri`, confirmând că rutinele legacy menționate nu se potrivesc direct acestor date.

**Confirmat după separare:** în baza OI sunt 40 intrări cu `Cant = 0` și 0 cu `Cant < 0`; în celelalte două ambele numărători sunt 0. Nu se clasifică drept eroare fără identificarea regulii de import.

**Incident rezolvat:** două controale de total au eșuat în DAO standalone din cauza `Nz`; reformulate fără funcții Access, au rezultatele valide din 2D. Eșecul verificării nu dovedea date incorecte.

**Defect static:** `queries/OI_Balanta.sql:4` numește `Sum(V2)` cantitate `CCOut` și `Sum(C2)` valoare `VCOut`, invers față de `OI_Union_Balanta`, apoi le scade din stoc/valoare. Fără casări în bazele primite, lipsește exemplul real; propusă probă sintetică 2 bucăți × 100 lei.

### Etapa 2D — rezultatele rundei suplimentare

Verificările de total au fost reformulate fără `Nz` și au rulat cu succes. Pragul de comparație este `Abs(antet - Sum(linii)) > 0,01`; NIR fără linii și bonurile de retur sunt excluse din comparațiile respective. Valorile NULL nu intră în această comparație și au fost verificate separat pentru `NIR.Valoare`.

| Control | Alimente | MF | OI |
|---|---:|---:|---:|
| NIR cu diferență antet/linii peste 0,01 | 0 | 1 | 0 |
| Bon ne-retur cu diferență antet/linii peste 0,01 | 0 | 1 | 0 |
| NIR cu valoare NULL | 0 | 1 | 0 |
| Sold OI negativ pe pereche deținător/produs | 0 | 0 | 0 |
| LegBCLA care indică un bon inexistent | 0 | 0 | 0 |

**Interpretare:** baza MF necesită reconciliere document cu document; codul permite anumite diferențe la NIR, iar unele documente sunt istorice. Nu ajustați automat antetele pentru a obține zero diferențe.

**Confirmat:** `Setari.FIFO=-1` în toate trei, `MF=-1` numai în MF, `OBINV=-1` numai în OI. `Ver=4` în Alimente, `124` în MF, tabel absent în OI: configurații/versiuni diferite. FIFO nu dovedește folosirea legacy `IntrariIesiriTotal`; formularul curent citește `QBC`.

**Defecte/riscuri suplimentare confirmate în surse:**

- `FCS:2708` salvează antetul storno și liniile fără begin/commit/rollback. O eroare între operații poate lăsa date parțiale.
- `tables/IntrariStorno.md`: `PUTVA` Long față de Double la variantă; `FCS` inserează din `TmpFact`. Risc de pierdere a fracțiunii, cu folosirea efectivă a câmpului de verificat înainte de migrare/corecție.
- `BC:2558` verifică stocul la 2 zecimale, față de `TempVars!Zec` (uzual 4) pentru linii/disponibil; probă propusă: disponibil 1,0000, consum 1,0040. Salvarea nu recalculează explicit disponibilul fiecărei linii; verificarea UI nu acoperă concurența/editările care o ocolesc.
- `BC:2570`: `.RecordCount <= 0` pentru `SELECT Count(1)` verifică rândul agregat mereu prezent, fără citirea valorii zero. De revizuit și similarele (`VaerCon`: `SELECT Max` pentru ultimul backup).

### Etapa 3A — actualizare, migrări, backup

**Confirmat:** `mdl_2024:Repara2024` compară backendul cu `VEROFFLINE`, citește schema țintă, cere `Fix_Database`/`Fix_Queries`, dar întoarce True chiar după False. Eticheta `EROARE` nu are `On Error GoTo EROARE` activ. Getterul `clsDatabase.Version` poate crea `Ver` lipsă: nu este read-only; clasele aplicației nu se folosesc pentru audit read-only.

**Confirmat:** `clsDatabase:Fix_Database` tranzacționează în două workspace-uri, poate continua după `Fix_Fields`/`Fix_Indexes`/`Fix_Relations` eșuate și nu condiționează commit de `Run_SQL_After_Update`. Propuse rezultate explicite pe pas, validare integrală înainte de commit, diagnostic separat de reparare, backup înainte de DDL. `Repara2024=True` nu dovedește reușita.

**Confirmat:** `mdl_UpdateFE:DescarcaFrontEnd` descarcă `VER.txt` și `vercon.zip`, dezarhivează, creează un BAT și rulează `taskkill /IM msaccess.exe`, apoi `xcopy /i /e /y`. Nu am găsit aici verificare de semnătură/hash autenticat sau rollback al frontendului. Comanda poate afecta toate procesele Access ale utilizatorului. Șabloanele/imaginile sunt comparate numai după mărime, apoi fișierul local este șters înainte de descărcare (`mdl_Autoexec:RefreshTemplates/RefreshImages`). Fișiere diferite, dar de aceeași mărime, nu sunt actualizate.

**Confirmat:** `mdl_siguranta:SalvezSiguranta` face maximum un ZIP local/zi în `C:\SIG\VERCON`, din backendul deschis; `frmMail` trimite ultima arhivă prin WinSCP și marchează `Trimis`. Restaurarea/consistența la scrieri/retenția sunt neverificate. Apelul din `VaerCon`, după `Repara2024`, nu protejează automat starea premigrare.

**Defect:** `mdl_ExecWait:ExecWait` întoarce True și la `CreateProcess=0`, nu citește exit code și nu închide `PI.hProcess`/`PI.hThread` pe ruta normală; `UnZip7` se bazează pe acest succes. Pointerii/handle-urile `STARTUPINFO` sunt Long; `PtrSafe` singur nu validează structura pe 64 biți. Propuse reparare și probe: executabil absent, exit nenul, arhivă invalidă, Office 32/64 biți.

### Etapa 3B — integrări și securitatea configurației

**Confirmat:** `clsWinSCP:Init_Options` folosește FTP cu TLS explicit (`Protocol_Ftp`, `FtpSecure_Explicit`) și timeout de 5 secunde. Există username/parolă literală în cod. Mai multe module includ o parolă comună de bază/arhivă. Nu reproduc valorile în plan. Propunere: inventarierea utilizărilor, mutarea secretelor într-un mecanism protejat pe stație/server și rotație coordonată cu accesul la arhivele vechi; nu schimbați doar literalul fără strategie de compatibilitate.

**Confirmat:** informațiile de firmă folosesc două endpointuri ANAF diferite (v9 și v8); mesageria folosește token Bearer din registry. Analiza se referă la URL-urile din surse; nu s-au făcut apeluri către ANAF și nu s-a verificat contractual actualitatea lor. `TrimiteMesajFactura` poate întoarce `True` chiar după status HTTP diferit de 200 ori eroare ANAF afișată. Inserarea în `EFT_M` referă un tabel absent din export și din `cale.accdb` furnizat.

**Defect static:** `Forms/UNIT` scrie/citește prefixul `VERCON_EFAUTH_` + DC, `mdl_EFactura` citește `VERCON_EFAUTH` + DC, fără underscore. `TempVars("SepData")` coincide în inițializare/consum, fără defect constatat. Lipsa tokenului nu se atribuie exclusiv ANAF.

**Confirmat:** codul descarcă și salvează executabil RustDesk și componente TreeView dintr-un server extern. Nu am executat sau descărcat aceste fișiere. Operațiile de actualizare, suport și backup sunt cuplate cu pornirea/lucrul aplicației; un mediu de verificare trebuie să le poată dezactiva explicit și să păstreze funcționarea locală.

**Completitudine e-Factura:** o căutare statică a procedurilor și bindingurilor UI găsește referințe fără definiție în export: `Popup_EFactura_IncarcaXML`, `EF_ORD`, `EF_ANG`, `EF_PROP`, `EF_OP`, `EF_GRUP_ANG`, `EF_GRUP_PROP`, `RAP_EF`. Formularul `EFACTURA` are doar declarații în code-behind și delegă în subformular/popup. Aceste proceduri pot proveni din componente lipsă sau pot fi rămășițe nefolosite; nu declarați integrarea completă fără verificare. Rutinele legacy apelează și `EdCod()` fără definiție găsită. **Clarificare după verificarea documentației:** `Produse.ID=GenUniqueID()` este o valoare specială a motorului DAO pentru generarea unui identificator aleator și nu necesită o funcție VBA în export; nu este o dependență lipsă. [Referință Microsoft](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/field2-defaultvalue-property-dao).

**Confirmat:** duplicate globale în module standard: `ChangeProperty`/`SetBypassProperty` în `Utile` și `mdl_siguranta`, `Compact_DB` în `Utile` și `mdl_compact&repair` (public explicit/implicit). Ramurile `#If/#Else` și metodele claselor nu sunt automat defecte; compilarea proiectului real va clarifica ambiguitățile/dependențele.

**Punct de reluare:** analiza statică și read-only a datelor este făcută; nu s-a compilat și nu s-a pornit frontendul. Urmează ordonarea constatărilor în pași mici, criterii de acceptare și lista elementelor încă neverificate.

### Etapa 3C — verificări finale și exemple de reconciliere

**Confirmat:** toate trei au `IntrariStorno.PUTVA.Type=4` (DAO Long). În Alimente, cele 8 linii storno diferă de `ProduseP.PUTVA` curent la prag 0,0001; MF/OI: 0. Confirmă diferența câmpurilor, fără cauza istorică. Calculele consultate pot citi `ProduseP`; nu dovedește că toate totalurile storno sunt greșite.

**Exemple MF:** NIR `IdIntrare=1`: antet 0, linii 1.262.155,34; bon `IdBonConsum=7`: antet 843.762,24, linii 843.275,53, diferență 486,71; încă un NIR cu valoare NULL. ID-uri locale bazei; înainte de corecție se verifică istoricul/importul și originalele.

**Confirmat:** `SFP` alege dinamic raportul/`QSFP_<I>` prin `MODEL` și metadatele TreeView; `IA` folosește `Balanta22_IA`. Nu este demonstrată accesibilitatea tuturor balanțelor exportate din meniul curent; fiecare defect SQL trebuie legat de raportul/opțiunea reală.

**Defect static în fișa pe furnizor:** `mdl_AdaugStocFM:AdaugaStoc_Furnizor` ordonează după `IDF, CodP, Data, Id`, dar resetează acumularea doar când se schimbă simultan furnizorul și produsul. Când se schimbă produsul în interiorul aceluiași furnizor, soldul anterior continuă în noul produs; când se schimbă furnizorul și produsul rămâne același, soldul continuă și el. Se apelează din `SFP:1730` pentru fișa cu `Furn=True`. Propunere: resetarea la schimbarea oricărei componente a cheii `(IDF,CODP)`; test cu două produse pentru un furnizor și același produs pentru doi furnizori.

**Referințe tehnice verificate:** [DAO Execute — dbFailOnError, RecordsAffected și tranzacții](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/database-execute-method-dao), [structura Windows STARTUPINFOA](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfoa). Acestea susțin verificările propuse pentru raportarea succesului și declarațiile native; nu înlocuiesc compilarea și proba pe stația țintă.

## Etapa 4 — impresie și ordine de implementare

VERCON are un nucleu util și coerent: nomenclator separat de variantele de preț/inventar, antete/linii, registre de obiecte de inventar, numeroase rapoarte și salvări tranzacționale în mai multe fluxuri. Bazele reale trec controalele de legături orfane efectuate. Problemele cele mai importante sunt la granițele fluxurilor: migrări/actualizări/backup, diferențe între generațiile de schemă, validări dependente de evenimente UI și unele formule de raportare.

**Propunere:** începeți cu stabilizarea și un mediu de referință. Acest document nu presupune că obiectivul final este rescriere web, integrare KBOT sau păstrare integrală în Access; utilizatorul a cerut acum analiza. Alegerea arhitecturii finale trebuie făcută după clarificarea modului de lucru, a numărului de utilizatori și a fluxurilor care trebuie păstrate.

### P0 — înainte de intervenții asupra datelor

| Pachet | Lucrare concretă | Criteriu de acceptare |
|---|---|---|
| P0-01 — mediu de referință | Recuperați frontendul real, userformul `ufTreeView`, ribbonul, datele locale necesare, RES/IMG/TMPLS și versiunile dependențelor. Copii separate ale celor 4 baze + hashuri; mod de test care dezactivează update/FTP/ANAF/suport automat. Compilați și identificați pornirea reală. | Pe o copie, se deschid cele trei gestiuni; legăturile indică doar copiile; nu pornește nicio actualizare sau transmisie automată; lista dependențelor lipsă este rezolvată ori fiecare funcție incompletă este marcată explicit. |
| P0-02 — rezultate financiare | Corectați inversarea C2/V2 din `OI_Balanta` și resetarea soldului pe furnizor/produs în `AdaugaStoc_Furnizor`; uniformizați intervalele casărilor și verificați filtrele de dată pentru storno din QBC/QFCS_SUB. Reconciliați exemplele MF și prețurile storno fără modificare automată a istoricului. | Cazul 2 bucăți × 100 lei scade cantitatea cu 2 și valoarea cu 200; prima/ultima zi se includ conform regulii alese; fiecare pereche furnizor/produs are sold separat; un storno viitor nu schimbă stocul istoric; diferențele MF au explicație documentată. |
| P0-03 — operații atomice | Acoperiți integral cu tranzacții salvarea/ștergerea: FCS, partea MF de după commit în FC, ștergerea BC și legăturile listelor. Adăugați `dbFailOnError` și verificări `RecordsAffected` unde sunt relevante. | O eroare injectată după antet, după ștergerea liniilor ori la ultima linie lasă documentul și legăturile în starea inițială; mesajul de succes apare numai după finalizarea întregii operații. |
| P0-04 — migrări și închidere an | `Repara2024` propagă rezultatul real; separați inspecția de DDL; backup verificat înainte de reparare. Închidere an într-o bază temporară, validare cantități/valori, apoi publicare. | A doua rulare nu schimbă datele deja migrate; o migrare eșuată nu avansează versiunea; o întrerupere la închiderea anului nu distruge anul sursă sau un an destinație existent. |
| P0-05 — procese, arhive și actualizare | Reparați `ExecWait`, structurile native și quotingul căilor 7-Zip. Verificare arhivă/checksum autenticat înainte de înlocuire, staging și revenire la versiunea anterioară; închidere doar a instanței proprii Access. | Executabil absent/exit code nenul/arhivă invalidă returnează eșec; pachet incomplet nu se instalează; celelalte sesiuni Access nu sunt închise; se poate restaura o copie de siguranță. |

Ordine recomandată: P0-01, apoi P0-02 și P0-03; P0-04 și P0-05 înainte de activarea automatizărilor de reparare/actualizare în mediul validat. Repararea surselor `.txt` trebuie însoțită de import și verificare în frontendul real; editarea exportului singur nu schimbă aplicația instalată.

### P1 — consolidarea nucleului

1. **ID-uri și concurență:** alocare atomică pentru documente; număr de afișare separat; verificarea disponibilului din nou în tranzacția de salvare. Verificați două sesiuni care încearcă același număr și același stoc. Prima operație poate reuși, a doua trebuie să primească o explicație fără document parțial.
2. **Parametri și validare comună:** QueryDef cu tipuri explicite pentru texte/date/numere; rutină comună pentru document fără linii, valori lipsă, cantități și intervale. Exemplu obligatoriu: explicație `O'Brien`, date 01/02/2026 în configurații regionale diferite, stoc 1,0000 vs cerere 1,0040. Regula privind diferența acceptată la NIR trebuie păstrată explicit.
3. **Politica numerică:** separați precizia cantității, a prețului unitar și a valorii documentului; decideți punctul de rotunjire și compatibilitatea cu istoricul. Nu convertiți toate câmpurile la Currency/Decimal mecanic. `IntrariStorno.PUTVA` necesită migrare tip/preț documentată și verificare înainte/după pe solduri.
4. **Configurație și secrete:** centralizați căile și prefixul registry, păstrați separarea pe unitate/an și planificați rotația credentialelor. Verificați că logurile și comenzile de arhivare nu expun parole (`ExecWait` scrie acum comanda completă la eroare).
5. **e-Factura:** identificați procedurile/tabelele lipsă, separați HTTP de UI și DB, tratați distinct statusul HTTP, eroarea de business, tokenul expirat și lipsa datelor. Nu raportați `True` după un răspuns eșuat; testare cu răspunsuri salvate/sintetice înainte de apeluri reale.
6. **Cod legacy și compatibilitate:** hartă a apelurilor către codul cu `Coduri`/`IntrariIesiri`; eliminați doar după confirmarea lipsei de utilizare. Rezolvați duplicatele globale, închideți recordseturile/handle-urile și verificați componentele native pe Office 32/64 biți.

### P2 — evoluție funcțională sau integrare

După stabilizarea P0/P1, extrageți servicii independente de formulare: calcul stoc la dată, validare document, alocare număr, registru OI, amortizare și închidere an. Interfața lor trebuie să primească parametri expliciți, fără `Forms!...`, `Eval` ori `TempVars` implicite. Păstrați rapoartele actuale drept reper de comparație și un set mic de documente reprezentative pe fiecare domeniu.

Dacă se decide integrarea cu KBOT sau un backend central, proiectați separat identitatea unitate/gestiune/an, maparea ID-urilor locale, drepturile de acces, auditul operațiilor și prevenirea importului dublu. Nu am analizat API-ul KBOT în această rundă și nu afirm că există deja o integrare VERCON în el.

## Ce nu este încă verificat

- Nu s-a pornit, compilat, reimportat sau modificat frontendul Access; nu s-au modificat bazele reale.
- Nu s-au urmărit exhaustiv toate cele 377 de obiecte; analiza este focalizată pe arhitectură, proceduri de scriere, calcule și integrări. Clasele UI/GDI/crypto/regex și aspectul tuturor rapoartelor nu au fost auditate integral.
- Nu s-a confirmat care dintre variantele de balanță/raport/funcții legacy sunt accesibile efectiv și nu s-a validat legislația fiscală/contabilă.
- Nu s-au verificat restaurarea backupului, comportamentul în rețea/multiuser, răspunsurile ANAF, serverul FTPS ori funcționarea RustDesk/TreeView externe.
- Valorile NULL ale totalurilor liniilor/bonurilor nu au primit încă un control exhaustiv separat. Verificările cu prag 0,01 nu detectează diferențe mai mici și nu certifică toate sumele.
- Bazele sunt din ani/configurații diferite; nu sunt un set complet de teste pentru retur, casare și transfer. Toate cele trei au 0 rânduri `IesiriRetur`, `OI_Casare`, `OI_Transfer` la verificare.

## Instrucțiuni de reluare pentru următorul model

1. Citește acest document, apoi verifică din nou inventarul: `cale.accdb` a apărut în timpul analizei, deci pachetul poate fi completat în continuare.
2. Nu confunda analiza încheiată cu implementare autorizată/efectuată. Pornește de la P0-01 dacă utilizatorul cere să implementăm; păstrează observațiile validate și actualizează acest fișier după fiecare pas.
3. Repo-ul are multe modificări preexistente în afara VERCON. Nu le reseta. `VERCON_ACCESS` este ignorat de Git prin `.gitignore`; `PLAN_IMPLEMENTARE.md` nu este ignorat. Nu adăuga forțat bazele/sursele cu credentiale la Git.
4. Pentru citire de date folosește direct `DAO.DBEngine.120.OpenDatabase(path, False, True, connect)` și recordset snapshot; închide recordset/bază în `finally`. Nu utiliza `clsDatabase.Version`, `Repara2024`, `AExec`, `Functii`, `IA.Transfer_Click` pentru inspecție: ele pot modifica date sau declanșa operații externe.
5. Parola de conectare existentă poate fi preluată în memorie din sursa locală relevantă, fără afișare și fără copiere în plan/loguri. Folosește numai controale read-only pe originale; orice scenariu de defect se reproduce pe copie.
6. Pentru verificări DAO standalone evită funcțiile Access indisponibile în acel context (`Nz` a făcut verificările inițiale să eșueze); exclude/tratează NULL explicit. Rezultatele finale ale numărătorilor sunt în etapa 2D, care înlocuiește verificările eșuate din 2C.
7. Prioritatea concretă după recuperarea mediului: `OI_Balanta` (C2/V2), `OI_Union_Balanta` (capete interval), `FCS.btnSavNIR_Click` (atomicitate), `FC.btnSavNIR_Click` (post-commit), `Repara2024` și `ExecWait` (rezultat real). Nu începe o rescriere completă înainte de a fixa regulile și criteriile de comparație.

### SQL read-only reutilizabil pentru reconciliere

Executați fiecare SELECT separat; liniile `--` sunt explicații pentru cititor și trebuie eliminate dacă motorul Access/DAO folosit nu acceptă comentarii SQL.

```sql
-- Intrări fără NIR corespunzător (așteptat 0 în cele trei baze)
SELECT Count(*) AS N
FROM Intrari AS I LEFT JOIN NIR AS H ON I.IdIntrare = H.IdIntrare
WHERE H.IdIntrare Is Null;

-- Diferențe de total NIR; NULL trebuie verificat separat
SELECT H.IdIntrare, H.Valoare, T.Total, H.Valoare - T.Total AS Diferenta
FROM NIR AS H INNER JOIN
  (SELECT IdIntrare, Sum(ValoareCUTVA) AS Total
   FROM Intrari GROUP BY IdIntrare) AS T
ON H.IdIntrare = T.IdIntrare
WHERE Abs(H.Valoare - T.Total) > 0.01;

-- Diferențe de total pentru bonuri ne-retur
SELECT H.IdBonConsum, H.ValoareBC, T.Total, H.ValoareBC - T.Total AS Diferenta
FROM Iesiri_H AS H INNER JOIN
  (SELECT IdBonConsum, Sum(ValoareCUTVA) AS Total
   FROM Iesiri GROUP BY IdBonConsum) AS T
ON H.IdBonConsum = T.IdBonConsum
WHERE H.Retur = False AND Abs(H.ValoareBC - T.Total) > 0.01;

-- Preț storno comparat cu varianta curentă; diferența nu explică singură cauza
SELECT Count(*) AS N
FROM IntrariStorno AS S INNER JOIN ProduseP AS P ON S.IdProdus = P.IdProdus
WHERE Abs(S.PUTVA - P.PUTVA) > 0.0001;
```

**Livrabilul acestei runde:** numai acest plan, scris incremental. Nu au fost făcute commituri sau modificări de cod/date în VERCON.
