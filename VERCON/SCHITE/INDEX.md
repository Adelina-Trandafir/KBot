# Index schite VERCON (SLICE-VC00-04)

Imagini PNG generate din exporturile `FORMS`/`REPORTS` de `tools/schite.js` + `tools/schite_render.ps1` (parsarea proprietatilor `Left/Top/Width/Height/Caption/ControlSource/SourceObject`). **Nu sunt functionalitate implementata** si nu sunt designul web final: dimensiunile si ordinea din export sunt repere; adaptarea web/mobil este in VC02-02 si VC08-03. Elementele nerecuperabile din export (ribbon, userform `ufTreeView`, imagini, ActiveX, valorile calculate la rulare) sunt marcate in coloana Limite.

Fiecare schita are un fisier de trasabilitate cu **fiecare control** (nume, tip, text/camp/sursa, pozitie, linia din export, evenimente). Totaluri: 101 schite (43 formulare + 58 rapoarte).

**Verificare (11.10.2026):** toate cele 101 imagini au fost privite individual; un defect de randare (controale fara `Height` explicit, desenate cu dimensiune 0) a fost gasit la `Fisa_Produsului`, corectat prin folosirea `LayoutCachedLeft/Top/Width/Height` din export, apoi imaginile au fost regenerate. Dupa corectie am reprivit individual doar Fisa_Produsului si 25 de rapoarte privite abia dupa corectie; restul (aprox. 70, privite inainte de corectie) nu au fost reprivite una cate una si sunt acoperite de o verificare automata: nicio schita nu mai are controale de dimensiune 0 in afara celor 23 de mai jos. Verificarea automata arata 23 de controale vizibile cu dimensiune 0 in export (nu se deseneaza; apar in fisierul de trasabilitate): `LABC` (3), `Consumuri_Produs`/`Consumuri_Unitati` (2+2 etichete), `Fisa_Evidenta`/`OI_Fisa_Evidenta` (2+2 dreptunghiuri), `FM1-u` (1), `NIR`/`NIR_Storno` (2+2), `nou_Furnizori` (7). Defecte ale **sursei** vazute pe schite: `OI_Fisa_Evidenta_Primitori` si `_STOC` afiseaza perioada ca `di - di` (a doua data ar trebui sa fie `dsf`); `Centralizator_OI_Bon_BS` are eticheta „Numar BP" la casare; `Plan_Achizitii` are acelasi antet ca `Situatie_Intrari`; `BR` are titlul „Bonuri de Consum" (copie din BC). Web-ul corecteaza, nu reproduce.

Cititi imaginile cu atentie la suprapuneri: in Access mai multe controale se suprapun intentionat (pagini de tab, elemente cu vizibilitate conditionata, `Visible` setat din cod); schita le deseneaza pe toate.

