''' <summary>
''' Everything one verification or one run needs. Built by the form, consumed unchanged by
''' <see cref="Verifier"/> and <see cref="TransferRunner"/>.
''' </summary>
''' <remarks>
''' The two Access passwords live here for the duration of a run only: never persisted,
''' never logged, never written into a dump file. The MariaDB password lives in
''' <see cref="TargetConnection"/> under the same rule.
''' </remarks>
Public NotInheritable Class TransferRequest

    Public Sub New(server As TargetServer, targetDatabase As String)
        If server Is Nothing Then Throw New ArgumentNullException(NameOf(server))
        If String.IsNullOrWhiteSpace(targetDatabase) Then
            Throw New ArgumentException("Numele bazei-țintă lipsește.", NameOf(targetDatabase))
        End If
        Me.Server = server
        Me.TargetDatabase = targetDatabase
        Units = New List(Of CaiUnit)()
        SelectedTables = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    End Sub

    Public ReadOnly Property Server As TargetServer
    ''' <summary>The DC, i.e. the MariaDB database name, verbatim from <c>cai.DC</c>.</summary>
    Public ReadOnly Property TargetDatabase As String

    ''' <summary>The units the operator chose. Decision D2 - never "all of the DC".</summary>
    Public ReadOnly Property Units As List(Of CaiUnit)

    ''' <summary>Target table names ticked for transfer. Empty means nothing to do.</summary>
    Public ReadOnly Property SelectedTables As HashSet(Of String)

    ''' <summary>Password for the unit files. Empty when they are not protected.</summary>
    Public Property UnitFilePassword As String = String.Empty

    ''' <summary>Password for the Forexe files. Empty when they are not protected.</summary>
    Public Property ForexeFilePassword As String = String.Empty

    ''' <summary>
    ''' The database holding the shared dictionaries the Clasificatii foreign keys point
    ''' at. Not created by the template copy - it must already exist on the server.
    ''' </summary>
    Public Property CommonDatabase As String = "AVACONT_COMUN"

    ''' <summary>The database a missing target is created from (plan §4).</summary>
    Public Property TemplateDatabase As String = "AVACONT_SURSA"

    ''' <summary>Where the SQL journal folder is written.</summary>
    Public Property JournalFolder As String = String.Empty

    ''' <summary>The operator's name, for the journal header. Never a credential.</summary>
    Public Property OperatorName As String = String.Empty

    ''' <summary>True when the tool should populate <c>Unitati</c> from the registry.</summary>
    ''' <remarks>
    ''' Operator decision, 23.08: the tool does it. Decision D7 makes it unavoidable
    ''' anyway - a database created from the template has structure but no rows, so
    ''' Unitati is empty and four foreign keys point into it.
    ''' </remarks>
    Public Property PopulateUnitati As Boolean = True

    ''' <summary>True when a missing target database may be created from the template.</summary>
    Public Property CreateDatabaseIfMissing As Boolean = True

    Public ReadOnly Property IsTableSelected(targetTable As String) As Boolean
        Get
            Return SelectedTables.Contains(targetTable)
        End Get
    End Property

End Class
