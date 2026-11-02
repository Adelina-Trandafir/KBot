# SLICE-0033-02 — arborele DDF adus la paritate cu cel din Rezervări (iconițe din `ImageList` + dimensiuni)

Sub-felie a lui 0033, cerere de operator: «fă arborele din DdfView să arate exact ca cel din RezervariView — iconițele din `ImageList` și aceleași font/dimensiuni». Se atinge **numai arborele**.

## Ce s-a schimbat și de ce

**1. `ImageList` are prioritate.** `tree_image_list` era legat prin `tree.NodeImages`, dar `DdfView` folosea numai `DdfIcons.StatusIcon`/`LunaIcon` GDI. Acum urmează `RezervariView.TipIconOf`: cheia din listă, altfel GDI colorat din paletă, fără noduri sau imagini pierdute.

Cheile citite (cele care sunt deja în `tree_image_list`, plus una nouă):

| nod | cheie | în listă azi? |
|---|---|---|
| lună, strânsă | `folder_closed` | da |
| lună, desfăcută | `folder_open` | da |
| revizie încărcată ▲ | `up` | da (scrisă `Up` — căutarea e insensibilă la litere mari/mici, `ImageList.IndexOfKey`) |
| revizie preluată ▼ | `down` | da |
| revizie neutră | `neutral` | **nu** → cade pe GDI până când operatorul pune poza |

Luna folosește acum imaginile distincte închis/deschis deja suportate de arbore, în locul aceleiași imagini GDI pentru ambele stări.

**2. `ItemHeight` 24 → 30**, singura dimensiune încă diferită: `Calibri 9`, `LeftIconSize 16`, `RightIconSize 14` coincideau deja cu `RezervariView`; 0027-03 aliniase antetul/subsolul.

Structura, cheile, etichetele, valorile, tooltipurile și comportamentul rămân. `MinimumCollapsedWidth` (DDF 80, Rezervări 120) păstrează lățimea de strângere pentru panouri cu proporții diferite; nu ține de font/iconițe.

Compatibil cu `PLAN_DdfSubViews` (felia 0032) în ambele sensuri: arborele rămâne în `DdfView.split.Panel1` indiferent dacă refactorizarea sub-paginilor a aterizat sau nu.

## Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.App/Views/DdfView.vb` | constantele de chei; `IconFor` listă-întâi; `LunaIcon(cheie, palette)` nou; `BuildTree` cere două poze de lună |
| `src/KBot.App/Views/DdfView.Designer.vb` | `tree.ItemHeight = 30` |

## Verificat / neverificat

* `dotnet build KBot.sln` — 0 erori (vezi `SLICE-0033-ord-view.md` pentru inventarul de avertismente).
* **Nerandat pe ecran.** Nici arborele DDF, nici efectul noii înălțimi de rând. Operatorul confirmă în designerul Visual Studio + la rulare.
* Lipsa cheii `neutral` din `tree_image_list` rămâne de rezolvat de operator; fallback-ul GDI din tabel păstrează aspectul anterior.