## Alimente

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **CN** | [F_CN.png](F_CN.png) | [Forms/CN.txt](../VERCON_ACCESS/Forms/CN.txt) · [controale](trasabilitate/F_CN.md) | Feluri, Tmp_Cina | 3 / 7 | — |
| formular **D** | [F_D.png](F_D.png) | [Forms/D.txt](../VERCON_ACCESS/Forms/D.txt) · [controale](trasabilitate/F_D.md) | Feluri, Tmp_Dejun | 3 / 7 | — |
| formular **Feluri** | [F_Feluri.png](F_Feluri.png) | [Forms/Feluri.txt](../VERCON_ACCESS/Forms/Feluri.txt) · [controale](trasabilitate/F_Feluri.md) |  | 3 / 21 | subformulare: Retetar |
| formular **LABC** | [F_LABC.png](F_LABC.png) | [Forms/LABC.txt](../VERCON_ACCESS/Forms/LABC.txt) · [controale](trasabilitate/F_LABC.md) | tmp_LABC | 3 / 13 | — |
| formular **LEGBCLA** | [F_LEGBCLA.png](F_LEGBCLA.png) | [Forms/LEGBCLA.txt](../VERCON_ACCESS/Forms/LEGBCLA.txt) · [controale](trasabilitate/F_LEGBCLA.md) | tmp_LEGBCLA | 1 / 6 | — |
| formular **ListaAlimente** | [F_ListaAlimente.png](F_ListaAlimente.png) | [Forms/ListaAlimente.txt](../VERCON_ACCESS/Forms/ListaAlimente.txt) · [controale](trasabilitate/F_ListaAlimente.md) | tmp_LEGBCLA | 5 / 68 | subformulare: md, d, cn, subTreeView, LEGBCLA |
| formular **ListaAlimente_copy** | [F_ListaAlimente_copy.png](F_ListaAlimente_copy.png) | [Forms/ListaAlimente_copy.txt](../VERCON_ACCESS/Forms/ListaAlimente_copy.txt) · [controale](trasabilitate/F_ListaAlimente_copy.md) |  | 5 / 68 | subformulare: md, d, cn, subTreeView |
| formular **MD** | [F_MD.png](F_MD.png) | [Forms/MD.txt](../VERCON_ACCESS/Forms/MD.txt) · [controale](trasabilitate/F_MD.md) | Tmp_MicDejun | 3 / 7 | — |
| raport **Cina subreport** | [R_Cina_subreport.png](R_Cina_subreport.png) | [Reports/Cina subreport.txt](../VERCON_ACCESS/Reports/Cina%20subreport.txt) · [controale](trasabilitate/R_Cina_subreport.md) | Cina | 5 / 4 | — |
| raport **Dejun subreport** | [R_Dejun_subreport.png](R_Dejun_subreport.png) | [Reports/Dejun subreport.txt](../VERCON_ACCESS/Reports/Dejun%20subreport.txt) · [controale](trasabilitate/R_Dejun_subreport.md) | Dejun | 5 / 4 | — |
| raport **ListaAlimente2015** | [R_ListaAlimente2015.png](R_ListaAlimente2015.png) | [Reports/ListaAlimente2015.txt](../VERCON_ACCESS/Reports/ListaAlimente2015.txt) · [controale](trasabilitate/R_ListaAlimente2015.md) | LABC, LegBCLA, ListaAlimente, Iesiri, Iesiri_H, Produse, ProduseP | 5 / 92 | — |
| raport **MicDejun subreport** | [R_MicDejun_subreport.png](R_MicDejun_subreport.png) | [Reports/MicDejun subreport.txt](../VERCON_ACCESS/Reports/MicDejun%20subreport.txt) · [controale](trasabilitate/R_MicDejun_subreport.md) | MicDejun | 5 / 4 | — |

## BC

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **ALEGE_BC** | [F_ALEGE_BC.png](F_ALEGE_BC.png) | [Forms/ALEGE_BC.txt](../VERCON_ACCESS/Forms/ALEGE_BC.txt) · [controale](trasabilitate/F_ALEGE_BC.md) |  | 3 / 4 | subformulare: subTreeView |
| formular **BC** | [F_BC.png](F_BC.png) | [Forms/BC.txt](../VERCON_ACCESS/Forms/BC.txt) · [controale](trasabilitate/F_BC.md) | Masini, TmpFactV | 5 / 60 | subformulare: subTreeView, TmpFactV |
| formular **BCP** | [F_BCP.png](F_BCP.png) | [Forms/BCP.txt](../VERCON_ACCESS/Forms/BCP.txt) · [controale](trasabilitate/F_BCP.md) |  | 3 / 4 | subformulare: subTreeView |

## BC/BR

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **BC subreport 2013** | [R_BC_subreport_2013.png](R_BC_subreport_2013.png) | [Reports/BC subreport 2013.txt](../VERCON_ACCESS/Reports/BC%20subreport%202013.txt) · [controale](trasabilitate/R_BC_subreport_2013.md) | Iesiri, Iesiri_H, LegBCLA, Produse, ProduseP | 3 / 7 | — |
| raport **BonRetur** | [R_BonRetur.png](R_BonRetur.png) | [Reports/BonRetur.txt](../VERCON_ACCESS/Reports/BonRetur.txt) · [controale](trasabilitate/R_BonRetur.md) | Clienti, IesiriRetur, Iesiri_H, Masini, Produse, ProduseP | 5 / 41 | — |
| raport **Bon_Consum** | [R_Bon_Consum.png](R_Bon_Consum.png) | [Reports/Bon_Consum.txt](../VERCON_ACCESS/Reports/Bon_Consum.txt) · [controale](trasabilitate/R_Bon_Consum.md) | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP | 5 / 44 | — |
| raport **BUConsum** | [R_BUConsum.png](R_BUConsum.png) | [Reports/BUConsum.txt](../VERCON_ACCESS/Reports/BUConsum.txt) · [controale](trasabilitate/R_BUConsum.md) | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP | 5 / 49 | — |
| raport **BUConsum_F** | [R_BUConsum_F.png](R_BUConsum_F.png) | [Reports/BUConsum_F.txt](../VERCON_ACCESS/Reports/BUConsum_F.txt) · [controale](trasabilitate/R_BUConsum_F.md) | Clienti, Furnizori, Iesiri, Iesiri_H, Masini | 5 / 4 | — |
| raport **BUConsum_F1** | [R_BUConsum_F1.png](R_BUConsum_F1.png) | [Reports/BUConsum_F1.txt](../VERCON_ACCESS/Reports/BUConsum_F1.txt) · [controale](trasabilitate/R_BUConsum_F1.md) | Clienti, Furnizori, Iesiri, Iesiri_H, Masini | 5 / 50 | — |

