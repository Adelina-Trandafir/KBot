# ME02 — formatul copiilor locale (KBK1)

Scris înainte de cod (ME02-03). Fișierele de cod: `KBotAdmin/Backup/`. Serverul: `PYTHON/routes/admin_cli/admin_snapshot.py` (ME02-04).

## Scop

O copie a unei baze de unitate pe PC-ul operatorului, **criptată**, care ocupă loc doar pentru ce s-a schimbat de la copia
precedentă, și din care se poate reconstitui oricare dintre momentele copiate („time-machine”).

## Criptografia (standard, nimic inventat)

| Ce | Cum |
|---|---|
| Din parolă | PBKDF2-SHA256, 600.000 de iterații, sare de 16 octeți (în `store.kbk`) → 32 de octeți |
| Sub-chei | HKDF-SHA256 din cei 32 de octeți: `kbk1-enc` (criptare) și `kbk1-name` (denumire) |
| Criptare | AES-256-GCM, nonce de 12 octeți aleator, tag de 16; AAD = tipul + numele fișierului (un fișier mutat sub alt nume nu se mai deschide) |
| Denumirea obiectelor | HMAC-SHA256(`kbk1-name`, conținutul necomprimat), 64 hex. Același conținut = același nume = un singur fișier; din nume nu se poate confirma un conținut cunoscut |
| Compresie | Brotli (rapid) înaintea criptării; dacă nu câștigă, octeții rămân așa (un octet de semn: 0 = brut, 1 = brotli) |

Cine are dosarul dar nu parola vede: câte fișiere sunt, cât de mari, când au fost scrise. Nu vede nume de baze, de tabele sau de câmpuri.
**Fără parolă nu se recuperează nimic.** Parola se ține în managerul de parole.

## Dosarul

```
<rădăcină>/store.kbk            antet în clar: {"magic":"KBK1","kdf":"pbkdf2-sha256","iterations":N,"salt":b64,"check":b64}
<rădăcină>/o/<2 hex>/<62 hex>   obiecte:   0x01 | nonce | text criptat | tag
<rădăcină>/s/<32 hex>.snp       instantanee: 0x02 | nonce | text criptat | tag   (numele e aleator)
```

Scrierea e atomică (fișier temporar în același dosar, apoi mutare). Obiectele se scriu întâi, instantaneul **ultimul**: o oprire la mijloc lasă
doar obiecte fără stăpân (curățate la retenție), niciodată un instantaneu cu obiecte lipsă.

## Un instantaneu (manifest)

Text JSON, comprimat și criptat. O copie a unei baze:

```
{ "v":1, "id":"…", "db":"001_X", "kind":"manual|periodic", "reason":"…", "mode":"fast|full",
  "started":"…", "finished":"…", "server_time":"…", "server_version":"…", "prev":"<id | null>",
  "tables":[ { "name":"T", "ddl":"<obiect>", "cols":[{"n","dt","bin"}], "pk":["Id"], "modcol":"…|null", "rows":N,
               "pages":[ { "lo":[…]|null, "hi":[…]|null, "n":rânduri, "sum":"n:a:b", "full":"n:a:b", "obj":"<obiect>", "bytes":N } ] } ],
  "extras":[ { "kind":"view|trigger|routine|event", "name":"…", "ddl":"<obiect>" } ],
  "stats":{ "pages_new":N, "pages_same":N, "bytes_new":N, "rows":N, "seconds":N } }
```

- **Pagina** = un interval de chei primare `(hi paginii dinainte, hi]`; prima are `lo = null`, ultima `hi = null`; intervalele se ating și acoperă tot
  spațiul de chei. Obiectul paginii e lista de rânduri (JSON, fiecare valoare text sau `null`; coloanele binare în base64), în ordinea coloanelor din `cols`.
  Coloanele generate nu se salvează (le recalculează baza).
- **`sum` / `full`** = valori de control calculate de baza de date pe intervalul paginii (număr de rânduri și două XOR-uri de MD5 pe rând). `sum` e cea
  rapidă; `full` numără și conținutul coloanelor mari (PDF, imagini) prin MD5, nu doar prin lungime. În tabelele fără coloane mari sunt identice.
- Tabela fără cheie primară (sau cu cheie binară) = o singură pagină cu `lo = hi = null`.

## Cum se alege ce se citește

La fiecare rulare clientul trimite serverului paginile copiei precedente (intervalele și valorile de control; fișier temporar în `backup/` de pe server, șters după citire).
Serverul, într-o singură tranzacție cu instantaneu consistent (`START TRANSACTION WITH CONSISTENT SNAPSHOT`, ca `--single-transaction`):

1. recalculează valoarea de control pe fiecare interval; dacă e la fel, răspunde doar cu descrierea paginii (`same`), fără să citească rândurile;
2. dacă diferă (rând nou, schimbat, șters), citește intervalul din nou și îl taie în pagini noi (500 de rânduri sau 4 MB), care ajung în obiecte noi;
3. paginile nemodificate arată în noul instantaneu către **aceleași obiecte**: de aici economia de spațiu.

Modul `full` (o dată pe săptămână) compară `full` în loc de `sum`, ca să prindă un fișier înlocuit cu unul de aceeași lungime fără ca „data modificării” să se schimbe.
Data modificării (ON UPDATE sau `DataModif`) e doar un ajutor pentru a nu citi PDF-urile zilnic; nu e singura apărare.

## Ce înseamnă „istoric”

Un instantaneu e complet: din el se reface baza așa cum era în clipa citirii. Istoricul unei înregistrări = aceeași cheie în instantaneele succesive.
**Granularitatea e cea a copiilor** (de ex. zilnic): ce s-a întâmplat între două rulări nu se vede.

## Ore

`started` / `finished` poartă ora PC-ului cu offset (ISO 8601). `server_time` e ora serverului, UTC, fără offset scris (politica din `PYTHON/docs/TIMEZONES.md`). Valorile de control ale paginilor
se calculează pe sesiuni MariaDB fixate pe UTC, deci nu se schimbă la trecerea la ora de vară. Coloanele noi din ME02-01 (`DataAdaugare` etc.) se completează cu ora sesiunii care scrie:
UTC pentru aplicația de pe server; clienții care își păstrează propriul fus (Access) pot scrie altă oră. Copiile nu se bazează pe aceste ore.

## Limite cunoscute

- Un rând cu coloanele însumate mai mari decât `max_allowed_packet` nu intră în valoarea de control (pagina se citește totuși întreagă prima dată).
- Valorile `float`/`double` se compară prin textul lor; chei `float` nu sunt potrivite (nu există în schema curentă).
- Procesul de pe server se poate opri fără ca clientul să afle imediat: lipsa înregistrării `end` face copia invalidă și nu se scrie niciun instantaneu.
