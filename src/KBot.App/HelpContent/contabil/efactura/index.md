---
id: contabil.efactura
title: E-Factura: facturile emise
part: contabil
order: 75
parent: contabil
screens: FacturiForm, DateUnitateForm, PrimiteForm, KeAngajamentForm, SetariAplicatieView.chkKeZi, VerconForm
keywords: contare, contari, arhiva, arhivare, arhiveaza factura, vercon, trimite la vercon, trimitere la vercon, nir, administrator, cont vercon, efactura, e-factura, factura electronica, facturi emise, arbore, trimitere, trimite factura, validare, stornare, storno, factura clasica, pdf, eroare anaf, factura, client, cumparator, date unitate, unitate emitenta, cont emitent, iban, unitate de masura, linii, ciorna, serie, numar factura, anaf, token, certificat, certificat calificat, autorizare, reinnoire, spv, pin, expirat, vanzare, cumparare, facturi primite, ribbon e-factura, receptie, recepție, receptii, legare factura, legare de receptie, alocare, clasificatie, clasificatiile liniilor, indicator, avans, partial, final, plata factura, modifica receptia, receptie noua, storno receptie
open: menu:efactura
---
<!-- slice: 00EF-05, 00EF-08, 00EF-09, 00EF-13, 00EF-16, 0000-54, 0000-55, 0000-57, 0000-58, 0000-60, 0113, 0119, 0114-01, 00EF-30, 0000-76, KE03, 0000-83, 0114-10, 0114-14, 0000-90 -->
E-Factura are fila ei în ribbon, **E-Factura**, cu două grupuri: **E-Factura** («Cumpărare», «Vânzare») și **ANAF**
(«Token ANAF»; «Sincronizează facturi primite», cu cele două casete ale lui, e în grupul din dreapta al machetei «Cumpărare», lângă «Închide»). **«Vânzare»** deschide macheta **«E-Factura — facturi emise»**: aici pregătești facturile electronice ale unității, pe care le trimiți apoi la ANAF. Pentru a lucra cu ANAF, K-BOT are nevoie și de un **token ANAF** (o autorizare pe care o dai o singură dată, cu certificatul calificat al unității, și o reînnoiești o dată pe an); îl vezi și îl reînnoiești din ribbon, **E-Factura › ANAF › «Token ANAF»** — vezi [Tokenul ANAF: obținerea și reînnoirea](topic:contabil.efactura.token).

<!-- capture: ribbon-efactura | caption: Fila «E-Factura» a ribbonului | prepare: Alegeți fila «E-Factura» a ribbonului; prindeți toată banda ribbonului. | redo: 2026-10-17 21:00 | why: 0114-18: lista si caseta de zile din grupul din dreapta arata ca meniul din subsolul arborelui (texte gri, lista se deschide); 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor -->

<!-- capture: efactura-facturi | caption: Macheta «E-Factura — facturi emise» | goto: menu:efactura | prepare: Deschideți fereastra pe o unitate care are deja câteva facturi, în stări diferite, și alegeți una din arbore. | redo: 2026-10-17 21:00 | why: 0114-19: fara lista de luni deasupra arborelui; randurile nu se mai taie -->

Macheta se deschide doar cât ai o unitate deschisă; altfel K-BOT spune «Nu există o unitate deschisă: autentificați-vă întâi.». Se deschide ca pagină a ferestrei principale; dacă o ceri a doua oară, K-BOT o aduce înapoi pe cea deschisă. O închizi cu **«Închide»** din ribbon sau cu Esc.

## Arborele facturilor
<!-- slice: 00EF-09, 00EF-13, 00EF-16, 0000-57, 0000-58, 0000-60, 0114-19, 0000-91, 0114-23, 0000-94 -->

Arborele arată **toate** facturile din **anul ales în K-BOT**, fără filtru de lună. Rândurile nu se mai taie după câteva litere: numărul și data facturii au tot locul rămas între pictogramă și sumă.

În stânga vezi **clienții** unității, în ordine alfabetică; sub fiecare client sunt **facturile lui**, cea mai nouă prima, cu numărul, data și totalul. Pe rândul clientului vezi totalul facturilor lui. Lupa din antet deschide căutarea în arbore (după client sau după număr); Esc o golește. O factură aleasă se vede în dreapta, într-o bară cu **vederi** (Generale, Cumpărător, Conținut și, după starea facturii, vederile cu document — vezi mai jos).

**Bulina din stânga** unei facturi spune unde a ajuns:

- **cerc gri, gol** — **netrimisă** (ciornă): a fost salvată, dar nu a plecat la ANAF; doar o ciornă se poate modifica;
- **portocaliu** — **trimisă, dar neconfirmată**: a plecat la ANAF și rezultatul nu este încă cunoscut; factura nu se modifică până aflați rezultatul;
- **verde** — **acceptată** de ANAF;
- **roșu** — **refuzată** de ANAF; motivul se vede în vederea **Eroare ANAF**.

La sfârșitul rândului poate apărea **· stornare** (factura anulează o alta). Când treci cu mouse-ul peste o factură, în dreapta rândului apare **semnul cu trei puncte**: apăsat, deschide **meniul facturii**.

## Meniul unei facturi
<!-- slice: 00EF-09, 00EF-16, 0000-57, 0000-60 -->

<!-- capture: efactura-meniu-factura | caption: Meniul unei facturi acceptate | goto: menu:efactura | prepare: Treceți mouse-ul peste o factură acceptată din arbore și apăsați semnul cu trei puncte din dreapta rândului. -->

Apeși semnul din dreapta rândului ei. Factura se deschide în dreapta (dacă nu era deja) și meniul arată doar ce se poate face cu ea acum:

| Starea facturii | Ce oferă meniul |
|---|---|
| Netrimisă (ciornă) | **Trimite factura în ANAF**; **Modifică factura** (doar o ciornă obișnuită, nu o factură de stornare; modificarea nu trimite nimic) |
| Trimisă, neconfirmată | **Validează la ANAF (starea facturii)** |
| Refuzată | **Afișează eroarea ANAF** |
| Acceptată | **Listează factura clasică (PDF)**; **Listează factura ANAF (PDF)**; **Corectează factura (tip 384)**; **Stornează factura în ANAF** (dacă nu a fost deja stornată) |

Un separator desparte, în meniu, trimiterea și verificarea de listări, iar listările de stornare și modificare.

## Corectarea unei facturi acceptate
<!-- slice: 00EF-16, 0000-60 -->

**«Corectează factura (tip 384)»** se află doar în meniul unei facturi **acceptate** de ANAF (nu și al uneia de stornare). Se deschide o fereastră mică în care poți schimba doar **comentariile** și **referința comenzii**; restul facturii (client, linii, dată, total) rămâne cum este — pentru orice altceva, factura se stornează. Cu **«Trimite corecția»** factura pleacă din nou la ANAF ca **factură corectată (tip 384)**, iar K-BOT citește rezultatul ca la o trimitere obișnuită. Factura se schimbă la tine abia după ce ANAF a primit fișierul; dacă trimiterea nu reușește, rămâne așa cum era. După o corecție, factura arată **· corectată** și are tipul 384.

## Trimiterea la ANAF
<!-- slice: 00EF-09, 00EF-16, 0000-57, 0000-60, 0119 -->

1. Din meniul unei ciorne alege **«Trimite factura în ANAF»** și confirmă.
2. K-BOT verifică factura. Dacă are erori, ți le arată într-o listă și **nu o trimite**; corectezi (**«Modifică factura»**) și încerci din nou. Dacă verificarea trece, factura se trimite. Dacă factura are bifat **«Atașează factura originală»**, K-BOT o trimite împreună cu o factură PDF în formatul clasic, atașată în fișierul electronic.
3. K-BOT așteaptă câteva secunde răspunsul ANAF, apoi îl citește o dată. Rezultatul apare în chenarul de sus: **verde** — acceptată; **roșu** — refuzată, cu motivul; **galben** — încă în prelucrare.
4. Dacă factura este încă în prelucrare, bulina rămâne portocalie. Peste câteva momente alege din meniul ei **«Validează la ANAF (starea facturii)»**, care citește din nou starea. Se poate repeta până ANAF dă un răspuns.

Dacă ANAF nu mai acceptă tokenul unității, mesajul îți cere să-l reînnoiești: butonul **«Token ANAF»** e în ribbon, **E-Factura › ANAF**. O factură trimisă nu se mai modifică și nu se șterge; una acceptată nu se mai trimite a doua oară.

