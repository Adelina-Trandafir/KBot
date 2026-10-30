# K-BOT — noutati pe versiuni

Aici gasiti schimbarile din fiecare versiune, incepand cu cea mai noua.

<!--Textul fiecarei versiuni este cel afisat utilizatorului in fereastra de actualizare, inainte de a apasa «Da». -->

<!-- Sectiunile sunt adaugate la fiecare `publish-release.ps1` / `push-update.ps1`, conform regulilor din [README.md](README.md). Nu stergeti marcajele ascunse `<!\-\- release: ... \-\->` si `<!\-\- felii: ... \-\->`: ele stabilesc punctul de la care sunt urmarite schimbarile pentru versiunea urmatoare.-->

## 1.1.3.1 (29.10.2026)
<!-- felii: AV01-17, 0010-08, 0000-138, 0000-139, 0000-140 -->

- «Sabloane»: la operatiile cu clasificatie tabelul arata coloana «Forma»; formele fara operatie apar ca randuri de completat la «Modifica».
- «Sabloane»: o linie se salveaza doar daca are si contul debit, si contul credit; altfel randul se anuleaza.
- «Sabloane»: listele de conturi arata planul de conturi al sursei, cu analitic si cu conturile clasificatiei liniei; listele se deschid mai late.
- «Sabloane», operatii diverse: tabelul ocupa toata latimea, are clasificatie pe linie, bifele O/P/C si optiunea «VARIABIL» pentru partener.
- «Sabloane», operatii cu clasificatie: butonul X de la capatul unui rand goleste randul si pastreaza forma. «Salveaza» e mai scurt.
- In toate tabelele in care scrieti: Esc renunta la ce ati scris in casuta; al doilea Esc, imediat dupa, anuleaza tot randul (unul nou dispare).

## 1.1.3.0 (24.10.2026)
<!-- release: utc=2026-10-24T08:25:26Z -->

<!-- felii: 0081-13, 0097-05, 00EF-49, 0114-27, 0123-05, AV01-04, AV01-06, 0000-119, 0000-121, 0000-122, 0000-123, 0000-124, 00EF-50, 0000-129, 0067-03, 00EF-51, 00EF-52 -->

- DDF: programul vine din SS-ul indicatorilor (01A = 0000002510, altfel 0000000000); un angajament manual nou n-are program.
- Fara program, DDF-ul nu se salveaza si nu se trimite; cele salvate gresit se repara cu «Repara programul» (ribbon).
- In lista angajamentelor, programul gresit in DDF are un rosu nou; la trecerea mouse-ului vedeti ce nu se potriveste.
- Angajament nou («+»): nu se poate cat timp exista angajamente cu rezervari, dar fara DDF; mesajul le numeste.
- Editorul DDF, sectiunea A: grila are mereu un rand gol; «Sterge rand» e buton in grila; bugetul se completeaza singur.
- «Clasificatie» e un arbore doar cu clasificatiile «Programului» ales.
- «Trimite la Vercon»: «Cont pentru toate liniile» pune acelasi cont peste tot; se bifeaza un singur furnizor.
- Filele VERCON si ADECHIT apar doar daca unitatea are date.
- O setare salvata se aplica si fara optiuni avansate; bifa de reafisare a autentificarii nu se mai intoarce pe bifat.
- «Plan de conturi» (Configurari > Date): planul urmeaza sursa din bara de titlu; cu mai multe subunitati, lista «Subunitate».
- In «Plan de conturi», «Afiseaza solduri initiale» si «Exporta in Excel» sunt in ribbon; conturile inactive, in optiunile arborelui.
- Facturi de vanzare, «Continut»: tabelul are mereu un rand gol la sfarsit; «Linie noua» a disparut. Fiecare linie are «Duplica» si «Sterge»; Tab trece prin Um, Cant, PU si apoi pe randul urmator.
- Facturi de vanzare: butoanele din josul ferestrei («Renunta», «Modifica», «Adauga», «Salveaza») arata si pictograma, si textul.
- «Cauta actualizari» (Setari): daca aveti deja ultima versiune, puteti totusi descarca din nou pachetul de pe server si actualiza cu el.
- Facturi de vanzare, «Cont emitent (IBAN)»: lista arata conturile unitatii; un cont nou scris acolo se adauga la «Conturi Unitate» la salvare.
- Ajutor: subiecte noi despre programul DDF, contul pentru toate liniile, «Plan de conturi» si noua intrebare de la actualizare.