## BR

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **ALEGE_BC_RETUR** | [F_ALEGE_BC_RETUR.png](F_ALEGE_BC_RETUR.png) | [Forms/ALEGE_BC_RETUR.txt](../VERCON_ACCESS/Forms/ALEGE_BC_RETUR.txt) · [controale](trasabilitate/F_ALEGE_BC_RETUR.md) |  | 3 / 4 | subformulare: subTreeView |
| formular **BR** | [F_BR.png](F_BR.png) | [Forms/BR.txt](../VERCON_ACCESS/Forms/BR.txt) · [controale](trasabilitate/F_BR.md) | Masini, TmpFactV | 5 / 57 | subformulare: subTreeView, TmpFactV |

## Centralizatoare

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **Centralizator_BC** | [R_Centralizator_BC.png](R_Centralizator_BC.png) | [Reports/Centralizator_BC.txt](../VERCON_ACCESS/Reports/Centralizator_BC.txt) · [controale](trasabilitate/R_Centralizator_BC.md) | Clienti, Iesiri, Iesiri_H, Masini, tmpSFP | 7 / 23 | — |
| raport **Centralizator_Conturi** | [R_Centralizator_Conturi.png](R_Centralizator_Conturi.png) | [Reports/Centralizator_Conturi.txt](../VERCON_ACCESS/Reports/Centralizator_Conturi.txt) · [controale](trasabilitate/R_Centralizator_Conturi.md) | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno … | 6 / 26 | — |
| raport **Centralizator_Conturi_Furnizori** | [R_Centralizator_Conturi_Furnizori.png](R_Centralizator_Conturi_Furnizori.png) | [Reports/Centralizator_Conturi_Furnizori.txt](../VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt) · [controale](trasabilitate/R_Centralizator_Conturi_Furnizori.md) | Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP | 9 / 37 | — |
| raport **Centralizator_Furnizori** | [R_Centralizator_Furnizori.png](R_Centralizator_Furnizori.png) | [Reports/Centralizator_Furnizori.txt](../VERCON_ACCESS/Reports/Centralizator_Furnizori.txt) · [controale](trasabilitate/R_Centralizator_Furnizori.md) | Furnizori, Intrari, IntrariStorno, Masini, NIR, Produse, ProduseP, tmpSFP | 7 / 30 | — |
| raport **Consumuri_Produs** | [R_Consumuri_Produs.png](R_Consumuri_Produs.png) | [Reports/Consumuri_Produs.txt](../VERCON_ACCESS/Reports/Consumuri_Produs.txt) · [controale](trasabilitate/R_Consumuri_Produs.md) | Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP | 7 / 31 | — |
| raport **Consumuri_Unitati** | [R_Consumuri_Unitati.png](R_Consumuri_Unitati.png) | [Reports/Consumuri_Unitati.txt](../VERCON_ACCESS/Reports/Consumuri_Unitati.txt) · [controale](trasabilitate/R_Consumuri_Unitati.md) | Clienti, Iesiri, Iesiri_H, Masini, tmpSFP | 7 / 31 | — |

