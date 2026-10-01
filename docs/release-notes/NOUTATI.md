# K-BOT — noutăți pe versiuni

Ce s-a schimbat în fiecare versiune, cea mai nouă sus. Textul unei versiuni este cel pe care
utilizatorul îl citește în fereastra de actualizare, înainte să apese «Da».

Secțiunile se scriu la fiecare `publish-release.ps1` / `push-update.ps1`, după regulile din
[README.md](README.md). Nu șterge marcajele ascunse `<!-- release: ... -->` și `<!-- felii: ... -->`:
după ele se știe de când se numără schimbările versiunii următoare.

## 1.1.1.1 (01.10.2026)
<!-- release: utc=2026-10-01T07:12:58Z -->
<!-- felii: 0056-02, 0084-02, 0094-02, 0000-26, 0000-27, 0000-28, 0000-29 -->

- La o descărcare nouă puteți modifica și legăturile recepțiilor vechi, atât timp cât nu există ordonanțare pentru ele.
- În vederea «Rezervări», iconița de acțiuni din subsolul arborelui apare abia când nu mai e nicio rezervare cu «+» (de adus din FOREXE).
- În «Sumar», butonul «Asociază parteneri» (doar la angajamentele cu DDF) leagă unul sau mai mulți parteneri de document.
- Editorul DDF are o pagină nouă, «Parteneri», în dreapta barei de pagini; partenerul din antet rămâne cel principal.
- Meniul «?» arată doar tururile ferestrei din care a fost apăsat; editorul DDF are acum turul lui ghidat.
- Butonul cu rotița din bara de titlu a ferestrei principale a dispărut; Setările se deschid din «MENIU › Configurare K-BOT».
- «MENIU» are rândul «Jurnal activitate», pentru jurnalele K-BOT; îl puteți ascunde din Setări › Aplicație.
- O legătură e blocată doar de o ordonanțare (de la ziua ei încolo), nu de o plată; ajutorul a fost actualizat.

## 1.1.1.0 (01.10.2026)
<!-- release: utc=2026-10-01T06:08:58Z -->
<!-- felii: 0097-02, 0097, 0098, 0098-02, 0096, 0095, 0095-02, 0093, 0094, 0078-07, 0078-08, 0000-01, 0000-02, 0000-03, 0000-04, 0000-05, 0000-06, 0000-07, 0000-08, 0000-09, 0000-10, 0000-11, 0000-12, 0000-13, 0000-14, 0000-15, 0000-16, 0000-17, 0000-18, 0000-19, 0000-20, 0000-21, 0000-22, 0000-23, 0000-24, 0000-25 -->

- Turul ferestrei principale pornește singur la deschidere; îl puteți opri sau reactiva din Setări.
- «MENIU» are un dosar nou, «Adăugare angajamente...», cu «Creează angajament în FOREXE».
- Pagina FOREXE: întrebările «Sunteți sigur...?» primesc singure «Da»; la o a doua recepție cu aceeași dată sunteți întrebat.
- Setări: fereastra principală poate porni mărită, iar mini-meniul K-BOT din pagina FOREXE poate fi ascuns.
- Unitatea se schimbă din bara de titlu (conexiunea FOREXE se închide singură); sesiunea expirată se reînnoiește mai discret.
- Un document semnat nu mai poate fi regenerat; «Note corecție» apare doar când angajamentul are note.
- Cât rulează robotul FOREXE, cererile de refresh se pun la coadă, într-o fereastră cu pauză și anulare.
- ORD și DDF: noduri noi «Toate ordonanțările» / «Toate reviziile», ștergere pe lună, fără mesaj după ștergere.
- Istoric are nodul «Tot istoricul»; Extrase are un meniu de afișare și se deschide din «Meniu › Extrase».
- Parteneri: cod fiscal unic, date completate din ANAF, banca dedusă din IBAN, câmpul «Adresa».
- Arborii așteaptă cât Adobe deschide un document; fereastra Adobe nu mai iese din panou.
- Ajutor nou: F1 și «?» cu căutare pe întrebări, tururi ghidate pe ferestre și manual exportabil.

## 1.1.0.6 (29.09.2026)
<!-- release: utc=2026-09-29T13:11:47Z baseline -->
<!-- felii: -->

Versiunea de la care pornește acest jurnal (cea de pe server la 01.10.2026). Nu are listă de
schimbări.
