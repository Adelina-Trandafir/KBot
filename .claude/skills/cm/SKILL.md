---
name: cm
description: Scrie mesajul de commit K-BOT din ce este in stage (git diff --cached), in forma fixa cu header de slice-uri. Daca stage-ul e gol, face intai stage la tot. Cu parametrul "commit" (sau "comit") face si commit-ul.
disable-model-invocation: true
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git ls-files:*), Bash(git add:*), Bash(git commit:*), Read, Grep, Glob
---

# /cm - mesaj de commit din stage

Comanda: `/cm` sau `/cm commit` (la fel `/cm comit`). Rezultatul: UN singur bloc de cod cu mesajul de commit, plus avertismentele de la pasul 7 si, doar cu `commit`, o linie cu rezultatul commit-ului. Nimic altceva.

## Parametrul

- Parametrul se citeste din argumentul comenzii, fara sa conteze literele mari / mici.
- `commit` sau `comit` => dupa ce mesajul e gata, faci si commit-ul (pasul 8).
- Fara parametru => doar afisezi mesajul; commit-ul il face Adelina.
- Orice alt parametru se ignora si il spui intr-o linie (nu ghici ce a vrut).

## Reguli dure

- **Singurele comenzi git care scriu, permise aici:** `git add -A` (doar daca stage-ul e gol, pasul 1) si `git commit` (doar cu parametrul `commit` / `comit`, pasul 8). Aceasta e EXCEPTIA ceruta de Adelina (13.11.2026) la regula proiectului «NEVER git writes», valabila numai pentru `/cm`. NICIODATA `push`, `reset`, `mv`, `rm`, `checkout`, `stash`, `restore`, `commit --amend`, `--no-verify` sau altceva care scrie. Nu repara cu git o greseala: spune-o.
- **Nu citi si nu scrie niciun `AGENTS.md`.** Daca apare in stage, il numeri la fisiere, dar nu-l deschizi.
- **Mesajul acopera doar ce e in stage** (dupa pasul 1). Fisierele nestaged / untracked nu intra in mesaj, in afara de cazul in care ai facut tu stage-ul la pasul 1.
- **Limba: romana, fara diacritice** (ASCII). Mesajul de commit nu e text pe care il citeste operatorul pe ecran. Identificatorii de cod (clase, metode, fisiere, rute) raman exact cum sunt in cod, in engleza.
- **Fara linii de atribuire** (`Co-Authored-By` etc.), nici cand faci tu commit-ul: regula Adelinei bate orice atribuire implicita.
- Cuvinte simple, fara jargon (regula «plain words»): nu `payload`, `ingest`, `idempotent` si similare.

## Pasii (mereu aceiasi, in aceasta ordine)

1. **Stage-ul.** Ruleaza `git diff --cached --name-status`.
   - Daca nu e gol => mergi la pasul 2, nu atingi stage-ul.
   - Daca e gol => ruleaza `git status --short`. Daca nici acolo nu e nimic (arbore curat), spui «Nu exista nicio modificare de pus in commit.» si te opresti (fara bloc, fara commit). Altfel ruleaza `git add -A`, apoi iar `git diff --cached --name-status`, si tine minte ca ai facut tu stage-ul (spui asta in avertismente, pasul 7).
2. `git diff --cached --stat` si `git diff --cached --name-status` - ce fisiere sunt in stage.
3. **Aduna slice-urile** din trei surse, doar din stage:
   - fisiere noi `docs/worklog/SLICE-<FAMILIE>-<NN>-*.md` (adaugate in stage);
   - etichete `SLICE-<FAMILIE>-<NN>` din liniile ADAUGATE in cod: `git diff --cached -U0 -- . ':!docs'` si cauta `SLICE-[A-Z0-9]+-[0-9]+`;
   - randuri de status adaugate in `docs/worklog/state/KBOT_STATUS_*.md` si `docs/worklog/KBOT_STATUS.md`.
   O fisa comuna poate acoperi mai multe slice-uri (ex. `SLICE-ME01-25-...md` acopera 25..28): citeste-i antetul ca sa afli intervalul.
