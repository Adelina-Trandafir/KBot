Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: opens the slice 0062 bench and asks for a human verdict. The operator
' switches schemes and the text size and reads, in the journal, that the window never drops
' below its base and grows to what the theme asks; opens the three probe windows and reads which
' monitor they landed on (the application's, not the mouse's); and watches the authored-short
' table grow its rows under Modern, keep its label column in step with the label, and survive a
' collapsed band. Every setting the bench touches is put back on close. Not destructive, no live
' connection.
Public NotInheritable Class FormFitHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Potrivirea ferestrei la temă + KBotTableLayoutPanel (0062)"
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
        Using f As New FormFitHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("potrivire la temă, centrare și tabel OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("probă vizuală respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
