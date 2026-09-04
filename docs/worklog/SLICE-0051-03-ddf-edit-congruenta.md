# SLICE-0051-03 — `DdfEditForm` aliniat la `OrdEditForm`

Rundă corectivă la felia 0051, cerută de operator în trei puncte: (1) **toate butoanele trebuie
să urmeze tema**; (2) **grilele secțiunilor A și B nu au voie să arate dezactivate** — un rând
care nu se editează rămâne un rând care se citește normal, nu unul stins; (3) restul
neconcordanțelor față de editorul de ordonanțare.

**Stare:** cod verde. `dotnet build src/KBot.App/KBot.App.vbproj` → **0 erori / 7 avertismente**,
toate `MSB3825` PREEXISTENTE pe `.resx`-uri (`*.ImageStream` prin `BinaryFormatter`); niciunul
nou. `dotnet test tests/KBot.App.Tests` → **194 trecute / 13 picate**, aceleași 13 înainte și
după (§4).

**NIMIC N-A FOST VĂZUT PE ECRAN.** `DdfEditForm` nu s-a deschis, nici în aplicație, nici în
designerul Visual Studio, nici prin `DrawToBitmap`. Ce e mai jos e citit din cod și din teste.

---

## 0. Ce s-a citit înainte de orice

`docs/worklog/CODE_WORKFLOW.md` · `docs/worklog/KBOT_STATUS.md` (rândurile 0051 și 0051-02) ·
`docs/worklog/SLICE-0051-02-combo-editabil.md` · `CLAUDE.md` · integral:
`src/KBot.App/Views/Ord/OrdEditForm.{vb,Designer.vb}`, `OrdAtasamentePage.vb`,
`OrdBeneficiariPage.Designer.vb`, `IOrdEditPage.vb` (referința) ·
`src/KBot.App/Views/Ddf/DdfEdit*.{vb,Designer.vb}`, `IDdfEditPage.vb` ·
`src/KBot.Theming/ButtonStyles.vb`, `ModernRenderer.vb`, `ThemeManager.vb` (regulile generice și
`Traverse`) · `src/KBot.Controls/DataView/KBotDataColumn.vb`, `KBotDataView.vb`,
`KBotDataView.Painting.vb` · `src/KBot.Controls/Combo/KBotComboBox.vb` ·
`src/KBot.Controls/RichText/KBotRichTextEditor.Theming.vb` ·
`tests/KBot.App.Tests/DdfEditGrileTests.vb`.

---

## 1. Butoanele

### Ce era

Cele două pagini cu butoane (`DdfEditSectiuneaAPage`, `DdfEditFisierePage`) și formularul însuși
își pictau butoanele cu mâna, dintr-o buclă care scria direct culorile din paletă:

```vb
btn.FlatStyle = FlatStyle.Flat
btn.BackColor = p.ButtonBackColor
...
```

Bucla asta **sare peste `ModernRenderer`**. Pe schema `Modern`, `ButtonStyles.ApplyPrimary` /
`ApplySecondary` cheamă `ModernRenderer.ApplyButton`, care pune un `Region` cu colțuri rotunjite
și umplutura schemei; bucla nu. Rezultatul, care e exact reclamația: butoanele DDF ieșeau
dreptunghiuri plate lângă butoanele rotunjite ale editorului de ordonanțare, și **niciunul nu
purta accentul** — «Adaugă» arăta la fel ca «Șterge».

### Ce e acum

| loc | buton | stil |
|---|---|---|
| `DdfEditSectiuneaAPage` | `btnAdauga` | `ButtonStyles.ApplyPrimary` |
| | `btnSterge` | `ApplySecondary` |
| `DdfEditFisierePage` | `btnAdauga` | `ApplyPrimary` |
| | `btnSterge`, `btnSalveazaPeDisc` | `ApplySecondary` |
| `DdfEditForm` (subsol) | `btnRenunta`, `btnSalveaza` | **nimic scris de mână** |

Aceeași repartiție ca în `OrdAtasamentePage` / `OrdDocumentePage`: acțiunea poartă accentul, iar
butonul distructiv rămâne secundar.

Subsolul formularului e cazul invers și de aceea nu se mai scrie nimic acolo. `btnRenunta` și
`btnSalveaza` stau DIRECT pe formular, nu într-un `IThemedControl`, deci
`MyBase.OnThemeChanged` → `ThemeManager` le-a dat deja regula generică de buton, `ModernRenderer`
inclus. Repictarea lor imediat după arunca rotunjirea. `OrdEditForm.OnThemeChanged` nu conține
nicio linie despre butoanele lui, tocmai din acest motiv; acum nici `DdfEditForm` nu conține.

