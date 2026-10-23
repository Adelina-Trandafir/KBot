# SLICE-VC03-08 — Diagnostic NIR_FARA_CF după rularea utilizatorului

Data: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Log furnizat de utilizator: Portare_parteneri_codfiscal, hash prefix
24beed22b0f0c6f2, 15 instrucțiuni. AVACONT_SURSA și 15 DC-uri OK; șapte DC-uri
s-au oprit în garda NIR_FARA_CF: 000_DEMO,001_GR23,005_CEVM,012_GR30,
018_GRRS,030_SCTC,050_GRSA. Conform runnerului, datele din bazele eșuate
sunt rollback și bazele nu sunt marcate. Cele trei rânduri raportate de
mai multe baze OK sunt cele trei inserări tehnice în gardă, nu trei NIR-uri.

Pregătit numai SQL de diagnostic:
sql/VC_20261023_FURNIZORI_CF_DIAGNOSTIC_KBOTADMIN.sql,
nume Portare_parteneri_codfiscal_diagnostic. Nu modifică tabelele de date;
folosește exclusiv tabele temporare și transmite un sumar prin eroarea
intenționată DIAG_CF (runnerul existent nu afișează rezultate SELECT).
Sumarul include numărul NIR-urilor fără CF după regula exactă a migrării,
numărul cazurilor cu normalizare mascată de prioritate, indicii textuale
STOC/SOLD INITIAL (doar diagnostic, fără reclasificare), până la trei
exemple IdIntrare/Numar și context, existența rândului partener și a CF sursă.
Mesajele duplicate-key pot fi trunchiate de server; prefixul păstrează
numărătorile, apoi exemplele. Baza cu rezultate diagnostice rămâne nemarcată
și se poate reinteroga; fără rezultate se marchează normal în registru.

Nu a fost modificat textul migrării originale deja executate pe unele baze;
nu se va relansa alt text sub numele Portare_parteneri_codfiscal. Nu s-a
relaxat garda, nu s-a inventat CF și nu s-a reclasificat vreun document.
Fără acces MariaDB, SQL executat, teste sau verificări automate. Cauza
concretă a documentelor rămâne de stabilit din diagnosticul utilizatorului.