## 1.1.2.7 (21.10.2026)
<!-- release: utc=2026-10-23T08:09:17Z -->

<!-- felii: 00EF-45, 00EF-46, 00EF-47, 00EF-48, 0114-24, 0114-25, 0114-26, 0000-116, 0000-117, 0000-118, 0000-119, 0000-120, 0123-01, 0123-02, 0123-03, 0123-04 -->

- Fila noua «VERCON» in ribbon, cu «Rapoarte» si «Trimite la Vercon» (mutat aici din fila E-Factura).
- VERCON «Rapoarte»: alegeti subunitatea, gestiunea, anul, perioada (an sau luna) si datele; apoi PDF sau Excel.
- Fila noua «ADECHIT» cu «Rapoarte»: situatie lunara, de plata, registru casa, raport banca, fisa cont, anulate.
- Rapoartele ADECHIT si VERCON se vad ca PDF in pagina si se pot salva ca Excel, cu toate coloanele.
- «Trimite la Vercon»: arbore cu bife luna > furnizor > factura; in dreapta produsele, cu «Cont Vercon» propus.
- Lista de conturi are grupe pe gestiuni care se strang si se desfac; cautarea gaseste contul si dupa gestiune.
- Fiecare factura arata starea: trimisa, preluata (nu se mai schimba) sau returnata, cu motiv; se poate retrimite.
- Eroarea la descarcarea facturii de la ANAF spune acum ce «id descarcare» s-a folosit.
- Rezervari: «Repara...» se deschide si pentru lunile cu revizii; o rezervare legata gresit se elibereaza (+).
- In pagina FOREXE nu incepeti o rezervare noua cat timp alta asteapta revizia; mesajul arata zilele.
- Lista angajamentelor: biletul de pe rand e un tabel cu litere mai mari si sta pana mutati mouse-ul.
- Ajutor: subiecte noi despre rapoartele VERCON si ADECHIT, trimiterea la Vercon si rezervarile legate gresit.

## 1.1.2.6 (21.10.2026)
<!-- release: utc=2026-10-21T09:08:28Z -->

<!-- felii: CHAT-08, CHAT-09, KE17, 0000-115, 00EF-45, 00EF-46, 00EF-48, VC06-07, VC06-08, 0000-117, 0000-118 -->

- E-Factura: buton nou «Trimite la Vercon», o pagina cu facturile primite, ca sa le preia Administratorul intr-un NIR.
- Facturile sunt intr-un arbore pe luni si furnizori; «Grupare dupa furnizor» e in optiunile arborelui.
- Bifa pe o luna sau pe un furnizor le bifeaza pe toate facturile lui; in dreapta apare un grup pentru fiecare factura.
- Se trimit toate liniile; pe fiecare linie puteti propune contul din Vercon (lista pe gestiuni), doar ca propunere.
- O factura trimisa care nu a fost preluata se poate retrage sau i se pot schimba conturile; cea returnata se retrimite.
- In VERCON, la «Adaugare NIR», butonul «NIR din e-Factura...»: Administratorul preia factura sau o returneaza cu motiv.
- Acasa: robotul K-BOT din colt se leagana si arata un numar rosu cand sunt mesaje noi; clic pe el deschide «Discutii».
- «Discutii»: mesajele dintre utilizatorii K-BOT, VERCON si ADECHIT; scrieti cu pseudonim sau cu nume, Enter trimite.
- In ele: «Raspunde» sub un mesaj, «Afiseaza» alege aplicatiile; un clic in afara balonului il inchide.
- Propuneri de legare: «Angajament» de ales cand factura are mai multe si «Receptie» (suma luata, poate fi mai mica).
- Receptiile sunt in doua grupe, cu aceeasi valoare si neasociate integral; coloana «Ramas» arata ce mai ramane din ele.
- Ajutorul: subiecte noi despre discutii si despre trimiterea la Vercon, cu un tutorial.

