# VERCON — contractul de calcul, stocare DECIMAL si date etalon

Stare: **DOCUMENTAT** (11.10.2026). Nimic implementat, nimic executat pe server. Dovezile numerice vin din interogari **agregate doar-citire** pe cele trei baze reale (`tools/oracle_probe*.ps1`, rezultate brute in `tools/out/oracle_probe*.txt`). Sistemul Coduri/FIFO legacy este exclus (N06) si nu apare mai jos.

Eticheta: **dovada** = masurat pe bazele reale; **sursa** = citit in cod/interogare; **contract** = regula pe care o implementeaza motorul web; **ipoteza** = de verificat.

## 1. Stocare DECIMAL — decizie 14.10.2026

Această revizie înlocuiește stocarea INT scalată. Codul și DDL-ul sunt pregătite;
nu s-au executat modificări pe server, compilări sau teste. Utilizatorul a confirmat
că vechea migrare VERCON există numai în demo și poate fi golită, apoi reluată.

### 1.1 Reguli

1. Cantitățile, prețurile, valorile de linie/document, sumele TVA și soldul SI_LA
   sunt valori directe `DECIMAL(20,4)`, uniform în EF și VC. Cheile, numerele de
   document, contoarele și duratele rămân întregi.
2. `Scara=4` desemnează precizia fixă de stocare. Nu se mai înmulțesc sau împart
   valori cu `10^Scara`. Exemplu: 12,3456 se scrie ca 12,3456.
3. `Zec` / `NR.ZEC` păstrează rolul de precizie a afișării și calculelor documentelor
   noi; nu reduce precizia stocării și nu modifică valorile documentelor existente.
4. Migrarea copiază valorile direct, normalizând numai zgomotul binar al Access
   Double. VRC refuză valori reale cu peste 4 zecimale sau în afara capacității
   DECIMAL(20,4). KBot raportează eventuala rotunjire la 4 zecimale. VRC nu
   recalculează totaluri, TVA, prețuri sau cantități și nu corectează semne; antetele
   0/NULL și diferențele față de linii se păstrează. Numerotarea actuală rămâne.
5. Calculele noi rotunjesc la `Zec` conform regulii existente HALF_EVEN; afișarea
   folosește HALF_UP. Agregările nu rotunjesc fiecare termen pentru afișare.
6. Cota `VC_TVA.TVA` este fracție decimală directă: 0,21 pentru 21%, fără x10000.
   EF.CotaTVA rămâne procentul exprimat în unitatea sa existentă (21 pentru 21%).
7. Aritmetica din aplicație folosește Decimal, niciodată float pentru sume.
   API transportă valorile ca text zecimal exact; SQL și rapoartele citesc valori directe.
8. Limita unui câmp este `abs(v) < 10^16`, cu maximum 4 zecimale. Nu se mai aplică
   limita INT de 214.748,3647 la patru zecimale.
9. Nulabilitatea și valorile implicite se păstrează. Semnele originale sunt acceptate
   la migrare; validarea documentelor noi din web este separată.
10. Denumirea internă a clasei `VALOARE-INT` rămâne pentru compatibilitatea
    exporturilor de schemă; tipul efectiv și conversia sunt acum DECIMAL.
11. Cele două migratoare cer schema DECIMAL(20,4) înainte de scriere. Nu creează
    și nu modifică automat tabele sau coloane.

### 1.2 Aplicare în demo

Ordinea: `sql/VC_DEMO_golire_pentru_decimal.sql` (numai datele VC din 000_DEMO),
`sql/VC_precizie_valori_4_zecimale.sql` și `sql/00EF_24_primite_linii_precizie.sql`
(pe AVACONT_SURSA), sincronizarea structurii către DC-uri, versiuni noi ale
migratoarelor și aplicației web, reimport VERCON. Nu este necesară rescalarea
vechilor date demo deoarece sunt golite. Interogările și tabelele se păstrează.
ALTER-ul VC nu se aplică peste coeficienți existenți într-o bază cu date păstrate.

