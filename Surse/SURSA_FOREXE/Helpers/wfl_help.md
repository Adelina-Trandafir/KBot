# ForexeBot — Help inline editor
<!-- 
  FORMAT: Fiecare secțiune începe cu "## NumeTag" (H2 exact).
  Parserul VB.NET din editor citește acest fișier o dată la startup
  și construiește un Dictionary(Of String, String) tag → conținut.
  Vezi snippet-ul de parsing la finalul acestui fișier.
-->

---

## AuthClick

Variantă specială de Click pentru link-uri care declanșează autentificarea Windows cu certificat digital (token). Dă click pe element, așteaptă dialogul Windows Security, confirmarea utilizatorului și finalizarea login-ului.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul pe care se dă click pentru a iniția autentificarea |
| `authTimeout` | — | `120` | Secunde de așteptat pentru confirmarea certificatului |
| `ExpectedUrlAfterAuth` | — | — | Dacă e furnizat, așteaptă ca URL-ul să conțină acest șir după login |
| `waitNavigation` | — | `false` | Așteaptă navigarea paginii după autentificare |
| `timeout` | — | `30` | Secunde de așteptat ca elementul să fie disponibil |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<AuthClick
  selector="a[href*='forexe.mfinante'] >> visible=true"
  authTimeout="120"
  waitNavigation="true"
  ExpectedUrlAfterAuth="https://forexe.mfinante.gov.ro/"
  LogValue="Aștept autentificarea cu certificat..."
/>
```

---

## Click

Simulează un click de mouse pe elementul identificat prin selector. Funcționează pentru butoane, link-uri, tab-uri, checkbox-uri și orice element clicabil.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul pe care se dă click |
| `waitNavigation` | — | `false` | `true` dacă click-ul navighează la o pagină nouă |
| `expectNewTab` | — | `false` | `true` dacă se deschide un tab nou; robotul trece automat pe el |
| `force` | — | `false` | `true` pentru a forța click-ul chiar dacă elementul pare acoperit |
| `jsClick` | — | `false` | `true` pentru click via JavaScript (util pentru elemente non-standard) |
| `timeout` | — | `30` | Secunde de așteptat ca elementul să fie disponibil |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Click selector="button:has-text('Caută')" timeout="10"/>

<Click selector="a:has-text('Listă angajamente')"
       waitNavigation="true" LogValue="Deschid lista"/>

<Click selector="a[href*='CABWeb']"
       waitNavigation="true" expectNewTab="true" isCheckpoint="true"/>
```

---

## Debug

Instrument de dezvoltare: evidențiază elementul în browser și înregistrează detalii tehnice în consolă. **Nu folosiți în producție.**

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul de inspectat și evidențiat |
| `haltWhenDone` | — | `false` | `true` → face pauză după debug, așteaptă click pe Next |
| `timeout` | — | `5` | Secunde de așteptat ca elementul să existe |
| `isCheckpoint` | — | `true` | Implicit checkpoint |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Debug selector="button:has-text('Salvează')" haltWhenDone="true"/>
```

---

## Download

Dă click pe un buton de descărcare și captează fișierul. Opțional, dacă fișierul este Excel (.xlsx), îl trimite la serviciul Python API pentru conversie în JSON (ca la ScrapeTable).

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Butonul sau link-ul care declanșează descărcarea |
| `saveFolder` | ✅¹ | — | ¹ Obligatoriu unul din grup: `saveFolder` SAU `saveTo` |
| `saveTo` | ✅¹ | — | ¹ Numele variabilei interne unde se salvează JSON-ul rezultat |
| `fileName` | — | *(auto)* | Suprascrie numele fișierului descărcat |
| `openFile` | — | `false` | `true` → deschide fișierul după descărcare |
| `parseExcel` | — | `false` | `true` → parsează Excel-ul în JSON (necesită API Python activ) |
| `headerRows` | — | `1` | Câte rânduri de sus formează antetul (folosit cu `parseExcel`) |
| `skipFirstNRows` | — | `0` | Rânduri de sărit după antet |
| `skipLastNRows` | — | `0` | Rânduri de sărit de la sfârșit |
| `skipFirstNColumns` | — | `0` | Coloane de sărit de la stânga |
| `skipLastNColumns` | — | `0` | Coloane de sărit de la dreapta |
| `filterColumn` | — | — | Filtru simplu: numele coloanei (format igienizat cu `_`) |
| `filter` | — | — | Valoarea / regex de potrivit față de `filterColumn` |
| `complexFilter` | — | — | Ex: `"Col1 LIKE '2024' AND Col2 = 'activ'"`. Operatori: `=` `!=` `>` `<` `>=` `<=` `LIKE` `REGEX` + `AND` |
| `timeout` | — | `60` | Secunde de așteptat ca descărcarea să înceapă |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Download
  selector="button[name='download']"
  parseExcel="true"
  saveTo="AngajamenteActive"
  headerRows="3"
  complexFilter="Data_creare LIKE '{{AN_DATE}}' AND Stare = 'activ'"
  LogValue="Descarc și parsez Excel-ul"
/>
```

---

## Exit

Oprește imediat execuția workflow-ului — stop curat, fără eroare. Folosit când o condiție face inutilă continuarea.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `message` | — | `"Execuție anulată manual"` | Mesaj scris în log la ieșire |
| `isCheckpoint` | — | `true` | Implicit checkpoint |

```xml
<Exit message="Nu s-au găsit rezultate. Opresc."/>
```

---

## ExtractXmlFromPdf

Scanează un folder de PDF-uri de extrase de cont ForexeSNM, filtrează după data din numele fișierului, extrage XML-ul embedded din fiecare PDF și returnează un JArray cu câte un obiect per fișier.

