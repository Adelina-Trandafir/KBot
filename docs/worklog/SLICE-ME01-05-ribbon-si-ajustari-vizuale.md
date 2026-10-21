# ME01-05 — KBotAdmin: ribbon și ajustări vizuale

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: fereastra nu a fost deschisă, nici un test, nici o conexiune la server.**

## Ce s-a cerut (14 puncte ale operatorului) și ce s-a făcut

1. **Ribbon în loc de bara de navigare.** `KBotRibbon` (cel din K-BOT) cu 5 file: *Baze de date* (Comenzi, Interogări unice | Sincronizare, Clonare DEMO),
   *Drepturi* (Conturi și drepturi | Drepturi ADECHIT, Drepturi VERCON | Adaugă cont, Acces la o bază, Rol K-BOT | Cereri, Resetare Login),
   *Monitorizare* (Loguri, Jurnal, Vizite | Server), *Copii de siguranță* (Copii server | Copii locale), *Publicare* (Publicare | Rapoarte VERCON).
   Fiecare buton are o iconiță din stocul K-BOT (`KBot.App\Resources`, legate în `KBotAdmin.vbproj` ca resurse `icon.<nume>`; `AdminIcons.Get`).
   Numărul cererilor în așteptare se scrie pe butonul «Cereri» (ribbon-ul nu are insignă).
2. **Pornește maximizat** (`WindowState = Maximized` în designer; `KBotShellForm` ține maximizarea în zona de lucru).
3. **Butoanele își arată tot textul.** Cauza: `KBotButton` se desenează singur, deci `KBotTableLayoutPanel` nu cere nimic pentru el și
   coloana fixă rămâne cât în designer. `ButtonFit.Apply` (apelat la fiecare afișare a unei pagini) lărgește coloanele fixe până la
   `GetPreferredSize` al butonului, prin `SetColumnWidth` (logic, scalat cu ecranul); nu îngustează niciodată.
4. **Arborele de baze are conturile ca frunze.** Rădăcina `<TOATE BAZELE>` → bază → conturile ei; un cont legat de mai multe baze apare sub fiecare.
   Un clic pe un cont alege baza lui și contul (pagina «Conturi și drepturi» sare pe rândul lui). Ramurile deschise rămân deschise la reîncărcare.
5. **Arborele de conturi de jos a dispărut** (`treeAcc`, `tlyLeft`, `tlyRight`, `navViews`).
6. **Comenzi: explicație sub caseta mare.** `txtExplain` (KBotTextBox doar-citire): «Ce face», «Ce te aștepți», țintă, dacă scrie / cere o valoare.
   Așteptările stau în `CommandsView.Expectations` (după numele comenzii de pe server; fără modificare de server). O comandă nouă pe server
   primește o propoziție generală până e adăugată în listă.
7. **Pagina «Diferență» scoasă** din fereastră (nu mai e în ribbon, nici în `MainForm`). Fișierele `DiffView*.vb` au rămas în proiect, nefolosite (nu se șterg fără acordul operatorului).
8. **Sincronizarea nu mai ia în calcul și nu mai arată bazele lipsă.** Serverul le ignora deja ca țintă, dar scria avertismentul «CAI listează X, dar baza nu există».
   `schema_common.discover_targets` nu-l mai scrie, iar `SyncView` ascunde linia și de la un server nerepornit cu versiunea nouă. Arborele le ascundea deja.
9. **Fila «Drepturi»: butoanele care deschid o fereastră s-au mutat în ribbon**: Drepturi ADECHIT, Drepturi VERCON (RightsForm), Adaugă cont, Acces la o bază, Rol K-BOT (AccountForm).
   Au rămas pe pagină cele care nu deschid altă fereastră: Reîmprospătează, Scoate din bază, Aplică drepturile MariaDB, Creează rolul «Editare lună închisă».
   Metodele sunt acum publice în `RightsView` (`OpenAdechitAsync`, `OpenVerconAsync`, `AddAccountAsync`, `GrantDatabaseAsync`, `ChangeRoleAsync`); butonul din ribbon deschide întâi pagina.
10. **Loguri:** combo, casetă de căutare și butoane au aceeași înălțime (margini identice, 32 px).
11. **Jurnal:** la fel.
12. **Copii server + Copii locale în aceeași filă** a ribbon-ului, «Copii de siguranță».
13. **Interogări unice:** butoanele au lățime suficientă (prin `ButtonFit`, ca la punctul 3).
14. **Rapoarte VERCON: SQL-ul într-un editor.** Control nou în `KBot.Controls\RichText`: `KBotSqlBox` (cuvinte cheie, șiruri, numere, comentarii, nume între ghilimele
    inverse și parametrii `{{TABLE:..}}` / `%(nume)s` colorate din tema activă; lipirea e mereu text simplu).
    **Nu s-a folosit TiSQLEditor:** e o bibliotecă web (CodeMirror 6 + React) și cere WebView2 plus un pachet JavaScript construit cu npm, adică o dependență
    nouă de runtime în KBotAdmin și o integrare pe care nu o pot verifica fără să rulez fereastra. Dacă tot vrei TiSQLEditor, e o felie separată.

Cerința operatorului «toate controalele folosite vor fi cele custom»: tot ce s-a adăugat folosește controale K-BOT (`KBotRibbon`, `KBotTextBox`, `KBotSqlBox`, `KBotTableLayoutPanel`, `KBotButton`).
Au rămas în desenatoarele existente `CheckBox`, `Panel` și `Label` simple: pentru `CheckBox` și `Panel` nu există control K-BOT; `KBotLabel` desenează implicit o margine de 1 px, deci schimbarea
tuturor etichetelor cere o decizie (BorderWidth = 0 pe fiecare).

## Fișiere

Noi: `KBotAdmin/AdminIcons.vb`, `KBotAdmin/ButtonFit.vb`, `src/KBot.Controls/RichText/KBotSqlBox.vb`.
Schimbate: `KBotAdmin/KBotAdmin.vbproj`, `MainForm.vb` + `.Designer.vb`, `Views/CommandsView.vb` + `.Designer.vb`, `Views/RightsView.vb` + `.Designer.vb`,
`Views/SyncView.vb`, `Views/LogsView.Designer.vb`, `Views/AuditView.Designer.vb`, `Views/VerconReportsView.Designer.vb`, `PYTHON/routes/schema_sync/schema_common.py`, `KBotAdmin/README.md`.

## Nevalidat / de știut

- Nimic nu a fost văzut pe ecran: aspectul ribbon-ului (înălțime, încăpere pe 1000 px lățime minimă), iconițele alese, `ButtonFit` la 125 / 150 %, colorarea din `KBotSqlBox`
  (făcută prin selecție, cu desenarea oprită; lipirea ca text simplu) sunt gândite, nu văzute.
- `schema_common.py` (server) trebuie împins de mână de operator; până atunci filtrul din `SyncView` ascunde linia.
- Ajutorul K-BOT nu se schimbă (KBotAdmin nu ajunge la clienți). `KBot.Controls` a primit `KBotSqlBox` (FileVersion neridicat: nu s-a cerut).