## 2. Linia si totalul documentului (N01/N02/TVA)

### 2.1 Relatiile masurate (dovada)

| Relatie | Alimente | MF | OI |
|---|---:|---:|---:|
| `Valoare = Cant x Pret` (±0,005) | 1671/1671 | 156/156 | 1136/1137 (1 abatere) |
| `PUTVA = Pret x (1 + cota)` (±0,00005) | 1671/1671 | 156/156 | 1137/1137 |
| `ValoareCUTVA = Cant x PUTVA` (±0,005) | 1671/1671 | 156/156 | 1136/1137 (1 abatere) |
| `ValoareCUTVA = Valoare x (1 + cota)` | 1671/1671 | 156/156 | 1137/1137 |
| `TVA(coloana) = Valoare x cota` | 1557/1671 | 156/156 | 1137/1137 |
| `Iesiri.ValoareCUTVA = Cant x PUTVA` | 3196/3196 | 154/154 | 1526/1529 (3 abateri) |
| Cote folosite (`IDTVA`) | 3 (0%)=272, 5 (21%)=149, 6 (11%)=1250 | 3 (0%)=156 | 3 (0%)=1137 |

**Concluzii (dovada):**
- **`Intrari.TVA` este suma TVA a liniei (valoare monetara), nu procent.** `TVA.TVA` (0,19; 0,09; 0; 0,24; 0,21; 0,11) este cota.
- In **114 linii Alimente** (IDTVA 5 si 6) coloana `TVA` stocata **nu** concorda cu cota (diferente intre 0,08 si 28,24), in timp ce `PUTVA`, `Valoare` si `ValoareCUTVA` concorda toate cu cota. Deci **coloana `TVA` nu este autoritara**. Contract: `TVA_linie = ValoareCUTVA - Valoare`, derivata; valoarea stocata a sursei merge in proveniență cand difera (conversie determinista, contorizata, fara blocaj).
- In MF si OI toate liniile sunt cu cota 0: totalul cu TVA pe linie = totalul fara TVA; recalcularea N01/N02 nu schimba rezultatul in aceste exemple (**dovada**).
- Antet vs linii (`hdr_*`): Alimente 0 diferente; MF: 1 NIR (antet 0) si 1 BC (diferenta 486,71); OI 0. Niciun caz in intervalul `(0,01 ; 1]`.

### 2.2 Algoritmul de linie (contract)

Pentru fiecare linie **a unui document nou** (valori Decimal directe, rotunjire la `ZEC`; la migrare nu se aplica acest algoritm):

```
PUTVA_i         = rnd( Pret_i * (1 + cota_i) )
Valoare_i       = rnd( Cant_i * Pret_i )
ValoareCUTVA_i  = rnd( Cant_i * PUTVA_i )
TVA_i           = ValoareCUTVA_i - Valoare_i          (derivata)
S               = suma ValoareCUTVA_i                   (TVA pe linie, fara cota globala)
```

Sursa rotunjirii pe linie: `Utile.bas` (`fTVA` si rutina de ajustare linie) (`fTVA = Round(Pret + Pret*TVA, Zec)`, `Valoare = Round(cant*pret, Zec)`, `ValoareCuTva = Round(cant*putva, Zec)`). Pretul din care se calculeaza poate fi deja TVA-inclus: un pret/total deja TVA-inclus nu primeste TVA a doua oara (se specifica prin cota 0 sau prin sursa `PUTVA`). Lipsa cotei nu se inlocuieste cu o cota globala.

### 2.3 Regula totalului (N01/N02)

> **Revizie 14.10.2026:** VRC copiază antetul exact din Access, inclusiv 0/NULL și diferențe față de suma liniilor. N01/N02 nu mai schimbă datele migrate. Regula de mai jos rămâne numai pentru documentele noi salvate din web.

Regula (N01/N02):