**Format nume fișier așteptat:** `..._SIGNED_DDMMYYYYhHHMM.pdf`
ex: `TREZ521_ExtrasEP_PDFCLI_2845508_XML_SIGNED_07042026h1614.pdf`

**Structura JArray returnat:**
```json
[
  {
    "PdfFisier":  "TREZ521_..._07042026h1614.pdf",
    "DataFisier": "07.04.2026 16:14:00",
    "XmlContent": "<?xml version=\"1.0\"?>..."
  }
]
```
`XmlContent` conține XML-ul brut extras din PDF — poate fi deschis direct cu `MSXML6.0` în Access.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `folder` | ✅ | — | Calea folderului cu PDF-urile de procesat. Suportă `{{variabile}}` |
| `saveTo` | ✅ | — | Numele variabilei în care se salvează JArray-ul rezultat |
| `dataDeLa` | — | — | Procesează doar PDF-urile cu data din nume `>=` această valoare. Format: `dd.MM.yyyy HH:mm:ss` sau `dd.MM.yyyy HH:mm` sau `dd.MM.yyyy` |
| `timeout` | — | `60` | Secunde maxim pentru întreaga operațiune |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

**Comportament la erori:**
- PDF fără dată în nume → **sărit** cu warning în log
- PDF cu dată înainte de `dataDeLa` → **sărit** cu info în log
- PDF fără XML valid embedded → **sărit** cu warning în log
- Niciuna din situațiile de mai sus nu oprește execuția
- Folderul inexistent → **eroare critică**, oprește workflow-ul

```xml
<!-- Prima rulare: toate PDF-urile din folder -->
<ExtractXmlFromPdf folder="C:\Downloads\Extrase"
                   saveTo="ExtraseDateResult"
                   LogValue="Extrag XML din toate PDF-urile"/>

<!-- Rulări ulterioare: doar PDF-urile noi -->
<ExtractXmlFromPdf folder="C:\Downloads\Extrase"
                   dataDeLa="{{DataUltimaDescarcare}}"
                   saveTo="ExtraseDateResult"
                   LogValue="Extrag XML din PDF-urile noi (>= {{DataUltimaDescarcare}})"/>

<!-- Cu timeout extins pentru loturi mari -->
<ExtractXmlFromPdf folder="{{FolderExtrase}}"
                   dataDeLa="{{DataDeLa}}"
                   saveTo="ExtraseDateResult"
                   timeout="120"
                   isCheckpoint="true"
                   LogValue="Extrag XML extrase"/>
```

---

## Fill

Completează un câmp de text, textarea sau input cu o valoare specificată. Implicit șterge conținutul anterior.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Câmpul de completat |
| `value` | ✅ | — | Textul de tastat. Acceptă `{{VAR}}` și `[[VAR]]` |
| `clear` | — | `true` | `false` → adaugă la conținutul existent |
| `pickFromList` | — | `false` | `true` → folosit cu `<Repeat>`: ia pe rând elementele dintr-un `value` separat prin `~` |
| `sequential` | — | `false` | `true` → tastează caracter cu caracter (`PressSequentially`); util pentru select2 și alte input-uri JS-heavy |
| `timeout` | — | `30` | Secunde de așteptat ca câmpul să fie pregătit |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Fill selector="input[name='cod']" value="{{COD_ANGAJAMENT}}" clear="true"/>

<!-- pickFromList: dacă LISTA = "01.01~01.02~01.03", fiecare Repeat ia câte unul -->
<Fill selector="#select2-drop input" value="{{LISTA}}" pickFromList="true"/>
```

---

## FindInTable

Caută primul rând dintr-un tabel HTML unde o coloană specifică conține valoarea dată. Poate da click pe un element din rândul găsit și/sau executa acțiuni copil.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul `<table>` în care se caută |
| `fieldName` | ✅ | — | Textul antetului coloanei în care se caută (case-insensitive) |
| `value` | ✅ | — | Valoarea de căutat. Acceptă `{{VAR}}` |
| `fieldTransform` | — | — | Transformări aplicate înainte de comparare (pe ambele valori). Înlănțuiește cu `\|`: `trim`, `lower`, `upper`, `stripNonAlphaNum`, `left:N`, `right:N`, `regex:/pattern/flags:` |
| `saveRowTo` | — | — | Salvează rândul găsit ca JSON în această variabilă internă |
| `clickSelector` | — | — | Selector relativ la rândul găsit — dacă e dat, dă click pe el |
| `timeout` | — | `30` | Secunde de așteptat ca tabelul să fie pregătit |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Acceptă acțiuni copil care rulează în contextul rândului găsit.

```xml
<FindInTable
  selector="table.table-striped"
  fieldName="Cod Angajament"
  value="{{COD_ANGAJAMENT}}"
  fieldTransform="trim|stripNonAlphaNum"
  clickSelector=".glyphicon-list"
  LogValue="Caut contractul în tabel">

  <Log message="Rând găsit!" level="success"/>
</FindInTable>
```

---

## ForEach

Iterează peste un set de elemente de pe pagină (ex: rânduri de tabel). Pentru fiecare element, dă click pe un sub-element din interior, apoi execută acțiunile copil.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Grupul de elemente de iterat. Ex: `table tbody tr` |
| `clickElement` | — | — | Sub-selector în fiecare element — ce se dă click pentru a-l „deschide". Adaugă `:Visible` pentru a sări elementele ascunse |
| `indexVariable` | — | — | Numele contorului. Accesibil ca `[[indexVariable]]` în acțiunile copil |
| `timeout` | — | `30` | Secunde de așteptat fiecare element |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil între tagurile de deschidere și închidere.

```xml
<ForEach selector="table tbody tr"
         clickElement=".glyphicon-eye-open"
         indexVariable="recIdx">

  <Read selector="select[name='receptie']" saveTo="Rec_[[recIdx]]"/>
  <GoBack/>

