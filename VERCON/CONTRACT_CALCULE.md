# VERCON — contractul de calcul, scalare INT si date etalon (SLICE-VC00-03)

Stare: **DOCUMENTAT** (11.10.2026). Nimic implementat, nimic executat pe server. Dovezile numerice vin din interogari **agregate doar-citire** pe cele trei baze reale (`tools/oracle_probe*.ps1`, rezultate brute in `tools/out/oracle_probe*.txt`). Sistemul Coduri/FIFO legacy este exclus (N06) si nu apare mai jos.

Eticheta: **dovada** = masurat pe bazele reale; **sursa** = citit in cod/interogare; **contract** = regula pe care o implementeaza motorul web; **ipoteza** = de verificat.

## 1. Contractul de scalare INT (D03, N03/N08) — revizuit 11.10.2026 dupa raspunsurile utilizatorului

**Decizii (utilizator):** (a) numarul de zecimale este `NR.ZEC`, indiferent de alt cod; (b) `NR.ZEC` este pe **gestiune**; (c) la migrare **nu se face niciun calcul** cu exceptia antetelor NIR/BC (0, NULL sau diferenta peste 1 leu fata de suma liniilor — Q1); (d) valorile sursa se **pastreaza exact** si se afiseaza cu `NR.ZEC` zecimale (Q2); (e) rotunjirea la afisare este **in sus** la jumatate (Q12).

### 1.1 Reguli

1. Toate coloanele de **valori** (cantitati, preturi unitare, valori de linie si de document) sunt `INT` cu semn in MariaDB. Nu exista `DECIMAL`/`DOUBLE` in DDL.
2. Doua numere distincte pe **gestiune**, ambele in `VC_Settings` la nivel `(SubunitId, GestiuneId)` (nu pe DC, nu implicit 2):
   - **`ZEC`** = `NR.ZEC` al gestiunii (fallback **4** cand lipseste, confirmat: `modStartup.Zec()` = `Nz(DLookup("ZEC","NR"),4)`). Este numarul de zecimale al **afisarii** si al **calculelor documentelor noi**. La Materiale/Alimente, cand fisierele anuale au `NR.ZEC` diferit, se ia **cel din anul cel mai recent** (Q3).
   - **`SCARA`** (scara de stocare) = cea mai mica scara `>= ZEC` care reprezinta **exact** toate valorile sursa ale gestiunii (calculata de migrator, aratata operatorului). Valoarea stocata = `valoare_reala x 10^SCARA`. Pe datele reale: Alimente `ZEC 4 → SCARA 4`; MF `ZEC 2 → SCARA 2`; OI `ZEC 2 → SCARA 3` (13 valori au a treia zecimala).
