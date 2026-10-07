---
id: contabil.efactura.conturi
title: Conturile unității emitente
part: contabil
order: 77
parent: contabil.efactura
screens: ConturiForm
keywords: conturi, conturile unitatii, cont emitent, iban, banca, cont bancar, cont nou, date unitate, unitate emitenta
---
<!-- slice: 00EF-12, 00EF-13, 0000-56, 0000-58 -->
Fereastra **«E-Factura — conturile unității»** ține lista conturilor bancare (IBAN) ale **unității care emite facturile**. Sunt conturile tale, nu ale partenerilor cărora le plătești. Se deschide din fereastra facturilor emise, cu butonul cu litera K din bara de titlu, rândul **«Conturi Unitate»** (celălalt rând, «Date Unitate», deschide datele unității — vezi [E-Factura: facturile emise](topic:contabil.efactura)). Pe o factură, contul se alege în vederea **Generale**.

<!-- capture: conturi-unitate | caption: Fereastra «E-Factura — conturile unității» | prepare: Din fereastra de facturi apăsați butonul K din bara de titlu și alegeți «Conturi Unitate», pe o unitate care are deja două-trei conturi salvate. -->

## Ce vezi
<!-- slice: 00EF-12, 0000-56 -->

Un tabel cu câte un cont pe rând:

- **Cont (IBAN)** — se scrie direct în tabel;
- **Banca (dedusă din cont)** — o completează K-BOT, nu o scrii tu: banca se află din codul băncii din IBAN și apare după ce salvezi. Dacă codul nu e în lista băncilor, rândul spune «— banca nu este în listă —»; contul se salvează oricum.

## Cum lucrezi
<!-- slice: 00EF-12, 0000-56 -->

1. **«Cont nou»** adaugă un rând gol la sfârșit; scrie IBAN-ul. Spațiile și literele mici nu contează: K-BOT le aduce la forma corectă.
2. **«✕»** de pe un rând șterge contul. Facturile deja emise nu se schimbă: fiecare își păstrează contul scris pe ea.
3. **«Salvează»** verifică toate conturile și le scrie. Până atunci nimic nu se schimbă pe server.
4. **«Închide»** (sau Esc) închide fereastra; dacă ai conturi nesalvate, K-BOT întreabă dacă le închizi fără salvare.

## Ce refuză K-BOT
<!-- slice: 00EF-12, 0000-56 -->

- **Un rând fără cont** — scrie IBAN-ul sau șterge rândul cu «✕»;
- **Un IBAN greșit** (cifrele de control nu se potrivesc, de obicei o cifră greșită) — mesajul arată rândul și contul;
- **Același cont de două ori**;
- **Mai mult de 50 de conturi**.

Dacă apare mesajul că **tabelele E-Factura ale unității lipsesc**, unitatea nu are încă pregătit locul pentru conturi: anunță administratorul K-BOT.