```
H = 0                    -> total = S
abs(H - S) > 1 leu       -> total = S
abs(H - S) <= 1 leu, H<>0 -> total = H   (exact 1 leu pastreaza H)
H = NULL                 -> marcat lipsa; total = S cu conversie explicita in proveniență (nu abs(NULL))
```

Comparatia se face in unitati monetare reale (nu pe coeficienti cu scari diferite) si **fara rotunjire automata la 2 zecimale**. Regula este folosită numai pentru salvările din web; migratorul copiază valorile sursei.

| H | S | Rezultat |
|---|---|---|
| 0 | 100,00 | S = 100,00 |
| 100,99 | 100,00 | H (diferenta 0,99) |
| 101,00 | 100,00 | H (diferenta exact 1,00) |
| 101,01 | 100,00 | S (diferenta 1,01) |
| NULL | 100,00 | S, marcat lipsa |

### 2.3.1 Câmpul manual din NIR — corecție UI 12.10.2026

Valoare factură este activă și completabilă manual, în aceeași coloană cu numărul și data facturii. Nu se prezintă ca total calculat/read-only în editor. Dacă nu este completată se folosește suma produselor. Valoarea declarată și suma calculată a liniilor sunt distincte; la salvare se aplică N01/N02 de mai sus, fără schimbarea pragului de 1 leu. Vezi [FLUXURI_UI](FLUXURI_UI.md).

### 2.3.2 Lista de alimente — mai multe BC-uri, revizia 2 din 12.10.2026

O listă poate avea mai multe BC-uri existente. DGV-ul arată valoarea totală a fiecărui bon, o singură dată; `TotalBC = suma valorilor BC-urilor distincte asociate`, `ValoareLista = VMD + VD + VC`, `Diferență = ValoareLista - TotalBC`. Se folosește aceeași reprezentare numerică exactă și precizie a gestiunii ca pentru documente; fără rotunjire suplimentară pentru agregare. Exemplu: 780,0000 + 660,0000 = 1.440,0000, listă 1.440,0000, diferență 0,0000. Diferența rămâne vizibilă și poate fi nenulă; nu impune egalitate nouă.

Salvarea listei scrie atomic antetul, mesele și întregul set de legături, revalidând eligibilitatea/contextul/revizia fiecărui BC și lipsa duplicatelor. Nu creează/resalvează bonuri și nu repetă efectele de stoc. BC-urile se creează separat. Adăugarea/scoaterea unei asocieri schimbă suma din draft, fără să modifice bonul; Renunță restabilește setul salvat. Un conflict pe orice bon împiedică salvarea parțială a setului.

### 2.4 Exemplele reale MF

- **N01**: NIR local `IdIntrare=1`, antet 0; linii cu cota 0; `S = 1.262.155,34` (**dovada**, `nir_max_sum`). Rezultat: total tinta `S`; antet `0` in proveniență. Al doilea NIR MF are antet NULL (`nir_hnull=1`): marcat lipsa.
- **N02**: bon local `IdBonConsum=7`, antet 843.762,24; `S = 843.275,53`; diferenta 486,71 > 1: total tinta `S`.

Identificatorii sunt locali fisierului MF.

## 3. Serviciile de calcul incluse (sursa -> contract)

Fara `Forms!`/`TempVars`/`Eval`: toti parametrii sunt explicit (`DC, SubunitId, GestiuneId, ExercitiuId, data, document_exclus, categorie, Z`).

### 3.1 Disponibil la data (QBC, QFCS_SUB)

**Sursa (QBC)**: `Cant = Round((CIN + CR) - COUT - CT - CS, Zec)`, filtrat `> 0`. `CIN` = `Intrari` cu `NIR.DataFc <= data`; `COUT` = `Iesiri` cu `Iesiri_H.DataBC <= data` si bonul curent exclus; `CR` = `IesiriRetur` cu `DataBC <= data`; `CT` = liniile draft `tmpFactV`; `CS` = `IntrariStorno` **fara filtru de data** (defect N08).

