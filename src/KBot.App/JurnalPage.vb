Option Strict On
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' «Jurnal activitate» as a macheta of its own (it used to be a page of the settings window): the log
''' viewer of <see cref="SetariJurnalView"/> -- the application's logs and, with a server, the server's --
''' in a page of the main window. The view is the same; it needs nothing from the settings window.
''' </summary>
Public Class JurnalPage

    Public Sub New(k_api As IApiClient)
        InitializeComponent()
        jurnalView.ApiClient = k_api
        AddHandler jurnalView.StatusChanged, AddressOf View_StatusChanged
    End Sub

    Private Sub JurnalPage_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            jurnalView.Activated()   ' the first activation loads the list
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("JurnalPage.JurnalPage_Load", ex)
        End Try
    End Sub

    Private Sub View_StatusChanged(k_text As String)
        lblStatus.Text = If(k_text, String.Empty)
    End Sub

    Private Sub JurnalPage_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If e.CloseReason = CloseReason.UserClosing AndAlso Not jurnalView.CanClose() Then e.Cancel = True
        Catch ex As Exception
            GlobalErrorLog.Write("JurnalPage.JurnalPage_FormClosing", ex)
        End Try
    End Sub
End Class
