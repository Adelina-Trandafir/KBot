# SLICE-00EF-57 - XmlViz: bara de derulare a XML-ului facturii urmeaza tema (03.11.2026)

Cererea operatorului (captura din «XML Factura»): sa fie rezolvate si scroll-urile din editorul de XML. `XmlViz` a fost atins ultima data de 00EF-29 / 00EF-36; 00EF-55 si 00EF-56 erau luate, deci 00EF-57. **Inchisa: operatorul a verificat pe ecran ca merge (03.11.2026).**

## What it is now
- Caseta de text a `XmlViz` (`rtb`) e un `KBotRichScrollBox` (baza din ME01-14; capcanele si comportamentul sunt acolo): bare `KBotScrollBar` tematizate, doar cand XML-ul nu incape, cursor-sageata, rotita si Shift + rotita.
- `XmlViz.ApplyTheme` da schema barelor (`rtb.ApplyBarTheme(k_scheme)`); `ApplyBarTheme` e `Public` in `KBotRichScrollBox`.
- Arborele din stanga e un `AdvancedTreeControl`, cu bara deja tematizata.
- KBot.Controls 1.84.0.0. Ajutor: nicio sectiune nu descrie barele XML-ului, nimic de etichetat.

## Files touched
`KBot.Controls/Xml/XmlViz.Designer.vb`, `XmlViz.vb`; `KBot.Controls/RichText/KBotRichScrollBox.vb` (`ApplyBarTheme` Public); `KBot.Controls.vbproj`; `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
Build `KBot.Controls`: 0 avertismente, 0 erori. Rulat si verificat de operator.

## Unverified / deferred
XML-uri foarte lungi (mii de linii): masurarea intinderii face o reimprospatare la fiecare schimbare de text; nemasurat ca timp.
