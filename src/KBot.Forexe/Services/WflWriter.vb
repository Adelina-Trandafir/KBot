Imports System.Text
Imports System.Xml
Imports WorkflowModels

''' <summary>
''' Serialises a compacted action list into .wfl XML.
''' </summary>
''' <remarks>
''' Attributes are written only when they differ from what WorkflowParser would
''' restore. That is not always the model default: ClickAction.WaitNavigation is
''' True in the model but the parser reads it with a False default, so a Click that
''' must wait for navigation has to say so explicitly. The parser wins here, because
''' the parser is what reads the file back.
''' </remarks>
Public NotInheritable Class WflWriter

    Private Const HasTextOpen As String = ":has-text('"
    Private Const HasTextClose As String = "')"
    Private Const MinHasTextLength As Integer = 6

    Private Sub New()
        ' Static only.
    End Sub

    ' =========================================================================
    '  Write
    ' =========================================================================
    Public Shared Function Write(actions As List(Of IWorkflowAction),
                                 workflowName As String,
                                 startUrl As String,
                                 expectedUrl As String) As String
        If actions Is Nothing Then Throw New ArgumentNullException(NameOf(actions))

        Dim root As New XElement("Workflow",
            New XAttribute("name", If(workflowName, "Workflow înregistrat")),
            New XAttribute("startUrl", If(String.IsNullOrEmpty(startUrl), "current", startUrl)),
            New XAttribute("expectedUrl", If(expectedUrl, String.Empty)))

        root.Add(BuildHeaderComment())

        For Each action As IWorkflowAction In actions
            root.Add(Render(action))
        Next

        Dim doc As New XDocument(New XDeclaration("1.0", "utf-8", Nothing), root)
        Return Serialize(doc)
    End Function

    Private Shared Function BuildHeaderComment() As XComment
        Dim text As New StringBuilder()
        text.AppendLine()
        text.AppendLine($"    Generat automat de K-BOT Recorder — {DateTime.Now:dd.MM.yyyy HH:mm}")
        text.AppendLine("    ATENȚIE: acesta este un SCHELET. Buclele (ForEach / While / ForEachVar),")
        text.AppendLine("    variabilele, ScrapeTable, Download, Read și checkpoint-urile NU se pot")
        text.AppendLine("    deduce dintr-o înregistrare liniară și trebuie adăugate manual.")
        text.Append("  ")
        Return New XComment(SafeComment(text.ToString()))
    End Function

    ' =========================================================================
    '  Render - one action to one XML node
    ' =========================================================================
    Private Shared Function Render(action As IWorkflowAction) As XNode
        If action Is Nothing Then Throw New ArgumentNullException(NameOf(action))

        Dim comment = TryCast(action, RecorderComment)
        If comment IsNot Nothing Then Return New XComment(SafeComment(comment.Text))

        Dim clickAction = TryCast(action, ClickAction)
        If clickAction IsNot Nothing Then Return RenderClick(clickAction)

        Dim fillAction = TryCast(action, FillAction)
        If fillAction IsNot Nothing Then Return RenderFill(fillAction)

        Dim selectAction = TryCast(action, SelectAction)
        If selectAction IsNot Nothing Then Return RenderSelect(selectAction)

        Dim waitForAction = TryCast(action, WaitForAction)
        If waitForAction IsNot Nothing Then Return RenderWaitFor(waitForAction)

        Dim waitAction = TryCast(action, WaitAction)
        If waitAction IsNot Nothing Then Return RenderWait(waitAction)

        Dim ifExistsAction = TryCast(action, IfExistsAction)
        If ifExistsAction IsNot Nothing Then Return RenderIfExists(ifExistsAction)

        Throw New InvalidOperationException(
            $"Recorderul nu știe să scrie acțiunea «{action.ActionType}» în fișierul .wfl.")
    End Function

    Private Shared Function RenderClick(a As ClickAction) As XElement
        Dim el As New XElement("Click", New XAttribute("selector", SafeSelector(a.Selector, "Click")))
        ' The parser reads waitNavigation with a False default, so True must be written.
        If a.WaitNavigation Then el.Add(New XAttribute("waitNavigation", "true"))
        If a.Force Then el.Add(New XAttribute("force", "true"))
        If a.JsClick Then el.Add(New XAttribute("jsClick", "true"))
        If a.ExpectNewTab Then el.Add(New XAttribute("expectNewTab", "true"))
        AddCommon(el, a, 30)
        Return el
    End Function

    Private Shared Function RenderFill(a As FillAction) As XElement
        Dim el As New XElement("Fill",
            New XAttribute("selector", SafeSelector(a.Selector, "Fill")),
            New XAttribute("value", If(a.Value, String.Empty)))
        If Not a.Clear Then el.Add(New XAttribute("clear", "false"))
        If a.Sequential Then el.Add(New XAttribute("sequential", "true"))
        If a.PickFromList Then el.Add(New XAttribute("pickFromList", "true"))
        AddCommon(el, a, 30)
        Return el
    End Function

    Private Shared Function RenderSelect(a As SelectAction) As XElement
        Dim el As New XElement("Select", New XAttribute("selector", SafeSelector(a.Selector, "Select")))
        If a.Value IsNot Nothing Then el.Add(New XAttribute("value", a.Value))
        If a.Text IsNot Nothing Then el.Add(New XAttribute("text", a.Text))
        If a.Index.HasValue Then el.Add(New XAttribute("index", a.Index.Value))
        AddCommon(el, a, 30)
        Return el
    End Function

    Private Shared Function RenderWaitFor(a As WaitForAction) As XElement
        Dim el As New XElement("WaitFor", New XAttribute("selector", SafeSelector(a.Selector, "WaitFor")))
        If Not String.Equals(a.State, "visible", StringComparison.Ordinal) Then
            el.Add(New XAttribute("state", a.State))
        End If
        If a.RefreshOnFail Then el.Add(New XAttribute("refreshOnFail", "true"))
        If a.Strict Then el.Add(New XAttribute("strict", "true"))
        AddCommon(el, a, 30)
        Return el
    End Function

    Private Shared Function RenderWait(a As WaitAction) As XElement
        Dim el As New XElement("Wait",
            New XAttribute("seconds", a.Seconds.ToString(Globalization.CultureInfo.InvariantCulture)))
        AddCommon(el, a, 30)
        Return el
    End Function

    Private Shared Function RenderIfExists(a As IfExistsAction) As XElement
        Dim el As New XElement("IfExists", New XAttribute("selector", SafeSelector(a.Selector, "IfExists")))
        If a.Strict Then el.Add(New XAttribute("strict", "true"))
        AddCommon(el, a, 5)

        For Each child As IWorkflowAction In a.Children
            el.Add(Render(child))
        Next

        If a.ElseChildren.Count > 0 Then
            Dim elseEl As New XElement("Else")
            For Each child As IWorkflowAction In a.ElseChildren
                elseEl.Add(Render(child))
            Next
            el.Add(elseEl)
        End If

        Return el
    End Function

    ''' <summary>timeout, isCheckpoint and LogValue - the three every action shares.</summary>
    Private Shared Sub AddCommon(el As XElement, a As IWorkflowAction, parserDefaultTimeout As Integer)
        If a.Timeout <> parserDefaultTimeout Then el.Add(New XAttribute("timeout", a.Timeout))
        If a.IsCheckpoint Then el.Add(New XAttribute("isCheckpoint", "true"))
        If Not String.IsNullOrEmpty(a.LogValue) Then el.Add(New XAttribute("LogValue", a.LogValue))
    End Sub

    ' =========================================================================
    '  Selector sanitising - :has-text('...') cannot carry an apostrophe
    ' =========================================================================
    Friend Shared Function SafeSelector(selector As String, actionName As String) As String
        If String.IsNullOrEmpty(selector) Then
            Throw New InvalidOperationException(
                $"Acțiunea «{actionName}» nu are selector. Alege un candidat înainte de generare.")
        End If

        Dim open As Integer = selector.IndexOf(HasTextOpen, StringComparison.Ordinal)
        If open < 0 Then Return selector

        If selector.IndexOf(HasTextOpen, open + 1, StringComparison.Ordinal) >= 0 Then
            Throw New InvalidOperationException(
                $"Selectorul acțiunii «{actionName}» conține două fragmente :has-text() " &
                $"și nu poate fi scris în siguranță: {selector}")
        End If

        Dim innerStart As Integer = open + HasTextOpen.Length
        Dim close As Integer = selector.LastIndexOf(HasTextClose, StringComparison.Ordinal)
        If close <= innerStart Then
            Throw New InvalidOperationException(
                $"Selectorul acțiunii «{actionName}» are un :has-text() neînchis: {selector}")
        End If

        Dim inner As String = selector.Substring(innerStart, close - innerStart)
        If inner.IndexOf("'"c) < 0 Then Return selector

        Dim trimmed As String = LongestApostropheFreeFragment(inner)
        If trimmed Is Nothing Then
            Throw New InvalidOperationException(
                $"Textul din selectorul acțiunii «{actionName}» conține apostrof și nu are " &
                $"niciun fragment de cel puțin {MinHasTextLength} caractere fără apostrof: {inner}")
        End If

        Return selector.Substring(0, innerStart) & trimmed & selector.Substring(close)
    End Function

    Private Shared Function LongestApostropheFreeFragment(text As String) As String
        Dim best As String = String.Empty
        For Each part As String In text.Split("'"c)
            Dim candidate As String = part.Trim()
            If candidate.Length > best.Length Then best = candidate
        Next
        If best.Length < MinHasTextLength Then Return Nothing
        Return best
    End Function

    ' =========================================================================
    '  Comment and serialisation helpers
    ' =========================================================================
    ''' <summary>XML comments cannot contain "--" nor end with "-".</summary>
    Private Shared Function SafeComment(text As String) As String
        Dim clean As String = If(text, String.Empty).Replace("--", "—")
        If clean.EndsWith("-", StringComparison.Ordinal) Then clean &= " "
        Return clean
    End Function

    Private Shared Function Serialize(doc As XDocument) As String
        Dim settings As New XmlWriterSettings With {
            .Indent = True,
            .IndentChars = "  ",
            .Encoding = New UTF8Encoding(False),
            .OmitXmlDeclaration = False
        }

        Using writer As New Utf8StringWriter()
            Using xml As XmlWriter = XmlWriter.Create(writer, settings)
                doc.Save(xml)
            End Using
            Return writer.ToString()
        End Using
    End Function

    ''' <summary>StringWriter declares UTF-16 by default, which would land in the XML header.</summary>
    Private NotInheritable Class Utf8StringWriter
        Inherits IO.StringWriter

        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return New UTF8Encoding(False)
            End Get
        End Property
    End Class

End Class
