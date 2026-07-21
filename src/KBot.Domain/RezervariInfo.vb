' POCO-urile vederii Rezervari (felia 0014) — echivalentul lui frmFX_MAIN_REZ.
'
' Sursa: FX_Rezervari, printr-un cititor brut (GET /api/forexe/rezervari). Serverul
' NU pre-formeaza arborele: intoarce un rand per inregistrare, iar clientul
' (RezervariView) le grupeaza pe luni (folder) si pe (data, tip) (frunza) plus umple
' grila master/detail. Modele pure (fara logica de I/O) -> nu poarta Try/Catch
' (regula casei: POCO-uri simple).

''' <summary>
''' Tipul operatiei de rezervare. Cele trei flag-uri EInitiala/EMarire/EMicsorare sunt
''' mutual exclusive pe un rand; <see cref="RezervareRow.Tip"/> le reduce la enum.
''' </summary>
Public Enum RezervareTip
    ''' <summary>Niciun flag setat — nu ar trebui sa apara pe date valide.</summary>
    Necunoscut = 0
    Initiala = 1
    Marire = 2
    Micsorare = 3
End Enum

''' <summary>
''' Un rand de rezervare = o inregistrare FX_Rezervari. Coloanele de bani vin deja
''' 0-ate de server (niciodata null), deci sunt Double simplu, nu Double?.
''' </summary>
Public NotInheritable Class RezervareRow
    ''' <summary>Cheia primara FX_Rezervari — identitatea randului (selectie / DDF viitor).</summary>
    Public Property Idrz As Integer
    Public Property CodIndicator As String = String.Empty
    ''' <summary>Codul de clasificatie punctat. Poate fi gol: o rezervare al carei
    ''' indicator nu are clasificatie ramane in lista (LEFT JOIN pe server).</summary>
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>Ziua rezervarii (fara ora). Folosita si la gruparea pe luna/an
    ''' (folder), si la gruparea pe (data, tip) (frunza).</summary>
    Public Property DataRezervare As Date
    Public Property RCreditBug As Double
    Public Property RInitiala As Double
    Public Property RValoare As Double
    Public Property RDefinitiva As Double
    Public Property EInitiala As Boolean
    Public Property EMarire As Boolean
    Public Property EMicsorare As Boolean
    ''' <summary>Are deja un DDF asociat. Frunza arata „+" doar cand grupul are
    ''' cel putin un rand cu AreDDF = False.</summary>
    Public Property AreDDF As Boolean

    ''' <summary>
    ''' Tipul derivat din cele trei flag-uri, in ordinea Access
    ''' (Initiala &gt; Marire &gt; Micsorare). Client-side (planul felia 0014).
    ''' </summary>
    Public ReadOnly Property Tip As RezervareTip
        Get
            If EInitiala Then Return RezervareTip.Initiala
            If EMarire Then Return RezervareTip.Marire
            If EMicsorare Then Return RezervareTip.Micsorare
            Return RezervareTip.Necunoscut
        End Get
    End Property

    ''' <summary>
    ''' Valoarea afisata a randului in frunza = R_Initiala pentru operatia initiala,
    ''' altfel R_Valoare. Oglindeste Suma din QFX_DDF_REZERVARI
    ''' (IIf(EInitiala, R_Initiala, R_Valoare)); frunza insumeaza aceasta valoare peste
    ''' grupul (data, tip).
    ''' </summary>
    Public ReadOnly Property ValoareOperatie As Double
        Get
            Return If(EInitiala, RInitiala, RValoare)
        End Get
    End Property
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/rezervari: lista de randuri (poate fi goala
''' — un angajament fara rezervari e legitim, raspuns 200, nu 404).
''' </summary>
Public NotInheritable Class RezervariInfo
    Public Property Rows As New List(Of RezervareRow)()
End Class
