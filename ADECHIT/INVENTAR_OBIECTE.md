# SLICE-ADE0-02 — inventarul obiectelor Access

Data: 07.10.2026. Acoperire: 66 tabele, 44 formulare, 27 interogări, 18 rapoarte (155 obiecte).
Clasificare statică, nu dovadă de utilizare în producție și nu autorizație de ștergere.
Un obiect condiționat sau legacy nu este declarat inutil. Datele și traseele reale pot schimba încadrarea.

Vezi [maparea fluxurilor](MAPARE_ACCESS_WEB.md) și [contractul calculelor](CONTRACT_CALCULE.md).
Modulele, clasele și macrocomenzile sunt referințe pentru trasee; acest tabel inventariază cele patru categorii de mai jos.
Prefixul AD_ este propus numai pentru datele persistente incluse; cele șapte cache-uri _L nu se migrează.

## tables — 66

| Obiect / sursă | Tratament |
|---|---|
| [_Mail](ACCESS_SOURCES/tables/_Mail.md) | CONDIȚIONAT — configurări/istoric mail; funcția de business rămâne de stabilit. |
| [_Struct_](ACCESS_SOURCES/tables/_Struct_.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [_TBLS_](ACCESS_SOURCES/tables/_TBLS_.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [@CFGs](ACCESS_SOURCES/tables/%40CFGs.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [@COMP](ACCESS_SOURCES/tables/%40COMP.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [@MutaCopil](ACCESS_SOURCES/tables/%40MutaCopil.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [A](ACCESS_SOURCES/tables/A.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [AlteDoc](ACCESS_SOURCES/tables/AlteDoc.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [B](ACCESS_SOURCES/tables/B.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Banci](ACCESS_SOURCES/tables/Banci.md) | REFERINȚĂ — verificare necesitate pentru conturi/sume în litere; reutilizare comună unde există. |
| [BonuriF](ACCESS_SOURCES/tables/BonuriF.md) | EXCLUS FUNCȚIONAL — fără emitere/import ca modul; contribuțiile istorice la Plati se reconciliază. |
| [cacat](ACCESS_SOURCES/tables/cacat.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [CFGs](ACCESS_SOURCES/tables/CFGs.md) | MIXT — CH către Unitati_Chitante; alte configurări se decid individual. |
| [Chitante](ACCESS_SOURCES/tables/Chitante.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Chitantex](ACCESS_SOURCES/tables/Chitantex.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [COMP](ACCESS_SOURCES/tables/COMP.md) | CONDIȚIONAT — compensare manuală între persoane; motorul 2021 nu o citește. |
| [DatePickerDate](ACCESS_SOURCES/tables/DatePickerDate.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [Delegati_L](ACCESS_SOURCES/tables/Delegati_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Delegati](ACCESS_SOURCES/tables/Delegati.md) | EXCLUS 08.10.2026 — decizia utilizatorului; nu se portează. |
| [Erori Intrare](ACCESS_SOURCES/tables/Erori%20Intrare.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Explicatie_Lunara](ACCESS_SOURCES/tables/Explicatie_Lunara.md) | DERIVAT — rezultat de calcul/raport; fără tabel comun golit între utilizatori. |
| [Facturi](ACCESS_SOURCES/tables/Facturi.md) | EXCLUS FUNCȚIONAL — fără emitere/import ca modul; contribuțiile istorice la Plati se reconciliază. |
| [FisaCont](ACCESS_SOURCES/tables/FisaCont.md) | DERIVAT — rezultat de calcul/raport; fără tabel comun golit între utilizatori. |
| [Grupe_L](ACCESS_SOURCES/tables/Grupe_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Grupe](ACCESS_SOURCES/tables/Grupe.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [IMPXLS](ACCESS_SOURCES/tables/IMPXLS.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [LIT](ACCESS_SOURCES/tables/LIT.md) | REFERINȚĂ — verificare necesitate pentru conturi/sume în litere; reutilizare comună unde există. |
| [LunaD_L](ACCESS_SOURCES/tables/LunaD_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [LunaD](ACCESS_SOURCES/tables/LunaD.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Luni](ACCESS_SOURCES/tables/Luni.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Mail](ACCESS_SOURCES/tables/Mail.md) | CONDIȚIONAT — configurări/istoric mail; funcția de business rămâne de stabilit. |
| [MutaCopil](ACCESS_SOURCES/tables/MutaCopil.md) | EXCLUS 08.10.2026 — decizia utilizatorului; nu se portează. |
| [OPuri](ACCESS_SOURCES/tables/OPuri.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Plati](ACCESS_SOURCES/tables/Plati.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Platitori_L](ACCESS_SOURCES/tables/Platitori_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Platitori_sub_L](ACCESS_SOURCES/tables/Platitori_sub_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Platitori_sub](ACCESS_SOURCES/tables/Platitori_sub.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Platitori](ACCESS_SOURCES/tables/Platitori.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Platix](ACCESS_SOURCES/tables/Platix.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Prezenta_L](ACCESS_SOURCES/tables/Prezenta_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Prezenta_sub_L](ACCESS_SOURCES/tables/Prezenta_sub_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Prezenta_sub](ACCESS_SOURCES/tables/Prezenta_sub.md) | EXCLUS 08.10.2026 — decizia utilizatorului; nu se portează. |
| [Prezenta](ACCESS_SOURCES/tables/Prezenta.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Retur](ACCESS_SOURCES/tables/Retur.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [RunOnce](ACCESS_SOURCES/tables/RunOnce.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [Situatie_buget](ACCESS_SOURCES/tables/Situatie_buget.md) | DERIVAT — rezultat de calcul/raport; fără tabel comun golit între utilizatori. |
| [Situatie](ACCESS_SOURCES/tables/Situatie.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Solduri_Lunare](ACCESS_SOURCES/tables/Solduri_Lunare.md) | DERIVAT — rezultat de calcul/raport; fără tabel comun golit între utilizatori. |
| [SQL](ACCESS_SOURCES/tables/SQL.md) | CONFIGURARE — lipsesc rândurile SQL din export; varianta veche le citește. |
| [SS_Buget](ACCESS_SOURCES/tables/SS_Buget.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [tblBinary](ACCESS_SOURCES/tables/tblBinary.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [TblExtern](ACCESS_SOURCES/tables/TblExtern.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [tmpCOMP_C](ACCESS_SOURCES/tables/tmpCOMP_C.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpCOMP_D](ACCESS_SOURCES/tables/tmpCOMP_D.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpExport_Chitante](ACCESS_SOURCES/tables/tmpExport_Chitante.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpPlatiAlte](ACCESS_SOURCES/tables/tmpPlatiAlte.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpPlatiChitante](ACCESS_SOURCES/tables/tmpPlatiChitante.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpPlatiRetur](ACCESS_SOURCES/tables/tmpPlatiRetur.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [tmpSitDebPrezenta](ACCESS_SOURCES/tables/tmpSitDebPrezenta.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [TP](ACCESS_SOURCES/tables/TP.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [Trimis](ACCESS_SOURCES/tables/Trimis.md) | CONDIȚIONAT — configurări/istoric mail; funcția de business rămâne de stabilit. |
| [TrimitereCalup](ACCESS_SOURCES/tables/TrimitereCalup.md) | CONDIȚIONAT — configurări/istoric mail; funcția de business rămâne de stabilit. |
| [Unitati](ACCESS_SOURCES/tables/Unitati.md) | COMUN — reutilizare Unitati_Detalii/Unitati_Conturi; fără duplicare. |
| [ValoriTaxe](ACCESS_SOURCES/tables/ValoriTaxe.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Variabile](ACCESS_SOURCES/tables/Variabile.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [VEROFFLINE](ACCESS_SOURCES/tables/VEROFFLINE.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |

## Forms — 44

| Obiect / sursă | Tratament |
|---|---|
| [AdaugaTabelInTBLS](ACCESS_SOURCES/Forms/AdaugaTabelInTBLS.txt) | TEHNIC — mecanism Access; folosim infrastructura/controalele web existente. |
| [ADECHIT](ACCESS_SOURCES/Forms/ADECHIT.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [AlteDoc](ACCESS_SOURCES/Forms/AlteDoc.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [BonuriF](ACCESS_SOURCES/Forms/BonuriF.txt) | EXCLUS — funcționalitate exclusă explicit de utilizator. |
| [Chitante_bun](ACCESS_SOURCES/Forms/Chitante_bun.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [ComboFrm](ACCESS_SOURCES/Forms/ComboFrm.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [ComboGrupa](ACCESS_SOURCES/Forms/ComboGrupa.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [ComboPrez](ACCESS_SOURCES/Forms/ComboPrez.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [COMP_C](ACCESS_SOURCES/Forms/COMP_C.txt) | CONDIȚIONAT — compensare manuală; distinctă de formula Compensare. |
| [COMP_D](ACCESS_SOURCES/Forms/COMP_D.txt) | CONDIȚIONAT — compensare manuală; distinctă de formula Compensare. |
| [COMP](ACCESS_SOURCES/Forms/COMP.txt) | CONDIȚIONAT — compensare manuală; distinctă de formula Compensare. |
| [Config_Mail](ACCESS_SOURCES/Forms/Config_Mail.txt) | CONDIȚIONAT — mail de business, fără includere implicită. |
| [Config_Taxe](ACCESS_SOURCES/Forms/Config_Taxe.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Config_Unit](ACCESS_SOURCES/Forms/Config_Unit.txt) | FĂRĂ PAGINĂ WEB — datele unității sunt gestionate prin sistemul existent. |
| [DatePicker](ACCESS_SOURCES/Forms/DatePicker.txt) | CONDIȚIONAT — calendar/variantă configurabilă; fără reducerile excluse. |
| [Delegati_L](ACCESS_SOURCES/Forms/Delegati_L.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Det_S](ACCESS_SOURCES/Forms/Det_S.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [ExplVar](ACCESS_SOURCES/Forms/ExplVar.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [Facturi](ACCESS_SOURCES/Forms/Facturi.txt) | EXCLUS — funcționalitate exclusă explicit de utilizator. |
| [frmMail](ACCESS_SOURCES/Forms/frmMail.txt) | CONDIȚIONAT — mail de business, fără includere implicită. |
| [frmProgressBar](ACCESS_SOURCES/Forms/frmProgressBar.txt) | TEHNIC — mecanism Access; folosim infrastructura/controalele web existente. |
| [Grupe_PR](ACCESS_SOURCES/Forms/Grupe_PR.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [Grupe](ACCESS_SOURCES/Forms/Grupe.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [IMPXLS](ACCESS_SOURCES/Forms/IMPXLS.txt) | CONDIȚIONAT — import operațional, separat de migrarea inițială. |
| [Luni](ACCESS_SOURCES/Forms/Luni.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [main](ACCESS_SOURCES/Forms/main.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [Plati_Alte](ACCESS_SOURCES/Forms/Plati_Alte.txt) | FLUX INCLUS — încasări/restituiri/anulări; actualizare snapshot la salvare. |
| [Plati_chitante](ACCESS_SOURCES/Forms/Plati_chitante.txt) | FLUX INCLUS — încasări/restituiri/anulări; actualizare snapshot la salvare. |
| [Plati_Retur](ACCESS_SOURCES/Forms/Plati_Retur.txt) | FLUX INCLUS — încasări/restituiri/anulări; actualizare snapshot la salvare. |
| [Plati_sub](ACCESS_SOURCES/Forms/Plati_sub.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [Plati](ACCESS_SOURCES/Forms/Plati.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [Platitori_sub_L_2](ACCESS_SOURCES/Forms/Platitori_sub_L_2.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Platitori_sub_L](ACCESS_SOURCES/Forms/Platitori_sub_L.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Platitori2016](ACCESS_SOURCES/Forms/Platitori2016.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Prezenta_bun](ACCESS_SOURCES/Forms/Prezenta_bun.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [Prezenta_sub_buget](ACCESS_SOURCES/Forms/Prezenta_sub_buget.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [Prezenta_sub](ACCESS_SOURCES/Forms/Prezenta_sub.txt) | CONDIȚIONAT — calendar/variantă configurabilă; fără reducerile excluse. |
| [Prezenta](ACCESS_SOURCES/Forms/Prezenta.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [Rapoarte](ACCESS_SOURCES/Forms/Rapoarte.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [Retur](ACCESS_SOURCES/Forms/Retur.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [SIIR](ACCESS_SOURCES/Forms/SIIR.txt) | CONDIȚIONAT — import operațional, separat de migrarea inițială. |
| [sub_Platitori](ACCESS_SOURCES/Forms/sub_Platitori.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [subTreeView](ACCESS_SOURCES/Forms/subTreeView.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [TrimitereCalup](ACCESS_SOURCES/Forms/TrimitereCalup.txt) | CONDIȚIONAT — mail de business, fără includere implicită. |

## queries — 27

| Obiect / sursă | Tratament |
|---|---|
| [AddFisaCont](ACCESS_SOURCES/queries/AddFisaCont.sql) | RAPORT INCLUS — păstrarea filtrelor/semnelor; TIP legacy de clarificat. |
| [CNPPROST](ACCESS_SOURCES/queries/CNPPROST.sql) | CORECȚIE ISTORICĂ — nu se execută automat la migrare. |
| [DocumenteAnulate](ACCESS_SOURCES/queries/DocumenteAnulate.sql) | RAPORT INCLUS — aliasurile cer probă Access, constatarea M08. |
| [Erori Chitante](ACCESS_SOURCES/queries/Erori%20Chitante.sql) | DIAGNOSTIC — referință pentru reconciliere; fără modificare automată de date. |
| [Export_Chitante](ACCESS_SOURCES/queries/Export_Chitante.sql) | CONDIȚIONAT — export prin tabel de lucru; cerința exportului de stabilit. |
| [FisaDebitor_C](ACCESS_SOURCES/queries/FisaDebitor_C.sql) | RAPORT CONDIȚIONAT — accesibil, dar dependențe legacy; nu este motorul 2021. |
| [FisaDebitor_D](ACCESS_SOURCES/queries/FisaDebitor_D.sql) | RAPORT CONDIȚIONAT — accesibil, dar dependențe legacy; nu este motorul 2021. |
| [FisaDebitor_L](ACCESS_SOURCES/queries/FisaDebitor_L.sql) | RAPORT CONDIȚIONAT — accesibil, dar dependențe legacy; nu este motorul 2021. |
| [FisaDebitor_P](ACCESS_SOURCES/queries/FisaDebitor_P.sql) | RAPORT CONDIȚIONAT — accesibil, dar dependențe legacy; nu este motorul 2021. |
| [qCOMP_D](ACCESS_SOURCES/queries/qCOMP_D.sql) | CONDIȚIONAT — suport compensare manuală, nu formula 2021. |
| [qExplicatie](ACCESS_SOURCES/queries/qExplicatie.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [qPrezenta](ACCESS_SOURCES/queries/qPrezenta.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [qSolduri](ACCESS_SOURCES/queries/qSolduri.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [qVariabile](ACCESS_SOURCES/queries/qVariabile.sql) | EXPLICAȚII — substituții; ramuri TIP/bonuri și șabloane reale de clarificat (M07). |
| [RaportBanca](ACCESS_SOURCES/queries/RaportBanca.sql) | RAPORT INCLUS — păstrarea filtrelor/semnelor; TIP legacy de clarificat. |
| [RegCasa](ACCESS_SOURCES/queries/RegCasa.sql) | RAPORT INCLUS — păstrarea filtrelor/semnelor; TIP legacy de clarificat. |
| [Restante](ACCESS_SOURCES/queries/Restante.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [Salvare_Lunara](ACCESS_SOURCES/queries/Salvare_Lunara.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [Sit](ACCESS_SOURCES/queries/Sit.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [SitDebPrezenta](ACCESS_SOURCES/queries/SitDebPrezenta.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [SitLunara](ACCESS_SOURCES/queries/SitLunara.sql) | AFIȘARE PREZENȚĂ — valoare curentă în ecran; distinctă de snapshot și qSolduri. |
| [SituatieDebitori](ACCESS_SOURCES/queries/SituatieDebitori.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [SoldInitial](ACCESS_SOURCES/queries/SoldInitial.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [Update_Compensare](ACCESS_SOURCES/queries/Update_Compensare.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [Update_detalii](ACCESS_SOURCES/queries/Update_detalii.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [Update_Situatie](ACCESS_SOURCES/queries/Update_Situatie.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [Update_Solduri](ACCESS_SOURCES/queries/Update_Solduri.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |

## Reports — 18

| Obiect / sursă | Tratament |
|---|---|
| [Chitanta](ACCESS_SOURCES/Reports/Chitanta.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [Dispozitie plata](ACCESS_SOURCES/Reports/Dispozitie%20plata.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [DocumenteAnulate](ACCESS_SOURCES/Reports/DocumenteAnulate.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [Factura_mail](ACCESS_SOURCES/Reports/Factura_mail.txt) | EXCLUS — fără facturi. |
| [FisaCont_s](ACCESS_SOURCES/Reports/FisaCont_s.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [FisaCont](ACCESS_SOURCES/Reports/FisaCont.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [FisaDebitor_C](ACCESS_SOURCES/Reports/FisaDebitor_C.txt) | CONDIȚIONAT — dependențe legacy, bonuri/reduceri de separat. |
| [FisaDebitor_D](ACCESS_SOURCES/Reports/FisaDebitor_D.txt) | CONDIȚIONAT — dependențe legacy, bonuri/reduceri de separat. |
| [FisaDebitor_L](ACCESS_SOURCES/Reports/FisaDebitor_L.txt) | CONDIȚIONAT — dependențe legacy, bonuri/reduceri de separat. |
| [FisaDebitor](ACCESS_SOURCES/Reports/FisaDebitor.txt) | CONDIȚIONAT — dependențe legacy, bonuri/reduceri de separat. |
| [RaportBanca](ACCESS_SOURCES/Reports/RaportBanca.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [RegistruCasa](ACCESS_SOURCES/Reports/RegistruCasa.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [rpt_Factura](ACCESS_SOURCES/Reports/rpt_Factura.txt) | EXCLUS — fără facturi. |
| [rpt_SitFin](ACCESS_SOURCES/Reports/rpt_SitFin.txt) | LEGACY — nu înlocuiește raportul bugetar 2021; dependențe de verificat. |
| [SituatieDebitori_buget_total_grp](ACCESS_SOURCES/Reports/SituatieDebitori_buget_total_grp.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [SituatieDebitori_buget_total](ACCESS_SOURCES/Reports/SituatieDebitori_buget_total.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [SituatieDebitori_buget](ACCESS_SOURCES/Reports/SituatieDebitori_buget.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [SituatieDebitori](ACCESS_SOURCES/Reports/SituatieDebitori.txt) | LEGACY — nu înlocuiește raportul bugetar 2021; dependențe de verificat. |
