# SLICE-ADE6-16 — lățimi DGV fără depășire accidentală

09.10.2026. SCRIS LOCAL.

Utilizatorul a raportat scroll orizontal în toate cele trei liste Plătitori pe PC.
În componenta comună, DGV avea width:100% plus borduri (content-box), iar lățimea
disponibilă folosea offsetWidth întreg, care poate rotunji în sus la zoom.

Rootul și benzile header/footer folosesc border-box; containerul de scroll poate
scădea în flex fără min-size implicit. Lățimea coloanelor este limitată la spațiul
interior real, calculat din clientWidth și getBoundingClientRect minus borduri și
scrollbarul existent. Dacă este anticipat un scrollbar vertical, se rezervă numai
lățimea încă neocupată. Rezultatul este rotunjit în jos pentru a evita depășirile
fracționare. Nu am ascuns scrollbarul orizontal ca soluție pe PC; rămâne disponibil
când coloanele sunt efectiv mai late decât spațiul disponibil.

Fișiere: PYTHON/static/css/dgv.css, js/dgv/datagrid.js, ajutorul adechit.html
și statusul ADE. Nu am rulat teste sau verificări vizuale, conform instrucțiunilor.
Fără backend/schema/date/SQL, commit sau publicare. Fișierele statice comune se
preiau după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie:
ADE6-17.