Paginile sunt cazul celălalt și de aceea acolo se scrie: `ThemeManager.Traverse` **nu coboară cu
regulile generice** în copiii unui control care e el însuși `IThemedControl` (`ThemeManager.vb`,
liniile 304–321), iar paginile sunt `IThemedControl`. Motivul e scris la fiecare dintre cele două
locuri, ca să nu pară nici omisiune, nici duplicare.

## 2. Grilele nu mai arată dezactivate

`DdfEditSectiuneaAPage.Designer.vb` punea **`Enabled = False` pe TOATE cele zece coloane**.
`KBotDataColumn.Enabled` nu înseamnă «nu se editează», ci — după propria lui documentație —
«întreaga coloană e desenată ștearsă (gri) și inertă»: e chiar aspectul de control stins.
Zece coloane așa = o grilă întreagă stinsă.

Două urmări, nu una:

1. **Aspectul.** Exact ce a semnalat operatorul. Nicio grilă din `OrdEditForm` nu folosește
   `Enabled = False`; toate spun `ReadOnly = True`.
2. **Funcțional.** Cele cinci coloane pe care clasa le declară EDITABILE (clasificația,
   elementul de fundamentare, parametrii, partenerul, valoarea curentă) erau și ele inerte, deci
   deblocarea din `AplicaModulDeEditare` — pentru un DDF construit manual — nu putea debloca
   nimic. Rândurile 1005–1018 din `KBotDataColumn.vb`: `Enabled` bate `ReadOnly`, iar
   `KBotDataView.EsteCelulaActiva` iese pe `Not col.Enabled` înainte de orice altceva.

Toate cele zece linii au fost scoase. Refuzul editării rămâne unde îi e locul și unde era deja:

- pe coloană, `ReadOnly = True` pentru cele derivate (`clsf`, `buget`, `val_rec`, `val_prec`,
  `val_tot`) — neatins;
- pe grilă, `grd.ReadOnlyGrid` pentru un document generat din rezervări — neatins;
- pe celula `cod_partener`, poarta din `AplicaGateulPartenerului` — neatins, tot prin `ReadOnly`.

Niciunul dintre cele trei nu atinge pictura. Regula e scrisă acum în capul designerului, ca să nu
se întoarcă.

`DdfEditSectiuneaBPage` **nu avea** `Enabled = False` pe nicio coloană — verificat, nu presupus.
Ce avea în schimb e la §3.

## 3. Restul neconcordanțelor

### 3.1 `cmbComp` — compartimentul tastat nu ajungea nicăieri (funcțional, blocant)

Felia 0051-02 a făcut combo-ul tastabil (`Editable = True`, `LimitToList = False`) și a lăsat
scris la «rămas»: perechea `txtComp` + `cmbComp` din `DdfEditForm` așteaptă strângerea.
Între timp `txtComp` a dispărut din designer, dar **codul care lega valoarea nu s-a mutat pe
combo** — au rămas doar cinci linii comentate. Consecințele, toate în același sens:

- `_draft.Comp` se scria **numai** din `CmbComp_SelectedIndexChanged`, adică numai la alegerea
  din listă. Un compartiment TASTAT nu ajungea niciodată în schiță, iar `MotiveDeRefuz` refuza
  salvarea cu «Compartimentul lipsește» pentru un câmp pe care operatorul îl vedea completat.
- Când lista venea goală de pe server — starea NORMALĂ pe o unitate fără documente anterioare,
  fiindcă nu există nomenclator de compartimente în MariaDB — `IncarcaListeleAsync` făcea
  `cmbComp.Enabled = False`, adică stingea singura cale de intrare rămasă. Pe o bază proaspătă
  documentul nu se putea salva deloc. Același lucru pe ramura `Catch`.
- La încărcare, `cmbComp.SelectedItem = _draft.Comp`: pentru o valoare care nu e în listă — cazul
  frecvent — atribuirea e un no-op tăcut, deci compartimentul documentului dispărea din antet.
- Sfatul de pe ecran spunea «Alegerea unuia îl scrie în caseta din stânga» și «Scrie
  compartimentul în caseta din stânga». Nu mai există nicio casetă în stânga.

