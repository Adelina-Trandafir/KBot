# SLICE-AV01-01 - fise de citire, etapa 1 (configurare)

Data: 23.10.2026. Felie doar de documentare: niciun cod, nicio rulare, niciun DDL.

## Ce s-a facut si de ce
Planul de portare Avacont (Access) -> K-BOT, aprobat de operator, incepe cu o sub-felie de citire
pentru etapa 1 (configurare cu sabloane), ca sa nu se porteze tot: fiecare element din ribbon primeste
o fisa si o eticheta PORT / EXISTA / ELIMINAT / DE DECIS, confirmata de operator inainte de cod.
Familia de felii: `AVnn-nn` (alegerea operatorului); AV01 = etapa 1.

## Fisiere create (toate in `docs/access-map/avacont/`)
`README.md` (index + decizii de luat), `conturi-plan-de-conturi.md`, `jurnale.md`,
`clasificatii-venituri.md`, `sablon-operatii-oci.md`, `sablon-operatii-ocs.md`,
`setari-implicite-si-bife.md`.

## Verificari
- Conturi si Jurnale: scrise direct din `AdaugCont_code`, `AdaugCont_sub_code`, `Jurnale_code`,
  `DefaCont subform_code`; o corectie facuta in cursul lucrului (stergerea jurnalului e cu confirmare,
  nu blocata).
- Clasificatii Venituri, Sabloane, Setari: redactate de subagenti; afirmatia ca modificarea/stergerea
  din fereastra Venituri sunt cod mort a fost verificata pe `ClasificatiiBugetareV_code.txt`
  (nu exista rutine de modificare/stergere; `GolBug`/`PrelBug` lucreaza pe tabelele de cheltuieli).
  Restul afirmatiilor din fisele subagentilor NU au fost reverificate de mine rand cu rand.

## Runda 2 (dupa comentariile operatorului, 23.10.2026)
- Stergere BLOCATA pentru conturi si jurnale daca au operatii (in Access relatiile sunt cu Delete in cascada; in
  MariaDB `RESTRICT`). Fisele Conturi si Jurnale actualizate.
- Sablon salarii: regula «nu se salveaza articol fara minim un rand PC» (fisa OCS, sectiunea C1).
- `baza2026.accdb` citita pe o COPIE read-only (DAO), originalul neatins; copia si extragerile au fost sterse din
  scratchpad. Corectie: tabelele NU sunt legate, ci locale (69 locale, 0 legate); structura reala si cifrele sunt in fise.
- Partener = furnizor SAU analitic: fisa noua `parteneri-ca-analitic.md` (32 din 75 fara cod fiscal; 13 din 20 coduri
  folosite in note sunt fara cod fiscal). Parteneri nu mai e «EXISTA».
- `DefaNoteA.CodPartener` are trei continuturi (cheie clasificatie / cod partener Tip M / `XXX`): raspunsul «CORECT» al
  operatorului acopera Tip C; la Tip M datele arata partener.

## Runda 3 (decizii operator, 23.10.2026) - detaliile sunt in `README.md`, sectiunea «Decizii luate»
Plan de conturi pe unitate (nomenclator din `AVACONT_COMUN`); solduri initiale separate; Clasificatii Venituri identic cu
cele normale; setari per utilizator, DelNC/DelCLSF = drept de rol; jurnale si analitice pe unitate; subunitate = alta
unitate subordonata (ca VERCON); sablon pe cont de baza, partener niciunul/al operatiei/fix; `Zec2` = salarii cu 2
zecimale pe subunitate (corectie: nu era rotunjire); «Protectie operatii» = regula ecranului Note, nu setare; `Chelt`
ramane marcaj pe operatie, sens la rapoarte; `Tip` si `ContPl` nu se portheaza.
Corectie de proces: la o raspuns ambiguu («NICIUNUL DIN ASTEA NU PORTAM») am extins gresit un raspuns pe un singur
element la toate patru; reconfirmat apoi explicit, fiecare separat.

## Nerezolvat / neverificat
- `Parteneri.Tip` 1/2: ELIMINAT de operator (23.10.2026, nu mai e relevant). Raman nedocumentate: `DefaClase` si
  `DefaClsfV` (nu exista in `baza2026.accdb`), sensul antetului `Tip = M`,
  al `RPC/RPD`, `Chelt`, `CM`, `Zec2` nu e documentat in cod.
- `ParteneriForm` si slicele 0121-0123 nu au fost citite: nu stiu daca impun cod fiscal obligatoriu.
- Intrebarile deschise din fise asteapta raspunsul operatorului.
- **Neactualizat:** randul familiei AV01 in `KBOT_STATUS.md` si fisierul `state/` corespunzator
  (de facut impreuna cu confirmarea listei). Fara ajutor `0000-NN`: nu s-a schimbat nimic vizibil operatorului.
- Nimic nu a fost commitat (regula operatorului: fara scrieri git).