## 1.1.2.5 (20.10.2026)

<!-- felii: 0114-04, 0114-05, 0114-06, 0114-07, 0114-08, 0114-09, 0114-10, 0114-11, 0114-12, 0114-13, 0114-14, 0114-15, 0114-16, 0114-17, 0114-18, 0114-19, 0114-20, 0114-21, 0114-22, 0114-23, 0115-01, 00EF-33, 00EF-38, 00EF-39, 00EF-40, 00EF-41, 00EF-42, 0000-95, 0000-96, 0000-97, 0000-98, 00EF-43, 0114-24, 0115-02, KE09, KE10, KE11, KE12, KE12-02, KE12-03, KE12-04, KE13, KE14, KE15, KE16, 0114-25, 0097-04, 0121-03, 0121-04, 0121-05, 000T-13, 0000-107, 0000-108, 0000-109, 0000-110, 0000-111, 0000-112, 0000-113, 0000-114 -->

- Ribbon: filele sunt «Configurari», «Angajamente» (la pornire), «E-Factura», «Extrase»; «Date» e grup.
- «Descarca extrase» e doar in ribbon, in grupul din dreapta; nu mai e in «Extrase de cont» si in arbori.
- Grupul din dreapta tine de fereastra: «Editeaza grupe», «Sincronizeaza facturi primite» cu campurile lui.
- «Sincronizeaza facturi primite»: lista de zile se deschide, al doilea camp e casuta de text; fara alegere, 7 zile.
- Lista angajamentelor: «Actualizeaza» din subsol are meniu: «Descarca angajamente NOI» / «...VRAC».
- Randul «Grupe» apare doar daca exista grupe; butonul E-Factura din bara vederilor sta sub «Extrase».
- Rezervari: «Grafice», butonul documentului si «Repara...» sunt in ribbon; arborele nu mai are butoane sus si jos.
- Receptii: «Asociaza receptii» si «Reface din istoric...» sunt in ribbon; in arbore ramane «Actualizeaza».
- Vederea E-Factura a angajamentului: facturile legate de DDF, pe luni; fara bifa «arata tot» si fara furnizor.
- E-Factura Vanzare: fara lista de luni, randurile din arbore nu mai sunt taiate, «Atasamente» e in «Continut».
- Facturi primite: fara banda goala de jos; «Actualizeaza» din subsol are iconita listei; arbori mai ingusti.
- Ferestrele modale se maximizeaza in zona paginilor. Panoul din stanga: buton de strangere jos, doar actiunile de azi.
- Facturi primite: «Toate facturile» fara numar, lunile fara an (anul apare doar daca sunt mai multi ani).
- Clic pe «Toate facturile»: lista cu bife, «Listeaza», «Salveaza XML», «...» pe rand si «Receptie asociata».
- Clasificatia liniilor se alege in «Leaga de receptie»; butonul «Clasificatii pe linii» nu mai exista.
- Facturi primite, doar cu Access: «Trimite in Access» si «Preia din Access», in grupul din dreapta al ribbonului.
- Ribbon strans: un clic pe o fila ii arata butoanele doar cat ai nevoie, peste fereastra, fara sa mute paginile; dublu clic il desface.
- Setari > Aplicatie: «Panglica din fereastra principala e compacta» micsoreaza iconitele si textul din ribbon, pentru ecrane mici.
- Pagina noua «Receptii» (ribbon, fila E-Factura, dupa «Cumparare»): bifati facturi si faceti o receptie pentru toate.
- In ea alegeti: «Receptie noua», «Leaga de o receptie existenta» sau «Modifica o receptie existenta».
- Arborele ei are cautare, porneste inchis, aduce facturile noi primele; produsele au casete de bifat (debifat = 0).
- In vederile facturilor primite, formularul receptiei ia locul tabelului de legaturi; «Scoate legatura» nu mai exista.
- Receptie noua: suma si data se scriu cum le cere FOREXE; daca le respinge, vedeti mesajul lui si nu se salveaza nimic.
- Setari > Aplicatie: «Meniul lateral sta in spatele machetelor» (bifat implicit) ascunde panoul din stanga cand e o pagina.
- Pagina «Receptii»: arborele arata toate facturile; cele asociate nu se mai bifeaza si au «Elimina asociere».
- O factura rezolvata arata doar receptia ei si produsele, fara modificari; optiune: «Arata facturile asociate».
- Sumele se scriu in formatul din Windows (77.49 sau 77,49); mesajul de rest arata unde mai are receptia rest.
- Legare in bloc: la un furnizor, fereastra cu PROPUNERI (receptie = factura, pana la 2 zile dupa); nu leaga singura.
- Angajamentul ales pentru un furnizor se tine minte pe codul fiscal; «Coduri angajament» merg tot dupa el.
- Furnizorul asociat unui DDF primeste singur angajamentul si indicatorii lui; fara cod fiscal nu are coduri.
- «Repara...» la Rezervari deschide o fereastra: ce s-a detectat si ce se va face; apoi «Renunta» sau «Repara».
- Autentificare: bifa «Tine minte parola...» ramane cum ati stabilit-o in Setari, chiar fara optiuni avansate.
- Tutorialul «Primii pasi in K-BOT» ruleaza o data la deschidere, se opreste din Setari; le inlocuieste pe cele vechi.
- Ajutorul: subiecte noi pentru receptii, propuneri, parteneri si repararea rezervarilor.

