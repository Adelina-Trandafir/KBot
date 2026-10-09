# SLICE-ADE5-06 — jurnalul erorilor și motivele blocării în formular

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; versiunea modificată nu a fost rulată.

## Ce s-a schimbat și de ce

Utilizatorul raportează că Migrează nu se activează și că nu vede niciun mesaj.
Citirea jurnalului local din bin/Debug/net8.0-windows/Logs/harness_errors.log a
identificat ultima verificare la 17:01:11: Connect Timeout expired în TestConnection.
Aceasta explică lipsa unei verificări reușite; nu dovedește cauza de rețea a timeout-ului.
Nu s-a accesat serverul pentru diagnostic.

- Motiv permanent lângă Migrează și lista tuturor motivelor în jurnalul formularului.
  Eroarea ultimei verificări este păstrată până la modificarea intrărilor sau reîncercare.
- Jurnal în Designer: mesaje, blocaje Access și server, excepții complete, gazdă/port
  la verificare. Nu se scrie parola conexiunii. Se păstrează GlobalErrorLog comun.
- Buton pentru citirea ultimilor maximum 256 KiB din logul comun, cu FileShare.ReadWrite;
  încărcare inițială la Shown, fără blocarea inițializării setărilor dacă citirea eșuează.
- Preferințele locale se salvează separat: eroarea este afișată/înregistrată, dar nu
  abandonează citirea sursei sau verificarea destinației deja începută.
- Testarea fără plan citit afișează pasul necesar. Condițiile de validare rămân active;
  butonul nu se activează când conexiunea/verificarea au eșuat sau există blocaje.
- Comentarii și identificatori noi în engleză; mesaje în română cu diacritice.

## Fișiere atinse

AdeMigratorForm.vb, AdeMigratorForm.Designer.vb și README.md din ADECHIT/ADE.Migrator;
ADECHIT_STATUS.md, ADECHIT/PLAN_IMPLEMENTARE.md,
docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

## Verificări efective

- Inspecție statică a condițiilor butonului și citirea jurnalului existent.
- Build: `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore --verbosity minimal -m:1`:
  0 avertismente, 0 erori. Unele încercări cu build paralel au eșuat fără diagnostice;
  buildul serial a reușit. Nu atribuim o cauză neverificată acelor eșecuri.
- Fără teste automate, probe vizuale, rulare a utilitarului, conexiuni MariaDB sau DDL.

## Neverificat și amânat

Utilizatorul redeschide executabilul actualizat, verifică jurnalul și reîncearcă
conectarea cu gazda/portul corecte. Accesul la server, cauza timeout-ului și activarea
butonului după o verificare reală reușită rămân de probat de utilizator.
Identificatorii românești existenți nu sunt redenumiți în această corecție.
Fără commit sau publicare în această intervenție.
