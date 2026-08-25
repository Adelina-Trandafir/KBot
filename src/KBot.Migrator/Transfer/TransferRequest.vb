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
        RegistryUnits = New List(Of CaiUnit)()
        SelectedTables = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    End Sub

    Public ReadOnly Property Server As TargetServer
    ''' <summary>The DC, i.e. the MariaDB database name, verbatim from <c>cai.DC</c>.</summary>
    Public ReadOnly Property TargetDatabase As String

    ''' <summary>The units the operator chose. Decision D2 - never "all of the DC".</summary>
    Public ReadOnly Property Units As List(Of CaiUnit)

    ''' <summary>
    ''' EVERY unit in the <c>cai</c> registry, across every DC. Not the selection.
    ''' </summary>
    ''' <remarks>
    ''' Decision D7 needs three answers, not two, and only this list can give the third.
    ''' A unit that is not selected but IS in the registry is a normal shape - either
    ''' another DC's row in a shared Forexe file, or a unit of this DC the operator did
    ''' not tick - and it is skipped in silence. A unit in NO registry row at all means the
    ''' file and the registry disagree, and that stops the run. Comparing only against
    ''' <see cref="Units"/> cannot tell those two apart.
    ''' </remarks>
    Public ReadOnly Property RegistryUnits As List(Of CaiUnit)

    ''' <summary>Every distinct IdUnitate the registry knows, across every DC.</summary>
    Public Function KnownUnitIds() As HashSet(Of Integer)
        Dim ids As New HashSet(Of Integer)(RegistryUnits.Select(Function(u) u.IdUnitate))
        ' The selection is part of the registry by construction, but a request assembled
        ' without one must not turn every selected unit into a blocking finding.
        For Each unit In Units
            ids.Add(unit.IdUnitate)
        Next
        Return ids
    End Function

    ''' <summary>
    ''' Typed by the operator to replace the registry's CodFiscal for this run only.
    ''' </summary>
    ''' <remarks>
    ''' D16: ONE value, not a list. Blank means use the registry. Never persisted to
    ''' <c>LocalStore</c> or to the settings file - it starts empty on every launch, so it
    ''' cannot quietly affect a real migration weeks later. The journal header records both
    ''' the registry value and the value actually used.
    ''' </remarks>
    Public Property CodFiscalOverride As String = String.Empty

    ''' <summary>The CodFiscal recorded in the registry for this DC, or empty.</summary>
    Public Function RegistryCodFiscal() As String
        Return CodFiscalRegistry.ForDc(TargetDatabase)
    End Function

    ''' <summary>
    ''' The CodFiscal the statement files are actually matched against: the override when
    ''' the operator typed one, the registry otherwise.
    ''' </summary>
    Public Function ResolvedCodFiscal() As String
        Dim typed = If(CodFiscalOverride, String.Empty).Trim()
        If typed.Length > 0 Then Return typed
        Return RegistryCodFiscal()
    End Function

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