**Contract**: `disponibil(data) = intrari(DataFc<=data) + retururi(DataBC<=data) - iesiri(DataBC<=data, document!=curent) - draft - storno(data NIR storno <= data)`. Fiecare ramura filtrata pe context (subunitate+gestiune) si pe data. Un storno posterior nu modifica disponibilul unei date anterioare. Validarea la salvare foloseste `ZEC`-ul gestiunii (nu 2): la `ZEC=4`, disponibil 1,0000 si cerere 1,0040 se **refuza**. Stocul negativ este **interzis** (Q10). Disponibilul afisat este informativ pana la verificarea sub acces exclusiv, in tranzactie.

### 3.2 Balanta / fisa (Balanta22, QSFP_BL)

**Sursa**: stoc initial `C0 = intrari + retur - (iesiri + storno)` inainte de `di`; rulaj intrari `C1 = Intrari - Storno` si iesiri `C2 = Iesiri - Retur` in `[di, dsf]`; valorile = `Round(PUTVA_varianta_curenta x cantitate, Zec)`; `CF = C0 + C1 - C2`.

**Contract**: SI + I + R - E - S = SF pe fiecare variante, cantitate si valoare separat; totalul general nu maschează diferentele locale. **Decis (Q9)**: valoarea stocului foloseste pretul variantei (`ProduseP.PUTVA`), **modificabil oricand**, ca in Access: o schimbare de pret modifica valoarea stocului si in trecut; liniile documentelor isi pastreaza valorile proprii (nu se rescriu).

### 3.3 Fisa pe furnizor (N09)

Sursa `AdaugaStoc_Furnizor` ordoneaza dupa `IDF, CodP, Data, Id` dar reseteaza soldul numai cand se schimba **simultan** furnizorul si produsul. Contract: reset la schimbarea oricarei componente a cheii `(IDF, CodP)`. Etalon: (a) doi furnizori, acelasi produs; (b) un furnizor, doua produse; (c) alternare.

### 3.4 OI / casare (N07)

**Sursa (`OI_Balanta`)**: aliasuri inversate: `Sum(V2) AS CCOut` (cantitate) si `Sum(C2) AS VCOut` (valoare), iar `OI_Union_Balanta` defineste `V2` din valoare si `C2` din cantitate. Capete interval: predarile folosesc `>=`/`<=` pe `[di, dsf]`; casarile folosesc `>` si `<` (strict) pentru rulaj si `<di` pentru initial.

**Contract**: `CCOut = suma cantitati casate`, `VCOut = suma valori casate`; intervalul este **inchis la ambele capete** (`di <= data <= dsf`) pentru toate documentele; stocul initial este `data < di`. Etalon sintetic: casare 2 buc x 100 lei: cantitatea scade cu 2, valoarea cu 200; pret +100 (nu −100); casarea chiar in prima/ultima zi apare in rulaj.

OI_Predare: efect de magazie (iesire) si crestere a detinerii; **nu** se aduna registrul OI si iesirea ca doua consumuri. Transfer: `-Cant` la `IDC_OLD`, `+Cant` la `IDC_BT`, net zero.

### 3.5 Amortizare (MF)

**Sursa (`Amortizare`)**: `ValoareLunara = Round(ValoareLuna, 2)` (2 zecimale fixe), `DurataFolosinta = min(DNF, DF + Abs(DateDiff("m", dsf, databp)))`, `DurataRamasa = max(0, DNF - DF + DateDiff("m", dsf, databp))`, `ValoareRamasa = IIf(DurataRamasa=1, ValoareIntrare - DurataFolosinta x ValoareLunara, DurataRamasa x ValoareLunara)`. Joinul `ProduseP-ProduseM-Intrari-NIR-OI_Predare` **multiplica randurile** cand un activ are mai multe intrari/predari.

