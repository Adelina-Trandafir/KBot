# SLICE-VC09-08 — Analiza pierderii zecimalelor la bonurile de retur

Data: 30.10.2026. **ANALIZAT ÎN SCHEMĂ ȘI COD; FĂRĂ MODIFICĂRI FUNCȚIONALE.**

Continuare autorizată de utilizator: [VC09-09](SLICE-VC09-09-retururi-si-rulaje-balanta.md) implementează recuperarea prețurilor retururilor în migrator și în comanda KBotAdmin. Limitarea „numai storno” descrisă mai jos este istoricul acestei analize, înlocuit de VC09-09.

## Cererea și constatarea

Utilizatorul a cerut verificarea aceleiași situații pentru bonurile de retur. Analiza confirmă **același defect de reprezentare în Access și propagare la migrare**: `IesiriRetur.PUTVA` este `Long`, iar migratorul copiază întregul fără recuperarea zecimalelor. Exemplu explicativ: un preț cu TVA de 14,4855 ajunge 14 în această coloană. Nu există un calcul nou al cotei TVA; pierderea privește prețul care include deja TVA.

Atât schema legată din `VERCON/VERCON_ACCESS/tables/IesiriRetur.md`, cât și schema sursă din `VERCON/VERCON_ACCESS/Databases/TZ_SCHEMA/tables/IesiriRetur.md` declară `PUTVA` ca Long. Cantitatea este Double. În `Iesiri`, prețul și valoarea cu TVA sunt Double: bonurile de consum nu au aceeași limitare la întreg a coloanei de preț.

## Traseul datelor

- `VERCON/VERCON_ACCESS/Forms/BR.txt`: salvează `TmpFactV.PUTVA` în `IesiriRetur.PUTVA` prin INSERT; la redeschiderea returului citește însă `ProduseP.PUTVA`, nu întregul din linia returului.
- `VERCON/VERCON_ACCESS/Reports/BonRetur.txt`: afișează prețul din `ProduseP.PUTVA` și calculează valoarea din cantitatea returului înmulțită cu acest preț. Astfel, documentul Access poate arăta corect chiar dacă prețul stocat pe linia de retur a fost rotunjit la întreg.
- `VERCON/VRC.Migrator/import_map.json`: `IesiriRetur.PUTVA` folosește conversia numerică obișnuită `VALOARE-INT`. În `VrcRunner.BuildRow`, recuperarea adăugată prin VC09-06 se aplică numai `IntrariStorno`; nu există o recuperare pentru `IesiriRetur`.
- `PYTHON/routes/vercon/documents.py`: citirea unui BR derivă valoarea liniei din `Cant × VC_IesiriRetur.PUTVA`. Prețul migrat întreg poate afecta valoarea afișată.
- `PYTHON/reports/vercon/queries/consumption.sql` și `report_data.documents`: raportul BR citește tot prețul documentului, iar valoarea nestocată se derivă din cantitate și acel preț. Aceeași pierdere poate afecta raportul web/PDF/Excel.
- La crearea/modificarea unui BR nou în VERCON, `_clean_return_lines` folosește prețul bonului de consum sursă prin `return_eligible`, apoi conversia zecimală. În acest traseu nu s-a identificat conversia la întreg din Access.
- Retururile sunt deja incluse cu plus în motorul de stoc și în sursa standard a balanței. Cantitatea și evaluarea balanței la prețul variantei sunt distincte de prețul documentului BR; problema identificată nu demonstrează omiterea cantității din balanță.

## Acoperirea curentă și limita intervenției

Comanda generală din KBotAdmin implementată prin VC09-07 verifică și repară **doar `VC_IntrariStorno`**. Nu include încă `VC_IesiriRetur`; nici migratorul 0.3.14.0 nu recuperează prețul retururilor. Această etapă consemnează constatarea cerută, fără extinderea automată a reparației la altă tabelă sau alt flux.

Nu au fost citite date reale, deci nu este stabilit câte bonuri sunt afectate și nu se afirmă că toate retururile au diferențe. Nu au fost rulate teste, build, verificări automate, KBotAdmin, SQL, SSH sau HTTP. Fără modificări MariaDB sau publicare. S-au actualizat numai acest worklog și trimiterile din status/index.
