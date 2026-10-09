# K-BOT — noutati pe versiuni

Aici gasiti schimbarile din fiecare versiune, incepand cu cea mai noua.

<!--Textul fiecarei versiuni este cel afisat utilizatorului in fereastra de actualizare, inainte de a apasa «Da». -->

<!-- Sectiunile sunt adaugate la fiecare `publish-release.ps1` / `push-update.ps1`, conform regulilor din [README.md](README.md). Nu stergeti marcajele ascunse `<!\-\- release: ... \-\->` si `<!\-\- felii: ... \-\->`: ele stabilesc punctul de la care sunt urmarite schimbarile pentru versiunea urmatoare.-->

## 1.1.1.9 (08.10.2026)

<!-- release: utc=2026-10-08T05:00:33Z -->
<!-- felii: 000T-05, 000T-06, 000T-07, 000T-09, 000T-10, 0111-01, 0078-13, 0078-14, 0078-15, 00EF-08, 00EF-09, 00EF-12, 00EF-13, 00EF-14, 0000-51, 0000-53, 0000-54, 0000-55, 0000-56, 0000-57, 0000-58 -->

- Meniul «E-Factura» are o fereastra noua pentru facturile emise: clientii in arbore, facturile sub ei, cu pictograma starii.
- Din meniul unei facturi: trimitere la ANAF, validare, eroarea ANAF, factura clasica sau cea ANAF, stornare, modificare.
- «Date Unitate» e o fereastra separata (denumire, cod fiscal, adresa, serie, primul numar); «Preia de la ANAF» le completeaza.
- Conturile bancare ale unitatii se tin intr-o lista proprie; banca se stabileste singura din IBAN.
- In fereastra de asociere a receptiilor se poate corecta valoarea unei receptii gresite, cu motiv obligatoriu.
- Tutorialele: 14 tutoriale noi (conectare la FOREXE, extrase, clasificatii, parteneri, actualizare angajamente etc.).
- Un tutorial poate trimite la altul printr-o legatura; mesajele tutorialului apar deasupra cardului de pas.
- La pornire apare un mic tutorial despre tutoriale; se opreste din «Setari», «Arata tutorialul de inceput la pornirea K-BOT».
- Documentele PDF din vizualizatorul incorporat se deschid mai sigur, fara sa mai ramana goale pana la un clic.
- La un Adobe mai vechi de 2025, K-BOT avertizeaza la pornire si recomanda Adobe Acrobat Reader 2025 sau mai nou.
- La salvarea unui PDF peste un fisier existent, intrebarea «Replace existing file?» primeste raspuns singura.
- Ajutor: actualizat pentru facturile emise, conturi, date unitate si corectarea valorii unei receptii.

## 1.1.1.8 (04.10.2026)

<!-- release: utc=2026-10-04T07:30:32Z -->
<!-- felii: 000T-01, 000T-02, 000T-03, 000T-04, 0000-50 -->

- «?» are un grup nou, «Tutoriale»: te conduc pe ecran, pas cu pas, chiar prin mai multe ferestre.
- Primul tutorial: «Adauga o revizie pe baza unei rezervari existente», pana la «Salveaza documentul».
- Tutorialul asteapta sa faci tu fiecare pas: ce ai de facut are chenar, restul ferestrei e intunecat.
- K-BOT nu apasa nimic in locul tau; cand ai facut pasul, trece singur la urmatorul.
- Cand apesi ceva care deschide o fereastra, tutorialul continua in fereastra noua.
- Pasii optionali au butonul «Sari peste» si spun de ce nu sunt obligatorii.
- Daca faci altceva decat pasul cerut, K-BOT intreaba «Vrei sa iesi din tutorial?»; «Nu» ramane pe acelasi pas.
- Il gasesti si scriind in cautarea din «?» ce vrei sa faci, de exemplu «cum adaug o revizie»: apare primul.
- Ajutor: pagina «Cum folosesti ajutorul» descrie tutorialele.

