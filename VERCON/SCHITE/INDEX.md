# Index schite VERCON (SLICE-VC00-04)

Imagini PNG generate din exporturile `FORMS`/`REPORTS` de `tools/schite.js` + `tools/schite_render.ps1` (parsarea proprietatilor `Left/Top/Width/Height/Caption/ControlSource/SourceObject`). **Nu sunt functionalitate implementata** si nu sunt designul web final: dimensiunile si ordinea din export sunt repere; adaptarea web/mobil este in VC02-02 si VC08-03. Elementele nerecuperabile din export (ribbon, userform `ufTreeView`, imagini, ActiveX, valorile calculate la rulare) sunt marcate in coloana Limite.

Fiecare schita are o fișă de trasabilitate (variantele cu structuri comune pot împărți un document) cu **fiecare control** (nume, tip, text/camp/sursa, pozitie, linia din export, evenimente). Totaluri: 101 schite (43 formulare + 58 rapoarte).

**Verificare (11.10.2026):** toate cele 101 imagini au fost privite individual; un defect de randare (controale fara `Height` explicit, desenate cu dimensiune 0) a fost gasit la `Fisa_Produsului`, corectat prin folosirea `LayoutCachedLeft/Top/Width/Height` din export, apoi imaginile au fost regenerate. Dupa corectie am reprivit individual doar Fisa_Produsului si 25 de rapoarte privite abia dupa corectie; restul (aprox. 70, privite inainte de corectie) nu au fost reprivite una cate una si sunt acoperite de o verificare automata: nicio schita nu mai are controale de dimensiune 0 in afara celor 23 de mai jos. Verificarea automata arata 23 de controale vizibile cu dimensiune 0 in export (nu se deseneaza; apar in fisierul de trasabilitate): `LABC` (3), `Consumuri_Produs`/`Consumuri_Unitati` (2+2 etichete), `Fisa_Evidenta`/`OI_Fisa_Evidenta` (2+2 dreptunghiuri), `FM1-u` (1), `NIR`/`NIR_Storno` (2+2), `nou_Furnizori` (7). Defecte ale **sursei** vazute pe schite: `OI_Fisa_Evidenta_Primitori` si `_STOC` afiseaza perioada ca `di - di` (a doua data ar trebui sa fie `dsf`); `Centralizator_OI_Bon_BS` are eticheta „Numar BP" la casare; `Plan_Achizitii` are acelasi antet ca `Situatie_Intrari`; `BR` are titlul „Bonuri de Consum" (copie din BC). Web-ul corecteaza, nu reproduce.

Cititi imaginile cu atentie la suprapuneri: in Access mai multe controale se suprapun intentionat (pagini de tab, elemente cu vizibilitate conditionata, `Visible` setat din cod); schita le deseneaza pe toate.

## Citirea fișelor consolidate <a id="fise-consolidate"></a>

SLICE-D003-09, 27.10.2026: consolidare exclusiv documentară. Fiecare variantă păstrează sursa și imaginea distincte; acestea rămân repere ale exportului, fără probe noi sau schimbări ale machetelor. RecordSource este păstrat exact, inclusiv trunchierile existente; fragmentele nu sunt SQL complet dacă se termină cu `…`.

În tabelul Secțiuni, celula este `ordine / înălțime în twips / linie-sursă`; `—` înseamnă secțiune absentă. În tabelele de controale, celula variantei este `ordine:linie`: poziția rândului în secțiunea originală și linia lui din export. `—` înseamnă control absent; textul gol rămâne gol. Ordinea numerică din fiecare coloană reconstituie exact lista originală, inclusiv locurile diferite ale controalelor; totalurile rămân în secțiunile lor.

Sunt grupate numai rândurile cu același nume, tip, text/câmp/sursă și evenimente. Tabelele cu „Pozitie (twips)” au poziția identică; în fișele cu „Pozitie comună (twips)”, celula variantei `ordine:linie / poziție` înlocuiește numai poziția comună, iar fără `/` păstrează acea poziție (extinderea D003-16…18). Secțiunea este indicată de subtitlu sau de prima coloană. Când fișa omite coloana Evenimente, declară explicit că toate celulele originale erau goale. Titlurile și ancorele originale sunt păstrate. Generatoarele și imaginile nu sunt modificate sau executate; regenerarea poate suprascrie această consolidare și recrea fișierele retrase.

Fiecare intrare de catalog: obiect — schiță — sursă/fișă de controale — secțiuni/controale; Date și Limite apar când celula originală nu era goală, respectiv `—`. Ordinea și categoriile rămân cele din export.

## Alimente

