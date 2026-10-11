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

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The list is the server's; only the "new copy" button depends on the chosen database.
    End Sub

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("BackupsView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citește lista copiilor...")
                Dim k_result As BackupsResult = Await _context.Client.CallAsync(Of BackupsResult)("backups")
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_file As BackupFile In k_result.Files
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row("name") = k_file.Name
                        k_row("kind") = k_file.Kind
                        k_row("size") = UiRun.FormatBytes(k_file.Size)
                        k_row("modified") = k_file.Modified.Replace("T", " ")
                        k_row("path") = k_file.Path
                    Next
                Finally
                    grid.EndUpdate()
                End Try
                lblState.Text = k_result.Files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere."
                _context.Say("Gata.")
            End Function)
    End Function

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
