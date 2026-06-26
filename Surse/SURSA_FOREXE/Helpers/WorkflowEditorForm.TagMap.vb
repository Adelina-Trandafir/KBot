Imports System.Drawing
Imports System.Linq
Imports System.Reflection
Imports WorkflowModels

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — TagMap                                            │
' │                                                                         │
' │  Toate structurile construite prin REFLECTION la startup (Shared New).  │
' │                                                                         │
' │  Necesită în WorkflowModels.vb două custom attributes:                  │
' │    <WflRequired>            → atribut obligatoriu simplu                │
' │    <WflRequiredOneOf("g")>  → obligatoriu minim unul din grup "g"       │
' │                                                                         │
' │  Structuri expuse:                                                       │
' │    _tagAttributes          → Dict(tagName, allAttrs())                  │
' │    _tagRequiredAttributes  → Dict(tagName, simpleRequired())            │
' │    _tagRequiredOneOfGroups → Dict(tagName, List(Of String()))           │
' │                               fiecare String() = un grup "one of"       │
' │    _allTags                → tagNames sortate alfabetic                  │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm

#Region "Colors — active (dark/light, se schimbă la runtime)"
    Friend Shared CLR_BACKGROUND As Color = Color.FromArgb(30, 30, 30)
    Friend Shared CLR_DEFAULT As Color = Color.FromArgb(212, 212, 212)
    Friend Shared CLR_TAG_BRACKET As Color = Color.FromArgb(212, 212, 212)
    Friend Shared CLR_TAG_NAME As Color = Color.FromArgb(78, 201, 176)
    Friend Shared CLR_ATTR_NAME As Color = Color.FromArgb(156, 220, 254)
    Friend Shared CLR_ATTR_VALUE As Color = Color.FromArgb(206, 145, 120)
    Friend Shared CLR_VARIABLE As Color = Color.FromArgb(214, 157, 133)
    Friend Shared CLR_COMMENT As Color = Color.FromArgb(106, 153, 85)
    Friend Shared CLR_INVALID_ATTR As Color = Color.FromArgb(252, 57, 57)
#End Region

#Region "Colors — VS CODE DARK (referință)"
    ' Folosite la SetColorScheme(dark:=True)
    Private Shared ReadOnly DARK_BACKGROUND As Color = Color.FromArgb(30, 30, 30)
    Private Shared ReadOnly DARK_DEFAULT As Color = Color.FromArgb(212, 212, 212)
    Private Shared ReadOnly DARK_TAG_BRACKET As Color = Color.FromArgb(212, 212, 212)
    Private Shared ReadOnly DARK_TAG_NAME As Color = Color.FromArgb(78, 201, 176)
    Private Shared ReadOnly DARK_ATTR_NAME As Color = Color.FromArgb(156, 220, 254)
    Private Shared ReadOnly DARK_ATTR_VALUE As Color = Color.FromArgb(206, 145, 120)
    Private Shared ReadOnly DARK_VARIABLE As Color = Color.FromArgb(214, 157, 133)
    Private Shared ReadOnly DARK_COMMENT As Color = Color.FromArgb(106, 153, 85)
    Private Shared ReadOnly DARK_INVALID_ATTR As Color = Color.FromArgb(252, 57, 57)
#End Region

#Region "Colors — VS CODE LIGHT+ (referință)"
    ' Folosite la SetColorScheme(dark:=False)
    Private Shared ReadOnly LIGHT_BACKGROUND As Color = Color.FromArgb(255, 255, 255)
    Private Shared ReadOnly LIGHT_DEFAULT As Color = Color.FromArgb(0, 0, 0)
    Private Shared ReadOnly LIGHT_TAG_BRACKET As Color = Color.FromArgb(128, 128, 128)
    Private Shared ReadOnly LIGHT_TAG_NAME As Color = Color.FromArgb(0, 100, 130)      ' <Tag>
    Private Shared ReadOnly LIGHT_ATTR_NAME As Color = Color.FromArgb(0, 16, 128)      ' attr=
    Private Shared ReadOnly LIGHT_ATTR_VALUE As Color = Color.FromArgb(163, 21, 21)    ' "value"
    Private Shared ReadOnly LIGHT_VARIABLE As Color = Color.FromArgb(0, 112, 193)      ' {{var}}
    Private Shared ReadOnly LIGHT_COMMENT As Color = Color.FromArgb(0, 128, 0)         ' <!--...-->
    Private Shared ReadOnly LIGHT_INVALID_ATTR As Color = Color.FromArgb(205, 49, 49)
