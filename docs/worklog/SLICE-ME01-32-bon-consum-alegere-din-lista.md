# SLICE-ME01-32 — Ștergerea bonului de consum: bonul se alege dintr-o listă

10.11.2026. Continuare a [ME01-31](SLICE-ME01-31-stergere-bon-consum.md). Compilat curat (`KBotAdmin.vbproj`, 0 avertismente); nerulat pe ecran, nerulat pe server.

## Ce s-a schimbat și de ce

În **Comenzi → VERCON → Șterge definitiv un bon de consum**, «Rulează» nu mai șterge și nu mai cere confirmare. Caută bonurile pentru valoarea scrisă (număr sau `ID …`) și le pune într-un combo (`cmbBon`, `KBotComboBox`) care apare între bara de comenzi și rezultat, împreună cu butonul «Șterge bonul» (`btnDeleteBon`). Un singur bon găsit este preselectat; la mai multe, nu e ales nimic și operatorul trebuie să aleagă.

«Șterge bonul» șterge DOAR bonul ales: repetă analiza serverului pentru identificatorul exact al bonului ales (`ID Subunitate/Gestiune/Exercitiu/IdBonConsum`), verifică că jetonul de confirmare primit începe cu cheia bonului ales, cere confirmarea cu eticheta lui și abia apoi apelează ștergerea. După ștergere, bonul pleacă din listă, celelalte rămân neatinse. Dacă bonul nu poate fi șters (retur, listă de alimente etc.), se afișează motivul serverului.

Combo-ul se ascunde când se schimbă comanda, baza sau numărul scris în casetă (lista aparține căutării care a produs-o). Rândul nou din `tlyMain` e Absolute și se închide/deschide cu `SetRowCollapsed`.

Serverul nu s-a schimbat: se folosesc aceleași comenzi `vercon_bc_analiza_stergerii` și `vercon_bc_sterge`, cu aceleași gărzi (blocare gestiune, recitire, amprentă).

## Fișiere

`KBotAdmin/Views/CommandsView.vb`, `KBotAdmin/Views/CommandsView.Designer.vb`.

## Nerezolvat / neverificat

- Nu s-a văzut pe ecran: aspectul rândului nou (înălțime, combo cu texte lungi), comportamentul `PlaceholderText` pe un `KBotComboBox` needitabil.
- Nu s-a rulat contra serverului; formatul rândurilor (`identificator` / `bon`, `… | ID s/g/e/i`) e citit din `vercon_bc_delete.py`.
- Ajutorul K-BOT nu e atins: KBotAdmin e instrumentul privat al proprietarului.
