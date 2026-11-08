# SLICE-0000-170 — Ajutor: vederea «Recepții» după KE21

Data: 08.11.2026.

## Ce s-a schimbat și de ce

Slice-urile KE21-02..04 au pus un singur view de recepții în formularul «Facturi primite» și în vederea «E-Factura» a unui angajament din fereastra principală; pagina «Recepții» din ribbon nu mai există. Se vede pe ecran:
- deasupra tabelului cu facturi, o linie îngroșată cu detaliile facturii bifate (total, alocat, rest, starea), cu mai multe facturi numărul lor și sumele, fără nicio bifă «Nicio factură bifată»;
- arborele facturilor are **căsuțe de bifat** (mai multe facturi pe o recepție) cât timp fila «Recepții» e pe ecran; factura acoperită se arată doar privită, cu «Elimină asociere»; butonul de propuneri pe furnizor și alegerea listei («fără recepții / asociate / toate») sunt în arbore și în meniul antetului;
- în vederea unui angajament tabelul «Angajamente» nu mai apare, iar tabelul clasificațiilor se întinde pe toată lățimea, sub arbore;
- în fila «Adaugă», «Tip», «Data» și «Descriere» sunt deasupra câmpurilor lor;
- panoul de mesaje stă sub «Adaugă / Asociate / Existente» și apare numai la mesaje.

## Teme atinse

`src/KBot.App/HelpContent/contabil/efactura/index.md`: «Recepțiile unei facturi primite» (trimite la secțiunea nouă), «Vederea «Recepții»: mai multe facturi pe o recepție» (fosta «Pagina «Recepții»», rescrisă), «Rezervare și ordonanțare din vederea «Recepții»»; `screens:` fără fosta fereastră de recepții. `vederi/plati.md`: o trimitere la pagina din ribbon. Etichetă `KE21, 0000-170` pe cele două secțiuni.

## Capturi

`efactura-receptii` marcată de refăcut (`redo`, 08.11.2026). Nimic șters sau suprascris din `img/`.

## Verificări / nerezolvat

`Check-Help.ps1 -Coverage` are erori mai vechi, fără legătură cu această temă (feliile `0000-5x` din `conturi.md`, tutoriale). Nu s-a rulat aplicația pentru ajutor. Descrierea butoanelor din ribbon în shell nu e în ajutor până nu se vede pe ecran cum arată.
