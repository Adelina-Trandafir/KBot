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
''' the shell saves the new ones into <c>FX_Operatiuni</c> and warns the operator.
'''
''' <para>Save first, then one message: it lists the operations and says in its last line
''' whether they were saved, so the operator reads one box, not two. A failed save does not
''' hide the warning.</para>
'''
''' <para>Next step (not in this slice): correlating these operations with the angajamente
''' in the database - see <c>docs/worklog/SLICE-0084-operatiuni-necorectate.md</c>.</para>
''' </summary>
Partial Public Class KbotForm

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
            Dim saveLine As String = Await SalveazaOperatiunileNecorectateAsync(page.Rows)
            Dim text As String = UncorrectedOperations.Message(page)
            If Not String.IsNullOrEmpty(saveLine) Then
                text &= Environment.NewLine & Environment.NewLine & saveLine
            End If
            KBotMessage.Show(Me, text, "FOREXE - Operațiuni necorectate",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaOperatiunileNecorectate", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the rows; returns the closing line of the warning (Romanian): how many were new,
    ''' which were skipped and why, or why the save failed. Never throws: the warning about the
    ''' operations matters more than the save.
    ''' </summary>
    Private Async Function SalveazaOperatiunileNecorectateAsync(rows As List(Of UncorrectedOperation)) As Task(Of String)
        Try
            Dim api As IUncorrectedOperationsApi = TryCast(_apiClient, IUncorrectedOperationsApi)
            If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IUncorrectedOperationsApi.")

            Dim result As UncorrectedOperationsSaveResult = Await WithReauth(
                Function() api.SaveUncorrectedOperationsAsync(rows, CancellationToken.None))

            Dim sb As New StringBuilder()
            If result.AlreadySaved = rows.Count Then
                sb.Append("Toate operațiunile erau deja salvate în K-BOT.")
            Else
                sb.Append("Salvate în K-BOT: ").Append(result.Inserted).Append(" noi")
                If result.AlreadySaved > 0 Then sb.Append(", ").Append(result.AlreadySaved).Append(" erau deja salvate")
                sb.Append("."c)
            End If
            For Each w As String In result.Warnings
                sb.AppendLine().Append("⚠ ").Append(w)
            Next
            Return sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SalveazaOperatiunileNecorectateAsync", ex)
            Return "⚠ Operațiunile NU au putut fi salvate în K-BOT: " & ex.Message
        End Try
    End Function

End Class