</ForEach>
```

---

## ForEachVar

Iterează peste un array JSON stocat într-o variabilă internă (de obicei produs de `ScrapeTable` sau `Download`). Expune câmpurile fiecărui obiect ca variabile cu prefix ales. Opțional, colectează automat variabilele setate în timpul fiecărei iterații într-un array și un map de lookup.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `source` | ✅ | — | Numele variabilei care conține array-ul JSON. Scrie fără `[[]]`: `source="DATE_RECEPTIE"` |
| `itemPrefix` | ✅ | `ITEM` | Prefixul câmpurilor expuse. Ex: prefix `R`, câmp `Cod` → `[[R.Cod]]` |
| `indexVariable` | — | — | Numele contorului (1-based). Accesibil ca `[[indexVariable]]` |
| `collectKey` | — | — | Câmpul din JSON sursă folosit ca cheie de colectare (ex: `"Cheie"`). Când e setat, activează colectarea automată — vezi secțiunea **Colectare** |
| `collectFields` | — | — | Listă de câmpuri separate prin virgulă de colectat la fiecare iterație (ex: `"Cod,Valoare,Poza"`). Alternativă mai explicită față de `collectKey` |
| `useMap` | — | `false` | `true` → generează și `[[{source}_results_map]]` — obiect indexat după prima valoare din `collectFields` / `collectKey`, pentru lookup direct |
| `timeout` | — | `30` | Timeout în secunde |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil între tagurile de deschidere și închidere.

### Variabile de sistem expuse per iterație

| Variabilă | Descriere |
|---|---|
| `[[PREFIX.NumeCamp]]` | Valoarea câmpului `NumeCamp` din obiectul curent |
| `[[PREFIX.IsLast]]` | `"true"` la ultima iterație, `"false"` la celelalte — util pentru ramificarea logicii de salvare |
| `[[indexVariable]]` | Contorul iterației (1-based), dacă `indexVariable` e setat |

### Colectare automată (`collectKey`)

Când `collectKey` e setat, după fiecare iterație executorul caută în `_variables` toate variabilele al căror **nume conține valoarea cheii curente** (ex: `"IdSecA-137"`), le stripuiește de sufix și le adaugă ca obiect într-o colecție. La finalul loop-ului, două variabile noi sunt disponibile automat:

| Variabilă generată | Tip | Conținut |
|---|---|---|
| `[[{source}_results]]` | JSON array | Un obiect per iterație cu câmpurile colectate (+ `_key`) |
| `[[{source}_results_map]]` | JSON object | Același conținut, indexat după valoarea cheii — lookup direct |

Variabilele care **nu conțin** cheia în nume (ex: `Poza_Final`, `TabelIndicatori`) sunt excluse automat din colecție.

```xml
<ScrapeTable selector="table.table-striped" saveTo="Lista"/>

<!-- Fără colectare: iterare simplă -->
<ForEachVar source="Lista" itemPrefix="R" indexVariable="idx">
  <Log message="Procesez rândul [[idx]]: [[R.Cod_Angajament]]" level="info"/>
  <Fill selector="input[name='cod']" value="[[R.Cod_Angajament]]"/>
</ForEachVar>
```

```xml
<!-- Cu colectare: salvează variabilele setate per iterație în DATE_RECEPTIE_results -->
<ForEachVar source="DATE_RECEPTIE" itemPrefix="R" indexVariable="idx" collectKey="Cheie">

  <!-- Acțiuni per rând... -->
  <Read selector=".well h4 span:nth-child(2)" saveTo="CodAng_[[R.Cheie]]"/>
  <Screenshot saveTo="Poza_[[R.Cheie]]"/>

  <!-- Ramificare pe IsLast -->
  <IfVar Value="[[R.IsLast]]" compare="eq:true">
    <Click selector="button.btn-success:nth-child(1)" waitNavigation="true"/>
    <Else>
      <Click selector="button.btn-success:nth-child(2)" waitNavigation="false"/>
    </Else>
  </IfVar>

</ForEachVar>

<!-- Disponibile după loop: -->
<!-- [[DATE_RECEPTIE_results]]     → [{"_key":"IdSecA-137","CodAng":"AAB3...","Poza":"..."}, ...] -->
<!-- [[DATE_RECEPTIE_results_map]] → {"IdSecA-137": {"_key":..., "CodAng":..., "Poza":...}, ...} -->
```

---

## GetAttribute

Citește valoarea unui **atribut HTML** al unui element (nu textul vizibil, ci ex: `title`, `href`, `value`, `data-id`) și o poate afișa ca mesaj de eroare sau informativ.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul de inspectat |
| `attributeName` | ✅ | — | Numele atributului HTML de citit: `title`, `href`, `class`, `value`, etc. |
| `saveTo` | — | — | Variabilă internă `[[VAR]]` în care se salvează valoarea atributului citit |
| `showErrorMessage` | — | `false` | `true` → afișează valoarea ca dialog de eroare pop-up |
| `showNormalMessage` | — | `false` | `true` → afișează valoarea ca dialog informativ pop-up |
| `timeout` | — | `5` | Secunde de așteptat ca elementul să existe |
| `isCheckpoint` | — | `true` | Implicit checkpoint |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<!-- Citește mesajul de eroare din tooltip și îl afișează ca dialog -->
<IfExists selector=".feedbackPanelERROR span[title]" timeout="5">
  <GetAttribute selector=".feedbackPanelERROR span[title]"
                attributeName="title"
                saveTo="MesajEroare"
                showErrorMessage="true"/>
  <Exit message="Eroare la introducerea datelor."/>
</IfExists>
```

---

## GoBack

