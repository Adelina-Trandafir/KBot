Imports System
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Forexe

' FOREXE, LIVE: reproduce exact pașii de conectare din MainForm.btnConnect_Click (calea A3),
' folosind serviciile reale din DI. CertificateSelectionForm și RichTextBoxLogger sunt în
' namespace global (din KBot.Forexe). Lasă browser-ul exact cum îl lasă A3 (runner-ul gestionează).
Public NotInheritable Class ForexeConnectTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — Conectare (live)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "FOREXE"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return True
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        ' 1) Runner-ul real din DI (singleton, același pe care îl folosește MainForm).
        Dim runner As IForexeRunner = context.GetService(Of IForexeRunner)()

        ' 2) Picker de certificat în mod manual de PIN (ca MainForm.SelectCertificate).
        Dim cert As X509Certificate2
        Using dlg As New CertificateSelectionForm(manualPin:=True)
            If dlg.ShowDialog() <> DialogResult.OK Then
                Return HarnessTestResult.Skipped("no certificate")
            End If
            cert = dlg.SelectedCertificate
        End Using
        If cert Is Nothing Then
            Return HarnessTestResult.Skipped("no certificate")
        End If

        ' 3) Același JobRequest ca MainForm pentru "adlop - Conectare.wfl".
        Dim job As New JobRequest With {
            .WorkflowName = "Conectare",
            .WflPath = Path.Combine(AppContext.BaseDirectory, "Workflows", "adlop - Conectare.wfl")
        }
        If Not File.Exists(job.WflPath) Then
            Return HarnessTestResult.Failed("Workflow lipsă: " & job.WflPath,
                "Copiază 'adlop - Conectare.wfl' în <AppDir>\Workflows\ înainte de a rula testul.")
        End If

        ' 4) Progres pe procent → înaintat la context (status + log), exact ca pbProgress din MainForm.
        Dim progress As New Progress(Of Integer)(
            Sub(p) context.Progress.Report(New HarnessProgressInfo(Math.Min(p, 100), "conectare " & p & "%")))

        ' 5) Același overload RunAsync ca MainForm; succes = result.Success.
        Dim result As JobResult = Await runner.RunAsync(job, cert, progress, ct)
        If result.Success Then
            Return HarnessTestResult.Passed("conectat" & If(String.IsNullOrEmpty(result.Message), "", " — " & result.Message))
        Else
            Return HarnessTestResult.Failed("conectare eșuată" & If(String.IsNullOrEmpty(result.Message), "", " — " & result.Message))
        End If
    End Function
End Class
