# SLICE-VC05-02 — planul si formularul de asociere

Stare: **IMPLEMENTAT_LOCAL**. Se compileaza curat; **nepornit, nicio baza reala folosita**.

## Facut
- `VrcPlan.vb`: planul unei subunitati (surse, gestiune/tip/an, ZEC si scara comune), validari cu motive concrete de blocare, hash al planului (contextul, regulile si hash-ul fiecarui fisier), export JSON fara parole.
- Formularul: sub tabelul de fisiere, grila de asociere (fisier, an exercitiu, gestiune, tip) cu propuneri din nume/steaguri (nu aprobari), campurile serverului si ale subunitatii, butoanele Verifica planul / Exporta planul / Testeaza / Migreaza / Opreste / Diagnostic, starea pe linia de jos.
- Blocaje: fisier necitit sau nestabil, tabele legate, UNIT.DC lipsa sau diferit intre fisiere, gestiune/tip lipsa, tz_2100 care nu e OI/MF sau fisier de an cu OI/MF, steaguri care contrazic tipul, doua surse pe acelasi an, OI/MF impartit pe ani, valori peste 4 zecimale sau care nu incap in INT, linii cu cantitatea si pretul negative, ProduseP folosit cu pret negativ.
- Avertizari: tabele/coloane absente, ZEC diferit intre surse, preturi negative inversate, antete fata de linii, ProduseP netransferat.
- Orice modificare a planului invalideaza Testeaza (hash comparat la Migreaza).

## Limite
- Un fisier de an da pana la doua gestiuni (Alimente si Materiale) dupa tipul contului din Masini (A = Alimente, M sau fara tip = Materiale); randurile se plaseaza dupa cont sau dupa liniile documentului (VrcPartition). Documentele cu linii din ambele parti blocheaza.
- Anul exercitiului pentru tz_2100 este propus ca cel mai mic an din NIR.Data (operatorul il poate schimba).
