# ADECHIT — starea detaliată AD20–AD29

## Slice AD20

Corecții pentru asocierea extraselor și introducerea manuală a documentelor în Alte plăți.

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-AD20-01 | Închide jos-stânga, Previzualizare automată lângă Asociază jos-dreapta; Număr primul câmp editabil, obligatoriu și unic pe subunitate/an calendaristic, cu refuz în dialogul comun | SCRIS LOCAL; fără teste, SQL sau publicare; înregistrare în așteptarea confirmării explicite | [Butoane și număr Alte plăți](../SLICE-AD20-01-extrase-butoane-numar-alte-plati.md) |
| SLICE-AD20-02 | Prima coloană a listei încasărilor afișează DataBanca cu titlul Data bancă | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Data bancă în asociere](../SLICE-AD20-02-asociere-data-banca.md) |
| SLICE-AD20-03 | Căutare copil după nume/CNP în toate grupele vizibile, fără limitare la lună, cu selectare și poziționare automată pe grupă/copil | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Căutare copil în asociere](../SLICE-AD20-03-cautare-copil-asociere-extrase.md) |
| SLICE-AD20-04 | Selectare integrală a textului la intrarea în căutarea copilului; eticheta în stânga comboboxului | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Text selectat și etichetă în stânga](../SLICE-AD20-04-cautare-selectare-text-eticheta-stanga.md) |
| SLICE-AD20-05 | Panou de înregistrare în dreapta de la partea de sus a tabelului de extrase; butoane în footer de 56 px, dublu față de anterior | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Panou sus în dreapta și butoane duble](../SLICE-AD20-05-panou-inregistrare-dreapta-butoane-duble.md) |
| SLICE-AD20-06 | Copii lărgit pentru CNP complet și SID-ul lunii; suma propusă din SID sau întreaga încasare dacă SID este zero; Plătitori fără Banca | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [CNP, SID, sumă și fără Banca](../SLICE-AD20-06-copii-cnp-sid-suma-fara-banca.md) |
| SLICE-AD20-07 | Transfer integral către Plătitori a 25% din lățimea listei Copii | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Lățime Copii transferată Plătitori](../SLICE-AD20-07-latime-copii-transfer-platitori.md) |
| SLICE-AD20-08 | Casetă cu următorul număr anual din AlteDoc, număr distinct pentru fiecare copil repartizat și verificare la salvare | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Numere distincte la asociere](../SLICE-AD20-08-numere-documente-asociere-copii.md) |
| SLICE-AD20-09 | Ultimul număr AlteDoc calculat numai în anul calendaristic în curs, după data curentă din România, la propunere și salvare | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Anul calendaristic în curs](../SLICE-AD20-09-numar-an-calendaristic-in-curs.md) |
| SLICE-AD20-10 | Număr document editabil, memorat individual pe copil și verificat pentru unicitate/disponibilitate în anul curent | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Număr document editabil](../SLICE-AD20-10-numar-document-editabil-asociere.md) |
| SLICE-AD20-11 | SIC lângă SID în lista Copii, pentru aceeași lună a înregistrării | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [SIC în lista Copii](../SLICE-AD20-11-sic-in-lista-copii-asociere.md) |
| SLICE-AD20-12 | Suma propusă din extras minus repartizări, validare la lost focus și salvare numai cu total exact egal cu extrasul | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Sumă din extras și validare](../SLICE-AD20-12-suma-extras-rest-validare-lost-focus.md) |
| SLICE-AD20-13 | Data_extras din antetul XML în DGV-ul asocierii și imediat după Încărcat în istoricul încărcărilor | TESTAT ȘI VALIDAT DE ADELINA (versiunea finală); ÎNREGISTRAT PENTRU SERVER | [Data extras în cele două liste](../SLICE-AD20-13-data-extras-asociere-istoric.md) |
| SLICE-AD20-14 | Arbore LUNA/ANUL și filtrarea încasărilor după Data_extras din antetul XML | VALIDAT DE ADELINA; ÎNREGISTRAT PENTRU SERVER | [Arbore după data antetului](../SLICE-AD20-14-arbore-perioade-data-header-extras.md) |
| SLICE-AD20-15 | SQL manual pentru ștergerea plăților din extrase în noiembrie 2026 și eliberarea repreluării, cu păstrarea legăturilor copil–plătitor | SCRIS LOCAL; SQL NERULAT; NEÎNREGISTRAT, în așteptarea confirmării explicite | [Resetare plăți din extrase](../SLICE-AD20-15-reset-plati-extrase-luna-curenta.md) |

### Current focus

05.11.2026: AD20-15 pregătește numai fișierul SQL manual pentru plățile provenite din extrase în luna curentă, precizată de Adelina: noiembrie 2026. Păstrează plătitorii și istoricul lor; elimină rezervările extras–plată pentru repreluare. SQL-ul nu a fost executat și fișierele noi nu sunt înregistrate. AD20-14 are cele șase fișiere înregistrate după «validat. inregistreaza-l»; AD20-01–13 rămân validate și înregistrate anterior (31 de fișiere distincte, 103 înregistrări). Agentul nu a executat teste, publicare sau Git. Următoarea subfelie liberă: **AD20-16**.

### Open threads

- AD20-15: rularea manuală și confirmarea rezultatelor de către Adelina rămân de făcut; înregistrarea celor cinci fișiere proprii așteaptă confirmarea explicită. Scriptul refuză lunile închise și situațiile salvate afectate, fără a le modifica.
- AD20-01–14 sunt validate și înregistrate; publicarea efectivă și Git pe server nu au fost executate de agent în această etapă.
- Capturile 12/13/14 pentru încărcare/asociere și 16 pentru Alte plăți rămân de refăcut cu blur puternic pe datele sensibile; nu s-au realizat capturi noi. Ajutorul și ghidul textual sunt actualizate.
