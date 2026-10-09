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
''' <see cref="EfCui"/>); their invoices, lines, notes and messages follow by key. EFT_O (the booking of an invoice) is
''' accounting and stays behind.
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
    ''' <summary>IdSol -> IdIncarcare of every message kept by the import: the archive is <c>fact&lt;IdIncarcare&gt;.zip</c>.</summary>
    Private _messageUploads As Dictionary(Of String, String)
    Private _receivedIds As HashSet(Of Integer)
    Private _umKnown As HashSet(Of String)
    Private _umUsed As Dictionary(Of String, Integer)
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
            cnUnit = _options.Server.Open(_options.UnitDatabase)
            If k_write Then txUnit = cnUnit.BeginTransaction()

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

            ' --- the tables, parents before children -----------------------------------------------------
            For Each spec In BuildSpecs()
                k_token.ThrowIfCancellationRequested()
                Dim source As OleDbConnection = If(IsReceivedTable(spec), accessReceived, accessIssued)
                If source Is Nothing Then Continue For
                CopyTable(spec, source, k_write, k_report, cnUnit, txUnit, plans, totalSteps, k_token)
            Next

            If _options.DoPrimite AndAlso accessReceived IsNot Nothing Then
                k_token.ThrowIfCancellationRequested()
                DoReceivedReferences(k_write, k_report, accessReceived, cnUnit, txUnit)
                DoReceivedArchives(k_write, k_report, cnUnit, txUnit)
            End If
            If _options.DoEmise Then ReportUnknownUm(k_report)

            k_report.Tables.AddRange(plans)
            If k_result IsNot Nothing Then k_result.Tables.AddRange(plans)

            If k_write Then
                If k_report.HasBlocking Then
                    Throw New InvalidOperationException(
                        "Importul a găsit date care nu se pot scrie (vezi constatările BLOCANTE din jurnal). Nu s-a scris nimic.")
                End If
                txUnit?.Commit()
                If k_result IsNot Nothing Then k_result.Committed = True
            End If
        Catch
            Try
                txUnit?.Rollback()
            Catch rollbackEx As Exception
                GlobalErrorLog.Write("EfImporter.Pass.RollbackUnit", rollbackEx)
            End Try
            Throw
        Finally
            If accessIssued IsNot Nothing Then accessIssued.Dispose()
            If accessReceived IsNot Nothing Then accessReceived.Dispose()
            If txUnit IsNot Nothing Then txUnit.Dispose()
            If cnUnit IsNot Nothing Then cnUnit.Dispose()
        End Try
    End Sub

    Private Sub ResetState()
        _accessClientIds = New HashSet(Of Integer)()
        _accessInvoiceIds = New HashSet(Of Integer)()
        _invoiceNumbers = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _messageIds = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _messageUploads = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        _receivedIds = New HashSet(Of Integer)()
        _umKnown = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        _umUsed = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    End Sub

    Private Function StepCount() As Integer
        Dim total As Integer = 0
        If _options.DoFurnizor Then total += 1
        If _options.DoClienti Then total += 1
        If _options.DoEmise Then total += 2
        If _options.DoPrimite Then total += 5
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
                If Not TableExists(cn, "EF_UM") Then Return known
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
        Dim detailsExist As Boolean = k_cn IsNot Nothing AndAlso DetailsTableExists(k_cn)
        If detailsExist Then
            plan.RowsBefore = CountDetails(k_cn)
            If plan.RowsBefore > 0 Then k_report.Add(EfSeverity.Info, "Unitati_Date", "Există deja datele furnizorului: rămân cum sunt (nu se suprascriu).")
        ElseIf k_cn IsNot Nothing Then
            k_report.Add(EfSeverity.Blocking, "Unitati_Date",
                $"Tabelul Unitati_Date lipsește din baza «{_options.CommonDatabase}». Rulați scriptul sql/00EF_13_unitati_detalii.sql.")
        End If

        If k_write AndAlso ok AndAlso Not k_report.HasBlocking Then
            WriteDetails(k_cn, k_tx, spec, values)
            plan.RowsAdded = CountDetails(k_cn) - plan.RowsBefore
            plan.RowsKept = plan.RowsSelected - plan.RowsAdded
        End If
        _say($"Unitati_Date: {Convert.ToString(values(0), CultureInfo.InvariantCulture)}, CUI {Convert.ToString(values(1), CultureInfo.InvariantCulture)}.")
        Advance(k_total, "Unitati_Date")
    End Sub

    Private Function DetailsTableExists(k_cn As MySqlConnection) As Boolean
        Using cmd = k_cn.CreateCommand()
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @s AND TABLE_NAME = 'Unitati_Date'"
            cmd.Parameters.AddWithValue("@s", _options.CommonDatabase)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Function CountDetails(k_cn As MySqlConnection) As Long
        Using cmd = k_cn.CreateCommand()
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
        s.Columns.Add(Col("Cant", "Cant", EfKind.Amount, k_scale:=3, k_default:=0D))
        s.Columns.Add(Col("PU", "PU", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=2, k_default:=0D))
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
        Dim uploadAt = s.IndexOf("IdIncarcare")
        s.AfterRow = Sub(row As Object())
                         Dim sol = Convert.ToString(row(solAt), CultureInfo.InvariantCulture)
                         _messageIds.Add(sol)
                         _messageUploads(sol) = Convert.ToString(row(uploadAt), CultureInfo.InvariantCulture)
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
        s.Columns.Add(Col("CotaTVA", "CotaTVA", EfKind.Amount, k_scale:=2, k_default:=0D))
        s.Columns.Add(Col("TVA", "TVA", EfKind.Amount, k_scale:=2, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=2, k_default:=0D))
        s.Columns.Add(Col("Total", "TOTAL", EfKind.Amount, k_scale:=2, k_default:=0D))
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
        s.AfterRow = Sub(row As Object())
                         _receivedIds.Add(Convert.ToInt32(row(idAt), CultureInfo.InvariantCulture))
                     End Sub
        Return s
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
        s.Columns.Add(Col("Cant", "Cant", EfKind.Amount, k_scale:=3, k_default:=0D))
        s.Columns.Add(Col("Pret", "Pret", EfKind.Amount, k_scale:=4, k_default:=0D))
        s.Columns.Add(Col("Valoare", "Valoare", EfKind.Amount, k_scale:=2, k_default:=0D))
        Return s
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

        Dim targetExists = k_cn IsNot Nothing AndAlso TableExists(k_cn, k_spec.TargetTable)
        If targetExists Then plan.RowsBefore = CountRows(k_cn, k_spec.TargetTable)

        Dim writer As EfBatchWriter = Nothing
        If k_write AndAlso Not k_report.HasBlocking AndAlso targetExists Then writer = New EfBatchWriter(k_cn, k_tx, k_spec)

        Dim wanted = k_spec.SourceColumnsToRead(k_write).Where(Function(c) present.Contains(c)).ToList()
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
            plan.RowsAdded = CountRows(k_cn, k_spec.TargetTable) - plan.RowsBefore
            plan.RowsKept = plan.RowsSelected - plan.RowsAdded
        End If
        If Not targetExists Then
            k_report.Add(EfSeverity.Blocking, area,
                $"Tabelul {k_spec.TargetTable} lipsește din baza «{_options.UnitDatabase}». Rulați scriptul din sql/00EF_02_*.sql.")
        End If
        If plan.RowsBefore > 0 AndAlso Not k_spec.UpdateExisting Then
            k_report.Add(EfSeverity.Info, area, $"Sunt deja {plan.RowsBefore} rânduri în țintă; cele cu aceeași cheie rămân cum sunt.")
        End If
        _say($"{area}: citite {plan.RowsRead}, de scris {plan.RowsSelected}, lăsate deoparte {plan.RowsLeftOut}" &
             If(k_write, $", adăugate {plan.RowsAdded}, deja existente {plan.RowsKept}.", "."))
        Advance(k_total, area)
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

    ' ---- the ANAF archives -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Stores <c>fact&lt;IdIncarcare&gt;.zip</c> in <c>EF_Mesaje.ZipContinut</c> for the messages the import keeps (the archive
    ''' holds the invoice XML AND ANAF's signature file) and, in the same statement, clears <c>XmlContinut</c>: a message keeps
    ''' its XML only when no usable archive was found for it. Archives without a message are ignored. A message that already has
    ''' an archive is left as it is. Only warnings: a missing or damaged archive never blocks (the XML stays in the row).
    ''' </summary>
    Private Sub DoReceivedArchives(k_write As Boolean, k_report As EfImportReport, k_cn As MySqlConnection, k_tx As MySqlTransaction)
        Dim folder = _options.ZipFolder
        If String.IsNullOrWhiteSpace(folder) Then
            _say("Arhive ANAF: niciun folder ales; toate mesajele își păstrează XML-ul.")
            Return
        End If
        If Not Directory.Exists(folder) Then
            k_report.Add(EfSeverity.Warning, "EF_Mesaje", $"Folderul de arhive «{folder}» nu există: toate mesajele își păstrează XML-ul.")
            Return
        End If
        If _messageUploads.Count = 0 Then
            _say("Arhive ANAF: nu sunt mesaje de legat.")
            Return
        End If

        Dim found As Integer = 0
        Dim missing As Integer = 0
        Dim damaged As Integer = 0
        Dim noSignature As Integer = 0
        Dim bytes As Long = 0
        For Each pair In _messageUploads
            Dim upload = pair.Value
            Dim filePath = If(String.IsNullOrWhiteSpace(upload), Nothing, Path.Combine(folder, $"fact{upload.Trim()}.zip"))
            If filePath Is Nothing OrElse Not File.Exists(filePath) Then
                missing += 1
                Continue For
            End If
            Dim data = File.ReadAllBytes(filePath)
            Dim content = InspectArchive(data)
            If Not content.HasInvoiceXml Then
                damaged += 1
                Continue For
            End If
            If Not content.HasSignature Then noSignature += 1
            found += 1
            bytes += data.Length
            If k_write AndAlso k_cn IsNot Nothing AndAlso Not k_report.HasBlocking Then
                Using cmd = k_cn.CreateCommand()
                    cmd.Transaction = k_tx
                    cmd.CommandText = "UPDATE `EF_Mesaje` SET `ZipContinut` = @z, `XmlContinut` = NULL WHERE `IdSol` = @s AND `ZipContinut` IS NULL"
                    cmd.Parameters.AddWithValue("@z", data)
                    cmd.Parameters.AddWithValue("@s", pair.Key)
                    cmd.ExecuteNonQuery()
                End Using
            End If
        Next
        If missing > 0 Then
            k_report.Add(EfSeverity.Warning, "EF_Mesaje", $"{missing} mesaje nu au arhiva fact<id>.zip în folder (își păstrează XML-ul din Access).")
        End If
        If damaged > 0 Then
            k_report.Add(EfSeverity.Warning, "EF_Mesaje", $"{damaged} arhive sunt deteriorate sau nu conțin XML-ul facturii (mesajele își păstrează XML-ul din Access).")
        End If
        If noSignature > 0 Then
            k_report.Add(EfSeverity.Warning, "EF_Mesaje", $"{noSignature} arhive nu conțin fișierul de semnătură ANAF (semnatura_*.xml).")
        End If
        _say($"Arhive ANAF: {found} găsite ({CLng(bytes / 1024)} KB)" & If(k_write, " și scrise; XML-ul acestor mesaje nu se mai păstrează.", "; XML-ul acestor mesaje nu se va mai păstra."))
    End Sub

    ''' <summary>What a zip holds: the invoice XML (an .xml entry that is not <c>semnatura_*</c>) and ANAF's signature file. A damaged zip holds neither.</summary>
    Private Shared Function InspectArchive(k_zip As Byte()) As (HasInvoiceXml As Boolean, HasSignature As Boolean)
        Try
            Using ms As New MemoryStream(k_zip)
                Using archive As New System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read)
                    Dim xmlEntries = archive.Entries.Where(Function(en) en.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).ToList()
                    Dim signature = xmlEntries.Any(Function(en) en.Name.StartsWith("semnatura_", StringComparison.OrdinalIgnoreCase))
                    Dim invoice = xmlEntries.Any(Function(en) Not en.Name.StartsWith("semnatura_", StringComparison.OrdinalIgnoreCase) AndAlso en.Length > 0)
                    Return (invoice, signature)
                End Using
            End Using
        Catch ex As InvalidDataException
            Return (False, False)
        End Try
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

    Private Shared Function TableExists(k_cn As MySqlConnection, k_table As String) As Boolean
        Using cmd = k_cn.CreateCommand()
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t"
            cmd.Parameters.AddWithValue("@t", k_table)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Shared Function CountRows(k_cn As MySqlConnection, k_table As String) As Long
        Using cmd = k_cn.CreateCommand()
            cmd.CommandText = "SELECT COUNT(*) FROM " & TargetServer.Quote(k_table)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

End Class
