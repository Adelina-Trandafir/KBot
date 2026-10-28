# VERCON — inventarul obiectelor Access (SLICE-VC00-01)

Generat de `VERCON/tools/inventar.js` din `VERCON_ACCESS` (citire doar-citire, 11.10.2026). Clasificare **statica** dupa nume si marcaje din continut; nu dovedeste utilizare in productie si nu autorizeaza stergeri. Fiecare obiect apare o singura data. Ce nu e demonstrat: utilizarea reala a obiectelor DE-CLARIFICAT (vezi [DECIZII_DESCHISE](DECIZII_DESCHISE.md)).

Total 377 obiecte: table 109, query 80, form 53, report 61, module 44, class 27, macro 3.

Compactare documentară SLICE-D003-11: fiecare profil `obiecte — Dispoziție; Familie=…; Nota=…` reunește numai obiecte cu Familie, Dispoziție și Notă identice, în aceeași categorie. Familie/Nota omise înseamnă exact celulă goală în inventarul original. Numărul dinaintea fiecărui nume păstrează poziția inițială în categorie (inclusiv sensul eventualelor note „idem”); legăturile sursă rămân individuale. Regenerarea inventarului poate suprascrie această prezentare.

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

- 1: [@ComisieInventar](VERCON_ACCESS/tables/@ComisieInventar.md) — EXCLUS-SCHEMA-ACCESS; Nota=sablon structura "@" folosit de mecanismul de schema Access; 5 campuri
- 2: [@EF](VERCON_ACCESS/tables/@EF.md)<br>21: [EF](VERCON_ACCESS/tables/EF.md) — EXCLUS-EFACTURA; Nota=e-Factura (N13); 9 campuri
- 3: [@EFS](VERCON_ACCESS/tables/@EFS.md)<br>22: [EFS](VERCON_ACCESS/tables/EFS.md)<br>24: [EF_TMP](VERCON_ACCESS/tables/EF_TMP.md) — EXCLUS-EFACTURA; Nota=e-Factura (N13); 10 campuri
- 4: [@EFT](VERCON_ACCESS/tables/@EFT.md)<br>23: [EFT](VERCON_ACCESS/tables/EFT.md) — EXCLUS-EFACTURA; Nota=e-Factura (N13); 15 campuri
- 5: [@ProduseM](VERCON_ACCESS/tables/@ProduseM.md) — EXCLUS-SCHEMA-ACCESS; Nota=sablon structura "@" folosit de mecanismul de schema Access; 6 campuri
- 6: [@RunOnce](VERCON_ACCESS/tables/@RunOnce.md)<br>57: [RunOnce](VERCON_ACCESS/tables/RunOnce.md)<br>108: [_TBLS_](VERCON_ACCESS/tables/_TBLS_.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 5 campuri
- 7: [@Setari](VERCON_ACCESS/tables/@Setari.md)<br>8: [@Trimis](VERCON_ACCESS/tables/@Trimis.md) — EXCLUS-SCHEMA-ACCESS; Nota=sablon structura "@" folosit de mecanismul de schema Access; 3 campuri
- 9: [@UNIT](VERCON_ACCESS/tables/@UNIT.md) — EXCLUS-SCHEMA-ACCESS; Nota=sablon structura "@" folosit de mecanismul de schema Access; 21 campuri
- 10: [Alimente](VERCON_ACCESS/tables/Alimente.md)<br>15: [Clienti](VERCON_ACCESS/tables/Clienti.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 7 campuri
- 11: [cale](VERCON_ACCESS/tables/cale.md) — INCLUS-CONFIG; Nota=identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 15 campuri
- 12: [Caletemp](VERCON_ACCESS/tables/Caletemp.md) — INCLUS-CONFIG; Nota=identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 9 campuri
- 13: [Cina](VERCON_ACCESS/tables/Cina.md)<br>20: [Dejun](VERCON_ACCESS/tables/Dejun.md)<br>44: [MicDejun](VERCON_ACCESS/tables/MicDejun.md) — INCLUS-DATE; Nota=date business istorice; 5 campuri
- 14: [Clasificari](VERCON_ACCESS/tables/Clasificari.md) — INCLUS-NOMENCLATOR; Nota=folosit de FC, Produse, CLS2024 (formulare active); date de profilat; 10 campuri
- 16: [ComisieInventar](VERCON_ACCESS/tables/ComisieInventar.md)<br>17: [ComisieNIR](VERCON_ACCESS/tables/ComisieNIR.md)<br>54: [ProduseP](VERCON_ACCESS/tables/ProduseP.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 5 campuri
- 18: [cpv](VERCON_ACCESS/tables/cpv.md) — INCLUS-NOMENCLATOR; Nota=nomenclator CPV in backend (9.454 randuri in baza Alimente), folosit de Produse; D08; 2 campuri
- 19: [CPV1](VERCON_ACCESS/tables/CPV1.md) — INCLUS-NOMENCLATOR; Nota=folosit de formularul CPV2017 (deschis din Produse); 9 campuri
- 25: [Erori Intrare Tbl](VERCON_ACCESS/tables/Erori%20Intrare%20Tbl.md) — EXCLUS-INFRA-ACCESS; Nota=jurnal de erori al mecanismului de schema Access (referit doar de VaerCon in cod comentat, clsDatabaseErrors, newProgressBar); 7 campuri
- 26: [Erori Intrare](VERCON_ACCESS/tables/Erori%20Intrare.md) — EXCLUS-INFRA-ACCESS; Nota=idem; 8 campuri
- 27: [Erori_Intrare](VERCON_ACCESS/tables/Erori_Intrare.md) — EXCLUS-INFRA-ACCESS; Nota=jurnal de erori clsDatabaseErrors (DBS); 7 campuri
- 28: [EXT_IDX](VERCON_ACCESS/tables/EXT_IDX.md)<br>101: [_IDX_](VERCON_ACCESS/tables/_IDX_.md)<br>107: [_tblRel](VERCON_ACCESS/tables/_tblRel.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 9 campuri
- 29: [Feluri](VERCON_ACCESS/tables/Feluri.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 1 campuri
- 30: [FeluriMancare](VERCON_ACCESS/tables/FeluriMancare.md)<br>43: [Masini](VERCON_ACCESS/tables/Masini.md)<br>93: [TVA](VERCON_ACCESS/tables/TVA.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 3 campuri
- 31: [FM_2](VERCON_ACCESS/tables/FM_2.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 11 campuri
- 32: [Furnizori](VERCON_ACCESS/tables/Furnizori.md) — INLOCUIT-PARTENERI-KBOT; Nota=Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real; 9 campuri
- 33: [gest_subCPV](VERCON_ACCESS/tables/gest_subCPV.md) — NEREFERIT-NEPORTAT; Nota=nicio referinta in export; 3 campuri
- 34: [Iesiri](VERCON_ACCESS/tables/Iesiri.md)<br>40: [LABC](VERCON_ACCESS/tables/LABC.md) — INCLUS-DATE; Nota=date business istorice; 7 campuri
- 35: [IesiriRetur](VERCON_ACCESS/tables/IesiriRetur.md)<br>38: [IntrariStorno](VERCON_ACCESS/tables/IntrariStorno.md)<br>53: [ProduseM](VERCON_ACCESS/tables/ProduseM.md) — INCLUS-DATE; Nota=date business istorice; 6 campuri
- 36: [Iesiri_H](VERCON_ACCESS/tables/Iesiri_H.md) — INCLUS-DATE; Nota=date business istorice; 10 campuri
- 37: [Intrari](VERCON_ACCESS/tables/Intrari.md) — INCLUS-DATE; Nota=date business istorice; 11 campuri
- 39: [Jud](VERCON_ACCESS/tables/Jud.md) — INCLUS-NOMENCLATOR; Nota=lista judete, folosita de formularul UNIT; 3 campuri
- 41: [LegBCLA](VERCON_ACCESS/tables/LegBCLA.md) — INCLUS-DATE; Nota=date business istorice; 2 campuri
- 42: [ListaAlimente](VERCON_ACCESS/tables/ListaAlimente.md) — INCLUS-DATE; Nota=date business istorice; 15 campuri
- 45: [NIR](VERCON_ACCESS/tables/NIR.md)<br>51: [OI_Transfer](VERCON_ACCESS/tables/OI_Transfer.md) — INCLUS-DATE; Nota=date business istorice; 13 campuri
- 46: [NR](VERCON_ACCESS/tables/NR.md) — INCLUS-CONFIG; Nota=identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 8 campuri
- 47: [OI_Casare](VERCON_ACCESS/tables/OI_Casare.md)<br>49: [OI_Produse](VERCON_ACCESS/tables/OI_Produse.md) — INCLUS-DATE; Nota=date business istorice; 12 campuri
- 48: [OI_Predare](VERCON_ACCESS/tables/OI_Predare.md) — INCLUS-DATE; Nota=date business istorice; 14 campuri
- 50: [OI_TmpFact](VERCON_ACCESS/tables/OI_TmpFact.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 14 campuri
- 52: [Produse](VERCON_ACCESS/tables/Produse.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 8 campuri
- 55: [Rbb](VERCON_ACCESS/tables/Rbb.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 7 campuri
- 56: [Retetar](VERCON_ACCESS/tables/Retetar.md) — INCLUS-NOMENCLATOR; Nota=nomenclator/registru business; 6 campuri
- 58: [Setari](VERCON_ACCESS/tables/Setari.md) — INCLUS-CONFIG; Nota=identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 3 campuri
- 59: [SFP_CautCBX](VERCON_ACCESS/tables/SFP_CautCBX.md) — TEMP-LUCRU; Nota=sursa casetei de cautare din SFP; 4 campuri
- 60: [SQL](VERCON_ACCESS/tables/SQL.md)<br>96: [VEROFFLINE](VERCON_ACCESS/tables/VEROFFLINE.md)<br>106: [_tblQry](VERCON_ACCESS/tables/_tblQry.md)<br>109: [_TBLS_Transfer](VERCON_ACCESS/tables/_TBLS_Transfer.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 3 campuri
- 61: [SQL_](VERCON_ACCESS/tables/SQL_.md)<br>104: [_REL_](VERCON_ACCESS/tables/_REL_.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 10 campuri
- 62: [SQL_FIX_BD](VERCON_ACCESS/tables/SQL_FIX_BD.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 6 campuri
- 63: [SQL_FIX_BD_SUB](VERCON_ACCESS/tables/SQL_FIX_BD_SUB.md)<br>103: [_QRY](VERCON_ACCESS/tables/_QRY.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 4 campuri
- 64: [TblExcel](VERCON_ACCESS/tables/TblExcel.md) — TEMP-LUCRU; Nota=tabel de lucru al exportului Excel (mdl_Export_Excel); 13 campuri
- 65: [tblFiles](VERCON_ACCESS/tables/tblFiles.md)<br>67: [tblPopupMsg](VERCON_ACCESS/tables/tblPopupMsg.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 5 campuri
- 66: [tblFTP](VERCON_ACCESS/tables/tblFTP.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 10 campuri
- 68: [TipFirmeL](VERCON_ACCESS/tables/TipFirmeL.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 2 campuri
- 69: [tmpCoduri](VERCON_ACCESS/tables/tmpCoduri.md)<br>84: [tmp_Coduri](VERCON_ACCESS/tables/tmp_Coduri.md) — EXCLUS-CODURI; Nota=sistem Coduri (N06); 4 campuri
- 70: [TmpFact](VERCON_ACCESS/tables/TmpFact.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 26 campuri
- 71: [TmpFactV](VERCON_ACCESS/tables/TmpFactV.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 9 campuri
- 72: [tmpFMANTET](VERCON_ACCESS/tables/tmpFMANTET.md)<br>83: [Tmp_Cina](VERCON_ACCESS/tables/Tmp_Cina.md)<br>85: [Tmp_Dejun](VERCON_ACCESS/tables/Tmp_Dejun.md)<br>90: [Tmp_MicDejun](VERCON_ACCESS/tables/Tmp_MicDejun.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 5 campuri
- 73: [tmpFM_2](VERCON_ACCESS/tables/tmpFM_2.md)<br>74: [tmpOI](VERCON_ACCESS/tables/tmpOI.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 10 campuri
- 75: [tmpOptiuniSFP](VERCON_ACCESS/tables/tmpOptiuniSFP.md) — INCLUS-CONFIG; Nota=meniul SFP (MODEL/TIP/Raport); DATE PRIMITE de la utilizator: REFERINTE/tmpOptiuniSFP.xml; se transforma in catalog de rapoarte (VC_ReportCatalog); 10 campuri
- 76: [tmpProduseM](VERCON_ACCESS/tables/tmpProduseM.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 7 campuri
- 77: [tmpQFC](VERCON_ACCESS/tables/tmpQFC.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 17 campuri
- 78: [tmpRbb](VERCON_ACCESS/tables/tmpRbb.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 1 campuri
- 79: [tmpSFP](VERCON_ACCESS/tables/tmpSFP.md) — DERIVAT; Nota=rezultat de calcul/raport; 20 campuri
- 80: [tmpXMLPath](VERCON_ACCESS/tables/tmpXMLPath.md) — EXCLUS-EFACTURA; Nota=e-Factura (N13); 6 campuri
- 81: [tmp_Balanta](VERCON_ACCESS/tables/tmp_Balanta.md) — DERIVAT; Nota=rezultat de calcul/raport; 14 campuri
- 82: [tmp_BC_DenProdus](VERCON_ACCESS/tables/tmp_BC_DenProdus.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 20 campuri
- 86: [tmp_II](VERCON_ACCESS/tables/tmp_II.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 2 campuri
- 87: [tmp_LABC](VERCON_ACCESS/tables/tmp_LABC.md)<br>88: [tmp_LEGBCLA](VERCON_ACCESS/tables/tmp_LEGBCLA.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 8 campuri
- 89: [Tmp_Lista](VERCON_ACCESS/tables/Tmp_Lista.md) — TEMP-LUCRU; Nota=scratch Access; drafturile se reconstruiesc izolat pe utilizator; 18 campuri
- 91: [Trimis](VERCON_ACCESS/tables/Trimis.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 3 campuri
- 92: [tSQL](VERCON_ACCESS/tables/tSQL.md)<br>97: [VER_RAP](VERCON_ACCESS/tables/VER_RAP.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 2 campuri
- 94: [UNIT](VERCON_ACCESS/tables/UNIT.md) — INCLUS-CONFIG; Nota=identitate/configurare; mapare pe comune/VC_Settings (nu copiere); 20 campuri
- 95: [Ver](VERCON_ACCESS/tables/Ver.md) — EXCLUS-SCHEMA-ACCESS; Nota=versiune schema Access; citit doar ca diagnostic daca exista; 1 campuri
- 98: [x](VERCON_ACCESS/tables/x.md) — NEREFERIT-NEPORTAT; Nota=tabel de lucru fara nicio referinta in export; 8 campuri
- 99: [XCX](VERCON_ACCESS/tables/XCX.md) — EXCLUS-INFRA-ACCESS; Nota=tabel de lucru al modulului de criptare; 26 campuri
- 100: [_ADODAODDL](VERCON_ACCESS/tables/_ADODAODDL.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 8 campuri
- 102: [_Mail](VERCON_ACCESS/tables/_Mail.md) — EXCLUS-INFRA-ACCESS; Nota=FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop; 14 campuri
- 105: [_Struct_](VERCON_ACCESS/tables/_Struct_.md) — EXCLUS-SCHEMA-ACCESS; Nota=mecanism schema Access (DBS/clsDatabase); 11 campuri

## queries — 80

- 1: [Amortizare](VERCON_ACCESS/queries/Amortizare.sql)<br>8: [Balanta22](VERCON_ACCESS/queries/Balanta22.sql)<br>9: [Balanta223](VERCON_ACCESS/queries/Balanta223.sql)<br>12: [BC subreport 2013](VERCON_ACCESS/queries/BC%20subreport%202013.sql)<br>19: [Bonuri consum](VERCON_ACCESS/queries/Bonuri%20consum.sql)<br>20: [Centralizator_BC](VERCON_ACCESS/queries/Centralizator_BC.sql)<br>24: [Fisa de magazie Detaliu Furnizori](VERCON_ACCESS/queries/Fisa%20de%20magazie%20Detaliu%20Furnizori.sql)<br>25: [Fisa de magazie Detaliu](VERCON_ACCESS/queries/Fisa%20de%20magazie%20Detaliu.sql)<br>27: [Iesiri-_Iesiri](VERCON_ACCESS/queries/Iesiri-_Iesiri.sql)<br>28: [Iesiri-_Intrari](VERCON_ACCESS/queries/Iesiri-_Intrari.sql)<br>29: [Listare Furnizori](VERCON_ACCESS/queries/Listare%20Furnizori.sql)<br>30: [Listare NIR 2](VERCON_ACCESS/queries/Listare%20NIR%202.sql)<br>31: [Listare NIR](VERCON_ACCESS/queries/Listare%20NIR.sql)<br>32: [Listare toti furnizorii](VERCON_ACCESS/queries/Listare%20toti%20furnizorii.sql)<br>34: [MF_Union_Balanta](VERCON_ACCESS/queries/MF_Union_Balanta.sql)<br>38: [OI_Union](VERCON_ACCESS/queries/OI_Union.sql)<br>39: [OI_Union_Balanta](VERCON_ACCESS/queries/OI_Union_Balanta.sql)<br>41: [QBC](VERCON_ACCESS/queries/QBC.sql)<br>43: [QBCR](VERCON_ACCESS/queries/QBCR.sql)<br>44: [QBP](VERCON_ACCESS/queries/QBP.sql)<br>45: [QDEP](VERCON_ACCESS/queries/QDEP.sql)<br>46: [QDEPL](VERCON_ACCESS/queries/QDEPL.sql)<br>50: [QFCP](VERCON_ACCESS/queries/QFCP.sql)<br>52: [QFCS_SUB](VERCON_ACCESS/queries/QFCS_SUB.sql)<br>53: [QFCTMP](VERCON_ACCESS/queries/QFCTMP.sql)<br>54: [QFCTMPMF](VERCON_ACCESS/queries/QFCTMPMF.sql)<br>56: [QFMD](VERCON_ACCESS/queries/QFMD.sql)<br>59: [QLABC](VERCON_ACCESS/queries/QLABC.sql)<br>60: [QOIC](VERCON_ACCESS/queries/QOIC.sql)<br>61: [QOICP](VERCON_ACCESS/queries/QOICP.sql)<br>62: [QOIT](VERCON_ACCESS/queries/QOIT.sql)<br>63: [QOITP](VERCON_ACCESS/queries/QOITP.sql)<br>64: [qRetur](VERCON_ACCESS/queries/qRetur.sql)<br>65: [QSFP_1](VERCON_ACCESS/queries/QSFP_1.sql)<br>66: [QSFP_2](VERCON_ACCESS/queries/QSFP_2.sql)<br>67: [QSFP_3](VERCON_ACCESS/queries/QSFP_3.sql)<br>68: [QSFP_4](VERCON_ACCESS/queries/QSFP_4.sql)<br>69: [QSFP_5](VERCON_ACCESS/queries/QSFP_5.sql) — INCLUS-CALCUL; Nota=calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03); Forms!/TempVars
- 2: [Balanta interimara](VERCON_ACCESS/queries/Balanta%20interimara.sql)<br>3: [Balanta interimara2](VERCON_ACCESS/queries/Balanta%20interimara2.sql) — EXCLUS-CODURI; Nota=dependenta tranzitiva de Balanta (Coduri); Forms!/TempVars
- 4: [Balanta listare 1](VERCON_ACCESS/queries/Balanta%20listare%201.sql)<br>6: [Balanta listare](VERCON_ACCESS/queries/Balanta%20listare.sql) — EXCLUS-CODURI; Nota=dependenta tranzitiva de Balanta interimara2 (Coduri); Forms!/TempVars
- 5: [Balanta listare 2](VERCON_ACCESS/queries/Balanta%20listare%202.sql)<br>11: [Balanta_inchidere_an](VERCON_ACCESS/queries/Balanta_inchidere_an.sql) — EXCLUS-CODURI; Nota=dependenta tranzitiva de Balanta interimara (Coduri); Forms!/TempVars
- 7: [Balanta](VERCON_ACCESS/queries/Balanta.sql)<br>14: [BC_Produs_BUN](VERCON_ACCESS/queries/BC_Produs_BUN.sql)<br>15: [BC_Produs_FIFO](VERCON_ACCESS/queries/BC_Produs_FIFO.sql)<br>16: [BC_Produs_S1](VERCON_ACCESS/queries/BC_Produs_S1.sql)<br>17: [BC_Produs_S2](VERCON_ACCESS/queries/BC_Produs_S2.sql)<br>73: [Retur_Produse](VERCON_ACCESS/queries/Retur_Produse.sql)<br>74: [Retur_produse_prost](VERCON_ACCESS/queries/Retur_produse_prost.sql)<br>76: [Storno](VERCON_ACCESS/queries/Storno.sql) — EXCLUS-CODURI; Nota=refera Coduri (N06); Forms!/TempVars
- 10: [Balanta22_IA](VERCON_ACCESS/queries/Balanta22_IA.sql)<br>75: [SFP_Balanta](VERCON_ACCESS/queries/SFP_Balanta.sql) — INCLUS-CALCUL; Nota=calcul actual; Forms!/TempVars
- 13: [BC_Produs](VERCON_ACCESS/queries/BC_Produs.sql) — EXCLUS-CODURI; Nota=dependenta tranzitiva de BC_Produs_S1 (Coduri); Forms!/TempVars
- 18: [Bon Consum](VERCON_ACCESS/queries/Bon%20Consum.sql)<br>21: [Copac_BC](VERCON_ACCESS/queries/Copac_BC.sql)<br>22: [Fisa de magazie antet furnizori](VERCON_ACCESS/queries/Fisa%20de%20magazie%20antet%20furnizori.sql)<br>23: [Fisa de magazie antet](VERCON_ACCESS/queries/Fisa%20de%20magazie%20antet.sql)<br>26: [FM_IDP](VERCON_ACCESS/queries/FM_IDP.sql)<br>33: [MF_Balanta](VERCON_ACCESS/queries/MF_Balanta.sql)<br>35: [OI_Balanta](VERCON_ACCESS/queries/OI_Balanta.sql)<br>36: [OI_Evidenta](VERCON_ACCESS/queries/OI_Evidenta.sql)<br>37: [OI_MaxID](VERCON_ACCESS/queries/OI_MaxID.sql)<br>40: [Per](VERCON_ACCESS/queries/Per.sql)<br>42: [qBCPT](VERCON_ACCESS/queries/qBCPT.sql)<br>47: [QEP](VERCON_ACCESS/queries/QEP.sql)<br>48: [QFC](VERCON_ACCESS/queries/QFC.sql)<br>49: [QFCDEL](VERCON_ACCESS/queries/QFCDEL.sql)<br>51: [QFCS](VERCON_ACCESS/queries/QFCS.sql)<br>55: [QFMA](VERCON_ACCESS/queries/QFMA.sql)<br>57: [QIESIRI](VERCON_ACCESS/queries/QIESIRI.sql)<br>58: [qIesiri_H](VERCON_ACCESS/queries/qIesiri_H.sql)<br>70: [QSFP_BL](VERCON_ACCESS/queries/QSFP_BL.sql)<br>71: [QSFP_FM](VERCON_ACCESS/queries/QSFP_FM.sql) — INCLUS-CALCUL; Nota=calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03)
- 72: [Query1](VERCON_ACCESS/queries/Query1.sql) — NEREFERIT-NEPORTAT; Nota=INSERT in TmpFact; nicio referinta in export; Forms!/TempVars
- 77: [temp1](VERCON_ACCESS/queries/temp1.sql) — NEREFERIT-NEPORTAT; Nota=coloane inexistente in schema curenta (Intrari.CodProdus); nicio referinta
- 78: [tempQ](VERCON_ACCESS/queries/tempQ.sql) — NEREFERIT-NEPORTAT; Nota=sursa de lista cu REȚETAR pentru ListaAlimente (Forms!ListaAlimente!mdv); nicio referinta; Forms!/TempVars
- 79: [XLS_OI_MAT](VERCON_ACCESS/queries/XLS_OI_MAT.sql) — EXCLUS-CODURI; Nota=dependenta tranzitiva de Balanta listare 2 (Coduri)
- 80: [_TEMP_Actualizeaza_BC_IDFV](VERCON_ACCESS/queries/_TEMP_Actualizeaza_BC_IDFV.sql) — NEREFERIT-NEPORTAT; Nota=UPDATE unic pe coloane inexistente (Iesiri.IdIntrare/IDF); nicio referinta

## Forms — 53

- 1: [AdaugaTabelInTBLS](VERCON_ACCESS/Forms/AdaugaTabelInTBLS.txt) — EXCLUS-SCHEMA-ACCESS; Familie=Exclus; Nota=marcaje: refera mecanism schema Access
- 2: [ALEGE_BC](VERCON_ACCESS/Forms/ALEGE_BC.txt) — INCLUS-UI; Familie=BC
- 3: [ALEGE_BC_RETUR](VERCON_ACCESS/Forms/ALEGE_BC_RETUR.txt)<br>6: [BR](VERCON_ACCESS/Forms/BR.txt) — INCLUS-UI; Familie=BR
- 4: [BC](VERCON_ACCESS/Forms/BC.txt) — INCLUS-UI; Familie=BC; Nota=Setarea FIFO legacy nu reactiveaza Coduri.
- 5: [BCP](VERCON_ACCESS/Forms/BCP.txt) — INCLUS-UI; Familie=BC; Nota=fereastra de alegere produs pentru BC (pozitionata langa Denprodus, BC.txt:2590); BC
- 7: [Clienti](VERCON_ACCESS/Forms/Clienti.txt)<br>10: [ComisieINV subform](VERCON_ACCESS/Forms/ComisieINV%20subform.txt)<br>11: [ComisieNIR subform](VERCON_ACCESS/Forms/ComisieNIR%20subform.txt)<br>31: [Masini](VERCON_ACCESS/Forms/Masini.txt)<br>32: [Masini_sub](VERCON_ACCESS/Forms/Masini_sub.txt)<br>40: [Produse](VERCON_ACCESS/Forms/Produse.txt)<br>41: [ProduseM](VERCON_ACCESS/Forms/ProduseM.txt) — INCLUS-UI; Familie=Nomenclatoare
- 8: [CLS2024](VERCON_ACCESS/Forms/CLS2024.txt) — INCLUS-UI; Familie=Nomenclatoare; Nota=depinde de decizia D08 (CPV comun)
- 9: [CN](VERCON_ACCESS/Forms/CN.txt)<br>14: [D](VERCON_ACCESS/Forms/D.txt)<br>18: [Feluri](VERCON_ACCESS/Forms/Feluri.txt)<br>26: [LABC](VERCON_ACCESS/Forms/LABC.txt)<br>27: [LEGBCLA](VERCON_ACCESS/Forms/LEGBCLA.txt)<br>28: [ListaAlimente](VERCON_ACCESS/Forms/ListaAlimente.txt)<br>33: [MD](VERCON_ACCESS/Forms/MD.txt) — INCLUS-UI; Familie=Alimente
- 12: [Config](VERCON_ACCESS/Forms/Config.txt) — INCLUS-UI; Familie=Configurare; Nota=mapare pe VC_Gestiuni/VC_Settings
- 13: [CPV2017](VERCON_ACCESS/Forms/CPV2017.txt) — INCLUS-UI; Familie=Nomenclatoare; Nota=depinde de D08
- 15: [EFACTURA](VERCON_ACCESS/Forms/EFACTURA.txt)<br>34: [Mesaje_ANAF](VERCON_ACCESS/Forms/Mesaje_ANAF.txt)<br>52: [XMLPath](VERCON_ACCESS/Forms/XMLPath.txt)<br>53: [XMLPath_sub](VERCON_ACCESS/Forms/XMLPath_sub.txt) — EXCLUS-EFACTURA; Familie=Exclus; Nota=marcaje: refera e-Factura
- 16: [FC](VERCON_ACCESS/Forms/FC.txt) — INCLUS-UI; Familie=FC; Nota=Contine cod legacy Coduri in ramuri exclusive: ignorat.; marcaje: refera Coduri
- 17: [FCS](VERCON_ACCESS/Forms/FCS.txt) — INCLUS-UI; Familie=FCS
- 19: [frmMail](VERCON_ACCESS/Forms/frmMail.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell; Nota=backup/FTP; marcaje: refera FTP
- 20: [frmPB](VERCON_ACCESS/Forms/frmPB.txt)<br>21: [frmProgressBar](VERCON_ACCESS/Forms/frmProgressBar.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell
- 22: [Furnizori](VERCON_ACCESS/Forms/Furnizori.txt) — INLOCUIT-PARTENERI-KBOT; Familie=Nomenclatoare; Nota=forma veche de furnizori; inlocuita de Parteneri KBOT
- 23: [Furnizori2022](VERCON_ACCESS/Forms/Furnizori2022.txt) — INLOCUIT-PARTENERI-KBOT; Familie=Nomenclatoare; Nota=Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real; completarea dupa CUI din ANAF nu se portează; marcaje: refera ANAF
- 24: [Gestionari](VERCON_ACCESS/Forms/Gestionari.txt) — NEREFERIT-NEPORTAT; Familie=Configurare; Nota=nedeschis din ribbon/cod; sursa „Gestionari" nu este tabel in export; ribbonul „Gestionari" deschide Clienti
- 25: [IA](VERCON_ACCESS/Forms/IA.txt) — INCLUS-UI; Familie=Inchidere an; Nota=NU se executa pe originale.
- 29: [ListaAlimente_copy](VERCON_ACCESS/Forms/ListaAlimente_copy.txt) — NEREFERIT-NEPORTAT; Familie=Alimente; Nota=copie a ListaAlimente, nereferita
- 30: [Main](VERCON_ACCESS/Forms/Main.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell; Nota=navigare Access; inlocuita de portal
- 35: [MonthPicker](VERCON_ACCESS/Forms/MonthPicker.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell; Nota=inlocuit de calendarul comun
- 36: [OI_Casare](VERCON_ACCESS/Forms/OI_Casare.txt)<br>37: [OI_Predare](VERCON_ACCESS/Forms/OI_Predare.txt)<br>38: [OI_TmpFact](VERCON_ACCESS/Forms/OI_TmpFact.txt)<br>39: [OI_Transfer](VERCON_ACCESS/Forms/OI_Transfer.txt) — INCLUS-UI; Familie=OI
- 42: [SETARI](VERCON_ACCESS/Forms/SETARI.txt) — INCLUS-UI; Familie=Configurare
- 43: [SFP](VERCON_ACCESS/Forms/SFP.txt) — INCLUS-UI; Familie=Situatii; Nota=Meniu din tmpOptiuniSFP + MODEL.
- 44: [subTreeView](VERCON_ACCESS/Forms/subTreeView.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell; Nota=componenta UI Access
- 45: [TmpFact](VERCON_ACCESS/Forms/TmpFact.txt)<br>46: [TmpFactMF](VERCON_ACCESS/Forms/TmpFactMF.txt) — INCLUS-UI; Familie=FC; Nota=marcaje: refera e-Factura
- 47: [TmpFactV](VERCON_ACCESS/Forms/TmpFactV.txt) — INCLUS-UI; Familie=FC
- 48: [tmpSFP](VERCON_ACCESS/Forms/tmpSFP.txt)<br>49: [tmp_Balanta subform](VERCON_ACCESS/Forms/tmp_Balanta%20subform.txt) — INCLUS-UI; Familie=Situatii
- 50: [UNIT](VERCON_ACCESS/Forms/UNIT.txt) — INCLUS-PARTIAL; Familie=Configurare; Nota=marcaje: refera e-Factura, refera FTP
- 51: [VaerCon](VERCON_ACCESS/Forms/VaerCon.txt) — EXCLUS-INFRA-ACCESS; Familie=Shell; Nota=inlocuit de selectorul de context; regulile de context se mapeaza

## Reports — 61

- 1: [Amortizare](VERCON_ACCESS/Reports/Amortizare.txt)<br>41: [Nota_Comanda](VERCON_ACCESS/Reports/Nota_Comanda.txt)<br>42: [nou_Furnizori](VERCON_ACCESS/Reports/nou_Furnizori.txt)<br>57: [Plan_Achizitii](VERCON_ACCESS/Reports/Plan_Achizitii.txt)<br>58: [Referat_periodic_Intrari](VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt)<br>59: [Referat_periodic_produs](VERCON_ACCESS/Reports/Referat_periodic_produs.txt)<br>60: [Situatie_Intrari](VERCON_ACCESS/Reports/Situatie_Intrari.txt) — INCLUS-RAPORT; Familie=Comenzi/referate/situatii
- 2: [Balanta_produse](VERCON_ACCESS/Reports/Balanta_produse.txt)<br>23: [Fisa_Evidenta](VERCON_ACCESS/Reports/Fisa_Evidenta.txt)<br>24: [Fisa_Magazie](VERCON_ACCESS/Reports/Fisa_Magazie.txt)<br>25: [Fisa_Magazie_BF](VERCON_ACCESS/Reports/Fisa_Magazie_BF.txt)<br>26: [Fisa_Produsului](VERCON_ACCESS/Reports/Fisa_Produsului.txt)<br>27: [FM1-u](VERCON_ACCESS/Reports/FM1-u.txt)<br>28: [FM2](VERCON_ACCESS/Reports/FM2.txt)<br>30: [LISTA_INVENTAR](VERCON_ACCESS/Reports/LISTA_INVENTAR.txt)<br>61: [Stoc_Final](VERCON_ACCESS/Reports/Stoc_Final.txt) — INCLUS-RAPORT; Familie=Stoc/fise/balante/inventar
- 3: [Balanta_produse_AFB](VERCON_ACCESS/Reports/Balanta_produse_AFB.txt) — EXCLUS-CODURI; Familie=Stoc/fise/balante/inventar; Nota=dependenta tranzitiva de Balanta (Coduri); obiect legacy exclus N06
- 4: [Balanta_Produse_BF](VERCON_ACCESS/Reports/Balanta_Produse_BF.txt) — EXCLUS-CODURI; Familie=Stoc/fise/balante/inventar; Nota=dependenta tranzitiva de Balanta listare 1 (Coduri); obiect legacy exclus N06
- 5: [Balanta_produse_furnizor](VERCON_ACCESS/Reports/Balanta_produse_furnizor.txt) — EXCLUS-CODURI; Familie=Stoc/fise/balante/inventar; Nota=dependenta tranzitiva de Balanta listare 2 (Coduri); obiect legacy exclus N06
- 6: [BC subreport 2013](VERCON_ACCESS/Reports/BC%20subreport%202013.txt)<br>7: [BonRetur](VERCON_ACCESS/Reports/BonRetur.txt)<br>9: [BUConsum](VERCON_ACCESS/Reports/BUConsum.txt)<br>10: [BUConsum_F](VERCON_ACCESS/Reports/BUConsum_F.txt) — INCLUS-RAPORT; Familie=BC/BR
- 8: [Bon_Consum](VERCON_ACCESS/Reports/Bon_Consum.txt)<br>11: [BUConsum_F1](VERCON_ACCESS/Reports/BUConsum_F1.txt) — INCLUS-RAPORT; Familie=BC/BR; Nota=marcaje: refera e-Factura
- 12: [Centralizator_BC](VERCON_ACCESS/Reports/Centralizator_BC.txt)<br>13: [Centralizator_Conturi](VERCON_ACCESS/Reports/Centralizator_Conturi.txt)<br>14: [Centralizator_Conturi_Furnizori](VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt)<br>15: [Centralizator_Furnizori](VERCON_ACCESS/Reports/Centralizator_Furnizori.txt)<br>20: [Consumuri_Produs](VERCON_ACCESS/Reports/Consumuri_Produs.txt)<br>21: [Consumuri_Unitati](VERCON_ACCESS/Reports/Consumuri_Unitati.txt) — INCLUS-RAPORT; Familie=Centralizatoare
- 16: [Centralizator_OI_Bon_BP](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt)<br>17: [Centralizator_OI_Bon_BS](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt)<br>18: [Centralizator_OI_Bon_BT](VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) — INCLUS-RAPORT; Familie=OI bonuri
- 19: [Cina subreport](VERCON_ACCESS/Reports/Cina%20subreport.txt)<br>22: [Dejun subreport](VERCON_ACCESS/Reports/Dejun%20subreport.txt)<br>37: [MicDejun subreport](VERCON_ACCESS/Reports/MicDejun%20subreport.txt) — INCLUS-RAPORT; Familie=Alimente
- 29: [ListaAlimente2015](VERCON_ACCESS/Reports/ListaAlimente2015.txt) — INCLUS-RAPORT; Familie=Alimente; Nota=marcaje: refera e-Factura, refera FTP
- 31: [MF_Balanta](VERCON_ACCESS/Reports/MF_Balanta.txt)<br>32: [MF_Balanta_Subinv](VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt)<br>36: [MF_LISTA_INVENTAR](VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) — INCLUS-RAPORT; Familie=MF
- 33: [MF_Bon_BP](VERCON_ACCESS/Reports/MF_Bon_BP.txt)<br>34: [MF_Bon_BS](VERCON_ACCESS/Reports/MF_Bon_BS.txt)<br>35: [MF_Bon_BT](VERCON_ACCESS/Reports/MF_Bon_BT.txt) — INCLUS-RAPORT; Familie=MF; Nota=marcaje: refera FTP
- 38: [NIR](VERCON_ACCESS/Reports/NIR.txt)<br>39: [NIR_SFP](VERCON_ACCESS/Reports/NIR_SFP.txt)<br>40: [NIR_Storno](VERCON_ACCESS/Reports/NIR_Storno.txt) — INCLUS-RAPORT; Familie=NIR
- 43: [OI_Balanta](VERCON_ACCESS/Reports/OI_Balanta.txt)<br>48: [OI_Fisa_Evidenta](VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt)<br>49: [OI_Fisa_Evidenta_Primitori](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori.txt)<br>50: [OI_Fisa_Evidenta_Primitori_STOC](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori_STOC.txt)<br>51: [OI_Fisa_Evidenta_Produse](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse.txt)<br>52: [OI_Fisa_Evidenta_Produse_STOC](VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse_STOC.txt)<br>53: [OI_LISTA_INVENTAR](VERCON_ACCESS/Reports/OI_LISTA_INVENTAR.txt)<br>54: [OI_LISTA_INVENTAR_TOTAL](VERCON_ACCESS/Reports/OI_LISTA_INVENTAR_TOTAL.txt) — INCLUS-RAPORT; Familie=OI evidenta/inventar
- 44: [OI_Bon_BP](VERCON_ACCESS/Reports/OI_Bon_BP.txt)<br>45: [OI_Bon_BS](VERCON_ACCESS/Reports/OI_Bon_BS.txt)<br>46: [OI_Bon_BT](VERCON_ACCESS/Reports/OI_Bon_BT.txt) — INCLUS-RAPORT; Familie=OI bonuri; Nota=marcaje: refera FTP
- 47: [OI_Casare](VERCON_ACCESS/Reports/OI_Casare.txt)<br>55: [OI_Predare](VERCON_ACCESS/Reports/OI_Predare.txt)<br>56: [OI_Transfer](VERCON_ACCESS/Reports/OI_Transfer.txt) — INCLUS-RAPORT; Familie=OI documente (formular-raport); Nota=marcaje: refera FTP

## Modules — 44

- 1: [basGDIPlus](VERCON_ACCESS/Modules/basGDIPlus.bas.txt)<br>2: [basRibbonCallbacks](VERCON_ACCESS/Modules/basRibbonCallbacks.bas.txt)<br>3: [basRibbonGlobals](VERCON_ACCESS/Modules/basRibbonGlobals.bas.txt)<br>4: [BCrypt](VERCON_ACCESS/Modules/BCrypt.bas.txt)<br>5: [BStorage](VERCON_ACCESS/Modules/BStorage.bas.txt)<br>7: [mdl_64Bits](VERCON_ACCESS/Modules/mdl_64Bits.bas.txt)<br>9: [mdl_AppendText](VERCON_ACCESS/Modules/mdl_AppendText.bas.txt)<br>10: [mdl_Autoexec](VERCON_ACCESS/Modules/mdl_Autoexec.bas.txt)<br>11: [mdl_ChangeRemovePWD](VERCON_ACCESS/Modules/mdl_ChangeRemovePWD.bas.txt)<br>12: [mdl_compact&repair](VERCON_ACCESS/Modules/mdl_compact&repair.bas.txt)<br>13: [mdl_CryptDecrypt](VERCON_ACCESS/Modules/mdl_CryptDecrypt.bas.txt)<br>14: [mdl_debugPrint](VERCON_ACCESS/Modules/mdl_debugPrint.bas.txt)<br>15: [mdl_Detect7Zip](VERCON_ACCESS/Modules/mdl_Detect7Zip.bas.txt)<br>16: [mdl_DISKRW](VERCON_ACCESS/Modules/mdl_DISKRW.bas.txt)<br>19: [mdl_ExecWait](VERCON_ACCESS/Modules/mdl_ExecWait.bas.txt)<br>20: [mdl_ExistaFunctia](VERCON_ACCESS/Modules/mdl_ExistaFunctia.bas.txt)<br>23: [mdl_FTP](VERCON_ACCESS/Modules/mdl_FTP.bas.txt)<br>25: [mdl_HTML](VERCON_ACCESS/Modules/mdl_HTML.bas.txt)<br>26: [mdl_JSON](VERCON_ACCESS/Modules/mdl_JSON.bas.txt)<br>28: [mdl_ListSeparator](VERCON_ACCESS/Modules/mdl_ListSeparator.bas.txt)<br>29: [mdl_MouseCursor](VERCON_ACCESS/Modules/mdl_MouseCursor.bas.txt)<br>30: [mdl_MouseScroll](VERCON_ACCESS/Modules/mdl_MouseScroll.bas.txt)<br>32: [mdl_Popup2022](VERCON_ACCESS/Modules/mdl_Popup2022.bas.txt)<br>33: [mdl_PositionForm](VERCON_ACCESS/Modules/mdl_PositionForm.bas.txt)<br>34: [mdl_PozitieForm](VERCON_ACCESS/Modules/mdl_PozitieForm.bas.txt)<br>35: [mdl_Retea](VERCON_ACCESS/Modules/mdl_Retea.bas.txt)<br>36: [mdl_siguranta](VERCON_ACCESS/Modules/mdl_siguranta.bas.txt)<br>37: [mdl_Timer](VERCON_ACCESS/Modules/mdl_Timer.bas.txt)<br>38: [mdl_UpdateFE](VERCON_ACCESS/Modules/mdl_UpdateFE.bas.txt)<br>40: [mdl_WindowsVersion](VERCON_ACCESS/Modules/mdl_WindowsVersion.bas.txt)<br>41: [mdl_Zip_Unzip](VERCON_ACCESS/Modules/mdl_Zip_Unzip.bas.txt)<br>42: [modStartup](VERCON_ACCESS/Modules/modStartup.bas.txt)<br>43: [newProgressBar](VERCON_ACCESS/Modules/newProgressBar.bas.txt) — EXCLUS-INFRA-ACCESS; Nota=infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web
- 6: [mdl_2024](VERCON_ACCESS/Modules/mdl_2024.bas.txt)<br>27: [mdl_LegTabele](VERCON_ACCESS/Modules/mdl_LegTabele.bas.txt) — EXCLUS-SCHEMA-ACCESS; Nota=migrare/legare schema Access
- 8: [mdl_AdaugStocFM](VERCON_ACCESS/Modules/mdl_AdaugStocFM.bas.txt)<br>21: [mdl_Export_Excel](VERCON_ACCESS/Modules/mdl_Export_Excel.bas.txt)<br>22: [mdl_FormatNumber](VERCON_ACCESS/Modules/mdl_FormatNumber.bas.txt)<br>31: [mdl_OI_Produse](VERCON_ACCESS/Modules/mdl_OI_Produse.bas.txt) — INCLUS-CALCUL; Nota=logica de business/export de analizat (N09 pentru AdaugStocFM)
- 17: [mdl_EditData](VERCON_ACCESS/Modules/mdl_EditData.bas.txt)<br>24: [mdl_Functii](VERCON_ACCESS/Modules/mdl_Functii.bas.txt) — INCLUS-PARTIAL; Nota=amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel; contine referinte Coduri
- 18: [mdl_EFactura](VERCON_ACCESS/Modules/mdl_EFactura.bas.txt) — EXCLUS-EFACTURA; Nota=e-Factura
- 39: [mdl_VerificaCF](VERCON_ACCESS/Modules/mdl_VerificaCF.bas.txt) — INLOCUIT-PARTENERI-KBOT; Nota=verificare CF/ANAF a furnizorului; Partenerii sunt ai KBOT
- 44: [Utile](VERCON_ACCESS/Modules/Utile.bas.txt) — INCLUS-PARTIAL; Nota=amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel

## Classes — 27

- 1: [clFormWindow](VERCON_ACCESS/Classes/clFormWindow.cls.txt)<br>16: [clsNode](VERCON_ACCESS/Classes/clsNode.cls.txt)<br>17: [clsPing](VERCON_ACCESS/Classes/clsPing.cls.txt)<br>18: [clsPopup](VERCON_ACCESS/Classes/clsPopup.cls.txt)<br>20: [clsRegExp](VERCON_ACCESS/Classes/clsRegExp.cls.txt)<br>21: [clsRegExp_Match](VERCON_ACCESS/Classes/clsRegExp_Match.cls.txt)<br>24: [clsTreeview](VERCON_ACCESS/Classes/clsTreeview.cls.txt)<br>27: [iTimer](VERCON_ACCESS/Classes/iTimer.cls.txt) — EXCLUS-INFRA-ACCESS; Nota=utilitar UI/Windows (regex, treeview, popup, timer, ping, form window)
- 2: [clsDatabase](VERCON_ACCESS/Classes/clsDatabase.cls.txt)<br>3: [clsDatabaseError](VERCON_ACCESS/Classes/clsDatabaseError.cls.txt)<br>4: [clsDatabaseErrors](VERCON_ACCESS/Classes/clsDatabaseErrors.cls.txt)<br>5: [clsField](VERCON_ACCESS/Classes/clsField.cls.txt)<br>15: [clsIndex](VERCON_ACCESS/Classes/clsIndex.cls.txt)<br>19: [clsQuery](VERCON_ACCESS/Classes/clsQuery.cls.txt)<br>22: [clsRelation](VERCON_ACCESS/Classes/clsRelation.cls.txt)<br>23: [clsTable](VERCON_ACCESS/Classes/clsTable.cls.txt) — EXCLUS-SCHEMA-ACCESS; Nota=DBS/clsDatabase si clasele copil
- 6: [clsFirma](VERCON_ACCESS/Classes/clsFirma.cls.txt)<br>7: [clsFirmaDG](VERCON_ACCESS/Classes/clsFirmaDG.cls.txt)<br>8: [clsFirmaInactiva](VERCON_ACCESS/Classes/clsFirmaInactiva.cls.txt)<br>9: [clsFirmaSF](VERCON_ACCESS/Classes/clsFirmaSF.cls.txt)<br>10: [clsFirmaSS](VERCON_ACCESS/Classes/clsFirmaSS.cls.txt)<br>11: [clsFirmaTVA](VERCON_ACCESS/Classes/clsFirmaTVA.cls.txt)<br>12: [clsFirmaTVAI](VERCON_ACCESS/Classes/clsFirmaTVAI.cls.txt)<br>13: [clsFirmaTVAS](VERCON_ACCESS/Classes/clsFirmaTVAS.cls.txt)<br>14: [clsFirme](VERCON_ACCESS/Classes/clsFirme.cls.txt) — INLOCUIT-PARTENERI-KBOT; Nota=client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează
- 25: [clsWinSCP](VERCON_ACCESS/Classes/clsWinSCP.cls.txt)<br>26: [clsWinScpFile](VERCON_ACCESS/Classes/clsWinScpFile.cls.txt) — EXCLUS-INFRA-ACCESS; Nota=FTP/WinSCP

## Macros — 3

- 1: [Afiseaza cod](VERCON_ACCESS/Macros/Afiseaza%20cod.txt)<br>2: [Autoexec_](VERCON_ACCESS/Macros/Autoexec_.txt)<br>3: [AutoKeys](VERCON_ACCESS/Macros/AutoKeys.txt) — EXCLUS-INFRA-ACCESS; Nota=pornire Access (Autoexec_ apeleaza Productie/AExec: nu se executa)
