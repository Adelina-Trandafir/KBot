Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: opens the KBotRichTextEditor bench -- the editor next to a PropertyGrid bound
' to it (so ALL of its editable properties, with their categories and descriptions), the switches
' that do not fit in a property (which icon set is bound, in which order, the application scale)
' and a measurement line: the scale factor, the header height in real pixels next to the logical
' number, and -- button by button -- whether a picture landed on it or the fallback letter did.
' NOT destructive, needs no live connection. Theme and scale are restored on close.
Public NotInheritable Class KBotRichTextEditorPlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "KBotRichTextEditor — proprietăți runtime (playground)"
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
        Using f As New RichTextPlaygroundForm(AddressOf context.Log)
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
