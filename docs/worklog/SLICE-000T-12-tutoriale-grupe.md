# SLICE-000T-12 — Tutoriale pentru grupele de angajamente

Cererea operatorului, 09.10.2026: ajutor pentru sistemul de grupe (0008-02), un tutorial pentru deschiderea ferestrei de editare,
trei tutoriale în fereastra de editare (grupă nouă, scos dintr-o grupă, adăugat într-o grupă existentă), capturi unde e cazul,
și notele de versiune în `NOUTATI.md` la **1.1.2.2**.

## What changed and why

### Tutoriale (`src/KBot.App/HelpContent/tutorials/`, etichetă `000T-12`)

| Fișier | Pornește din | Ce învață |
|--------|--------------|-----------|
| `grupe-deschide-fereastra.md` (`host-key: grupe-meniu`) | fereastra principală | rotița listei → «Grupe» (lista grupelor se deschide lângă) → «Editează grupe...» → fereastra grupelor; la final trimite spre celelalte trei prin `<link tutorial>` |
| `grupe-grupa-noua.md` (`grupe-grupa-noua`) | `GrupeForm` | «+ Adăugare grupă», denumire, culoare, bifat angajamente (indicatori, alias), «Salvează» (`guard: yes`: butonul nu rulează în tutorial) |
| `grupe-scoate-angajament.md` (`grupe-scoate`) | `GrupeForm` | alege o grupă, debifează (așteaptă semnalul `angajament-scos`), «Salvează» (guard) |
| `grupe-adauga-angajament.md` (`grupe-adauga`) | `GrupeForm` | «Angajamente negrupate», trage un rând peste o grupă (așteaptă semnalul `angajament-adaugat`) |

### Cod

- `Views/GrupeForm.Tutorial.vb` (nou): `GrupeForm` implementează `IKBotTutorialHost` (cele patru chei de mai sus), cu semnalele
  `angajament-scos` (debifat într-o grupă existentă) și `angajament-adaugat` (după un drag and drop salvat). `GrupeForm` a devenit `Partial`.
- `KbotForm.Tutorial.vb`: prefix nou de ancoră `grupe.<cheie>` = un rând al submeniului de grupe (`menuGrupe.RowScreenBounds`), pentru «Editează grupe...».
  Ancora `popup.grupe` (rândul «Grupe» din meniul rotiței) și semnalul `tree-menu:grupe` existau deja prin mecanismul 000T-10.

### Ajutor (`contabil/fereastra.md`, secțiunea «Grupele de angajamente»)

Etichete `0008-02, 000T-12`; textul spune că «Grupe» are săgeată și lasă meniul deschis, că în grupă lista are toți anii și toate sursele (sectorul și
sortarea dispar), și trimite la cele patru tutoriale (din meniul «?»). **Șase capturi noi, toate lipsă** (le face operatorul din «Capturi pentru ajutor»;
nu au `goto:`, fiindcă fereastra grupelor și submeniul nu sunt ținte ale uneltei, deci fiecare are `prepare:`):
`grupe-meniu`, `grupe-arbore-filtrat`, `grupe-fereastra`, `grupe-negrupate`, `grupe-grupa-noua`, `grupe-indicatori`.

### Note de versiune

`docs/release-notes/NOUTATI.md`: secțiune nouă **1.1.2.2 (09.10.2026)**, 8 rânduri, fără diacritice, felii `0008-02, 000T-12, 0000-61`.
Marcajul `release: utc=` e cel dat de `ReleaseNotes.ps1 -Action Request -Version 1.1.2.2` (13:08:14Z). Rândurile despre alte lucrări ale zilei
(E-Factura 00EF-16 etc.) NU sunt în secțiune: n-am citit acele worklog-uri pentru această versiune; de verificat la publicare.

## Files touched

`src/KBot.App/HelpContent/tutorials/grupe-*.md` (4 noi) · `src/KBot.App/HelpContent/contabil/fereastra.md` · `src/KBot.App/Views/GrupeForm.Tutorial.vb` (nou) ·
`src/KBot.App/Views/GrupeForm.vb` (semnale, `Partial`) · `src/KBot.App/KbotForm.Tutorial.vb` · `docs/release-notes/NOUTATI.md`.

## Test results

- `Check-Help.ps1 -Coverage`: fără erori noi (rămâne vechea eroare din `tutorials\ordonantare-din-plata.md`).
- `dotnet build` KBot.App (într-un folder de ieșire separat, aplicația fiind deschisă în Visual Studio): 0 erori, 0 avertismente.
- Tutorialele **nu au fost rulate**.

## Left unverified or deferred

- Nimic din tutoriale n-a rulat. De văzut: (1) pasul `anchor: popup.grupe` + `wait: anchor:grupe.grupe-editeaza` (submeniul e o fereastră ne-activantă; dacă
  inelul nu apare pe rândul din submeniu, ancora `grupe.` trebuie reverificată); (2) pasul «Culoarea grupei» cu `allow: GrupeForm.btnCuloare`: dialogul nativ de culori
  peste vălul tutorialului; (3) tragerea din tabel peste arbore sub văl (`allow: GrupeForm.tree`); (4) `wait: select` pe rândul deja ales («Angajamente negrupate»
  e selectat la deschidere: pasul poate cere o alegere a altui rând și apoi înapoi).
- Dacă un unit nu are angajamente negrupate, tutorialul «Adaugă un angajament...» nu poate fi parcurs (textul spune asta).
- Capturile de mai sus lipsesc; `help-version.txt` rămâne `2026-10-09`.
- Starea ajutorului se înregistrează aici (0000-61 acoperă textul; tutorialele sunt 000T-12).
