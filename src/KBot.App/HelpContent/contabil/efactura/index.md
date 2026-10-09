---
id: contabil.efactura
title: E-Factura: facturile emise
part: contabil
order: 75
parent: contabil
screens: FacturiForm, DateUnitateForm
keywords: efactura, e-factura, factura electronica, facturi emise, arbore, trimitere, trimite factura, validare, stornare, storno, factura clasica, pdf, eroare anaf, factura, client, cumparator, date unitate, unitate emitenta, cont emitent, iban, unitate de masura, linii, ciorna, serie, numar factura, anaf, token, certificat, certificat calificat, autorizare, reinnoire, spv, pin, expirat
open: menu:efactura
---
<!-- slice: 00EF-05, 00EF-08, 00EF-09, 00EF-13, 0000-54, 0000-55, 0000-57, 0000-58 -->
Din **MENIU › E-Factura** se deschide fereastra **«E-Factura — facturi emise»**: aici pregătești facturile electronice ale unității, pe care le trimiți apoi la ANAF. Pentru a lucra cu ANAF, K-BOT are nevoie și de un **token ANAF** (o autorizare pe care o dai o singură dată, cu certificatul calificat al unității, și o reînnoiești o dată pe an); îl vezi și îl reînnoiești din butonul **«Token ANAF»** — vezi [Tokenul ANAF: obținerea și reînnoirea](topic:contabil.efactura.token).

<!-- capture: efactura-facturi | caption: Fereastra «E-Factura — facturi emise» | goto: menu:efactura | prepare: Deschideți fereastra pe o unitate care are deja câteva facturi, în stări diferite, și alegeți una din arbore. -->

Fereastra se deschide doar cât ai o unitate deschisă; altfel K-BOT spune «Nu există o unitate deschisă: autentificați-vă întâi.». Poți lucra în paralel în fereastra principală, iar dacă o ceri a doua oară, K-BOT o aduce în față pe cea deschisă.

## Arborele facturilor
<!-- slice: 00EF-09, 00EF-13, 0000-57, 0000-58 -->

În stânga vezi **clienții** unității, în ordine alfabetică; sub fiecare client sunt **facturile lui**, cea mai nouă prima, cu numărul, data și totalul. Pe rândul clientului vezi totalul facturilor lui. Lupa din antet deschide căutarea în arbore (după client sau după număr); Esc o golește. O factură aleasă se vede în dreapta, într-o bară cu **vederi** (Generale, Cumpărător, Atașamente, Conținut și, după starea facturii, vederile cu document — vezi mai jos).

**Bulina din stânga** unei facturi spune unde a ajuns:

- **cerc gri, gol** — **netrimisă** (ciornă): a fost salvată, dar nu a plecat la ANAF; doar o ciornă se poate modifica;
- **portocaliu** — **trimisă, dar neconfirmată**: a plecat la ANAF și rezultatul nu este încă cunoscut; factura nu se modifică până aflați rezultatul;
- **verde** — **acceptată** de ANAF;
- **roșu** — **refuzată** de ANAF; motivul se vede în vederea **Eroare ANAF**.

La sfârșitul rândului poate apărea **· stornare** (factura anulează o alta). Când treci cu mouse-ul peste o factură, în dreapta rândului apare **semnul cu trei puncte**: apăsat, deschide **meniul facturii**.

## Meniul unei facturi
<!-- slice: 00EF-09, 0000-57 -->

<!-- capture: efactura-meniu-factura | caption: Meniul unei facturi acceptate | goto: menu:efactura | prepare: Treceți mouse-ul peste o factură acceptată din arbore și apăsați semnul cu trei puncte din dreapta rândului. -->

Apeși semnul din dreapta rândului ei. Factura se deschide în dreapta (dacă nu era deja) și meniul arată doar ce se poate face cu ea acum:

| Starea facturii | Ce oferă meniul |
|---|---|
| Netrimisă (ciornă) | **Trimite factura în ANAF**; **Modifică factura** (doar o ciornă obișnuită, nu o factură de stornare) |
| Trimisă, neconfirmată | **Validează la ANAF (starea facturii)** |
| Refuzată | **Afișează eroarea ANAF** |
| Acceptată | **Listează factura clasică (PDF)**; **Listează factura ANAF (PDF)**; **Stornează factura în ANAF** (dacă nu a fost deja stornată) |

