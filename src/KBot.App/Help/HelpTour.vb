Option Strict On
Imports System.IO
Imports System.Text

''' <summary>One step of a guided tour: where to go, what to point at, what to say.</summary>
Public NotInheritable Class HelpTourStep

    ''' <summary>Romanian heading of the bubble.</summary>
    Public Property Title As String = String.Empty

    ''' <summary>
    ''' The control pointed at: <c>TypeName</c> (a form or user control) or
    ''' <c>TypeName.controlName</c> (a named control inside it). Empty = no pointer, bubble centred.
    ''' </summary>
    Public Property Target As String = String.Empty

    ''' <summary>Where K-BOT goes first; same values as a capture's <c>goto</c>. Empty = stay.</summary>
    Public Property GoToTarget As String = String.Empty

    ''' <summary>Romanian text of the bubble (plain text; «**» marks are removed).</summary>
    Public Property Text As String = String.Empty

End Class

''' <summary>
''' A guided tour (slice 0000-04): a Markdown file under <c>Help\tours\</c>. Header block like a
''' topic (<c>id</c>, <c>title</c>, <c>part</c>, <c>topic</c> = the help topic that offers it), then
''' one <c>## Step title</c> per step, whose first lines may be <c>target:</c> and <c>goto:</c>.
''' Format and rules: <c>HelpContent\README.md</c>.
''' </summary>
Public NotInheritable Class HelpTour

    Public Property Id As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Part As HelpPart = HelpPart.Contabil

    ''' <summary>The topic that shows the «Tur ghidat» link; empty = only on the start page.</summary>
    Public Property TopicId As String = String.Empty

    Public Property Steps As New List(Of HelpTourStep)()
    Public Property SourcePath As String = String.Empty

    ''' <summary>Reads a tour file. Throws <see cref="ArgumentException"/> on a malformed file.</summary>
    Friend Shared Function ParseFile(file As String) As HelpTour
        Dim tour As HelpTour = Parse(System.IO.File.ReadAllText(file, Encoding.UTF8))
        tour.SourcePath = file
        Return tour
    End Function

    Friend Shared Function Parse(text As String) As HelpTour
        Dim lines As String() = text.Replace(vbCrLf, vbLf).Split(ChrW(10))
        If lines.Length = 0 OrElse lines(0).Trim() <> "---" Then Throw New ArgumentException("the tour does not start with a '---' header block")

        Dim tour As New HelpTour()
        Dim i As Integer = 1
        Dim closed As Boolean = False
        While i < lines.Length
            Dim line As String = lines(i).Trim()
            i += 1
            If line = "---" Then
                closed = True
                Exit While
            End If
            If line.Length = 0 OrElse line.StartsWith("#", StringComparison.Ordinal) Then Continue While
            Dim colon As Integer = line.IndexOf(":"c)
            If colon <= 0 Then Throw New ArgumentException("header line without 'key:' -> " & line)
            Dim key As String = line.Substring(0, colon).Trim().ToLowerInvariant()
            Dim value As String = line.Substring(colon + 1).Trim()
            Select Case key
                Case "id" : tour.Id = value
                Case "title" : tour.Title = value
                Case "part" : tour.Part = HelpLibrary.ParsePart(value)
                Case "topic" : tour.TopicId = value
                Case Else : Throw New ArgumentException("unknown tour header key '" & key & "'")
            End Select
        End While
        If Not closed Then Throw New ArgumentException("the tour header block is not closed with '---'")
        If tour.Id.Length = 0 OrElse tour.Title.Length = 0 Then Throw New ArgumentException("tour header needs 'id' and 'title'")

        Dim current As HelpTourStep = Nothing
        Dim body As New StringBuilder()
        Dim inPreamble As Boolean = False
        While i < lines.Length
            Dim raw As String = lines(i)
            i += 1
            If raw.StartsWith("## ", StringComparison.Ordinal) Then
                If current IsNot Nothing Then current.Text = CleanText(body.ToString())
                current = New HelpTourStep With {.Title = raw.Substring(3).Trim()}
                tour.Steps.Add(current)
                body.Clear()
                inPreamble = True
                Continue While
            End If
            If current Is Nothing Then
                If raw.Trim().Length > 0 Then Throw New ArgumentException("text before the first '## ' step")
                Continue While
            End If
            Dim t As String = raw.Trim()
            If inPreamble Then
                If t.StartsWith("target:", StringComparison.OrdinalIgnoreCase) Then
                    current.Target = t.Substring(7).Trim()
                    Continue While
                End If
                If t.StartsWith("goto:", StringComparison.OrdinalIgnoreCase) Then
                    current.GoToTarget = t.Substring(5).Trim()
                    Continue While
                End If
                If t.Length = 0 Then Continue While
                inPreamble = False
            End If
            body.AppendLine(raw)
        End While
        If current IsNot Nothing Then current.Text = CleanText(body.ToString())
        If tour.Steps.Count = 0 Then Throw New ArgumentException("the tour has no '## ' steps")
        Return tour
    End Function

    ' The bubble is a label, not a browser: bold marks go, paragraphs stay, wrapped lines join.
    Private Shared Function CleanText(s As String) As String
        Dim paragraphs As String() = s.Replace("**", String.Empty).Trim().Split({vbCrLf & vbCrLf, vbLf & vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim out As New List(Of String)()
        For Each p As String In paragraphs
            Dim lines As String() = p.Replace(vbCrLf, vbLf).Split(ChrW(10))
            Dim sb As New StringBuilder()
            For Each l As String In lines
                Dim x As String = l.Trim()
                If x.Length = 0 Then Continue For
                If x.StartsWith("- ", StringComparison.Ordinal) Then
                    If sb.Length > 0 Then sb.Append(vbLf)
                    sb.Append("• ").Append(x.Substring(2))
                Else
                    If sb.Length > 0 Then sb.Append(" "c)
                    sb.Append(x)
                End If
            Next
            out.Add(sb.ToString())
        Next
        Return String.Join(vbLf & vbLf, out)
    End Function

End Class
