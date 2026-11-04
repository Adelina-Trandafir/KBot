Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports KBot.Migrator
Imports MySqlConnector

Public NotInheritable Class AdeOpeningBalanceRow
    Public Property SourceId As Long
    Public Property TargetId As Long
    Public Property Name As String
    Public Property ExistingInitial As Integer?
    Public Property OldValue As Long?
    Public Property NewValue As Long
    Public Property CombinedInitial As Long
    Public Property HasProposed As Boolean
    Public Property Version As Long
    Public Property InitialVersion As Long
    Public Property PreviousSourceHash As String
    Public Property Origin As String
    Public Property State As String
    Public Property CanApply As Boolean
    Public Property Resolved As Boolean
End Class

Public NotInheritable Class AdeOpeningBalancePreview
    Public Property Source As AdeSource
    Public Property SubunitId As Long
    Public Property Rows As List(Of AdeOpeningBalanceRow)
    Public Property Stamp As String
End Class

''' <summary>SLICE-AD19-02. Only the separate AD_SOLDURI_REPORTATE table can be written.</summary>
Public NotInheritable Class AdeOpeningBalanceTransfer
    Private Sub New()
    End Sub

    Public Shared Function Preview(server As TargetServer, dc As String, subunit As String, file As String) As AdeOpeningBalancePreview
        Dim source = ReadStable(file)
        Dim candidates = AdeOpeningBalanceSource.Build(source)
        Using cn = server.Open(dc)
            Return BuildPreview(cn, Nothing, subunit, source, candidates)
        End Using
    End Function

    Private Shared Function ReadStable(file As String) As AdeSource
        Dim source = AdeSource.Read(file)
        Using stream As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim afterRead = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant()
            If source.FileHash <> afterRead Then Throw New InvalidOperationException("MDB-ul s-a schimbat în timpul citirii. Repetați previzualizarea.")
        End Using
        Return source
    End Function

    Private Shared Function Load(cn As MySqlConnection, tx As MySqlTransaction, sql As String, subunitId As Long) As DataTable
        Dim table As New DataTable With {.Locale = CultureInfo.InvariantCulture}
        Using cmd As New MySqlCommand(sql, cn, tx)
            cmd.Parameters.AddWithValue("@s", subunitId)
            Using reader = cmd.ExecuteReader()
                table.Load(reader)
            End Using
        End Using
        Return table
    End Function

    Private Shared Function BuildPreview(cn As MySqlConnection, tx As MySqlTransaction, subunit As String, source As AdeSource,
                                         candidates As List(Of AdeOpeningBalance)) As AdeOpeningBalancePreview
        Dim subunitId As Long
        Using cmd As New MySqlCommand("SELECT SubunitId, Active FROM AD_Subunits WHERE Name=@n", cn, tx)
            cmd.Parameters.AddWithValue("@n", subunit.Trim())
            Using reader = cmd.ExecuteReader()
                If Not reader.Read() OrElse Not reader.GetBoolean(1) Then
                    Throw New InvalidOperationException("Alegeți o subunitate existentă și activă, în care s-a făcut migrarea din 2021.")
                End If
                subunitId = reader.GetInt64(0)
            End Using
        End Using
        Dim months = Load(cn, tx, "SELECT IDL, Anul, Luna FROM AD_LunaD WHERE SubunitId=@s", subunitId)
        If months.Rows.Count = 0 OrElse months.AsEnumerable().Any(Function(r) AdeOpeningBalanceSource.Number(r, "Anul") < 2021) OrElse
           Not months.AsEnumerable().Any(Function(r) AdeOpeningBalanceSource.Number(r, "Anul") = 2021) Then
            Throw New InvalidOperationException("Operația se aplică unei subunități migrate începând cu 2021, fără luni anterioare deja importate.")
        End If
        Dim firstTarget = months.AsEnumerable().Where(Function(r) AdeOpeningBalanceSource.Number(r, "Anul") = 2021).
            Min(Function(r) AdeOpeningBalanceSource.Number(r, "Luna"))
        Dim firstSource = source.Tables("LunaD").AsEnumerable().Where(Function(r) AdeOpeningBalanceSource.Number(r, "Anul") = 2021).
            Min(Function(r) AdeOpeningBalanceSource.Number(r, "Luna"))
        If firstTarget <> firstSource Then
            Throw New InvalidOperationException("Prima lună din 2021 diferă între sursă și destinație. Completarea numai a istoricului anterior lui 2021 nu este suficientă.")
        End If
        Using cmd As New MySqlCommand("SELECT ENGINE FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='AD_SOLDURI_REPORTATE'", cn, tx)
            If Not String.Equals(Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), "InnoDB", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Lipsește tabelul InnoDB AD_SOLDURI_REPORTATE. Aplicați manual sql/AD_19_02_solduri_reportate.sql și sincronizați schema destinației.")
            End If
        End Using
        Dim existing = Load(cn, tx, "SELECT IDP, SOLD, VERSION, SOURCE_HASH FROM AD_SOLDURI_REPORTATE WHERE SUBUNIT_ID=@s AND AN_START=2021", subunitId).
            AsEnumerable().ToDictionary(Function(r) AdeOpeningBalanceSource.Id(r, "IDP"))
        Dim mapped = Load(cn, tx, "SELECT m.SourceId, p.IDP, p.Nume, p.CNP, p.SI, p.Version FROM AD_IdMap m " &
            "JOIN AD_Platitori p ON p.SubunitId=m.SubunitId AND p.IDP=m.TargetId " &
            "WHERE m.SubunitId=@s AND m.TableName='Platitori'", subunitId)
        Dim bySource = mapped.AsEnumerable().ToDictionary(Function(r) AdeOpeningBalanceSource.Id(r, "SourceId"))
        Dim duplicateTargets = mapped.AsEnumerable().GroupBy(Function(r) AdeOpeningBalanceSource.Id(r, "IDP")).
            Where(Function(g) g.Count() > 1).Select(Function(g) g.Key).ToHashSet()
        Dim present = Load(cn, tx, "SELECT DISTINCT z.IDP FROM AD_Prezenta z JOIN AD_LunaD m ON m.SubunitId=z.SubunitId AND m.IDL=z.IDL " &
            "WHERE z.SubunitId=@s AND m.Anul=2021", subunitId).AsEnumerable().Select(Function(r) AdeOpeningBalanceSource.Id(r, "IDP")).ToHashSet()
        Dim orphaned = Load(cn, tx, "SELECT IDP FROM AD_Plati WHERE SubunitId=@s AND Anulata=0 AND IDL IS NULL AND (Data<'2021-01-01' OR Data IS NULL) " &
            "UNION SELECT r.IDP FROM AD_Retur r LEFT JOIN AD_Prezenta z ON z.SubunitId=r.SubunitId AND z.IDZ=r.IDZ " &
            "WHERE r.SubunitId=@s AND r.Anulat=0 AND COALESCE(r.IDL,z.IDL) IS NULL AND (r.Data<'2021-01-01' OR r.Data IS NULL)", subunitId).
            AsEnumerable().Select(Function(r) AdeOpeningBalanceSource.Id(r, "IDP")).ToHashSet()
        Dim rows As New List(Of AdeOpeningBalanceRow)()
        For Each candidate In candidates
            Dim item As New AdeOpeningBalanceRow With {.SourceId = candidate.SourceId, .Name = candidate.Name,
                .Origin = candidate.Origin,
                .State = If(candidate.Problem.Length = 0, String.Empty, "Blocat: " & candidate.Problem)}
            rows.Add(item)
            Dim target As DataRow = Nothing
            If Not bySource.TryGetValue(candidate.SourceId, target) Then
                item.State = "Omis: copilul nu este asociat prin harta migrării existente."
                Continue For
            End If
            item.TargetId = AdeOpeningBalanceSource.Id(target, "IDP")
            item.InitialVersion = AdeOpeningBalanceSource.Id(target, "Version")
            item.ExistingInitial = If(target.IsNull("SI"), CType(Nothing, Integer?), Convert.ToInt32(target("SI"), CultureInfo.InvariantCulture))
            Dim reported As DataRow = Nothing
            If existing.TryGetValue(item.TargetId, reported) Then
                item.Version = AdeOpeningBalanceSource.Id(reported, "VERSION")
                item.OldValue = AdeOpeningBalanceSource.Id(reported, "SOLD")
                item.PreviousSourceHash = AdeOpeningBalanceSource.Text(reported, "SOURCE_HASH")
            End If
            If candidate.HasProposed Then
                item.HasProposed = True
                ' Remove the original opening once: the separate component must not contain SI again.
                Dim original = candidate.OriginalInitial.GetValueOrDefault()
                item.NewValue = CLng(candidate.Proposed) - original
                item.CombinedInitial = CLng(item.ExistingInitial.GetValueOrDefault()) + item.NewValue
                item.Origin &= "; componentă separată după scăderea SI de bază"
            End If
            If item.State.Length > 0 Then Continue For
            If duplicateTargets.Contains(item.TargetId) Then
                item.State = "Blocat: mai multe ID-uri sursă sunt asociate aceluiași copil."
            ElseIf Not present.Contains(item.TargetId) Then
                item.State = "Omis: copilul nu are prezență în 2021 în destinație."
            ElseIf orphaned.Contains(item.TargetId) Then
                item.State = "Blocat: destinația conține mișcări vechi sau fără dată, fără legătură cu luna."
            ElseIf Not SameIdentity(candidate, target) Then
                item.State = "Blocat: numele sau CNP-ul nu corespunde hărții ID-urilor."
            ElseIf candidate.OriginalInitial.HasValue AndAlso item.ExistingInitial.GetValueOrDefault() <> candidate.OriginalInitial.Value Then
                item.State = "Blocat: SI existent diferă de SI de bază din MDB; verificați dacă reportarea este deja inclusă."
            ElseIf Not candidate.OriginalInitial.HasValue AndAlso item.ExistingInitial.GetValueOrDefault() <> 0 Then
                item.State = "Blocat: SI de bază lipsește din MDB, iar SI existent este nenul; separarea trebuie clarificată."
            ElseIf item.CombinedInitial < Integer.MinValue OrElse item.CombinedInitial > Integer.MaxValue Then
                item.State = "Blocat: soldul inițial combinat nu încape în Access Long."
            ElseIf item.OldValue.HasValue AndAlso item.OldValue.Value = item.NewValue Then
                item.State = "Sold reportat deja corect; nu se scrie."
                item.Resolved = True
            ElseIf Not item.OldValue.HasValue AndAlso item.NewValue = 0 Then
                item.State = "Fără sold suplimentar de reportat."
                item.Resolved = True
            Else
                item.State = "De aplicat"
                item.CanApply = True
            End If
        Next
        Dim preview As New AdeOpeningBalancePreview With {.Source = source, .SubunitId = subunitId, .Rows = rows}
        Dim json = JsonSerializer.Serialize(New With {.source = source.FileHash, .subunit = subunitId,
            .items = rows.OrderBy(Function(r) r.SourceId).ToList()})
        preview.Stamp = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)))
        Return preview
    End Function

    Private Shared Function SameIdentity(candidate As AdeOpeningBalance, target As DataRow) As Boolean
        Dim comparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)
        If Not comparer.Equals(candidate.Name.Trim(), AdeOpeningBalanceSource.Text(target, "Nume").Trim()) Then Return False
        Dim sourceCnp = candidate.Cnp.Trim()
        If sourceCnp = "-1" OrElse sourceCnp.Equals("FARA CNP", StringComparison.OrdinalIgnoreCase) Then sourceCnp = String.Empty
        Dim targetCnp = AdeOpeningBalanceSource.Text(target, "CNP").Trim()
        Return sourceCnp.Length = 0 OrElse targetCnp.Length = 0 OrElse sourceCnp = targetCnp
    End Function

    Public Shared Function Apply(server As TargetServer, dc As String, subunit As String, preview As AdeOpeningBalancePreview,
                                 journalRoot As String, log As Action(Of String)) As Integer
        ' Re-read the Access file before opening a destination transaction.
        Dim source = ReadStable(preview.Source.FilePath)
        If source.FileHash <> preview.Source.FileHash Then Throw New InvalidOperationException("MDB-ul s-a schimbat. Refaceți previzualizarea.")
        Dim candidates = AdeOpeningBalanceSource.Build(source)
        Using dump As New SqlDumpWriter(journalRoot, dc, log)
            dump.WriteInfo({"Operație: solduri reportate separat dinainte de 2021", "Bază: " & dc, "Subunitate: " & subunit,
                "Fișier: " & Path.GetFileName(source.FilePath), "SHA-256: " & source.FileHash})
            Using cn = server.Open(dc)
                Using tx = cn.BeginTransaction()
                    Dim commitAttempted = False
                    Try
                        LockRow(cn, tx, 0)
                        LockRow(cn, tx, preview.SubunitId)
                        Dim fresh = BuildPreview(cn, tx, subunit, source, candidates)
                        If fresh.Stamp <> preview.Stamp Then Throw New InvalidOperationException("Datele destinației s-au schimbat. Refaceți previzualizarea înainte de aplicare.")
                        Dim count = 0
                        For Each item In fresh.Rows.Where(Function(r) r.CanApply)
                            Dim sql = If(item.OldValue.HasValue,
                                "UPDATE AD_SOLDURI_REPORTATE SET SOLD=@new, SOURCE_IDP=@source, SOURCE_HASH=@hash, SOURCE_FILE=@file, VERSION=VERSION+1 " &
                                "WHERE SUBUNIT_ID=@s AND IDP=@id AND AN_START=2021 AND VERSION=@v AND SOLD=@old",
                                "INSERT INTO AD_SOLDURI_REPORTATE (SUBUNIT_ID,IDP,AN_START,SOLD,SOURCE_IDP,SOURCE_HASH,SOURCE_FILE) " &
                                "VALUES (@s,@id,2021,@new,@source,@hash,@file)")
                            Using cmd As New MySqlCommand(sql, cn, tx)
                                cmd.Parameters.AddWithValue("@new", item.NewValue)
                                cmd.Parameters.AddWithValue("@s", fresh.SubunitId)
                                cmd.Parameters.AddWithValue("@id", item.TargetId)
                                cmd.Parameters.AddWithValue("@source", item.SourceId)
                                cmd.Parameters.AddWithValue("@hash", source.FileHash)
                                cmd.Parameters.AddWithValue("@file", Path.GetFileName(source.FilePath))
                                If item.OldValue.HasValue Then
                                    cmd.Parameters.AddWithValue("@v", item.Version)
                                    cmd.Parameters.AddWithValue("@old", item.OldValue.Value)
                                End If
                                Dim values = cmd.Parameters.Cast(Of MySqlParameter)().ToDictionary(Function(p) p.ParameterName, Function(p) ValueConverter.ToLiteral(p.Value))
                                Dim journalSql = System.Text.RegularExpressions.Regex.Replace(sql, "@[A-Za-z][A-Za-z0-9_]*", Function(m) values(m.Value))
                                dump.WriteStatement("AD_SOLDURI_REPORTATE", journalSql)
                                If cmd.ExecuteNonQuery() <> 1 Then Throw New InvalidOperationException("Un sold a fost modificat între timp. Operația se anulează integral; refaceți previzualizarea.")
                                count += 1
                            End Using
                        Next
                        commitAttempted = True
                        tx.Commit()
                        dump.WriteFinal(True, {$"Solduri reportate salvate separat: {count}. AD_Platitori.SI nu a fost modificat."}, Nothing)
                        Return count
                    Catch ex As Exception
                        If commitAttempted Then
                            dump.WriteComment("_outcome", "COMMIT neconfirmat. Refaceți previzualizarea pentru a afla soldurile existente înainte de orice reluare.")
                            dump.FlushAll()
                            Throw New InvalidOperationException("Confirmarea COMMIT nu a fost primită. Verificați soldurile printr-o previzualizare nouă înainte de reluare.", ex)
                        End If
                        Try
                            tx.Rollback()
                        Catch rollbackError As Exception
                            dump.WriteComment("_outcome", "Rollback neconfirmat. Verificați soldurile înainte de reluare.")
                            dump.FlushAll()
                            Throw New AggregateException("Rollback neconfirmat. Verificați soldurile înainte de reluare.", ex, rollbackError)
                        End Try
                        dump.WriteFinal(False, {"Rollback confirmat; niciun sold actualizat."}, ex)
                        Throw
                    End Try
                End Using
            End Using
        End Using
    End Function

    Private Shared Sub LockRow(cn As MySqlConnection, tx As MySqlTransaction, id As Long)
        Using cmd As New MySqlCommand("SELECT ID FROM AD_Lock WHERE ID=@id FOR UPDATE", cn, tx)
            cmd.Parameters.AddWithValue("@id", id)
            If cmd.ExecuteScalar() Is Nothing Then Throw New InvalidOperationException("Lipsește blocarea existentă a subunității. Operația nu creează date auxiliare.")
        End Using
    End Sub
End Class
