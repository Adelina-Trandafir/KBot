# SLICE-0010-09 - KBotDataView: butonul standard de stergere, acelasi in toate tabelele (31.10.2026)

Operator: butonul de stergere facut in Clasificatii sa fie in grila insasi, cu aceleasi proprietati peste tot; toate butoanele de stergere din tabele inlocuite cu cel implicit; iconita mana la hover. Felie: primul sub-numar liber din 0010 (0010-08 era ultimul).

## What changed and why
- **`KBotButtonKind`** (`Custom` implicit, `Delete`) si **`KBotDataColumn.ButtonKind`**: pe o coloana `Button`, `Delete` = butonul standard K-BOT: glyph «✕» (`KBotDataColumn.DeleteGlyph`), umple toata celula (fara margin, padding, chenar), plat (arata culoarea randului, inclusiv alternant / selectat), o spalare din `ErrorColor` al temei (alpha 70, `_cDeleteHover`) la hover si cursorul mana. Celelalte proprietati `Button*` sunt ignorate. Definit o singura data in grila: citirile din `KBotDataView.CellButton.vb` trec prin membri `Effective*` ai coloanei.
- **`KBotDataColumn.ButtonHoverBackColor`** (gol = fara efect): hover si pentru butoanele obisnuite; urmarirea mouse-ului pe celula-buton (`UpdateButtonHover`, `_hoverButtonRow/Column`) in `OnMouseMove` / `OnMouseLeave`; `Cursor = Hand` peste un buton cu hover.
- **Inlocuite cele 14 coloane de stergere** («sterge») din designere: ClasificatiiBugetView (2), ClasificatiiVenituriView (2), JurnaleForm, SablonOperatiiView, SablonSalariiView (3), DdfEditSectiuneaAPage, ParteneriForm (2), ConturiForm (EFactura), VanzareContinutPage (EFactura). Proprietatile `Button*` de pe ele au fost scoase (inclusiv cele puse la mana in 0126-02) si codul de hover din `ClasificatiiBugetView.ApplyTheme` a fost sters.
- Neatinse (nu sunt «stergere»): `scoate` / `opreste` din RobotQueueForm (pictograma minus rosu), `opt` din KeLegaForm si PrimiteListaPage, `duplica`, `poza` etc. din HelpCaptureForm.
- FileVersion: Controls 1.77.0.0, App 1.1.5.0, EFactura 1.13.0.0. Ajutor: eticheta `0010-09` pe `clasificatii.md`; `KBotDataView.md`.

## Files touched
`KBot.Controls/DataView/KBotButtonKind.vb` (nou), `KBotDataColumn.vb`, `KBotDataView.CellButton.vb`, `.Input.vb`, `.Painting.vb`, `.Theming.vb`, `KBotDataView.md`; 9 `*.Designer.vb` (lista de mai sus) si `ClasificatiiBugetView.vb`; trei `.vbproj`; `state/KBOT_STATUS_0010-0019.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. Nimic rulat, nici un test.

## Unverified / deferred
- **Nevazut pe ecran** in niciuna din cele 9 ferestre: spalarea la hover, mana, butonul pe rand alternant / selectat, latimea de 34 a coloanelor.
- Valorile «✕» puse pe rand de cod in unele ecrane raman (nu mai conteaza: glyph-ul vine din coloana); curatenia lor nu s-a facut.
- Alte ecrane cu stergere facuta altfel (ex. pictograme, meniuri) nu au fost cautate dincolo de coloanele numite «sterge».
- Ajutorul altor ferestre nu a fost recitit pentru descrieri ale butonului «✕» (aspectul se schimba usor).
