Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 0084 - the «Operațiuni necorectate» of the FOREXE landing page. Right after every
''' successful FOREXE login <see cref="ForexeController"/> reads the table; when it has rows
''' the shell saves the new ones into <c>FX_Operatiuni</c>.
'''
''' <para>Slice 0088 (operator, 28.09.2026): no message box any more. The rows already in
''' <c>FX_Operatiuni</c> are neither saved again nor shown again; when the save inserted NEW
''' «ERRRRRRRRRR» rows, the note window opens straight away on exactly those. Everything not
''' correlated stays reachable from the menu «Operațiuni necorelate» (<c>KbotForm.CabNotes.vb</c>).</para>
''' </summary>
Partial Public Class KbotForm

    Private Const UncorrectedCaption As String = "FOREXE - Operațiuni necorectate"

    Private Sub LeagaOperatiunileNecorectate()
        AddHandler _controller.UncorrectedOperationsFound, AddressOf Controller_UncorrectedOperationsFound
    End Sub

    Private Sub DezleagaOperatiunileNecorectate()
        RemoveHandler _controller.UncorrectedOperationsFound, AddressOf Controller_UncorrectedOperationsFound
    End Sub

    ' Raised inside ConnectAsync: queued onto the UI thread so the connection finishes first.
    Private Sub Controller_UncorrectedOperationsFound(sender As Object, page As UncorrectedOperationsPage)
        Try
            If page Is Nothing OrElse page.Rows.Count = 0 Then Return
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            BeginInvoke(Sub() TrateazaOperatiunileNecorectate(page))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Controller_UncorrectedOperationsFound", ex)
        End Try
    End Sub

    ' UI boundary (async Sub started from BeginInvoke): log and tell, never rethrow.
    Private Async Sub TrateazaOperatiunileNecorectate(page As UncorrectedOperationsPage)
        Try
            Dim result As UncorrectedOperationsSaveResult = Await SalveazaOperatiunileNecorectateAsync(page.Rows)
            Dim newIds As IEnumerable(Of Integer) = If(result Is Nothing, Enumerable.Empty(Of Integer)(), result.NewIds)
            Await ShowUncorrelatedAsync(onlyIds:=newIds.ToList())
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaOperatiunileNecorectate", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the rows (the server inserts only those not in <c>FX_Operatiuni</c> yet). The rows it
    ''' skipped and why go to the operator log; a failed save is the one thing said in a box --
    ''' without it the rows cannot be correlated at all. Returns Nothing when the save failed.
    ''' </summary>
    Private Async Function SalveazaOperatiunileNecorectateAsync(rows As List(Of UncorrectedOperation)) As Task(Of UncorrectedOperationsSaveResult)
        Try
            Dim api As IUncorrectedOperationsApi = TryCast(_apiClient, IUncorrectedOperationsApi)
            If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IUncorrectedOperationsApi.")

            Dim result As UncorrectedOperationsSaveResult = Await WithReauth(
                Function() api.SaveUncorrectedOperationsAsync(rows, CancellationToken.None))

            If result.Warnings.Count > 0 Then
                Dim sb As New StringBuilder("Operațiuni necorectate care NU au fost salvate în K-BOT:")
                For Each w As String In result.Warnings
                    sb.AppendLine().Append("⚠ ").Append(w)
                Next
                OperatorLog.Write("MainForm.SalveazaOperatiunileNecorectateAsync", UncorrectedCaption,
                                  sb.ToString(), KBotLogLevel.Warn)
            End If
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SalveazaOperatiunileNecorectateAsync", ex)
            KBotMessage.Show(Me, "Operațiunile necorectate din FOREXE NU au putut fi salvate în K-BOT: " & ex.Message,
                             UncorrectedCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return Nothing
        End Try
    End Function

End Class
