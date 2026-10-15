Imports System.Globalization

''' <summary>How serious one finding of the E-Factura import is.</summary>
Public Enum EfSeverity
    ''' <summary>Nothing is written until this is fixed: the row or the table cannot be imported as it is.</summary>
    Blocking = 0
    ''' <summary>Written anyway; the operator is told what was changed or left out.</summary>
    Warning = 1
    ''' <summary>A fact, no action needed.</summary>
    Info = 2
End Enum

''' <summary>One thing the verification noticed.</summary>
Public NotInheritable Class EfFinding

    Public Sub New(k_severity As EfSeverity, k_area As String, k_text As String)
        Severity = k_severity
        Area = If(k_area, String.Empty)
        Text = If(k_text, String.Empty)
    End Sub

    Public ReadOnly Property Severity As EfSeverity
    ''' <summary>The target table, or a short name such as «Furnizor».</summary>
    Public ReadOnly Property Area As String
    Public ReadOnly Property Text As String

    Public ReadOnly Property Label As String
        Get
            Select Case Severity
                Case EfSeverity.Blocking : Return "BLOCANT"
                Case EfSeverity.Warning : Return "ATENȚIE"
                Case Else : Return "INFO"
            End Select
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{Label}] {Area}: {Text}"
    End Function

End Class

''' <summary>What one table of the import will do (verification) or did (run).</summary>
Public NotInheritable Class EfTablePlan

    Public Sub New(k_targetTable As String, k_database As String)
        TargetTable = k_targetTable
        Database = k_database
    End Sub

    Public ReadOnly Property TargetTable As String
    Public ReadOnly Property Database As String
    ''' <summary>Rows in the Access table (or lines in the text file), before any filter.</summary>
    Public Property RowsRead As Long
    ''' <summary>Rows that pass the filter and convert cleanly: what would be written.</summary>
    Public Property RowsSelected As Long
    ''' <summary>Rows left out on purpose (another unit's, or their parent is not imported).</summary>
    Public Property RowsLeftOut As Long
    ''' <summary>Rows already in the target table before the run.</summary>
    Public Property RowsBefore As Long
    ''' <summary>Rows added by the run (run only).</summary>
    Public Property RowsAdded As Long
    ''' <summary>Rows that were already there, so left as they were (run only).</summary>
    Public Property RowsKept As Long

    Public Function Describe() As String
        Return $"{TargetTable} ({Database}): citite {RowsRead}, de scris {RowsSelected}, lăsate deoparte {RowsLeftOut}, deja în țintă {RowsBefore}"
    End Function

End Class

''' <summary>The outcome of a verification.</summary>
Public NotInheritable Class EfImportReport

    Private Const MaxPerKind As Integer = 25
    Private ReadOnly _counts As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

    Public ReadOnly Property Findings As New List(Of EfFinding)()
    Public ReadOnly Property Tables As New List(Of EfTablePlan)()

    Public ReadOnly Property HasBlocking As Boolean
        Get
            Return Findings.Any(Function(f) f.Severity = EfSeverity.Blocking)
        End Get
    End Property

    Public Sub Add(k_severity As EfSeverity, k_area As String, k_text As String)
        Findings.Add(New EfFinding(k_severity, k_area, k_text))
    End Sub

    ''' <summary>
    ''' Adds a finding of a repeating kind, but only the first <see cref="MaxPerKind"/> of each kind,
    ''' so a column that is wrong on 3000 rows is one screen of text, not 3000 lines.
    ''' <see cref="Flush"/> writes the «și încă N» lines once the reading is done.
    ''' </summary>
    Public Sub AddLimited(k_severity As EfSeverity, k_area As String, k_kind As String, k_text As String)
        Dim key = $"{CInt(k_severity)}|{k_area}|{k_kind}"
        Dim count As Integer = 0
        _counts.TryGetValue(key, count)
        _counts(key) = count + 1
        If count < MaxPerKind Then Add(k_severity, k_area, k_text)
    End Sub

    Public Sub Flush()
        For Each pair In _counts
            If pair.Value <= MaxPerKind Then Continue For
            Dim parts = pair.Key.Split("|"c)
            Add(CType(Integer.Parse(parts(0), CultureInfo.InvariantCulture), EfSeverity), parts(1),
                $"… și încă {pair.Value - MaxPerKind} de același fel ({parts(2)}).")
        Next
        _counts.Clear()
    End Sub

End Class

''' <summary>What the import asks for. Built by the form, read by <see cref="EfImporter"/>.</summary>
Public NotInheritable Class EfImportOptions

    Public Sub New(k_server As TargetServer, k_unitDatabase As String, k_commonDatabase As String)
        If k_server Is Nothing Then Throw New ArgumentNullException(NameOf(k_server))
        If String.IsNullOrWhiteSpace(k_unitDatabase) Then Throw New ArgumentException("Numele bazei unității lipsește.", NameOf(k_unitDatabase))
        If String.IsNullOrWhiteSpace(k_commonDatabase) Then Throw New ArgumentException("Numele bazei comune lipsește.", NameOf(k_commonDatabase))
        Server = k_server
        UnitDatabase = k_unitDatabase
        CommonDatabase = k_commonDatabase
    End Sub

    Public ReadOnly Property Server As TargetServer
    ''' <summary>The unit database (the DC).</summary>
    Public ReadOnly Property UnitDatabase As String
    ''' <summary>AVACONT_COMUN (or the name the settings give it): only READ, to know the codes of EF_UM (the list is written by the operator on the server, not by this tool).</summary>
    Public ReadOnly Property CommonDatabase As String

    ''' <summary>Typed on the form, never stored, never logged.</summary>
    Public Property AccessPassword As String = String.Empty

    ''' <summary>The unit's <c>baza&lt;year&gt;.accdb</c>: UNIT, Scheme, ClientiEF, Factura, FacturaC.</summary>
    Public Property IssuedFile As String = String.Empty
    ''' <summary>The e-invoice store <c>ef_&lt;year&gt;.accdb</c>: EF, EFT, EFS, EFT_C, EFT_M.</summary>
    Public Property ReceivedFile As String = String.Empty
    ''' <summary>The unit's tax code. Selects the received messages that belong to this unit.</summary>
    Public Property UnitCui As String = String.Empty

    Public Property DoFurnizor As Boolean
    Public Property DoClienti As Boolean
    Public Property DoEmise As Boolean
    Public Property DoPrimite As Boolean

    Public ReadOnly Property AnythingSelected As Boolean
        Get
            Return DoFurnizor OrElse DoClienti OrElse DoEmise OrElse DoPrimite
        End Get
    End Property

End Class

''' <summary>What a run did.</summary>
Public NotInheritable Class EfImportResult

    Public ReadOnly Property Tables As New List(Of EfTablePlan)()
    Public Property Committed As Boolean

    Public ReadOnly Property TotalAdded As Long
        Get
            Return Tables.Sum(Function(t) t.RowsAdded)
        End Get
    End Property

End Class
