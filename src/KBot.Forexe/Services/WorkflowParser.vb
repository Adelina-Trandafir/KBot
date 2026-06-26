Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows
Imports WorkflowModels

''' <summary>
''' Parses workflow XML files into executable action lists
''' </summary>
Public Class WorkflowParser

    ''' <summary>
    ''' Logger opțional — se setează din exterior înainte de parsare.
    ''' </summary>
    Public Shared Property Logger As RichTextBoxLogger

    Public Shared Function ParseFromFile(filePath As String, _logger As RichTextBoxLogger) As Workflow
        Logger = _logger
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"Fișierul workflow nu a fost găsit: {filePath}")
        End If

        Dim content = File.ReadAllText(filePath)
        Return Parse(content, filePath)
    End Function

    Public Shared Function Parse(xmlContent As String, xmlFilePath As String) As Workflow
        Dim doc = XDocument.Parse(xmlContent)
        Dim root = doc.Root

        If root Is Nothing OrElse root.Name.LocalName <> "Workflow" Then
            Throw New InvalidOperationException("XML invalid")
        End If

        Dim workflow As New Workflow With {
            .TaskId = CInt(GetAttributeValue(root, "taskId", "-1")),
            .Name = GetAttributeValue(root, "name", "Workflow fără nume"),
            .StartUrl = GetAttributeValue(root, "startUrl", ""),
            .Receive = GetBoolAttribute(root, "receive", False),
            .Actions = ParseActions(root.Elements()),
            .ExpectedUrl = GetAttributeValue(root, "expectedUrl", ""),
            .FilePath = xmlFilePath
        }
        Return workflow
    End Function

    Public Shared Function ApplyVariables(xmlContent As String,
                                       vars As Dictionary(Of String, String)) As String
        For Each kvp In vars
            ' !? => suportă atât {{VAR}} cât și {{!VAR}}
            ' \s* => tolerează spații după {{ 
            ' (?:\|[\s\S]*?)? => sare opțiunile/descrierea (non-greedy, opțional)
            Dim pattern As String = "\{\{!?\s*" & Regex.Escape(kvp.Key) & "(?:\|[\s\S]*?)?\}\}"
            xmlContent = Regex.Replace(xmlContent, pattern, kvp.Value, RegexOptions.IgnoreCase)
        Next
        Return xmlContent
    End Function

    Public Shared Function ExtractVariablesDetailed(xmlContent As String) As Dictionary(Of String, WorkflowVariable)
        Dim vars As New Dictionary(Of String, WorkflowVariable)

        ' Regex nou: {{! | Nume (| Descriere)? (| Optiuni)? }}
        ' Grupul 1: ! - variabila obligatorie (dacă există)
        ' Grupul 2: Nume
        ' Grupul 3: Descriere (Opțional)
        ' Grupul 4: Optiuni (Opțional)
        'Dim pattern As String = "\{\{([^|{}]+)(?:\|([^|{}]+))?(?:\|([^}]+))?\}\}"
        Dim pattern As String = "\{\{(!?)([^|{}]+)(?:\|([^|{}]+))?(?:\|([^}]+))?\}\}"

        Dim matches As MatchCollection = Regex.Matches(xmlContent, pattern)

        For Each m As Match In matches
            Dim varName As String = m.Groups(2).Value.Trim()

            ' Dacă variabila există deja, o sărim (sau facem update, depinde de logică)
            If vars.ContainsKey(varName) Then Continue For

            Dim newVar As New WorkflowVariable With {
                .Name = varName,
                .IsRequired = m.Groups(1).Value = "!"
            }

            ' 1. Descrierea
            If m.Groups(3).Success Then
                newVar.Description = m.Groups(3).Value.Trim()
            End If

            ' 2. Opțiunile (Ex: "Type=Numeric;Min=10")
            If m.Groups(4).Success Then
                Dim optionsString As String = m.Groups(4).Value.Trim()
                ParseOptions(newVar, optionsString)
            End If

            vars.Add(varName, newVar)
        Next

        Return vars
    End Function

    Private Shared Sub ParseOptions(var As WorkflowVariable, optionsStr As String)
        ' Spargem după punct și virgulă
        Dim parts As String() = optionsStr.Split(";"c)

        For Each part In parts
            Dim pair As String() = part.Split("="c)
            If pair.Length = 2 Then
                Dim key As String = pair(0).Trim().ToLower()
                Dim val As String = pair(1).Trim()

                Select Case key
                    Case "type"
                        var.VarType = val ' Numeric / Text
                    Case "min"
                        'testeaza daca e numeric înainte de conversie
                        If Double.TryParse(val, Nothing) Then var.Min = CDbl(val)
                    Case "max"
                        If Double.TryParse(val, Nothing) Then var.Max = CDbl(val)
                    Case "mask"
                        var.Mask = val
                    Case "len", "length"
                        If Integer.TryParse(val, Nothing) Then var.Length = CInt(val)
                End Select
            End If
        Next
    End Sub

    Private Shared Function ParseActions(elements As IEnumerable(Of XElement)) As List(Of IWorkflowAction)
        Dim actions As New List(Of IWorkflowAction)
        For Each el In elements
            Dim a = ParseAction(el)
            If a IsNot Nothing Then actions.Add(a)
        Next
        Return actions
    End Function

    Private Shared Sub ApplyLogValue(action As IWorkflowAction, element As XElement)
        Dim lv = GetXAttribute(element, "LogValue")?.Value
        If String.IsNullOrEmpty(lv) Then
            lv = GetXAttribute(element, "description")?.Value
        End If
        If Not String.IsNullOrEmpty(lv) Then
            If lv = "_" Then Return

            action.LogValue = lv
        End If
    End Sub

    Private Shared Sub ApplyTimeoutValue(action As IWorkflowAction, element As XElement)
        Dim toAttr = GetXAttribute(element, "timeout")?.Value
        If Not String.IsNullOrEmpty(toAttr) Then
            Dim val As Integer
            If Integer.TryParse(toAttr, val) Then
                action.Timeout = val
            End If
        End If
    End Sub

    Private Shared Function ParseAction(element As XElement) As IWorkflowAction
        Dim action As IWorkflowAction

        Select Case element.Name.LocalName
            Case "AuthClick" : action = ParseAuthClickAction(element)
            Case "Click" : action = ParseClickAction(element)
            Case "Debug" : action = ParseDebugAction(element)
            Case "Download" : action = ParseDownloadAction(element)
            Case "Exit" : action = New ExitAction With {.Message = GetAttributeValue(element, "message", "")}
            Case "ExtractXmlFromPdf" : action = ParseExtractXmlFromPdfAction(element)
            Case "Fill" : action = ParseFillAction(element)
            Case "FindInTable" : action = ParseFindInTableAction(element)
            Case "ForEachVar" : action = ParseForEachVarAction(element)
            Case "ForEach" : action = ParseForEachAction(element)
            Case "GetAttribute" : action = ParseGetAttributeAction(element)
            Case "GoBack" : action = ParseGoBackAction(element)
            Case "IfExists" : action = ParseIfExistsAction(element)
            Case "IfUnique" : action = ParseIfUniqueAction(element)
            Case "IfVar" : action = ParseIfVarAction(element)
            Case "Log" : action = ParseLogAction(element)
            Case "Minimize" : action = ParseMinimizeAction(element)
            Case "Read" : action = ParseReadAction(element)
            Case "Reload" : action = ParseReloadAction(element)
            Case "Repeat" : action = ParseRepeatAction(element)
            Case "ScrapeTable" : action = ParseScrapeTableAction(element)
            Case "Screenshot" : action = ParseScreenshotAction(element)
            Case "ScrollToView" : action = ParseScrollToViewAction(element)
            Case "Select" : action = ParseSelectAction(element)
            Case "SetInternalVar" : action = ParseSetInternalVarAction(element)
            Case "Stop" : action = New StopAction With {.Message = GetAttributeValue(element, "message", "")}
            Case "SwitchTab" : action = ParseSwitchTabAction(element)
            Case "Upload" : action = ParseUploadAction(element)
            Case "Wait" : action = ParseWaitAction(element)
            Case "WaitFor" : action = ParseWaitForAction(element)
            Case "WaitForJS" : action = ParseWaitForJSAction(element)
            Case "While" : action = ParseWhileAction(element)

            Case Else
                Logger?.LogError($"Acțiune necunoscută: {element.Name.LocalName}")
                Return Nothing
        End Select

        If action IsNot Nothing Then
            ApplyLogValue(action, element)
            ApplyTimeoutValue(action, element)
            action.SetSkipIdleWait(GetBoolAttribute(element, "skipIdleWait", False))
        End If

        Return action
    End Function

    Private Shared Function ParseReloadAction(e As XElement) As ReloadAction
        Try
            Dim a As New ReloadAction With {
                .WaitNavigation = GetBoolAttribute(e, "waitNavigation", True),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Reload>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseScrollToViewAction(e As XElement) As ScrollToViewAction
        Try
            Dim a As New ScrollToViewAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <ScrollToView>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseScrapeTableAction(e As XElement) As ScrapeTableAction
        Try
            Dim a As New ScrapeTableAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .SaveTo = GetRequiredAttribute(e, "saveTo"),
                .SaveToFile = GetAttributeValue(e, "saveToFile", ""),
                .NextPageSelector = GetAttributeValue(e, "nextPageSelector", ""),
                .WaitSelector = GetAttributeValue(e, "waitSelector", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .SkipFirstNColumns = GetIntAttribute(e, "skipFirstNColumns", 0),
                .SkipFirstNRows = GetIntAttribute(e, "skipFirstNRows", 0),
                .SkipLastNColumns = GetIntAttribute(e, "skipLastNColumns", 0),
                .SkipLastNRows = GetIntAttribute(e, "skipLastNRows", 0),
                .PrevPageSelector = GetAttributeValue(e, "prevPageSelector", ""),
                .FirstPageSelector = GetAttributeValue(e, "firstPageSelector", ""),
                .LastPageSelector = GetAttributeValue(e, "lastPageSelector", ""),
                .StartFromLast = GetBoolAttribute(e, "startFromLast", False),
                .ExitIfCellEquals = GetAttributeValue(e, "exitIfCellEquals", ""),
                .ExitIfCellDate = GetAttributeValue(e, "exitIfCellDate", ""),
                .FingerprintSelector = GetAttributeValue(e, "fingerprintSelector", ""),
                .Page = GetAttributeValue(e, "page", ""),
                .Row = GetAttributeValue(e, "row", ""),
                .Strict = GetBoolAttribute(e, "strict", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <ScrapeTable>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseClickAction(e As XElement) As ClickAction
        Try
            Dim a As New ClickAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .WaitNavigation = GetBoolAttribute(e, "waitNavigation", False),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .Force = GetBoolAttribute(e, "force", False),
                .JsClick = GetBoolAttribute(e, "jsClick", False),
                .ExpectNewTab = GetBoolAttribute(e, "expectNewTab", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Click>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseAuthClickAction(e As XElement) As AuthClickAction
        Try
            Dim a As New AuthClickAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .AuthTimeout = GetIntAttribute(e, "authTimeout", 120),
                .ExpectedUrlAfterAuth = GetAttributeValue(e, "ExpectedUrlAfterAuth", ""),
                .WaitNavigation = GetBoolAttribute(e, "waitNavigation", False),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <AuthClick>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseFillAction(e As XElement) As FillAction
        Try
            Dim a As New FillAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .Value = GetRequiredAttribute(e, "value"),
                .Clear = GetBoolAttribute(e, "clear", True),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .PickFromList = GetBoolAttribute(e, "pickFromList", False),
                .Sequential = GetBoolAttribute(e, "sequential", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Fill>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseSelectAction(e As XElement) As SelectAction
        Try
            Dim a As New SelectAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .Value = e.Attribute("value")?.Value,
                .Text = e.Attribute("text")?.Value
            }

            Dim idxStr = e.Attribute("index")?.Value
            Dim idxVal As Integer

            If idxStr IsNot Nothing AndAlso Integer.TryParse(idxStr, idxVal) Then
                a.Index = idxVal
            End If

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Select>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseReadAction(e As XElement) As ReadAction
        Try
            Dim a As New ReadAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .SaveTo = e.Attribute("saveTo")?.Value,
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Read>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseWaitForAction(e As XElement) As WaitForAction
        Try
            Dim a As New WaitForAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .State = GetAttributeValue(e, "state", "visible"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .RefreshOnFail = GetBoolAttribute(e, "refreshOnFail", False),
                .MaxRetries = GetIntAttribute(e, "maxRetries", 3),
                .Strict = GetBoolAttribute(e, "strict", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <WaitFor>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseWaitAction(e As XElement) As WaitAction
        Try
            Dim sec As Double
            Double.TryParse(GetAttributeValue(e, "seconds", "1"), NumberStyles.Any, CultureInfo.InvariantCulture, sec)

            Dim a As New WaitAction With {
                .Seconds = sec,
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Wait>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseUploadAction(e As XElement) As UploadAction
        Try
            Dim a As New UploadAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .Path = GetRequiredAttribute(e, "path"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Upload>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseIfExistsAction(e As XElement) As IfExistsAction
        Try
            Dim a As New IfExistsAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .Strict = GetBoolAttribute(e, "strict", False),
                .JsCondition = GetAttributeValue(e, "jsCondition", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            For Each c In e.Elements()
                If c.Name.LocalName.Equals("Else", StringComparison.OrdinalIgnoreCase) Then
                    For Each ec In c.Elements()
                        a.ElseChildren.Add(ParseAction(ec))
                    Next
                Else
                    a.Children.Add(ParseAction(c))
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <IfExists>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseIfUniqueAction(e As XElement) As IfUniqueAction
        Try
            Dim a As New IfUniqueAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .OnlyIfVisible = GetBoolAttribute(e, "onlyIfVisible", True),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            For Each c In e.Elements()
                If c.Name.LocalName.Equals("Else", StringComparison.OrdinalIgnoreCase) Then
                    For Each ec In c.Elements()
                        a.ElseChildren.Add(ParseAction(ec))
                    Next
                Else
                    a.Children.Add(ParseAction(c))
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <IfUnique>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseLogAction(e As XElement) As LogAction
        Try
            Dim a As New LogAction With {
                .Message = GetRequiredAttribute(e, "message"),
                .Level = GetAttributeValue(e, "level", "info"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Log>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseScreenshotAction(e As XElement) As ScreenshotAction
        Try
            Dim a As New ScreenshotAction With {
                .ScreenshotPath = e.Attribute("path")?.Value,
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .SaveTo = GetAttributeValue(e, "SaveTo", "")
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Screenshot>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseMinimizeAction(e As XElement) As MinimizeAction
        Try
            Dim a As New MinimizeAction With {
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Minimize>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseIfVarAction(e As XElement) As IfVarAction
        Try
            Dim a As New IfVarAction With {
                .Value = GetAttributeValue(e, "value", ""),
                .Compare = GetAttributeValue(e, "compare", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            For Each c In e.Elements()
                If c.Name.LocalName.Equals("Else", StringComparison.OrdinalIgnoreCase) Then
                    ' Parsăm copiii din interiorul tag-ului <Else>
                    For Each ec In c.Elements()
                        a.ElseChildren.Add(ParseAction(ec))
                    Next
                Else
                    ' Parsăm copiii blocului principal (True)
                    a.Children.Add(ParseAction(c))
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <IfVar>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseDownloadAction(e As XElement) As DownloadAction
        Try
            Dim a As New DownloadAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .SaveFolder = GetAttributeValue(e, "saveFolder", ""),
                .FileName = GetAttributeValue(e, "fileName", ""),
                .OpenFile = GetBoolAttribute(e, "openFile", False),
                .ParseExcel = GetBoolAttribute(e, "parseExcel", False),
                .SaveTo = GetAttributeValue(e, "saveTo", ""),
                .HeaderRows = GetIntAttribute(e, "headerRows", 1),
                .SkipFirstNRows = GetIntAttribute(e, "skipFirstNRows", 0),
                .SkipLastNRows = GetIntAttribute(e, "skipLastNRows", 0),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .SkipFirstNColumns = GetIntAttribute(e, "skipFirstNColumns", 0),
                .SkipLastNColumns = GetIntAttribute(e, "skipLastNColumns", 0),
                .FilterColumn = GetAttributeValue(e, "filterColumn", ""),
                .Filter = GetAttributeValue(e, "filter", ""),
                .ComplexFilter = GetAttributeValue(e, "complexFilter", "")
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Download>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseFindInTableAction(e As XElement) As FindInTableAction
        Try
            Dim a As New FindInTableAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .FieldName = GetRequiredAttribute(e, "fieldName"),
                .Value = GetRequiredAttribute(e, "value"),
                .SaveRowTo = GetAttributeValue(e, "saveRowTo", ""),
                .ClickSelector = GetAttributeValue(e, "clickSelector", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .FieldTransform = GetAttributeValue(e, "fieldTransform", "")
            }

            For Each c In e.Elements()
                Dim childAction = ParseAction(c)
                If childAction IsNot Nothing Then
                    a.Children.Add(childAction)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <FindInTable>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseRepeatAction(e As XElement) As RepeatAction
        Try
            Dim a As New RepeatAction With {
                .Iterations = GetIntAttribute(e, "iterations", 1),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .IndexVariable = GetAttributeValue(e, "indexVariable", "")
            }

            ' Parsăm recursiv acțiunile din interiorul tag-ului <Repeat>
            For Each child In e.Elements()
                Dim childAction = ParseAction(child)
                If childAction IsNot Nothing Then
                    a.Children.Add(childAction)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Repeat>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseGoBackAction(e As XElement) As GoBackAction
        Try
            Dim a As New GoBackAction With {
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <GoBack>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseForEachAction(e As XElement) As ForEachAction
        Try
            ' 1. Citim atributele elementului <ForEach>
            Dim a As New ForEachAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .ClickElement = GetRequiredAttribute(e, "clickElement"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False),
                .IndexVariable = GetAttributeValue(e, "indexVariable", "")
            }

            ' 2. Citim recursiv acțiunile copil (ce se află în interiorul tag-ului <ForEach>)
            For Each c In e.Elements()
                Dim childAction = ParseAction(c)
                If childAction IsNot Nothing Then
                    a.Children.Add(childAction)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <ForEach>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseWhileAction(e As XElement) As WhileAction
        Try
            Dim a As New WhileAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .Condition = GetAttributeValue(e, "condition", "Visible"),
                .MaxIterations = GetIntAttribute(e, "maxIterations", 50),
                .RunFirstTime = GetBoolAttribute(e, "runFirstTime", True),
                .IndexVariable = GetAttributeValue(e, "indexVariable", ""),
                .JsCondition = GetAttributeValue(e, "jsCondition", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            ' Parsăm copiii recursiv
            For Each c In e.Elements()
                Dim childAction = ParseAction(c)
                If childAction IsNot Nothing Then
                    a.Children.Add(childAction)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <While>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseDebugAction(e As XElement) As DebugAction
        Try
            Dim a As New DebugAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .HaltWhenDone = GetBoolAttribute(e, "haltWhenDone", False),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", True)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <Debug>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseForEachVarAction(e As XElement) As ForEachVarAction
        Try
            Dim a As New ForEachVarAction With {
                .Source = GetRequiredAttribute(e, "source"),
                .ItemPrefix = GetAttributeValue(e, "itemPrefix", "ITEM"),
                .IndexVariable = GetAttributeValue(e, "indexVariable", ""),
                .CollectKey = GetAttributeValue(e, "collectKey", ""),
                .CollectFields = GetAttributeValue(e, "collectFields", ""),
                .UseMap = GetBoolAttribute(e, "useMap", False),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            For Each c In e.Elements()
                Dim childAction = ParseAction(c)
                If childAction IsNot Nothing Then
                    a.Children.Add(childAction)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <ForEachVar>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseSetInternalVarAction(e As XElement) As SetInternalVarAction
        Try
            Dim a As New SetInternalVarAction With {
                .Name = GetRequiredAttribute(e, "name"),
                .Value = GetRequiredAttribute(e, "value"),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <SetInternalVar>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseGetAttributeAction(e As XElement) As GetAttributeAction
        Try
            Dim a As New GetAttributeAction With {
                .Selector = GetRequiredAttribute(e, "selector"),
                .AttributeName = GetRequiredAttribute(e, "attributeName"),
                .SaveTo = GetAttributeValue(e, "saveTo", ""),
                .ShowErrorMessage = GetBoolAttribute(e, "showErrorMessage", False),
                .ShowNormalMessage = GetBoolAttribute(e, "showNormalMessage", False),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", True)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <GetAttribute>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseWaitForJSAction(e As XElement) As WaitForJSAction
        Try
            Dim a As New WaitForJSAction With {
                .Expression = GetRequiredAttribute(e, "expression"),
                .ExpectedValue = e.Attribute("expectedValue")?.Value,   ' Nothing dacă absent
                .Compare = GetAttributeValue(e, "compare", "eq"),
                .WaitMode = GetAttributeValue(e, "waitMode", "truthy"),
                .PollingMs = GetIntAttribute(e, "pollingMs", 100),
                .SaveTo = GetAttributeValue(e, "saveTo", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <WaitForJS>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseSwitchTabAction(e As XElement) As SwitchTabAction
        Try
            Dim a As New SwitchTabAction With {
                .TabIndex = GetIntAttribute(e, "tabIndex", -1),
                .UrlEquals = GetAttributeValue(e, "urlEquals", ""),
                .UrlContains = GetAttributeValue(e, "urlContains", ""),
                .UrlPattern = GetAttributeValue(e, "urlPattern", ""),
                .Reload = GetBoolAttribute(e, "reload", False),
                .SavePreviousTabTo = GetAttributeValue(e, "savePreviousTabTo", ""),
                .CloseTabWhenDone = GetBoolAttribute(e, "closeTabWhenDone", False),
                .SaveCurrentUrlTo = GetAttributeValue(e, "saveCurrentUrlTo", ""),
                .ExpectedUrl = GetAttributeValue(e, "expectedUrl", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }

            For Each c In e.Elements()
                If c.Name.LocalName.Equals("Else", StringComparison.OrdinalIgnoreCase) Then
                    For Each ec In c.Elements()
                        a.ElseChildren.Add(ParseAction(ec))
                    Next
                Else
                    Dim child = ParseAction(c)
                    If child IsNot Nothing Then a.Children.Add(child)
                End If
            Next

            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <SwitchTab>: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Shared Function ParseExtractXmlFromPdfAction(e As XElement) As ExtractXmlFromPdfAction
        Try
            Dim a As New ExtractXmlFromPdfAction With {
                .Folder = GetRequiredAttribute(e, "folder"),
                .SaveTo = GetRequiredAttribute(e, "saveTo"),
                .DataDeLa = GetAttributeValue(e, "dataDeLa", ""),
                .IsCheckpoint = GetBoolAttribute(e, "isCheckpoint", False)
            }
            Return a
        Catch ex As Exception
            Logger?.LogError($"Eroare parsare <ExtractXmlFromPdf>: {ex.Message}")
            Return Nothing
        End Try
    End Function

#Region "Helpers"

    ''' <summary>
    ''' Caută un atribut ignorând diferențele de litere mari/mici (Case-Insensitive).
    ''' </summary>
    Private Shared Function GetXAttribute(e As XElement, n As String) As XAttribute
        Return e.Attributes().FirstOrDefault(Function(a) a.Name.LocalName.Equals(n, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function GetRequiredAttribute(e As XElement, n As String) As String
        Dim attr = GetXAttribute(e, n)

        If attr Is Nothing OrElse String.IsNullOrEmpty(attr.Value) Then
            Logger?.LogWarning($"Atribut lipsă: {n} (în elementul <{e.Name.LocalName}>)")
            Return Nothing
        End If

        Return attr.Value
    End Function

    Private Shared Function GetAttributeValue(e As XElement, n As String, d As String) As String
        Dim attr = GetXAttribute(e, n)
        Return If(attr IsNot Nothing, attr.Value, d)
    End Function

    Private Shared Function GetBoolAttribute(e As XElement, n As String, d As Boolean) As Boolean
        Dim attr = GetXAttribute(e, n)

        If attr Is Nothing OrElse String.IsNullOrEmpty(attr.Value) Then
            Return d
        End If

        ' Facem comparatia valorii (true/false) tot case-insensitive pentru siguranță
        Return attr.Value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function GetIntAttribute(e As XElement, n As String, d As Integer) As Integer
        Dim attr = GetXAttribute(e, n)

        If attr Is Nothing Then
            Return d
        End If

        Dim r As Integer
        If Integer.TryParse(attr.Value, r) Then
            Return r
        End If

        Return d
    End Function

#End Region
End Class