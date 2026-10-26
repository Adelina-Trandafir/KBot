# SLICE-ADE0-02 — inventarul obiectelor Access

Data: 07.10.2026. Acoperire: 66 tabele, 44 formulare, 27 interogări, 18 rapoarte (155 obiecte). Clasificare statică, nu dovadă de utilizare în producție și nu autorizație de ștergere. Un obiect condiționat sau legacy nu este declarat inutil. Datele și traseele reale pot schimba încadrarea.

Vezi [maparea fluxurilor](MAPARE_ACCESS_WEB.md) și [contractul calculelor](CONTRACT_CALCULE.md). Obiectele cu tratament identic sunt grupate pe același rând, în fiecare categorie; fiecare nume și link spre sursa sa este păstrat. Clasificările se citesc în contextul analizei și al deciziilor datate.

Modulele, clasele și macrocomenzile sunt referințe pentru trasee; acest tabel inventariază cele patru categorii de mai jos. Prefixul AD_ este propus numai pentru datele persistente incluse; cele șapte cache-uri _L nu se migrează.

## tables — 66

| Obiect / sursă | Tratament |
|---|---|
| [_Mail](ACCESS_SOURCES/tables/_Mail.md), [Mail](ACCESS_SOURCES/tables/Mail.md), [Trimis](ACCESS_SOURCES/tables/Trimis.md), [TrimitereCalup](ACCESS_SOURCES/tables/TrimitereCalup.md) | CONDIȚIONAT — configurări/istoric mail; funcția de business rămâne de stabilit. |
| [_Struct_](ACCESS_SOURCES/tables/_Struct_.md), [_TBLS_](ACCESS_SOURCES/tables/_TBLS_.md), [RunOnce](ACCESS_SOURCES/tables/RunOnce.md), [tblBinary](ACCESS_SOURCES/tables/tblBinary.md), [TblExtern](ACCESS_SOURCES/tables/TblExtern.md), [VEROFFLINE](ACCESS_SOURCES/tables/VEROFFLINE.md) | TEHNIC ACCESS — nu se portează mecanismul; sistemele web comune îl înlocuiesc. |
| [@CFGs](ACCESS_SOURCES/tables/%40CFGs.md), [@COMP](ACCESS_SOURCES/tables/%40COMP.md), [@MutaCopil](ACCESS_SOURCES/tables/%40MutaCopil.md), [A](ACCESS_SOURCES/tables/A.md), [B](ACCESS_SOURCES/tables/B.md), [cacat](ACCESS_SOURCES/tables/cacat.md), [Chitantex](ACCESS_SOURCES/tables/Chitantex.md), [Erori Intrare](ACCESS_SOURCES/tables/Erori%20Intrare.md), [Luni](ACCESS_SOURCES/tables/Luni.md), [OPuri](ACCESS_SOURCES/tables/OPuri.md), [Platix](ACCESS_SOURCES/tables/Platix.md), [Situatie](ACCESS_SOURCES/tables/Situatie.md), [TP](ACCESS_SOURCES/tables/TP.md) | DE CLARIFICAT — fără destinație aprobată; structura singură nu justifică importul sau ștergerea. |
| [AlteDoc](ACCESS_SOURCES/tables/AlteDoc.md), [Chitante](ACCESS_SOURCES/tables/Chitante.md), [Grupe](ACCESS_SOURCES/tables/Grupe.md), [LunaD](ACCESS_SOURCES/tables/LunaD.md), [Plati](ACCESS_SOURCES/tables/Plati.md), [Platitori_sub](ACCESS_SOURCES/tables/Platitori_sub.md), [Platitori](ACCESS_SOURCES/tables/Platitori.md), [Prezenta](ACCESS_SOURCES/tables/Prezenta.md), [Retur](ACCESS_SOURCES/tables/Retur.md), [SS_Buget](ACCESS_SOURCES/tables/SS_Buget.md), [ValoriTaxe](ACCESS_SOURCES/tables/ValoriTaxe.md) | PERSISTENT — candidat AD_; relații și reguli în mapare. |
| [Banci](ACCESS_SOURCES/tables/Banci.md), [LIT](ACCESS_SOURCES/tables/LIT.md) | REFERINȚĂ — verificare necesitate pentru conturi/sume în litere; reutilizare comună unde există. |
| [BonuriF](ACCESS_SOURCES/tables/BonuriF.md), [Facturi](ACCESS_SOURCES/tables/Facturi.md) | EXCLUS FUNCȚIONAL — fără emitere/import ca modul; contribuțiile istorice la Plati se reconciliază. |
| [CFGs](ACCESS_SOURCES/tables/CFGs.md) | MIXT — CH către Unitati_Chitante; alte configurări se decid individual. |
| [COMP](ACCESS_SOURCES/tables/COMP.md) | CONDIȚIONAT — compensare manuală între persoane; motorul 2021 nu o citește. |
| [DatePickerDate](ACCESS_SOURCES/tables/DatePickerDate.md), [IMPXLS](ACCESS_SOURCES/tables/IMPXLS.md), [tmpCOMP_C](ACCESS_SOURCES/tables/tmpCOMP_C.md), [tmpCOMP_D](ACCESS_SOURCES/tables/tmpCOMP_D.md), [tmpExport_Chitante](ACCESS_SOURCES/tables/tmpExport_Chitante.md), [tmpPlatiAlte](ACCESS_SOURCES/tables/tmpPlatiAlte.md), [tmpPlatiChitante](ACCESS_SOURCES/tables/tmpPlatiChitante.md), [tmpPlatiRetur](ACCESS_SOURCES/tables/tmpPlatiRetur.md), [tmpSitDebPrezenta](ACCESS_SOURCES/tables/tmpSitDebPrezenta.md), [Variabile](ACCESS_SOURCES/tables/Variabile.md) | LUCRU — fără import automat; stare per operație dacă funcția este inclusă. |
| [Delegati_L](ACCESS_SOURCES/tables/Delegati_L.md), [Grupe_L](ACCESS_SOURCES/tables/Grupe_L.md), [LunaD_L](ACCESS_SOURCES/tables/LunaD_L.md), [Platitori_L](ACCESS_SOURCES/tables/Platitori_L.md), [Platitori_sub_L](ACCESS_SOURCES/tables/Platitori_sub_L.md), [Prezenta_L](ACCESS_SOURCES/tables/Prezenta_L.md), [Prezenta_sub_L](ACCESS_SOURCES/tables/Prezenta_sub_L.md) | EXCLUS — cache _L; regulile din formulare se păstrează. |
| [Delegati](ACCESS_SOURCES/tables/Delegati.md), [MutaCopil](ACCESS_SOURCES/tables/MutaCopil.md), [Prezenta_sub](ACCESS_SOURCES/tables/Prezenta_sub.md) | EXCLUS 08.10.2026 — decizia utilizatorului; nu se portează. |
| [Explicatie_Lunara](ACCESS_SOURCES/tables/Explicatie_Lunara.md), [FisaCont](ACCESS_SOURCES/tables/FisaCont.md), [Situatie_buget](ACCESS_SOURCES/tables/Situatie_buget.md), [Solduri_Lunare](ACCESS_SOURCES/tables/Solduri_Lunare.md) | DERIVAT — rezultat de calcul/raport; fără tabel comun golit între utilizatori. |
| [SQL](ACCESS_SOURCES/tables/SQL.md) | CONFIGURARE — lipsesc rândurile SQL din export; varianta veche le citește. |
| [Unitati](ACCESS_SOURCES/tables/Unitati.md) | COMUN — reutilizare Unitati_Detalii/Unitati_Conturi; fără duplicare. |

