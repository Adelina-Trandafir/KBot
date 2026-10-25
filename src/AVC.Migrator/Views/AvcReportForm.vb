Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' Read-only HTML view of the report (the verification and, after a migration, what was written), in the desktop HTML control, with
''' «Salvează HTML…» to keep or send the page. Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AvcReportForm

    Private Const AppTitle As String = "Migrare Avacont"

    Private ReadOnly _html As String = String.Empty
    Private _shown As Boolean
    Private _pendingHtml As Boolean = True

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_html">The finished page (<see cref="AvcReportHtml.Create"/>).</param>
    ''' <param name="k_dc">The DC, used in the proposed file name.</param>
    Friend Sub New(k_html As String, k_dc As String)
        InitializeComponent()
        _html = k_html
        sfdHtml.FileName = $"avc_raport_{If(String.IsNullOrWhiteSpace(k_dc), "DC", k_dc.Trim())}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
    End Sub

    Private Sub AvcReportForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        _shown = True
        ShowPendingHtml()
    End Sub

    Private Sub WebRaport_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles webRaport.DocumentCompleted
        ShowPendingHtml()
    End Sub

    Private Sub ShowPendingHtml()
        If Not _shown OrElse Not _pendingHtml OrElse webRaport.IsBusy OrElse _html.Length = 0 Then Return
        Try
            _pendingHtml = False
            webRaport.DocumentText = _html
        Catch ex As Exception
            GlobalErrorLog.Write("AvcReportForm.ShowHtml", ex)
            KBotMessage.Show("Raportul HTML nu a putut fi afișat." & Environment.NewLine & ex.Message,
                             AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Only the journal folder link is followed (in Explorer); the page never navigates anywhere else.</summary>
    Private Sub WebRaport_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs) Handles webRaport.Navigating
        Try
            If e.Url Is Nothing Then Return
            If e.Url.IsFile Then
                e.Cancel = True
                Dim k_folder = e.Url.LocalPath
                If Directory.Exists(k_folder) Then
                    Process.Start(New ProcessStartInfo("explorer.exe", $"""{k_folder}""") With {.UseShellExecute = False})
                End If
                Return
            End If
            Dim k_url = e.Url.AbsoluteUri
            e.Cancel = Not String.Equals(k_url, "about:blank", StringComparison.OrdinalIgnoreCase) AndAlso
                       Not k_url.StartsWith("about:blank#", StringComparison.OrdinalIgnoreCase)
        Catch ex As Exception
            GlobalErrorLog.Write("AvcReportForm.Navigating", ex)
        End Try
    End Sub

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If sfdHtml.ShowDialog(Me) <> DialogResult.OK Then Return
            File.WriteAllText(sfdHtml.FileName, _html, New UTF8Encoding(False))
            lblInfo.Text = "Salvat: " & sfdHtml.FileName
        Catch ex As Exception
            GlobalErrorLog.Write("AvcReportForm.SaveHtml", ex)
            KBotMessage.Show("Raportul HTML nu a putut fi salvat." & Environment.NewLine & ex.Message,
                             AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
