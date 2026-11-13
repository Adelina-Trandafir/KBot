Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Slice 00EF-03: the «E-Factura» tab of the Migrator. Copies, from the unit's Access files into MariaDB:
''' the issuer's data, the customers, the issued invoices with their lines, and
''' the received invoices with their lines, notes and messages. Accounting columns and tables are NOT carried.
''' </summary>
''' <remarks>
''' <para>
''' Two phases, the shape the other runners have. <see cref="Verify"/> reads everything the run would read, writes nothing,
''' and reports; <see cref="Run"/> reads again and writes in ONE transaction on the unit database;
''' any failure rolls it back. The units of measure (EF_UM, AVACONT_COMUN) are NOT migrated: the operator writes that list
''' on the server; this tool only reads it, to flag a line whose unit of measure is not in it.
''' </para>
''' <para>
''' Every Access key (IdClient, IdFactura, IDEFT, ...) is kept as the MariaDB key, so rows keep pointing at each other
''' without a mapping table, and AUTO_INCREMENT carries on after the largest one. A row already in the target is left as it
''' is (see <see cref="EfBatchWriter"/>): the import can be repeated without undoing what was done in K-BOT meanwhile.
''' </para>
''' <para>
''' Received invoices: only the messages whose <c>cui_unit</c> is this unit's tax code (compared without «RO» and spaces,
''' <see cref="EfCui"/>); their invoices, lines, notes, messages and operation links (EFT_O, which says the invoice was processed)
''' follow by key. The accounting operations themselves (Oper, Documente, PlatiFacturi) stay behind.
''' </para>
''' </remarks>
Public NotInheritable Class EfImporter

    Private ReadOnly _options As EfImportOptions
    Private ReadOnly _say As Action(Of String)
    Private ReadOnly _step As Action(Of Integer, Integer, String)

    ' Key sets of ONE pass: reset at the start of every pass.
    Private _accessClientIds As HashSet(Of Integer)
    Private _accessInvoiceIds As HashSet(Of Integer)
    Private _invoiceNumbers As HashSet(Of String)
    Private _messageIds As HashSet(Of String)
    Private _receivedIds As HashSet(Of Integer)
    ' Clasificatii of the target: (IdClsfAcc, IdUnitate) -> IDClsf values; and the EFT_O pairs that did not resolve to exactly one
    Private _classificationPairs As Dictionary(Of (Integer, Integer), List(Of Integer))
    Private _classificationProblems As Dictionary(Of (Integer, Integer), (Matches As Integer, Rows As Integer))
    ' Slice ME01-34: the target's Clasificatii by their code (Clsf), to propose the right row for an Access id that did not resolve
    Private _classificationsByCode As Dictionary(Of String, List(Of ClsfCandidate))
    ' ... the Access classifications by id (one entry per unit file), how many files were read, and the (Access id, unit) -> IDClsf ties made by code
    Private _accessClassifications As Dictionary(Of Integer, List(Of AccessClsf))
    Private _classificationFilesRead As Integer
    Private _classificationsByCodeUsed As Dictionary(Of (Integer, Integer), (IdClsf As Integer, Rows As Integer))
    ' ... classifications that Clasificatii lacks and Access holds: created (write pass) / to be created (verification), and why one could not be
    Private _classificationsCreated As Dictionary(Of (Integer, Integer), (IdClsf As Integer, Unit As Integer, Code As String, Denumire As String, Rows As Integer))
    Private _classificationsPlanned As Dictionary(Of (Integer, Integer), (Unit As Integer, Code As String, Denumire As String, Rows As Integer))
    Private _classificationNotCreated As Dictionary(Of (Integer, Integer), String)
    ' ... the EFT_O rows behind each unresolved (Access id, unit), first 10 each, and how many EFT_O rows got their unit from their Sursa
    Private _classificationProblemRows As Dictionary(Of (Integer, Integer), List(Of String))
    Private _unitsFromSursa As Dictionary(Of String, Integer)
    Private _umKnown As HashSet(Of String)
    Private _umUsed As Dictionary(Of String, Integer)
    ' Slice 00EF-38: received invoices read in this pass, and the ones already in the target before it (those are never linked here)
    Private _linkInvoices As List(Of EfLinkInvoice)
    Private _receivedBefore As HashSet(Of Integer)
    Private _stepsDone As Integer

    Public Sub New(k_options As EfImportOptions, k_say As Action(Of String),
                   k_step As Action(Of Integer, Integer, String))
        If k_options Is Nothing Then Throw New ArgumentNullException(NameOf(k_options))
        _options = k_options
        _say = If(k_say, Sub(k_text As String)
                         End Sub)
        _step = If(k_step, Sub(k_done As Integer, k_total As Integer, k_label As String)
                       End Sub)
    End Sub

    ' ---- the two public phases -----------------------------------------------------------------

    ''' <summary>Reads everything, writes nothing.</summary>
    Public Function Verify(k_token As CancellationToken) As EfImportReport
        Dim report As New EfImportReport()
        Try
            If Not _options.AnythingSelected Then
                report.Add(EfSeverity.Blocking, "E-Factura", "Nu a fost bifat nimic de importat.")
                Return report
            End If
            Pass(False, report, k_token)
            report.Flush()
            Return report
        Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
            GlobalErrorLog.Write("EfImporter.Verify", ex)
            Throw
        End Try
    End Function

    ''' <summary>Writes, in one transaction per database, committed together at the end.</summary>
    Public Function Run(k_token As CancellationToken) As EfImportResult
        Dim report As New EfImportReport()
        Dim result As New EfImportResult()
        Try
            If Not _options.AnythingSelected Then Throw New InvalidOperationException("Nu a fost bifat nimic de importat.")
            Pass(True, report, k_token, result)
            report.Flush()
            Return result
        Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
            GlobalErrorLog.Write("EfImporter.Run", ex)
            Throw
        End Try
    End Function

    ' ---- one pass (verification when write = False) ---------------------------------------------

    Private Sub Pass(k_write As Boolean, k_report As EfImportReport, k_token As CancellationToken,
                     Optional k_result As EfImportResult = Nothing)
        ResetState()
        Dim cnUnit As MySqlConnection = Nothing
        Dim txUnit As MySqlTransaction = Nothing
        Dim accessIssued As OleDbConnection = Nothing
        Dim accessReceived As OleDbConnection = Nothing
        Dim plans As New List(Of EfTablePlan)()
        Try
            If k_write AndAlso _options.ExternalConnection IsNot Nothing Then
                cnUnit = _options.ExternalConnection
                txUnit = _options.ExternalTransaction
                If txUnit Is Nothing Then Throw New InvalidOperationException("Conexiunea externă are nevoie de tranzacția ei.")
            Else
                cnUnit = _options.Server.Open(_options.UnitDatabase)
                If k_write Then txUnit = cnUnit.BeginTransaction()
            End If

            Dim totalSteps As Integer = StepCount()
            _stepsDone = 0

            _umKnown = ReadKnownUm(k_report)

            ' --- Access files -----------------------------------------------------------------------
            If _options.DoFurnizor OrElse _options.DoClienti OrElse _options.DoEmise Then
                accessIssued = OpenAccess(_options.IssuedFile, "Facturi emise (fișierul unității)", k_report)
            End If
            If _options.DoPrimite Then
                If EfCui.Normalize(_options.UnitCui).Length = 0 Then
                    k_report.Add(EfSeverity.Blocking, "EF_Mesaje",
                        "Codul fiscal al unității lipsește: fără el nu se știe care mesaje primite sunt ale unității. Completați «Cod fiscal».")
                Else
                    accessReceived = OpenAccess(_options.ReceivedFile, "Facturi primite (ef_<an>.accdb)", k_report)
                End If
            End If

            ' --- issuer ---------------------------------------------------------------------------------
            If _options.DoFurnizor AndAlso accessIssued IsNot Nothing Then
                k_token.ThrowIfCancellationRequested()
                DoFurnizor(k_write, k_report, accessIssued, cnUnit, txUnit, plans, totalSteps)
            End If

            If _options.DoPrimite Then
                _classificationPairs = LoadClassificationPairs(cnUnit, txUnit)
                _classificationsByCode = LoadClassificationCodes(cnUnit, txUnit)
                _accessClassifications = LoadAccessClassifications(k_report)
                If TableExists(cnUnit, "EF_Primite", txUnit) Then _receivedBefore = LoadTargetIntKeys(cnUnit, txUnit, "EF_Primite", "IdPrimita")
            End If

            ' --- the tables, parents before children -----------------------------------------------------
            For Each spec In BuildSpecs()
                k_token.ThrowIfCancellationRequested()
                Dim source As OleDbConnection = If(IsReceivedTable(spec), accessReceived, accessIssued)
                If source Is Nothing Then Continue For
                Dim isLinks As Boolean = String.Equals(spec.TargetTable, "EF_PrimiteOperatii", StringComparison.Ordinal)
                If isLinks Then ReportLinkRewrite(k_write, k_report, cnUnit, txUnit)
                If isLinks Then PrepareClassifications(source, k_write, k_report, cnUnit, txUnit)
                CopyTable(spec, source, k_write, k_report, cnUnit, txUnit, plans, totalSteps, k_token)
                If isLinks AndAlso k_write AndAlso Not k_report.HasBlocking Then PointLinksAtOperations(cnUnit, txUnit)
            Next

            If _options.DoPrimite AndAlso accessReceived IsNot Nothing Then
                k_token.ThrowIfCancellationRequested()
                DoReceivedReferences(k_write, k_report, accessReceived, cnUnit, txUnit)
                k_token.ThrowIfCancellationRequested()
                DoReceivedCpv(k_write, k_report, accessReceived, cnUnit, txUnit, k_token)
                k_token.ThrowIfCancellationRequested()
                DoReceptionLinks(k_write, k_report, cnUnit, txUnit, plans, totalSteps)
            End If
            If _options.DoEmise Then ReportUnknownUm(k_report)
            If _options.DoPrimite Then ReportClassificationProblems(k_report)

            k_report.Tables.AddRange(plans)
            If k_result IsNot Nothing Then k_result.Tables.AddRange(plans)

            If k_write Then
                If k_report.HasBlocking Then
                    Throw New InvalidOperationException(
                        "Importul a găsit date care nu se pot scrie (vezi constatările BLOCANTE din jurnal). Nu s-a scris nimic.")
                End If
                If _options.ExternalConnection Is Nothing Then
                    txUnit?.Commit()
                    If k_result IsNot Nothing Then k_result.Committed = True
                End If
            End If
        Catch
            If _options.ExternalConnection Is Nothing Then
                Try
                    txUnit?.Rollback()
                Catch rollbackEx As Exception
                    GlobalErrorLog.Write("EfImporter.Pass.RollbackUnit", rollbackEx)
                End Try
            End If
            Throw
        Finally
            If accessIssued IsNot Nothing Then accessIssued.Dispose()
            If accessReceived IsNot Nothing Then accessReceived.Dispose()
            If _options.ExternalConnection Is Nothing Then
                If txUnit IsNot Nothing Then txUnit.Dispose()
                If cnUnit IsNot Nothing Then cnUnit.Dispose()
            End If
        End Try
    End Sub

    Private Sub ResetState()
        _accessClientIds = New HashSet(Of Integer)()
        _accessInvoiceIds = New HashSet(Of Integer)()
        _invoiceNumbers = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _messageIds = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _receivedIds = New HashSet(Of Integer)()
        _classificationPairs = New Dictionary(Of (Integer, Integer), List(Of Integer))()
        _classificationProblems = New Dictionary(Of (Integer, Integer), (Matches As Integer, Rows As Integer))()
        _classificationsByCode = New Dictionary(Of String, List(Of ClsfCandidate))(StringComparer.Ordinal)
        _accessClassifications = New Dictionary(Of Integer, List(Of AccessClsf))()
        _classificationFilesRead = 0
        _classificationsByCodeUsed = New Dictionary(Of (Integer, Integer), (IdClsf As Integer, Rows As Integer))()
        _classificationsCreated = New Dictionary(Of (Integer, Integer), (IdClsf As Integer, Unit As Integer, Code As String, Denumire As String, Rows As Integer))()
        _classificationsPlanned = New Dictionary(Of (Integer, Integer), (Unit As Integer, Code As String, Denumire As String, Rows As Integer))()
        _classificationNotCreated = New Dictionary(Of (Integer, Integer), String)()
        _classificationProblemRows = New Dictionary(Of (Integer, Integer), List(Of String))()
        _unitsFromSursa = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        _umKnown = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _umUsed = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        _linkInvoices = New List(Of EfLinkInvoice)()
        _receivedBefore = New HashSet(Of Integer)()
    End Sub

    Private Function StepCount() As Integer
        Dim total As Integer = 0
        If _options.DoFurnizor Then total += 1
        If _options.DoClienti Then total += 1
        If _options.DoEmise Then total += 2
        If _options.DoPrimite Then total += 7
        Return Math.Max(total, 1)
    End Function

    Private Sub Advance(k_total As Integer, k_label As String)
        _stepsDone += 1
        _step(Math.Min(_stepsDone, k_total), k_total, k_label)
    End Sub

    ' ---- the units of measure: READ only ----------------------------------------------------------------

    ''' <summary>The codes in AVACONT_COMUN.EF_UM (written by the operator on the server), so a line with another code can be flagged.</summary>
    Private Function ReadKnownUm(k_report As EfImportReport) As HashSet(Of String)
        Dim known As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Not _options.DoEmise Then Return known
        Try
            Using cn = _options.Server.Open(_options.CommonDatabase)
                If Not TableExists(cn, "EF_UM", Nothing) Then Return known
                Using cmd As New MySqlCommand("SELECT `Cod` FROM `EF_UM`", cn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            known.Add(reader.GetString(0))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Not blocking: the check on line units of measure then has nothing to compare with.
            GlobalErrorLog.Write("EfImporter.ReadKnownUm", ex)
            k_report.Add(EfSeverity.Warning, "EF_UM", "Lista unităților de măsură din baza comună nu a putut fi citită: codurile de pe linii nu se verifică.")
        End Try
        Return known
    End Function

    Private Sub ReportUnknownUm(k_report As EfImportReport)
        ' An empty list (not written yet, or unreadable) says nothing about any code: no warnings then.
        If _umKnown.Count = 0 Then Return
        For Each pair In _umUsed.OrderBy(Function(p) p.Key, StringComparer.OrdinalIgnoreCase)
            If pair.Key.Length = 0 OrElse _umKnown.Contains(pair.Key) Then Continue For
            k_report.AddLimited(EfSeverity.Warning, "EF_FacturiLinii", "UM necunoscută",
                $"unitatea de măsură «{pair.Key}» ({pair.Value} linii) nu este în EF_UM; ANAF poate refuza factura.")
        Next
    End Sub

    ' ---- the issuer -------------------------------------------------------------------------------------

    Private Sub DoFurnizor(k_write As Boolean, k_report As EfImportReport, k_access As OleDbConnection,
                           k_cn As MySqlConnection, k_tx As MySqlTransaction, k_plans As List(Of EfTablePlan),
                           k_total As Integer)
        Dim plan As New EfTablePlan("Unitati_Date", _options.CommonDatabase)
        k_plans.Add(plan)

        Dim spec As New EfTableSpec("UNIT", "Unitati_Date", "DC", "DC")
        spec.Columns.Add(New EfColumn("Denumire", Nothing, EfKind.Text) With {.MaxLength = 255, .Required = True})
        spec.Columns.Add(New EfColumn("CodFiscal", Nothing, EfKind.Text) With {.MaxLength = 32, .Required = True})
        spec.Columns.Add(New EfColumn("Adresa", Nothing, EfKind.Text) With {.MaxLength = 255})
        spec.Columns.Add(New EfColumn("Orasul", Nothing, EfKind.Text) With {.MaxLength = 255})
        spec.Columns.Add(New EfColumn("Judetul", Nothing, EfKind.Text) With {.MaxLength = 8})
        spec.Columns.Add(New EfColumn("Mail", Nothing, EfKind.Text) With {.MaxLength = 255})
        spec.Columns.Add(New EfColumn("Telefon", Nothing, EfKind.Text) With {.MaxLength = 64})
        spec.Columns.Add(New EfColumn("SerieFactura", Nothing, EfKind.Text) With {.MaxLength = 10})
        spec.Columns.Add(New EfColumn("AfiseazaPrimiteNoi", Nothing, EfKind.Flag) With {.DefaultValue = 0})

        Dim unitTable = AccessSchema.ResolveTableName(k_access, "UNIT")
        If unitTable Is Nothing Then
            k_report.Add(EfSeverity.Blocking, "Unitati_Date", "Tabelul UNIT lipsește din fișierul unității.")
            Advance(k_total, "Unitati_Date")
            Return
        End If

        Dim raw As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
        Dim unitRows As Integer = 0
        Using reader = AccessSchema.OpenReader(k_access, unitTable)
            While reader.Read()
                unitRows += 1
                If unitRows > 1 Then Continue While
                raw("Denumire") = reader.ValueOrMissing("NumeUnitate")
                raw("CodFiscal") = reader.ValueOrMissing("CUI")
                raw("Adresa") = reader.ValueOrMissing("Adresa")
                raw("Orasul") = reader.ValueOrMissing("Orasul")
                raw("Judetul") = reader.ValueOrMissing("Judetul")
                raw("Mail") = reader.ValueOrMissing("AdresaMail")
                raw("Telefon") = reader.ValueOrMissing("TelefonContact")
            End While
        End Using
        plan.RowsRead = unitRows
        If unitRows = 0 Then
            k_report.Add(EfSeverity.Blocking, "Unitati_Date", "Tabelul UNIT din fișierul unității nu are niciun rând.")
            Advance(k_total, "Unitati_Date")
            Return
        End If
        If unitRows > 1 Then
            k_report.Add(EfSeverity.Warning, "Unitati_Date", $"UNIT are {unitRows} rânduri; se folosește primul.")
        End If

        Dim schemeTable = AccessSchema.ResolveTableName(k_access, "Scheme")
        If schemeTable Is Nothing Then
            k_report.Add(EfSeverity.Warning, "Unitati_Date", "Tabelul Scheme lipsește: seria facturii rămâne necompletată.")
        Else
            Dim found As Boolean = False
            Using reader = AccessSchema.OpenReader(k_access, schemeTable)
                While reader.Read()
                    Dim form = Convert.ToString(reader.ValueOrMissing("NumeForm"), CultureInfo.InvariantCulture)
                    If Not String.Equals(If(form, String.Empty).Trim(), "DPIFV", StringComparison.OrdinalIgnoreCase) Then Continue While
                    found = True
                    raw("SerieFactura") = reader.ValueOrMissing("T3")
                    raw("AfiseazaPrimiteNoi") = reader.ValueOrMissing("C2")
                    Exit While
                End While
            End Using
            If Not found Then k_report.Add(EfSeverity.Warning, "Unitati_Date", "În Scheme nu există rândul «DPIFV»: seria facturii rămâne necompletată.")
        End If

        Dim values(spec.Columns.Count - 1) As Object
        Dim ok As Boolean = True
        For i = 0 To spec.Columns.Count - 1
            Dim col = spec.Columns(i)
            Dim value As Object = Nothing
            raw.TryGetValue(col.Target, value)
            Dim issue = EfConverter.ToTarget(col, value, values(i))
            If issue.Kind = EfIssueKind.Blocking Then
                ok = False
                k_report.Add(EfSeverity.Blocking, "Unitati_Date", $"{col.Target}: {issue.Text}")
            ElseIf issue.Kind = EfIssueKind.Warning Then
                k_report.Add(EfSeverity.Warning, "Unitati_Date", $"{col.Target}: {issue.Text}")
            End If
        Next
        If ok Then plan.RowsSelected = 1

        ' Slice 00EF-13: the issuer is a row of AVACONT_COMUN.Unitati_Date (key DC = the unit database), written through the
        ' unit connection and its transaction (same server: the statements name the schema).
        Dim detailsExist As Boolean = k_cn IsNot Nothing AndAlso DetailsTableExists(k_cn, k_tx)
        If detailsExist Then
            plan.RowsBefore = CountDetails(k_cn, k_tx)
            If plan.RowsBefore > 0 Then k_report.Add(EfSeverity.Info, "Unitati_Date", "Există deja datele furnizorului: rămân cum sunt (nu se suprascriu).")
        ElseIf k_cn IsNot Nothing Then
            k_report.Add(EfSeverity.Blocking, "Unitati_Date",
                $"Tabelul Unitati_Date lipsește din baza «{_options.CommonDatabase}». Rulați scriptul sql/00EF_13_unitati_detalii.sql.")
        End If

        If k_write AndAlso ok AndAlso Not k_report.HasBlocking Then
            WriteDetails(k_cn, k_tx, spec, values)
            plan.RowsAdded = CountDetails(k_cn, k_tx) - plan.RowsBefore
            plan.RowsKept = plan.RowsSelected - plan.RowsAdded
        End If
        _say($"Unitati_Date: {Convert.ToString(values(0), CultureInfo.InvariantCulture)}, CUI {Convert.ToString(values(1), CultureInfo.InvariantCulture)}.")
        Advance(k_total, "Unitati_Date")
    End Sub

    Private Function DetailsTableExists(k_cn As MySqlConnection, k_tx As MySqlTransaction) As Boolean
        Using cmd = k_cn.CreateCommand()
            cmd.Transaction = k_tx
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @s AND TABLE_NAME = 'Unitati_Date'"
            cmd.Parameters.AddWithValue("@s", _options.CommonDatabase)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Function CountDetails(k_cn As MySqlConnection, k_tx As MySqlTransaction) As Long
        Using cmd = k_cn.CreateCommand()
            cmd.Transaction = k_tx
            cmd.CommandText = "SELECT COUNT(*) FROM " & TargetServer.Quote(_options.CommonDatabase) & ".`Unitati_Date` WHERE `DC` = @dc"
            cmd.Parameters.AddWithValue("@dc", _options.UnitDatabase)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

    ''' <summary>Inserts the unit's row; a row that is already there stays as it is (INSERT IGNORE on the key DC).</summary>
    Private Sub WriteDetails(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_spec As EfTableSpec, k_values As Object())
        Try
            Using cmd = k_cn.CreateCommand()
                cmd.Transaction = k_tx
                Dim names As New List(Of String)()
                names.Add("`DC`")
                Dim marks As New List(Of String)()
                marks.Add("@dc")
                cmd.Parameters.AddWithValue("@dc", _options.UnitDatabase)
                For i = 0 To k_spec.Columns.Count - 1
                    names.Add(TargetServer.Quote(k_spec.Columns(i).Target))
                    marks.Add("@p" & i.ToString(CultureInfo.InvariantCulture))
                    cmd.Parameters.AddWithValue("@p" & i.ToString(CultureInfo.InvariantCulture), If(k_values(i), DBNull.Value))
                Next
                cmd.CommandText = "INSERT IGNORE INTO " & TargetServer.Quote(_options.CommonDatabase) & ".`Unitati_Date` (" &
                                  String.Join(", ", names) & ") VALUES (" & String.Join(", ", marks) & ")"
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("EfImporter.WriteDetails", ex)
            Throw
        End Try
    End Sub

    ' ---- the table definitions ------------------------------------------------------------------------------

    Private Function IsReceivedTable(k_spec As EfTableSpec) As Boolean
        Return k_spec.TargetTable.StartsWith("EF_Mesaje", StringComparison.Ordinal) OrElse
               k_spec.TargetTable.StartsWith("EF_Primite", StringComparison.Ordinal)
    End Function

    Private Function BuildSpecs() As List(Of EfTableSpec)
        Dim specs As New List(Of EfTableSpec)()
        If _options.DoClienti OrElse _options.DoEmise Then specs.Add(ClientSpec())
        If _options.DoEmise Then
            specs.Add(InvoiceSpec())
            specs.Add(InvoiceLineSpec())
        End If
        If _options.DoPrimite Then
            specs.Add(MessageSpec())
            specs.Add(ReceivedSpec())
            specs.Add(ReceivedLineSpec())
            specs.Add(ReceivedOperationSpec())
            specs.Add(ReceivedNoteSpec())
            specs.Add(ReceivedMessageSpec())
        End If
        Return specs
    End Function

    Private Shared Function Col(k_target As String, k_source As String, k_kind As EfKind,
                                Optional k_maxLength As Integer = 0, Optional k_required As Boolean = False,
                                Optional k_scale As Integer = 0, Optional k_default As Object = Nothing,
                                Optional k_optionalSource As Boolean = False) As EfColumn
        Return New EfColumn(k_target, k_source, k_kind) With {
            .MaxLength = k_maxLength, .Required = k_required, .Scale = k_scale,
            .DefaultValue = k_default, .OptionalSource = k_optionalSource
        }
    End Function

    Private Function ClientSpec() As EfTableSpec
        Dim s As New EfTableSpec("ClientiEF", "EF_Clienti", "IdClient", "IdClient")
        s.Columns.Add(Col("IdClient", "IdClient", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("DenumireClient", "DenumireClient", EfKind.Text, 255, True))
        s.Columns.Add(Col("CodFiscal", "CodFiscal", EfKind.Text, 32))
        s.Columns.Add(Col("IndFiscal", "IndFiscal", EfKind.Text, 8))
        s.Columns.Add(Col("Cont", "Cont", EfKind.Text, 34))
        s.Columns.Add(Col("Banca", "Banca", EfKind.Text, 255))
        s.Columns.Add(Col("Adresa", "Adresa", EfKind.Text, 255))
        s.Columns.Add(Col("Judetul", "Judetul", EfKind.Text, 8))
        s.Columns.Add(Col("Orasul", "Orasul", EfKind.Text, 255))
        s.Columns.Add(Col("Sector", "Sector", EfKind.Text, 8))
        s.Columns.Add(Col("CNP", "CNP", EfKind.Flag, k_default:=0))
        Return s
    End Function

    Private Function InvoiceSpec() As EfTableSpec
        Dim s As New EfTableSpec("Factura", "EF_Facturi", "IdFactura", "IdFactura")
        s.Columns.Add(Col("IdFactura", "IdFactura", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdClient", "IdClient", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("SerieFactura", "SerieFactura", EfKind.Text, 10, k_default:=String.Empty))
        s.Columns.Add(Col("NumarFactura", "NumarFactura", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("DataFactura", "DataFactura", EfKind.DateOnly, k_required:=True))
        s.Columns.Add(Col("TipFactura", "TipFactura", EfKind.Text, 3, k_default:="380"))
        s.Columns.Add(Col("Comentarii", "Comentarii", EfKind.Text, 255))
        s.Columns.Add(Col("BT_13", "BT_13", EfKind.Text, 30, k_optionalSource:=True))
        s.Columns.Add(Col("ContPlata", "ContPlata", EfKind.Text, 34))
        s.Columns.Add(Col("Anulata", "Anulata", EfKind.Flag, k_default:=0))
        s.Columns.Add(Col("Trimisa", "TRIMISA", EfKind.Flag, k_default:=0))
        s.Columns.Add(Col("id_incarcare", "id_incarcare", EfKind.Text, 32))
        s.Columns.Add(Col("id_descarcare", "id_descarcare", EfKind.Text, 32))
        s.Columns.Add(Col("AtasamentOriginal", "ATT", EfKind.Flag, k_default:=0))
        s.Columns.Add(Col("IdFacturaA", "IdFacturaA", EfKind.Whole, k_optionalSource:=True))
        s.Columns.Add(Col("SerieFacturaA", "SerieFacturaA", EfKind.Text, 10, k_optionalSource:=True))
        s.Columns.Add(Col("NumarFacturaA", "NumarFacturaA", EfKind.Text, 10, k_optionalSource:=True))
        s.Columns.Add(Col("Corectata", "Corectata", EfKind.Amount, k_scale:=0, k_default:=0, k_optionalSource:=True))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        s.RowCheck = Function(r As AccessTableReader) As EfRowDecision
                         Dim client = ToInt(r.ValueOrMissing("IdClient"))
                         If client.HasValue AndAlso Not _accessClientIds.Contains(client.Value) Then
                             Return EfRowDecision.Block($"factura {Convert.ToString(r.ValueOrMissing("IdFactura"), CultureInfo.InvariantCulture)} " &
                                                       $"are clientul {client.Value}, care nu există în ClientiEF")
                         End If
                         Dim serie = Convert.ToString(r.ValueOrMissing("SerieFactura"), CultureInfo.InvariantCulture)
                         Dim number = ToInt(r.ValueOrMissing("NumarFactura"))
                         If number.HasValue Then
                             Dim key = $"{If(serie, String.Empty).Trim()}|{number.Value}"
                             If Not _invoiceNumbers.Add(key) Then
                                 Return EfRowDecision.Block($"seria/numărul «{key.Replace("|", " / ")}» apare pe mai multe facturi (cheia e unică în noul tabel)")
                             End If
                         End If
                         Return EfRowDecision.Keep
                     End Function
        Return s
    End Function

    Private Function InvoiceLineSpec() As EfTableSpec
        Dim s As New EfTableSpec("FacturaC", "EF_FacturiLinii", "IdContinut", "IdContinut")
        s.Columns.Add(Col("IdContinut", "IdContinut", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdFactura", "IdFactura", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("NrCrt", "NrCrt", EfKind.Text, 16, k_default:=String.Empty))
        s.Columns.Add(Col("Continut", "Continut", EfKind.Text, 65535, k_default:=String.Empty))
        s.Columns.Add(Col("Um", "Um", EfKind.Text, 8, k_default:=String.Empty))
        s.Columns.Add(Col("Cant", "Cant", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("PU", "PU", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Platit", "Platit", EfKind.Flag, k_default:=0))
        s.Columns.Add(Col("Grup", "Grup", EfKind.Whole, k_default:=0))
        s.RowCheck = Function(r As AccessTableReader) As EfRowDecision
                         Dim invoice = ToInt(r.ValueOrMissing("IdFactura"))
                         If invoice.HasValue AndAlso Not _accessInvoiceIds.Contains(invoice.Value) Then
                             Return EfRowDecision.Block($"linia {Convert.ToString(r.ValueOrMissing("IdContinut"), CultureInfo.InvariantCulture)} " &
                                                       $"aparține facturii {invoice.Value}, care nu există în Factura")
                         End If
                         Return EfRowDecision.Keep
                     End Function
        Dim umAt = s.IndexOf("Um")
        s.AfterRow = Sub(row As Object())
                         Dim code = TryCast(row(umAt), String)
                         If code Is Nothing Then Return
                         Dim count As Integer = 0
                         _umUsed.TryGetValue(code, count)
                         _umUsed(code) = count + 1
                     End Sub
        Return s
    End Function

    Private Function MessageSpec() As EfTableSpec
        Dim s As New EfTableSpec("EF", "EF_Mesaje", "IdMesaj", "IDEF")
        s.Columns.Add(Col("IdMesaj", "IDEF", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdSol", "id_sol", EfKind.Text, 32, True))
        s.Columns.Add(Col("IdIncarcare", "id", EfKind.Text, 32))
        s.Columns.Add(Col("CifEmitent", "cif", EfKind.Text, 32))
        s.Columns.Add(Col("DataMesaj", "data", EfKind.DateOnly))
        s.Columns.Add(Col("CuiUnitate", "cui_unit", EfKind.Text, 32))
        s.Columns.Add(Col("Nou", "nou", EfKind.Flag, k_default:=1))
        s.Columns.Add(Col("NumeFisier", "nume_fisier", EfKind.Text, 255, k_optionalSource:=True))
        s.Columns.Add(Col("XmlContinut", "XML", EfKind.Binary, k_optionalSource:=True))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        Dim wanted = EfCui.Normalize(_options.UnitCui)
        s.RowCheck = Function(r As AccessTableReader) As EfRowDecision
                         Dim own = EfCui.Normalize(Convert.ToString(r.ValueOrMissing("cui_unit"), CultureInfo.InvariantCulture))
                         Return If(String.Equals(own, wanted, StringComparison.Ordinal), EfRowDecision.Keep, EfRowDecision.LeaveOut)
                     End Function
        Dim solAt = s.IndexOf("IdSol")
        s.AfterRow = Sub(row As Object())
                         Dim sol = Convert.ToString(row(solAt), CultureInfo.InvariantCulture)
                         _messageIds.Add(sol)
                     End Sub
        s.BatchSize = 25
        Return s
    End Function

    Private Function ReceivedSpec() As EfTableSpec
        Dim s As New EfTableSpec("EFT", "EF_Primite", "IdPrimita", "IDEFT")
        s.ExtraSourceColumns.AddRange({"CUI", "NC", "IDEFT_REF"})
        s.Columns.Add(Col("IdPrimita", "IDEFT", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdSol", "id_sol", EfKind.Text, 32, True))
        s.Columns.Add(Col("NrFact", "NrFact", EfKind.Text, 64))
        s.Columns.Add(Col("DataFact", "DataFact", EfKind.DateOnly))
        s.Columns.Add(Col("DataScad", "DataScad", EfKind.DateOnly))
        s.Columns.Add(Col("CotaTVA", "CotaTVA", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("TVA", "TVA", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Total", "TOTAL", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("CUI", "CUI", EfKind.Text, 32))
        s.Columns.Add(New EfColumn("CuiNormalizat", Nothing, EfKind.Text) With {
            .MaxLength = 32,
            .Compute = Function(r As AccessTableReader) As Object
                           Dim normalised = EfCui.Normalize(Convert.ToString(r.ValueOrMissing("CUI"), CultureInfo.InvariantCulture))
                           Return If(normalised.Length = 0, CType(DBNull.Value, Object), normalised)
                       End Function})
        s.Columns.Add(Col("DenumireP", "DenumireP", EfKind.Text, 255))
        s.Columns.Add(Col("Adresa", "Adresa", EfKind.Text, 500))
        s.Columns.Add(Col("Atasament", "Attach", EfKind.Text, 255, k_optionalSource:=True))
        s.Columns.Add(Col("Tip", "Tip", EfKind.Text, 16, k_default:="FC"))
        s.Columns.Add(New EfColumn("Semn", Nothing, EfKind.Whole) With {
            .DefaultValue = 1,
            .Compute = Function(r As AccessTableReader) As Object
                           Dim sign = ToInt(r.ValueOrMissing("NC"))
                           Return If(sign.HasValue AndAlso sign.Value < 0, -1, 1)
                       End Function})
        s.Columns.Add(Col("Ref", "Ref", EfKind.Text, 255, k_optionalSource:=True))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        s.RowCheck = Function(r As AccessTableReader) As EfRowDecision
                         Dim sol = Convert.ToString(r.ValueOrMissing("id_sol"), CultureInfo.InvariantCulture)
                         Return If(_messageIds.Contains(If(sol, String.Empty).Trim()), EfRowDecision.Keep, EfRowDecision.LeaveOut)
                     End Function
        Dim idAt = s.IndexOf("IdPrimita")
        Dim nrAt = s.IndexOf("NrFact")
        Dim dateAt = s.IndexOf("DataFact")
        Dim totalAt = s.IndexOf("Total")
        Dim cuiAt = s.IndexOf("CuiNormalizat")
        Dim signAt = s.IndexOf("Semn")
        s.AfterRow = Sub(row As Object())
                         _receivedIds.Add(Convert.ToInt32(row(idAt), CultureInfo.InvariantCulture))
                         ' what the automatic linking to receptions needs (slice 00EF-38); the total carries the invoice sign, as the K-BOT view reads it
                         Dim k_sign As Integer = If(row(signAt) Is DBNull.Value, 1, Convert.ToInt32(row(signAt), CultureInfo.InvariantCulture))
                         _linkInvoices.Add(New EfLinkInvoice With {
                             .IdPrimita = Convert.ToInt32(row(idAt), CultureInfo.InvariantCulture),
                             .NrFact = If(TryCast(row(nrAt), String), String.Empty),
                             .Cui = If(TryCast(row(cuiAt), String), String.Empty),
                             .Total = If(row(totalAt) Is DBNull.Value, 0D, Convert.ToDecimal(row(totalAt), CultureInfo.InvariantCulture)) * k_sign,
                             .DataFact = If(TypeOf row(dateAt) Is DateTime, CType(DirectCast(row(dateAt), DateTime), DateTime?), Nothing)})
                     End Sub
        Return s
    End Function

    ''' <summary>
    ''' Slice 00EF-38. Fourth pass over the received invoices: links to the receptions that already exist, only where the match is certain
    ''' (the rules are in <see cref="EfReceptionLinker"/>). Only the invoices this run adds are looked at.
    ''' </summary>
    Private Sub DoReceptionLinks(k_write As Boolean, k_report As EfImportReport, k_cn As MySqlConnection, k_tx As MySqlTransaction,
                                 k_plans As List(Of EfTablePlan), k_total As Integer)
        Dim k_plan As New EfTablePlan("EF_PrimiteReceptii", _options.UnitDatabase)
        k_plans.Add(k_plan)
        Dim k_fresh = _linkInvoices.Where(Function(k_i) Not _receivedBefore.Contains(k_i.IdPrimita)).ToList()
        Dim k_linker As New EfReceptionLinker(k_cn, k_tx, _say)
        Dim k_proposals = k_linker.Propose(k_fresh, k_report, k_plan)
        If k_write AndAlso k_proposals.Count > 0 AndAlso Not k_report.HasBlocking Then k_linker.Write(k_proposals, k_report, k_plan)
        Advance(k_total, "EF_PrimiteReceptii")
    End Sub

    Private Shared Function LoadTargetIntKeys(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_table As String, k_column As String) As HashSet(Of Integer)
        Dim k_keys As New HashSet(Of Integer)()
        Using k_cmd = k_cn.CreateCommand()
            k_cmd.Transaction = k_tx
            k_cmd.CommandText = "SELECT " & TargetServer.Quote(k_column) & " FROM " & TargetServer.Quote(k_table)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_keys.Add(k_reader.GetInt32(0))
                End While
            End Using
        End Using
        Return k_keys
    End Function

    Private Function ReceivedChild(k_source As String, k_target As String, k_key As String, k_keySource As String) As EfTableSpec
        Dim s As New EfTableSpec(k_source, k_target, k_key, k_keySource)
        s.RowCheck = Function(r As AccessTableReader) As EfRowDecision
                         Dim parent = ToInt(r.ValueOrMissing("IDEFT"))
                         Return If(parent.HasValue AndAlso _receivedIds.Contains(parent.Value), EfRowDecision.Keep, EfRowDecision.LeaveOut)
                     End Function
        Return s
    End Function

    Private Function ReceivedLineSpec() As EfTableSpec
        Dim s = ReceivedChild("EFS", "EF_PrimiteLinii", "IdLinie", "IDEFS")
        s.Columns.Add(Col("IdLinie", "IDEFS", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdPrimita", "IDEFT", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("NrLinie", "ID", EfKind.Text, 16))
        s.Columns.Add(Col("Denumire", "Denumire", EfKind.Text, 500))
        s.Columns.Add(Col("Explicatie", "Explicatie", EfKind.Text, 65535))
        s.Columns.Add(Col("Unit", "UNIT", EfKind.Text, 16))
        s.Columns.Add(Col("Cant", "Cant", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Pret", "Pret", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=4, k_default:=0D))
        Return s
    End Function

    ''' <summary>
    ''' Slice 00EF-27. EFT_O: the link between a received invoice and the accounting operations it was booked on (what tells the operator the invoice
    ''' was processed). Same rule as the seven FX_ tables (slice 0080-01, <c>PYTHON/utils/clsf_pair.py</c>): the Access <c>IdClsf</c> is the
    ''' Access id of a classification; it is resolved on the row's own <c>IdUnitate</c> to <c>Clasificatii.IDClsf</c> through
    ''' (<c>IdClsfAcc</c>, <c>IdUnitate</c>) and the Access id is NOT kept. 0 / NULL = no classification = NULL. A pair that matches no row
    ''' or more than one is BLOCKING (a prior test: nothing is written). The rows follow their invoice by key and are NOT filtered by
    ''' IdUnitate (one unit has an IdUnitate per source, 02A / 02E, and both belong to the invoice).
    ''' </summary>
    Private Function ReceivedOperationSpec() As EfTableSpec
        Dim s = ReceivedChild("EFT_O", "EF_PrimiteOperatii", "IdOperatiePrimita", "IDEFTO")
        s.ExtraSourceColumns.Add("IdClsf")
        s.ExtraSourceColumns.Add("IdUnitate")
        s.Columns.Add(Col("IdOperatiePrimita", "IDEFTO", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdPrimita", "IDEFT", EfKind.Whole, k_required:=True))
        ' sql/00EF_66: IdOperatie is the key of the operation (TZ_Oper.IdOper), set after the copy; the number Access held goes to IdOperatieAccess.
        s.Columns.Add(New EfColumn("IdOperatie", Nothing, EfKind.Whole) With {.Compute = Function(r As AccessTableReader) As Object
                                                                                  Return 0
                                                                              End Function})
        s.Columns.Add(Col("IdOperatieAccess", "IdOperatie", EfKind.Whole, k_default:=0))
        s.Columns.Add(New EfColumn("IdClsf", Nothing, EfKind.Whole) With {
            .Compute = Function(r As AccessTableReader) As Object
                           Dim k_resolved = ResolveClassification(ToInt(r.ValueOrMissing("IdClsf")), RowUnit(r, False), DescribeRow(r))
                           Return If(k_resolved.HasValue, CType(k_resolved.Value, Object), CType(DBNull.Value, Object))
                       End Function})
        ' slice ME01-34: an EFT_O row without a unit gets the unit of its source (EFT_O.Sursa = cale.accdb Sursa of exactly one ticked unit)
        s.Columns.Add(New EfColumn("IdUnitate", Nothing, EfKind.Whole) With {
            .Compute = Function(r As AccessTableReader) As Object
                           Dim k_unit = RowUnit(r, True)
                           Return If(k_unit.HasValue, CType(k_unit.Value, Object), CType(DBNull.Value, Object))
                       End Function})
        s.Columns.Add(Col("Sursa", "Sursa", EfKind.Text, 255))
        s.Columns.Add(Col("Suma", "Suma", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        Return s
    End Function

    ''' <summary>
    ''' Slice ME01-34: the unit of an EFT_O row. Its own <c>IdUnitate</c> when it has one; otherwise the unit whose source (<c>cale.accdb</c> Sursa) equals the row's <c>Sursa</c>, when
    ''' exactly ONE ticked unit has that source (counted in <c>_unitsFromSursa</c> so the report says it). Otherwise Nothing: the row stays without a unit.
    ''' </summary>
    Private Function RowUnit(k_row As AccessTableReader, k_count As Boolean) As Integer?
        Dim k_own = ToInt(k_row.ValueOrMissing("IdUnitate"))
        If k_own.HasValue AndAlso k_own.Value <> 0 Then Return k_own
        Return UnitFromSursa(AccessText(k_row.ValueOrMissing("Sursa")), k_count)
    End Function

    Private Function UnitFromSursa(k_sursa As String, k_count As Boolean) As Integer?
        If k_sursa.Length = 0 Then Return Nothing
        Dim k_units As List(Of Integer) = Nothing
        If Not _options.UnitBySursa.TryGetValue(k_sursa, k_units) OrElse k_units.Count <> 1 Then Return Nothing
        If k_count Then
            Dim k_rows As Integer = 0
            _unitsFromSursa.TryGetValue(k_sursa, k_rows)
            _unitsFromSursa(k_sursa) = k_rows + 1
        End If
        Return k_units(0)
    End Function

    ''' <summary>Slice ME01-34: which EFT_O row is meant, in the words of the Access file: its key IDEFTO, its invoice IDEFT, the operation, the source and the amount.</summary>
    Private Shared Function DescribeRow(k_row As AccessTableReader) As String
        Return $"IDEFTO {AccessText(k_row.ValueOrMissing("IDEFTO"))} (factura IDEFT {AccessText(k_row.ValueOrMissing("IDEFT"))}, operația Access {AccessText(k_row.ValueOrMissing("IdOperatie"))}, " &
               $"Sursa «{AccessText(k_row.ValueOrMissing("Sursa"))}», suma {AccessText(k_row.ValueOrMissing("Suma"))})"
    End Function

    ''' <summary>The target's Clasificatii as (IdClsfAcc, IdUnitate) ▸ every IDClsf with that pair (more than one is a problem).</summary>
    Private Function LoadClassificationPairs(k_cn As MySqlConnection, k_tx As MySqlTransaction) As Dictionary(Of (Integer, Integer), List(Of Integer))
        Dim k_pairs As New Dictionary(Of (Integer, Integer), List(Of Integer))()
        If k_cn Is Nothing OrElse Not TableExists(k_cn, "Clasificatii", k_tx) Then Return k_pairs
        Using k_cmd As New MySqlCommand("SELECT `IDClsf`, `IdClsfAcc`, `IdUnitate` FROM `Clasificatii`", k_cn, k_tx)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(2) Then Continue While
                    Dim k_key = (k_reader.GetInt32(1), k_reader.GetInt32(2))
                    Dim k_list As List(Of Integer) = Nothing
                    If Not k_pairs.TryGetValue(k_key, k_list) Then
                        k_list = New List(Of Integer)()
                        k_pairs(k_key) = k_list
                    End If
                    k_list.Add(k_reader.GetInt32(0))
                End While
            End Using
        End Using
        Return k_pairs
    End Function

    ''' <summary>
    ''' The <c>Clasificatii.IDClsf</c> of an Access classification id on a unit; Nothing for 0 / NULL (no classification) and for a
    ''' pair that does not resolve to exactly one row (recorded in <c>_classificationProblems</c>; the run is then stopped).
    ''' </summary>
    Private Function ResolveClassification(k_accessId As Integer?, k_unit As Integer?, k_where As String) As Integer?
        If Not k_accessId.HasValue OrElse k_accessId.Value = 0 Then Return Nothing
        Dim k_found As List(Of Integer) = Nothing
        Dim k_matches As Integer = 0
        If k_unit.HasValue AndAlso k_unit.Value <> 0 AndAlso
           _classificationPairs.TryGetValue((k_accessId.Value, k_unit.Value), k_found) Then k_matches = k_found.Count
        If k_matches = 1 Then Return k_found(0)
        ' Slice ME01-34: no pair of (IdClsfAcc, IdUnitate) -- usually because the EFT_O row has no unit -- but the code of the Access classification (Clsf) names exactly one row
        Dim k_unitValue As Integer = If(k_unit.HasValue, k_unit.Value, 0)
        Dim k_viaCode As Integer? = ResolveByCode(k_accessId.Value, k_unitValue)
        If k_viaCode.HasValue Then
            Dim k_used As (IdClsf As Integer, Rows As Integer) = (k_viaCode.Value, 0)
            _classificationsByCodeUsed.TryGetValue((k_accessId.Value, k_unitValue), k_used)
            _classificationsByCodeUsed((k_accessId.Value, k_unitValue)) = (k_viaCode.Value, k_used.Rows + 1)
            Return k_viaCode
        End If
        If _classificationsPlanned.ContainsKey((k_accessId.Value, k_unitValue)) Then
            ' verification only: the classification will be created by the write pass, so the row is not a problem (its key is not needed to verify)
            Dim k_plan = _classificationsPlanned((k_accessId.Value, k_unitValue))
            _classificationsPlanned((k_accessId.Value, k_unitValue)) = (k_plan.Unit, k_plan.Code, k_plan.Denumire, k_plan.Rows + 1)
            Return Nothing
        End If
        Dim k_problemKey = (k_accessId.Value, k_unitValue)
        Dim k_rowsOf As List(Of String) = Nothing
        If Not _classificationProblemRows.TryGetValue(k_problemKey, k_rowsOf) Then
            k_rowsOf = New List(Of String)()
            _classificationProblemRows(k_problemKey) = k_rowsOf
        End If
        If k_rowsOf.Count < 10 Then k_rowsOf.Add(k_where)
        Dim k_known As (Matches As Integer, Rows As Integer) = (0, 0)
        _classificationProblems.TryGetValue(k_problemKey, k_known)
        _classificationProblems(k_problemKey) = (k_matches, k_known.Rows + 1)
        Return Nothing
    End Function

    ''' <summary>
    ''' One BLOCKING finding per (Access id, unit) that did not resolve to exactly one Clasificatii row, as the FX tables do: a booking
    ''' without its classification is worse than a refusal. Clasificatii is moved by the main transfer, which must have run first.
    ''' </summary>
    Private Sub ReportClassificationProblems(k_report As EfImportReport)
        For Each k_pair In _unitsFromSursa.OrderBy(Function(p) p.Key)
            k_report.Add(EfSeverity.Info, "EF_PrimiteOperatii", $"{k_pair.Value} rânduri EFT_O fără unitate au primit unitatea {_options.UnitBySursa(k_pair.Key)(0)} după Sursa «{k_pair.Key}» (cale.accdb).")
        Next
        ReportClassificationsByCode(k_report)
        ReportClassificationsCreated(k_report)
        If _classificationProblems.Count = 0 Then Return
        Dim k_area As String = "EF_PrimiteOperatii"
        If _classificationPairs.Count = 0 Then
            k_report.Add(EfSeverity.Blocking, k_area,
                $"Tabelul Clasificatii nu are rânduri în baza «{_options.UnitDatabase}»: transferați întâi nomenclatorul de clasificații, apoi E-Factura.")
        End If
        ' Slice ME01-34: say WHICH classification each Access id is (its code and name from the unit file) and propose the MariaDB row with that code
        For Each k_pair In _classificationProblems.OrderBy(Function(p) p.Key.Item2).ThenBy(Function(p) p.Key.Item1)
            Dim k_unit As String = If(k_pair.Key.Item2 = 0, "fără unitate", $"unitatea {k_pair.Key.Item2}")
            Dim k_text As String =
                $"clasificația Access {k_pair.Key.Item1}, {k_unit}: {k_pair.Value.Matches} potriviri în Clasificatii (IdClsfAcc + IdUnitate) " &
                $"în loc de exact una; {k_pair.Value.Rows} rânduri EFT_O. Nimic nu s-a scris."
            Dim k_entries As List(Of AccessClsf) = Nothing
            Dim k_why As String = Nothing
            If _classificationNotCreated.TryGetValue(k_pair.Key, k_why) Then k_text &= " Nu s-a putut crea din Access: " & k_why
            If Not _accessClassifications.TryGetValue(k_pair.Key.Item1, k_entries) Then
                k_text &= $" Id-ul nu există în tabelul Clasificatii din niciunul din cele {_classificationFilesRead} fișiere de unitate citite din Access, deci codul lui nu se poate afla."
            ElseIf k_entries.Select(Function(k_e) ClsfKey(k_e.Code)).Distinct().Count() > 1 Then
                k_text &= " În Access același id este altă clasificație în fișiere de unități diferite: " &
                    String.Join("; ", k_entries.Select(Function(k_e) $"unitatea {k_e.IdUnitate}: «{k_e.Code}» ({k_e.Denumire})")) & ". Nu se poate propune nimic."
            Else
                Dim k_info As AccessClsf = k_entries(0)
                k_text &= $" În Access este «{k_info.Code}» ({k_info.Denumire})." & ClassificationProposal(k_info.Code, k_info.Denumire, k_pair.Key.Item2)
            End If
            Dim k_rowsText As List(Of String) = Nothing
            If _classificationProblemRows.TryGetValue(k_pair.Key, k_rowsText) AndAlso k_rowsText.Count > 0 Then
                k_text &= " Rânduri EFT_O: " & String.Join("; ", k_rowsText) & If(k_pair.Value.Rows > k_rowsText.Count, $"; … și încă {k_pair.Value.Rows - k_rowsText.Count}.", ".")
            End If
            k_report.AddLimited(EfSeverity.Blocking, k_area, "clasificație nerezolvată", k_text)
        Next
    End Sub

    ''' <summary>Slice ME01-34: the classifications created from Access (write pass) or that will be (verification).</summary>
    Private Sub ReportClassificationsCreated(k_report As EfImportReport)
        For Each k_pair In _classificationsCreated.OrderBy(Function(p) p.Key.Item2).ThenBy(Function(p) p.Key.Item1)
            k_report.AddLimited(EfSeverity.Warning, "EF_PrimiteOperatii", "clasificație creată din Access",
                $"clasificația Access {k_pair.Key.Item1} lipsea din Clasificatii și a fost creată din Access: Clsf «{k_pair.Value.Code}» ({k_pair.Value.Denumire}), unitatea {k_pair.Value.Unit}, " &
                $"IDClsf {k_pair.Value.IdClsf}; {k_pair.Value.Rows} rânduri EFT_O.")
        Next
        For Each k_pair In _classificationsPlanned.OrderBy(Function(p) p.Key.Item2).ThenBy(Function(p) p.Key.Item1)
            k_report.AddLimited(EfSeverity.Warning, "EF_PrimiteOperatii", "clasificație de creat din Access",
                $"clasificația Access {k_pair.Key.Item1} lipsește din Clasificatii și SE VA CREA din Access la scriere: Clsf «{k_pair.Value.Code}» ({k_pair.Value.Denumire}), unitatea {k_pair.Value.Unit}; " &
                $"{k_pair.Value.Rows} rânduri EFT_O.")
        Next
    End Sub

    ''' <summary>
    ''' Slice ME01-34: BEFORE the EFT_O rows are copied, finds the Access classifications that those rows (of the invoices kept) name and that no <c>Clasificatii</c> row of the
    ''' target answers to -- neither by (IdClsfAcc, IdUnitate) nor by code -- and, when Access knows them, creates them in <c>Clasificatii</c> (write pass) or announces them
    ''' (verification). Done as a pass of its own so no INSERT happens while the rows are being read. The unit is the row's own, or the single unit whose Access file holds
    ''' the id; a row without a unit whose id sits in several units' files is NOT created (it would be a guess). Nothing is created when the code is already in that unit, the
    ''' unit is not in <c>Unitati</c>, the id has different codes in different files, or the code has no known sector; MariaDB refusing the row (a missing key in
    ''' AVACONT_COMUN) is logged and shown on the blocking line of the id.
    ''' </summary>
    Private Sub PrepareClassifications(k_source As OleDbConnection, k_write As Boolean, k_report As EfImportReport, k_cn As MySqlConnection, k_tx As MySqlTransaction)
        Dim k_table As String = AccessSchema.ResolveTableName(k_source, "EFT_O")
        If k_table Is Nothing OrElse k_cn Is Nothing Then Return
        Dim k_wanted As New Dictionary(Of (Integer, Integer), Integer)()
        Using k_reader = AccessSchema.OpenKeyReader(k_source, k_table, New String() {"IDEFT", "IdClsf", "IdUnitate", "Sursa"})
            While k_reader.Read()
                Dim k_parent = ToInt(k_reader.ValueOrMissing("IDEFT"))
                If Not k_parent.HasValue OrElse Not _receivedIds.Contains(k_parent.Value) Then Continue While
                Dim k_id = ToInt(k_reader.ValueOrMissing("IdClsf"))
                If Not k_id.HasValue OrElse k_id.Value = 0 Then Continue While
                Dim k_unit = ToInt(k_reader.ValueOrMissing("IdUnitate"))
                If Not k_unit.HasValue OrElse k_unit.Value = 0 Then k_unit = UnitFromSursa(AccessText(k_reader.ValueOrMissing("Sursa")), False)
                Dim k_key = (k_id.Value, If(k_unit.HasValue, k_unit.Value, 0))
                Dim k_rows As Integer = 0
                k_wanted.TryGetValue(k_key, k_rows)
                k_wanted(k_key) = k_rows + 1
            End While
        End Using
        For Each k_item In k_wanted.OrderBy(Function(p) p.Key.Item2).ThenBy(Function(p) p.Key.Item1)
            Dim k_id As Integer = k_item.Key.Item1
            Dim k_unit As Integer = k_item.Key.Item2
            Dim k_found As List(Of Integer) = Nothing
            If k_unit <> 0 AndAlso _classificationPairs.TryGetValue((k_id, k_unit), k_found) AndAlso k_found.Count >= 1 Then Continue For   ' answered (one row) or a duplicate: not for creation
            If ResolveByCode(k_id, k_unit).HasValue Then Continue For
            CreateClassification(k_id, k_unit, k_item.Value, k_write, k_cn, k_tx)
        Next
    End Sub

    Private Sub CreateClassification(k_accessId As Integer, k_unit As Integer, k_rows As Integer, k_write As Boolean, k_cn As MySqlConnection, k_tx As MySqlTransaction)
        Dim k_key = (k_accessId, k_unit)
        Dim k_entries As List(Of AccessClsf) = Nothing
        If Not _accessClassifications.TryGetValue(k_accessId, k_entries) Then Return   ' Access does not know it: the blocking line says so
        If k_entries.Select(Function(k_e) ClsfKey(k_e.Code)).Distinct().Count() > 1 Then
            _classificationNotCreated(k_key) = "id-ul are coduri diferite în fișierele unităților."
            Return
        End If
        Dim k_target As Integer = k_unit
        If k_target = 0 Then
            Dim k_units = k_entries.Select(Function(k_e) k_e.IdUnitate).Where(Function(k_u) k_u <> 0).Distinct().ToList()
            If k_units.Count <> 1 Then
                _classificationNotCreated(k_key) = If(k_units.Count = 0, "nu se știe unitatea (rândul nu are unitate, iar fișierul citit nu aparține unei unități).",
                                                      $"rândul nu are unitate, iar id-ul este în fișierele a {k_units.Count} unități ({String.Join(", ", k_units)}): unitatea ar fi o presupunere.")
                Return
            End If
            k_target = k_units(0)
        End If
        Dim k_first As AccessClsf = k_entries(0)
        Dim k_all As List(Of ClsfCandidate) = Nothing
        If _classificationsByCode.TryGetValue(ClsfKey(k_first.Code), k_all) AndAlso k_all.Any(Function(k_c) k_c.IdUnitate.HasValue AndAlso k_c.IdUnitate.Value = k_target) Then
            _classificationNotCreated(k_key) = $"unitatea {k_target} are deja în Clasificatii un rând cu Clsf «{k_first.Code}» (nu se face dublură)."
            Return
        End If
        Dim k_derived As New ClasificatieDerived(k_first.Capitol, k_first.Subcapitol, k_first.Articol, k_first.Alineat, k_first.Sursa)
        If k_derived.Sector.Length = 0 Then
            _classificationNotCreated(k_key) = $"capitolul «{k_first.Capitol}» nu are un sector cunoscut."
            Return
        End If
        Using k_check As New MySqlCommand("SELECT COUNT(*) FROM `Unitati` WHERE `IdUnitate` = @u", k_cn, k_tx)
            k_check.Parameters.AddWithValue("@u", k_target)
            If Convert.ToInt64(k_check.ExecuteScalar(), CultureInfo.InvariantCulture) = 0 Then
                _classificationNotCreated(k_key) = $"unitatea {k_target} nu există în Unitati."
                Return
            End If
        End Using
        Dim k_name As String = If(k_first.Denumire.Length > 255, k_first.Denumire.Substring(0, 255), k_first.Denumire)
        If Not k_write Then
            _classificationsPlanned(k_key) = (k_target, k_first.Code, k_name, 0)
            Return
        End If
        Try
            Using k_cmd As New MySqlCommand("INSERT INTO `Clasificatii` (`IdClsfAcc`, `IdUnitate`, `Capitol`, `Subcapitol`, `Articol`, `Alineat`, `Denumire`, `Sector`, `Sursa`, `SS`) " &
                                            "VALUES (@acc, @u, @c, @s, @a, @al, @d, @sec, @sur, @ss)", k_cn, k_tx)
                k_cmd.Parameters.AddWithValue("@acc", k_accessId)
                k_cmd.Parameters.AddWithValue("@u", k_target)
                k_cmd.Parameters.AddWithValue("@c", k_derived.Capitol)
                k_cmd.Parameters.AddWithValue("@s", k_derived.Subcapitol)
                k_cmd.Parameters.AddWithValue("@a", k_derived.Articol)
                k_cmd.Parameters.AddWithValue("@al", k_derived.Alineat)
                k_cmd.Parameters.AddWithValue("@d", k_name)
                k_cmd.Parameters.AddWithValue("@sec", k_derived.Sector)
                k_cmd.Parameters.AddWithValue("@sur", k_derived.Sursa)
                k_cmd.Parameters.AddWithValue("@ss", k_derived.SS)
                k_cmd.ExecuteNonQuery()
                Dim k_newId As Integer = CInt(k_cmd.LastInsertedId)
                ' the new row answers to its pair and to its code from now on, so the EFT_O rows (with or without a unit) resolve through the normal path
                _classificationPairs((k_accessId, k_target)) = New List(Of Integer) From {k_newId}
                If k_all Is Nothing Then
                    k_all = New List(Of ClsfCandidate)()
                    _classificationsByCode(ClsfKey(k_first.Code)) = k_all
                End If
                k_all.Add(New ClsfCandidate(k_newId, k_accessId, k_target, k_name))
                _classificationsCreated(k_key) = (k_newId, k_target, k_first.Code, k_name, k_rows)
            End Using
        Catch ex As MySqlException
            ' a statement the server refuses (a key missing in AVACONT_COMUN, a column too long) leaves the transaction usable: logged, shown on the blocking line of the id
            GlobalErrorLog.Write("EfImporter.CreateClassification", ex)
            _classificationNotCreated(k_key) = "MariaDB a refuzat rândul: " & ex.Message
        End Try
    End Sub

    ''' <summary>Slice ME01-34: the Access ids that were tied to a Clasificatii row by their code, so the operator sees what was decided for him.</summary>
    Private Sub ReportClassificationsByCode(k_report As EfImportReport)
        For Each k_pair In _classificationsByCodeUsed.OrderBy(Function(p) p.Key.Item2).ThenBy(Function(p) p.Key.Item1)
            Dim k_unit As String = If(k_pair.Key.Item2 = 0, "fără unitate", $"unitatea {k_pair.Key.Item2}")
            Dim k_entries As List(Of AccessClsf) = Nothing
            Dim k_code As String = If(_accessClassifications.TryGetValue(k_pair.Key.Item1, k_entries), k_entries(0).Code, "?")
            k_report.AddLimited(EfSeverity.Info, "EF_PrimiteOperatii", "clasificație găsită după Clsf",
                $"clasificația Access {k_pair.Key.Item1}, {k_unit}: nu are pereche unică (IdClsfAcc + IdUnitate) în Clasificatii; s-a legat după Clsf «{k_code}» de IDClsf {k_pair.Value.IdClsf} " &
                $"({k_pair.Value.Rows} rânduri EFT_O).")
        Next
    End Sub

    ''' <summary>One <c>Clasificatii</c> row of the target, as the proposal for an unresolved Access id needs it (slice ME01-34).</summary>
    Private NotInheritable Class ClsfCandidate
        Public ReadOnly IdClsf As Integer
        Public ReadOnly IdClsfAcc As Integer
        Public ReadOnly IdUnitate As Integer?
        Public ReadOnly Denumire As String

        Public Sub New(k_idClsf As Integer, k_idClsfAcc As Integer, k_idUnitate As Integer?, k_denumire As String)
            IdClsf = k_idClsf
            IdClsfAcc = k_idClsfAcc
            IdUnitate = k_idUnitate
            Denumire = k_denumire
        End Sub

        Public Function Describe() As String
            Return $"IDClsf {IdClsf} (unitatea {If(IdUnitate.HasValue, IdUnitate.Value.ToString(CultureInfo.InvariantCulture), "-")}, IdClsfAcc {IdClsfAcc})"
        End Function
    End Class

    ''' <summary>Same normalisation on both sides of the comparison between an Access code and <c>Clasificatii.Clsf</c>.</summary>
    Private Shared Function ClsfKey(k_code As String) As String
        Return k_code.Trim().ToUpperInvariant()
    End Function

    Private Shared Function AccessText(k_value As Object) As String
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim()
    End Function

    ''' <summary>The target's Clasificatii by <c>Clsf</c> (Capitol.Subcapitol.Articol.Alineat), every row with that code.</summary>
    Private Function LoadClassificationCodes(k_cn As MySqlConnection, k_tx As MySqlTransaction) As Dictionary(Of String, List(Of ClsfCandidate))
        Dim k_codes As New Dictionary(Of String, List(Of ClsfCandidate))(StringComparer.Ordinal)
        If k_cn Is Nothing OrElse Not TableExists(k_cn, "Clasificatii", k_tx) Then Return k_codes
        Using k_cmd As New MySqlCommand("SELECT `IDClsf`, `IdClsfAcc`, `IdUnitate`, `Clsf`, `Denumire` FROM `Clasificatii`", k_cn, k_tx)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(3) Then Continue While
                    Dim k_key As String = ClsfKey(k_reader.GetString(3))
                    Dim k_list As List(Of ClsfCandidate) = Nothing
                    If Not k_codes.TryGetValue(k_key, k_list) Then
                        k_list = New List(Of ClsfCandidate)()
                        k_codes(k_key) = k_list
                    End If
                    k_list.Add(New ClsfCandidate(k_reader.GetInt32(0), k_reader.GetInt32(1),
                                                 If(k_reader.IsDBNull(2), CType(Nothing, Integer?), k_reader.GetInt32(2)),
                                                 If(k_reader.IsDBNull(4), String.Empty, k_reader.GetString(4))))
                End While
            End Using
        End Using
        Return k_codes
    End Function

    ''' <summary>One Access <c>Clasificatii</c> row: its code and name, the unit whose file holds it (the Access table has no unit column: its ids belong to that file).</summary>
    Private NotInheritable Class AccessClsf
        Public ReadOnly Capitol As String
        Public ReadOnly Subcapitol As String
        Public ReadOnly Articol As String
        Public ReadOnly Alineat As String
        Public ReadOnly Sursa As String
        Public ReadOnly Denumire As String
        Public ReadOnly IdUnitate As Integer

        Public Sub New(k_capitol As String, k_subcapitol As String, k_articol As String, k_alineat As String, k_sursa As String, k_denumire As String, k_idUnitate As Integer)
            Capitol = k_capitol
            Subcapitol = k_subcapitol
            Articol = k_articol
            Alineat = k_alineat
            Sursa = k_sursa
            Denumire = k_denumire
            IdUnitate = k_idUnitate
        End Sub

        ''' <summary>The code as <c>Clasificatii.Clsf</c> builds it: <c>concat_ws('.', Capitol, Subcapitol, Articol, Alineat)</c>.</summary>
        Public ReadOnly Property Code As String
            Get
                Return String.Join(".", Capitol, Subcapitol, Articol, Alineat)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Every Access classification of the unit files given in the options (<see cref="EfImportOptions.ClassificationFiles"/>; the unit file of the issuer when none is
    ''' given), by Access id: its code (Capitol.Subcapitol.Articol.Alineat, as <c>Clasificatii.Clsf</c> builds it), its name and the unit of the file. The same id can be
    ''' a different classification in two files, so an id keeps one entry per file. A file that cannot be read is logged and shown as a warning (the id is then
    ''' simply not found there), so the findings that need this still come out.
    ''' </summary>
    Private Function LoadAccessClassifications(k_report As EfImportReport) As Dictionary(Of Integer, List(Of AccessClsf))
        Dim k_found As New Dictionary(Of Integer, List(Of AccessClsf))()
        Dim k_files As New List(Of (IdUnitate As Integer, FilePath As String))(_options.ClassificationFiles)
        If k_files.Count = 0 AndAlso Not String.IsNullOrWhiteSpace(_options.IssuedFile) Then k_files.Add((0, _options.IssuedFile))
        _classificationFilesRead = 0
        For Each k_file In k_files
            If Not File.Exists(k_file.FilePath) Then Continue For
            Try
                Using k_cn = AccessProvider.Open(k_file.FilePath, _options.AccessPassword)
                    Dim k_table As String = AccessSchema.ResolveTableName(k_cn, "Clasificatii")
                    If k_table Is Nothing Then
                        k_report.Add(EfSeverity.Warning, "EF_PrimiteOperatii", $"Fișierul «{Path.GetFileName(k_file.FilePath)}» nu are tabelul «Clasificatii».")
                        Continue For
                    End If
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                        While k_reader.Read()
                            Dim k_id = ToInt(k_reader.ValueOrMissing("IDClsf"))
                            If Not k_id.HasValue OrElse k_id.Value = 0 Then Continue While
                            Dim k_list As List(Of AccessClsf) = Nothing
                            If Not k_found.TryGetValue(k_id.Value, k_list) Then
                                k_list = New List(Of AccessClsf)()
                                k_found(k_id.Value) = k_list
                            End If
                            k_list.Add(New AccessClsf(AccessText(k_reader.ValueOrMissing("Capitol")), AccessText(k_reader.ValueOrMissing("Subcapitol")),
                                                      AccessText(k_reader.ValueOrMissing("Articol")), AccessText(k_reader.ValueOrMissing("Alineat")),
                                                      AccessText(k_reader.ValueOrMissing("Sursa")), AccessText(k_reader.ValueOrMissing("Denumire")), k_file.IdUnitate))
                        End While
                    End Using
                End Using
                _classificationFilesRead += 1
            Catch ex As Exception
                GlobalErrorLog.Write("EfImporter.LoadAccessClassifications", ex)
                k_report.Add(EfSeverity.Warning, "EF_PrimiteOperatii", $"Clasificațiile din «{Path.GetFileName(k_file.FilePath)}» nu s-au putut citi: {ex.Message}")
            End Try
        Next
        Return k_found
    End Function

    ''' <summary>
    ''' The <c>Clasificatii.IDClsf</c> of an Access id found by its CODE: the Access code of the id (all unit files agree on it) = <c>Clasificatii.Clsf</c>, and exactly one
    ''' MariaDB row has it (on the row's unit when it has one; without a unit, among the units whose Access file holds the id when there are several). Nothing otherwise.
    ''' </summary>
    Private Function ResolveByCode(k_accessId As Integer, k_unit As Integer) As Integer?
        Dim k_entries As List(Of AccessClsf) = Nothing
        If Not _accessClassifications.TryGetValue(k_accessId, k_entries) Then Return Nothing
        Dim k_codes = k_entries.Select(Function(k_e) ClsfKey(k_e.Code)).Distinct().ToList()
        If k_codes.Count <> 1 Then Return Nothing   ' the same Access id is another classification in another unit file
        Dim k_all As List(Of ClsfCandidate) = Nothing
        If Not _classificationsByCode.TryGetValue(k_codes(0), k_all) Then Return Nothing
        Dim k_pool As List(Of ClsfCandidate) = k_all
        If k_unit <> 0 Then
            k_pool = k_all.Where(Function(k_c) k_c.IdUnitate.HasValue AndAlso k_c.IdUnitate.Value = k_unit).ToList()
        ElseIf k_pool.Count > 1 Then
            Dim k_units As New HashSet(Of Integer)(k_entries.Select(Function(k_e) k_e.IdUnitate))
            k_pool = k_all.Where(Function(k_c) k_c.IdUnitate.HasValue AndAlso k_units.Contains(k_c.IdUnitate.Value)).ToList()
        End If
        Return If(k_pool.Count = 1, CType(k_pool(0).IdClsf, Integer?), Nothing)
    End Function

    ''' <summary>
    ''' What to do about an Access classification that did not resolve: the MariaDB row with the same code (<c>Clsf</c>) when there is exactly one (on the row's unit when
    ''' it has one), the list when there are several, and creating it from Access (the main transfer of KBot.Migrator, which moves <c>Clasificatii</c>) when there is none.
    ''' This only runs for what <see cref="ResolveByCode"/> could NOT tie (it ties the single match by itself): several candidates, none, or a code that Access does not give.
    ''' </summary>
    Private Function ClassificationProposal(k_code As String, k_name As String, k_unit As Integer) As String
        Dim k_all As List(Of ClsfCandidate) = Nothing
        If Not _classificationsByCode.TryGetValue(ClsfKey(k_code), k_all) OrElse k_all.Count = 0 Then
            Return $" Propunere: în MariaDB nu există nicio clasificație cu Clsf «{k_code}»; se creează din Access (denumire «{k_name}») prin transferul nomenclatorului Clasificatii din KBot.Migrator, apoi se rulează din nou."
        End If
        Dim k_pool As List(Of ClsfCandidate) = k_all
        If k_unit <> 0 Then k_pool = k_all.Where(Function(k_c) k_c.IdUnitate.HasValue AndAlso k_c.IdUnitate.Value = k_unit).ToList()
        If k_pool.Count = 1 Then
            Return $" Propunere: în MariaDB, Clsf «{k_code}» este {k_pool(0).Describe()}, «{k_pool(0).Denumire}»; singura cu acest cod" & If(k_unit <> 0, $" la unitatea {k_unit}.", ".")
        End If
        If k_pool.Count = 0 Then
            Return $" Propunere: Clsf «{k_code}» nu există la unitatea {k_unit}, ci la: {String.Join("; ", k_all.Take(6).Select(Function(k_c) k_c.Describe()))}; sau se creează din Access (denumire «{k_name}») prin transferul nomenclatorului Clasificatii din KBot.Migrator."
        End If
        Return $" Nu există o propunere unică: {k_pool.Count} clasificații în MariaDB au Clsf «{k_code}»: {String.Join("; ", k_pool.Take(6).Select(Function(k_c) k_c.Describe()))}."
    End Function

    Private Function ReceivedNoteSpec() As EfTableSpec
        Dim s = ReceivedChild("EFT_C", "EF_PrimiteNote", "IdNota", "IDEFTC")
        s.Columns.Add(Col("IdNota", "IDEFTC", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdPrimita", "IDEFT", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("Nota", "Note", EfKind.Text, 65535))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        Return s
    End Function

    Private Function ReceivedMessageSpec() As EfTableSpec
        Dim s = ReceivedChild("EFT_M", "EF_PrimiteMesaje", "IdMsg", "IDMSG")
        s.Columns.Add(Col("IdMsg", "IDMSG", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdPrimita", "IDEFT", EfKind.Whole, k_required:=True))
        s.Columns.Add(Col("IdSol", "id_sol", EfKind.Text, 32))
        s.Columns.Add(Col("IdMesajAnaf", "ID", EfKind.Text, 32))
        s.Columns.Add(Col("Mesaj", "Mesaj", EfKind.Text, 65535))
        s.Columns.Add(Col("DataMesaj", "Data", EfKind.DateAndTime))
        s.Columns.Add(Col("DataAdaugare", "DTQ", EfKind.DateAndTime))
        Return s
    End Function

    ' ---- the links of the invoices to their operations (EFT_O) ---------------------------------------------------

    ''' <summary>
    ''' EFT_O is rewritten COMPLETE on every run: the links that came from Access (IdOperatieAccess filled) are deleted and read again, so an operation deleted in Access
    ''' leaves no link behind. Links made in K-BOT (IdOperatieAccess empty) stay. Needs sql/00EF_66.
    ''' </summary>
    Private Sub ReportLinkRewrite(k_write As Boolean, k_report As EfImportReport, k_cn As MySqlConnection, k_tx As MySqlTransaction)
        If k_cn Is Nothing OrElse Not TableExists(k_cn, "EF_PrimiteOperatii", k_tx) Then Return
        If Not ColumnExists(k_cn, "EF_PrimiteOperatii", "IdOperatieAccess", k_tx) Then
            k_report.Add(EfSeverity.Blocking, "EF_PrimiteOperatii", "Lipsește coloana «IdOperatieAccess». Rulați scriptul sql/00EF_66_primite_operatii_id_access.sql și sincronizați schema.")
            Return
        End If
        Dim existing As Long
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM `EF_PrimiteOperatii` WHERE `IdOperatieAccess` IS NOT NULL", k_cn, k_tx)
            existing = Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
        k_report.Add(EfSeverity.Info, "EF_PrimiteOperatii", $"Legăturile din Access ({existing} existente) se rescriu complet; cele făcute în K-BOT rămân.")
        If Not k_write Then Return
        Using cmd As New MySqlCommand("DELETE FROM `EF_PrimiteOperatii` WHERE `IdOperatieAccess` IS NOT NULL", k_cn, k_tx)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' After the copy: every link that has an Access number gets the key of its operation (IdOperatie = TZ_Oper.IdOper; the operation is found by Access number + unit).
    ''' Without the TZ_ tables (operations not migrated yet) the links stay at 0 and the migration of the operations sets them.
    ''' </summary>
    Private Sub PointLinksAtOperations(k_cn As MySqlConnection, k_tx As MySqlTransaction)
        For Each table In {"TZ_Oper", "TZ_Documente", "TZ_NoteC", "TZ_Subunitati"}
            If Not TableExists(k_cn, table, k_tx) Then
                _say("EF_PrimiteOperatii: operațiile contabile nu sunt migrate încă; legăturile se leagă la migrarea operațiilor.")
                Return
            End If
        Next
        Using cmd As New MySqlCommand(
            "UPDATE `EF_PrimiteOperatii` e JOIN (SELECT o.`IdOper`, o.`IdOperatieAccess`, s.`IdUnitateAccess` FROM `TZ_Oper` o " &
            "JOIN `TZ_Documente` d ON d.`IdDocument` = o.`IdDocument` JOIN `TZ_NoteC` n ON n.`IdNota` = d.`IdNota` JOIN `TZ_Subunitati` s ON s.`IdSubunitate` = n.`IdSubunitate` " &
            "WHERE o.`IdOperatieAccess` IS NOT NULL AND s.`IdUnitateAccess` IS NOT NULL) m ON e.`IdOperatieAccess` = m.`IdOperatieAccess` AND e.`IdUnitate` = m.`IdUnitateAccess` " &
            "SET e.`IdOperatie` = m.`IdOper` WHERE e.`IdOperatie` <> m.`IdOper`", k_cn, k_tx)
            _say($"EF_PrimiteOperatii: {cmd.ExecuteNonQuery().ToString(CultureInfo.InvariantCulture)} legături au primit cheia operației (IdOperatie = TZ_Oper.IdOper).")
        End Using
    End Sub

    Private Shared Function ColumnExists(k_cn As MySqlConnection, k_table As String, k_column As String, k_tx As MySqlTransaction) As Boolean
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t AND COLUMN_NAME = @c", k_cn, k_tx)
            cmd.Parameters.AddWithValue("@t", k_table)
            cmd.Parameters.AddWithValue("@c", k_column)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    ' ---- copying one table ------------------------------------------------------------------------------------

    Private Sub CopyTable(k_spec As EfTableSpec, k_source As OleDbConnection, k_write As Boolean,
                          k_report As EfImportReport, k_cn As MySqlConnection, k_tx As MySqlTransaction,
                          k_plans As List(Of EfTablePlan), k_total As Integer, k_token As CancellationToken)
        Dim plan As New EfTablePlan(k_spec.TargetTable, _options.UnitDatabase)
        k_plans.Add(plan)
        Dim area = k_spec.TargetTable

        Dim tableName = AccessSchema.ResolveTableName(k_source, k_spec.SourceTable)
        If tableName Is Nothing Then
            k_report.Add(EfSeverity.Blocking, area, $"Tabelul Access «{k_spec.SourceTable}» lipsește din fișier.")
            Advance(k_total, area)
            Return
        End If

        ' The key sets that the checks need, read before the table itself.
        If String.Equals(k_spec.SourceTable, "Factura", StringComparison.OrdinalIgnoreCase) Then
            _accessClientIds = LoadIntKeys(k_source, "ClientiEF", "IdClient")
        End If
        If String.Equals(k_spec.SourceTable, "FacturaC", StringComparison.OrdinalIgnoreCase) Then
            _accessInvoiceIds = LoadIntKeys(k_source, "Factura", "IdFactura")
        End If

        Dim present = New HashSet(Of String)(AccessSchema.Columns(k_source, tableName).Select(Function(c) c.Name), StringComparer.OrdinalIgnoreCase)
        For Each c In k_spec.Columns
            If c.Source IsNot Nothing AndAlso Not c.OptionalSource AndAlso Not present.Contains(c.Source) Then
                k_report.Add(EfSeverity.Blocking, area, $"În tabelul Access «{tableName}» lipsește coloana «{c.Source}».")
            End If
        Next
        For Each extra In k_spec.ExtraSourceColumns
            If Not present.Contains(extra) Then k_report.Add(EfSeverity.Blocking, area, $"În tabelul Access «{tableName}» lipsește coloana «{extra}».")
        Next
        If k_report.Findings.Any(Function(f) f.Severity = EfSeverity.Blocking AndAlso f.Area = area AndAlso f.Text.Contains("lipsește coloana")) Then
            Advance(k_total, area)
            Return
        End If

        Dim targetExists = k_cn IsNot Nothing AndAlso TableExists(k_cn, k_spec.TargetTable, k_tx)
        If targetExists Then
            plan.RowsBefore = CountRows(k_cn, k_spec.TargetTable, k_tx)
            CheckDecimalStorage(k_spec, k_cn, k_tx, k_report)
        End If

        Dim writer As EfBatchWriter = Nothing
        If k_write AndAlso Not k_report.HasBlocking AndAlso targetExists Then writer = New EfBatchWriter(k_cn, k_tx, k_spec)

        Dim wanted = k_spec.SourceColumnsToRead(k_write).Where(Function(c) present.Contains(c)).ToList()
        ' Count filter matches before conversion: ownership is different from rows ready to write.
        Dim rowsMatchingFilter As Long = 0
        Using reader = AccessSchema.OpenKeyReader(k_source, tableName, wanted)
            While reader.Read()
                k_token.ThrowIfCancellationRequested()
                plan.RowsRead += 1

                If k_spec.RowCheck IsNot Nothing Then
                    Dim decision = k_spec.RowCheck.Invoke(reader)
                    If decision.Kind = EfRowKind.LeaveOut Then
                        plan.RowsLeftOut += 1
                        Continue While
                    End If
                    If decision.Kind = EfRowKind.Block Then
                        k_report.AddLimited(EfSeverity.Blocking, area, "rând respins", decision.Text)
                        Continue While
                    End If
                End If

                rowsMatchingFilter += 1
                Dim values(k_spec.Columns.Count - 1) As Object
                Dim rowOk As Boolean = True
                For i = 0 To k_spec.Columns.Count - 1
                    Dim column = k_spec.Columns(i)
                    If column.Kind = EfKind.Binary AndAlso Not k_write Then
                        values(i) = DBNull.Value
                        Continue For
                    End If
                    Dim rawValue As Object
                    If column.Compute IsNot Nothing Then
                        rawValue = column.Compute.Invoke(reader)
                    ElseIf column.Source Is Nothing Then
                        rawValue = Nothing
                    Else
                        rawValue = reader.ValueOrMissing(column.Source)
                    End If
                    Dim issue = EfConverter.ToTarget(column, rawValue, values(i))
                    If issue.Kind = EfIssueKind.Blocking Then
                        rowOk = False
                        k_report.AddLimited(EfSeverity.Blocking, area, "valoare: " & column.Target,
                            $"{k_spec.KeySource} = {Convert.ToString(reader.ValueOrMissing(k_spec.KeySource), CultureInfo.InvariantCulture)}: {column.Target}: {issue.Text}")
                    ElseIf issue.Kind = EfIssueKind.Warning Then
                        k_report.AddLimited(EfSeverity.Warning, area, "rotunjit: " & column.Target,
                            $"{k_spec.KeySource} = {Convert.ToString(reader.ValueOrMissing(k_spec.KeySource), CultureInfo.InvariantCulture)}: {column.Target}: {issue.Text}")
                    End If
                Next
                If Not rowOk Then Continue While

                plan.RowsSelected += 1
                If k_spec.AfterRow IsNot Nothing Then k_spec.AfterRow.Invoke(values)
                If writer IsNot Nothing Then writer.Add(values)

                If plan.RowsRead Mod 500 = 0 Then _say($"   {area}: citite {plan.RowsRead}, de scris {plan.RowsSelected}…")
            End While
        End Using

        If writer IsNot Nothing Then
            writer.Flush()
            plan.RowsAdded = CountRows(k_cn, k_spec.TargetTable, k_tx) - plan.RowsBefore
            plan.RowsKept = plan.RowsSelected - plan.RowsAdded
        End If
        If Not targetExists Then
            k_report.Add(EfSeverity.Blocking, area,
                $"Tabelul {k_spec.TargetTable} lipsește din baza «{_options.UnitDatabase}». Rulați scriptul " &
                If(String.Equals(k_spec.TargetTable, "EF_PrimiteOperatii", StringComparison.Ordinal),
                   "sql/00EF_27_primite_operatii.sql.", "din sql/00EF_02_*.sql."))
        End If
        If plan.RowsBefore > 0 AndAlso Not k_spec.UpdateExisting Then
            k_report.Add(EfSeverity.Info, area, $"Sunt deja {plan.RowsBefore} rânduri în țintă; cele cu aceeași cheie rămân cum sunt.")
        End If
        _say($"{area}: citite {plan.RowsRead}, de scris {plan.RowsSelected}, lăsate deoparte {plan.RowsLeftOut}" &
             If(k_write, $", adăugate {plan.RowsAdded}, deja existente {plan.RowsKept}.", "."))
        If String.Equals(area, "EF_Mesaje", StringComparison.Ordinal) Then
            _say($"Filtru CUI pentru DC «{_options.UnitDatabase}»: din {plan.RowsRead} mesaje găsite în baza Access, " &
                 $"{rowsMatchingFilter} au EF.cui_unit corespunzător CUI-ului «{_options.UnitCui}» " &
                 $"(normalizat «{EfCui.Normalize(_options.UnitCui)}»); {plan.RowsLeftOut} sunt excluse de filtrul CUI.")
        ElseIf String.Equals(area, "EF_Primite", StringComparison.Ordinal) Then
            _say($"Din {plan.RowsRead} facturi primite găsite în baza Access, {rowsMatchingFilter} aparțin unității " &
                 $"cu DC «{_options.UnitDatabase}» și CUI «{_options.UnitCui}», conform legăturii id_sol cu mesajele EF selectate după cui_unit. " &
                 $"{plan.RowsLeftOut} sunt lăsate deoparte fiindcă nu au un mesaj EF selectat pentru această unitate.")
            If plan.RowsRead > 0 AndAlso rowsMatchingFilter = 0 Then
                Dim message = $"Nu este selectată nicio factură primită pentru DC «{_options.UnitDatabase}», CUI «{_options.UnitCui}», " &
                              $"deși baza Access conține {plan.RowsRead} facturi primite. Verificați codul fiscal folosit pentru DC și fișierul de facturi primite ales."
                k_report.Add(EfSeverity.Warning, area, message)
                _say("[ATENȚIE] " & message)
            End If
        End If
        Advance(k_total, area)
    End Sub

    Private Sub CheckDecimalStorage(k_spec As EfTableSpec, k_cn As MySqlConnection,
                                    k_tx As MySqlTransaction, k_report As EfImportReport)
        Dim amounts As New HashSet(Of String)(k_spec.Columns.Where(Function(c) c.Kind = EfKind.Amount AndAlso c.Scale = 4).Select(Function(c) c.Target), StringComparer.OrdinalIgnoreCase)
        If amounts.Count = 0 Then Return
        Using cmd As New MySqlCommand("SELECT COLUMN_NAME, DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=@t", k_cn, k_tx)
            cmd.Parameters.AddWithValue("@t", k_spec.TargetTable)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim name = reader.GetString(0)
                    If Not amounts.Remove(name) Then Continue While
                    If Not String.Equals(reader.GetString(1), "decimal", StringComparison.OrdinalIgnoreCase) OrElse
                       reader.IsDBNull(2) OrElse reader.IsDBNull(3) OrElse
                       Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture) <> 20 OrElse
                       Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture) <> 4 Then
                        k_report.Add(EfSeverity.Blocking, k_spec.TargetTable,
                            $"Coloana {name} trebuie să fie DECIMAL(20,4). Aplicați sql/00EF_24_primite_linii_precizie.sql pe sursă și sincronizați schema în «{_options.UnitDatabase}».")
                    End If
                End While
            End Using
        End Using
        For Each name In amounts
            k_report.Add(EfSeverity.Blocking, k_spec.TargetTable, $"Lipsește coloana de valoare {name} din țintă.")
        Next
    End Sub

    ''' <summary>
    ''' Second pass over the received invoices: EFT.IDEFT_REF (a credit note's original invoice) points at another
    ''' invoice of the same table, which may come later in the file than the note. The rows were written without it;
    ''' here each reference whose target travelled is set, the others are reported and left empty.
    ''' </summary>
    Private Sub DoReceivedReferences(k_write As Boolean, k_report As EfImportReport, k_access As OleDbConnection,
                                     k_cn As MySqlConnection, k_tx As MySqlTransaction)
        Dim tableName = AccessSchema.ResolveTableName(k_access, "EFT")
        If tableName Is Nothing Then Return
        Dim pairs As New List(Of KeyValuePair(Of Integer, Integer))()
        Dim outside As Integer = 0
        Using reader = AccessSchema.OpenKeyReader(k_access, tableName, New String() {"IDEFT", "IDEFT_REF"})
            While reader.Read()
                Dim id = ToInt(reader.ValueOrMissing("IDEFT"))
                Dim ref = ToInt(reader.ValueOrMissing("IDEFT_REF"))
                If Not id.HasValue OrElse Not ref.HasValue OrElse ref.Value = 0 Then Continue While
                If Not _receivedIds.Contains(id.Value) Then Continue While
                If _receivedIds.Contains(ref.Value) Then
                    pairs.Add(New KeyValuePair(Of Integer, Integer)(id.Value, ref.Value))
                Else
                    outside += 1
                End If
            End While
        End Using
        If outside > 0 Then
            k_report.Add(EfSeverity.Warning, "EF_Primite",
                $"{outside} facturi trimit la o factură care nu face parte din import (IdPrimitaRef rămâne gol).")
        End If
        If k_write AndAlso pairs.Count > 0 AndAlso k_cn IsNot Nothing AndAlso Not k_report.HasBlocking Then
            For Each pair In pairs
                Using cmd = k_cn.CreateCommand()
                    cmd.Transaction = k_tx
                    cmd.CommandText = "UPDATE `EF_Primite` SET `IdPrimitaRef` = @r WHERE `IdPrimita` = @i AND `IdPrimitaRef` IS NULL"
                    cmd.Parameters.AddWithValue("@r", pair.Value)
                    cmd.Parameters.AddWithValue("@i", pair.Key)
                    cmd.ExecuteNonQuery()
                End Using
            Next
        End If
        _say($"EF_Primite: {pairs.Count} trimiteri între facturi" & If(k_write, " scrise.", "."))
    End Sub

    ''' <summary>
    ''' Slice 00EF-27. Third pass over the received invoices: the CPV of each line (<c>EF_PrimiteLinii.Cpv</c>). Access kept the ANAF zip of every
    ''' message in <c>EF.XML</c>; each zip is opened here, its invoice XML read, and the code of every line written to the line it
    ''' belongs to (matched by invoice and line number, <c>EFS.ID</c> = the XML's line <c>ID</c>). A line whose invoice could not be
    ''' read keeps NULL («not read»); a line of a read invoice that has no STI code gets «» (the view paints it dark red).
    ''' A line that already has a value is left as it is, like every other row of this import.
    ''' </summary>
    Private Sub DoReceivedCpv(k_write As Boolean, k_report As EfImportReport, k_access As OleDbConnection,
                              k_cn As MySqlConnection, k_tx As MySqlTransaction, k_token As CancellationToken)
        Dim k_area As String = "EF_PrimiteLinii"
        Dim k_messagesTable = AccessSchema.ResolveTableName(k_access, "EF")
        Dim k_invoicesTable = AccessSchema.ResolveTableName(k_access, "EFT")
        Dim k_linesTable = AccessSchema.ResolveTableName(k_access, "EFS")
        If k_messagesTable Is Nothing OrElse k_invoicesTable Is Nothing OrElse k_linesTable Is Nothing Then Return
        If k_cn Is Nothing OrElse Not CpvColumnExists(k_cn, k_tx) Then
            k_report.Add(EfSeverity.Blocking, k_area,
                $"Lipsește coloana Cpv din EF_PrimiteLinii. Aplicați sql/00EF_27_primite_linii_cpv.sql pe sursă și sincronizați schema în «{_options.UnitDatabase}».")
            Return
        End If

        ' message (id_sol) -> the invoice(s) of it that travel
        Dim k_invoicesOfMessage As New Dictionary(Of String, List(Of Integer))(StringComparer.OrdinalIgnoreCase)
        Using k_reader = AccessSchema.OpenKeyReader(k_access, k_invoicesTable, New String() {"IDEFT", "id_sol"})
            While k_reader.Read()
                Dim k_id = ToInt(k_reader.ValueOrMissing("IDEFT"))
                Dim k_sol As String = Convert.ToString(k_reader.ValueOrMissing("id_sol"), CultureInfo.InvariantCulture)
                If Not k_id.HasValue OrElse Not _receivedIds.Contains(k_id.Value) OrElse String.IsNullOrWhiteSpace(k_sol) Then Continue While
                Dim k_list As List(Of Integer) = Nothing
                If Not k_invoicesOfMessage.TryGetValue(k_sol.Trim(), k_list) Then
                    k_list = New List(Of Integer)()
                    k_invoicesOfMessage(k_sol.Trim()) = k_list
                End If
                k_list.Add(k_id.Value)
            End While
        End Using

        ' invoice -> its lines in the target: (IdLinie, NrLinie)
        Dim k_linesOfInvoice As New Dictionary(Of Integer, List(Of KeyValuePair(Of Integer, String)))()
        Using k_reader = AccessSchema.OpenKeyReader(k_access, k_linesTable, New String() {"IDEFS", "IDEFT", "ID"})
            While k_reader.Read()
                Dim k_lineId = ToInt(k_reader.ValueOrMissing("IDEFS"))
                Dim k_invoiceId = ToInt(k_reader.ValueOrMissing("IDEFT"))
                If Not k_lineId.HasValue OrElse Not k_invoiceId.HasValue OrElse Not _receivedIds.Contains(k_invoiceId.Value) Then Continue While
                Dim k_number As String = Convert.ToString(k_reader.ValueOrMissing("ID"), CultureInfo.InvariantCulture)
                Dim k_list As List(Of KeyValuePair(Of Integer, String)) = Nothing
                If Not k_linesOfInvoice.TryGetValue(k_invoiceId.Value, k_list) Then
                    k_list = New List(Of KeyValuePair(Of Integer, String))()
                    k_linesOfInvoice(k_invoiceId.Value) = k_list
                End If
                k_list.Add(New KeyValuePair(Of Integer, String)(k_lineId.Value, If(k_number, String.Empty).Trim()))
            End While
        End Using

        Dim k_filled As Integer = 0
        Dim k_without As Integer = 0
        Dim k_unmatched As Integer = 0
        Dim k_unreadable As Integer = 0
        Dim k_messages As Integer = 0
        Dim k_updates As New List(Of KeyValuePair(Of Integer, String))()

        Using k_reader = AccessSchema.OpenKeyReader(k_access, k_messagesTable, New String() {"id_sol", "XML"})
            While k_reader.Read()
                k_token.ThrowIfCancellationRequested()
                Dim k_sol As String = Convert.ToString(k_reader.ValueOrMissing("id_sol"), CultureInfo.InvariantCulture)
                Dim k_invoices As List(Of Integer) = Nothing
                If String.IsNullOrWhiteSpace(k_sol) OrElse Not k_invoicesOfMessage.TryGetValue(k_sol.Trim(), k_invoices) Then Continue While
                k_messages += 1

                Dim k_raw As Object = k_reader.ValueOrMissing("XML")
                Dim k_bytes As Byte() = TryCast(k_raw, Byte())
                If k_bytes Is Nothing AndAlso TypeOf k_raw Is String Then k_bytes = System.Text.Encoding.UTF8.GetBytes(DirectCast(k_raw, String))
                Dim k_parsed As List(Of EfCpvReader.LineCpv) = EfCpvReader.Read(k_bytes)
                If k_parsed Is Nothing Then
                    k_unreadable += 1
                    k_report.AddLimited(EfSeverity.Warning, k_area, "CPV: XML necitit",
                        $"mesajul {k_sol.Trim()}: din EF.XML nu s-a putut citi factura; liniile ei rămân fără CPV (necitit).")
                    Continue While
                End If

                ' a line number that repeats inside the invoice cannot be told apart: those lines stay unread
                Dim k_codeOfNumber As New Dictionary(Of String, String)(StringComparer.Ordinal)
                Dim k_repeated As New HashSet(Of String)(StringComparer.Ordinal)
                For Each k_item In k_parsed
                    If k_codeOfNumber.ContainsKey(k_item.NrLinie) Then
                        k_repeated.Add(k_item.NrLinie)
                    Else
                        k_codeOfNumber(k_item.NrLinie) = k_item.Cpv
                    End If
                Next

                For Each k_invoiceId In k_invoices
                    Dim k_lines As List(Of KeyValuePair(Of Integer, String)) = Nothing
                    If Not k_linesOfInvoice.TryGetValue(k_invoiceId, k_lines) Then Continue For
                    For Each k_line In k_lines
                        Dim k_code As String = Nothing
                        If k_repeated.Contains(k_line.Value) OrElse Not k_codeOfNumber.TryGetValue(k_line.Value, k_code) Then
                            k_unmatched += 1
                            Continue For
                        End If
                        If k_code.Length = 0 Then k_without += 1 Else k_filled += 1
                        k_updates.Add(New KeyValuePair(Of Integer, String)(k_line.Key, k_code))
                    Next
                Next
                If k_messages Mod 100 = 0 Then _say($"   CPV: {k_messages} mesaje citite…")
            End While
        End Using

        If k_unmatched > 0 Then
            k_report.Add(EfSeverity.Warning, k_area,
                $"{k_unmatched} linii nu se potrivesc cu nicio linie din XML (număr de linie lipsă sau repetat): rămân fără CPV (necitit).")
        End If
        If k_write AndAlso k_updates.Count > 0 AndAlso Not k_report.HasBlocking Then
            For Each k_update In k_updates
                Using k_cmd = k_cn.CreateCommand()
                    k_cmd.Transaction = k_tx
                    k_cmd.CommandText = "UPDATE `EF_PrimiteLinii` SET `Cpv` = @c WHERE `IdLinie` = @i AND `Cpv` IS NULL"
                    k_cmd.Parameters.AddWithValue("@c", k_update.Value)
                    k_cmd.Parameters.AddWithValue("@i", k_update.Key)
                    k_cmd.ExecuteNonQuery()
                End Using
            Next
        End If
        _say($"{k_area}: CPV din {k_messages} mesaje; {k_filled} linii cu cod, {k_without} fără cod (roșii în K-BOT), " &
             $"{k_unmatched} nepotrivite, {k_unreadable} mesaje necitite" & If(k_write, " — scrise.", "."))
    End Sub

    Private Function CpvColumnExists(k_cn As MySqlConnection, k_tx As MySqlTransaction) As Boolean
        Using k_cmd = k_cn.CreateCommand()
            k_cmd.Transaction = k_tx
            k_cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() " &
                                "AND TABLE_NAME = 'EF_PrimiteLinii' AND COLUMN_NAME = 'Cpv'"
            Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    ' ---- helpers ------------------------------------------------------------------------------------------------------

    Private Function OpenAccess(k_filePath As String, k_label As String, k_report As EfImportReport) As OleDbConnection
        If String.IsNullOrWhiteSpace(k_filePath) Then
            k_report.Add(EfSeverity.Blocking, k_label, "Fișierul nu a fost ales.")
            Return Nothing
        End If
        If Not File.Exists(k_filePath) Then
            k_report.Add(EfSeverity.Blocking, k_label, $"Fișierul «{k_filePath}» nu există.")
            Return Nothing
        End If
        Try
            Return AccessProvider.Open(k_filePath, _options.AccessPassword)
        Catch ex As AccessOpenException
            k_report.Add(EfSeverity.Blocking, k_label, ex.Message.Split(vbLf(0))(0).Trim())
            Return Nothing
        End Try
    End Function

    Private Shared Function LoadIntKeys(k_cn As OleDbConnection, k_table As String, k_column As String) As HashSet(Of Integer)
        Dim keys As New HashSet(Of Integer)()
        Dim tableName = AccessSchema.ResolveTableName(k_cn, k_table)
        If tableName Is Nothing Then Return keys
        Using reader = AccessSchema.OpenKeyReader(k_cn, tableName, New String() {k_column})
            While reader.Read()
                Dim value = ToInt(reader.ValueOrMissing(k_column))
                If value.HasValue Then keys.Add(value.Value)
            End While
        End Using
        Return keys
    End Function

    Private Shared Function ToInt(k_value As Object) As Integer?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        Try
            Dim d = Convert.ToDecimal(k_value, CultureInfo.InvariantCulture)
            If d <> Math.Truncate(d) OrElse d < Integer.MinValue OrElse d > Integer.MaxValue Then Return Nothing
            Return CInt(d)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            Return Nothing
        End Try
    End Function

    Private Shared Function TableExists(k_cn As MySqlConnection, k_table As String, k_tx As MySqlTransaction) As Boolean
        Using cmd = k_cn.CreateCommand()
            cmd.Transaction = k_tx
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t"
            cmd.Parameters.AddWithValue("@t", k_table)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Shared Function CountRows(k_cn As MySqlConnection, k_table As String, k_tx As MySqlTransaction) As Long
        Using cmd = k_cn.CreateCommand()
            cmd.Transaction = k_tx
            cmd.CommandText = "SELECT COUNT(*) FROM " & TargetServer.Quote(k_table)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

End Class
