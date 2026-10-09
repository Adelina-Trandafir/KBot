# SLICE-ADE6-18 — consolidarea documentației threadului

09.10.2026. DOCUMENTAT LOCAL, la cererea utilizatorului de actualizare a fișierelor MD.

Am creat [UTILIZARE_WEB](../../ADECHIT/UTILIZARE_WEB.md) ca descriere a formei finale:
PC/mobil, ferestre modale, liste și editori, filtre numai Nume/Grupa, coloane I,
navigare din taste, calendar comun, proporții mobile, taxe lună/an cu perioade
migrate nullable și plătitor activ unic per copil la salvare. Include predarea
AD_04 după AD_03 și necesitatea restartului preview-ului pentru backendul nou.

Am corectat referințele active la propuneri depășite: apăsare lungă/ecran fără
margini, editarea tuturor cataloagelor direct în celule, M03/M06 fără răspuns.
Worklogurile anterioare rămân istorice; ghidul final prevalează pentru interfață.
M03 este decis, dar neimplementat; M06 este implementat parțial, fără a declara
transferul lot sau compensarea terminate.

Fișiere actualizate:

- ADECHIT_STATUS.md și docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md:
  starea finală, diferența testat/scris, registru ADE6-18; următorul număr ADE6-19.
- ADECHIT/PLAN_IMPLEMENTARE.md: checkpoint actual, editori modali și contract DGV,
  perioade taxe, activare unică și operații încă lipsă.
- ADECHIT/DECIZII_DESCHISE.md: deciziile explicite ale threadului și M06 parțial.
- ADECHIT/MAPARE_ACCESS_WEB.md: interfața finală, datele AD_03/04 și jurnalul nou.
- ADECHIT/CONTRACT_CALCULE.md: contextul nou, IDV istoric păstrat și limitele probelor.
- ADECHIT/REGULI_PROIECT.md: preferința utilizatorului pentru testare manuală;
  excepția ADE1-06 este limitată la pornire/deschidere nonvizuală.
- docs/worklog/SLICE-0110-05-control-js-dgv.md: notă despre extensiile comune ulterioare.
- docs/worklog/KBOT_STATUS.md: trimitere la registrul separat ADE și ghidul actual.

Verificări: recitirea documentelor și a referințelor relative către ghid/worklog/DDL.
Nu am modificat codul aplicației și nu am rulat teste sau verificări vizuale.
ADE6-06 rămâne testat la starea lui; ADE6-07–17 rămân SCRIS LOCAL. Proba startup
ADE1-06 nu validează schimbările ulterioare. Fără commit, push, DDL executat,
publicare sau validare server. Sursele Access și fișierele Claude.md nu au fost citite/modificate.
