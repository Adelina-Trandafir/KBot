Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' One question typed in K-BOT's help (slice 0000-21), as it goes on the wire to
''' <c>POST /api/help/feedback</c>. Field names exactly as <c>PYTHON/routes/help_feedback.py</c>
''' reads them. Nothing about who or where: no user, unit, machine or session.
''' </summary>
Public NotInheritable Class HelpFeedbackRow
    ''' <summary>Random GUID made on the client for this question, lower case, 36 characters.</summary>
    Public Property qid As String = String.Empty
    ''' <summary>The question, trimmed, at most 300 characters.</summary>
    Public Property question As String = String.Empty
    ''' <summary>UTC, <c>yyyy-MM-ddTHH:mm:ssZ</c>.</summary>
    Public Property asked_utc As String = String.Empty
    ''' <summary>The help parts the login could read: <c>contabil</c>, <c>contabil+avansat</c>, <c>director</c>.</summary>
    Public Property parts As String = String.Empty
    Public Property app_version As String = String.Empty
    ''' <summary>The date the help content was last brought up to date (<c>yyyy-MM-dd</c>), or empty.</summary>
    Public Property help_version As String = String.Empty
    ''' <summary>The top hits shown, best first, at most 5, as <c>topicId#section</c>.</summary>
    Public Property hits As List(Of String) = New List(Of String)()
    ''' <summary>The hit used (<c>topicId#section</c>), or Nothing.</summary>
    Public Property opened As String
    ''' <summary><c>open</c> / <c>tour</c> (the hit's button), or Nothing.</summary>
    Public Property action As String
    ''' <summary>1..5, or Nothing.</summary>
    Public Property rating As Integer?
    ''' <summary><c>popup</c> or <c>fereastra</c>.</summary>
    Public Property where As String = String.Empty
End Class

''' <summary>What the server did with a batch.</summary>
Public NotInheritable Class HelpFeedbackResult
    Public Property saved As Integer
    Public Property rejected As Integer
End Class

''' <summary>
''' The help's question log (slice 0000-21): <c>POST /api/help/feedback</c>. Kept out of
''' <see cref="IApiClient"/> like <see cref="IMarcajApi"/>: one call, one caller.
''' </summary>
Public Interface IHelpFeedbackApi

    ''' <summary>
    ''' Sends a batch (the server takes at most 50 rows / 64 KB). An idempotent upsert per
    ''' <c>qid</c> on the server, so a retry is harmless. Throws <see cref="ApiException"/> on any
    ''' non-2xx.
    ''' </summary>
    Function SendHelpFeedbackAsync(rows As IReadOnlyList(Of HelpFeedbackRow), ct As CancellationToken) As Task(Of HelpFeedbackResult)

End Interface
