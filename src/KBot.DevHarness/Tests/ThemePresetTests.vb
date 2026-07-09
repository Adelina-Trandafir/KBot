Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Theming

' Probe vizuale per-temă: deschid galeria pre-setată pe o schemă anume, ca operatorul
' să inspecteze fiecare temă individual. Base MustInherit (sărită de discovery, e
' abstractă); subclasele concrete au ctor fără parametri => apar singure în harness.
Public MustInherit Class ThemePresetTestBase
    Implements IHarnessTest

    ''' <summary>Schema cu care se deschide galeria.</summary>
    Protected MustOverride Function BuildScheme() As ThemeScheme

    Public MustOverride ReadOnly Property Name As String Implements IHarnessTest.Name

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
        Using f As New ThemeGalleryForm(AddressOf context.Log, BuildScheme())
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("temă OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("temă respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class

' ── Classic ──────────────────────────────────────────────────────────────────
Public NotInheritable Class ThemeClassicTest
    Inherits ThemePresetTestBase

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Temă — Classic (previzualizare)"
        End Get
    End Property
    Protected Overrides Function BuildScheme() As ThemeScheme
        Return BuiltInSchemes.Classic()
    End Function
End Class

' ── Dark ─────────────────────────────────────────────────────────────────────
Public NotInheritable Class ThemeDarkTest
    Inherits ThemePresetTestBase

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Temă — Dark (previzualizare)"
        End Get
    End Property
    Protected Overrides Function BuildScheme() As ThemeScheme
        Return BuiltInSchemes.Dark()
    End Function
End Class

' ── Modern ───────────────────────────────────────────────────────────────────
Public NotInheritable Class ThemeModernTest
    Inherits ThemePresetTestBase

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Temă — Modern (previzualizare)"
        End Get
    End Property
    Protected Overrides Function BuildScheme() As ThemeScheme
        Return BuiltInSchemes.Modern()
    End Function
End Class
