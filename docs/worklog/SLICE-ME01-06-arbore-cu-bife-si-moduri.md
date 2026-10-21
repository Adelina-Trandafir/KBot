# ME01-06 — KBotAdmin: arborele cu bife, două moduri, «Selectează tot»

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: fereastra nu a fost deschisă.**

## Ce s-a cerut și ce s-a făcut

1. **Bară deasupra arborelui, lipită de el**, cu două butoane (`navMode`, `KBotNavList`): «Baze de date» (stânga) și «Utilizatori» (dreapta).
   În modul «Utilizatori» rădăcinile sunt utilizatorii, iar frunzele bazele fiecăruia (doar baze care există pe server). Clic pe un utilizator = contul ales,
   toate bazele ca context; clic pe o bază de sub el = acel cont în acea bază.
2. **Căsuță de bifat pe fiecare rând** (`CheckBoxes = True`, `HasCheckBox` pe toate rândurile). Bifa unui rând cu copii bifează și copiii; un părinte cu copii bifați doar o parte
   e «nedeterminat». Bifele frunzelor se păstrează la reîncărcarea arborelui și la schimbarea modului. `AdminContext.CheckedDatabases` / `CheckedAccounts` arată ce e bifat;
   **nicio pagină nu le folosește încă**: operatorul nu a spus ce face bifa.
3. **Titlul arborelui a dispărut** (`HeaderVisible = False`).
4. **«Selectează tot»** sub arbore (`chkAll`): bifează sau debifează toate rândurile; se aliniază singur cu arborele (e bifat doar când toate rădăcinile sunt bifate).

Adăugat în plus, din cerința «toate controalele custom»: **`KBotCheckBox`** (`KBot.Controls\CheckBox`), un `CheckBox` desenat din temă (aceeași suprafață ca `CheckBox`).
Toate căsuțele simple din `KBotAdmin` au trecut pe el (Loguri, Vizite, Sincronizare, Publicare, Editorul de raport, fereastra de parolă).

Comenzi: caseta «Ce face» e acum de două ori mai înaltă (240 px) și cu fontul cu 2 pt mai mare (Segoe UI 11).

## Fișiere
Nou: `src/KBot.Controls/CheckBox/KBotCheckBox.vb`. Schimbate: `KBotAdmin/MainForm.vb` + `.Designer.vb`, `AdminContext.vb`, `Views/CommandsView.Designer.vb`, designerele cu căsuțe de bifat.

## Nevalidat
Nimic văzut pe ecran: desenarea `KBotCheckBox`, bifele din arbore (propagare, stări parțiale), comutarea modurilor și rândul «Utilizatori» la clic sunt gândite, nu văzute.
Bifarea unui rând îl și alege (comportamentul arborelui), deci schimbă baza-context.

## Adăugat după prima predare

- **Arborele se redimensionează cu mausul:** un `SplitContainer` (`splMain`) între arbore și pagini (bara de 8 px; arborele minim 180 px, paginile minim 400 px; la redimensionarea ferestrei arborele își păstrează lățimea). Nu există un separator K-BOT; `ThemeManager` colorează `SplitContainer`-ul, ca în restul programului.
- **Imaginea rămasă după prima închidere în systray:** s-a observat că dispare doar la a doua închidere. Presupunere (nevăzută pe ecran): animația de ascundere a Windows lasă poza când fereastra își face din nou mânerul (`ShowInTaskbar`) imediat după `Hide()`. Animațiile ferestrei sunt oprite (`DwmSetWindowAttribute` în `OnHandleCreated`); repictarea zonei de după ascundere a rămas.
- **Jurnal de diagnostic pentru închiderea în systray** (imaginea tot rămânea): `KBotAdmin/TrayLog.vb` scrie `<AppDir>\Logs\minimizare_tray.log`. Per închidere: starea ferestrei (mâner, vizibilă, stare, dreptunghi, stil) înainte de `Hide`, după `Hide` și după `ShowInTaskbar = False`; mesajele ferestrei (afișare, ascundere, mutare, pictare, creare, distrugere) timp de 3 s; lista tuturor ferestrelor programului și a formularelor deschise imediat, la 0,4 s și la 2 s (poza ar putea fi o a doua fereastră: tooltip, fly-out). Nu schimbă comportamentul ferestrei. Cauza rămâne necunoscută până se citește jurnalul.
