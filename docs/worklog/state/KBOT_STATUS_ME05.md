# K-BOT — STATUS, felia ME05 (KBotAdmin, ADECHIT)

Familia ME05 deschisă la 17.11.2026 (ME01-ME04 erau luate), la cererea Adelinei. KBotAdmin este programul privat al proprietarului, fără Ajutor.

## Registry rows

| Felie | Ce | Stare |
|---|---|---|
| ME05-01 | [Comenzi > ADECHIT: «Mutare plata intre copii»](../SLICE-ME05-01-muta-plata-intre-copii.md): trece o plată întreagă (chitanță sau altă plată) de la un copil la altul din aceeași subunitate și lună; dacă luna e închisă, refă situațiile salvate (`AD_SS_Buget`) ale ambilor copii cu funcțiile aplicației; două casete (copilul de la care pleacă plata și copilul care o primește, CNP sau ID) → lista plăților primului → alegi plata și se arată analiza (doar citire) → butonul «Mută plată» (scrie, cu Jurnal) | 17.11.2026: KBotAdmin compilat curat, Python `py_compile` curat; **nerulat**, nevăzut pe ecran, netrimis pe server |

## Current focus
- ME05-01: copiază de mână `adechit_muta_plata.py` și `admin_cli.py` pe server; rulează pe o copie a bazei: lista plăților, analiza, mutarea; compară `AD_SS_Buget` al celor doi copii înainte și după.

## Open threads
- Plățile venite din extras de bancă sunt refuzate (asocierea își ține singură copilul, plătitorul și luna).
- Suma parțială nu e construită (Adelina a ales plata întreagă).
