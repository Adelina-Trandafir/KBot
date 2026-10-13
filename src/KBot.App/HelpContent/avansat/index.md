---
id: avansat
title: Opțiuni avansate
part: avansat
order: 0
screens: ParolaAvansataForm, SetariAplicatieView.chkAvansate
keywords: optiuni avansate, parola, setari avansate, activare
---
<!-- slice: 0072-02 -->
Opțiunile avansate sunt setările care schimbă felul în care K-BOT lucrează pe calculator:
cum afișează documentele, cum arată pagina FOREXE, tema și scalarea, folderele în care scrie.
Sunt ascunse ca un operator să nu le schimbe din greșeală.

## Cum le pornești
<!-- slice: 0072-02, 0100-02, 0114 -->

1. Deschide **Setări** (din ribbon, **Configurări › Configurare › «Configurare K-BOT»**), pagina **Aplicație**.
2. Bifează **«Activează opțiuni avansate»**. K-BOT cere **parola** opțiunilor avansate.
3. Cu parola corectă apar:
   - fila **Documente** în pagina «Aplicație»;
   - paginile **Pagina FOREXE**, **Temă** și **Căi fișiere**;
   - în pagina **FOREXE**, secțiunea **Istoric (avansat)** — vezi mai jos.

Debifarea nu cere parolă și ascunde din nou paginile. Setările făcute rămân în vigoare și cât
timp paginile sunt ascunse.

<!-- capture: avansat-activare | caption: «Activează opțiuni avansate» în Setări › Aplicație | goto: setari:aplicatie | redo: 2026-10-01 23:30 | why: 0100-02: comutatoarele paginii sunt regrupate, iar sub «Activează opțiuni avansate» nu mai apare grupul «Descărcări multiple» (e pagina separată «Descărcări multiple»). -->

> Parola nu e o protecție de securitate, ci o barieră împotriva unei schimbări făcute din
> neatenție. Nu o da operatorilor care nu au nevoie de ea.

## Ștergerea ultimei luni din istoric
<!-- slice: 0092-01, 0000-68 -->

În **Setări › FOREXE**, secțiunea **Istoric (avansat)** (se vede doar cât timp opțiunile avansate
sunt pornite) arată ultima lună din istoricul FOREXE și ce conține: rânduri de istoric,
angajamente, rezervări, recepții și plăți. Butonul **«Șterge ultima lună din istoric…»** o șterge,
ca valorile ei să se refacă la următoarea descărcare din FOREXE.

- Se poate șterge **doar o lună întreagă** și **doar ultima lună**, în ordine cronologică.
- Dacă luna are **DDF** (document de fundamentare) sau **ORD** (ordonanțare), K-BOT arată un mesaj
  de eroare și nu șterge nimic.
- Înainte de ștergere K-BOT îți spune câte rânduri dispar și cere confirmarea. Operația nu se
  poate anula.
- După ștergere, reîncarcă arborele și descarcă din nou din FOREXE angajamentele lunii.

Cât timp opțiunile avansate sunt pornite, ajutorul arată și această parte, iar manualul exportat
din fereastra de ajutor conține toate cele trei părți.
