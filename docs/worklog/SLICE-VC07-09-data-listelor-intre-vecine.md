# SLICE-VC07-09 — Data listelor de alimente între vecine

30.10.2026 — **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

## Cerere

Data unei liste care nu este ultima poate fi corectată între datele listelor active vecine, fără schimbarea succesiunii. Exemplul cerut: o listă din 27.10 între o listă din 23.10 și alta din 27.10 poate primi o dată între 23.10 și 27.10, inclusiv capetele, păstrând poziția.

## Implementare

- `PYTHON/routes/vercon/food_list_dates.py` (nou): vecine active din aceeași subunitate, gestiune și exercițiu, ordonate cronologic și numeric după numărul listei la egalitate de dată. ID-ul identifică lista, fără rol de sortare. Recalculează limitele sub blocarea gestiunii la salvare; noua pereche dată/număr trebuie să rămână între vecine. Aceeași dată cu o vecină este permisă când ordinea numerelor se păstrează. Lista fără o vecină are ca limită corespunzătoare capătul anului exercițiului. Data inițială neschimbată nu introduce o restricție suplimentară pentru corecțiile existente de porții.
- `PYTHON/routes/vercon/food_list_portions.py`: extinde ruta existentă `PUT /api/vercon/doc/lista/<id>/portions` cu `date` opțional și `portions` opțional. Numai data poate fi corectată și în liste din luni mai vechi, cu perioadă deschisă. Când se trimit porții, verificarea ultimei luni active se aplică atât datei inițiale, cât și celei noi. Cererile vechi care conțin doar porțiile rămân acceptate. Revizia, starea neanulată, dreptul și blocarea gestiunii rămân obligatorii.
- Corecția numai a datei actualizează exclusiv `VC_LABC.DATALA` și revizia, cu audit `lista.date_update`; nu rescrie mesele, prețurile, valorile sau legăturile cu BC. Corecția combinată se salvează în aceeași tranzacție. Soldurile calculate afectate primesc audit prin mecanismul existent de recalculare, fără rescrierea documentelor următoare.
- `PYTHON/static/js/vercon/lista.js`: data este editabilă în modul restrâns al listelor mai vechi; meniul și mesajul disting data de corecțiile permise ale porțiilor. Tooltipul datei prezintă intervalul. Trimite porțiile numai dacă au fost modificate; în lunile mai vechi trimite numai data. Arborele păstrează sensul descrescător existent pentru dată și numărul numeric, eliminând departajarea după ID intern.
- `PYTHON/static/js/vercon/document-history.js`: denumire lizibilă pentru corecția datei în istoric.
- `PYTHON/static/help/vercon.json`: descrierea noului comportament.

Modificarea completă și anularea păstrează regulile existente pentru ultimul document. Nu sunt modificate bonurile de consum sau alte familii de documente.

## Stare și limite

Numai analiză de proiectare pe surse. Nu s-au rulat teste, compilări, verificări automate, browser, HTTP sau SQL, conform instrucțiunilor proiectului. Fără modificări de schemă/date în MariaDB și fără publicare. Exemplul și concurența nu sunt confirmate prin execuție. Previzualizările generate anterior trebuie regenerate după salvare.
