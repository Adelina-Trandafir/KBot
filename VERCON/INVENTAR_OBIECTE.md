# VERCON — inventarul obiectelor Access (SLICE-VC00-01)

Generat de `VERCON/tools/inventar.js` din `VERCON_ACCESS` (citire doar-citire, 11.10.2026). Clasificare **statica** dupa nume si marcaje din continut; nu dovedeste utilizare in productie si nu autorizeaza stergeri. Fiecare obiect apare o singura data. Ce nu e demonstrat: utilizarea reala a obiectelor DE-CLARIFICAT (vezi [DECIZII_DESCHISE](DECIZII_DESCHISE.md)).

Total 377 obiecte: table 109, query 80, form 53, report 61, module 44, class 27, macro 3.

## Sinteza pe dispozitie

| Dispozitie | Nr. |
|---|---:|
| DERIVAT | 2 |
| EXCLUS-CODURI | 21 |
| EXCLUS-EFACTURA | 13 |
| EXCLUS-INFRA-ACCESS | 65 |
| EXCLUS-SCHEMA-ACCESS | 36 |
| INCLUS-CALCUL | 64 |
| INCLUS-CONFIG | 6 |
| INCLUS-DATE | 17 |
| INCLUS-NOMENCLATOR | 15 |
| INCLUS-PARTIAL | 4 |
| INCLUS-RAPORT | 58 |
| INCLUS-UI | 36 |
| INLOCUIT-PARTENERI-KBOT | 13 |
| NEREFERIT-NEPORTAT | 8 |
| TEMP-LUCRU | 19 |

Legenda: INCLUS-* = portat/migrat/reconstruit; EXCLUS-* = decizie utilizator (N06 Coduri, N13 e-Factura, DBS/clsDatabase, infrastructura desktop); DE-CLARIFICAT = fara destinatie aprobata (cu dovada adunata); NEREFERIT-NEPORTAT = nicio referinta in export, neportat pana la cerere; INLOCUIT-PARTENERI-KBOT = furnizorii se leaga de Partenerii KBOT dupa CodFiscal; TEMP-LUCRU/DERIVAT = nu se migreaza ca date.