**Contract**: calcul pe activ (nu pe join); `ValoareLunara` la `ZEC`-ul gestiunii (nu 2 fix) cu restul la ultima luna; `DurataNormala=0/NULL` nu produce impartire invalida; luni calendaristice exacte (inclusiv februarie bisect); `DateDiff("m")` din Access numara schimbarile de luna calendaristica (nu luni complete) — comparator cerut in VC08-02. `ValoareLuna` si `ValoareLunara` (aceasta apare doar in baza MF) se mapeaza explicit.

### 3.5.1 Implementarea locală VC08-02

**Implementare locală VC08-02, 12.10.2026:** [motor amortizare și reguli concretizate](../docs/worklog/SLICE-VC08-02-motor-amortizare.md). Motorul pornește de la prima BP neanulată, fără `Abs` înainte de predare, adaugă DF, păstrează începutul la transfer și oprește la casarea integrală. Baza este `ProduseP.PUTVA`, ca în sursa `Amortizare.sql`; corecția web închide exact restul în ultima lună și folosește valoarea lunară la ZEC. `ValoareLuna` este primară, `ValoareLunara` fallback la NULL; ambele surse rămân păstrate. DNF/DF/date invalide și cantitățile ambigue au diagnostice, nu totaluri fictive. Este proiecție cu parametrii curenți, nu replay istoric. Comparatorul sintetic pe activ este **pregătit, neexecutat**; paritatea pe date reale nu este declarată.

### 3.6 Salvare FC/BC/BR/FCS/OI si inchidere an

Protocolul de scriere este cel din PLAN (sesiune + acces exclusiv + tranzactie + RequestKey + calcul + audit + COMMIT). Defectele de atomicitate (N10) se rezolva prin tranzactie unica: variante, MF (`ProduseM`), registrul OI, `LegBCLA`. `IntrariStorno.PUTVA` si `IesiriRetur.PUTVA` sunt `Long` in sursa: se citesc ca intregi, se stochează direct în DECIMAL(20,4) (fara fractiune inventata; copiate ca in sursa, fara calcul la migrare); diferenta fata de `ProduseP.PUTVA` este in proveniență (Alimente: 8 linii storno).

## 4. Date etalon (sintetice + reale)

| ID | Caz | Intrare | Rezultat asteptat |
|---|---|---|---|
| E01 | TVA mixt | linii 0%, 11%, 21%, fara cota, `Z=4`, `H=0` | `S` = suma `ValoareCUTVA_i`; fara cota globala; fara dublarea TVA |
| E02 | prag | tabelul din §2.3 | conform tabelului |
| E03 | NIR MF 1 / BC 7 | date reale MF | `S` (vezi §2.4) |
| E04 | casare | 2 x 100 lei in prima si in ultima zi de interval | −2 / −200 si inclus in rulaj |
| E05 | pret negativ | -100 la NIR/BC/storno/retur/OI/MF/Alimente/import | refuz fara `ABS` |
| E06 | disponibil `Z=4` | disp. 1,0000, cerere 1,0040 | refuz |
| E07 | storno ulterior | storno cu data posterioara datei de calcul | disponibilul datei anterioare nemodificat |
| E08 | fisa furnizor | cazurile (a),(b),(c) din §3.3 | sold separat pe `(IDF, CodP)` |
| E09 | overflow DECIMAL | valoare 10^16 | refuz înainte de scriere; 214.748,3648 este acum admisă |
| E13 | valoare sursa cu mai multe zecimale decat `NR.ZEC` | OI: pret 12,345 cu `ZEC=2`, `SCARA=3` | stocat exact `12.3450`; afisat 12,35 (in sus); 13 randuri reale |
| E14 | `NR.ZEC` NULL | baza fara valoare in `NR.ZEC` | `Z=4` (fallback cod) |
| E15 | `NR.ZEC` diferit intre fisierele anuale ale aceleiasi gestiuni | Alimente `tz_2024` cu 4, `tz_2025` cu 2 | `ZEC` din anul cel mai recent (2); `SCARA` = max necesar din toate anii (>= 4 daca datele cer) |
| E16 | antet NIR/BC | `H=0`, `NULL`, diferenta 486,71 (BC 7 MF), diferenta 0,99 | `S`, `S`, `S`, `H` neschimbat; nimic altceva recalculat |
| E17 | stoc initial | NIR pe «furnizor» `STOC INITIAL`/`S.I.`/«Stoc Initial 2025» (cu/fara diacritice) | document de tip «Stoc initial» fara partener; operatorul confirma lista in migrator |
| E10 | scara | 100,25 cu `Z=4`; apoi afisare 2/3/4 zecimale | stocat 100.2500 neschimbat |
| E11 | TVA stocat inconsistent | linie Alimente IDTVA 5 cu `TVA` diferit de `Valoare x cota` | `TVA_i` derivat; originalul in proveniență |
| E12 | cantitate zero | 40 intrari OI cu `Cant=0` | pastrate; rol explicat |

