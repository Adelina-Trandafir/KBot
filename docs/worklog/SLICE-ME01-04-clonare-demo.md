# ME01-04 — «Clonare DEMO» în KBotAdmin

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: nici o conexiune la server, nici SQL, nici teste.**

## Ce s-a cerut

O opțiune în KBotAdmin care golește `000_DEMO` și clonează în el o altă bază, la alegerea operatorului. Codul fiscal al lui `000_DEMO` se păstrează peste tot unde e nevoie, iar `DC`, dacă există într-un tabel, devine `000_DEMO`.

## Ce s-a făcut

Pagină nouă **Clonare DEMO** (sursa = baza aleasă în arborele din stânga; destinația e mereu `000_DEMO`):

- **Previzualizare** — nu scrie nimic. Arată tabelele, rândurile din `000_DEMO` (care se șterg) și din sursă, coloanele care se rescriu cu numărul de rânduri, și avertismentele.
- **Golește și clonează** — cere să scrii `000_DEMO` în căsuța de confirmare, apoi o întrebare Da/Nu. Serverul:
  1. blocare pe server (o singură clonare odată);
  2. copie de siguranță a lui `000_DEMO` (`backup/`); fără ea nu se atinge nimic;
  3. o singură tranzacție: `DELETE` din toate tabelele lui `000_DEMO`, apoi `INSERT … SELECT` din sursă pe coloanele comune;
  4. verificare înainte de `COMMIT` (nici un `DC` străin, nici un cod fiscal al sursei rămas); orice eșec = `ROLLBACK`, `000_DEMO` rămâne cum era;
  5. intrare în `Jurnal` (`ADMIN_DEMO_CLONE`).

### Regulile de rescriere (în `admin_demo.py`)

| Ce | Cum |
|---|---|
| Coloane numite `DC` (text) | `000_DEMO`, NULL rămâne NULL (azi: `FX_Angajamente`, `FX_DDF`, `FX_DDF_REV`; `FX_NumberLock` nu se copiază) |
| Coloane de cod fiscal (nume `CF`, `CUI*`, `CIF*`, `CodFiscal*`, `*_cui`, tip text) | valoarea egală cu CF-ul sursei (fără `RO`, fără spații) devine CF-ul lui `000_DEMO` din `AVACONT_COMUN.Unitati`; prefixul `RO` se păstrează dacă îl avea. CF-ul unui furnizor/client nu se atinge |
| `EF_Furnizor.CodFiscal` | rămâne ce avea `000_DEMO` înainte de golire (restul rândului vine din sursă) |
| `EF_Mesaje.CuiUnitate` (unitatea care a primit mesajul) | CF-ul lui `000_DEMO`, orice ar fi avut |

`AVACONT_COMUN` (Unitati, Unitati_Ani, Unitati_Utilizatori, CAI, EF_Token …) NU se atinge: identitatea lui `000_DEMO` stă acolo.

## Fișiere

- `PYTHON/routes/admin_cli/admin_demo.py` (nou; verbele `demo-clone-plan`, `demo-clone`, consemnate și în vechiul registru ME01-04) + 2 linii în `admin_cli.py` (înregistrare, antet)
- `KBotAdmin/Views/DemoCloneView.vb` + `.Designer.vb` (noi), `KBotAdmin/Server/Dtos.vb` (DTO-uri), `KBotAdmin/MainForm.vb` + `.Designer.vb` (butonul «Clonare DEMO»)

## Nevalidat / limite (de știut înainte de prima rulare)

- **Nerulat nicăieri.** Prima rulare: **Previzualizare** pe o bază mică, apoi clonarea. Interogările (`REGEXP_REPLACE`, `INSERT … SELECT` între scheme) sunt scrise de mână, neverificate pe MariaDB 10.11.
- **IdUnitate nu se rescrie.** Aplicația ia `IdUnitate` din tabela `Unitati` a bazei (după `SursaSector`), deci clona merge cu ce a adus sursa. Rândurile `CAI` ale lui `000_DEMO` (alte `IdUnitate`) rămân neatinse. Dacă sursa are un sector-sursă pe care `000_DEMO` nu îl are în `AVACONT_COMUN.Unitati_Ani`, previzualizarea avertizează: acea combinație nu se poate alege la login până nu o adaugi.
- **Nu se rescriu** codurile din XML/PDF/imagini (`EF_Facturi`, `EF_Mesaje.XmlContinut`, `FX_*_PDF` …) și din texte libere; rămân ale sursei.
- Tabelele de blocări (`FX_NumberLock`, `AD_Lock`, `VC_ContextLock`) rămân goale.
- Se refuză dacă o tabelă din `000_DEMO` nu e InnoDB, sau dacă `000_DEMO` are coloane obligatorii pe care sursa nu le are (aduceți schema la SURSA din pagina Sincronizare).
- O coliziune de chei (de ex. un partener din sursă care ar avea același CF cu `000_DEMO` după înlocuire) oprește clonarea cu rollback.
- Clonarea unei baze mari ține o tranzacție lungă; serverul răspunde la sfârșit (bara de ocupat rulează până atunci).
- Ajutorul K-BOT: nu se schimbă (KBotAdmin nu e pentru clienți).

## De făcut de operator

1. Împinge `PYTHON/` (Publicare în KBotAdmin) și repornește serviciul — fără asta pagina nouă nu găsește verbele pe server.
2. Republică `_Admin` cu `publish-admin.ps1`.
