Imports System.Globalization
Imports System.IO
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Slice 0103-03: the OPENING budget of the year, taken from the previous year's Access files.
''' </summary>
''' <remarks>
''' <para>
''' Until the new year's budget is adopted only a twelfth of last year's budget may be spent a
''' month. For every selected unit this reads the PREVIOUS year's unit file
''' (<c>baza2025.accdb</c>, the registry's <c>baza2026.accdb</c> with the year stepped back),
''' sums <c>Clasificatii.Trim1..4</c> and <c>Rectificari.Trim1..4</c> per classification, and
''' writes one <c>Clasificatii_Buget</c> version starting on 01.01 of the transfer year with
''' <c>Trim1 = CEILING(sum / 12)</c> and <c>Trim2..4 = 0</c> (operator, 02.10.2026: the
''' budget is read cumulatively to the quarter of the day, so for January only Trim1 counts).
''' </para>
''' <para>
''' <b>Mapping.</b> The sums are made INSIDE the previous file (Rectificari joins Clasificatii on
''' its own IDClsf), then a classification is
''' matched to MariaDB on <c>Access.IdClsf = Clasificatii.IdClsfAcc AND IdUnitate = the unit</c>
''' (operator, 02.10.2026); no row, or more than one row, is reported as unmatched, never guessed.
''' </para>
''' <para>
''' Two phases: <see cref="Plan"/> only reads; <see cref="Write"/> inserts the plan in one
''' transaction. A version already present for (classification, year, 01.01) is left as it is -
''' the operator may have corrected it - and counted.
''' </para>
''' </remarks>
Public NotInheritable Class BudgetOpeningRunner

    Private ReadOnly _server As TargetServer
    Private ReadOnly _database As String
    Private ReadOnly _accessPassword As String
    Private ReadOnly _say As Action(Of String)

    Public Sub New(server As TargetServer, database As String, accessPassword As String,
                   say As Action(Of String))
        If server Is Nothing Then Throw New ArgumentNullException(NameOf(server))
        If String.IsNullOrWhiteSpace(database) Then Throw New ArgumentException("Numele bazei lipseste.", NameOf(database))
        _server = server
        _database = database
        _accessPassword = If(accessPassword, String.Empty)
        _say = If(say, Sub(m As String)
                       End Sub)
    End Sub

    ''' <summary>The date the opening version starts: 01.01 of the transfer year.</summary>
    Public Shared ReadOnly Property StartDate As Date
        Get
            Return New Date(TableMaps.TransferYear, 1, 1)
        End Get
    End Property

    ''' <summary>The previous year's file of a unit: the unit file with the year stepped back.</summary>
    Public Shared Function PreviousYearFile(unitFilePath As String) As String
        If String.IsNullOrWhiteSpace(unitFilePath) Then Return String.Empty
        Dim folder = Path.GetDirectoryName(unitFilePath)
        Dim name = Path.GetFileName(unitFilePath)
        Dim current = TableMaps.TransferYear.ToString(CultureInfo.InvariantCulture)
        Dim previous = (TableMaps.TransferYear - 1).ToString(CultureInfo.InvariantCulture)
        If name.IndexOf(current, StringComparison.Ordinal) < 0 Then Return String.Empty
        Return Path.Combine(folder, name.Replace(current, previous))
    End Function

    ''' <summary>Reads Access and MariaDB and builds what would be written. Writes nothing.</summary>
    Public Function Plan(units As IEnumerable(Of CaiUnit), token As Threading.CancellationToken) As BudgetOpeningPlan
        Try
            Dim result As New BudgetOpeningPlan()
            Using cn = _server.Open(_database)
                For Each unit In units
                    token.ThrowIfCancellationRequested()
                    PlanUnit(cn, unit, result)
                Next
            End Using
            Return result
        Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
            GlobalErrorLog.Write("BudgetOpeningRunner.Plan", ex)
            Throw
        End Try
    End Function

    ''' <summary>Inserts the planned rows in ONE transaction. Returns the rows written.</summary>
    Public Function Write(plan As BudgetOpeningPlan) As Integer
        If plan Is Nothing Then Throw New ArgumentNullException(NameOf(plan))
        Dim written = 0
        Try
            Using cn = _server.Open(_database)
                Using tx = cn.BeginTransaction()
                    Try
                        For Each row In plan.Rows
                            Using cmd = cn.CreateCommand()
                                cmd.Transaction = tx
                                cmd.CommandText =
                                    "INSERT INTO `Clasificatii_Buget` (IdClsf, IdUnitate, Trim1, Trim2, Trim3, Trim4, An, DataInceput) " &
                                    "VALUES (@c, @u, @t1, 0, 0, 0, @an, @d)"
                                cmd.Parameters.AddWithValue("@c", row.IdClsf)
                                cmd.Parameters.AddWithValue("@u", row.IdUnitate)
                                cmd.Parameters.AddWithValue("@t1", row.Trim1)
                                cmd.Parameters.AddWithValue("@an", TableMaps.TransferYear)
                                cmd.Parameters.AddWithValue("@d", StartDate)
                                written += cmd.ExecuteNonQuery()
                            End Using
                        Next
                        tx.Commit()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
            Return written
        Catch ex As Exception
            GlobalErrorLog.Write("BudgetOpeningRunner.Write", ex)
            Throw
        End Try
    End Function

    ' ---- one unit ---------------------------------------------------------------------------

    Private Sub PlanUnit(cn As MySqlConnection, unit As CaiUnit, result As BudgetOpeningPlan)
        Dim file = PreviousYearFile(unit.UnitFilePath)
        If file.Length = 0 Then
            result.Notes.Add($"Unitatea {unit.IdUnitate}: calea fișierului din registru nu conține anul {TableMaps.TransferYear}, anul anterior nu se poate deduce.")
            Return
        End If
        If Not System.IO.File.Exists(file) Then
            result.Notes.Add($"Unitatea {unit.IdUnitate}: lipsește fișierul anului anterior «{file}».")
            Return
        End If

        Dim access As New Dictionary(Of Integer, AccessClassification)()
        ReadAccess(file, access)
        _say($"Unitatea {unit.IdUnitate}: {access.Count} clasificații în «{Path.GetFileName(file)}».")

        Dim byAcc As New Dictionary(Of Integer, List(Of Target))()
        Dim existing As New HashSet(Of Integer)()
        ReadTargets(cn, unit.IdUnitate, byAcc, existing)

        For Each acc In access.Values
            Dim sum = acc.Total
            Dim matches As List(Of Target) = Nothing
            Dim chosen As Target = Nothing
            If byAcc.TryGetValue(acc.IdClsf, matches) AndAlso matches.Count = 1 Then chosen = matches(0)

            If chosen Is Nothing Then
                result.Unmatched.Add($"Unitatea {unit.IdUnitate}, {acc.Code} (Access {acc.IdClsf}): " &
                                     If(matches Is Nothing, "nu există în MariaDB (IdClsfAcc).", "mai multe rânduri cu același IdClsfAcc."))
                Continue For
            End If
            If existing.Contains(chosen.IdClsf) Then
                result.AlreadyThere += 1
                Continue For
            End If
            result.Rows.Add(New BudgetOpeningRow(chosen.IdClsf, unit.IdUnitate, acc.Code,
                                                 sum, Math.Ceiling(sum / 12.0R)))
        Next
    End Sub

    Private Sub ReadAccess(file As String, into As Dictionary(Of Integer, AccessClassification))
        Using cn = AccessProvider.Open(file, _accessPassword)
            Dim clsName = AccessSchema.ResolveTableName(cn, "Clasificatii")
            If clsName Is Nothing Then
                Throw New InvalidOperationException($"Fișierul «{file}» nu conține tabelul «Clasificatii».")
            End If
            Using reader = AccessSchema.OpenReader(cn, clsName)
                While reader.Read()
                    Dim id = ToInt(reader.ValueOrMissing("IDClsf"))
                    If id = 0 Then Continue While
                    Dim acc As New AccessClassification(id,
                        Part(reader.ValueOrMissing("Capitol")), Part(reader.ValueOrMissing("Subcapitol")),
                        Part(reader.ValueOrMissing("Articol")), Part(reader.ValueOrMissing("Alineat")))
                    acc.Total += Sum4(reader)
                    into(id) = acc
                End While
            End Using

            Dim rectName = AccessSchema.ResolveTableName(cn, "Rectificari")
            If rectName Is Nothing Then Return
            Using reader = AccessSchema.OpenReader(cn, rectName)
                While reader.Read()
                    Dim acc As AccessClassification = Nothing
                    If into.TryGetValue(ToInt(reader.ValueOrMissing("IdClsf")), acc) Then
                        acc.Total += Sum4(reader)
                    End If
                End While
            End Using
        End Using
    End Sub

    Private Shared Function Sum4(reader As AccessTableReader) As Double
        Return Num(reader.ValueOrMissing("Trim1")) + Num(reader.ValueOrMissing("Trim2")) +
               Num(reader.ValueOrMissing("Trim3")) + Num(reader.ValueOrMissing("Trim4"))
    End Function

    Private Shared Sub ReadTargets(cn As MySqlConnection, idUnitate As Integer,
                                   byAcc As Dictionary(Of Integer, List(Of Target)),
                                   existing As HashSet(Of Integer))
        Using cmd = cn.CreateCommand()
            cmd.CommandText = "SELECT IDClsf, IdClsfAcc FROM `Clasificatii` WHERE IdUnitate = @u"
            cmd.Parameters.AddWithValue("@u", idUnitate)
            Using r = cmd.ExecuteReader()
                While r.Read()
                    Dim t As New Target(r.GetInt32(0), r.GetInt32(1))
                    Dim list As List(Of Target) = Nothing
                    If Not byAcc.TryGetValue(t.IdClsfAcc, list) Then
                        list = New List(Of Target)()
                        byAcc(t.IdClsfAcc) = list
                    End If
                    list.Add(t)
                End While
            End Using
        End Using
        Using cmd = cn.CreateCommand()
            cmd.CommandText = "SELECT IdClsf FROM `Clasificatii_Buget` WHERE IdUnitate = @u AND An = @an AND DataInceput = @d"
            cmd.Parameters.AddWithValue("@u", idUnitate)
            cmd.Parameters.AddWithValue("@an", TableMaps.TransferYear)
            cmd.Parameters.AddWithValue("@d", StartDate)
            Using r = cmd.ExecuteReader()
                While r.Read()
                    existing.Add(r.GetInt32(0))
                End While
            End Using
        End Using
    End Sub

    Private Shared Function Part(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function Num(value As Object) As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0
        Return Convert.ToDouble(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ToInt(value As Object) As Integer
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0
        Return Convert.ToInt32(value, CultureInfo.InvariantCulture)
    End Function

    Private NotInheritable Class AccessClassification
        Public Sub New(idClsf As Integer, capitol As String, subcapitol As String, articol As String, alineat As String)
            Me.IdClsf = idClsf
            Code = MakeCode(capitol, subcapitol, articol, alineat)
        End Sub
        Public ReadOnly Property IdClsf As Integer
        Public ReadOnly Property Code As String
        Public Property Total As Double
        Public Shared Function MakeCode(capitol As String, subcapitol As String, articol As String, alineat As String) As String
            Return $"{capitol}|{subcapitol}|{articol}|{alineat}"
        End Function
    End Class

    Private NotInheritable Class Target
        Public Sub New(idClsf As Integer, idClsfAcc As Integer)
            Me.IdClsf = idClsf
            Me.IdClsfAcc = idClsfAcc
        End Sub
        Public ReadOnly Property IdClsf As Integer
        Public ReadOnly Property IdClsfAcc As Integer
    End Class

End Class

''' <summary>One <c>Clasificatii_Buget</c> row the opening run would insert.</summary>
Public NotInheritable Class BudgetOpeningRow
    Public Sub New(idClsf As Integer, idUnitate As Integer, code As String, previousYearTotal As Double, trim1 As Double)
        Me.IdClsf = idClsf
        Me.IdUnitate = idUnitate
        Me.Code = code
        Me.PreviousYearTotal = previousYearTotal
        Me.Trim1 = trim1
    End Sub
    Public ReadOnly Property IdClsf As Integer
    Public ReadOnly Property IdUnitate As Integer
    ''' <summary>Capitol|Subcapitol|Articol|Alineat, for the log.</summary>
    Public ReadOnly Property Code As String
    Public ReadOnly Property PreviousYearTotal As Double
    Public ReadOnly Property Trim1 As Double
End Class

''' <summary>What <see cref="BudgetOpeningRunner.Plan"/> found, shown to the operator before anything is written.</summary>
Public NotInheritable Class BudgetOpeningPlan
    Public ReadOnly Property Rows As New List(Of BudgetOpeningRow)()
    ''' <summary>Classifications of the previous year that found no (or no single) match in MariaDB.</summary>
    Public ReadOnly Property Unmatched As New List(Of String)()
    ''' <summary>Units skipped (missing file ...), one sentence each.</summary>
    Public ReadOnly Property Notes As New List(Of String)()
    ''' <summary>Classifications that already have the 01.01 version, left untouched.</summary>
    Public Property AlreadyThere As Integer
End Class
