# K-BOT — noutăți pe versiuni

Aici găsiți schimbările din fiecare versiune, începând cu cea mai nouă.

<!--Textul fiecărei versiuni este cel afișat utilizatorului în fereastra de actualizare, înainte de a apăsa «Da». -->

<!-- Secțiunile sunt adăugate la fiecare `publish-release.ps1` / `push-update.ps1`, conform regulilor din [README.md](README.md). Nu ștergeți marcajele ascunse `<!\-\- release: ... \-\->` și `<!\-\- felii: ... \-\->`: ele stabilesc punctul de la care sunt urmărite schimbările pentru versiunea următoare.-->

## 1.1.1.6 (03.10.2026)
<!-- release: utc=2026-10-03T07:56:21Z -->
<!-- felii: 0105, 0106, 0107, 0000-43 -->

- «Clasificații bugetare»: un nod de deasupra alineatelor arată un sumar, doar pentru citire: clasificația, ultimul buget și totalul rectificărilor.
- Arborele arată implicit doar clasificațiile cu mișcare în an (vreun trimestru de buget sau de rectificare diferit de zero).
- Bifa nouă «Arată toate clasificațiile», de sub arbore, le arată pe toate cele configurate.
- «Verifică bugetul» arată doar clasificațiile la care diferența față de FOREXE nu este zero.
- La actualizarea informațiilor unui angajament nu mai apare «Capturile nu au putut fi trimise: Value cannot be null».
- Clic dreapta în arborele DDF și ORD nu mai reîncarcă documentul PDF; documentul se schimbă doar la clic stânga.
- În arborii DDF, ORD și Asocieri, clic dreapta nu mai alege un rând: dați întâi clic stânga, apoi clic dreapta pentru meniu.
- Ajutor: actualizat pentru clasificațiile bugetare și pentru clicul dreapta în DDF, ORD și Asocieri.

## 1.1.1.5 (02.10.2026)
<!-- release: utc=2026-10-02T07:24:34Z -->
<!-- felii: 0101-02, 0102, 0000-40, SLICELESS-lista-sursa-asociere-denumire-wicket, 0000-39 -->

- În fereastra «Actualizează angajamente», angajamentele apar în aceeași ordine ca în arborele principal.
- «Clasificații bugetare»: bugetul se ține pe versiuni, fiecare cu data de «Început»; nu mai există total pe an.
- Documentul de fundamentare arată bugetul clasificației de la data reviziei, cu rectificările până atunci, nu pe cel de azi.
- Dacă la data reviziei clasificația nu are buget, se folosește creditul bugetar de azi și un mesaj o spune.
- Fereastra de asociere a recepțiilor arată în titlu și denumirea angajamentului.
- Cu arborele sortat după dată, «Lista de angajamente» aduce toate sursele unității, nu doar sursa aleasă.
- După o descărcare pe mai multe taburi, citirea următoare a listelor nu se mai oprește la așteptarea paginilor.
- Ajutor: actualizat pentru clasificațiile bugetare, editorul DDF, lista de angajamente și asocieri.

## 1.1.1.4 (02.10.2026)
<!-- release: utc=2026-10-02T06:38:48Z -->

<!-- felii: 0101, 0101-01, 0000-37, 0000-38 -->

- În arborele principal, angajamentele ale căror recepții nu se închid (ultimul instantaneu nu are valoarea recepției) apar cu roșu; indicația arată care recepții și ce valori.
- În fereastra de asociere a recepțiilor, recepția și ultimul ei instantaneu se scriu cu roșu când valorile nu coincid.
- În Plăți, «+» de la o lună generează ordonanțările după confirmare, fără mesajul de succes de la final.
- În vederile Ordonanțare și Fundamentare, bara cu pagini rămâne și pe o lună sau pe «Toate…»: «Vizualizare» arată valorile, iar pagina «Documente» arată lista fișierelor, de generat și de tipărit.
- Ajutor: actualizări în paginile despre fereastra principală, asocierea recepțiilor, ordonanțări și fundamentare.

## 1.1.1.3 (02.10.2026)

<!-- release: utc=2026-10-01T21:00:00Z -->
<!-- felii: 0100, 0100-02, 0000-34, 0000-36 -->
<!-- git: 4aae6bd -->

- Descărcări multiple: mai multe angajamente pot fi actualizate simultan.
- Din meniul arborelui, «Actualizează angajamente...» deschide o fereastră în care bifați angajamentele; se pot bifa și cele neactualizate de un anumit număr de zile.
- La conectarea la FOREXE, K-BOT poate actualiza singur angajamentele vechi, iar după o reîmprospătare a listei vă întreabă: «Dorești actualizarea angajamentelor noi?».
- Indicația de la un angajament arată acum și data ultimei actualizări («Actualizat: ...»).
- Setări › «Descărcări multiple» este o pagină separată, afișată doar dacă unitatea permite această funcție; numărul de angajamente descărcate simultan este limitat de unitate.
- Ajutor: pagină nouă, «Mai multe descărcări deodată», și actualizări în paginile despre Setări și despre coadă.

## 1.1.1.2 (01.10.2026)

<!-- release: utc=2026-10-01T15:54:33Z -->
<!-- felii: 0777-02, 0097-03, 0078-09, 0078-10, 0099, 0000-30, 0000-31, 0000-32, 0000-33, 0072-03, 0089-01, 0000-35 -->
<!-- git: 714b886 -->

