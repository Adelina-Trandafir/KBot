#If DEBUG Then
Option Strict On
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Slice 0098 bench: the K-BOT server with no network and no database. Sits at the END of the
''' bench's own HttpClient pipeline (behind a real <c>ServerGateHandler</c>), so what reaches it
''' is exactly what the gate let through. Answers every request with a small JSON after
''' <see cref="DelayMs"/>, and tells the bench when each request ARRIVED and when it was answered.
''' </summary>
Public NotInheritable Class FakeServerHandler
    Inherits HttpMessageHandler

    Private ReadOnly _log As Action(Of String)
    Private _count As Integer

    ''' <summary>How long the "server" takes to answer.</summary>
    Public Property DelayMs As Integer = 400

    ''' <param name="log">Called from a pool thread; the bench marshals it.</param>
    Public Sub New(log As Action(Of String))
        ArgumentNullException.ThrowIfNull(log)
        _log = log
    End Sub

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage,
                                                 cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Dim n As Integer = Interlocked.Increment(_count)
        Dim what As String = $"#{n} {request.Method} {request.RequestUri?.AbsolutePath}"
        _log($"SERVER ← a primit {what}")
        Await Task.Delay(DelayMs, cancellationToken).ConfigureAwait(False)
        Dim body As String = If(request.Method = HttpMethod.Get,
                                $"{{""ok"":true,""request"":{n},""rows"":[{{""CodAngajament"":""PROBA"",""Stare"":""In derulare""}}]}}",
                                $"{{""ok"":true,""request"":{n},""written"":1}}")
        _log($"SERVER → a răspuns {what}")
        Return New HttpResponseMessage(HttpStatusCode.OK) With {
            .Content = New StringContent(body, Encoding.UTF8, "application/json"),
            .RequestMessage = request
        }
    End Function
End Class
#End If
