# K-BOT — STATUS (single source of truth)

This file is the one place that says what is done, what is in progress, and what is next.
Three people rely on it: the developer, Claude (chat), and Code (Claude Code / Codex).
Keep it current. If reality and this file disagree, fix this file in the same commit.

**ADECHIT:** its separate registry is [ADECHIT_STATUS](../../ADECHIT_STATUS.md);
the current PC/mobile, DataGrid and catalog rules are in
[UTILIZARE_WEB](../../ADECHIT/UTILIZARE_WEB.md). ADE numbering is separate from K-BOT.

**Rule:** every task also produces a worklog in `docs/worklog/` (see
`docs/worklog/CODE_WORKFLOW.md`). This STATUS file is the summary; the worklog is the detail.

**How this file is split (since 28.09.2026).** This file holds only what is needed every
time: the slice index, the next free number, the cross-cutting notes and the locked
decisions. Everything recorded about a slice (its full registry row, its «Current focus»
notes, its «Open threads» notes) lives in `state/KBOT_STATUS_<tens>.md`, ten slices per file
(`0080-0089` holds 0080…0089). Work the operator asks for OUTSIDE the slice system lives in
`state/KBOT_STATUS_SLICELESS.md`.

**Reading rule:** read this file first, then open ONLY the `state/` files the task needs,
through the links in the index below. Never read all of `state/` "to be safe".

---

## Slice registry

Numbers are permanent once assigned. A slice done in several passes keeps one number and
gets sub-numbered worklogs (`SLICE-0007-01-…`, `SLICE-0007-02-…`). The **next free slice
number** is recorded at the bottom of this section — bump it when you assign a new one.

> The numbers below are a starting proposal. Confirm or renumber them once, then treat
> them as fixed. Felia 1 (auth) is the only one anchored by history; the rest are assigned
> here for the first time.

_Full rows, focus notes and open threads per slice live in `state/` (see the index below)._

### Slice index (generated 28.09.2026 — name + short status; the full text is in the linked file)