3. **La migrare NU se calculeaza si NU se rotunjeste nimic.** Valorile se copiaza exact in scara `SCARA`. Singura exceptie: antetul NIR/BC (vezi §2.3). `Intrari.TVA` si celelalte coloane se copiaza ca in sursa, chiar daca sunt inconsistente.
4. Documentele **noi** din web: valorile se calculeaza si se rotunjesc o singura data pe linie la `ZEC` (acolo unde sursa folosea `Round(...,Zec)`), apoi se stocheaza in `SCARA` (zerouri la coada). Modul de rotunjire al **calculelor**: `HALF_EVEN` (paritate cu `Round` VBA; **ipoteza**, de verificat la VC08-05); modul de rotunjire la **afisare**: **in sus la jumatate** (12,345 → 12,35; decizie Q12).
5. **Afisarea** rotunjeste fiecare valoare la `ZEC`; totalurile se calculeaza din valorile stocate (exacte) si se afiseaza rotunjite la `ZEC`. Poate rezulta ca suma liniilor afisate difera cu 0,01 de totalul afisat — comportament identic cu Access (care stocheaza exact si afiseaza cu `DecimalPlaces = Zec`). Cazuri reale: 13 valori OI.
6. `SCARA` este **imuabila dupa prima scriere business** a gestiunii; cresterea ei e o operatie explicita si exacta (inmultire cu 10^(S'-S), sub acces exclusiv, tranzactie, verificare de overflow); `ZEC` se poate schimba doar ca setare de afisare/calcul pentru documentele noi, fara a reinterpreta valorile stocate.
7. Preturile sunt `>= 0` (`CHECK` + validare API). Semnul apartine miscarii.
8. Aritmetica intermediara in intregi de precizie arbitrara; niciodata float. API transporta valori ca text zecimal exact sau `{coef, scala}`.
9. Cota TVA (`TVA.TVA`) este `INT = cota x 10000`.
10. Limita `|v| x 10^SCARA <= 2.147.483.647`: verificata inainte de import/scriere si **refuzata**, fara schimbarea tipului. (Cu scarile de mai sus nu se depaseste in cele trei baze, vezi §1.3.)
11. Gestiunile unei subunitati pot avea `ZEC`/`SCARA` diferite: totalurile pe subunitate se aduna pe 64 de biti convertind fiecare valoare la scara comuna a raportului. Nu exista conflict intre gestiuni.

### 1.2 Totalul documentului

Cu `SCARA` din §1.3 totalurile **incap** in INT: se stocheaza ca `INT` scalat ca orice valoare, cu verificarea de capacitate din regula 10. **Revizuire**: propunerea anterioara `AjustareAntet = H - S` (care evita depasirea la 4 zecimale) este retrasa — nu mai e necesara si nu s-a implementat nimic pe ea.

### 1.3 Profilul real (dovada: `oracle_probe2`, `oracle_probe4`)

| Baza | `NR.ZEC` | `SCARA` necesara | Valoare maxima pe linie | Total maxim pe document | Maxim stocabil la `SCARA` | Valori cu mai multe zecimale decat `NR.ZEC` |
|---|---:|---:|---:|---:|---:|---|
| Alimente | 4 | 4 | 5.747,43 | 10.944,71 | 214.748,3647 | 0 |
| Mijloace fixe | 2 | 2 | 196.354,35 | 1.262.155,34 | 21.474.836,47 | 0 |
| Obiecte inventar | 2 | **3** | 52.189,83 | 1.091.160,61 | 2.147.483,647 | `Intrari.Pret` 5, `Intrari.PUTVA` 5, `ProduseP.PUTVA` 5, `Iesiri.ValoareCUTVA` 8 (a treia zecimala, exact la jumatate) |

Concluzii: (a) cu `SCARA` de mai sus **nu exista depasiri de INT** (OI: 1.091.160,61 < 2.147.483,647; la `SCARA 4` ar fi depasit, de aceea scara se alege minim, nu maxim); (b) OI este singura baza unde `SCARA > ZEC`; (c) cele 13 valori OI se **pastreaza exact** si se **afiseaza** rotunjite in sus la 2 zecimale (12,345 → 12,35); diferenta fata de afisarea cu rotunjire bancara este 0,01 pe valoare; (d) ideea „stocam exact, afisam cu `NR.ZEC`" corespunde comportamentului Access (stocheaza `Double`, afiseaza cu `DecimalPlaces = Zec`).

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

Pentru fiecare linie **a unui document nou** (intregi scalati cu `SCARA`, rotunjire la `ZEC`, cota x10000; la migrare nu se aplica nimic din acest algoritm):

```
PUTVA_i         = rnd( Pret_i * (10000 + cota_i) / 10000 )
Valoare_i       = rnd( Cant_i * Pret_i   / 10^Z )
ValoareCUTVA_i  = rnd( Cant_i * PUTVA_i  / 10^Z )
TVA_i           = ValoareCUTVA_i - Valoare_i          (derivata)
S               = suma ValoareCUTVA_i                   (TVA pe linie, fara cota globala)
```

Sursa rotunjirii pe linie: `Utile.bas` (`fTVA` si rutina de ajustare linie) (`fTVA = Round(Pret + Pret*TVA, Zec)`, `Valoare = Round(cant*pret, Zec)`, `ValoareCuTva = Round(cant*putva, Zec)`). Pretul din care se calculeaza poate fi deja TVA-inclus: un pret/total deja TVA-inclus nu primeste TVA a doua oara (se specifica prin cota 0 sau prin sursa `PUTVA`). Lipsa cotei nu se inlocuieste cu o cota globala.

### 2.3 Regula totalului (N01/N02)

> **Revizie 11.10.2026 — confirmata (Q1):** la migrare ramane regula N01/N02 integral: antet `0`, `NULL` sau `abs(H-S) > 1 leu` -> total = suma liniilor `S`; in celelalte cazuri antetul se pastreaza exact. **Acestea sunt singurele calcule de la migrare**; nimic altceva nu se recalculeaza (nici TVA, nici preturi, nici valori de linie). Originalul antetului ramane in proveniența importului. Aceeasi regula se aplica documentelor noi din web (NIR/FC si BC).

Regula (N01/N02):

```
H = 0                    -> total = S
abs(H - S) > 1 leu       -> total = S
abs(H - S) <= 1 leu, H<>0 -> total = H   (exact 1 leu pastreaza H)
H = NULL                 -> marcat lipsa; total = S cu conversie explicita in proveniență (nu abs(NULL))
```

Comparatia se face in unitati monetare reale (nu pe coeficienti cu scari diferite) si **fara rotunjire automata la 2 zecimale**. Aceeasi functie in migrator, FC si BC (un singur serviciu numeric).

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

### 3.6 Salvare FC/BC/BR/FCS/OI si inchidere an

Protocolul de scriere este cel din PLAN (sesiune + acces exclusiv + tranzactie + RequestKey + calcul + audit + COMMIT). Defectele de atomicitate (N10) se rezolva prin tranzactie unica: variante, MF (`ProduseM`), registrul OI, `LegBCLA`. `IntrariStorno.PUTVA` si `IesiriRetur.PUTVA` sunt `Long` in sursa: se citesc ca intregi, se reprezinta exact in `SCARA` gestiunii (fara fractiune inventata; copiate ca in sursa, fara calcul la migrare); diferenta fata de `ProduseP.PUTVA` este in proveniență (Alimente: 8 linii storno).

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
| E09 | overflow | valoare 214.748,3648 cu `Z=4` | refuz inainte de scriere |
| E13 | valoare sursa cu mai multe zecimale decat `NR.ZEC` | OI: pret 12,345 cu `ZEC=2`, `SCARA=3` | stocat exact `12345`; afisat 12,35 (in sus); 13 randuri reale |
| E14 | `NR.ZEC` NULL | baza fara valoare in `NR.ZEC` | `Z=4` (fallback cod) |
| E15 | `NR.ZEC` diferit intre fisierele anuale ale aceleiasi gestiuni | Alimente `tz_2024` cu 4, `tz_2025` cu 2 | `ZEC` din anul cel mai recent (2); `SCARA` = max necesar din toate anii (>= 4 daca datele cer) |
| E16 | antet NIR/BC | `H=0`, `NULL`, diferenta 486,71 (BC 7 MF), diferenta 0,99 | `S`, `S`, `S`, `H` neschimbat; nimic altceva recalculat |
| E17 | stoc initial | NIR pe «furnizor» `STOC INITIAL`/`S.I.`/«Stoc Initial 2025» (cu/fara diacritice) | document de tip «Stoc initial» fara partener; operatorul confirma lista in migrator |
| E10 | scara | 100,25 cu `Z=4`; apoi afisare 2/3/4 zecimale | stocat 1002500 neschimbat |
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
