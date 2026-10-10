# ADE10-01 — Schema pentru subunități (sql/AD_05_subunitati.sql)

Implementează secțiunile 3, 4 și 8 din [planul subunităților](../../ADECHIT/plan_subunitati.md).

## Ce s-a schimbat și de ce

Un singur script nou, `sql/AD_05_subunitati.sql`, de rulat o dată pe fiecare bază ADE
(și pe șablonul `000_DEMO`, ca unitățile noi să pornească cu schema nouă). Scriptul a fost generat
cu un program de unică folosință; textul final este cel din repo.

- **Tabele noi:** `AD_Subunits` (id, denumire unică, activă), `AD_ReceiptConfig` (serie, contor,
  explicație, versiune — una pe subunitate) și `AD_IdMap` (import + tabel + ID Access → ID server).
- **`SubunitId` obligatoriu** în cele 17 tabele ADE: Grupe, ValoriTaxe, LunaD, Platitori,
  Platitori_sub, Prezenta, Plati, Chitante, AlteDoc, Retur, SS_Buget, Grupe_Educator,
  Platitori_Istoric, Compensare, Settings, Operations, Imports.
- **Chei care erau pe tot DC-ul devin pe subunitate:** `(SubunitId, Luna, Anul)`,
  `(SubunitId, Serie, Numar)`, setare `(SubunitId, SettingKey)`, cheie de reîncercare
  `(SubunitId, RequestKey)`. `AD_Imports` păstrează `SourceHash` drept cheie: același MDB nu se
  importă de două ori în același DC.
- **Relații compuse:** cele 29 de chei străine dintre tabelele ADE sunt refăcute ca
  `(SubunitId, ColoanaLegată)` către `(SubunitId, Cheie)` — baza refuză o legătură între subunități
  (inclusiv compensările dintre doi copii și ambele capete ale istoricului). Plus câte o cheie
  străină spre `AD_Subunits` pe fiecare tabel (46 de `ADD CONSTRAINT` în total).
- **Strategia de ștergere pe relație:** cele 6 chei `ON DELETE SET NULL` (Plati/Retur `IDZ`,`IDL`;
  Chitante/AlteDoc `IDL`) devin `RESTRICT`, pentru că un SET NULL compus ar încerca să golească și
  `SubunitId`. Codul le golește singur înainte de ștergere (`reopen_month`: Plati/Retur erau deja
  tratate; s-a adăugat pentru Chitante și AlteDoc). `SS_Buget` rămâne CASCADE.
- **`AD_LunaD.Ordine`:** ordinea locală a lunilor (plan §5). La conversie `Ordine = IDL`; la import
  `Ordine` = IDL-ul din Access (cu eventualele goluri); o lună nouă primește `max(Ordine)+1` în subunitate.
- **Conversia DC-urilor deja populate:** subunitate inițială cu numele din `@ade_subunit_name`
  (de editat la începutul scriptului); toate rândurile îi sunt atribuite; seria și contorul se copiază din
  `AVACONT_COMUN.Unitati_Chitante` (rândul cu `DC = DATABASE()`); rândul de blocare `AD_Lock` are ID = SubunitId.
  Un DC fără date nu primește subunitate (o creează migratorul). Dacă `AD_Imports` are mai multe loturi,
  scriptul se oprește cu mesaj (date din mai multe surse → reconciliere pe proveniență, nu atribuire
  automată). Scriptul refuză și o a doua rulare.
- `AVACONT_COMUN.Unitati_Chitante` rămâne neschimbat și nu mai este citit de ADE.

## Fișiere

`sql/AD_05_subunitati.sql` (nou).

## Teste

Niciunul: DDL nerulat, neverificat pe MariaDB. Numele cheilor străine sunt luate din
`MariaDB_Schema/000_DEMO.sql` (export 09.10.2026); alte baze pot avea nume diferite. Dacă un
`DROP FOREIGN KEY` eșuează, scriptul se oprește (DDL nu e tranzacțional; revenirea = restaurarea copiei
de siguranță).

## Rămâne neverificat / amânat

Rularea reală; compatibilitatea numelor de chei în baze neexportate; timpul pe baze mari.
După rularea scriptului aplicația veche și migratorul vechi nu mai sunt compatibile (plan §9).
