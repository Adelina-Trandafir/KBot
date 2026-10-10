# SLICE-ADE9-01 — rapoartele ADECHIT

Stare: **SCRIS LOCAL**. Nerulat: nicio probă, nicio redare, nicio tipărire (preferința din REGULI_PROIECT §5).
Verificat doar: `py_compile` pe fișierele Python și `node --input-type=module --check` pe JavaScript.

## Ce s-a schimbat și de ce

Panoul «Rapoarte» din dreapta era dezactivat. Acum fiecare buton cere serverului un raport (pagină de listare sau PDF).
Serverul doar citește: nu scrie rânduri și nu consumă numere de chitanță. Cifrele vin din aceleași rânduri ca ecranul
(luna deschisă = `calculate`, luna închisă = `AD_SS_Buget`), nu dintr-un al doilea calcul.

| Raport Access | În web | Panou |
|---|---|---|
| SituatieDebitori_buget / _total / _total_grp | `situation` (o grupă / toate grupele / grupe separate) | Situație lunară + «Toate grupele?» + «Separă grupe?» + «Generează PDF?» |
| RegistruCasa | `cash` (interval de date) | Registru casă |
| RaportBanca | `bank` (interval de date) | Raport bancă |
| DocumenteAnulate | `cancelled` (luna aleasă) | Documente anulate |
| FisaCont + FisaCont_s | `account` (copilul ales sau toți din grupă) | Fișă cont |
| FisaDebitor (+_L, _C) | `debtor` | Fișă debitor (buton nou) |
| rpt_SitFin | `financial` | Situație financiară (buton nou) |
| Dispozitie plata | `payment-order` (o restituire) | butonul 📥 de pe rândul restituirii |
| Chitanta | deja făcută (ADE8) | neschimbată |

## Abateri față de Access (de confirmat)

- **Registru/Bancă/Anulate**: sume cu 2 zecimale (Access arăta 0 zecimale; ar fi ascuns bani).
- **Documente anulate**: Access filtrează după luna prezenței, deși antetul arată un interval; antetul arată acum luna.
  Plățile fără chitanță și fără alt document apar ca «Bon fiscal». Totalul «TOTAL DOCUMENTE ANULATE» din Access
  folosea două casete inexistente în raport (`[si]-[SP]`) și nu a fost reprodus; rămâne rândul TOTAL.
- **Fișă cont**: rândurile «Transfer» vin din jurnalul `Platitori_Istoric` (MutaCopil nu se portează, M06).
  «Explicație» a documentelor alte plăți = textul din `AlteDoc.Explicatie` + număr (FelDoc nu mai există, AD_07).
  Ordinea rândurilor cu aceeași dată: sold inițial, prezență, plăți, restituiri, transferuri (Access nu definea o ordine).
- **Fișă debitor**: lipsesc calendarul pe zile (`Prezenta_sub`), bonurile fiscale și coloanele Hrană / %Frate / Avans
  (date neportate). Absența = zile lucrătoare ale lunii − zile prezență. Lunile sunt în ordine cronologică
  (Access sorta textul `LA`, deci greșit între ani). Plătitorii activi se listează toți pe pagina copilului.
- **Dispoziție de plată**: beneficiar = copilul (ca în Access), nu plătitorul. Seria/numărul actului de identitate rămân de completat de mână.
- **Situație financiară (rpt_SitFin)**: sursa Access (`Situatii salvate`) nu e exportată (M09). Coloanele sunt mapate pe nume:
  Valoare achitată = total chitanțe, Achitat anticipat = Anticipat, Plăți = alte plăți, Număr chitanță = chitanțele lunii.
  **Maparea e presupusă, nu verificată.**
- **Fonturi PDF**: DejaVu Sans normal (nu există variantă bold în proiect); evidențierea se face prin fundal.
- Mail la Fișă cont: bifa rămâne dezactivată (mailul nu trimite, ADE8-05).

## Neconstruite

- **SituatieDebitori** (varianta veche): exclus de utilizator, 10.10.2026.
- **rpt_Factura, Factura_mail**: necesită tabelul `Facturi` (emitere, numerotare, delegat) — modul exclus în inventar («fără facturi»); nu există în schema web.
- «Încarcă chitanțe» (buton): nu e raport; rămâne dezactivat.

## Fișiere

Noi: `PYTHON/routes/adechit/reports.py`, `report_render.py`, `static/js/adechit/reports.js`, `report-print.js`,
`static/css/adechit-report.css`, `adechit-report-landscape.css`.
Modificate: `routes/adechit/__init__.py` (ruta `GET /api/adechit/reports/<kind>/<print|pdf>`), `receipt_output.py` (`issuer_data` extras și reutilizat),
`static/adechit.html` (panou), `static/js/adechit/app.js` (legare panou, butonul dispoziției), `tests/adechit_lazy.test.mjs` (doar importurile simulate noi).

## De probat de utilizator

Fiecare raport pe date reale, listare + PDF; interogările SQL nu au fost rulate pe MariaDB (nici pe SQLite);
`reportlab` lipsește din `.venv` local (PDF-ul se probează doar pe server); comparație cifre cu Access pe aceeași lună.
