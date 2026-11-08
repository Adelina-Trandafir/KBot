Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' Step «E-Factura»: the e-invoice data of the unit moves from Access into the unit database -- the issuer, the customers, the issued invoices with their lines, the received invoices with
''' their lines, notes and messages (the messages of the unit's tax code), the links of the received invoices to their accounting operations (EFT_O), and the links to receptions.
''' Ticking ONLY this step is «doar e-facturi»: it can be run at any time to bring what is new.
''' </summary>
''' <remarks>
''' <para>
''' The work is done by <see cref="EfImporter"/> (moved here from the E-Factura tab of the old Migrator); this step only builds its options and writes on the runner's connection and
''' transaction (<see cref="EfImportOptions.ExternalConnection"/>), so a failure rolls back with the other steps.
''' </para>
''' <para>
''' Existing rows are left as they are, so what the operators did in K-BOT is never overwritten and the run can be repeated: only what is new in Access is added. The ONE exception is the
''' links to the accounting operations (EFT_O): the links that came from Access are deleted and read again every time, so an operation deleted in Access leaves no link. Each link gets
''' <c>IdOperatie</c> = <c>TZ_Oper.IdOper</c> when the operations are migrated; otherwise the step «Operații» sets it.
''' </para>
''' <para>
''' Files: the unit file (the first ticked unit that has one) for the issuer, customers and issued invoices; the store <c>ef_&lt;year&gt;.accdb</c> for the received ones, found at
''' <c>&lt;folder of cale.accdb&gt;\EFACTURA\ef_&lt;year&gt;.accdb</c> or at the path set in <c>avc-migrator-settings.json</c> (<c>EfReceivedFile</c>). Needs <c>sql/00EF_66_primite_operatii_id_access.sql</c>.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcEFacturaStep
    Inherits AvcStep

    Private Shared ReadOnly Tables As String() = {"EF_Clienti", "EF_Facturi", "EF_FacturiLinii", "EF_Mesaje", "EF_Primite", "EF_PrimiteLinii", "EF_PrimiteOperatii", "EF_PrimiteNote", "EF_PrimiteMesaje"}

    Private _issuedFile As String = String.Empty

    Public Overrides ReadOnly Property Key As String
        Get
            Return "EFactura"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 3 - E-Factura"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "E-Factura (facturi emise și primite, mesaje, legături cu operațiile)"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return "EFT"
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return Tables
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _issuedFile = String.Empty
    End Sub

    ' ── the options ─────────────────────────────────────────────────────────────

    Private Shared Function FirstUnitFile(k_context As AvcContext) As String
        Dim k_unit = k_context.Units.FirstOrDefault(Function(k_u) k_u.HasUnitFile)
        Return If(k_unit Is Nothing, String.Empty, k_unit.UnitFilePath)
    End Function

    Private Function BuildOptions(k_context As AvcContext) As EfImportOptions
        Return New EfImportOptions(k_context.Server, k_context.TargetDatabase, k_context.CommonDatabase) With {
            .AccessPassword = k_context.AccessPassword,
            .IssuedFile = _issuedFile,
            .ReceivedFile = k_context.EfReceivedFile,
            .UnitCui = k_context.CodFiscal,
            .DoFurnizor = True, .DoClienti = True, .DoEmise = True, .DoPrimite = True
        }
    End Function

    ' ── read ────────────────────────────────────────────────────────────────────

    ''' <summary>Checks that both Access files exist and counts the received invoices; the rows themselves are read by the importer, in the verification and in the write.</summary>
    Public Overrides Sub ReadAccess(k_context As AvcContext)
        Try
            _issuedFile = FirstUnitFile(k_context)
            If _issuedFile.Length = 0 Then Throw New FileNotFoundException("Nicio unitate bifată nu are fișier de unitate (baza<an>.accdb).")
            If k_context.EfReceivedFile.Length = 0 OrElse Not File.Exists(k_context.EfReceivedFile) Then
                Throw New FileNotFoundException("Magazinul de facturi primite (ef_" & k_context.Year.ToString(CultureInfo.InvariantCulture) & ".accdb) nu a fost găsit la «" &
                                                k_context.EfReceivedFile & "». Puneți fișierul acolo sau scrieți calea în avc-migrator-settings.json (EfReceivedFile).")
            End If
            Using k_cn = AccessProvider.Open(k_context.EfReceivedFile, k_context.AccessPassword)
                Dim k_table As String = AccessSchema.ResolveTableName(k_cn, "EFT")
                Dim k_rows As Integer = 0
                If k_table IsNot Nothing Then
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                        While k_reader.Read()
                            k_rows += 1
                        End While
                    End Using
                End If
                AccessRows = k_rows
                RowsToWrite = k_rows
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AvcEFacturaStep.ReadAccess", ex)
            Throw
        End Try
    End Sub

    ' ── verify ──────────────────────────────────────────────────────────────────

    Private Shared Function ToFinding(k_finding As EfFinding) As Finding
        Dim k_severity As FindingClass = If(k_finding.Severity = EfSeverity.Blocking, FindingClass.Blocant, If(k_finding.Severity = EfSeverity.Warning, FindingClass.Atentie, FindingClass.Informativ))
        Return New Finding(AvcKinds.EFACTURA, k_severity, k_finding.Area, String.Empty, k_finding.Text)
    End Function

    ' The verification reports through the findings, not through the console.
    Private Shared Sub IgnoreText(k_text As String)
    End Sub

    Private Shared Sub IgnoreStep(k_done As Integer, k_total As Integer, k_label As String)
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _issuedFile.Length = 0 Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«E-Factura» nu a fost citit din Access."))
                Return k_findings
            End If
            If k_context.CodFiscal.Trim().Length = 0 Then
                k_findings.Add(New Finding(AvcKinds.EFACTURA, FindingClass.Blocant, "EF_Mesaje", String.Empty,
                                           "Codul fiscal al unității lipsește: fără el nu se știe care mesaje primite sunt ale unității. Completați «Cod fiscal»."))
                Return k_findings
            End If
            Dim k_importer As New EfImporter(BuildOptions(k_context), AddressOf IgnoreText, AddressOf IgnoreStep)
            Dim k_report As EfImportReport = k_importer.Verify(CancellationToken.None)
            For Each k_item In k_report.Findings
                k_findings.Add(ToFinding(k_item))
            Next
            Dim k_toWrite As Long = k_report.Tables.Sum(Function(k_t) k_t.RowsSelected)
            RowsToWrite = CInt(Math.Min(k_toWrite, Integer.MaxValue))
            For Each k_plan In k_report.Tables
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, k_plan.TargetTable, String.Empty, k_plan.Describe(), CInt(Math.Min(k_plan.RowsSelected, Integer.MaxValue))))
            Next
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcEFacturaStep.Verify", ex)
            Throw
        End Try
    End Function

    ' ── write ───────────────────────────────────────────────────────────────────

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _issuedFile.Length = 0 Then Throw New InvalidOperationException("«E-Factura» nu a fost citit din Access.")
            k_dump.WriteComment("EF_Primite", "E-Factura: scrisă de EfImporter pe conexiunea și tranzacția migrării (instrucțiunile nu se jurnalizează rând cu rând). " &
                                "Rândurile existente rămân; legăturile din Access (EF_PrimiteOperatii) se rescriu complet.")
            Dim k_options As EfImportOptions = BuildOptions(k_context)
            k_options.ExternalConnection = k_cn
            k_options.ExternalTransaction = k_transaction
            Dim k_importer As New EfImporter(k_options, Sub(k_text) k_progress.Report(k_text), Sub(k_done, k_total, k_label) k_context.Tick(k_done, k_total, "«" & k_label & "»"))
            Dim k_result As EfImportResult = k_importer.Run(k_cancel)
            Return k_result.Tables.Select(Function(k_t) New AvcWritten(k_t.TargetTable, CInt(Math.Min(k_t.RowsAdded, Integer.MaxValue)))).ToList()
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcEFacturaStep.Write", ex)
            Throw
        End Try
    End Function

End Class