## tables — 109

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [@ComisieInventar](VERCON_ACCESS/tables/@ComisieInventar.md) |  | EXCLUS-SCHEMA-ACCESS | sablon structura "@" folosit de mecanismul de schema Access; 5 campuri |
| [@EF](VERCON_ACCESS/tables/@EF.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 9 campuri |
| [@EFS](VERCON_ACCESS/tables/@EFS.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 10 campuri |
| [@EFT](VERCON_ACCESS/tables/@EFT.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 15 campuri |
| [@ProduseM](VERCON_ACCESS/tables/@ProduseM.md) |  | EXCLUS-SCHEMA-ACCESS | sablon structura "@" folosit de mecanismul de schema Access; 6 campuri |
| [@RunOnce](VERCON_ACCESS/tables/@RunOnce.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 5 campuri |
| [@Setari](VERCON_ACCESS/tables/@Setari.md) |  | EXCLUS-SCHEMA-ACCESS | sablon structura "@" folosit de mecanismul de schema Access; 3 campuri |
| [@Trimis](VERCON_ACCESS/tables/@Trimis.md) |  | EXCLUS-SCHEMA-ACCESS | sablon structura "@" folosit de mecanismul de schema Access; 3 campuri |
| [@UNIT](VERCON_ACCESS/tables/@UNIT.md) |  | EXCLUS-SCHEMA-ACCESS | sablon structura "@" folosit de mecanismul de schema Access; 21 campuri |
| [Alimente](VERCON_ACCESS/tables/Alimente.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 7 campuri |
| [cale](VERCON_ACCESS/tables/cale.md) |  | INCLUS-CONFIG | identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 15 campuri |
| [Caletemp](VERCON_ACCESS/tables/Caletemp.md) |  | INCLUS-CONFIG | identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 9 campuri |
| [Cina](VERCON_ACCESS/tables/Cina.md) |  | INCLUS-DATE | date business istorice; 5 campuri |
| [Clasificari](VERCON_ACCESS/tables/Clasificari.md) |  | INCLUS-NOMENCLATOR | folosit de FC, Produse, CLS2024 (formulare active); date de profilat; 10 campuri |
| [Clienti](VERCON_ACCESS/tables/Clienti.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 7 campuri |
| [ComisieInventar](VERCON_ACCESS/tables/ComisieInventar.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 5 campuri |
| [ComisieNIR](VERCON_ACCESS/tables/ComisieNIR.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 5 campuri |
| [cpv](VERCON_ACCESS/tables/cpv.md) |  | INCLUS-NOMENCLATOR | nomenclator CPV in backend (9.454 randuri in baza Alimente), folosit de Produse; D08; 2 campuri |
| [CPV1](VERCON_ACCESS/tables/CPV1.md) |  | INCLUS-NOMENCLATOR | folosit de formularul CPV2017 (deschis din Produse); 9 campuri |
| [Dejun](VERCON_ACCESS/tables/Dejun.md) |  | INCLUS-DATE | date business istorice; 5 campuri |
| [EF](VERCON_ACCESS/tables/EF.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 9 campuri |
| [EFS](VERCON_ACCESS/tables/EFS.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 10 campuri |
| [EFT](VERCON_ACCESS/tables/EFT.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 15 campuri |
| [EF_TMP](VERCON_ACCESS/tables/EF_TMP.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 10 campuri |
| [Erori Intrare Tbl](VERCON_ACCESS/tables/Erori%20Intrare%20Tbl.md) |  | EXCLUS-INFRA-ACCESS | jurnal de erori al mecanismului de schema Access (referit doar de VaerCon in cod comentat, clsDatabaseErrors, newProgressBar); 7 campuri |
| [Erori Intrare](VERCON_ACCESS/tables/Erori%20Intrare.md) |  | EXCLUS-INFRA-ACCESS | idem; 8 campuri |
| [Erori_Intrare](VERCON_ACCESS/tables/Erori_Intrare.md) |  | EXCLUS-INFRA-ACCESS | jurnal de erori clsDatabaseErrors (DBS); 7 campuri |
| [EXT_IDX](VERCON_ACCESS/tables/EXT_IDX.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 9 campuri |
| [Feluri](VERCON_ACCESS/tables/Feluri.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 1 campuri |
| [FeluriMancare](VERCON_ACCESS/tables/FeluriMancare.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 3 campuri |
| [FM_2](VERCON_ACCESS/tables/FM_2.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 11 campuri |
| [Furnizori](VERCON_ACCESS/tables/Furnizori.md) |  | INLOCUIT-PARTENERI-KBOT | Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real; 9 campuri |
| [gest_subCPV](VERCON_ACCESS/tables/gest_subCPV.md) |  | NEREFERIT-NEPORTAT | nicio referinta in export; 3 campuri |
| [Iesiri](VERCON_ACCESS/tables/Iesiri.md) |  | INCLUS-DATE | date business istorice; 7 campuri |
| [IesiriRetur](VERCON_ACCESS/tables/IesiriRetur.md) |  | INCLUS-DATE | date business istorice; 6 campuri |
| [Iesiri_H](VERCON_ACCESS/tables/Iesiri_H.md) |  | INCLUS-DATE | date business istorice; 10 campuri |
| [Intrari](VERCON_ACCESS/tables/Intrari.md) |  | INCLUS-DATE | date business istorice; 11 campuri |
| [IntrariStorno](VERCON_ACCESS/tables/IntrariStorno.md) |  | INCLUS-DATE | date business istorice; 6 campuri |
| [Jud](VERCON_ACCESS/tables/Jud.md) |  | INCLUS-NOMENCLATOR | lista judete, folosita de formularul UNIT; 3 campuri |
| [LABC](VERCON_ACCESS/tables/LABC.md) |  | INCLUS-DATE | date business istorice; 7 campuri |
| [LegBCLA](VERCON_ACCESS/tables/LegBCLA.md) |  | INCLUS-DATE | date business istorice; 2 campuri |
| [ListaAlimente](VERCON_ACCESS/tables/ListaAlimente.md) |  | INCLUS-DATE | date business istorice; 15 campuri |
| [Masini](VERCON_ACCESS/tables/Masini.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 3 campuri |
| [MicDejun](VERCON_ACCESS/tables/MicDejun.md) |  | INCLUS-DATE | date business istorice; 5 campuri |
| [NIR](VERCON_ACCESS/tables/NIR.md) |  | INCLUS-DATE | date business istorice; 13 campuri |
| [NR](VERCON_ACCESS/tables/NR.md) |  | INCLUS-CONFIG | identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 8 campuri |
| [OI_Casare](VERCON_ACCESS/tables/OI_Casare.md) |  | INCLUS-DATE | date business istorice; 12 campuri |
| [OI_Predare](VERCON_ACCESS/tables/OI_Predare.md) |  | INCLUS-DATE | date business istorice; 14 campuri |
| [OI_Produse](VERCON_ACCESS/tables/OI_Produse.md) |  | INCLUS-DATE | date business istorice; 12 campuri |
| [OI_TmpFact](VERCON_ACCESS/tables/OI_TmpFact.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 14 campuri |
| [OI_Transfer](VERCON_ACCESS/tables/OI_Transfer.md) |  | INCLUS-DATE | date business istorice; 13 campuri |
| [Produse](VERCON_ACCESS/tables/Produse.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 8 campuri |
| [ProduseM](VERCON_ACCESS/tables/ProduseM.md) |  | INCLUS-DATE | date business istorice; 6 campuri |
| [ProduseP](VERCON_ACCESS/tables/ProduseP.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 5 campuri |
| [Rbb](VERCON_ACCESS/tables/Rbb.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 7 campuri |
| [Retetar](VERCON_ACCESS/tables/Retetar.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 6 campuri |
| [RunOnce](VERCON_ACCESS/tables/RunOnce.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 5 campuri |
| [Setari](VERCON_ACCESS/tables/Setari.md) |  | INCLUS-CONFIG | identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 3 campuri |
| [SFP_CautCBX](VERCON_ACCESS/tables/SFP_CautCBX.md) |  | TEMP-LUCRU | sursa casetei de cautare din SFP; 4 campuri |
| [SQL](VERCON_ACCESS/tables/SQL.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 3 campuri |
| [SQL_](VERCON_ACCESS/tables/SQL_.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 10 campuri |
| [SQL_FIX_BD](VERCON_ACCESS/tables/SQL_FIX_BD.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 6 campuri |
| [SQL_FIX_BD_SUB](VERCON_ACCESS/tables/SQL_FIX_BD_SUB.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 4 campuri |
| [TblExcel](VERCON_ACCESS/tables/TblExcel.md) |  | TEMP-LUCRU | tabel de lucru al exportului Excel (mdl_Export_Excel); 13 campuri |
| [tblFiles](VERCON_ACCESS/tables/tblFiles.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 5 campuri |
| [tblFTP](VERCON_ACCESS/tables/tblFTP.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 10 campuri |
| [tblPopupMsg](VERCON_ACCESS/tables/tblPopupMsg.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 5 campuri |
| [TipFirmeL](VERCON_ACCESS/tables/TipFirmeL.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 2 campuri |
| [tmpCoduri](VERCON_ACCESS/tables/tmpCoduri.md) |  | EXCLUS-CODURI | sistem Coduri (N06); 4 campuri |
| [TmpFact](VERCON_ACCESS/tables/TmpFact.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 26 campuri |
| [TmpFactV](VERCON_ACCESS/tables/TmpFactV.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 9 campuri |
| [tmpFMANTET](VERCON_ACCESS/tables/tmpFMANTET.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 5 campuri |
| [tmpFM_2](VERCON_ACCESS/tables/tmpFM_2.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 10 campuri |
| [tmpOI](VERCON_ACCESS/tables/tmpOI.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 10 campuri |
| [tmpOptiuniSFP](VERCON_ACCESS/tables/tmpOptiuniSFP.md) |  | INCLUS-CONFIG | meniul SFP (MODEL/TIP/Raport); DATE PRIMITE de la utilizator: REFERINTE/tmpOptiuniSFP.xml; se transforma in catalog de rapoarte (VC_ReportCatalog); 10 campuri |
| [tmpProduseM](VERCON_ACCESS/tables/tmpProduseM.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 7 campuri |
| [tmpQFC](VERCON_ACCESS/tables/tmpQFC.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 17 campuri |
| [tmpRbb](VERCON_ACCESS/tables/tmpRbb.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 1 campuri |
| [tmpSFP](VERCON_ACCESS/tables/tmpSFP.md) |  | DERIVAT | rezultat de calcul/raport; 20 campuri |
| [tmpXMLPath](VERCON_ACCESS/tables/tmpXMLPath.md) |  | EXCLUS-EFACTURA | e-Factura (N13); 6 campuri |
| [tmp_Balanta](VERCON_ACCESS/tables/tmp_Balanta.md) |  | DERIVAT | rezultat de calcul/raport; 14 campuri |
| [tmp_BC_DenProdus](VERCON_ACCESS/tables/tmp_BC_DenProdus.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 20 campuri |
| [Tmp_Cina](VERCON_ACCESS/tables/Tmp_Cina.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 5 campuri |
| [tmp_Coduri](VERCON_ACCESS/tables/tmp_Coduri.md) |  | EXCLUS-CODURI | sistem Coduri (N06); 4 campuri |
| [Tmp_Dejun](VERCON_ACCESS/tables/Tmp_Dejun.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 5 campuri |
| [tmp_II](VERCON_ACCESS/tables/tmp_II.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 2 campuri |
| [tmp_LABC](VERCON_ACCESS/tables/tmp_LABC.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 8 campuri |
| [tmp_LEGBCLA](VERCON_ACCESS/tables/tmp_LEGBCLA.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 8 campuri |
| [Tmp_Lista](VERCON_ACCESS/tables/Tmp_Lista.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 18 campuri |
| [Tmp_MicDejun](VERCON_ACCESS/tables/Tmp_MicDejun.md) |  | TEMP-LUCRU | scratch Access; drafturile se reconstruiesc izolat pe utilizator; 5 campuri |
| [Trimis](VERCON_ACCESS/tables/Trimis.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 3 campuri |
| [tSQL](VERCON_ACCESS/tables/tSQL.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 2 campuri |
| [TVA](VERCON_ACCESS/tables/TVA.md) |  | INCLUS-NOMENCLATOR | nomenclator/registru business; 3 campuri |
| [UNIT](VERCON_ACCESS/tables/UNIT.md) |  | INCLUS-CONFIG | identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 20 campuri |
| [Ver](VERCON_ACCESS/tables/Ver.md) |  | EXCLUS-SCHEMA-ACCESS | versiune schema Access; citit doar ca diagnostic daca exista; 1 campuri |
| [VEROFFLINE](VERCON_ACCESS/tables/VEROFFLINE.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 3 campuri |
| [VER_RAP](VERCON_ACCESS/tables/VER_RAP.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 2 campuri |
| [x](VERCON_ACCESS/tables/x.md) |  | NEREFERIT-NEPORTAT | tabel de lucru fara nicio referinta in export; 8 campuri |
| [XCX](VERCON_ACCESS/tables/XCX.md) |  | EXCLUS-INFRA-ACCESS | tabel de lucru al modulului de criptare; 26 campuri |
| [_ADODAODDL](VERCON_ACCESS/tables/_ADODAODDL.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 8 campuri |
| [_IDX_](VERCON_ACCESS/tables/_IDX_.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 9 campuri |
| [_Mail](VERCON_ACCESS/tables/_Mail.md) |  | EXCLUS-INFRA-ACCESS | FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 14 campuri |
| [_QRY](VERCON_ACCESS/tables/_QRY.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 4 campuri |
| [_REL_](VERCON_ACCESS/tables/_REL_.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 10 campuri |
| [_Struct_](VERCON_ACCESS/tables/_Struct_.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 11 campuri |
| [_tblQry](VERCON_ACCESS/tables/_tblQry.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 3 campuri |
| [_tblRel](VERCON_ACCESS/tables/_tblRel.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 9 campuri |
| [_TBLS_](VERCON_ACCESS/tables/_TBLS_.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 5 campuri |
| [_TBLS_Transfer](VERCON_ACCESS/tables/_TBLS_Transfer.md) |  | EXCLUS-SCHEMA-ACCESS | mecanism schema Access (DBS/clsDatabase); 3 campuri |

## queries — 80

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [Amortizare](VERCON_ACCESS/queries/Amortizare.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Balanta interimara](VERCON_ACCESS/queries/Balanta%20interimara.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta (Coduri); Forms!/TempVars |
| [Balanta interimara2](VERCON_ACCESS/queries/Balanta%20interimara2.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta (Coduri); Forms!/TempVars |
| [Balanta listare 1](VERCON_ACCESS/queries/Balanta%20listare%201.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta interimara2 (Coduri); Forms!/TempVars |
| [Balanta listare 2](VERCON_ACCESS/queries/Balanta%20listare%202.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta interimara (Coduri); Forms!/TempVars |
| [Balanta listare](VERCON_ACCESS/queries/Balanta%20listare.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta interimara2 (Coduri); Forms!/TempVars |
| [Balanta](VERCON_ACCESS/queries/Balanta.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [Balanta22](VERCON_ACCESS/queries/Balanta22.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Balanta223](VERCON_ACCESS/queries/Balanta223.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Balanta22_IA](VERCON_ACCESS/queries/Balanta22_IA.sql) |  | INCLUS-CALCUL | calcul actual; Forms!/TempVars |
| [Balanta_inchidere_an](VERCON_ACCESS/queries/Balanta_inchidere_an.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta interimara (Coduri); Forms!/TempVars |
| [BC subreport 2013](VERCON_ACCESS/queries/BC%20subreport%202013.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [BC_Produs](VERCON_ACCESS/queries/BC_Produs.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de BC_Produs_S1 (Coduri); Forms!/TempVars |
| [BC_Produs_BUN](VERCON_ACCESS/queries/BC_Produs_BUN.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [BC_Produs_FIFO](VERCON_ACCESS/queries/BC_Produs_FIFO.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [BC_Produs_S1](VERCON_ACCESS/queries/BC_Produs_S1.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [BC_Produs_S2](VERCON_ACCESS/queries/BC_Produs_S2.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [Bon Consum](VERCON_ACCESS/queries/Bon%20Consum.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Bonuri consum](VERCON_ACCESS/queries/Bonuri%20consum.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Centralizator_BC](VERCON_ACCESS/queries/Centralizator_BC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Copac_BC](VERCON_ACCESS/queries/Copac_BC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Fisa de magazie antet furnizori](VERCON_ACCESS/queries/Fisa%20de%20magazie%20antet%20furnizori.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Fisa de magazie antet](VERCON_ACCESS/queries/Fisa%20de%20magazie%20antet.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Fisa de magazie Detaliu Furnizori](VERCON_ACCESS/queries/Fisa%20de%20magazie%20Detaliu%20Furnizori.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Fisa de magazie Detaliu](VERCON_ACCESS/queries/Fisa%20de%20magazie%20Detaliu.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [FM_IDP](VERCON_ACCESS/queries/FM_IDP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Iesiri-_Iesiri](VERCON_ACCESS/queries/Iesiri-_Iesiri.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Iesiri-_Intrari](VERCON_ACCESS/queries/Iesiri-_Intrari.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Listare Furnizori](VERCON_ACCESS/queries/Listare%20Furnizori.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Listare NIR 2](VERCON_ACCESS/queries/Listare%20NIR%202.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Listare NIR](VERCON_ACCESS/queries/Listare%20NIR.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Listare toti furnizorii](VERCON_ACCESS/queries/Listare%20toti%20furnizorii.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [MF_Balanta](VERCON_ACCESS/queries/MF_Balanta.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [MF_Union_Balanta](VERCON_ACCESS/queries/MF_Union_Balanta.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [OI_Balanta](VERCON_ACCESS/queries/OI_Balanta.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [OI_Evidenta](VERCON_ACCESS/queries/OI_Evidenta.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [OI_MaxID](VERCON_ACCESS/queries/OI_MaxID.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [OI_Union](VERCON_ACCESS/queries/OI_Union.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [OI_Union_Balanta](VERCON_ACCESS/queries/OI_Union_Balanta.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [Per](VERCON_ACCESS/queries/Per.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QBC](VERCON_ACCESS/queries/QBC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [qBCPT](VERCON_ACCESS/queries/qBCPT.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QBCR](VERCON_ACCESS/queries/QBCR.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QBP](VERCON_ACCESS/queries/QBP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QDEP](VERCON_ACCESS/queries/QDEP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QDEPL](VERCON_ACCESS/queries/QDEPL.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QEP](VERCON_ACCESS/queries/QEP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QFC](VERCON_ACCESS/queries/QFC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QFCDEL](VERCON_ACCESS/queries/QFCDEL.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QFCP](VERCON_ACCESS/queries/QFCP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QFCS](VERCON_ACCESS/queries/QFCS.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QFCS_SUB](VERCON_ACCESS/queries/QFCS_SUB.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QFCTMP](VERCON_ACCESS/queries/QFCTMP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QFCTMPMF](VERCON_ACCESS/queries/QFCTMPMF.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QFMA](VERCON_ACCESS/queries/QFMA.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QFMD](VERCON_ACCESS/queries/QFMD.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QIESIRI](VERCON_ACCESS/queries/QIESIRI.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [qIesiri_H](VERCON_ACCESS/queries/qIesiri_H.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QLABC](VERCON_ACCESS/queries/QLABC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QOIC](VERCON_ACCESS/queries/QOIC.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QOICP](VERCON_ACCESS/queries/QOICP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QOIT](VERCON_ACCESS/queries/QOIT.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QOITP](VERCON_ACCESS/queries/QOITP.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [qRetur](VERCON_ACCESS/queries/qRetur.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_1](VERCON_ACCESS/queries/QSFP_1.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_2](VERCON_ACCESS/queries/QSFP_2.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_3](VERCON_ACCESS/queries/QSFP_3.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_4](VERCON_ACCESS/queries/QSFP_4.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_5](VERCON_ACCESS/queries/QSFP_5.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars |
| [QSFP_BL](VERCON_ACCESS/queries/QSFP_BL.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [QSFP_FM](VERCON_ACCESS/queries/QSFP_FM.sql) |  | INCLUS-CALCUL | calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03) |
| [Query1](VERCON_ACCESS/queries/Query1.sql) |  | NEREFERIT-NEPORTAT | INSERT in TmpFact; nicio referinta in export; Forms!/TempVars |
| [Retur_Produse](VERCON_ACCESS/queries/Retur_Produse.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [Retur_produse_prost](VERCON_ACCESS/queries/Retur_produse_prost.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [SFP_Balanta](VERCON_ACCESS/queries/SFP_Balanta.sql) |  | INCLUS-CALCUL | calcul actual; Forms!/TempVars |
| [Storno](VERCON_ACCESS/queries/Storno.sql) |  | EXCLUS-CODURI | refera Coduri (N06); Forms!/TempVars |
| [temp1](VERCON_ACCESS/queries/temp1.sql) |  | NEREFERIT-NEPORTAT | coloane inexistente in schema curenta (Intrari.CodProdus); nicio referinta |
| [tempQ](VERCON_ACCESS/queries/tempQ.sql) |  | NEREFERIT-NEPORTAT | sursa de lista cu REȚETAR pentru ListaAlimente (Forms!ListaAlimente!mdv); nicio referinta; Forms!/TempVars |
| [XLS_OI_MAT](VERCON_ACCESS/queries/XLS_OI_MAT.sql) |  | EXCLUS-CODURI | dependenta tranzitiva de Balanta listare 2 (Coduri) |
| [_TEMP_Actualizeaza_BC_IDFV](VERCON_ACCESS/queries/_TEMP_Actualizeaza_BC_IDFV.sql) |  | NEREFERIT-NEPORTAT | UPDATE unic pe coloane inexistente (Iesiri.IdIntrare/IDF); nicio referinta |

## Forms — 53

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [AdaugaTabelInTBLS](VERCON_ACCESS/Forms/AdaugaTabelInTBLS.txt) | Exclus | EXCLUS-SCHEMA-ACCESS | marcaje: refera mecanism schema Access |
| [ALEGE_BC](VERCON_ACCESS/Forms/ALEGE_BC.txt) | BC | INCLUS-UI |  |
| [ALEGE_BC_RETUR](VERCON_ACCESS/Forms/ALEGE_BC_RETUR.txt) | BR | INCLUS-UI |  |
| [BC](VERCON_ACCESS/Forms/BC.txt) | BC | INCLUS-UI | Setarea FIFO legacy nu reactiveaza Coduri. |
| [BCP](VERCON_ACCESS/Forms/BCP.txt) | BC | INCLUS-UI | fereastra de alegere produs pentru BC (pozitionata langa Denprodus, BC.txt:2590); BC |
| [BR](VERCON_ACCESS/Forms/BR.txt) | BR | INCLUS-UI |  |
| [Clienti](VERCON_ACCESS/Forms/Clienti.txt) | Nomenclatoare | INCLUS-UI |  |
| [CLS2024](VERCON_ACCESS/Forms/CLS2024.txt) | Nomenclatoare | INCLUS-UI | depinde de decizia D08 (CPV comun) |
| [CN](VERCON_ACCESS/Forms/CN.txt) | Alimente | INCLUS-UI |  |
| [ComisieINV subform](VERCON_ACCESS/Forms/ComisieINV%20subform.txt) | Nomenclatoare | INCLUS-UI |  |
| [ComisieNIR subform](VERCON_ACCESS/Forms/ComisieNIR%20subform.txt) | Nomenclatoare | INCLUS-UI |  |
| [Config](VERCON_ACCESS/Forms/Config.txt) | Configurare | INCLUS-UI | mapare pe VC_Gestiuni/VC_Settings |
| [CPV2017](VERCON_ACCESS/Forms/CPV2017.txt) | Nomenclatoare | INCLUS-UI | depinde de D08 |
| [D](VERCON_ACCESS/Forms/D.txt) | Alimente | INCLUS-UI |  |
| [EFACTURA](VERCON_ACCESS/Forms/EFACTURA.txt) | Exclus | EXCLUS-EFACTURA | marcaje: refera e-Factura |
| [FC](VERCON_ACCESS/Forms/FC.txt) | FC | INCLUS-UI | Contine cod legacy Coduri in ramuri exclusive: ignorat.; marcaje: refera Coduri |
| [FCS](VERCON_ACCESS/Forms/FCS.txt) | FCS | INCLUS-UI |  |
| [Feluri](VERCON_ACCESS/Forms/Feluri.txt) | Alimente | INCLUS-UI |  |
| [frmMail](VERCON_ACCESS/Forms/frmMail.txt) | Shell | EXCLUS-INFRA-ACCESS | backup/FTP; marcaje: refera FTP |
| [frmPB](VERCON_ACCESS/Forms/frmPB.txt) | Shell | EXCLUS-INFRA-ACCESS |  |
| [frmProgressBar](VERCON_ACCESS/Forms/frmProgressBar.txt) | Shell | EXCLUS-INFRA-ACCESS |  |
| [Furnizori](VERCON_ACCESS/Forms/Furnizori.txt) | Nomenclatoare | INLOCUIT-PARTENERI-KBOT | forma veche de furnizori; inlocuita de Parteneri KBOT |
| [Furnizori2022](VERCON_ACCESS/Forms/Furnizori2022.txt) | Nomenclatoare | INLOCUIT-PARTENERI-KBOT | Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real; completarea dupa CUI din ANAF nu se portează; marcaje: refera ANAF |
| [Gestionari](VERCON_ACCESS/Forms/Gestionari.txt) | Configurare | NEREFERIT-NEPORTAT | nedeschis din ribbon/cod; sursa „Gestionari" nu este tabel in export; ribbonul „Gestionari" deschide Clienti |
| [IA](VERCON_ACCESS/Forms/IA.txt) | Inchidere an | INCLUS-UI | NU se executa pe originale. |
| [LABC](VERCON_ACCESS/Forms/LABC.txt) | Alimente | INCLUS-UI |  |
| [LEGBCLA](VERCON_ACCESS/Forms/LEGBCLA.txt) | Alimente | INCLUS-UI |  |
| [ListaAlimente](VERCON_ACCESS/Forms/ListaAlimente.txt) | Alimente | INCLUS-UI |  |
| [ListaAlimente_copy](VERCON_ACCESS/Forms/ListaAlimente_copy.txt) | Alimente | NEREFERIT-NEPORTAT | copie a ListaAlimente, nereferita |
| [Main](VERCON_ACCESS/Forms/Main.txt) | Shell | EXCLUS-INFRA-ACCESS | navigare Access; inlocuita de portal |
| [Masini](VERCON_ACCESS/Forms/Masini.txt) | Nomenclatoare | INCLUS-UI |  |
| [Masini_sub](VERCON_ACCESS/Forms/Masini_sub.txt) | Nomenclatoare | INCLUS-UI |  |
| [MD](VERCON_ACCESS/Forms/MD.txt) | Alimente | INCLUS-UI |  |
| [Mesaje_ANAF](VERCON_ACCESS/Forms/Mesaje_ANAF.txt) | Exclus | EXCLUS-EFACTURA | marcaje: refera e-Factura |
| [MonthPicker](VERCON_ACCESS/Forms/MonthPicker.txt) | Shell | EXCLUS-INFRA-ACCESS | inlocuit de calendarul comun |
| [OI_Casare](VERCON_ACCESS/Forms/OI_Casare.txt) | OI | INCLUS-UI |  |
| [OI_Predare](VERCON_ACCESS/Forms/OI_Predare.txt) | OI | INCLUS-UI |  |
| [OI_TmpFact](VERCON_ACCESS/Forms/OI_TmpFact.txt) | OI | INCLUS-UI |  |
| [OI_Transfer](VERCON_ACCESS/Forms/OI_Transfer.txt) | OI | INCLUS-UI |  |
| [Produse](VERCON_ACCESS/Forms/Produse.txt) | Nomenclatoare | INCLUS-UI |  |
| [ProduseM](VERCON_ACCESS/Forms/ProduseM.txt) | Nomenclatoare | INCLUS-UI |  |
| [SETARI](VERCON_ACCESS/Forms/SETARI.txt) | Configurare | INCLUS-UI |  |
| [SFP](VERCON_ACCESS/Forms/SFP.txt) | Situatii | INCLUS-UI | Meniu din tmpOptiuniSFP + MODEL. |
| [subTreeView](VERCON_ACCESS/Forms/subTreeView.txt) | Shell | EXCLUS-INFRA-ACCESS | componenta UI Access |
| [TmpFact](VERCON_ACCESS/Forms/TmpFact.txt) | FC | INCLUS-UI | marcaje: refera e-Factura |
| [TmpFactMF](VERCON_ACCESS/Forms/TmpFactMF.txt) | FC | INCLUS-UI | marcaje: refera e-Factura |
| [TmpFactV](VERCON_ACCESS/Forms/TmpFactV.txt) | FC | INCLUS-UI |  |
| [tmpSFP](VERCON_ACCESS/Forms/tmpSFP.txt) | Situatii | INCLUS-UI |  |
| [tmp_Balanta subform](VERCON_ACCESS/Forms/tmp_Balanta%20subform.txt) | Situatii | INCLUS-UI |  |
| [UNIT](VERCON_ACCESS/Forms/UNIT.txt) | Configurare | INCLUS-PARTIAL | marcaje: refera e-Factura, refera FTP |
| [VaerCon](VERCON_ACCESS/Forms/VaerCon.txt) | Shell | EXCLUS-INFRA-ACCESS | inlocuit de selectorul de context; regulile de context se mapeaza |
| [XMLPath](VERCON_ACCESS/Forms/XMLPath.txt) | Exclus | EXCLUS-EFACTURA | marcaje: refera e-Factura |
| [XMLPath_sub](VERCON_ACCESS/Forms/XMLPath_sub.txt) | Exclus | EXCLUS-EFACTURA | marcaje: refera e-Factura |

## Reports — 61

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [Amortizare](VERCON_ACCESS/Reports/Amortizare.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [Balanta_produse](VERCON_ACCESS/Reports/Balanta_produse.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [Balanta_produse_AFB](VERCON_ACCESS/Reports/Balanta_produse_AFB.txt) | Stoc/fise/balante/inventar | EXCLUS-CODURI | dependenta tranzitiva de Balanta (Coduri); obiect legacy exclus N06 |
| [Balanta_Produse_BF](VERCON_ACCESS/Reports/Balanta_Produse_BF.txt) | Stoc/fise/balante/inventar | EXCLUS-CODURI | dependenta tranzitiva de Balanta listare 1 (Coduri); obiect legacy exclus N06 |
| [Balanta_produse_furnizor](VERCON_ACCESS/Reports/Balanta_produse_furnizor.txt) | Stoc/fise/balante/inventar | EXCLUS-CODURI | dependenta tranzitiva de Balanta listare 2 (Coduri); obiect legacy exclus N06 |
| [BC subreport 2013](VERCON_ACCESS/Reports/BC%20subreport%202013.txt) | BC/BR | INCLUS-RAPORT |  |
| [BonRetur](VERCON_ACCESS/Reports/BonRetur.txt) | BC/BR | INCLUS-RAPORT |  |
| [Bon_Consum](VERCON_ACCESS/Reports/Bon_Consum.txt) | BC/BR | INCLUS-RAPORT | marcaje: refera e-Factura |
| [BUConsum](VERCON_ACCESS/Reports/BUConsum.txt) | BC/BR | INCLUS-RAPORT |  |
| [BUConsum_F](VERCON_ACCESS/Reports/BUConsum_F.txt) | BC/BR | INCLUS-RAPORT |  |
| [BUConsum_F1](VERCON_ACCESS/Reports/BUConsum_F1.txt) | BC/BR | INCLUS-RAPORT | marcaje: refera e-Factura |
| [Centralizator_BC](VERCON_ACCESS/Reports/Centralizator_BC.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Centralizator_Conturi](VERCON_ACCESS/Reports/Centralizator_Conturi.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Centralizator_Conturi_Furnizori](VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Centralizator_Furnizori](VERCON_ACCESS/Reports/Centralizator_Furnizori.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Centralizator_OI_Bon_BP](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt) | OI bonuri | INCLUS-RAPORT |  |
| [Centralizator_OI_Bon_BS](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt) | OI bonuri | INCLUS-RAPORT |  |
| [Centralizator_OI_Bon_BT](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) | OI bonuri | INCLUS-RAPORT |  |
| [Cina subreport](VERCON_ACCESS/Reports/Cina%20subreport.txt) | Alimente | INCLUS-RAPORT |  |
| [Consumuri_Produs](VERCON_ACCESS/Reports/Consumuri_Produs.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Consumuri_Unitati](VERCON_ACCESS/Reports/Consumuri_Unitati.txt) | Centralizatoare | INCLUS-RAPORT |  |
| [Dejun subreport](VERCON_ACCESS/Reports/Dejun%20subreport.txt) | Alimente | INCLUS-RAPORT |  |
| [Fisa_Evidenta](VERCON_ACCESS/Reports/Fisa_Evidenta.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [Fisa_Magazie](VERCON_ACCESS/Reports/Fisa_Magazie.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [Fisa_Magazie_BF](VERCON_ACCESS/Reports/Fisa_Magazie_BF.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [Fisa_Produsului](VERCON_ACCESS/Reports/Fisa_Produsului.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [FM1-u](VERCON_ACCESS/Reports/FM1-u.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [FM2](VERCON_ACCESS/Reports/FM2.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [ListaAlimente2015](VERCON_ACCESS/Reports/ListaAlimente2015.txt) | Alimente | INCLUS-RAPORT | marcaje: refera e-Factura, refera FTP |
| [LISTA_INVENTAR](VERCON_ACCESS/Reports/LISTA_INVENTAR.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |
| [MF_Balanta](VERCON_ACCESS/Reports/MF_Balanta.txt) | MF | INCLUS-RAPORT |  |
| [MF_Balanta_Subinv](VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt) | MF | INCLUS-RAPORT |  |
| [MF_Bon_BP](VERCON_ACCESS/Reports/MF_Bon_BP.txt) | MF | INCLUS-RAPORT | marcaje: refera FTP |
| [MF_Bon_BS](VERCON_ACCESS/Reports/MF_Bon_BS.txt) | MF | INCLUS-RAPORT | marcaje: refera FTP |
| [MF_Bon_BT](VERCON_ACCESS/Reports/MF_Bon_BT.txt) | MF | INCLUS-RAPORT | marcaje: refera FTP |
| [MF_LISTA_INVENTAR](VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) | MF | INCLUS-RAPORT |  |
| [MicDejun subreport](VERCON_ACCESS/Reports/MicDejun%20subreport.txt) | Alimente | INCLUS-RAPORT |  |
| [NIR](VERCON_ACCESS/Reports/NIR.txt) | NIR | INCLUS-RAPORT |  |
| [NIR_SFP](VERCON_ACCESS/Reports/NIR_SFP.txt) | NIR | INCLUS-RAPORT |  |
| [NIR_Storno](VERCON_ACCESS/Reports/NIR_Storno.txt) | NIR | INCLUS-RAPORT |  |
| [Nota_Comanda](VERCON_ACCESS/Reports/Nota_Comanda.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [nou_Furnizori](VERCON_ACCESS/Reports/nou_Furnizori.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [OI_Balanta](VERCON_ACCESS/Reports/OI_Balanta.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Bon_BP](VERCON_ACCESS/Reports/OI_Bon_BP.txt) | OI bonuri | INCLUS-RAPORT | marcaje: refera FTP |
| [OI_Bon_BS](VERCON_ACCESS/Reports/OI_Bon_BS.txt) | OI bonuri | INCLUS-RAPORT | marcaje: refera FTP |
| [OI_Bon_BT](VERCON_ACCESS/Reports/OI_Bon_BT.txt) | OI bonuri | INCLUS-RAPORT | marcaje: refera FTP |
| [OI_Casare](VERCON_ACCESS/Reports/OI_Casare.txt) | OI documente (formular-raport) | INCLUS-RAPORT | marcaje: refera FTP |
| [OI_Fisa_Evidenta](VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Fisa_Evidenta_Primitori](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Fisa_Evidenta_Primitori_STOC](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori_STOC.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Fisa_Evidenta_Produse](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Fisa_Evidenta_Produse_STOC](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse_STOC.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_LISTA_INVENTAR](VERCON_ACCESS/Reports/OI_LISTA_INVENTAR.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_LISTA_INVENTAR_TOTAL](VERCON_ACCESS/Reports/OI_LISTA_INVENTAR_TOTAL.txt) | OI evidenta/inventar | INCLUS-RAPORT |  |
| [OI_Predare](VERCON_ACCESS/Reports/OI_Predare.txt) | OI documente (formular-raport) | INCLUS-RAPORT | marcaje: refera FTP |
| [OI_Transfer](VERCON_ACCESS/Reports/OI_Transfer.txt) | OI documente (formular-raport) | INCLUS-RAPORT | marcaje: refera FTP |
| [Plan_Achizitii](VERCON_ACCESS/Reports/Plan_Achizitii.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [Referat_periodic_Intrari](VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [Referat_periodic_produs](VERCON_ACCESS/Reports/Referat_periodic_produs.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [Situatie_Intrari](VERCON_ACCESS/Reports/Situatie_Intrari.txt) | Comenzi/referate/situatii | INCLUS-RAPORT |  |
| [Stoc_Final](VERCON_ACCESS/Reports/Stoc_Final.txt) | Stoc/fise/balante/inventar | INCLUS-RAPORT |  |

## Modules — 44

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [basGDIPlus](VERCON_ACCESS/Modules/basGDIPlus.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [basRibbonCallbacks](VERCON_ACCESS/Modules/basRibbonCallbacks.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [basRibbonGlobals](VERCON_ACCESS/Modules/basRibbonGlobals.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [BCrypt](VERCON_ACCESS/Modules/BCrypt.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [BStorage](VERCON_ACCESS/Modules/BStorage.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_2024](VERCON_ACCESS/Modules/mdl_2024.bas.txt) |  | EXCLUS-SCHEMA-ACCESS | migrare/legare schema Access |
| [mdl_64Bits](VERCON_ACCESS/Modules/mdl_64Bits.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_AdaugStocFM](VERCON_ACCESS/Modules/mdl_AdaugStocFM.bas.txt) |  | INCLUS-CALCUL | logica de business/export de analizat (N09 pentru AdaugStocFM) |
| [mdl_AppendText](VERCON_ACCESS/Modules/mdl_AppendText.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Autoexec](VERCON_ACCESS/Modules/mdl_Autoexec.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_ChangeRemovePWD](VERCON_ACCESS/Modules/mdl_ChangeRemovePWD.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_compact&repair](VERCON_ACCESS/Modules/mdl_compact&repair.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_CryptDecrypt](VERCON_ACCESS/Modules/mdl_CryptDecrypt.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_debugPrint](VERCON_ACCESS/Modules/mdl_debugPrint.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Detect7Zip](VERCON_ACCESS/Modules/mdl_Detect7Zip.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_DISKRW](VERCON_ACCESS/Modules/mdl_DISKRW.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_EditData](VERCON_ACCESS/Modules/mdl_EditData.bas.txt) |  | INCLUS-PARTIAL | amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel; contine referinte Coduri |
| [mdl_EFactura](VERCON_ACCESS/Modules/mdl_EFactura.bas.txt) |  | EXCLUS-EFACTURA | e-Factura |
| [mdl_ExecWait](VERCON_ACCESS/Modules/mdl_ExecWait.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_ExistaFunctia](VERCON_ACCESS/Modules/mdl_ExistaFunctia.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Export_Excel](VERCON_ACCESS/Modules/mdl_Export_Excel.bas.txt) |  | INCLUS-CALCUL | logica de business/export de analizat (N09 pentru AdaugStocFM) |
| [mdl_FormatNumber](VERCON_ACCESS/Modules/mdl_FormatNumber.bas.txt) |  | INCLUS-CALCUL | logica de business/export de analizat (N09 pentru AdaugStocFM) |
| [mdl_FTP](VERCON_ACCESS/Modules/mdl_FTP.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Functii](VERCON_ACCESS/Modules/mdl_Functii.bas.txt) |  | INCLUS-PARTIAL | amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel; contine referinte Coduri |
| [mdl_HTML](VERCON_ACCESS/Modules/mdl_HTML.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_JSON](VERCON_ACCESS/Modules/mdl_JSON.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_LegTabele](VERCON_ACCESS/Modules/mdl_LegTabele.bas.txt) |  | EXCLUS-SCHEMA-ACCESS | migrare/legare schema Access |
| [mdl_ListSeparator](VERCON_ACCESS/Modules/mdl_ListSeparator.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_MouseCursor](VERCON_ACCESS/Modules/mdl_MouseCursor.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_MouseScroll](VERCON_ACCESS/Modules/mdl_MouseScroll.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_OI_Produse](VERCON_ACCESS/Modules/mdl_OI_Produse.bas.txt) |  | INCLUS-CALCUL | logica de business/export de analizat (N09 pentru AdaugStocFM) |
| [mdl_Popup2022](VERCON_ACCESS/Modules/mdl_Popup2022.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_PositionForm](VERCON_ACCESS/Modules/mdl_PositionForm.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_PozitieForm](VERCON_ACCESS/Modules/mdl_PozitieForm.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Retea](VERCON_ACCESS/Modules/mdl_Retea.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_siguranta](VERCON_ACCESS/Modules/mdl_siguranta.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Timer](VERCON_ACCESS/Modules/mdl_Timer.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_UpdateFE](VERCON_ACCESS/Modules/mdl_UpdateFE.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_VerificaCF](VERCON_ACCESS/Modules/mdl_VerificaCF.bas.txt) |  | INLOCUIT-PARTENERI-KBOT | verificare CF/ANAF a furnizorului; Partenerii sunt ai KBOT |
| [mdl_WindowsVersion](VERCON_ACCESS/Modules/mdl_WindowsVersion.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [mdl_Zip_Unzip](VERCON_ACCESS/Modules/mdl_Zip_Unzip.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [modStartup](VERCON_ACCESS/Modules/modStartup.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [newProgressBar](VERCON_ACCESS/Modules/newProgressBar.bas.txt) |  | EXCLUS-INFRA-ACCESS | infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web |
| [Utile](VERCON_ACCESS/Modules/Utile.bas.txt) |  | INCLUS-PARTIAL | amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel |

## Classes — 27

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [clFormWindow](VERCON_ACCESS/Classes/clFormWindow.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsDatabase](VERCON_ACCESS/Classes/clsDatabase.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsDatabaseError](VERCON_ACCESS/Classes/clsDatabaseError.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsDatabaseErrors](VERCON_ACCESS/Classes/clsDatabaseErrors.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsField](VERCON_ACCESS/Classes/clsField.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsFirma](VERCON_ACCESS/Classes/clsFirma.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaDG](VERCON_ACCESS/Classes/clsFirmaDG.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaInactiva](VERCON_ACCESS/Classes/clsFirmaInactiva.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaSF](VERCON_ACCESS/Classes/clsFirmaSF.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaSS](VERCON_ACCESS/Classes/clsFirmaSS.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaTVA](VERCON_ACCESS/Classes/clsFirmaTVA.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaTVAI](VERCON_ACCESS/Classes/clsFirmaTVAI.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirmaTVAS](VERCON_ACCESS/Classes/clsFirmaTVAS.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsFirme](VERCON_ACCESS/Classes/clsFirme.cls.txt) |  | INLOCUIT-PARTENERI-KBOT | client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează |
| [clsIndex](VERCON_ACCESS/Classes/clsIndex.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsNode](VERCON_ACCESS/Classes/clsNode.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsPing](VERCON_ACCESS/Classes/clsPing.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsPopup](VERCON_ACCESS/Classes/clsPopup.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsQuery](VERCON_ACCESS/Classes/clsQuery.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsRegExp](VERCON_ACCESS/Classes/clsRegExp.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsRegExp_Match](VERCON_ACCESS/Classes/clsRegExp_Match.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsRelation](VERCON_ACCESS/Classes/clsRelation.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsTable](VERCON_ACCESS/Classes/clsTable.cls.txt) |  | EXCLUS-SCHEMA-ACCESS | DBS/clsDatabase si clasele copil |
| [clsTreeview](VERCON_ACCESS/Classes/clsTreeview.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |
| [clsWinSCP](VERCON_ACCESS/Classes/clsWinSCP.cls.txt) |  | EXCLUS-INFRA-ACCESS | FTP/WinSCP |
| [clsWinScpFile](VERCON_ACCESS/Classes/clsWinScpFile.cls.txt) |  | EXCLUS-INFRA-ACCESS | FTP/WinSCP |
| [iTimer](VERCON_ACCESS/Classes/iTimer.cls.txt) |  | EXCLUS-INFRA-ACCESS | utilitar UI/Windows (regex, treeview, popup, timer, ping, form window) |

## Macros — 3

| Obiect | Familie | Dispozitie | Nota |
|---|---|---|---|
| [Afiseaza cod](VERCON_ACCESS/Macros/Afiseaza%20cod.txt) |  | EXCLUS-INFRA-ACCESS | pornire Access (Autoexec_ apeleaza Productie/AExec: nu se executa) |
| [Autoexec_](VERCON_ACCESS/Macros/Autoexec_.txt) |  | EXCLUS-INFRA-ACCESS | pornire Access (Autoexec_ apeleaza Productie/AExec: nu se executa) |
| [AutoKeys](VERCON_ACCESS/Macros/AutoKeys.txt) |  | EXCLUS-INFRA-ACCESS | pornire Access (Autoexec_ apeleaza Productie/AExec: nu se executa) |
