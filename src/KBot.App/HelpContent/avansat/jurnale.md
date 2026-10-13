---
id: avansat.jurnale
title: Când ceva nu merge: jurnalele
part: avansat
order: 50
parent: avansat
screens: SetariAplicatieView.cboVerbose, SetariAplicatieView.chkLogViewer
keywords: jurnal, log, erori, diagnostic, harness_errors, adobe_preview, mesaje_operator, consola detaliata
open: menu:jurnal
---
<!-- slice: 0031-01, 0072, 0089, 0089-01, 0118 -->
K-BOT scrie tot ce face în jurnale, în folderul **Logs** (vezi [Căi fișiere](topic:avansat.foldere)).
Le citești din macheta **«Jurnal activitate»** (ribbon, **Configurări › Configurare**) — [Jurnal activitate](topic:contabil.jurnal) — sau direct din folder.

| Jurnal | Ce conține |
|--------|------------|
| `harness_errors.log` | erorile aplicației, cu toate detaliile tehnice |
| `mesaje_operator.log` | fiecare mesaj arătat operatorului, exact cum l-a văzut |
| `adobe_preview.log` | afișarea documentelor în Adobe: ce s-a hotărât și de ce |
| `activex_check.log` | diagnosticul detaliat al controlului ActiveX (doar cu comutatorul pornit) |

Macheta **«Jurnal activitate»** arată și jurnalele serverului pentru utilizatorul și sesiunea ta, dar numai cât timp
[opțiunile avansate](topic:avansat) sunt pornite. Cu ele pornite, lista de deasupra tabelului îți lasă să alegi
între **Jurnale locale**, **Server FOREXE** și **Timpi FOREXE** și câte dintre ultimele tale conectări să se
arate. Fără opțiunile avansate lista nu se vede deloc: macheta arată doar jurnalele de pe acest calculator.

## Consola FOREXE detaliată
<!-- slice: 0071, 0072 -->

În **Setări › Aplicație**, **«Consola FOREXE detaliată»** alege cât scrie consola robotului:
«Pornit» arată fiecare pas și fiecare așteptare, «Oprit» doar mesajele importante și erorile.
Fișierul de jurnal primește oricum tot.

## Ce trimiți când ceri ajutor
<!-- slice: 0000-05 -->

Când raportezi o problemă, trimite **ora** la care s-a întâmplat și fișierele `harness_errors.log`
și `mesaje_operator.log` din folderul Logs de pe calculatorul **pe care** a apărut problema.