## 1.1.1.7 (03.10.2026)

<!-- release: utc=2026-10-03T12:24:10Z -->
<!-- felii: 0107-02, 0107, 0000-44, 0000-46, 0000-47, 0100-03, 0000-48 -->

- «Clasificatii bugetare»: cele doua tabele au coloane de aceeasi latime; cel de sus arata si clasificatia.
- Bugetul are o coloana «Total» pe fiecare rand; totalul rectificarilor apare doar la o clasificatie aleasa.
- Sub cele doua tabele, un rand nou aduna ultimul buget (cel cu data cea mai noua) cu toate rectificarile.
- Bifa noua «Arata DOAR clasificatiile folosite in FOREXE», in dreapta bifei «Arata toate clasificatiile».
- «Verifica bugetul» verifica doar clasificatiile folosite in FOREXE, adica cele cu credit raportat de FOREXE.
- In «Verificare buget FOREXE», dublu clic pe un rand inchide fereastra si alege clasificatia in arbore.
- La descarcarea mai multor angajamente, «Coada robotului» arata un rand cu bara de progres pentru fiecare.
- «X» de pe un rand opreste doar descarcarea acelui angajament; celelalte merg mai departe.
- Cele care asteapta un tab apar dedesubt, intr-o lista, fiecare cu «X» care il scoate din descarcare.
- Daca nicio descarcare nu a reusit, mesajul «urmeaza ingestia» nu mai ramane in bara de stare.

## 1.1.1.6 (03.10.2026)

<!-- release: utc=2026-10-03T07:56:21Z -->
<!-- felii: 0105, 0106, 0107, 0000-43 -->

- «Clasificatii bugetare»: un nod de deasupra alineatelor arata un sumar, doar pentru citire: clasificatia, ultimul buget si totalul rectificarilor.
- Arborele arata implicit doar clasificatiile cu miscare in an (vreun trimestru de buget sau de rectificare diferit de zero).
- Bifa noua «Arata toate clasificatiile», de sub arbore, le arata pe toate cele configurate.
- «Verifica bugetul» arata doar clasificatiile la care diferenta fata de FOREXE nu este zero.
- La actualizarea informatiilor unui angajament nu mai apare «Capturile nu au putut fi trimise: Value cannot be null».
- Clic dreapta in arborele DDF si ORD nu mai reincarca documentul PDF; documentul se schimba doar la clic stanga.
- In arborii DDF, ORD si Asocieri, clic dreapta nu mai alege un rand: dati intai clic stanga, apoi clic dreapta pentru meniu.
- Ajutor: actualizat pentru clasificatiile bugetare si pentru clicul dreapta in DDF, ORD si Asocieri.

## 1.1.1.5 (02.10.2026)

<!-- release: utc=2026-10-02T07:24:34Z -->
<!-- felii: 0101-02, 0102, 0000-40, SLICELESS-lista-sursa-asociere-denumire-wicket, 0000-39 -->

- In fereastra «Actualizeaza angajamente», angajamentele apar in aceeasi ordine ca in arborele principal.
- «Clasificatii bugetare»: bugetul se tine pe versiuni, fiecare cu data de «Inceput»; nu mai exista total pe an.
- Documentul de fundamentare arata bugetul clasificatiei de la data reviziei, cu rectificarile pana atunci, nu pe cel de azi.
- Daca la data reviziei clasificatia nu are buget, se foloseste creditul bugetar de azi si un mesaj o spune.
- Fereastra de asociere a receptiilor arata in titlu si denumirea angajamentului.
- Cu arborele sortat dupa data, «Lista de angajamente» aduce toate sursele unitatii, nu doar sursa aleasa.
- Dupa o descarcare pe mai multe taburi, citirea urmatoare a listelor nu se mai opreste la asteptarea paginilor.
- Ajutor: actualizat pentru clasificatiile bugetare, editorul DDF, lista de angajamente si asocieri.