## Stornarea unei facturi
<!-- slice: 00EF-09, 0000-57 -->

**«Stornează factura în ANAF»** se află în meniul unei facturi acceptate care nu a fost încă stornată. După o confirmare (stornarea nu se poate anula), K-BOT:

1. întocmește **factura de stornare**: aceleași linii, cu semnul schimbat; primește următorul număr al seriei și data de azi (sau a ultimei facturi, dacă aceasta este mai nouă);
2. o trimite la ANAF și îi citește starea, ca la orice factură;
3. pregătește o **factură nouă**, copie a celei stornate — o ciornă — pe care o corectezi cu **«Modifică factura»** și o trimiți la ANAF.

O factură de stornare nu se mai modifică și nu se mai stornează.

## O factură nouă
<!-- slice: 00EF-08, 00EF-13, 0000-55, 0000-58 -->

Pentru un traseu ghidat, pas cu pas, deschide din ajutor (butonul «?») lista **Tutoriale** și alege **«Cum adaug o factură nouă (E-Factura)»**.

1. Apasă **«Adăugare»**. Seria și numărul apar sus, cu mențiunea «provizoriu; numărul se dă la salvare»: numărul real este următorul liber al seriei și îl primește factura în clipa salvării.
2. În vederea **Cumpărător** alegi clientul (vezi mai jos).
3. În vederea **Conținut** adaugi liniile (vezi mai jos).
4. În vederea **Generale** verifici data (azi, implicit), comentariile și referința comenzii; tot acolo verifici **contul emitent** (cel al ultimei facturi, implicit).
5. Apasă **«Salvare»**. Factura apare în listă și rămâne ciornă.

K-BOT nu salvează până nu ai: un client ales, o dată, un cont emitent și cel puțin o linie cu conținut și unitate de măsură; îți spune ce lipsește și te duce la vederea și câmpul respectiv. **«Renunță»** aruncă ce ai scris. Dacă treci la altă factură sau închizi fereastra cu modificări nesalvate, K-BOT întreabă dacă le salvezi.

**«Modificare»** (la fel ca **«Modifică factura»** din meniul facturii) deblochează o ciornă pentru schimbări (apoi «Salvare» sau «Renunță»). **Data** se poate schimba la o factură nouă și la **ultima factură a seriei**, dar nu mai veche decât factura dinainte; la celelalte facturi ea rămâne cea de la salvare. **«Ștergere»** șterge o ciornă, dar numai dacă are **ultimul număr al seriei** — altfel ar rămâne un număr lipsă; o factură trimisă la ANAF nu se șterge.

## Vederea Generale
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57, 00EF-13, 00EF-16, 0000-58, 0000-60, 00EF-52, 0000-130 -->

Numărul facturii, **Data facturii** (scrisă sau aleasă din calendar; se schimbă doar la o factură nouă sau la ultima factură a seriei, și nu înainte de factura anterioară), **Tip factură** (380 — factură; 384 — factură corectată; îl pune K-BOT), **Stare**, **Comentarii factură** (cel mult 255 de caractere), **Ref. comandă (BT-13)** (cel mult 30), **Cont emitent (IBAN)** — contul în care se plătește **această factură**; lista oferă **conturile unității** (din fereastra «Conturi Unitate»); poți și scrie un cont nou, care se adaugă la conturile unității când salvezi factura. O factură nouă începe cu singurul cont al unității, dacă unitatea are unul singur — și **TOTAL FACTURĂ**, care se adună singur din linii. Sub ele, K-BOT scrie în cuvinte situația facturii (ciornă, acceptată, motivul unui refuz, ce stornează).

## Vederea Cumpărător
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57, 00EF-15 -->

Sus alegi clientul din listă; tastează o parte din denumire sau din codul fiscal ca să-l găsești. Câmpurile de dedesubt îl arată: **persoană fizică** (bifă; atunci «Cod fiscal» este CNP-ul), **cod fiscal** și, pe același rând, **prefix fiscal** («RO» pentru plătitor de TVA), **denumire**, **județ**, **oraș**, **sector** (doar pentru București), **adresă**, **cont (IBAN)** și **bancă**.

- **«Client nou»** golește câmpurile pentru un client nou și le activează; până îl apeși, câmpurile sunt inactive și doar lista de clienți se poate folosi. Cursorul merge pe **Cod fiscal**: după ce l-ai scris (și ai trecut mai departe sau ai apăsat Enter), K-BOT întreabă ANAF și completează singur denumirea, prefixul, județul, orașul și adresa. Verifici datele și apeși «Salvează clientul». Dacă ANAF nu răspunde sau nu cunoaște codul, primești un mesaj și completezi de mână.
- **«Salvează clientul»** îl scrie în baza de date **separat de factură** și îl alege pentru ea. Dacă ai schimbat câmpurile unui client și nu l-ai salvat, factura nu se salvează până nu apeși «Salvează clientul».
- **«Șterge clientul»** îl șterge; un client care are facturi nu se poate șterge.

Câmpurile se pot schimba doar cât factura este deschisă pentru modificare.

## Date unitate
<!-- slice: 00EF-13, 0000-58, 00EF-14, 0119 -->

Datele unității care emite facturile (aceleași pe toate facturile) se țin într-o fereastră a lor, **«Date Unitate»**. O deschizi din ribbon, **Configurări › Unitate › «Date Unitate»**; nu e nevoie să ai deschisă macheta facturilor (dacă e deschisă, preia singură ce ai salvat). Dacă unitatea nu are încă aceste date, fereastra se deschide singură și la intrarea în «Vânzare», iar facturi noi nu poți adăuga până nu le salvezi. La deschidere, dacă denumirea este goală, K-BOT ia singur de la ANAF denumirea, județul, orașul și adresa, după codul fiscal al unității; restul le completezi tu.

<!-- capture: date-unitate | caption: Fereastra «Date Unitate» | prepare: Din ribbon, Configurări › Unitate › «Date Unitate», pe o unitate cu datele completate. -->

- **Denumire** și **Cod fiscal** (cifre, cu «RO» în față dacă unitatea plătește TVA) sunt obligatorii;
- **Județ** și **Oraș**, apoi **Adresă**;
- **Telefon** — zece cifre; punctele se pun singure (0721.123.456); poate rămâne gol;
- **E-mail**;
- **Seria facturilor** și **Primul număr** (contează doar pentru o serie care nu are încă nicio factură). **După prima factură emisă, cele două nu se mai pot schimba** și apar blocate.

**«Salvează»** scrie datele; **«Ieșire»** închide fereastra (dacă ai modificări nesalvate, K-BOT întreabă). Atenție: o modificare are efect asupra tuturor documentelor generate de aici înainte.

**«Preia de la ANAF»** completează din nou denumirea, județul, orașul și adresa unității după codul ei fiscal din lista unităților și le scrie imediat. Îl poți folosi oricând, dacă ai schimbat de mână aceste câmpuri și vrei să le readuci; K-BOT întreabă înainte, pentru că ce ai scris în ele se înlocuiește.

Conturile bancare ale unității sunt în fereastra **«Conturi Unitate»**, alături în ribbon (**Configurări › Unitate**) — vezi [Conturile unității emitente](topic:contabil.efactura.conturi).

## Vederea Conținut
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57, 00EF-16, 0000-60, 0114-23, 0000-94, 00EF-50, 0000-129, 00EF-51 -->

Liniile facturii sunt într-un tabel în care scrii direct: **Nr**, **Conținut**, **Um**, **Cant**, **PU** și **Valoare**. 

- cât scrii sau modifici factura, tabelul se termină cu un **rând gol**: scrie în el și devine o linie nouă, cu unitatea **XPP** (bucată) și cantitatea 1, iar dedesubt apare alt rând gol;
- **Tab** trece de la **Conținut** la **Um**, apoi la **Cant**, apoi la **PU**, iar după **PU** pe rândul următor (ca **Enter**);
- **Um** se alege din lista unităților de măsură (tastează codul sau o parte din nume); dacă lista nu se poate citi, scrii codul de mână;
- **Cant** poate avea cel mult 3 zecimale, **PU** cel mult 4; se scriu cu virgulă sau cu punct;
- **Valoare** se calculează singură (cantitate × preț, 2 zecimale) și nu se scrie;
- **«❐»** de pe un rând copiază linia la sfârșitul tabelului și te duce în **Conținutul** copiei, ca s-o schimbi;
- **«✕»** de pe un rând șterge linia; totalul din subsolul tabelului și din vederea **Generale** se actualizează;
- sub tabel, în subsolul vederii, este bifa **«Atașează factura originală»** (înainte avea o vedere a ei, «Atașamente», scoasă): alegerea se păstrează pe factură și se schimbă doar cât factura este o ciornă deschisă pentru modificare. Când este bifată, la trimiterea la ANAF (și la o corecție) K-BOT întocmește factura PDF în formatul clasic și o atașează în fișierul electronic; fără bifă nu se atașează nimic.