Navighează înapoi la pagina anterioară din istoricul browser-ului. Echivalent cu butonul Back / tasta Alt+←.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `timeout` | — | `30` | Secunde de așteptat ca pagina anterioară să se încarce |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<GoBack/>
<WaitFor selector="h3:has-text('Listă')" timeout="5"/>
```

---

## IfExists

Execută acțiunile copil dacă elementul specificat **există** pe pagină. Opțional, blocul `<Else>` se execută dacă elementul nu există.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul de căutat |
| `strict` | — | `false` | `true` → Playwright aruncă eroare dacă selectorul găsește mai mult de un element (comportament strict); `false` → folosește automat `.First` |
| `timeout` | — | `5` | Secunde de așteptat înainte de a considera că nu există |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil. Suportă `<Else>` opțional.

```xml
<IfExists selector="#select2-drop-mask">
  <Click selector="#select2-drop-mask" force="true"/>
</IfExists>

<IfExists selector=".alert-success" timeout="5">
  <Log message="Salvat cu succes." level="success"/>
  <Else>
    <Log message="Salvarea a eșuat!" level="error"/>
    <Exit/>
  </Else>
</IfExists>
```

---

## IfUnique

Execută acțiunile copil doar dacă **exact un singur** element se potrivește cu selectorul. Util după o căutare pentru a confirma un singur rezultat.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Selectorul care trebuie să se potrivească exact o dată |
| `onlyIfVisible` | — | `true` | `true` → numără doar elementele vizibile |
| `timeout` | — | `5` | Secunde de așteptat |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil. Suportă `<Else>` opțional.

```xml
<IfUnique selector="table tbody tr" timeout="5">
  <Click selector="tbody tr:first-child .haction" force="true"/>
  <Else>
    <Log message="Nu e un singur rezultat!" level="error"/>
    <Exit/>
  </Else>
</IfUnique>
```

---

## IfVar
Execută acțiunile copil doar dacă variabila specificată **are o valoare** (nu e goală și nu e substituent nerezolvat). Modalitatea standard de a face secțiuni de workflow opționale.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `value` | ✅ | — | Variabila de testat, scrisă ca `{{NUME_VAR}}` sau `[[NUME_VAR]]` |
| `compare` | — | *(lipsă)* | Dacă lipsește → verifică doar că valoarea e non-goală. Vezi tabelul operatorilor de mai jos. |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

### Operatori `compare`

| Sintaxă | Alias | Descriere |
|---|---|---|
| *(lipsă)* | — | Valoarea e non-goală și rezolvată (comportament implicit) |
| `=:VALOARE` | `eq:VALOARE` | Egal (numeric sau text) |
| `!=:VALOARE` | `neq:VALOARE` | Diferit |
| `gt:VALOARE` | `>:VALOARE`* | Mai mare (numeric) |
| `lt:VALOARE` | `<:VALOARE`* | Mai mic (numeric) |
| `gte:VALOARE` | `>=:VALOARE`* | Mai mare sau egal (numeric) |
| `lte:VALOARE` | `<=:VALOARE`* | Mai mic sau egal (numeric) |
| `regex:PATTERN` | `~:PATTERN` | Potrivire regex. Suportă flags opționale: `regex:PATTERN:FLAGS` |

> ⚠️ *Aliasurile cu `<` și `>` necesită escape în XML: `&lt;` și `&gt;`. Preferă forma text (`gt`, `lt` etc.) pentru lizibilitate.

> Necesită acțiuni copil. Suportă `<Else>` opțional.

```xml
<!-- Fara flags (case-sensitive implicit) -->
<IfVar value="{{Clasificatia}}" compare="regex:^65\.">

<!-- Cu flag i = case-insensitive -->
<IfVar value="{{Stare}}" compare="regex:^activ$:i">

<!-- Cu flags multiple -->
<IfVar value="{{Descriere}}" compare="regex:proba \d+:im">
```

---

## Include

Încarcă și execută un alt fișier `.wfl` la acest punct. Toate variabilele din workflow-ul curent sunt disponibile în fișierul inclus. Util pentru rutine comune (ex: login) partajate între mai multe workflow-uri.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `workflowPath` | ✅ | — | Calea completă spre fișierul `.wfl`. Ex: `C:\AVACONT\Workflows\Conectare.wfl` |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Include workflowPath="C:\AVACONT\Workflows\adlop_-_Conectare.wfl"/>
<Click selector="a:has-text('Listă angajamente')" waitNavigation="true"/>
```

---

## Log

Scrie un mesaj colorat în panoul de log al aplicației. Folosiți pentru a marca progresul și a face log-urile lizibile.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `message` | ✅ | — | Textul de afișat. Acceptă `{{VAR}}` și `[[VAR]]` |
| `level` | — | `info` | `info` (negru) · `action` (albastru) · `success` (verde) · `warning` (portocaliu) · `error` (roșu) |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |

```xml
<Log message="Pornesc extragerea..." level="info"/>
<Log message="✓ Gata. Rânduri procesate: [[idx]]" level="success"/>
<Log message="Element negăsit — verific." level="warning"/>
<Log message="EROARE: pagină neașteptată." level="error"/>
```

---

## Minimize

Minimizează fereastra browser-ului. Folosit după autentificare pentru a muta browser-ul în fundal și a continua automat.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `timeout` | — | `30` | Timeout în secunde |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Minimize LogValue="Mut browser-ul în fundal"/>
```

---

## Read

Citește textul vizibil sau valoarea curentă a unui element și o salvează într-o variabilă internă sau o transmite prin LogValue.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul de citit |
| `saveTo` | ✅¹ | — | ¹ Obligatoriu unul din grup: `saveTo` SAU `LogValue` |
| `LogValue` | ✅¹ | — | ¹ Dacă folosești LogValue în loc de saveTo, valoarea se afișează direct în log |
| `timeout` | — | `30` | Secunde de așteptat ca elementul să fie pregătit |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |

```xml
<Read selector="select[name='receptie']" saveTo="Receptie_[[idx]]"/>
<Read selector=".well.well-small" saveTo="CodAngajament"/>