## 1.1.1.4 (02.10.2026)

<!-- release: utc=2026-10-02T06:38:48Z -->

<!-- felii: 0101, 0101-01, 0000-37, 0000-38 -->

- In arborele principal, angajamentele ale caror receptii nu se inchid (ultimul instantaneu nu are valoarea receptiei) apar cu rosu; indicatia arata care receptii si ce valori.
- In fereastra de asociere a receptiilor, receptia si ultimul ei instantaneu se scriu cu rosu cand valorile nu coincid.
- In Plati, «+» de la o luna genereaza ordonantarile dupa confirmare, fara mesajul de succes de la final.
- In vederile Ordonantare si Fundamentare, bara cu pagini ramane si pe o luna sau pe «Toate…»: «Vizualizare» arata valorile, iar pagina «Documente» arata lista fisierelor, de generat si de tiparit.
- Ajutor: actualizari in paginile despre fereastra principala, asocierea receptiilor, ordonantari si fundamentare.

## 1.1.1.3 (02.10.2026)

<!-- release: utc=2026-10-01T21:00:00Z -->
<!-- felii: 0100, 0100-02, 0000-34, 0000-36 -->
<!-- git: 4aae6bd -->

- Descarcari multiple: mai multe angajamente pot fi actualizate simultan.
- Din meniul arborelui, «Actualizeaza angajamente...» deschide o fereastra in care bifati angajamentele; se pot bifa si cele neactualizate de un anumit numar de zile.
- La conectarea la FOREXE, K-BOT poate actualiza singur angajamentele vechi, iar dupa o reimprospatare a listei va intreaba: «Doresti actualizarea angajamentelor noi?».
- Indicatia de la un angajament arata acum si data ultimei actualizari («Actualizat: ...»).
- Setari › «Descarcari multiple» este o pagina separata, afisata doar daca unitatea permite aceasta functie; numarul de angajamente descarcate simultan este limitat de unitate.
- Ajutor: pagina noua, «Mai multe descarcari deodata», si actualizari in paginile despre Setari si despre coada.

## 1.1.1.2 (01.10.2026)

<!-- release: utc=2026-10-01T15:54:33Z -->
<!-- felii: 0777-02, 0097-03, 0078-09, 0078-10, 0099, 0000-30, 0000-31, 0000-32, 0000-33, 0072-03, 0089-01, 0000-35 -->
<!-- git: 714b886 -->

- In arborele principal sunt evidentiate elementele descarcate in sesiunea curenta. Daca faceti mai multe actualizari la rand, fereastra de receptii nu mai este afisata de fiecare data.
- Anul si «Sursa/Sector» apar acum in bara de titlu, langa unitate, in loc sa fie afisate dedesubt.
- Adobe: la inchidere, intrebarea «Salvati modificarile?» primeste automat raspunsul «Nu». Optiunea «Adobe porneste in interfata clasica» a revenit in Setari › Documente.
- Ajutor: tururile pot afisa si elementele ascunse si explica in ce situatii apar. Au fost adaugate un tur si o pagina pentru coada robotului, iar fereastra de ajutor poate fi derulata continuu si permite imprimarea sau exportarea pe sectiuni.
- Pagina «FOREXE» din Setari nu mai da eroare la deschidere.
- In Setari › Jurnal, jurnalele de server si lista cu tipul jurnalului apar doar cu «Optiuni avansate» activate; fara ele raman doar jurnalele de pe acest calculator.

## 1.1.1.1 (01.10.2026)

<!-- release: utc=2026-10-01T07:12:58Z -->
<!-- felii: 0056-02, 0084-02, 0094-02, 0000-26, 0000-27, 0000-28, 0000-29 -->
<!-- git: b3325b1 -->

