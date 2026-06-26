Imports System
Imports Xunit
Imports KBot.LocalStore

Public Class SqliteTempStoreTests
    <Fact>
    Public Sub Open_StubAruncaNotImplemented()
        Using store As ITempStore = New SqliteTempStore()
            Assert.Throws(Of NotImplementedException)(Sub() store.Open())
        End Using
    End Sub
End Class
