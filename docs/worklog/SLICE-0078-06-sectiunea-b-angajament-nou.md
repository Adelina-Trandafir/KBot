# SLICE 0078-06 — Secțiunea B inserată în DDF-ul semnat pe A, în fluxul real (angajament nou)

Data: 29.09.2026. Cererea operatorului: calea probată pe banc în 0078-04/05 (A reală + B
provizorie ▸ semnat A ▸ B reală scrisă incremental în fișierul semnat ▸ semnat B) se aplică la
crearea unui angajament nou (cod «!…»):

1. «Generează» în pagina «Document» a DDF-ului, pentru revizia netrimisă: ambele secțiuni; B cu
   valorile provizorii pe care le are MariaDB (codul «!…»).
2. Utilizatorul care a generat semnează A.
3. Alt utilizator îl trimite în FOREXE (Creare Angajament); codurile reale se salvează în MariaDB.
4. La deschiderea DDF-ului în vederea «Doc. Fund.»: documentul cu A semnată + Secțiunea B nouă
   inserată, ca pe bancul `DdfSectiuneaBHarnessForm`.
5. Semnează B (nu e obligatoriu) ▸ se salvează pe server; fără semnătură nu se salvează nimic.

Număr: **0078-06** (propus, sub-felie a 0078, ca 0078-04/05; neconfirmat de operator).

Hotărâri ale operatorului (întrebate, 29.09.2026):
- «Generează PDF final» nu mai regenerează un PDF semnat pe A: A+B semnate ▸ doar trece revizia la
  etapa 3; doar A ▸ se deschide vederea DDF cu B inserată, iar etapa 3 se pune când semnătura B
  ajunge pe server.
- Inserarea pune ȘI capturile FOREXE (Table4), nu doar rândurile B.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Documentul intermediar: codul provizoriu din MariaDB

`DdfXmlBuilder.InterimSectionB` (mod `Interim`): codul angajamentului din B = codul revizuit din
antet (`FX_DDF.CodAngajament`, la angajament nou «!» + 10 caractere = 11) când are exact 11
caractere, altfel `___________`. Indicatorul = `FX_DDF_REV_SB.CodIndicator` (potrivit după poziție,
doar când A și B au același număr de rânduri) când are exact 3 caractere, altfel `___`.

**Verificat în machetă** (scripturile din `C:\KBOT\Temp\PDF\DDF_NR_12_REV_1_AAB2KXS6FKM.PDF`):
«Validează» cere cod angajament = **11** caractere și indicator = **3** caractere în Secțiunea B.
Indicatorul provizoriu al K-BOT e «!» + 3 = **4** caractere ▸ formularul l-ar refuza, deci în
practică indicatorul rămâne `___`. Programul rămâne `0000000000` (A și B), SSI și sumele ca înainte.

### 1.2 Vederea DDF: Secțiunea B scrisă în documentul semnat pe A

`DdfView.EnsureSignedPdfAsync`, după ce copia de pe server e verificată în cache (`PdfCache` ▸
`Gata`), apelează `SectiuneaBInseratAsync`. Inserarea are loc când (`DdfSectionBInsert.IsCandidate`):
- revizia e trimisă (`StareTrimitere` 2 sau 3);
- PDF-ul de pe server e semnat pe A și NU pe B (`FX_DDF_REV.Semnatura`);
- angajamentul are deja codul FOREXE (nu începe cu «!»);
- și (`ServerRowsReady`) toate rândurile `FX_DDF_REV_SB` ale reviziei au cod de 11 și indicator de
  3 caractere, fără «!» și fără `_`. Altfel: un mesaj (o dată pe revizie) că B nu are încă codurile,
  și se arată documentul semnat pe A.

Inserarea (`DdfSectionBInsert.InsertAsync`, clasă NOUĂ, același drum ca bancul):
- datele reviziei cu `pentru_generare=1`; rândurile B de pe server; capturile FOREXE ale reviziei
  (`DdfPdfGenerator.CapturileAsync`, devenit `Friend`);
- XML-ul = `DdfXmlBuilder.BuildFormXml` tăiat la `SubformSectiuneaB` (rânduri + `CheckBox9` +
  `Subform51/Table4`); programul lui B = programul din Secțiunea A a documentului semnat;
- `XfaSignedDocument.FillIncremental` (doar `datasets` + NOTAFD.xml) într-un fișier NOU din zona
  de lucru: `<nume canonic>_B_HHmmss.pdf`; sursa se citește partajat și nu se atinge;
- verificare: octeții semnați = prefixul fișierului nou, altfel excepție (nu se arată nimic
  care ar fi pierdut semnătura A); se numără capturile ajunse în Table4.
- Mesaj către operator: B pusă, semnați B, fără semnătură nu se salvează nimic (+ avertisment dacă
  au intrat mai puține capturi decât s-au trimis). Linia NOTAFD în `mesaje_operator.log`.
- Fișierul făcut se refolosește cât timp suma de pe server e aceeași (`_inserariB`), deci un al
  doilea click nu mai face alt fișier.

