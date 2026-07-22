' POCO-urile vederii Receptii (felia 0015) — echivalentul lui frmFX_MAIN_REC.
'
' Sursa: FX_Receptii_R -> FX_Receptii_H -> FX_Receptii, printr-un cititor brut
' (GET /api/forexe/receptii). Serverul NU pre-formeaza arborele: intoarce un rand per
' linie FX_Receptii (cu antetul H si receptia R purtate pe rand) plus lista de plati a
' angajamentului. Clientul (ReceptiiView) deriva:
'   * arborele pe 2 niveluri: receptie (IDRR) -> antet (IDRH);
'   * grila LISTA (per antet selectat): un rand-total sintetic + un rand per clsf;
'   * (felia 0015-02) tooltip-ul de receptie, din DIFH-uri necumulate + plati.
' Modele pure (fara logica de I/O) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' O linie de receptie = o inregistrare FX_Receptii (grain IDR), cu parintele antet (H)
''' si parintele receptie (R). Un antet FARA linii vine tot (LEFT JOIN pe server), caz in
''' care <see cref="Idr"/> este Nothing si campurile de linie sunt neutre.
''' Coloanele de bani vin deja 0-ate de server (niciodata null), deci sunt Double simplu.
''' </summary>
Public NotInheritable Class ReceptieRow
    ' --- receptia (R) ---
    ''' <summary>Cheia primara FX_Receptii_R — identitatea receptiei (radacina arborelui).</summary>
    Public Property Idrr As Integer
    ''' <summary>NRCRT al receptiei — cheia principala de ordonare a radacinilor.</summary>
    Public Property NrCrtR As Integer?
    Public Property DataR As Date?
    Public Property SumaAntet As Double
    ''' <summary>Receptie incarcata -> iconita „sus". Vezi si <see cref="Preluat"/>.</summary>
    Public Property Incarcat As Boolean
    ''' <summary>Receptie preluata -> iconita „jos" (daca nu e si Incarcat).</summary>
    Public Property Preluat As Boolean

    ' --- antetul (H) ---
    ''' <summary>Cheia primara FX_Receptii_H — identitatea antetului (nodul arborelui).</summary>
    Public Property Idrh As Integer
    Public Property NrCrtH As Integer?
    Public Property DataH As Date?
    Public Property Total As Double
    ''' <summary>Diferenta antetului (per H). Folosita la cumulul „Recepții cumulate" din
    ''' tooltip-ul de receptie (felia 0015-02); se insumeaza distinct per antet.</summary>
    Public Property Difh As Double
    ''' <summary>Antet sters. Arborele il arata oricum (qFX_MAIN_REC_TREE nu filtreaza
    ''' Sters), dar cumulul DIFH din tooltip il EXCLUDE (qFX_MAIN_REC_TT_DIFH: Sters=False).</summary>
    Public Property StersH As Boolean
    ''' <summary>Descrierea antetului. Coloana „Descriere" a grilei o afiseaza pe randurile
    ''' per-clsf (randul-total arata „Toți indicatorii"). Poate fi goala.</summary>
    Public Property DescriereH As String = String.Empty

    ' --- linia (FX_Receptii) ---
    ''' <summary>Cheia primara FX_Receptii — Nothing daca antetul nu are linii.</summary>
    Public Property Idr As Integer?
    Public Property IdClsf As Integer
    Public Property CodIndicator As String = String.Empty
    ''' <summary>Codul de clasificatie punctat. Poate fi gol: o linie al carei indicator
    ''' nu are clasificatie ramane in lista (LEFT JOIN pe server).</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Denumirea clasificatiei (Clasificatii.Denumire). Coloana „Descriere" a
    ''' grilei o afiseaza pe randurile per-clsf — bine definita la orice nivel de agregare
    ''' (luna / receptie / antet), spre deosebire de <see cref="DescriereH"/> (antetul).</summary>
    Public Property Denumire As String = String.Empty
    ''' <summary>NrCrt-ul indicatorului (FX_Indicatori.NrCrt). Coloana „NrCrt" a grilei.</summary>
    Public Property NrCrtInd As Integer?
    Public Property Valoare As Double
    ''' <summary>Diferenta liniei (per IDR). Randul-total al grilei = Sum(DIF) pe antet.</summary>
    Public Property Dif As Double
End Class

''' <summary>O plata a angajamentului (qFX_MAIN_REC_TT_PLATI). Folosita numai de
''' tooltip-ul de receptie (felia 0015-02): platile cumulate pana la MaxDataH.</summary>
Public NotInheritable Class ReceptiePlata
    Public Property DataPlata As Date?
    Public Property Suma As Double
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/receptii: lista de linii de receptie
''' (poate fi goala — un angajament fara receptii e legitim, raspuns 200, nu 404) plus
''' lista de plati (pentru tooltip). <see cref="Cod"/> = angajamentul cerut.
''' </summary>
Public NotInheritable Class ReceptiiInfo
    Public Property Cod As String = String.Empty
    Public Property Receptii As New List(Of ReceptieRow)()
    Public Property Plati As New List(Of ReceptiePlata)()
End Class
