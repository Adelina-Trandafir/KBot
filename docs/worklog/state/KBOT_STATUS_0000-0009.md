# K-BOT — STATUS, slices 0000–0009

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0000 — HELP (ajutorul interactiv + manualul)

**Standing slice.** Operator, 30.09.2026: EVERY piece of work on the help (engine, capture tool,
topics, screenshots, tours, manual) is recorded HERE, as a sub-slice `0000-NN`, never under a new
slice number. Worklogs: `SLICE-0000-NN-<slug>.md`.

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0000 | **Ajutor interactiv + manual, în trei părți: Contabil / Opțiuni avansate / Director (cererea operatorului, 30.09.2026)** | în lucru | — | Plan: 01 motorul; 02 unealta de capturi; 03 conținut Partea 1; 04 tururi ghidate; 05 Partea 2; 06 Partea 3 + cerințele MF la un DDF nou (ORD nou încă nu există → exclus). Textul se scrie din fișierele .md ale feliilor; fără interiorul controalelor și al browserului; pagina FOREXE doar ca mod de funcționare. Imaginile le face operatorul prin unealta de capturi (0000-02), inclusiv pe PC-ul clientului pentru fluxul FOREXE. |
| 0000-01 | **Motorul de ajutor: F1 oriunde, «?» pe fiecare bară de titlu, fereastra de ajutor (cuprins, căutare, Înapoi/Înainte), export manual HTML** | GATA pe cod (build **0 avertismente, 0 erori**) / **văzut pe ecran** (login F1, «?» în fereastra principală, căutare) | `SLICE-0000-01-help-engine.md` | Subiecte Markdown în `src/KBot.App/HelpContent/` → `<AppDir>\Help\`; Markdig 0.45.0. `KBotHelp` (Theming) + `HelpService` (App). Director → doar Partea 3; ceilalți Partea 1 + Partea 2 cu opțiunile avansate active. FileVersion nebumped. (Numerotat întâi 0097-01, renumerotat la cererea operatorului.) |
| 0000-02 | **Unealta de capturi pentru ajutor** — etichete `<!-- capture: ... -->` în subiecte, fereastra «Capturi pentru ajutor» cu «Fă poza» pe fiecare rând, poziționare pe ecran + selecție cu dreptunghi, PNG salvat unde îl cere eticheta | GATA pe cod (build **0 avertismente, 0 erori**); comutatorul văzut de operator; fluxul de captură nerulat de Claude | `SLICE-0000-02-help-capture-tool.md` | Comutator în Setări › Aplicație, vizibil doar cu opțiunile avansate; nedescris în ajutor. Etichete `<!-- capture: id | caption | goto | prepare -->`; salvare în `<AppDir>Helpimg` + sursă pe build din repo. |
| 0000-03 | **Partea 1 «Contabil»: textul, cu etichete de captură** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-03-partea-1-contabil.md` | 31 de subiecte + 30 de etichete de captură; secțiunea «Ce cere MF la un DDF nou» din `FUNDAMENT_DocumentFundamentare_CAB.md`. Tooltipul lui `cboSs` spune «Subperioada» dar e sursă/sector — nemodificat. |
| 0000-04 | **Tururi ghidate** (+ tooltipul «Sursă / sector» corectat) | GATA pe cod (build **0 avertismente, 0 erori**), nevăzut pe ecran | `SLICE-0000-04-tururi-ghidate.md` | Fișiere `HelpContent/tours/*.md`; inel colorat în jurul controlului + bulă cu Înapoi / Înainte / Închide; pornire din pagina subiectului și din pagina de start. Patru tururi pentru Partea 1. |
| 0000-05 | **Partea 2 «Opțiuni avansate»: textul, cu etichete de captură** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-05-partea-2-optiuni-avansate.md` | 6 subiecte + 7 etichete: activare, Documente (Adobe / Excel), Pagina FOREXE, Temă, Căi fișiere, jurnale. Modul de capturi NU e descris (regula operatorului). |
| 0000-06 | **Partea 3 «Director»: textul, cu etichete de captură, + turul ferestrei directorului** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-06-partea-3-director.md` | 4 subiecte (prezentare, lista, alte unități, semnarea) + 3 etichete + `tur-director` (5 pași). Pozele se fac pe calculatorul directorului și se încarcă cu «Încarcă» (lista de capturi e doar în fereastra contabilului). |
| 0000-07 | **Acoperire F1 + finisare**: fiecare fereastră a operatorului are subiect; subiect nou «Actualizarea K-BOT»; turul Părții 2 | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-07-acoperire-si-finisare.md` | Ferestre noi acoperite: Alegerea unității, Grafice și benzi, Istoric angajament (preluarea salvărilor din pagină), Golește jurnale, Actualizare, Informații interne. Fraza «ce modifici de mână ocolește K-BOT» înlocuită (greșită de la 0073). Editorul ORD lăsat afară — decizia operatorului. |
| 0000-08 | **Ordonanțarea în ajutor + documentația sistemului de ajutor** — vederea ORD rescrisă, subiecte noi «Ordonanțări noi (și ștergerea lor)» și «Editorul de ordonanțare»; `docs/HELP_SYSTEM.md` (ghidul de întreținere + procedura de actualizare); `tools/HelpCheck/Check-Help.ps1`; trimiteri din CLAUDE.md, CODE_WORKFLOW.md, README | GATA (text + script; build **0 avertismente, 0 erori**; verificarea **fără erori**, acoperire completă) / de citit de operator | `SLICE-0000-08-ord-si-documentatie.md` | Fluxul MF pentru trimiterea ORD în FOREXE NU e descris (neclar, K-BOT nu trimite încă ORD). Tooltipul greșit al lui «Lipește» din atașamentele ORD corectat. |
| 0000-09 | **Semnarea ORD din formularul MF** — validarea în doi pași, cele cinci semnături în ordine, minimul (1 + 2 + Ordonator), CFP înaintea ordonatorului; `docs/FUNDAMENT_Ordonantare_CAB.md` | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-09-semnarea-ord.md` | Din `Surse/ord_xdp.xml` + `ord_xdp_full.xml`. Pasul următor (recepție + încărcare pe serverul CAB) doar numit, nefăcut în K-BOT. 1 + 2 + 5 și cele două validări confirmate de operator; avertisment «semnează după a doua validare» + ordinea pe două persoane. |
| 0000-10 | **Semnarea ORD refăcută pe macheta A1.0.11** — validare → semnătura 1 → validare → semnătura 2 → (CFP) → ordonator; «Alte Avize», «Verificat/Avizat», «Anulare Validare»; fără ORDNT.xml la validare | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-10-ord-a1-0-11.md` | Înlocuiește descrierea din 0000-09 (era macheta veche A1.0.08). Din `Surse/ETAPE ORDONANTARE/`; `Etapa3_xdp.xml` = copie a lui Etapa1. Serverul servește A1.0.11 (confirmat). Etapa 1 doar cu col. 4 = fundătură (starea de după semnătura 1 nu se salvează); tabelul complet înaintea semnăturii 1. |
| 0000-11 | **Cinci tururi noi**: Ordonanțare, Recepții, Plăți, Extrase de cont, Setări (11 tururi în total) | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / nevăzut pe ecran | `SLICE-0000-11-tururi-noi.md` | Fără tur pentru editorul ORD și Asocieri (ferestre modale). Corectat în trecere: schimbarea parolei e pe pagina «Informații», nu «Autentificare». |
| 0000-12 | **Fereastra Asocieri, explicată pe larg** — secțiune nouă (de ce există, fereastra pe părți, pas cu pas, cazuri speciale) + `tur-asocieri` (8 pași, pornit din fereastră) | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-12-asocieri.md` | Din `FUNDAMENT_Asociere_Receptii.md` + `AsociereForm`. F14 (oprit) și regula datei (retrasă) lăsate afară. Turul merge doar pornit cu fereastra deschisă (fereastră modală). |
| 0000-13 | **Sursa fiecărei explicații + ajutorul pentru 0097** — eticheta ascunsă `<!-- slice: … -->` sub fiecare secțiune și pas de tur (222, cât s-a putut stabili), scoasă la citire de motor, verificată de `Check-Help.ps1`; regula nouă «orice schimbare vizuală intră în ajutor, cu numărul feliei» (CLAUDE.md, CODE_WORKFLOW, HELP_SYSTEM); fereastra «Ce recepții reîmprospătez?» la descărcare, unitatea din bara de titlu, Setări › Autentificare, sesiunea expirată, rădăcinile și ștergerile ORD/DDF | GATA (build **0 avertismente, 0 erori**; verificarea fără erori, acoperire «(none)») / de citit de operator | `SLICE-0000-13-sursa-explicatiilor-si-0097.md` | Etichetele vechi sunt «cel mai bun efort». Capturi noi: `unitate-selector`, `setari-autentificare`; de refăcut: `ord`, `ddf-vedere`. |
| 0000-14 | **Documentul semnat nu se mai generează niciodată** (cod: `SigningMessages`, `OrdView`, `DdfView` — refuz în loc de întrebare) + **DDF după trimitere** corectat (B în documentul semnat pe A, 0078-06) + «încărcat» / «preluat» + subiect nou `contabil.liste` (butoanele din capul și subsolul arborilor și tabelelor) și butoanele fiecărei vederi | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-14-semnat-nu-se-regenereaza-si-butoanele-listelor.md` | Trei iconițe de subsol fără acțiune (Plăți, Istoric, DDF «+») — scoase din ajutor, decizia operatorului. |
| 0000-15 | **Iconițele de subsol fără acțiune scoase** (Plăți «Descarcă din CAB», Istoric «Reîncarcă», DDF «+ Adaugă») + starea «PDF final — de semnat B» (fost «… A și B») | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / nevăzut pe ecran | `SLICE-0000-15-iconite-moarte-si-starea-pdf-final.md` | Designer editat de mână. Capturi de refăcut: `plati`, `istoric`, `ddf-vedere`. |
| 0000-16 | **Arborele așteaptă cât se deschide un document** (0078-08) — secțiune nouă în `contabil.liste` + câte un rând cu legătura în `contabil.ddf`, `contabil.vederi.ord`, `contabil.notecab`, `contabil.fereastra`; fără nimic din felul în care lucrează K-BOT pe dinăuntru (regula nouă în HELP_SYSTEM §5) | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-16-arbore-blocat-la-deschidere.md` | Fără capturi noi. Reperul «la zi până la» nemutat. |
| 0000-17 | **Schimbarea unității închide singură conexiunea FOREXE; certificatul memorat se uită doar cu opțiunea nouă din Setări › FOREXE (debifată la început)** (0097, cererea operatorului 30.09.2026): `contabil.fereastra` «Unitatea de lucru», `contabil.setari` secțiune nouă «Pagina FOREXE: certificatul memorat», pașii din `tur-fereastra` și `tur-setari` | GATA (verificarea fără erori) / nevăzut pe ecran | `SLICE-0000-17-schimbarea-unitatii-deconecteaza-forexe.md` | Fără capturi noi. `RobotQueueForm` (0098, alt fir) fără subiect — semnalat de verificare. |
| 0000-18 | **Căutarea înțelege o întrebare (fără model)**: cuvintele de umplutură scoase, rădăcini de cuvânt («trimit» = «trimiterea»), scor după câte cuvinte se potrivesc (titlu > cuvinte-cheie > titlul secțiunii > text), căutare pe SECȚIUNI (`## `) cu fragment de text și deschiderea paginii chiar la secțiune; `HelpHit` (`HelpSearch.vb`) | GATA pe cod (build **0 avertismente, 0 erori**; verificarea fără erori) / nevăzut pe ecran | `SLICE-0000-18-cautare-pe-intrebari.md` | Plan: `docs/PLAN_help_assistant.md`. Ponderile sunt o primă estimare, de reglat din datele 0000-21. Textul ajutorului despre căutare: în 0000-22. |
| 0000-19 | **`open:` pe subiecte**: cheie nouă de antet cu aceleași valori ca `goto:` (verificată de motor și de `Check-Help.ps1`); un rezultat de căutare știe ecranul subiectului («Deschide «Rezervări»», textul luat din fereastra principală) și turul lui; `open:` pe 25 de subiecte + sinonime evidente în `keywords:` | GATA pe cod (build **0 avertismente, 0 erori**; verificarea fără erori) / butoanele apar abia în 0000-20 | `SLICE-0000-19-open-pe-subiecte.md` | Fără `open:` pe capitole, pe fereastra principală și pe partea directorului. |
| 0000-20 | **Meniul «?»**: butonul «?» din bara de titlu deschide un meniu sub el — căsuță de căutare (caută pe loc, local), pagina ecranului curent, tururile ferestrelor vizibile (un dosar pe fereastră când sunt mai multe), «Deschide ajutorul complet (F1)»; F1 neschimbat; fereastra de ajutor are aceeași căutare (lista ține locul cuprinsului cât căsuța are text); controale noi în `KBot.Controls/Popup/` (`KBotHelpPopup`, `KBotHelpSearchPanel`, `KBotHelpList`); cheie opțională `screens:` pe tururi; 5 tururi noi (Sumar, Istoric, Note corecție, Clasificații, Parteneri) | GATA pe cod (build **0 avertismente, 0 erori**; verificarea fără erori) / **nevăzut pe ecran** | `SLICE-0000-20-popup-ajutor.md` | Dosarele se deschid în listă (nu submeniu). Fără tur: editoarele DDF/ORD (decizia operatorului), `BrowserView`, `RobotQueueForm` (0098). Textul ajutorului: 0000-22. |
| 0000-21 | **Întrebările și notele, pe server**: fiecare întrebare scrisă în ajutor (meniul «?» și fereastra) se păstrează cu rezultatele arătate (doar id-uri), rezultatul folosit, butonul folosit și nota 1-5 («A fost util răspunsul?», stele noi sub rezultate); FĂRĂ utilizator, unitate, IP, calculator — nici în tabelă, nici în jurnalele codului nostru; listă de așteptare locală (câte un fișier pe întrebare în `%APPDATA%\AVACONT\KBot\HelpOutbox\`), trimisă în loturi (la 3 minute, la 10 în așteptare, la pornire, la ieșire); tabela `AVACONT_COMUN.FX_AjutorIntrebari` (`sql/0000_21_fx_ajutor_intrebari.sql`), ruta `POST /api/help/feedback` (`routes/help_feedback.py`, upsert după `Qid`) | GATA pe cod (build **0 avertismente, 0 erori**; Python compilat; verificarea fără erori) / **DDL + ruta neaplicate pe VPS**, nerulat | `SLICE-0000-21-intrebari-si-note.md` | Interogările pentru «pasul 4» (fără clic, note ≤ 2, cele mai repetate, poarta de ~85 %) sunt în worklog. |
| 0000-22 | **Textul ajutorului pentru asistent + documentația**: `contabil.ajutor` rescris (meniul «?», cum pui o întrebare, butoanele unui rezultat, stelele, tururile din meniu, căutarea din fereastră, fraza despre trimiterea fără nume), nota din `director`, pașii «?» din `tur-fereastra` și `tur-asocieri`; `docs/HELP_SYSTEM.md` (§1, §2, §3, §4, §6, §7 nou), `HelpContent/README.md`, `help-version.txt` în procedură | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-22-text-asistent-si-documentatie.md` | Capturi noi: `ajutor-meniu`, `ajutor-cautare`, `ajutor-fereastra-cautare` (meniul se închide la clic în altă parte: Win+Shift+S + «Încarcă»). |
| 0000-23 | **Tururi cu vârf și pe fiecare buton + fereastra de ajutor + capturi estompate** (cererea operatorului, 30.09.2026): bula turului e un «callout» cu vârful pe inel; `part:` pe pașii de tur (`IKBotHelpParts` în Theming, implementat de arbore / grilă / bara de titlu / bara de vederi) — turul trece prin fiecare buton, cel care apare doar sub mouse e aprins pentru exemplu, cel care lipsește se sare; ~60 de pași noi în cele 17 tururi; fereastra de ajutor: istoric pe toată rularea (`HelpHistory`, «Istoric ▾»), «A−/A+» (`AppSettings.HelpTextPercent`), mereu deasupra, se închide singură la modalul altei ferestre; capturile estompează TEXTUL (nu bara / meniul) cu utilizatorul, unitatea, «RO»+cifre, CNP, cod fiscal fără RO (doar în câmpuri / coloane de cod fiscal), e-mail, telefon (`HelpCaptureRedaction`, `IKBotCaptureRedaction`); `Check-Help.ps1` verifică `part:` | GATA pe cod (build **0 avertismente, 0 erori** pe `src\`; verificarea fără erori) / **nevăzut pe ecran** | `SLICE-0000-23-tururi-pe-butoane-istoric-capturi-estompate.md` | Captură de refăcut: `ajutor-fereastra-cautare`. De citit: «Fișiere» din Fundamentare e ascuns în designer, dar `contabil.ddf` încă îl descrie. Categorii noi de estompat: doar cu acordul operatorului. |
| 0000-24 | **Bula turului fără bară de titlu + titlurile din meniul «?»** (cererea operatorului, 30.09.2026): bula nu mai are bară de titlu (numele turului stă în rândul «… · Pasul n din m»), text mai mare (titlu 12,5 pt, text 10,5 pt), 440 px lățime; «gaura» din stânga = Windows 11 rotunjea și încadra tot dreptunghiul ferestrei, peste Region — acum `HelpWindowNative.PlainFrame` (fără rotunjire, fără chenar DWM), după fiecare așezare / afișare / schimbare de temă; în `KBotHelpList` titlurile de parte («Pe ecranul acesta», «Tururi ghidate») au aer deasupra, culoarea accentului și o linie dedesubt; `contabil.ajutor` actualizat | GATA pe cod (build **0 avertismente, 0 erori**; verificarea fără erori) / **nevăzut pe ecran** | `SLICE-0000-24-bula-tur-fara-bara-titluri-meniu.md` | Captură de refăcut: `ajutor-meniu`. Cauza «găurii» e citită din captură, nemăsurată. |
| 0000-25 | **Ajutorul pentru 0097-02** (cererea operatorului, 01.10.2026): dosarul «Adăugare angajamente...» din MENIU și «Creează angajament în FOREXE»; pornirea ferestrei (mărită / turul care pornește singur, «Nu mai arăta turul inițial»); fereastra de conectare fără «?»; în `contabil.forexe.browser` patru secțiuni noi (angajament nou făcut direct în FOREXE, întrebările «Sunteți sigur...?», două recepții pe aceeași dată, mini-meniul K-BOT din pagină); `contabil.setari` (pornirea K-BOT, mini-meniul); pașii din `tur-fereastra`, `tur-ddf`, `tur-setari`; `HELP_SYSTEM.md` §1 | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / **nevăzut pe ecran**, de citit de operator | `SLICE-0000-25-ajutor-pentru-0097-02.md` | Capturi de refăcut: `setari`, `conectare-fereastra`. De citit: ferestrele de confirmare FOREXE și câmpul datei n-au fost văzute — textul e scris din cod. |
| 0000-27 | **Tururile din meniul «?» doar pentru fereastra al cărei «?» a fost apăsat** (cererea operatorului, 01.10.2026): `HelpPopupTours.Collect` primește acea fereastră și ia doar tururile ei și ale vederilor din ea; nicio altă fereastră (nici deținută de ea) nu intră; secțiunea «Meniul «?»» din `contabil/ajutor.md` | GATA (build, verificarea ajutorului) / **nevăzut pe ecran** | `SLICE-0000-27-tururi-doar-fereastra-activa.md` | Fără capturi de refăcut. |
| 0000-26 | **Ajutorul pentru 0056-02** (cererea operatorului, 01.10.2026): `contabil.asocieri.cazuri` («Legături blocate» — doar ordonanțarea blochează, aceeași regulă și după o descărcare; «După o descărcare» — legăturile vechi se corectează în aceeași fereastră, se scriu doar cele mutate, «Golește așezările» le readuce la cum sunt pe server), `contabil.asocieri.fereastra` (rândul blocat, tabelul butoanelor), pasul «Arbore › Asocieri» din `tur-receptii` | GATA (build **0 avertismente, 0 erori**; verificarea fără erori) / **nevăzut pe ecran** | `SLICE-0000-26-ajutor-pentru-0056-02.md` | Fără capturi de refăcut. S-a corectat și «sau o plată» (plățile nu mai blochează din 18.09.2026). |
| 0000-28 | **Ajutorul pentru 0084/02 + 0094-02 și turul ghidat al editorului DDF** (cererea operatorului, 01.10.2026): `contabil.vederi.sumar` (secțiune nouă «Asociază parteneri»), `contabil.ddf.editor` (rândul «Partener asociat» rescris, secțiune nouă «Parteneri»), turul NOU `tur-ddf-editor` (13 pași, pornit din «?» al editorului) și un pas nou în `tur-sumar` | GATA (build **0 avertismente, 0 erori**; verificarea ajutorului fără erori, 18 tururi) / nevăzut pe ecran | `SLICE-0000-28-ajutor-si-tur-ddf-editor.md` | Capturi de refăcut: `ddf-editor`, `sumar`. De citit de operator: secțiunea «Parteneri» e scrisă din cod. |
| 0000-29 | **Ajutorul pentru butonul «Setări» scos din bara de titlu** (cererea operatorului, 01.10.2026; codul e fără felie, vezi SLICELESS): rândurile «Jurnal activitate» și «Configurare K-BOT» din MENIU; `tur-fereastra` (pasul «Bara de titlu › Setări» scos, pasul «Butonul MENIU» actualizat), `contabil.fereastra` (MENIU), `contabil.setari`, `avansat` (cum pornești opțiunile avansate); bifa din Setări › Aplicație s-a redenumit în «Rândul «Jurnal activitate»…» | GATA (build **0 avertismente, 0 erori**; verificarea ajutorului fără erori) / **nevăzut pe ecran** | `SLICE-0000-29-meniu-jurnal-setari.md` | Capturi de refăcut: `fereastra-zone` (și orice poză cu bara de titlu și rotița). |
| 0000-30 | **Tururile arată ce ascunde aplicația și spun cum apare** (cererea operatorului, 01.10.2026): motor nou `IKBotHelpReveal` (`HelpTourRunner`: control ascuns sub părinte vizibil se arată pe durata pasului; `KBotNavList` arată butonul de vedere ascuns; `AdvancedTreeControl` arată «+» și iconița din subsol din imaginile date de `RezervariView` / `PlatiView`), nota «Îl vezi acum doar pentru tur»; pașii și paginile despre ceva condiționat spun acum condiția (tururi fereastră / FOREXE / sumar / DDF / rezervări / plăți / ord / editor DDF / note CAB; `contabil.fereastra` cu tabelul «vederea → când apare», `forexe`, `ddf.rezervare`, `ddf.editor`, `notecab`, `vederi.sumar`, `ajutor`) | GATA (build **0 avertismente, 0 erori**; verificarea ajutorului fără erori) / **nevăzut pe ecran** | `SLICE-0000-30-tururi-arata-ce-e-ascuns.md` | Rândurile MENIU nu se deschid în tur (meniul se închide când bula ia focusul). `btnIstoric` din banda FOREXE nu e afișat niciodată de cod — de decis. Capturi de refăcut: niciuna. |
| 0000-31 | **MENIU deschis de tur, ferestrele mici rămân deschise la captură, ajutor pentru coada robotului** (cererea operatorului, 01.10.2026): pas nou `reveal: menu` (meniul se deschide în tur, cu rândurile ascunse); `KBotPopupGuard` (meniuri, filtrul coloanei, clic dreapta, calendar, liste — nu se închid la clic în afară cât ține captura sau pasul de tur); topic nou `contabil.forexe.coada`, tur nou `tur-coada`, pas nou în `tur-forexe` pentru «Coadă N» (felia 0098 intră în ajutor) | GATA (compilare **0 avertismente, 0 erori**; verificarea ajutorului fără erori) / **nevăzut pe ecran** | `SLICE-0000-31-meniu-in-tur-popupuri-captura-coada.md` | Captură nouă: `coada-robot`. Build normal blocat cât rulează K-BOT / Visual Studio. |
| 0000-32 | **Fereastra de ajutor: o singură pagină continuă, tipărire, export pe bucăți, înghețarea de la «Exportă»** (cererea operatorului, 01.10.2026): pagina din dreapta e TOT ajutorul (`HelpHtml.Book`), derularea trece dintr-un subiect în următorul și cuprinsul selectează subiectul la care s-a ajuns (`tmrScroll`); «Imprimă» (`btnPrint`) și «Exportă» au fiecare un meniu — tot ajutorul / subiectul curent / subiectul cu cele de sub el / capitolul părinte (`HelpScope`); tipărirea e direct din fereastră (`@media print` + `ShowPrintDialog`, fără PDF), «(!) Tot ajutorul» și capitolele de peste 15 subiecte cer confirmare; exportul rămâne HTML; înghețarea: fereastra se închidea singură sub propriul «Save as» care încă nu apăruse (verificarea de 150 ms din 0000-23) — acum dialogurile proprii stau în `OwnDialog()` și verificarea așteaptă până apare un dialog | GATA pe cod (build **0 avertismente, 0 erori**) / **nerulat, nevăzut pe ecran**; cauza înghețării e citită din cod, nedovedită | `SLICE-0000-32-ajutor-pagina-continua-tiparire-export.md` | Captură de refăcut: `ajutor-fereastra-cautare`. Butonul «Capturi...» din fereastra de ajutor a dispărut (lista se deschide din meniul de capturi al ferestrei principale). Antetul / subsolul puse de Windows la tipărire rămân. |
| 0000-36 | **Ajutor pentru 0100-02: «Descărcări multiple» e pagină separată în Setări, vizibilă doar când unitatea o permite** (cererea operatorului, 01.10.2026): `descarcare-multipla` («Cum o pornești»), `setari` (rând nou în tabelul paginilor), `avansat` (fără grupul), pas nou în `tur-setari`; etichetă `0100-02`. Capturi roșii: `setari`, `avansat-activare`, `setari-descarcari-multiple` | GATA (text) / nevăzut pe ecran, nicio captură făcută | `SLICE-0000-36-ajutor-setari-multithread.md` | Capturile cer o unitate cu `Multithread = 1`. |
| 0000-35 | **Ajutor pentru 0089-01: jurnalele serverului cer opțiunile avansate** (cererea operatorului, 01.10.2026): `avansat.jurnale` — pagina «Jurnal» arată jurnalele serverului și alegerea tipului de jurnal doar cu opțiunile avansate pornite; fără ele, doar jurnalele de pe calculator (etichetă `0089-01`). Nicio captură nouă. | GATA | `SLICE-0000-35-ajutor-jurnale-server-avansat.md` | Nicio schimbare de mecanism. |
| 0000-34 | **Ajutor pentru felia 0100 (descărcări pe mai multe taburi) + capturile de refăcut marcate cu roșu** (cererea operatorului, 01.10.2026): subiect nou `contabil.forexe.descarcare-multipla` (pornirea din Setări › Aplicație cu opțiunile avansate, fereastra «Actualizează angajamente», coada FIFO, actualizarea la conectare, angajamentele noi, «Actualizează implicit toate recepțiile»); pagina «Aplicație» regrupată în `contabil.setari`, trimiteri în `avansat`, `descarcare`, `lista`, `coada`; **mecanism nou**: eticheta de captură primește `redo:` + `why:` și lista de capturi pictează cu ROȘU rândurile a căror poză e mai veche decât schimbarea (`HelpCapture.RedoSince`, `HelpCaptureForm.Grid_RowFormatting`); `Check-Help.ps1` cunoaște cheile | GATA pe cod (build **0 avertismente, 0 erori**) / nerulat, nevăzut pe ecran | `SLICE-0000-34-ajutor-descarcari-multiple-capturi-rosii.md` | Capturi de refăcut (roșii): `setari`, `avansat-activare`. Capturi noi, lipsă: `setari-descarcari-multiple`, `arbore-meniu-actualizare`, `actualizare-multipla`. |
| 0000-33 | **Ajutor pentru 0097-03: anul și sursa/sectorul în bara de titlu** (01.10.2026): pașii de tur «Anul de lucru» / «Sursa și sectorul» trec pe `capBar` (`part: year` / `ss`), secțiune nouă «Anul și sursa/sectorul» în `contabil.fereastra`, `contabil.autentificare` spune «bara de titlu»; `Check-Help.ps1` cunoaște părțile `year` și `ss` | GATA (text) / nevăzut pe ecran | `SLICE-0097-03-an-si-sector-in-bara-de-titlu.md` | Captura `fereastra-zone` de refăcut. |

### Ajutorul e la zi până la

**01.10.2026 — codul de azi, până la felia 0097 inclusiv cu a doua ei trecere (0097-02, acoperită în 0000-25), plus corecturile la descărcare din Asocieri (0056-02, acoperite în 0000-26), plus asistentul de ajutor (0000-18…0000-22), tururile pe butoane / fereastra de ajutor (0000-23) și bula fără bară de titlu (0000-24), plus asocierea mai multor parteneri cu un DDF (0084/02 în Sumar și 0094-02 în editor, acoperite în 0000-28, cu turul editorului DDF), plus butonul «Setări» mutat în MENIU (0000-29), plus tururile care arată ce ascunde aplicația și spun cum apare (0000-30), plus meniul în tur, ferestrele mici ținute deschise la captură și coada robotului (0000-31, felia 0098 acoperită), plus fereastra de ajutor cu pagină continuă, tipărire și export pe bucăți (0000-32). Felia 0098 («Coada robotului») NU e încă în ajutor — vezi lista de mai jos**
(`help-version.txt` = `2026-10-01`). Textul a fost scris din codul curent, nu din planuri. Următoarea
actualizare pornește de la feliile de după 0096 și de la lista de mai jos (`docs/HELP_SYSTEM.md` §4).
Mută acest reper la fiecare 0000-NN (și data din `src/KBot.App/HelpContent/help-version.txt`).

### Open threads

- **Ajutor de actualizat** (feliile de funcționalitate adaugă aici ce subiecte / capturi au
  învechit, ex. `0098: contabil.vederi.plati — coloana nouă «Cont»; captura plati de refăcut`):
  - (0097 acoperită în 0000-13; «Reanalizează rezervările» în 0000-23; 0097-02 în 0000-25; 0056-02 în 0000-26; 0084/02 + 0094-02 în 0000-28)
  - `contabil.ddf` — pagina «Fișiere» a vederii Fundamentare e ascunsă în designer (commit «temp»); subiectul încă o descrie. De hotărât de operator (0000-23)
  - 0098: `contabil.forexe.descarcare`, `contabil.fereastra`, `contabil.vederi.receptii`, `contabil.ddf.index` — descărcările / reîmprospătările / trimiterea merg acum prin «Coada robotului» (fereastră nouă + butonul «Coadă N» din subsol; vederile așteaptă cât rulează robotul); captură nouă pentru fereastra cozii

- **0000-21 — ÎNAINTE de a publica un client cu 0000-21:** aplică `sql/0000_21_fx_ajutor_intrebari.sql`
  pe VPS (AVACONT_COMUN) și pune `PYTHON/routes/help_feedback.py` + `PYTHON/main.py` pe server
  (repornire). Altfel întrebările așteaptă în lista locală și trimiterea eșuează la fiecare 3 minute.
- **0000-20 / 0000-21** — meniul «?», stelele și trimiterea întrebărilor nu au fost văzute pe ecran
  și nici rulate (fereastra principală singură; cu Setări deschisă peste ea = două dosare de tururi).
- **0000-25** — de citit de operator: secțiunile noi din `contabil.forexe.browser` (întrebările
  «Sunteți sigur...?», două recepții pe aceeași dată) sunt scrise din cod; paginile FOREXE reale
  n-au fost văzute. Capturi de refăcut când există: `setari`, `conectare-fereastra`.
- **0000-32** — nimic nevăzut pe ecran: pagina continuă (timpul de încărcare cu toate pozele într-un
  singur document), cuprinsul care urmărește derularea, tipărirea (dacă secțiunile lăsate afară chiar
  rămân afară), exportul pe bucăți. Dacă «Exportă» mai îngheață, ipoteza din worklog e greșită și e
  nevoie de stiva firului principal din momentul înghețării. `Check-Help.ps1` mai raportează, din
  afara acestei felii: eticheta `0077-3` din `contabil\fereastra.md` și trei ferestre fără subiect
  (`HelpCaptureViewForm`, `SetariIstoricView`, `UpdateOfferForm`).
- **0000-01** — exportul manualului apăsat de operator la 01.10.2026: a înghețat K-BOT (vezi 0000-32); fereastra Director nevăzută; tema întunecată nevăzută.
- **Observat în trecere:** `000_DEMO` nu are tabela `FX_NoteCAB_Corectii` (eroare 1146 la fiecare pornire, `RefreshUncorrelatedMarkAsync`).

---

## Slice 0001

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0001 | Auth — bearer tokens, session store (Felia 1) | DONE | (pre-worklog-rule) | Static API key eliminated for K-BOT; legacy FOREXE still uses X-Api-Key |

---

## Slice 0002

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0002 | Split-brain 401 fix + reason codes | DONE | (pre-worklog-rule) | login mints via STORE; every 401 carries a reason code |

---

## Slice 0003

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0003 | Redis session backend | DONE (code) / config PENDING on VPS | — | `SESSION_BACKEND="redis"`, `SESSION_KEY_PREFIX`, `REDIS_DB=2` must be set in host config.py; verify DB 2 is free |

---

## Slice 0004

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0004 | Tier 1 hardening | IN PROGRESS | — | rate limiter DONE+pushed; remaining: ApiOptions address, retire AppConfig, verify gunicorn guard. Plan: `KBOT_Tier1_Plan.md` |

---

## Slice 0005

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0005 | Phase A cleanup (lying tests, https guard) | DONE | — | Python 75 passed / 7 skipped, 0 fail/error; .NET 80 green |

---

## Slice 0006

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0006 | MainForm scaffolding | DONE | (pre-worklog-rule) | Plan: `PLAN_MainForm_Scaffolding.md`. Built against its own 11-item checklist; `WithReauth(Of T)` + ListaAngajamente vertical preserved. Real DI signature is 5 params (`forexeRunner, session, apiClient, authApi, loginFactory`), not the 4 the plan expected |

---

## Slice 0007

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0007 | AngajamentTreeInfo POCO correction | SUPERSEDED by 0008 | `SLICE-0008-tree-data-api.md` | Was done WRONG: built against `qFX_MAIN_TREE` alone, so `Salarii` was dropped and `IDORD` kept. 0008 rewrote the POCO against the real contract (row-source `_DESCRIERE` + flags `qFX_MAIN_TREE`): `Salarii` restored, `IDORD` dropped |

---

## Slice 0008

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0008 | Tree data API + `MainForm.LoadTree` | DONE (code) / UNVERIFIED on a live DB | `SLICE-0008-tree-data-api.md`, `SLICE-0009-maintree-loadtree.md` (Part A amendment) | Plan: `PLAN_TreeDataApi.md`. `GET /api/forexe/tree` (an/ss/include_hidden, base from session), nine `EXISTS` flags, POCO rewrite, tree load + nav gating. **Amended by 0009:** the SS filter now has an orphan escape (`EXISTS SS OR NOT EXISTS any indicators`) so zero-indicator angajamente stay visible. **No part of it has touched a real database** — all route tests are host-only and skip off-host |

### Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).

### Open threads

- ~~`KBotNavList` has no `SetItemVisible` — 0008 gates views with `SetItemEnabled`
  (grey-out) instead of hiding.~~ RESOLVED in slice 0018: `SetItemVisible` added and
  `ApplyViewGating` now hides (not greys) a view whose `Are*` flag is FALSE.

---

## Slice 0009

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0009 | `MainForm.LoadTree` (client half) + tree orphan escape | DONE (code) / UNVERIFIED on a live DB | `SLICE-0009-maintree-loadtree.md` | The brief's Parts B/C/D (DTOs, `GetTreeAsync`, `LoadTreeAsync` + gating) were **already shipped by 0008**; the real deltas are Part A (orphan escape on the server, see 0008 row) + its 2 host-only tests, and the 4 `GetTreeAsync` client tests 0008 never added (Api 26 → 30). Kept 0008's choices: mapping in the client (no `BuildTreeInfo`), token from session (no param), `IDDF As Long?` throughout. LoadTree is period-driven (runs on load + every An/SS change = the `SetPeriod` precondition) |
