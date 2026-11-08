Option Strict On
Imports System.Globalization
Imports System.IO

''' <summary>A themed window for reading a Markdown file selected in the project history.</summary>
Friend NotInheritable Class MdReaderForm
    Inherits KBotThemedForm

    ' SLICE-ME01-27. The same range and mouse snap points as the VERCON online menu.
    Private Const ZoomStep As Integer = 10
    Private Shared ReadOnly ZoomStops As Integer() = {90, 100, 110, 125}
    Private Shared _lastZoom As Integer = MdReaderControl.DefaultZoom

    Private ReadOnly _root As String
    Private ReadOnly _relativePath As String
    Private WithEvents reader As New MdReaderControl With {.Dock = DockStyle.Fill, .TabIndex = 1}
    Private WithEvents btnBack As New KBotButton With {.Text = "Înapoi", .Enabled = False, .Dock = DockStyle.Fill, .TabIndex = 0}
    Private ReadOnly lblPath As New Label With {.Dock = DockStyle.Fill, .AutoEllipsis = True, .TextAlign = ContentAlignment.MiddleLeft}
    Private WithEvents btnZoomOut As New KBotButton With {.Text = "−", .AccessibleName = "Micșorează documentul", .Dock = DockStyle.Fill, .TabIndex = 1}
    Private WithEvents trkZoom As New TrackBar With {
        .AccessibleName = "Zoom document", .Dock = DockStyle.Fill, .AutoSize = False, .TickStyle = TickStyle.None,
        .Minimum = MdReaderControl.MinimumZoom, .Maximum = MdReaderControl.MaximumZoom,
        .Value = MdReaderControl.DefaultZoom, .SmallChange = 1, .LargeChange = ZoomStep, .TabIndex = 2
    }
    Private WithEvents btnZoomIn As New KBotButton With {.Text = "+", .AccessibleName = "Mărește documentul", .Dock = DockStyle.Fill, .TabIndex = 3}
    Private ReadOnly lblZoom As New Label With {.Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleCenter}

    Friend Sub New(k_root As String, k_relativePath As String)
        _root = k_root
        _relativePath = k_relativePath
        Text = Path.GetFileName(k_relativePath) & " — Cititor Markdown"
        lblPath.Text = k_relativePath
        Icon = AdminIcon.Value
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1050, 730)
        MinimumSize = New Size(540, 360)
        StartPosition = FormStartPosition.CenterParent
        ShowInTaskbar = False

        Dim k_close As New KBotButton With {.Text = "Închide", .DialogResult = DialogResult.Cancel, .Dock = DockStyle.Fill, .TabIndex = 4}
        Dim k_bar As New KBotTableLayoutPanel With {.Dock = DockStyle.Top, .Height = 46, .Padding = New Padding(8, 5, 8, 5),
            .ColumnCount = 7, .RowCount = 1, .AutoFitToTheme = False, .TabIndex = 0}
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 94F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 34F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 148F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 34F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        k_bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        k_bar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        k_bar.Controls.Add(btnBack, 0, 0)
        k_bar.Controls.Add(lblPath, 1, 0)
        k_bar.Controls.Add(btnZoomOut, 2, 0)
        k_bar.Controls.Add(trkZoom, 3, 0)
        k_bar.Controls.Add(btnZoomIn, 4, 0)
        k_bar.Controls.Add(lblZoom, 5, 0)
        k_bar.Controls.Add(k_close, 6, 0)
        SuspendLayout()
        Controls.Add(reader)
        Controls.Add(k_bar)
        CancelButton = k_close
        trkZoom.Value = _lastZoom
        reader.ZoomPercent = trkZoom.Value
        UpdateZoomLabel()
        ResumeLayout()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            reader.OpenProjectFile(_root, _relativePath)
        Catch ex As Exception
            GlobalErrorLog.Write("MdReaderForm.OnShown", ex)
            KBotMessage.Show(Me, "Nu am putut citi fișierul " & _relativePath & ": " & ex.Message, "Cititor Markdown",
                             MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Close()
        End Try
    End Sub

    Private Sub Reader_DocumentChanged(k_relativePath As String) Handles reader.DocumentChanged
        lblPath.Text = k_relativePath
        Text = Path.GetFileName(k_relativePath) & " — Cititor Markdown"
        btnBack.Enabled = reader.CanGoBack
    End Sub

    Private Sub BtnBack_Click(k_sender As Object, e As EventArgs) Handles btnBack.Click
        reader.GoBack()
    End Sub

    Private Sub BtnZoomOut_Click(k_sender As Object, e As EventArgs) Handles btnZoomOut.Click
        CommitZoom(trkZoom.Value - ZoomStep)
    End Sub

    Private Sub BtnZoomIn_Click(k_sender As Object, e As EventArgs) Handles btnZoomIn.Click
        CommitZoom(trkZoom.Value + ZoomStep)
    End Sub

    Private Sub TrkZoom_ValueChanged(k_sender As Object, e As EventArgs) Handles trkZoom.ValueChanged
        UpdateZoomLabel()
    End Sub

    Private Sub UpdateZoomLabel()
        lblZoom.Text = trkZoom.Value.ToString(CultureInfo.CurrentCulture) & "%"
        btnZoomOut.Enabled = trkZoom.Value > trkZoom.Minimum
        btnZoomIn.Enabled = trkZoom.Value < trkZoom.Maximum
    End Sub

    Private Sub TrkZoom_MouseUp(k_sender As Object, e As MouseEventArgs) Handles trkZoom.MouseUp
        If e.Button <> MouseButtons.Left OrElse trkZoom.Value = reader.ZoomPercent Then Return
        Dim k_level As Integer = trkZoom.Value
        For Each k_stop As Integer In ZoomStops
            If Math.Abs(k_stop - k_level) > 2 Then Continue For
            k_level = k_stop
            Exit For
        Next
        CommitZoom(k_level)
    End Sub

    Private Sub TrkZoom_Commit(k_sender As Object, e As EventArgs) Handles trkZoom.KeyUp, trkZoom.Leave
        CommitZoom(trkZoom.Value)
    End Sub

    Private Sub TrkZoom_MouseWheel(k_sender As Object, e As MouseEventArgs) Handles trkZoom.MouseWheel
        ' Let the native track bar update its value before reading it.
        BeginInvoke(New MethodInvoker(Sub() CommitZoom(trkZoom.Value)))
    End Sub

    Private Sub CommitZoom(k_level As Integer)
        If IsDisposed OrElse Disposing Then Return
        Try
            trkZoom.Value = Math.Clamp(k_level, trkZoom.Minimum, trkZoom.Maximum)
            reader.ZoomPercent = trkZoom.Value
            _lastZoom = trkZoom.Value
        Catch ex As Exception
            GlobalErrorLog.Write("MdReaderForm.CommitZoom", ex)
        End Try
    End Sub

End Class
