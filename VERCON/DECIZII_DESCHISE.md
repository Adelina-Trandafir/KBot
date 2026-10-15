# VERCON — registrul deciziilor (SLICE-VC00-05)

Stare: ALINIAT la plan, 12.10.2026 (runda 3 + corecțiile vizuale ale utilizatorului). Autoritare sunt secțiunile din [PLAN_IMPLEMENTARE](PLAN_IMPLEMENTARE.md) datate 11.10.2026, runda 3, și 12.10.2026, corecții UI/fluxuri. Distinge: **DECIS** (utilizator, nu se redeschide), **PROPUS** (model, de verificat) si **DESCHIS** (fara raspuns).

## A. Decise de utilizator

**Anterioare:** schema comuna identica; ierarhia DC → subunitate → gestiune; prefixe `VC_`/`VCxx`, executabil `VRC.Migrator`; migrare **o singura data** pe DC+subunitate, fara reimport/sincronizare/compatibilitate VBA; MF/OI = o baza multianuala, Materiale/Alimente = `tz_aaaa.accdb` pe subunitate; valori INT; pret >= 0; N07 reparat in web; acces exclusiv pe DC+subunitate+gestiune; tip gestiune imuabil dupa populare; Coduri exclus complet; e-Factura exclusa (viitor: Kbot.Efactura); DBS/clsDatabase neportate; reutilizare `AVACONT_COMUN`; schite PNG; `Intrari.TVA` = suma TVA calculata.

**Runda 3 (11.10.2026), Q1–Q12 si indicatiile libere:**

| Tema | Decizie |
|---|---|
| Zecimale | `NR.ZEC` **pe gestiune**; la Materiale/Alimente, anul cel mai recent (Q3); fallback 4 |
| Stocare vs afisare | valori pastrate **exact**; afisare la `NR.ZEC`, **in sus la jumatate** (Q2, Q12); scara de stocare a gestiunii = cea mai mica `>= NR.ZEC` care reprezinta exact datele (Alimente 4, MF 2, OI 3) |
| Calcule la migrare | **niciunul**, cu exceptia antetelor NIR/BC: `0`, `NULL` sau diferenta **> 1 leu** fata de suma liniilor → suma liniilor (Q1: regula N01/N02 ramane); restul se copiaza exact |
| Inchidere anuala | numai Materiale si Alimente; **OI si MF nu au buton de inchidere** |
| OI/MF — perioade | nu se inchid; in selectia datelor (SFP) arborele an/luna contine **toti anii de la prima inregistrare** (NIR) si **de la prima predare** (BP), **doar ultimul an expandat**; deci „perioada deschisa" = mereu, nu exista blocare prin inchidere |
| Furnizori | = Parteneri KBOT, legare dupa `CodFiscal`; furnizorii fara inregistrari nu se migreaza; ANAF/`clsFirma*` nu se portează; fara CodFiscal sau fara potrivire (cu NIR) → **alege operatorul in migrator** (Q5) |
| «Stoc initial» | nu e furnizor real (variante: „Stoc Initial 2025", „S.I.", cu/fara diacritice) → document de tip **«Stoc initial» fara partener** (Q4); lista candidatilor o confirma operatorul in migrator |
| Export Excel | `.xlsx` simplu pe fiecare raport, fara sabloane `.tmpl` (Q6) |
| Editare/anulare | ca la ADECHIT: anulare cu motiv obligatoriu, document ramane marcat „anulat"; editare cat timp perioada e deschisa; drept pe cont (Q7). Pentru OI/MF perioada este mereu deschisa |
| Numerotare | automata, unica pe gestiune + tip + an, reset anual; numar manual doar daca e liber (Q8) |
| Valorizare stoc | pretul variantei, **modificabil oricand**, ca in Access (Q9); liniile documentelor isi pastreaza valorile |
| Stoc negativ | **interzis** (Q10) |
| Drepturi | doar roluri pe DC; fara ACL pe subunitate/gestiune (`VC_SubunitAccess`/`VC_GestiuneAccess` nu se creeaza) (Q11) |
| Surse (12.10.2026) | folderul sursa se citeste dintr-o data: **tz_aaaa** (cate un exercitiu Materiale/Alimente), **tz_2100** = baza speciala OI/MF (**2100 nu este un an**; tipul il spun steagurile OBINV/MF), atat `.accdb` cat si `.mdb` (ambele se listeaza); **`tz.mdb` / `tz.accdb` nu se iau niciodata**; operatorul bifeaza anii doriti intr-un tabel |
| D01 | atribuirea fisier → subunitate/gestiune/tip/DC o face operatorul in migrator, la import |

### Corecții UI decise — 12.10.2026

Sursa: [imaginile adnotate](MOCKUPURI/Corectii_mockup-uri_01/). Contract complet: [FLUXURI_UI](FLUXURI_UI.md).