## Forms — 44

| Obiect / sursă | Tratament |
|---|---|
| [AdaugaTabelInTBLS](ACCESS_SOURCES/Forms/AdaugaTabelInTBLS.txt), [frmProgressBar](ACCESS_SOURCES/Forms/frmProgressBar.txt) | TEHNIC — mecanism Access; folosim infrastructura/controalele web existente. |
| [ADECHIT](ACCESS_SOURCES/Forms/ADECHIT.txt), [Grupe_PR](ACCESS_SOURCES/Forms/Grupe_PR.txt), [Luni](ACCESS_SOURCES/Forms/Luni.txt), [Prezenta](ACCESS_SOURCES/Forms/Prezenta.txt), [Rapoarte](ACCESS_SOURCES/Forms/Rapoarte.txt), [subTreeView](ACCESS_SOURCES/Forms/subTreeView.txt) | FLUX INCLUS — navigare/ciclu lunar/rapoarte; adaptare la portal și controalele comune. |
| [AlteDoc](ACCESS_SOURCES/Forms/AlteDoc.txt), [Chitante_bun](ACCESS_SOURCES/Forms/Chitante_bun.txt), [ComboFrm](ACCESS_SOURCES/Forms/ComboFrm.txt), [Det_S](ACCESS_SOURCES/Forms/Det_S.txt), [ExplVar](ACCESS_SOURCES/Forms/ExplVar.txt), [main](ACCESS_SOURCES/Forms/main.txt), [Plati_sub](ACCESS_SOURCES/Forms/Plati_sub.txt), [Plati](ACCESS_SOURCES/Forms/Plati.txt), [Prezenta_bun](ACCESS_SOURCES/Forms/Prezenta_bun.txt), [Retur](ACCESS_SOURCES/Forms/Retur.txt) | LEGACY / AUXILIAR — nu constituie automat o pagină web; traseele incluse sunt în matrice. |
| [BonuriF](ACCESS_SOURCES/Forms/BonuriF.txt), [Facturi](ACCESS_SOURCES/Forms/Facturi.txt) | EXCLUS — funcționalitate exclusă explicit de utilizator. |
| [ComboGrupa](ACCESS_SOURCES/Forms/ComboGrupa.txt), [ComboPrez](ACCESS_SOURCES/Forms/ComboPrez.txt), [Config_Taxe](ACCESS_SOURCES/Forms/Config_Taxe.txt), [Delegati_L](ACCESS_SOURCES/Forms/Delegati_L.txt), [Grupe](ACCESS_SOURCES/Forms/Grupe.txt), [Platitori_sub_L_2](ACCESS_SOURCES/Forms/Platitori_sub_L_2.txt), [Platitori_sub_L](ACCESS_SOURCES/Forms/Platitori_sub_L.txt), [Platitori2016](ACCESS_SOURCES/Forms/Platitori2016.txt), [Prezenta_sub_buget](ACCESS_SOURCES/Forms/Prezenta_sub_buget.txt), [sub_Platitori](ACCESS_SOURCES/Forms/sub_Platitori.txt) | REGULI INCLUSE — grile editabile; fără tabelele-cache _L. |
| [COMP_C](ACCESS_SOURCES/Forms/COMP_C.txt), [COMP_D](ACCESS_SOURCES/Forms/COMP_D.txt), [COMP](ACCESS_SOURCES/Forms/COMP.txt) | CONDIȚIONAT — compensare manuală; distinctă de formula Compensare. |
| [Config_Mail](ACCESS_SOURCES/Forms/Config_Mail.txt), [frmMail](ACCESS_SOURCES/Forms/frmMail.txt), [TrimitereCalup](ACCESS_SOURCES/Forms/TrimitereCalup.txt) | CONDIȚIONAT — mail de business, fără includere implicită. |
| [Config_Unit](ACCESS_SOURCES/Forms/Config_Unit.txt) | FĂRĂ PAGINĂ WEB — datele unității sunt gestionate prin sistemul existent. |
| [DatePicker](ACCESS_SOURCES/Forms/DatePicker.txt), [Prezenta_sub](ACCESS_SOURCES/Forms/Prezenta_sub.txt) | CONDIȚIONAT — calendar/variantă configurabilă; fără reducerile excluse. |
| [IMPXLS](ACCESS_SOURCES/Forms/IMPXLS.txt), [SIIR](ACCESS_SOURCES/Forms/SIIR.txt) | CONDIȚIONAT — import operațional, separat de migrarea inițială. |
| [Plati_Alte](ACCESS_SOURCES/Forms/Plati_Alte.txt), [Plati_chitante](ACCESS_SOURCES/Forms/Plati_chitante.txt), [Plati_Retur](ACCESS_SOURCES/Forms/Plati_Retur.txt) | FLUX INCLUS — încasări/restituiri/anulări; actualizare snapshot la salvare. |

