---
name: slice
description: Stabileste slice-ul taskului curent din ce scrie Adelina, cu valori fixe (SLICE-ME01-50) sau cu substituenti (SLICE-MEnn-mm, SLICE-ME01-mm, SLICE-MEnn-50, SLICE-nnnn-mm) rezolvati la primul numar liber din proiect.
argument-hint: SLICE-ME01-50 | SLICE-MEnn-mm | SLICE-ME01-mm | SLICE-MEnn-50 | SLICE-nnnn-mm | SLICE-nnEF-mm
disable-model-invocation: true
---

# /slice - slice-ul taskului, fix sau cu numar liber

Comanda: `/slice <specificatie>`. Este raspunsul Adelinei la regula «orice schimbare are nevoie de un slice
de la ea inainte de cod» (`CODE_WORKFLOW.md` pct. 1, memoria «ANY change needs a slice FROM ADELINA»).
Slice-ul si familia le alege ea; tu rezolvi doar numarul liber. Fara argument: intreaba ce slice, nu ghici.

## Forme acceptate (prefixul `SLICE-` e optional; literele familiei sunt MARI)

| Scrisa de Adelina | Inseamna |
|---|---|
| `SLICE-ME01-50` | totul fix. Nu rezolvi nimic; verifici doar daca numarul e deja folosit |
| `SLICE-ME01-mm` | slice fix, `mm` = primul sub-slice liber din ME01 |
| `SLICE-MEnn-50` | `nn` = primul numar liber al familiei ME (ME03...), sub-slice-ul ramane 50 |
| `SLICE-MEnn-mm` | primul numar liber ME si sub-slice-ul `01` (un slice nou nu are sub-slice-uri) |
| `SLICE-nnEF-mm` | cifre + litere (familia 00EF, 01EF...): primul `nn` liber inaintea literelor |
| `SLICE-nnnn-mm` | primul slice de patru cifre liber (0131...) |
| `SLICE-nnnn` / `SLICE-ME01` | doar slice-ul, fara sub-slice |

Substituentii sunt `nn`, `mm`, `nnnn` (merg si cu litere mari). Un substituent se rezolva doar daca l-a scris
Adelina; fara substituent si fara slice complet nu ghici, cere slice-ul. Numarul incepe de la 1 (`00` nu se propune),
`nn`/`mm` au doua cifre, `nnnn` patru, cu zerouri in fata.

## Ce inseamna «liber»

Numarul nu apare NICAIERI in proiect: nume de fisiere si continut al tuturor fisierelor text pe care Git le cunoaste
(worklog-uri `SLICE-*.md`, `KBOT_STATUS.md`, `state/`, `src/`, `PYTHON/`, `VERCON/`, etichete de ajutor
`<!-- slice: ... -->`), fara `.claude/`, `Surse/`, `MariaDB_Schema/` si fara vreun `AGENTS.md`. Cautarea e larga
dinadins: o potrivire in plus doar sare un numar, nu refoloseste niciodata unul. Gaurile pomenite undeva (0021
absorbit de 0078, 0038/0039 din comentariile din `KBot.Controls/Tree`) sunt deci numere luate. Scriptul nu tine cont
de randurile «Next free number: ...» (anunta un numar liber, nu il folosesc).

## Pasii

1. Din radacina KBOT, rulezi o singura data (scriptul doar citeste, nu scrie nimic; dureaza cateva secunde):
   `powershell -NoProfile -File .claude\skills\slice\Resolve-Slice.ps1 -Spec '<specificatia scrisa de Adelina>'`
   Scriptul da `RESOLVED` (slice-ul complet), `SLICE`, `SUB` cu nota fiecaruia si liniile `STATUS` (ce spun fisierele
   de status ca e «Next free» pentru familie).
2. Spune-i Adelinei valoarea aleasa, inainte de orice cod: `SLICE-ME03-01` si o jumatate de rand de unde vine
   (ex. «ME01 si ME02 sunt luate»). Daca linia `STATUS` arata alt «Next free» decat `RESOLVED`, mentioneaza ambele
   intr-un rand.
3. **Opreste-te si intreaba (un singur rand)** in aceste cazuri, inainte sa scrii ceva:
   - nota contine `GAP`: primul numar liber e o gaura sub cel mai mare folosit (ex. ME01-24 cand ultimul e 37).
     Da amandoua valorile si lasa-o sa aleaga;
   - o valoare fixa data de ea apare `ALREADY IN USE`: e continuare a unui slice existent sau o greseala. Arata unde e
     folosit si intreaba daca e intentionat;
   - scriptul da eroare sau nu intelege forma.
   In rest (numar liber clar) continua direct: comanda ei este decizia.
4. Slice-ul stabilit este **slice-ul taskului**: pune-l peste tot unde se cere de `docs/worklog/CODE_WORKFLOW.md` pct. 3:
   numele `docs/worklog/SLICE-<slice>-<sub>-<slug>.md` (slug scurt, ASCII), randul din `state/KBOT_STATUS_*.md` si linia
   din indexul `KBOT_STATUS.md`, eticheta `<!-- slice: ... -->` din ajutor pentru orice schimbare vizibila operatorului,
   iar «Next free» al familiei se mareste daca un slice nou l-a folosit. Nimic din acestea nu se scrie pana nu e clar
   taskul; numarul nu este rezervat de comanda, ci de worklog-ul creat, deci nu lasa taskul fara worklog.
5. Nu scrie in `AGENTS.md`, nu face `git add/commit/push` (regula «NEVER git writes»); raporteaza fisierele schimbate.

## Exemple (starea proiectului la 12.11.2026, doar ilustrativ)

- `/slice SLICE-MEnn-mm` -> `SLICE-ME03-01` (ME01, ME02 luate).
- `/slice SLICE-ME01-mm` -> primul sub-slice neluat din ME01; daca exista o gaura, `GAP` si intrebarea de la pasul 3.
- `/slice SLICE-ME01-50` -> `SLICE-ME01-50`, cu mentiunea ca 50 nu e folosit.
- `/slice SLICE-nnnn-mm` -> primul slice de patru cifre liber, cu `-01`.
