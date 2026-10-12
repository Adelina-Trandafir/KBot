Imports System.Data.OleDb
Imports KBot.Common

''' <summary>
''' The only way VRC.Migrator opens an Access file: READ-ONLY (<c>Mode=Read</c>), no session pooling. The shared AVACONT
''' file password is tried through <see cref="AccessFilePassword"/>; it is never logged or shown. A file without a
''' password opens on the last attempt. Nothing in this class can write to the file.
''' </summary>
Friend NotInheritable Class VrcAccess

    Private Shared ReadOnly Providers As String() = {"Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0"}

    Private Sub New()
    End Sub

    ''' <summary>Opens <paramref name="k_path"/> read-only. Returns the connection and the provider that worked.</summary>
    Friend Shared Function OpenReadOnly(k_path As String, ByRef k_provider As String) As OleDbConnection
        Try
            If String.IsNullOrWhiteSpace(k_path) OrElse Not IO.File.Exists(k_path) Then
                Throw New IO.FileNotFoundException("Fișierul Access nu există.", k_path)
            End If
            Dim k_failures As New List(Of String)()
            Dim k_passwords As New List(Of String) From {AccessFilePassword.Value(), String.Empty}
            For Each k_candidateProvider In Providers
                For Each k_password In k_passwords
                    Dim k_cn As OleDbConnection = Nothing
                    Try
                        k_cn = New OleDbConnection(Build(k_path, k_password, k_candidateProvider))
                        k_cn.Open()
                        k_provider = k_candidateProvider
                        Return k_cn
                    Catch ex As Exception
                        k_failures.Add($"{k_candidateProvider}: {ex.Message}")   ' never the password
                        k_cn?.Dispose()
                    End Try
                Next
            Next
            Throw New InvalidOperationException("Fișierul nu a putut fi deschis (doar citire). " & String.Join(" | ", k_failures.Distinct()))
        Catch ex As Exception
            GlobalErrorLog.Write("VrcAccess.OpenReadOnly", ex)
            Throw
        End Try
    End Function

    Private Shared Function Build(k_path As String, k_password As String, k_provider As String) As String
        Dim k_builder As New OleDbConnectionStringBuilder() With {.Provider = k_provider, .DataSource = k_path}
        k_builder("Mode") = "Read"                 ' read-only: the file is never written, not even a lock-file update of the data
        k_builder("OLE DB Services") = "-4"        ' no native session pooling (see KBot.Migrator AccessProvider)
        If Not String.IsNullOrEmpty(k_password) Then k_builder("Jet OLEDB:Database Password") = k_password
        Return k_builder.ConnectionString
    End Function

End Class
