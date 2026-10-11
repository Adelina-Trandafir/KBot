Option Strict On
Imports System.Globalization

''' <summary>The dump files and schema plans on the server, and a new dump of the chosen database (slice ME01).</summary>
Public Class BackupsView
    Implements IAdminView

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
                        k_row("modified") = k_file.Modified.Replace("T", " ")
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

    Private Async Sub BtnMake_Click(k_sender As Object, e As EventArgs) Handles btnMake.Click
        Try
            Dim k_db As String = _context.RequireSingleDb("Copia de siguranță")
            If k_db = String.Empty Then Return
            If Not UiRun.Confirm("Fac acum o copie a bazei " & k_db & " pe server?" & vbLf & "Poate dura câteva minute pe o bază mare.", "Copie de siguranță") Then Return
            Await UiRun.Guard("BackupsView.BtnMake_Click", _context,
                Async Function()
                    _context.Say("Se face copia bazei " & k_db & "...")
                    Dim k_made As BackupMade = Await _context.Client.CallAsync(Of BackupMade)("backup", "--db", k_db)
                    _context.Say("Copie făcută: " & k_made.Path & " (" & UiRun.FormatBytes(k_made.Size) & ")")
                End Function)
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("BackupsView.BtnMake_Click", ex)
        End Try
    End Sub

End Class
