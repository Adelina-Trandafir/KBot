# SLICE-D001-01 — Documentație: duplicate și index

Data: 24.10.2026. Familia documentară `SLICE-Dnnn-nn`, stabilită de utilizator.

## What changed and why

- În `implementare_vercon_v2.md`, șase copii integrale ale textelor VC06-18, VC03-07/08/09/10 și VC05-11 sunt înlocuite cu linkuri la worklogurile existente. Titlurile secțiunilor și destinațiile sunt păstrate; worklogurile conțin detaliile, limitele și stările istorice originale. Reducere: 232 de rânduri.

- În jurnalul KE01-02 este eliminată introducerea copiată accidental în fraza despre validarea codului de angajament. Fraza este refăcută cu expresia `^[A-Za-z0-9_./-]{1,60}$` din `PYTHON/routes/admin_cli/admin_cli.py`. Restul jurnalului și limitele sale istorice sunt păstrate. Reducere: 22 de rânduri.

- README-ul generic devine un index de documentație. Cerințele specifice noutăților sunt păstrate, iar formatul este delegat ghidului existent, fără redefinirea lui. Bilanțul lotului: 227 de rânduri mai puțin, incluzând cele două fișiere noi necesare urmăririi familiei D (registrul și acest jurnal).

## Files touched

`README.md`; `implementare_vercon_v2.md`; `docs/worklog/SLICE-KE01-02-fisa-angajament.md`; `docs/worklog/KBOT_STATUS.md`; registrul `state/KBOT_STATUS_D000-D009.md`; acest jurnal.

## Test results

Numai analiză de text și de proiectare: cele șase corpuri retrase au fost comparate integral cu destinațiile, cu normalizarea sfârșiturilor de rând. Nu s-au rulat teste, builduri, aplicații, verificatoare de ajutor, comenzi Git sau SQL. Nu se schimbă starea validării vreunei funcționalități.

## Unverified / deferred

- Modificările sunt locale, fără commit sau push.

- Reorganizarea completă continuă cu indexurile/statusurile și cu informația unică din jurnalul general VERCON; acest lot nu declară încheiată curățarea proiectului.

- Fișierele AVACONT, documentația activă AV/AVACONT și orice CLAUDE.md sau variantă istorică CLAUDE_*.md sunt excluse. Ajutorul runtime, scripturile și numele worklogurilor existente nu sunt modificate.

- Comasarea jurnalelor dependente de nume și schimbarea regulilor de întreținere rămân pentru loturi distincte.
