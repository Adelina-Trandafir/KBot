Option Strict On
Imports System.Collections.Generic

' ══════════════════════════════════════════════════════════════════════════════════════
' Felia 0048-04 — editorul de asociere R <-> H, disponibil ORICAND.
'
' De ce e alt set de tipuri decat cele din AsociereInfo.vb: acelea descriu asocierea
' facuta IN TIMPUL unei descarcari. Acolo exista o sarcina utila, iar fiecare instantaneu
' e ancorat pe INDICELE randului lui in `TabelIstoric` (F24), fiindca id-urile atribuite
' in faza de propunere dispar la derularea inapoi.
'
' Aici nu exista sarcina utila si nu se deruleaza nimic inapoi: operatorul deschide
' legaturile deja scrise si le corecteaza. Ancora e `FX_Receptii_H.IDRH`, cheia reala.
'
' Access avea EXACT gazda asta si nu e in export: `frmFX_DUBII_LISTA_HA.Form_Open` si
' `frmFX_DUBII_LISTA_RH.Form_Open` se ramifica pe `isLoaded("frmFX_ASOC")` — gazda de
' ingestie e `frmFX_DUBII`, gazda de oricand e `frmFX_ASOC`. Cele patru subformulare sunt
' exportate si ele poarta regulile; lipseste doar aspectul gazdei.
'
' Perechea de pe fir: routes/forexe/asociere.py. Numele campurilor JSON sunt ASCII pe
' AMANDOUA laturile (regula 0).
' ══════════════════════════════════════════════════════════════════════════════════════

