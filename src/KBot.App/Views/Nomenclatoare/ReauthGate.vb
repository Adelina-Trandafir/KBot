Option Strict On
Imports System.Threading.Tasks

''' <summary>
''' The shell's 401 net for a window that makes calls of many different response types (slice 0087).
'''
''' <para><c>KbotForm.WithReauth</c> is private and generic, so until now a window was handed one
''' closure per response type (<c>DdfEditReauth</c> carries eight). The «Clasificatii» and
''' «Parteneri» windows make seven different calls between them; instead of seven closures they get
''' this one gate, built by the shell around <c>WithReauth(Of Object)</c>. The re-login policy still
''' lives only in the shell; the gate only boxes the result on the way in and unboxes it on the way
''' out.</para>
''' </summary>
Public NotInheritable Class ReauthGate

    Private ReadOnly _run As Func(Of Func(Of Task(Of Object)), Task(Of Object))

    Public Sub New(run As Func(Of Func(Of Task(Of Object)), Task(Of Object)))
        ArgumentNullException.ThrowIfNull(run)
        _run = run
    End Sub

    ''' <summary>A gate with no re-login net: the call runs as is (for hosts without a shell).</summary>
    Public Shared ReadOnly Property Direct As New ReauthGate(Function(action) action())

    ''' <summary>Runs <paramref name="action"/> under the re-login net and returns its result.</summary>
    Public Async Function RunAsync(Of T)(action As Func(Of Task(Of T))) As Task(Of T)
        ArgumentNullException.ThrowIfNull(action)
        Dim result As Object = Await _run(
            Async Function() As Task(Of Object)
                Return Await action().ConfigureAwait(True)
            End Function).ConfigureAwait(True)
        Return CType(result, T)
    End Function

    ''' <summary>The same, for a call that returns nothing.</summary>
    Public Async Function RunAsync(action As Func(Of Task)) As Task
        ArgumentNullException.ThrowIfNull(action)
        Await _run(
            Async Function() As Task(Of Object)
                Await action().ConfigureAwait(True)
                Return Nothing
            End Function).ConfigureAwait(True)
    End Function

End Class
