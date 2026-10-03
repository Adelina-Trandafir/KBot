Option Strict On
Imports System

''' <summary>
''' Slice 0104 -- what kind of client this installation is, as the SERVER said it for the e-mail the
''' operator entered: with the Access application (<c>access = 1</c>) or without it (<c>access = 0</c>).
''' It decides ONE thing: which update package the client asks for (the one with the Migrator and the
''' Access code, or the one without them). The Access FEATURES inside the application are gated by
''' <see cref="AccessFeature"/> instead, from the connected unit's own <c>Setari</c> row.
'''
''' <para><b>Unknown until the server answers.</b> Nothing is assumed: while <see cref="AccessKnown"/> is
''' False the application does not check for updates at all (it would not know which package to ask
''' for). The answer is asked when the application starts (the e-mail remembered from the last login)
''' and again whenever the operator changes the e-mail on the login window.</para>
''' </summary>
Public NotInheritable Class ClientProfile

    ' One immutable snapshot, replaced whole: a reader never sees an e-mail with another e-mail's answer.
    Private NotInheritable Class Snapshot
        Public ReadOnly Email As String
        Public ReadOnly Access As Boolean
        Public Sub New(email As String, access As Boolean)
            Me.Email = email
            Me.Access = access
        End Sub
    End Class

    Private Shared _current As Snapshot

    ''' <summary>Raised after <see cref="Resolved"/> / <see cref="Forget"/>. May fire on any thread.</summary>
    Public Shared Event Changed As EventHandler

    Private Sub New()
    End Sub

    ''' <summary>True once the server has said, for <see cref="Email"/>, whether the client has Access.</summary>
    Public Shared ReadOnly Property AccessKnown As Boolean
        Get
            Return _current IsNot Nothing
        End Get
    End Property

    ''' <summary>The server's answer. False while it is not known -- read <see cref="AccessKnown"/> first.</summary>
    Public Shared ReadOnly Property IsAccess As Boolean
        Get
            Dim s As Snapshot = _current
            Return s IsNot Nothing AndAlso s.Access
        End Get
    End Property

    ''' <summary>The e-mail the answer was asked for; Nothing while it is not known.</summary>
    Public Shared ReadOnly Property Email As String
        Get
            Dim s As Snapshot = _current
            Return If(s Is Nothing, Nothing, s.Email)
        End Get
    End Property

    ''' <summary>True when the answer in hand is the one for this e-mail (the comparison ignores case, as the server does).</summary>
    Public Shared Function IsResolvedFor(email As String) As Boolean
        Dim s As Snapshot = _current
        Return s IsNot Nothing AndAlso String.Equals(s.Email, Normalize(email), StringComparison.Ordinal)
    End Function

    ''' <summary>Records the server's answer for <paramref name="email"/>.</summary>
    Public Shared Sub Resolved(email As String, access As Boolean)
        Dim key As String = Normalize(email)
        If key.Length = 0 Then Throw New ArgumentException("E-mailul lipsește.", NameOf(email))
        _current = New Snapshot(key, access)
        RaiseEvent Changed(Nothing, EventArgs.Empty)
    End Sub

    ''' <summary>Forgets the answer: the e-mail changed and the new one has not been asked yet.</summary>
    Public Shared Sub Forget()
        If _current Is Nothing Then Return
        _current = Nothing
        RaiseEvent Changed(Nothing, EventArgs.Empty)
    End Sub

    Private Shared Function Normalize(email As String) As String
        Return If(email, String.Empty).Trim().ToLowerInvariant()
    End Function

End Class