## queries — 27

| Obiect / sursă | Tratament |
|---|---|
| [AddFisaCont](ACCESS_SOURCES/queries/AddFisaCont.sql), [RaportBanca](ACCESS_SOURCES/queries/RaportBanca.sql), [RegCasa](ACCESS_SOURCES/queries/RegCasa.sql) | RAPORT INCLUS — păstrarea filtrelor/semnelor; TIP legacy de clarificat. |
| [CNPPROST](ACCESS_SOURCES/queries/CNPPROST.sql) | CORECȚIE ISTORICĂ — nu se execută automat la migrare. |
| [DocumenteAnulate](ACCESS_SOURCES/queries/DocumenteAnulate.sql) | RAPORT INCLUS — aliasurile cer probă Access, constatarea M08. |
| [Erori Chitante](ACCESS_SOURCES/queries/Erori%20Chitante.sql) | DIAGNOSTIC — referință pentru reconciliere; fără modificare automată de date. |
| [Export_Chitante](ACCESS_SOURCES/queries/Export_Chitante.sql) | CONDIȚIONAT — export prin tabel de lucru; cerința exportului de stabilit. |
| [FisaDebitor_C](ACCESS_SOURCES/queries/FisaDebitor_C.sql), [FisaDebitor_D](ACCESS_SOURCES/queries/FisaDebitor_D.sql), [FisaDebitor_L](ACCESS_SOURCES/queries/FisaDebitor_L.sql), [FisaDebitor_P](ACCESS_SOURCES/queries/FisaDebitor_P.sql) | RAPORT CONDIȚIONAT — accesibil, dar dependențe legacy; nu este motorul 2021. |
| [qCOMP_D](ACCESS_SOURCES/queries/qCOMP_D.sql) | CONDIȚIONAT — suport compensare manuală, nu formula 2021. |
| [qExplicatie](ACCESS_SOURCES/queries/qExplicatie.sql), [qPrezenta](ACCESS_SOURCES/queries/qPrezenta.sql), [qSolduri](ACCESS_SOURCES/queries/qSolduri.sql), [Salvare_Lunara](ACCESS_SOURCES/queries/Salvare_Lunara.sql), [Update_Compensare](ACCESS_SOURCES/queries/Update_Compensare.sql), [Update_detalii](ACCESS_SOURCES/queries/Update_detalii.sql), [Update_Situatie](ACCESS_SOURCES/queries/Update_Situatie.sql), [Update_Solduri](ACCESS_SOURCES/queries/Update_Solduri.sql) | NUCLEU 2021 — ordine și formule obligatorii în CONTRACT_CALCULE. |
| [qVariabile](ACCESS_SOURCES/queries/qVariabile.sql) | EXPLICAȚII — substituții; ramuri TIP/bonuri și șabloane reale de clarificat (M07). |
| [Restante](ACCESS_SOURCES/queries/Restante.sql), [Sit](ACCESS_SOURCES/queries/Sit.sql), [SitDebPrezenta](ACCESS_SOURCES/queries/SitDebPrezenta.sql), [SituatieDebitori](ACCESS_SOURCES/queries/SituatieDebitori.sql), [SoldInitial](ACCESS_SOURCES/queries/SoldInitial.sql) | LEGACY — formule/filtre diferite; nu înlocuiesc lanțul 2021. |
| [SitLunara](ACCESS_SOURCES/queries/SitLunara.sql) | AFIȘARE PREZENȚĂ — valoare curentă în ecran; distinctă de snapshot și qSolduri. |

