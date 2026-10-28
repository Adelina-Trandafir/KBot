# VERCON — catalogul rapoartelor (SLICE-VC00-04)

Inventarul de mai jos este generat de `VERCON/tools/mapare.js` și descrie sursele Access. 61 rapoarte în export; cele care referă Coduri sunt EXCLUSE (N06) și nu se investighează.

Actualizare 13.10.2026: generarea pentru Materiale/Alimente este **IMPLEMENTAT_LOCAL, NERULAT** în `PYTHON/routes/vercon/report_*.py`. OI/MF sunt amânate la cererea utilizatorului. Catalogul activ este `report_catalog.py`; subreports alimentare sunt incluse în lista zilnică. Sursele SQL, machetele și antetul/subsolul sunt configurabile per unitate din KbotAdmin. Toate exporturile Excel folosesc modulul comun `PYTHON/reporting/excel_widths.py` și `AVACONT_COMUN.RAPOARTE_EXCEL_COLOANE`. Capturi, implementare și limite: [worklog VC08-03](../docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md). Nicio validare vizuală sau interogare reală executată în această etapă.

Coloane: **Familie** (din denumire), **Surse** (tabele citite transitiv prin interogarile din RecordSource), **Capabilitate** (MAT/OI/MF/AL: da/nu/de verificat), **Parametri din formular** (`[Forms]![...]`/`TempVars` in interogari — de inlocuit cu parametri expliciti), **Sectiuni** (inclusiv grupari), schita.

SLICE-D003-20: sunt grupate numai obiecte cu toate celelalte celule identice; numărul păstrează poziția inițială, iar fiecare schiță rămâne legată de obiectul său.

