# SLICE 0063 — Formularul de login ține minte ultimul utilizator conectat

**Data:** 15.09.2026
**Cerut de operator:** «`LoginForm` nu salvează nicăieri ultimul utilizator conectat. Am
nevoie să fie salvat local, ca utilizatorul să nu-și mai scrie numele de fiecare dată.»

---

## Ce s-a făcut

**`KBot.Common\LastLoginStore.vb`** (nou). Un fișier JSON per utilizator Windows,
`%APPDATA%\AVACONT\KBot\last_login.json`, în același folder cu `settings.json`
(`SetariFoldere.DirectorSetari()`), cu exact două câmpuri: `Username` și `UnitDc`.

* **Niciodată parola, niciodată token-ul.** API-ul magazinului nu le primește deloc, deci
  fișierul nu le poate conține. Testul `TheFile_NeverContainsAPassword` păzește asta.
* `Load()` nu aruncă și nu loghează: fișier lipsă / gol / stricat / cu formă greșită =
  magazin gol, iar operatorul își scrie numele o dată în plus. Rulează înainte ca orice
  altceva să fie pornit — același raționament ca la `SetariFoldere.Incarca`.
* `Save(username, unitDc)` e frontieră de I/O: loghează și rearuncă (regula casei). Refuză un
  nume gol.

**`LoginForm`**:

* La `Load`, numele ținut minte se pune în `txtUser` (peste implicitul de Debug — e numele pe
  care operatorul l-ar tasta altfel).
* `ShowPhaseCreds` pune cursorul în **parolă** când numele e deja completat (ținut minte sau
  rămas după «Înapoi»); altfel în nume, ca înainte.
* La faza 2, dacă e ACELAȘI utilizator ca data trecută și unitatea lui mai e pe listă, combo-ul
  pornește pe ea; altfel pe prima, ca înainte.
* `Save` se cheamă **doar după ce serverul a spus da** (`LoginAsync` a întors token), într-un
  `Try` propriu: un fișier de comoditate care nu se poate scrie nu are voie să strice un login
  care tocmai a reușit. `Save` a logat deja motivul.

Unitatea ținută minte e o extensie mică a cererii («numele»), făcută fiindcă e același gest
și același fișier: a doua întrebare a formularului e chiar «care unitate».

## Fișiere atinse

**Nou**
- `src/KBot.Common/LastLoginStore.vb`
- `tests/KBot.Common.Tests/LastLoginStoreTests.vb` (9 teste, pe folder temporar)
- `docs/worklog/SLICE-0063-login-ultimul-utilizator.md` (acest fișier)

**Modificat**
- `src/KBot.App/LoginForm.vb` — `_lastLogin`, prefill la `Load`, focusul din
  `ShowPhaseCreds`, preselecția unității, `Save` după login
- `src/KBot.Common/KBot.Common.vbproj` — FileVersion 1.5.1.0 ▸ 1.5.2.0
- (`KBot.App.vbproj` e bumpat o singură dată, în felia 0062, pentru amândouă)

## Rezultate

- `dotnet test tests/KBot.Common.Tests` — **94 trecute, 0 eșuate** (9 noi).
- `dotnet build KBot.sln --no-incremental` — 0 erori.

## Ce NU s-a făcut, și trebuie spus

- **Formularul n-a fost deschis pe ecran**: că focusul ajunge în parolă și că unitatea se
  preselectează e verificat prin citirea codului, nu prin rulare. `LoginForm` nu are teste
  headless (are nevoie de un `IAuthApi` fals și de STA); nu s-au scris în felia asta.
- **Implicitele de Debug rămân în `LoginForm_Load`** (`#If DEBUG`: un e-mail și o parolă
  scrise în cod). Nu erau în cerere și nu s-au atins, dar merită scoase: o parolă în sursă e o
  parolă în depozit.
- **Nimic nu s-a comis** (arbore de lucru cu WIP străin).
