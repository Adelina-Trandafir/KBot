# ME02-06 / ME02-07 / ME02-08 — verificare + export SQL, istoricul unui rând, retenție

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT.**

## ME02-06 — Verificare și restaurare

- **Verifică** (`BackupStore.Verify`): deschide fiecare obiect (nume = HMAC-ul conținutului), fiecare instantaneu, controlează că fiecare copie își găsește toate obiectele;
  raportează fișiere stricate, lipsă și „fără stăpân”. Nu atinge serverul.
- **Export SQL** (`Backup/BackupSql.vb`): scrie o copie (toate tabelele, apoi views/triggere/rutine/evenimente) ca fișier SQL (`.sql` sau `.sql.gz`) pentru o **bază de probă** `zz_restore_<baza>_<data>`.
  Numele țintă trebuie să înceapă cu `zz_`: fișierul nu poate suprascrie o bază vie. Valorile sunt scrise ca text între ghilimele (MariaDB le convertește), binarele ca `0x…`;
  `FOREIGN_KEY_CHECKS=0` cât se încarcă; `DEFINER` scos din views/triggere. Nu atinge nicio bază: fișierul îl rulează operatorul.
- **Abatere de la plan**: planul vorbea despre încărcarea într-o bază de probă prin server. Am ales exportul ca fișier SQL: nu cere nicio scriere nouă pe server, iar rularea o decide operatorul.
  Încărcarea automată pe server rămâne **neconstruită**.

## ME02-07 — «Istoricul unui rând» (time-machine)

- Fereastra `RecordHistoryForm`: tabela aleasă (cu o copie selectată în pagina «Copii locale»), cheia primară scrisă de operator (părțile separate prin `;`), apoi lista tuturor copiilor
  bazei: unde rândul **apare / e neschimbat / modificat / a dispărut / nu există**, cu coloanele schimbate; jos, valorile rândului în copia aleasă, cu valoarea de dinainte la coloanele schimbate.
- **Copiază SQL pentru această versiune**: pune în clipboard `INSERT … ON DUPLICATE KEY UPDATE` care readuce rândul la acea versiune. Nu îl rulează.
- Citește doar din depozitul local; o pagină identică în mai multe copii se deschide o singură dată.
- Limita onestă: istoricul e la fel de fin ca frecvența copiilor (zilnic ⇒ nu se vede ce s-a întâmplat între două rulări). Pagina «Copii locale» arată și, pe fiecare tabelă a unei copii, câte pagini s-au schimbat față de copia precedentă.
- **Neconstruit**: răsfoirea tuturor rândurilor unei tabele la un moment dat (există doar exportul SQL și istoricul pe cheie); comparația completă între două date.

## ME02-08 — Retenție

- `BackupRetention`: pentru fiecare bază păstrează ultima copie a fiecăreia dintre ultimele **N zile**, **N săptămâni ISO**, **N luni** (14 / 8 / 12, în `admin_settings.json`), ultima copie oricât de veche,
  **toate copiile făcute de mână, fără limită de timp** (hotărâre operator: se șterg doar de mână) și orice copie cu data ilizibilă. Șterge restul și fișierele la care nu mai trimite nimic (inclusiv resturi ale unei rulări eșuate).
  Cum toate copiile sunt acum manuale (fără copie automată), «Curăță» nu mai șterge practic decât resturi de fișiere.
- Butonul **Șterge**: șterge copia aleasă (cu confirmare) și fișierele pe care nu le mai folosește nicio altă copie. Dacă vreo copie nu se poate deschide, nu șterge fișiere (ca să nu piardă ceva de care are nevoie), ci cere întâi «Verifică».
- **Curăță** arată întâi ce se șterge și cât se eliberează, cere confirmare; ia lacătul depozitului (nu rulează cât se scrie o copie) și recalculează ce e „fără stăpân” după lacăt.
- Se aplică și automat la sfârșitul copiei periodice.

## Nevalidat / limite

- **Nimic nu a rulat.** Mai ales exportul SQL trebuie încercat pe o bază mică într-un MariaDB de probă (potrivirea tipurilor, coloanele generate, ordinea creării) înainte să te bazezi pe el.
- Ghilimelele din jurul numerelor sunt valide în MariaDB cu `sql_mode` fără strict; fișierul pune `sql_mode='NO_AUTO_VALUE_ON_ZERO'`.
- Istoricul unui rând caută cheia prin toate paginile tabelei (nu folosește intervalele), deci pe tabele foarte mari prima căutare e lentă.
- Regulile de păstrare (14/8/12 și un an pentru manuale) sunt alegerea mea, nu a operatorului: se schimbă în `admin_settings.json`.

## Fișiere

`KBotAdmin/Backup/BackupSql.vb`, `BackupReader.vb`, `BackupRetention.vb` (noi), `KBotAdmin/RecordHistoryForm.vb|.Designer.vb` (nou).
