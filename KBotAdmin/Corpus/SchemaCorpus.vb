Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>One column of a table, as the dump declares it.</summary>
Public NotInheritable Class SchemaColumn
    Public Property Name As String = String.Empty
    Public Property TypeText As String = String.Empty
    Public Property NotNull As Boolean
    Public Property IsPrimary As Boolean
End Class

''' <summary>A declared foreign key: columns of this table that point at columns of another table.</summary>
Public NotInheritable Class SchemaForeignKey
    Public Property Name As String = String.Empty
    Public Property Columns As New List(Of String)()
    Public Property RefSchemaName As String = String.Empty
    Public Property RefTable As String = String.Empty
    Public Property RefColumns As New List(Of String)()
    Public Property OnDelete As String = String.Empty
    ''' <summary>The table it points at, when that table is in the scanned dumps.</summary>
    Public Property Target As SchemaTable
End Class

''' <summary>One table of a dump with its columns, primary key and foreign keys.</summary>
Public NotInheritable Class SchemaTable
    ''' <summary>SURSA or COMUN.</summary>
    Public Property Schema As String = String.Empty
    Public Property Name As String = String.Empty
    ''' <summary>The part of the name before the first «_» («AD», «TZ», «FX»...), or «(fără prefix)».</summary>
    Public Property Prefix As String = String.Empty
    Public Property IndexCount As Integer
    Public ReadOnly Property Columns As New List(Of SchemaColumn)()
    Public ReadOnly Property ForeignKeys As New List(Of SchemaForeignKey)()
    ''' <summary>Tables this one points at (declared keys only, once each, never itself).</summary>
    Public ReadOnly Property RefersTo As New List(Of SchemaTable)()
    ''' <summary>Tables that point at this one.</summary>
    Public ReadOnly Property ReferencedBy As New List(Of SchemaTable)()

    Public ReadOnly Property Key As String
        Get
            Return Schema & "." & Name
        End Get
    End Property

    Public ReadOnly Property GroupKey As String
        Get
            Return Schema & ":" & Prefix
        End Get
    End Property

    Public ReadOnly Property PrimaryKeyColumns As IEnumerable(Of String)
        Get
            Return Columns.Where(Function(k_c) k_c.IsPrimary).Select(Function(k_c) k_c.Name)
        End Get
    End Property
End Class

''' <summary>A link between two tables made by one or more foreign keys.</summary>
Public NotInheritable Class SchemaLink
    Public Property FromTable As SchemaTable
    Public Property ToTable As SchemaTable
End Class