## Factură PDF, Factură ANAF și Eroare ANAF
<!-- slice: 00EF-09, 0000-57 -->

După starea facturii, pe bara de vederi apar vederile cu document. Ele se deschid și din meniul facturii, și au același vizualizator PDF ca vederea «Document PDF» de la DDF (din el se poate și tipări):

- **Factură PDF** (factură acceptată) — factura în formatul clasic, întocmit de K-BOT: furnizorul, cumpărătorul, liniile și totalul;
- **Factură ANAF** (factură acceptată) — factura așa cum o desenează ANAF din fișierul pe care îl păstrează. K-BOT o aduce de la ANAF, deci are nevoie de conexiune și de un token valabil; cât se aduce, vizualizatorul spune «Se aduce factura de la ANAF…»;
- **Eroare ANAF** (factură refuzată) — motivul dat de ANAF, pe o pagină, cu fiecare motiv pe rândul lui.

Cât timp scrii sau modifici o factură, aceste vederi nu se văd. Dacă nu există vizualizator PDF în fereastră, vederea spune acest lucru.

## Tokenul ANAF
<!-- slice: 00EF-05, 0000-54, 0000-55, 0000-56, 0119 -->

Butonul **«Token ANAF»** din ribbon (**E-Factura › ANAF**) deschide fereastra tokenului, unde vezi starea lui și îl obții sau îl reînnoiești cu certificatul calificat al unității. Pașii, mesajele și ce faci când ceva nu merge sunt în [Tokenul ANAF: obținerea și reînnoirea](topic:contabil.efactura.token).

## Facturile primite
<!-- slice: 00EF-18, 00EF-21, 00EF-22, 00EF-23, 0000-61, 0000-64, 0000-65, 0000-66, 0119, 00EF-25, 00EF-26, 00EF-27, 00EF-29, 0000-75, 00EF-31, 0000-77, 00EF-35, 0000-80, 00EF-36, 0000-81, 00EF-37, 0000-82, KE02, KE03, 0000-83, 0114-10, 0114-14, 0000-90, 0114-17, 0000-91, 0114-22, 0000-93, 00EF-40, 0000-96, 00EF-41, 0000-97, 00EF-42, 0000-98, 00EF-47, 0000-116, 00EF-48, 0000-117, 00EF-53, 0000-143 -->

În bara din stânga a ferestrei principale apare vederea **«E-Factura»**, sub «Extrase», sus, doar pentru un angajament a cărui fundamentare (DDF) are facturi primite — legate de furnizorii ei sau de tine; la celelalte angajamente nu se vede. Starea se citește odată cu arborele: după o sincronizare sau după ce legi o factură, fila apare când arborele se reîmprospătează. Când salvezi un document de fundamentare cu un furnizor care are facturi primite, fila apare singură, chiar după salvare, fără să mai reîncarci arborele. Ea arată facturile electronice primite de la furnizori, aduse de la ANAF.

- În stânga este un arbore: rădăcina **«Toate facturile»** (fără numărul facturilor, doar cu totalul), sub ea lunile (cea mai nouă prima, numai cu numele lunii; anul apare doar dacă lista are facturi din mai mulți ani), iar sub fiecare lună **direct facturile**, cu data, numărul și totalul (în vedere nu există nivelul furnizorului; în macheta «Facturi primite», sub lună vin furnizorii, grupați după codul fiscal, în ordinea numelui, și sub ei facturile lor); la fiecare rând valoarea stă în dreapta. Punctul din fața unei facturi este deocamdată neutru: nu arată dacă ai deschis factura, iar culoarea lui va căpăta mai târziu un alt înțeles; deschiderea unei facturi nu mută arborele. **Lupa** din antetul arborelui deschide căutarea (scrii cel puțin 3 caractere, după furnizor, dată sau număr; Esc o golește). În macheta «Facturi primite», butonul din dreapta antetului deschide **opțiunile arborelui** (vederea angajamentului nu are acest buton): **«Afișare facturi fără operații în AVACONT»** (doar facturile fără nicio operație înregistrată; bifat cât este în vigoare), **«Afișare facturi noi»** (doar facturile aduse de la ANAF în sesiunea aceasta; apare doar după ce ai adus facturi — altfel nu se știe care sunt noi), apoi **«Grupare după furnizor»** / **«Grupare după lună»** (se oferă cea care nu e în vigoare). Grupat după furnizor, arborele are furnizorii pe primul nivel și facturile lor dedesubt, iar meniul mai oferă **«Ordonare crescătoare facturi»** și **«Ordonare descrescătoare facturi»** (după dată). Opțiunile țin cât ține sesiunea;
- clic pe rădăcina **«Toate facturile»** arată în dreapta **lista tuturor facturilor** din arbore (cele rămase după filtre), cea mai nouă prima, cu data, numărul, **recepția asociată** (data, descrierea și valoarea recepției legate de factură, într-o singură celulă; mai multe recepții se despart cu «|»; goală dacă factura nu e legată de nicio recepție), scadența și totalul; coloana din stânga are o **bifă** pe fiecare rând, iar iconița din capul ei bifează sau debifează toate rândurile. Cele două butoane din subsol lucrează pe facturile **bifate**: **«Listează»** pregătește fiecare factură (factura clasică, făcută de K-BOT) și o trimite la imprimantă fără să o deschidă — K-BOT întreabă o singură dată imprimanta și numărul de exemplare —, iar **«Salvează XML»** cere un dosar și pune în el XML-ul fiecărei facturi (un nume deja luat primește «(2)», «(3)»…, nimic nu se suprascrie). Ultima coloană a fiecărui rând are butonul **«…»**, al cărui meniu oferă aceleași două acțiuni, **numai pentru rândul acela** (la «Salvează XML» se deschide fereastra obișnuită de salvare). Dacă una dintre facturi nu poate fi listată sau salvată, K-BOT le face pe celelalte și o numește la sfârșit. Un clic pe o factură din arbore readuce bara de vederi;
- vederea arată **numai** facturile asociate angajamentului: ale furnizorilor care sunt parteneri ai fundamentării lui (se compară codul fiscal, fără «RO» și fără spații), plus cele pe care le-ai legat chiar tu. Celelalte facturi primite se văd în macheta **«Facturi primite»**, unde clic dreapta pe una și **«Leagă de un DDF…»** o leagă de fundamentare; **«Scoate legătura cu acest DDF»**, din meniul unei facturi din vedere, desface o legătură făcută de tine. Antetul arborelui din vedere are doar lupa, fără butonul de opțiuni;
- clic dreapta pe o factură oferă și **«Salvează ca ZIP»** (arhiva semnată de ANAF, păstrată împreună cu factura; dacă lipsește, K-BOT o aduce din nou de la ANAF) și **«Salvează ca XML»** (factura din arhivă);
- în dreapta, pe bara de vederi: **Conținut** (sus, aproximativ două cincimi din înălțime: **furnizorul** — nume, cod fiscal, adresă, scadență, conturile în care cere plata — și **observațiile** scrise de el în factură; jos, liniile scrise de furnizor, fiecare cu codul lui CPV, cu **valoarea cu TVA** a liniei și cu **clasificația** pe care ai ales-o pentru ea (vezi [Recepțiile unei facturi primite](topic:contabil.efactura)), după ce le-ai ales; la facturile aduse de la ANAF de acum înainte, o linie fără cod CPV apare cu roșu închis; la cele aduse mai demult coloana CPV rămâne goală), **Factură PDF** (are propria bară, cu **Factură clasică** — factura întocmită de K-BOT din datele ei, în formatul unei facturi obișnuite —, **Factură ANAF** — factura desenată de ANAF — și, doar dacă factura are fișiere atașate, **Atașamente**: un clic deschide o listă cu numele fișierelor, iar cel ales se deschide în aceeași fereastră, fie că e PDF sau imagine; dacă nu alegi nimic, rămâne ce vedeai) și **Mesaje** (mesajele schimbate prin sistemul E-Factura despre factură, cu data lor; sub listă scrii un mesaj pentru furnizor și apeși **«Trimite»** — după o întrebare de confirmare, mesajul pleacă prin ANAF și nu mai poate fi retras). La capătul din dreapta al barei este vederea **«XML Factură»**: XML-ul facturii alese, colorat pe părți (nume de elemente, atribute, valori, comentarii), cu o **hartă** a elementelor în stânga — cu numele **în română** («Data emiterii», «Date furnizor», «Subtotal TVA»), fără prefixele cac / cbc, cu valoarea lângă elementele simple; numele din fișier se vede în sfatul de la trecerea mouse-ului peste element, iar un element netradus își păstrează numele; un clic pe un element din hartă îl selectează în text (textul rămâne cum e în fișier). Se citește doar și nu se verifică nimic. O valoare foarte lungă (conținutul unui atașament) apare tăiată, cu numărul de caractere lăsate deoparte. Nu există file separate pentru atașamente și observații: fișierele atașate se deschid din **Factură PDF**, iar observațiile sunt în **Conținut**, deasupra liniilor;
- în vederea **Factură PDF**, butonul de salvare al Adobe nu scrie peste fișierul de pe ecran: K-BOT întreabă **dacă** vrei să-l salvezi și **unde**; dacă anulezi, nu se salvează nimic.