- În arborele principal sunt evidențiate elementele descărcate în sesiunea curentă. Dacă faceți mai multe actualizări la rând, fereastra de recepții nu mai este afișată de fiecare dată.
- Anul și «Sursă/Sector» apar acum în bara de titlu, lângă unitate, în loc să fie afișate dedesubt.
- Adobe: la închidere, întrebarea «Salvați modificările?» primește automat răspunsul «Nu». Opțiunea «Adobe pornește în interfața clasică» a revenit în Setări › Documente.
- Ajutor: tururile pot afișa și elementele ascunse și explică în ce situații apar. Au fost adăugate un tur și o pagină pentru coada robotului, iar fereastra de ajutor poate fi derulată continuu și permite imprimarea sau exportarea pe secțiuni.
- Pagina «FOREXE» din Setări nu mai dă eroare la deschidere.
- În Setări › Jurnal, jurnalele de server și lista cu tipul jurnalului apar doar cu «Opțiuni avansate» activate; fără ele rămân doar jurnalele de pe acest calculator.

## 1.1.1.1 (01.10.2026)

<!-- release: utc=2026-10-01T07:12:58Z -->
<!-- felii: 0056-02, 0084-02, 0094-02, 0000-26, 0000-27, 0000-28, 0000-29 -->
<!-- git: b3325b1 -->

- La o descărcare nouă puteți modifica și legăturile recepțiilor mai vechi, atât timp cât pentru acestea nu există o ordonanțare.
- În vederea «Rezervări», pictograma de acțiuni din partea de jos a arborelui apare doar după ce nu mai există rezervări marcate cu «+», adică rezervări care trebuie aduse din FOREXE.
- În «Sumar», butonul «Asociază parteneri», disponibil doar pentru angajamentele cu DDF, permite asocierea unuia sau mai multor parteneri cu documentul.
- Editorul DDF are o pagină nouă, «Parteneri», în partea dreaptă a barei de pagini. Partenerul din antet rămâne partenerul principal.
- Meniul «?» afișează doar tururile disponibile pentru fereastra din care a fost deschis. Editorul DDF are acum propriul tur ghidat.
- Butonul cu rotița din bara de titlu a ferestrei principale a fost eliminat. Setările se deschid acum din «MENIU › Configurare K-BOT».
- În «MENIU» a fost adăugată opțiunea «Jurnal activitate», pentru jurnalele K-BOT. Aceasta poate fi ascunsă din Setări › Aplicație.
- O legătură este blocată doar de existența unei ordonanțări, începând cu data acesteia, nu și de o plată. Ajutorul a fost actualizat pentru a reflecta această regulă.

## 1.1.1.0 (01.10.2026)

<!-- release: utc=2026-10-01T06:08:58Z -->
<!-- felii: 0097-02, 0097, 0098, 0098-02, 0096, 0095, 0095-02, 0093, 0094, 0078-07, 0078-08, 0000-01, 0000-02, 0000-03, 0000-04, 0000-05, 0000-06, 0000-07, 0000-08, 0000-09, 0000-10, 0000-11, 0000-12, 0000-13, 0000-14, 0000-15, 0000-16, 0000-17, 0000-18, 0000-19, 0000-20, 0000-21, 0000-22, 0000-23, 0000-24, 0000-25 -->
<!-- git: cd41f68 -->

- Turul ferestrei principale pornește automat la deschiderea aplicației și poate fi dezactivat sau reactivat din Setări.
- În «MENIU» a fost adăugat dosarul «Adăugare angajamente...», care conține opțiunea «Creează angajament în FOREXE».
- În pagina FOREXE, întrebările de tip «Sunteți sigur...?» primesc automat răspunsul «Da». Dacă introduceți o a doua recepție cu aceeași dată, K-BOT vă cere confirmarea.
- Setări: fereastra principală poate porni maximizată, iar mini-meniul K-BOT din pagina FOREXE poate fi ascuns.
- Unitatea poate fi schimbată direct din bara de titlu, iar conexiunea FOREXE se închide automat la schimbare. Reînnoirea unei sesiuni expirate este acum mai discretă.
- Un document semnat nu mai poate fi regenerat. «Note corecție» este afișat doar atunci când angajamentul conține note.
- Cât timp rulează robotul FOREXE, cererile de actualizare sunt puse într-o coadă. Coada are o fereastră proprie, din care puteți pune procesarea pe pauză sau o puteți anula.
- ORD și DDF au noduri noi: «Toate ordonanțările» și «Toate reviziile». Este disponibilă și ștergerea pe lună, fără mesaj suplimentar după ștergere.
- Istoric are acum nodul «Tot istoricul». Extrase are un meniu propriu de afișare și se deschide din «MENIU › Extrase».
- Parteneri: codul fiscal este unic, datele pot fi completate din ANAF, banca este identificată automat pe baza IBAN-ului, iar formularul include și câmpul «Adresa».
- Arborii așteaptă deschiderea documentului în Adobe înainte de a continua, iar fereastra Adobe rămâne în panoul în care este afișată.
- Ajutor: F1 și «?» oferă căutare după întrebări, tururi ghidate pentru ferestre și un manual care poate fi exportat.

## 1.1.0.6 (29.09.2026)

<!-- release: utc=2026-09-29T13:11:47Z baseline -->
<!-- felii: -->

Versiunea de la care începe acest jurnal, aflată pe server la 01.10.2026. Nu are o listă de schimbări.