## Comenzi/referate/situatii

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **Amortizare** | [R_Amortizare.png](R_Amortizare.png) | [Reports/Amortizare.txt](../VERCON_ACCESS/Reports/Amortizare.txt) · [controale](trasabilitate/R_Amortizare.md) | Intrari, NIR, OI_Predare, Produse, ProduseM, ProduseP, tmpSFP | 7 / 39 | — |
| raport **Nota_Comanda** | [R_Nota_Comanda.png](R_Nota_Comanda.png) | [Reports/Nota_Comanda.txt](../VERCON_ACCESS/Reports/Nota_Comanda.txt) · [controale](trasabilitate/R_Nota_Comanda.md) | Furnizori, Intrari, NIR, Produse, ProduseP, tmpSFP | 5 / 19 | — |
| raport **nou_Furnizori** | [R_nou_Furnizori.png](R_nou_Furnizori.png) | [Reports/nou_Furnizori.txt](../VERCON_ACCESS/Reports/nou_Furnizori.txt) · [controale](trasabilitate/R_nou_Furnizori.md) | Furnizori, Intrari, NIR, tmpSFP | 8 / 29 | — |
| raport **Plan_Achizitii** | [R_Plan_Achizitii.png](R_Plan_Achizitii.png) | [Reports/Plan_Achizitii.txt](../VERCON_ACCESS/Reports/Plan_Achizitii.txt) · [controale](trasabilitate/R_Plan_Achizitii.md) | cpv, Intrari, Masini, Produse, ProduseP, tmpSFP | 7 / 21 | — |
| raport **Referat_periodic_Intrari** | [R_Referat_periodic_Intrari.png](R_Referat_periodic_Intrari.png) | [Reports/Referat_periodic_Intrari.txt](../VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt) · [controale](trasabilitate/R_Referat_periodic_Intrari.md) | Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP | 5 / 21 | — |
| raport **Referat_periodic_produs** | [R_Referat_periodic_produs.png](R_Referat_periodic_produs.png) | [Reports/Referat_periodic_produs.txt](../VERCON_ACCESS/Reports/Referat_periodic_produs.txt) · [controale](trasabilitate/R_Referat_periodic_produs.md) | Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP | 5 / 21 | — |
| raport **Situatie_Intrari** | [R_Situatie_Intrari.png](R_Situatie_Intrari.png) | [Reports/Situatie_Intrari.txt](../VERCON_ACCESS/Reports/Situatie_Intrari.txt) · [controale](trasabilitate/R_Situatie_Intrari.md) | Intrari, Masini, Produse, ProduseP, tmpSFP | 7 / 21 | — |

## Configurare

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **Config** | [F_Config.png](F_Config.png) | [Forms/Config.txt](../VERCON_ACCESS/Forms/Config.txt) · [controale](trasabilitate/F_Config.md) |  | 1 / 11 | — |
| formular **Gestionari** | [F_Gestionari.png](F_Gestionari.png) | [Forms/Gestionari.txt](../VERCON_ACCESS/Forms/Gestionari.txt) · [controale](trasabilitate/F_Gestionari.md) |  | 3 / 6 | — |
| formular **SETARI** | [F_SETARI.png](F_SETARI.png) | [Forms/SETARI.txt](../VERCON_ACCESS/Forms/SETARI.txt) · [controale](trasabilitate/F_SETARI.md) | Jud, UNIT, ComisieNIR, ComisieInventar | 1 / 9 | subformulare: UNIT, Config, ComisieNIR subform, ComisieINV subform |
| formular **UNIT** | [F_UNIT.png](F_UNIT.png) | [Forms/UNIT.txt](../VERCON_ACCESS/Forms/UNIT.txt) · [controale](trasabilitate/F_UNIT.md) | Jud, UNIT | 3 / 35 | — |

## FC

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **FC** | [F_FC.png](F_FC.png) | [Forms/FC.txt](../VERCON_ACCESS/Forms/FC.txt) · [controale](trasabilitate/F_FC.md) | Masini, TVA | 5 / 83 | subformulare: tmpFactMF, subTreeView |
| formular **TmpFact** | [F_TmpFact.png](F_TmpFact.png) | [Forms/TmpFact.txt](../VERCON_ACCESS/Forms/TmpFact.txt) · [controale](trasabilitate/F_TmpFact.md) | TmpFact | 3 / 24 | — |
| formular **TmpFactMF** | [F_TmpFactMF.png](F_TmpFactMF.png) | [Forms/TmpFactMF.txt](../VERCON_ACCESS/Forms/TmpFactMF.txt) · [controale](trasabilitate/F_TmpFactMF.md) | TmpFact | 3 / 32 | — |
| formular **TmpFactV** | [F_TmpFactV.png](F_TmpFactV.png) | [Forms/TmpFactV.txt](../VERCON_ACCESS/Forms/TmpFactV.txt) · [controale](trasabilitate/F_TmpFactV.md) | TmpFactV | 3 / 13 | — |

## FCS

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **FCS** | [F_FCS.png](F_FCS.png) | [Forms/FCS.txt](../VERCON_ACCESS/Forms/FCS.txt) · [controale](trasabilitate/F_FCS.md) | Masini, NIR, TVA | 5 / 68 | subformulare: tmpFact, subTreeView |

## Inchidere an

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **IA** | [F_IA.png](F_IA.png) | [Forms/IA.txt](../VERCON_ACCESS/Forms/IA.txt) · [controale](trasabilitate/F_IA.md) | tmp_Balanta | 3 / 18 | subformulare: tmp_Balanta subform |