| Temă | Decizie |
|---|---|
| Arbori documente | În dreapta, numai în consultare; ascunși la adăugare/modificare, reapar la salvare confirmată/Renunță; frunzele au ⋯ la hover; Adaugă în toate familiile |
| NIR antet | Factură/Dată factură/Valoare factură pe aceeași coloană, Număr NIR/Dată NIR pe alta; valoarea facturii activă/manuală, necompletată → suma produselor, cu N01/N02 păstrat |
| NIR linii/MF | Produs prin combobox cu căutare la tastare; coloanele MF în aceeași grilă Produse recepționate, fără panou separat |
| NIR salvat | Arbore furnizor → dată → număr factură + total |
| Aspectul tuturor grilelor — revizia 2 | **NIR este unica referință vizuală corectă**: celule compacte, fără padding mare, aceleași editoare/tipografie/borduri; Adaugă rând mic în footer și totaluri aliniate pe coloane |
| BC | DGV identic vizual cu NIR; arbore bonuri în dreapta, ascuns în editor; BC se creează separat și în Alimente |
| ListaAlimente | Rezumatele porțiilor și felurile DGV în stilul NIR; fără Alimente necesare; arbore Liste salvate în dreapta în consultare, formatat ca NIR/BC/BP |
| Legătura listă–BC — revizia 2 | **O listă poate avea mai multe BC-uri**: DGV cu un rând per bon, valoare pe rând și total general; Adaugă rând deschide popupul cu BC existente din Alimente neasociate altei liste; selecție aditivă fără duplicate, întregul set revalidat/salvat atomic; fără generare/resalvare BC sau consum repetat; IdLista țintă neunic în legături |
| OI/MF documente | Fără Perioada/Deținători în stânga și Efectul transferului/Istoric produs în dreapta; arbore de BP/BT/BS în consultare |
| Predare | Sursa este contul în care au intrat produsele |
| Transfer/casare | Numai produse de la predătorul ales; casare fără Primitor |
| SFP | Arborele multianual rămâne în rapoarte |

## B. Constatari VC00 (dovada)

