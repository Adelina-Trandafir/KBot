# VERCON — mockupuri corectate, revizia 2

12.10.2026. Cerințele explicite din mesajul cu cinci atașamente prevalează: **tabelul NIR este unica referință vizuală**, iar **o listă de alimente poate avea mai multe BC-uri**. Acest folder conține numai cele **9 PNG-uri modificate**. Imaginile nemodificate (Stocuri, NIR consultare/editor, Rapoarte) se găsesc în [setul anterior](../2026-10-12-corectate/INDEX.md).

[Atașamentele și cerințele utilizatorului](../Corectii_mockup-uri_02/INDEX.md) sunt păstrate separat; capturile criticate nu sunt referințe de stil.

## Consultare

| Ecran | PNG |
|---|---|
| BC — grilă compactă și Adaugă rând dezactivat în footer | [03-bon-consum.png](03-bon-consum.png) |
| ListaAlimente — porții/feluri/BC-uri în DGV, două bonuri, total general; arbore Liste salvate în dreapta | [04-lista-alimente.png](04-lista-alimente.png) |
| Transfer — grilă compactă și footer în stil NIR, arbore BT | [05-transfer-obiecte-inventar.png](05-transfer-obiecte-inventar.png) |

## Editor și selecție BC

| Ecran | PNG |
|---|---|
| BC — editoare compacte în celule, Adaugă rând în footer | [03b-bon-consum-modificare.png](03b-bon-consum-modificare.png) |
| ListaAlimente — mese/feluri compacte, DGV cu mai multe BC-uri, totaluri și Adaugă rând în footer | [04b-lista-alimente-modificare.png](04b-lista-alimente-modificare.png) |
| Popup — alegerea adaugă un BC la setul existent | [04c-alegere-bon-consum.png](04c-alegere-bon-consum.png) |
| Transfer — produse ale predătorului, footer NIR | [05b-transfer-modificare.png](05b-transfer-modificare.png) |
| Predare — sursă cont de intrare, footer NIR | [05c-predare-adaugare.png](05c-predare-adaugare.png) |
| Casare — fără primitor, footer NIR | [05d-casare-adaugare.png](05d-casare-adaugare.png) |

## Reguli

Grilele urmează NIR: celule contigue, borduri subțiri, text și rânduri compacte, editoare mici în celulă, valori calculate ca text; Adaugă rând în footer, distinct de Adaugă document. Consultarea păstrează arborii în dreapta; editorul îi ascunde.

DGV-ul BC-urilor are câte un rând per bon, fără duplicate. Exemplul folosește 780,0000 + 660,0000 = 1.440,0000 lei; valoarea listei este 1.440,0000 lei și diferența 0,0000. Adăugarea nu înlocuiește bonurile alese, scoaterea unui rând nu anulează BC-ul. Salvarea tratează lista/mesele/setul de legături atomic și nu repetă consumul.

[Fluxuri UI](../../FLUXURI_UI.md) · [Plan](../../PLAN_IMPLEMENTARE.md) · [Calcule](../../CONTRACT_CALCULE.md) · [Cardinalitate țintă și limita DDL](../../SCHEMA_VC.md) · [Probe planificate](../../PROBE_BROWSER.md) · [Worklog](../../../docs/worklog/SLICE-VC00-07-corectii-mockupuri-v2.md).

Mockupuri și documentație, nu modificarea aplicației. Date fictive, stil ADECHIT/Portal. Imagini editate cu **image_gen integrat**; [prompturi](prompts.json), [corecții de verificare](prompts-verificare.json), [proveniență](manifest.json). [Arhiva ZIP](../vercon-mockupuri-corectate-v2-2026-10-12.zip).
