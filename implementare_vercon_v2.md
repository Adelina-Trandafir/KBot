# Implementare VERCON v2

Început: 12.10.2026. Cerință: rescrierea tuturor ecranelor VERCON deja existente în `PYTHON/static`, după mockupurile v2 și folosind controalele custom comune. Referințe: `VERCON/FLUXURI_UI.md`, `VERCON/CONTROALE_CUSTOM_THREAD.md`, `VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md`.

Încheiat local: 12.10.2026. Toate ecranele existente din tabel sunt rescrise. Nu au fost rulate teste, build, verificări de sintaxă sau aplicația. Starea IMPLEMENTAT_LOCAL descrie modificările de cod; nu certifică funcționarea în browser.

## Reguli pentru continuarea în alt chat

- Se lucrează în checkoutul curent. Nu se execută teste, build, verificări de sintaxă sau aplicația fără cererea explicită a utilizatorului. Citirea codului și proiectarea sunt permise.
- Nu se execută modificări de scheme MariaDB. Această rescriere este de interfață; API-urile, drepturile, contextul și regulile de salvare se păstrează.
- Grile DataGrid compacte ca NIR, selecții Combobox, date DatePicker, arbori TreeView; meniuri/dialoguri/mesaje/PDF comune. Draftul se salvează la comanda documentului, nu prin HTTP la fiecare celulă.
- Arborele documentelor este în dreapta în consultare, ascuns în editor. Lista de alimente are mai multe BC-uri și nu repetă consumul.
- Se completează acest fișier imediat după fiecare ecran/familie, apoi se continuă cu următorul. „IMPLEMENTAT_LOCAL” înseamnă cod scris, nu verificat sau acceptat.

## Progres

| Ecran / familie | Stare |
|---|---|
| Structură comună, context și navigare | IMPLEMENTAT_LOCAL |
| NIR, inclusiv extensia MF | IMPLEMENTAT_LOCAL |
| Bon de consum | IMPLEMENTAT_LOCAL |
| Storno și retur | IMPLEMENTAT_LOCAL |
| Predare, transfer, casare | IMPLEMENTAT_LOCAL |
| Lista de alimente și alegere BC | IMPLEMENTAT_LOCAL |
| Registru MF | IMPLEMENTAT_LOCAL |
| Nomenclatoare și variante de preț | IMPLEMENTAT_LOCAL |
| Setări gestiune | IMPLEMENTAT_LOCAL |
| Editorul de preview existent | IMPLEMENTAT_LOCAL |

Stocuri, registrul OI și rapoartele complete nu au în prezent un ecran funcțional în această pagină; existența lor în mockupuri nu înseamnă că API-urile sunt implementate. Se consemnează separat la final, fără date fictive în fluxul real.

## Integrarea finală și predarea

- Toate familiile comune sunt utilizate: DataGrid, TreeView, Combobox, DatePicker, openMenu/closeMenu, confirmBox/reasonBox/pickBox, viewer PDF, mesaje/jurnal și timer de sesiune. Layouturile grilelor sunt conectate prin providerul comun și ID-uri proprii VERCON.
- `js/vercon/ui.js` centralizează montarea și cleanupul. `js/vercon/snapshot-pdf.js` produce local PDF-uri paginate ale valorilor din previzualizare, cu diacritice, în viewerul comun; se pot descărca și deschide pentru tipărire. PDF-ul este rasterizat și nu înlocuiește rapoartele oficiale încă neimplementate.
- Antetele și celulele păstrează valori canonice, sumele monetare fiind șiruri. Calculele locale sunt informative; serverul păstrează autoritatea asupra valorilor, disponibilului, drepturilor și reviziilor.
- POST-urile incerte păstrează corpul exact și cheia; câmpurile sunt blocate până la reluarea salvării sau renunțare. Editorul preview folosește reconcilierea existentă. O salvare confirmată este afișată înainte de reîncărcarea arborelui, astfel încât eșecul reîncărcării să nu transforme salvarea într-un răspuns incert.
- Meniurile și overlayurile Combobox se montează în dialogul gazdă; dropdownurile se poziționează la deschidere și scroll/resize. Footerul DataGrid respectă disabled/span și permite totaluri explicite independente de filtrare.
- BC cere disponibilul pentru data documentului și produsul selectat, excluzând bonul editat. Prețurile produselor existente pe BC/BP sunt păstrate din document. BT/BS păstrează lotul și disponibilul predătorului.
- Navigarea ține cont de ciorne, serializarea deschiderii formularelor și context; accesul exclusiv se actualizează după ocupare/pierdere. Timerul existent are cleanup; nu pornește un al doilea monitor.
- Fișiere comune adaptate: `js/dgv/datagrid.js`, `js/dgv/editing.js`, `js/components/combobox/combobox-ui.js`, `js/adechit/menu.js`, `js/adechit/session.js`. Restul modificărilor sunt în `vercon.html`, `css/vercon.css` și `js/vercon`.
- Harta componentelor pentru alt thread este actualizată în `VERCON/CONTROALE_CUSTOM_THREAD.md`. Nu s-au schimbat scheme, nu s-a executat SQL și nu s-au adăugat API-uri fictive.

Pentru o continuare: citiți acest jurnal și harta componentelor, apoi mockupurile/FLUXURI_UI. Validarea în browser și testele necesită o cerere explicită a utilizatorului conform AGENTS.md. Implementarea ecranelor lipsă (stocuri, registru OI, rapoarte oficiale) este o etapă separată; rescrierea ecranelor existente este încheiată.

