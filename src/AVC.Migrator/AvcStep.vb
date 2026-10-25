Imports KBot.Migrator
Imports MySqlConnector

''' <summary>The finding kinds AVC.Migrator adds to the shared ones of <see cref="Finding"/>. ASCII tokens: protocol, not prose.</summary>
Friend NotInheritable Class AvcKinds
    Private Sub New()
    End Sub

    ''' <summary>The Access file has no table for the step.</summary>
    Public Const SURSA_LIPSA As String = "SURSA_LIPSA"
    ''' <summary>A step needs another step to have run first.</summary>
    Public Const PAS_DEPENDENT As String = "PAS_DEPENDENT"
    ''' <summary>Nothing is ticked.</summary>
    Public Const NICIUN_PAS As String = "NICIUN_PAS"
    ''' <summary>What the step will write - always one line.</summary>
    Public Const REZUMAT As String = "REZUMAT"
    ''' <summary>A unit has no subunit to put its chart in, and the subunits step is not ticked to create it.</summary>
    Public Const SUBUNITATE_LIPSA As String = "SUBUNITATE_LIPSA"
    ''' <summary>The subunits the grid describes would be refused (denomination, chapter, source, decimals, parent).</summary>
    Public Const SUBUNITATE_INVALIDA As String = "SUBUNITATE_INVALIDA"
    ''' <summary>Accounts of the Access chart the target would refuse, or that need a look.</summary>
    Public Const PLAN_CONT_INVALID As String = "PLAN_CONT_INVALID"
    ''' <summary>Balances per partner the target would refuse, or that need a look.</summary>
    Public Const SOLD_PARTENER_INVALID As String = "SOLD_PARTENER_INVALID"
    ''' <summary>The partner of a balance is not in <c>Parteneri</c> (KBot.Migrator has not moved it).</summary>
    Public Const PARTENER_LIPSA As String = "PARTENER_LIPSA"
    ''' <summary>The account of a balance is not in <c>TZ_PlanCont</c>.</summary>
    Public Const CONT_LIPSA As String = "CONT_LIPSA"
End Class

''' <summary>How many rows a step wrote into one table.</summary>
Friend NotInheritable Class AvcWritten
    Public Sub New(k_table As String, k_rows As Integer)
        Table = k_table
        Rows = k_rows
    End Sub

    Public ReadOnly Property Table As String
    Public ReadOnly Property Rows As Integer
End Class

''' <summary>
''' One thing AVC.Migrator moves from the Access file into the unit database: the chart of accounts, the journals, the
''' templates, ... Each step reads its own Access table(s), checks the target and writes its own <c>TZ_</c> tables; the
''' runner calls them in catalogue order inside ONE transaction.
''' </summary>
''' <remarks>
''' A step keeps what it read between <see cref="ReadAccess"/>, <see cref="Verify"/> and <see cref="Write"/>, so what was
''' reported is what is written. Adding a migration = one subclass + one line in <see cref="AvcCatalog"/>.
''' </remarks>
Friend MustInherit Class AvcStep

    ''' <summary>ASCII id, also the Access table name where there is one table (<c>PlanCont</c>).</summary>
    Public MustOverride ReadOnly Property Key As String

    ''' <summary>The stage of the porting plan, shown in the grid (<c>Etapa 1 - Configurare</c>).</summary>
    Public MustOverride ReadOnly Property Stage As String

    ''' <summary>What the operator reads in the grid.</summary>
    Public MustOverride ReadOnly Property Title As String

    ''' <summary>The Access table the step reads (for the grid).</summary>
    Public MustOverride ReadOnly Property AccessTable As String

    ''' <summary>The <c>TZ_</c> tables the step writes; they must exist in the unit database.</summary>
    Public MustOverride ReadOnly Property TargetTables As IReadOnlyList(Of String)

    ''' <summary>Keys of the steps whose data this one points into (they must have run before, or run in the same pass).</summary>
    Public Overridable ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return Array.Empty(Of String)()
        End Get
    End Property

    ''' <summary>Ticked in the grid.</summary>
    Public Property Selected As Boolean = True

    ''' <summary>Rows in the Access table, as read; -1 = not read yet.</summary>
    Public Property AccessRows As Integer = -1

    ''' <summary>Rows the step will write into its first target table.</summary>
    Public Property RowsToWrite As Integer = -1

    ''' <summary>Rows each target table has right now (-1 = the table does not exist), after a verification.</summary>
    Public ReadOnly Property ExistingRows As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Forgets what was read.</summary>
    Public Overridable Sub Reset()
        AccessRows = -1
        RowsToWrite = -1
        ExistingRows.Clear()
    End Sub

    ''' <summary>Reads the Access table(s) into memory and sets <see cref="AccessRows"/> / <see cref="RowsToWrite"/>. Access I/O: logs and rethrows.</summary>
    Public MustOverride Sub ReadAccess(k_context As AvcContext)

    ''' <summary>
    ''' Checks what <see cref="ReadAccess"/> read against the target (read-only). The runner has already checked that the
    ''' <see cref="TargetTables"/> exist. BLOCANT stops the run; ATENTIE is read first; INFORMATIV changes nothing.
    ''' </summary>
    Public MustOverride Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)

    ''' <summary>Writes inside the runner's transaction. Throws to roll everything back.</summary>
    Public MustOverride Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                       k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                       k_cancel As Threading.CancellationToken) As List(Of AvcWritten)

End Class
