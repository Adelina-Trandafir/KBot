Imports System
Imports Microsoft.Extensions.DependencyInjection

' Punte între test și aplicație: serviciile DI ale shell-ului real, un log textual
' și canalul de progres. Nu depinde de API-ul exact al loggerului (doar Action(Of String)).
Public NotInheritable Class HarnessContext
    Private ReadOnly _provider As IServiceProvider
    Private ReadOnly _log As Action(Of String)
    Public ReadOnly Property Progress As IProgress(Of HarnessProgressInfo)

    Public Sub New(provider As IServiceProvider, log As Action(Of String), progress As IProgress(Of HarnessProgressInfo))
        _provider = provider
        _log = log
        Me.Progress = progress
    End Sub

    Public Function GetService(Of T)() As T
        Return _provider.GetRequiredService(Of T)()
    End Function

    Public Sub Log(message As String)
        _log(message)
    End Sub
End Class
