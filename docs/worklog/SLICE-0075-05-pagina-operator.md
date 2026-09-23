# SLICE-0075-05 — pagina operatorului: aprobarea cererilor

Data: 23.09.2026. Plan: `docs/PLAN_AutoProvisioning.md` §7, D10, D25.
Cerută de operator după prima aprobare reușită din linia de comandă («let's do the next part
of this slice»), cu o cerință nouă venită în timpul lucrului: **«the page should allow ME to
edit the name of the database, before confirming it!»**

## Decizii (operator, 23.09.2026)

- **Autentificare: parola K-BOT + cod pe e-mail**, iar adresa trebuie să fie în `OPERATORI`
  din `config.py`. Închide întrebarea lăsată deschisă de plan la §7 («Decide in 0075-05»).
- **Numele bazei se poate schimba pe pagină înainte de aprobare**, pe lângă `CodProgram`
  (D25). Ținut la aceleași reguli ca numele calculat — vezi mai jos de ce nu e o regulă de stil.

## Ce s-a construit

| Fișier | Ce |
|---|---|
| `PYTHON/routes/inregistrare/operator.py` | **nou** — blueprint `operator_bp`: 11 rute API + `GET /operator` |
| `PYTHON/static/operator.html` | **nou** — pagina: autentificare (2 panouri), listă, detaliu, job |
| `PYTHON/static/js/operator/operator.js`, `api.js` | **noi** — logica paginii și clientul (`X-Operator-Token`) |
| `PYTHON/static/css/operator.css` | **nou** — peste `inregistrare.css` |
| `PYTHON/routes/inregistrare/provizionare.py` | `plan()`/`approve()` primesc `db_name`; `_choose_name()`; `reject()` nou |
| `PYTHON/scripts/aproba_cerere.py` | `--nume-baza`; întrebarea DA numește baza |
| `PYTHON/routes/auth/mailer.py` | `send_operator_code`, `send_registration_rejected`, `operator_page_link`; anunțul către operator poartă linkul paginii |
| `PYTHON/routes/auth/auth.py` | nume publice `verify_operator` / `log_action` (aceleași funcții, nu copii) |
| `PYTHON/main.py` | `operator_bp` înregistrat |
| `PYTHON/config.py` (local, în `.gitignore`) | `OPERATORI = []` |
| `.claude/launch.json` | `operator-stub` (port 8767, ciot din scratchpad-ul sesiunii) |

**Nimic nou în SQL.** Contul de provizionare are deja `SELECT, UPDATE` pe `FX_Inregistrari`
(lista, detaliul, respingerea), iar nomenclatoarele se citesc cu contul de serviciu, ca pe
pagina publică.

## Cum merge

**Autentificarea.** `/login` verifică parola printr-o conectare MariaDB CA operatorul (aceeași
funcție ca `LoginForm`), apoi `OPERATORI`, apoi trimite codul; răspunde cu un token `pending`
(o notă de 10 minute). `/verifica` schimbă `pending` + cod pe tokenul de sesiune. Parola bună
a unui cont care NU e operator primește `NU_ESTE_OPERATOR` și se numără la limitator ca un eșec.
Sesiunea e o notă în `STORE` sub numele `operator_session`, în antetul `X-Operator-Token` —
**nu** sesiunea K-BOT (aceea e legată de o bază, iar rutele de date o cred pe cuvânt).
Pagina ține tokenul în `sessionStorage`: o filă închisă = sesiune închisă.

**Detaliul** cheamă `provizionare.plan()` pentru cererile `InAsteptare`/`Esuata` și arată
numele propus, `IdUnitate ≈`, `CodProgram` (editabile), numărul de rânduri — sau motivul
pentru care nu se poate aproba. Denumirea ANAF apare lângă cea scrisă de solicitant numai când
diferă. Codurile F și E sunt în liste pliabile, cu denumiri.

**Numele bazei.** Câmp pe pagină, precompletat cu propunerea, buton «Propunerea» care o pune
la loc. «Verifică din nou» rulează `plan()` cu valorile de pe ecran. La «Aprobă» pagina
trimite **numele de pe ecran** — jobul construiește exact ce a confirmat operatorul, nu un
nume recalculat între timp. `_choose_name()` cere: forma `^1[1-9]{2}_[A-Z]{4}$`, un număr pe
care nu-l folosește altă bază (`SCHEMATA` + `CAI.DbName`, aceeași regulă ca la calcul), baza să
nu existe. **Forma nu e o preferință:** contul de provizionare poate crea numai baze care
se potrivesc tiparului `1__\_____`, iar `proc_Provizionare_Grant` refuză orice alt nume.
Un nume greșit e refuzat **înainte** de pasul 2 și **nu** trece cererea în `Esuata`.

