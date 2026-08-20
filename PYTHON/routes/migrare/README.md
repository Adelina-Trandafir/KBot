# migrare — Access împins pe server, citit pe server (felia 0044)

Înlocuiește lanțul feliei 0042 (VBA scrie JSON → `KBot.Migrator` citește
fișierele → rute de seed). Aici operatorul **împinge chiar fișierul Access**,
iar serverul face restul: îl citește, rutează rândurile către baze, le
măsoară față de schema MariaDB și abia apoi scrie.

---

## Pasul pe care îl face operatorul, o dată, în Access

**Fișierele trebuie să ajungă pe server FĂRĂ parolă de bază de date.**

`FX_2026.accdb` și `cale.accdb` sunt criptate cu parola `andreI`
(`Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas`). Verificat pe fișierele reale: în
`DB_STRUCT.accdb`, care nu are parolă, numele de tabele se citesc direct din
octeți; în cele două de mai sus, nu se citește niciunul. `mdbtools` **nu
decriptează** — nicio unealtă din lanțul de pe Linux nu o face.

Deci, pe o **copie** (niciodată pe fișierul de lucru):

1. Access ▸ Fișier ▸ Deschidere, cu **Deschidere în mod exclusiv**;
2. Fișier ▸ Informații ▸ **Decriptare bază de date**, se dă parola;
3. copia rezultată este cea care se împinge.

Migratorul refuză și el, cu mesaj: dacă `mdb-tables` nu vede niciun tabel,
răspunsul spune exact că fișierul e cel mai probabil încă protejat.

---

## Instalarea pe serverul Linux

### 1. mdbtools

```
sudo apt update
sudo apt install -y mdbtools
```

Trebuie să existe **trei** binare: `mdb-tables`, `mdb-schema` și `mdb-json`.
`mdb-json` a apărut în mdbtools 0.9; pe o distribuție cu un pachet mai vechi
nu e de ajuns `mdb-export` — acela scoate CSV, unde NULL și șirul gol arată la
fel, iar «NULL într-o coloană NOT NULL» este chiar una dintre constatările pe
care le vede operatorul.

Verificare:

```
mdb-tables --help && mdb-schema --help && mdb-json --help
```

Dacă binarele nu sunt în `PATH` (compilare din surse, `/opt/...`), se pune
folderul lor în `config.MDB_TOOLS_BIN`.

### 2. Folderele

```
sudo mkdir -p /var/lib/kbot/pushed-accdb /var/lib/kbot/tmp-upload
sudo chown -R <utilizatorul-gunicorn>:<grupul-lui> /var/lib/kbot
sudo chmod 750 /var/lib/kbot/pushed-accdb /var/lib/kbot/tmp-upload
```

`pushed-accdb` ține fișierele Access complete ale unităților. Nu e public și
nu trebuie servit de nginx.

### 3. `config.py`

```python
PUSHED_ACCDB_DIR = "/var/lib/kbot/pushed-accdb"
TEMP_UPLOAD_DIR  = "/var/lib/kbot/tmp-upload"   # există deja pentru routes/ftp.py
MDB_TOOLS_BIN    = None                          # sau folderul binarelor mdbtools
```

Fără `PUSHED_ACCDB_DIR` rutele răspund cu numele cheii lipsă, nu scriu
undeva la întâmplare.

### 4. nginx

Bucata de încărcare e de 4 MB (`storage.MAX_CHUNK_BYTES`), sub plafonul
global de 17 MB al aplicației (`main.py`, `MAX_CONTENT_LENGTH`). `nginx`
trebuie să lase să treacă cel puțin atât:

```
client_max_body_size 20m;
```

### 5. Repornire

```
sudo systemctl restart kbot-api      # sau cum se numește unitatea de gunicorn
```

**Un singur worker Gunicorn** rămâne obligatoriu (decizie blocată, cu gardă
în `gunicorn.conf.py`): și sesiunile de încărcare, și registrul de lucrări
stau în memoria procesului.

### Nimic de instalat cu pip

Pachetele Python folosite sunt cele care există deja
(`flask`, `mysql-connector-python`). **`pyaccdb` nu există pe PyPI** —
verificat pe index; planul care îl pomenea era scris fără acces la structura
reală.

---

## Rutele

| Rută | Ce face |
|---|---|
| `GET /api/migrare/baze` | bazele de unitate de pe MariaDB + câte tabele `FX_` au deja |
| `GET /api/migrare/fisiere` | ce fișiere Access sunt deja împinse |
| `POST /api/migrare/push/init` | deschide o încărcare în bucăți |
| `POST /api/migrare/push/bucata` | o bucată, cu amprentă SHA-256 |
| `POST /api/migrare/push/final` | lipește, verifică amprenta întregului fișier, mută în loc |
| `POST /api/migrare/analiza` | pornește analiza; întoarce un id de lucrare |
| `POST /api/migrare/rulare` | pornește scrierea; cere id-ul analizei care a aprobat-o |
| `GET /api/migrare/stare/<id>` | starea unei lucrări + jurnalul ei |

Garda e `X-Api-Key`, ca pe rutele de seed pe care le înlocuiesc.

Numele fișierelor pe server: `fx_<an>_<dc>.accdb` (litere mici) și
`cale.accdb`, unul singur pentru toate unitățile — el poartă tabelul `[Cai]`,
adică legătura `IdUnitate → bază de date`, fără de care nu se poate ruta
niciun rând.

---

## Cele două butoane

Constatările analizei au două clase, și clasa e cea care aprinde butoanele:

**BLOCANT** — tabel lipsă, coloană lipsă, tip greșit, depășire de lungime sau
de interval, NULL într-o coloană `NOT NULL`. Cât timp există una, **niciun**
buton nu pornește; nici «Forțează rularea» nu trece peste ele, fiindcă acelea
strică date, nu doar legături.

**FORȚABIL** — cheie străină fără corespondent, id `IDDF`/`IDREV` absent,
cheie primară dublă în fișier, rând care nu se rutează în nicio bază.
«Rulează» rămâne oprit, «Forțează rularea» pornește și **sare** peste
rândurile vinovate — rămân în raport, nu ajung în baza de date.

Serverul verifică regula din nou, la `POST /api/migrare/rulare` și încă o dată
în `execute.run()`. Interfața nu e singura pază.

---

## Ce nu face

* **Nu creează și nu modifică niciun tabel.** Un tabel absent pe țintă
  oprește scrierea cu numele lui; schema se instalează separat (`schema_sync`).
* **Nu suprascrie rânduri.** `ON DUPLICATE KEY UPDATE <prima coloană> =
  <prima coloană>` — auto-atribuire fără efect, aleasă deliberat în locul lui
  `INSERT IGNORE`, care ar degrada la avertisment și erorile de tip.
* **Nu traduce id-uri.** `IDDF`/`IDREV` sunt `AUTO_INCREMENT` pe MariaDB și
  nu păstrează id-ul Access alături; se verifică, iar lipsa e o constatare.
