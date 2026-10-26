# K-BOT — STATUS, felia ME02 (copii locale criptate, cu istoric / «time-machine»)

Cererea operatorului, 20.10.2026: copii periodice, **pe PC-ul ei**, ale bazelor de pe server, într-un format propriu (conținutul nu se vede), cu posibilitatea unui istoric pe momente. Doar bazele de unitate (nu `AVACONT_COMUN`, `AVACONT_SURSA`, `000_DEMO`). Planul: [ME02_PLAN.md](../../ME02_PLAN.md). Numărul ME02 este o presupunere a mea (nu exista), nu o hotărâre a operatorului.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME02-01 | [Date creare/modificare — SQL generat, exceptii documentate](../SLICE-ME02-01-date-creare-modificare.md) | GATA pe cod: fișier generat, **nerulat pe nicio bază** |
| ME02-02 | [Dump manual pe bazele de unitate, cu motiv](../SLICE-ME02-02-copie-manuala-toate-bazele.md) | GATA pe cod (KBotAdmin 0 avertismente), **nerulat pe server, de trimis de mână** |
| ME02-03 | [Motorul local si formatul KBK1](../SLICE-ME02-03-format-si-motor.md#slice-me02-03) | GATA pe cod (KBotAdmin 0 avertismente), **nerulat** |
| ME02-04 | [snap-run — fluxul de citire al serverului](../SLICE-ME02-03-format-si-motor.md#slice-me02-04) | GATA pe cod (`py_compile`), **nerulat pe server, de trimis de mână** |
| ME02-05 | [Pagina Copii locale; programarea initiala retrasa](../SLICE-ME02-03-format-si-motor.md#slice-me02-05) | GATA pe cod, **nerulat, nevăzut pe ecran** |
| ME02-06 | [Verificare si export SQL pentru baza de proba](../SLICE-ME02-03-format-si-motor.md#slice-me02-06) | GATA pe cod, **nerulat** |
| ME02-07 | [Istoricul unui rand si SQL in clipboard](../SLICE-ME02-03-format-si-motor.md#slice-me02-07) | GATA pe cod, **nerulat** |
| ME02-08 | [Retentie; manualele se sterg doar de mana](../SLICE-ME02-03-format-si-motor.md#slice-me02-08) | GATA pe cod, **nerulat** |

## Hotărâri ale operatorului, 20.10.2026 (după predare)

Cheia copiilor se păstrează pe PC (DPAPI; aplicația nu pleacă de pe acest calculator). **Nu** se face copie automată de noapte: la deschiderea KBotAdmin, dacă azi nu există copie locală, programul întreabă dacă se face acum. Copiile făcute de mână se păstrează fără limită de timp și se șterg doar de mână («Șterge»). Regulile 14 zile / 8 săptămâni / 12 luni rămân (se aplică doar copiilor „periodic”, care nu mai apar). Cele de mai jos (ME02-05, -08) sunt corectate în acest sens. `PYTHON/docs/TIMEZONES.md` (orele serverului în UTC) a fost citit: verbele mele nu depind de fus (valorile de control nu folosesc ore salvate cu fus); `admin_backup.py` și `admin_snapshot.py` au fost deja trecute pe `utils.clock` de cealaltă lucrare; lista «Copii server» arată acum ora PC-ului.

## Închis (operator, 21.10.2026)

Operatorul a încercat pagina «Copii locale» pe serverul real după corecția permisiunilor SFTP (`ChangePermissions(…, 600)`): prima copie și a doua (cu lista copiei precedente trimisă serverului) merg, spune ea. Felia e dată ca închisă. Rămân neîncercate de ea, din câte știu eu: Verifică, Export SQL, Istoricul unui rând, Curăță, Șterge și mesajul de la deschidere; nimic din ele n-a fost executat de mine.

## Idee amânată (operator, 21.10.2026): copii locale pe PC-urile userilor

Ideea: o copie a bazei unității pe fiecare PC de user, cel mult una pe zi la deschiderea programului, cu păstrare 7 zile / câte una pe lună / câte una pe an, pe care operatorul o poate citi și restaura cu o aplicație proprie.
**RENUNȚAT pentru acum, rămâne pe viitor.** Ce ar cere (analizat, nepornit): o rută nouă de server care citește baza unității userului (programul K-BOT nu are acces direct la bază, doar prin API), motorul mutat din KBotAdmin
într-o parte comună (KBotAdmin nu are voie la clienți) cu doar scrierea la clienți, criptare cu cheie dublă (PC-ul doar încuie, operatorul descuie) ca userul să nu poată citi copiile, timp lung de cerere la Gunicorn și limitare, rută legată de unitate și scrisă în Jurnal, mesajul de la deschidere, ajutorul K-BOT (0000-NN) și notele de versiune. Restaurarea doar în bază de probă. Nu se face nimic din asta până nu se reia.

## Open threads

- [01 — metadatele SQL](../SLICE-ME02-01-date-creare-modificare.md): aplicarea pe sablon/schema_sync, tabelele mari si reimprospatarea MariaDB_Schema raman distincte de copiile confirmate. Publicarea admin_backup.py/admin_snapshot.py/admin_cli.py si publish-admin.ps1 erau pasii consemnati inainte de prima copie, fara restart Flask; 21.10 confirma copiile, fara inventar separat al tuturor livrarilor.
- [02 — dumpul text](../SLICE-ME02-02-copie-manuala-toate-bazele.md): lipsa --hex-blob si proba restaurarii cu PDF-uri raman deschise.
- [03…08](../SLICE-ME02-03-format-si-motor.md): verificarile pe chei/tipuri/performanta, DPAPI, operatiile offline, limitele de interfata si functiile neconstruite raman pe etapa. Primele doua copii confirmate nu probeaza Verifica sau un export SQL restaurat.
- Bara KBotAdmin cu 16 intrari nu fusese privita; marimea reala a bazelor nu este cunoscuta aici (apare in arbore). Alerta de copie veche, rasfoirea completa, comparatia intre date, anularea si incarcarea automata pe server raman neconstruite.
- Fara teste in aceasta lucrare; verificarea produsului este functia Verifica, inca neincercata la reperul confirmarii.
