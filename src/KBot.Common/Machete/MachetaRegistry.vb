Option Strict On
Imports System.Collections.Generic
Imports System.Threading.Tasks

''' <summary>
''' The one place that knows every kind of macheta (form) of the application and how to open it
''' again from a <see cref="MachetaRef"/>. A module registers its kinds once; whoever opens a macheta
''' either goes through <see cref="OpenAsync"/> (which records the opening) or, when the macheta was
''' already shown by its own code, tells the registry with <see cref="Note"/>. The history of the
''' last openings lives in <see cref="History"/>.
''' </summary>
Public NotInheritable Class MachetaRegistry

    Private ReadOnly _types As New Dictionary(Of String, MachetaType)(StringComparer.Ordinal)
    Private _noteCount As Integer

    Public Sub New(k_history As MachetaHistory)
        ArgumentNullException.ThrowIfNull(k_history)
        History = k_history
    End Sub

    Public ReadOnly Property History As MachetaHistory

    ''' <summary>Registers a kind. A key registered twice is an error: two modules would be fighting over one name.</summary>
    Public Sub Register(k_type As MachetaType)
        ArgumentNullException.ThrowIfNull(k_type)
        If _types.ContainsKey(k_type.Key) Then Throw New ArgumentException("Macheta type '" & k_type.Key & "' is already registered.", NameOf(k_type))
        _types.Add(k_type.Key, k_type)
    End Sub

    Public Function IsRegistered(k_key As String) As Boolean
        Return _types.ContainsKey(k_key)
    End Function

    ''' <summary>The registered kind; an unknown key throws (a typo must be seen, not turned into nothing).</summary>
    Public Function Find(k_key As String) As MachetaType
        Dim k_type As MachetaType = Nothing
        If Not _types.TryGetValue(If(k_key, String.Empty), k_type) Then
            Throw New ArgumentException("Unknown macheta type '" & k_key & "'.", NameOf(k_key))
        End If
        Return k_type
    End Function

    ''' <summary>How many times <see cref="Note"/> has been called: lets a caller tell whether a macheta reported itself.</summary>
    Public ReadOnly Property NoteCount As Integer
        Get
            Return _noteCount
        End Get
    End Property

    ''' <summary>
    ''' Opens the macheta and, if it was shown, records it -- unless the macheta reported itself with <see cref="Note"/>
    ''' while opening (its own reference, with the arguments only it knows), so one opening is one entry.
    ''' False = the opener did not show it (it told the operator why).
    ''' </summary>
    Public Async Function OpenAsync(k_ref As MachetaRef) As Task(Of Boolean)
        ArgumentNullException.ThrowIfNull(k_ref)
        Dim k_type As MachetaType = Find(k_ref.TypeKey)
        Dim k_notesBefore As Integer = _noteCount
        Dim k_shown As Boolean = Await k_type.Opener(k_ref).ConfigureAwait(True)
        If k_shown AndAlso _noteCount = k_notesBefore Then History.Record(k_ref, k_type.TitleFor(k_ref))
        Return k_shown
    End Function

    ''' <summary>A macheta the caller's own code has just shown: only the history is told.</summary>
    Public Sub Note(k_ref As MachetaRef)
        ArgumentNullException.ThrowIfNull(k_ref)
        _noteCount += 1
        History.Record(k_ref, Find(k_ref.TypeKey).TitleFor(k_ref))
    End Sub
End Class
