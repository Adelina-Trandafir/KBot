Option Strict On
Imports KBot.Domain

''' <summary>
''' Slice 0086-01 -- the state the pieces of the main shell share: what the tree holds, which
''' node is selected, which view is showing, the year / SS catalogue. It used to be ten loose
''' fields of <see cref="KbotForm"/> read from thirty files; now it is one object, so a piece
''' that leaves the form (a flow class) takes THIS and not the whole form.
''' </summary>
''' <remarks>
''' A plain holder: no logic, no UI. The form creates it once and hands it to whoever needs it.
''' What does NOT live here on purpose: the cancellation source of the synchronisation
''' (<c>KbotForm._cts</c>, born and killed inside one flow), the group in use and the
''' per-flow flags (they belong to the flow that owns them).
''' </remarks>
Friend NotInheritable Class ShellState

    ''' <summary>The year / SS / CodProgram catalogue of the current database (from /api/auth/periods).</summary>
    Friend Property Periods As IReadOnlyList(Of PeriodInfo)

    ''' <summary>Lazily created views (key -> instance); only one is visible.</summary>
    Friend ReadOnly Property Views As New Dictionary(Of String, IAngajamentView)()

    ''' <summary>The view on screen.</summary>
    Friend Property ActiveView As IAngajamentView

    ''' <summary>Context of the current tree selection (Nothing = nothing selected / chapter node).</summary>
    Friend Property CurrentInfo As AngajamentTreeInfo

    ''' <summary>NodeKey -> info, rebuilt on every LoadTree.</summary>
    Friend ReadOnly Property TreeInfos As New Dictionary(Of String, AngajamentTreeInfo)()

    ''' <summary>The codes in the order the tree shows them (the dictionary promises no order).</summary>
    Friend ReadOnly Property TreeOrder As New List(Of String)()

    ''' <summary>The btnOpt option: also show the HIDDEN (ASCUNS) angajamente (off by default).</summary>
    Friend Property IncludeHidden As Boolean

    ''' <summary>
    ''' The last rows GET /api/forexe/tree handed back, in the server's own order. Kept so a change
    ''' of sort re-lays the tree without asking for them again.
    ''' </summary>
    Friend Property TreeRows As IReadOnlyList(Of AngajamentTreeInfo)

    ''' <summary>
    ''' Angajamente whose download / refresh was SAVED during this run of the program: their tree
    ''' row is underlined. Not persisted -- it starts empty on every start.
    ''' </summary>
    Friend ReadOnly Property DescarcateInSesiune As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>
    ''' Angajamente queued while another FOREXE action was already under way: their receptii are
    ''' all downloaded, without the selection window.
    ''' </summary>
    Friend ReadOnly Property DescarcareFaraIntrebare As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

End Class
