# SLICE-0033-02 — arborele DDF adus la paritate cu cel din Rezervări (iconițe din `ImageList` + dimensiuni)

Sub-felie a lui 0033, cerere de operator: «fă arborele din DdfView să arate exact ca cel din
RezervariView — iconițele din `ImageList` și aceleași font/dimensiuni». Se atinge **numai arborele**.

## Ce s-a schimbat și de ce

**1. Iconițele se rezolvă acum ÎNTÂI din `ImageList`.** `DdfView` desena toate iconițele cu formele
GDI din `DdfIcons` (`StatusIcon` / `LunaIcon`), deși `tree_image_list` exista deja în designer și era
legat prin `tree.NodeImages` — pozele erau acolo, dar nimeni nu le cerea. Acum se folosește regula
listă-întâi din `RezervariView.TipIconOf`: se caută cheia în listă și, doar dacă lipsește, se cade pe
forma GDI colorată din paletă. Nicio poză nu se pierde, niciun nod nu rămâne gol.

Cheile citite (cele care sunt deja în `tree_image_list`, plus una nouă):

| nod | cheie | în listă azi? |
|---|---|---|
| lună, strânsă | `folder_closed` | da |
| lună, desfăcută | `folder_open` | da |
| revizie încărcată ▲ | `up` | da (scrisă `Up` — căutarea e insensibilă la litere mari/mici, `ImageList.IndexOfKey`) |
| revizie preluată ▼ | `down` | da |
| revizie neutră | `neutral` | **nu** → cade pe GDI până când operatorul pune poza |

Câștig secundar: luna are acum două poze distincte (închis / deschis), pe care structura arborelui
le suporta dintotdeauna — înainte ambele stări primeau aceeași imagine GDI.

**2. `ItemHeight` 24 → 30.** Singura dimensiune care mai diferea. Fontul (`Calibri 9`) și
dimensiunile iconițelor (`LeftIconSize 16`, `RightIconSize 14`) erau deja identice cu ale
`RezervariView` — felia 0027-03 aliniase deja benzile de antet/subsol.

**Nimic altceva.** Structura, cheile nodurilor, etichetele, valorile, tooltip-urile și
comportamentul arborelui DDF sunt neatinse. `MinimumCollapsedWidth` (80 la DDF, 120 la Rezervări)
a rămas cum era: e o lățime de strângere, nu o dimensiune de font/iconiță, iar arborele DDF stă
într-un panou cu alte proporții.

Compatibil cu `PLAN_DdfSubViews` (felia 0032) în ambele sensuri: arborele rămâne în
`DdfView.split.Panel1` indiferent dacă refactorizarea sub-paginilor a aterizat sau nu.

## Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.App/Views/DdfView.vb` | constantele de chei; `IconFor` listă-întâi; `LunaIcon(cheie, palette)` nou; `BuildTree` cere două poze de lună |
| `src/KBot.App/Views/DdfView.Designer.vb` | `tree.ItemHeight = 30` |

## Verificat / neverificat

* `dotnet build KBot.sln` — 0 erori (vezi `SLICE-0033-ord-view.md` pentru inventarul de avertismente).
* **Nerandat pe ecran.** Nici arborele DDF, nici efectul noii înălțimi de rând. Operatorul confirmă
  în designerul Visual Studio + la rulare.
* Cheia `neutral` **nu este** în `tree_image_list` — până când operatorul pune poza, reviziile
  neutre rămân pe forma GDI (adică arată exact ca înainte). Nu e o regresie; e fallback-ul.
