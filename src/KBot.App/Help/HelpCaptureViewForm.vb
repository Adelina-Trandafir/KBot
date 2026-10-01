Option Strict On
Imports System.IO
Imports KBot.Common

''' <summary>«Vezi»: shows the picture a help capture saved, as it is on disk.</summary>
Public Class HelpCaptureViewForm

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(caption As String, path As String)
        Me.New()
        Try
            Text = "Imagine salvată: " & caption
            ' Copied into a fresh bitmap so the file stays free (GDI+ holds a file open).
            Using source As Image = Image.FromFile(path)
                picImagine.Image = New Bitmap(source)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureViewForm.New", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        If picImagine.Image IsNot Nothing Then
            picImagine.Image.Dispose()
            picImagine.Image = Nothing
        End If
    End Sub

End Class
