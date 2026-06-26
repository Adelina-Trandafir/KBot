Imports System.Drawing
Imports System.Windows.Forms
Imports GeneralClasses
Imports WorkflowModels

Public Class ThrottleCustomDialog
    Inherits Form

    Private _result As ThrottleSettings = Nothing

    Public ReadOnly Property Result As ThrottleSettings
        Get
            Return _result
        End Get
    End Property

    Public Sub New(Optional existing As ThrottleSettings = Nothing)
        InitializeComponent()

        If existing IsNot Nothing AndAlso existing.Enabled Then
            nudDownload.Value = CDec(Math.Max(0, existing.DownloadThroughput / 1000))
            nudUpload.Value = CDec(Math.Max(0, existing.UploadThroughput / 1000))
            nudLatency.Value = CDec(existing.Latency)
        End If

        KBotTheme.ApplyTheme(Me)
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Dim dl As Double = CDbl(nudDownload.Value) * 1000
        Dim ul As Double = CDbl(nudUpload.Value) * 1000
        Dim lat As Double = CDbl(nudLatency.Value)

        _result = New ThrottleSettings With {
            .Enabled = True,
            .DownloadThroughput = If(dl = 0, -1, dl),
            .UploadThroughput = If(ul = 0, -1, ul),
            .Latency = lat,
            .Label = $"Custom (DL:{nudDownload.Value} KB/s, UL:{nudUpload.Value} KB/s, Lat:{nudLatency.Value}ms)"
        }

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

End Class