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
End Class

''' <summary>
''' Un instantaneu asa cum sta acum in baza, cu legatura lui si cu starea de blocare.
''' POCO.
''' </summary>
Public NotInheritable Class InstantaneuLegat

    ''' <summary><c>FX_Receptii_H.IDRH</c>. Ancora, aici — nu un indice de rand.</summary>
    Public Property Idrh As Integer

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
