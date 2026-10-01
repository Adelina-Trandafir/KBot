Option Strict On

''' <summary>How much of the help a print or an export takes (slice 0000-32).</summary>
Friend Enum HelpScopeKind
    ''' <summary>Everything.</summary>
    All
    ''' <summary>One topic, without what is under it.</summary>
    Topic
    ''' <summary>The topic on screen with everything under it.</summary>
    Branch
    ''' <summary>The parent of the topic on screen with everything under it.</summary>
    Chapter
End Enum

''' <summary>
''' One row of the help window's print / export menus (slice 0000-32): which topics it
''' takes, how the row reads and how the saved file is named.
''' </summary>
Friend NotInheritable Class HelpScope

    ''' <summary>More topics than this on paper is asked about first.</summary>
    Public Const LargeTopicCount As Integer = 15

    Public ReadOnly Property Kind As HelpScopeKind

    ''' <summary>The topic the scope starts from; Nothing for <see cref="HelpScopeKind.All"/>.</summary>
    Public ReadOnly Property Root As HelpTopic

    ''' <summary>The topics taken, in contents order.</summary>
    Public ReadOnly Property Topics As IReadOnlyList(Of HelpTopic)

    Private Sub New(kind As HelpScopeKind, root As HelpTopic, topics As IReadOnlyList(Of HelpTopic))
        Me.Kind = kind
        Me.Root = root
        Me.Topics = topics
    End Sub

    ''' <summary>Every topic of <paramref name="parts"/>.</summary>
    Public Shared Function All(library As HelpLibrary, parts As IEnumerable(Of HelpPart)) As HelpScope
        Return New HelpScope(HelpScopeKind.All, Nothing, parts.SelectMany(Function(p) library.InReadingOrder(p)).ToList())
    End Function

    Public Shared Function One(topic As HelpTopic) As HelpScope
        Return New HelpScope(HelpScopeKind.Topic, topic, New List(Of HelpTopic) From {topic})
    End Function

    ''' <summary>
    ''' <paramref name="root"/> with everything under it: as <see cref="HelpScopeKind.Chapter"/>
    ''' when it is the parent of the topic on screen, else as <see cref="HelpScopeKind.Branch"/>.
    ''' </summary>
    Public Shared Function Under(library As HelpLibrary, root As HelpTopic, asChapter As Boolean) As HelpScope
        Return New HelpScope(If(asChapter, HelpScopeKind.Chapter, HelpScopeKind.Branch), root, library.Branch(root))
    End Function

    ''' <summary>True when printing it should be asked about first: everything, or a long chapter.</summary>
    Public ReadOnly Property IsLarge As Boolean
        Get
            Return Kind = HelpScopeKind.All OrElse Topics.Count > LargeTopicCount
        End Get
    End Property

    ''' <summary>The menu row; a large scope on paper carries the «(!)» mark.</summary>
    Public Function MenuText(forPrint As Boolean) As String
        Dim text As String
        Select Case Kind
            Case HelpScopeKind.All : text = "Tot ajutorul"
            Case HelpScopeKind.Topic : text = "Subiectul curent: " & Root.Title
            Case HelpScopeKind.Branch : text = "«" & Root.Title & "» cu subiectele de sub el (" & Topics.Count & ")"
            Case HelpScopeKind.Chapter : text = "Tot capitolul «" & Root.Title & "» (" & Topics.Count & " subiecte)"
            Case Else : Throw New ArgumentException("Unknown help scope: " & Kind.ToString(), NameOf(Kind))
        End Select
        Return If(forPrint AndAlso IsLarge, "<b>(!)</b> ", String.Empty) & text
    End Function

    ''' <summary>The scope as it reads inside the confirmation question.</summary>
    Public ReadOnly Property SpokenName As String
        Get
            Return If(Kind = HelpScopeKind.All, "TOT ajutorul", "«" & Root.Title & "» cu tot ce are sub el")
        End Get
    End Property

    ''' <summary>The start of the exported file's name (ASCII: the topic id).</summary>
    Public ReadOnly Property FileStem As String
        Get
            Return If(Kind = HelpScopeKind.All, "Manual_KBOT", "Ajutor_KBOT_" & Root.Id)
        End Get
    End Property

End Class
