# VERCON — matricea de verificare VC07-05 (flux → rezultat)

Stare: **PLANIFICAT, NEEXECUTAT** (12.10.2026). Nimic din aceasta matrice nu a fost rulat; toate scenariile OI/MF/Retetar sunt **sintetice** (bazele reale nu au retururi, casari, transferuri, predari OI — F6), deci nu sunt paritate pe productie. Ruleaza pe `000_DEMO`, cu o gestiune de fiecare tip si doua gestiuni de acelasi tip.

| # | Tip | Flux | Rezultat asteptat |
|---|---|---|---|
| 1 | Materiale | recepție NIR → BC → retur BR → storno FCS | stoc = intrari + retur − iesiri − storno; BC peste stoc refuzat; retur peste cantitatea bonului refuzat; storno peste cantitatea receptiei refuzat |
| 2 | Materiale | NIR cu TVA mixta, valoare factura 0 / 0,99 / 1,00 / 1,01 diferenta | total = suma liniilor (0 si 1,01), antet pastrat (0,99 si 1,00); TVA o singura data |
| 3 | Materiale | editare NIR cu storno; anulare BC cu retur | refuzate (HAS_STORNO / HAS_RETUR) pana se anuleaza dependenta |
| 4 | Alimente | NIR → BC (separat) → lista cu 2 BC-uri → salvare | lista, mesele, feluri, 2 legaturi intr-o tranzactie; BC-ul nu se resalveaza; stocul nu scade a doua oara |
| 5 | Alimente | adauga la lista un BC deja in alta lista / anulat | refuz, nicio legatura partiala, draftul ramane |
| 6 | Alimente | scoate un BC din lista (⋮), salveaza; anuleaza lista | BC-ul ramane neatins si liber; la anulare toate bonurile se elibereaza |
| 7 | Alimente | modifica un BC legat de lista | refuzat (LINKED_LIST) |
| 8 | Alimente | masa absenta / porții 0 / NULL | valoare masa 0, lista se salveaza; fara impartire invalida |
| 9 | OI | NIR → BP → BT → BS | stoc magazie scade la BP (prin BC); detinerea: BP +, BT +/− net zero, BS −; casare 2 × 100 → −2 buc / −200 lei, pret pozitiv |
| 10 | OI | transfer peste ce detine predatorul / lot al altui detinator / inainte de intrarea lotului | refuzat pe linia respectiva |
| 11 | OI | editare/anulare BP din care s-a transferat | refuzat (HAS_OUTPUTS); dupa anularea transferului — permis, detinerea revine |
| 12 | OI | doua BP cu aceeasi cifra de IDBP in istoric (import) si un transfer nou | id-urile noi sunt peste orice id folosit; loturile nu se amesteca |
| 13 | MF | NIR MF 3 active → BP → registru MF | 3 variante + 3 randuri ProduseM; evidenta arata detinatorul; valoare lunara = total/DNF |
| 14 | MF | modifica DNF/DF in registru cu revizie veche | CONFLICT; cu revizie corecta — valoare lunara recalculata |
| 15 | toate | apel direct al functiilor straine tipului (BC pe OI, BP pe Materiale, lista pe MF, registru MF pe OI) | refuzat de server (403), meniul nu le afiseaza |
| 16 | toate | eroare provocata la ultima linie / la registru | rollback complet: niciun antet, linie sau rand de registru ramas |
| 17 | toate | doi utilizatori pe aceeasi gestiune (si pe ani diferiti) | al doilea blocat inainte de acces |
| 18 | toate | an inchis (Materiale/Alimente) | scrierea refuzata prin apel direct (PERIOD_CLOSED); consultarea merge |

Nu sunt acoperite: rapoartele si amortizarea (VC08), retetarul (`VC_Retetar`, model neclar, 0 randuri), corectiile istorice pe an inchis (D05).