## Reports — 18

| Obiect / sursă | Tratament |
|---|---|
| [Chitanta](ACCESS_SOURCES/Reports/Chitanta.txt), [Dispozitie plata](ACCESS_SOURCES/Reports/Dispozitie%20plata.txt), [DocumenteAnulate](ACCESS_SOURCES/Reports/DocumenteAnulate.txt), [FisaCont_s](ACCESS_SOURCES/Reports/FisaCont_s.txt), [FisaCont](ACCESS_SOURCES/Reports/FisaCont.txt), [RaportBanca](ACCESS_SOURCES/Reports/RaportBanca.txt), [RegistruCasa](ACCESS_SOURCES/Reports/RegistruCasa.txt), [SituatieDebitori_buget_total_grp](ACCESS_SOURCES/Reports/SituatieDebitori_buget_total_grp.txt), [SituatieDebitori_buget_total](ACCESS_SOURCES/Reports/SituatieDebitori_buget_total.txt), [SituatieDebitori_buget](ACCESS_SOURCES/Reports/SituatieDebitori_buget.txt) | INCLUS — adaptare document/raport; date și filtre conform mapării. |
| [Factura_mail](ACCESS_SOURCES/Reports/Factura_mail.txt), [rpt_Factura](ACCESS_SOURCES/Reports/rpt_Factura.txt) | EXCLUS — fără facturi. |
| [FisaDebitor_C](ACCESS_SOURCES/Reports/FisaDebitor_C.txt), [FisaDebitor_D](ACCESS_SOURCES/Reports/FisaDebitor_D.txt), [FisaDebitor_L](ACCESS_SOURCES/Reports/FisaDebitor_L.txt), [FisaDebitor](ACCESS_SOURCES/Reports/FisaDebitor.txt) | CONDIȚIONAT — dependențe legacy, bonuri/reduceri de separat. |
| [rpt_SitFin](ACCESS_SOURCES/Reports/rpt_SitFin.txt), [SituatieDebitori](ACCESS_SOURCES/Reports/SituatieDebitori.txt) | LEGACY — nu înlocuiește raportul bugetar 2021; dependențe de verificat. |
