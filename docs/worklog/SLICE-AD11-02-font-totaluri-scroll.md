# SLICE-AD11-02 — Font selector, fundal totaluri și scroll mobil

## Ce s-a schimbat și de ce

Observațiile utilizatorului: opțiunile copilului aveau font prea mic; subtotalurile
și totalul final necesitau fundal gri cu contrast diferit; pe telefon pagina nu se
derula. Regula globală din portal.css fixa html/body la 100dvh și overflow:hidden.
Override-ul este limitat la parent-page și permite scroll document până la raport.
Font 16px în dropdown, rânduri minimum 44px; gri #eeeeee subtotal / #d6d6d6 total
în HTML și PDF, culori CSS în paleta comună. Print-color-adjust păstrează fundalul.

## Fișiere

adechit-parents.css, variables.css, adechit-statement.css, statement.html,
parent_statement.py, PORTAL_PARINTI.md, UTILIZARE_WEB.md și statusurile ADE.

## Verificări

Citire reguli/CSS și identificarea overflow:hidden global. Compilare sintactică Python.
Fără teste automate sau verificări vizuale, conform preferinței proiectului.

## Nevalidat

Proba mobil/dropdown/print/PDF aparține utilizatorului pe localhost. Niciun SQL nou.
Restart local pentru generatorul PDF; autentificarea trebuie reluată după restart.
