Option Strict On
Imports System.Globalization

''' <summary>Visits to the public site (table Vizite_Site), by day range; bots hidden unless asked (slice ME01).</summary>
Public Class VisitsView
    Implements IAdminView

    Private ReadOnly _daysChoices As Integer() = {1, 7, 30, 90}
    Private _context As AdminContext

    Public Sub New()
        InitializeComponent()
        For Each k_days As Integer In _daysChoices
            cmbDays.Items.Add(k_days.ToString(CultureInfo.InvariantCulture) & If(k_days = 1, " zi", " zile"))
        Next
        cmbDays.SelectedIndex = 1
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadAsync()
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' Visits do not belong to a database.
    End Sub

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("VisitsView.LoadAsync", _context,
            Async Function()
                Dim k_days As Integer = _daysChoices(Math.Max(0, cmbDays.SelectedIndex))
                _context.Say("Se citesc vizitele...")
                Dim k_args As New List(Of String) From {"--days", k_days.ToString(CultureInfo.InvariantCulture)}
                If chkBots.Checked Then k_args.Add("--bots")
                Dim k_result As VisitsResult = Await _context.Client.CallAsync(Of VisitsResult)("visits", k_args.ToArray())
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_visit As VisitRow In k_result.Rows
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row("first") = k_visit.First.Replace("T", " ")
                        k_row("seconds") = k_visit.Seconds.ToString(CultureInfo.InvariantCulture)
                        k_row("ip") = k_visit.Ip
                        k_row("country") = k_visit.Country
                        k_row("kind") = If(k_visit.Bot, "robot", If(k_visit.Mobile, "mobil", "calculator"))
                        k_row("page") = k_visit.Page
                        k_row("referrer") = k_visit.Referrer
                        k_row("sections") = k_visit.Sections
                    Next
                Finally
                    grid.EndUpdate()
                End Try
                lblState.Text = k_result.Rows.Count.ToString(CultureInfo.InvariantCulture) & " vizite în ultimele " & k_days.ToString(CultureInfo.InvariantCulture) & " zile."
                _context.Say("Gata.")
            End Function)
    End Function

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("VisitsView.BtnRefresh_Click", ex)
        End Try
    End Sub

End Class
