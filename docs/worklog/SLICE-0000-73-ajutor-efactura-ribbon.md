# SLICE-0000-73 - Ajutorul pentru 0119: E-Factura pe ribbon (13.10.2026)

Sincronizarea din dialog descrisă inițial este amendată ulterior în [00EF-30](SLICE-00EF-30-primite-sincronizare-din-ribbon.md) și [0000-90](SLICE-0000-70-ajutor-ribbon-buton-ajutor.md#slice-0000-90); nu se rescrie ajutorul runtime aici.

Sub-felie de ajutor a feliei 0119. Context, capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

## Ce s-a schimbat si de ce
- `contabil.efactura`: intro (fila E-Factura cu grupurile E-Factura si ANAF; «Vanzare» deschide macheta facturilor emise, ca pagina, inchisa cu «Inchide» / Esc); tokenul, sincronizarea si «Date Unitate» / «Conturi Unitate» se deschid din ribbon (fara macheta facturilor deschise), nu din butonul K / meniul cu trei puncte al barei de titlu (ascuns in 0119) si nici din «subsolul ferestrei»; sectiunea «Fereastra «Facturi primite»» devine **«E-Factura Cumparare: «Facturi primite»»** (macheta din E-Factura › Cumparare, fara «Iesire»). `screens:` + `PrimiteForm`; cuvinte cheie noi. Capturi noi: `ribbon-efactura`, `efactura-cumparare`. `prepare:` actualizat: `date-unitate`.
- `contabil.efactura.token`: se deschide din ribbon (E-Factura › ANAF); `screens:` fara `FacturiForm.btnToken` (nu exista in cod — eroarea veche a verificatorului dispare). `prepare:` actualizat: `efactura-token`.
- `contabil.efactura.conturi`: se deschide din ribbon (Configurari › Unitate); macheta facturilor deschisa isi actualizeaza singura conturile. `prepare:` actualizat: `conturi-unitate`.
- Dialogurile raman dialoguri, cu butoanele lor («Iesire» la «Date Unitate», «Inchide» la conturi, token, sincronizare): textele lor neschimbate.

## Fisiere
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `contabil/efactura/token.md`, `contabil/efactura/conturi.md`.

## Teste / verificare
Vezi 0000-74. Nimic rulat in aplicatie.

## Capturi de refacut
Noi: `ribbon-efactura`, `efactura-cumparare`. `efactura-facturi` / `efactura-meniu-factura` nu exista inca (lipsa; `goto: menu:efactura` ajunge). `prepare:` actualizat: `date-unitate`, `conturi-unitate`, `efactura-token`.

## Neverificat / de citit de operator
- Mesajul de cod «Reinnoiti tokenul cu butonul «Token ANAF» din subsolul ferestrei.» e invechit (0119 Open threads); ajutorul spune unde e butonul acum, fara sa-l citeze.
- Tutorialul `efactura-factura-noua` porneste din MENIU si tinteste controale care nu mai exista in `FacturiForm` (`cmbClient`, `btnLinieNoua`, `gridLinii`, `cmbContPlata` — eroare veche a verificatorului, de dinainte de 0119).
