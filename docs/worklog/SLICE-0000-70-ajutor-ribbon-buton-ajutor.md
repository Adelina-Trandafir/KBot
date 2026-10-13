# SLICE-0000-70 - Ajutorul pentru 0114: ribbonul si butonul «Ajutor» (13.10.2026)

Sub-felie de ajutor a feliei 0114. Context, capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

## Ce s-a schimbat si de ce
- Subiect NOU `contabil.ribbon` («Ribbonul (banda de comenzi)», `contabil/ribbon.md`, `screens: AvacontMain.ribbon`):
  filele si grupurile cu toate butoanele (cu conditiile: «Operatiuni necorelate» doar cand exista, «Jurnal activitate»
  doar cu comutatorul pornit; grupul Administrare lasat afara, e unealta operatorului / Debug), etichetele la mouse,
  strangerea (dublu clic pe fila / sageata; clic pe fila o desface; tinuta minte), butoanele fixate «Ajutor» si
  «Inchide» (cel din urma: inchide macheta, din lista revine la «Acasa», lipseste pe «Acasa», aplicatia se inchide din X;
  Esc), tabelul **«Unde gasesti ce era in MENIU»**. Capturi noi: `ribbon-file`, `ribbon-configurari`, `ribbon-strans`.
- Subiect NOU `contabil.ajutor.buton` («Butonul «Ajutor» din ribbon», `contabil/ajutor-buton.md`, copil al
  `contabil.ajutor`): pe «Acasa» = ajutorul ca pagina (in «Machete deschise», inchis cu «Inchide»); cu o macheta pe ecran
  = meniul «?» al ei; F1 merge oriunde. Capturi noi: `ajutor-buton-pagina`, `ajutor-buton-meniu`.
- `contabil.ajutor`: «trei feluri» de deschidere (F1, «?», butonul «Ajutor»); «Butoanele unui rezultat» spun «macheta din
  ribbon» in loc de «fereastra din MENIU»; din «Tururile ghidate» a iesit fraza despre pasii care deschid meniul MENIU
  (tururile acelea sunt rupte, vezi 0000-69); «Fereastra de ajutor sta deasupra» spune ca, deschisa din «Ajutor» pe
  «Acasa», e pagina. `prepare:` al capturii `ajutor-meniu` trimite la ribbon (redo-ul ei vechi ramane).
- `contabil.setari`: comutatorul `chkLogViewer` citat exact, cu ce face acum (butonul din ribbon).
- Etichete `0114` pe sectiuni.

## Fisiere
`src/KBot.App/HelpContent/contabil/ribbon.md` (nou), `contabil/ajutor-buton.md` (nou), `contabil/ajutor.md`,
`contabil/setari.md` (o sectiune).

## Teste / verificare
Vezi 0000-74 (rularea finala `Check-Help.ps1 -Coverage -Map`). Nimic rulat in aplicatie.

## Capturi de refacut
Noi: `ribbon-file`, `ribbon-configurari`, `ribbon-strans`, `ajutor-buton-pagina`, `ajutor-buton-meniu` (doar `prepare:`;
fara `goto:` pana la tipurile noi propuse in 0000-69). `prepare:` schimbat: `ajutor-meniu`.

## Neverificat
- Textul butonului «Operatiuni necorelate» din ribbon (copiat din randul meniului, cu numar si culoare): ajutorul nu il
  citeaza, spune doar cand apare.
- Unde exact sta sageata de strangere («the chevron at the right»): ajutorul spune «sageata din dreapta lui».
