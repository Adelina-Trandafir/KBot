Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
Imports Xunit

' Slice 0057. Why an empty download came back is a DIFFERENT question from whether it came
' back empty, and the shell needs both: it shows a message box for a failure and stays
' silent when the operator cancelled. LastFailure is that second answer.
'
' The case behind these tests: a «Prelucrare Completa» whose workflow hits
'   <Log message="EROARE: Angajamentul ... " level="error"/>
'   <Exit message="Angajamentul ... nu a fost gasit. Opresc executia."/>
' used to come back Success=True with zero tables, and the shell carried the empty package
' on to the server, which then worked on nothing. The runner now reports that run as
' FAILED; here we pin what the coordinator does with a failed run.
Public Class ForexeControllerFailureTests

    ' A runner with a live session that answers RunJobAsync however the test says.
    ' Nothing here launches a browser: IForexeRunner is exactly the seam that makes the
    ' coordinator testable without Playwright.
    Private NotInheritable Class FakeRunner
        Implements IForexeRunner

        Public Property Rezultat As JobResult
        Public Property AreSesiune As Boolean = True

        Public Event StatusUpdated As EventHandler(Of String) Implements IForexeRunner.StatusUpdated

        Public ReadOnly Property HasLiveSession As Boolean Implements IForexeRunner.HasLiveSession
            Get
                Return AreSesiune
            End Get
        End Property

        Public ReadOnly Property IsBrowserVisible As Boolean Implements IForexeRunner.IsBrowserVisible
            Get
                Return False
            End Get
        End Property

        Public Function RunAsync(job As JobRequest, certificate As X509Certificate2,
                                 progress As IProgress(Of Integer), ct As CancellationToken) _
            As Task(Of JobResult) Implements IForexeRunner.RunAsync
            Return Task.FromResult(New JobResult With {.Success = True, .Message = "conectat"})
        End Function

        Public Function RunJobAsync(job As JobRequest, progress As IProgress(Of Integer),
                                    ct As CancellationToken) As Task(Of JobResult) _
            Implements IForexeRunner.RunJobAsync
            Return Task.FromResult(Rezultat)
        End Function

        Public Function DescarcaExtraseAsync(folderDescarcare As String, dataDeLa As Date?,
                                             progres As Action(Of Integer, Integer, String),
                                             ct As CancellationToken) _
            As Task(Of List(Of ExtrasDescarcat)) Implements IForexeRunner.DescarcaExtraseAsync
            Throw New NotSupportedException()
        End Function

        Public Function ShowBrowserAsync() As Task Implements IForexeRunner.ShowBrowserAsync
            Return Task.CompletedTask
        End Function

        Public Function HideBrowserAsync() As Task Implements IForexeRunner.HideBrowserAsync
            Return Task.CompletedTask
        End Function

        Public Sub ShowRecorder(owner As IWin32Window) Implements IForexeRunner.ShowRecorder
            Throw New NotSupportedException()
        End Sub

        ' Silences the "event never raised" warning without changing behaviour.
        Private Sub Nefolosit()
            RaiseEvent StatusUpdated(Me, String.Empty)
        End Sub
    End Class

    Private Shared Function NewController(rezultat As JobResult) As ForexeController
        Dim runner As New FakeRunner() With {.Rezultat = rezultat}
        Return New ForexeController(runner, New SessionContext() With {.DbName = "000_DEMO"})
    End Function

    ' The <Exit> case, end to end from the coordinator's side: nothing comes back, and the
    ' reason is the workflow's own message -- ready to be put in front of the operator.
    <Fact>
    Public Async Function FluxOpritDeExit_NuIntoarceNimicSiSpuneDeCe() As Task
        Const motiv As String = "Angajamentul AAB4FBAT96M nu a fost găsit. Opresc execuția."
        Dim ctrl As ForexeController = NewController(
            New JobResult With {.Success = False, .Message = motiv})

        Dim pachet As PrelucrareRezultat = Await ctrl.DownloadNodeAsync("AAB4FBAT96M", Nothing)

        Assert.Null(pachet)
        Assert.Contains(motiv, ctrl.LastFailure)
        Assert.Contains("AAB4FBAT96M", ctrl.LastFailure)
    End Function

    ' The same for the list: a failed run must not look like «FOREXE has no angajamente».
    <Fact>
    Public Async Function ListaEsuata_SpuneDeCe() As Task
        Dim ctrl As ForexeController = NewController(
            New JobResult With {.Success = False, .Message = "sesiune pierdută"})

        Dim mapate As List(Of Angajament) = Await ctrl.DownloadListaAsync()

        Assert.Null(mapate)
        Assert.Contains("sesiune pierdută", ctrl.LastFailure)
    End Function

    ' A run that succeeds clears the previous failure. Without this the shell would keep
    ' showing an old message box after a download that worked.
    <Fact>
    Public Async Function OReusitaStergeEseculAnterior() As Task
        Dim runner As New FakeRunner() With {
            .Rezultat = New JobResult With {.Success = False, .Message = "prima a picat"}}
        Dim ctrl As New ForexeController(runner, New SessionContext() With {.DbName = "000_DEMO"})

        Await ctrl.DownloadNodeAsync("AAB4FBAT96M", Nothing)
        Assert.NotEmpty(ctrl.LastFailure)

        Dim reusit As New JobResult With {.Success = True, .Message = "gata"}
        reusit.Tables("ListaAngajamente") = New TabelRezultat()
        runner.Rezultat = reusit

        Await ctrl.DownloadNodeAsync("AAB4FBAT96M", Nothing)
        Assert.Equal(String.Empty, ctrl.LastFailure)
    End Function

    ' The table the list flow expects is missing: that is a FOREXE change, not an empty
    ' list, so it must be reported and not read as «zero angajamente».
    <Fact>
    Public Async Function TabelLipsa_ENumitInEsec() As Task
        Dim ctrl As ForexeController = NewController(
            New JobResult With {.Success = True, .Message = "rulat"})

        Dim mapate As List(Of Angajament) = Await ctrl.DownloadListaAsync()

        Assert.Null(mapate)
        Assert.Contains(WorkflowCatalog.ListaAngajamenteTable, ctrl.LastFailure)
    End Function

End Class
