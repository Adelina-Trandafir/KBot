Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json

''' <summary>What a parameter of a catalog query is: decides how the typed text is checked and how it travels to the server.</summary>
Public Enum QueryParamType
    Text
    WholeNumber
    Number
    [Date]
End Enum

''' <summary>One parameter of a catalog query: the name used as <c>:name</c> in the text, the label and the explanation the operator reads, and its type.</summary>
Public NotInheritable Class QueryParamDef

    Public Sub New(k_name As String, k_label As String, k_type As QueryParamType, k_required As Boolean, k_explanation As String)
        Name = k_name
        Label = k_label
        ParamType = k_type
        Required = k_required
        Explanation = k_explanation
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property Label As String
    Public ReadOnly Property ParamType As QueryParamType

    ''' <summary>False = the field may stay empty and then travels as NULL (the text can say <c>(:x IS NULL OR col = :x)</c>).</summary>
    Public ReadOnly Property Required As Boolean
    Public ReadOnly Property Explanation As String

    ''' <summary>
    ''' The typed text as the value the server takes (text, whole number, decimal number, or an ISO date), or Nothing for an empty optional field.
    ''' <paramref name="k_problem"/> is empty when the text is fine, else what is wrong with it (the operator reads it).
    ''' </summary>
    Public Function Parse(k_typed As String, ByRef k_problem As String) As Object
        k_problem = String.Empty
        Dim k_text As String = If(k_typed, String.Empty).Trim()
        If k_text.Length = 0 Then
            If Required Then k_problem = "«" & Label & "» trebuie completat."
            Return Nothing
        End If
        Select Case ParamType
            Case QueryParamType.WholeNumber
                Dim k_whole As Long
                If Long.TryParse(k_text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, k_whole) Then Return k_whole
                k_problem = "«" & Label & "» trebuie să fie un număr întreg."
            Case QueryParamType.Number
                Dim k_number As Decimal
                If Decimal.TryParse(k_text.Replace(","c, "."c), NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, k_number) Then Return k_number
                k_problem = "«" & Label & "» trebuie să fie un număr (zecimalele cu virgulă sau punct)."
            Case QueryParamType.Date
                Dim k_date As DateTime
                If DateTime.TryParseExact(k_text, {"dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then
                    Return k_date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                End If
                k_problem = "«" & Label & "» trebuie să fie o dată, de forma 31.12.2026."
            Case Else
                Return k_text
        End Select
        Return Nothing
    End Function

End Class

''' <summary>One query of the catalog: its title and explanation, its parameters, and the text that runs on the chosen database.</summary>
Public NotInheritable Class QueryDef

    Public Sub New(k_id As String, k_title As String, k_description As String, k_params As IReadOnlyList(Of QueryParamDef), k_sql As String)
        Id = k_id
        Title = k_title
        Description = k_description
        Params = k_params
        Sql = k_sql
    End Sub

    ''' <summary>The file name without its extension: stable, used as the key of the tree row.</summary>
    Public ReadOnly Property Id As String
    Public ReadOnly Property Title As String
    Public ReadOnly Property Description As String
    Public ReadOnly Property Params As IReadOnlyList(Of QueryParamDef)

    ''' <summary>The text sent to the server: the file without its <c>-- @</c> header lines.</summary>
    Public ReadOnly Property Sql As String

    ''' <summary>The arguments of the <c>query-run</c> verb for this query on one database, with the values typed (name -> value as <see cref="QueryParamDef.Parse"/> returned them).</summary>
    Public Function BuildArgs(k_db As String, k_values As IDictionary(Of String, Object), k_limit As Integer) As String()
        Dim k_args As New List(Of String) From {"--db", k_db, "--sql-b64", Convert.ToBase64String(Encoding.UTF8.GetBytes(Sql))}
        If Params.Count > 0 Then
            Dim k_json As String = JsonSerializer.Serialize(k_values)
            k_args.AddRange({"--params-b64", Convert.ToBase64String(Encoding.UTF8.GetBytes(k_json))})
        End If
        k_args.AddRange({"--limit", k_limit.ToString(CultureInfo.InvariantCulture)})
        Return k_args.ToArray()
    End Function

End Class

''' <summary>What a read of the queries folder found: the queries that read well, and one line (file id, what is wrong) for each file that does not.</summary>
Public NotInheritable Class QueryCatalogResult

    Public Sub New(k_folder As String, k_folderFound As Boolean, k_queries As IReadOnlyList(Of QueryDef), k_problems As IReadOnlyList(Of KeyValuePair(Of String, String)))
        Folder = k_folder
        FolderFound = k_folderFound
        Queries = k_queries
        Problems = k_problems
    End Sub

    Public ReadOnly Property Folder As String
    Public ReadOnly Property FolderFound As Boolean
    Public ReadOnly Property Queries As IReadOnlyList(Of QueryDef)
    Public ReadOnly Property Problems As IReadOnlyList(Of KeyValuePair(Of String, String))

