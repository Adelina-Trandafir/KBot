# K-BOT — STATUS, felia ME06 (KBotAdmin, AVACONT)

Familia ME06 deschisă la 19.11.2026 (ME01-ME05 erau luate), la cererea Adelinei. KBotAdmin este programul privat al proprietarului, fără Ajutor.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME06-01 | [Comenzi > AVACONT: «Facturi si plati pe CodFiscal»](../SLICE-ME06-01-facturi-plati-pe-cod-fiscal.md): pentru un furnizor ales dintr-o listă (partenerii bazei, unul pe CodFiscal, cu căutare la tastare) arată, pe fiecare SS și an, facturile (evidențe Tip E) cu plătit și rest, plățile (Tip P, cu numărul ordinului) și celelalte operații ale partenerului; născută din «Facturi an curent» care nu arăta nicio factură | 19.11.2026: KBotAdmin compilat curat, Python `py_compile` curat; **nerulat**, nevăzut pe ecran, netrimis pe server |
| ME06-02 | [`CommandsView.vb` împărțit pe responsabilități](../SLICE-ME06-02-comandsview-impartit-pe-responsabilitati.md): cele 1100 de linii devin opt fișiere ale aceleiași clase (nume, alegeri, furnizor, VERCON, mutare plată, reparări, afișare), niciunul peste 310 linii; cod mutat, nimic schimbat; partial-urile înregistrate în `KBotAdmin.vbproj` | 19.11.2026: KBotAdmin compilat curat; **nerulat**, formularul nedeschis |

## Current focus
- ME06-01: copiază de mână `facturi_plati_cui.py` și `admin_cli.py` pe server; în KBotAdmin alege baza, comanda, furnizorul și compară cu ce arată «Facturi si plati».

## Open threads
- Dacă blocul «Alte operatii» arată facturile furnizorului cu alt Tip decât E, fereastra «Facturi si plati» (și migratorul) trebuie corectate: decizia și felia ei sunt ale Adelinei.
