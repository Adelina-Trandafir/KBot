Option Strict On
Imports System.Collections.Generic

''' <summary>
''' Cum s-a terminat un <c>POST /api/forexe/prelucrare</c>.
''' </summary>
Public Enum PrelucrareStare
    ''' <summary>Serverul a scris și a confirmat (200).</summary>
    Salvat = 0
    ''' <summary>
    ''' Serverul a găsit cel puțin o clasificație care se potrivește cu mai multe unități
    ''' și NU a scris nimic (409, <c>reason = ALEGERE_UNITATE</c>). Operatorul trebuie
    ''' întrebat, iar aceeași sarcină se retrimite cu alegerile atașate.
    ''' </summary>
    AlegereUnitate = 1
    ''' <summary>
    ''' Faza UNU a ingestiei (felia 0048-03): serverul a construit tabloul, l-a derulat
    ''' înapoi și l-a întors. NU s-a scris nimic. <see cref="PrelucrareRaspuns.Propunere"/>
    ''' poartă recepțiile, instantaneele neașezate și amprenta.
    ''' </summary>
    Propunere = 2
End Enum

''' <summary>
''' Răspunsul rutei de ingestie, în amândouă formele lui. POCO — fără logică.
''' </summary>
''' <remarks>
''' Un singur tip pentru 200 și pentru 409, fiindcă apelantul trebuie oricum să distingă
''' între ele: <see cref="Stare"/> spune care este, iar câmpurile celeilalte forme rămân
''' goale (niciodată Nothing — listele se inițializează, ca apelantul să nu le păzească).
''' </remarks>
Public NotInheritable Class PrelucrareRaspuns

    ''' <summary>
    ''' Codul-motiv pe care serverul îl pune în câmpul «reason» al răspunsului de 409.
    ''' Stabil, ca și codurile de la 401 (TOKEN_UNKNOWN, CONTEXT_MISMATCH…).
    ''' </summary>
    Public Const MotivAlegereUnitate As String = "ALEGERE_UNITATE"

    Public Property Stare As PrelucrareStare = PrelucrareStare.Salvat
    Public Property CodAngajament As String = String.Empty

    ''' <summary>Mesajul românesc al serverului (câmpul «error» la 409).</summary>
    Public Property Mesaj As String = String.Empty

    ''' <summary>Steagul «are indicatori» — portul lui <c>FX_Angajament_Are</c>.</summary>
    Public Property AreIndicatori As Boolean

    ''' <summary>Câte rânduri s-au scris, per tabel.</summary>
    Public Property Scrise As New Dictionary(Of String, Integer)

    ''' <summary>Avertismente pentru operator (română, diacritice literale).</summary>
    Public Property Avertismente As New List(Of String)

    ''' <summary>
    ''' Întrebările la care operatorul trebuie să răspundă. Goală când
    ''' <see cref="Stare"/> este <see cref="PrelucrareStare.Salvat"/>.
    ''' </summary>
    Public Property AlegeriNecesare As New List(Of AlegereNecesara)

    ''' <summary>
    ''' Tabloul propus (felia 0048-03). Completat DOAR când <see cref="Stare"/> este
    ''' <see cref="PrelucrareStare.Propunere"/>; Nothing altfel.
    ''' </summary>
    Public Property Propunere As PrelucrarePropunere
End Class

''' <summary>
''' O clasificație care se potrivește cu mai multe unități, cu tot ce trebuie ca
''' operatorul să poată alege în cunoștință de cauză. POCO.
''' </summary>
Public NotInheritable Class AlegereNecesara
    ''' <summary>Sector + Sursă, ex. «02E».</summary>
    Public Property Ss As String = String.Empty
    ''' <summary>Articol + Alineat fără puncte, ex. «200101» — cheia pe care se întreabă.</summary>
    Public Property ClsfE As String = String.Empty
    ''' <summary>Clasificația așa cum a venit din FOREXE, ex. «02E- 65. 04. 02. 20. 01. 01».</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Primul indicator care a lovit ambiguitatea (pentru titlu).</summary>
    Public Property CodIndicator As String = String.Empty
    ''' <summary>TOȚI indicatorii angajamentului care folosesc aceeași pereche.</summary>
    Public Property Indicatori As New List(Of String)
    ''' <summary>Unitățile posibile, în ordinea în care le-a trimis serverul.</summary>
    Public Property Unitati As New List(Of UnitateCandidat)
End Class

''' <summary>O unitate posibilă. POCO.</summary>
Public NotInheritable Class UnitateCandidat
    Public Property IdUnitate As Integer
    ''' <summary>Numele citibil (<c>Unitati.Detalii</c>) — de-asta se întreabă cu nume, nu cu numere.</summary>
    Public Property Detalii As String = String.Empty
    Public Property SursaSector As String = String.Empty
    Public Property CodProgram As String = String.Empty
End Class

''' <summary>
''' Răspunsul operatorului la o <see cref="AlegereNecesara"/>. POCO.
''' </summary>
Public NotInheritable Class AlegereUnitate
    Public Property Ss As String = String.Empty
    Public Property ClsfE As String = String.Empty
    Public Property IdUnitate As Integer
    ''' <summary>
    ''' Bifa «Nu mă mai întreba pentru această combinație». True o trimite serverului spre
    ''' <c>FX_Alegeri_Unitate</c>, de unde va răspunde singur data viitoare. O combinație
    ''' NOUĂ se întreabă oricum din nou — memoria e per pereche (SS, ClsfE), nu globală.
    ''' </summary>
    Public Property Retine As Boolean
End Class
