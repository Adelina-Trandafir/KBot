Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide bancul de sarcina al lui KBotChartView + KBotLaneView — aceleasi
' doua suprafete pe care le pune AsociereForm, umplute cu un tablou nascocit a carui marime se
' tasteaza sus (R recepții × H instantanee, plus neasezate si plati). Nu cere live, nu strica
' nimic. Ce se masoara aici, si nu se poate masura fara ecran: milisecunde pe cadru pentru
' fiecare suprafata in parte, timpul reconstructiei dupa o mutare, si cadrele pe secunda cat
' timp operatorul trage un marcaj cu mouse-ul.
Public NotInheritable Class AsociereStressTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Chart + Lane — banc de sarcină (R × H)"
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
        Using f As New AsociereStressForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("suprafețele se mișcă acceptabil"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("suprafețele rămân prea încete"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
