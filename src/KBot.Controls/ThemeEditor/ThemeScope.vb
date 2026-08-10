Option Strict On
Imports System.Windows.Forms

''' <summary>
''' O „gazdă” pe care editorul de stiluri o poate lua în lucru: fie FEREASTRA (formularul), fie o
''' SUB-FEREASTRĂ (o vedere găzduită). Cerința operatorului cere exact distincția asta — «detect
''' the window (form) or the sub-window (view)» — iar în K-BOT ea are o formă concretă: vederile
''' (`SumarView`, `IstoricView`, …) sunt `UserControl`-uri așezate în gazda de vederi din MainForm.
'''
''' <see cref="Root"/> e rădăcina relativ la care se calculează căile din fișier, iar
''' <see cref="ScopeName"/> e numele TIPULUI ei — cheia scrisă în <c>ThemeOverrideSet.Scope</c>.
''' </summary>
Public NotInheritable Class ThemeScope

    Public ReadOnly Property Root As Control
    Public ReadOnly Property IsForm As Boolean

    Public Sub New(root As Control, isForm As Boolean)
        If root Is Nothing Then Throw New ArgumentNullException(NameOf(root))
        Me.Root = root
        Me.IsForm = isForm
    End Sub

    ''' <summary>Numele tipului gazdei — cheia de potrivire a fișierului de suprascrieri.</summary>
    Public ReadOnly Property ScopeName As String
        Get
            Return Root.GetType().Name
        End Get
    End Property

    ''' <summary>Eticheta din lista derulantă: «Fereastră: MainForm» / «Vedere: SumarView».</summary>
    Public Overrides Function ToString() As String
        Return If(IsForm, $"Fereastră: {ScopeName}", $"Vedere: {ScopeName}")
    End Function

    ''' <summary>
    ''' Formularul + fiecare <see cref="UserControl"/> descendent, în ordinea întâlnirii. Vederile
    ''' K-BOT se creează LENEȘ (la prima activare din bara de navigare), deci lista conține doar
    ''' vederile deja deschise — de aceea editorul are un buton de reîmprospătare.
    ''' </summary>
    Public Shared Function Collect(host As Form) As List(Of ThemeScope)
        Dim result As New List(Of ThemeScope)()
        If host Is Nothing Then Return result
        result.Add(New ThemeScope(host, True))
        CollectViews(host, result)
        Return result
    End Function

    ' Recursie oprită la prima vedere găsită pe o ramură: o vedere e o gazdă în sine, iar
    ' controalele ei se văd deja în arborele acelei vederi.
    Private Shared Sub CollectViews(parent As Control, into As List(Of ThemeScope))
        For Each child As Control In parent.Controls
            If TypeOf child Is UserControl Then
                into.Add(New ThemeScope(child, False))
            Else
                CollectViews(child, into)
            End If
        Next
    End Sub

End Class