## MF

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **MF_Balanta** | [R_MF_Balanta.png](R_MF_Balanta.png) | [Reports/MF_Balanta.txt](../VERCON_ACCESS/Reports/MF_Balanta.txt) · [controale](trasabilitate/R_MF_Balanta.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP | 9 / 53 | — |
| raport **MF_Balanta_Subinv** | [R_MF_Balanta_Subinv.png](R_MF_Balanta_Subinv.png) | [Reports/MF_Balanta_Subinv.txt](../VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt) · [controale](trasabilitate/R_MF_Balanta_Subinv.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP | 10 / 53 | — |
| raport **MF_Bon_BP** | [R_MF_Bon_BP.png](R_MF_Bon_BP.png) | [Reports/MF_Bon_BP.txt](../VERCON_ACCESS/Reports/MF_Bon_BP.txt) · [controale](trasabilitate/R_MF_Bon_BP.md) | Clienti, Masini, OI_Predare, Produse, ProduseP, tmpSFP | 6 / 45 | — |
| raport **MF_Bon_BS** | [R_MF_Bon_BS.png](R_MF_Bon_BS.png) | [Reports/MF_Bon_BS.txt](../VERCON_ACCESS/Reports/MF_Bon_BS.txt) · [controale](trasabilitate/R_MF_Bon_BS.md) | Clienti, Masini, OI_Casare, tmpSFP | 5 / 41 | — |
| raport **MF_Bon_BT** | [R_MF_Bon_BT.png](R_MF_Bon_BT.png) | [Reports/MF_Bon_BT.txt](../VERCON_ACCESS/Reports/MF_Bon_BT.txt) · [controale](trasabilitate/R_MF_Bon_BT.md) | Clienti, Masini, OI_Transfer, tmpSFP | 5 / 41 | — |
| raport **MF_LISTA_INVENTAR** | [R_MF_LISTA_INVENTAR.png](R_MF_LISTA_INVENTAR.png) | [Reports/MF_LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) · [controale](trasabilitate/R_MF_LISTA_INVENTAR.md) | Clienti, Masini, Produse, ProduseP, tmpSFP, OI_Casare, OI_Predare, OI_Transfer | 6 / 84 | — |

## NIR

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **NIR** | [R_NIR.png](R_NIR.png) | [Reports/NIR.txt](../VERCON_ACCESS/Reports/NIR.txt) · [controale](trasabilitate/R_NIR.md) | ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP | 9 / 86 | — |
| raport **NIR_SFP** | [R_NIR_SFP.png](R_NIR_SFP.png) | [Reports/NIR_SFP.txt](../VERCON_ACCESS/Reports/NIR_SFP.txt) · [controale](trasabilitate/R_NIR_SFP.md) | ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP | 7 / 78 | — |
| raport **NIR_Storno** | [R_NIR_Storno.png](R_NIR_Storno.png) | [Reports/NIR_Storno.txt](../VERCON_ACCESS/Reports/NIR_Storno.txt) · [controale](trasabilitate/R_NIR_Storno.md) | ComisieNIR, Furnizori, IntrariStorno, Masini, NIR, Produse, ProduseP | 9 / 82 | — |

## Nomenclatoare

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **Clienti** | [F_Clienti.png](F_Clienti.png) | [Forms/Clienti.txt](../VERCON_ACCESS/Forms/Clienti.txt) · [controale](trasabilitate/F_Clienti.md) | Clienti | 3 / 9 | — |
| formular **CLS2024** | [F_CLS2024.png](F_CLS2024.png) | [Forms/CLS2024.txt](../VERCON_ACCESS/Forms/CLS2024.txt) · [controale](trasabilitate/F_CLS2024.md) |  | 3 / 6 | subformulare: subTreeView |
| formular **ComisieINV subform** | [F_ComisieINV_subform.png](F_ComisieINV_subform.png) | [Forms/ComisieINV subform.txt](../VERCON_ACCESS/Forms/ComisieINV%20subform.txt) · [controale](trasabilitate/F_ComisieINV_subform.md) | ComisieInventar | 5 / 11 | — |
| formular **ComisieNIR subform** | [F_ComisieNIR_subform.png](F_ComisieNIR_subform.png) | [Forms/ComisieNIR subform.txt](../VERCON_ACCESS/Forms/ComisieNIR%20subform.txt) · [controale](trasabilitate/F_ComisieNIR_subform.md) | ComisieNIR | 5 / 11 | — |
| formular **CPV2017** | [F_CPV2017.png](F_CPV2017.png) | [Forms/CPV2017.txt](../VERCON_ACCESS/Forms/CPV2017.txt) · [controale](trasabilitate/F_CPV2017.md) |  | 3 / 6 | subformulare: subTreeView |
| formular **Furnizori** | [F_Furnizori.png](F_Furnizori.png) | [Forms/Furnizori.txt](../VERCON_ACCESS/Forms/Furnizori.txt) · [controale](trasabilitate/F_Furnizori.md) | Furnizori | 3 / 7 | — |
| formular **Furnizori2022** | [F_Furnizori2022.png](F_Furnizori2022.png) | [Forms/Furnizori2022.txt](../VERCON_ACCESS/Forms/Furnizori2022.txt) · [controale](trasabilitate/F_Furnizori2022.md) | Furnizori | 3 / 26 | subformulare: Furnizori |
| formular **Masini** | [F_Masini.png](F_Masini.png) | [Forms/Masini.txt](../VERCON_ACCESS/Forms/Masini.txt) · [controale](trasabilitate/F_Masini.md) | Masini | 3 / 7 | subformulare: Masini_sub |
| formular **Masini_sub** | [F_Masini_sub.png](F_Masini_sub.png) | [Forms/Masini_sub.txt](../VERCON_ACCESS/Forms/Masini_sub.txt) · [controale](trasabilitate/F_Masini_sub.md) | Masini | 3 / 4 | — |
| formular **Produse** | [F_Produse.png](F_Produse.png) | [Forms/Produse.txt](../VERCON_ACCESS/Forms/Produse.txt) · [controale](trasabilitate/F_Produse.md) |  | 3 / 23 | subformulare: subTreeView |
| formular **ProduseM** | [F_ProduseM.png](F_ProduseM.png) | [Forms/ProduseM.txt](../VERCON_ACCESS/Forms/ProduseM.txt) · [controale](trasabilitate/F_ProduseM.md) | tmpProduseM | 3 / 15 | — |

## OI

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **OI_Casare** | [F_OI_Casare.png](F_OI_Casare.png) | [Forms/OI_Casare.txt](../VERCON_ACCESS/Forms/OI_Casare.txt) · [controale](trasabilitate/F_OI_Casare.md) | Masini, OI_TmpFact | 5 / 50 | subformulare: subTreeView, OI_TmpFact |
| formular **OI_Predare** | [F_OI_Predare.png](F_OI_Predare.png) | [Forms/OI_Predare.txt](../VERCON_ACCESS/Forms/OI_Predare.txt) · [controale](trasabilitate/F_OI_Predare.md) | Masini, TmpFactV | 5 / 49 | subformulare: subTreeView, TmpFactV |
| formular **OI_TmpFact** | [F_OI_TmpFact.png](F_OI_TmpFact.png) | [Forms/OI_TmpFact.txt](../VERCON_ACCESS/Forms/OI_TmpFact.txt) · [controale](trasabilitate/F_OI_TmpFact.md) | OI_TmpFact | 3 / 15 | — |
| formular **OI_Transfer** | [F_OI_Transfer.png](F_OI_Transfer.png) | [Forms/OI_Transfer.txt](../VERCON_ACCESS/Forms/OI_Transfer.txt) · [controale](trasabilitate/F_OI_Transfer.md) |  | 5 / 51 | subformulare: subTreeView, oi_tmpfact |

## OI bonuri

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **Centralizator_OI_Bon_BP** | [R_Centralizator_OI_Bon_BP.png](R_Centralizator_OI_Bon_BP.png) | [Reports/Centralizator_OI_Bon_BP.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BP.md) | Clienti, Masini, OI_Predare, tmpSFP | 7 / 22 | — |
| raport **Centralizator_OI_Bon_BS** | [R_Centralizator_OI_Bon_BS.png](R_Centralizator_OI_Bon_BS.png) | [Reports/Centralizator_OI_Bon_BS.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BS.md) | Clienti, Masini, OI_Casare, tmpSFP | 7 / 22 | — |
| raport **Centralizator_OI_Bon_BT** | [R_Centralizator_OI_Bon_BT.png](R_Centralizator_OI_Bon_BT.png) | [Reports/Centralizator_OI_Bon_BT.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BT.md) | Clienti, Masini, OI_Transfer, tmpSFP | 7 / 25 | — |
| raport **OI_Bon_BP** | [R_OI_Bon_BP.png](R_OI_Bon_BP.png) | [Reports/OI_Bon_BP.txt](../VERCON_ACCESS/Reports/OI_Bon_BP.txt) · [controale](trasabilitate/R_OI_Bon_BP.md) | Clienti, Masini, OI_Predare, tmpSFP | 5 / 41 | — |
| raport **OI_Bon_BS** | [R_OI_Bon_BS.png](R_OI_Bon_BS.png) | [Reports/OI_Bon_BS.txt](../VERCON_ACCESS/Reports/OI_Bon_BS.txt) · [controale](trasabilitate/R_OI_Bon_BS.md) | Clienti, Masini, OI_Casare, tmpSFP | 5 / 41 | — |
| raport **OI_Bon_BT** | [R_OI_Bon_BT.png](R_OI_Bon_BT.png) | [Reports/OI_Bon_BT.txt](../VERCON_ACCESS/Reports/OI_Bon_BT.txt) · [controale](trasabilitate/R_OI_Bon_BT.md) | Clienti, Masini, OI_Transfer, tmpSFP | 5 / 41 | — |

## OI documente (formular-raport)

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **OI_Casare** | [R_OI_Casare.png](R_OI_Casare.png) | [Reports/OI_Casare.txt](../VERCON_ACCESS/Reports/OI_Casare.txt) · [controale](trasabilitate/R_OI_Casare.md) | Clienti, Masini, OI_Casare, Produse, ProduseP | 5 / 41 | — |
| raport **OI_Predare** | [R_OI_Predare.png](R_OI_Predare.png) | [Reports/OI_Predare.txt](../VERCON_ACCESS/Reports/OI_Predare.txt) · [controale](trasabilitate/R_OI_Predare.md) | Clienti, Masini, OI_Predare, Produse, ProduseP | 5 / 41 | — |
| raport **OI_Transfer** | [R_OI_Transfer.png](R_OI_Transfer.png) | [Reports/OI_Transfer.txt](../VERCON_ACCESS/Reports/OI_Transfer.txt) · [controale](trasabilitate/R_OI_Transfer.md) | Clienti, Masini, OI_Transfer, Produse, ProduseP | 5 / 41 | — |

## OI evidenta/inventar

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **OI_Balanta** | [R_OI_Balanta.png](R_OI_Balanta.png) | [Reports/OI_Balanta.txt](../VERCON_ACCESS/Reports/OI_Balanta.txt) · [controale](trasabilitate/R_OI_Balanta.md) | Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP | 7 / 45 | — |
| raport **OI_Fisa_Evidenta** | [R_OI_Fisa_Evidenta.png](R_OI_Fisa_Evidenta.png) | [Reports/OI_Fisa_Evidenta.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta.md) | Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 5 / 50 | — |
| raport **OI_Fisa_Evidenta_Primitori** | [R_OI_Fisa_Evidenta_Primitori.png](R_OI_Fisa_Evidenta_Primitori.png) | [Reports/OI_Fisa_Evidenta_Primitori.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Primitori.md) | Clienti, OI_Transfer, tmpSFP, Masini, OI_Casare, OI_Predare, Produse, ProduseP | 8 / 40 | — |
| raport **OI_Fisa_Evidenta_Primitori_STOC** | [R_OI_Fisa_Evidenta_Primitori_STOC.png](R_OI_Fisa_Evidenta_Primitori_STOC.png) | [Reports/OI_Fisa_Evidenta_Primitori_STOC.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori_STOC.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Primitori_STOC.md) | Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 4 / 25 | — |
| raport **OI_Fisa_Evidenta_Produse** | [R_OI_Fisa_Evidenta_Produse.png](R_OI_Fisa_Evidenta_Produse.png) | [Reports/OI_Fisa_Evidenta_Produse.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Produse.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 6 / 47 | — |
| raport **OI_Fisa_Evidenta_Produse_STOC** | [R_OI_Fisa_Evidenta_Produse_STOC.png](R_OI_Fisa_Evidenta_Produse_STOC.png) | [Reports/OI_Fisa_Evidenta_Produse_STOC.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse_STOC.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Produse_STOC.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 6 / 32 | — |
| raport **OI_LISTA_INVENTAR** | [R_OI_LISTA_INVENTAR.png](R_OI_LISTA_INVENTAR.png) | [Reports/OI_LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR.txt) · [controale](trasabilitate/R_OI_LISTA_INVENTAR.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 5 / 79 | — |
| raport **OI_LISTA_INVENTAR_TOTAL** | [R_OI_LISTA_INVENTAR_TOTAL.png](R_OI_LISTA_INVENTAR_TOTAL.png) | [Reports/OI_LISTA_INVENTAR_TOTAL.txt](../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR_TOTAL.txt) · [controale](trasabilitate/R_OI_LISTA_INVENTAR_TOTAL.md) | Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP | 5 / 77 | — |

## Shell

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **Main** | [F_Main.png](F_Main.png) | [Forms/Main.txt](../VERCON_ACCESS/Forms/Main.txt) · [controale](trasabilitate/F_Main.md) | TVA | 1 / 0 | fara controale in export |
| formular **VaerCon** | [F_VaerCon.png](F_VaerCon.png) | [Forms/VaerCon.txt](../VERCON_ACCESS/Forms/VaerCon.txt) · [controale](trasabilitate/F_VaerCon.md) | cale | 3 / 12 | — |

## Situatii

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| formular **SFP** | [F_SFP.png](F_SFP.png) | [Forms/SFP.txt](../VERCON_ACCESS/Forms/SFP.txt) · [controale](trasabilitate/F_SFP.md) | SFP_CautCBX | 5 / 48 | subformulare: subTreeView |
| formular **tmpSFP** | [F_tmpSFP.png](F_tmpSFP.png) | [Forms/tmpSFP.txt](../VERCON_ACCESS/Forms/tmpSFP.txt) · [controale](trasabilitate/F_tmpSFP.md) | tmpSFP | 3 / 4 | subformulare: tmpSFP_sub |
| formular **tmp_Balanta subform** | [F_tmp_Balanta_subform.png](F_tmp_Balanta_subform.png) | [Forms/tmp_Balanta subform.txt](../VERCON_ACCESS/Forms/tmp_Balanta%20subform.txt) · [controale](trasabilitate/F_tmp_Balanta_subform.md) | tmp_Balanta | 3 / 6 | — |

## Stoc/fise/balante/inventar

| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |
|---|---|---|---|---|---|
| raport **Balanta_produse** | [R_Balanta_produse.png](R_Balanta_produse.png) | [Reports/Balanta_produse.txt](../VERCON_ACCESS/Reports/Balanta_produse.txt) · [controale](trasabilitate/R_Balanta_produse.md) | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno … | 7 / 52 | — |
| raport **Fisa_Evidenta** | [R_Fisa_Evidenta.png](R_Fisa_Evidenta.png) | [Reports/Fisa_Evidenta.txt](../VERCON_ACCESS/Reports/Fisa_Evidenta.txt) · [controale](trasabilitate/R_Fisa_Evidenta.md) | Clienti, Iesiri, Iesiri_H, tmpSFP | 5 / 50 | — |
| raport **Fisa_Magazie** | [R_Fisa_Magazie.png](R_Fisa_Magazie.png) | [Reports/Fisa_Magazie.txt](../VERCON_ACCESS/Reports/Fisa_Magazie.txt) · [controale](trasabilitate/R_Fisa_Magazie.md) | Masini, Produse, ProduseP, tmpSFP | 5 / 22 | — |
| raport **Fisa_Magazie_BF** | [R_Fisa_Magazie_BF.png](R_Fisa_Magazie_BF.png) | [Reports/Fisa_Magazie_BF.txt](../VERCON_ACCESS/Reports/Fisa_Magazie_BF.txt) · [controale](trasabilitate/R_Fisa_Magazie_BF.md) | FM_2, Furnizori, Intrari, Masini, NIR | 5 / 25 | — |
| raport **Fisa_Produsului** | [R_Fisa_Produsului.png](R_Fisa_Produsului.png) | [Reports/Fisa_Produsului.txt](../VERCON_ACCESS/Reports/Fisa_Produsului.txt) · [controale](trasabilitate/R_Fisa_Produsului.md) | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno … | 3 / 26 | — |
| raport **FM1-u** | [R_FM1-u.png](R_FM1-u.png) | [Reports/FM1-u.txt](../VERCON_ACCESS/Reports/FM1-u.txt) · [controale](trasabilitate/R_FM1-u.md) | FM_2, Intrari, Masini | 5 / 34 | — |
| raport **FM2** | [R_FM2.png](R_FM2.png) | [Reports/FM2.txt](../VERCON_ACCESS/Reports/FM2.txt) · [controale](trasabilitate/R_FM2.md) | FM_2 | 6 / 22 | — |
| raport **LISTA_INVENTAR** | [R_LISTA_INVENTAR.png](R_LISTA_INVENTAR.png) | [Reports/LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/LISTA_INVENTAR.txt) · [controale](trasabilitate/R_LISTA_INVENTAR.md) | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno … | 7 / 82 | — |
| raport **Stoc_Final** | [R_Stoc_Final.png](R_Stoc_Final.png) | [Reports/Stoc_Final.txt](../VERCON_ACCESS/Reports/Stoc_Final.txt) · [controale](trasabilitate/R_Stoc_Final.md) | Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno … | 7 / 31 | — |
