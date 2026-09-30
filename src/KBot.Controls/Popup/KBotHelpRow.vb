Option Strict On
Imports System.Collections.Generic

''' <summary>What a row of <see cref="KBotHelpList"/> is (slice 0000-20).</summary>
Public Enum KBotHelpRowKind
    ''' <summary>A dim caption over a group of rows; not selectable.</summary>
    Header = 0
    ''' <summary>A search result: title, section, snippet and up to two buttons.</summary>
    Hit = 1
    ''' <summary>A help page (the topic of the current screen): one line.</summary>
    Topic = 2
    ''' <summary>A window's tours, folded; a click opens or closes it.</summary>
    Folder = 3
    ''' <summary>A guided tour: one line with a play mark.</summary>
    Tour = 4
    ''' <summary>A plain message line (nothing found); not selectable.</summary>
    Note = 5
End Enum

''' <summary>What the operator did with a row.</summary>
Public Enum KBotHelpRowAction
    ''' <summary>Click on the row, or Enter while it is selected.</summary>
    Activate = 0
    ''' <summary>The row's «Deschide ...» button.</summary>
    Open = 1
    ''' <summary>The row's «Tur ghidat» button.</summary>
    Tour = 2
End Enum

''' <summary>
''' One row of the help popup / the help window's search list (slice 0000-20). Built by the
''' application (<see cref="IKBotHelpSearchSource"/>); the list only draws it and reports clicks.
''' Every text is what the operator reads (Romanian).
''' </summary>
Public NotInheritable Class KBotHelpRow

    Public Sub New(kind As KBotHelpRowKind, title As String)
        Me.Kind = kind
        Me.Title = If(title, String.Empty)
    End Sub

    Public ReadOnly Property Kind As KBotHelpRowKind

    ''' <summary>The first line (bold for a hit).</summary>
    Public ReadOnly Property Title As String

    ''' <summary>Hit: the section heading, drawn after the title. Empty = none.</summary>
    Public Property Subtitle As String = String.Empty

    ''' <summary>Hit: a few words of the section, up to two lines.</summary>
    Public Property Snippet As String = String.Empty

    ''' <summary>Hit: the caption of the «Deschide ...» button; empty = no button.</summary>
    Public Property OpenText As String = String.Empty

    ''' <summary>Hit: the caption of the tour button; empty = no button.</summary>
    Public Property TourText As String = String.Empty

    ''' <summary>Hover text of the row (and of its buttons); empty = none.</summary>
    Public Property ToolTipText As String = String.Empty

    ''' <summary>Folder: its tours.</summary>
    Public ReadOnly Property Children As New List(Of KBotHelpRow)()

    ''' <summary>Folder: open (its children shown under it).</summary>
    Public Property Expanded As Boolean

    ''' <summary>The application's own object behind the row (never read by the controls).</summary>
    Public Property Tag As Object

    ''' <summary>True for the rows the keyboard and the mouse can pick.</summary>
    Public ReadOnly Property IsSelectable As Boolean
        Get
            Return Kind <> KBotHelpRowKind.Header AndAlso Kind <> KBotHelpRowKind.Note
        End Get
    End Property

End Class

''' <summary>A row and what was done with it.</summary>
Public NotInheritable Class KBotHelpRowEventArgs
    Inherits EventArgs

    Public Sub New(row As KBotHelpRow, action As KBotHelpRowAction)
        If row Is Nothing Then Throw New ArgumentNullException(NameOf(row))
        Me.Row = row
        Me.Action = action
    End Sub

    Public ReadOnly Property Row As KBotHelpRow
    Public ReadOnly Property Action As KBotHelpRowAction
End Class

''' <summary>
''' What feeds a <see cref="KBotHelpSearchPanel"/> (slice 0000-20). Implemented in KBot.App,
''' which knows the topics, the login and the windows; the controls cannot reference it.
''' One source per panel: it also keeps the question being asked there (slice 0000-21).
''' </summary>
Public Interface IKBotHelpSearchSource

    ''' <summary>The rows shown while the search box is empty (may be empty).</summary>
    Function HomeRows() As IList(Of KBotHelpRow)

    ''' <summary>The rows for <paramref name="query"/>. Local and quick: called while the operator types.</summary>
    Function Search(query As String) As IList(Of KBotHelpRow)

    ''' <summary>
    ''' The operator used a row. Returns True when the popup should close (the help window, a
    ''' screen or a tour took over).
    ''' </summary>
    Function Invoke(row As KBotHelpRow, action As KBotHelpRowAction) As Boolean

    ''' <summary>Stars given to the current answer, 1..5 (0 = taken back).</summary>
    Sub Rate(stars As Integer)

    ''' <summary>The stars already given to the question now in the box (0 = none), shown again after a search.</summary>
    Function CurrentRating() As Integer

    ''' <summary>The panel is closing or its box was emptied: the current question is over.</summary>
    Sub EndQuestion()

End Interface
