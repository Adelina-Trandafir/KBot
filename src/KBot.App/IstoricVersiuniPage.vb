Option Strict On
Imports KBot.Common

''' <summary>
''' «Istoric versiuni» as a macheta of its own (it used to be a page of the settings window): what every
''' version of K-BOT changed, newest first -- the view is <see cref="SetariIstoricView"/>, read from the
''' <c>NOUTATI.md</c> that ships next to the program.
''' </summary>
Public Class IstoricVersiuniPage

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub IstoricVersiuniPage_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            istoricView.Activated()   ' reads the file when the page comes on screen
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("IstoricVersiuniPage.IstoricVersiuniPage_Load", ex)
        End Try
    End Sub
End Class