Facturile se aduc de la ANAF din ribbon, cu **«Sincronizează facturi primite»**, în grupul din dreapta, lângă «Închide» — butonul apare doar cât este deschisă macheta **«Cumpărare»**; vezi mai jos.

## Recepțiile unei facturi primite
<!-- slice: KE01, KE02, KE03, KE04, KE05, KE06, KE07, 0000-83, 00EF-39, 0000-95, KE09, KE10, 0000-99, KE11, 0000-101, KE12, 0000-102, KE12-04, 0000-105, 0128-01, 0128-03, 0000-152, 02EF-02, 0000-155, KE21, 0000-170 -->

O factură primită se plătește la un moment dat, întreagă sau în părți. Ca să poată fi plătită, trebuie legată de o **recepție**. Vederea **«Recepții»** din bara facturii (apare după ce alegi o factură, la fel în macheta «Facturi primite» și în vederea «E-Factura» de la fundamentare) arată cum stă factura:

Cât timp vederea «Recepții» e pe ecran, fereastra se așază altfel decât la celelalte file; aranjarea, bifarea mai multor facturi și butoanele sunt descrise în secțiunea «Vederea «Recepții»: mai multe facturi pe o recepție», mai jos.

În vedere nu mai e tabelul cu legăturile deja făcute: cât din factură acoperă recepțiile se vede în linia de sus (alocat și rest).

**Numai un utilizator cu rolul «Contabil»** poate lega facturile de recepții; ceilalți văd aceleași cifre, dar pagina nu le lasă să scrie nimic.

**Sumele sunt cu TVA** (suma de plată). Fiecare linie are cota ei de TVA din factură; centimele de rotunjire dintre linii și totalul facturii se pun pe ultima linie a cotei, ca liniile să dea exact totalul. Dacă totalul facturii are și o parte care nu se explică prin linii (de exemplu o reducere pe toată factura), partea aceea se arată în notă și nu se alocă pe linii. O factură de stornare are sumele cu minus, iar recepția ei este o recepție obișnuită, cu minus.

### Vederea «Recepții»: mai multe facturi pe o recepție
<!-- slice: KE11, 0000-101, KE12, 0000-102, 0000-103, KE12-03, 0000-104, KE13, 0000-108, KE14, 0000-109, KE15, 0000-111, KE16, 0000-112, 0121-05, 0000-114, KE17, 0000-115, 0000-152, 02EF-01, 0000-153, 02EF-02, 0000-155, KE20-01, 0000-157, KE20-02, 0000-158, 02EF-04, 0000-159, 02EF-06, 0000-161, 02EF-07, 0000-162, KE20-04, 0000-163, 02EF-08, 0000-164, KE21, 0000-170 -->

Aceeași vedere **«Recepții»** o găsești în două locuri: în fereastra **«Facturi primite»** (ribbon, E-Factura › Cumpărare) și în vederea **«E-Factura»** a unui angajament din fereastra principală. Cât timp e pe ecran, toată fereastra se așază astfel:

