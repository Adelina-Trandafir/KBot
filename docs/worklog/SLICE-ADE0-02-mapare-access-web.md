# SLICE-ADE0-02 — mapare Access → web și contractul calculelor

Data: 07.10.2026. Stare: DOCUMENTAT LOCAL. Fără implementare sau publicare.

## Ce s-a schimbat și de ce

Am urmărit traseele din formulare/module și interogările motorului 2021 pentru a separa
regulile care trebuie reproduse de cache-uri, funcții excluse și variante legacy.
Inventarul clasifică 66 de tabele, 44 de formulare, 27 de interogări și 18 rapoarte.
Contractul descrie formulele, ordinea fazelor, filtrele de anulare, tipurile/conversiile,
snapshotul și cazurile necesare comparației Access/web. Nu constituie dovadă de paritate.

Utilizatorul a confirmat păstrarea încasărilor în lună închisă cu actualizarea situației
salvate; prezența rămâne blocată. Am corectat formularea generală din plan.
Codul identificat folosește CFGs/NUMAR drept următorul număr în Plati_chitante;
seria/numărul ilustrative nu devin seed-uri. Neconcordanțele M01–M10 sunt explicite,
inclusiv lipsa actualizării snapshotului în traseele de anulare găsite.

## Fișiere

- [Maparea fluxurilor](../../ADECHIT/MAPARE_ACCESS_WEB.md).
- [Contractul calculelor](../../ADECHIT/CONTRACT_CALCULE.md).
- [Inventarul obiectelor](../../ADECHIT/INVENTAR_OBIECTE.md).
- [Indexul ADECHIT](../../ADECHIT_STATUS.md).
- [Starea ADE0–ADE9](state/ADECHIT_STATUS_ADE0-ADE9.md).
- Acest worklog.

## Verificări efective

- Inspecție statică a surselor Access, relațiilor și interogărilor; fără rulare Access.
- Inventarul generat din numele fișierelor are clasificări explicite pentru 155 de obiecte;
  verificarea refuză un obiect lipsă din clasificare sau o clasificare duplicată.
- Verificare automată: 210 legături locale au ținte existente; fiecare dintre cele 155
  de obiecte apare exact o dată în inventar; exact șapte tabele sunt excluse drept cache _L.
  Verificarea legăturilor confirmă fișierele/directoarele, nu ancorele interne Markdown.
- Reverificarea Salvare_Lunara confirmă 25 de câmpuri copiate în snapshot.
- Nu s-au rulat teste de aplicație: intervenția adaugă numai documentație.

## Neverificat sau amânat

- Rezultatele Access, conversiile ACE/ordinea evaluării, datele reale și rularea rapoartelor.
- M01–M10, traseul real de redeschidere, anularea în lună închisă și rapoartele legacy.
- Schema/API/editorul și previzualizarea 5050 rămân de implementat în feliile alocate.
- Nicio conexiune Linux/MariaDB, SQL executat, commit sau push. Utilizatorul publică
  prin AvacontPush și testează serverul. Modificările preexistente ale altor module nu au fost incluse.
- Nicio modificare vizuală în aplicație; nu există pagină de ajutor devenită neactualizată.

Următoarea lucrare planificată: ADE1-01, integrarea cu proiectul și sistemele comune.
