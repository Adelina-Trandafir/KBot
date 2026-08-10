Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide «playground-ul» CustomPopup — meniul contextual tematizat, cu
' pictogramă pe fiecare rând, literă de acces («&Salvează» → S), selecție pusă din constructor și
' tastatură egală cu mouse-ul. Se deschide în trei feluri (sub buton, la cursor, clic dreapta),
' cu conținutul comutabil, și se poate comuta schema LIVE ca să se vadă că-și ia culorile din ea.
' NU e distructiv, nu cere live.
Public NotInheritable Class CustomPopupPlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "CustomPopup — meniu tematizat (playground)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Controls/UI"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Dim verdict As DialogResult
        Using f As New PopupPlaygroundForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("meniu tematizat OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("playground respins de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
