# SLICE-0000-83/95/99/101…115 — Ajutorul recepțiilor facturilor primite

Istoricul urmărește aceeași secțiune din `src/KBot.App/HelpContent/contabil/efactura/index.md`; toate etapele de mai jos au acest fișier în domeniu, iar căile suplimentare rămân în etapa lor. Interfața descrisă nu a fost văzută rulând la aceste checkpointuri. Rezultatele HelpCheck sunt păstrate separat: 109/111/112/115 nu l-au rulat, iar celelalte raportează propriul rezultat. 95 mută clasificațiile în legare; 101 înlocuiește ferestrele succesive cu una comună; 102 o mută în ribbon și 105 o încorporează în vederi. 108 reafișează facturile acoperite, 109 mută desfacerea sub recepții, 114 păstrează angajamentele furnizorului, iar 115 permite sume parțiale în propuneri. Afirmațiile anterioare despre lipsa butoanelor sau propunerile 1:1 sunt istorice. Capturile neschimbate, lipsă sau de refăcut rămân explicit consemnate.

- [0000-83](#slice-0000-83)
- [0000-95](#slice-0000-95)
- [0000-99](#slice-0000-99)
- [0000-101](#slice-0000-101)
- [0000-102](#slice-0000-102)
- [0000-103](#slice-0000-103)
- [0000-104](#slice-0000-104)
- [0000-105](#slice-0000-105)
- [0000-108](#slice-0000-108)
- [0000-109](#slice-0000-109)
- [0000-111](#slice-0000-111)
- [0000-112](#slice-0000-112)
- [0000-114](#slice-0000-114)
- [0000-115](#slice-0000-115)

## SLICE-0000-83 - Help: the "Receptii" view of a received invoice (slices KE01..KE07) (16.10.2026) <a id="slice-0000-83"></a>

### What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Topic `contabil.efactura` (`HelpContent/contabil/efactura/index.md`): new section "Receptiile unei facturi primite" (what the view shows, who may write, the amounts with VAT, the classifications of the lines, the three kinds of link, the check before anything starts, what happens after the save, "Scoate legatura"); the bullet of the "Continut" view mentions the new columns; `screens:` names `KeLegaForm`, `KeAngajamentForm`, `KeClasificariForm`; new search words in `keywords:`; slice tags KE01..KE07 and 0000-83.
- `tools/HelpCheck/Check-Help.ps1` accepts the slice ids `KE` / `KEnn` (a worklog or an index row), like `00EF`.
- Watermark: `help-version.txt` = 2026-10-16.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `help-version.txt`, `tools/HelpCheck/Check-Help.ps1`.

### Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

Check-Help.ps1 -Coverage: no error comes from the new section or its capture tag. 20 errors remain, all in files this slice did not touch (the tutorials efactura-factura-noua and ordonantare-din-plata, a nav item of tur-setari); coverage still lists EFacturaView, SetariAccessView and UpdateOfferForm, as before.

### Unverified / deferred
- **Captures to take:** `efactura-receptii` (new tag, in the new section). **Captures to redo:** `efactura-cumparare` (the "Continut" grid has two new columns).
- No tour or tutorial was changed.

## SLICE-0000-95 - Help for 00EF-39 (17.10.2026) <a id="slice-0000-95"></a>

### What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: the step «Primul pas: clasificațiile liniilor» (with the button «Clasificații pe linii…» and its own window) is now «Clasificațiile liniilor»: no separate button, the classifications are chosen in the window opened by any of the three link buttons (angajament on top, the table of lines, «Ca la prima linie»), and they are saved with the link. «Al doilea pas: recepția» is just «Recepția». The front matter no longer lists `KeClasificariForm`; the keyword «clasificatii pe linii» became «clasificatiile liniilor»; the section tag carries `00EF-39, 0000-95`.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: the only error about this page was the unknown slice `0000-95`, which this worklog and its status row answer; the other errors printed (tutorial `efactura-factura-noua.md` controls, sections without a slice tag in other tutorials) were there before and are not touched.

### Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` (the «Receptii» view) shows five buttons and has to be redone.

## SLICE-0000-99 - Help for KE09 / KE10 (18.10.2026) <a id="slice-0000-99"></a>

### What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: a paragraph for the button «Recepție nouă» of the list (several ticked invoices, one reception) and one for the message when FOREXE refuses a reception (CAB-0031), the date written with dots and amounts with a comma. Section tag gains `KE09, KE10, 0000-99`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: only the old warnings of the tutorials (no `<!-- slice -->` under their sections), nothing from this section.

### Unverified / deferred
Nothing seen on screen; the capture `efactura-receptii` is unchanged.

## SLICE-0000-101 - Help for KE11 (18.10.2026) <a id="slice-0000-101"></a>

### What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: the subsection «Clasificațiile liniilor» became **«Fereastra recepției»** and describes the four zones of the rebuilt window (tree of invoices with ticks, «Produse Factura …» with «Recepționată» and the classification, «Angajamente» with the empty row and «Elimină asociere», «Date recepție …» with the amounts by classification); the three bullets of «Recepția» follow the new window; the paragraph «Mai multe facturi pe o singură recepție» says the invoices are ticked in the window (no more one window per invoice). «Ca la prima linie» and «Alt angajament…» are gone from the text. Section tag gains `KE11, 0000-101`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: nothing left about this topic except the old tutorial warnings.

### Unverified / deferred
Nothing seen on screen; the capture `efactura-receptii` (the view, not the window) is unchanged; no capture of the window was made.

## SLICE-0000-102 - Help for KE12 (18.10.2026) <a id="slice-0000-102"></a>

### What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite», rewritten: the view «Recepții» only shows the state (no buttons now, so no «Scoate legătura» either); linking is described as the page **«Recepții»** of the ribbon (tab E-Factura, after «Cumpărare», a separator before «Vânzare»): its four zones, the combo that chooses new / existing / modified, the button of the footer, the three kinds of reception, several invoices in one reception (ticked in the tree), the refusal message of FOREXE. The mentions of the modal window and of the list's «Recepție nouă» button are gone. Section tags gain `KE12, 0000-102`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: nothing left about this topic except the old tutorial warnings.

### Unverified / deferred
Nothing seen on screen; no capture of the page was made (the capture `efactura-receptii` shows the view).

## SLICE-0000-103 - Help for KE12-02 (18.10.2026) <a id="slice-0000-103"></a>

### What changed and why
`contabil/efactura/index.md`, subsection «Pagina «Recepții»», zone «Facturi de recepționat»: the supplier is only a heading without a tick, the tree starts closed, the lens searches by supplier name or invoice number (at least 3 characters, Esc clears), invoices newest first under each supplier, and the invoices covered in full by one or several receptions, or with the value 0, are not listed. Tag gains `KE12-02, 0000-103`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: nothing about this topic beyond the old tutorial warnings (to be re-run).

### Unverified / deferred

## SLICE-0000-104 - Help for KE12-03 (18.10.2026) <a id="slice-0000-104"></a>

### What changed and why
`contabil/efactura/index.md`, zone «Produse Factura …»: every line has a tick box in the first column and the invoice row has one too (it ticks / clears all its lines); a cleared line puts 0 in «Recepționată» and locks it, ticking it again gives back the sum of «Valoare cu TVA». Tag gains `KE12-03, 0000-104`.

### Test results
`tools\HelpCheck\Check-Help.ps1` run: nothing about this topic.

### Unverified / deferred

## SLICE-0000-105 - Help for KE12-04 (18.10.2026) <a id="slice-0000-105"></a>

### What changed and why
`contabil/efactura/index.md`, «Recepțiile unei facturi primite»: the view «Recepții» of an invoice shows the reception form of the invoice (without the tree of invoices; without the table of angajamente in the view of an angajament) instead of the table of links; the summary line stays. Tag gains `KE12-04, 0000-105`.

### Test results
`tools\HelpCheck\Check-Help.ps1` run: nothing about this topic.

### Unverified / deferred
Nothing seen on screen. The capture comment under the section («Vederea «Recepții» a unei facturi primite») needs a new capture when the screen can be seen.

## SLICE-0000-108 - Help for KE13 (18.10.2026) <a id="slice-0000-108"></a>

### What changed and why
`contabil/efactura/index.md`, «Pagina «Recepții»», zone «Facturi primite» (was «Facturi de recepționat»): all invoices are listed; the covered ones are ticked and locked (grey tick, dim text); right click «Desface asocierea cu recepția»; the options icon of the header with «Arată facturile asociate»; the total in the footer; after a reception the covered invoices stay in the tree. Tag gains `KE13, 0000-108`.

### Test results
`tools\HelpCheck\Check-Help.ps1` run: reports only older missing tags in other files (`tutorials\ordonantare-din-plata.md` and others), nothing about this topic.

### Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` needs a new take.

## SLICE-0000-109 - Help for KE14 (19.10.2026) <a id="slice-0000-109"></a>

### What changed and why
`contabil/efactura/index.md`, zone «Facturi primite» of the page «Recepții»: the right-click «Desface asocierea» is replaced by the text of the new view (a press on a resolved invoice shows its products read-only and only its reception; the button «Elimină asociere» under the table of receptions; any other press or a tick brings the page back; a resolved invoice pressed while others are ticked asks to untick them first). Tag gains `KE14, 0000-109`.

### Test results
Not run: `Check-Help.ps1` reported only older missing tags elsewhere on the previous pass; this edit adds none.

### Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` needs a new take.

## SLICE-0000-111 - Help for KE15 (19.10.2026) <a id="slice-0000-111"></a>

`contabil/efactura/index.md`, page «Recepții» (the paragraph after the check «clasificația trebuie să fie indicator…»): one paragraph added - the sums of «Recepționată» are typed in the operator's Windows regional format (a dot user's «77.49» is 77.49); the «not enough rest» message also names the classifications where the chosen reception does have rest. Tag gains `KE15, 0000-111`.

Files: `src/KBot.App/HelpContent/contabil/efactura/index.md`. Not run (`Check-Help.ps1` not run), not seen on screen. The capture `efactura-receptii` is unchanged (still to redo, see 0000-109).

## SLICE-0000-112 - Help for KE16 (19.10.2026) <a id="slice-0000-112"></a>

`contabil/efactura/index.md`, page «Recepții»: one paragraph «Legare în bloc, cu propuneri» (the button with two chain links that shows on a supplier of the tree when the mouse is over it; the window lists the supplier's invoices without a reception, each with the proposed reception; the date rule; «Leagă»; nothing is saved before it; one reception to one invoice; role «Contabil»). Tag gains `KE16, 0000-112`.

Files: `src/KBot.App/HelpContent/contabil/efactura/index.md`. `Check-Help.ps1` not run, not seen on screen. The capture `efactura-receptii` is unchanged (to redo, see 0000-109).

## SLICE-0000-114 - Help for 0121-05: the angajamente picked on the page «Recepții» stay with the supplier (19.10.2026) <a id="slice-0000-114"></a>

### What changed and why
`contabil/efactura/index.md`, section «Pagina «Recepții»» (tag `0121-05, 0000-114`), the bullet «stânga jos, «Angajamente»»: the table shows the DDF's angajamente, the ones picked for the supplier and the ones already linked; a pick on the empty row now stays linked to the supplier (by its tax code), seen again on any invoice of the supplier and after closing the page, and used by the bulk link; a supplier with no tax code on the invoice cannot be linked (the page says so); «Elimină asociere» removes a picked angajament after a question (its indicators go too).

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`; registry row in `docs/worklog/state/KBOT_STATUS_0000-0009.md`; `help-version.txt` stays 2026-10-19.

### Test results
`Check-Help.ps1 -Coverage`: nothing reported for `efactura/index.md` (the existing errors elsewhere are untouched). App build 0 warnings / 0 errors. Not run on screen.

### Left unverified or deferred
No capture changed visibly (the table looks the same), nothing marked `redo`.

## SLICE-0000-115 - Help for KE17 (20.10.2026) <a id="slice-0000-115"></a>

`contabil/efactura/index.md`, page «Recepții»: the paragraph «Legare în bloc, cu propuneri» rewritten as a short intro + three points (change the angajament, write the sum received in «Recepție», pick another reception from the table below with its two groups and the column «Rămas»); a reception may cover several invoices. Tag gains `KE17, 0000-115`.

Files: `src/KBot.App/HelpContent/contabil/efactura/index.md`. `Check-Help.ps1` not run, not seen on screen. The capture `efactura-receptii` is unchanged.
