# SLICE-04EF-01 — Plăți din Contări

## Protecția la salvarea unei plăți noi

Bifa **„Protectie contari Plati/Receptii”** din **Setări → Aplicație** controlează verificarea recepției asociate facturii în **FacturiCumparare → Contări**. Cu protecția dezactivată, lipsa recepției produce un avertisment și permite salvarea. Cu protecția activată, lipsa recepției blochează salvarea. Protecția pornește dezactivată în 2026. Adelina o poate activa manual mai devreme pentru utilizatorii aleși pentru probe.

Migratorul importă valorile istorice din Access ca atare, fără recalcularea lor și fără verificarea existenței recepției.

## Cerință pentru viitorul slice de închidere a anului

Regula de activare automată este în [stadiul portării Avacont, la „Închidere anuală”](state/KBOT_STATUS_AV.md#închidere-anuală).
