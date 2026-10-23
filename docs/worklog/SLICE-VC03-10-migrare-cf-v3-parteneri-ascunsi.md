# SLICE-VC03-10 — Migrare CF v3: ignoră partenerii Ascuns=1

Data: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Clarificare explicită a utilizatorului: partenerii cu Ascuns=1 NU au NIR-uri
sau alte documente. Nu este necesară și nu s-a implementat vreo logică de
propagare/excludere pentru documente ale unor parteneri ascunși.

Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql,
nume NOU Portare_parteneri_codfiscal_v3. Păstrează V2 și adaugă doar
AND COALESCE(P.Ascuns,0)<>1 la cele două căutări în Parteneri (recuperarea
CF pentru NIR și factura comună). NULL este tratat ca 0. Tabela Parteneri
nu este actualizată, ca și în variantele precedente. Documentele istorice
fără CF nu blochează baza; contradicțiile între CF-uri cunoscute rămân
blocante. Nu se inventează CF și nu se reclasifică documente.

Fișierele și interogările originale/V2 rămân neschimbate, deoarece pot
fi deja înregistrate în Interogari_Unice. V3 folosește nume și hash noi;
nu inversează schimbări ale unei rulari deja încheiate.
Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql (nou);
PYTHON/routes/vercon/supplier_identity.py (referință la V3);
implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-10-migrare-cf-v3-parteneri-ascunsi.md.
Stare: SQL PREGĂTIT, NEEXECUTAT de asistent. Fără teste, verificări
automate/de sintaxă sau acces MariaDB. Nicio modificare de date/structură
executată. De rulat de utilizator prin Interogări unice, cu numele V3.
