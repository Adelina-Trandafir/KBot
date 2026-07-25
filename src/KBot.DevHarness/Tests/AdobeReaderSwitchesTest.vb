Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Adobe/PDF, safe: deschide bancul de probă care încorporează Adobe Reader/Acrobat DC într-un
' panou-gazdă și expune ca bife fiecare switch de linie de comandă / parametru de deschidere care
' ascunde chrome-ul (bare de instrumente, panouri de navigare, bară de stare/mesaje, derulare).
' La orice bifă schimbată închide instanța Adobe curentă și o redeschide cu noul set de switch-uri;
' la închidere OMOARĂ forțat Adobe după PID. NU e distructiv (nu atinge date), nu cere live.
Public NotInheritable Class AdobeReaderSwitchesTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Adobe Reader DC — încorporare + switch-uri (bare ascunse)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
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
        Using f As New AdobeReaderHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("încorporare + switch-uri OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
