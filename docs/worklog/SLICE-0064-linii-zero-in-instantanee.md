# SLICE 0064 — Liniile cu zero ale unui instantaneu sunt linii și se păstrează (F31)

**Data:** 17.09.2026
**Cerut de operator:** «Unele H au indicatori cu valoare zero. Nu e o eroare — înseamnă că nu
s-a schimbat nimic la acel indicator. Când iau un H neașezat din coș, văd doar rândul care
are valoare, și asta declanșează «Instantaneul pierde indicatorii…», deci nu-l pot pune unde
trebuie. Ori luăm din MariaDB și indicatorii cu 0, ori nu mai folosim mesajul. Aș merge pe prima.»

S-a mers pe prima.

---

## 1. De unde venea refuzul

`MotivulRefuzului` din `AsociereForm` (și perechea lui de pe server, `valideaza_plasarile`
din `prelucrare_asociere.py`) aplică F16: de-a lungul lanțului ordonat după `DataH`, mulțimea
indicatorilor unui instantaneu trebuie s-o conțină pe a celui dinainte. Mulțimile se iau din
`FX_Receptii` — liniile instantaneului.

Numai că liniile cu zero **nu ajungeau niciodată în `FX_Receptii`**. Pasul 4a al ingestiei
(`prelucrare_pasi.step4a_populeaza_receptii`) e portul fidel al lui `FX_Istoric_Populeaza_
Receptii` din Access (`mdl_FX_Istoric.md`, linia 623: `ElseIf CDbl(rcHis!Val_Receptie) <> 0`),
care păstra doar rândurile cu valoare. Un indicator care nu s-a mișcat într-o salvare (site-ul
îl trimite cu `Suma receptie: 0 RON`) dispărea deci din instantaneu, deși recepția îl are în
continuare în `RHR` (pasul 4b scrie TOATE rândurile din `Detaliu`, și pe cele cu zero).
Rezultatul: primul instantaneu al lanțului numea {AAB, AA2}, următorul — cu AA2 la zero — numea
doar {AAB}, și F16 spunea, corect după datele pe care le vedea, că AA2 «a dispărut». Mesajul
însuși zice «un indicator poate cădea la zero, dar nu poate dispărea» — regula presupunea exact
liniile pe care ingestia le arunca.

Nu era un defect al vetoului, ci al datelor de sub el. Vetoul rămâne.

## 2. Ce s-a schimbat

**`PYTHON/routes/forexe/prelucrare_helpers.py`** — `este_linie_receptie(rand)` (nou): un rând
de istoric care nu e antet e LINIE dacă are `Val_Receptie <> 0` **sau** numește un indicator
(`CodAI` nevid). Un rând cu zero fără indicator se sare, ca înainte; un rând cu valoare al cărui
indicator lipsește din `FX_Indicatori` ridică în continuare aceeași eroare. Criteriul trăiește
o singură dată, fiindcă îl folosesc două plimbări care trebuie să fie de acord:

* **`prelucrare_pasi.step4a_populeaza_receptii`** — `elif este_linie_receptie(r)` în locul lui
  `elif float(r["Val_Receptie"] or 0) != 0`. Linia cu zero se scrie cu `Valoare = 0`,
  `ValoareOrig = 0`, hash-ul calculat pe 0, `TipIntern` după aceeași regulă.
* **`receptii_refacere.refa_receptii`** (felia 0062) — aceeași înlocuire. Prin asta refacerea
  devine și **umplerea înapoi** pentru F31: liniile cu zero pe care ingestiile de dinainte le-au
  aruncat sunt «lipsă după `IDH`», se inserează sub antetul lor existent, iar `DIF`/`DIFC` ale
  lanțului atins se recalculează (`step4d_calculeaza_dif`).

**`docs/FUNDAMENT_Asociere_Receptii.md`** — **F31** adăugat, cu sursa VBA și motivul.

**`src/KBot.App/Forexe/AsociereForm.vb`** — doar comentariul lui `MotivulRefuzului`: spune de
unde vin mulțimile, că zerourile sunt acum acolo, și că nu are voie să apară un filtru pe
`Valoare`. Nicio linie de cod VB schimbată.

## 3. Ce se schimbă în rest, prin construcție

