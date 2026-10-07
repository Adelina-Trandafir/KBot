#If DEBUG Then
Option Strict On
Imports System.IO
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' The bench for slice 0078-15: ONLY the AcroPDF ActiveX control (<see cref="AcroPdfViewer"/>, the engine the DDF / ORD
''' pages use when «ActiveX» is chosen) loading the PDF the operator picks -- no copy, no signing session, no server, no
''' Save As trap, no settings preface. What is seen here is the control alone. Every load writes its watch lines to
''' <c>Logs\activex_check.log</c>; «Jurnalul ActiveX…» opens them in <see cref="ActivexLogViewerForm"/>.
''' </summary>
Public NotInheritable Class ActiveXPdfHarnessForm

    Private ReadOnly _viewer As AcroPdfViewer

    Public Sub New()
        InitializeComponent()
        _viewer = New AcroPdfViewer(pnlHost, AddressOf AdobeHostLog.Write) With {
            .PrimerPath = Path.Combine(AppContext.BaseDirectory, "empty_pdf.pdf")}
        If Not _viewer.IsAvailable Then lblFile.Text = "Controlul Adobe (AcroPDF) nu este înregistrat pe această mașină."
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Async Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Try
            If dlgOpen.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_path As String = dlgOpen.FileName
            _viewer.LoadThroughSrc = chkSrc.Checked
            lblFile.Text = "Se încarcă «" & Path.GetFileName(k_path) & "»" & If(chkSrc.Checked, " prin src", " prin LoadFile") & "…"
            Dim k_result As AcroPdfResult = Await _viewer.ShowDocumentAsync(k_path)
            lblFile.Text = If(k_result.Succeeded, k_path, k_result.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnOpen_Click", ex)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Try
            _viewer.Clear()
            lblFile.Text = "Niciun document"
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnClose_Click", ex)
        End Try
    End Sub

    Private Sub btnSeen_Click(sender As Object, e As EventArgs) Handles btnSeen.Click
        Try
            _viewer.MarkOperatorSeen()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnSeen_Click", ex)
        End Try
    End Sub

    ' Not modal: the viewer stays open next to the bench and is reloaded by hand.
    Private Sub btnLogViewer_Click(sender As Object, e As EventArgs) Handles btnLogViewer.Click
        Try
            Dim k_form As New ActivexLogViewerForm()
            k_form.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnLogViewer_Click", ex)
        End Try
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub ShutDownBench()
        Try
            _viewer?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.ShutDownBench", ex)
        End Try
    End Sub

End Class
#End If