| ID | Constatare | Efect |
|---|---|---|
| F1 | 114 linii Alimente (cote 21%/11%) au `TVA` stocat inconsistent cu cota; `PUTVA`/`Valoare`/`ValoareCUTVA` concorda | La migrare se copiaza ca in sursa; derivarea (`ValoareCUTVA − Valoare`) este pentru documentele noi |
| F2 | ~~Totalurile depasesc INT la 4 zecimale~~ | RETRAS: cu scarile alese incap |
| F3 | 13 valori OI au a treia zecimala (diferenta exact 0,005) cu `NR.ZEC = 2` | Pastrate exact (scara OI 3), afisate rotunjite in sus |
| F4 | 3 rapoarte de balanta (`AFB`, `BF`, `furnizor`) si 16 interogari depind tranzitiv de Coduri | Excluse (N06) |
| F5 | `Iesiri_H → LegBCLA` are atribut DAO 8192 in toate bazele | Neinterpretat; proba pe copie la VC03 |
| F6 | Bazele nu au retururi, casari, transferuri, predari OI reale | Scenarii sintetice |
| F7 | `FC` contine ramuri cu `Coduri`; `mdl_EditData`/`mdl_Functii`/`Utile` amesteca business si infrastructura | INCLUS-PARTIAL |
| F8 | `subTreeView` (ActiveX) este subformular in BC/BR/OI/FC/FCS/ListaAlimente/Produse | Inlocuit de selector + panou web |
| F9 | `FC`: panoul MF se suprapune cu cel de detalii in export | Regula MF vine din tip |
| F10 | Gestiunea in Access = folderul `cale.Cale`; exercitiile = `tz_<an>.accdb`; subunitatea nu exista in Access | Atribuire in migrator |
| F11 | Tipul in Access = `Setari.OBINV`/`MF`; Alimente vs Materiale nu are steag | Alegere a operatorului la migrare |
| F12 | `tmpOptiuniSFP` (47 randuri) si `Rbb` (49 randuri: toate `V=1,E=1`, exceptie `Tab_MF` ascuns) primite | Ribbonul nu diferentiaza dupa tip; gating-ul vine din `Form_Open` |
| F13 | Meniul refera 3 rapoarte MF absente din export; 106/107 acelasi raport; 19 rapoarte fara meniu | Se trateaza la VC08-03 |
| F14 | Defecte de sursa pe schite (perioada `di - di`, eticheta „Numar BP" la casare, antet duplicat `Plan_Achizitii`, titlu `BR`) | Web corecteaza |
| F15 | Furnizori: Alimente 28 (4 cu NIR), MF 2 (ambii cu NIR; unul = «stoc initial»: 1 NIR / 154 linii), OI 5 (1 cu NIR); `CF` gol la cei cu NIR: 1 / 1 / 0; `NIR.IDF = NIR.IDFV` peste tot; «stoc initial» pe Alimente: 2 NIR / 79 linii | Reguli Furnizori si «Stoc initial» de mai sus |

## C. Intrebari

Toate cele 12 intrebari (Q1–Q12) au primit raspuns si sunt in sectiunea A. **Nu mai exista intrebari deschise cerute utilizatorului.**

**Nesolicitate acum (nu se inventeaza):** D12 (transfer intre contexte), rolul liniilor cu `Cant=0` (OI: 40 — pastrate), eventuale baze `tz_aaaa` lipsa, semantica atributului DAO 8192.

## D. Propuneri ale modelului ramase de verificat (nu sunt reguli)

- Rotunjirea **calculelor** documentelor noi: `HALF_EVEN` (paritate cu `Round` VBA) — verificare la VC08-05.
- Rotunjirea la **afisare** in sus (decizia Q12) — de verificat pe ecranul Access.
- Detectia «Stoc initial» prin nume normalizat (fara diacritice, litere mici: contine „stoc"+„ini", „s.i.", „sold ini") cu confirmare de operator.

## E. Backlog independent de aceste decizii

VC01 (schelet web pe date fictive), VC02 pe fixture, reader/profilare VC05-01 (doar citire), motorul numeric (`ZEC`, `SCARA`, N01/N02), DDL VC03-01 (ierarhie, ocupare). La import (nu acum): atribuirea fisierelor, tipul Alimente/Materiale, DC real.

## F. Decise de utilizator la profilarea VC05-01 — 12.10.2026

Sursa: raportul de profilare pe `tz_2025` / `tz_2026` (047_GR47). Toate mesajele de mai jos sunt **avertismente care nu blochează** importul.

| Tema | Decizie |
|---|---|
| Număr document negativ la NIR (`NIR.Numar`) | Dacă `NrFact` conține «stoc», documentul este stoc inițial și numărul negativ se acceptă (numai pentru stoc). În alt caz, numărul negativ sau cu zecimale se raportează ca avertisment; nu se trunchiază tacit. |
| Total antet NIR diferit de suma liniilor | Se aplică regula totalului liniilor (cu TVA); avertisment nebloc, cu antetul și diferența. |
| Preț negativ în `Intrari` / `Iesiri` | Inversarea se aplică **numai** în `Intrari` și `Iesiri`, unde există `Cant`: la migrare prețul devine **pozitiv** (aceeași valoare absolută) și cantitatea **negativă** (aceeași valoare absolută), cu mesaj de avertisment, nebloc. **BLOCANT:** dacă `Cant` ȘI prețul sunt ambele negative pe aceeași linie, importul se oprește pentru acel caz (operatorul îl corectează în sursă). `Cant` poate fi negativă, cu avertisment. Prețul din schemă rămâne ≥ 0 (CHECK). |
| `ProduseP` (legătura Produse ↔ Intrari / Iesiri / Bonuri) | Se migrează **numai rândurile care au corelație efectivă** în `Intrari`, `Iesiri` sau bonuri (pentru OI și MF). Restul nu se transferă. Prețul negativ (−0,0941) vine dintr-o bază foarte veche în care era permis; un rând negativ fără corelație nu se transferă, deci nu mai e problemă. Dacă un rând negativ are corelație, se aplică regula `Intrari` / `Iesiri` de mai sus. |
| `IdBonConsum` pe mai multe liste | Nu există în Access. Listă 1:N BC este o funcționalitate nouă, cerută pentru web; nu se verifică la import. |
| Tip gestiune (Materiale / Alimente) | O bază poate avea atât conturi de Alimente, cât și de Materiale; **niciodată** OI sau MF în aceeași bază. Tipul îl alege operatorul; raportul nu îl deduce din `ListaAlimente`. |
| Contoare `NR.*` (NIR, NR, NRBC, NRFCT) | Nu se mai folosesc. Numerotarea pornește de la ultimul număr real salvat, peste tot: NIR, BC și celelalte documente. |
| Număr NIR cu zecimale (707,1) | Un NIR introdus la locul lui cronologic între două numere (nu își schimbă numărul). Se păstrează: partea întreagă în `Numar`, fracția în sutimi în `NumarSub` (707,1 = 707 + 10), ordonare după (Numar, NumarSub); mai mult de 2 zecimale blochează. Decis 12.10.2026. |
| Alimente / Materiale într-un fișier | Tipul NU vine din fișier, ci din tipul contului (`Masini.TIP`): `A` = Alimente, `M` sau fără tip = Materiale. Un fișier de an dă până la două gestiuni (Alimente și Materiale); documentele se plasează după contul lor (sau după liniile lor); listele de alimente sunt Alimente; TVA, comisiile și clienții se copiază în ambele. OI și MF nu se amestecă cu ele. Decis 12.10.2026. |
| Anul exercițiului la tz_2100 | Cel mai mic an din `NIR.Data`. Decis 12.10.2026. |
| Motor Access | Toți utilizatorii au Access pe 64 de biți; nu se testează 32 de biți. |



### Revizie 14.10.2026 — migrare fără recalcularea valorilor

Decizie utilizator: VRC copiază valorile din Access, chiar dacă antetul nu corespunde
liniilor. Aceasta înlocuiește regulile istorice N01/N02 de corectare la migrare și
regulile de inversare a semnelor. Totalurile 0/NULL, prețurile și cantitățile negative
rămân în sursă și în țintă identice. Diferențele sunt doar raportate. Numerotarea după
ultimul număr real rămâne explicit neschimbată; calculele documentelor noi din web
rămân în codul aplicației și nu sunt aplicate datelor migrate.