Un separator desparte, în meniu, trimiterea și verificarea de listări, iar listările de stornare și modificare.

## Trimiterea la ANAF
<!-- slice: 00EF-09, 0000-57 -->

1. Din meniul unei ciorne alege **«Trimite factura în ANAF»** și confirmă.
2. K-BOT verifică factura. Dacă are erori, ți le arată într-o listă și **nu o trimite**; corectezi (**«Modifică factura»**) și încerci din nou. Dacă verificarea trece, factura se trimite.
3. K-BOT așteaptă câteva secunde răspunsul ANAF, apoi îl citește o dată. Rezultatul apare în chenarul de sus: **verde** — acceptată; **roșu** — refuzată, cu motivul; **galben** — încă în prelucrare.
4. Dacă factura este încă în prelucrare, bulina rămâne portocalie. Peste câteva momente alege din meniul ei **«Validează la ANAF (starea facturii)»**, care citește din nou starea. Se poate repeta până ANAF dă un răspuns.

Dacă ANAF nu mai acceptă tokenul unității, mesajul te trimite la butonul **«Token ANAF»** din subsolul ferestrei. O factură trimisă nu se mai modifică și nu se șterge; una acceptată nu se mai trimite a doua oară.

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
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57, 00EF-13, 0000-58 -->

Numărul facturii, **Data facturii** (scrisă sau aleasă din calendar; se schimbă doar la o factură nouă sau la ultima factură a seriei, și nu înainte de factura anterioară), **Tip factură** (380 — factură; 384 — factură corectată; îl pune K-BOT), **Stare**, **Comentarii factură** (cel mult 255 de caractere), **Ref. comandă (BT-13)** (cel mult 30), **Cont emitent (IBAN)** — contul în care se plătește **această factură**; alegi unul folosit pe facturile anterioare sau scrii altul — și **TOTAL FACTURĂ**, care se adună singur din linii. Sub ele, K-BOT scrie în cuvinte situația facturii (ciornă, acceptată, motivul unui refuz, ce stornează).

## Vederea Cumpărător
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57, 00EF-15 -->

Sus alegi clientul din listă; tastează o parte din denumire sau din codul fiscal ca să-l găsești. Câmpurile de dedesubt îl arată: **persoană fizică** (bifă; atunci «Cod fiscal» este CNP-ul), **cod fiscal** și, pe același rând, **prefix fiscal** («RO» pentru plătitor de TVA), **denumire**, **județ**, **oraș**, **sector** (doar pentru București), **adresă**, **cont (IBAN)** și **bancă**.

- **«Client nou»** golește câmpurile pentru un client nou și le activează; până îl apeși, câmpurile sunt inactive și doar lista de clienți se poate folosi. Cursorul merge pe **Cod fiscal**: după ce l-ai scris (și ai trecut mai departe sau ai apăsat Enter), K-BOT întreabă ANAF și completează singur denumirea, prefixul, județul, orașul și adresa. Verifici datele și apeși «Salvează clientul». Dacă ANAF nu răspunde sau nu cunoaște codul, primești un mesaj și completezi de mână.
- **«Salvează clientul»** îl scrie în baza de date **separat de factură** și îl alege pentru ea. Dacă ai schimbat câmpurile unui client și nu l-ai salvat, factura nu se salvează până nu apeși «Salvează clientul».
- **«Șterge clientul»** îl șterge; un client care are facturi nu se poate șterge.

Câmpurile se pot schimba doar cât factura este deschisă pentru modificare.

## Date unitate
<!-- slice: 00EF-13, 0000-58, 00EF-14 -->

Datele unității care emite facturile (aceleași pe toate facturile) se țin într-o fereastră a lor, **«Date Unitate»**. O deschizi din butonul cu litera K din bara de titlu a ferestrei de facturi, care desface un meniu cu două rânduri: **«Date Unitate»** și **«Conturi Unitate»**. Dacă unitatea nu are încă aceste date, fereastra se deschide singură la intrare, iar facturi noi nu poți adăuga până nu le salvezi. La deschidere, dacă denumirea este goală, K-BOT ia singur de la ANAF denumirea, județul, orașul și adresa, după codul fiscal al unității; restul le completezi tu.

