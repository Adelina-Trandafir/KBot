' POCO-urile vederii Plăți (felia 0017) — echivalentul lui frmFX_MAIN_PLATI.
'
' Sursa: FX_Plati, printr-un cititor brut (GET /api/forexe/plati). Serverul NU pre-formeaza
' arborele: intoarce un rand per inregistrare FX_Plati, cu extrasul bancar (FX_Extrase)
' purtat pe rand (1:1 pe Referinta_TREZOR — afirmatie de operator) plus flag-ul are_ord.
' Clientul (PlatiView) deriva:
'   * arborele pe 3 niveluri: luna -> zi (frunza, TOATE randurile zilei intr-un nod) ->
'     plata (IdPlataFX);
'   * grila LISTA FILTRATA (nu agregata — spre deosebire de Recepții): randurile nodului;
'   * panoul de detaliu = extrasul bancar, deja pe rand (fara un al doilea apel de retea).
' Modele pure (fara logica de I/O) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' Extrasul bancar asociat unei plati (FX_Extrase, LEFT JOIN pe server). Nothing pe
''' <see cref="PlataRow.Extras"/> cand plata nu are extras (panoul arata starea goala).
''' Coloanele de bani vin deja 0-ate de server (niciodata null), deci sunt Double simplu.
''' </summary>
Public NotInheritable Class ExtrasBancar
    ''' <summary>Cheia primara FX_Extrase — prezenta ei = „plata are extras".</summary>
    Public Property Idfxe As Integer
    Public Property DataBanca As Date?
    ''' <summary>Data documentului. E TEXT in FX_Extrase (nu data), deci ramane string brut.</summary>
    Public Property DataDoc As String = String.Empty
    Public Property NrDoc As String = String.Empty
    Public Property Referinta As String = String.Empty
    Public Property PlatitorNume As String = String.Empty
    Public Property PlatitorCui As String = String.Empty
    Public Property PlatitorIban As String = String.Empty
    Public Property SumaDebit As Double
    Public Property SumaCredit As Double
    Public Property Explicatii As String = String.Empty
End Class

''' <summary>
''' O plata = o inregistrare FX_Plati a angajamentului. Coloanele de bani vin deja 0-ate de
''' server. <see cref="Extras"/> este Nothing cand plata nu are extras bancar asociat.
''' </summary>
Public NotInheritable Class PlataRow
    ''' <summary>Cheia primara FX_Plati — identitatea platii (nodul de nivel 2 al arborelui).</summary>
    Public Property IdPlataFX As Integer
    ''' <summary>Id-ul de clasificatie (Access) al platii — nefolosit de vedere, purtat brut.</summary>
    Public Property IdClsf As Integer
    Public Property CodAI As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    ''' <summary>Numarul OP — captionul nodului de plata (fallback pe <see cref="ReferintaTrezor"/>).</summary>
    Public Property NrOP As String = String.Empty
    ''' <summary>Ziua platii (fara ora). Baza gruparii pe luna (folder) si pe zi (frunza).</summary>
    Public Property DataPlata As Date?
    Public Property Suma As Double
    ''' <summary>„PLATA" / „INCASARE". INCASARE -> nod verde (case-insensitive).</summary>
    Public Property Tip As String = String.Empty
    ''' <summary>Plata incarcata -> iconita „sus". Vezi si <see cref="Preluat"/>.</summary>
    Public Property Incarcat As Boolean
    ''' <summary>Plata preluata -> iconita „jos" (daca nu e si Incarcat).</summary>
    Public Property Preluat As Boolean
    Public Property ReferintaTrezor As String = String.Empty
    ''' <summary>Clasificatia din nomenclator (Clasificatii.Clsf). Poate fi goala — atunci
    ''' vederea cade pe <see cref="ClsfPlata"/>.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Denumirea clasificatiei (Clasificatii.Denumire). Poate fi goala.</summary>
    Public Property Denumire As String = String.Empty
    ''' <summary>Clasificatia denormalizata (coloana bruta FX_Plati.Clsf) — fallback cand
    ''' nomenclatorul (<see cref="Clsf"/>) nu a intors nimic, ca o plata sa nu ramana fara
    ''' clasificatie afisata.</summary>
    Public Property ClsfPlata As String = String.Empty
    ''' <summary>Are deja o ordonantare (FX_ORD_TBL_REC). «+» apare doar pe cea mai veche zi
    ''' cu cel putin o plata avand AreOrd = False.</summary>
    Public Property AreOrd As Boolean
    ''' <summary>Extrasul bancar (FX_Extrase), sau Nothing cand plata nu are extras.</summary>
    Public Property Extras As ExtrasBancar

    ''' <summary>Clasificatia efectiva de afisat: nomenclatorul daca exista, altfel bruta.</summary>
    Public ReadOnly Property ClsfEfectiv As String
        Get
            Return If(String.IsNullOrEmpty(Clsf), ClsfPlata, Clsf)
        End Get
    End Property

    ''' <summary>Captionul nodului de plata: NrOP, sau ReferintaTrezor cand NrOP e gol.</summary>
    Public ReadOnly Property EtichetaPlata As String
        Get
            Return If(String.IsNullOrEmpty(NrOP), ReferintaTrezor, NrOP)
        End Get
    End Property

    ''' <summary>Plata e INCASARE (case-insensitive)? -> foreground verde.</summary>
    Public ReadOnly Property EsteIncasare As Boolean
        Get
            Return String.Equals(Tip, "INCASARE", StringComparison.OrdinalIgnoreCase)
        End Get
    End Property
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/plati: lista de plati (poate fi goala — un
''' angajament fara plati e legitim, raspuns 200, nu 404). <see cref="Cod"/> = angajamentul cerut.
''' </summary>
Public NotInheritable Class PlatiInfo
    Public Property Cod As String = String.Empty
    Public Property Plati As New List(Of PlataRow)()
End Class
