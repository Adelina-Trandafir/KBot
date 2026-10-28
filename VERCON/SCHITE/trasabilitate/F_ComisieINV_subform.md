# Trasabilitate schite comisii de inventar și NIR

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-13, 28.10.2026. Secțiunea este coloana întâi a matricei de controale.

## Variante și surse

### Trasabilitate schita formular ComisieINV subform

Sursa: [Forms/ComisieINV subform.txt](../../VERCON_ACCESS/Forms/ComisieINV%20subform.txt) · imagine: [F_ComisieINV_subform.png](../F_ComisieINV_subform.png)

- Titlu (Caption): ComisieINV subform · Sursa de date (RecordSource): `ComisieInventar` · Latime: 5985 twips · sectiuni: 5 · controale: 11

### Trasabilitate schita formular ComisieNIR subform

Sursa: [Forms/ComisieNIR subform.txt](../../VERCON_ACCESS/Forms/ComisieNIR%20subform.txt) · imagine: [F_ComisieNIR_subform.png](../F_ComisieNIR_subform.png)

- Titlu (Caption): ComisieNIR subform · Sursa de date (RecordSource): `ComisieNIR` · Latime: 5985 twips · sectiuni: 5 · controale: 11

## Secțiuni

| Secțiune | ComisieINV_subform | ComisieNIR_subform |
|---|---|---|
| FormHeader <a id="formheader-inaltime-285-twips-linia-180"></a><a id="formheader-inaltime-285-twips-linia-182"></a> | 1 / 285 / 180 | 1 / 285 / 182 |
| PageHeaderSection <a id="pageheadersection-inaltime-285-twips-linia-271"></a><a id="pageheadersection-inaltime-285-twips-linia-273"></a> | 2 / 285 / 271 | 2 / 285 / 273 |
| Detail <a id="detail-inaltime-286-twips-linia-302"></a><a id="detail-inaltime-286-twips-linia-304"></a> | 3 / 286 / 302 | 3 / 286 / 304 |
| PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-479"></a><a id="pagefootersection-inaltime-0-twips-linia-482"></a> | 4 / 0 / 479 | 4 / 0 / 482 |
| FormFooter <a id="formfooter-inaltime-600-twips-linia-490"></a><a id="formfooter-inaltime-600-twips-linia-493"></a> | 5 / 600 / 490 | 5 / 600 / 493 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie (twips) | Evenimente | ComisieINV_subform | ComisieNIR_subform |
|---|---|---|---|---|---|---|---|
| FormHeader | Receptie1_Label | Label | Persoana 1 | 285,0 1425x285 |  | 1:188 | 1:190 |
| FormHeader | Receptie2_Label | Label | Persoana 2 | 1710,0 1425x285 |  | 2:208 | 2:210 |
| FormHeader | Receptie3_Label | Label | Persoana 3 | 3135,0 1425x285 |  | 3:228 | 3:230 |
| FormHeader | Label12 | Label | Persoana 4 | 4560,0 1425x285 |  | 4:248 | 4:250 |
| PageHeaderSection | IdComisieInv | TextBox | IdComisieInv | 0,0 1581x285 |  | 1:282 | — |
| PageHeaderSection | IdComisie | TextBox | IdComisie | 0,0 1581x285 |  | — | 1:284 |
| Detail | Receptie1 | TextBox | Receptie1 | 285,0 1425x285 |  | 1:309 | 1:311 |
| Detail | Receptie2 | TextBox | Receptie2 | 1710,0 1425x285 |  | 2:333 | 2:335 |
| Detail | Receptie3 | TextBox | Receptie3 | 3135,0 1425x285 |  | 3:358 | 3:360 |
| Detail | Receptie4 | TextBox | Receptie4 | 4560,0 1425x285 |  | 4:383 | 4:385 |
| Detail | Command4 | CommandButton | Command4 | 0,0 291x286 | OnClick | 5:409 | 5:411 |
| FormFooter | btnIesire | CommandButton | IEȘIRE | 2208,72 1365x453 | OnClick | 1:498 | 1:501 |
