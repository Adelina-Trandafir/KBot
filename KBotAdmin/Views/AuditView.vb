Option Strict On
Imports System.Globalization

''' <summary>
''' The activity journal (table Jurnal): sign-ins, failures, refusals and what KBotAdmin itself changed.
''' Filtered by action group, period, account text and the database chosen in the tree (slice ME01).
''' </summary>
Public Class AuditView
    Implements IAdminView

    ' Group name -> the actions it lists ("" = no filter).
    Private ReadOnly _actionGroups As New List(Of KeyValuePair(Of String, String)) From {
        New KeyValuePair(Of String, String)("Toate acțiunile", String.Empty),
        New KeyValuePair(Of String, String)("Autentificări (sesiuni)", "LOGIN,PORTAL_LOGIN,PORTAL_LOGOUT,PORTAL_UNIT"),
        New KeyValuePair(Of String, String)("Eșecuri și refuzuri", "PORTAL_AUTH_FAIL,PORTAL_DENIED"),
        New KeyValuePair(Of String, String)("Făcute de KBotAdmin", "ADMIN_GRANT,ADMIN_REVOKE,ADMIN_ACCT_ADD,ADMIN_ACCT_ROLE,ADMIN_ACCT_REMOVE,ADMIN_ROLE_ENSURE")
    }
    Private ReadOnly _hourChoices As Integer() = {24, 72, 168, 720}
    Private ReadOnly _hourCaptions As String() = {"Ultimele 24 h", "Ultimele 3 zile", "Ultimele 7 zile", "Ultimele 30 zile"}
    Private _context As AdminContext

    Public Sub New()
        InitializeComponent()
        For Each k_group As KeyValuePair(Of String, String) In _actionGroups
            cmbAction.Items.Add(k_group.Key)
        Next
        cmbAction.SelectedIndex = 0
        For Each k_caption As String In _hourCaptions
            cmbHours.Items.Add(k_caption)
        Next
        cmbHours.SelectedIndex = 1
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadAsync()
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        BeginLoad()
    End Sub

    Private Async Sub BeginLoad()
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AuditView.BeginLoad", ex)
        End Try
    End Sub

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("AuditView.LoadAsync", _context,
            Async Function()
                Dim k_args As New List(Of String) From {
                    "--hours", _hourChoices(Math.Max(0, cmbHours.SelectedIndex)).ToString(CultureInfo.InvariantCulture),
                    "--limit", "1500"
                }
                If Not _context.IsAll Then k_args.AddRange({"--db", _context.SelectedDb})
                If txtUser.Text.Trim() <> String.Empty Then k_args.AddRange({"--un", txtUser.Text.Trim()})
                Dim k_actions As String = _actionGroups(Math.Max(0, cmbAction.SelectedIndex)).Value
                If k_actions <> String.Empty Then k_args.AddRange({"--actions", k_actions})
                _context.Say("Se citește jurnalul de activitate...")
                Dim k_result As AuditResult = Await _context.Client.CallAsync(Of AuditResult)("audit", k_args.ToArray())
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_item As AuditRow In k_result.Rows
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row("moment") = k_item.Moment.Replace("T", " ")
                        k_row("un") = k_item.Un
                        k_row("dc") = k_item.Dc
                        k_row("action") = k_item.Action
                        k_row("target") = k_item.Target
                        k_row("result") = k_item.Result
                        k_row("ip") = k_item.Ip
                        k_row("details") = k_item.Details
                    Next
                Finally
                    grid.EndUpdate()
                End Try
                lblState.Text = k_result.Rows.Count.ToString(CultureInfo.InvariantCulture) & " intrări" &
                                If(_context.IsAll, " (toate bazele).", " pentru " & _context.SelectedDb & ".")
                _context.Say("Gata.")
            End Function)
    End Function

    Private Sub Grid_RowFormatting(k_sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Try
            Dim k_action As String = Convert.ToString(e.Row("action"), CultureInfo.InvariantCulture)
            If k_action.Contains("FAIL", StringComparison.Ordinal) OrElse k_action.Contains("DENIED", StringComparison.Ordinal) Then
                e.ForeColor = ThemeManager.Current.Palette.ErrorColor
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AuditView.Grid_RowFormatting", ex)
        End Try
    End Sub

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AuditView.BtnRefresh_Click", ex)
        End Try
    End Sub

End Class
