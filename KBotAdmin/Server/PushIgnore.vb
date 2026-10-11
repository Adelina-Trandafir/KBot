Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' A .pushignore file, read with gitignore rules (the same file AvacontPush reads): <c>#</c> comment,
''' <c>name</c> = any folder or file with that name at any depth, <c>/name</c> = only at the root,
''' <c>name/</c> = only a folder, <c>*.log</c> = by extension, <c>a/**/b</c> = any number of folders,
''' <c>!name</c> = put back what an earlier rule took out. The LAST matching rule wins.
''' </summary>
Public NotInheritable Class PushIgnore

    Public Const FileNameOnly As String = ".pushignore"

    Private NotInheritable Class Rule
        Public Property Matcher As Regex
        Public Property Negated As Boolean
    End Class

    Private ReadOnly _rules As New List(Of Rule)()

    ''' <summary>How many usable rules were read. Zero when there is no file.</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _rules.Count
        End Get
    End Property

    ''' <summary>Full path of the file the rules came from, or empty when absent.</summary>
    Public Property SourcePath As String = String.Empty

    ''' <summary>
    ''' Reads .pushignore from the root of the pushed tree. A missing file means no extra rules; one that
    ''' exists but cannot be read is an error (pushing what the operator wanted to keep back is worse).
    ''' </summary>
    Public Shared Function Load(k_rootPath As String) As PushIgnore
        Dim k_result As New PushIgnore()
        If String.IsNullOrWhiteSpace(k_rootPath) Then Return k_result
        Dim k_fullPath As String = Path.Combine(k_rootPath, FileNameOnly)
        If Not File.Exists(k_fullPath) Then Return k_result
        Dim k_lines As String()
        Try
            k_lines = File.ReadAllLines(k_fullPath, Encoding.UTF8)
        Catch ex As IOException
            GlobalErrorLog.Write("PushIgnore.Load", ex)
            Throw New ApplicationException($"Nu s-a putut citi fișierul {FileNameOnly} din {k_rootPath}.", ex)
        End Try
        k_result.SourcePath = k_fullPath
        For Each k_raw As String In k_lines
            k_result.AddRule(k_raw)
        Next
        Return k_result
    End Function

    Public Sub AddRule(k_rawLine As String)
        Dim k_line As String = If(k_rawLine, String.Empty).Trim()
        If k_line = String.Empty OrElse k_line.StartsWith("#", StringComparison.Ordinal) Then Return
        Dim k_negated As Boolean = False
        If k_line.StartsWith("!", StringComparison.Ordinal) Then
            k_negated = True
            k_line = k_line.Substring(1).Trim()
            If k_line = String.Empty Then Return
        End If
        Dim k_dirOnly As Boolean = k_line.EndsWith("/", StringComparison.Ordinal)
        If k_dirOnly Then k_line = k_line.TrimEnd("/"c)
        If k_line = String.Empty Then Return
        ' Anchored to the root when it carries a slash anywhere but at the end (git's rule).
        Dim k_anchored As Boolean = k_line.StartsWith("/", StringComparison.Ordinal)
        If k_anchored Then k_line = k_line.TrimStart("/"c)
        If Not k_anchored AndAlso k_line.Contains("/") Then k_anchored = True
        If k_line = String.Empty Then Return
        _rules.Add(New Rule With {.Negated = k_negated, .Matcher = BuildMatcher(k_line, k_anchored, k_dirOnly)})
    End Sub

    ''' <summary>True/False when a rule had an opinion about the path, Nothing when none did.</summary>
    Public Function Match(k_relativePath As String) As Boolean?
        If _rules.Count = 0 Then Return Nothing
        Dim k_rel As String = If(k_relativePath, String.Empty).Replace("\"c, "/"c).TrimStart("/"c)
        If k_rel = String.Empty Then Return Nothing
        Dim k_verdict As Boolean? = Nothing
        For Each k_rule As Rule In _rules
            If k_rule.Matcher.IsMatch(k_rel) Then k_verdict = Not k_rule.Negated
        Next
        Return k_verdict
    End Function

    Private Shared Function BuildMatcher(k_glob As String, k_anchored As Boolean, k_dirOnly As Boolean) As Regex
        Dim k_body As New StringBuilder()
        Dim k_i As Integer = 0
        Do While k_i < k_glob.Length
            Dim k_ch As Char = k_glob(k_i)
            Select Case k_ch
                Case "*"c
                    If k_i + 1 < k_glob.Length AndAlso k_glob(k_i + 1) = "*"c Then
                        If k_i + 2 < k_glob.Length AndAlso k_glob(k_i + 2) = "/"c Then
                            k_body.Append("(?:.*/)?")
                            k_i += 3
                            Continue Do
                        End If
                        k_body.Append(".*")
                        k_i += 2
                        Continue Do
                    End If
                    k_body.Append("[^/]*")
                Case "?"c
                    k_body.Append("[^/]")
                Case "/"c
                    k_body.Append("/"c)
                Case Else
                    k_body.Append(Regex.Escape(k_ch.ToString()))
            End Select
            k_i += 1
        Loop
        Dim k_prefix As String = If(k_anchored, "^", "^(?:.*/)?")
        Dim k_suffix As String = If(k_dirOnly, "/.*$", "(?:/.*)?$")
        Return New Regex(k_prefix & k_body.ToString() & k_suffix, RegexOptions.IgnoreCase Or RegexOptions.Compiled)
    End Function

End Class