## 1.1.2.4 (17.10.2026)
<!-- release: utc=2026-10-17T14:35:52Z -->

<!-- felii: 0113, 0114, 0114-01, 0115, 0116, 0117, 0118, 0119, 0120, 0000-69, 0000-70, 0000-71, 0000-72, 0000-73, 0000-74, 00EF-27, 00EF-29, 00EF-30, 00EF-31, 00EF-32, 00EF-33, 00EF-34, 00EF-35, 00EF-36, 0000-75, 0000-76, 0000-77, 0000-78, 0000-79, 0000-80, 0000-81 -->

- K-BOT porneste intr-o singura fereastra principala, pe pagina «Acasa»; ferestrele se deschid in ea, ca pagini.
- Sus e un ribbon cu filele «Angajamente», «Date», «E-Factura», «Configurari»; a luat locul butonului MENIU.
- «Inchide» (dreapta) inchide pagina deschisa; din lista angajamentelor te duce la «Acasa». Esc inchide la fel.
- «Ajutor» din ribbon da ajutorul paginii deschise; dublu clic pe o fila strange ribbonul.
- «Acasa» are cardul «Ce este nou», cu noutatile ultimelor versiuni; se desface singur dupa fiecare actualizare.
- Panou in stanga: «Machete deschise» (revii la o pagina cu un clic), «Actiuni recente», «Informatii», «Erori».
- «Jurnal activitate» si «Istoric versiuni» sunt pagini proprii, in ribbon: «Configurari» > «Configurare».
- Editorul DDF: «Adauga rand» si «Sterge randul» stau pe randul lui «Salveaza documentul».
- E-Factura: «Cumparare» e inaintea lui «Vanzare»; «Conturi Unitate» si «Date Unitate» sunt in «Configurari».
- Facturile primite: arbore pe furnizori sau pe luni, cu cautare si optiuni de filtrare in antetul lui.
- Se aduc de la ANAF din «Actualizare...» (subsolul arborelui) sau din ribbon; la final apare lista facturilor noi.
- «Continut» arata CPV (rosu cand lipseste) si observatiile; nou «XML Factura»; PDF-ul se salveaza unde alegeti.