<!-- Sau direct în log, fără variabilă: -->
<Read selector="h3.title" LogValue="Titlu pagină curentă"/>
```

---

## Reload

Reîncarcă pagina curentă din browser. Echivalent cu F5.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `waitNavigation` | — | `true` | Dacă să aștepte reîncărcarea completă înainte de a continua |
| `timeout` | — | `30` | Secunde de așteptat |

```xml
<Reload waitNavigation="true"/>
```

---

## Repeat

Execută acțiunile copil de un număr fix de ori. Contorul curent e disponibil ca `[[RepeatIndex]]` (1-based).

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `iterations` | ✅ | — | Numărul de repetări. Poate fi `{{VAR}}` |
| `indexVariable` | — | — | Nume alternativ pentru contor (pe lângă `[[RepeatIndex]]` care e mereu disponibil) |
| `timeout` | — | `30` | Timeout în secunde |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil. `[[RepeatIndex]]` e disponibil automat (pornind de la 1).

```xml
<!-- Dacă LISTA = "01.01~01.02~01.03", fiecare iterație ia câte un element -->
<Repeat iterations="{{NUMAR_REPETARI}}" LogValue="Procesez lista">
  <Fill selector="#select2-drop input"
        value="{{LISTA}}" pickFromList="true"/>
  <Click selector="#select2-drop .select2-match >> nth=0"/>
  <Download selector="a:has-text('Export')" parseExcel="true"
            saveTo="Result_[[RepeatIndex]]" headerRows="3"/>
</Repeat>
```

---

## ScrapeTable

Extrage toate rândurile dintr-un tabel HTML și le salvează ca array JSON.
Detectează automat anteturile. Suportă paginare automată și condiții de oprire per rând.

> **Numele coloanelor** în JSON sunt igienizate: spații → `_`, accente eliminate, anteturi imbricate (colspan) unite cu `!`.
> Ex: `Prevedere_bugetara!Credit_bugetar!An_curent`

---

Atribute

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul `<table>` de extras |
| `saveTo` | ✅ | — | Variabila internă în care se salvează JSON array-ul rezultat |
| `saveToFile` | — | — | Cale fișier unde se scrie JSON-ul pe disc (opțional, suplimentar față de `saveTo`) |
| `page` | — | — | Navighează la pagina specificată înainte de extragere: `first` · `last` · număr întreg (necesită `firstPageSelector` / `nextPageSelector`) |
| `row` | — | — | Extrage doar un singur rând și îl salvează ca array cu un element: `first` · `last` · număr întreg (1-based) |
| `nextPageSelector` | — | — | Butonul „Pagina următoare" pentru paginare forward |
| `prevPageSelector` | — | — | Butonul „Pagina anterioară" pentru paginare reverse (folosit cu `startFromLast`) |
| `firstPageSelector` | — | — | Butonul „Prima pagină" — necesar pentru `page=first` sau `page=N` |
| `lastPageSelector` | — | — | Butonul „Ultima pagină" — folosit cu `startFromLast` pentru a sări direct la capăt înainte de extragere |
| `waitSelector` | — | — | Selector de așteptat după click pe Next/Prev (confirmă că datele noi s-au încărcat) |
| `fingerprintSelector` | — | — | Override pentru detectarea schimbării paginii: selector a cărui valoare se verifică să se fi schimbat. Implicit: ultima celulă din ultimul rând al tabelului |
| `startFromLast` | — | `false` | Începe extragerea de la ultima pagină spre prima (necesită `lastPageSelector` + `prevPageSelector`) |
| `skipFirstNRows` | — | `0` | Rânduri de sărit de la începutul body-ului tabelului |
| `skipLastNRows` | — | `0` | Rânduri de sărit de la sfârșitul body-ului |
| `skipFirstNColumns` | — | `0` | Coloane de sărit de la stânga |
| `skipLastNColumns` | — | `0` | Coloane de sărit de la dreapta |
| `exitIfCellEquals` | — | — | Oprire per rând pe valoare exactă sau regex — vezi sintaxa mai jos |
| `exitIfCellDate` | — | — | Oprire per rând pe comparație de dată/timp — vezi sintaxa mai jos |
| `timeout` | — | `30` | Secunde de așteptat ca tabelul să apară |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

---

exitIfCellEquals

Oprește scraping-ul imediat ce un rând conține în coloana specificată valoarea dată.
Rândul care declanșează condiția **este inclus** în rezultat.

**Sintaxă:**

```
exitIfCellEquals="NumeColoana:Valoare"          → egalitate exactă (case-insensitive)
exitIfCellEquals="NumeColoana:~:Pattern"         → regex
exitIfCellEquals="NumeColoana:~:Pattern:i"       → regex cu flag (i = ignore case, m = multiline, s = singleline)
```

> `NumeColoana` trebuie scris exact cum apare în JSON după igienizare (diacritice eliminate, spații → `_`).

**Exemple:**

```xml
<!-- Oprește dacă coloana Stare = "Anulat" -->
<ScrapeTable selector="table#t" saveTo="rows"
             nextPageSelector="a.next"
             exitIfCellEquals="Stare:Anulat" />

<!-- Oprește dacă suma e zero (0,00 sau 0.00) -->
<ScrapeTable selector="table#t" saveTo="rows"
             nextPageSelector="a.next"
             exitIfCellEquals="Suma:~:^0[,.]00$" />
