Imports System
Imports Microsoft.Data.Sqlite

' Stub. Conexiunea/schema reală vin în felia 1.
Public Class SqliteTempStore
    Implements ITempStore

    Private _connection As SqliteConnection

    Public Sub Open() Implements ITempStore.Open
        Throw New NotImplementedException()
    End Sub

    Public Sub Reset() Implements ITempStore.Reset
        Throw New NotImplementedException()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _connection?.Dispose()
        _connection = Nothing
        GC.SuppressFinalize(Me)
    End Sub
End Class