* **`SUM(DIF)` coboară acum odată cu indicatorul.** Înainte, un indicator cu 100 în H1 și 0 în
  H2 lăsa în `FX_Receptii` doar linia lui H1 (`DIF = 100`); acum H2 are linia cu 0 și
  `DIF = -100`, deci `Sumar.TotalReceptii` și `qFX_ORD_REC_ANT` (care adună `DIF`) dau valoarea
  reală. Access avea același defect — nu se copiază.
* **Recepțiile reconstituite** (F26) își primesc `RHR` din ultimul instantaneu cu linii, deci
  vor avea și rânduri cu 0 — exact cum le are o recepție descărcată de pe site.
* **F15** compară liniile cu zerourile deja scoase pe amândouă laturile (`_valori_pe_indicator`,
  felia 0058), deci nu vede nicio diferență.
* **Vederea Recepții** arată un rând per linie `FX_Receptii`: sub un antet vor apărea și liniile
  cu 0. E adevărul instantaneului; dacă operatorul le vrea ascunse, e un filtru de afișare,
  nu unul de ingestie.
* Pasul 4c (trecerea automată) potrivește doar pe `Total` — neatins.

## 4. Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/prelucrare_helpers.py` | `este_linie_receptie` (nou) |
| `PYTHON/routes/forexe/prelucrare_pasi.py` | 4a folosește criteriul; docstring F31 |
| `PYTHON/routes/forexe/receptii_refacere.py` | refacerea folosește criteriul; docstring |
| `PYTHON/tests/test_forexe_prelucrare_pasi.py` | +2 teste (zero cu indicator = linie; zero fără indicator = sărit) |
| `PYTHON/tests/test_forexe_receptii_refacere.py` | +1 test (linia cu zero aruncată de o ingestie veche se pune înapoi sub antet, cu DIF recalculat) |
| `docs/FUNDAMENT_Asociere_Receptii.md` | F31 |
| `src/KBot.App/Forexe/AsociereForm.vb` | comentariu la `MotivulRefuzului` |

## 5. Rezultatele testelor

* PYTHON (`.venv`): suita întreagă **538 trecute / 26 sărite / 0 picate** (de la 535/26;
  +3 teste noi). Cele patru fișiere direct atinse: 138 trecute.
