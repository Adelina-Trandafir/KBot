Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports KBot.Common

''' <summary>
''' Opens an .accdb file through the ACE OLE DB provider.
''' </summary>
''' <remarks>
''' Slice 0045-01 verified on the target machine that ACE 16.0 and 12.0 are both
''' registered 64-bit, and that all three .accdb files in the estate open with no
''' password at all - despite slice 0044 recording them as encrypted. The password
''' path stays wired because an operator's own copy may still be protected.
'''
''' The connection string is built with OleDbConnectionStringBuilder rather than
''' concatenated, so a password containing ';' or '"' is escaped by the builder
''' instead of terminating the string.
''' </remarks>
Public NotInheritable Class AccessProvider

    ''' <summary>Providers tried in order. 16.0 first, 12.0 as the fallback.</summary>
    Public Shared ReadOnly Providers As String() = {"Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0"}

    ''' <summary>The property name that carries an .accdb password.</summary>
    ''' <remarks>
    ''' Still spelled "Jet OLEDB:" on ACE, despite the engine no longer being Jet.
    ''' </remarks>
    Private Const PasswordProperty As String = "Jet OLEDB:Database Password"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Opens <paramref name="path"/>, trying each provider in turn.
    ''' </summary>
    ''' <returns>An open connection. The caller owns it.</returns>
    ''' <exception cref="AccessOpenException">
    ''' No provider could open the file. The message is operator-facing Romanian and
    ''' never contains the password.
    ''' </exception>
    Public Shared Function Open(path As String, password As String) As OleDbConnection
        Try
            If String.IsNullOrWhiteSpace(path) Then
                Throw New ArgumentException("Calea fișierului Access lipsește.", NameOf(path))
            End If

            If Not File.Exists(path) Then
                Throw New AccessOpenException(
                    $"Fișierul Access «{path}» nu există.", Nothing)
            End If

            Dim failures As New List(Of String)()

            For Each provider In Providers
                Dim cn As OleDbConnection = Nothing
                Try
                    cn = New OleDbConnection(BuildConnectionString(path, password, provider))
                    cn.Open()
                    Return cn
                Catch ex As Exception
                    ' Not a failure yet - the next provider may still open it. Record the
                    ' reason so a total failure can name every attempt.
                    failures.Add($"{provider}: {ex.Message}")
                    If cn IsNot Nothing Then cn.Dispose()
                End Try
            Next

            Throw New AccessOpenException(BuildFailureMessage(path, failures), Nothing)

        Catch ex As Exception
            GlobalErrorLog.Write("AccessProvider.Open", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' True when at least one ACE provider is registered and can open
    ''' <paramref name="probePath"/>. Used by the form at startup so a missing provider
    ''' is reported in Romanian rather than as a raw COM error at the first read.
    ''' </summary>
    Public Shared Function CanOpen(probePath As String, password As String, ByRef reason As String) As Boolean
        reason = String.Empty
        Try
            Using cn = Open(probePath, password)
                Return cn.State = ConnectionState.Open
            End Using
        Catch ex As AccessOpenException
            reason = ex.Message
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("AccessProvider.CanOpen", ex)
            reason = ex.Message
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The ACE providers registered on this machine, for the failure message.
    ''' </summary>
    Public Shared Function RegisteredProviders() As List(Of String)
        Dim found As New List(Of String)()
        Try
            Using enumerator = OleDbEnumerator.GetRootEnumerator()
                While enumerator.Read()
                    Dim name = TryCast(enumerator.GetValue(enumerator.GetOrdinal("SOURCES_NAME")), String)
                    If Not String.IsNullOrEmpty(name) Then found.Add(name)
                End While
            End Using
        Catch ex As Exception
            ' The list is a courtesy in an error message, never the point of the call.
            GlobalErrorLog.Write("AccessProvider.RegisteredProviders", ex)
        End Try
        Return found
    End Function

    Private Shared Function BuildConnectionString(path As String, password As String, provider As String) As String
        Dim builder As New OleDbConnectionStringBuilder() With {
            .Provider = provider,
            .DataSource = path
        }
        If Not String.IsNullOrEmpty(password) Then
            builder(PasswordProperty) = password
        End If
        ' -4 turns off native resource (session) pooling and auto transaction enlistment.
        ' Without it a pooled ACE session can outlive cn.Dispose(), sitting in a
        ' process-wide native pool until the CLR's own non-deterministic cleanup - a
        ' known source of an access violation racing against process teardown on the
        ' .NET 8 System.Data.OleDb port.
        builder("OLE DB Services") = "-4"
        Return builder.ConnectionString
    End Function

    Private Shared Function BuildFailureMessage(path As String, failures As IEnumerable(Of String)) As String
        Dim sb As New Text.StringBuilder()
        sb.Append($"Fișierul Access «{path}» nu a putut fi deschis.")
        sb.AppendLine()
        sb.AppendLine()
        sb.AppendLine("Motivele, pe furnizor:")
        For Each f In failures
            sb.AppendLine($"  - {f}")
        Next

        Dim registered = RegisteredProviders()
        sb.AppendLine()
        If registered.Count = 0 Then
            sb.AppendLine("Pe acest calculator nu s-a putut citi lista furnizorilor OLE DB.")
        Else
            sb.AppendLine("Furnizorii OLE DB înregistrați pe acest calculator:")
            For Each r In registered
                sb.AppendLine($"  - {r}")
            Next
        End If
        sb.AppendLine()
        sb.Append("Dacă lipsește «Microsoft.ACE.OLEDB.16.0» sau «Microsoft.ACE.OLEDB.12.0», ")
        sb.Append("instalați «Microsoft Access Database Engine» pe 64 de biți.")
        Return sb.ToString()
    End Function

End Class

''' <summary>
''' An .accdb file could not be opened. Carries an operator-facing Romanian message.
''' </summary>
Public NotInheritable Class AccessOpenException
    Inherits Exception

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub

End Class