### 1. Structură comună, context și navigare

Scrise: `PYTHON/static/js/vercon/ui.js`, modificări în `vercon.html`, `app.js`, `css/vercon.css`. Header cu căutare în ecranul curent, ajutor, temă și schimbarea unității; contextul folosește Combobox și pentru tip. Navigarea folosește operațiile primite de la server și meniul comun pentru nomenclatoare. Spațiu de lucru pe lățimea paginii și stil compact NIR; CSS-urile tuturor controalelor sunt încărcate.

Adaptoare comune pentru câmpuri, grile, arbori, meniuri, modale și preview/PDF; resursele au scope și cleanup. DataGrid a primit totaluri explicite independente de filtrare și footerAction dezactivabil. Editorul de listă comun a primit opțiuni per rând, căutare și eticheta `label` corectă. Timerul comun de sesiune întoarce cleanup; politica sesiunii nu se schimbă și nu se pornește un al doilea monitor. Nerulat/netestat.

### 2. NIR, inclusiv MF

Rescris `js/vercon/docs.js`: antet în trei coloane (furnizor/cont/comisie, factură cu valoare activă, NIR), o singură grilă DataGrid. La MF aceeași grilă se extinde cu inventar/data intrării/DNF/DF/valoare lunară. Combobox cu căutare pentru produse/TVA, DatePicker pentru date. Arbore TreeView furnizor → dată → factură/total în dreapta. Totalurile documentului sunt explicite; calculele locale sunt informative, regulile de salvare rămân pe server. Nerulat/netestat.

### 3. Bon de consum

Același modul: panou TreeView pentru produse/categorii în stânga, detalii produs și disponibil pentru liniile draftului, grilă DataGrid în centru, arbore bonuri în dreapta la consultare. Produsele din editor sunt limitate la cont. Disponibilul se cere pentru data bonului, excluzând BC editat; răspunsurile vechi sunt ignorate. Renunță păstrează documentul anterior. Nerulat/netestat.

### 4. Storno și retur

Selector sursă Combobox, cantități sursă/deja/rămas în DataGrid, prețurile sursei păstrate și DatePicker. Numai liniile cu cantitate completată intră în payload. Arborele documentelor și stările consultare/editor sunt comune. Nerulat/netestat.

### 10. Editorul de preview existent

Rescris `js/vercon/editor.js`: antet cu câmpurile comune, produse în DataGrid, footer Adaugă rând, meniu ⋮ și previzualizarea comună. Draftul extern și protocolul preview existent se păstrează. După răspuns incert, editarea este oprită până la reconciliere; conflictul păstrează ciorna și revizia originală. Renunță restaurează ultima stare acceptată. Nerulat/netestat.

### 8. Nomenclatoare și variante de preț

Rescris `js/vercon/nomenclatoare.js`: liste DataGrid pentru nomenclatoarele descrise de server și TreeView categorie → produs. Formulare comune cu Combobox pentru referințe, câmpuri compacte și draft separat; variantele urmează produsul selectat. Schimbarea produsului ține cont și de ciorna variantei. Filtrul active/inactive/toate folosește pickBox, meniurile ⋮ și confirmările sunt comune; dezactivarea păstrează documentele salvate. Salvarea, request_key și reviziile rămân în protocolul existent; conflictele păstrează datele introduse. Nerulat/netestat.

### 9. Setări gestiune

Același modul: câmpuri comune pentru zecimale/scară și DataGrid readonly pentru contoare. Editabilitatea depinde de drepturi și de absența datelor; limita 0–4 este afișată și aplicată în formular. Mesajele serverului rămân lângă câmpuri. Nerulat/netestat.

### 7. Registru MF

Rescris `js/vercon/mf.js` cu DataGrid, DatePicker în celulă și meniul ⋮. Modificarea datei/DNF/DF se păstrează într-o ciornă pentru mijlocul fix selectat, apoi se salvează împreună cu revizia originală. Identitatea, valoarea, deținătorii și casările sunt readonly; conflictul nu aruncă ciorna. Căutare din header și preview comune. Nerulat/netestat.

### 6. Lista de alimente și alegere BC

Rescris `js/vercon/lista.js`: trei panouri de mese alăturate, fiecare cu un singur rând P/VMD/V/D în DataGrid și grila de feluri. Porțiile felurilor reflectă masa; câmpul Tip existent rămâne separat. Antet cu DatePicker, totaluri BC/listă/diferență, footer Adaugă rând și meniuri comune. Popup modal cu TreeView primitor → dată → BC, selecție urmată de Adaugă BC; eliminarea scoate doar legătura din ciornă. Arbore liste an → lună → dată → număr/valoare ascuns în editor. Salvare atomică, revizie păstrată, cerere POST identică la răspuns incert, preview și reasonBox comune. Nerulat/netestat.

### 5. Predare, transfer și casare

Predare: cont de intrare → primitor/cont primitor și produse din cont. Transfer/casare: Combobox produse din loturile eligibile ale predătorului, cu protecție împotriva dublării aceluiași lot în draft. Casarea nu are primitor. Fără panouri Perioada/Deținători; arborele familiei în dreapta în consultare. Schimbarea predătorului cere confirmarea eliminării liniilor existente. Footer Adaugă rând, total și meniul ⋮ comune. Anularea folosește reasonBox; istoricul și previzualizarea ciornei/documentului folosesc dialogul și DataGrid comune. Payloadurile, reviziile și cheile de creare păstrate; nicio schemă schimbată. Nerulat/netestat.
