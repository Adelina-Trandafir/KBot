---
name: cm
description: Scrie mesajul de commit K-BOT din ce este in stage (git diff --cached), in forma fixa cu header de slice-uri. Doar citeste git si afiseaza mesajul; nu face commit.
disable-model-invocation: true
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git ls-files:*), Read, Grep, Glob
---

# /cm - mesaj de commit din stage

Comanda: `/cm`. Rezultatul: UN singur bloc de cod cu mesajul de commit, plus avertismentele de la pasul 5. Nimic altceva.

## Reguli dure

- **Doar citire.** Folosesti numai `git status`, `git diff --cached`, `git log`, `git ls-files`. NICIODATA `git add`, `commit`, `push`, `reset`, `mv` sau altceva care scrie (regula proiectului «NEVER git writes»). Mesajul il copiaza Adelina.
- **Nu citi si nu scrie niciun `AGENTS.md`.** Daca apare in stage, il numeri la fisiere, dar nu-l deschizi.
- **Doar ce e in stage.** Fisierele nestaged / untracked nu intra in mesaj.
- **Limba: romana, fara diacritice** (ASCII). Mesajul de commit nu e text pe care il citeste operatorul pe ecran. Identificatorii de cod (clase, metode, fisiere, rute) raman exact cum sunt in cod, in engleza.
- **Fara linii de atribuire** (`Co-Authored-By` etc.): commit-ul il face Adelina.
- Cuvinte simple, fara jargon (regula «plain words»): nu `payload`, `ingest`, `idempotent` si similare.

## Pasii (mereu aceiasi, in aceasta ordine)

1. `git diff --cached --stat` si `git diff --cached --name-status` - ce fisiere sunt in stage.
2. **Aduna slice-urile** din trei surse, doar din stage:
   - fisiere noi `docs/worklog/SLICE-<FAMILIE>-<NN>-*.md` (adaugate in stage);
   - etichete `SLICE-<FAMILIE>-<NN>` din liniile ADAUGATE in cod: `git diff --cached -U0 -- . ':!docs'` si cauta `SLICE-[A-Z0-9]+-[0-9]+`;
   - randuri de status adaugate in `docs/worklog/state/KBOT_STATUS_*.md` si `docs/worklog/KBOT_STATUS.md`.
   O fisa comuna poate acoperi mai multe slice-uri (ex. `SLICE-ME01-25-...md` acopera 25..28): citeste-i antetul ca sa afli intervalul.
3. **Header-ul** (prima linie), exact in forma:
   `SLICE-0028-10; SLICE-0066-03; SLICE-00EF-61/64; SLICE-0128-02; SLICE-0129-01/03; SLICE-0130-01`
   - o familie cu un singur slice -> `SLICE-<FAMILIE>-<NN>`;
   - o familie cu mai multe -> `SLICE-<FAMILIE>-<primul>/<ultimul>` (cel mai mic / cel mai mare numar din familie, din stage);
   - separator `; ` (punct si virgula + un spatiu);
   - familiile in ordine crescatoare ASCII (`0000` < `0028` < `00EF` < `03EF` < `ME01`);
   - in header intra DOAR slice-urile cu munca reala in stage (cod, fisa de worklog noua, ajutor). Inchiderile de status (randuri modificate doar ca «INCHIS») NU intra in header, ci in sectiunea «Status / documentatie».
4. **Corpul** (dupa o linie goala), cate o sectiune per slice major sau per interval, in aceeasi ordine ca in header:
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
5. **Sectiunea finala `Status / documentatie`** (mereu ultima), cu: ce slice-uri s-au inchis in status (cu data din fisier), fisiere de status noi, planuri mutate / adaugate in `docs/PLANS`, alte note de documentatie. Omiti `AGENTS.md` din descriere in afara de o mentiune scurta «nota in AGENTS.md» daca e in stage.
6. **Verificari inainte de afisare** (spune-le dupa bloc, in cel mult 3 linii, doar daca apar):
   - un slice din cod fara fisa de worklog in stage sau fara rand in status -> «lipseste documentatia pentru SLICE-...»;
   - o fisa care spune «build/teste nerulate» sau «nevazut pe ecran» -> mentioneaza starea o data;
   - fisiere din stage care nu apartin niciunui slice -> le listezi.

## Ce afisezi

1. Blocul de cod ` ```text ` cu mesajul complet.
2. Sub el, doar avertismentele de la pasul 6 (daca exista). Fara introduceri, fara rezumat, fara intrebari.
