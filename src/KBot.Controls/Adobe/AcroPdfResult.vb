Option Strict On

' The outcome of AcroPdfViewer.ShowDocumentAsync. Moved here unchanged when the old ActiveX viewer (AcroPdfSurface)
' was removed (slice 0078-15, operator 07.10.2026).
''' <summary>How an attempt to show a document in the ActiveX control ended.</summary>
Public Enum AcroPdfStatus
    ''' <summary>Loaded, laid out, and the panes are collapsed.</summary>
    Shown = 0
    ''' <summary>The AcroPDF control is not registered on this machine.</summary>
    NotRegistered = 1
    ''' <summary>The file is not on disk.</summary>
    FileMissing = 2
    ''' <summary>The control refused the document, or could not be created.</summary>
    Failed = 3
End Enum

''' <summary>The status, the Romanian sentence for the operator, and what the collapse achieved.</summary>
Public NotInheritable Class AcroPdfResult

    Public ReadOnly Property Status As AcroPdfStatus
    Public ReadOnly Property Message As String
    ''' <summary>True when the document view ended up filling the panel (panes collapsed).</summary>
    Public ReadOnly Property Collapsed As Boolean

    Public Sub New(status As AcroPdfStatus, message As String, Optional collapsed As Boolean = False)
        Me.Status = status
        Me.Message = If(message, "")
        Me.Collapsed = collapsed
    End Sub

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return Status = AcroPdfStatus.Shown
        End Get
    End Property

End Class
