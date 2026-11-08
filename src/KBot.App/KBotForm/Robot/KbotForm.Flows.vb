Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 0086-02 -- the seam between the main window and the flows that left it (<c>KBotForm\Robot\*Flow.vb</c>).
''' The form creates each flow once, in its constructor, and answers the few things a flow still asks of it
''' through <see cref="IShellHost"/>. Every member below only forwards to the method the form already had.
''' </summary>
Partial Public Class KbotForm
    Implements IShellHost

    ''' <summary>The pictures of the FOREXE page (<c>CaptureFlow.vb</c>).</summary>
    Private ReadOnly _captures As CaptureFlow

    ''' <summary>From a downloaded package to the tables, and the partial refreshes (<c>IngestFlow.vb</c>).</summary>
    Private ReadOnly _ingest As IngestFlow

    ''' <summary>
    ''' Shows why the last thing asked of the robot came back empty (the flows that left the form call
    ''' <see cref="ForexeNotices.RobotFailure"/> directly; the many callers still in the form keep this name).
    ''' </summary>
    Private Sub AratEsecul(cePorneam As String)
        ForexeNotices.RobotFailure(Me, _controller, cePorneam)
    End Sub

    Private Function IShellHost_WithReauth(Of T)(k_action As Func(Of Task(Of T))) As Task(Of T) Implements IShellHost.WithReauth
        Return WithReauth(Of T)(k_action)
    End Function

    Private Sub IShellHost_SetBusy(k_running As Boolean) Implements IShellHost.SetBusy
        busyBar.Running = k_running
    End Sub

    Private Function IShellHost_LoadTreeAsync(k_keepSelection As Boolean, k_cod As String) As Task Implements IShellHost.LoadTreeAsync
        Return LoadTreeAsync(k_keepSelection, k_cod)
    End Function

    Private Function IShellHost_ShowModalAsync(k_form As KBotModalForm, k_ref As MachetaRef) As Task(Of Boolean) Implements IShellHost.ShowModalAsync
        Return ShowModalAsync(k_form, k_ref)
    End Function

    Private Function IShellHost_CodRef(k_typeKey As String, k_cod As String) As MachetaRef Implements IShellHost.CodRef
        Return CodRef(k_typeKey, k_cod)
    End Function

    Private Function IShellHost_AngajamentName(k_cod As String) As String Implements IShellHost.AngajamentName
        Return DenumireAngajament(k_cod)
    End Function

    Private Sub IShellHost_MarkDownloadWithoutQuestion(k_cod As String) Implements IShellHost.MarkDownloadWithoutQuestion
        MarcheazaDescarcareaFaraIntrebare(k_cod)
    End Sub

    Private Function IShellHost_AssociationAllowsAsync(k_cod As String, k_operation As String) As Task(Of Boolean) Implements IShellHost.AssociationAllowsAsync
        Return AsocierePermiteAsync(k_cod, k_operation)
    End Function

End Class
