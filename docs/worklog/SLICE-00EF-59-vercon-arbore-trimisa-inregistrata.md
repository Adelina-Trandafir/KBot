# SLICE-00EF-59 — «Trimite la Vercon»: arborele spune «Trimisă în VERCON» / «Înregistrată în VERCON» (03.11.2026)

Cererea operatorului: în pagina «Trimite la Vercon», în arbore, facturile trimise să poarte textul «Trimisă în VERCON», cele preluate «Înregistrată în VERCON», iar cele trimise să fie și bifate. Operatorul a confirmat că bifa de pe cele trimise e **blocată**. Ajutor: 0000-150.

## What changed and why
- **Text în arbore** (`VerconForm.VerconWord`): `Trimisă` → «Trimisă în VERCON» (retrimisă după NIR anulat: «Trimisă din nou în VERCON»); `Preluată` → «Înregistrată în VERCON». «asociată parțial» și «returnată» neschimbate.
- **Bifă** (`VerconForm.AddSupplierNode`): o factură în starea `Trimisa` primește bifă **blocată** (`CheckLocked`), în culoarea normală; cele preluate / asociate parțial rămân bifate, blocate și gri, ca înainte. Fiind blocate, bifele nu intră în `TickedIds()`, deci nu pornesc citiri și nu duc la a doua trimitere.
- **Consecință asumată:** o factură trimisă nu mai poate fi bifată singură, deci butoanele «Salvează conturile» și «Retrage de la Vercon» (modul `_modify`, `FillGrid`) nu mai pot fi atinse din arbore. Codul lor a rămas neatins.
- **Ajutor:** `contabil/efactura/index.md` și `tutorials/efactura-trimite-la-vercon.md` (cuvintele din arbore, bifa, scoaterea textelor despre retragere / schimbarea conturilor); etichete `00EF-59, 0000-150`.
- FileVersion KBot.EFactura 1.14.0.0 → 1.14.1.0.

## Files touched
`src/KBot.EFactura/Forms/VerconForm.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj`, `src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/tutorials/efactura-trimite-la-vercon.md`.

## Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors (înainte de bump-ul de versiune). Niciun test scris sau rulat (regula operatorului), nimic văzut pe ecran, `Check-Help.ps1` nerulat.

## Unverified / deferred
- Aspectul nu a fost văzut pe ecran (bifa blocată în culoare normală vs. gri pentru cele înregistrate).
- Retragerea unei facturi trimise și schimbarea conturilor ei nu mai sunt accesibile din interfață; dacă operatorul le vrea înapoi, e nevoie de altă cale (ex. buton pe factura bifată blocat).
- Capturile din tutorial nu au fost refăcute; textul arborelui din ele poate fi vechi.
