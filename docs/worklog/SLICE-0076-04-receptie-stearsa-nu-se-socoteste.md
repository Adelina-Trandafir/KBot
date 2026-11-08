# SLICE-0076-04 - receptia stearsa nu se mai socoteste (arbore, tooltip, ordonantari, verificari)

**Data:** 07.11.2026. **Cerere (operator):** o receptie stearsa in K-BOT a fost recunoscuta ca stearsa, dar era luata in considerare in verificarile din arborele principal, in arborele Receptii, in tooltip si in ordonantari. «TREBUIE SA NU FIE LUATE IN CALCUL GENUL ASTA DE RECEPTII»; in fereastra de asocieri sa fie scoasa in evidenta. Continua [0076-02](SLICE-0076-02-receptie-stearsa-in-browser.md). (0076-03 ramane rezervat pentru «plasa pentru schimbari din afara K-BOT».)

## 1. Ce s-a gasit (din jurnalele din `jurnale_alti_utilizatori`)

- `capturi_forexe.log` 11:45:05: receptia din 10/11/2026 a lui `AAB3BP4TMXT` (IDRH 791, IDR 987) stearsa in FOREXE; lista a scazut de la 23 la 22 de randuri (`Log_20261107_114025.txt`).
- `mesaje_operator.log` 11:45:35: ingestul a asezat singur randul «Stergere receptie» (`asocieri.asociat: 1`, nicio refuzare 409) — deci receptia are `Sters = 1`, `StersObservatLa` si rand de stergere.
- `mesaje_operator.log` 11:48:20: **«07.11.2026 AAB: rezervat 9.008,07, receptionat 9.324,86»** — diferenta (316,79) e chiar receptia stearsa, inca numarata.
- **Cauza:** randul de stergere (F21) nu are linii, deci nimic nu anuleaza liniile `FX_Receptii` (`DIF`) ale receptiei sterse. Toate cititoarele care aduna `DIF` le numarau mai departe; `H.Sters` (nivelul instantaneelor, «nu consemneaza nicio schimbare») nu are legatura cu `R.Sters` (receptia stearsa). F22 (operator, 26.08.2026) spune exact opusul: o receptie stearsa se numara pentru platile **dinainte** de stergere si **nu** dupa; nu era implementata nicaieri in sume.

## 2. Ce s-a facut

- **Un singur loc:** `prelucrare_helpers.fara_receptii_sterse(alias_h, pana_la=None)` + `STERGERE_DATA_SQL`. O receptie e stearsa cand `FX_Receptii_R.Sters = 1` si are o data de stergere: `StersObservatLa` (vazuta in FOREXE) sau, in lipsa, `DataH` a randului ei `EsteStergere`. Fara `pana_la`: scoasa de la prima zi. Cu `pana_la` (ordonantari): scoasa doar daca stergerea e in ziua `pana_la` sau inainte.
- **Ordonantari** (`ord_edit._SQL_REC_ANT`): cu `pana_la = DT` (F22 pastrata: plati dinaintea stergerii vad receptia). `incarca_dic_receptii` trimite `DT` de doua ori.
- **Arborele principal:** `receptii_cumulat._SQL_RECEPTII` (verificarea receptii vs rezervari: tooltip + rand rosu + avertismentul la salvare) si `tree._LANT_NEINCHIS` — fara receptiile sterse.
- **Sumar** (`sumar.py`, `aggRec` — total receptii): liniile receptiilor sterse nu se aduna.
- **Vederea Receptii:** `receptii.py` trimite `data_stergere_r` (data stergerii sau null); `GetReceptieRow.data_stergere_r` → `ReceptieRow.DataStergereR`; `ReceptiiView`: `EsteStearsa` (data de stergere SAU ultimul antet = rand de stergere) → `ValoareaReceptiei` = 0 → totalul lunii, al radacinii, cumulul si tooltip-urile nu o mai includ; semnul «[stearsa · nu se socoteste]»; grila unei luni/radacini nu-i aduna indicatorii (pe nodul receptiei sterse ii vezi).
- **Fereastra Asocieri** (`AsociereForm`): fond rosiatic din paleta (`ErrorColor` amestecat 0,3 peste `SurfaceAlt`, in loc de nuanta stinsa) pe orice receptie `Sters`; rand «ȘTEARSĂ» (rosu) in tooltip-ul-tabel si text in tooltip-ul simplu.
- **Ajutor:** `vederi/receptii.md` (alineat nou, tag 0076-04), `asocieri/fereastra.md` (tabelul de semne, tag 0076-04).

## 3. Fisiere atinse

`PYTHON/routes/forexe/`: `prelucrare_helpers.py`, `ord_edit.py`, `receptii_cumulat.py`, `tree.py`, `sumar.py`, `receptii.py`. `src/`: `KBot.Api/UpsertAngajamenteRequest.vb`, `KBot.Api/ApiClient.vb`, `KBot.Domain/ReceptiiInfo.vb`, `KBot.App/KBotForm/Views/ReceptiiView.vb`, `KBot.App/Forexe/Asociere/AsociereForm.vb`. Ajutor: doua fisiere .md.

## 4. Verificare

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. `py_compile` pe cele 6 fisiere Python: OK; fragmentul SQL generat, citit cu ochii. **Nimic rulat**: nici K-BOT, nici MariaDB, nicio suita (nu s-au scris teste).

## 5. Nerezolvat / neverificat

- **Nu s-a rulat pe date.** Pe `AAB3BP4TMXT` cifra asteptata: dupa punerea fisierelor Python pe server, avertismentul «rezervat 9.008,07, receptionat 9.324,86» trebuie sa dispara (receptionat devine 9.008,07 daca singura diferenta era receptia stearsa).
- **Efect mai larg decat o receptie observata:** F22 e aplicata acum pentru ORICE receptie cu `Sters = 1` si rand de stergere (cele vechi, din Access, incluse): nu mai intra in sume de la data stergerii. Receptiile cu `Sters = 1` fara nicio data (reconstituite fara rand de stergere asezat) raman numarate ca pana acum.
- **Nepuse in mana:** `ddf_edit.py` (`_SQL_RECEPTII_PANA_LA`, `ValRec`) si `clasificatii_edit.py` (TotalReceptii) aduna tot `DIF` si numara inca receptiile sterse; `primite_ke.py` le exclude deja. Daca vrei aceeasi regula si acolo, e o felie separata.
- Eticheta «Toate recepțiile» din tooltip-ul radacinii numara si receptiile sterse (e un numar de receptii, nu o suma).
- Un tooltip de receptie-ștearsă arată cumulul până la ea fără ea (valoarea ei e 0) — așa cum a cerut operatorul.
- Nimic pus pe server; `Register-DeploymentTask.ps1` nerulat (doar dupa testul Adelinei). `Check-Help.ps1` nerulat.
