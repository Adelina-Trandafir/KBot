# K-BOT — STATUS (single source of truth)

This file is the one place that says what is done, what is in progress, and what is next.
Three people rely on it: the developer, Claude (chat), and Code (Claude Code / Codex).
Keep it current. If reality and this file disagree, fix this file in the same commit.

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
|          0001 | Auth                                                                                                                     | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0002 | Split-brain 401 fix + reason codes                                                                                       | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0003 | Redis session backend                                                                                                    | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0004 | Tier 1 hardening                                                                                                         | IN PROGRESS                                   | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0005 | Phase A cleanup (lying tests, https guard)                                                                               | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0006 | MainForm scaffolding                                                                                                     | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0007 | AngajamentTreeInfo POCO correction                                                                                       | SUPERSEDED by 0008                            | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
|          0008 | Tree data API + `MainForm.LoadTree`                                                                                      | DONE                                          | [0000-0009](state/KBOT_STATUS_0000-0009.md) |
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
|          0073 | Recorder: mod de vizualizare, meniu plutitor în pagină, urmărirea operațiunilor FOREXE                                   | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0067-01 | Instalatorul Inno: detectează versiunea instalată, permite doar actualizări, actualizează cu regulile sistemului de act… | GATA pe cod                                   | [0060-0069](state/KBOT_STATUS_0060-0069.md) |
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
|          0078 | Semnarea DDF / ORD în vizualizatorul Adobe din K-BOT (absoarbe 0021)                                                     | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-03 | Jurnal de diagnostic exhaustiv pentru vizualizatorul ActiveX                                                             | GATA pe cod                                   | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-04 | Secțiunea B după A pe DDF: sesiunea de semnare refăcută când suma de pe server se schimbă + proba pe banc             | PROBAT pe banc A→B→Ordonator; erori JS în vizualizator | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
|       0078-05 | Fereastra găzduită fără poziționare / ascundere (Ctrl+H) + proba ordinii «Salvare ca» → scriere → încărcare  | GATA pe cod, nerulat (+ §8: acțiune unică ActiveX→fereastră, opțiune «ecran întreg la închidere») | [0070-0079](state/KBOT_STATUS_0070-0079.md) |
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
|          0085 | `KBotDataView`: editorul arată ca celula, editare la un clic, Sus/Jos prin coloană (cererea operatorului, 26.09.2026)    | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0086 | `KbotForm` împărțit în clase parțiale + «Angajament nou» buton primar (cererea operatorului, 26.09.2026)                 | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0087 | «Clasificații bugetare», «Parteneri», `KBotDropDownMenu`, meniul din antet (cererea operatorului, 26.09.2026)            | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0088 | «Nota contabila corectie CAB» (F1135) pentru operațiunile «ERRRRRRRRRR» (cererea operatorului, 28.09.2026)               | GATA pe cod                                   | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0089 | Jurnalele serverului în K-BOT, pe utilizator și pe sesiune (cererea operatorului, 28.09.2026)                            | done                                          | [0080-0089](state/KBOT_STATUS_0080-0089.md) |
|          0090 | `KBOT_STATUS.md` împărțit: index + fișiere de câte zece felii (cererea operatorului, 28.09.2026)                         | GATA (doar documente)                         | [0090-0099](state/KBOT_STATUS_0090-0099.md) |

Work outside the slice system: [KBOT_STATUS_SLICELESS.md](state/KBOT_STATUS_SLICELESS.md).

**Next free slice number: 0091.** (0021 — semnarea DDF — a fost absorbită de felia 0078.)
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
- Latest (28.09.2026): slice 0090, this file split ([0090-0099](state/KBOT_STATUS_0090-0099.md));
  slices 0088 and 0089 ([0080-0089](state/KBOT_STATUS_0080-0089.md)), plus
  the sliceless «silent FOREXE downloads + FOREXE page pictures» work, whose cause is still NOT
  found ([SLICELESS](state/KBOT_STATUS_SLICELESS.md)).

## Open threads (not yet scheduled)

- Each slice's open threads are in its `state/KBOT_STATUS_*.md` file, under «Open threads».
  Threads that belong to no single slice (server housekeeping, DDL gaps, in-process dicts,
  the 11 red tests, code shipped without a slice…) are in
  [KBOT_STATUS_SLICELESS.md](state/KBOT_STATUS_SLICELESS.md).

## Locked decisions (do not relitigate without a note here)

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
