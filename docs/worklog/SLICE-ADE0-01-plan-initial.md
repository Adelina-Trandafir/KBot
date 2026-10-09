# SLICE-ADE0-01 — planul inițial ADECHIT

Data: 07.10.2026. Stare: DOCUMENTAT LOCAL, numai documentație.

## Ce s-a schimbat și de ce

Am creat registrul separat ADECHIT, după structura KBOT_STATUS: index în rădăcină,
detalii în fișier de stare pentru zece felii, worklog pentru intervenție, decizii fixe,
subfelii și numere libere. Utilizatorul a cerut explicit numerotarea ADE0, ADE1 etc.

Planul include corecțiile utilizatorului: toate sistemele comune sunt reutilizate;
editarea DataGrid în celule este critică din prima versiune; fără facturi, bonuri fiscale
sau reduceri pentru absențe/frați; mdl_Situatie și interogările lui se transpun 1 la 1;
cache-urile `_L` nu se migrează. Datele unității rămân în sistemul existent, fără pagină
sau rută ADE pentru editarea lor. Unitati_Chitante este excepția explicită de la prefixul
AD_, iar seria și numărul oferite în conversație sunt doar exemple.

Codex scrie și verifică local pe localhost:5050; utilizatorul publică prin AvacontPush
și testează pe Linux/MariaDB. Acest flux prevalează asupra cerinței generice de push
din CODE_WORKFLOW. Nu s-a făcut commit sau push în această intervenție.

## Fișiere atinse

- [ADECHIT_STATUS.md](../../ADECHIT_STATUS.md) — index, reguli, decizii și ordinea lucrărilor.
- [ADECHIT_STATUS_ADE0-ADE9.md](state/ADECHIT_STATUS_ADE0-ADE9.md) — planul detaliat,
  criterii de acceptare, dependențe și întrebări rămase.
- Acest worklog.

## Rezultatele verificărilor

- Citite regulile din KBOT_STATUS.md și CODE_WORKFLOW.md și un exemplu de fișier de stare.
- Planul confruntat cu sursele locale inspectate în conversație: exportul Access,
  mdl_Situatie și interogări, portalul Python/JS, autentificarea/conexiunile, loggerul,
  controalele, schema_sync, AvacontPush și DateUnitateForm/API-ul de date unitate.
- Verificate local existența țintelor linkurilor și consecvența numerotării planului.
- Fără schimbare de cod executabil; nu este necesară compilare sau rularea suitei.
  Preview-ul 5050 nu este încă implementat sau pornit.

## Neverificat sau amânat

- Niciun acces la server, MariaDB sau datele live; niciun SQL executat, niciun deploy.
- Nu este încă terminată maparea tuturor fluxurilor active; aceasta este ADE0-02.
- Lipsesc datele și rezultatele Access pentru paritate, schema live și matricea finală
  a drepturilor. Aceste dependențe sunt înregistrate în status.
- Planul nu reprezintă implementarea sau acceptarea funcțională a vreunei felii ADE1–ADE9.
- Fără schimbări vizibile în aplicație: nu există pagini de ajutor de actualizat acum.
- Nu a fost citit sau modificat niciun fișier Claude.md.
