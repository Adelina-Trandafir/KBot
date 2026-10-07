Option Strict On
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Slice 0078-05, signing bench: an <see cref="IApiClient"/> that only RECORDS the signed-PDF
''' uploads. A real <see cref="PdfSigningSession"/> runs on it, so the bench sees exactly what the
''' session would send, and when -- with no server, no login and no row written.
'''
''' Built with <see cref="DispatchProxy"/>: the interface has some fifty members and the bench needs
''' two. Every other call throws <see cref="NotSupportedException"/>. Must stay a public,
''' non-sealed class with a parameterless constructor (DispatchProxy derives from it).
''' </summary>
Public Class RecordingPdfApi
    Inherits DispatchProxy

    ''' <summary>
    ''' Raised on the caller's thread (the session's: UI) for every upload. Arguments: document
    ''' type ("DDF"/"ORD"), id, the bytes, the roles («Semnatura»), the signature records.
    ''' </summary>
    Public Property Uploaded As Action(Of String, Integer, Byte(), String, IReadOnlyList(Of PdfSignatureRecord))

    ''' <summary>A recording client whose uploads go to <paramref name="onUpload"/>.</summary>
    Public Shared Function ForUploads(onUpload As Action(Of String, Integer, Byte(), String, IReadOnlyList(Of PdfSignatureRecord))) As IApiClient
        Try
            Dim api As IApiClient = DispatchProxy.Create(Of IApiClient, RecordingPdfApi)()
            DirectCast(CObj(api), RecordingPdfApi).Uploaded = onUpload
            Return api
        Catch ex As Exception
            GlobalErrorLog.Write("RecordingPdfApi.ForUploads", ex)
            Throw
        End Try
    End Function

    Protected Overrides Function Invoke(targetMethod As MethodInfo, args As Object()) As Object
        Try
            Dim name As String = If(targetMethod Is Nothing, "", targetMethod.Name)
            Select Case name
                Case NameOf(IApiClient.UploadDdfPdfAsync), NameOf(IApiClient.UploadOrdPdfAsync)
                    Dim id As Integer = CInt(args(0))
                    Dim bytes As Byte() = DirectCast(args(1), Byte())
                    Dim semnatura As String = TryCast(args(3), String)
                    Dim records As IReadOnlyList(Of PdfSignatureRecord) = TryCast(args(4), IReadOnlyList(Of PdfSignatureRecord))
                    Uploaded?.Invoke(If(name = NameOf(IApiClient.UploadDdfPdfAsync), "DDF", "ORD"), id, bytes, semnatura, records)
                    Return Task.FromResult(New PutPdfResponse With {
                        .sha256 = PdfHash.Compute(bytes), .semnatura = semnatura, .dimensiune = bytes.Length})
                Case Else
                    Throw New NotSupportedException($"The recording client does not implement {name}.")
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("RecordingPdfApi.Invoke", ex)
            Throw
        End Try
    End Function

End Class