<!-- capture: date-unitate | caption: Fereastra «Date Unitate» | prepare: Din fereastra de facturi apăsați «Date unitate», pe o unitate cu datele completate. -->

- **Denumire** și **Cod fiscal** (cifre, cu «RO» în față dacă unitatea plătește TVA) sunt obligatorii;
- **Județ** și **Oraș**, apoi **Adresă**;
- **Telefon** — zece cifre; punctele se pun singure (0721.123.456); poate rămâne gol;
- **E-mail**;
- **Seria facturilor** și **Primul număr** (contează doar pentru o serie care nu are încă nicio factură). **După prima factură emisă, cele două nu se mai pot schimba** și apar blocate.

**«Salvează»** scrie datele; **«Ieșire»** închide fereastra (dacă ai modificări nesalvate, K-BOT întreabă). Atenție: o modificare are efect asupra tuturor documentelor generate de aici înainte.

**«Preia de la ANAF»** completează din nou denumirea, județul, orașul și adresa unității după codul ei fiscal din lista unităților și le scrie imediat. Îl poți folosi oricând, dacă ai schimbat de mână aceste câmpuri și vrei să le readuci; K-BOT întreabă înainte, pentru că ce ai scris în ele se înlocuiește.

Conturile bancare ale unității sunt în fereastra **«Conturi Unitate»**, din același meniu — vezi [Conturile unității emitente](topic:contabil.efactura.conturi).

## Vederea Atașamente
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57 -->

Bifa **«Atașează factura originală»**; alegerea se păstrează pe factură și se schimbă doar cât factura este o ciornă deschisă pentru modificare.

## Vederea Conținut
<!-- slice: 00EF-08, 0000-55, 00EF-09, 0000-57 -->

Liniile facturii sunt într-un tabel în care scrii direct: **Nr**, **Conținut**, **Um**, **Cant**, **PU** și **Valoare**. 

- **«Linie nouă»** adaugă o linie la sfârșit, cu unitatea **XPP** (bucată) și cantitatea 1;
- **Um** se alege din lista unităților de măsură (tastează codul sau o parte din nume); dacă lista nu se poate citi, scrii codul de mână;
- **Cant** poate avea cel mult 3 zecimale, **PU** cel mult 4; se scriu cu virgulă sau cu punct;
- **Valoare** se calculează singură (cantitate × preț, 2 zecimale) și nu se scrie;
- **«✕»** de pe un rând șterge linia; totalul din subsolul tabelului și din vederea **Generale** se actualizează.

## Factură PDF, Factură ANAF și Eroare ANAF
<!-- slice: 00EF-09, 0000-57 -->

După starea facturii, pe bara de vederi apar vederile cu document. Ele se deschid și din meniul facturii, și au același vizualizator PDF ca vederea «Document PDF» de la DDF (din el se poate și tipări):

- **Factură PDF** (factură acceptată) — factura în formatul clasic, întocmit de K-BOT: furnizorul, cumpărătorul, liniile și totalul;
- **Factură ANAF** (factură acceptată) — factura așa cum o desenează ANAF din fișierul pe care îl păstrează. K-BOT o aduce de la ANAF, deci are nevoie de conexiune și de un token valabil; cât se aduce, vizualizatorul spune «Se aduce factura de la ANAF…»;
- **Eroare ANAF** (factură refuzată) — motivul dat de ANAF, pe o pagină, cu fiecare motiv pe rândul lui.

Cât timp scrii sau modifici o factură, aceste vederi nu se văd. Dacă nu există vizualizator PDF în fereastră, vederea spune acest lucru.

## Tokenul ANAF
<!-- slice: 00EF-05, 0000-54, 0000-55, 0000-56 -->

Butonul **«Token ANAF»** din subsolul ferestrei deschide fereastra tokenului, unde vezi starea lui și îl obții sau îl reînnoiești cu certificatul calificat al unității. Pașii, mesajele și ce faci când ceva nu merge sunt în [Tokenul ANAF: obținerea și reînnoirea](topic:contabil.efactura.token).