#End Region

    ' =========================================================================
    ' SWITCH CULORI TEMĂ
    ' =========================================================================
    Friend Shared Sub SetColorScheme(dark As Boolean)
        If dark Then
            CLR_BACKGROUND = DARK_BACKGROUND
            CLR_DEFAULT = DARK_DEFAULT
            CLR_TAG_BRACKET = DARK_TAG_BRACKET
            CLR_TAG_NAME = DARK_TAG_NAME
            CLR_ATTR_NAME = DARK_ATTR_NAME
            CLR_ATTR_VALUE = DARK_ATTR_VALUE
            CLR_VARIABLE = DARK_VARIABLE
            CLR_COMMENT = DARK_COMMENT
            CLR_INVALID_ATTR = DARK_INVALID_ATTR
        Else
            CLR_BACKGROUND = LIGHT_BACKGROUND
            CLR_DEFAULT = LIGHT_DEFAULT
            CLR_TAG_BRACKET = LIGHT_TAG_BRACKET
            CLR_TAG_NAME = LIGHT_TAG_NAME
            CLR_ATTR_NAME = LIGHT_ATTR_NAME
            CLR_ATTR_VALUE = LIGHT_ATTR_VALUE
            CLR_VARIABLE = LIGHT_VARIABLE
            CLR_COMMENT = LIGHT_COMMENT
            CLR_INVALID_ATTR = LIGHT_INVALID_ATTR
        End If
    End Sub

#Region "Colors — NORD DARK"
    'Friend Shared ReadOnly CLR_BACKGROUND   As Color = Color.FromArgb(46, 52, 64)
    'Friend Shared ReadOnly CLR_DEFAULT      As Color = Color.FromArgb(216, 222, 233)
    'Friend Shared ReadOnly CLR_TAG_BRACKET  As Color = Color.FromArgb(216, 222, 233)
    'Friend Shared ReadOnly CLR_TAG_NAME     As Color = Color.FromArgb(136, 192, 208)
    'Friend Shared ReadOnly CLR_ATTR_NAME    As Color = Color.FromArgb(129, 161, 193)
    'Friend Shared ReadOnly CLR_ATTR_VALUE   As Color = Color.FromArgb(163, 190, 140)
    'Friend Shared ReadOnly CLR_VARIABLE     As Color = Color.FromArgb(208, 135, 112)
    'Friend Shared ReadOnly CLR_COMMENT      As Color = Color.FromArgb(143, 188, 187)
    'Friend Shared ReadOnly CLR_INVALID_ATTR As Color = Color.FromArgb(236, 95, 95)
#End Region

#Region "Colors — DRACULA"
    'Friend Shared ReadOnly CLR_BACKGROUND   As Color = Color.FromArgb(40, 42, 54)
    'Friend Shared ReadOnly CLR_DEFAULT      As Color = Color.FromArgb(248, 248, 242)
    'Friend Shared ReadOnly CLR_TAG_BRACKET  As Color = Color.FromArgb(248, 248, 242)
    'Friend Shared ReadOnly CLR_TAG_NAME     As Color = Color.FromArgb(139, 233, 253)
    'Friend Shared ReadOnly CLR_ATTR_NAME    As Color = Color.FromArgb(255, 184, 108)
    'Friend Shared ReadOnly CLR_ATTR_VALUE   As Color = Color.FromArgb(255, 121, 198)
    'Friend Shared ReadOnly CLR_VARIABLE     As Color = Color.FromArgb(189, 147, 249)
    'Friend Shared ReadOnly CLR_COMMENT      As Color = Color.FromArgb(98, 114, 164)
    'Friend Shared ReadOnly CLR_INVALID_ATTR As Color = Color.FromArgb(255, 85, 85)
#End Region

#Region "Colors — MONOKAI PRO"
    'Friend Shared ReadOnly CLR_BACKGROUND   As Color = Color.FromArgb(39, 40, 34)
    'Friend Shared ReadOnly CLR_DEFAULT      As Color = Color.FromArgb(248, 248, 242)
    'Friend Shared ReadOnly CLR_TAG_BRACKET  As Color = Color.FromArgb(248, 248, 242)
    'Friend Shared ReadOnly CLR_TAG_NAME     As Color = Color.FromArgb(166, 226, 46)
    'Friend Shared ReadOnly CLR_ATTR_NAME    As Color = Color.FromArgb(253, 151, 31)
    'Friend Shared ReadOnly CLR_ATTR_VALUE   As Color = Color.FromArgb(230, 219, 116)
    'Friend Shared ReadOnly CLR_VARIABLE     As Color = Color.FromArgb(174, 129, 255)
    'Friend Shared ReadOnly CLR_COMMENT      As Color = Color.FromArgb(117, 113, 94)
    'Friend Shared ReadOnly CLR_INVALID_ATTR As Color = Color.FromArgb(255, 80, 80)
