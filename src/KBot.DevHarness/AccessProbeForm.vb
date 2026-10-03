Option Strict On
Imports System.Data
Imports System.Globalization
Imports System.Text
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' Slice 0104-02 (Debug only): the smallest window that does what «Setări ▸ Access» does and nothing else -- loads
''' <c>cale.accdb</c> through the Access component and shows the rows of its table <c>cai</c> -- plus a button that
''' ends the application at once (<see cref="FastExit.TerminateNow"/>). It exists to reproduce, in isolation, the
''' crash Office's Access driver causes when a process that used it exits (see <see cref="FastExit"/>).
'''
''' <para>The year and the source narrow the rows like the selection in K-BOT does; both empty = every row. Closing the
''' window the usual way leaves through <c>Program.Main</c>, which ends the process the same way when the driver was
''' loaded; «Închide direct» does it without leaving the window first.</para>
''' </summary>
Public NotInheritable Class AccessProbeForm

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub AccessProbeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            txtCale.Text = AppSettings.Current.AccessRegistryPath
            lblStare.Text = "Alege anul / sursa (sau lasă goale pentru toate rândurile) și apasă «Încarcă și citește»."
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("AccessProbeForm.AccessProbeForm_Load", ex)
        End Try
    End Sub

    ' UI boundary (async void handler): log, say it on the form, swallow.
    Private Async Sub BtnCiteste_Click(sender As Object, e As EventArgs) Handles btnCiteste.Click
        Try
            btnCiteste.Enabled = False
            Dim bridge As IAccessBridge = AccessBridge.Instance
            If bridge Is Nothing Then
                lblStare.Text = "Componenta Access (KBot.Access.dll) nu este lângă aplicație."
                Return
            End If
            Dim cale As String = txtCale.Text.Trim()
            Dim an As Integer = 0
            If txtAn.Text.Trim().Length > 0 AndAlso
               Not Integer.TryParse(txtAn.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, an) Then
                lblStare.Text = "Anul trebuie să fie un număr (sau gol)."
                Return
            End If
            Dim sursa As String = txtSursa.Text.Trim()

            lblStare.Text = "Se citește…"
            Dim tabel As DataTable = Await Task.Run(Function() bridge.ReadRegistry(cale, an, sursa)).ConfigureAwait(True)
            If IsDisposed Then Return
            txtRezultat.Text = Render(tabel)
            lblStare.Text = $"{tabel.Rows.Count} rânduri. Driverul Office este încărcat în proces: " &
                            If(FastExit.OfficeDriverLoaded(), "da", "nu") & "."
        Catch ex As Exception
            GlobalErrorLog.Write("AccessProbeForm.BtnCiteste_Click", ex)
            If Not IsDisposed Then lblStare.Text = "Nu s-a putut citi: " & ex.Message
        Finally
            If Not IsDisposed Then btnCiteste.Enabled = True
        End Try
    End Sub

    ' The whole application ends here, now: no form is closed first, nothing after the call runs.
    Private Sub BtnInchideDirect_Click(sender As Object, e As EventArgs) Handles btnInchideDirect.Click
        Try
            FastExit.TerminateNow()
        Catch ex As Exception
            ' TerminateNow logged it; the window stays open and says so.
            lblStare.Text = "Nu s-a putut închide direct: " & ex.Message
        End Try
    End Sub

    Private Shared Function Render(tabel As DataTable) As String
        Dim sb As New StringBuilder()
        sb.AppendLine(String.Join(" | ", tabel.Columns.Cast(Of DataColumn)().Select(Function(c) c.ColumnName)))
        For Each r As DataRow In tabel.Rows
            sb.AppendLine(String.Join(" | ", r.ItemArray.Select(
                Function(v) If(v Is Nothing OrElse v Is DBNull.Value, String.Empty,
                               Convert.ToString(v, CultureInfo.InvariantCulture)))))
        Next
        Return sb.ToString()
    End Function

End Class
