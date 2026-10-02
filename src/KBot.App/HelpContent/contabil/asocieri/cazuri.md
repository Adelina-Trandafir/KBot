---
id: contabil.asocieri.cazuri
title: Asocieri — cazuri speciale
part: contabil
order: 30
parent: contabil.asocieri
screens: AsociereForm.btnReseteaza, AsociereForm.btnRenunta, AsociereForm.ntfMesaj
keywords: fara schimbare, nu consemneaza nicio schimbare, randul de stergere, receptie stearsa, receptie noua, reconstituita, legatura blocata, golește asezarile, renunta, lantul nu se inchide
---
<!-- slice: 0048-04, 0061 -->
Toate comenzile de mai jos sunt în **meniul de clic dreapta** al unui instantaneu (sau al unui grup
de instantanee alese cu Ctrl / Shift).

## O salvare fără nicio schimbare — «Nu consemnează nicio schimbare»
<!-- slice: 0048-04, 0064 -->

Cineva a deschis o recepție pe site și a salvat-o fără să schimbe nimic. Istoricul a primit un
instantaneu întreg, cu **aceleași cifre** ca salvarea de dinainte. Un astfel de instantaneu nu spune
nimic nou:

- clic dreapta pe el în coș › **«Nu consemnează nicio schimbare»**; rămâne în coș, marcat
  **[fără schimbare]**, și nu mai trebuie așezat;
- **«Consemnează o schimbare»** scoate marcajul.

K-BOT arată când un instantaneu e identic cu cel dinainte din lanț, dar **nu hotărăște niciodată
singur**: două instantanee cu aceleași cifre pot fi și o salvare fără schimbare, și o modificare
reală a **altei** recepții care are întâmplător aceeași valoare. Tu știi care e.

> A ignora o salvare fără schimbare nu pierde nimic. A o pune pe recepția greșită strică cifrele.
> Când ai de ales între cele două, alege «fără schimbare».

## O recepție ștearsă pe site — «Este rândul de ștergere»
<!-- slice: 0048-04, 0059-02 -->

Ștergerea unei recepții lasă în istoric un rând «Stergere receptie», cu valoarea pe care o avea
recepția atunci. Acela e **ultimul** instantaneu din lanțul ei:

- așază-l pe recepție ca pe oricare altul, apoi clic dreapta › **«Este rândul de ștergere»**; rândul
  se scrie înclinat, cu **[ștergere]**;
- după ștergere recepția **nu mai poate avea alte instantanee**;
- **«Nu mai e rândul de ștergere»** anulează.

O recepție ștearsă rămâne în socoteală pentru plățile **dinaintea** ștergerii și iese din ea după.

## O recepție care nu mai există nicăieri — «Începe o recepție nouă»
<!-- slice: 0059, 0059-02, 0062 -->

Dacă o recepție a fost creată **și** ștearsă pe site înainte ca K-BOT să fi descărcat vreodată
angajamentul, ea nu apare în lista recepțiilor, dar instantaneele ei sunt în istoric. Pentru ele:

1. clic dreapta pe **primul** ei instantaneu din coș › **«Începe o recepție nouă»** (sau pe un grup,
   «Începe o recepție nouă din toate»); apare în stânga o recepție **[nouă]**;
2. trage pe ea restul instantaneelor ei;
3. dacă a fost ștearsă, marchează ultimul instantaneu **«Este rândul de ștergere»** — altfel se
   scrie ca recepție reconstituită **neștearsă**;
4. **«Renunță la recepția nouă»** (clic dreapta pe ea) o desface și pune instantaneele înapoi în coș.

Reguli: instantaneul care pornește recepția nu poate fi și rândul ei de ștergere, iar rândul de
ștergere trebuie să fie ultimul din lanț. Cât nu sunt respectate, K-BOT spune ce lipsește și nu
salvează.

> Dacă pe un angajament ajung **două sau mai multe** recepții reconstituite, K-BOT le marchează
> **«⚠ Reconstituire nesigură»**: gruparea instantaneelor a fost o judecată a ta, nu o verificare.
> Marcajul rămâne, ca o cifră care nu se leagă peste luni să poată fi urmărită până aici.

## Legături blocate
<!-- slice: 0048-04, 0058-02, 0056-02 -->

Singurul lucru care blochează o legătură este o **ordonanțare**: dacă angajamentul are o ordonanțare
din ziua instantaneului sau de după ea, instantaneul se vede stins, cu lacăt, și nu se mai poate
muta. Clicul dreapta spune «Această legătură nu se mai poate modifica» și motivul (numărul și data
ordonanțării). Ca să-l muți, trebuie întâi schimbată sau ștearsă ordonanțarea. O simplă plată nu
blochează nimic.

Regula e aceeași **oricând** deschizi fereastra, deci și **după o descărcare**: orice legătură
fără ordonanțare se poate corecta, fie că e veche, fie că abia a venit.

## Refuzuri la tragere
<!-- slice: 0048-07, 0065 -->

- **«Instantaneul pierde indicatorii ..., prezenți mai devreme în lanțul recepției. Un indicator
  poate cădea la zero, dar nu poate dispărea.»** — instantaneul nu e al acestei recepții, sau un
  instantaneu mai vechi din lanț e pus greșit.
