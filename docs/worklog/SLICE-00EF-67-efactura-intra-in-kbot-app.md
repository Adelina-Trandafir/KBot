# SLICE-00EF-67 - E-Factura nu mai e proiect separat: codul e in KBot.App\EFactura (09.11.2026)

Cererea operatorului: `KBot.EFactura` sa nu mai fie un proiect separat de K-BOT. Mutare de structura; nicio comportare noua, nimic schimbat pentru operator. **Inchis 09.11.2026: operatorul a deschis si verificat toate paginile E-Factura.**

## What changed and why
- **Structura**: codul E-Factura (112 fisiere `.vb` + `.resx`) sta in `src/KBot.App/EFactura/{Authorization,Certificates,Forms,Views}`, namespace `KBot.App`. Nu mai exista `src/KBot.EFactura`, nici proiectul in `KBot.sln`, nici `ProjectReference` in `KBot.App.vbproj`.
- **`KBot.App.vbproj`**: 68 de intrari `Compile Update`, dupa regula fisierelor partial din CLAUDE.md (partial = cod simplu, nested sub fisierul principal: `DependentUpon` + `SubType Code`; designerele la fel fara `SubType`; cele 7 controale au `SubType UserControl`). `iTextSharp` 5.5.13.3 (`NoWarn NU1701`, ca in `KBot.Xfa`) e referinta directa a `KBot.App`: il folosesc `FacturaPdf` si `FacturaPrimitaPdf`.
- **Resurse**: 15 imagini E-Factura in `My Project\Resources.resx` + `Resources.Designer.vb` (scris de mana) ale `KBot.App`, PNG-urile in `src/KBot.App/Resources/`. Alte 6 (`cells`, `filter`, `kbot_64`, `save_32`, `Jonas-Rask-Danish-Royalty-Free-Refresh.32`, `credit_card`) existau deja in App, identice (md5), cu acelasi nume de proprietate. Codul citeste `My.Resources.Resources.<nume>`, neschimbat.
- **Namespace**: fisierele nu au blocuri `Namespace`, deci tipurile sunt acum `KBot.App.*`; fara `Imports KBot.EFactura` (7 fisiere) si fara `Global.KBot.EFactura.ISubunitHost`.
- **Contractele cu shell-ul au ramas**: `IFacturaPdfViewer` + fabrica (Adobe porneste abia la prima vedere PDF), `IKeRobot`, `ISubunitHost`, `IKeAngajamenteHost` — robotul, bara si subunitatea apartin lui `KbotForm`. Comentariile care le explicau prin «nu poate referenta App» sunt rescrise (9 locuri, inclusiv `KBot.Common\ReauthGate.vb`, care ramane in Common).
- **Updater**: nu sterge nimic din dosar (`UpdateApplier.vb`), deci `KBot.EFactura.dll` ramane orfan nevatamator pe PC-urile care il aveau; `KBot.App.dll` nu il incarca, iar manifestul nu il mai contine.

## Trap (de tinut minte)
Vechea plasa de siguranta `EmbeddedResource Remove="**\*.*.resx"` (pentru `.resx` cu doua puncte) scotea din build TOATE cele 22 de `.resx`-uri E-Factura: in MSBuild `*.*` potriveste si numele cu un singur punct. **Nu se reia.** Nu exista `.resx` cu doua puncte; daca VS creeaza unul (MSB3577), se sterge ca oriunde in `KBot.App`. `FacturiForm` citeste `capBar.OptionButtonImage` din resurse, deci avea nevoie de resx incorporat.

## Files touched
`KBot.sln`; `src/KBot.App/KBot.App.vbproj`; `My Project\Resources.resx` + `Resources.Designer.vb`; `src/KBot.App/Resources/` (15 PNG); `src/KBot.App/EFactura/**` (mutate); `KbotForm.PlanCont.vb`, `KbotForm.Subunit.vb`, `AvacontMain.vb`, `FacturiPlatiPage.vb`, `KbotForm.DdfDelete.vb`, `KbotForm.EFactura.vb`, `IngestFlow.vb`, `KbotForm.KeRobot.vb`, `FacturaPdfViewer.vb`; `KBot.Common\ReauthGate.vb`; `PLAN_00EF_EFactura.md`, `CLAUDE.md` (randul `KBot.App`).

## Test results
`dotnet build` Debug si Release pe `KBot.App` si Debug pe `KBot.sln`: 0 erori, fara avertismente de aici (unul vechi, BC42108 in `AVC.Migrator\...\AvcOperatiiStep.vb`). In `KBot.App.dll`: tipurile `FacturiForm`, `FacturiCumparareForm`, `VerconForm` si resursele `KBot.App.<Clasa>.resources` ale formularelor mutate. **Toate paginile E-Factura deschise si verificate de operator.**

## Unverified / deferred
- `Config\mesaje_catalog.json` si cele 6 fisiere din `docs\deployment\pending\` pastreaza caile vechi `src/KBot.EFactura/...` (catalogul avea deja unele cai invechite); neatinse. `artifacts\` sunt iesiri de build vechi.
- Ajutor: niciunul (nimic nu s-a schimbat pentru operator).