```

---

exitIfCellDate

Oprește scraping-ul imediat ce un rând conține în coloana specificată o dată care satisface operatorul față de valoarea de referință.
Rândul care declanșează condiția **este inclus** în rezultat.

Comparația se face prin conversie la `OADate` (Double) — echivalent `CDbl()` din VBA/Access — **indiferent de locale-ul sistemului**.

**Sintaxă:**

```
exitIfCellDate="NumeColoana:operator:Valoare"
```

| Operator | Semnificație |
|---|---|
| `eq` | egal |
| `neq` | diferit |
| `lt` | mai mic (data din pagină e mai veche) |
| `lte` | mai mic sau egal |
| `gt` | mai mare (data din pagină e mai nouă) |
| `gte` | mai mare sau egal |

**Formate acceptate** (pagina și WFL):

| Format | Exemplu |
|---|---|
| `dd.MM.yyyy HH:mm:ss` | `15.03.2024 09:30:00` |
| `dd.MM.yyyy` | `15.03.2024` |

> Pagina afișează mereu `dd.MM.yyyy HH:mm:ss`. Valoarea din WFL poate fi scrisă cu sau fără oră.
> Valoarea din WFL suportă variabile: `{{DataReferinta}}` este înlocuit înainte de evaluare.

**Exemple:**

```xml
<!-- Oprește când data documentului e mai veche decât 01.01.2024 -->
<ScrapeTable selector="table#t" saveTo="rows"
             nextPageSelector="a.next"
             exitIfCellDate="Data_Document:lt:01.01.2024" />

<!-- Oprește când data e mai veche decât o variabilă din workflow -->
<ScrapeTable selector="table#t" saveTo="rows"
             nextPageSelector="a.next"
             exitIfCellDate="Data_Document:lt:{{DataUltimeiSincronizari}}" />
```

---

Combinare condiții

Ambele condiții pot fi specificate simultan. **Prima care se declanșează** (per rând, în ordine) oprește extragerea.

```xml
<ScrapeTable selector="table#t" saveTo="rows"
             nextPageSelector="a.next"
             exitIfCellEquals="Stare:Anulat"
             exitIfCellDate="Data_Document:lt:{{DataReferinta}}" />
```

---

Exemple complete

```xml
<!-- Un singur tabel, o singură pagină -->
<ScrapeTable selector="table.table-striped" saveTo="DateContract" />

<!-- Multi-pagină forward: urmărește butonul Next automat -->
<ScrapeTable selector="table.table-striped"
             saveTo="ListaAngajamente"
             nextPageSelector="a[rel='next']"
             waitSelector="table.table-striped"
             LogValue="Extrag toate paginile" />

<!-- Multi-pagină reverse: începe de la ultima pagină spre prima -->
<ScrapeTable selector="table.table-striped"
             saveTo="TabelIstoric"
             lastPageSelector="a.last"
             prevPageSelector="a.prev"
             waitSelector="table.table-striped"
             startFromLast="true"
             timeout="5" />

<!-- Extragere cu oprire automată pe dată + skip coloane auxiliare -->
<ScrapeTable selector="table.table-striped"
             saveTo="Angajamente"
             nextPageSelector="a.next"
             skipFirstNColumns="1"
             skipLastNColumns="1"
             exitIfCellDate="Data_Document:lt:{{DataReferinta}}"
             LogValue="Extrag angajamente până la data de referință" />
```

---

## Screenshot

Captează ecranul paginii curente și îl salvează într-un fișier sau într-o variabilă internă (base64). Cel puțin unul din `path` sau `saveTo` este obligatoriu.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `path` | ¹ | — | Calea fișierului în care se salvează imaginea (PNG). Obligatoriu dacă `saveTo` e absent |
| `saveTo` | ¹ | — | Numele variabilei interne în care se salvează imaginea ca base64. Obligatoriu dacă `path` e absent |
| `isCheckpoint` | — | `false` | Marchează pasul ca punct de control |
| `LogValue` | — | — | Text afișat în log la execuție |

¹ Cel puțin unul dintre `path` / `saveTo` este obligatoriu.

```xml
<!-- Salvare în fișier -->
<Screenshot path="C:\temp\captură.png" LogValue="Captez ecranul înainte de trimitere" />

<!-- Salvare în variabilă (base64) -->
<Screenshot saveTo="ImagineB64" />
```

---

## ScrollToView

Derulează pagina până când elementul specificat devine vizibil în viewport. Util când butoanele sau câmpurile sunt în afara zonei vizibile.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✓ | — | Selectorul CSS / XPath al elementului țintă |
| `timeout` | — | `30` | Timeout în secunde |
| `isCheckpoint` | — | `false` | Marchează pasul ca punct de control |
| `LogValue` | — | — | Text afișat în log la execuție |

```xml
<ScrollToView selector="button:has-text('Salvează')" />
<ScrollToView selector="#sectiunea-detalii" LogValue="Derulează la secțiunea detalii" />
```

---

## Select

Selectează o opțiune dintr-un element `<select>` HTML după valoare, text vizibil sau index numeric. Cel puțin unul din `value`, `text` sau `index` este obligatoriu.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✓ | — | Selectorul CSS / XPath al elementului `<select>` |
| `value` | ¹ | — | Valoarea atributului `value` al opțiunii |
| `text` | ¹ | — | Textul vizibil al opțiunii |
| `index` | ¹ | — | Indexul numeric al opțiunii (0-based) |
| `timeout` | — | `30` | Timeout în secunde |
| `isCheckpoint` | — | `false` | Marchează pasul ca punct de control |
| `LogValue` | — | — | Text afișat în log la execuție |

¹ Cel puțin unul dintre `value` / `text` / `index` este obligatoriu.

```xml
<!-- Selectare după valoare -->
<Select selector="#tipDocument" value="factura" LogValue="Selectez tip document: factură" />

<!-- Selectare după text vizibil -->
<Select selector="#judet" text="Cluj" />