- **stânga sus, arborele facturilor** — pe fiecare factură e o **căsuță de bifat**: poți bifa **mai multe facturi** și le legi pe toate de **o singură recepție**. Facturile pe care recepțiile le acoperă deja în întregime sunt **bifate de la început, cu căsuța gri și scrise mai șters**: nu se mai pot bifa sau debifa, iar cursorul pe ele arată recepțiile cu care sunt asociate. Un **clic simplu** pe o astfel de factură arată, în dreapta, doar ce are deja: **clasificațiile** pe care e alocată, **angajamentul** și **recepția** cu care e asociată, iar sub tabelul recepției, butonul **«Elimină asociere»** scoate, după o întrebare de confirmare, legăturile facturii cu recepția din K-BOT (recepția din FOREXE nu se atinge); un clic pe altă factură sau o bifă te aduce înapoi. La un **furnizor** care are facturi fără recepție, cât ții cursorul pe rândul lui apare un buton care propune **legarea lor în bloc** (o factură, o recepție); fereastra doar propune, nu salvează nimic până nu apeși «Leagă» în ea. **Butonul din antet** are în plus alegerea listei: **«Arată facturile fără recepții»** (cum se deschide), **«Arată facturile asociate»** sau **«Arată toate facturile»**. În vederea «E-Factura» a unui angajament arborele arată toate facturile lui, fără această alegere.
- **sus, în dreapta arborelui** — bara filelor facturii (**Conținut**, **Factură PDF**, **Mesaje**, **Recepții**, **XML Factură**); rămâne unde e, doar că arborele începe la același nivel cu ea.
- **sus, la mijloc** — deasupra tabelului, o linie îngroșată spune pe scurt cum stă factura bifată cu recepțiile: **«Total factură număr / dată: …»**, cât ai alocat deja, restul și starea ei; cu mai multe facturi bifate arată numărul lor și sumele însumate, iar fără nicio bifă, «Nicio factură bifată». Dedesubt, facturile bifate, grupate pe factură. Rândul grupului (fără fundal colorat, cu text îngroșat și chenar) arată numărul și data facturii, **totalul** ei (și restul, dacă are deja legături) și, sub coloana clasificației, **clasificația aleasă pentru factură**; la **«Recepționată»** arată suma rândurilor ei. Grupurile pornesc **strânse**: un clic pe rând desface rândurile facturii. Factura se leagă pe **toată valoarea ei** (totalul din factură, cu sau fără TVA) sau pe valoarea pe care o scrii tu, **nu pe produse**: produsele facturii nu mai apar în această pagină. Rândurile unei facturi sunt **alocările** ei: primul rând are clasificația aleasă în antetul facturii și, ca sumă, **tot restul facturii**; sub el apare un **rând gol**, numai cât timp suma alocată este mai mică decât restul facturii, în care alegi o altă clasificație și o altă sumă (la prima alegere ia ce a mai rămas din factură). Dacă n-ai ales nicio clasificație în antet, la desfacerea grupului apare doar rândul gol. Suma unui rând o poți scădea (pentru o plată parțială; rândul gol apare singur), dar suma tuturor rândurilor nu poate depăși restul facturii și are semnul facturii; K-BOT îți spune de ce. Clasificația o alegi din lista indicatorilor **tuturor angajamentelor** din tabelul «Angajamente» (dacă alegi una care aparține altui angajament, rândul curent din «Angajamente» trece pe acela, fiindcă o recepție se face pe un singur angajament); dacă angajamentul pe care ai dat clic are **un singur indicator**, clasificația lui se pune singură;
- **stânga jos, «Angajamente»** (tabel fără titlu deasupra, tot cât arborele de lățime și cât tabelul clasificațiilor de înălțime) — angajamentele la care pot merge facturile bifate: cele din DDF-ul furnizorului, cele pe care le-ai ales tu pentru furnizor și cele cu care factura e deja legată. Rândul pe care stai este cel pe care se face recepția. Rândurile existente nu se pot schimba; pe **rândul gol de la sfârșit** scrii codul sau descrierea altui angajament și îl alegi din listă. Ce alegi acolo **rămâne legat de furnizor** (după codul lui fiscal): îl găsești la fel la orice altă factură a furnizorului și după ce închizi pagina, iar legarea în bloc îl ia în calcul. Dacă furnizorul nu are cod fiscal pe factură, angajamentul nu se poate lega de el și pagina îți spune. Pe fiecare rând (în afară de cel gol și de cele cu recepție legată) e butonul standard de ștergere **✕**, care scoate asocierea: un angajament ales de tine pentru furnizor se scoate (după o întrebare; dispar și indicatorii legați de el), iar o asociere făcută de tine cu un DDF se scoate. O asociere care vine din DDF-ul la care furnizorul e partener sau din legături cu recepții nu se scoate de aici; **În vederea «E-Factura» a unui angajament acest tabel nu apare**: angajamentul este cel ales în lista din stânga a ferestrei principale, iar **tabelul clasificațiilor se întinde pe toată lățimea, sub arborele facturilor**.
- **sus, în dreapta, «Date recepție …»** — sus, o bară de file: **«Adaugă»**, **«Asociate»** (apare numai când o factură bifată are deja recepții legate) și **«Existente»**. Sub **«Adaugă»** sunt datele unei recepții noi: tipul, data și descrierea; fila există **numai dacă factura bifată nu e asociată integral** cu recepții (o factură acoperită în întregime nu mai primește recepție nouă). Sub **«Asociate»** vezi recepțiile cu care sunt legate facturile bifate, cu data, descrierea și, la **«Valoare»**, cât din ele revine facturilor bifate (nu se face nimic de acolo, iar butonul din ribbon nu e oferit). Sub **«Existente»** vezi lista recepțiilor angajamentului, din care o alegi, iar deasupra ei, la **«Acțiune»**, spui ce faci cu ea: **«Leagă facturile de recepție»** sau **«Modifică recepția (valorile cresc)»**. La **«Leagă facturile de recepție»**, dacă ai bifat o factură mai veche (dinaintea zilei de azi), lista arată recepțiile **așa cum erau la data facturii**: fiecare cu valoarea pe care o avea atunci (calculată ca la ordonanțare, din rândurile recepției datate până în ziua aceea; o recepție făcută abia după factură, cum se întâmplă de obicei, apare cu valoarea cu care a fost făcută, nu cu cea de azi), iar restul nealocat scade tot ce au luat deja facturile legate de ea, oricare ar fi data lor (o recepție luată de o factură mai nouă nu mai apare liberă; dacă recepția valora mai puțin la data facturii decât au luat facturile, restul e 0); titlul spune ziua folosită. Dacă ai bifat mai multe facturi, ziua este a celei mai vechi. Până sosește răspunsul, lista e goală. La facturile de azi, și la celelalte acțiuni, lista arată recepțiile așa cum sunt acum.
- **dreapta jos, tabelul clasificațiilor** — ocupă tot restul lățimii de sub tabelul cu facturi (sub arbore, în vederea unui angajament); pe fiecare clasificație, tabelul are, ca la fundamentare: **Buget** (bugetul clasificației în ziua recepției), **Total recepții** (toate recepțiile clasificației, de la toate angajamentele, de dinaintea acelei zile) și **Disponibil** (buget minus recepții); apoi, pentru **angajamentul pe care stai**, trei coloane ale lui: **Rezervări**, **Recepții** (ale lui, de dinaintea zilei recepției) și **Disponibil** (rezervări minus recepții); la sfârșit **Valoare** (suma coloanei «Recepționată») și **Rămas** (disponibilul angajamentului minus valoare). La o recepție **nouă** sau **modificată**, tabelul arată **toți indicatorii angajamentului**, nu doar clasificațiile facturilor bifate, iar **valoarea** o poți scrie pe oricare: poate fi **mai mare** decât totalul facturilor bifate (în limita disponibilului), de exemplu când factura pentru o clasificație lipsește. Ce depășește facturile rămâne doar pe recepție: **nu** se scrie înapoi în tabelul cu facturi și nu leagă nimic de nicio factură; recepția din FOREXE se scrie cu valoarea întreagă. Valoarea **nu poate fi mai mică** decât totalul facturilor bifate pe acea clasificație (pentru mai puțin scazi la «Recepționată», în tabelul cu facturi). Dacă mai târziu scazi o sumă în tabelul cu facturi, valoarea scade cu atât, iar ce era peste facturi rămâne. La **«Leagă facturile de recepție»** recepția nu se schimbă, deci peste facturi nu poți scrie; acolo coloana «Valoare» nu se scrie, iar sumele sunt cele din tabelul cu facturi. Orice schimbi în «Recepționată» se vede aici. **Rămas** nu poate fi sub 0: o valoare mai mare decât disponibilul angajamentului nu se primește. Ca în FOREXE, la o recepție nouă sau modificată valoarea nu poate depăși disponibilul: dacă o clasificație aleasă nu are rezervare suficientă, apare un mesaj de eroare, iar butonul rămâne oprit până scazi suma (sau faci întâi o revizie de rezervare). Ziua este data recepției noi (dacă o schimbi, cifrele se citesc din nou) sau data recepției alese. O clasificație fără buget în acea zi folosește creditul de azi, iar pagina te anunță.

Butonul din **ribbon**, în grupul **«Recepții»** din dreapta (**«Creează recepția»**, **«Leagă»** sau **«Modifică recepția»**, după ce ai ales; același ca în vederea «Recepții» a unei facturi), pornește lucrul. Pagina nu mai are buton în subsol. Alegerile liniilor se salvează odată cu el. După ce termină, pagina își citește din nou facturile: cele acoperite acum rămân în arbore, bifate și blocate, iar restul arborelui rămâne cum era (furnizorii desfăcuți rămân desfăcuți), cu căsuțele golite.

> O recepție se poate face doar pe o clasificație care există deja ca indicator al angajamentului. Dacă ea lipsește, K-BOT nu te lasă să continui și spune că este nevoie întâi de o **revizie nouă de rezervare** (din documentul de fundamentare), abia apoi de recepție.

### Rezervare și ordonanțare din vederea «Recepții»
<!-- slice: 02EF-10, 02EF-11, 02EF-12, 0000-168 -->

În același grup **«Recepții»** din ribbon apar încă două butoane:

- **«Creează rezervare»** — apare pe o recepție nouă sau modificată, cât timp pagina știe angajamentul, și se aprinde când facturile bifate pun o sumă pe cel puțin o clasificație. Deschide **aceeași fereastră ca «Adaugă rezervare»** din vederea Rezervări (cu indicatorii angajamentului), cu **valoarea recepției deja trecută pe fiecare indicator**. Poți schimba valorile, dar rezervarea **trebuie să încapă recepția**: dacă pe un indicator valoarea nu ajunge (după ce se ține seama de cât e liber acum), fereastra spune care și cu cât, și **nu salvează**. Rezervarea se salvează în K-BOT; o trimiți în FOREXE din vederea DDF, ca pe oricare alta. Când cifrele angajamentului se schimbă (un DDF salvat, o descărcare citită), pagina își reia singură cifrele și mesajul despre rezervări prea mici dispare dacă nu mai e valabil;
- **«Generează ordonanțarea»** — apare **după ce ai salvat o recepție** din vederea «Recepții» (nouă sau modificată) și dispare când bifezi alte facturi. Pregătește ordonanțarea pe baza **recepției**, nu a plăților: beneficiarul, contul și codul fiscal sunt ale furnizorului facturilor, **luate din Parteneri** (dacă furnizorul lipsește de acolo sau nu are cont, primești un avertisment și completezi tu), iar rândurile sunt valorile recepției. Se deschide în fereastra obișnuită a ordonanțării, unde o verifici și o salvezi. Ordonanțarea cere un document de fundamentare salvat pentru angajament.

La **crearea unei recepții** din vederea «Recepții», K-BOT face acum și **cele două capturi de ecran** de care ordonanțarea are nevoie (fila Recepții și «Informații complete contract») și le atașează recepției; ordonanțarea generată le găsește gata.

### Cele trei feluri de recepție
<!-- slice: KE04, KE06, KE07, KE11, 0000-101, KE12, 0000-102 -->

