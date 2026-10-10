# SLICE-ADE5-12 — referințe la taxe lipsă importate ca NULL

Data: 09.10.2026. Stare: CONSTRUIT LOCAL; fără teste sau migrare.

## Schimbare

La cererea utilizatorului, LunaD.IDV și Prezenta.IDV intră în lista explicită
DanglingAllowed. FixLinks transformă în Nothing numai referințele fără părinte
în ValoriTaxe, inclusiv 0 dacă nu există taxa cu IDV=0. Planul afișează numărătorile
ca observații, nu ca blocaje. Rândurile și valorile de business se păstrează;
referințele existente sunt remapate de writer. SQL primește DBNull.Value.

## Fișiere și verificări

- ADECHIT/ADE.Migrator/AdeSchema.vb și README.md.
- ADECHIT_STATUS.md, PLAN_IMPLEMENTARE.md și registrul ADE0–ADE9.

Build în `bin/Debug/cnp-null`: 0 erori, 0 avertismente (include și ADE5-11).
Dosarul obișnuit are DLL-uri folosite de un proces deschis; nu s-a oprit procesul.
Verificare statică: exportul local 000_DEMO permite NULL în ambele coloane;
FixLinks rulează înainte de scriere și păstrează referințele existente.
Nu s-au rulat teste automate, probe vizuale sau migrare; MDB-urile și serverul
rămân nemodificate. Utilizatorul recitește sursa și verifică faptul că cele 3/2305
legături raportate anterior apar la conversii, nu la blocaje, apoi testează destinația.
