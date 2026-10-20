# SLICE-ADE9-06 — butoane ADECHIT în stilul VERCON

19.10.2026. **SCRIS LOCAL**, fără teste, verificări automate, browser sau probe
vizuale, conform instrucțiunilor proiectului. Numai analiză de design din surse.

## Schimbări

Butoanele de comandă din panoul Rapoarte renunță la fontul monospațiat gros și
fundalul în degrade. Folosesc fontul interfeței, greutate normală, fundal
transparent, bordură simplă și colțuri de 5px, ca acțiunile VERCON. Lățimea
panoului și înălțimea minimă a comenzilor rămân cele existente.

Butoanele de acțiune `.btn` din ADECHIT folosesc aceeași bordură, rotunjire și
greutate a fontului. Comenzile folosesc feedback-ul comun extras din VERCON:
contur albastru, umbră și ridicare de 1px la hover/focus; apăsarea revine la
poziția normală cu umbră interioară. Butoanele dezactivate nu primesc aceste
efecte. Preferința de mișcare redusă elimină deplasarea și tranzițiile.

Feedback-ul este într-un singur stylesheet pentru VERCON și ADECHIT. Importul
istoric VERCON rămâne un adaptor CSS. Meniurile și acțiunile integrate în
controale păstrează stilurile lor; în ADECHIT sunt incluse aici și comenzile
din celulele încasărilor, taburile, arborele de ani și editorul de text.
Modul activ al asocierii bancare își păstrează fundalul de selecție.

## Fișiere și publicare

Prin AvacontPush, publicați împreună din `PYTHON/static/css/`:

- `application-button-effects.css` (nou, comun);
- `vercon-button-effects.css` (import compatibil către cel comun);
- `adechit-buttons.css` (nou, adaptări ADECHIT);
- `adechit-reports.css`;
- `adechit.css`.

Ajutorul actualizat: `PYTHON/static/help/adechit.json`. Documentație și stare:
`ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, starea ADE0–ADE9 și
`implementare_vercon_v2.md`. Următoarea subfelie de rapoarte: **ADE9-07**.

Nu există modificări Python/JS, DDL sau seed. Nu s-au executat publicare,
restart, commit sau push. Utilizatorul probează aspectul în temele deschisă și
închisă, hover/focus/apăsare/dezactivare, panoul Rapoarte extins și restrâns,
formularele și comenzile încorporate. Aspectul nu este declarat validat.