Acum: un `PreiaCompartimentul()` cheamă `cmbComp.CommitText()` (idempotent, exact pentru asta a
fost scris în 0051-02) și pune `cmbComp.Text.Trim()` în schiță. Se cheamă din `cmbComp.Leave` și
din nou din `BtnSalveaza_Click`, fiindcă operatorul poate apăsa butonul fără să iasă din câmp.
`SelectedIndexChanged` rămâne, ca alegerea din listă să se vadă în schiță pe loc. Câmpul **rămâne
aprins** când lista e goală sau când cererea eșuează — se schimbă doar sfatul. Reumplerea listei
golește caseta, deci textul curent se pune înapoi, sub `_seIncarca`.

Cele cinci linii comentate `txtComp` și nota din capul designerului care descria perechea au fost
șterse; nota descrie acum un singur control.

### 3.2 `DdfEditFisierePage` construia coloanele în cod

Un `ConstruiesteColoanele()` chemat din constructor — singurul loc din ambele editoare care face
așa. Regula casei (`docs/kbot-forms-ui-convention.md`, și §4.5 din worklogul 0051, care mutase
deja celelalte trei grile) cere declararea în `.Designer.vb`; altfel grila e goală la design time.
Cele patru coloane sunt acum în designer, cu font, aliniere, `ReadOnly` și coloană de umplere
(`cale_fisier`), la fel ca grilele din `OrdAtasamentePage`.

### 3.3 `DdfEditSectiuneaBPage` era îmbrăcată altfel decât sora ei

Aceeași formă, un clic de navigație distanță, același document — și patru diferențe:

| | secțiunea A (și `OrdEditForm`) | secțiunea B, înainte |
|---|---|---|
| `AutoScaleDimensions` | `10F, 25F` | `7F, 15F` ⇒ pagina ieșea scalată cu ≈1,43 față de surorile ei |
| font coloană / antet | Calibri 9 / Calibri 9 bold | nesetat |
| format bani | `Standard` (separatori de mii) | `Fixed` (fără) |
| bandă subsol, chenare, umplere | `FooterFont`, `Footer/HeaderBackColor`, separatori, `FillColumnKey` | nimic |

Toate aliniate pe secțiunea A. `KBotFormat.Standard` e ce folosesc **toate** cele opt coloane de
bani din `OrdEditForm` — verificat, nu presupus.

Componenta `tips` era declarată și nefolosită pe pagina B; are acum sfatul de pe grilă (ce e
acolo se recalculează din secțiunea A la fiecare modificare).

### 3.4 `edtLunga.BackColor = Color.White`

Culoare scrisă de mână în designer, ceea ce regula casei interzice — dar aici e mai rău decât o
încălcare de stil: `KBotRichTextEditor.BackColor` are un setter care aprinde `_backColorPinned`,
iar `ShouldSerializeBackColor` întoarce apoi True. Linia **pironea** fundalul editorului pe alb
pentru totdeauna, deci suprafața rămânea albă și pe schema întunecată. Linia a fost ștearsă;
controlul își ia din nou fundalul din temă.

### 3.5 Un test care pica

`DdfEditGrileTests.SectiuneaA_AreColoaneleDinDesigner` cerea `KBotFormat.Fixed` pe coloanele de
bani, iar designerul scria `Standard`. **Testul pica pe ramură înainte de rundă asta.** Cum
`Standard` e ce folosesc `OrdEditForm` și secțiunea A, testul a fost corectat la `Standard`, nu
invers.

---

## 4. Ce s-a rulat și cu ce rezultat

```
dotnet build src\KBot.App\KBot.App.vbproj      → 0 erori / 7 avertismente (MSB3825, preexistente)
dotnet test  tests\KBot.App.Tests              → 194 trecute / 13 picate / 207 total
```

`DdfEditGrileTests` — **6 din 6 trecute**, inclusiv cele două care verifică blocarea secțiunii A
pentru un document din rezervări: `ReadOnlyGrid` și butoanele stinse rămân exact cum erau, doar
grila nu mai e desenată ștearsă.

**Cele 13 picate sunt PREEXISTENTE pe ramură și nu ating niciun fișier atins aici:**
`DdfXfaParserTests` (2), `XfaXmlPreviewTests` (1), `MainFormNavItemsTests` (1), `DdfViewTests` (3),
`IstoricViewTests` (6). Aceleași 13 s-au numărat înainte de prima modificare din runda asta.
Exemplu, `DdfViewTests.StaleResponse_IsDiscarded`: `Assert.Empty()` pe arborele lui `DdfView` —
garda de răspuns învechit din vederea DOAR-CITIRE, altă felie. **Nu s-au atins.**

