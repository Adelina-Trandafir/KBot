Namespace WorkflowModels
    ''' <summary>
    ''' Setările de throttle pentru rețea. Se aplică via CDP înainte de fiecare rulare.
    ''' downloadThroughput / uploadThroughput: bytes/s, -1 = fără limită
    ''' latency: ms
    ''' </summary>
    Public Class ThrottleSettings

        Public Property Enabled As Boolean = False
        Public Property DownloadThroughput As Double = -1   ' bytes/s
        Public Property UploadThroughput As Double = -1     ' bytes/s
        Public Property Latency As Double = 0               ' ms
        Public Property Label As String = "Niciun throttle"

        ' ----------------------------------------------------------------
        ' Preset-uri predefinite
        ' ----------------------------------------------------------------
        Public Shared ReadOnly Property None As ThrottleSettings
            Get
                Return New ThrottleSettings With {
                    .Enabled = False,
                    .DownloadThroughput = -1,
                    .UploadThroughput = -1,
                    .Latency = 0,
                    .Label = "Niciun throttle"
                }
            End Get
        End Property

        Public Shared ReadOnly Property Fast3G As ThrottleSettings
            Get
                Return New ThrottleSettings With {
                    .Enabled = True,
                    .DownloadThroughput = 187500,  ' 1.5 Mbps
                    .UploadThroughput = 93750,     ' 750 Kbps
                    .Latency = 40,
                    .Label = "Fast 3G (~1.5 Mbps)"
                }
            End Get
        End Property

        Public Shared ReadOnly Property Slow3G As ThrottleSettings
            Get
                Return New ThrottleSettings With {
                    .Enabled = True,
                    .DownloadThroughput = 50000,   ' 400 Kbps
                    .UploadThroughput = 50000,
                    .Latency = 400,
                    .Label = "Slow 3G (~400 Kbps)"
                }
            End Get
        End Property

        Public Shared ReadOnly Property TwoG As ThrottleSettings
            Get
                Return New ThrottleSettings With {
                    .Enabled = True,
                    .DownloadThroughput = 25000,   ' 200 Kbps
                    .UploadThroughput = 25000,
                    .Latency = 800,
                    .Label = "2G (~200 Kbps)"
                }
            End Get
        End Property

        ''' <summary>Returnează parametrii CDP gata de trimis.</summary>
        Public Function ToCDPParams() As Dictionary(Of String, Object)
            Return New Dictionary(Of String, Object) From {
                {"offline", False},
                {"latency", Latency},
                {"downloadThroughput", DownloadThroughput},
                {"uploadThroughput", UploadThroughput}
            }
        End Function

        ''' <summary>Dezactivare throttle — parametrii CDP pentru reset.</summary>
        Public Shared Function DisableParams() As Dictionary(Of String, Object)
            Return New Dictionary(Of String, Object) From {
                {"offline", False},
                {"latency", 0},
                {"downloadThroughput", -1},
                {"uploadThroughput", -1}
            }
        End Function

    End Class

    ''' <summary>
    ''' Marchează o proprietate ca atribut REQUIRED în XML.
    ''' Reflection-ul din TagMap o detectează automat.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property)>
    Public Class WflRequiredAttribute
        Inherits Attribute
    End Class

    ''' <summary>
    ''' Marchează o proprietate ca fiind parte dintr-un grup în care trebuie completat cel puțin unul dintre atribute.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property)>
    Public Class WflRequiredOneOfAttribute
        Inherits Attribute
        Public Property GroupName As String
        Public Sub New(groupName As String)
            Me.GroupName = groupName  ' toate prop cu același GroupName formează un grup
        End Sub
    End Class

    ''' <summary>
    ''' Workflow variable definition
    ''' </summary>
    Public Class WorkflowVariable
        Public Property Name As String
        Public Property Description As String = ""
        Public Property Value As String = ""
        Public Property VarType As String = "Text" ' Text, Numeric
        Public Property Min As Double? = Nothing
        Public Property Max As Double? = Nothing
        Public Property Length As Integer = 0
        Public Property IsRequired As Boolean = False
        Public Property Mask As String = ""
    End Class

    ''' <summary>
    ''' Root workflow containing all actions to execute
    ''' </summary>
    Public Class Workflow
        Public Property TaskId As Integer = 0
        Public Property Name As String = String.Empty
        Public Property StartUrl As String = String.Empty
        Public Property ExpectedUrl As String = String.Empty
        Public Property Actions As New List(Of IWorkflowAction)
        Public Property Receive As Boolean = False
        Public Property FilePath As String = String.Empty
    End Class

    ''' <summary>
    ''' Base interface for all workflow actions
    ''' </summary>
    Public Interface IWorkflowAction
        ReadOnly Property ActionType As String
        Property Timeout As Integer
        Property IsCheckpoint As Boolean
        Property LogValue As String
    End Interface

    Public Interface ILoopAction
        Inherits IWorkflowAction

        ' Variabila INTERNĂ care se suprascrie la fiecare iterație
        Property RuntimeIndex As Integer
    End Interface

    Public Class WorkflowExitException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

End Namespace
