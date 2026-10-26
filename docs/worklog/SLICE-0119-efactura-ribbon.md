# SLICE-0119 - E-Factura pe ribbon; «Conturi Unitate» si «Date Unitate» in Configurari (13.10.2026)

Reper ulterior: [00EF-30](SLICE-00EF-30-primite-sincronizare-din-ribbon.md) mută sincronizarea din dialog în pagina facturilor; [0114-10/18](SLICE-0114-ribbon.md#slice-0114-10) păstrează mutarea comenzilor și corecția câmpurilor. Corpul de mai jos este descrierea inițială.

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md` (sectiunea 3d, cele doua runde din 14.10 ale copiei). Felie inregistrata dupa unire, ca lucru deja facut (operatorul a ales numar nou, nu o sub-felie 00EF).

## Ce s-a schimbat si de ce
1. **Fila «E-Factura» a ribbonului** (0114):
   - grupul **E-Factura**: **«Vanzare»** (`efactura_vanzare`, macheta registrului; fosta intrare «E-Factura» din MENIU, `FacturiForm`, facturile emise) si **«Cumparare»** (`efactura_cumparare`, nou: `PrimiteForm` ca pagina, toate facturile primite ale anului, cu cautare);
   - grupul **ANAF**: **«Token ANAF»** (`ef_token`, `TokenForm`, dialog) si **«Sincronizeaza facturi primite»** (`ef_primite_sync`, `SincronizarePrimiteForm`, dialog), deschise direct din `KbotForm.EFactura.vb`.
2. **Optiunile din bara de titlu a `FacturiForm` au iesit**: `capBar.ShowOptionsButton = False`; meniul lui (Conturi Unitate, Date Unitate, Sincronizeaza facturi primite, Facturi primite) e inlocuit de butoanele din ribbon.
3. **Fila «Configurari» › grupul «Unitate»**: **«Conturi Unitate»** (`ConturiForm`) si **«Date Unitate»** (`DateUnitateForm`), dialoguri, se deschid direct (`KbotForm.RunUnitCommand`), fara fereastra E-Factura deschisa. Daca aceea e deschisa, afla ce s-a salvat (`FacturiForm.UnitDataChangedElsewhere` / `UnitAccountsChangedElsewhere`).
4. Machetele E-Factura sunt pagini (0113): «Iesire» al ferestrelor de facturi e ascuns; se inchid din «Inchide» al ribbonului. A doua cerere aduce in fata pagina deja deschisa.

## Fisiere
- Modificate: `src/KBot.App/KbotForm.EFactura.vb` (`DeschideEFactura`, `DeschideEFacturaCumparare`, `RunEFacturaCommand`, `RunUnitCommand`, `OpenSincronizarePrimite`, `OpenTokenAnaf`), `src/KBot.EFactura/FacturiForm.Designer.vb` (o linie), `src/KBot.EFactura/FacturiForm.Furnizor.vb` (`UnitDataChangedElsewhere`, `UnitAccountsChangedElsewhere`), `src/KBot.App/AvacontMain.Designer.vb` (fila).

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

## Neverificat / amanat
- Mesajul din `FacturiForm.Trimitere.vb` (tokenul refuzat de ANAF) spune inca «Reinnoiti tokenul cu butonul «Token ANAF» din subsolul ferestrei.»: butonul e acum in ribbon (E-Factura › ANAF). Text de pe ecran invechit, nemodificat (cod).
- Meniul din bara de titlu a `FacturiForm` ramane in Designer, doar ascuns.
- FileVersion `KBot.EFactura`, `KBot.App`: nebumpate.
- Ajutorul: 0000-73.
