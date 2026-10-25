# VERCON — mockupuri corectate după adnotări

**Set istoric, înlocuit de [revizia 2](../2026-10-12-corectate-v2/INDEX.md)** după cele cinci atașamente ulterioare ale utilizatorului: aspectul NIR este unica referință pentru toate grilele, iar o listă poate avea mai multe BC-uri. Selectorul unic și paddingurile din acest set sunt depășite. PNG-urile istorice sunt păstrate pentru comparație.
12.10.2026. Set revizuit pe baza celor șase imagini din [Corectii_mockup-uri_01](../Corectii_mockup-uri_01/). Sunt **13 PNG-uri**: 11 imagini editate/generate și 2 imagini păstrate, Stocuri și Rapoarte, care nu aveau adnotări. Originalele și notele utilizatorului rămân în folderele lor.

## Consultare

Arborele documentelor este în dreapta; documentul selectat este pentru citire. Adaugă/Modifică intră în editor. Frunza selectată ilustrează butonul ⋯ la hover.

| Ecran | PNG |
|---|---|
| Stocuri Materiale — păstrat | [01-gestiune-materiale.png](01-gestiune-materiale.png) |
| NIR MF — factură/NIR grupate, MF în grila produselor, arbore furnizor → dată → factură + total | [02-nir-mijloace-fixe.png](02-nir-mijloace-fixe.png) |
| BC — DGV comun, arbore în dreapta | [03-bon-consum.png](03-bon-consum.png) |
| ListaAlimente — DGV-uri mese, BC existent asociat, fără Alimente necesare; arbore în dreapta | [04-lista-alimente.png](04-lista-alimente.png) |
| Transfer OI — fără sidebar Perioada/Deținători și Efect/Istoric; arbore BT | [05-transfer-obiecte-inventar.png](05-transfer-obiecte-inventar.png) |
| Rapoarte/SFP — păstrat, inclusiv arbore multianual | [06-rapoarte-previzualizare.png](06-rapoarte-previzualizare.png) |

## Adăugare, modificare și popup

Arborele documentelor salvate se ascunde în aceste stări. Reapare după salvare confirmată sau Renunță; erorile păstrează draftul în editor.

| Stare | PNG |
|---|---|
| NIR MF — adăugare, valoare factură activă/manuală, combobox produs cu căutare „Bir”, coloane MF în aceeași grilă | [02b-nir-adaugare.png](02b-nir-adaugare.png) |
| BC — modificare, editoare în celule și disponibil la data bonului | [03b-bon-consum-modificare.png](03b-bon-consum-modificare.png) |
| ListaAlimente — modificare, sumar mese DGV și buton Salvează lista | [04b-lista-alimente-modificare.png](04b-lista-alimente-modificare.png) |
| Popup Alege B.C. — arbore cu bonuri existente din Alimente, neasociate altei liste | [04c-alegere-bon-consum.png](04c-alegere-bon-consum.png) |
| Transfer — modificare, combobox numai cu produse ale predătorului | [05b-transfer-modificare.png](05b-transfer-modificare.png) |
| Predare — adăugare, cont de intrare ca sursă, primitor | [05c-predare-adaugare.png](05c-predare-adaugare.png) |
| Casare — adăugare, fără Primitor, produse ale predătorului | [05d-casare-adaugare.png](05d-casare-adaugare.png) |

## Fluxuri și reguli actualizate

[FLUXURI_UI.md](../../FLUXURI_UI.md) este contractul detaliat al acestei revizii. Corecțiile sunt integrate în [PLAN](../../PLAN_IMPLEMENTARE.md), [DECIZII](../../DECIZII_DESCHISE.md), [MAPARE](../../MAPARE_ACCESS_WEB.md), [CONTRACT_CALCULE](../../CONTRACT_CALCULE.md), [PROBE_BROWSER](../../PROBE_ACCES.md#probe-browser) și [STATUS](../../VERCON_STATUS.md).

Schimbarea principală față de prima propunere: BC se creează separat și se selectează din popup. Salvează lista nu generează/resalvează BC și nu repetă consumul. N01/N02 și pragul totalului NIR rămân păstrate; câmpul Valoare factură este editabil manual. Predarea pornește din contul de intrare; transferul/casarea folosesc produsele predătorului.

Aspectul rămâne bazat pe ADECHIT/Portal. Datele sunt fictive. Setul este o propunere de interfață, verificată vizual, nu implementarea sau testarea funcțiilor aplicației.

## Generare

Instrument folosit: image_gen integrat. Prompturi finale de bază: [prompts.json](prompts.json); corecțiile de verificare: [prompts-verificare.json](prompts-verificare.json); proveniență/fișiere selectate: [manifest.json](manifest.json). Arhivă completă: [ZIP](../vercon-mockupuri-corectate-2026-10-12.zip).