<!-- Selectare după index -->
<Select selector="#luna" index="2" />
```

---

## SetInternalVar

Setează o variabilă internă `[[NumeVariabila]]` care poate fi utilizată în acțiunile ulterioare din workflow. Nu se expune prin `LogValue` și nu se transmite la Access. Util pentru valori temporare sau pentru controlul fluxului.

> **Notă:** Dacă valoarea conține încă `{{...}}` (nerezolvat din exterior), acțiunea este ignorată automat — variabila externă are prioritate.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `name` | ✓ | — | Numele variabilei (fără `[[` `]]`) |
| `value` | ✓ | — | Valoarea de atribuit; poate conține `[[AltaVariabila]]` |
| `isCheckpoint` | — | `false` | Marchează pasul ca punct de control |
| `LogValue` | — | — | Text afișat în log la execuție |

```xml
<!-- Setare valoare simplă -->
<SetInternalVar name="Contor" value="0" />

<!-- Setare valoare derivată din altă variabilă -->
<SetInternalVar name="NumeComplet" value="[[Prenume]] [[Nume]]" />
```

---

## Stop

Oprește execuția și afișează un mesaj utilizatorului. Workflow-ul **nu continuă** până când utilizatorul nu apasă „Next" în aplicație. Folosit pentru puncte de revizuire manuală.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `message` | — | — | Mesajul afișat utilizatorului în timp ce e în pauză |

```xml
<Stop message="Verifică formularul. Apasă Next când ești gata să salvezi."/>
<Click selector="button:has-text('Salvează')" waitNavigation="true"/>
```

---

## SwitchTab

Comută contextul de execuție pe un **tab deja deschis** în browser. Criteriile de identificare sunt evaluate în ordine: `tabIndex` → `urlEquals` → `urlContains` → `urlPattern`. Dacă niciun tab nu corespunde, se aruncă **eroare critică** și execuția se oprește.

Dacă `expectedUrl` este setat, după comutare se verifică URL-ul tabului:
- URL conține `expectedUrl` → se execută acțiunile din `<Children>`
- URL nu conține `expectedUrl` → se execută `<Else>`

Fără `expectedUrl`, `SwitchTab` este o acțiune simplă (fără Children).

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `tabIndex` | ⚠️¹ | `–1` | Index 1-based al tabului în ordinea deschiderii. `-1` = ignorat |
| `urlEquals` | ⚠️¹ | — | URL exact al tabului (case-insensitive) |
| `urlContains` | ⚠️¹ | — | Substring prezent în URL (case-insensitive) |
| `urlPattern` | ⚠️¹ | — | Expresie regex aplicată pe URL |
| `expectedUrl` | — | — | Substring verificat în URL după comutare. Activează Children/Else |
| `reload` | — | `false` | `true` → reîncarcă tabul după comutare (NetworkIdle) |
| `savePreviousTabTo` | — | — | Salvează URL-ul tabului **anterior** switch-ului într-o variabilă |
| `saveCurrentUrlTo` | — | — | Salvează URL-ul tabului **nou** după switch |
| `timeout` | — | `30` | Secunde așteptare pentru reload (dacă `reload=true`) |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |
| `closeTabWhenDone` | — | `false` | `true` → închide tabul după execuție și revine automat pe tabul anterior |

¹ Cel puțin unul dintre cele 4 criterii este obligatoriu.
```xml
<!-- Simplu: comut pe tabul de bază -->
<SwitchTab urlContains="forexe.mfinante.gov.ro"
           savePreviousTabTo="TabAnterior"
           LogValue="Comut pe tabul principal"/>

<!-- Cu verificare URL după switch -->
<SwitchTab urlContains="forexe.mfinante.gov.ro"
           expectedUrl="forexe.mfinante.gov.ro/ForexeSNM">
  <Log message="Tab corect, continui." level="success"/>
  <Else>
    <Log message="Tab greșit!" level="error"/>
    <Exit/>
  </Else>
</SwitchTab>

<!-- Cu regex (orice tab ForexeSNM sau CABWeb) -->
<SwitchTab urlPattern="forexe\.mfinante\.gov\.ro\/(CABWeb|ForexeSNM)"
           reload="false"/>

<!-- Întoarcere pe tabul anterior -->
<SwitchTab urlContains="[[TabAnterior]]"
           LogValue="Revin pe CABWeb"/>
```

---

## Upload

Atașează un fișier local la un input de tip `file`. Folosit când portalul are un buton „Răsfoiți / Browse".

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul `<input type="file">` |
| `path` | ✅ | — | Calea completă a fișierului pe disc. Ex: `C:\Documente\contract.pdf` |
| `timeout` | — | `30` | Secunde de așteptat ca elementul să fie pregătit |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Upload selector="input[type='file']"
        path="C:\AVACONT\Exports\document.pdf"
        LogValue="Încarc documentul justificativ"/>
```

---

## Wait

Pauzează execuția un număr fix de secunde. **Preferă `<WaitFor>` ori de câte ori posibil** — e mai rapid și mai robust.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `seconds` | ✅ | `1` | Numărul de secunde. Acceptă zecimale: `0.5` |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<Wait seconds="2"/>
<Wait seconds="0.5"/>
```

---

## WaitFor

Așteaptă până când un element de pe pagină atinge o anumită stare. **Metoda preferată de sincronizare** față de `<Wait>`.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul de urmărit |
| `state` | — | `visible` | `visible` · `hidden` · `attached` · `detached` |
| `strict` | — | `false` | `true` → Playwright aruncă eroare dacă selectorul găsește mai mult de un element; `false` → folosește automat `.First` |
| `refreshOnFail` | — | `false` | `true` → reîncarcă pagina și reîncearcă dacă timeout-ul expiră |
| `maxRetries` | — | `3` | Numărul de reîncărcări când `refreshOnFail="true"` |
| `timeout` | — | `30` | Secunde de așteptat starea |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

```xml
<WaitFor selector="table.table-striped" timeout="30"/>

