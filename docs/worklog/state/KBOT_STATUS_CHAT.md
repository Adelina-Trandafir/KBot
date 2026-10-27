# K-BOT — STATUS, slice CHAT (chatul comun în K-BOT.app)

Chatul comun al aplicațiilor web (VERCON / ADECHIT / site) adus în K-BOT.app: robotul din colțul paginii «Acasă» și balonul cu mesaje care iese din el. CHAT-01…09 web sunt în [proiectul și istoricul web](../../../PYTHON/docs/chat/PROIECT.md#istoricul-livrării-web--jurnale-absorbite-în-d001-04), consolidat în D001-04. CHAT-08/09 desktop sunt intervenții distincte, în `docs/worklog`, identificate prin titlu și platformă; numerele istorice nu sunt schimbate.

## Registry row

| Slice | What | Status |
|---|---|---|
| [CHAT-08](../SLICE-CHAT-08-robot-si-balon.md) | Robotul K-BOT (`KBotChatRobot`) se leagănă și poartă un număr roșu la mesaje necitite; un clic deschide balonul (`KBotChatBubble`) cu mesaje și casetă de scris; `ChatService` sondează; server: ușa cu bearer-ul K-BOT pe `/api/chat/*`, fără a prelungi sesiunea | GATA pe cod (build curat), nimic văzut pe ecran, serverul neurcat |
| [CHAT-09](../SLICE-CHAT-08-robot-si-balon.md#slice-chat-09) | Răspuns la mesaj («Răspunde» + bandă), filtrul V/A/K, balonul se închide la clic în afară | GATA pe cod (build curat), nimic văzut pe ecran |

## Current focus

- Operatorul nu are încă mesaje de la alți utilizatori: de privit, la prima rulare cu serverul urcat, aspectul balonului, coada, rândul de filtre, «Răspunde», scalarea la 125/150% și tema închisă.

## Open threads

- **Serverul neurcat** (îl urcă operatorul de mână): `routes/chat/api.py`, `routes/auth/guard.py`, `routes/auth/session_store.py`.
- **Contul**: se presupune că `username` din sesiunea K-BOT = e-mailul din portalul web (cheia `CONT` din chat).
- Semnal pe celelalte pagini (nu doar «Acasă») și GIF propriu — nepornite, la cererea operatorului.
- Ajutor: secțiunea «Discuții» din `HelpContent/contabil/acasa.md` (etichete `CHAT-08, CHAT-09`).