- formular **CN** — [F_CN.png](F_CN.png) — [Forms/CN.txt](../VERCON_ACCESS/Forms/CN.txt) · [controale](trasabilitate/F_CN.md#trasabilitate-schita-formular-cn) — 3 / 7; Date=Feluri, Tmp_Cina
- formular **D** — [F_D.png](F_D.png) — [Forms/D.txt](../VERCON_ACCESS/Forms/D.txt) · [controale](trasabilitate/F_CN.md#trasabilitate-schita-formular-d) — 3 / 7; Date=Feluri, Tmp_Dejun
- formular **Feluri** — [F_Feluri.png](F_Feluri.png) — [Forms/Feluri.txt](../VERCON_ACCESS/Forms/Feluri.txt) · [controale](trasabilitate/F_Feluri.md) — 3 / 21; Limite=subformulare: Retetar
- formular **LABC** — [F_LABC.png](F_LABC.png) — [Forms/LABC.txt](../VERCON_ACCESS/Forms/LABC.txt) · [controale](trasabilitate/F_LABC.md) — 3 / 13; Date=tmp_LABC
- formular **LEGBCLA** — [F_LEGBCLA.png](F_LEGBCLA.png) — [Forms/LEGBCLA.txt](../VERCON_ACCESS/Forms/LEGBCLA.txt) · [controale](trasabilitate/F_LEGBCLA.md) — 1 / 6; Date=tmp_LEGBCLA
- formular **ListaAlimente** — [F_ListaAlimente.png](F_ListaAlimente.png) — [Forms/ListaAlimente.txt](../VERCON_ACCESS/Forms/ListaAlimente.txt) · [controale](trasabilitate/F_ListaAlimente.md#trasabilitate-schita-formular-listaalimente) — 5 / 68; Date=tmp_LEGBCLA; Limite=subformulare: md, d, cn, subTreeView, LEGBCLA
- formular **ListaAlimente_copy** — [F_ListaAlimente_copy.png](F_ListaAlimente_copy.png) — [Forms/ListaAlimente_copy.txt](../VERCON_ACCESS/Forms/ListaAlimente_copy.txt) · [controale](trasabilitate/F_ListaAlimente.md#trasabilitate-schita-formular-listaalimente_copy) — 5 / 68; Limite=subformulare: md, d, cn, subTreeView
- formular **MD** — [F_MD.png](F_MD.png) — [Forms/MD.txt](../VERCON_ACCESS/Forms/MD.txt) · [controale](trasabilitate/F_CN.md#trasabilitate-schita-formular-md) — 3 / 7; Date=Tmp_MicDejun
- raport **Cina subreport** — [R_Cina_subreport.png](R_Cina_subreport.png) — [Reports/Cina subreport.txt](../VERCON_ACCESS/Reports/Cina%20subreport.txt) · [controale](trasabilitate/R_Cina_subreport.md#trasabilitate-schita-raport-cina-subreport) — 5 / 4; Date=Cina
- raport **Dejun subreport** — [R_Dejun_subreport.png](R_Dejun_subreport.png) — [Reports/Dejun subreport.txt](../VERCON_ACCESS/Reports/Dejun%20subreport.txt) · [controale](trasabilitate/R_Cina_subreport.md#trasabilitate-schita-raport-dejun-subreport) — 5 / 4; Date=Dejun
- raport **ListaAlimente2015** — [R_ListaAlimente2015.png](R_ListaAlimente2015.png) — [Reports/ListaAlimente2015.txt](../VERCON_ACCESS/Reports/ListaAlimente2015.txt) · [controale](trasabilitate/R_ListaAlimente2015.md) — 5 / 92; Date=LABC, LegBCLA, ListaAlimente, Iesiri, Iesiri_H, Produse, ProduseP
- raport **MicDejun subreport** — [R_MicDejun_subreport.png](R_MicDejun_subreport.png) — [Reports/MicDejun subreport.txt](../VERCON_ACCESS/Reports/MicDejun%20subreport.txt) · [controale](trasabilitate/R_Cina_subreport.md#trasabilitate-schita-raport-micdejun-subreport) — 5 / 4; Date=MicDejun

## BC

- formular **ALEGE_BC** — [F_ALEGE_BC.png](F_ALEGE_BC.png) — [Forms/ALEGE_BC.txt](../VERCON_ACCESS/Forms/ALEGE_BC.txt) · [controale](trasabilitate/F_ALEGE_BC.md) — 3 / 4; Limite=subformulare: subTreeView
- formular **BC** — [F_BC.png](F_BC.png) — [Forms/BC.txt](../VERCON_ACCESS/Forms/BC.txt) · [controale](trasabilitate/F_BC.md#trasabilitate-schita-formular-bc) — 5 / 60; Date=Masini, TmpFactV; Limite=subformulare: subTreeView, TmpFactV
- formular **BCP** — [F_BCP.png](F_BCP.png) — [Forms/BCP.txt](../VERCON_ACCESS/Forms/BCP.txt) · [controale](trasabilitate/F_BCP.md) — 3 / 4; Limite=subformulare: subTreeView

## BC/BR

- raport **BC subreport 2013** — [R_BC_subreport_2013.png](R_BC_subreport_2013.png) — [Reports/BC subreport 2013.txt](../VERCON_ACCESS/Reports/BC%20subreport%202013.txt) · [controale](trasabilitate/R_BC_subreport_2013.md) — 3 / 7; Date=Iesiri, Iesiri_H, LegBCLA, Produse, ProduseP
- raport **BonRetur** — [R_BonRetur.png](R_BonRetur.png) — [Reports/BonRetur.txt](../VERCON_ACCESS/Reports/BonRetur.txt) · [controale](trasabilitate/R_Bon_Consum.md#trasabilitate-schita-raport-bonretur) — 5 / 41; Date=Clienti, IesiriRetur, Iesiri_H, Masini, Produse, ProduseP
- raport **Bon_Consum** — [R_Bon_Consum.png](R_Bon_Consum.png) — [Reports/Bon_Consum.txt](../VERCON_ACCESS/Reports/Bon_Consum.txt) · [controale](trasabilitate/R_Bon_Consum.md#trasabilitate-schita-raport-bon_consum) — 5 / 44; Date=Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP
- raport **BUConsum** — [R_BUConsum.png](R_BUConsum.png) — [Reports/BUConsum.txt](../VERCON_ACCESS/Reports/BUConsum.txt) · [controale](trasabilitate/R_Bon_Consum.md#trasabilitate-schita-raport-buconsum) — 5 / 49; Date=Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP
- raport **BUConsum_F** — [R_BUConsum_F.png](R_BUConsum_F.png) — [Reports/BUConsum_F.txt](../VERCON_ACCESS/Reports/BUConsum_F.txt) · [controale](trasabilitate/R_Bon_Consum.md#trasabilitate-schita-raport-buconsum_f) — 5 / 4; Date=Clienti, Furnizori, Iesiri, Iesiri_H, Masini
- raport **BUConsum_F1** — [R_BUConsum_F1.png](R_BUConsum_F1.png) — [Reports/BUConsum_F1.txt](../VERCON_ACCESS/Reports/BUConsum_F1.txt) · [controale](trasabilitate/R_Bon_Consum.md#trasabilitate-schita-raport-buconsum_f1) — 5 / 50; Date=Clienti, Furnizori, Iesiri, Iesiri_H, Masini

## BR

- formular **ALEGE_BC_RETUR** — [F_ALEGE_BC_RETUR.png](F_ALEGE_BC_RETUR.png) — [Forms/ALEGE_BC_RETUR.txt](../VERCON_ACCESS/Forms/ALEGE_BC_RETUR.txt) · [controale](trasabilitate/F_ALEGE_BC_RETUR.md) — 3 / 4; Limite=subformulare: subTreeView
- formular **BR** — [F_BR.png](F_BR.png) — [Forms/BR.txt](../VERCON_ACCESS/Forms/BR.txt) · [controale](trasabilitate/F_BC.md#trasabilitate-schita-formular-br) — 5 / 57; Date=Masini, TmpFactV; Limite=subformulare: subTreeView, TmpFactV

## Centralizatoare

- raport **Centralizator_BC** — [R_Centralizator_BC.png](R_Centralizator_BC.png) — [Reports/Centralizator_BC.txt](../VERCON_ACCESS/Reports/Centralizator_BC.txt) · [controale](trasabilitate/R_Centralizator_BC.md) — 7 / 23; Date=Clienti, Iesiri, Iesiri_H, Masini, tmpSFP
- raport **Centralizator_Conturi** — [R_Centralizator_Conturi.png](R_Centralizator_Conturi.png) — [Reports/Centralizator_Conturi.txt](../VERCON_ACCESS/Reports/Centralizator_Conturi.txt) · [controale](trasabilitate/R_Centralizator_Conturi.md) — 6 / 26; Date=Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno …
- raport **Centralizator_Conturi_Furnizori** — [R_Centralizator_Conturi_Furnizori.png](R_Centralizator_Conturi_Furnizori.png) — [Reports/Centralizator_Conturi_Furnizori.txt](../VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt) · [controale](trasabilitate/R_Centralizator_Conturi_Furnizori.md#trasabilitate-schita-raport-centralizator_conturi_furnizori) — 9 / 37; Date=Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP
- raport **Centralizator_Furnizori** — [R_Centralizator_Furnizori.png](R_Centralizator_Furnizori.png) — [Reports/Centralizator_Furnizori.txt](../VERCON_ACCESS/Reports/Centralizator_Furnizori.txt) · [controale](trasabilitate/R_Centralizator_Conturi_Furnizori.md#trasabilitate-schita-raport-centralizator_furnizori) — 7 / 30; Date=Furnizori, Intrari, IntrariStorno, Masini, NIR, Produse, ProduseP, tmpSFP
- raport **Consumuri_Produs** — [R_Consumuri_Produs.png](R_Consumuri_Produs.png) — [Reports/Consumuri_Produs.txt](../VERCON_ACCESS/Reports/Consumuri_Produs.txt) · [controale](trasabilitate/R_Consumuri_Produs.md) — 7 / 31; Date=Clienti, Iesiri, Iesiri_H, Masini, Produse, ProduseP, tmpSFP
- raport **Consumuri_Unitati** — [R_Consumuri_Unitati.png](R_Consumuri_Unitati.png) — [Reports/Consumuri_Unitati.txt](../VERCON_ACCESS/Reports/Consumuri_Unitati.txt) · [controale](trasabilitate/R_Consumuri_Produs.md#trasabilitate-schita-raport-consumuri_unitati) — 7 / 31; Date=Clienti, Iesiri, Iesiri_H, Masini, tmpSFP

## Comenzi/referate/situatii

- raport **Amortizare** — [R_Amortizare.png](R_Amortizare.png) — [Reports/Amortizare.txt](../VERCON_ACCESS/Reports/Amortizare.txt) · [controale](trasabilitate/R_Amortizare.md) — 7 / 39; Date=Intrari, NIR, OI_Predare, Produse, ProduseM, ProduseP, tmpSFP
- raport **Nota_Comanda** — [R_Nota_Comanda.png](R_Nota_Comanda.png) — [Reports/Nota_Comanda.txt](../VERCON_ACCESS/Reports/Nota_Comanda.txt) · [controale](trasabilitate/R_Nota_Comanda.md) — 5 / 19; Date=Furnizori, Intrari, NIR, Produse, ProduseP, tmpSFP
- raport **nou_Furnizori** — [R_nou_Furnizori.png](R_nou_Furnizori.png) — [Reports/nou_Furnizori.txt](../VERCON_ACCESS/Reports/nou_Furnizori.txt) · [controale](trasabilitate/R_nou_Furnizori.md) — 8 / 29; Date=Furnizori, Intrari, NIR, tmpSFP
- raport **Plan_Achizitii** — [R_Plan_Achizitii.png](R_Plan_Achizitii.png) — [Reports/Plan_Achizitii.txt](../VERCON_ACCESS/Reports/Plan_Achizitii.txt) · [controale](trasabilitate/R_Plan_Achizitii.md) — 7 / 21; Date=cpv, Intrari, Masini, Produse, ProduseP, tmpSFP
- raport **Referat_periodic_Intrari** — [R_Referat_periodic_Intrari.png](R_Referat_periodic_Intrari.png) — [Reports/Referat_periodic_Intrari.txt](../VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt) · [controale](trasabilitate/R_Plan_Achizitii.md#trasabilitate-schita-raport-referat_periodic_intrari) — 5 / 21; Date=Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP
- raport **Referat_periodic_produs** — [R_Referat_periodic_produs.png](R_Referat_periodic_produs.png) — [Reports/Referat_periodic_produs.txt](../VERCON_ACCESS/Reports/Referat_periodic_produs.txt) · [controale](trasabilitate/R_Plan_Achizitii.md#trasabilitate-schita-raport-referat_periodic_produs) — 5 / 21; Date=Furnizori, Intrari, IntrariStorno, NIR, Produse, ProduseP, tmpSFP
- raport **Situatie_Intrari** — [R_Situatie_Intrari.png](R_Situatie_Intrari.png) — [Reports/Situatie_Intrari.txt](../VERCON_ACCESS/Reports/Situatie_Intrari.txt) · [controale](trasabilitate/R_Plan_Achizitii.md#trasabilitate-schita-raport-situatie_intrari) — 7 / 21; Date=Intrari, Masini, Produse, ProduseP, tmpSFP

## Configurare

- formular **Config** — [F_Config.png](F_Config.png) — [Forms/Config.txt](../VERCON_ACCESS/Forms/Config.txt) · [controale](trasabilitate/F_Config.md) — 1 / 11
- formular **Gestionari** — [F_Gestionari.png](F_Gestionari.png) — [Forms/Gestionari.txt](../VERCON_ACCESS/Forms/Gestionari.txt) · [controale](trasabilitate/F_Gestionari.md) — 3 / 6
- formular **SETARI** — [F_SETARI.png](F_SETARI.png) — [Forms/SETARI.txt](../VERCON_ACCESS/Forms/SETARI.txt) · [controale](trasabilitate/F_SETARI.md) — 1 / 9; Date=Jud, UNIT, ComisieNIR, ComisieInventar; Limite=subformulare: UNIT, Config, ComisieNIR subform, ComisieINV subform
- formular **UNIT** — [F_UNIT.png](F_UNIT.png) — [Forms/UNIT.txt](../VERCON_ACCESS/Forms/UNIT.txt) · [controale](trasabilitate/F_UNIT.md) — 3 / 35; Date=Jud, UNIT

## FC

- formular **FC** — [F_FC.png](F_FC.png) — [Forms/FC.txt](../VERCON_ACCESS/Forms/FC.txt) · [controale](trasabilitate/F_FC.md) — 5 / 83; Date=Masini, TVA; Limite=subformulare: tmpFactMF, subTreeView
- formular **TmpFact** — [F_TmpFact.png](F_TmpFact.png) — [Forms/TmpFact.txt](../VERCON_ACCESS/Forms/TmpFact.txt) · [controale](trasabilitate/F_TmpFact.md#trasabilitate-schita-formular-tmpfact) — 3 / 24; Date=TmpFact
- formular **TmpFactMF** — [F_TmpFactMF.png](F_TmpFactMF.png) — [Forms/TmpFactMF.txt](../VERCON_ACCESS/Forms/TmpFactMF.txt) · [controale](trasabilitate/F_TmpFact.md#trasabilitate-schita-formular-tmpfactmf) — 3 / 32; Date=TmpFact
- formular **TmpFactV** — [F_TmpFactV.png](F_TmpFactV.png) — [Forms/TmpFactV.txt](../VERCON_ACCESS/Forms/TmpFactV.txt) · [controale](trasabilitate/F_TmpFact.md#trasabilitate-schita-formular-tmpfactv) — 3 / 13; Date=TmpFactV

## FCS

- formular **FCS** — [F_FCS.png](F_FCS.png) — [Forms/FCS.txt](../VERCON_ACCESS/Forms/FCS.txt) · [controale](trasabilitate/F_FCS.md) — 5 / 68; Date=Masini, NIR, TVA; Limite=subformulare: tmpFact, subTreeView

## Inchidere an

- formular **IA** — [F_IA.png](F_IA.png) — [Forms/IA.txt](../VERCON_ACCESS/Forms/IA.txt) · [controale](trasabilitate/F_IA.md) — 3 / 18; Date=tmp_Balanta; Limite=subformulare: tmp_Balanta subform

## MF

- raport **MF_Balanta** — [R_MF_Balanta.png](R_MF_Balanta.png) — [Reports/MF_Balanta.txt](../VERCON_ACCESS/Reports/MF_Balanta.txt) · [controale](trasabilitate/R_MF_Balanta.md) — 9 / 53; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP
- raport **MF_Balanta_Subinv** — [R_MF_Balanta_Subinv.png](R_MF_Balanta_Subinv.png) — [Reports/MF_Balanta_Subinv.txt](../VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt) · [controale](trasabilitate/R_MF_Balanta.md#trasabilitate-schita-raport-mf_balanta_subinv) — 10 / 53; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP
- raport **MF_Bon_BP** — [R_MF_Bon_BP.png](R_MF_Bon_BP.png) — [Reports/MF_Bon_BP.txt](../VERCON_ACCESS/Reports/MF_Bon_BP.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-mf_bon_bp) — 6 / 45; Date=Clienti, Masini, OI_Predare, Produse, ProduseP, tmpSFP
- raport **MF_Bon_BS** — [R_MF_Bon_BS.png](R_MF_Bon_BS.png) — [Reports/MF_Bon_BS.txt](../VERCON_ACCESS/Reports/MF_Bon_BS.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-mf_bon_bs) — 5 / 41; Date=Clienti, Masini, OI_Casare, tmpSFP
- raport **MF_Bon_BT** — [R_MF_Bon_BT.png](R_MF_Bon_BT.png) — [Reports/MF_Bon_BT.txt](../VERCON_ACCESS/Reports/MF_Bon_BT.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-mf_bon_bt) — 5 / 41; Date=Clienti, Masini, OI_Transfer, tmpSFP
- raport **MF_LISTA_INVENTAR** — [R_MF_LISTA_INVENTAR.png](R_MF_LISTA_INVENTAR.png) — [Reports/MF_LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) · [controale](trasabilitate/R_MF_LISTA_INVENTAR.md) — 6 / 84; Date=Clienti, Masini, Produse, ProduseP, tmpSFP, OI_Casare, OI_Predare, OI_Transfer

## NIR

- raport **NIR** — [R_NIR.png](R_NIR.png) — [Reports/NIR.txt](../VERCON_ACCESS/Reports/NIR.txt) · [controale](trasabilitate/R_NIR.md) — 9 / 86; Date=ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP
- raport **NIR_SFP** — [R_NIR_SFP.png](R_NIR_SFP.png) — [Reports/NIR_SFP.txt](../VERCON_ACCESS/Reports/NIR_SFP.txt) · [controale](trasabilitate/R_NIR.md#trasabilitate-schita-raport-nir_sfp) — 7 / 78; Date=ComisieNIR, Furnizori, Intrari, Masini, NIR, Produse, ProduseP, tmpSFP
- raport **NIR_Storno** — [R_NIR_Storno.png](R_NIR_Storno.png) — [Reports/NIR_Storno.txt](../VERCON_ACCESS/Reports/NIR_Storno.txt) · [controale](trasabilitate/R_NIR.md#trasabilitate-schita-raport-nir_storno) — 9 / 82; Date=ComisieNIR, Furnizori, IntrariStorno, Masini, NIR, Produse, ProduseP

## Nomenclatoare

- formular **Clienti** — [F_Clienti.png](F_Clienti.png) — [Forms/Clienti.txt](../VERCON_ACCESS/Forms/Clienti.txt) · [controale](trasabilitate/F_Clienti.md) — 3 / 9; Date=Clienti
- formular **CLS2024** — [F_CLS2024.png](F_CLS2024.png) — [Forms/CLS2024.txt](../VERCON_ACCESS/Forms/CLS2024.txt) · [controale](trasabilitate/F_CLS2024.md) — 3 / 6; Limite=subformulare: subTreeView
- formular **ComisieINV subform** — [F_ComisieINV_subform.png](F_ComisieINV_subform.png) — [Forms/ComisieINV subform.txt](../VERCON_ACCESS/Forms/ComisieINV%20subform.txt) · [controale](trasabilitate/F_ComisieINV_subform.md#trasabilitate-schita-formular-comisieinv-subform) — 5 / 11; Date=ComisieInventar
- formular **ComisieNIR subform** — [F_ComisieNIR_subform.png](F_ComisieNIR_subform.png) — [Forms/ComisieNIR subform.txt](../VERCON_ACCESS/Forms/ComisieNIR%20subform.txt) · [controale](trasabilitate/F_ComisieINV_subform.md#trasabilitate-schita-formular-comisienir-subform) — 5 / 11; Date=ComisieNIR
- formular **CPV2017** — [F_CPV2017.png](F_CPV2017.png) — [Forms/CPV2017.txt](../VERCON_ACCESS/Forms/CPV2017.txt) · [controale](trasabilitate/F_CPV2017.md) — 3 / 6; Limite=subformulare: subTreeView
- formular **Furnizori** — [F_Furnizori.png](F_Furnizori.png) — [Forms/Furnizori.txt](../VERCON_ACCESS/Forms/Furnizori.txt) · [controale](trasabilitate/F_Furnizori.md) — 3 / 7; Date=Furnizori
- formular **Furnizori2022** — [F_Furnizori2022.png](F_Furnizori2022.png) — [Forms/Furnizori2022.txt](../VERCON_ACCESS/Forms/Furnizori2022.txt) · [controale](trasabilitate/F_Furnizori2022.md) — 3 / 26; Date=Furnizori; Limite=subformulare: Furnizori
- formular **Masini** — [F_Masini.png](F_Masini.png) — [Forms/Masini.txt](../VERCON_ACCESS/Forms/Masini.txt) · [controale](trasabilitate/F_Masini.md) — 3 / 7; Date=Masini; Limite=subformulare: Masini_sub
- formular **Masini_sub** — [F_Masini_sub.png](F_Masini_sub.png) — [Forms/Masini_sub.txt](../VERCON_ACCESS/Forms/Masini_sub.txt) · [controale](trasabilitate/F_Masini_sub.md) — 3 / 4; Date=Masini
- formular **Produse** — [F_Produse.png](F_Produse.png) — [Forms/Produse.txt](../VERCON_ACCESS/Forms/Produse.txt) · [controale](trasabilitate/F_Produse.md) — 3 / 23; Limite=subformulare: subTreeView
- formular **ProduseM** — [F_ProduseM.png](F_ProduseM.png) — [Forms/ProduseM.txt](../VERCON_ACCESS/Forms/ProduseM.txt) · [controale](trasabilitate/F_ProduseM.md) — 3 / 15; Date=tmpProduseM

## OI

- formular **OI_Casare** — [F_OI_Casare.png](F_OI_Casare.png) — [Forms/OI_Casare.txt](../VERCON_ACCESS/Forms/OI_Casare.txt) · [controale](trasabilitate/F_OI_Predare.md#trasabilitate-schita-formular-oi_casare) — 5 / 50; Date=Masini, OI_TmpFact; Limite=subformulare: subTreeView, OI_TmpFact
- formular **OI_Predare** — [F_OI_Predare.png](F_OI_Predare.png) — [Forms/OI_Predare.txt](../VERCON_ACCESS/Forms/OI_Predare.txt) · [controale](trasabilitate/F_OI_Predare.md#trasabilitate-schita-formular-oi_predare) — 5 / 49; Date=Masini, TmpFactV; Limite=subformulare: subTreeView, TmpFactV
- formular **OI_TmpFact** — [F_OI_TmpFact.png](F_OI_TmpFact.png) — [Forms/OI_TmpFact.txt](../VERCON_ACCESS/Forms/OI_TmpFact.txt) · [controale](trasabilitate/F_OI_TmpFact.md) — 3 / 15; Date=OI_TmpFact
- formular **OI_Transfer** — [F_OI_Transfer.png](F_OI_Transfer.png) — [Forms/OI_Transfer.txt](../VERCON_ACCESS/Forms/OI_Transfer.txt) · [controale](trasabilitate/F_OI_Predare.md#trasabilitate-schita-formular-oi_transfer) — 5 / 51; Limite=subformulare: subTreeView, oi_tmpfact

## OI bonuri

- raport **Centralizator_OI_Bon_BP** — [R_Centralizator_OI_Bon_BP.png](R_Centralizator_OI_Bon_BP.png) — [Reports/Centralizator_OI_Bon_BP.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BP.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BP.md) — 7 / 22; Date=Clienti, Masini, OI_Predare, tmpSFP
- raport **Centralizator_OI_Bon_BS** — [R_Centralizator_OI_Bon_BS.png](R_Centralizator_OI_Bon_BS.png) — [Reports/Centralizator_OI_Bon_BS.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BS.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BP.md#trasabilitate-schita-raport-centralizator_oi_bon_bs) — 7 / 22; Date=Clienti, Masini, OI_Casare, tmpSFP
- raport **Centralizator_OI_Bon_BT** — [R_Centralizator_OI_Bon_BT.png](R_Centralizator_OI_Bon_BT.png) — [Reports/Centralizator_OI_Bon_BT.txt](../VERCON_ACCESS/Reports/Centralizator_OI_Bon_BT.txt) · [controale](trasabilitate/R_Centralizator_OI_Bon_BP.md#trasabilitate-schita-raport-centralizator_oi_bon_bt) — 7 / 25; Date=Clienti, Masini, OI_Transfer, tmpSFP
- raport **OI_Bon_BP** — [R_OI_Bon_BP.png](R_OI_Bon_BP.png) — [Reports/OI_Bon_BP.txt](../VERCON_ACCESS/Reports/OI_Bon_BP.txt) · [controale](trasabilitate/R_OI_Bon_BP.md) — 5 / 41; Date=Clienti, Masini, OI_Predare, tmpSFP
- raport **OI_Bon_BS** — [R_OI_Bon_BS.png](R_OI_Bon_BS.png) — [Reports/OI_Bon_BS.txt](../VERCON_ACCESS/Reports/OI_Bon_BS.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_bon_bs) — 5 / 41; Date=Clienti, Masini, OI_Casare, tmpSFP
- raport **OI_Bon_BT** — [R_OI_Bon_BT.png](R_OI_Bon_BT.png) — [Reports/OI_Bon_BT.txt](../VERCON_ACCESS/Reports/OI_Bon_BT.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_bon_bt) — 5 / 41; Date=Clienti, Masini, OI_Transfer, tmpSFP

## OI documente (formular-raport)

- raport **OI_Casare** — [R_OI_Casare.png](R_OI_Casare.png) — [Reports/OI_Casare.txt](../VERCON_ACCESS/Reports/OI_Casare.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_casare) — 5 / 41; Date=Clienti, Masini, OI_Casare, Produse, ProduseP
- raport **OI_Predare** — [R_OI_Predare.png](R_OI_Predare.png) — [Reports/OI_Predare.txt](../VERCON_ACCESS/Reports/OI_Predare.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_predare) — 5 / 41; Date=Clienti, Masini, OI_Predare, Produse, ProduseP
- raport **OI_Transfer** — [R_OI_Transfer.png](R_OI_Transfer.png) — [Reports/OI_Transfer.txt](../VERCON_ACCESS/Reports/OI_Transfer.txt) · [controale](trasabilitate/R_OI_Bon_BP.md#trasabilitate-schita-raport-oi_transfer) — 5 / 41; Date=Clienti, Masini, OI_Transfer, Produse, ProduseP

## OI evidenta/inventar

- raport **OI_Balanta** — [R_OI_Balanta.png](R_OI_Balanta.png) — [Reports/OI_Balanta.txt](../VERCON_ACCESS/Reports/OI_Balanta.txt) · [controale](trasabilitate/R_MF_Balanta.md#trasabilitate-schita-raport-oi_balanta) — 7 / 45; Date=Masini, tmpSFP, OI_Casare, OI_Predare, Produse, ProduseP
- raport **OI_Fisa_Evidenta** — [R_OI_Fisa_Evidenta.png](R_OI_Fisa_Evidenta.png) — [Reports/OI_Fisa_Evidenta.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta.txt) · [controale](trasabilitate/R_Fisa_Evidenta.md#trasabilitate-schita-raport-oi_fisa_evidenta) — 5 / 50; Date=Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP
- raport **OI_Fisa_Evidenta_Primitori** — [R_OI_Fisa_Evidenta_Primitori.png](R_OI_Fisa_Evidenta_Primitori.png) — [Reports/OI_Fisa_Evidenta_Primitori.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Primitori.md) — 8 / 40; Date=Clienti, OI_Transfer, tmpSFP, Masini, OI_Casare, OI_Predare, Produse, ProduseP
- raport **OI_Fisa_Evidenta_Primitori_STOC** — [R_OI_Fisa_Evidenta_Primitori_STOC.png](R_OI_Fisa_Evidenta_Primitori_STOC.png) — [Reports/OI_Fisa_Evidenta_Primitori_STOC.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Primitori_STOC.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Primitori_STOC.md) — 4 / 25; Date=Clienti, tmpSFP, Masini, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP
- raport **OI_Fisa_Evidenta_Produse** — [R_OI_Fisa_Evidenta_Produse.png](R_OI_Fisa_Evidenta_Produse.png) — [Reports/OI_Fisa_Evidenta_Produse.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Produse.md) — 6 / 47; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP
- raport **OI_Fisa_Evidenta_Produse_STOC** — [R_OI_Fisa_Evidenta_Produse_STOC.png](R_OI_Fisa_Evidenta_Produse_STOC.png) — [Reports/OI_Fisa_Evidenta_Produse_STOC.txt](../VERCON_ACCESS/Reports/OI_Fisa_Evidenta_Produse_STOC.txt) · [controale](trasabilitate/R_OI_Fisa_Evidenta_Produse_STOC.md) — 6 / 32; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP
- raport **OI_LISTA_INVENTAR** — [R_OI_LISTA_INVENTAR.png](R_OI_LISTA_INVENTAR.png) — [Reports/OI_LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR.txt) · [controale](trasabilitate/R_MF_LISTA_INVENTAR.md#trasabilitate-schita-raport-oi_lista_inventar) — 5 / 79; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP
- raport **OI_LISTA_INVENTAR_TOTAL** — [R_OI_LISTA_INVENTAR_TOTAL.png](R_OI_LISTA_INVENTAR_TOTAL.png) — [Reports/OI_LISTA_INVENTAR_TOTAL.txt](../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR_TOTAL.txt) · [controale](trasabilitate/R_MF_LISTA_INVENTAR.md#trasabilitate-schita-raport-oi_lista_inventar_total) — 5 / 77; Date=Clienti, Masini, tmpSFP, OI_Casare, OI_Predare, OI_Transfer, Produse, ProduseP

## Shell

- formular **Main** — [F_Main.png](F_Main.png) — [Forms/Main.txt](../VERCON_ACCESS/Forms/Main.txt) · [controale](trasabilitate/F_Main.md) — 1 / 0; Date=TVA; Limite=fara controale in export
- formular **VaerCon** — [F_VaerCon.png](F_VaerCon.png) — [Forms/VaerCon.txt](../VERCON_ACCESS/Forms/VaerCon.txt) · [controale](trasabilitate/F_VaerCon.md) — 3 / 12; Date=cale

## Situatii

- formular **SFP** — [F_SFP.png](F_SFP.png) — [Forms/SFP.txt](../VERCON_ACCESS/Forms/SFP.txt) · [controale](trasabilitate/F_SFP.md) — 5 / 48; Date=SFP_CautCBX; Limite=subformulare: subTreeView
- formular **tmpSFP** — [F_tmpSFP.png](F_tmpSFP.png) — [Forms/tmpSFP.txt](../VERCON_ACCESS/Forms/tmpSFP.txt) · [controale](trasabilitate/F_tmpSFP.md) — 3 / 4; Date=tmpSFP; Limite=subformulare: tmpSFP_sub
- formular **tmp_Balanta subform** — [F_tmp_Balanta_subform.png](F_tmp_Balanta_subform.png) — [Forms/tmp_Balanta subform.txt](../VERCON_ACCESS/Forms/tmp_Balanta%20subform.txt) · [controale](trasabilitate/F_tmp_Balanta_subform.md) — 3 / 6; Date=tmp_Balanta

## Stoc/fise/balante/inventar

- raport **Balanta_produse** — [R_Balanta_produse.png](R_Balanta_produse.png) — [Reports/Balanta_produse.txt](../VERCON_ACCESS/Reports/Balanta_produse.txt) · [controale](trasabilitate/R_MF_Balanta.md#trasabilitate-schita-raport-balanta_produse) — 7 / 52; Date=Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno …
- raport **Fisa_Evidenta** — [R_Fisa_Evidenta.png](R_Fisa_Evidenta.png) — [Reports/Fisa_Evidenta.txt](../VERCON_ACCESS/Reports/Fisa_Evidenta.txt) · [controale](trasabilitate/R_Fisa_Evidenta.md#trasabilitate-schita-raport-fisa_evidenta) — 5 / 50; Date=Clienti, Iesiri, Iesiri_H, tmpSFP
- raport **Fisa_Magazie** — [R_Fisa_Magazie.png](R_Fisa_Magazie.png) — [Reports/Fisa_Magazie.txt](../VERCON_ACCESS/Reports/Fisa_Magazie.txt) · [controale](trasabilitate/R_Fisa_Magazie.md) — 5 / 22; Date=Masini, Produse, ProduseP, tmpSFP
- raport **Fisa_Magazie_BF** — [R_Fisa_Magazie_BF.png](R_Fisa_Magazie_BF.png) — [Reports/Fisa_Magazie_BF.txt](../VERCON_ACCESS/Reports/Fisa_Magazie_BF.txt) · [controale](trasabilitate/R_Fisa_Magazie_BF.md) — 5 / 25; Date=FM_2, Furnizori, Intrari, Masini, NIR
- raport **Fisa_Produsului** — [R_Fisa_Produsului.png](R_Fisa_Produsului.png) — [Reports/Fisa_Produsului.txt](../VERCON_ACCESS/Reports/Fisa_Produsului.txt) · [controale](trasabilitate/R_Fisa_Produsului.md) — 3 / 26; Date=Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno …
- raport **FM1-u** — [R_FM1-u.png](R_FM1-u.png) — [Reports/FM1-u.txt](../VERCON_ACCESS/Reports/FM1-u.txt) · [controale](trasabilitate/R_FM1-u.md) — 5 / 34; Date=FM_2, Intrari, Masini
- raport **FM2** — [R_FM2.png](R_FM2.png) — [Reports/FM2.txt](../VERCON_ACCESS/Reports/FM2.txt) · [controale](trasabilitate/R_FM2.md) — 6 / 22; Date=FM_2
- raport **LISTA_INVENTAR** — [R_LISTA_INVENTAR.png](R_LISTA_INVENTAR.png) — [Reports/LISTA_INVENTAR.txt](../VERCON_ACCESS/Reports/LISTA_INVENTAR.txt) · [controale](trasabilitate/R_MF_LISTA_INVENTAR.md#trasabilitate-schita-raport-lista_inventar) — 7 / 82; Date=Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno …
- raport **Stoc_Final** — [R_Stoc_Final.png](R_Stoc_Final.png) — [Reports/Stoc_Final.txt](../VERCON_ACCESS/Reports/Stoc_Final.txt) · [controale](trasabilitate/R_Stoc_Final.md) — 7 / 31; Date=Masini, Produse, ProduseP, Iesiri, IesiriRetur, Iesiri_H, Intrari, IntrariStorno …
