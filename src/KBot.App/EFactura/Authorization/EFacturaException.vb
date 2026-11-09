Option Strict On

''' <summary>
''' A failure of an E-Factura step the operator has to be told about (slice 00EF-05): no certificate on the PC,
''' the connection with the certificate to ANAF failed, ANAF gave no code. The message is Romanian, ready to show, and
''' never carries a code, a token or a secret. Server refusals stay <c>KBot.Api.ApiException</c> (their text comes
''' from the server).
''' </summary>
Public Class EFacturaException
    Inherits Exception

    Public Sub New(k_message As String)
        MyBase.New(k_message)
    End Sub

    Public Sub New(k_message As String, k_inner As Exception)
        MyBase.New(k_message, k_inner)
    End Sub

End Class
