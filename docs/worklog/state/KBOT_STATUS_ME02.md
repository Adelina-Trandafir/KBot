# K-BOT — STATUS, felia ME02 (copii locale criptate, cu istoric / «time-machine»)

Cererea operatorului, 20.10.2026: copii periodice, **pe PC-ul ei**, ale bazelor de pe server, într-un format propriu (conținutul nu se vede),
cu posibilitatea unui istoric pe momente. Doar bazele de unitate (nu `AVACONT_COMUN`, `AVACONT_SURSA`, `000_DEMO`). Planul: [ME02_PLAN.md](../../ME02_PLAN.md).
Numărul ME02 este o presupunere a mea (nu exista), nu o hotărâre a operatorului.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME02-01 | Data creării / modificării adăugată tabelelor care nu o au: `sql/ME02_01_date_creare_modificare.sql`, generat din schema șablon de `tools/Me02Dates/gen_dates_ddl.js` (79 de tabele din 133; 47 le au deja, 7 sărite cu rost) | GATA pe cod: fișier generat, **nerulat pe nicio bază** |
| ME02-02 | Copia manuală (KBotAdmin › Copii) pe toate bazele de unitate fără `000_DEMO`, cu motiv obligatoriu (jurnal + coloana «Motiv»); verbele de copie mutate în `admin_backup.py` | GATA pe cod (KBotAdmin 0 avertismente), **nerulat pe server, de trimis de mână** |
| ME02-03 | Formatul și motorul în KBotAdmin (AES-256-GCM, obiecte cu nume prin HMAC, pagini pe intervale de cheie, instantanee, cheie DPAPI) | GATA pe cod (KBotAdmin 0 avertismente), **nerulat** |
| ME02-04 | Verbul de server `snap-run` (doar citire, o tranzacție consistentă pe bază) | GATA pe cod (`py_compile`), **nerulat pe server, de trimis de mână** |
| ME02-05 | Pagina «Copii locale» + copia periodică (`KBotAdmin.exe --backup`, sarcină Windows programată din buton) | GATA pe cod, **nerulat, nevăzut pe ecran** |
| ME02-06 | Verifică (integritatea depozitului) + export SQL pentru o bază de probă `zz_*` (încărcarea automată pe server NU s-a construit) | GATA pe cod, **nerulat** |
| ME02-07 | Istoricul unui rând prin copii + SQL de readucere în clipboard (răsfoirea unei tabele la un moment dat NU s-a construit) | GATA pe cod, **nerulat** |
| ME02-08 | Retenție (zile / săptămâni / luni) și curățare cu previzualizare, aplicată și după copia periodică | GATA pe cod, **nerulat** |

## Hotărâri ale operatorului, 20.10.2026 (după predare)

Cheia copiilor se păstrează pe PC (DPAPI; aplicația nu pleacă de pe acest calculator). **Nu** se face copie automată de noapte: la deschiderea KBotAdmin, dacă azi nu există
copie locală, programul întreabă dacă se face acum. Copiile făcute de mână se păstrează fără limită de timp și se șterg doar de mână («Șterge»). Regulile 14 zile / 8 săptămâni /
12 luni rămân (se aplică doar copiilor „periodic”, care nu mai apar). Cele de mai jos (ME02-05, -08) sunt corectate în acest sens.
`PYTHON/docs/TIMEZONES.md` (orele serverului în UTC) a fost citit: verbele mele nu depind de fus (valorile de control nu folosesc ore salvate cu fus); `admin_backup.py` și `admin_snapshot.py`
au fost deja trecute pe `utils.clock` de cealaltă lucrare; lista «Copii server» arată acum ora PC-ului.

## Închis (operator, 21.10.2026)

Operatorul a încercat pagina «Copii locale» pe serverul real după corecția permisiunilor SFTP (`ChangePermissions(…, 600)`): prima copie și a doua (cu lista copiei precedente trimisă serverului) merg, spune ea.
Felia e dată ca închisă. Rămân neîncercate de ea, din câte știu eu: Verifică, Export SQL, Istoricul unui rând, Curăță, Șterge și mesajul de la deschidere; nimic din ele n-a fost executat de mine.

## Idee amânată (operator, 21.10.2026): copii locale pe PC-urile userilor

Ideea: o copie a bazei unității pe fiecare PC de user, cel mult una pe zi la deschiderea programului, cu păstrare 7 zile / câte una pe lună / câte una pe an, pe care operatorul o poate citi și restaura cu o aplicație proprie.
**RENUNȚAT pentru acum, rămâne pe viitor.** Ce ar cere (analizat, nepornit): o rută nouă de server care citește baza unității userului (programul K-BOT nu are acces direct la bază, doar prin API), motorul mutat din KBotAdmin
într-o parte comună (KBotAdmin nu are voie la clienți) cu doar scrierea la clienți, criptare cu cheie dublă (PC-ul doar încuie, operatorul descuie) ca userul să nu poată citi copiile, timp lung de cerere la Gunicorn și limitare,
rută legată de unitate și scrisă în Jurnal, mesajul de la deschidere, ajutorul K-BOT (0000-NN) și notele de versiune. Restaurarea doar în bază de probă. Nu se face nimic din asta până nu se reia.

## Open threads

- **De rulat de operator (nimic n-a rulat):** `sql/ME02_01_date_creare_modificare.sql` pe `AVACONT_SURSA`, apoi `schema_sync` (independent de copii); publicarea `admin_backup.py`, `admin_snapshot.py` și
  `admin_cli.py` pe server (nu cere repornirea serviciului); `publish-admin.ps1`; apoi reîmprospătarea `MariaDB_Schema/`. **Prima copie locală: pe o singură bază mică, nu pe toate.**
- **Prima rulare reală a motorului** poate scoate la iveală greșeli pe care compilarea nu le vede (SQL pe chei compuse, tipuri, performanță). Funcția «Verifică» și un export SQL încercat într-un MariaDB de probă sunt dovada.
- **ALTER-ul pe tabelele mari** (PDF, imagini, `EF_Mesaje`) poate dura și bloca scrierea în ele; de rulat într-un moment liniștit, sau de scos acele tabele din fișier.
- **Dump-urile text de pe server** nu folosesc `--hex-blob`: de verificat dacă se restaurează curat cu PDF-uri înainte de a conta pe ele.
- **Cheia pentru rulările automate:** s-a ales fișierul protejat cu DPAPI (cont Windows), plus parola din managerul de parole ca unică cale de recuperare; de confirmat de operator.
- **Lipsesc încă:** alerta „ultima copie e veche”, răsfoirea unei tabele la un moment dat, comparația între două date, anularea unei copii în curs, încărcarea automată într-o bază de probă pe server.
- Barra de pagini a KBotAdmin are acum 16 intrări (încape?) — nevăzut pe ecran.
- Mărimea reală a bazelor nu e cunoscută aici (se vede în arborele din KBotAdmin, coloana de mărime).
- Nu se rulează teste (regula proiectului); verificarea backup-ului va fi funcția proprie «Verifică» (ME02-06), nu xUnit.
