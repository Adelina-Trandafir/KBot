Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' A .pushignore file, read with gitignore rules.
''' </summary>
''' <remarks>
''' The JSON IgnorePatterns list stays what it is: a short, fixed list of
''' build/IDE noise that every push should skip. This is the other half --
''' a file that lives WITH the tree being pushed, editable without touching
''' the app's configuration, and expressive enough to name a single folder,
''' a single file, or an extension.
'''
''' Supported, same as git:
'''   # comment            a whole-line comment
'''   bin                  any segment called bin, and everything under it
'''   /schema_diff         only at the root of the pushed tree
'''   logs/                only when it is a folder
'''   *.log                by extension, in any folder
'''   docs/**/*.tmp        ** spans any number of folders
'''   !pastreaza.log       negation: put back something an earlier rule took out
'''
''' Order matters and the LAST matching rule wins, so a negation has to come
''' after the rule it undoes -- exactly as in .gitignore.
''' </remarks>
Public Class PushIgnore

    Public Const FileName As String = ".pushignore"

    Private Class Rule
        Public Property Pattern As String
        Public Property Matcher As Regex
        Public Property Negated As Boolean
        Public Property DirectoryOnly As Boolean
    End Class

    Private ReadOnly _rules As New List(Of Rule)()

    ''' <summary>How many usable rules were read. Zero when there is no file.</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _rules.Count
        End Get
    End Property

    ''' <summary>Full path of the file the rules came from, or "" when absent.</summary>
    Public Property SourcePath As String = ""

    ''' <summary>
    ''' Reads .pushignore from the root of the pushed tree. A missing file is
    ''' not an error -- it simply means no extra rules. A file that exists but
    ''' cannot be read IS an error and is thrown, never swallowed: silently
    ''' pushing what the operator asked to keep back is worse than stopping.
    ''' </summary>
    Public Shared Function Load(rootPath As String) As PushIgnore
        Dim result As New PushIgnore()
        If String.IsNullOrWhiteSpace(rootPath) Then Return result

        Dim fullPath = Path.Combine(rootPath, FileName)
        If Not File.Exists(fullPath) Then Return result

        Dim lines As String()
        Try
            lines = File.ReadAllLines(fullPath, Encoding.UTF8)
        Catch ex As IOException
            Throw New ApplicationException(
                $"Nu s-a putut citi fișierul {FileName} din {rootPath}.", ex)
        End Try

        result.SourcePath = fullPath
        For Each raw In lines
            result.AddRule(raw)
        Next
        Return result
    End Function

    ''' <summary>Adds one line. Blank lines and comments are skipped.</summary>
    Public Sub AddRule(rawLine As String)
        Dim line = If(rawLine, "").Trim()
        If line = "" Then Return
        If line.StartsWith("#", StringComparison.Ordinal) Then Return

        Dim negated = False
        If line.StartsWith("!", StringComparison.Ordinal) Then
            negated = True
            line = line.Substring(1).Trim()
            If line = "" Then Return
        End If

        Dim dirOnly = line.EndsWith("/", StringComparison.Ordinal)
        If dirOnly Then line = line.TrimEnd("/"c)
        If line = "" Then Return

        ' A pattern is anchored to the root when it carries a slash anywhere
        ' but at the end -- git's rule. "bin" matches any bin; "a/bin" only
        ' the one directly under a.
        Dim anchored = line.StartsWith("/", StringComparison.Ordinal)
        If anchored Then line = line.TrimStart("/"c)
        If Not anchored AndAlso line.Contains("/") Then anchored = True
        If line = "" Then Return

        _rules.Add(New Rule With {
            .Pattern = rawLine.Trim(),
            .Negated = negated,
            .DirectoryOnly = dirOnly,
            .Matcher = BuildMatcher(line, anchored, dirOnly)
        })
    End Sub

    ''' <summary>
    ''' Whether this path is held back. Nothing = no rule had an opinion, so
    ''' the caller keeps whatever it already decided.
    ''' </summary>
    ''' <param name="relativePath">Path relative to the pushed root.</param>
    Public Function Match(relativePath As String) As Boolean?
        If _rules.Count = 0 Then Return Nothing

        Dim rel = If(relativePath, "").Replace("\"c, "/"c).TrimStart("/"c)
        If rel = "" Then Return Nothing

        Dim verdict As Boolean? = Nothing
        ' Last match wins, so the scan cannot stop early: a later negation
        ' is allowed to undo an earlier rule.
        For Each r In _rules
            If r.Matcher.IsMatch(rel) Then
                verdict = Not r.Negated
            End If
        Next
        Return verdict
    End Function

    ''' <summary>True when the path is held back. Convenience over Match.</summary>
    Public Function IsIgnored(relativePath As String) As Boolean
        Dim verdict = Match(relativePath)
        Return verdict.HasValue AndAlso verdict.Value
    End Function

    ''' <summary>
    ''' Turns one glob into a regex over the whole relative path.
    ''' </summary>
    ''' <remarks>
    ''' Every rule matches the entry itself OR anything beneath it, which is
    ''' what makes a folder name hold back the folder's contents. A
    ''' directory-only rule drops the "itself" half: it must be followed by a
    ''' slash, so "logs/" never matches a FILE called logs.
    ''' </remarks>
    Private Shared Function BuildMatcher(glob As String, anchored As Boolean,
                                         dirOnly As Boolean) As Regex
        Dim body As New StringBuilder()
        Dim i = 0
        While i < glob.Length
            Dim ch = glob(i)
            Select Case ch
                Case "*"c
                    If i + 1 < glob.Length AndAlso glob(i + 1) = "*"c Then
                        ' "**/" spans any number of folders, including none.
                        If i + 2 < glob.Length AndAlso glob(i + 2) = "/"c Then
                            body.Append("(?:.*/)?")
                            i += 3
                            Continue While
                        End If
                        body.Append(".*")
                        i += 2
                        Continue While
                    End If
                    body.Append("[^/]*")      ' a single * stops at a folder
                Case "?"c
                    body.Append("[^/]")
                Case "/"c
                    body.Append("/"c)
                Case Else
                    body.Append(Regex.Escape(ch.ToString()))
            End Select
            i += 1
        End While

        Dim prefix = If(anchored, "^", "^(?:.*/)?")
        Dim suffix = If(dirOnly, "/.*$", "(?:/.*)?$")
        Return New Regex(prefix & body.ToString() & suffix,
                         RegexOptions.IgnoreCase Or RegexOptions.Compiled)
    End Function

End Class