- **«Existente» › «Leagă facturile de recepție»** — facturile bifate se leagă de o recepție care are deja valoarea. Alegi recepția din tabelul din stânga jos (recepțiile care nu mai au nimic nealocat nu apar) și vezi, pe fiecare clasificație, restul ei; sumele din «Recepționată» se taie singure la ce mai are recepția, iar tu le poți scădea (pentru o plată parțială). Nimic nu se schimbă în FOREXE. Dacă recepția nu ajunge, pagina îți spune pe ce clasificație și nu te lasă mai departe: alege acțiunea **«Modifică recepția»** sau fila **«Adaugă»** pentru diferență;
- **«Adaugă»** (fila) — K-BOT face în FOREXE o recepție nouă. Alegi **tipul** (Avans, Partial sau Final; implicit «Final», ca în FOREXE), **data** (implicit data celei mai noi facturi bifate) și **descrierea** (o casetă pe mai multe rânduri; implicit «plata factura număr/data», iar cu mai multe facturi bifate «plata facturi» și toate numerele; în FOREXE se scrie pe un singur rând); valoarea pe fiecare clasificație este suma coloanei «Recepționată», adică restul facturilor sau o parte din el dacă o scazi. Implicit poți face mai multe recepții în aceeași zi pe același angajament; dacă vrei una singură pe zi, bifează în **«Setări» › «Aplicație»** căsuța **«La o factură primită, nu permite a doua recepție nouă în aceeași zi»**: dacă angajamentul are deja o recepție la data aleasă, pagina nu merge mai departe și îți cere să modifici recepția existentă sau să alegi altă dată;
- **«Existente» › «Modifică recepția»** — pentru o recepție existentă care nu ajunge: alegi recepția din tabelul din stânga jos, iar valorile ei cresc cu sumele din «Recepționată». K-BOT o caută în FOREXE după **dată și sumă**; dacă n-o găsește sigur, **nu salvează nimic** și îți spune.

Înainte să pornească ceva, K-BOT verifică alocarea: clasificația trebuie să fie indicator al angajamentului, iar sumele să încapă în restul facturii. Dacă recepțiile ar depăși angajamentul legal pe un indicator, primești un avertisment și alegi dacă continui.

Sumele din **«Recepționată»** le scrii în formatul din Windows al calculatorului tău (cu punct sau cu virgulă la zecimale, ca în celelalte celule): cu punctul ca separator, «77.49» înseamnă 77,49. Când o recepție existentă nu are destul rest pe clasificația unui rând, mesajul din josul paginii spune și pe ce clasificații mai are rest, iar tabelul clasificațiilor (dreapta jos) arată un rând în plus pentru fiecare astfel de clasificație, ca să alegi clasificația potrivită sau altă recepție.

**Legare în bloc, cu propuneri.** Când treci cu mouse-ul peste un furnizor din arbore care are facturi fără recepție, apare în dreapta rândului un buton cu două zale de lanț. Apasă-l și se deschide o fereastră cu facturile neasociate ale furnizorului. Pentru fiecare, K-BOT **propune** recepția care are exact valoarea facturii; dacă sunt mai multe cu aceeași valoare, o ia pe cea datată de la data facturii până la cel mult 2 zile după ea, iar dacă tot nu e una singură, factura rămâne nebifată. Pe fiecare rând poți:

- **schimba angajamentul**, în coloana «Angajament», dacă factura are mai mult de unul (altfel angajamentul doar se vede);
- **scrie în «Recepție» cât primești din factură**: implicit este valoarea ei și nu poate fi mai mare; o sumă mai mică înseamnă o recepție parțială, iar restul facturii rămâne de legat din pagina de recepții; 0 scoate recepția de pe rând;
- **alege altă recepție** în tabelul de jos, cu dublu clic pe ea. Tabelul arată recepțiile cu rest ale angajamentului facturii, în două grupuri: întâi cele cu aceeași valoare ca suma de recepționat, apoi restul recepțiilor neasociate integral. Coloana **«Rămas»** este valoarea recepției minus tot ce i s-a asociat (și ce iau celelalte facturi bifate din fereastră); o recepție la care nu a mai rămas nimic dispare din listă.

O recepție poate acoperi mai multe facturi, cât îi ajunge restul. Bifezi ce ești de acord să legi și apeși **«Leagă»**; până atunci nu se salvează nimic. Doar rolul «Contabil» poate lega.

După ce recepția este salvată în FOREXE, K-BOT **citește din nou angajamentul** și o ia în baza lui, ca la orice descărcare, iar legătura cu factura se scrie abia atunci. Dacă FOREXE nu a arătat mesajul de reușită, K-BOT verifică singur, la citirea înapoi, dacă recepția chiar a fost salvată; dacă nu poate confirma, **nu leagă nimic** și îți spune să verifici în FOREXE înainte să încerci din nou. Cât durează, pagina este oprită.

**Mai multe facturi pe o singură recepție.** Bifezi pur și simplu mai multe facturi în arbore: alegi o singură dată **angajamentul, tipul, data și descrierea** (descrierea propusă cuprinde toate facturile bifate) și, pe liniile fiecărei facturi, clasificațiile și sumele. K-BOT verifică toate facturile înainte să pornească; dacă una nu e în regulă, îți spune care și oprește totul. Apoi face în FOREXE **o singură recepție**, cu sumele facturilor adunate pe fiecare indicator, și leagă **fiecare factură** de ea. Dacă o legătură nu se poate scrie, îți spune care facturi au rămas nelegate; le legi apoi de recepția existentă.

**Dacă FOREXE refuză recepția** (de exemplu cu mesajul «CAB-0031 - Datele introduse nu sunt valide»), K-BOT îți arată mesajul site-ului, **nu leagă nimic** și nu mai citește angajamentul. Data se scrie în FOREXE cu puncte (zz.ll.aaaa) și sumele cu virgulă la zecimale, oricum ar fi setat Windows-ul.

<!-- capture: efactura-receptii | caption: Vederea «Recepții» a unei facturi primite | prepare: Din ribbon, E-Factura › Cumpărare, alegeți o factură primită care are deja o clasificație aleasă și deschideți vederea «Recepții». | redo: 2026-11-08 12:00 | why: 0128: arborele se scurtează, recepția se întinde pe toată lățimea, butonul de lucru e în ribbon, linia de sus poartă numărul și data facturii; KE21: linia cu detaliile facturii deasupra tabelului, fila «Adaugă» pe rândul de sus, tabelul clasificațiilor aliniat cu angajamentele -->

## Contări
<!-- slice: 01EF-02, 0000-144, 01EF-04, 0000-145, 00EF-55, 01EF-07, 0000-149 -->

**Contări** este prima filă din bara unei facturi primite și apare doar în fereastra cu toate facturile (nu și în vederea unui angajament). Contarea nu are legătură cu recepțiile.

**Cum pornești.** Dai clic dreapta pe o factură din arbore și alegi **«Adaugă operații contabile»**. Factura se bifează, se deschide pagina «Contări» și tabelul facturilor se completează singur cu ea. Doar celelalte facturi ale **aceluiași furnizor** capătă căsuțe de bifat (furnizorii nu au căsuțe). Pagina arată ca formularul din Access: sus clasificația, codul de client, contul IBAN și felul facturii pentru ordinele de plată; la mijloc **«Documente evidențiere»** și tabelul facturilor; jos **«Documente plată»** și ordinul de plată; la capăt **«Renunță»** (stânga) și **«Salvează»** (dreapta).

