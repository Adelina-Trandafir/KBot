# SLICE-VC07-04 — evidența MF și continuitatea activului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»).

## Ce s-a facut (`mf.py`, `js/vercon/mf.js`, rutele `/api/vercon/mf/assets`)
- Formular nou **«Registru MF»** (doar gestiunile MF, drept `mf`): un rand per activ — denumire, nr. inventar (`COD`), data intrarii, valoare (suma totalului cu TVA din liniile de receptie neanulate ale variantei), durata normala/folosita (luni), valoare lunara, **deținătorul** (din registrul OI: cine detine cat) si cantitatea **casata**.
- Editarea evidentei (data intrarii, DNF, DF) cu revizia `VC_ProduseM.Version` (`CONFLICT` la revizie veche), DF ≤ DNF, DNF ≥ 1; **valoarea lunara se recalculeaza** pe server = cost / DNF rounded o data la ZEC; audit cu valorile inainte/dupa.
- **Identitatea activului nu se schimba**: se editeaza doar randul din `VC_ProduseM`, varianta (`VC_ProduseP`) ramane aceeasi; istoricul este unic si multianual (fara copiere/reimport pe an), deci nu se suprascriu date ale unor ani inchisi.
- Activele se creeaza de **receptia MF** (VC06-02: varianta + rand `VC_ProduseM` in aceeasi tranzactie); predarea/transferul/casarea (VC07-02/03) le muta intre detinatori.

## Presupuneri si neverificat
- Cele doua campuri istorice `ValoareLuna` / `ValoareLunara`: se citeste/scrie doar `ValoareLuna` (`ValoareLunara` exista doar in baza MF si ramane neatinsa; asocierea lor nu a fost decisa).
- Amortizarea (rapoarte, DateDiff) este VC08-02, nu aici; nu s-a facut un istoric de schimbari al evidentei dincolo de randurile de audit.
- Cele 12 randuri `ProduseM` observate nu au fost verificate pe date reale.
- Nimic rulat.
