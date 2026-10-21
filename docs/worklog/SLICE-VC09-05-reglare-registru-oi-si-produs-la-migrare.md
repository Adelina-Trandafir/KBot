# VC09-05 — Reglare registru OI în VERCON, produs completat la migrare, mesaje clare

Data: 2026-10-21.

## Ce s-a schimbat și de ce

După migrarea unei baze OI, pozițiile de primire (BP) din registrul `OI_Produse` au venit fără `IdProdus`
(în `tz_2100.accdb` din proiect: 355 din 535). Migratorul le căuta ca primire și blocava 111 scoateri; după ce
blocajele au dispărut, VERCON refuza Balanța OI cu «contul nu poate fi stabilit», aceeași cauză.
Programul Access nu citește acele rânduri: ia primirile din `OI_Predare` (bon + produs).

1. **Migrator (0.3.11.0).** Contul scoaterii se ia din linia din `OI_Predare` (bon + produs, fără preț).
   Produsul lipsă din pozițiile de primire BP se completează din `OI_Predare` numai când e sigur
   (aceeași cantitate și același preț, exact atâtea linii libere câte poziții goale, un singur produs).
   Restul rămân goale (N17). Rapoarte noi: N16 (completat), N17 (rămas). Mesajele de blocaj (X14–X17, N10, N13,
   N14, X04, X08) rescrise în română clară; X16 are titlu general și un rând explicit pe poziție.
   **Asocierea facturilor (`VrcRunner.Invoices.vb`)** era lentă: ~16.000 de cereri una după alta către server. Acum
   sursele (origini, linii NIR) se citesc o dată pe subunitate, iar câte 20 de NIR-uri se scriu printr-o singură comandă
   cu mai multe instrucțiuni (aceleași instrucțiuni, în aceeași ordine, cu `LAST_INSERT_ID()` și variabile de utilizator),
   cu progres în jurnal («N/M NIR-uri asociate»). Rezultatul scris în baza de date trebuie să fie identic; nerulat.
2. **VERCON — fereastra «Reglare registru OI»** (butonul din bara paginilor Bon de predare / transfer / casare).
   Listează pozițiile de primire fără produs cu propunerea din liniile bonului (Sigur / De verificat / De ales /
   Fără linie potrivită); «Aplică propunerile sigure» sau produs ales pe o poziție. Scrie numai `IdProdus`,
   cu revizie, în aceeași tranzacție cu blocarea gestiunii, cu audit (`oi.register.product`) și cheie de cerere;
   după aplicare raportează dacă registrul mai are altă problemă.
3. **VERCON — mesaje.** `oi_accounts.py` și `oi_custody.py` explică în română clară ce lipsește și trimit la fereastră.

## Fișiere

- Migrator: `VERCON/VRC.Migrator/VrcRunner.OiAccounts.vb`, `VrcRunner.OiOrigins.vb`, `VrcRunner.Oi.vb`,
  `VrcRunner.vb`, `VrcRun.vb`, `VRC.Migrator.vbproj`, `README.md`.
- VERCON server: `PYTHON/routes/vercon/oi_register_repair.py` (nou), `doc_routes.py`, `oi_accounts.py`, `oi_custody.py`.
- VERCON pagină: `PYTHON/static/js/vercon/oi-register-repair.js` (nou), `docs.js`, `PYTHON/static/css/vercon-migration.css`.
- Ajutor: `PYTHON/static/help/vercon.json` (secțiunea «Bon de predare — OI și MF»).
- Documente: `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

## Rezultate verificări

- `dotnet build VERCON/VRC.Migrator/VRC.Migrator.vbproj`: reușit, 0 avertismente, 0 erori.
- `py_compile` pe cele patru fișiere Python; `node --check` pe `oi-register-repair.js` și `docs.js`.
- Cifrele pe `tz_2100.accdb` (citite din copie, fără modificări): 130 scoateri; 127 legături unice bon + produs, 3 cu
  două linii pe același bon și produs, la același preț și cont (2); 0 fără primire; 0 diferențe de cont între
  registru și `OI_Predare`.
- Fără teste, fără rulare a migratorului sau a VERCON, fără browser, SQL sau modificări de schemă.

## Nerezolvat / neverificat

- Migratorul și fereastra nu au fost rulate pe date reale; câte poziții ies «Sigur» este o estimare.
- Fereastra nu se deschide automat la prima intrare în VERCON; se deschide din buton, iar mesajul de la balanță trimite la ea.
- Datele deja migrate cu versiunea veche nu se repară de migrator; se reglează din fereastră.
- `VERCON/REGISTRU_OI_MF.md` încă descrie regula veche a primirii din registru.
- Pozițiile de primire prin bon de transfer fără produs nu sunt completate de migrator; fereastra le tratează.
- Baze protejate cu parolă din `C:\vercon` nu au fost citite.