#End Region

#Region "Colors — DARK GRAY CLEAN"
    'Friend Shared ReadOnly CLR_BACKGROUND   As Color = Color.FromArgb(24, 24, 24)
    'Friend Shared ReadOnly CLR_DEFAULT      As Color = Color.FromArgb(220, 220, 220)
    'Friend Shared ReadOnly CLR_TAG_BRACKET  As Color = Color.FromArgb(200, 200, 200)
    'Friend Shared ReadOnly CLR_TAG_NAME     As Color = Color.FromArgb(86, 156, 214)
    'Friend Shared ReadOnly CLR_ATTR_NAME    As Color = Color.FromArgb(181, 206, 168)
    'Friend Shared ReadOnly CLR_ATTR_VALUE   As Color = Color.FromArgb(206, 145, 120)
    'Friend Shared ReadOnly CLR_VARIABLE     As Color = Color.FromArgb(214, 157, 133)
    'Friend Shared ReadOnly CLR_COMMENT      As Color = Color.FromArgb(128, 128, 128)
    'Friend Shared ReadOnly CLR_INVALID_ATTR As Color = Color.FromArgb(252, 57, 57)
#End Region

    ' =========================================================================
    ' PROPRIETĂȚI EXCLUSE DIN REFLECTION
    ' =========================================================================
    Private Shared ReadOnly _excludedProps As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "ActionType",
        "Timeout",
        "IsCheckpoint",
        "LogValue",
        "RuntimeIndex",
        "IndexVariableName",
        "Children",
        "ElseChildren",
        "FilePath"
    }

    ' Atribute comune tuturor action-urilor (din IWorkflowAction) — adăugate explicit
    Private Shared ReadOnly _commonAttrs As String() = {"timeout", "isCheckpoint", "LogValue"}

    ' =========================================================================
    ' STRUCTURI PUBLICE — populate în Shared Sub New()
    ' =========================================================================

    ''' <summary>Tag → toate atributele posibile (pentru highlight + autocomplete).</summary>
    Friend Shared ReadOnly _tagAttributes As Dictionary(Of String, String())

    ''' <summary>Tag → atribute REQUIRED simple (toate trebuie prezente).</summary>
    Friend Shared ReadOnly _tagRequiredAttributes As Dictionary(Of String, String())

    ''' <summary>
    ''' Tag → listă de grupuri "one of".
    ''' Fiecare String() din listă = un grup din care MINIM UN atribut trebuie prezent.
    ''' Ex: SelectAction → {{"value", "text", "index"}}
    ''' </summary>
    Friend Shared ReadOnly _tagRequiredOneOfGroups As Dictionary(Of String, List(Of String()))

    ''' <summary>Lista sortată alfabetic a tuturor tag-urilor — pentru autocomplete.</summary>
    Friend Shared ReadOnly _allTags As String()

    ' =========================================================================
    ' SHARED CONSTRUCTOR — rulează O SINGURĂ DATĂ
    ' =========================================================================
    Shared Sub New()
        Dim actionTypes = GetActionTypes()
        _tagAttributes = BuildAllAttributes(actionTypes)
        _tagRequiredAttributes = BuildRequiredSimple(actionTypes)
        _tagRequiredOneOfGroups = BuildRequiredOneOf(actionTypes)
        _allTags = _tagAttributes.Keys.OrderBy(Function(k) k).ToArray()
    End Sub

    ' =========================================================================
    ' HELPERS REFLECTION — comuni tuturor funcțiilor de build
    ' =========================================================================

    Private Shared Function GetActionTypes() As List(Of Type)
        Dim actionInterface = GetType(IWorkflowAction)
        Return Assembly.GetExecutingAssembly().GetTypes().
            Where(Function(t) Not t.IsInterface AndAlso
                              Not t.IsAbstract AndAlso
                              actionInterface.IsAssignableFrom(t)).
            ToList()
    End Function

    ''' <summary>
    ''' Instanțiază temporar un tip și returnează ActionType (tagname).
    ''' Nothing dacă instanțierea eșuează.
    ''' </summary>
    Private Shared Function GetTagName(t As Type) As String
        Try
            Dim instance = TryCast(Activator.CreateInstance(t), IWorkflowAction)
            Return instance?.ActionType
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"[TagMap] Skip {t.Name}: {ex.Message}")
            Return Nothing
        End Try
    End Function

    ''' <summary>Proprietățile publice de instanță, exclus _excludedProps.</summary>
    Private Shared Function GetOwnProps(t As Type) As IEnumerable(Of PropertyInfo)
        Return t.GetProperties(BindingFlags.Public Or BindingFlags.Instance).
            Where(Function(p) p.CanRead AndAlso
                              p.CanWrite AndAlso
                              Not _excludedProps.Contains(p.Name))
    End Function

    ' =========================================================================
    ' BUILD — toate atributele
    ' =========================================================================
    Private Shared Function BuildAllAttributes(actionTypes As List(Of Type)) As Dictionary(Of String, String())
        Dim result As New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase)

        For Each t In actionTypes
            Dim tagName = GetTagName(t)
            If String.IsNullOrEmpty(tagName) Then Continue For

            Dim attrs = GetOwnProps(t).
                Select(Function(p) p.Name).
                OrderBy(Function(n) n).
                ToList()

            ' Adaugă atributele comune din interfață (excluse din reflection)
            For Each common In _commonAttrs
                If Not attrs.Any(Function(a) String.Equals(a, common, StringComparison.OrdinalIgnoreCase)) Then
                    attrs.Add(common)
                End If
            Next

            result(tagName) = attrs.ToArray()
        Next

        ' Tag-uri structurale fără clasă IWorkflowAction
        result("Else") = Array.Empty(Of String)()
        result("Variable") = New String() {"name", "description", "varType",
                                           "min", "max", "length", "isRequired", "mask"}
        result("Workflow") = New String() {"name", "startUrl", "expectedUrl", "receive"}

        Return result
    End Function

    ' =========================================================================
    ' BUILD — required simplu (proprietăți cu <WflRequired>)
    ' =========================================================================
    Private Shared Function BuildRequiredSimple(actionTypes As List(Of Type)) As Dictionary(Of String, String())
        Dim result As New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase)
        Dim requiredAttrType = GetType(WflRequiredAttribute)

        For Each t In actionTypes
            Dim tagName = GetTagName(t)
            If String.IsNullOrEmpty(tagName) Then Continue For

            Dim requiredProps = GetOwnProps(t).
                Where(Function(p) p.GetCustomAttribute(requiredAttrType) IsNot Nothing).
                Select(Function(p) p.Name).
                OrderBy(Function(n) n).
                ToArray()

            result(tagName) = requiredProps
        Next

        ' Tag-uri structurale
        result("Workflow") = New String() {"name", "startUrl"}
        result("Variable") = New String() {"name"}
        result("Else") = Array.Empty(Of String)()

        Return result
    End Function

    ' =========================================================================
    ' BUILD — required "one of" (proprietăți cu <WflRequiredOneOf("grupNume")>)
    ' =========================================================================
    Private Shared Function BuildRequiredOneOf(actionTypes As List(Of Type)) As Dictionary(Of String, List(Of String()))
        Dim result As New Dictionary(Of String, List(Of String()))(StringComparer.OrdinalIgnoreCase)
        Dim oneOfAttrType = GetType(WflRequiredOneOfAttribute)

        For Each t In actionTypes
            Dim tagName = GetTagName(t)
            If String.IsNullOrEmpty(tagName) Then Continue For

            ' Grupăm proprietățile după GroupName din atribut
            ' Dict: grupNume → lista de propertyName-uri
            Dim groups As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

            For Each prop In GetOwnProps(t)
                Dim oneOfAttr = TryCast(prop.GetCustomAttribute(oneOfAttrType), WflRequiredOneOfAttribute)
                If oneOfAttr Is Nothing Then Continue For

                Dim grpName = oneOfAttr.GroupName

                Dim value As List(Of String) = Nothing

                If Not groups.TryGetValue(grpName, value) Then
                    value = New List(Of String)()
                    groups(grpName) = value
                End If

                value.Add(prop.Name)
            Next

            If groups.Count > 0 Then
                ' Convertim la List(Of String()) — ordinea grupurilor nu contează
                Dim groupList = groups.Values.
                    Select(Function(g) g.OrderBy(Function(n) n).ToArray()).
                    ToList()
                result(tagName) = groupList
            End If
        Next

        Return result
    End Function

End Class