''' <summary>
''' Tabloul de asociere al unui angajament, citit direct din baza. POCO, fara logica.
''' </summary>
Public NotInheritable Class AsociereStare

    ''' <summary>
    ''' Codul-motiv pe care serverul il pune in «reason» cand o comanda atinge o legatura
    ''' inghetata de regula de blocare. Stabil, ca si celelalte coduri-motiv.
    ''' </summary>
    Public Const MotivInstantaneuBlocat As String = "INSTANTANEU_BLOCAT"

    Public Property CodAngajament As String = String.Empty

    ''' <summary>
    ''' Amprenta starii la momentul citirii. Se trimite inapoi la salvare; daca intre timp
    ''' alta sesiune a miscat ceva, serverul raspunde 409 si nu scrie nimic.
    ''' </summary>
    Public Property Amprenta As String = String.Empty

    ''' <summary>TOATE recepțiile angajamentului, inclusiv cele sterse.</summary>
    Public Property Receptii As New List(Of ReceptiePropusa)

    ''' <summary>
    ''' TOATE instantaneele angajamentului — si cele asociate, si cele neasezate, si cele
    ''' marcate «nu consemneaza nicio schimbare». Cele asociate sunt chiar subiectul aici.
    ''' </summary>
    Public Property Instantanee As New List(Of InstantaneuLegat)

    ''' <summary>Plățile angajamentului, pentru contextul din formular.</summary>
    Public Property Plati As New List(Of PlataAsociere)

    ''' <summary>Instantaneul cu acel <c>IDRH</c>, sau Nothing.</summary>
    Public Function Instantaneu(idrh As Integer) As InstantaneuLegat
        For Each i As InstantaneuLegat In Instantanee
            If i.Idrh = idrh Then Return i
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Propunerea unei descărcări, adusă la forma pe care o citește editorul (felia 0055).
    '''
    ''' <para><b>De ce se convertește în loc să se scrie a doua oară același ecran.</b> Cele
    ''' două tablouri descriu ACEEAȘI treabă — care instantaneu ține de care recepție — și
    ''' diferă doar prin ancoră și prin cine le-a propus. Un al doilea formular ar fi însemnat
    ''' două desene ale aceleiași hotărâri, care alunecă unul față de altul, și două locuri de
    ''' învățat pentru operator.</para>
    '''
    ''' <para><b>Cheia și ancora sunt două lucruri, și de la 18.09.2026 nu se mai suprapun.</b>
    ''' <see cref="InstantaneuLegat.Idrh"/> e cheia pe care editorul își ține dicționarele:
    ''' în propunere e <c>IDRH</c>-ul dat în faza întâi — unic în tabloul de față, dar NU un
    ''' nume care supraviețuiește derulării înapoi, deci nu pleacă niciodată spre server.
    ''' Ancora — ce pleacă înapoi la salvare — călătorește separat, pe
    ''' <see cref="InstantaneuLegat.RandIstoric"/> (indicele rândului în <c>TabelIstoric</c>,
    ''' F24) sau, când rândul nu e în această descărcare, pe
    ''' <see cref="InstantaneuLegat.Idh"/> (F34). Până la felia asta cheia ERA indicele,
    ''' ceea ce lăsa fără nume orice instantaneu rămas dintr-o rulare mai veche: fluxul
    ''' REVERSE nu-i mai aduce rândul de istoric. Vezi <see cref="AncoraAsociere"/>.</para>
    '''
    ''' <para><see cref="InstantaneuLegat.Idrr"/> primește SUGESTIA automată a serverului, ca
    ''' operatorul să vadă unde ar cădea rândul dacă n-ar face nimic — se ARATĂ ca sugestie,
    ''' nu ca fapt (F18), fiindcă trecerea automată poate fi și greșită, nu doar incompletă.
    ''' <see cref="InstantaneuLegat.Blocat"/> rămâne False: nimic nu e încă scris, deci nimic
    ''' nu poate fi înghețat de o ordonanțare.</para>
    '''
    ''' <para><b>CONTEXTUL vine si el, si e o schimbare din 08.09.2026.</b> Pana atunci
    ''' tabloul propunerii continea DOAR randurile de asezat, deci o recepție al carei lanț
    ''' era deja legat sosea aici goală: fără linie în grafic (graficul sare peste lanțurile
    ''' de lungime zero), fără marcaje pe bandă, fără nimic sub ea în arbore. De la locul
    ''' operatorului, «recepțiile vechi nu mai vin». Erau acolo; povestea lor nu era.
    ''' <see cref="PrelucrarePropunere.InstantaneeAsezate"/> aduce restul, iar
    ''' <see cref="PrelucrarePropunere.Plati"/> aduce reperele de plată — §1.3, chiar miza
    ''' așezării.</para>
    '''
    ''' <para><b>Cheile lor sunt NEGATIVE, și de-asta.</b> Toate cheile sunt <c>IDRH</c>-uri
    ''' reale, deci distincte între ele; negarea (<c>-IDRH</c>, reversibilă fiindcă
    ''' <c>IDRH</c> pornește de la 1) spune dintr-o privire că rândul nu poartă o hotărâre
    ''' și îl ține departe de cele pozitive ale rândurilor de decis. Nimic nu le trimite
    ''' înapoi — toate sunt <see cref="InstantaneuLegat.Blocat"/>, iar formularul nu
    ''' construiește decizii din rânduri blocate.</para>
    ''' </summary>
    Public Shared Function DinPropunere(propunere As PrelucrarePropunere) As AsociereStare
        If propunere Is Nothing Then Throw New ArgumentNullException(NameOf(propunere))

        Dim stare As New AsociereStare() With {
            .CodAngajament = propunere.CodAngajament,
            .Amprenta = propunere.Amprenta
        }
        stare.Receptii.AddRange(propunere.Receptii)
        stare.Plati.AddRange(propunere.Plati)

        For Each p As InstantaneuPropus In propunere.Instantanee
            Dim legat As New InstantaneuLegat() With {
                .Idrh = p.Idrh,
                .RandIstoric = p.RandIstoric,
                .Idrr = p.SugestieIdrr,
                .Idh = p.Idh,
                .DataH = p.DataH,
                .Descriere = p.Descriere,
                .Total = p.Total,
                .Stergere = p.Stergere,
                .Ignorat = False,
                .Blocat = False
            }
            legat.Linii.AddRange(p.Linii)
            stare.Instantanee.Add(legat)
        Next

        ' `Blocat = True` fara exceptie, si nu e o parere a clientului: acoperirea ceruta de
        ' server e exact multimea de decis, iar o decizie pentru un rand din afara ei e
        ' respinsa cu 400. Corectarea unei legaturi vechi ramane treaba editorului de oricand.
        For Each c As InstantaneuLegat In propunere.InstantaneeAsezate
            Dim legat As New InstantaneuLegat() With {
                .Idrh = -c.Idrh,
                .Idrr = c.Idrr,
                .Idh = c.Idh,
                .DataH = c.DataH,
                .Descriere = c.Descriere,
                .Total = c.Total,
                .TipReceptie = c.TipReceptie,
                .Stergere = c.Stergere,
                .Ignorat = c.Ignorat,
                .Blocat = True
            }
            legat.Motive.AddRange(c.Motive)
            legat.Linii.AddRange(c.Linii)
            stare.Instantanee.Add(legat)
        Next
        Return stare
    End Function
End Class

