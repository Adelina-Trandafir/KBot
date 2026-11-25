# SLICE-00EF-68 — «Trimite la Vercon»: clic pe o factură trimisă o arată, «Retrage» o eliberează (25.11.2026)

Cererea operatorului: la «Trimite la Vercon», un clic pe o factură deja trimisă să arate ce conține, dar butonul de trimitere să nu mai fie activ; dacă e doar trimisă, nu și preluată de Administrator, să se poată retrage și apoi trimite în altă parte (alt cont, altă gestiune). Ajutor: 0000-183.

## What changed and why
- **Situația dinainte:** o factură trimisă (starea TRIMISA) era bifată și blocată în arbore (`CheckLocked`, 00EF-59), deci nu putea fi nici bifată, nici deschisă; modul «Salvează conturile» + «Retrage de la Vercon» se deschidea doar pentru o factură bifată, adică nu mai putea fi atins. Un clic nu arăta nimic.
- **Clic pe o factură trimisă** (`VerconForm.Tree_NodeMouseDown`): factura blocată din arbore se citește de la server (`GetVerconAsync`, pentru că Administratorul poate să o fi preluat între timp) și apare în grila din dreapta, **doar de citit** (`gridLinii.ReadOnlyGrid`), cu contul propus pe fiecare linie (`NU SE TRIMITE` / contul / «(fără cont)»), nota trimisă (read-only) și o notă deasupra care spune unde e factura. «Trimite la Vercon» și lista «Cont pentru toate liniile» stau stinse. Dacă starea nu mai e activă (retrasă / returnată între timp), arborele se citește din nou și factura se eliberează.
- **«Retrage de la Vercon»** apare doar cât starea e TRIMISA; pentru PRELUATA / PARTIALA factura se vede, dar nu se retrage. După retragere (serverul refuză dacă Administratorul a preluat-o între timp: mesajul lui e arătat, starea citită din nou) arborele se reface unde era, factura se bifează singură (`BuildTree(..., k_keepView)` + `SyncTicksAsync`), cu liniile gata de trimis în altă parte.
- **Ieșirea din vedere:** o bifă nouă (`Tree_NodeChecked`) sau un clic pe altă factură aduce grila înapoi la facturile bifate; un clic pe o lună/un furnizor nu schimbă nimic.
- **Scos:** modul «Salvează conturile» (`SetVerconConturiAsync` din `BtnTrimite_Click`, textul butonului): nu mai e dorit (butonul de trimitere stă stins pe o factură trimisă). Ruta serverului și metoda din `IEFacturaApi` / `ApiClient` rămân neatinse. Variabilele `_modify` / `_wasModify` -> `_view` / `_wasView` / `_viewId`; `_modify` înseamnă acum «factura din vedere se mai poate retrage».
- Textul de intro al paginii (`NotaInitiala`) are o propoziție despre clic pe o factură trimisă. Serverul nu s-a schimbat (DELETE-ul de retragere exista din 00EF-45).
- **Ajutor:** 0000-183 (`contabil/efactura/index.md`, tutorial `efactura-trimite-la-vercon.md`), etichete `00EF-68, 0000-183`; `help-version.txt` 2026-11-25.

## Files touched
`src/KBot.App/EFactura/Forms/VerconForm.vb`; `src/KBot.App/HelpContent/contabil/efactura/index.md`, `tutorials/efactura-trimite-la-vercon.md`, `help-version.txt`; status (`state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 avertismente, 0 erori. Niciun test scris sau rulat (regula operatorului); nimic văzut pe ecran; niciun server apelat. `Check-Help.ps1` rulat fără `-Coverage`: ieșirea e plină de erori vechi (tutoriale parteneri / ordonanțare); nu s-a căutat nimic nou despre această schimbare.

## Unverified / deferred
- **Nimic văzut pe ecran:** clicul pe frunza blocată (dacă `NodeMouseDown` vine și pe rândul blocat, așa cum arată citirea `AdvancedTreeControl.Overrides.vb`), grila read-only (combo-ul coloanei «Cont» nu trebuie să se deschidă), notele, butonul «Retrage», bifarea automată după retragere.
- Un clic cu **tastatura** (săgeți) pe o factură trimisă nu o deschide: control-ul ridică atunci doar `NodeMouseUp`, nu `NodeMouseDown`; gestul acoperit e clicul de mouse.
- Contul unei linii trimise se arată din lista de conturi încărcată; dacă un cont nu mai e în listă, se arată `Gestiune / Cont` cum le dă serverul pe linie (`Linie.Cont` / `Linie.Gestiune`), neverificat pe date reale că serverul le umple pentru o factură trimisă.
- La retragere, conturile propuse la prima trimitere nu se păstrează: se aleg din nou (liniile apar cu «(fără cont)» sau contul din lista de lângă notă).
- Versiunea `KBot.App` nu s-a ridicat (o întreabă `push-update.ps1`).
- Capturile din tutorial nu au fost refăcute.
