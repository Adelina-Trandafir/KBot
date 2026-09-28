Imports System.Threading.Tasks

''' <summary>
''' Slice 0088 -- uploading a PDF into FOREXE through «Transmitere documente electronice» (the CAB
''' correction note). Kept apart from <c>IForexeRunner</c> so the runners that do not need it (the
''' test doubles) stay as they are; <c>ForexeRunner</c> implements both and the coordinator asks
''' for this one with TryCast.
''' </summary>
Public Interface IForexeDocumentUpload

    ''' <summary>
    ''' Uploads <paramref name="pdfPath"/> on the live session and returns FOREXE's answer (the text
    ''' the form's frame shows after «Trimite»). Throws without a session or when the page does not
    ''' behave as expected.
    ''' </summary>
    Function UploadElectronicDocumentAsync(pdfPath As String) As Task(Of String)

    ''' <summary>
    ''' Slice 0088-04: looks in the FOREXE SNM inbox (category 1, «recipise») for the receipt of the
    ''' upload registered under <paramref name="registrationIndex"/> («1230450081») and downloads its
    ''' file. A receipt that is not there yet comes back with <c>Found = False</c>; throws without a
    ''' session or when the inbox does not answer as expected.
    ''' </summary>
    Function FindReceiptAsync(registrationIndex As String) As Task(Of ForexeReceipt)

End Interface

''' <summary>What <see cref="IForexeDocumentUpload.FindReceiptAsync"/> found. POCO.</summary>
Public NotInheritable Class ForexeReceipt
    Public Property Found As Boolean
    ''' <summary>How many inbox messages were read looking for it.</summary>
    Public Property MessagesRead As Integer
    Public Property RegistrationIndex As String = String.Empty
    ''' <summary>«INTERNT-1230450081-2026/28-09-2026».</summary>
    Public Property RegistrationNumber As String = String.Empty
    ''' <summary>FOREXE's id of the SNM message.</summary>
    Public Property MessageId As Long
    ''' <summary>The message text («recipisa pentru CIF ..., tip F1135, numar_inregistrare ...»).</summary>
    Public Property MessageText As String = String.Empty
    Public Property MessageDate As DateTime?
    Public Property FileName As String = String.Empty
    Public Property Content As Byte()
End Class
