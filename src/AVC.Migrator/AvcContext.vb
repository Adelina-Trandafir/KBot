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

    ''' <summary>IdUnitate ▸ the subunit the run will CREATE for it; filled by the subunits step's verification so the steps after it know the unit gets one.</summary>
    Public ReadOnly Property PlannedSubunits As New Dictionary(Of Integer, AvcSubunit)()

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
