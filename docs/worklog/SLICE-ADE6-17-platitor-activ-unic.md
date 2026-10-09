# SLICE-ADE6-17 — plătitor activ unic per copil

09.10.2026. SCRIS LOCAL.

La crearea prin catalog a unui Platitori_sub, API forțează Activ=True și dezactivează
ceilalți plătitori activi cu același IDP. La salvarea unui plătitor existent activ,
ceilalți plătitori ai copilului sunt dezactivați în același mod. Nu sunt afectate
persoanele asociate altor copii. Un plătitor existent poate fi dezactivat explicit.

Toate scrierile sunt în tranzacția și sub lockul de unitate existente, cu rollback,
versiuni și idempotency. Regula se aplică și apelurilor API și catalog-save, nu doar
interfeței. Nu este o curățare globală a datelor migrate sau o modificare a importului.

Editorul unui plătitor nou arată Activ bifat și blocat; salvarea trimite True.
Pentru un plătitor existent bifa rămâne editabilă. Reîncărcarea catalogului după
salvare arată I nebifat pentru cel activ și bifat pentru ceilalți, conform ADE6-11.

Fișiere: PYTHON/routes/adechit/service.py, static/js/adechit/payers.js,
ajutorul adechit.html și statusul ADE. Nu am rulat teste sau verificări vizuale,
conform cerinței utilizatorului. Fără SQL nou, commit sau publicare. Preview-ul
trebuie repornit pentru schimbarea backendului; publicarea aparține utilizatorului
prin AvacontPush. Următoarea subfelie liberă: ADE6-18.
