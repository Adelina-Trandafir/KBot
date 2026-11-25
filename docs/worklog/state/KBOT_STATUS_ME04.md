# K-BOT — STATUS, felia ME04 (KBotAdmin, copii locale și căutare în tabele)

Familia ME04 deschisă la 16.11.2026 (ME01-ME03 erau luate), la cererea Adelinei. KBotAdmin este programul privat al proprietarului, fără Ajutor.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME04-01 | [«Copii locale» > «Caută în copii»](../SLICE-ME04-01-cautare-in-copii-locale.md): caută un text în toate copiile unei baze și arată fiecare versiune de rând cu prima/ultima copie în care apare și dacă există și în cea mai nouă; dublu clic deschide istoricul rândului | 16.11.2026: compilat curat; **nerulat**, nevăzut pe ecran |
| ME04-02 | [Baze Date > Tabele: căutare pe server](../SLICE-ME04-02-cautare-pe-server-tabele.md): condițiile filtrelor de coloană (conține, egal, între...) se caută pe server în tot tabelul, nu doar în rândurile încărcate | 16.11.2026: KBotAdmin compilat curat, Python `py_compile` curat; **nerulat**, nevăzut pe ecran, netrimis pe server |
| ME04-03 | [Comenzi > FOREXE: «Verificare extrase lipsa» + «Repară»](../SLICE-ME04-03-extrase-lipsa-reciteste-xml.md): recitește XML-ul din `FX_Extrase_F` și găsește antetele (`FX_Extrase_H`) și operațiunile (`FX_Extrase`) care lipsesc; «Repară» le scrie cu funcțiile importului, într-o tranzacție, cu Jurnal | 16.11.2026: KBotAdmin compilat curat, Python `py_compile` curat; **nerulat**, nevăzut pe ecran, netrimis pe server |

## Current focus
- ME04-03: copiază de mână `extrase_lipsa.py` și `admin_cli.py` pe server; rulează întâi verificarea pe baza cu `TZ521102968922`, apoi «Repară».
- Prima rulare: copiază de mână `admin_dbview.py` pe server; încearcă un filtru «Conține» pe un tabel mare și «Caută în copii» pe o bază cu copii.

## Open threads
- Răsfoirea unui tabel «așa cum era» la o dată dintr-o copie nu este construită (nu a fost cerută după întrebare).
- Lista de valori bifate din meniul filtrului rămâne locală.