## 1.1.2.2 (12.10.2026)

<!-- release: utc=2026-10-12T10:45:12Z -->
<!-- felii: 0092-01, 0000-68 -->

- Rezervari: butonul «Repara» (dreapta-jos) apare doar cand sunt rezervari gresite; nu repara luni cu revizii.
- Un angajament cu rezervari gresite apare cu rosu in lista; la trecerea mouse-ului vedeti ce valori nu se potrivesc.
- Meniul din stanga-jos al rezervarilor nu mai are «Reanalizeaza rezervarile».
- Optiuni avansate: «Setari» > «FOREXE» > «Istoric (avansat)» sterge ultima luna din istoric, doar daca nu are DDF sau ORD.
- Descarcarea nu mai calculeaza rezervarile unei luni de la valorile unei luni mai vechi.
- Ajutor: «Rezervari» si «Optiuni avansate» descriu butonul «Repara» si stergerea ultimei luni.

## 1.1.2.1 (09.10.2026)

<!-- release: utc=2026-10-09T15:42:42Z -->
<!-- felii: 0112, 0112-02, 0112-03, 0112-04, 0008-02, 000T-12, 00EF-16, 00EF-17, 00EF-18, 00EF-19, 00EF-20, 00EF-21, 00EF-22, 00EF-23, 0000-59, 0000-60, 0000-61, 0000-62, 0000-63, 0000-64, 0000-65, 0000-66 -->

- Casetele de mesaj ale K-BOT sunt proprii, in culorile temei; «Da»/«OK» stau in dreapta, «Nu»/«Anulare» in stanga.
- La un mesaj de eroare, butonul din bara de titlu trimite eroarea, cu detalii, la serverul K-BOT.
- Angajamentele se pot grupa (denumire si culoare); rotita listei are randul «Grupe» si arata doar angajamentele grupei.
- «Grupe» > «Editeaza grupe...»: adaugati grupe, bifati angajamente, alegeti culoarea; «Alias» se scrie in tabel.
- Coloana «Indicatori» arata, la trecerea mouse-ului, ce indicatori are fiecare angajament.
- «E-Factura - facturi emise»: facturile anului ales, cu filtru pe luni; «Cont emitent» propune conturile unitatii.
- O factura acceptata are in meniu «Corecteaza factura (tip 384)».
- Facturile primite se aduc de la ANAF: meniul unitatii, «Sincronizeaza facturi primite» (7-60 zile).
- Fereastra noua «Facturi primite»: toate facturile anului, cu cautare dupa furnizor, cod fiscal sau numar.
- Fila «E-Factura» de la fundamentare apare doar la angajamente cu facturi primite: Linii, Factura PDF, Atasamente, Mesaje.
- «Leaga de un DDF...» leaga o factura primita de o fundamentare; «Salveaza ca ZIP» da arhiva semnata de ANAF.
- Ajutor: subiecte noi pentru mesaje, grupe si facturile primite, plus tutoriale pentru grupe.

## 1.1.2.0 (09.10.2026)

<!-- release: utc=2026-10-09T08:30:00Z -->
<!-- felii: SLICELESS -->

- Un angajament cu o clasificatie adaugata manual in K-BOT se poate acum fundamenta: nu mai apare eroarea de legare.
- Cand adaugati clasificatii noi, indicatorii care le asteptau se leaga singuri de ele.
- La descarcarea din FOREXE, un indicator ramas fara clasificatie se leaga de ea imediat ce clasificatia exista.
- Ordonantarea nu mai da «Clasificatia lipseste» / «Cod SSI lipsa» pe platile unui indicator legat acum de clasificatie.

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
