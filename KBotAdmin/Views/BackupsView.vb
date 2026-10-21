Option Strict On
Imports System.Globalization

''' <summary>The dump files and schema plans on the server, and a new dump of the chosen database (slice ME01).</summary>
Public Class BackupsView
    Implements IAdminView

    Private Const DemoDatabase As String = "000_DEMO"

    Private _context As AdminContext

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadAsync()
    End Function

    Public Async Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' A chosen database keeps only its own dumps and the plans that name it.
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.SelectionChanged", ex)
        End Try
    End Sub

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("BackupsView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citește lista copiilor...")
                Dim k_result As BackupsResult = Await _context.Client.CallAsync(Of BackupsResult)("backups", "--db", _context.DbArgument)
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_file As BackupFile In k_result.Files
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row.Tag = k_file
                        k_row("name") = k_file.Name
                        k_row("kind") = k_file.Kind
                        k_row("size") = UiRun.FormatBytes(k_file.Size)
                        k_row("modified") = LocalTime(k_file.Modified)
                        k_row("reason") = k_file.Reason
                        k_row("path") = k_file.Path
                    Next
                Finally
                    grid.EndUpdate()
                End Try
                lblState.Text = k_result.Files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere" &
                                If(_context.IsAll, ".", " pentru baza " & _context.SelectedDb & ".") & " Dublu clic pe un rând arată textul SQL."
                _context.Say("Gata.")
            End Function)
    End Function

    ''' <summary>The server writes file times as UTC without an offset (PYTHON/docs/TIMEZONES.md); the grid shows this PC's time.</summary>
    Private Shared Function LocalTime(k_utcText As String) As String
        Dim k_value As DateTime
        If DateTime.TryParse(k_utcText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal, k_value) Then
            Return k_value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        End If
        Return If(k_utcText, String.Empty).Replace("T", " ")
    End Function

    Private Async Sub Grid_CellDoubleClick(k_sender As Object, e As KBotCellEventArgs) Handles grid.CellDoubleClick
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= grid.Rows.Count Then Return
            Dim k_file As BackupFile = TryCast(grid.Rows(e.RowIndex).Tag, BackupFile)
            If k_file Is Nothing Then Return
            Dim k_text As BackupText = Nothing
            Await UiRun.Guard("BackupsView.Grid_CellDoubleClick", _context,
                Async Function()
                    _context.Say("Se citește " & k_file.Name & "...")
                    k_text = Await _context.Client.CallAsync(Of BackupText)("backup-text", "--path", k_file.Path)
                    _context.Say("Gata.")
                End Function)
            If k_text Is Nothing Then Return
            Using k_form As New SqlTextForm(k_text.Name, k_text.Size, k_text.Text, k_text.ShownBytes, k_text.Truncated)
                k_form.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.Grid_CellDoubleClick", ex)
        End Try
    End Sub

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.BtnRefresh_Click", ex)
        End Try
    End Sub

    ''' <summary>The unit databases an «all databases» copy covers: the existing ones, never 000_DEMO (the server leaves it out too).</summary>
    Private Function CopyableDatabases() As List(Of DatabaseInfo)
        Dim k_list As New List(Of DatabaseInfo)()
        For Each k_info As DatabaseInfo In _context.Databases
            If k_info.Exists AndAlso Not String.Equals(k_info.Db, DemoDatabase, StringComparison.OrdinalIgnoreCase) Then k_list.Add(k_info)
        Next
        Return k_list
    End Function

    Private Async Sub BtnMake_Click(k_sender As Object, e As EventArgs) Handles btnMake.Click
        Try
            Dim k_all As Boolean = _context.IsAll
            Dim k_target As String = _context.DbArgument
            Dim k_prompt As String
            If k_all Then
                Dim k_dbs As List(Of DatabaseInfo) = CopyableDatabases()
                If k_dbs.Count = 0 Then
                    KBotMessage.Show("Nu există nicio bază de unitate de copiat.", "Copie de siguranță", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
                Dim k_mb As Double = 0
                For Each k_info As DatabaseInfo In k_dbs
                    k_mb += k_info.Mb
                Next
                k_prompt = "Fac acum câte o copie pentru fiecare din cele " & k_dbs.Count.ToString(CultureInfo.InvariantCulture) &
                           " baze de unitate (fără " & DemoDatabase & ", fără AVACONT_COMUN și AVACONT_SURSA), în dosarul backup de pe server. Bazele au în total cam " &
                           k_mb.ToString("0.#", CultureInfo.InvariantCulture) & " MB; poate dura câteva minute. Scrieți motivul copiei, același pentru toate bazele:"
            Else
                k_prompt = "Fac acum o copie a bazei " & k_target & " în dosarul backup de pe server. Poate dura câteva minute pe o bază mare. Scrieți motivul copiei:"
            End If
            Dim k_reason As String = String.Empty
            Using k_form As New ReasonForm(k_prompt, "Copie de siguranță", "Fă copia",
                                           "Pornește copia acum. Motivul se păstrează în jurnalul serverului, lângă fiecare copie.",
                                           "Între 5 și 2000 de caractere. Apare în coloana Motiv și în Jurnal.",
                                           "Scrieți motivul copiei (cel puțin 5 caractere).")
                If k_form.ShowDialog(Me) <> DialogResult.OK Then Return
                k_reason = k_form.Reason
            End Using
            Dim k_run As BackupRun = Nothing
            Await UiRun.Guard("BackupsView.BtnMake_Click", _context,
                Async Function()
                    _context.Say(If(k_all, "Se fac copiile bazelor...", "Se face copia bazei " & k_target & "..."))
                    k_run = Await _context.Client.CallAsync(Of BackupRun)("backup", "--db", k_target, "--reason", k_reason)
                    _context.Say("Copii făcute: " & k_run.Made.ToString(CultureInfo.InvariantCulture) & ", cu eroare: " &
                                 k_run.Failed.ToString(CultureInfo.InvariantCulture) & " (" & UiRun.FormatBytes(k_run.TotalSize) & ")")
                End Function)
            Await LoadAsync()
            If k_run IsNot Nothing Then ReportRun(k_run)
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.BtnMake_Click", ex)
        End Try
    End Sub

    ''' <summary>Tells the operator what a run did when anything went wrong; a clean run is only the line in the status bar.</summary>
    Private Sub ReportRun(k_run As BackupRun)
        Try
            Dim k_lines As New List(Of String)()
            For Each k_item As BackupItem In k_run.Results
                If k_item.Error <> String.Empty Then
                    k_lines.Add(k_item.Db & ": NU s-a făcut — " & k_item.Error)
                ElseIf k_item.JournalError <> String.Empty Then
                    k_lines.Add(k_item.Db & ": " & k_item.JournalError)
                End If
            Next
            If k_lines.Count = 0 Then Return
            KBotMessage.Show("Copii făcute: " & k_run.Made.ToString(CultureInfo.InvariantCulture) & " din " &
                             k_run.Results.Count.ToString(CultureInfo.InvariantCulture) & "." & vbLf & vbLf & String.Join(vbLf, k_lines),
                             "Copie de siguranță", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.ReportRun", ex)
        End Try
    End Sub

End Class