- **«Instantaneul este mai nou decât ultimul din lanț (...) și ar schimba valoarea recepției: are ...,
  recepția valorează ...»** — valoarea o dă **recepția**, nu instantaneele. Un instantaneu mai nou
  decât ultimul din lanț ar deveni capătul lanțului, deci trebuie să aibă exact valoarea recepției.
  Dacă n-o are, recepția nu se aprinde și aruncarea nu se face. Unul mai vechi decât ultimul se
  așază oricum. Dacă tragi deodată mai multe, contează doar cel care ar ajunge ultimul. Regula nu
  privește recepțiile noi (pornite în fereastră), recepțiile șterse, recepțiile cu detaliul venit
  incomplet din FOREXE, nici o recepție fără niciun instantaneu.
- **«Instantaneul este deja pe această recepție.»** — nimic de făcut.

## După o descărcare: salvare sau renunțare
<!-- slice: 0055, 0058, 0056-02 -->

- **Salvarea** se poate face abia când fiecare instantaneu adus de descărcare are o hotărâre:
  așezat, «fără schimbare» sau pe o recepție nouă.
- **Legăturile vechi se pot corecta tot aici.** Instantaneele care erau deja așezate (sau în coș)
  înainte de descărcare se mută la fel ca cele noi: le tragi pe altă recepție, în coș, le marchezi
  din meniul de clic dreapta. Nu ești obligat să le atingi — pe cele nemutate K-BOT le lasă exact
  cum sunt și scrie doar ce ai schimbat. Rămân pe loc numai cele blocate de o ordonanțare (vezi
  «Legături blocate», mai sus).
- De ce contează: dacă un instantaneu vechi stă pe recepția greșită, lanțul ei nu se mai închide și
  salvarea descărcării e refuzată cu «... Lanțul nu se închide.» Acum îl muți pe recepția lui în
  aceeași fereastră și salvezi, fără să renunți la descărcare.
- Dacă iei un instantaneu de pe o recepție și nu pui altul în loc, iar lanțul ei rămâne neînchis,
  K-BOT salvează și te **avertizează** după salvare; nu refuză.
- **«Golește așezările»** te duce înapoi la început: tot ce a adus descărcarea trece în coș, iar
  marcajele tale se anulează. Legăturile vechi pe care le-ai mutat aici revin la cum sunt pe
  server; cele neatinse nu se schimbă.
- **Renunțarea** (sau închiderea ferestrei) **aruncă toată descărcarea**: nimic nu ajunge în tabele —
  nici recepțiile, nici plățile, nici istoricul. K-BOT întreabă întâi; descărcarea va trebui reluată.

În fereastra deschisă **oricând** (din Recepții), «Salvează legăturile» scrie doar ce ai schimbat, iar
închiderea fără salvare pierde doar mutările tale nesalvate (K-BOT te întreabă).

## «Lanțul nu se închide» oprește descărcările și ordonanțările
<!-- slice: fara-felie, 0101 -->

Dacă o recepție are ca **ultim instantaneu** unul cu **altă valoare** decât a ei (de obicei un
instantaneu pus pe recepția greșită), K-BOT **nu pornește**:

- descărcarea completă a angajamentului și reîmprospătarea recepțiilor sau a rezervărilor;
- o ordonanțare nouă (și nici generarea în lot).

Mesajul arată recepțiile cu pricina (data, valoarea ei, ultimul instantaneu și valoarea lui) și
întreabă dacă deschide fereastra de asociere. Acolo muți instantaneul pe recepția lui (sau în coș)
și apeși «Salvează legăturile». După salvare K-BOT verifică din nou și, dacă totul se închide,
continuă singur operația cerută. Dacă închizi fereastra fără să salvezi, operația nu pornește.

Cum îl găsești dinainte: angajamentul cu un astfel de lanț este scris **cu roșu** în lista din fereastra
principală (indicația lui spune ce recepții sunt și ce valori au), iar în fereastra de asociere
recepția și ultimul ei instantaneu sunt tot roșii.

Motivul: fără verificarea asta, descărcarea rula până la capăt și abia salvarea ei era refuzată cu
«... Lanțul nu se închide.», iar o ordonanțare s-ar fi făcut pe cifre greșite ale recepțiilor.

### Două recepții din aceeași zi, adunate într-una

Până la actualizarea din 30.09.2026, K-BOT deosebea recepțiile doar după dată, așa că două recepții
din **aceeași zi** ajungeau în K-BOT ca **una singură**, cu instantaneele amândurora pe ea. Acum le
deosebește după ordinea lor în lista din FOREXE. Ca să le desparți:

1. În fereastra de asociere (se deschide din mesajul de blocare sau din vederea Recepții), trage
   **în coș** instantaneele celeilalte recepții din ziua aceea — de obicei cele cu valoarea ei — până
   când ultimul instantaneu rămas are valoarea recepției din K-BOT. Apasă «Salvează legăturile»:
   aici salvarea e permisă și cu instantanee rămase în coș.
2. Reîmprospătează recepțiile angajamentului: K-BOT creează recepția care lipsea.
3. Deschide din nou fereastra de asociere și trage instantaneele din coș pe recepția nouă.

## Dacă altcineva a lucrat între timp
<!-- slice: 0048-04 -->

Dacă între deschiderea ferestrei și salvare altcineva a modificat recepțiile aceluiași angajament,
K-BOT nu scrie nimic și îți spune; închide și deschide din nou fereastra.
