# SLICE-VC05-03 — writer atomic si maparea cheilor

Stare: **IMPLEMENTAT_LOCAL**. Se compileaza curat; **nerulat pe nicio baza**, nicio linie scrisa vreodata in MariaDB.

## Facut
- `tools/ddl.js` scrie acum si `VRC.Migrator/target_schema.json` (26 de tabele in ordinea parintilor, cu clasa fiecarei coloane si felul legaturii: FK declarat, legatura logica documentata, grup OI, furnizor, tehnic, neclasificat). Scriitorul se conduce dupa acest fisier, nu dupa 26 de mapari scrise de mana.
- `VrcRunner.vb`: o singura tranzactie: subunitate, gestiuni, exercitii, markerul VC_Imports, surse, apoi randurile (tabelele de gestiune o data, reunite pe ani: aceeasi cheie = aceeasi entitate, anul mai nou castiga atributele; apoi tabelele de exercitiu pe fiecare an), harta VC_IdMap, constatarile, contoarele, soldul initial Alimente.
- Valori exacte INT x 10^scara; refuz (nu rotunjire) la prea multe zecimale sau overflow. Cota TVA x 10000.
- Reguli aplicate: N01/N02 (totalul liniilor cu TVA), pret negativ => pret pozitiv + cantitate negativa (avertisment, numai Intrari/Iesiri/IesiriRetur), ambele negative = blocaj, ProduseP numai cand are folosire, numar NIR negativ numai pentru stoc initial («stoc» in NrFact; TipDoc STOC_INITIAL), furnizor => Parteneri KBOT dupa cod fiscal (lipsa = fara partener + avertisment, CodFiscalSursa pastrat), contoarele pornesc de la ultimul numar real (NIR, BC, LISTA), nu din NR.*.
- Legaturile fara parinte hotarat (IdFel, IdRetur, IdStorno, IDBO, IDM, IdMasinaP) blocheaza doar daca au valori nenule; AutoNumber-urile secundare (LABC.ID, ProduseM.IDPMF) se pastreaza NULL.

## Limite / presupuneri de confirmat
- Numerele NIR cu zecimale (ex. 707,1 = document inserat cronologic) se pastreaza: partea intreaga in Numar, fractia in sutimi in VC_NIR.NumarSub (coloana noua); mai mult de 2 zecimale blocheaza.
- Legaturile logice rezolvate dupa denumirea parintelui: IdMasina -> Masini, IntrariStorno.IdIntrare -> NIR, IntrariStorno/ProduseM.IdProdus -> ProduseP, OI_Transfer.IDC_OLD -> Clienti, OI_Predare.IdBonConsum -> Iesiri_H.
- OI/MF: tabelele OI nu s-au vazut cu date; grupurile IDBP/IDBT/IDBC sunt renumerotate pe gestiune, neprobate.
- Performanta nemasurata (inserare pe rand in tranzactie unica).
