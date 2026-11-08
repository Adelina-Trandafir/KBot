# SLICE-02EF-07 - «Date recepție»: tabs «Adaugă» / «Asociate» / «Existente»; no «Tip» column (06.11.2026)

## Request
«Recepție nouă» -> **«Adaugă»**; a tab **«Asociate»** while a ticked invoice has receptions linked (grid like the existing receptions with **«Valoare»** = what the ticked invoices take of each); «Recepții existente» -> **«Existente»**; column «Tip» removed; **«Adaugă» only while no ticked invoice is associated in full**.

## Final state (now in `KeReceptiiView`)
Tabs «Adaugă» (`noua`), «Asociate» (only while a ticked invoice has links; nothing is made from it, the button is disabled), «Existente». «Adaugă» is hidden while a ticked invoice is covered in full; the view moves to the first tab left when the one on goes away. A partly linked invoice keeps «Adaugă» (a new reception for the rest). Help 0000-162. Never seen on screen.