|         Slice | Name                                                                                                                     | Status (short)                                | File                                        |
| ------------: | ------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------- | ------------------------------------------- |
|          0000 | **AJUTOR (felie permanentă): ajutor interactiv + manual — toată munca la ajutor se trece aici, ca 0000-NN** | 0000-01…47 + 0000-49, 0000-50, 0000-51, 0000-52, 0000-53, 0000-54, 0000-55, 0000-56, 0000-57, 0000-58, 0000-59, 0000-60, 0000-61, 0000-62, 0000-63, 0000-64 GATA | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          000T | **TUTORIALE interactive (felia operatorului, 04.10.2026): pași care așteaptă ce face utilizatorul, întunecare, opțional/«Sari peste», ieșire la abatere; designer vizual + recorder** | 000T-01 (motorul), -02 (fluxul, gazdele, căutarea), -03 (designer + selector), -04 (recorder), -05 (ajutorul, 0000-50; plus corecția «caseta de ieșire deasupra cardului», `ShowOnTop`), -06 (textul bulei ca HTML: `KBotHtmlLabel`), -07 (tutorialul de început, cu comutator în Setări; ajutorul 0000-51), -12 (tutoriale pentru grupele de angajamente) GATA pe cod / text, **nevăzute pe ecran** | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          00EF | **E-FACTURA (proiect nou `KBot.EFactura`, folosit direct de KBot.App; felia operatorului, 06.10.2026)**: facturi emise (ecranul `EFACTURA_ADD` din Access) prima, apoi cele primite fără contabilitate; tokenul ANAF în MariaDB (criptat, pe server), obținut prin pasul cu certificatul din K-BOT; secretele rămân în Python | 00EF-01 (analiza + planul), 00EF-02 (modelul de date: SQL scris, nerulat), 00EF-03 (Migrator: filă «E-Factura», import din Access; nerulat), 00EF-04 (serverul tokenului: `routes/efactura/`, `start`/`cod`/`stare`, criptat; pe cod, nedeployat, netestat), 00EF-05 (proiectul `KBot.EFactura`, pasul cu certificatul, intrarea «E-Factura» din MENIU; pe cod, nerulat, fără certificat real; ajutorul în 0000-54), 00EF-06 (serverul facturilor emise: CRUD, XML UBL, validare; `routes/efactura/`; pe cod, nedeployat, netestat, SQL nerulat), 00EF-07 (trimiterea la ANAF: încărcare, stare, descărcare, mesaje; adrese de producție; pe cod, nedeployat, nimic apelat la ANAF) GATA; 00EF-08 (ecranul «E-Factura — facturi emise»: lista + cinci file, clienți, vânzător, linii; intrarea din MENIU îl deschide; pe cod, nerulat, nevăzut pe ecran; ajutorul în 0000-55) GATA; 00EF-09 (arborele de facturi cu meniu, bara de vederi, trimiterea și stornarea în UI, regula datei, vederile PDF; pe cod, nerulat, nevăzut pe ecran, serverul nedeployat; ajutorul în 0000-57) GATA; 00EF-13 («Date Unitate»: emitentul și conturile lui trec în `AVACONT_COMUN.Unitati_Detalii` / `Unitati_Conturi`, formular propriu, «Preia de la ANAF» o dată, pictograma E; pe cod, nicio compilare, nimic rulat; ajutorul în 0000-58) GATA; 00EF-16 (cont emitent din lista unității, «Corectează factura» 384, anul din K-BOT + filtru pe luni, PDF atașat în XML; pe cod, build curat, serverul nedeployat, nimic văzut; ajutorul în 0000-60) GATA; 00EF-12 (conturile unității emitente: tabel `EF_FurnizorConturi`, rute, fereastra `ConturiForm`, banca dedusă din IBAN; pe cod, nerulat, nevăzut; ajutorul în 0000-56) GATA; 00EF-17 (facturi primite, serverul: cititor UBL verificat pe 3 XML-uri reale, sincronizare, lista, fisiere, legatura manuala cu DDF; pe cod, nedeployat, DDL nerulat, nimic apelat la ANAF) GATA; 00EF-18 (clientul: controlul `PrimiteView`, vederea «E-Factura» din fundamentare, sincronizarea din meniul ferestrei; pe cod, build curat, nimic rulat; ajutorul 0000-61) GATA; 00EF-19 (fereastra «Facturi primite»: toate primitele anului, cautare, sincronizare; pe cod, build curat, nimic rulat; ajutorul 0000-62) GATA; 00EF-20 (selectorul de DDF pentru legatura manuala din fereastra «Facturi primite»; pe cod, build curat, nimic rulat; ajutorul 0000-63) GATA; 00EF-21 (fila «E-Factura» din shell doar la angajamentele cu facturi primite; pe cod, build curat, nimic rulat; ajutorul 0000-64) GATA; restul nepornit | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0001 | Auth                                                                                                                     | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0002 | Split-brain 401 fix + reason codes                                                                                       | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0003 | Redis session backend                                                                                                    | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0004 | Tier 1 hardening                                                                                                         | IN PROGRESS                                   | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0005 | Phase A cleanup (lying tests, https guard)                                                                               | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0006 | MainForm scaffolding                                                                                                     | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0007 | AngajamentTreeInfo POCO correction                                                                                       | SUPERSEDED by 0008                            | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0008 | Tree data API + `MainForm.LoadTree`                                                                                      | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|       0008-02 | Alias + grupe de angajamente: fereastra modală «Grupe de angajamente», meniul «Grupe» al arborelui, filtrul arborelui (cererea operatorului, 09.10.2026; ajutorul în 0000-61) | GATA pe cod (build curat); **SQL nerulat, server nedeployat, nimic văzut pe ecran** | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0009 | `MainForm.LoadTree` (client half) + tree orphan escape                                                                   | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0010 | `KBotDataView`                                                                                                           | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0011 | Sumar                                                                                                                    | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0012 | Migrare Access → MariaDB                                                                                                 | 0012-01 DONE                                  | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0013 | `KBotDataView`                                                                                                           | DONE / VISUALLY ACCEPTED in harness playgrou… | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0014 | Rezervări                                                                                                                | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0015 | Recepții                                                                                                                 | 0015-01/02/03 DONE                            | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0016 | `KBotDataView`                                                                                                           | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0017 | Plăți                                                                                                                    | 0017-01..04 DONE                              | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0018 | `KBotNavList`                                                                                                            | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|          0019 | XFA_WRITTER → `KBot.Xfa` (librărie în proces, nu exe separat)                                                            | DONE                                          | [0010-0019](state/KBOT_STATUS_0010-0019.md) |
|       0020-01 | DDF                                                                                                                      | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0020-02 | DDF                                                                                                                      | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0020-03 | DDF                                                                                                                      | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0020-04 | DDF                                                                                                                      | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0020-05 | DDF                                                                                                                      | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0022 | Istoric                                                                                                                  | 0022-01/02 DONE                               | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0023 | Harness Adobe RHP                                                                                                        | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0024 | DDF, fila «Document»                                                                                                     | 0024-01/02/03 DONE                            | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0025 | Suprafețe editabile din designer (`KBotNavList`, `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`) + `MainForm.navViews`… | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0026 | Consolidarea controalelor în `KBot.Controls`, grupate pe familii                                                         | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0027 | `AdvancedTreeControl`                                                                                                    | DONE — VERIFICAT PE ECRAN 08.08.2026          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0027-02 | `AdvancedTreeControl`                                                                                                    | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0027-03 | Valorile de designer ale arborelui în cele patru vederi (Rezervări / Recepții / Plăți / DDF)                             | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0028 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-02 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-03 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-04 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-05 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-06 | Meniul de filtrare, AUTORAT ÎN DESIGNER (nu desenat în cod)                                                              | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-07 | Schema «Modern»: butonul crește cât să încapă și umplutura, și textul                                                    | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-08 | Trecerea OPERATORULUI peste cele două ferestre de filtrare (+ ce a rupt designerul pe drum)                              | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|       0028-09 | Butonul de TEMĂ intră în bara de titlu (selectorul de teme nu mai stă în gazdă)                                          | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0029 | `KBotDataView`                                                                                                           | DONE                                          | [0020-0029](state/KBOT_STATUS_0020-0029.md) |
|          0030 | Meniul de coloană: TREI FILE (Sortare / Filtrare / Grupare), rânduri de tabel pe măsura temei, și bifele care se pierdu… | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|       0031-01 | Vizualizator de jurnale                                                                                                  | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|       0031-03 | Vizualizator de jurnale                                                                                                  | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0033 | Vederea ORD (Ordonanțări), read-only + ruta nouă `GET /api/forexe/ord`                                                   | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|       0033-02 | Arborele `DdfView` adus la paritate cu cel din `RezervariView` (iconițe din `ImageList` + dimensiuni)                    | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0034 | Descărcarea FOREXE: bandă de subsol + consolă + descărcări din arbore                                                    | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0035 | `KBotToolTip` (control nou) + scalarea la DPI a arborelui și a grilei + etichete în română                               | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0036 | Opțiunile temei într-o fereastră proprie + scalarea reglabilă                                                            | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|       0036-01 | Mărimea textului și a controalelor (cursor în meniu + în opțiuni)                                                        | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|       0036-02 | Cursorul aplică la SFÂRȘITUL gestului, nu la fiecare pas                                                                 | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0037 | `KBotDataView`                                                                                                           | DONE                                          | [0030-0039](state/KBOT_STATUS_0030-0039.md) |
|          0040 | Arborele, trecut PRIN TOT cu sistemul de scalare                                                                         | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0041 | Stocarea PDF-urilor SEMNATE pe server (`FX_DDF_PDF` / `FX_ORD_PDF`)                                                      | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0042 | Migrarea tabelelor `FX_` din Access în MariaDB                                                                           | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0043 | Pasul de deblocare a cheilor străine din `schema_sync`                                                                   | DONE / verificat pe un MariaDB 10.3.32 real   | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0043-01 | `schema_sync`: o singură bază + integritatea legăturilor spre `AVACONT_COMUN`                                            | DONE / verificat pe MariaDB 10.3.32 real      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0043-02 | `schema_sync`: eroarea 1054 de la prima rulare pe cele 22 de baze                                                        | DONE / reprodusă și corectată pe MariaDB 10.… | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0044 | Migrarea prin fișier `.accdb` împins pe server                                                                           | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0044-02 | `cale.accdb` scos cu totul · selecție pe unitate · upsert · bife pe tabele                                               | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0044-03 | Corelații de coloane Access ▸ MariaDB · filă nouă · toate grilele pe `KBotDataView`                                      | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0044-04 | Coloane obligatorii apărate · dosarul de instrucțiuni SQL                                                                | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-01 | Migrator direct Access ▸ MariaDB                                                                                         | DONE / niciun cod de transfer scris, niciun…  | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-02 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-03 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-04 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-05 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-06 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0045-07 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0046 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0047 | Migrator direct                                                                                                          | DONE                                          | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-01 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-02 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-03 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|     0048-03-c | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-04 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-05 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-06 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-07 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-08 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0048-09 | Ingestie FOREXE                                                                                                          | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0049 | Editorul de ordonanțare                                                                                                  | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0049-01 | «+»-ul din arborele de Plăți cheamă editorul de ordonanțare                                                              | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|       0049-02 | Cele cinci corecții cerute pe editorul de ordonanțare                                                                    | ÎN LUCRU                                      | [0040-0049](state/KBOT_STATUS_0040-0049.md) |
|          0050 | Calendarul tematizat                                                                                                     | ÎN LUCRU                                      | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0051 | `DdfEditForm` — calea de SCRIERE a documentului de fundamentare                                                          | ÎN LUCRU                                      | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|       0051-02 | `KBotComboBox` se poate TASTA                                                                                            | GATA                                          | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|       0051-03 | `DdfEditForm` aliniat la `OrdEditForm`                                                                                   | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0052 | Calibri peste tot + comutatorul «Font din temă» + ștampilele `AutoScaleDimensions`                                       | GATA                                          | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0053 | K-BOT Recorder                                                                                                           | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0054 | Cutia neagră a descărcătorului FOREXE                                                                                    | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0055 | Ingestia legată de buton                                                                                                 | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0056 | Tabloul întreg al propunerii                                                                                             | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|       0056-02 | Legăturile vechi se pot corecta la așezarea unei descărcări, dacă nu le blochează o ordonanțare (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, netestat, nerulat; server nedeployat) | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0057 | FOREXE: lista pe butonul din dreapta, oprirea pe «nu există în listă», extrasele SNM                                     | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0058 | Așezarea recepțiilor: cache la descărcare, buton de golire, tăcere pe data recepției, lag pe benzi                       | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|       0058-02 | Jurnalul asocierii (`asociere.log`)                                                                                      | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0059 | «Începe o recepție nouă», din coșul instantaneelor neașezate                                                             | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|       0059-02 | Corectură pe felia 0059: recepția pornită de operator e ȘTEARSĂ prin definiție                                           | GATA pe cod                                   | [0050-0059](state/KBOT_STATUS_0050-0059.md) |
|          0060 | Reîmprospătarea cerută de operator: macheta care nu se mai deschide degeaba, nodul care rămâne selectat, și cele două r… | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0061 | Selecție multiplă în arbore, ecran de asociere curat, și locul dat de ceas                                               | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|       0061-02 | Arborele in fereastra graficelor, si graficul rezervarilor                                                               | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0064 | Liniile cu zero ale unui instantaneu sunt linii și se păstrează (F31)                                                    | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0065 | Recepția valorează ultimul ei antet; rădăcina «Toate recepțiile»; DIF după salvarea din editor; perechile unice se așaz… | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0066 | Toate tabelele din soluție sunt `KBotTableLayoutPanel`, cu rândurile, coloanele fixe și marginea calculate la DPI ca ar… | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|       0066-02 | Corectură pe felia 0066: formularele se scalează la DPI, nu la font                                                      | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0067 | Actualizarea automată a aplicației                                                                                       | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0068 | Ancora pe id-ul de istoric (F34) și rândul de istoric consumat o singură dată (F33)                                      | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0069 | Umbra ferestrelor fără chenar (cu chenar de rezervă)                                                                     | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0070 | Browserul FOREXE nu mai apare niciodată singur pe ecran                                                                  | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0071 | Consola FOREXE fără zgomot: doar `<Log>` și erorile în Release                                                           | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0072 | Fereastra «Setări»: cinci pagini, comutatoarele operatorului, parola cu doi factori                                      | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0072-01 | Setări, a doua trecere: pagina «Jurnal», dialogul ferestrei găzduite Adobe, cursorul cu opriri, meniurile barei de titlu | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0072-02 | «Activează opțiuni avansate»                                                                                             | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0072-03 | Pagina «Setări ▸ FOREXE» nu mai cade la deschidere (cererea operatorului, 01.10.2026)                                      | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0073 | Recorder: mod de vizualizare, meniu plutitor în pagină, urmărirea operațiunilor FOREXE                                   | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0067-01 | Instalatorul Inno: detectează versiunea instalată, permite doar actualizări, actualizează cu regulile sistemului de act… | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|       0067-02 | Notele de versiune: la fiecare `publish-release` / `push-update`, un asistent AI (Copilot din VS sau Claude) scrie în română ce s-a schimbat (`docs/release-notes/NOUTATI.md`), trimis ca `notes` (cererea operatorului, 01.10.2026) | GATA pe scripturi (parsate, publicarea nerulată) | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
|          0074 | Vederea «Browser FOREXE» în shell: pagina andocată, arborele ↔ pagina                                                    | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0075-05 | Înregistrare publică: pagina operatorului                                                                                | GATA pe cod / verificat în browser pe un cio… | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0075-03 | Înregistrare publică: aprobarea — jobul de provizionare                                                                  | GATA pe cod / jobul n-a rulat niciodată — ni… | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0075-04 | Înregistrare publică: pagina solicitantului                                                                              | GATA pe cod / verificat în browser pe un cio… | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0075-02 | Înregistrare publică: nomenclatoarele, numele bazei, `FX_Inregistrari`, `/cerere`                                        | GATA pe cod / NIMIC nu a fost pornit — nicio… | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0075-01 | Înregistrare publică: magazinul dinainte de autentificare, proxy-ul ANAF, `/cod`, `/verifica`                            | GATA pe cod / NIMIC nu a fost pornit          | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
| 0075-00 rev.2 | `Clasificatii`: `Sector`, `Sursa` și `SS` devin coloane scrise                                                           | APLICAT PE SERVER de operator                 | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
| 0075-00 rev.1 | `Clasificatii.Sursa` devine coloană scrisă                                                                               | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0076 | Editările din «Browser FOREXE»: recepția salvată, rezervările ținute în memorie, marcajele de id                         | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0777 | Arborele principal: sortare + coloanele CODANGAJAMENT / SURSE; «Aplicație» primește file                                 | GATA pe cod, build curat                      | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0777-02 | Arbore: subliniat pentru descărcatele din sesiune; fără fereastra de recepții la actualizări multiple; coada se închide singură | GATA pe cod, build curat | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0078 | Semnarea DDF / ORD în vizualizatorul Adobe din K-BOT (absoarbe 0021)                                                     | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-03 | Jurnal de diagnostic exhaustiv pentru vizualizatorul ActiveX                                                             | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-04 | Secțiunea B după A pe DDF: sesiunea de semnare refăcută când suma de pe server se schimbă + proba pe banc             | PROBAT pe banc A→B→Ordonator; erori JS în vizualizator | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-05 | Fereastra găzduită fără poziționare / ascundere (Ctrl+H) + proba ordinii «Salvare ca» → scriere → încărcare  | GATA pe cod, nerulat (+ §8: acțiune unică ActiveX→fereastră, opțiune «ecran întreg la închidere») | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-06 | Secțiunea B inserată în DDF-ul semnat pe A, în fluxul real (angajament nou)                | GATA pe cod, nerulat                          | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-07 | Fereastra Adobe găzduită care se mărește singură: urmărită și readusă la panou              | GATA pe cod, nerulat                          | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-08 | Arborele blocat cât Adobe încă deschide documentul (semnal de la Adobe, nu cronometru)        | GATA pe cod, nerulat                          | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-09 | Întrebarea Adobe «salvați modificările?» la închiderea documentului: capcana rămâne pornită cât se închide și răspunde «Nu» (fără salvare) (cererea operatorului, 01.10.2026) | GATA pe cod, nerulat | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-10 | Opțiunea «Adobe pornește în interfața clasică» înapoi în Setări ▸ Documente (cererea operatorului, 01.10.2026): pune `bEnableAv2=0` la deschiderea unui PDF, restaurează la închiderea K-BOT | GATA pe cod, nerulat | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-11 | Procesele Adobe pornite de K-BOT nu mai rămân fantome după închidere: `AdobeProcessRegistry` (pid + ora de pornire; WM_CLOSE, răgaz 1,5 s, apoi kill pe arbore) (cererea operatorului, 03.10.2026) | GATA pe cod, nerulat; presupunere: fantomele sunt copii ai K-BOT | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-13 | «Replace existing file?» recunoscută și pe Acrobat vechi (fereastra găzduită) + F8 (bara din dreapta) trimis cu Ctrl+H / Ctrl+2; nimic pe ActiveX (cererea operatorului, 06.10.2026) | GATA pe cod, nerulat; ajutor neactualizat | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-12 | Fereastra găzduită cu un Adobe VECHI (< 2024, Acrobat DC / 2020): monitorul erorilor JS (`AdobeScriptMonitor`, evenimentul `ScriptAlertSeen`, sumar pe document în jurnal, trace) + banc propriu `OldAdobeHostHarnessForm` în DevHarness ▸ «Adobe/PDF» (cererea operatorului, 06.10.2026) | GATA pe cod (build Controls + App + teste: 0 erori, 0 avertismente); **bancul nerulat, teste scrise dar nerulate**; în jurnalul real: confirmarea «Replace existing file?» nerecunoscută pe Acrobat 19.12 (nereparată) | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-14 | Avertisment la pornire când Adobe e mai vechi de 2025: pe Acrobat Pro 2020 (crăpat) formularul DDF dă erori de permisiuni la semnare, pe Reader gratuit 2025+ nu; mesaj Da/Nu «Nu mai primești acest avertisment» + comutator în Setări ▸ Aplicație (cererea operatorului, 06.10.2026) | GATA pe cod, nerulat; build de reconfirmat; ajutor în `setari.md` (tag 0078-14) | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-15 | Vizualizatorul ActiveX (AcroPDF) refăcut de la zero: încărcare prin `src`, mărimea retrimisă când Adobe se naște 0×0, Ctrl+H verificat, capcana «Salvare ca»; vechiul `AcroPdfSurface` șters; acțiune unică ▸ ActiveX implicit (06–07.10.2026; partea din 06.10 commit-uită ca SLICELESS) | GATA pe cod, probat pe banca ActiveX; **Ctrl+S automat după semnătură NU merge**; ajutor neactualizat | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-16 | Banca ActiveX ▸ vizualizatorul jurnalului: butonul «Pe culoare…» deschide arborele din stânga ca *lane-uri* (`KBotLaneView`): rădăcină = culoar, frunză = o schimbare, axa = ora cu milisecunde, fără dată (cererea operatorului, 07.10.2026) | GATA pe cod (build App: 0 erori, 0 avertismente), **nerulat pe ecran**; DEBUG-only, fără ajutor | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|          0079 | Jurnalul semnăturilor (FX_PDF_SEMNATURI)                                                                                 | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0080-01 | `IdClsf` = `Clasificatii.IDClsf` pe șapte tabele FX\_ + `FX_Extrase.DataDoc` → DATE                                      | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0080-02 | Vederea «Extrase» + pagina «Setări → Extrase»                                                                            | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0080-03 | Fereastra «Extrase de cont»                                                                                              | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0080-04 | `IdClsfAcc` doar în `Clasificatii`                                                                                       | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0081 | Trimiterea unei revizii DDF din K-BOT în FOREXE (G0-G6)                                                                  | GATA pe cod — sub-feliile 0081-01 … 0081-10   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-01 | Starea reviziei (G0a)                                                                                                    | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-02 | Editorul: moduri noi, Secțiunea B ascunsă, PDF intermediar (G6a)                                                         | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-03 | Workflow-urile (G1, G2; G3 renunțat)                                                                                     | GATA pe fișiere                               | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-04 | Trimiterea (G6b)                                                                                                         | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-05 | Capturile în PDF-ul final (G5)                                                                                           | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-06 | K-BOT-ul directorului (G0b)                                                                                              | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-07 | Proba și reîncărcarea (cererea operatorului, 26.09.2026)                                                                 | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-08 | Rândul din Secțiunea A într-o fereastră (cererea operatorului, 26.09.2026)                                               | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-09 | Antet blocat, sursa în Secțiunea A, partenerul doar în antet (cererea operatorului, 26.09.2026)                          | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-10 | Clasificația fără «.02», lista de parteneri, antetul reviziei 0 (cererea operatorului, 26.09.2026)                       | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0081-12 | Revizie nouă din indicatorii existenți, clasificațiile folosite întâi, rândurile cu 0 șterse la salvare (cererea operat… | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0082 | `KBotComboBox`: `FindAsYouType`, `FindAfterNChars`, `InputMask` (cererea operatorului, 26.09.2026)                       | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0083 | `KBotComboBox.OfferNewItem` (cererea operatorului, 26.09.2026)                                                           | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0084 | «Operațiuni necorectate» după conectarea FOREXE (cererea operatorului, 26.09.2026)                                       | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0084/01 | Indicatorii la reîmprospătare + redenumirea `FX_Indicatori.Prevedere_Bugetara_Initiala` → `Credit_Bugetar` (operator, 2… | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0084/02 | Butonul «Asociază parteneri» din Sumar + tabela `FX_DDF_Parteneri` (mai mulți parteneri pe un DDF; fără DDF nu există asociere) (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, nerulat, nevăzut; SQL + server nedeployate) | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0085 | `KBotDataView`: editorul arată ca celula, editare la un clic, Sus/Jos prin coloană (cererea operatorului, 26.09.2026)    | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0085-02 | `KBotDataView`: butonul coloanei `Button` — imagine, culoare (și transparent), padding interior / exterior, font, caption, aliniere, mărime | GATA pe cod (build Controls curat, nerulat, nevăzut) | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0086 | `KbotForm` împărțit în clase parțiale + «Angajament nou» buton primar (cererea operatorului, 26.09.2026)                 | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0087 | «Clasificații bugetare», «Parteneri», `KBotDropDownMenu`, meniul din antet (cererea operatorului, 26.09.2026)            | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0088 | «Nota contabila corectie CAB» (F1135) pentru operațiunile «ERRRRRRRRRR» (cererea operatorului, 28.09.2026)               | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0089 | Jurnalele serverului în K-BOT, pe utilizator și pe sesiune (cererea operatorului, 28.09.2026)                            | done                                          | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|       0089-01 | Jurnalele serverului doar cu «Opțiuni avansate»; fără ele combo-urile de tip nu se văd (cererea operatorului, 01.10.2026)  | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0090 | `KBOT_STATUS.md` împărțit: index + fișiere de câte zece felii (cererea operatorului, 28.09.2026)                         | GATA (doar documente)                         | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0091 | Detaliul recepției citit tăiat din FOREXE: recunoscut pe server + recitire oferită; Setări → FOREXE: test de viteză, citire dublă, multiplicator de timpi; F14 în pauză (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0092 | Rezervarea inițială = un singur eveniment, pe ultima zi inițială (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0093 | Parteneri: doar Tip 1 cu cod fiscal (nu al unității, neascunși), unul pe cod fiscal; Tip scos, «Alte detalii» → «Adresa»; ANAF la codul fiscal; banca din IBAN (BIC); cod fiscal unic la salvare (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0094 | `KBotComboBox` rescris pe `Control` (fără `ComboBox`, fără `DataSource`), o singură listă (săgeată = toate, tastare = potrivite); editorul de combo din `KBotDataView` (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|       0094-02 | Editorul DDF: pagina «Parteneri» (aliniată la dreapta); combo-ul din antet rămâne partenerul principal (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, nerulat, nevăzut; SQL + server nedeployate) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|       0094-03 | Editorul DDF: pagina «Parteneri» apare doar cât timp «Partener asociat» e bifat (corectiv, 04.10.2026) | GATA pe cod (build curat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0095 | Istoric: nod rădăcină «Tot istoricul» în arbore (toate rândurile, ca la încărcare) (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|       0095-02 | Subsolul arborelui descarcă direct extrasele; «Extrase de cont» din «Meniu → Extrase»; `btnMeniu` urmează lățimea lui `navViews` strâns (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0096 | Extrase: meniu de afișare în antetul arborelui — «antet + operații» / «operații + detalii» (cererea operatorului, 29.09.2026) | GATA pe cod (build curat, netestat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0097 | Corecturi: ORD/DDF (fără mesaj la ștergere, rădăcini «Toate…», ștergere pe lună/toate, semnat = fără meniu); eticheta barei strânse la DPI; «Note corecție» doar cu note; selector de unitate în bara de titlu; reafișarea ferestrei de autentificare + «Ține minte parola» (cererea operatorului, 30.09.2026) | GATA pe cod (build curat, netestat, nevăzut; server nedeployat) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|       0097-02 | Corecturi, a doua trecere: turul ferestrei principale pornește singur (până e văzut / «Nu mai arăta turul inițial»); «Da» automat la întrebările «Sunteți sigur...?» din pagina FOREXE; MENIU «Adăugare angajamente...» + «Creează angajament în FOREXE»; mini-meniul din pagină după setare; conectarea fără «?»; pornire mărită; întrebare la a doua recepție pe aceeași dată (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, nerulat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|       0097-03 | Anul și Sursa/Sectorul se mută din banda de sus în bara de titlu, lângă selectorul de unitate; dispar din `KbotForm` (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, netestat, nevăzut) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0098 | Coada robotului (o singură coadă, în ordine, pauză / scoatere, fereastra «Coada robotului») + poarta serverului (nicio scriere pe server cât rulează robotul; citirile trec) (cererea operatorului, 30.09.2026) | GATA pe cod (build curat, netestat, nevăzut); 0098-02: citirile trec poarta + banc DevHarness (nerulat) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0099 | `PrintCount`: de câte ori a fost tipărit un document — coloană pe cele patru tabele de PDF + `FX_DDF_REV` / `FX_ORD` pentru nesemnate; tipărirea văzută în coada de tipărire Windows (`AdobePrintWatcher`), numărată prin `POST …/print`; legat și în bancul de semnare; **lista de tipărire** pe lună / «Toate» din ORD și DDF («Generează și imprimă», «Salvează local», «Listat» de mână) (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; SQL + server nedeployate) | [0090-0099](state/KBOT_STATUS_0090-0099.md) |
|          0100 | Descărcări multiple: mai multe angajamente deodată, câte un tab FOREXE fiecare; `FX_Angajamente.DataActualizare`; fereastra «Actualizează angajamente»; actualizarea la conectare și a angajamentelor noi; pagina «Aplicație» regrupată (cererea operatorului, 01.10.2026) | GATA pe cod (build `src/` curat, nimic rulat, nevăzut; SQL + server nedeployate); ajutorul: 0000-34 | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0100-02 | `Setari` pe fiecare bază: serverul decide multi-thread (`Multithread`, `Multithread_Max`); pagină nouă «Descărcări multiple» în Setări, ascunsă cât serverul o oprește; grupul scos din «Aplicație» (cererea operatorului, 01.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; SQL + server nedeployate; ajutorul: 0000-36, fără capturi) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0100-03 | Descărcări multiple: în «Coada robotului», câte un rând pentru fiecare angajament în lucru (cod, bară de progres după pașii fluxului, «X» care oprește doar acea descărcare și îi închide tabul), numărul celor care așteaptă; nimic la un singur angajament (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: 0000-48, captura `coada-descarcari-multiple` de făcut) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0100-04 | Clic acoperit de bara fixă FOREXE: timpii fluxului «Reverse» la 15 s + reîncercare cu elementul în mijlocul ferestrei (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; fără ajutor) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0100-05 | Fără zgomotul «nicio captură de … de trimis» după «Actualizare multiplă» (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat; fără ajutor) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|          0101 | Lanț de recepții care nu se închide: rândul și motivul în roșu în arborele principal și în fereastra de asociere; «+» pe lună în Plăți fără mesaj de succes (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; `tree.py` nedeployat; ajutorul: 0000-37) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0101-01 | ORD și DDF: bara cu pagini rămâne pe o lună / «Toate…»; pagina «Documente» are lista de tipărire, «Vizualizare» arată din nou datele (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: 0000-38, fără capturi) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
|       0101-02 | Fără fereastra «Istoric angajament» după o salvare în pagina FOREXE; fereastra rămâne în proiect, doar referințele dispar (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: fără paragraful și captura `istoric-interval`) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0102 | Bugetul clasificației cu date (`Clasificatii_Buget.DataInceput`): DDF-ul arată bugetul de la data reviziei, nu pe cel de azi; fereastra «Clasificații bugetare» pe versiuni, fără total (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; **DDL + server nedeployate**; ajutorul: 0000-40) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103 | Interogări unice: o interogare rulată o singură dată pe AVACONT_SURSA și pe fiecare unitate, ținută minte în `Interogari_Unice`; unitățile noi se nasc cu ea «rulată»; fila nouă din AvacontPush (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat; **DDL pe șablon + server nedeployate**; fără ajutor, unealtă de dezvoltare) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103-02 | AvacontPush: login cu cheia SSH, fără parolă; toate bazele existente se bifează la detecție, și în fila «Interogări unice» (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nicio conectare făcută; fără ajutor) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103-03 | Buget de deschidere din fișierele anului anterior: butonul «Buget 1/12» în Migrator — `Trim1 = ⌈(Clasificatii + Rectificari)/12⌉`, `Trim2..4 = 0`, `DataInceput` 01.01 (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat; cere DDL-ul 0102; fișierul anului anterior dedus din nume) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103-04 | Verificarea bugetului FOREXE față de K-BOT: fereastra «Verificare buget FOREXE» (Buget K-BOT / Credit FOREXE / Diferență), butonul «Verifică bugetul», verificare automată după o descărcare (doar la diferențe) (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; server nedeployat; ajutorul: 0000-41) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103-05 | Funcție VBA Access → baza NOUĂ MariaDB: `docs/vba/mdl_FX_CLS_SYNC_TO_KBOT.bas` + ruta `POST /api/clasificatii/sync_acc_kbot` (IdUnitate + IDClsf ↔ IdClsfAcc) (cererea operatorului, 02.10.2026) | GATA pe cod (Python compilat; VBA necompilat, neimportat în Access; server nedeployat; fără ajutor, e cod VBA) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0103-06 | Butonul «Trimite în Access» în «Clasificații bugetare»: bugetul în vigoare azi + rectificările anului, în fișierul Access al unității din registrul `cale.accdb` (ACE 64-bit) (cererea operatorului, 02.10.2026) | GATA pe cod (build curat, nimic rulat pe un fișier Access real; «registru» citit ca `cale.accdb`; parola unică ofuscată în cod; ajutorul: 0000-41) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0104 | Componentele Access (Migratorul + comunicarea cu Access) separate de aplicația principală: proiectul `KBot.Access`, pachet «cu Access» / «fără Access» (întrebare în `publish-release` / `push-update`), `Setari.Access` pe fiecare bază, `POST /api/access/client-type`, actualizări pe două categorii (`?access=`, `latest.json` cu două blocuri) (cererea operatorului, 02.10.2026) | GATA pe cod (build `src/` curat, nimic rulat cap-coadă: scripturile, serverul, instalatorul fără Access; **DDL + server nedeployate**; 0104-02: pagina «Access» în Setări + calea către `cale.accdb` editabilă) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0105 | Clasificații bugetare: sumar doar-citire pe nodurile de deasupra alineatelor, arbore filtrat pe mișcare + «Arată toate clasificațiile», «Verifică bugetul» fără diferențe zero (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; **server nedeployat**; ajutorul: 0000-42) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0106 | Trei reparații din jurnale: folderul de capturi nul («path1»), reîncercări Adobe fără zgomot în jurnalul de erori, clic dreapta în arborele DDF / ORD nu mai reîncarcă PDF-ul (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; fără ajutor) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0107 | Controlul arbore: clic dreapta nu selectează un rând neselectat (proprietatea `RightClickSelects`, pusă pe `False` în DDF, ORD și Asocieri) (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: 0000-43) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0107-02 | Clasificații bugetare: coloana «Total» pe rândul grilei de buget, fără total jos; rectificările fără rând de total când nu e aleasă o frunză (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: 0000-44) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0108 | Creditul bugetar pe clasificație (`FX_Indicatori_Buget`), bugetul pe zi fără trimestre, creditul inițial citit o singură dată din «Informații complete contract», `DTI`/`DTQ`, Migrator: totalul în Trim1 (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul: 0000-45) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0109 | «Actualizează angajamente...» din meniul listei și fără descărcare multi-thread: angajamentele bifate intră în «Coada robotului», unul după altul, ca la apăsări succesive pe iconița fiecăruia (cererea operatorului, 03.10.2026) | GATA pe cod (build curat, nimic rulat, nevăzut; ajutorul + turul: 0000-49; NOUTATI: linia de adăugat la următoarea versiune) | [0100-0109](state/KBOT_STATUS_0100-0109.md) |
| 0110-01 | Sait: secțiunea «K-BOT te învață singur» (tutoriale interactive) pe pagina publică (cererea operatorului, 05.10.2026) | GATA pe cod, nevăzut, server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-02 | Sait: pagina «Cere mai multe detalii» (`/detalii`, tabel `FX_CereriDetalii`, mail la info@avatarsoft.ro) | GATA pe cod, probat cu stub; **DDL `sql/0110_02_fx_cereri_detalii.sql` + server nedeployate** | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-03 | Sait: portal, autentificare web (email + parolă + cod pe e-mail), sesiune, alegerea unității, monitorul de sesiune | GATA pe cod, probat cu server de probă; server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-04 | Sait: portal, intrare cu certificat digital calificat (nginx mTLS, host separat `cert.k-bot.ro`) | Deployat pe VPS și probat real cu un certificat certSIGN; deschise în worklog | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-05 | Sait: control JS DGV doar-citire (`static/js/dgv/`) | GATA pe cod, comportament probat în browser, aspectul nevăzut pe ecran | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-06 | Sait: pagini de vizualizare (Sumar, Rezervări, Recepții, Plăți) | GATA pe cod, probat cu date fictive; baza reală neîncercată, server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-07 | Sait: cercetare PDF (XFA) în browser | Cercetare încheiată (doar verdict, fără cod) | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-08 | Sait: fila «Documente» (DDF, ORD, note) cu vizualizator PDF doar-citire | GATA pe cod, probat cu PDF-urile-exemplu; DDF semnat din K-BOT neavut, server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-09 | Sait: pagina «Administrare» doar pentru scavatarsoft@gmail.com: baze de date, jurnale (filtru pe fisier si pe baza, inclusiv «fara baza»), vizitatorii paginii de prezentare (IP, steag, timp, sectiuni, mobil) (cererea operatorului, 05.10.2026) | GATA pe cod, probat cu date inventate; **nevazut pe ecran, baza reala neincercata; DDL `sql/0110_09_vizite_site.sql` + `maxminddb` + fisierul de tari + server nedeployate** | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-10 | Sait: fiecare card al angajamentului are arborele lui pe calculator, fara grupari pe data (in afara de Istoric), «Documente» devine «Fundamentari» + «Ordonantari» (cererea operatorului, 05.10.2026) | GATA pe cod, probat cu date inventate; nevazut pe ecran, server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-11 | Sait: Administrare > «Coloane»: coloanele vizibile, pozitia, latimea si coloana care se extinde pentru toate grilele; export JSON pentru dezvoltator (cererea operatorului, 05.10.2026) | GATA pe cod, probat cu date inventate; asteapta fisierul administratorului; server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0110-12 | Sait, doar calculator: detalii sub grila la Istoric si Plati, meniul de vizualizare la Extrase, navbar + header la Fundamentari / Ordonantari (cererea operatorului, 05.10.2026) | GATA pe cod, probat cu date inventate; nevazut pe ecran, server nedeployat | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
| 0111 | Valoarea unui instantaneu de recepție se poate corecta din fereastra Asocieri când FOREXE a scris una greșită (ex. total 0 peste o linie de 1635): coloanele de lucru `Total` / `Valoare` se corectează, `TotalOrig` / `ValoareOrig` păstrează ce a zis FOREXE, `CorectatDe/La/Motiv` pe antet; totalul trebuie să fie egal cu suma liniilor (eroare blocantă); `FX_Istoric` nu se atinge (cererea operatorului, 06.10.2026) | GATA pe cod (build `KBot.App` curat, nimic rulat pe o bază; **DDL pe șablon + interogare unică + server nedeployate**; ajutorul: 0000-53) | [0110-0119](state/KBOT_STATUS_0110-0119.md) |
|          0112 | **Caseta de mesaj K-BOT + catalog de mesaje (debug)**: `KBotMessageBox` în Controls înlocuiește `MessageBox`-ul nativ în tot codul prin `KBotMessage.Presenter`; macheta DevHarness cu toate cele 474 de mesaje; 0112-04: buton «Trimite eroarea» în bara casetei (tabel FX_RaportErori, `POST /api/errors/report`) | GATA pe cod, nevăzut pe ecran; tabelul + serverul de aplicat înainte de publicare | [0110-0119](state/KBOT_STATUS_0110-0119.md) |

Work outside the slice system: [KBOT_STATUS_SLICELESS.md](state/KBOT_STATUS_SLICELESS.md).

**Next free slice number: 0113.** (0021 — semnarea DDF — a fost absorbită de felia 0078.)
⚠️ **Registrul are o gaură: 0038 și 0039 lipsesc.** Codul din `KBot.Controls/Tree` se referă în
comentarii la «felia 0038» (culoarea/grosimea separatorilor) și «felia 0039» (marginile scalate),
amândouă vizibile în arborele de lucru, dar niciuna n-are rând aici, iar linia de mai sus declara
încă «Next free: 0038». Felia 0040 a luat următorul număr liber REAL; rândurile 0038/0039 rămân de
completat de cine le-a făcut.
(0031 e în curs: trecerile 01 și 03 sunt gata; mai vin 02 jurnale de server și 04 `LogViewerForm`.)
(0032 = sub-paginile DDF, în lucru în arborele de lucru la data feliei 0033.)

**Sinkuri terminale ne-rearuncătoare — sunt CINCI, nu trei.** Planul feliei 0031 §1.4 cerea
corectarea, aici, a unei linii care ar fi spus «exact trei». Linia aceea **nu a existat niciodată în
acest fișier** (căutată în tot `docs/`); singura formulare a regulii stă în `CLAUDE.md:43` și nu
numără nimic. Situația reală, ca să nu mai fie nevoie de căutare data viitoare:
`GlobalErrorLog.Write`, `AdobeHostLog.Write`, `DevHarnessForm.HandleUiError` (scrierea în runlog),
plus, din felia 0031-01, `TreeLogger.Write` și `TreeLogger.Init`. Ultimele două **nu pot** rearunca:
`AdvancedTreeControl` le cheamă din căi de desenare, iar o excepție dintr-o scriere de jurnal în
`OnPaint` omoară procesul. Toate cinci scriu motivul pe `Trace` — niciunul nu înghite tăcut.

---

## Current focus

- Each slice's focus notes are in its `state/KBOT_STATUS_*.md` file, under «Current focus».
- Latest (01.10.2026): slice 0100, several angajamente downloaded at once, one FOREXE tab each
  ([0100-0109](state/KBOT_STATUS_0100-0109.md); branch `SLICE-0100-Multithreading`; deploy order: the DDL first).
- Earlier (01.10.2026): slice 0067-02, release notes written by an AI assistant at every
  `publish-release.ps1` / `push-update.ps1` ([0060-0069](state/KBOT_STATUS_0060-0069.md);
  procedure: `docs/release-notes/README.md`).
- Same day: slice 0097-02 (second corrective pass) and its help, 0000-25
  ([0090-0099](state/KBOT_STATUS_0090-0099.md), [0000-0009](state/KBOT_STATUS_0000-0009.md)).
- Same day: slice 0056-02, old links can be corrected while placing a download, and its help,
  0000-26 ([0050-0059](state/KBOT_STATUS_0050-0059.md)).
- Same day: slice 0000-27, the «?» popup lists tours only for the window whose «?» was pressed and
  its views only ([0000-0009](state/KBOT_STATUS_0000-0009.md)).
- Same day: slices 0084/02 (Sumar button «Asociază parteneri» + table `FX_DDF_Parteneri`) and 0094-02
  (DDF editor page «Parteneri»), and their help + the guided tour of the DDF editor, 0000-28
  ([0080-0089](state/KBOT_STATUS_0080-0089.md), [0090-0099](state/KBOT_STATUS_0090-0099.md),
  [0000-0009](state/KBOT_STATUS_0000-0009.md)). Deploy order: the DDL first.
- Same day (no slice): the «Setări» button left the main caption bar; «Jurnal activitate» and «Configurare K-BOT» in the MENIU menu, and their help, 0000-29
  ([SLICELESS](state/KBOT_STATUS_SLICELESS.md), [0000-0009](state/KBOT_STATUS_0000-0009.md)).
- Same day: slice 0000-32, the help window is one continuous page (the tree follows the scroll),
  «Imprimă» and «Exportă» each with a menu of how much, and the freeze on «Exportă»
  ([0000-0009](state/KBOT_STATUS_0000-0009.md)). Not run, not seen on screen.
- Same day: slice 0099, `PrintCount` — prints of the documents shown in the Adobe pane, noticed in
  the Windows print queue and counted on the server ([0090-0099](state/KBOT_STATUS_0090-0099.md)).
  Nothing run. Deploy order: the DDL first. No help change (nothing the operator sees).
- Earlier (28.09.2026): slice 0090, this file split ([0090-0099](state/KBOT_STATUS_0090-0099.md));
  slices 0088 and 0089 ([0080-0089](state/KBOT_STATUS_0080-0089.md)), plus
  the sliceless «silent FOREXE downloads + FOREXE page pictures» work, whose cause is still NOT
  found ([SLICELESS](state/KBOT_STATUS_SLICELESS.md)).

## Open threads (not yet scheduled)

- Each slice's open threads are in its `state/KBOT_STATUS_*.md` file, under «Open threads».
  Threads that belong to no single slice (server housekeeping, DDL gaps, in-process dicts,
  the 11 red tests, code shipped without a slice…) are in
  [KBOT_STATUS_SLICELESS.md](state/KBOT_STATUS_SLICELESS.md).

## Locked decisions (do not relitigate without a note here)

- **HELP = slice 0000, permanently** (operator, 30.09.2026). All work on the interactive help and
  the manual (engine, capture tool, topics under `src/KBot.App/HelpContent/`, screenshots, guided
  tours, manual export) is recorded as a sub-slice `0000-NN` in
  [state/KBOT_STATUS_0000-0009.md](state/KBOT_STATUS_0000-0009.md), worklog `SLICE-0000-NN-*.md`.
  Never give help work a new slice number. A feature slice that changes a screen should also say
  which help topic / capture tag it makes stale («Ajutor de actualizat» in the 0000 Open threads).
  Maintainer guide + update procedure: `docs/HELP_SYSTEM.md`.

- **Clasificația pe tabelele FX\_: `IdClsf` = `Clasificatii.IDClsf` (cheia MariaDB)** (felia 0080-01,
  24.09.2026). Nu mai există tabel FX\_ care să țină id-ul Access în `IdClsf`; legăturile se fac pe
  `C.IDClsf = X.IdClsf`. Cele șapte tabele convertite NU țin o copie a id-ului Access (operatorul:
  «I don't want to keep IdClsfAcc in those tables»); el stă doar în `Clasificatii.IdClsfAcc`.
  Marcajul unei tabele convertite = comentariul `Clasificatii.IDClsf (0080-01)` pe `IdClsf`.
- **Id-ul Access al clasificației stă DOAR în `Clasificatii.IdClsfAcc`** (felia 0080-04,
  25.09.2026, operatorul: «Clasificatii ... should be the source of truth»). Niciun alt tabel nu
  are coloana `IdClsfAcc`; cine are nevoie de id-ul Access îl citește prin `IdClsf`.

- Single-worker Gunicorn stays; `on_starting` guard refuses `workers > 1`.
- Under Redis: TTL counts down to the 30-min absolute cap (no slide); the 20-min idle
  window is enforced in-app from `expires_at`; operator password is never written to Redis;
  keys namespaced `kbot:sess:` in K-BOT's own DB; never FLUSHDB/FLUSHALL (shared Redis).
- Static API key kept ONLY for the legacy FOREXE fleet on shared routes; eliminated for
  K-BOT.
- **Două servere MariaDB, alese după CINE cheamă ruta** (2026-08-25). `DB_CONFIG` =
  serverul vechi, unde clienții Access/VBA scriu în continuare; `DB_CONFIG_NEW` = serverul
  K-BOT, unde trăiește tot ce face `KBot.App`. Regula practică e garda rutei: `X-Api-Key`
  (`require_api_key`) → vechi, prin `utils.database.get_db_connection`; token bearer
  (`require_session`) → nou, prin `utils.database.get_kbot_connection`. Adică
  `routes/auth/*` și `routes/forexe/*` fără `seed.py`. Aceleași conturi pe ambele mașini
  (AVACONT, Admin, loginurile operatorilor) — diferă doar adresa.
  - **Excepția, deliberată:** `routes/migrare/*` și `routes/schema_sync/*` sunt rute cu
    `X-Api-Key` care serveau `KBot.Migrator`, dar **rămân pe `DB_CONFIG` și nu se ating** —
    jumătatea Python a migrării nu mai e folosită, `KBot.Migrator` face acea treabă singur,
    în VB.NET, către adresa din `migrator-settings.json`.
  - `db_reader` (contul read-only pentru tabelele de login) **există doar pe serverul
    vechi**. Pe cel nou citirile din `AVACONT_COMUN` merg prin contul de serviciu
    (`get_kbot_comun_connection`); `READER_DB_CONFIG` rămâne în `config.py`, nefolosit,
    ca drumul înapoi să fie o singură funcție dacă se creează contul și acolo.
- **The "drop `IdUnitate`" rule applies to `FX_` tables ONLY.** Shared nomenclatoare
  (`Clasificatii`, `Parteneri`, and others of that kind) **keep** the `IdUnitate`
  predicate: the per-unit database holds them for SEVERAL units. On `000_DEMO`,
  `Clasificatii` carries 8 (48, 75, 76, 121, 123, 135, 136, 157). Measured cost of
  getting this wrong in slice 0011 (`FX_Indicatori` = 29 rows, 25 with `IdClsf <> 0`):
  `ON I.IdClsf = C.IDClsf` → **0** rows (wrong key); `ON I.IdClsf = C.IdClsfAcc` →
  **67** (multi-unit fan-out); `+ AND I.IdUnitate = C.IdUnitate` → **50** (duplicate
  fan-out). Full detail: `SLICE-0011-03-sumar-join-clasificatii.md`.
- **Key-naming conventions, both confirmed against the live schema:**
  - `Clasificatii`: `IDClsf` = MariaDB PK, `IdClsfAcc` = retained Access id.
    **BUT `FX_Indicatori.IdClsf` holds the ACCESS id — it does NOT follow the
    convention.** The column name is not evidence.
  - `FX_ORD` family: suffix "P" = MariaDB PK (`IDORDTBLP`), without "P" = Access id
    (`IDORDTBL`).
  - **Operational rule that beats both tables: never infer the key from the column
    name — COUNT rows before and after the join.** A join returning 0, or more rows
    than its left-hand table, is a defect, not a data quirk. Slice 0011 lost three
    live runs to this; one `COUNT(*)` would have caught all three.
- **Migrarea `FX_` NU recreează schema (varianta A, nedistructivă).** Tabelele există
  deja în MariaDB cu DDL curat; `POST /api/forexe/seed/schema` rămâne în cod pentru o
  bază complet goală, dar **utilitarul de migrare nu îl apelează** — face
  `DROP TABLE IF EXISTS` înainte de un `CREATE` derivat din tipurile DAO ale Access-ului
  (`LONGTEXT` pentru orice tip necunoscut, `VARCHAR(255)` implicit), deci ar înlocui o
  schemă bună cu una mai slabă, peste date reale. Lista de coloane se citește în schimb
  din destinație, cu `GET /api/forexe/seed/columns` (slice 0012-01).
- Shared nomenclatoare are read via **scalar subqueries with `LIMIT 1`**, not joins,
  wherever the value is display-only (`Clsf`, `Partener` in Sumar). `Clasificatii` has
  real duplicates on `(IdClsfAcc, IdUnitate)`, so even a fully-predicated join fans
  out; a scalar subquery guarantees one row per indicator.

---

## How to update this file

- When a slice changes state, edit its full row and notes in its `state/KBOT_STATUS_<tens>.md`
  file, AND its short line in the index above (name, status in a few words, link).
- When you assign a new slice, add a line to the index, add a `## Slice NNNN` section
  (Registry / Current focus / Open threads) to the right `state/` file (create the file when
  a new group of ten starts, e.g. `KBOT_STATUS_0090-0099.md`), and bump "Next free slice number".
- Work the operator asks for without a slice number goes to `state/KBOT_STATUS_SLICELESS.md`.
- Only rules that apply to EVERY task (locked decisions, cross-cutting notes) belong in this
  file. Keep it small: that is the reason it was split.
