Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: opens the message catalog -- every message box of the application in one grid
' (type, buttons, caption, text, the function it is called from), an editor on the right, and a
' preview that opens the real KBotMessageBoxForm. «Salvează în JSON» writes
' src\KBot.DevHarness\Config\mesaje_catalog.json (made by tools\MessageCatalog\scan.js).
' NOT destructive, needs no live connection.
Public NotInheritable Class MessageCatalogTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Mesaje — catalogul tuturor casetelor de mesaj (editor)"
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
        Using f As New MessageCatalogForm()
            f.ShowDialog()
        End Using
        Return Task.FromResult(HarnessTestResult.Passed("catalog deschis"))
    End Function
End Class
