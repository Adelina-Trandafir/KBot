Option Strict On
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' Slice 0098: the one place every K-BOT server request passes -- it waits at
''' <see cref="ServerGate"/> while the robot runs, and it carries the request timeout.
''' </summary>
''' <remarks>
''' <para><b>Why the timeout moved here.</b> <c>HttpClient.Timeout</c> counts from the moment the
''' request enters the pipeline, so a request held for a four-minute download would die at 100
''' seconds without ever leaving the PC. The client is built with an infinite timeout and the
''' configured one starts HERE, after the gate. When it fires the error is the same one
''' <c>HttpClient</c> throws (a <c>TaskCanceledException</c> over a <c>TimeoutException</c>), so the
''' retry in <c>ApiClient</c> recognises it as before.</para>
''' <para><b>A write's answer is read whole before it counts as finished</b>: the robot never
''' starts while a write is still half on the wire. Reads are neither counted nor buffered.</para>
''' </remarks>
Public NotInheritable Class ServerGateHandler
    Inherits DelegatingHandler

    Private ReadOnly _gate As ServerGate
    Private ReadOnly _timeout As TimeSpan

    Public Sub New(gate As ServerGate, timeout As TimeSpan, inner As HttpMessageHandler)
        MyBase.New(inner)
        ArgumentNullException.ThrowIfNull(gate)
        _gate = gate
        _timeout = timeout
    End Sub

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage,
                                                 cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Try
            Dim bypass As Boolean = False
            request.Options.TryGetValue(ServerGate.Bypass, bypass)
            ' Reads pass (operator, 30.09.2026: only writes are held), and so do the robot's own
            ' requests and the update channel: no wait, no count, and the body is left to the caller
            ' (the update zip is streamed).
            If ServerGate.IsRead(request.Method) Then bypass = True
            If bypass Then Return Await SendTimedAsync(request, bufferBody:=False, cancellationToken).ConfigureAwait(False)

            Await _gate.EnterAsync(cancellationToken).ConfigureAwait(False)
            Try
                Return Await SendTimedAsync(request, bufferBody:=True, cancellationToken).ConfigureAwait(False)
            Finally
                _gate.Leave()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ServerGateHandler.SendAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function SendTimedAsync(request As HttpRequestMessage, bufferBody As Boolean,
                                          ct As CancellationToken) As Task(Of HttpResponseMessage)
        Using timer As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct)
            timer.CancelAfter(_timeout)
            Dim resp As HttpResponseMessage = Nothing
            Try
                resp = Await MyBase.SendAsync(request, timer.Token).ConfigureAwait(False)
                ' ReadAsByteArrayAsync buffers the content inside HttpContent, so the caller's own
                ' ReadAsStringAsync is served from memory afterwards.
                If bufferBody AndAlso resp.Content IsNot Nothing Then
                    Await resp.Content.ReadAsByteArrayAsync(timer.Token).ConfigureAwait(False)
                End If
                Return resp
            Catch ex As OperationCanceledException When timer.IsCancellationRequested AndAlso Not ct.IsCancellationRequested
                resp?.Dispose()
                Throw New TaskCanceledException(
                    $"The request was canceled due to the configured HttpClient.Timeout of {_timeout.TotalSeconds:0} seconds elapsing.",
                    New TimeoutException(ex.Message, ex))
            Catch
                resp?.Dispose()
                Throw
            End Try
        End Using
    End Function
End Class