* `dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente** (schimbare de
  comentariu, dar s-a compilat).
* Suitele VB.NET nu s-au rulat — nicio linie de cod VB nu s-a schimbat.

## 6. Ce rămâne neverificat / de făcut de operator

* **Nimic n-a rulat pe MariaDB.** Că site-ul trimite într-adevăr rândul cu `Suma receptie: 0 RON`
  pentru indicatorul nemișcat e afirmația operatorului (F31 `OPERATOR`), nu un rând citit
  de mine din `FX_Istoric`. Dacă rândul cu zero lipsește din istoric, atunci nu are de unde
  veni și nici refacerea nu-l poate inventa.
* **Datele deja ingerate NU se repară singure.** Pentru angajamentele descărcate înainte de
  azi, operatorul trebuie să apese **«Refacere din istoric»** (vederea Recepții, felia 0062) — pe
  angajamentul cu pricina sau pe toată baza; proba (`aplica = false`) arată întâi câte linii ar
  adăuga. Abia după aceea `MotivulRefuzului` vede și indicatorii cu zero pe lanțurile vechi.
* Zerourile în vederea Recepții nu s-au văzut pe ecran.
* Rândurile 0062 și 0063 lipsesc încă din registrul `KBOT_STATUS.md` (worklog-urile lor există,
  necomise); s-a adăugat doar rândul 0064 și s-a mutat numărul liber la 0065.

---

# 0064-02 — Corrective follow-up: salvarea din editor cădea cu «'rand_istoric'»; antetele fără linii se ignoră peste tot (F32)

**Data:** 17.09.2026 (aceeași zi)
**Raportat de operator:** «În AsociereForm, la salvare: `ApiException: Eroare la salvarea
asocierii: 'rand_istoric'`. Se întâmplă la ORICE salvare, nu doar pe anumite rânduri. Separat:
am descoperit în FX_Istoric rânduri care au doar rândul-total (fără niciun indicator). Sunt
erori din vechea aplicație Access și trebuie ignorate complet, în AsociereForm și în orice alt
calcul.»

Două defecte, fără legătură între ele, ambele confirmate pe cod.

## 1. Căderea la salvare — de unde venea

`'rand_istoric'` este un `KeyError` din Python, ajuns la operator prin ramura `except Exception`
a lui `post_asociere` (500, `Eroare la salvarea asocierii: {e}`).

Editorul de oricând (`routes/forexe/asociere.py`) ancorează instantaneele pe `idrh` — nu există
sarcină utilă, deci nu există indice de rând (scris în antetul fișierului). `normalizeaza_comenzi`
pune totuși pe fiecare COMANDĂ aliasul `rand_istoric = idrh`, «ca să putem refolosi neschimbate
funcțiile din prelucrare_asociere». Dar `citeste_instantanee` din același fișier NU punea aliasul
pe INSTANTANEE. Felia 0058 (jurnalul `asociere.log`) a adăugat în `valideaza_plasarile` un
`journal.table(...)` peste lanț care citește `i["rand_istoric"]` — iar lista de argumente se
evaluează chiar dacă jurnalul e stins. `valideaza_plasarile` rulează la fiecare salvare care
atinge un lanț cu cel puțin un instantaneu rămas, deci **orice salvare** cădea acolo.
`materializeaza_reconstituite` (reconstituirile, F26) cheiază la fel și ar fi căzut și ea.

**Fix:** `citeste_instantanee` din `asociere.py` pune `"rand_istoric": idrh` pe fiecare
instantaneu, exact ca pe comenzi. Un rând, plus docstring. Clientul VB citește GET-ul prin
`GetAsociereInstantaneu`, care nu are câmpul, deci îl ignoră — nimic de schimbat în VB.

## 2. Antetele fără linii — F32

În istoric, un instantaneu = rândurile-linie (câte unul per indicator) urmate de rândul-antet
(`(activ:true)`, cu totalul). Pasul 4a acumulează liniile într-un tampon și le varsă sub antet
când sosește. Un antet fără nicio linie înaintea lui producea un `FX_Receptii_H` cu zero rânduri
în `FX_Receptii`. Vederea Recepții îl arăta deliberat (LEFT JOIN, «un antet FĂRĂ linii nu are
voie să DISPARĂ din arbore», ca în Access); editorul îl arăta; F16 îl vedea cu mulțimea de
indicatori VIDĂ în mijlocul lanțului (deci «indicatorii dispar» pe un rând pe care operatorul nu
l-a atins); `DIFH` îl lua ca `precedent`; `recalculeaza_final` îl putea face `Final`; trecerea
automată 4c îl putea potrivi pe `Total`.

Singurul antet fără linii care E instantaneu: rândul de ștergere (F21). Rămâne.

**Criteriul, o singură dată** (`prelucrare_helpers.py`):
* `is_header_only_snapshot(este_stergere, line_rows_seen)` — pentru cele două plimbări peste
  istoric. Numără RÂNDURILE-LINIE văzute de la antetul precedent, nu liniile construite: o linie
  cu indicator necunoscut în `FX_Indicatori` (refacerea o sare și o spune) tot face antetul un
  instantaneu adevărat, ca ancora să rămână și linia să se poată atașa la o refacere ulterioară.
* `SNAPSHOT_COUNTS_SQL` — predicatul de citire: `(COALESCE(H.EsteStergere,0) <> 0 OR EXISTS
  (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H.IDRH))`, cu tabelul aliasat `H`.

**Unde se aplică:**

| Fișier | Ce |
|---|---|
| `prelucrare_pasi.step4a_populeaza_receptii` | antetul fără linii (și nu ștergere) NU se inserează; `logger.info` per antet + total; docstring F32. Rândul de istoric primește oricum `Prelucrat = 1` la pasul 7, deci nu se recitește |
| `prelucrare_pasi._DIF_H_SQL` (4d) | filtru F32 — nu mai e `precedent` pentru instantaneul real de după el |
| `receptii_refacere.refa_receptii` | nu se inserează; dacă există deja în bază rămâne (o ștergere ar trebui să ajungă la `FX_ORD.IDRH`) dar se numește în `avertismente` cu IDRH-ul; docstring |
| `prelucrare_asociere._INSTANTANEE_SQL`, `_TOATE_INSTANTANEELE_SQL` | tabloul de hotărât și contextul propunerii |
| `prelucrare_asociere._H_LANT_SQL` (`recalculeaza_final`) | antetul gol nu poate fi «ultimul» |
| `prelucrare_asociere._H_MEMBRI_LANT_SQL` (nou, în locul SELECT-ului inline din `aplica_decizii`) | membrii deja așezați ai lanțului pe care îi judecă F15/F16 |
| `asociere._INSTANTANEE_SQL` | editorul de oricând; o comandă care ar numi un antet gol cade în `verifica_blocajele` cu «nu există pe acest angajament» |
| `receptii._SQL_RECEPTII` | vederea Recepții; comentariul despre LEFT JOIN rescris: LEFT JOIN-ul rămâne PENTRU rândul de ștergere |
| `tree._SELECT` (`AreReceptii`) | un angajament cu numai antete goale nu aprinde steagul |
| `docs/FUNDAMENT_Asociere_Receptii.md` | **F32** + linia de revizie din antet (F31 și F32) |

**Neatinse, deliberat:** `_AMPRENTA_SQL` (`hc`/`hm`/`hn`) — amprenta e un hash comparat între
GET și POST ale aceluiași cod, deci filtrul nu-i schimbă rostul; `_BLOCAJE_SQL` — un antet gol
niciodată returnat nu poate fi comandat; `_R_DEMARCHEAZA_SQL` — privește doar rândurile de
ștergere; `ord_edit._SQL_REC_ANT` și `_DIF_R_SQL` — pornesc din LINII, deci antetele goale nu
contribuiau oricum. Nicio linie VB schimbată.

## 3. Teste

* `test_forexe_asociere.py` (+2): `citeste_instantanee` din editor poartă `rand_istoric == idrh`
  și trece prin `valideaza_plasarile`; **cap la cap** — lanț de DOUĂ instantanee, se desprinde
  unul, POST-ul ajunge la commit (200). Amândouă **verificate că pică fără fix** (aliasul scos
  temporar: `KeyError: 'rand_istoric'`, respectiv 500 — exact eroarea operatorului). Cu un
  singur instantaneu lanțul rezultat e gol și defectul nu se vedea — de-asta două. Falsul
  `FakeConnection` a învățat `UPDATE`, `_H_LANT_SQL` și `_RECONSTITUITE_SQL`; dispecerizarea
  pe prefix deosebește acum `_BLOCAJE_SQL` de `_INSTANTANEE_SQL` (ambele încep cu `SELECT H.IDRH`)
  după ` AS ord_h`.
* `test_forexe_prelucrare_pasi.py` (+4): antetul fără linii nu produce H; antetul gol nu fură
  liniile antetului următor și nu consumă `NrCrt`; criteriul scutește ștergerea; **paza**: toate
  cele opt citiri ale lui `FX_Receptii_H` conțin `SNAPSHOT_COUNTS_SQL`. Două teste vechi aveau ca
  fixtură incidentală chiar un antet fără linii — au primit o linie; cel cu «zero fără indicator»
  asertează acum că singura linie scrisă e cea reală.
* `test_forexe_receptii_refacere.py` (+2): antetul gol nici nu se inserează, nici nu se numără
  «lipsă», și e numit în avertismente; unul deja în bază rămâne neatins și e numit cu IDRH.
  Cheia moartă `SELECT IDRH, Total FROM FX_Receptii_H` din falsul cursor actualizată la forma
  nouă (dispecerul cade pe `[]` pentru prefixe necunoscute, deci era inertă, dar mințea).

PYTHON (`.venv`, `KBOT_ASOCIERE=0`): suita întreagă **546 trecute / 26 sărite / 0 picate**
(de la 538/26; +8 teste).

## 4. Ce rămâne neverificat

* **Nimic n-a rulat pe MariaDB.** Că `EXISTS (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H.IDRH)`
  e ieftin pe volumul real nu s-a măsurat; `FX_Receptii.IDRH` e FK, deci ar trebui indexat.
* **Salvarea din AsociereForm nu s-a apăsat pe ecran** după fix; drumul e acoperit cap la cap
  doar de testul cu falsul de conexiune.
* **Antetele goale deja în bază rămân în `FX_Receptii_H`**, nevăzute. Nu se șterg: un
  `DELETE` ar trebui să se uite la `FX_ORD.IDRH` și la orice altceva îi ține cheia. Refacerea
  le numește în avertismente, ca operatorul să știe că sunt.
* **Instantaneele ingerate înainte de F31 ale căror singure linii erau zerouri** arată, în
  `FX_Receptii`, exact ca un antet gol și sunt ascunse de același filtru până la «Refacere din
  istoric» — reparația pe care 0064 o cerea deja. Până n-o apasă operatorul, pe angajamentele
  vechi pot lipsi din editor instantanee reale «fără schimbare».
