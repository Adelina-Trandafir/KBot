Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: opens the KBotComboBox bench -- the combo next to a PropertyGrid bound to it
' (so ALL of its editable properties, with their categories and descriptions), plus quick switches
' for Editable / LimitToList / TextOffsetY and a measurement line reading the native EDIT's real
' rectangle back from Windows, the only way to see whether TextOffsetY actually moved it.
' NOT destructive, needs no live connection. Theme and scale are restored on close.
Public NotInheritable Class KBotComboBoxPlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "KBotComboBox — proprietăți runtime (playground)"
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
        Using f As New ComboPlaygroundForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("proprietăți runtime OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("playground respins de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
