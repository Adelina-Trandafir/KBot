Option Strict On
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' One error message sent from the K-BOT message window (slice 0112-04), as it goes on the wire to
''' <c>POST /api/errors/report</c>. Field names exactly as <c>PYTHON/routes/error_report.py</c> reads
''' them. Who sent it (user, unit id, database, PC name, IP) is NOT here: the server takes it from the session.
''' </summary>
Public NotInheritable Class ErrorReportRequest
    ''' <summary>Random GUID made on the client, lower case, 36 characters: the retry guard.</summary>
    Public Property rid As String = String.Empty
    ''' <summary>When the message was shown, UTC, <c>yyyy-MM-ddTHH:mm:ssZ</c>.</summary>
    Public Property shown_utc As String = String.Empty

    ' --- the message ---
    Public Property source As String = String.Empty
    Public Property source_line As Integer
    Public Property caption As String = String.Empty
    Public Property header As String = String.Empty
    Public Property text As String = String.Empty
    Public Property buttons As String = String.Empty
    Public Property owner_form As String = String.Empty
    Public Property active_form As String = String.Empty
    Public Property open_forms As String = String.Empty

    ' --- the working context ---
    Public Property unit_name As String = String.Empty
    Public Property cf As String = String.Empty
    Public Property year As Integer
    Public Property ss As String = String.Empty
    Public Property program As String = String.Empty
    Public Property role As String = String.Empty

    ' --- the application and the machine ---
    Public Property app_version As String = String.Empty
    Public Property os As String = String.Empty
    Public Property runtime As String = String.Empty
    Public Property machine As String = String.Empty
    Public Property windows_user As String = String.Empty
    Public Property culture As String = String.Empty
    Public Property screen As String = String.Empty
    Public Property theme As String = String.Empty
    Public Property memory_mb As Integer
    Public Property uptime_min As Integer

    ' --- the tail of the two local logs ---
    Public Property error_log_tail As String = String.Empty
    Public Property message_log_tail As String = String.Empty
End Class

''' <summary>
''' The error report (slice 0112-04): <c>POST /api/errors/report</c>. Kept out of
''' <see cref="IApiClient"/> like <see cref="IHelpFeedbackApi"/>: one call, one caller.
''' </summary>
Public Interface IErrorReportApi

    ''' <summary>
    ''' Sends one report. An idempotent insert by <c>rid</c> on the server, so a retry is harmless.
    ''' Throws <see cref="ApiException"/> on any non-2xx.
    ''' </summary>
    Function SendErrorReportAsync(report As ErrorReportRequest, ct As CancellationToken) As Task

End Interface
