# K-BOT — STATUS, felia ME03 (KBotAdmin, comenzi ADECHIT)

Familia ME03 deschisă la 12.11.2026 (ME01 și ME02 erau luate), la cererea Adelinei. KBotAdmin este programul privat al proprietarului, fără Ajutor.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME03-01 | [«Comenzi» > ADECHIT > «Informatii copil»](../SLICE-ME03-01-comenzi-adechit-informatii-copil.md): cere un CNP și arată, pe baza aleasă, tot ce face referire la IDP-ul copilului (copil, plătitori, istoric grupe, luni, situație lunară, plăți, chitanțe, alte documente, retururi, compensări, extrase) | 12.11.2026: KBotAdmin compilat curat, Python `py_compile` curat; **nerulat pe nicio bază**, nevăzut pe ecran, netrimis pe server |

## Current focus
- Prima rulare pe o bază reală (de trimis de mână pe server: `adechit_copil.py`, `admin_cli.py`).

## Open threads
- SQL-ul nu a fost încercat (vezi worklogul, «Unverified»). Dacă ieșirea e prea lungă pe un copil cu mulți ani, se poate adăuga filtru pe an.