## 5. Reproducerea probelor

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File VERCON\tools\oracle_probe.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File VERCON\tools\oracle_probe2.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File VERCON\tools\oracle_probe3.ps1
```

Toate deschid bazele `ReadOnly`, nu citesc continut (doar agregate) si nu pornesc Access. Nu s-a pornit nicio instanta MSACCESS.

## 6. Limite

- Etalonul `E01`, `E04`–`E10`, `E17` este sintetic: bazele reale nu au retururi, casari, transferuri.
- Rotunjirea `HALF_EVEN` a **calculelor** documentelor noi este documentata pentru `Round` VBA dar nu a fost comparata pe date (nu exista inca motor); comparatia este VC08-05. Rotunjirea la **afisare** este in sus la jumatate (decis, Q12; de verificat pe ecranul Access).
- Alinierea la planul din 11.10.2026 (runda 3) a inlocuit conceptul „Z pe subunitate" cu `ZEC` si `SCARA` pe gestiune; exemplele E01/E06/E09/E10/E14 de mai jos folosesc notatia `Z` pentru **scara de stocare** a gestiunii din exemplu.
- Formulele de raport (≈70 interogari) nu au fost toate urmarite; cele de mai sus sunt cele din VC00-03 (QBC/QFCS/QSFP/Balanta22/OI/MF/amortizare/fisa furnizor). Restul se urmaresc la VC08-03 cu RAPOARTE.md.


Revizie finală migrare 14.10.2026: exemplele și măsurătorile istorice N01/N02 de mai sus descriu diferențe observate; VRC nu le repară. Valorile efective ale sursei sunt păstrate. Numerotarea actuală este păstrată explicit la cererea utilizatorului.

## Revizie 16.10.2026 — factură comună și corecție NIR exactă

Cerință aprobată: pentru NIR-urile noi, diferența față de valoarea declarată nu
se acceptă doar în antet. Suma liniilor trebuie egalată exact. Corecția automată
(de cel mult 1 leu) păstrează TVA real prin calcul invers din brut, identificat
prin existența metadatelor în VC_NIR_CALCULE. Această regulă înlocuiește N01/N02 pentru salvarea NIR-urilor noi;
nu recalculează importurile istorice și nu schimbă aici regulile celorlalte bonuri.

Rândurile AUTO sunt nemodificabile; singura operație este ștergerea cu revenirea
cantității în grupul original, recalculare și simularea stocului pentru ambele
variante. La o factură comună, fiecare NIR se egalează cu propria valoare
repartizată; la finalizarea completă, totalul recepțiilor active este ValoareFC.
Antetele Access nu sunt însumate automat ca total global de factură.

Implementare și activare: [FACTURI_NIR_IMPLEMENTARE.md](FACTURI_NIR_IMPLEMENTARE.md).
Stare: surse și DDL pregătit, fără teste, compilări sau operații pe baze.
