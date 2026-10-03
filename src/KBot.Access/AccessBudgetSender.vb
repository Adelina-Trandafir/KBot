Option Strict On
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0103-06: «Trimite in Access» of the «Clasificatii bugetare» window. Writes the budget and
''' the rectifications of ONE classification, as K-BOT holds them, into the Access file of its unit.
''' </summary>
''' <remarks>
''' <para>
''' <b>Which file.</b> The registry <c>cale.accdb</c> (table <c>cai</c>, the same one the Migrator
''' reads) names one unit file per <c>IdUnitate</c>; its <c>FullPath</c> is relative to the folder
''' holding <c>cale.accdb</c>, or absolute. The unit's own file is the correct database - never a
''' file picked by hand.
''' </para>
''' <para>
''' <b>What is written.</b> <c>Clasificatii.Trim1..4</c> of the row whose <c>IDClsf</c> is the
''' classification's <c>IdClsfAcc</c> (the budget version in force today), and every rectification of
''' the year into <c>Rectificari</c> (matched on IdClsf + Data + Document: updated when found, added
''' when not). Nothing is deleted: a rectification removed in K-BOT stays in Access. One transaction.
''' </para>
''' <para>
''' ACE is 64-bit on this estate (operator, 02.10.2026); no 32-bit path is attempted. The Access files
''' share ONE password (operator, 02.10.2026), kept in the code in obfuscated form (see
''' <see cref="AccessFilePassword"/> in KBot.Common); a file is opened without it first and with it when that fails.
''' Obfuscation only hides the text from a casual look at the binary - it is not protection.
''' </para>
''' <para>
''' <b>Where it lives (slice 0104).</b> In KBot.Access, an assembly only the package for Access clients
''' contains; KBot.App never references it (it reaches it through <see cref="IAccessBridge"/>).
''' </para>
''' </remarks>
Public NotInheritable Class AccessBudgetSender

    Private Shared ReadOnly Providers As String() = {"Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0"}


    Private Sub New()
    End Sub

    ''' <summary>
    ''' The unit file named by the registry for <paramref name="idUnitate"/>. Throws
    ''' <see cref="InvalidOperationException"/> (Romanian message) when the registry, the unit row or
    ''' the file is missing.
    ''' </summary>
    Public Shared Function ResolveUnitFile(registryPath As String, idUnitate As Integer) As String
        Try
            If Not File.Exists(registryPath) Then
                Throw New InvalidOperationException(
                    $"Registrul AVACONT «{registryPath}» nu există pe acest calculator.")
            End If
            Dim baseFolder As String = Path.GetDirectoryName(Path.GetFullPath(registryPath))
            Dim stored As String = String.Empty

            Using cn As OleDbConnection = OpenAccess(registryPath)
                Using cmd As New OleDbCommand("SELECT FullPath FROM [cai] WHERE IdUnitate = ?", cn)
                    cmd.Parameters.AddWithValue("@u", idUnitate)
                    Dim value As Object = cmd.ExecuteScalar()
                    If value IsNot Nothing AndAlso value IsNot DBNull.Value Then
                        stored = Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
                    End If
                End Using
            End Using

            If stored.Length = 0 Then
                Throw New InvalidOperationException(
                    $"Registrul nu are o cale pentru unitatea {idUnitate.ToString(CultureInfo.InvariantCulture)}.")
            End If
            Dim full As String = If(Path.IsPathRooted(stored), stored, Path.Combine(baseFolder, stored))
            full = Path.GetFullPath(full)
            If Not File.Exists(full) Then
                Throw New InvalidOperationException($"Fișierul unității «{full}» nu există.")
            End If
            Return full
        Catch ex As Exception
            GlobalErrorLog.Write("AccessBudgetSender.ResolveUnitFile", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The rows of the registry table <c>cai</c> of <paramref name="registryPath"/> whose <c>AnDate</c> is
    ''' <paramref name="an"/> and whose <c>SURSA</c> is <paramref name="sursa"/> (the year and the source
    ''' selected in K-BOT), every column as stored. Filtered here, not in SQL: the registry keeps the year as
    ''' text and the type of a parameter would have to guess it. A year of 0 or less, or an empty source, means «any»
    ''' (the test form uses that; the settings page always passes both). Throws <see cref="InvalidOperationException"/>
    ''' (Romanian message) when the file is missing or cannot be opened.
    ''' </summary>
    Public Shared Function ReadRegistry(registryPath As String, an As Integer, sursa As String) As DataTable
        Try
            If String.IsNullOrWhiteSpace(registryPath) OrElse Not File.Exists(registryPath) Then
                Throw New InvalidOperationException(
                    $"Registrul AVACONT «{registryPath}» nu există pe acest calculator.")
            End If
            Dim all As New DataTable("cai")
            Using cn As OleDbConnection = OpenAccess(registryPath)
                Using da As New OleDbDataAdapter("SELECT * FROM [cai]", cn)
                    da.Fill(all)
                End Using
            End Using

            Dim wantedYear As String = an.ToString(CultureInfo.InvariantCulture)
            Dim wantedSource As String = If(sursa, String.Empty).Trim()
            Dim result As DataTable = all.Clone()
            result.TableName = "cai"
            Dim anyYear As Boolean = an <= 0
            Dim anySource As Boolean = wantedSource.Length = 0
            If (anyYear OrElse all.Columns.Contains("AnDate")) AndAlso (anySource OrElse all.Columns.Contains("SURSA")) Then
                For Each row As DataRow In all.Rows
                    If (anyYear OrElse String.Equals(Text(row("AnDate")), wantedYear, StringComparison.Ordinal)) AndAlso
                       (anySource OrElse String.Equals(Text(row("SURSA")), wantedSource, StringComparison.OrdinalIgnoreCase)) Then
                        result.ImportRow(row)
                    End If
                Next
            End If
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("AccessBudgetSender.ReadRegistry", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Writes the budget version and the rectifications of the classification whose Access id is
    ''' <paramref name="idClsfAcc"/> into <paramref name="unitFile"/>, in one transaction.
    ''' </summary>
    Public Shared Function Send(unitFile As String, idClsfAcc As Integer, budget As BudgetVersion,
                                corrections As IEnumerable(Of RectificareBugetara)) As AccessSendResult
        ArgumentNullException.ThrowIfNull(corrections)
        Dim result As New AccessSendResult() With {.File = unitFile}
        Try
            Using cn As OleDbConnection = OpenAccess(unitFile)
                Using tx As OleDbTransaction = cn.BeginTransaction()
                    Try
                        If budget IsNot Nothing Then
                            result.BudgetUpdated = UpdateBudget(cn, tx, idClsfAcc, budget.Amounts)
                        End If
                        For Each c As RectificareBugetara In corrections
                            If Not c.Data.HasValue OrElse String.IsNullOrWhiteSpace(c.Document) Then Continue For
                            If UpsertCorrection(cn, tx, idClsfAcc, c) Then
                                result.RectificariInserted += 1
                            Else
                                result.RectificariUpdated += 1
                            End If
                        Next
                        tx.Commit()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("AccessBudgetSender.Send", ex)
            Throw
        End Try
    End Function

    ' ---- helpers, reached only through the wrapped methods above --------------------------------

    Private Shared Function OpenAccess(path As String) As OleDbConnection
        Dim failures As New List(Of String)()
        For Each provider As String In Providers
            For Each withPassword As Boolean In {False, True}
                Dim cn As OleDbConnection = Nothing
                Try
                    Dim text As String = $"Provider={provider};Data Source={path};Persist Security Info=False;"
                    If withPassword Then text &= $"Jet OLEDB:Database Password={AccessFilePassword.Value()};"
                    cn = New OleDbConnection(text)
                    cn.Open()
                    Return cn
                Catch ex As Exception
                    cn?.Dispose()
                    failures.Add($"{provider}{If(withPassword, " (cu parolă)", String.Empty)}: {ex.Message}")
                End Try
            Next
        Next
        Throw New InvalidOperationException(
            $"Fișierul «{path}» nu a putut fi deschis (este deschis exclusiv, are altă parolă sau lipsește " &
            "driverul Access pe 64 de biți)." & Environment.NewLine & String.Join(Environment.NewLine, failures))
    End Function

    Private Shared Function UpdateBudget(cn As OleDbConnection, tx As OleDbTransaction,
                                         idClsfAcc As Integer, amounts As QuarterAmounts) As Integer
        Using cmd As New OleDbCommand(
            "UPDATE [Clasificatii] SET [Trim1] = ?, [Trim2] = ?, [Trim3] = ?, [Trim4] = ? WHERE [IDClsf] = ?", cn, tx)
            cmd.Parameters.Add(Number("@t1", amounts.Trim1))
            cmd.Parameters.Add(Number("@t2", amounts.Trim2))
            cmd.Parameters.Add(Number("@t3", amounts.Trim3))
            cmd.Parameters.Add(Number("@t4", amounts.Trim4))
            cmd.Parameters.AddWithValue("@id", idClsfAcc)
            Return cmd.ExecuteNonQuery()
        End Using
    End Function

    ''' <summary>True when a row was added, False when an existing one was updated.</summary>
    Private Shared Function UpsertCorrection(cn As OleDbConnection, tx As OleDbTransaction,
                                             idClsfAcc As Integer, c As RectificareBugetara) As Boolean
        Dim exists As Boolean
        Using find As New OleDbCommand(
            "SELECT COUNT(*) FROM [Rectificari] WHERE [IdClsf] = ? AND [Data] = ? AND [Document] = ?", cn, tx)
            find.Parameters.AddWithValue("@id", idClsfAcc)
            find.Parameters.Add(DateParam("@d", c.Data.Value))
            find.Parameters.AddWithValue("@doc", c.Document.Trim())
            exists = Convert.ToInt32(find.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using

        If exists Then
            Using upd As New OleDbCommand(
                "UPDATE [Rectificari] SET [Trim1] = ?, [Trim2] = ?, [Trim3] = ?, [Trim4] = ? " &
                "WHERE [IdClsf] = ? AND [Data] = ? AND [Document] = ?", cn, tx)
                upd.Parameters.Add(Number("@t1", c.Amounts.Trim1))
                upd.Parameters.Add(Number("@t2", c.Amounts.Trim2))
                upd.Parameters.Add(Number("@t3", c.Amounts.Trim3))
                upd.Parameters.Add(Number("@t4", c.Amounts.Trim4))
                upd.Parameters.AddWithValue("@id", idClsfAcc)
                upd.Parameters.Add(DateParam("@d", c.Data.Value))
                upd.Parameters.AddWithValue("@doc", c.Document.Trim())
                upd.ExecuteNonQuery()
            End Using
            Return False
        End If

        ' Capitol..Alineat travel with a new row, like the legacy route does, so the Access row is
        ' complete; they come from the classification's own Access row (read first: ACE does not
        ' type a parameter that stands in a SELECT list).
        Dim parts(3) As String
        Using read As New OleDbCommand(
            "SELECT [Capitol], [Subcapitol], [Articol], [Alineat] FROM [Clasificatii] WHERE [IDClsf] = ?", cn, tx)
            read.Parameters.AddWithValue("@id", idClsfAcc)
            Using r As OleDbDataReader = read.ExecuteReader()
                If Not r.Read() Then
                    Throw New InvalidOperationException(
                        $"Clasificația cu id Access {idClsfAcc.ToString(CultureInfo.InvariantCulture)} nu există în fișierul Access.")
                End If
                For i As Integer = 0 To 3
                    parts(i) = If(r.IsDBNull(i), String.Empty, Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture))
                Next
            End Using
        End Using

        Using ins As New OleDbCommand(
            "INSERT INTO [Rectificari] ([IdClsf], [Capitol], [Subcapitol], [Articol], [Alineat], " &
            "[Data], [Document], [Trim1], [Trim2], [Trim3], [Trim4]) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", cn, tx)
            ins.Parameters.AddWithValue("@id", idClsfAcc)
            ins.Parameters.AddWithValue("@cap", parts(0))
            ins.Parameters.AddWithValue("@sub", parts(1))
            ins.Parameters.AddWithValue("@art", parts(2))
            ins.Parameters.AddWithValue("@ali", parts(3))
            ins.Parameters.Add(DateParam("@d", c.Data.Value))
            ins.Parameters.AddWithValue("@doc", c.Document.Trim())
            ins.Parameters.Add(Number("@t1", c.Amounts.Trim1))
            ins.Parameters.Add(Number("@t2", c.Amounts.Trim2))
            ins.Parameters.Add(Number("@t3", c.Amounts.Trim3))
            ins.Parameters.Add(Number("@t4", c.Amounts.Trim4))
            ins.ExecuteNonQuery()
        End Using
        Return True
    End Function

    Private Shared Function Text(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function Number(name As String, value As Decimal?) As OleDbParameter
        Dim p As New OleDbParameter(name, OleDbType.Double)
        p.Value = If(value.HasValue, CObj(CDbl(value.Value)), DBNull.Value)
        Return p
    End Function

    Private Shared Function DateParam(name As String, value As Date) As OleDbParameter
        Dim p As New OleDbParameter(name, OleDbType.Date)
        p.Value = value.Date
        Return p
    End Function

End Class
