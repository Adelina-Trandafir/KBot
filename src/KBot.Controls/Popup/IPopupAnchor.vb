Option Strict On

''' <summary>
''' Controlul care DESFĂȘOARĂ un <see cref="CustomPopup"/> și vrea să arate că meniul e al lui.
'''
''' Un buton care deschide un meniu nu trebuie să se stingă în clipa în care meniul apare: cât
''' timp meniul e pe ecran, butonul rămâne aprins, iar cele două citesc ca o singură suprafață
''' desfășurată — la fel ca la orice bară de instrumente Windows. Altfel operatorul vede un meniu
''' plutind lângă un buton care pare deja uitat, și nu mai are din ce deduce de unde a ieșit.
'''
''' <para>De ce interfață și nu «gazda își pune singură un steag»: gazda ar trebui să-l ridice
''' înainte de <c>ShowBelow</c> ȘI să-l coboare pe <c>FormClosed</c> — iar Esc, clicul în afară și
''' alegerea unui rând sunt trei drumuri diferite de închidere. Prima gazdă care uită unul lasă
''' butonul aprins pentru totdeauna. Așa, <see cref="CustomPopup"/> stinge butonul EL, pe sinkul
''' prin care trec toate trei.</para>
'''
''' Implementarea NU trebuie să fie idempotentă din partea apelantului: popup-ul cheamă
''' <c>SetPopupOpen(True)</c> exact o dată la deschidere și <c>SetPopupOpen(False)</c> exact o
''' dată la închidere.
''' </summary>
Public Interface IPopupAnchor

    ''' <summary>
    ''' Meniul desfășurat din acest control tocmai s-a deschis (<paramref name="open"/> = True)
    ''' sau s-a închis. Implementarea își aprinde/stinge butonul și se repictează.
    ''' </summary>
    Sub SetPopupOpen(open As Boolean)

End Interface