- **Bifează sau debifează** alte facturi ale furnizorului: fiecare factură bifată primește un rând în tabelul facturilor, iar la debifare rândul dispare. **Trebuie să rămână bifată cel puțin o factură**; ca să renunți la tot, apeși «Renunță».
- **Clasificația**: lista oferă doar clasificațiile pentru care există șabloane de operații; cele configurate pentru furnizor sunt primele. Dacă se potrivește una singură, se alege singură.
- **Documente evidențiere** și **Documente plată**: șabloanele de operații potrivite clasificației; alegi unul cu un clic. Șablonul de evidențiere, dacă este singurul, se alege singur. **Șablonul de plată nu se alege niciodată singur**: ordinul de plată (tabelul de jos) este inactiv până nu alegi un șablon de plată; un clic pe șablonul ales îl scoate din nou.
- **NC (nota contabilă)**: coloana «NC» are o listă cu notele existente ale anului. Alegi una și documentele noi se adaugă la ea, **sau tastezi un număr nou** și se face o notă cu acel număr. Lăsată goală, se face o notă nouă, cu numărul următor (îl vezi în detaliile din josul paginii). Nota este una singură pentru toată salvarea, deci aceeași valoare apare pe toate rândurile. Un text care nu este număr întreg pozitiv nu se acceptă.
- **Subunitatea**: contarea se face în **subunitatea aleasă în bara de titlu**. Lista **Subunitate** stă **înaintea sursei** și apare oricând, pe orice pagină, dacă sursa aleasă are cel puțin două subunități; când o schimbi, pagina se calculează din nou, iar clasificațiile oferite sunt cele din sursa subunității. Cu o singură subunitate lista nu apare. K-BOT în sine nu ține cont de subunitate; operațiile contabile da.
- **Contul IBAN** al furnizorului pentru clasificația aleasă apare sus (nu se modifică aici). **Codul de client** (codul tău la furnizor) îl scrii tu; dacă este scris, intră în explicația ordinului de plată după litera «C».
- În josul paginii, între «Renunță» și «Salvează», vezi furnizorul și **avertismentul cel mai grav** (litere îngroșate = nu se poate continua), iar la final câte altele mai sunt. Dacă treci cu mouse-ul peste această linie, vezi toate avertismentele, codurile de angajament și indicator, numărul următor al notei și **operațiile care s-ar înregistra**, cu debitul și creditul fiecărei linii. Avertismente posibile: furnizor care nu are niciun partener cu codul lui fiscal (sau are mai mulți și nu ai ales încă), factură deja înregistrată integral, notă de credit, factură din alt an, factură care apare de mai multe ori, partener fără cont bancar, coduri neconfigurate.
- Furnizorul se caută în lista de parteneri **numai după codul fiscal** (și partenerii ascunși se găsesc). Dacă **mai mulți parteneri** au același cod fiscal și nu ai mai ales unul pentru el, la bifarea facturilor se deschide o fereastră mică, **«Contări — alegerea partenerului»**: alegi partenerul (dublu clic îl alege direct) și apeși **«Alege»**; cu **«Renunță»** nu se alege nimic, iar salvarea rămâne blocată până alegi. Alegerea **se ține minte** pentru acel cod fiscal și nu se mai cere; partenerul pentru care salvezi evidențierea **devine activ** dacă era ascuns.
- **Tabelul facturilor**: nota, numărul documentului, felul, data, **valoarea rămasă de înregistrat** și **valoarea care se plătește**. Ultima se poate modifica; implicit este tot ce a rămas și nu poate depăși valoarea rămasă. Jos, în tabel, vezi totalurile.
- **Ordinul de plată** (tabelul de jos, o singură linie) este activ doar cât timp ai ales un **șablon de plată**. Pe linie: nota, data (cea mai nouă dată a facturilor), **numărul OP** (îl scrii tu, doar cifre; completarea lui automată vine odată cu salvarea plății), **explicația O.P.** și **valoarea** (suma valorilor care se plătesc). Când bifezi sau debifezi o factură, se schimbă numai valoarea și explicația; nota, data și numărul OP rămân.
- **Explicația O.P.** începe cu «F» și numerele facturilor plătite, apoi «C» și codul de client (dacă l-ai scris), apoi felul facturii din câmpul de sus (luat din șablonul de plată, dacă acesta are unul; îl poți modifica). O poți și scrie sau schimba direct. Sub ea, o **bară** arată câte caractere are, între **1 și 75** (limita oficială a ordinului de plată): se face roșie când explicația e goală și galbenă aproape de limită. Pe ordinul de plată **nu intră diacritice și nici virgule**: orice scrii sau lipești, literele cu diacritice devin litere simple (ă→a, ș→s, ț→t …), iar virgula se înlocuiește cu spațiu, pentru că ANAF refuză ordinul cu diacritice, iar codul de bare citește virgula ca «câmpul următor». Același lucru se întâmplă și în câmpul «Fel factură».
- **Renunță** (stânga jos) scoate toate bifele și golește pagina.
- **Salvează** (dreapta jos) înregistrează facturile bifate. Butonul e activ doar pentru rolul **Contabil**, când ai ales clasificația și șablonul de evidențiere, nu e nicio avertizare îngroșată și, dacă faci ordin de plată, explicația are cel puțin un caracter.

La salvare, avertizările care cer confirmare (notă de credit, factură dintr-un alt an, factură care apare de mai multe ori) îți sunt arătate toate odată și te întreabă dacă continui. Apoi se face **o notă contabilă nouă** și, pentru fiecare factură, **un document** și **o operație** cu debitul și creditul din șablon. Dacă luna facturii era închisă, data documentului se mută pe prima zi a lunii următoare; o factură dintr-un an anterior primește 1 ianuarie al anului de lucru. Poți salva doar ce a rămas de înregistrat, deci o factură deja înregistrată integral nu se mai salvează a doua oară.

**Plata nu se salvează încă**: ordinul de plată se întocmește pe pagină, dar la «Salvează» se înregistrează doar evidențierea; dacă ai ales un șablon de plată, K-BOT te întreabă dacă vrei să continui doar cu evidențierea.

## Arhiva facturilor primite
<!-- slice: 01EF-03, 0000-144 -->

O factură pe care nu o înregistrezi (o copie, o factură greșită) poate fi pusă în **arhivă** și iese din lista de lucru. Doar un utilizator cu rolul **Contabil** o poate face.

- Clic dreapta pe factură, **«Arhivează factura»**. O factură care are deja operații contabile nu se arhivează.
- Butonul din dreapta antetului arborelui are rândul **«Afișare facturi din arhivă»**: bifat, arborele arată **numai** facturile arhivate; nebifat, arată lista de lucru, fără ele.
- Pentru o factură din arhivă, clic dreapta, **«Scoate din arhivă»**. Sfatul facturii spune când și de cine a fost arhivată, dacă se știe.

## Trimiterea la Vercon
<!-- slice: 00EF-45, 00EF-47, 0000-116, 00EF-48, 0000-117, 0123-01, 0000-119, 00EF-49, 0000-122, 00EF-55, 00EF-56, 0000-149, 00EF-59, 0000-150 -->

O factură primită cu produse sau materiale care intră în gestiunea unității se trimite la **Vercon**, ca Administratorul să o preia într-un NIR. **Tu decizi** ce se trimite; Administratorul vede doar ce ai trimis. Doar un utilizator cu rolul **«Contabil»** poate trimite.

