# VERCON — maparea machetelor Access la web (SLICE-VC00-04)

Generat de `VERCON/tools/mapare.js`. Starea: DOCUMENTAT. Capabilitatile sunt cele din PLAN (matricea functionala a gestiunilor) aplicate pe dependentele de date detectate static in exporturi (tabele/interogari citite transitiv, 4 nivele). **da** = disponibil; **nu** = nu se expune (si apelul direct este refuzat de server); **de verificat** = depinde de verificarea de la VC06/VC08.

Tipuri: MAT = Materiale/alt tip fara MF si OI; OI; MF; AL = Alimente.

## Formulare

SLICE-D003-20: sunt grupate numai obiecte cu toate celelalte celule identice; numărul păstrează poziția inițială, iar fiecare schiță rămâne legată de obiectul său.

| Obiecte (#: nume · Schita) | Familie | Dispozitie | MAT | OI | MF | AL | Date (tabele) | Interogari |
|---|---|---|---|---|---|---|---|---|
| 1: AdaugaTabelInTBLS · — | Exclus | EXCLUS-SCHEMA-ACCESS | da | da | da | da |  |  |
| 2: ALEGE_BC · [png](SCHITE/F_ALEGE_BC.png)<br>5: BCP · [png](SCHITE/F_BCP.png) | BC | INCLUS-UI | da | nu | nu | da |  |  |
| 3: ALEGE_BC_RETUR · [png](SCHITE/F_ALEGE_BC_RETUR.png) | BR | INCLUS-UI | de verificat | de verificat | de verificat | de verificat |  |  |
| 4: BC · [png](SCHITE/F_BC.png) | BC | INCLUS-UI | da | nu | nu | da | Masini, TmpFactV |  |
| 6: BR · [png](SCHITE/F_BR.png) | BR | INCLUS-UI | da | nu | nu | da | Masini, TmpFactV |  |
| 7: Clienti · [png](SCHITE/F_Clienti.png) | Nomenclatoare | INCLUS-UI | da | da | da | da | Clienti |  |
| 8: CLS2024 · [png](SCHITE/F_CLS2024.png)<br>13: CPV2017 · [png](SCHITE/F_CPV2017.png)<br>40: Produse · [png](SCHITE/F_Produse.png) | Nomenclatoare | INCLUS-UI | da | da | da | da |  |  |
| 9: CN · [png](SCHITE/F_CN.png) | Alimente | INCLUS-UI | nu | nu | nu | da | Feluri, Tmp_Cina |  |
| 10: ComisieINV subform · [png](SCHITE/F_ComisieINV_subform.png) | Nomenclatoare | INCLUS-UI | da | da | da | da | ComisieInventar |  |
| 11: ComisieNIR subform · [png](SCHITE/F_ComisieNIR_subform.png) | Nomenclatoare | INCLUS-UI | da | da | da | da | ComisieNIR |  |
| 12: Config · [png](SCHITE/F_Config.png) | Configurare | INCLUS-UI | da | da | da | da |  |  |
| 14: D · [png](SCHITE/F_D.png) | Alimente | INCLUS-UI | nu | nu | nu | da | Feluri, Tmp_Dejun |  |
| 15: EFACTURA · — | Exclus | EXCLUS-EFACTURA | da | da | da | da | Masini |  |
| 16: FC · [png](SCHITE/F_FC.png) | FC | INCLUS-UI | da | da | da | da | Masini, TVA |  |
| 17: FCS · [png](SCHITE/F_FCS.png) | FCS | INCLUS-UI | de verificat | de verificat | de verificat | de verificat | Masini, NIR, TVA |  |
| 18: Feluri · [png](SCHITE/F_Feluri.png) | Alimente | INCLUS-UI | nu | nu | nu | da |  |  |
| 19: frmMail · —<br>20: frmPB · —<br>21: frmProgressBar · —<br>35: MonthPicker · —<br>44: subTreeView · — | Shell | EXCLUS-INFRA-ACCESS | da | da | da | da |  |  |
| 22: Furnizori · [png](SCHITE/F_Furnizori.png)<br>23: Furnizori2022 · [png](SCHITE/F_Furnizori2022.png) | Nomenclatoare | INLOCUIT-PARTENERI-KBOT | da | da | da | da | Furnizori |  |
| 24: Gestionari · [png](SCHITE/F_Gestionari.png) | Configurare | NEREFERIT-NEPORTAT | da | da | da | da |  |  |
| 25: IA · [png](SCHITE/F_IA.png) | Inchidere an | INCLUS-UI | da | nu | nu | da | tmp_Balanta |  |
| 26: LABC · [png](SCHITE/F_LABC.png) | Alimente | INCLUS-UI | nu | nu | nu | da | tmp_LABC |  |
| 27: LEGBCLA · [png](SCHITE/F_LEGBCLA.png) | Alimente | INCLUS-UI | nu | nu | nu | da | tmp_LEGBCLA |  |
| 28: ListaAlimente · [png](SCHITE/F_ListaAlimente.png) | Alimente | INCLUS-UI | da | nu | nu | da | tmp_LEGBCLA |  |
| 29: ListaAlimente_copy · [png](SCHITE/F_ListaAlimente_copy.png) | Alimente | NEREFERIT-NEPORTAT | nu | nu | nu | da |  |  |
| 30: Main · [png](SCHITE/F_Main.png) | Shell | EXCLUS-INFRA-ACCESS | da | da | da | da | TVA |  |
| 31: Masini · [png](SCHITE/F_Masini.png)<br>32: Masini_sub · [png](SCHITE/F_Masini_sub.png) | Nomenclatoare | INCLUS-UI | da | da | da | da | Masini |  |
| 33: MD · [png](SCHITE/F_MD.png) | Alimente | INCLUS-UI | nu | nu | nu | da | Tmp_MicDejun |  |
| 34: Mesaje_ANAF · —<br>52: XMLPath · —<br>53: XMLPath_sub · — | Exclus | EXCLUS-EFACTURA | da | da | da | da |  |  |
| 36: OI_Casare · [png](SCHITE/F_OI_Casare.png) | OI | INCLUS-UI | nu | da | da | nu | Masini, OI_TmpFact |  |
| 37: OI_Predare · [png](SCHITE/F_OI_Predare.png) | OI | INCLUS-UI | nu | da | da | nu | Masini, TmpFactV |  |
| 38: OI_TmpFact · [png](SCHITE/F_OI_TmpFact.png) | OI | INCLUS-UI | nu | da | da | nu | OI_TmpFact |  |
| 39: OI_Transfer · [png](SCHITE/F_OI_Transfer.png) | OI | INCLUS-UI | nu | da | da | nu |  |  |
| 41: ProduseM · [png](SCHITE/F_ProduseM.png) | Nomenclatoare | INCLUS-UI | da | da | da | da | tmpProduseM |  |
| 42: SETARI · [png](SCHITE/F_SETARI.png) | Configurare | INCLUS-UI | da | da | da | da | Jud, UNIT, ComisieNIR, ComisieInventar |  |
| 43: SFP · [png](SCHITE/F_SFP.png) | Situatii | INCLUS-UI | da | da | da | da | SFP_CautCBX |  |
| 45: TmpFact · [png](SCHITE/F_TmpFact.png)<br>46: TmpFactMF · [png](SCHITE/F_TmpFactMF.png) | FC | INCLUS-UI | da | da | da | da | TmpFact |  |
| 47: TmpFactV · [png](SCHITE/F_TmpFactV.png) | FC | INCLUS-UI | da | da | da | da | TmpFactV |  |
| 48: tmpSFP · [png](SCHITE/F_tmpSFP.png) | Situatii | INCLUS-UI | da | da | da | da | tmpSFP |  |
| 49: tmp_Balanta subform · [png](SCHITE/F_tmp_Balanta_subform.png) | Situatii | INCLUS-UI | da | da | da | da | tmp_Balanta |  |
| 50: UNIT · [png](SCHITE/F_UNIT.png) | Configurare | INCLUS-PARTIAL | da | da | da | da | Jud, UNIT |  |
| 51: VaerCon · [png](SCHITE/F_VaerCon.png) | Shell | EXCLUS-INFRA-ACCESS | da | da | da | da | cale |  |

## Reguli de capabilitate verificate la schita

- **FC**: formular comun; extensia MF (nr. inventar, data intrarii, durate, ProduseM, panoul „TABELA INTERACTIVA") apare numai la gestiunea MF. Sursa `FC.Form_Load` citeste `MF`/`OBINV`; schita arata ambele stari suprapuse.
- **BC**: nu apare pentru OI si MF; apel direct refuzat. Alimente acceseaza BC numai prin circuitul listei (traseul manual se stabileste la VC08-03).
- **OI_Predare / OI_Transfer / OI_Casare**: OI si MF; nu pentru MAT si AL (implicit).
- **ListaAlimente, LABC, LEGBCLA, Feluri, subformularele de mese**: numai AL.
- **BR / FCS**: disponibilitate si efect pe registru OI / activ MF de verificat la VC06/VC08.
- **Meniul SFP**: se construieste din `tmpOptiuniSFP` (MODEL, TIP) — metadatele **nu sunt in export** si au fost primite de la utilizator (vezi mai jos)
- **Forme de shell Access** (Main, VaerCon, subTreeView, MonthPicker, frmMail, frmPB, frmProgressBar) nu se portează; `VaerCon` si `Main` sunt schitate doar ca referinta pentru selectorul de context (VC02-01).

## Corecțiile UI ale utilizatorului — 12.10.2026

Sursa: [Corectii_mockup-uri_01](MOCKUPURI/Corectii_mockup-uri_01/) și mesajul ulterior cu cinci atașamente, revizia 2. Această secțiune descrie interfața web solicitată, nu o nouă constatare despre Access. Contract: [FLUXURI_UI](FLUXURI_UI.md). Mockupuri: [index revizuit v2](MOCKUPURI/2026-10-12-corectate-v2/INDEX.md).

| Familie | Adaptare obligatorie web |
|---|---|
| FC/NIR | Antet grupat pe furnizor/cont/comisie, factură, NIR; valoare factură activă/manuală, lipsă → suma liniilor, N01/N02 păstrat; produs combobox cu căutare; coloane MF în grila produselor, fără secțiune separată |
| FC/NIR documente | Arbore în dreapta furnizor → dată → număr factură + total; ⋯ la hover/focus; ascuns la Adaugă/Modifică |
| BC | DGV **identic vizual** cu NIR, compact, Adaugă rând în footer și totaluri pe coloane; arbore în dreapta, ascuns în editor; Adaugă document și pentru Alimente |
| ListaAlimente/MD/D/CN | Rezumate porții/preț/valoare/distribuit și feluri DGV în stilul NIR, fără padding mare; fără Alimente necesare; Liste salvate în dreapta în consultare, formatate ca NIR/BC/BP, ascunse în editor |
| ListaAlimente/ALEGE_BC/LEGBCLA | **Mai multe BC-uri per listă**, DGV NIR cu valoare pe rând și total general; Adaugă rând deschide popupul cu BC existente din Alimente curentă neasociate altei liste; selecție aditivă fără duplicate; salvează atomic lista/mese/setul de legături și revalidează toate BC-urile, fără creare/resalvare BC sau consum nou; IdLista țintă neunic |
| OI_Predare | Fără Perioada/Deținători sau Efect/Istoric; arbore BP în dreapta numai în consultare; sursa = conturile de intrare ale produselor |
| OI_Transfer | Aceeași structură; arbore cu toate BT în consultare; produse numai ale predătorului |
| OI_Casare | Aceeași structură, arbore BS; numai produse ale predătorului, fără Primitor |

Salvare confirmată/Renunță → reafișarea arborelui; eșec/conflict/rezultat necunoscut → draft păstrat. Toate grilele BP/BT/BS folosesc și ele aspectul NIR, cu Adaugă rând în footer. Arborii de documente diferă de selectorul de produse și de arborele perioadelor SFP. SCHITE rămâne reprezentarea exporturilor Access. Cardinalitatea istorică LegBCLA din export nu limitează ținta 1:N decisă; DDL/generatorul necesită aliniere, vezi SCHEMA_VC.

## Rapoarte

Vezi [RAPOARTE.md](RAPOARTE.md).

## Limite

- Detectia de dependente este textuala (nume de tabele/interogari in RecordSource/RowSource si in SQL), nu demonstreaza ca ramura este rulata.
- Formularele cu **subformulare** includ dependentele subformularului, dar controalele legate prin `LinkChildFields` nu sunt analizate.
- Starea de vizibilitate setata din VBA nu este in export.

## Ribbonul „Vercon" (furnizat de utilizator; [XML](REFERINTE/ribbon_Vercon.xml))

La pornire ribbonul este `startFromScratch`; **vizibilitatea si activarea fiecarui buton vin din tabelul `Rbb` (coloanele `N`, `V`, `E`)** prin `GetVisible`/`GetEnabled` (`basRibbonCallbacks`). Codul exportat nu scrie in `Rbb` nicaieri; **continutul tabelului a fost primit de la utilizator** ([Rbb.xml](REFERINTE/Rbb.xml), [Rbb.xsd](REFERINTE/Rbb.xsd), 49 randuri, 11.10.2026): toate butoanele si grupurile au `V=1, E=1`; singura exceptie este tabul `Tab_MF` (`V=0, E=0`, fila MF este ascunsa, MF foloseste fila „Obiecte de Inventar / Mijloace Fixe") — **ribbonul nu diferentiaza dupa tip prin date**; diferentierea o face doar `Form_Open`. `Rbb` mai contine butoane absente din ribbonul primit (`btnDelNir`, `btnDelBC`), elementele ribbonului de previzualizare la tiparire (`ExportExcel`, `FilePrintQuick`, `FileSaveAsPdfOrXps`, `PageSetupDialog`, `PrintPreviewClose`, `tabPPVW`) si, in coloana `C`, `gest_CPV` (la `btnAddProd`), `Masini` (`btnCategorii`), `Config` (`btnUnitate`); codul deschide insa `produse`, `Masini` si `SETARI`, deci coloana `C` nu este folosita de `OnActionButton`. Reguli de gestiune sunt insa impuse si in `Form_Open` al formularelor (mai jos), deci nu depind de ribbon.

| Tab / grup | Buton | Id | Deschide | Disponibilitate (dovada) |
|---|---|---|---|---|
| Configurari | Adauga produse | btnAddProd | Produse | toate |
| Configurari | Categorii / Conturi | btnCategorii | Masini | toate |
| Configurari | Furnizori | btnFurnizori | Furnizori2022 | toate |
| Configurari | Gestionari | btnClienti | Clienti | toate |
| Configurari | Unitate | btnUnitate | SETARI | toate |
| Configurari | Editare coduri | btnCoduri | frmPB + UpDateP + IntrariIesiriTotal (Coduri) | EXCLUS N06 |
| Configurari | Inchidere an | btnInchidere | IA | nu la OI si MF (decizie utilizator) |
| Configurari | Retetar | btnRetetar | (fara actiune in OnActionButton: „Lipseste actiunea") | neimplementat in Access |
| Configurari / Online | Incarca unitate | btnUPDN | frmUpl (formular absent din export) | infrastructura, exclus |
| Configurari / Online | Creeaza backup | btnBck | frmMail | infrastructura, exclus |
| Gestiuni / Intrari | Adaugare / Editare N.I.R. | btnAddNIR | FC | toate |
| Gestiuni / Intrari | Adaugare / Editare Storno | btnAddStorno | FCS | toate (cod nu blocheaza) |
| Gestiuni / Iesiri | Bon de consum | btnAddBC | BC | nu la OI/MF (cod) |
| Gestiuni / Iesiri | Adaugare / Editare Retur | btnAddRE | BR | nu la OI/MF (cod) |
| Gestiuni / Iesiri | Lista de alimente | btnAddLA | ListaAlimente | nu la OI/MF (cod) |
| OI / MF | Bon de predare | btnAddBP | OI_Predare | numai OI/MF (cod) |
| OI / MF | Bon de transfer | btnAddBT | OI_Transfer | numai OI/MF (cod) |
| OI / MF | Bon de casare | btnAddBS | OI_Casare | numai OI/MF (cod) |
| OI / MF | Lista Mijloace Fixe | btnListMF | (fara actiune in OnActionButton: „Lipseste actiunea") | neimplementat in Access |
| Rapoarte | TOATE RAPOARTELE | btnLstSFP | SFP | meniu dupa TIP |

Note: `OnActionButton` mai contine ramuri pentru butoane care **nu sunt in ribbonul furnizat** (`btnAmmy`, `btn_AddMF`, `btn_Amortizare`, `btn_Casari`, `btn_Restaurare`), spre formulare absente din export (`Mijloace Fixe`, `Amortizare`, `Casari`, `MFIesite`): cod mort sau ribbon mai vechi. Butonul „Gestionari" deschide formularul `Clienti`; formularul `Gestionari` din export nu este deschis de ribbon. `Furnizori` din ribbon deschide `Furnizori2022`.

## Reguli din cod (confirmate in `Form_Open` / `Form_Load`)

| Formular | Regula | Linia in export |
|---|---|---|
| BC | Refuza daca `Setari.OBINV` sau `Setari.MF` (mesaj „baza de date pentru Obiecte de Inventar / Mijloace Fixe… nu se pot adauga Bonuri de CONSUM") | Forms/BC.txt:2639 |
| BR | idem BC | Forms/BR.txt:2436 |
| ListaAlimente | idem BC (deci este permisa si la o baza fara Alimente) | Forms/ListaAlimente.txt:2473 |
| OI_Predare / OI_Transfer / OI_Casare | Refuza daca **nici** OBINV **nici** MF | OI_Predare.txt:2756, OI_Transfer.txt:2449, OI_Casare.txt:2355 |
| IA | Access refuza **doar** daca OBINV („nu se face inchidere anuala"); **decizie utilizator 11.10.2026: OI si MF NU se inchid anual** (la MF Access nu o impiedica, web o interzice) | Forms/IA.txt:896 |
| FC | Citeste `MF` si `OBINV` ca `isMF`/`isOI` si adapteaza ecranul | Forms/FC.txt:4461 |
| Main | La incarcare, daca `UNIT` este goala, o completeaza din `CaleTemp` (Firma, CUI, DC, Detalii) | Forms/Main.txt:180 |

Concluzie: in Access exista trei situatii efective determinate de `Setari`: **OI** (`OBINV`), **MF** (`MF`) si **restul** (fara steaguri). **Alimente vs Materiale nu este stabilit de niciun steag**: formularul `ListaAlimente` este permis oricarei baze fara OBINV/MF. Diferenta apare doar in date (ex.: `ListaAlimente` are 127 randuri in baza Alimente si 0 in MF si OI). Tipul web `ALIMENTE` vs `MATERIALE` este deci o alegere a operatorului in migrator.

## Intrarea in aplicatie — `VaerCon.Intrare_Click` (Forms/VaerCon.txt:1697)

Ordinea executata la „Intrare" (confirmat in cod):

1. Citeste randul ales din `cale` (`Combo1` = ID; coloana 2 = folderul `Cale`); `AnDate` obligatoriu (altfel „Vercon 3 deschide doar bazele incepand cu 2022").
2. Fisierul este `<folderul aplicatiei><Cale>\tz_<AnDate>.accdb`; daca lipseste dar exista `tz.accdb`, il **redenumeste** in `tz_<AnDate>.accdb` (si `tz_vechi.accdb` in `tz.accdb`); altfel „Lipseste baza de date pentru anul selectat!".
3. Completeaza `Caletemp` (ID, CaleTemp = folder, Firma, ANL = an, DC, CaleReala = fisier, Detalii, Gestionar = `cale.Reprezentant`, CUI).
4. `Repara2024` (repara/actualizeaza schema backendului: **exclus**), apoi `CnnTbl_2022` (leaga tabelele din fisier).
5. `TempVars`: separatorul zecimal si **`Zec = Nz(DLookup("ZEC","NR"),4)`** (fallback 4 si la eroare).
6. `Setari` (UseTreescroll), `SalvezSiguranta` (ZIP local in C:\SIG), `Functii` (reparatii unice din `RunOnce`), verificare backup online (>10 zile, doar anul curent), deschide `Main` ascuns.

**Ce rezulta pentru atribuirea in migrator (dovada):** un rand din `cale` este o pereche (folder, an): `IA` creeaza pentru anul urmator un rand nou cu acelasi `Cale`, `DC`, `Detalii`. Deci **gestiunea in Access = folderul `cale.Cale`** (+ `Detalii`), iar fisierele `tz_<an>.accdb` din folder sunt exercitiile ei. **Subunitatea nu exista in Access**; se atribuie in `VRC.Migrator`. `cale.DC` este codul DC; in `cale.accdb` furnizat toate cele 7 randuri au DC `00000000` (valoare-semnal, nu un DC real).

| Rand `cale` | Folder | An | Observatie |
|---:|---|---:|---|
| 1 si 12 | \ISJ2020 | 2024 | doua randuri pentru acelasi folder si an |
| 11 | \14 | 2024 | folderul pentru care frontendul leaga `14\tz_2024.accdb` |
| 13 | \OI_MONNET | 2024 | |
| 14 | \AUTO | 2024 | |
| 15 | \14 | 2100 | an 2100: codul nu il trateaza special (ar cauta `tz_2100.accdb`); **ipoteza**: marcaj pentru baza fara an |
| 16 | \MF | 2100 | idem |

Toate au `Detalii = Manuale` (`UNIT` il suprascrie pe toate randurile din `AlteDetalii`). Fisierele de pe disc nu au denumirile din `cale`: `tz_Alimente.accdb`, `tz_Mijloace_Fixe.accdb`, `tz_Obiecte_Inventar.accdb` sunt nume puse la export; legatura cu folderele \14, \MF, \OI_MONNET, \AUTO, \ISJ2020 o stabileste operatorul in migrator, la import.
