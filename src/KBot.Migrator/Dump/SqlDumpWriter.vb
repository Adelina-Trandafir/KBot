Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' Writes what the run sent, so a failure can be read afterwards instead of guessed at.
''' </summary>
''' <remarks>
''' <para>
''' Layout, one folder per run:
''' </para>
''' <code>
''' &lt;journal&gt;\&lt;DC&gt;\&lt;yyyyMMdd_HHmmss&gt;\
'''     _00_info.txt        server, database, files, units, operator, time
'''     Clasificatii.sql    one INSERT per row, values inline, terminated with ;
'''     ...
'''     _99_final.txt       COMMIT or ROLLBACK, totals, or the error and the last statement
''' </code>
''' <para>
''' Two warnings are written into the files themselves rather than only living here.
''' First, the .sql files are a <b>RECONSTRUCTION</b>: the driver sends parameters, not
''' text, so these are a rendering of the same values and not the bytes on the wire.
''' Second, <b>disk writing is not inside the transaction</b>, so on a rollback the .sql
''' files above _99_final.txt describe work that no longer exists.
''' </para>
''' <para>
''' The statement is written BEFORE it executes, and flushed per batch, so the failing row
''' is on disk even if the process is killed. Recording can never break a migration: any
''' disk failure is logged with its full stack, said once in the job log, and then
''' recording turns itself off. The one exception is the constructor - a missing journal
''' folder stops the run by name, with no silent fallback.
''' </para>
''' <para>
''' <b>No password ever reaches these files</b>, in any header, any statement or any error
''' text.
''' </para>
''' </remarks>
Public NotInheritable Class SqlDumpWriter
    Implements IDisposable

    Private ReadOnly _folder As String
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _writers As New Dictionary(Of String, StreamWriter)(StringComparer.OrdinalIgnoreCase)
    Private _disabled As Boolean
    Private _saidItFailed As Boolean
    Private _lastStatement As String = String.Empty
    Private _disposed As Boolean

    ''' <summary>Creates the dated run folder.</summary>
    ''' <exception cref="InvalidOperationException">
    ''' The journal root is not configured. Deliberately fatal: silently not recording a
    ''' migration is worse than refusing to start one.
    ''' </exception>
    Public Sub New(journalRoot As String, dc As String, log As Action(Of String))
        If String.IsNullOrWhiteSpace(journalRoot) Then
            Throw New InvalidOperationException(
                "Dosarul jurnalului SQL nu este configurat. Transferul nu pornește fără el, " &
                "ca să nu existe o migrare nescrisă nicăieri.")
        End If

        _log = log
        _folder = Path.Combine(journalRoot,
                               SafeName(dc),
                               DateTime.Now.ToString("yyyyMMdd_HHmmss", Globalization.CultureInfo.InvariantCulture))
        Directory.CreateDirectory(_folder)
    End Sub

    ''' <summary>The run folder, for the operator's log line.</summary>
    Public ReadOnly Property Folder As String
        Get
            Return _folder
        End Get
    End Property

    ''' <summary>The last statement handed to <see cref="WriteStatement"/>.</summary>
    Public ReadOnly Property LastStatement As String
        Get
            Return _lastStatement
        End Get
    End Property

    ''' <summary>Writes the run header.</summary>
    Public Sub WriteInfo(lines As IEnumerable(Of String))
        Try
            If _disabled Then Return
            Dim filePath = Path.Combine(_folder, "_00_info.txt")
            Using writer As New StreamWriter(filePath, False, New UTF8Encoding(False))
                writer.WriteLine("K-BOT — jurnalul unei migrări Access ▸ MariaDB")
                writer.WriteLine(New String("="c, 70))
                For Each line In lines
                    writer.WriteLine(line)
                Next
                writer.WriteLine()
                WriteDisclaimer(writer)
            End Using
        Catch ex As Exception
            Fail("SqlDumpWriter.WriteInfo", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Records one statement, BEFORE it is executed.
    ''' </summary>
    Public Sub WriteStatement(table As String, statement As String)
        _lastStatement = statement
        Try
            If _disabled Then Return
            Dim writer = WriterFor(table)
            If writer Is Nothing Then Return
            writer.WriteLine(statement)
            writer.WriteLine(";")
        Catch ex As Exception
            Fail("SqlDumpWriter.WriteStatement", ex)
        End Try
    End Sub

    ''' <summary>Records a comment line in one table's file.</summary>
    Public Sub WriteComment(table As String, comment As String)
        Try
            If _disabled Then Return
            Dim writer = WriterFor(table)
            If writer Is Nothing Then Return
            writer.WriteLine("-- " & comment.Replace(vbCr, " ").Replace(vbLf, " "))
        Catch ex As Exception
            Fail("SqlDumpWriter.WriteComment", ex)
        End Try
    End Sub

    ''' <summary>Flushes every open file. Called once per batch.</summary>
    Public Sub FlushAll()
        Try
            If _disabled Then Return
            For Each writer In _writers.Values
                writer.Flush()
            Next
        Catch ex As Exception
            Fail("SqlDumpWriter.FlushAll", ex)
        End Try
    End Sub

    ''' <summary>Writes the closing file: COMMIT with totals, or ROLLBACK with the error.</summary>
    Public Sub WriteFinal(committed As Boolean, totals As IEnumerable(Of String), error1 As Exception)
        Try
            If _disabled Then Return
            FlushAll()
            Dim filePath = Path.Combine(_folder, "_99_final.txt")
            Using writer As New StreamWriter(filePath, False, New UTF8Encoding(False))
                writer.WriteLine(If(committed, "COMMIT", "ROLLBACK"))
                writer.WriteLine(New String("="c, 70))
                writer.WriteLine($"Încheiat: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                writer.WriteLine()

                For Each line In totals
                    writer.WriteLine(line)
                Next

                If Not committed Then
                    writer.WriteLine()
                    writer.WriteLine("EROARE")
                    writer.WriteLine(New String("-"c, 70))
                    If error1 IsNot Nothing Then
                        writer.WriteLine(error1.GetType().FullName)
                        writer.WriteLine(error1.Message)
                        writer.WriteLine()
                        writer.WriteLine(error1.StackTrace)
                    Else
                        writer.WriteLine("Rularea a fost oprită de operator.")
                    End If
                    writer.WriteLine()
                    writer.WriteLine("ULTIMA INSTRUCȚIUNE ÎNAINTE DE OPRIRE")
                    writer.WriteLine(New String("-"c, 70))
                    writer.WriteLine(_lastStatement)
                    writer.WriteLine()
                    writer.WriteLine("ATENȚIE: tranzacția a fost derulată înapoi, deci fișierele .sql")
                    writer.WriteLine("de deasupra descriu muncă ce NU mai există pe server.")
                End If

                writer.WriteLine()
                WriteDisclaimer(writer)
            End Using
        Catch ex As Exception
            Fail("SqlDumpWriter.WriteFinal", ex)
        End Try
    End Sub

    Private Shared Sub WriteDisclaimer(writer As StreamWriter)
        writer.WriteLine(New String("-"c, 70))
        writer.WriteLine("Fișierele .sql din acest dosar sunt o RECONSTRUCȚIE, nu o transcriere:")
        writer.WriteLine("driverul trimite parametri, nu text, iar valorile de aici sunt redarea")
        writer.WriteLine("acelorași valori, nu octeții de pe fir.")
        writer.WriteLine("Scrierea pe disc NU face parte din tranzacție.")
        writer.WriteLine("Nicio parolă nu apare în acest dosar.")
    End Sub

    Private Function WriterFor(table As String) As StreamWriter
        Dim writer As StreamWriter = Nothing
        If _writers.TryGetValue(table, writer) Then Return writer

        Dim filePath = Path.Combine(_folder, SafeName(table) & ".sql")
        writer = New StreamWriter(filePath, False, New UTF8Encoding(False))
        writer.WriteLine($"-- {table}")
        writer.WriteLine("-- RECONSTRUCȚIE: driverul trimite parametri, nu text.")
        writer.WriteLine($"-- Început: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        writer.WriteLine()
        _writers(table) = writer
        Return writer
    End Function

    ''' <summary>
    ''' A disk failure disables recording; it never propagates into the migration.
    ''' </summary>
    Private Sub Fail(where As String, ex As Exception)
        GlobalErrorLog.Write(where, ex)
        _disabled = True
        If _saidItFailed Then Return
        _saidItFailed = True
        _log?.Invoke("Jurnalul SQL nu a putut fi scris și a fost dezactivat pentru restul " &
                     $"rulării: {ex.Message}. Transferul continuă.")
    End Sub

    Private Shared Function SafeName(value As String) As String
        If String.IsNullOrEmpty(value) Then Return "necunoscut"
        Dim sb As New StringBuilder(value.Length)
        Dim invalid = Path.GetInvalidFileNameChars()
        For Each c In value
            sb.Append(If(Array.IndexOf(invalid, c) >= 0, "_"c, c))
        Next
        Return sb.ToString()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        For Each writer In _writers.Values
            Try
                writer.Flush()
                writer.Dispose()
            Catch ex As Exception
                GlobalErrorLog.Write("SqlDumpWriter.Dispose", ex)
            End Try
        Next
        _writers.Clear()
    End Sub

End Class
