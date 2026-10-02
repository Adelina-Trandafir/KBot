' Contextul unui nod de angajament din arborele MainForm — înlocuitorul tipizat al
' celor ~15 textbox-uri ascunse din Access frmFX_MAIN (CodAngajament, Stare, TIP_NOD,
' Are*, IDDF...). Un obiect = un rând întors de GET /api/forexe/tree + starea de
' navigare a nodului.
'
' CONTRACT (felia 0008): endpoint-ul NU oglindește o singură query Access, ci compune
' două, exact cum cere PLAN_TreeDataApi.md:
'   - „row-source” = qFX_MAIN_TREE_DESCRIERE (populează NODURILE arborelui, prin
'     mdl_FX_PopulareTree.Angajamente_SQL) — de aici vin coloanele de afișare;
'   - „flags”      = qFX_MAIN_TREE (sursa rcAngInd în frmFX_MAIN.RefreshTreeQuery)
'     — de aici vin cele nouă Are*.
' Ambele citite verbatim din FX_System_Export/QUERIES/ la 2026-07-15.
'
' Proveniența, câmp cu câmp:
'   CodAngajament     qFX_MAIN_TREE_DESCRIERE (FA.CodAngajament); coloană FX_Angajamente
'   IDDF              qFX_MAIN_TREE_DESCRIERE (FA.IDDF) — coloană PE FX_Angajamente,
'                     deci FĂRĂ join spre FX_DDF (qFX_MAIN_TREE o lua prin join)
'   Descriere         qFX_MAIN_TREE_DESCRIERE (FA.Descriere); coloană FX_Angajamente
'   Stare             qFX_MAIN_TREE_DESCRIERE (FA.Stare); coloană FX_Angajamente
'   DataCreare        qFX_MAIN_TREE_DESCRIERE (FA.DataCreare); coloană FX_Angajamente
'   DataDefinitivare  qFX_MAIN_TREE_DESCRIERE (FA.DataDefinitivare); coloană FX_Angajamente
'   EIncarcat         qFX_MAIN_TREE_DESCRIERE (FA.Incarcat); coloană FX_Angajamente
'   EPreluat          qFX_MAIN_TREE_DESCRIERE (FA.Preluat); coloană FX_Angajamente
'   Salarii           qFX_MAIN_TREE_DESCRIERE (FA.Salarii); coloană FX_Angajamente
'                     (Boolean — confirmat în FX_System_Export/TABLES/FX_Angajamente.md)
'   ASCUNS            coloană FX_Angajamente (confirmată în export ȘI în DDL-ul MariaDB
'                     DDL_FX_ListaAngajamente.sql:42); în Access apare selectată în
'                     qFX_MAIN_TREE_DATA, nu în _DESCRIERE
'   Surse             lista SS concatenată, DOAR pentru afișare: Access ConcatRelated(
'                     'SS','FX_Indicatori',...) -> MariaDB GROUP_CONCAT(DISTINCT i.SS)
'
' Cele nouă flag-uri (toate din qFX_MAIN_TREE; Access COUNT(*)>0/CBool/Nz -> EXISTS):
'   AreIndicatori     EXISTS FX_Indicatori
'   AreIstoric        EXISTS FX_Istoric
'   AreRevizii        EXISTS FX_DDF_REV_SA
'   AreRezervari      EXISTS FX_Rezervari
'   AreReceptii       EXISTS FX_Receptii_H
'   ArePlati          EXISTS FX_Plati
'   AreDDF            IDDF IS NOT NULL (fără subquery — IDDF e deja pe rând)
'   AreORD            EXISTS FX_ORD prin DDF-ul angajamentului (FX_ORD.IDDF = FX_DDF.IDDF)
'   ArePartener       FX_DDF.PartAng = 1 (LEFT JOIN; fals/NULL când nu există DDF)
'
' ArePartener: Access scria «Not IsNull([CODPARTENER])». Decizia feliei 0008 e PartAng,
' coloană reală pe FX_DDF (export: FX_DDF.md). Cele două sunt aproape echivalente —
' frmFX_DDF activează CodPartener DOAR când PartAng e bifat (frmFX_DDF.md:453, 580) și
' validarea cere partener obligatoriu când PartAng (frmFX_DDF.md:701) — dar PartAng e
' sursa curată (da/nu), nu un efect secundar. CARE partener (IdPartener din Parteneri,
' pe CodFiscal) e treaba vederii Partener, nu a arborelui.
'
' IDORD a fost SCOS intenționat: qFX_MAIN_TREE îl lua cu First(FX_ORD.IDORD) — o alegere
' arbitrară sub GROUP BY când un IDDF are mai multe ORD-uri. Arborele nu are nevoie de un
' IDORD anume, doar de AreORD; odată scos, dispare și problema alegerii arbitrare.
'
' NU vin din query:
'   NodeKey / ParentKey / Caption — asamblarea arborelui, ale noastre.
'   TipNod / CodIndicator / CodAi / IdPartener — în Access le seta handler-ul de click
'   pe arbore (mcTree_Click), după nivelul/tipul nodului.
Public NotInheritable Class AngajamentTreeInfo

    ' --- asamblarea arborelui ---
    Public Property NodeKey As String = String.Empty     ' cheia unică a nodului în arbore
    Public Property ParentKey As String = String.Empty   ' vid = nod rădăcină
    Public Property Caption As String = String.Empty     ' textul afișat pe nod

    ' --- identitate + rândul întors de /api/forexe/tree ---
    Public Property CodAngajament As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty
    Public Property DataCreare As Date?
    ' Slice 0777: when FOREXE recorded the angajament as made -- the DataFX (DATETIME, time
    ' included) of its FX_Istoric row «Angajament nou.». The date sort orders on this.
    Public Property DataAngajamentNou As Date?
    ' Slice 0100: when the last FOREXE download of this angajament was SAVED on the server
    ' (FX_Angajamente.DataActualizare). Nothing = none since the column exists.
    Public Property DataActualizare As Date?

    ''' <summary>
    ''' Slice 0100: this angajament was downloaded before (it has history or indicators) and its
    ''' last saved download is older than <paramref name="zile"/> days at <paramref name="acum"/> --
    ''' or has no date at all (downloaded before the date existed). A header-only angajament (never
    ''' downloaded) is NOT «old»: there is nothing local to bring up to date. Used by the
    ''' on-connect update and the selection window.
    ''' </summary>
    Public Function EsteNeactualizatDe(zile As Integer, acum As Date) As Boolean
        If Not (AreIstoric OrElse AreIndicatori) Then Return False
        If Not DataActualizare.HasValue Then Return True
        Return DataActualizare.Value < acum.AddDays(-zile)
    End Function

    Public Property DataDefinitivare As Date?
    Public Property IDDF As Long?
    Public Property EIncarcat As Boolean                 ' query: Incarcat
    Public Property EPreluat As Boolean                  ' query: Preluat
    Public Property Salarii As Boolean
    Public Property Ascuns As Boolean                    ' query: ASCUNS
    Public Property Surse As String = String.Empty       ' lista SS, doar afișare

    ' --- flag-urile de vizibilitate (fiecare poartă exact o vedere) ---
    Public Property AreIndicatori As Boolean
    Public Property AreIstoric As Boolean
    Public Property AreRevizii As Boolean
    Public Property AreRezervari As Boolean
    Public Property ArePartener As Boolean
    Public Property AreORD As Boolean                    ' query: AreOrd
    Public Property AreDDF As Boolean
    Public Property AreReceptii As Boolean
    Public Property ArePlati As Boolean
    ' Slice 0080-02: FX_Extrase carries this angajament in CodContract.
    Public Property AreExtrase As Boolean
    ' Slice 0097: a CAB correction note (FX_NoteCAB_Corectii) corrects an operation on it.
    Public Property AreNoteCab As Boolean

    ' Slice 0101: the receptions of this angajament whose chain does not close (the last snapshot
    ' is not a deletion row and its total differs from the reception's value). Empty = all close.
    Public Property LantNeinchis As IReadOnlyList(Of ReceptieNeinchisa) = Array.Empty(Of ReceptieNeinchisa)()

    ' --- stare de navigare (în Access: setate de mcTree_Click, după nivelul nodului) ---
    Public Property TipNod As String = String.Empty      ' "D" / "R" / "P" / ... (TIP_NOD)
    Public Property CodIndicator As String = String.Empty
    Public Property CodAi As String = String.Empty
    Public Property IdPartener As Long?

End Class