## 5. Fișiere atinse

| fișier | ce |
|---|---|
| `Views/Ddf/DdfEditSectiuneaAPage.Designer.vb` | cele zece `Enabled = False` scoase; nota despre `ReadOnly` vs `Enabled` |
| `Views/Ddf/DdfEditSectiuneaAPage.vb` | `ButtonStyles.ApplyPrimary/ApplySecondary` în locul buclei |
| `Views/Ddf/DdfEditSectiuneaBPage.Designer.vb` | `AutoScaleDimensions`, fonturi, `Standard`, bandă/chenare/umplere, sfat pe grilă |
| `Views/Ddf/DdfEditFisierePage.Designer.vb` | cele patru coloane, mutate din cod |
| `Views/Ddf/DdfEditFisierePage.vb` | `ConstruiesteColoanele` scoasă; `ButtonStyles` |
| `Views/Ddf/DdfEditDescrierePage.Designer.vb` | `edtLunga.BackColor = Color.White` scoasă |
| `Views/Ddf/DdfEditForm.Designer.vb` | nota despre compartiment rescrisă; sfatul lui `cmbComp` |
| `Views/Ddf/DdfEditForm.vb` | `PreiaCompartimentul`, `CmbComp_Leave`, `Text` în loc de `SelectedItem`, combo-ul rămâne aprins pe listă goală, liniile `txtComp` comentate șterse, bucla de butoane din `OnThemeChanged` scoasă |
| `tests/KBot.App.Tests/DdfEditGrileTests.vb` | `Fixed` ▸ `Standard` |
| `docs/worklog/SLICE-0051-03-ddf-edit-congruenta.md` | fișierul ăsta |
| `docs/worklog/KBOT_STATUS.md` | rândul feliei |

Nicio versiune de proiect n-a fost urcată: `KBot.App` n-are `FileVersion` propriu de urcat în
runda asta (doar `Views/`), iar `KBot.Controls` și `KBot.Theming` **nu s-au atins**.

## 6. Rămas neverificat / amânat

- **`DdfEditForm` tot n-a fost văzut pe ecran** — nici în aplicație, nici în designerul VS, nici
  prin `DrawToBitmap`. Toate afirmațiile despre aspect (butoane rotunjite pe `Modern`, grilă
  nestinsă, secțiunea B la aceeași scară cu A) sunt citite din cod, **nu văzute**. Asta e
  verificarea care lipsește și e cea care contează cel mai mult pentru o rundă despre aspect.
- **Nimic n-a rulat pe MariaDB sau pe serverul Python** — nici de data asta. Calea corectată a
  compartimentului (tastare ⇒ `_draft.Comp` ⇒ POST) **n-a fost probată cu un server adevărat**;
  e citită din cod și din contractul lui `KBotComboBox.CommitText` descris în 0051-02.
- **Zero teste noi.** Ce s-a schimbat aici e în cea mai mare parte designer și pictură; singurul
  lucru testabil ieftin — compartimentul tastat — cere un `DdfEditForm` construit cu un
  `IApiClient` fals și un `DdfEditReauth`, adică o schelă pe care felia n-o are. **Merită scrisă**
  și e cel mai bun candidat pentru runda următoare.
- **Cele 13 teste picate de pe ramură rămân picate.** Sunt din alte felii (`DdfView`,
  `IstoricView`, `XfaXmlPreview`, parserul XFA, iconițele din `MainForm`) și n-au fost atinse aici,
  ca să nu se amestece o rundă de aspect cu remedieri fără legătură.
- **`AplicaModulDeEditare` lasă `lblStare` cu mesajul de blocare** dacă aceeași instanță de pagină
  primește pe rând un document blocat și apoi unul liber: mesajul se rescrie abia la
  `ReimprospateazaCombo`. Astăzi formularul e modal și pornește o pagină nouă de fiecare dată,
  deci nu se vede. Consemnat, nu remediat.
- **`Dim _salariiArFiFostEditabil`** din `AplicaEnablement` rămâne o variabilă scrisă și necitită,
  ținută intenționat de felia 0051 ca urmă a regulii din Access. Lăsată cum era.
