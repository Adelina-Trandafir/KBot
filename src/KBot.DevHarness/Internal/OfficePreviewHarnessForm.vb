Option Strict On
Imports System.Diagnostics
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' The bench for <see cref="OfficeDocumentHost"/> — the piece behind the DDF editor's file preview
''' that carries all of the risk: it starts a private Office instance, takes the ribbon, the formula
''' bar and the status bar down, reparents the window into a panel, and lets go of it again.
'''
''' <para><b>What to watch.</b> The counter in the toolbar. Every open must show exactly one
''' <c>EXCEL.EXE</c> or <c>WINWORD.EXE</c>, and every close must take it back to whatever it was
''' before the bench started. A count that climbs is the defect this bench exists to catch: the
''' hosted window is a child of <c>pnlHost</c>, so a process still holding one when the panel goes
''' away is left running with nothing left to reach it by.</para>
'''
''' <para><b>What is NOT here.</b> <c>DdfFisierPreview</c> and the «Fisiere» page around it live in
''' KBot.App, which references this project on Debug — referencing it back would be circular. Images
''' and PDFs are previewed there, not here; the Adobe side already has its own bench
''' (<see cref="AdobeReaderHarnessForm"/>).</para>
''' </summary>
Public NotInheritable Class OfficePreviewHarnessForm

    Private ReadOnly _log As Action(Of String)
    Private _host As OfficeDocumentHost
    ''' <summary>How many Office processes were running BEFORE the bench opened anything. The count
    ''' on screen is meaningless without it — the operator may well have Excel open already.</summary>
    Private ReadOnly _procesePornire As Integer

    Public Sub New(log As Action(Of String))
        InitializeComponent()
        _log = If(log, New Action(Of String)(Sub(s) Trace.WriteLine(s)))
        _procesePornire = NumaraProcesele()
        Scrie($"Bench pornit. Procese Office deja pornite: {_procesePornire}.")
        ActualizeazaContorul()
        tmrProcese.Start()
    End Sub

    Private Sub BtnExcel_Click(sender As Object, e As EventArgs) Handles btnExcel.Click
        Deschide(OfficeDocumentKind.Excel, "Tabele|*.xls;*.xlsx;*.xlsm;*.xlsb;*.csv|Toate fișierele|*.*")
    End Sub

    Private Sub BtnWord_Click(sender As Object, e As EventArgs) Handles btnWord.Click
        Deschide(OfficeDocumentKind.Word, "Documente|*.doc;*.docx;*.docm;*.rtf|Toate fișierele|*.*")
    End Sub

    ''' <summary>UI boundary: logs and swallows. The host itself never throws — it answers with a
    ''' Romanian sentence — so anything caught here is the bench's own fault, not Office's.</summary>
    Private Sub Deschide(fel As OfficeDocumentKind, filtru As String)
        Try
            dlgAlege.Filter = filtru
            If dlgAlege.ShowDialog(Me) <> DialogResult.OK Then Return

            ' Same order the pane uses: the previous container is CLOSED before the next opens.
            InchideGazda()

            btnExcel.Enabled = False
            btnWord.Enabled = False
            btnInchide.Enabled = False
            Cursor = Cursors.WaitCursor

            Try
                _host = New OfficeDocumentHost(pnlHost, AddressOf Scrie)
                ' Set before ShowDocument: the workbook is opened inside it, and the method is read
                ' there. Ticking the box with a document already on screen changes the next one.
                _host.ExcelRibbon = If(chkAscundeBara.Checked,
                                       ExcelRibbonMode.HideDockWindow,
                                       ExcelRibbonMode.Excel4Macro)
                Dim rezultat As OfficeHostResult = _host.ShowDocument(dlgAlege.FileName, fel)
                Scrie(If(rezultat.Succeeded,
                         $"Incorporat in {rezultat.ElapsedMs} ms.",
                         "Esuat: " & rezultat.Message))

            Finally
                Cursor = Cursors.Default
                btnExcel.Enabled = True
                btnWord.Enabled = True
                btnInchide.Enabled = True
            End Try

            ActualizeazaContorul()
        Catch ex As Exception
            GlobalErrorLog.Write("OfficePreviewHarnessForm.Deschide", ex)
            Scrie("EXCEPTIE: " & OfficeHostLog.Describe(ex))
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        InchideGazda()
        ActualizeazaContorul()
    End Sub

    ''' <summary>Disposes the host and clears the field — closed, never dropped for the collector
    ''' to notice later. Called from <c>Dispose</c> too (see the Designer).</summary>
    Friend Sub InchideGazda()
        Try
            If _host Is Nothing Then Return
            _host.Dispose()
            _host = Nothing
            Scrie("Container inchis.")
        Catch ex As Exception
            GlobalErrorLog.Write("OfficePreviewHarnessForm.InchideGazda", ex)
        End Try
    End Sub

    Private Sub TmrProcese_Tick(sender As Object, e As EventArgs) Handles tmrProcese.Tick
        ActualizeazaContorul()
    End Sub

    Private Sub ActualizeazaContorul()
        Try
            Dim acum As Integer = NumaraProcesele()
            Dim ale_noastre As Integer = acum - _procesePornire
            lblProcese.Text = $"EXCEL/WINWORD: {acum} (peste linia de pornire: {ale_noastre})"
            lblProcese.ForeColor = If(_host Is Nothing AndAlso ale_noastre > 0,
                                      ThemeManagerErrorColor(), lblProcese.ForeColor)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficePreviewHarnessForm.ActualizeazaContorul", ex)
        End Try
    End Sub

    ''' <summary>The accent the scheme uses for trouble, so a leaked process is visible without
    ''' reading the number.</summary>
    Private Shared Function ThemeManagerErrorColor() As Color
        Return KBot.Theming.ThemeManager.Current.Palette.ErrorColor
    End Function

    Private Shared Function NumaraProcesele() As Integer
        Try
            Return Process.GetProcessesByName("EXCEL").Length + Process.GetProcessesByName("WINWORD").Length
        Catch ex As Exception
            GlobalErrorLog.Write("OfficePreviewHarnessForm.NumaraProcesele", ex)
            Return -1
        End Try
    End Function

    Private Sub Scrie(linie As String)
        Try
            txtJurnal.AppendText(linie & Environment.NewLine)
            _log(linie)
        Catch ex As Exception
            GlobalErrorLog.Write("OfficePreviewHarnessForm.Scrie", ex)
        End Try
    End Sub

End Class