''' <summary>What the scan of the schema dumps found.</summary>
Public NotInheritable Class SchemaCorpus
    Public Property Folder As String = String.Empty
    Public ReadOnly Property Tables As New List(Of SchemaTable)()
    Public ReadOnly Property Links As New List(Of SchemaLink)()
    ''' <summary>Schema label to the file it was read from and when that file was last written.</summary>
    Public ReadOnly Property Sources As New Dictionary(Of String, Tuple(Of String, Date))(StringComparer.OrdinalIgnoreCase)
    ''' <summary>Foreign keys that point at a table no scanned dump contains.</summary>
    Public Property UnresolvedKeys As Integer
End Class

''' <summary>
''' Reads the table structures out of the schema dumps kept in <c>MariaDB_Schema\</c> (Navicat format: «DROP TABLE IF EXISTS» then
''' «CREATE TABLE»). Only the structure is read; the data lines (the COMUN dump is ~10 MB, mostly rows) are skipped. A CREATE TABLE
''' counts only right after the DROP of the same name, so SQL text stored inside data rows is never mistaken for a table.
''' </summary>
Public Module SchemaScanner

    Private ReadOnly Sources As (Label As String, FileName As String)() = {
        ("SURSA", "AVACONT_SURSA.sql"),
        ("COMUN", "AVACONT_COMUN.sql")
    }

    Private ReadOnly DropPattern As New Regex("^DROP TABLE IF EXISTS `([^`]+)`", RegexOptions.Compiled)
    Private ReadOnly CreatePattern As New Regex("^CREATE TABLE `([^`]+)`", RegexOptions.Compiled)
    Private ReadOnly ColumnPattern As New Regex("^`([^`]+)`\s+(\S+)(.*)$", RegexOptions.Compiled)
    Private ReadOnly BacktickPattern As New Regex("`([^`]+)`", RegexOptions.Compiled)
    Private ReadOnly ForeignKeyPattern As New Regex("CONSTRAINT\s+`([^`]+)`\s+FOREIGN KEY\s*\(([^)]*)\)\s*REFERENCES\s+(?:`([^`]+)`\.)?`([^`]+)`\s*\(([^)]*)\)(.*)$", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
    Private ReadOnly OnDeletePattern As New Regex("ON DELETE\s+(CASCADE|SET NULL|SET DEFAULT|NO ACTION|RESTRICT)", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

    ''' <summary>The folder with the dumps, next to KBot.sln (it is on the owner's disk, not in git), or empty.</summary>
    Public Function FindFolder(k_hint As String) As String
        Dim k_root As String = MdScanner.FindRoot(k_hint)
        If k_root.Length = 0 Then Return String.Empty
        Dim k_folder As String = Path.Combine(k_root, "MariaDB_Schema")
        Return If(Directory.Exists(k_folder), k_folder, String.Empty)
    End Function

    Public Function Scan(k_folder As String) As SchemaCorpus
        Try
            If String.IsNullOrWhiteSpace(k_folder) OrElse Not Directory.Exists(k_folder) Then
                Throw New AdminException("Nu găsesc folderul MariaDB_Schema (e pe discul tău, nu vine din git).")
            End If
            Dim k_corpus As New SchemaCorpus With {.Folder = k_folder}
            For Each k_source In Sources
                Dim k_path As String = Path.Combine(k_folder, k_source.FileName)
                If Not File.Exists(k_path) Then Continue For
                k_corpus.Sources(k_source.Label) = Tuple.Create(k_source.FileName, File.GetLastWriteTime(k_path))
                ReadDump(k_path, k_source.Label, k_corpus)
            Next
            If k_corpus.Tables.Count = 0 Then Throw New AdminException("Dump-urile din MariaDB_Schema nu conțin niciun tabel (AVACONT_SURSA.sql, AVACONT_COMUN.sql).")
            Resolve(k_corpus)
            Return k_corpus
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaScanner.Scan", ex)
            Throw
        End Try
    End Function

    Private Sub ReadDump(k_path As String, k_label As String, k_corpus As SchemaCorpus)
        Dim k_lastDrop As String = Nothing
        Dim k_current As SchemaTable = Nothing
        For Each k_raw As String In File.ReadLines(k_path, Encoding.UTF8)
            If k_current Is Nothing Then
                Dim k_drop As Match = DropPattern.Match(k_raw)
                If k_drop.Success Then
                    k_lastDrop = k_drop.Groups(1).Value
                    Continue For
                End If
                Dim k_create As Match = CreatePattern.Match(k_raw)
                If k_create.Success AndAlso k_lastDrop IsNot Nothing AndAlso String.Equals(k_create.Groups(1).Value, k_lastDrop, StringComparison.OrdinalIgnoreCase) Then
                    k_current = New SchemaTable With {.Schema = k_label, .Name = k_create.Groups(1).Value}
                    k_current.Prefix = PrefixOf(k_current.Name)
                    k_lastDrop = Nothing
                End If
                Continue For
            End If

            If k_raw.StartsWith(")", StringComparison.Ordinal) Then
                k_corpus.Tables.Add(k_current)
                k_current = Nothing
                Continue For
            End If
            ReadDefinition(k_raw.Trim(), k_current)
        Next
    End Sub

    Private Sub ReadDefinition(k_line As String, k_table As SchemaTable)
        If k_line.StartsWith("`", StringComparison.Ordinal) Then
            Dim k_match As Match = ColumnPattern.Match(k_line)
            If k_match.Success Then
                k_table.Columns.Add(New SchemaColumn With {
                    .Name = k_match.Groups(1).Value,
                    .TypeText = k_match.Groups(2).Value.TrimEnd(","c),
                    .NotNull = k_match.Groups(3).Value.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase)})
            End If
        ElseIf k_line.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) Then
            Dim k_open As Integer = k_line.IndexOf("("c)
            Dim k_close As Integer = k_line.IndexOf(")"c, k_open + 1)
            If k_open >= 0 AndAlso k_close > k_open Then
                For Each k_name As String In BackticksIn(k_line.Substring(k_open + 1, k_close - k_open - 1))
                    Dim k_column As SchemaColumn = k_table.Columns.FirstOrDefault(Function(k_c) String.Equals(k_c.Name, k_name, StringComparison.OrdinalIgnoreCase))
                    If k_column IsNot Nothing Then k_column.IsPrimary = True
                Next
            End If
        ElseIf k_line.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase) Then
            Dim k_match As Match = ForeignKeyPattern.Match(k_line)
            If k_match.Success Then
                Dim k_key As New SchemaForeignKey With {
                    .Name = k_match.Groups(1).Value,
                    .RefSchemaName = k_match.Groups(3).Value,
                    .RefTable = k_match.Groups(4).Value}
                k_key.Columns.AddRange(BackticksIn(k_match.Groups(2).Value))
                k_key.RefColumns.AddRange(BackticksIn(k_match.Groups(5).Value))
                Dim k_delete As Match = OnDeletePattern.Match(k_match.Groups(6).Value)
                If k_delete.Success Then k_key.OnDelete = k_delete.Groups(1).Value.ToUpperInvariant()
                k_table.ForeignKeys.Add(k_key)
            End If
        ElseIf k_line.StartsWith("INDEX", StringComparison.OrdinalIgnoreCase) OrElse k_line.StartsWith("UNIQUE", StringComparison.OrdinalIgnoreCase) OrElse
               k_line.StartsWith("FULLTEXT", StringComparison.OrdinalIgnoreCase) OrElse k_line.StartsWith("KEY", StringComparison.OrdinalIgnoreCase) Then
            k_table.IndexCount += 1
        End If
    End Sub

    Private Function BackticksIn(k_text As String) As List(Of String)
        Dim k_result As New List(Of String)()
        For Each k_match As Match In BacktickPattern.Matches(k_text)
            k_result.Add(k_match.Groups(1).Value)
        Next
        Return k_result
    End Function

    ''' <summary>The family of a table: the part of the name before the first underscore, or «(fără prefix)».</summary>
    Public Function PrefixOf(k_name As String) As String
        Dim k_cut As Integer = k_name.IndexOf("_"c)
        Return If(k_cut >= 1 AndAlso k_cut <= 8, k_name.Substring(0, k_cut).ToUpperInvariant(), "(fără prefix)")
    End Function

    ''' <summary>Joins each foreign key to its target table and builds the one-per-pair links.</summary>
    Private Sub Resolve(k_corpus As SchemaCorpus)
        Dim k_bySchema As New Dictionary(Of String, SchemaTable)(StringComparer.OrdinalIgnoreCase)
        For Each k_table As SchemaTable In k_corpus.Tables
            k_bySchema(k_table.Key) = k_table
        Next
        Dim k_pairs As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_table As SchemaTable In k_corpus.Tables
            For Each k_key As SchemaForeignKey In k_table.ForeignKeys
                Dim k_target As SchemaTable = Nothing
                Dim k_schema As String = SchemaLabelOf(k_key.RefSchemaName, k_table.Schema)
                If Not k_bySchema.TryGetValue(k_schema & "." & k_key.RefTable, k_target) Then
                    k_corpus.UnresolvedKeys += 1
                    Continue For
                End If
                k_key.Target = k_target
                If k_target Is k_table Then Continue For
                If Not k_pairs.Add(k_table.Key & ">" & k_target.Key) Then Continue For
                k_table.RefersTo.Add(k_target)
                k_target.ReferencedBy.Add(k_table)
                k_corpus.Links.Add(New SchemaLink With {.FromTable = k_table, .ToTable = k_target})
            Next
        Next
    End Sub

    ''' <summary>«AVACONT_COMUN» to «COMUN»; no schema written = the schema of the table that holds the key.</summary>
    Private Function SchemaLabelOf(k_written As String, k_own As String) As String
        If String.IsNullOrEmpty(k_written) Then Return k_own
        For Each k_source In Sources
            If k_written.EndsWith(k_source.Label, StringComparison.OrdinalIgnoreCase) Then Return k_source.Label
        Next
        Return k_own
    End Function

End Module
