Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Theming
Imports Microsoft.Extensions.DependencyInjection

''' <summary>
''' Slice 0112-04: what happens when the operator presses the «send the error» button of an error message.
''' Takes the message as the window saw it (<see cref="MessageErrorReport"/>), adds everything that helps tell
''' what happened -- the version, the machine, the session (unit, year, source), the windows open, the tail of the
''' two local logs (exceptions with their stacks, and what the operator was told) -- and sends it to the server
''' (<c>POST /api/errors/report</c>, table <c>FX_RaportErori</c>). The server adds who sent it from the session.
'''
''' <para>Installed as <see cref="KBotMessage.ErrorReporter"/> once the services exist (<see cref="Install"/>).</para>
''' </summary>
Public NotInheritable Class MessageErrorReporter

    ' The tails are what the server keeps per report; the server cuts at 500 000 characters, the client stays well under.
    Private Const ErrorLogTailBytes As Integer = 48 * 1024
    Private Const MessageLogTailBytes As Integer = 24 * 1024
    Private Const MaxOpenForms As Integer = 40

    Private ReadOnly _api As IErrorReportApi
    Private ReadOnly _session As SessionContext

    Public Sub New(k_api As IErrorReportApi, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_session)
        _api = k_api
        _session = k_session
    End Sub

    ''' <summary>Makes the «send the error» button of the message window work.</summary>
    Public Shared Sub Install(k_provider As IServiceProvider)
        Try
            Dim k_reporter As New MessageErrorReporter(k_provider.GetRequiredService(Of IErrorReportApi)(),
                                                       k_provider.GetRequiredService(Of SessionContext)())
            KBotMessage.ErrorReporter = AddressOf k_reporter.SendAsync
        Catch ex As Exception
            GlobalErrorLog.Write("MessageErrorReporter.Install", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Builds and sends the report. Faults (with a message the operator can read) when it could not be sent.</summary>
    Public Async Function SendAsync(k_report As MessageErrorReport) As Task
        Try
            ArgumentNullException.ThrowIfNull(k_report)
            If Not _session.IsAuthenticated Then
                Throw New InvalidOperationException("Trimiterea cere să fiți conectat la K-BOT. Conectați-vă și încercați din nou.")
            End If
            ' The windows are read here, on the interface thread; the files are read on a worker.
            Dim k_request As ErrorReportRequest = BuildRequest(k_report)
            Await Task.Run(Sub()
                               k_request.error_log_tail = SafeTail(GlobalErrorLog.FileNameOnly, ErrorLogTailBytes)
                               k_request.message_log_tail = SafeTail(OperatorLog.FileNameOnly, MessageLogTailBytes)
                           End Sub)
            Await _api.SendErrorReportAsync(k_request, CancellationToken.None)
        Catch ex As Exception
            GlobalErrorLog.Write("MessageErrorReporter.SendAsync", ex)
            Throw
        End Try
    End Function

    Private Function BuildRequest(k_report As MessageErrorReport) As ErrorReportRequest
        Dim k_active As Form = Form.ActiveForm
        Dim k_request As New ErrorReportRequest() With {
            .rid = k_report.Rid,
            .shown_utc = k_report.ShownUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            .source = k_report.Source,
            .source_line = k_report.SourceLine,
            .caption = k_report.Caption,
            .header = k_report.Header,
            .text = k_report.Text,
            .buttons = k_report.Buttons,
            .owner_form = k_report.OwnerForm,
            .unit_name = _session.NumeUnitate,
            .cf = _session.CF,
            .year = _session.An,
            .ss = _session.SectorSursa,
            .program = _session.CodProgram,
            .role = _session.Role,
            .app_version = AppVersion(),
            .os = RuntimeInformation.OSDescription & " (" & RuntimeInformation.OSArchitecture.ToString() & ")",
            .runtime = RuntimeInformation.FrameworkDescription,
            .machine = Environment.MachineName,
            .windows_user = Environment.UserName,
            .culture = CultureInfo.CurrentUICulture.Name,
            .screen = ScreenInfo(k_active),
            .theme = ThemeManager.Current.Name
        }
        Using k_proc As Process = Process.GetCurrentProcess()
            k_request.memory_mb = CInt(k_proc.WorkingSet64 \ (1024 * 1024))
            k_request.uptime_min = CInt(Math.Max(0, (DateTime.Now - k_proc.StartTime).TotalMinutes))
        End Using
        k_request.active_form = If(k_active Is Nothing, String.Empty, FormLine(k_active))
        k_request.open_forms = OpenFormsText()
        Return k_request
    End Function

    Private Shared Function AppVersion() As String
        Dim k_asm As Assembly = If(Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly())
        Dim k_attr As AssemblyFileVersionAttribute = k_asm.GetCustomAttribute(Of AssemblyFileVersionAttribute)()
        Return If(k_attr Is Nothing, "?", k_attr.Version)
    End Function

    Private Shared Function ScreenInfo(k_form As Form) As String
        Dim k_screen As Screen = If(k_form Is Nothing, Screen.PrimaryScreen, Screen.FromControl(k_form))
        If k_screen Is Nothing Then Return String.Empty
        Dim k_dpi As String = If(k_form Is Nothing, String.Empty, "; dpi " & k_form.DeviceDpi.ToString(CultureInfo.InvariantCulture))
        Return k_screen.Bounds.Width.ToString(CultureInfo.InvariantCulture) & "x" & k_screen.Bounds.Height.ToString(CultureInfo.InvariantCulture) & k_dpi &
               "; monitoare " & Screen.AllScreens.Length.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Shared Function FormLine(k_form As Form) As String
        Return k_form.GetType().FullName & " | " & k_form.Text & If(k_form.Visible, "", " | ascuns") &
               If(k_form.WindowState = FormWindowState.Normal, "", " | " & k_form.WindowState.ToString())
    End Function

    Private Shared Function OpenFormsText() As String
        Dim k_sb As New StringBuilder()
        Dim k_n As Integer = 0
        For Each k_form As Form In Application.OpenForms
            If k_n >= MaxOpenForms Then
                k_sb.AppendLine("...")
                Exit For
            End If
            k_sb.AppendLine(FormLine(k_form))
            k_n += 1
        Next
        Return k_sb.ToString()
    End Function

    ' A log that cannot be read never stops the report: the report says so in its place.
    Private Shared Function SafeTail(k_fileName As String, k_bytes As Integer) As String
        Try
            Return TailOf(LogPaths.Combine(k_fileName), k_bytes)
        Catch ex As Exception
            GlobalErrorLog.Write("MessageErrorReporter.SafeTail", ex)
            Return "(jurnalul " & k_fileName & " nu a putut fi citit: " & ex.GetType().Name & ")"
        End Try
    End Function

    ''' <summary>The last <paramref name="k_bytes"/> of a text file, from the start of a whole line; empty when there is no file.</summary>
    Private Shared Function TailOf(k_path As String, k_bytes As Integer) As String
        If Not File.Exists(k_path) Then Return String.Empty
        Using k_fs As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim k_start As Long = Math.Max(0L, k_fs.Length - k_bytes)
            k_fs.Seek(k_start, SeekOrigin.Begin)
            Dim k_buf(CInt(k_fs.Length - k_start) - 1) As Byte
            Dim k_read As Integer = 0
            While k_read < k_buf.Length
                Dim k_got As Integer = k_fs.Read(k_buf, k_read, k_buf.Length - k_read)
                If k_got <= 0 Then Exit While
                k_read += k_got
            End While
            Dim k_text As String = Encoding.UTF8.GetString(k_buf, 0, k_read)
            If k_start > 0 Then
                Dim k_nl As Integer = k_text.IndexOf(ControlChars.Lf)
                If k_nl >= 0 Then k_text = k_text.Substring(k_nl + 1)
            End If
            Return k_text
        End Using
    End Function

End Class