End Class

''' <summary>
''' The queries of the tree group «_Interogari» of the page «Tabele». They are plain files, <c>KBotAdmin\Interogari\*.sql</c> in the repository (the folder
''' is found like the PYTHON folder: walking up from the exe to KBot.sln; the setting <c>QueriesFolder</c> of admin_settings.json overrides it), and
''' they are read EVERY time they are needed, never from the exe: a new or changed file shows after «Reîncarcă» (the title and the list) or at the next click
''' on its row (the parameters and the text). Nothing to build or publish, and a publish of KBotAdmin cannot lose them. Nothing is created in the
''' databases and nothing is kept on the server (the text travels with each run, see <c>admin_query.py</c>). A file is read-only SQL under a header of
''' comment lines:
''' <code>
''' -- @title Tabele cu o coloană
''' -- @description What it does, in a few words (several lines are joined).
''' -- @param coloana | Coloana | text | What to type here
''' -- @param pana_la? | Până la data | date | A «?» after the name makes the field optional (empty = NULL)
''' SELECT ... WHERE x = :coloana
''' </code>
''' The types are text, int, number and date. A file whose header cannot be read does not stop the others: it shows in the tree as a row marked as wrong,
''' and the row says what is wrong with it.
''' </summary>
Public NotInheritable Class QueryCatalog

    Private Const Marker As String = "-- @"

    Private Sub New()
    End Sub

    ''' <summary>Reads the whole folder: the queries ordered by title, and the files that do not read well. A missing folder is not an error, the result says so.</summary>
    Public Shared Function Load(k_folder As String) As QueryCatalogResult
        Try
            Dim k_queries As New List(Of QueryDef)()
            Dim k_problems As New List(Of KeyValuePair(Of String, String))()
            Dim k_found As Boolean = Not String.IsNullOrWhiteSpace(k_folder) AndAlso Directory.Exists(k_folder)
            If k_found Then
                For Each k_file As String In Directory.EnumerateFiles(k_folder, "*.sql").OrderBy(Function(k_f) k_f, StringComparer.OrdinalIgnoreCase)
                    Dim k_id As String = Path.GetFileNameWithoutExtension(k_file)
                    Try
                        k_queries.Add(Parse(k_id, File.ReadAllText(k_file, Encoding.UTF8)))
                    Catch ex As ArgumentException
                        ' A mistake in the header of one file: shown as a row of the tree, the other queries stay.
                        GlobalErrorLog.Write("QueryCatalog.Load", ex)
                        k_problems.Add(New KeyValuePair(Of String, String)(k_id, ex.Message))
                    End Try
                Next
            End If
            Return New QueryCatalogResult(k_folder, k_found, k_queries.OrderBy(Function(k_q) k_q.Title, StringComparer.CurrentCultureIgnoreCase).ToList(), k_problems)
        Catch ex As Exception
            GlobalErrorLog.Write("QueryCatalog.Load", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' One query, read from its file NOW (so an edit made since the tree was built is used). Nothing = no such file. A header that cannot be read
    ''' throws <see cref="ArgumentException"/> whose message is for the operator.
    ''' </summary>
    Public Shared Function Find(k_folder As String, k_id As String) As QueryDef
        Try
            If String.IsNullOrWhiteSpace(k_folder) OrElse String.IsNullOrEmpty(k_id) OrElse Not String.Equals(k_id, Path.GetFileName(k_id), StringComparison.Ordinal) Then Return Nothing
            Dim k_path As String = Path.Combine(k_folder, k_id & ".sql")
            If Not File.Exists(k_path) Then Return Nothing
            Return Parse(k_id, File.ReadAllText(k_path, Encoding.UTF8))
        Catch ex As ArgumentException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("QueryCatalog.Find", ex)
            Throw
        End Try
    End Function

    ''' <summary>Reads one file: the header lines give the title, the explanation and the parameters; the rest is the text that runs.</summary>
    Friend Shared Function Parse(k_id As String, k_file As String) As QueryDef
        Dim k_title As String = String.Empty
        Dim k_description As New List(Of String)()
        Dim k_params As New List(Of QueryParamDef)()
        Dim k_body As New StringBuilder()
        For Each k_raw As String In k_file.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
            Dim k_line As String = k_raw.TrimEnd()
            If Not k_line.TrimStart().StartsWith(Marker, StringComparison.Ordinal) Then
                k_body.AppendLine(k_line)
                Continue For
            End If
            Dim k_head As String = k_line.TrimStart().Substring(Marker.Length)
            Dim k_space As Integer = k_head.IndexOf(" "c)
            Dim k_key As String = If(k_space < 0, k_head, k_head.Substring(0, k_space))
            Dim k_value As String = If(k_space < 0, String.Empty, k_head.Substring(k_space + 1).Trim())
            Select Case k_key
                Case "title"
                    k_title = k_value
                Case "description"
                    k_description.Add(k_value)
                Case "param"
                    k_params.Add(ParseParam(k_id, k_value))
                Case Else
                    Throw New ArgumentException("Interogarea «" & k_id & "»: antet necunoscut «-- @" & k_key & "».")
            End Select
        Next
        If k_title.Length = 0 Then Throw New ArgumentException("Interogarea «" & k_id & "» nu are titlu (-- @title ...).")
        Dim k_sql As String = k_body.ToString().Trim()
        If k_sql.Length = 0 Then Throw New ArgumentException("Interogarea «" & k_id & "» nu are text SQL.")
        For Each k_p As QueryParamDef In k_params
            If Not k_sql.Contains(":" & k_p.Name, StringComparison.Ordinal) Then
                Throw New ArgumentException("Interogarea «" & k_id & "»: parametrul «" & k_p.Name & "» nu apare în text ca :" & k_p.Name & ".")
            End If
        Next
        Return New QueryDef(k_id, k_title, String.Join(" ", k_description), k_params, k_sql)
    End Function

    Private Shared Function ParseParam(k_id As String, k_value As String) As QueryParamDef
        Dim k_parts As String() = k_value.Split("|"c).Select(Function(k_s) k_s.Trim()).ToArray()
        If k_parts.Length < 4 Then
            Throw New ArgumentException("Interogarea «" & k_id & "»: -- @param cere «nume | etichetă | tip | explicație».")
        End If
        Dim k_name As String = k_parts(0)
        Dim k_required As Boolean = Not k_name.EndsWith("?", StringComparison.Ordinal)
        If Not k_required Then k_name = k_name.Substring(0, k_name.Length - 1)
        If k_name.Length = 0 OrElse Not (Char.IsLetter(k_name(0)) OrElse k_name(0) = "_"c) OrElse k_name.Any(Function(k_c) Not (Char.IsLetterOrDigit(k_c) OrElse k_c = "_"c)) Then
            Throw New ArgumentException("Interogarea «" & k_id & "»: numele parametrului «" & k_name & "» nu e valid (litere, cifre, _).")
        End If
        Dim k_type As QueryParamType
        Select Case k_parts(2).ToLowerInvariant()
            Case "text" : k_type = QueryParamType.Text
            Case "int" : k_type = QueryParamType.WholeNumber
            Case "number" : k_type = QueryParamType.Number
            Case "date" : k_type = QueryParamType.Date
            Case Else
                Throw New ArgumentException("Interogarea «" & k_id & "»: tipul «" & k_parts(2) & "» al parametrului «" & k_name & "» nu există (text, int, number, date).")
        End Select
        Return New QueryParamDef(k_name, k_parts(1), k_type, k_required, String.Join(" | ", k_parts.Skip(3)))
    End Function

End Class