**Aprobarea** pornește un fir (`threading.Thread`, daemon) și răspunde `202 {job}`; pagina
citește `/joburi/<job>?de_la=N` la 0,8 s și afișează rândurile (erorile roșu, anulările
galben). Un al doilea «Aprobă» pe aceeași cerere ▸ `409 JOB_IN_CURS`; două cereri diferite
nu se pot încurca oricum (`GET_LOCK('kbot_provizionare')`). Joburile încheiate stau o oră.

**Respingerea** cere motiv (5–2000 de caractere), ia același lacăt ca aprobarea (nu poate
cădea în mijlocul unei aprobări din linia de comandă), scrie `Respinsa` + `Motiv`, apoi
trimite motivul solicitantului. E-mailul picat nu desface respingerea — pagina spune
«NU a plecat. Anunțați solicitantul altfel».

**Linkul nou** (cereri `Aprobata`): starea linkului e pe pagină (valabil până la…, expirat,
parola aleasă); dacă e-mailul nu pleacă, linkul apare pe pagină, de dat de mână.

Totul ajunge în `Jurnal`: `OPERATOR_LOGIN`, `OPERATOR_AUTH_FAIL`, `OPERATOR_DENIED`,
`CERERE_APROBARE_START`, `CERERE_APROBATA` / `CERERE_APROBARE_ESUATA`, `CERERE_RESPINSA`,
`CERERE_LINK_NOU`.

## Verificat

- Importurile (blueprint cu 12 reguli), Regula 0 prin `tokenize` pe cele cinci fișiere
  Python și pe comentariile JS/CSS: **zero** diacritice în afara textelor pentru ecran.
  (`import main` pică local numai pe `pandas`, lipsă din venv — fără legătură.)
- **În browser, pe un ciot** care montează `operator_bp` și `operator.html` **adevărate**,
  cu baza, e-mailul, `plan/approve/reject/link_nou` mimate:
  parolă greșită / bună, cod greșit / bun, filtrele și numerele lor, detaliul (tabelul SS cu
  `IdUnitate ≈` și `CodProgram`, codurile F/E), numele cu formă greșită (oprit pe pagină),
  numărul ocupat (refuzul serverului, «Aprobă» dezactivat, reactivat la o editare), numele
  propriu `115_TRND` acceptat, **corpul ajuns la job: `115_TRND` și `02E=ABC123`, exact ce s-a
  tastat**, jobul urmărit până la capăt, cererea redeschisă ca `Aprobata`; cererea eșuată
  cu motivul și refuzul planului; textul ANAF cu `<b>` afișat ca text; respingerea (motiv prea
  scurt, apoi reușită); linkul nou cu e-mail picat (linkul pe pagină); ieșirea; un token mort
  la reîncărcare ▸ înapoi la autentificare cu «Sesiunea a expirat».
- Telefonul la 375 px: nimic nu iese din pagină; tabelele se derulează în cutia lor.

## NU e verificat

- **Nimic viu**: nicio rută n-a atins un Flask pe VPS, un MariaDB sau SMTP-ul adevărat.
- Firul de aprobare sub gunicorn `gthread` (în ciot a rulat sub serverul Flask).
- ⚠ **O repornire a gunicorn în timpul unei aprobări** omoară firul fără anulare. Același
  risc îl are linia de comandă la Ctrl+C; nu e nou, dar pagina îl face mai ușor de atins.
- `RedisSessionStore` cu notele operatorului (numai backend-ul `memory` e folosit azi).

## Pe server (23.09.2026)

Pusă pe VPS de operator: **pagina merge**. Tot atunci, verificarea 7 din §9 a trecut pentru
cererea 1 și 0075-06 a fost făcută.

## De făcut de operator pe VPS (făcut)

1. Fișierele de mai sus (în afară de `config.py` local și `launch.json`) ▸ `/root/AVACONT`.
2. În `config.py`: `OPERATORI = ["adresa-ta@..."]` — un cont K-BOT existent, litere mici.
3. Repornirea gunicorn, apoi `https://kbot.avatarsoft.ro/operator`.