''' <summary>
''' Un instantaneu asa cum sta acum in baza, cu legatura lui si cu starea de blocare.
''' POCO.
''' </summary>
Public NotInheritable Class InstantaneuLegat

    ''' <summary>
    ''' <c>FX_Receptii_H.IDRH</c>. In editorul de oricand e si ancora comenzilor. In modul
    ''' propunere e DOAR cheia dictionarelor formularului (id-ul dat in faza intai, care nu
    ''' supravietuieste derularii inapoi); ancora e atunci <see cref="RandIstoric"/> /
    ''' <see cref="Idh"/>. Vezi <see cref="AsociereStare.DinPropunere"/>.
    ''' </summary>
    Public Property Idrh As Integer

    ''' <summary>
    ''' Doar in modul propunere: indicele randului de istoric in <c>TabelIstoric</c> (F24),
    ''' sau Nothing cand randul nu e in aceasta descarcare — atunci ancora e <see cref="Idh"/>
    ''' (F34). Nothing si fara sens in editorul de oricand.
    ''' </summary>
    Public Property RandIstoric As Integer?

    ''' <summary>Recepția pe care sta acum. 0 = neasezat.</summary>
    Public Property Idrr As Integer

    ''' <summary><c>FX_Istoric.ID</c> din care a venit. 0 = necunoscut.</summary>
    Public Property Idh As Integer

    ''' <summary>Momentul editarii. ESTE axa timpului lantului (F2).</summary>
    Public Property DataH As Date

    Public Property Descriere As String = String.Empty

    ''' <summary>Valoarea INTREGII recepții la acel moment, nu marimea schimbarii (F3).</summary>
    Public Property Total As Double

    ''' <summary><c>Final</c> / <c>Partial</c>, recalculat de server per recepție.</summary>
    Public Property TipReceptie As String = String.Empty

    ''' <summary>Randul de stergere al lantului (F21). Nu are linii pe indicator.</summary>
    Public Property Stergere As Boolean

    ''' <summary>
    ''' Marcat de operator ca «nu consemneaza nicio schimbare» (F17) — adica
    ''' <c>FX_Receptii_H.Sters</c>. Nu se confunda cu <c>FX_Receptii_R.Sters</c>, care
    ''' spune ca recepția a fost stearsa pe site: alt tabel, alt fapt.
    ''' </summary>
    Public Property Ignorat As Boolean

    ''' <summary>
    ''' Legatura nu mai poate fi modificata, dar RAMANE VIZIBILA. Adevarat doar pentru un
    ''' instantaneu care are deja o recepție: unul neasezat nu are legatura, deci nu are
    ''' ce sa fie blocat.
    '''
    ''' <para>EXCEPTIE, in modul propunere: acolo TOT contextul e blocat, si randurile
    ''' neasezate care nu si-au gasit randul de istoric in descarcarea asta. Vezi
    ''' <see cref="AsociereStare.DinPropunere"/>.</para>
    ''' </summary>
    Public Property Blocat As Boolean

    ''' <summary>
    ''' De ce e blocat, in romana, gata de aratat. De la cel mai specific la cel mai
    ''' general: primul e cel care spune cel mai mult.
    ''' </summary>
    Public Property Motive As New List(Of String)

    Public Property Linii As New List(Of LinieInstantaneu)

    ''' <summary>Indicatorii pe care ii numeste instantaneul. Pentru vetourile F14/F16.</summary>
    Public Function Indicatori() As HashSet(Of String)
        Dim set_ As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each l As LinieInstantaneu In Linii
            If Not String.IsNullOrEmpty(l.CodIndicator) Then set_.Add(l.CodIndicator)
        Next
        Return set_
    End Function
End Class

''' <summary>O plata a angajamentului. POCO — context, nu subiect.</summary>
Public NotInheritable Class PlataAsociere
    Public Property DataPlata As Date
    Public Property Suma As Double
    Public Property NrOp As String = String.Empty
End Class

''' <summary>
''' O modificare ceruta de operator asupra UNEI legaturi. POCO.
''' </summary>
''' <remarks>
''' Lista de comenzi e PARTIALA prin definitie: aici tacerea inseamna «lasa legatura cum
''' e», ceea ce e un raspuns adevarat. In ingestie ar fi fost o alegere ascunsa, de-asta
''' acolo acoperirea e obligatorie si lipsa unei decizii e 400.
''' </remarks>
Public NotInheritable Class ComandaAsociere
    Public Property Idrh As Integer
    Public Property Actiune As ActiuneAsociere

    ''' <summary>Recepția existenta pe care se aseaza. 0 = niciuna.</summary>
    Public Property Idrr As Integer

    ''' <summary>Eticheta unei recepții reconstituite. Nothing = niciuna.</summary>
    Public Property ReceptieNoua As String
End Class

''' <summary>Ce a scris serverul, si ce a avut de semnalat. POCO.</summary>
Public NotInheritable Class AsociereRezultat
    Public Property CodAngajament As String = String.Empty

    ''' <summary>Amprenta NOUA, ca formularul sa continue fara sa reincarce tot.</summary>
    Public Property Amprenta As String = String.Empty

    Public Property Scrise As New Dictionary(Of String, Integer)

    ''' <summary>
    ''' Semnalari, nu erori. Aici ajunge F15 — «lanțul nu se închide» — care in editor
    ''' avertizeaza in loc sa refuze: desprinderea ultimului instantaneu lasa, prin
    ''' definitie, un lant deschis, iar un veto acolo ar face imposibil tocmai lucrul
    ''' pentru care exista editorul.
    ''' </summary>
    Public Property Avertismente As New List(Of String)
End Class
