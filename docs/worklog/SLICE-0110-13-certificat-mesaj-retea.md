# Slice 0110-13 — Mesaj corect pentru eroarea de rețea la intrarea cu certificat

12.10.2026. Cererea operatorului: corectat doar mesajul; pagina fusese deschisă
la o adresă greșită. Configurația CORS și comportamentul certificatului în Chrome
rămân în afara acestei schimbări.

În `PYTHON/static/js/portal/certificat.js`, `NETWORK` și `CERT_ABSENT` au mesaje
separate. Un eșec fetch (inclusiv CORS) nu dovedește absența certificatului și
nu permite identificarea sigură a cauzei din interfață.

- `NETWORK`: „Nu s-a putut comunica cu serviciul de autentificare. Verifică adresa
  paginii și conexiunea, apoi reîncearcă. Poți intra cu parola și codul.”
- `CERT_ABSENT`: „Serverul nu a primit niciun certificat. Verifică dacă tokenul
  este conectat și certificatul a fost selectat în browser. Poți intra cu parola și codul.”

Publicare: doar fișierul JavaScript, apoi reîncărcarea paginii cu Ctrl+F5.
Fără DDL sau restart al backendului. Ajutorul aplicației desktop nu este afectat;
mesajele portalului sunt documentate aici.

Verificare: `node --check` și `git diff --check`. Fără teste funcționale sau vizuale;
verificarea în browser și publicarea rămân la operator.
