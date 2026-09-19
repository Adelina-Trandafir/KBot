Option Strict On
Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

''' <summary>
''' The updater's only window (slice 0067): a title, a status line, a progress bar. The work
''' runs on a background task started from <see cref="OnShown"/>; every label change hops
''' back to the UI thread through <see cref="Post"/>.
'''
''' <para><b>Plain WinForms on purpose.</b> This exe runs while the K-BOT assemblies are
''' being overwritten, so it references none of them -- see the .vbproj header.</para>
'''
''' <para>Sequence: wait for the app to exit -> verify the package -> check the folder is
''' writable (else relaunch elevated) -> apply -> restart the app -> delete the package.
''' Any failure ends in one message box naming the log, and exit code 1.</para>
''' </summary>
Friend Class UpdaterForm

    Private Const WAIT_FOR_APP_MS As Integer = 60_000

    Private ReadOnly _args As UpdaterArgs
    Private ReadOnly _log As UpdaterLog
    Private _started As Boolean

    ''' <summary>0 = applied, 1 = failed, 3 = handed over to an elevated copy.</summary>
    Public Property ExitCode As Integer = 1

    Public Sub New(args As UpdaterArgs, log As UpdaterLog)
        If args Is Nothing Then Throw New ArgumentNullException(NameOf(args))
        If log Is Nothing Then Throw New ArgumentNullException(NameOf(log))
        _args = args
        _log = log
        InitializeComponent()
        If Not String.IsNullOrWhiteSpace(_args.Version) Then
            lblTitlu.Text = "Actualizare K-BOT " & _args.Version
            Text = lblTitlu.Text
        End If
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If _started Then Return
        _started = True
        Try
            Dim ignored As Task = Task.Run(AddressOf Work)
        Catch ex As Exception
            ' UI boundary: log and finish through the same failure path as the worker.
            _log.Write("UpdaterForm.OnShown", ex)
            Fail(ex)
        End Try
    End Sub

    Private Sub Work()
        Try
            _log.Write("=== update start: zip=" & _args.ZipPath & " target=" & _args.TargetDir &
                       " wait=" & _args.WaitPid & " elevated=" & _args.Elevated & " privileged=" & Environment.IsPrivilegedProcess)

            ' 1. The app that launched us must be gone before its DLLs can be replaced.
            If _args.WaitPid > 0 Then
                Status("Aștept închiderea aplicației…", "")
                WaitForProcess(_args.WaitPid)
            End If

            ' 2. Integrity, before a single byte is written.
            If Not String.IsNullOrWhiteSpace(_args.Sha256) Then
                Status("Verific pachetul…", Path.GetFileName(_args.ZipPath))
                UpdateApplier.VerifySha256(_args.ZipPath, _args.Sha256)
                _log.Write("sha256 OK")
            End If

            ' 3. Can we write there? If not, hand over to an elevated copy of ourselves.
            Directory.CreateDirectory(_args.TargetDir)
            If Not UpdateApplier.IsWritable(_args.TargetDir) Then
                If _args.Elevated Then
                    Throw New UnauthorizedAccessException("Nu am drept de scriere în " & _args.TargetDir & " nici cu drepturi de administrator.")
                End If
                _log.Write("target not writable as invoker -> relaunching elevated")
                Status("Sunt necesare drepturi de administrator…", _args.TargetDir)
                Program.RelaunchElevated(_args)
                ExitCode = 3
                Post(Sub() Close())
                Return
            End If

            ' 4. Apply.
            Status("Instalez fișierele…", "")
            Post(Sub()
                     bara.Style = ProgressBarStyle.Continuous
                     bara.Minimum = 0
                     bara.Value = 0
                 End Sub)
            Dim result As UpdateApplier.ApplyResult = UpdateApplier.Apply(
                _args.ZipPath, _args.TargetDir,
                Sub(line) _log.Write(line),
                Sub(done, total)
                    Post(Sub()
                             If bara.Maximum <> total Then bara.Maximum = total
                             bara.Value = Math.Min(done, total)
                             lblDetaliu.Text = done & " / " & total
                         End Sub)
                End Sub,
                CancellationToken.None)
            _log.Write("applied: written=" & result.Written & " skipped=" & result.Skipped)

            ' 5. Restart the app.
            If Not String.IsNullOrWhiteSpace(_args.RestartExe) Then
                Status("Pornesc aplicația…", Path.GetFileName(_args.RestartExe))
                Program.StartApplication(_args.RestartExe)
                _log.Write("restarted " & _args.RestartExe)
            End If

            ' 6. The package is no longer needed. Best effort: a leftover zip is not a failure.
            Try
                File.Delete(_args.ZipPath)
            Catch ex As Exception
                _log.Write("package not deleted: " & ex.Message)
            End Try

            _log.Write("=== update done")
            ExitCode = 0
            Post(Sub() Close())
        Catch ex As Exception
            _log.Write("=== update FAILED", ex)
            Fail(ex)
        End Try
    End Sub

    Private Sub WaitForProcess(pid As Integer)
        Dim p As Process = Nothing
        Try
            p = Process.GetProcessById(pid)
        Catch ex As ArgumentException
            _log.Write("process " & pid & " already gone")
            Return
        End Try
        Using p
            If Not p.WaitForExit(WAIT_FOR_APP_MS) Then
                ' Not fatal by itself: the file moves retry on their own. Say it, go on.
                _log.Write("process " & pid & " still running after " & WAIT_FOR_APP_MS & " ms; continuing, the file moves will retry")
            Else
                _log.Write("process " & pid & " exited")
            End If
        End Using
    End Sub

    Private Sub Status(main As String, detail As String)
        _log.Write(main & If(String.IsNullOrEmpty(detail), "", " " & detail))
        Post(Sub()
                 lblStare.Text = main
                 lblDetaliu.Text = detail
             End Sub)
    End Sub

    Private Sub Fail(ex As Exception)
        ExitCode = 1
        Post(Sub()
                 bara.Style = ProgressBarStyle.Continuous
                 bara.Value = 0
                 lblStare.Text = "Actualizarea a eșuat."
                 lblDetaliu.Text = ex.Message
                 MessageBox.Show(Me,
                     "Actualizarea K-BOT nu s-a putut face." & Environment.NewLine & Environment.NewLine &
                     ex.Message & Environment.NewLine & Environment.NewLine &
                     "Detalii în: " & _log.Path & Environment.NewLine &
                     "Aplicația se poate reinstala oricând din pachetul de instalare.",
                     "Actualizare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
                 Close()
             End Sub)
    End Sub

    ''' <summary>Runs <paramref name="action"/> on the UI thread; a closed form swallows it.</summary>
    Private Sub Post(action As Action)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        Try
            BeginInvoke(action)
        Catch ex As InvalidOperationException
            ' The handle went away between the check and the call (form closing): nothing to show.
            _log.Write("Post skipped: " & ex.Message)
        End Try
    End Sub
End Class
