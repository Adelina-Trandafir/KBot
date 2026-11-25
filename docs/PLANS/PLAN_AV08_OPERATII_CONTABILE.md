# SLICE-AV08 — Operațiile contabile: un singur flux, regulile Access ale formularului `Note`

Continuarea planului [AV02-07](PLAN_AV02_07_SALVARE_OPERATII_CONTABILE.md), după compararea lui cu formularul Access `Note`
(`Surse/RawExport/Forms/Note.txt`) și deciziile Adelinei din 24.11.2026. Planul nu ține progres: progresul e în
[`KBOT_STATUS_AV.md`](../worklog/state/KBOT_STATUS_AV.md).

## Deciziile (24.11.2026)

| Tema | Decizie |
|---|---|
| Protecția operațiilor | Bifă «Protecție operații» într-un tab nou «AVACONT» din Setări › Aplicație, **per utilizator**. Pornită: conturile oferite în editor și cele salvate se potrivesc clasificației operației; la schimbarea clasificației conturile liniilor trec pe varianta clasificației noi. Nu se aplică operațiilor de închidere. |
| Anul anterior și perioada închisă | Ca în Access, inclusiv la ștergere. |
| `NrAng` | Nu se mai dă în niciun flux (ALOP vechi, 2005). Ordonanțare / Propunere / Angajament / NrOpe nu intră în NoteC_editor. |
| Avertismente | «Lună nouă» (Access 217) și «număr de document dublat în aceeași notă» (Access 347). |
| Datele | Data documentului se copiază în data operației. |
| Răspunsul la salvare | ID-urile, numerele și datele salvate; lista «Modificare operații» se reîncarcă și selectează operația. |
| Mesajele | Refuzurile spun ce selecție sau ce câmp trebuie corectat. |
| Conflict | Editorul păstrează ce s-a introdus, marchează casetele greșite ca eroare și oferă reîncărcarea operației. |
| Un singur flux | Salvările ȘI ștergerile (operație, document, «Șterge plata» din Facturi și plăți și din Contări) trec prin același serviciu, cu aceeași blocare. O regulă se schimbă într-un singur loc. |
| Totul într-un loc | Tot ce scrie note contabile (și calculul Contărilor) stă în fișierele cu prefixul `NoteC_`, adunate de `NoteC.py`; cuvântul «postare» dispare; niciun fișier peste ~500 de rânduri, împărțit pe logică (funcții întregi); rutele ferestrelor doar citesc cererea și răspund. |
| Operații diverse și `JI` | Operațiile diverse nu scriu în jurnalul de închidere `JI`. |
| `IdEvidentiereAnterior` | Ține `IdOperatie` din fișierul Access al anului ANTERIOR (numerotare Access, nu `IdOper` KBOT). O evidențiere la care trimite o plată din anul următor este «plătită»: nu se șterge și nu se editează ca neplătită. |
| Note goale | Nota rămasă fără documente (după ștergere sau mutarea documentului) se șterge. |
| Evidențiere neplătită | Numărul, felul și data facturii se pot corecta, ca în Access. |
| Excepțiile Access | Jurnalul `JI` poate scrie într-o lună închisă; ultimul document de închidere (CCV) se poate modifica la data lui. |

## Regulile Access, citite (VERIFICAT)

- `EInchis(Dt)` (`mdl_Functii2022.bas.txt:9`): închis = data ≤ ultimul document `Fel = I`, SAU anul datei < anul curent și luna curentă > ianuarie.
  Îl folosesc ștergerile (`STO_Click`, `STD_Click` din `ModificareNC1280`) ca REFUZ; deschiderea pentru modificare (`Modifica_Operatie`) doar AVERTIZEAZĂ (138) și deschide `Note`.
- `Note.Form_Load`: «Salvare» activ doar dacă data documentului > ultima închidere CCV; un document CCV doar la data ultimei închideri.
- `Note.DataDoc/DataOp_BeforeUpdate`: anul datei = anul de lucru; data ≤ ultima închidere refuzată, cu excepția jurnalului `JI`; avertisment 217 când luna datei e după luna celei mai noi operații.
- `Note.VerificaDateIntroduse`: câmpurile obligatorii; total debit = total credit; ambele părți au linii; nu amândouă cu mai multe linii; an mai vechi de un an refuzat; an din viitor refuzat; evidențierea are clasificație, o singură linie de credit cu partener.
- `Note.NumarDC_LostFocus`: 347 când numărul documentului există deja în aceeași notă (doar la adăugare).
- `Note.Form_Load` + `CTD.Tag`: protecția (`Scheme._MNC.C5`, forțată pe activ la deschidere) restrânge conturile la cele potrivite clasificației; `mPick_ClsfClick` refăcea analiticul conturilor după clasificația nouă și avertiza când contul lipsea (10310). Pentru `Fel = I` protecția e oprită.

## Sub-felii

| Sub-felie | Conținut |
|---|---|
| AV08-01 | Setări › Aplicație › «AVACONT»: «Protecție operații»; filtrarea conturilor și schimbarea lor la clasificație în editor; verificarea pe server. |
| AV08-02 | Perioada închisă și anul anterior ca în Access, într-o singură regulă din `NoteC`, folosită de editor și de ștergere; excepția `JI`; ultimul CCV editabil. |
| AV08-03 | `NrAng` scos din toate fluxurile. |
| AV08-04 | Un singur flux de scriere: ștergerile prin `NoteC.salvare` (blocare + tranzacție) și funcțiile comune de ștergere; `IdEvidentiereAnterior`; note goale; familia `NoteC_*` (tot ce scrie note și calculul Contărilor, câte un fișier pe logică). |
| AV08-05 | Editorul: avertismentele 217 / 347 confirmate de operator, data operației urmează data documentului, mesaje cu câmpul de corectat, casete marcate, conflict cu reîncărcare, răspunsul complet la salvare. |
| AV08-06 | Evidențierea neplătită: număr, fel și dată factură corectabile. |

## Contractul salvării (editor)

- Cererea primește în plus `Protectie` (bifa utilizatorului) și `Confirmari` (codurile avertismentelor acceptate: `LUNA_NOUA`, `DOCUMENT_DUBLAT`).
- Refuzul: `{error, reason, camp?, parte?, linie?}`; `camp` numește caseta (`IdJurnal`, `IdClsf`, `NumarDocument`, `FelDocument`, `DataDocument`, `DataOperatie`, `Explicatie`, `IdNota`, `IdDocument`), `parte` + `linie` (index de la 0) o linie din Debit / Credit.
- Avertismentul neconfirmat: 409 cu `reason = CONFIRMARE` și `confirmare = <cod>`; clientul întreabă Da/Nu și retrimite cu codul.
- Conflictul de versiune: 409 cu `reason = VERSIUNE`.
- Răspunsul: `{IdOper, IdDocument, IdNota, NumarNC, NumarDocument, DataDocument, DataOperatie, OperatiiDocument: [IdOper]}`.

## Ce rămâne afară

Integrarea importului Access / migratoarelor; salarii, burse, extrase; asocierea cu altă e-factură; redistribuirea alocărilor unei plăți postate.