| Obiecte (#: nume · Schita) | Familie | Dispozitie | MAT | OI | MF | AL | Surse | Interogari | Parametri Forms!/TempVars |
|---|---|---|---|---|---|---|---|---|---|
| 1: Amortizare · [png](SCHITE/R_Amortizare.png) | Comenzi/referate/situatii | INCLUS-RAPORT | nu | nu | da | nu | Intrari, NIR, OI_Predare, Produse, ProduseM, ProduseP, tmpSFP | Amortizare | sfp.dsf |
| 2: Balanta_produse · [png](SCHITE/R_Balanta_produse.png)<br>30: LISTA_INVENTAR · [png](SCHITE/R_LISTA_INVENTAR.png)<br>61: Stoc_Final · [png](SCHITE/R_Stoc_Final.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno, NIR, tmpSFP | Balanta22, QSFP_BL | TempVars!zec, SFP.di, SFP.dsf, sfp.st |
| 3: Balanta_produse_AFB · — | Stoc/fise/balante/inventar | EXCLUS-CODURI (dependenta tranzitiva de Balanta (Coduri); obiect legacy exclus N06) | da | da | da | da |  |  |  |
| 4: Balanta_Produse_BF · — | Stoc/fise/balante/inventar | EXCLUS-CODURI (dependenta tranzitiva de Balanta listare 1 (Coduri); obiect legacy exclus N06) | da | da | da | da |  |  |  |
| 5: Balanta_produse_furnizor · — | Stoc/fise/balante/inventar | EXCLUS-CODURI (dependenta tranzitiva de Balanta listare 2 (Coduri); obiect legacy exclus N06) | da | da | da | da |  |  |  |
| 6: BC subreport 2013 · [png](SCHITE/R_BC_subreport_2013.png) | BC/BR | INCLUS-RAPORT | da | de verificat | de verificat | da | Iesiri, Iesiri_H, LegBCLA, Produse, ProduseP |  | ListaAlimente.IDLISTA |
| 7: BonRetur · [png](SCHITE/R_BonRetur.png) | BC/BR | INCLUS-RAPORT | da | de verificat | de verificat | da | Clienti, IesiriRetur, Iesiri_H, Masini, Produse, ProduseP |  | TempVars!zec, br.IdBonConsum |
| 8: Bon_Consum · [png](SCHITE/R_Bon_Consum.png) | BC/BR | INCLUS-RAPORT | da | nu | nu | da | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP |  | SFP.di, SFP.dsf |
| 9: BUConsum · [png](SCHITE/R_BUConsum.png) | BC/BR | INCLUS-RAPORT | da | de verificat | de verificat | da | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP |  | TempVars!zec, bc.IdBonConsum |
| 10: BUConsum_F · [png](SCHITE/R_BUConsum_F.png)<br>11: BUConsum_F1 · [png](SCHITE/R_BUConsum_F1.png) | BC/BR | INCLUS-RAPORT | da | de verificat | de verificat | da | Clienti, Furnizori, Iesiri, Iesiri_H, Masini |  | TempVars!zec, bc.IdBonConsum |
| 12: Centralizator_BC · [png](SCHITE/R_Centralizator_BC.png) | Centralizatoare | INCLUS-RAPORT | da | nu | nu | da | Clienti, Iesiri, Iesiri_H, Masini, tmpSFP | Centralizator_BC | sfp.di, sfp.dsf |
| 13: Centralizator_Conturi · [png](SCHITE/R_Centralizator_Conturi.png) | Centralizatoare | INCLUS-RAPORT | da | da | da | da | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno, NIR, tmpSFP | Balanta22, QSFP_BL | TempVars!zec, SFP.di, SFP.dsf, sfp.st |
| 14: Centralizator_Conturi_Furnizori · [png](SCHITE/R_Centralizator_Conturi_Furnizori.png) | Centralizatoare | INCLUS-RAPORT | da | da | da | da | Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP |  | sfp.di, sfp.dsf |
| 15: Centralizator_Furnizori · [png](SCHITE/R_Centralizator_Furnizori.png) | Centralizatoare | INCLUS-RAPORT | da | nu | nu | da | Furnizori, Intrari, IntrariStorno, Masini, NIR, Produse, ProduseP, tmpSFP | Listare toti furnizorii | sfp.di, sfp.dsf |
| 16: Centralizator_OI_Bon_BP · [png](SCHITE/R_Centralizator_OI_Bon_BP.png)<br>44: OI_Bon_BP · [png](SCHITE/R_OI_Bon_BP.png) | OI bonuri | INCLUS-RAPORT | nu | da | nu | nu | Clienti, Masini, OI_Predare, tmpSFP |  |  |
| 17: Centralizator_OI_Bon_BS · [png](SCHITE/R_Centralizator_OI_Bon_BS.png)<br>45: OI_Bon_BS · [png](SCHITE/R_OI_Bon_BS.png) | OI bonuri | INCLUS-RAPORT | nu | da | nu | nu | Clienti, Masini, OI_Casare, tmpSFP |  |  |
| 18: Centralizator_OI_Bon_BT · [png](SCHITE/R_Centralizator_OI_Bon_BT.png)<br>46: OI_Bon_BT · [png](SCHITE/R_OI_Bon_BT.png) | OI bonuri | INCLUS-RAPORT | nu | da | nu | nu | Clienti, Masini, OI_Transfer, tmpSFP |  |  |
| 19: Cina subreport · [png](SCHITE/R_Cina_subreport.png) | Alimente | INCLUS-RAPORT | nu | nu | nu | da | Cina |  |  |
| 20: Consumuri_Produs · [png](SCHITE/R_Consumuri_Produs.png) | Centralizatoare | INCLUS-RAPORT | da | nu | nu | da | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP |  | SFP.di, SFP.dsf |
| 21: Consumuri_Unitati · [png](SCHITE/R_Consumuri_Unitati.png) | Centralizatoare | INCLUS-RAPORT | da | nu | nu | da | Clienti, Iesiri, Iesiri_H, Masini, tmpSFP |  | SFP.di, SFP.dsf |
| 22: Dejun subreport · [png](SCHITE/R_Dejun_subreport.png) | Alimente | INCLUS-RAPORT | nu | nu | nu | da | Dejun |  |  |
| 23: Fisa_Evidenta · [png](SCHITE/R_Fisa_Evidenta.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | nu | nu | da | Clienti, Iesiri, Iesiri_H, tmpSFP |  | SFP.di, SFP.dsf |
| 24: Fisa_Magazie · [png](SCHITE/R_Fisa_Magazie.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | Masini, Produse, ProduseP, tmpSFP | QFMA |  |
| 25: Fisa_Magazie_BF · [png](SCHITE/R_Fisa_Magazie_BF.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | FM_2, Furnizori, Intrari, Masini, NIR | Fisa de magazie antet furnizori, Fisa de magazie antet |  |
| 26: Fisa_Produsului · [png](SCHITE/R_Fisa_Produsului.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno, NIR, tmpSFP | QSFP_BL |  |
| 27: FM1-u · [png](SCHITE/R_FM1-u.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | FM_2, Intrari, Masini | Fisa de magazie antet |  |
| 28: FM2 · [png](SCHITE/R_FM2.png) | Stoc/fise/balante/inventar | INCLUS-RAPORT | da | da | da | da | FM_2 |  | sfp.di, sfp.dsf |
| 29: ListaAlimente2015 · [png](SCHITE/R_ListaAlimente2015.png) | Alimente | INCLUS-RAPORT | nu | nu | nu | da | LABC, LegBCLA, ListaAlimente, Iesiri, Iesiri_H, Produse, ProduseP | QDEP, qIesiri_H | listaalimente.idlista, TempVars!zec, ListaAlimente.DataLA |
| 31: MF_Balanta · [png](SCHITE/R_MF_Balanta.png)<br>32: MF_Balanta_Subinv · [png](SCHITE/R_MF_Balanta_Subinv.png) | MF | INCLUS-RAPORT | nu | nu | da | nu | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP | MF_Balanta, MF_Union_Balanta | sfp.di, sfp.dsf |
| 33: MF_Bon_BP · [png](SCHITE/R_MF_Bon_BP.png) | MF | INCLUS-RAPORT | nu | nu | da | nu | Clienti, Masini, OI_Predare, Produse, ProduseP, tmpSFP |  |  |
| 34: MF_Bon_BS · [png](SCHITE/R_MF_Bon_BS.png) | MF | INCLUS-RAPORT | nu | nu | da | nu | Clienti, Masini, OI_Casare, tmpSFP |  |  |
| 35: MF_Bon_BT · [png](SCHITE/R_MF_Bon_BT.png) | MF | INCLUS-RAPORT | nu | nu | da | nu | Clienti, Masini, OI_Transfer, tmpSFP |  |  |
| 36: MF_LISTA_INVENTAR · [png](SCHITE/R_MF_LISTA_INVENTAR.png) | MF | INCLUS-RAPORT | nu | nu | da | nu | Clienti, Masini, Produse, ProduseP, tmpSFP, OI_Casare, OI_Predare, OI_Transfer | OI_Union | sfp.di, sfp.dsf |
| 37: MicDejun subreport · [png](SCHITE/R_MicDejun_subreport.png) | Alimente | INCLUS-RAPORT | nu | nu | nu | da | MicDejun |  |  |
| 38: NIR · [png](SCHITE/R_NIR.png) | NIR | INCLUS-RAPORT | da | da | da | da | ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP |  | FC.IdIntrare |
| 39: NIR_SFP · [png](SCHITE/R_NIR_SFP.png) | NIR | INCLUS-RAPORT | da | nu | nu | da | ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP |  |  |
| 40: NIR_Storno · [png](SCHITE/R_NIR_Storno.png) | NIR | INCLUS-RAPORT | da | da | da | da | ComisieNIR, Furnizori, IntrariStorno, Masini, NIR, Produse, ProduseP |  | TempVars!zec |
| 41: Nota_Comanda · [png](SCHITE/R_Nota_Comanda.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | nu | nu | da | Furnizori, Intrari, NIR, Produse, ProduseP, tmpSFP |  | sfp.di, sfp.dsf |
| 42: nou_Furnizori · [png](SCHITE/R_nou_Furnizori.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | da | da | da | Furnizori, Intrari, NIR, tmpSFP | Listare Furnizori | sfp.di, sfp.dsf |
| 43: OI_Balanta · [png](SCHITE/R_OI_Balanta.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP | OI_Balanta, OI_Union_Balanta | sfp.di, sfp.dsf |
| 47: OI_Casare · [png](SCHITE/R_OI_Casare.png) | OI documente (formular-raport) | INCLUS-RAPORT | nu | da | da | nu | Clienti, Masini, OI_Casare, Produse, ProduseP |  | OI_Casare.IDBC |
| 48: OI_Fisa_Evidenta · [png](SCHITE/R_OI_Fisa_Evidenta.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | OI_Evidenta, OI_Union | sfp.di, sfp.dsf |
| 49: OI_Fisa_Evidenta_Primitori · [png](SCHITE/R_OI_Fisa_Evidenta_Primitori.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Clienti, OI_Transfer, tmpSFP, Masini, OI_Casare, OI_Predare, Produse, ProduseP | OI_Evidenta, OI_Union | SFP.dsf, sfp.di, sfp.dsf |
| 50: OI_Fisa_Evidenta_Primitori_STOC · [png](SCHITE/R_OI_Fisa_Evidenta_Primitori_STOC.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | OI_Evidenta, OI_Union | SFP.dsf, sfp.di, sfp.dsf |
| 51: OI_Fisa_Evidenta_Produse · [png](SCHITE/R_OI_Fisa_Evidenta_Produse.png)<br>52: OI_Fisa_Evidenta_Produse_STOC · [png](SCHITE/R_OI_Fisa_Evidenta_Produse_STOC.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | OI_Evidenta, OI_Union | sfp.di, sfp.dsf |
| 53: OI_LISTA_INVENTAR · [png](SCHITE/R_OI_LISTA_INVENTAR.png)<br>54: OI_LISTA_INVENTAR_TOTAL · [png](SCHITE/R_OI_LISTA_INVENTAR_TOTAL.png) | OI evidenta/inventar | INCLUS-RAPORT | nu | da | nu | nu | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | OI_Union | sfp.di, sfp.dsf |
| 55: OI_Predare · [png](SCHITE/R_OI_Predare.png) | OI documente (formular-raport) | INCLUS-RAPORT | nu | da | da | nu | Clienti, Masini, OI_Predare, Produse, ProduseP |  | OI_Predare.IDBP |
| 56: OI_Transfer · [png](SCHITE/R_OI_Transfer.png) | OI documente (formular-raport) | INCLUS-RAPORT | nu | da | da | nu | Clienti, Masini, OI_Transfer, Produse, ProduseP |  | OI_Transfer.IDBt |
| 57: Plan_Achizitii · [png](SCHITE/R_Plan_Achizitii.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | da | nu | da | cpv, Intrari, Masini, Produse, ProduseP, tmpSFP |  |  |
| 58: Referat_periodic_Intrari · [png](SCHITE/R_Referat_periodic_Intrari.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | nu | nu | da | Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP |  | sfp.di, sfp.dsf |
| 59: Referat_periodic_produs · [png](SCHITE/R_Referat_periodic_produs.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | da | da | da | Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP |  | sfp.di, sfp.dsf |
| 60: Situatie_Intrari · [png](SCHITE/R_Situatie_Intrari.png) | Comenzi/referate/situatii | INCLUS-RAPORT | da | da | nu | da | Intrari, Masini, Produse, ProduseP, tmpSFP |  |  |

## Observatii

- Rapoartele **comune** (NIR*, Bon_Consum, BonRetur, fise, balante, centralizatoare, inventare, comenzi/referate) sunt oferite numai in variantele compatibile cu tipul; variantele de balanta (BF/AFB/furnizor) **nu sunt interschimbabile** si nu se presupune ca meniul curent le foloseste pe toate (VC08-03).
- Parametrii `[Forms]!SFP.di/dsf/st`, `[Forms]!FC.IdIntrare`, `[Forms]!BC.*`, `TempVars!Zec` din interogari devin parametri expliciti ai serviciilor (CONTRACT_CALCULE §3).
- Rapoartele de gestiune (OI/MF/Alimente) nu apar la alt tip. PDF/listare/export folosesc aceleasi drepturi si date ca ecranul.

## Meniul SFP — `tmpOptiuniSFP` (furnizat de utilizator, 47 randuri; [XML](REFERINTE/tmpOptiuniSFP.xml), [XSD](REFERINTE/tmpOptiuniSFP.xsd))

`TIP`: **G** = gestiune de materiale/alimente, **O** = obiecte de inventar, **M** = mijloace fixe; `G-O-M` = vizibil la toate. `Raport` = raportul deschis; `RaportA` = varianta alternativa; `XLS` = export Excel disponibil; `TMPL` = sablon Excel din `TMPLS` (**sabloanele .tmpl nu se portează**: decizie 11.10.2026, export .xlsx simplu pe fiecare raport). Randurile cu `Tot=1` sunt titluri de grup. Compararea numelor se face fara diferenta de litere (Access).

| ID | Grup | Optiune | TIP | Raport | Disp. | RaportA | Disp. | XLS | Sablon |
|---:|---|---|---|---|---|---|---|---|---|
| 0 | 1) Magazie | **PRODUSE** (grup) | G-O-M | | | | | | |
| 1 | 1) Magazie | Fișă magazie | G-O-M | Fisa_Magazie | INCLUS-RAPORT | Fisa_Magazie_BF | INCLUS-RAPORT | da | fm.tmpl |
| 2 | 1) Magazie | Balanță stocuri | G-O-M | Balanta_produse | INCLUS-RAPORT | Balanta_Produse_BF | EXCLUS-CODURI | da | Balanta.tmpl |
| 3 | 1) Magazie | Fișa produsului | G-O-M | Fisa_Produsului | INCLUS-RAPORT |  |  |  |  |
| 4 | 1) Magazie | Stoc final | G-O-M | Stoc_Final | INCLUS-RAPORT |  |  |  |  |
| 5 | 1) Magazie | Listă inventar | G-O-M | Lista_Inventar | INCLUS-RAPORT |  |  | da | ListaInventar.tmpl |
| 6 | 1) Magazie | Referat de achiziție | G-O-M | Referat_Periodic_produs | INCLUS-RAPORT |  |  | da | referat_per.tmpl |
| 7 | 1) Magazie | Centralizator conturi | G-O-M | Centralizator_Conturi | INCLUS-RAPORT |  |  | da |  |
| 8 | 1) Magazie | Centralizator conturi / furnizori | G-O-M | Centralizator_Conturi_Furnizori | INCLUS-RAPORT |  |  |  |  |
| 9 | 1) Magazie | Situație intrări anuale | G-O | Situatie_Intrari | INCLUS-RAPORT |  |  |  |  |
| 10 | 1) Magazie | Plan anual de achiziții | G-O | Plan_Achizitii | INCLUS-RAPORT |  |  | da | plan.tmpl |
| 100 | 1) Magazie | **INTRĂRI** (grup) | G | | | | | | |
| 101 | 1) Magazie | Listare N.I.R. | G | NIR_SFP | INCLUS-RAPORT |  |  |  |  |
| 102 | 1) Magazie | Fișă magazie | G | Fisa_Magazie | INCLUS-RAPORT |  |  |  |  |
| 103 | 1) Magazie | Referat achiziție | G | Referat_Periodic_Intrari | INCLUS-RAPORT |  |  | da | referat_per.tmpl |
| 104 | 1) Magazie | Notă de comandă | G | Nota_Comanda | INCLUS-RAPORT |  |  | da | nota_comanda.tmpl |
| 106 | 1) Magazie | Centralizator intrări / perioadă | G | Centralizator_Furnizori | INCLUS-RAPORT |  |  |  |  |
| 107 | 1) Magazie | Balanță produse / furnizor | G | Centralizator_Furnizori | INCLUS-RAPORT |  |  |  |  |
| 200 | 1) Magazie | **IEȘIRI** (grup) | G | | | | | | |
| 201 | 1) Magazie | Listare bon consum | G | Bon_Consum | INCLUS-RAPORT |  |  |  |  |
| 202 | 1) Magazie | Centralizator bonuri consum | G | Centralizator_BC | INCLUS-RAPORT |  |  | da |  |
| 203 | 1) Magazie | Consumuri / produs | G | Consumuri_Produs | INCLUS-RAPORT |  |  |  |  |
| 204 | 1) Magazie | Consumuri / gestionari | G | Consumuri_Unitati | INCLUS-RAPORT |  |  |  |  |
| 205 | 1) Magazie | Fișă de evidență / spații | G | Fisa_Evidenta | INCLUS-RAPORT |  |  |  |  |
| 300 | 2) Obiecte de inventar | **BONURI** (grup) | O | | | | | | |
| 301 | 2) Obiecte de inventar | Bonuri casare | O | OI_BON_BS | INCLUS-RAPORT | Centralizator_OI_BON_BS | INCLUS-RAPORT |  |  |
| 302 | 2) Obiecte de inventar | Bonuri predare | O | OI_BON_BP | INCLUS-RAPORT | Centralizator_OI_BON_BP | INCLUS-RAPORT |  |  |
| 303 | 2) Obiecte de inventar | Bonuri transfer | O | OI_BON_Bt | INCLUS-RAPORT | Centralizator_OI_BON_BT | INCLUS-RAPORT |  |  |
| 400 | 2) Obiecte de inventar | **PRODUSE** (grup) | O | | | | | | |
| 401 | 2) Obiecte de inventar | Situație produse | O | OI_Fisa_Evidenta_Produse | INCLUS-RAPORT |  |  |  |  |
| 402 | 2) Obiecte de inventar | Stoc final produse | O | OI_Fisa_Evidenta_Produse_STOC | INCLUS-RAPORT |  |  |  |  |
| 403 | 2) Obiecte de inventar | Balanță obiecte inventar | O | OI_Balanta | INCLUS-RAPORT |  |  | da |  |
| 404 | 2) Obiecte de inventar | Lista inventar | O | OI_LISTA_INVENTAR_TOTAL | INCLUS-RAPORT |  |  |  |  |
| 500 | 2) Obiecte de inventar | **PRIMITORI** (grup) | O | | | | | | |
| 501 | 2) Obiecte de inventar | Situație primitor | O | OI_Fisa_Evidenta_Primitori | INCLUS-RAPORT |  |  |  |  |
| 502 | 2) Obiecte de inventar | Stoc final primitor | O | OI_Fisa_Evidenta_Primitori_STOC | INCLUS-RAPORT |  |  |  |  |
| 503 | 2) Obiecte de inventar | Lista inventar | O | OI_Lista_Inventar | INCLUS-RAPORT |  |  |  |  |
| 504 | 2) Obiecte de inventar | Fișă de evidență / spații O.I. | O | OI_Fisa_Evidenta | INCLUS-RAPORT |  |  |  |  |
| 600 | 3) Mijloace Fixe | **BONURI** (grup) | M | | | | | | |
| 601 | 3) Mijloace Fixe | Bonuri Casare | M | MF_BON_BS | INCLUS-RAPORT | Centralizator_MF_BON_BS | LIPSA din export |  |  |
| 602 | 3) Mijloace Fixe | Bonuri Predare | M | MF_BON_BP | INCLUS-RAPORT | Centralizator_MF_BON_BP | LIPSA din export |  |  |
| 603 | 3) Mijloace Fixe | Bonuri Transfer | M | MF_BON_Bt | INCLUS-RAPORT | Centralizator_MF_BON_BT | LIPSA din export |  |  |
| 700 | 3) Mijloace Fixe | **PRODUSE** (grup) | M | | | | | | |
| 703 | 3) Mijloace Fixe | Balanță Mijloace Fixe | M | MF_Balanta | INCLUS-RAPORT | MF_Balanta_Subinv | INCLUS-RAPORT |  |  |
| 704 | 3) Mijloace Fixe | Amortizare | M | Amortizare | INCLUS-RAPORT |  |  |  |  |
| 800 | 3) Mijloace Fixe | **PRIMITORI** (grup) | M | | | | | | |
| 803 | 3) Mijloace Fixe | Listă Inventar | M | MF_Lista_Inventar | INCLUS-RAPORT |  |  |  |  |

