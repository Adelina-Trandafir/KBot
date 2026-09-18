Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: opens the slice 0066 bench and asks for a human verdict. The operator reads
' in the journal that every fixed column, fixed row and the padding of a KBotTableLayoutPanel
' are the authored LOGICAL number times the K-BOT scale (whole pixels, never the platform's
' font ratio); that the tree next to it scales its ItemHeight by the same factor; that the
' Automatic / Fixed 100% / Manual modes, the text size and the schemes all recompute from the
' logical source without compounding; and that the runtime API (collapse row/column,
' SetRowHeight, Padding) survives every pass. Every setting the bench touches is put back on
' close. Not destructive, no live connection.
Public NotInheritable Class TableLayoutHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "KBotTableLayoutPanel — măsuri la DPI ca arborele (0066)"
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
        Using f As New TableLayoutHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("măsurile tabelului urmează scara K-BOT"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("probă vizuală respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
