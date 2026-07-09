' Datele pentru conversia unui Excel în JSON pe server (/api/tools/process_excel).
' Stă în KBot.Common ca să fie văzut și de executorul FOREXE (care îl umple) și de
' ApiClient (care face apelul), fără ca FOREXE să depindă de KBot.Api.
Public Class ExcelJob
    Public Property FileBase64 As String = String.Empty
    Public Property HeaderRows As Integer = 1
    Public Property SkipFirstNRows As Integer = 0
    Public Property SkipLastNRows As Integer = 0
    Public Property SkipFirstNColumns As Integer = 0
    Public Property SkipLastNColumns As Integer = 0
    Public Property ComplexFilter As String = String.Empty
    Public Property FilterColumn As String = String.Empty
    Public Property Filter As String = String.Empty
End Class
