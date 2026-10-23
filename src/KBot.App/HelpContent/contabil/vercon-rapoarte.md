---
id: contabil.vercon.rapoarte
title: VERCON: Rapoarte
part: contabil
order: 78
parent: contabil
screens: VerconRapoarteForm
keywords: vercon, rapoarte, raport, balanta, fisa magazie, fisa produsului, stoc final, inventar, nir, bon consum, centralizator, lista alimente, obiecte de inventar, mijloace fixe, amortizare, gestiune, subunitate, perioada, luna, selectare date, pdf, excel, previzualizare
---
<!-- slice: 0123-02 -->
Rapoartele **VERCON** se fac acum direct în K-BOT, cu aceleași alegeri și aceleași cifre ca pe site. Din ribbon, fila **VERCON**, apasă **«Rapoarte»**. Se deschide o pagină care rămâne deschisă până o închizi. Ai nevoie de dreptul de rapoarte în VERCON pentru unitate; dacă nu îl ai, pagina spune asta și nu arată nimic.

## Subunitate, gestiune, an
<!-- slice: 0123-02 -->

Primele rânduri din stânga sunt cele de pe site:

- **Subunitate**: o alegi dacă unitatea are mai multe; dacă are una singură, e aleasă.
- **Gestiune**: apare doar când subunitatea are mai multe gestiuni. Tipul gestiunii (materiale, alimente, obiecte de inventar, mijloace fixe) hotărăște ce rapoarte primești.
- **An**: exercițiul gestiunii. La obiecte de inventar și la mijloace fixe nu există an, perioada se alege direct.

O schimbare de sus în jos șterge ce era sub ea: dacă schimbi subunitatea, gestiunea și raportul ales se aleg din nou.

## Perioada
<!-- slice: 0123-02 -->

**Perioada** este o listă cu anii, cel mai nou primul. Sub fiecare an găsești **«Tot anul»** și lunile în care a fost mișcare, cea mai nouă prima; apeși pe an ca să-l desfaci sau să-l strângi. Poți și să scrii în ea («2025», «martie»). Sus în listă mai pot fi **«Anul curent până azi»** și **«TOTI anii»**.

Alegerea din listă pune datele **Dată început** și **Dată sfârșit**. Dacă schimbi tu o dată, lista arată **«Interval personalizat»**. La rapoartele „la o dată” (amortizarea) rămâne doar **La data**.

## Tipul raportului
<!-- slice: 0123-02 -->

Rapoartele gestiunii sunt grupate (de exemplu Produse, Intrări, Ieșiri, Liste de alimente; la obiecte de inventar și mijloace fixe, grupele lor). Apeși pe numele unui raport ca să-l alegi; o grupă se deschide și se strânge cu un clic. Sub perioadă apar, doar la rapoartele care le au, **opțiunile** raportului:

- **Grupează pe cont, cu pagină nouă** și **Afiseaza FARA miscari** (balanțe);
- **Afișează Documente** (alegi datele pe documente, nu pe produse);
- **Afișează Primitori** (alegi datele pe primitori);
- **Grupează după Primitori** (amortizare).

## Selectare date
<!-- slice: 0123-02 -->

Coloana din mijloc este lista datelor raportului (produse, conturi, furnizori, documente, bunuri). Fiecare rând are o căsuță. **Fără nicio bifă se folosesc toate datele.** Ramurile se citesc de pe server când le deschizi; dacă bifezi o ramură neîncărcată, ea se citește întâi, apoi se bifează tot ce e sub ea. **«Selectează TOATE»** citește tot și bifează tot. Lupa din antet caută în tot arborele (minim 3 caractere); căutarea citește întâi ramurile rămase.

Unele rapoarte cer o alegere precisă (de exemplu „un singur produs”); sub listă scrie ce lipsește, iar butoanele rămân gri până alegi.

## Listare și Excel
<!-- slice: 0123-02 -->

- **Listare**: serverul pregătește raportul cu perioada, opțiunile și datele alese, iar PDF-ul apare în dreapta, potrivit pe lățimea panoului. Îl poți tipări, salva sau mări cu butoanele vizualizatorului. Alt raport îl înlocuiește pe cel de pe ecran.
- **Export Excel**: pregătește același raport și îl salvează ca fișier Excel, cu toate coloanele; alegi unde.

Dacă schimbi ceva după ce ai cerut raportul (perioada, o bifă, o opțiune), următorul **Listare** sau **Export Excel** pregătește raportul din nou. Cât se așteaptă răspunsul serverului, butoanele sunt gri și sub bara de titlu aleargă o bară de încărcare. Orice problemă (nu ai drepturi, gestiunea nu mai există, serverul nu răspunde) apare pe linia de sub raport, cu textul serverului.