- Din ribbon, fila **«VERCON»**, apasă **«Trimite la Vercon»**. Se deschide pagina **«Trimite la Vercon»**, la fel ca «Cumpărare» sau «Recepții»: rămâne deschisă până o închizi.
- În **stânga** este arborele facturilor primite din anul ales (fără notele de credit): pe luni și furnizori, ca în «Cumpărare». Butonul din dreapta antetului arborelui schimbă gruparea în **«Grupare după furnizor»** și înapoi. Fiecare factură are data, numărul, totalul și, după număr, un cuvânt care spune ce s-a întâmplat cu ea în Vercon: **Trimisă în VERCON**, **Înregistrată în VERCON** sau **returnată**; cu mouse-ul deasupra vezi textul întreg. Bifează facturile de trimis, **ale unui singur furnizor odată**: bifa unui **furnizor** le bifează pe toate facturile lui, iar dacă bifezi o factură a altui furnizor, bifele celui dintâi se scot (pagina te anunță). O lună nu are căsuță de bifat. În dreapta apare câte un grup pentru fiecare factură. O factură **trimisă** rămâne bifată și nu se mai poate debifa; cea **înregistrată** în VERCON (preluată într-un NIR) rămâne bifată în gri. Niciuna nu se mai trimite.
- În **dreapta** apar liniile facturilor bifate, grupate pe factură. Se trimit **toate** liniile fiecărei facturi, în afară de cele pe care le marchezi **«NU SE TRIMITE»**.
- Pe fiecare linie poți alege, dacă vrei, **contul din Vercon**. Lista arată conturile grupate pe gestiune, din toate gestiunile; un clic pe numele unei gestiuni o strânge sau o desface, iar dacă tastezi o parte din numele contului sau al gestiunii, rămân doar rândurile potrivite. Contul ales pe rândul unei facturi se scrie pe toate liniile ei. **Lista de lângă notă** («Cont pentru toate liniile») arată aceleași conturi, pe gestiuni: ce alegi acolo se scrie pe **toate liniile din tabel**, din toate facturile bifate; apoi poți schimba contul pe o factură sau pe o linie. **«(fără cont)»** lasă alegerea Administratorului. **«NU SE TRIMITE»**, din aceeași listă (pe o linie, pe antetul unei facturi sau în lista de lângă notă), face ca linia să **nu ajungă deloc în Vercon**: Administratorul nu o vede în factură, nu apare în NIR și nicăieri altundeva. Linia rămâne neatinsă în e-Factura. O factură trebuie să aibă cel puțin o linie care se trimite.
- Contul este o **propunere**: Administratorul îl găsește ales când preia factura și îl poate schimba.
- Sub tabel vezi unde merg liniile (câte pe fiecare gestiune și cu ce valoare, plus cele fără cont). În câmpul **Notă**, de lângă lista de conturi, poți scrie o notă pentru Administrator.
- Nu se trimit notele de credit (storno). Furnizorul facturii se caută în lista de parteneri **numai după codul fiscal**; dacă niciun partener nu are acel cod fiscal, factura nu se trimite și pagina spune că trebuie adăugat întâi partenerul. Dacă **mai mulți parteneri** au același cod fiscal, la apăsarea butonului de trimitere se deschide o fereastră mică, **«Trimite la Vercon — alegerea partenerului»** (numai dacă nu ai ales deja unul pentru acel cod fiscal), cu partenerii (denumire, adresă, cont IBAN și dacă sunt **activi** sau **ascunși**): alegi unul (dublu clic îl alege direct) și apeși **«Alege»**; cu **«Renunță»** nu se trimite nimic. Partenerul pentru care se trimite factura **devine activ** dacă era ascuns, iar alegerea **se ține minte** pentru acel cod fiscal: data viitoare (la Vercon și la «Contări») nu mai ești întrebat. Dacă un cod fiscal are un singur partener, acela se folosește fără să te întrebe. Dacă numărul facturii pare deja înregistrat în Vercon, pagina te avertizează.
- Sfatul unei facturi din arbore spune unde e ea: **«Trimisă, așteaptă Administratorul»**, **«Preluată în NIR …»** (cu numărul NIR) sau **«Returnată: motivul»**. O factură returnată se poate trimite din nou. Dacă un NIR făcut din ea este anulat în Vercon, factura așteaptă iarăși Administratorul.

## Sincronizarea facturilor primite
<!-- slice: 00EF-18, 0000-61, 0119, 0114-01, 00EF-30, 0000-76, 00EF-32, 0000-78, 00EF-33, 0000-79, 00EF-34, 0114-10, 0114-14, 0000-90, 0114-17, 0114-18, 0000-91 -->

Butonul **«Sincronizează facturi primite»**, din grupul din dreapta al ribbonului (lângă «Închide»), se vede doar cât este deschisă macheta **«Cumpărare»**; lângă el apar două casete, care arată ca rândurile meniului din subsolul arborelui (mai jos):

- o **listă** cu textul gri **«Perioada de adus»**: se deschide la clic și are ultimele 7, 15, 30, 45 sau 60 de zile; în ea nu se scrie, doar se alege. Dacă nu alegi nimic și nu scrii nimic în caseta de lângă ea, sincronizarea ia ultimele 7 zile;
- o **casetă pentru zile**, goală la început, cu textul gri **«Între 1 și 60 de zile»**: scrii un număr între 1 și 60 și sincronizarea ia atunci acest număr de zile, nu pe cel din listă. Dacă o lași goală, se folosește lista. Un număr în afara intervalului 1–60 nu pornește sincronizarea și primești un mesaj.

**Din subsolul arborelui**: sub arborele facturilor primite (în macheta «Cumpărare» și în vederea «E-Factura» de la fundamentare) este un subsol **«Actualizează»** (cu aceeași pictogramă ca subsolul listei de angajamente). Un clic pe el deschide deasupra lui un mic meniu cu o **listă** a perioadelor (7, 15, 30, 45, 60 de zile) — alegerea unei perioade pornește actualizarea — și o **casetă** în care scrii un număr de zile între 1 și 60; **Enter** pornește actualizarea. Un număr în afara intervalului nu pornește nimic și primești un mesaj; Cele două rânduri (lista și caseta) au pictograma ANAF și arată un text gri («Perioada de adus», «Între 1 și 60 de zile»); textul din casetă dispare când scrii prima cifră. Sub ele, după o linie de separare, este rândul **«Sincronizează cu serverul»**: citește din nou lista facturilor de pe serverul K-BOT (nu merge la ANAF), iar factura afișată rămâne afișată. Esc sau un clic în afara meniului îl închid. Cât durează o actualizare de la ANAF, subsolul scrie cât s-a adus.

Apeși butonul (sau alegi din meniul subsolului) și sincronizarea pornește direct, fără fereastră:

- se aduc de la ANAF doar facturile care nu sunt deja în baza de date, câte o tranșă; subsolul arborelui arată cât s-a făcut; poți porni sincronizarea de câte ori vrei, nu se dublează nimic (ANAF păstrează mesajele cel mult 60 de zile);
- la sfârșit linia scrie că facturile primite sunt la zi, iar lista se citește din nou dacă s-au adus facturi; dacă un mesaj nu a putut fi citit, primești un mesaj, iar la următoarea sincronizare se încearcă din nou;
- apoi K-BOT arată ce a adus: o fereastră cu **tabelul facturilor noi** (data, numărul, furnizorul, codul fiscal, totalul) și «OK», sau, dacă nu s-a adus nimic, mesajul **«Nu există facturi noi pe serverul ANAF.»**; facturile din alt an decât cel ales în K-BOT nu apar în tabel (mesajul spune câte s-au descărcat);
- sincronizarea are nevoie de un token ANAF valabil (butonul **«Token ANAF»**, alături în ribbon).

## E-Factura Cumpărare: «Facturi primite»
<!-- slice: 00EF-19, 00EF-20, 0000-62, 0000-63, 0113, 0119, 00EF-27, 00EF-29, 0000-75, 0114-01, 00EF-30, 0000-76, 00EF-31, 0000-77, 00EF-33, 0000-79, 00EF-34 -->

Toate facturile primite de unitate, pe anul ales în K-BOT, se văd în macheta **«Facturi primite»**. Bara de vederi din dreapta (Conținut, Factură PDF …) are aceeași înălțime ca antetul arborelui și începe în dreptul tabelelor de dedesubt; tabelele nu mai au sfaturi la trecerea mouse-ului. O deschizi din ribbon, **E-Factura › E-Factura › «Cumpărare»**, ca pagină a ferestrei principale; o închizi cu **«Închide»** din ribbon sau cu Esc.

<!-- capture: efactura-cumparare | caption: Macheta «Facturi primite», deschisă din E-Factura › Cumpărare | prepare: Din ribbon, E-Factura › Cumpărare, pe o unitate care are facturi primite; alegeți o factură din arbore. | redo: 2026-10-17 21:00 | why: 0114-17: subsolul arborelui scrie «Actualizeaza», cu pictograma listei de angajamente; 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 00EF-29: arborele are furnizori, cautarea e in antet, fara TVA pe cote; KE02: «Continut» are doua coloane noi (valoare cu TVA, clasificatie) -->

- fereastra are același arbore și aceleași vederi ca vederea «E-Factura» de la fundamentare (rădăcina «Toate facturile», lunile, furnizorii, **Conținut**, **Factură PDF**, **Mesaje**; clic dreapta oferă «Salvează ca ZIP» și «Salvează ca XML»), dar fără lista „doar a unui DDF";
- căutarea este lupa din antetul arborelui: scrii o parte din numele furnizorului, din data sau din numărul facturii și arborele rămâne doar cu rândurile potrivite; Esc o golește;
- sincronizarea cu ANAF se pornește din subsolul **«Actualizează»** al arborelui sau din ribbon, cu butonul **«Sincronizează facturi primite»** care apare cât macheta e deschisă (vezi mai sus); progresul se vede în subsol și, după ce s-au adus facturi noi, lista se citește din nou;
- clic dreapta pe o factură oferă și **«Leagă de un DDF…»**: se deschide o listă a fundamentărilor, cu o casetă de căutare (după angajament, obiect, partener sau cod fiscal); alegi una (dublu clic o alege direct) și apeși **«Leagă»**. Legăturile făcute de tine se văd în vederea «E-Factura» a angajamentului respectiv; după ce ai deschis factura (clic pe ea), meniul ei oferă și **«Scoate legătura cu …»** pentru fiecare;
