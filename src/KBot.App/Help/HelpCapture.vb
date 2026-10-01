Option Strict On
Imports System.Text.RegularExpressions

''' <summary>
''' One screenshot a help topic asks for (slice 0000-02). Written in the topic as ONE line:
''' <code>&lt;!-- capture: ddf-revizii-1 | caption: Lista reviziilor | goto: view:ddf | prepare: Selectați un angajament cu două revizii. --&gt;</code>
''' The tag IS the picture: the page shows <c>img/&lt;id&gt;.png</c> in its place, or the «Imagine
''' lipsă» box until the picture is taken. <c>caption</c> and <c>prepare</c> are Romanian (the
''' operator reads them); the keys, <c>id</c> and <c>goto</c> are ASCII.
'''
''' <para><b>A picture that a later change made out of date</b> (slice 0000-34, operator request) carries two
''' more keys: <c>redo: yyyy-MM-dd HH:mm</c> (the moment the screen changed) and <c>why:</c> (what changed,
''' Romanian, starts with the slice). While the saved picture is OLDER than that moment the capture
''' list paints the row in red; taking the picture again (or loading one) clears it by itself.
''' A picture that does not exist yet is simply «lipsă» -- there is nothing to redo.</para>
''' </summary>
Public NotInheritable Class HelpCapture

    ''' <summary>Matches one capture tag; group «body» is what sits between «capture:» and «--&gt;».</summary>
    Friend Shared ReadOnly TagPattern As New Regex("<!--\s*capture:(?<body>[^\r\n]*?)-->", RegexOptions.Compiled)

    Private Shared ReadOnly IdPattern As New Regex("^[a-z0-9][a-z0-9\-]*$", RegexOptions.Compiled)

    ''' <summary>File name without extension, ASCII lower case and dashes (<c>ddf-revizii-1</c>).</summary>
    Public Property Id As String = String.Empty

    ''' <summary>Text under the picture and in the capture list.</summary>
    Public Property Caption As String = String.Empty

    ''' <summary>
    ''' Where K-BOT goes before the shot: <c>view:&lt;key&gt;</c>, <c>menu:&lt;key&gt;</c>,
    ''' <c>setari:&lt;page&gt;</c>, <c>help</c>; empty = stay where you are.
    ''' </summary>
    Public Property Target As String = String.Empty

    ''' <summary>What the operator sets up by hand before pressing «Capturează».</summary>
    Public Property Prepare As String = String.Empty

    ''' <summary>
    ''' Slice 0000-34: the screen changed at this moment, so a picture taken BEFORE it is out of date.
    ''' Nothing = the picture is not known to be stale.
    ''' </summary>
    Public Property RedoSince As Date?

    ''' <summary>Slice 0000-34: what changed on the screen (Romanian, starts with the slice); empty when nothing did.</summary>
    Public Property RedoWhy As String = String.Empty

    ''' <summary>
    ''' True when this capture has a saved picture (<paramref name="takenAt"/>) that predates the change
    ''' marked on its tag -- the row to re-shoot.
    ''' </summary>
    Public Function NeedsRedo(takenAt As DateTime?) As Boolean
        Return RedoSince.HasValue AndAlso takenAt.HasValue AndAlso takenAt.Value < RedoSince.Value
    End Function

    Public Property TopicId As String = String.Empty
    Public Property TopicTitle As String = String.Empty
    Public Property Part As HelpPart

    ''' <summary>The picture's path relative to the Help folder.</summary>
    Public ReadOnly Property RelativePath As String
        Get
            Return "img/" & Id & ".png"
        End Get
    End Property

    ''' <summary>
    ''' Reads the text between «capture:» and «--&gt;». Throws <see cref="ArgumentException"/> for a
    ''' bad id, an unknown key or a missing caption.
    ''' </summary>
    Friend Shared Function Parse(body As String) As HelpCapture
        Dim parts As String() = body.Split("|"c)
        Dim c As New HelpCapture With {.Id = parts(0).Trim()}
        If Not IdPattern.IsMatch(c.Id) Then Throw New ArgumentException("capture id '" & c.Id & "' must be lower-case ASCII letters, digits and dashes")
        For i As Integer = 1 To parts.Length - 1
            Dim p As String = parts(i).Trim()
            If p.Length = 0 Then Continue For
            Dim colon As Integer = p.IndexOf(":"c)
            If colon <= 0 Then Throw New ArgumentException("capture '" & c.Id & "': part without 'key:' -> " & p)
            Dim key As String = p.Substring(0, colon).Trim().ToLowerInvariant()
            Dim value As String = p.Substring(colon + 1).Trim()
            Select Case key
                Case "caption" : c.Caption = value
                Case "goto" : c.Target = value
                Case "prepare" : c.Prepare = value
                Case "redo"
                    Dim moment As Date
                    If Not Date.TryParseExact(value, "yyyy-MM-dd HH:mm", Globalization.CultureInfo.InvariantCulture,
                                              Globalization.DateTimeStyles.None, moment) Then
                        Throw New ArgumentException("capture '" & c.Id & "': redo must be 'yyyy-MM-dd HH:mm' -> " & value)
                    End If
                    c.RedoSince = moment
                Case "why" : c.RedoWhy = value
                Case Else : Throw New ArgumentException("capture '" & c.Id & "': unknown key '" & key & "'")
            End Select
        Next
        If c.Caption.Length = 0 Then Throw New ArgumentException("capture '" & c.Id & "' has no caption")
        If c.RedoSince.HasValue AndAlso c.RedoWhy.Length = 0 Then
            Throw New ArgumentException("capture '" & c.Id & "': redo needs a why: that says what changed")
        End If
        Return c
    End Function

End Class
