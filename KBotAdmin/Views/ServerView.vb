Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>The server's own state, as a report (slice ME01): disk, memory, load, MariaDB, processes, certificates, logs.</summary>
Public Class ServerView
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
        ' The server is one for all databases.
    End Sub

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("ServerView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citește starea serverului...")
                Dim k_status As StatusInfo = Await _context.Client.CallAsync(Of StatusInfo)("status")
                txtReport.Text = BuildReport(k_status)
                lblState.Text = "Citit la " & k_status.Time.Replace("T", " ") & " (ora serverului)."
                _context.Say("Gata.")
            End Function)
    End Function

    Private Shared Function BuildReport(k_s As StatusInfo) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine("== SERVER ==")
        If k_s.UptimeS.HasValue Then k_sb.AppendLine("Pornit de:       " & FormatSpan(k_s.UptimeS.Value))
        If k_s.Load.Count >= 3 Then k_sb.AppendLine("Încărcare 1/5/15 min: " & String.Join("  ", k_s.Load))
        If k_s.MemTotal.HasValue AndAlso k_s.MemFree.HasValue Then
            Dim k_used As Long = k_s.MemTotal.Value - k_s.MemFree.Value
            k_sb.AppendLine("Memorie:        " & UiRun.FormatBytes(k_used) & " folosită din " & UiRun.FormatBytes(k_s.MemTotal.Value) &
                            " (" & Percent(k_used, k_s.MemTotal.Value) & ")")
        End If
        If k_s.SwapTotal.HasValue AndAlso k_s.SwapTotal.Value > 0 AndAlso k_s.SwapFree.HasValue Then
            Dim k_swap As Long = k_s.SwapTotal.Value - k_s.SwapFree.Value
            k_sb.AppendLine("Swap:           " & UiRun.FormatBytes(k_swap) & " din " & UiRun.FormatBytes(k_s.SwapTotal.Value))
        End If

        k_sb.AppendLine()
        k_sb.AppendLine("== DISCURI ==")
        For Each k_d As DiskInfo In k_s.Disks
            k_sb.AppendLine(k_d.Label.PadRight(10) & UiRun.FormatBytes(k_d.Used) & " din " & UiRun.FormatBytes(k_d.Total) &
                            " (" & Percent(k_d.Used, k_d.Total) & "), liber " & UiRun.FormatBytes(k_d.Free) & "   " & k_d.Path)
        Next

        k_sb.AppendLine()
        k_sb.AppendLine("== MARIADB ==")
        For Each k_pair As KeyValuePair(Of String, String) In k_s.Mariadb
            k_sb.AppendLine(k_pair.Key.PadRight(22) & k_pair.Value)
        Next

        k_sb.AppendLine()
        k_sb.AppendLine("== SESIUNI ==")
        k_sb.AppendLine(k_s.Redis)

        k_sb.AppendLine()
        k_sb.AppendLine("== PROCESE (gunicorn, redis, mariadb) ==")
        If k_s.Processes.Count = 0 Then k_sb.AppendLine("(niciunul găsit)")
        For Each k_p As String In k_s.Processes
            k_sb.AppendLine(k_p)
        Next

        k_sb.AppendLine()
        k_sb.AppendLine("== CERTIFICATE ==")
        If k_s.Certificates.Count = 0 Then k_sb.AppendLine("(niciun certificat Let's Encrypt găsit)")
        For Each k_c As CertInfo In k_s.Certificates
            k_sb.AppendLine(k_c.Name.PadRight(30) & "expiră " & k_c.Until &
                            If(k_c.Days.HasValue, "  (în " & k_c.Days.Value.ToString(CultureInfo.InvariantCulture) & " zile)", String.Empty))
        Next

        k_sb.AppendLine()
        k_sb.AppendLine("== FIȘIERE DE JURNAL ==")
        For Each k_f As LogFileInfo In k_s.Logs
            k_sb.AppendLine(k_f.Label.PadRight(22) & If(k_f.Exists, UiRun.FormatBytes(k_f.Size).PadLeft(10) & "   " &
                            If(k_f.Modified, String.Empty).Replace("T", " ") & "   copii rotite: " & k_f.Rotated.ToString(CultureInfo.InvariantCulture),
                            "(nu există)"))
        Next
        Return k_sb.ToString()
    End Function

    Private Shared Function Percent(k_part As Long, k_whole As Long) As String
        If k_whole <= 0 Then Return "-"
        Return (100.0 * k_part / k_whole).ToString("0", CultureInfo.InvariantCulture) & "%"
    End Function

    Private Shared Function FormatSpan(k_seconds As Double) As String
        Dim k_span As TimeSpan = TimeSpan.FromSeconds(k_seconds)
        Return CInt(Math.Floor(k_span.TotalDays)).ToString(CultureInfo.InvariantCulture) & " zile, " &
               k_span.Hours.ToString(CultureInfo.InvariantCulture) & " h, " & k_span.Minutes.ToString(CultureInfo.InvariantCulture) & " min"
    End Function

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("ServerView.BtnRefresh_Click", ex)
        End Try
    End Sub

End Class
