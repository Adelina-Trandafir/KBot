# SLICE-00EF-58 - Jurnalul serverului spune ce factura a fost respinsa la PDF-ul ANAF (03.11.2026)

Cererea operatorului: un utilizator (006_GR35) primeste «ANAF nu a dat PDF-ul facturii» si nu aflam de ce. Logurile lui arata ca ANAF raspunde HTTP 200 cu pagina «Request Rejected» (firewall F5, cu «support ID»), nu cu un PDF. `api_server.log` scria doar `HTTP 200, the answer is not a PDF`: nici factura, nici textul raspunsului, nici marimea XML-ului. Fara cod nou de comportament: doar jurnal. 00EF-57 era ultima luata, deci 00EF-58.

## What it is now
- `anaf_api.pdf_din_xml(xml_bytes, k_label="")`: linia de eroare din `api_server.log` poarta eticheta facturii, standardul trimis (FCN / FACT1), marimea XML-ului, HTTP-ul, `Content-Type`-ul raspunsului, marimea raspunsului si primele 300 de caractere din el (aici apare «support ID» al firewall-ului, pe care ANAF il poate cauta). La fel pentru ramura 5xx / 408 / 429.
- `primite.pdf_anaf` da eticheta `primita <id_primita> <nume fisier>`, `trimitere.pdf_anaf` da `trimisa <id_factura> <nume fisier>`.
- Mesajul pe care il vede operatorul nu s-a schimbat.

## Cause found (03.11.2026)
Proba cu `curl` de pe PC, aceeasi factura (primita 932, XML de 16 KB, fara atasamente), aceleasi antete in afara de `Content-Type`:
- `text/plain; charset=UTF-8` (ce trimitea codul): «Request Rejected», si fara `charset`, si cu User-Agent-ul implicit al `curl`.
- `application/xml` si `application/xml; charset=UTF-8`: PDF (25 KB, `%PDF`).
Deci firewall-ul ANAF refuza corpul `text/plain`; User-Agent-ul nu conteaza. Corectura: `pdf_din_xml` trimite `application/xml; charset=UTF-8`. Probat doar de pe PC, nu de pe VPS.

## Files touched
`PYTHON/routes/efactura/anaf_api.py`, `primite.py`, `trimitere.py`; `state/KBOT_STATUS_0000-0009.md`.

## Test results
`py_compile` pe cele trei fisiere: fara erori. Nerulat pe server, nerulat contra ANAF.

## Unverified / deferred
- Corectura nu e probata din aplicatie, pe server: curl de pe PC a primit PDF cu `application/xml`; de pe VPS (alt IP) nu s-a incercat. Daca dupa urcare mai apare «Request Rejected», linia de log ramane cu support ID-ul pentru ANAF.
- Fisierele din `PYTHON/` le urca operatorul de mana pe server.
- Ajutor: nimic de etichetat (nu se schimba nimic din ce vede operatorul).
