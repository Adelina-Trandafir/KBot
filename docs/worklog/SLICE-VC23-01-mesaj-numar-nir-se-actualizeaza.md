# SLICE-VC23-01 — Mesajul despre numărul NIR/FCS se actualizează la schimbarea datei sau a numărului

19.11.2026 — Cerere (VERCON online, macheta NIR): validarea numărului era corectă, dar mesajul roșu rămânea pe ecran după ce se schimba data (NIR sau factură). Exemplu: există NIR 10 pe 11.11.2026 și NIR 12 pe 13.11.2026; la o factură nouă data inițială e cea de azi (19.11.2026); se tastează 11 → «Numărul trebuie să fie mai mare decât 185»; dacă apoi data se schimbă pe 12.11.2026, 11 este valid, dar mesajul rămânea.

## Cauza

Verificarea se refăcea deja la schimbarea datei (`docs.js`, ramura `Data` / `DataFc` → `checkNumber(true)`), iar serverul (`receipt_number.check`) o calcula corect pe data nouă. Refuzul ajungea în bara de stare a machetei (`report` → `say(..., true)`). După o verificare reușită, `documentNumbers.check` ștergea doar mesajul de pe câmp, nu și pe cel din bara de stare, deci mesajul vechi rămânea. La fel, un număr corectat și părăsit (blur) nu șterge mesajul anterior.

## Ce s-a schimbat

- `PYTHON/static/js/vercon/document-number.js`: `documentNumbers` reține mesajul ultimului refuz al verificării (`refused`); la prima verificare reușită după el cheamă `clearStatus(mesaj)` și îl uită. `invalidate()` îl uită și el (ciorna s-a schimbat).
- `PYTHON/static/js/vercon/docs.js`: `clearStatus` primit de `documentNumbers`: golește bara de stare doar dacă e încă eroare (`role="alert"`) și textul începe cu mesajul reținut, deci nu atinge alte erori (rând de produs etc.).
- Serverul neschimbat. Comportamentul când data nouă nu se potrivește cu numărul tastat rămâne cel de acum: data revine la cea veche și apare mesajul.

## Ajutor

Nicio pagină de ajutor nu descrie acest mesaj; nimic de actualizat.

## Teste

Nerulate (regula «fără teste»). Verificat doar sintaxa JS (`node --check` pe ambele fișiere).

## Neverificat

- Nevăzut pe ecran, nepublicat. Scenariul din cerere (11 pe azi → eroare → data 12.11.2026 → mesajul dispare) trebuie încercat de Adelina.
- La FCS (storno) și la schimbarea numărului prin blur se aplică aceeași cale; nevăzute.
