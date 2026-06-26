Namespace WorkflowModels

    ''' <summary>
    ''' ExtractXmlFromPdf — Scanează un folder de PDF-uri de extrase de cont,
    ''' filtrează după data din numele fișierului, extrage XML-ul embedded din fiecare PDF
    ''' și returnează un JArray cu { PdfFisier, DataFisier, XmlContent } per fișier.
    ''' </summary>
    Public Class ExtractXmlFromPdfAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "ExtractXmlFromPdf"
            End Get
        End Property

        Public Property Timeout As Integer = 60 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Folder As String = String.Empty
        <WflRequired> Public Property SaveTo As String = String.Empty
        Public Property DataDeLa As String = String.Empty
    End Class

End Namespace