Fișierul nou devine calea rezolvată a reviziei ▸ `EnsureSigning` pornește sesiunea de semnare pe
el, cu precedentul = suma de pe server și cache-ul = calea canonică. Semnat B ▸ sesiunea încarcă
(și scrie cache-ul). Nesemnat ▸ nimic pe server.

### 1.3 După încărcarea semnăturii B: etapa 3

`DdfView.OnSigningCompleted`: încărcare reușită, documentul era cel inserat, rolurile conțin B,
revizia e la etapa 2 ▸ comanda nouă `DdfActiune.FinalizeazaPdf` către shell
(`KbotForm.FinalizeazaPdfDupaSemnareBAsync`): recitește revizia; dacă e la etapa 2 cu A și B
semnate, o trece la etapa 3 (`SeteazaStareTrimitereDdfAsync(FinalPdf)`) și reîncarcă. **Excepție:**
Rev 0 a unui angajament creat în K-BOT (`FX_DDF.Manual`) cât timp FOREXE nu e «În derulare» —
acolo «Definitivează» / «Derulează» vin întâi (varianta a); etapa se mută mai târziu din
«Generează PDF final».

### 1.4 «Generează PDF final» nu mai regenerează un PDF semnat

`KbotForm.GenereazaPdfFinalAsync` (meniul Rezervări și trimiterea unei revizii de angajament
existent) citește revizia proaspăt:
- semnată A și B ▸ doar etapa 3;
- semnată doar A ▸ mesaj «nu se mai generează din nou…» și apelantul deschide vederea DDF, care
  inserează B (1.2); etapa 3 la încărcarea semnăturii B (1.3);
- nesemnată ▸ ca înainte (regenerare completă + încărcare nesemnată + etapa 3).

### 1.5 Bancul

`DdfSectiuneaBHarnessForm` folosește acum `DdfSectionBInsert.BuildFillXml` și `SectionAProgram`
(o singură construcție a lui B pentru banc și producție; bancul nu trimite capturi).

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/Ddf/DdfSectionBInsert.vb` | **NOU** — condițiile, XML-ul lui B, inserarea în zona de lucru |
| `src/KBot.App/Views/DdfView.vb` | `SectiuneaBInseratAsync`, `IsInsertedFile`, `_inserariB`, cererea `FinalizeazaPdf` după încărcarea B |
| `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` | B provizorie cu codul / indicatorul din MariaDB când au lungimea cerută (`InterimValue`) |
| `src/KBot.App/Views/Ddf/DdfPdfGenerator.vb` | `CapturileAsync` Private ▸ Friend |
| `src/KBot.App/Views/Ddf/DdfComanda.vb` | `DdfActiune.FinalizeazaPdf` |
| `src/KBot.App/KbotForm.Ddf.vb` | ramura `FinalizeazaPdf` |
| `src/KBot.App/KbotForm.DdfSendMenu.vb` | `GenereazaPdfFinalAsync` fără regenerare pe PDF semnat; `FinalizeazaPdfDupaSemnareBAsync` |
| `src/KBot.App/HarnessTests/DdfSectiuneaBHarnessForm.vb` | folosește constructorul comun al lui B |

FileVersion: `KBot.App` rămâne **1.1.0.2** (crescut în 0078-04; 0078-05 îl nota «nelivrat» — nu am
putut verifica dacă între timp s-a livrat; dacă da, de crescut la publicare).

## 3. Rezultatele testelor

- `dotnet build src\KBot.App\KBot.App.vbproj` (ieșire în afara `bin\`): **0 erori, 0 avertismente**.
- Nicio suită rulată, nimic pornit pe ecran (regula casei).

## 4. Rămas NEVERIFICAT / amânat

1. **Nimic rulat.** Fluxul real (generare ▸ semnat A ▸ trimitere ▸ inserare în vedere ▸ semnat B ▸
   încărcare ▸ etapa 3) nu a fost văzut; bancul a probat doar pașii de fișier.
2. **Codul «!…» în Secțiunea B provizorie**: are lungimea 11, dar nu știu dacă «Validează» / XSD-ul
   NOTAFD acceptă «!» (am văzut doar verificarea de lungime). Dacă refuză, `InterimValue` trebuie să
   cadă pe `___________`.
3. **Capturile în Table4 pe documentul semnat**: `AdobeUtils.ProcessXmlNodes` caută `Table4` în
   datele documentului; dacă documentul semnat nu are nodul, capturile nu intră (mesajul spune câte
   au intrat). Nu știu nici dacă Adobe desenează imaginile venite într-o actualizare incrementală,
   nici cât crește fișierul.
4. **Angajament nou, B semnată înainte de «Derulează»**: capturile făcute la «Definitivează» /
   «Derulează» NU mai intră în PDF (după semnătura B documentul nu se mai completează);
   «Generează PDF final» doar mută etapa.
5. **Programul `0000000000`** rămâne în A și B (ca pe banc).
6. **Două mesaje la trimiterea unei revizii de angajament existent** (cel din «Generează PDF final» și
   cel al inserării din vedere). Acceptat; de redus dacă deranjează.
7. Dacă semnătura B e încărcată din alt loc decât documentul inserat (de ex. fișier ales din pagina
   «Fișiere»), comanda `FinalizeazaPdf` nu pleacă; etapa se mută din «Generează PDF final».
