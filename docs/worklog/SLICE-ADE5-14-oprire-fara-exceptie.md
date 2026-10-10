# SLICE-ADE5-14 — oprire normală fără excepție în debugger

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; fără teste sau migrare.

## Schimbare

La cererea utilizatorului, oprirea normală nu mai folosește
ThrowIfCancellationRequested. InsertRows întoarce numărul pregătit dacă tokenul
este anulat; Write verifică imediat rezultatul prin token și face rollback
înainte de alte scrieri sau COMMIT. RollbackIfStopped confirmă rollback-ul și
scrie jurnalul, apoi Write întoarce Nothing. Înainte de tranzacție, oprirea
întoarce Nothing fără scrieri. Formularul recunoaște Nothing ca oprire,
invalidează verificarea destinației și nu marchează migrarea drept reușită.

Erorile reale de SQL/rollback/jurnal nu sunt ascunse. Nu se verifică tokenul după
COMMIT, astfel încât o oprire tardivă nu prezintă importul confirmat ca anulat.

## Fișiere și verificări

AdeWriter.vb, AdeMigratorForm.vb, README.md, ADECHIT_STATUS.md,
PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.
Verificare statică a tuturor punctelor de oprire și a singurului apelant Write.
Build în `bin/Debug/stop-without-exception`: 0 erori, 0 avertismente.
Fără teste automate, probe vizuale, migrare sau acces la server.
Utilizatorul probează Oprește în debug, confirmarea rollback-ului în jurnal și
închiderea normală; rerularea cere Testează din nou.
