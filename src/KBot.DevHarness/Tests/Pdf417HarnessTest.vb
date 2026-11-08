Imports System.Threading
Imports System.Threading.Tasks

Public NotInheritable Class Pdf417HarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String = "PDF417 AVACONT — probă cu telefonul" Implements IHarnessTest.Name
    Public ReadOnly Property Category As String = "Controls/UI" Implements IHarnessTest.Category
    Public ReadOnly Property RequiresLiveConnection As Boolean = False Implements IHarnessTest.RequiresLiveConnection
    Public ReadOnly Property IsDestructive As Boolean = False Implements IHarnessTest.IsDestructive

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Using form As New Pdf417HarnessForm()
            Select Case form.ShowDialog()
                Case DialogResult.OK
                    Return Task.FromResult(HarnessTestResult.Passed("Cititorul a returnat textul introdus."))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("Cititorul nu a returnat textul introdus."))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("Proba a fost închisă fără verdict."))
            End Select
        End Using
    End Function
End Class
