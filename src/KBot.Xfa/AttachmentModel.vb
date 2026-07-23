Option Strict On

''' <summary>
''' Model pentru atașamentele PDF extrase din XML (codate base64).
''' POCO pur — fără logică, portat ca atare din XFA_WRITTER.
''' </summary>
Public Class AttachmentModel
    Public Property FileName As String
    Public Property FileData As Byte()
    Public Property IsDeleted As Boolean = False

    ''' <summary>
    ''' Alias read-only pentru FileName (compatibilitate cu apelanții vechi
    ''' care citeau FilePath).
    ''' </summary>
    Public ReadOnly Property FilePath As String
        Get
            Return FileName
        End Get
    End Property
End Class
