Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>Read-only HTML view of a report snapshot, using the existing desktop HTML control.</summary>
Public Class VrcReportForm

    Private ReadOnly _html As String = String.Empty
    Private _shown As Boolean
    Private _pendingHtml As Boolean = True

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(k_report As String, k_status As String)
        InitializeComponent()
        _html = VrcReportHtml.Create(k_report, k_status)
        sfdHtml.FileName = $"vrc_raport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
    End Sub

    Private Sub ReportForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
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
            GlobalErrorLog.Write("VrcReportForm.ShowHtml", ex)
            KBotMessage.Show("Raportul HTML nu a putut fi afișat." & Environment.NewLine & ex.Message,
                             "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub WebRaport_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs) Handles webRaport.Navigating
        If e.Url Is Nothing Then Return
        If e.Url.IsFile Then
            e.Cancel = True
            VrcShell.RevealInExplorer(e.Url.LocalPath)
            Return
        End If
        Dim k_url = e.Url.AbsoluteUri
        e.Cancel = Not String.Equals(k_url, "about:blank", StringComparison.OrdinalIgnoreCase) AndAlso
                   Not k_url.StartsWith("about:blank#", StringComparison.OrdinalIgnoreCase)
    End Sub

    Private Sub BtnSalveazaHtml_Click(sender As Object, e As EventArgs) Handles btnSalveazaHtml.Click
        Try
            If sfdHtml.ShowDialog(Me) <> DialogResult.OK Then Return
            File.WriteAllText(sfdHtml.FileName, _html, New UTF8Encoding(False))
            lblInfo.Text = "Salvat: " & sfdHtml.FileName
        Catch ex As Exception
            GlobalErrorLog.Write("VrcReportForm.SaveHtml", ex)
            KBotMessage.Show("Raportul HTML nu a putut fi salvat." & Environment.NewLine & ex.Message,
                             "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