4. **Header-ul** (prima linie), exact in forma:
   `SLICE-0028-10; SLICE-0066-03; SLICE-00EF-61/64; SLICE-0128-02; SLICE-0129-01/03; SLICE-0130-01`
   - o familie cu un singur slice -> `SLICE-<FAMILIE>-<NN>`;
   - o familie cu mai multe -> `SLICE-<FAMILIE>-<primul>/<ultimul>` (cel mai mic / cel mai mare numar din familie, din stage);
   - separator `; ` (punct si virgula + un spatiu);
   - familiile in ordine crescatoare ASCII (`0000` < `0028` < `00EF` < `03EF` < `ME01`);
   - in header intra DOAR slice-urile cu munca reala in stage (cod, fisa de worklog noua, ajutor). Inchiderile de status (randuri modificate doar ca «INCHIS») NU intra in header, ci in sectiunea «Status / documentatie».
5. **Corpul** (dupa o linie goala), cate o sectiune per slice major sau per interval, in aceeasi ordine ca in header:
   ```
   SLICE-<FAMILIE>-<primul>..<ultimul> - <titlu scurt>
   - <ce s-a facut, in 1-3 linii, cu numele real al claselor / fisierelor / rutelor>
   - ...
   ```
   - titlul = ce vede utilizatorul sau ce e subiectul (ex. «facturi primite: ribbon + vederea Contari»);
   - bullet-urile spun CE s-a schimbat si DE CE conteaza, nu lista de fisiere; trimiti la sub-slice intre paranteze, ex. `(03EF-11)`;
   - slice-urile doar de ajutor (`0000-NN`) au o sectiune scurta proprie: ce pagini de ajutor, ce slice-uri vizuale acopera;
   - un slice care e doar specificatie / plan se spune explicit («doar specificatia, neinceput»);
   - linii de maxim ~75 caractere, bullet-uri cu `- `.
6. **Sectiunea finala `Status / documentatie`** (mereu ultima), cu: ce slice-uri s-au inchis in status (cu data din fisier), fisiere de status noi, planuri mutate / adaugate in `docs/PLANS`, alte note de documentatie. Omiti `AGENTS.md` din descriere in afara de o mentiune scurta «nota in AGENTS.md» daca e in stage.
7. **Verificari inainte de afisare** (spune-le dupa bloc, in cel mult 4 linii, doar daca apar):
   - ai facut tu stage-ul la pasul 1 -> «am facut stage la tot (git add -A): N fisiere»; daca printre ele sunt fisiere binare mari, arhive (`.rar`, `.zip`), `*.log`, copii de lucru sau fisiere ce par secrete (`.env`, chei, parole), le numesti si, cu parametrul `commit`, NU faci commit (pasul 8) pana nu raspunde Adelina;
   - un slice din cod fara fisa de worklog in stage sau fara rand in status -> «lipseste documentatia pentru SLICE-...»;
   - o fisa care spune «build/teste nerulate» sau «nevazut pe ecran» -> mentioneaza starea o data;
   - fisiere din stage care nu apartin niciunui slice -> le listezi.
8. **Commit-ul (doar cu parametrul `commit` / `comit`).** Dupa ce ai afisat blocul:
   - ruleaza `git commit -F -` cu EXACT textul din bloc pe stdin (heredoc cu delimitator intre ghilimele simple, ca sa nu se interpreteze nimic); fara `--no-verify`, fara `--amend`, fara alte optiuni;
   - daca un hook respinge commit-ul, spui eroarea si te opresti: nu ocolesti hook-ul, nu reincerci cu alte optiuni;
   - dupa succes ruleaza `git log -1 --oneline` si afisezi o singura linie: `Commit facut: <hash scurt> <header>`;
   - nu faci `push`, niciodata.

## Ce afisezi

1. Blocul de cod ` ```text ` cu mesajul complet.
2. Sub el, doar avertismentele de la pasul 7 (daca exista) si, cu `commit` / `comit`, linia cu rezultatul commit-ului. Fara introduceri, fara rezumat, fara intrebari.