<!-- Așteaptă dispariția unui spinner -->
<WaitFor selector="img[alt='Loading...']" state="detached" timeout="15"
          refreshOnFail="true" maxRetries="5"/>

<!-- Așteaptă ca o fereastră modală să se închidă -->
<WaitFor selector="h3:has-text('Modifică recepție')" state="hidden" timeout="5"/>
```

---

## WaitForJS

Evaluează o expresie JavaScript în browser și așteaptă (prin polling) până când rezultatul îndeplinește condiția specificată. Salvează rezultatul **indiferent de succes sau eșec** — util pentru debugging.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `expression` | ✅ | — | Expresia JS de evaluat. Returnează orice valoare. Suportă `[[VAR]]` rezolvat înainte de evaluare |
| `expectedValue` | — | — | Valoarea așteptată ca string. Dacă lipsește, se aplică `waitMode` |
| `compare` | — | `eq` | Operator de comparație. Vezi secțiunea **Operatori** |
| `waitMode` | — | `truthy` | Comportament când `expectedValue` lipsește. Vezi secțiunea **waitMode** |
| `saveTo` | — | — | Variabilă internă `[[VAR]]` unde se salvează rezultatul (inclusiv la timeout) |
| `timeout` | — | `10` | Secunde de așteptat înainte de a arunca excepție |
| `pollingMs` | — | `100` | Interval de verificare în milisecunde |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

### Operatori (`compare`)

| Operator | Descriere |
|---|---|
| `eq` | Egalitate exactă (case-insensitive) — **default** |
| `neq` | Diferit de valoarea așteptată |
| `contains` | Rezultatul conține șirul specificat |
| `regex` | Expresie regulată. Flag-urile se adaugă după `:` la sfârșitul `expectedValue` (ex: `^\d+$:i`) |

### waitMode

Se aplică **doar** când `expectedValue` lipsește:

| Valoare | Comportament |
|---|---|
| `truthy` | Orice valoare JS truthy = succes (`true`, text nenul, număr nenul). `false`, `null`, `undefined`, `""`, `0` = eșec — **default** |
| `nonNull` | Orice valoare non-null / non-undefined = succes (inclusiv `false` și `0`) |
| `noWait` | Execută expresia o singură dată, salvează rezultatul în `saveTo` și continuă imediat fără polling |

```xml
<!-- Așteaptă ca select2 să preia focus-ul după deschidere -->
<WaitForJS
    expression="document.activeElement === document.querySelector('#select2-drop input.select2-input')"
    expectedValue="true"
    compare="eq"
    timeout="3"
    pollingMs="100"
    LogValue="Aștept focus select2"
/>
```

```xml
<!-- Salvează titlul paginii fără să aștepte o condiție -->
<WaitForJS
    expression="document.title"
    waitMode="noWait"
    saveTo="titluPagina"
/>
```

```xml
<!-- Verifică că un element a primit o clasă specifică -->
<WaitForJS
    expression="document.querySelector('.status-badge')?.className"
    expectedValue="active"
    compare="contains"
    saveTo="statusClass"
    timeout="5"
    pollingMs="200"
    LogValue="Aștept status activ"
/>
```

```xml
<!-- Verifică un număr cu regex, salvează rezultatul pentru debugging -->
<WaitForJS
    expression="document.querySelector('input[name=cod]')?.value"
    expectedValue="^\d{10}$:i"
    compare="regex"
    saveTo="codCitit"
    timeout="5"
    LogValue="Verific format cod"
/>
```

```xml
<!-- Așteaptă ca un element să fie prezent în DOM (nonNull) -->
<WaitForJS
    expression="document.querySelector('.alert.alert-success')"
    waitMode="nonNull"
    timeout="10"
    pollingMs="150"
    LogValue="Aștept confirmarea salvării"
/>
```

### Note

- Expresia este învelită automat în `() => { return (...); }` înainte de evaluare.
- `[[INTERNAL_VAR]]` din `expression` sunt rezolvate **înainte** de trimiterea către browser.
- `{{USER_VAR}}` sunt deja rezolvate de `KBOT_STANDALONE` înainte de execuție — nu mai există în expresie la momentul evaluării.
- La **timeout**, ultimul rezultat JS este salvat în `saveTo` (dacă e specificat), apoi se aruncă excepție.
- Erorile JS din expresie (selector invalid, proprietate lipsă etc.) sunt prinse și loggate ca `[JS Error]`, iar polling-ul continuă.

---

## While

Repetă acțiunile copil **atâta timp cât** o condiție despre un element rămâne adevărată. Cel mai frecvent folosit pentru paginare.

| Atribut | Oblig. | Implicit | Valori / Note |
|---|---|---|---|
| `selector` | ✅ | — | Elementul a cărui stare e verificată la fiecare iterație |
| `condition` | — | `Visible` | `Visible` · `Hidden` · `Present` (există în DOM indiferent de vizibilitate) |
| `runFirstTime` | — | `true` | `true` → execută o dată fără a verifica condiția la prima iterație |
| `maxIterations` | — | `50` | Limită de siguranță anti-buclă infinită |
| `indexVariable` | — | — | Numele contorului. Accesibil ca `[[indexVariable]]` |
| `timeout` | — | `30` | Secunde de așteptat la verificarea condiției |
| `isCheckpoint` | — | `false` | Marchează ca punct de reluare |
| `LogValue` | — | — | Mesaj afișat în log la execuție |

> Necesită acțiuni copil între tagurile de deschidere și închidere.

```xml
<While selector=".btn-next" condition="Visible"
       runFirstTime="true" indexVariable="pageIdx">

  <ScrapeTable selector="table.table-striped" saveTo="Pagina_[[pageIdx]]"/>

  <IfExists selector=".btn-next">
    <Click selector=".btn-next"/>
    <Wait seconds="2"/>
  </IfExists>

</While>
```

---