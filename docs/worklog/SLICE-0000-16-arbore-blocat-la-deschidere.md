# SLICE 0000-16 — Ajutor: arborele așteaptă cât se deschide un document (0078-08)

Data: 30.09.2026. Pentru felia 0078-08 (arborii nu mai iau alt rând cât Adobe încă deschide
documentul).

## Ce s-a schimbat și de ce

| Subiect | Secțiunea | Ce | Eticheta |
|---|---|---|---|
| `contabil.liste` | «Cât se deschide un document» (NOUĂ) | locul care deține explicația: ce se blochează, cât, ce merge în continuare | `0078-08` |
| `contabil.ddf` | «Vederea «Fundamentare»» | un rând: arborele și lista «Fișiere» așteaptă, cu legătura | + `0078-08` |
| `contabil.vederi.ord` | «Documentul și semnarea» | un rând + legătura | + `0078-08` |
| `contabil.notecab` | «Vederea «Note corecție»» | o propoziție + legătura | + `0078-08` |
| `contabil.fereastra` | «Lista angajamentelor» | lista angajamentelor așteaptă și ea, cu legătura | + `0078-08` |

**A treia trecere, la cererea operatorului («the helper doesn't need to know about the inner
workings of the app... it's meant for users only»):** scoase din nou secțiunile care descriau cum
lucrează K-BOT pe dinăuntru — «Cum se deschide documentul» (`contabil.ddf.semnare`: Ctrl+H, Ctrl+2,
fereastra ținută cât panoul), «Ce face K-BOT cu fereastra găzduită» (`avansat.documente`) și «Ce cauți
în `adobe_preview.log`» (`avansat.jurnale`) — și propoziția despre de ce se blochează arborele
(`contabil.liste`). Cele trei fișiere sunt din nou identice cu versiunea comisă. Regula a intrat în
`docs/HELP_SYSTEM.md` §5 «Left out on purpose».

## Fișiere atinse

- `src/KBot.App/HelpContent/contabil/liste.md`
- `src/KBot.App/HelpContent/contabil/ddf/index.md`
- `src/KBot.App/HelpContent/contabil/vederi/ord.md`
- `src/KBot.App/HelpContent/contabil/notecab.md`
- `src/KBot.App/HelpContent/contabil/fereastra.md`
- `docs/HELP_SYSTEM.md` (regula «fără felul în care lucrează K-BOT pe dinăuntru»)

## Rezultatele testelor

`Check-Help.ps1 -Coverage` și build `KBot.App` — vezi mai jos, după a treia trecere.

## Rămas / de citit de operator

- Nicio captură nouă și niciuna de refăcut (se schimbă doar comportamentul; cursorul arată că se lucrează).
- Reperul «Ajutorul e la zi până la» NU s-a mutat: acoperă doar această schimbare.
