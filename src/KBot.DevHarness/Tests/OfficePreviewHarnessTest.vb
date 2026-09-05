Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: the bench for OfficeDocumentHost -- the Excel/Word hosting behind the DDF
' editor's file preview. Pick a spreadsheet or a document, watch it embed with no ribbon, no formula
' bar and no status bar, then close it and check that the process counter goes back to where it
' started. Needs Microsoft Office on the machine; without it the host says so in Romanian and the
' bench is still a valid run (that message IS the behaviour to check).
' NOT destructive -- the file is opened READ-ONLY, in a private instance -- and does not need live.
Public NotInheritable Class OfficePreviewHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Office — previzualizare Excel/Word în panou (banc)"
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
        Using f As New OfficePreviewHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("document incorporat si eliberat curat"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("banc respins de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("inchis fara verdict"))
        End Select
    End Function

End Class
