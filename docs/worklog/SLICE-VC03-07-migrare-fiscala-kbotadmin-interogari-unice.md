# SLICE-VC03-07 — Migrare fiscală din KBotAdmin / Interogări unice

Data: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Pregătită sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql pentru mecanismul
existent routes/one_time/runner.py: fără START TRANSACTION/COMMIT și fără
SELECT-uri finale ale căror rezultate runnerul le-ar consuma fără afișare.
KBotAdmin păstrează tranzacția per bază și scrie registrul numai după
reușita întregii migrări. Nu a fost schimbat runnerul sau KBotAdmin.

Datele migrează ca în scriptul manual. Înainte de final, o gardă în tabelă
temporară oprește baza cu eroare duplicate-key explicită dacă există
NIR_FARA_CF, FACTURA_FARA_CF sau CF_FACTURA_DIFERIT_DE_NIR. Cheia duplicată
este intenționată și indică motivul; nu depinde de CHECK sau strict mode.
Runnerul face rollback la modificările de date și nu marchează baza ca
rulată. Pe eroare de AVACONT_SURSA, runnerul nu continuă la DC-uri.
După corectarea datelor, aceeași interogare poate fi reluată; bazele deja
marcate sunt sărite. Scriptul manual și cele trei SELECT-uri diagnostice
rămân disponibile separat în VC_20261023_FURNIZORI_CF_DATE.sql.

Antetul scriptului de structură a fost adaptat pentru rulare și ca
interogare unică, cu numele VC_20261023_FURNIZORI_CF_STRUCTURA. Poate rula
pe AVACONT_SURSA + DC-urile bifate, apoi migrarea de date pe aceleași baze;
alternativa structură manual pe sursă + sincronizare rămâne valabilă.
DDL este pregătit în fișier și va fi executat exclusiv de utilizator.

Pași KBotAdmin: Interogări unice -> Din fișier pentru structură, nume
VC_20261023_FURNIZORI_CF_STRUCTURA, baze bifate, Vezi (nu execută), Execută.
Apoi Din fișier pentru varianta KBOTADMIN; SCHIMBAȚI explicit numele în
VC_20261023_FURNIZORI_CF_KBOTADMIN (UI păstrează numele anterior dacă nu e
gol), aceleași baze, Vezi (nu execută), Execută. Publicați online după
reușita migrării. Interogarea include întotdeauna AVACONT_SURSA.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql (nou);
sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql (antet);
implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-07-migrare-fiscala-kbotadmin-interogari-unice.md.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
