Imports KBot.Migrator

''' <summary>
''' Everything one read, one verification or one run needs. Built by the form, consumed unchanged by <see cref="AvcRunner"/>
''' and by every <see cref="AvcStep"/>.
''' </summary>
''' <remarks>
''' Same model as KBot.Migrator: the registry <c>cale.accdb</c> names the DC (= the MariaDB database) and its units; the operator
''' ticks the units to move; each ticked unit has its own <c>baza&lt;year&gt;.accdb</c> next to the registry. The MariaDB
''' password lives in <see cref="TargetConnection"/> and is never logged or written into a journal.
''' </remarks>
Friend NotInheritable Class AvcContext

    ''' <summary>The only year moved (same decision as KBot.Migrator: the current year, plus opening balances).</summary>
    Public Const TransferYear As Integer = 2026

    Public Sub New(k_dc As String, k_units As IEnumerable(Of CaiUnit))
        If String.IsNullOrWhiteSpace(k_dc) Then Throw New ArgumentException("DC-ul lipsește.", NameOf(k_dc))
        Dc = k_dc.Trim()
        Units = New List(Of CaiUnit)(k_units)
    End Sub

    ''' <summary>The DC, i.e. the unit database name, verbatim from <c>cai.DC</c>. This is where everything is written.</summary>
    Public ReadOnly Property Dc As String

    ''' <summary>The units the operator ticked (never "all of the DC").</summary>
    Public ReadOnly Property Units As List(Of CaiUnit)

    ''' <summary>The target database - the DC.</summary>
    Public ReadOnly Property TargetDatabase As String
        Get
            Return Dc
        End Get
    End Property

    ''' <summary>The MariaDB server; Nothing while only the Access files are being read.</summary>
    Public Property Server As TargetServer

    ''' <summary>Typed on the form when the unit files are protected; empty otherwise. Never persisted.</summary>
    Public Property AccessPassword As String = String.Empty

    ''' <summary>The database with the shared nomenclators (<c>DefaCont</c>, <c>DefaClsfF</c>, ...).</summary>
    Public Property CommonDatabase As String = "AVACONT_COMUN"

    ''' <summary>The CodFiscal of the unit as typed on the form (proposed from the Windows registry); goes into the journal header. Never persisted.</summary>
    Public Property CodFiscal As String = String.Empty

    ''' <summary>Every unit of the DC (ticked or not): the subunits step looks up the parents of the ticked ones here.</summary>
    Public Property DcUnits As IReadOnlyList(Of CaiUnit) = New List(Of CaiUnit)()

    ''' <summary>
    ''' One draft per ticked unit, with the denomination, chapter and salary decimals as the grid shows them (set by the form before every
    ''' verification and run). The subunits step writes the ones the server does not hold yet.
    ''' </summary>
    Public Property SubunitDrafts As IReadOnlyList(Of AvcSubunit) = New List(Of AvcSubunit)()

    ''' <summary>Set by <see cref="AvcRunner"/> for the step being written: (done, total, what) -> the console. Nothing = nobody is watching.</summary>
    Friend Property OnTick As Action(Of Integer, Integer, String)

    Private ReadOnly _tickClock As Stopwatch = Stopwatch.StartNew()
    Private _lastTickMs As Long = -1000

    ''' <summary>
    ''' A step calls this inside its row loop ("<paramref name="k_done"/> of <paramref name="k_total"/> <paramref name="k_what"/>"). Costs one
    ''' clock read per call and passes on at most five times a second (the last one of a table always), so it does not slow the write.
    ''' </summary>
    Public Sub Tick(k_done As Integer, k_total As Integer, k_what As String)
        Dim k_callback = OnTick
        If k_callback Is Nothing Then Return
        Dim k_now = _tickClock.ElapsedMilliseconds
        If k_done < k_total AndAlso k_now - _lastTickMs < 200 Then Return
        _lastTickMs = k_now
        k_callback(k_done, k_total, k_what)
    End Sub

    ''' <summary>
    ''' How the operator recognises a unit in a message or in the report: the <c>AlteDetalii</c> of <c>cale.accdb</c>, then the registry name,
    ''' then the bare id (an id such as 75 tells the operator nothing). Looks in the ticked units first, then in every unit of the DC.
    ''' </summary>
    Public Function UnitLabel(k_idUnitate As Integer) As String
        Dim k_unit = Units.FirstOrDefault(Function(k_u) k_u.IdUnitate = k_idUnitate)
        If k_unit Is Nothing Then k_unit = DcUnits.FirstOrDefault(Function(k_u) k_u.IdUnitate = k_idUnitate)
        Return LabelOf(k_unit, k_idUnitate)
    End Function

    ''' <summary>The label of <paramref name="k_unit"/>; <paramref name="k_idUnitate"/> stands in when the unit is not known.</summary>
    Public Shared Function LabelOf(k_unit As CaiUnit, k_idUnitate As Integer) As String
        If k_unit Is Nothing Then Return k_idUnitate.ToString(Globalization.CultureInfo.InvariantCulture)
        If Not String.IsNullOrWhiteSpace(k_unit.AlteDetalii) Then Return k_unit.AlteDetalii.Trim()
        If Not String.IsNullOrWhiteSpace(k_unit.NumeUnitate) Then Return k_unit.NumeUnitate.Trim()
        Return k_unit.IdUnitate.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

    ''' <summary>IdUnitate ▸ the subunit the run will CREATE for it; filled by the subunits step's verification so the steps after it know the unit gets one.</summary>
    Public ReadOnly Property PlannedSubunits As New Dictionary(Of Integer, AvcSubunit)()

    ''' <summary>The account symbols and journal keys contributed by the verified steps of this run.</summary>
    Public ReadOnly Property PlannedAccounts As New HashSet(Of (IdUnitate As Integer, Simbol As String))()
    Public ReadOnly Property PlannedJournals As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>The year the opening balances and turnovers belong to (the <c>An</c> of the <c>TZ_</c> rows).</summary>
    Public ReadOnly Property Year As Integer
        Get
            Return TransferYear
        End Get
    End Property

    ''' <summary>SHA-256 of each ticked unit file as it was read, by path; the run refuses to start when one changed since.</summary>
    Public ReadOnly Property FileHashes As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Where the SQL journal folders are written.</summary>
    Public Property JournalRoot As String = String.Empty

End Class