- La o descarcare noua puteti modifica si legaturile receptiilor mai vechi, atat timp cat pentru acestea nu exista o ordonantare.
- In vederea «Rezervari», pictograma de actiuni din partea de jos a arborelui apare doar dupa ce nu mai exista rezervari marcate cu «+», adica rezervari care trebuie aduse din FOREXE.
- In «Sumar», butonul «Asociaza parteneri», disponibil doar pentru angajamentele cu DDF, permite asocierea unuia sau mai multor parteneri cu documentul.
- Editorul DDF are o pagina noua, «Parteneri», in partea dreapta a barei de pagini. Partenerul din antet ramane partenerul principal.
- Meniul «?» afiseaza doar tururile disponibile pentru fereastra din care a fost deschis. Editorul DDF are acum propriul tur ghidat.
- Butonul cu rotita din bara de titlu a ferestrei principale a fost eliminat. Setarile se deschid acum din «MENIU › Configurare K-BOT».
- In «MENIU» a fost adaugata optiunea «Jurnal activitate», pentru jurnalele K-BOT. Aceasta poate fi ascunsa din Setari › Aplicatie.
- O legatura este blocata doar de existenta unei ordonantari, incepand cu data acesteia, nu si de o plata. Ajutorul a fost actualizat pentru a reflecta aceasta regula.

## 1.1.1.0 (01.10.2026)

<!-- release: utc=2026-10-01T06:08:58Z -->
<!-- felii: 0097-02, 0097, 0098, 0098-02, 0096, 0095, 0095-02, 0093, 0094, 0078-07, 0078-08, 0000-01, 0000-02, 0000-03, 0000-04, 0000-05, 0000-06, 0000-07, 0000-08, 0000-09, 0000-10, 0000-11, 0000-12, 0000-13, 0000-14, 0000-15, 0000-16, 0000-17, 0000-18, 0000-19, 0000-20, 0000-21, 0000-22, 0000-23, 0000-24, 0000-25 -->
<!-- git: cd41f68 -->

- Turul ferestrei principale porneste automat la deschiderea aplicatiei si poate fi dezactivat sau reactivat din Setari.
- In «MENIU» a fost adaugat dosarul «Adaugare angajamente...», care contine optiunea «Creeaza angajament in FOREXE».
- In pagina FOREXE, intrebarile de tip «Sunteti sigur...?» primesc automat raspunsul «Da». Daca introduceti o a doua receptie cu aceeasi data, K-BOT va cere confirmarea.
- Setari: fereastra principala poate porni maximizata, iar mini-meniul K-BOT din pagina FOREXE poate fi ascuns.
- Unitatea poate fi schimbata direct din bara de titlu, iar conexiunea FOREXE se inchide automat la schimbare. Reinnoirea unei sesiuni expirate este acum mai discreta.
- Un document semnat nu mai poate fi regenerat. «Note corectie» este afisat doar atunci cand angajamentul contine note.
- Cat timp ruleaza robotul FOREXE, cererile de actualizare sunt puse intr-o coada. Coada are o fereastra proprie, din care puteti pune procesarea pe pauza sau o puteti anula.
- ORD si DDF au noduri noi: «Toate ordonantarile» si «Toate reviziile». Este disponibila si stergerea pe luna, fara mesaj suplimentar dupa stergere.
- Istoric are acum nodul «Tot istoricul». Extrase are un meniu propriu de afisare si se deschide din «MENIU › Extrase».
- Parteneri: codul fiscal este unic, datele pot fi completate din ANAF, banca este identificata automat pe baza IBAN-ului, iar formularul include si campul «Adresa».
- Arborii asteapta deschiderea documentului in Adobe inainte de a continua, iar fereastra Adobe ramane in panoul in care este afisata.
- Ajutor: F1 si «?» ofera cautare dupa intrebari, tururi ghidate pentru ferestre si un manual care poate fi exportat.

## 1.1.0.6 (29.09.2026)

<!-- release: utc=2026-09-29T13:11:47Z baseline -->
<!-- felii: -->

Versiunea de la care incepe acest jurnal, aflata pe server la 01.10.2026. Nu are o lista de schimbari.