**Constatari din meniu (dovada, comparatie automata):**

- Rapoarte din meniu care **nu exista** in export: centralizator_mf_bon_bs, centralizator_mf_bon_bp, centralizator_mf_bon_bt.
- Rapoarte din export **fara intrare in meniul SFP** (19): Balanta_produse_AFB [EXCLUS-CODURI], Balanta_produse_furnizor [EXCLUS-CODURI], BC subreport 2013 [INCLUS-RAPORT], BonRetur [INCLUS-RAPORT], BUConsum [INCLUS-RAPORT], BUConsum_F [INCLUS-RAPORT], BUConsum_F1 [INCLUS-RAPORT], Cina subreport [INCLUS-RAPORT], Dejun subreport [INCLUS-RAPORT], FM1-u [INCLUS-RAPORT], FM2 [INCLUS-RAPORT], ListaAlimente2015 [INCLUS-RAPORT], MicDejun subreport [INCLUS-RAPORT], NIR [INCLUS-RAPORT], NIR_Storno [INCLUS-RAPORT], nou_Furnizori [INCLUS-RAPORT], OI_Casare [INCLUS-RAPORT], OI_Predare [INCLUS-RAPORT], OI_Transfer [INCLUS-RAPORT]. Ele sunt deschise din alte locuri (butoane ale formularelor, subrapoarte, `IA`) sau sunt orfane; se verifica la VC08-03.
- Aceeasi optiune pe doua intrari: fisa_magazie (ID 1, 102); centralizator_furnizori (ID 106, 107). Intrarea 107 „Balanta produse / furnizor" deschide **Centralizator_Furnizori**, la fel ca 106: probabil eroare de configurare (de confirmat; **Balanta_produse_furnizor nu este referit in meniu**).
- Variantele alternative `Balanta_Produse_BF` (ID 2) si `Fisa_Magazie_BF` (ID 1) sunt `RaportA`; `Balanta_Produse_BF` depinde de Coduri (exclus), `Fisa_Magazie_BF` nu.
- Optiunile cu TIP `G-O` (Situatie intrari anuale, Plan anual de achizitii) **nu** apar la MF.
