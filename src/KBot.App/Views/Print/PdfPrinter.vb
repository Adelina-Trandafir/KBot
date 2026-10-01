Option Strict On
Imports System.Diagnostics
Imports System.Drawing.Printing
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls

''' <summary>The printer the operator picked, and how many copies the dialog asked for.</summary>
Friend NotInheritable Class PrinterChoice
    Public ReadOnly Property PrinterName As String
    Public ReadOnly Property Copies As Integer

    Public Sub New(printerName As String, copies As Integer)
        Me.PrinterName = printerName
        Me.Copies = Math.Max(1, copies)
    End Sub
End Class

''' <summary>
''' Sends a PDF file to a printer WITHOUT showing it (slice 0099): no viewer window of K-BOT, no
''' Adobe pane. The file goes to the program Windows has for PDF through the shell's «printto» verb
''' (Adobe Reader prints and closes on its own).
'''
''' <para>K-BOT does not draw the pages itself: there is no PDF rendering package in the solution,
''' and a hand-made print would not match what Adobe prints. What this needs from the computer is a
''' PDF program that registers «printto» -- Adobe Reader does, and K-BOT already requires it for the
''' document pane. Without one, <see cref="PrintAsync"/> throws the shell's own message.</para>
''' </summary>
Friend NotInheritable Class PdfPrinter

    ' A print that does not return in this time is reported as failed. The handler process leaves as
    ' soon as the job is in the queue, so this is generous.
    Private Const PRINT_TIMEOUT_MS As Integer = 120000

    ' The printer picked last time, offered again (the dialog starts on it).
    Private Shared _lastPrinter As String

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Asks which printer. Nothing = the operator cancelled. UI thread. UI boundary: logs and
    ''' re-throws (the caller is a click handler that reports it).
    ''' </summary>
    Public Shared Function ChoosePrinter(owner As IWin32Window) As PrinterChoice
        Try
            If PrinterSettings.InstalledPrinters.Count = 0 Then
                Throw New InvalidOperationException("Nu este instalată nicio imprimantă pe acest calculator.")
            End If

            Using dlg As New PrintDialog()
                dlg.UseEXDialog = True
                dlg.AllowSelection = False
                dlg.AllowSomePages = False
                dlg.AllowCurrentPage = False
                dlg.AllowPrintToFile = False
                Dim settings As New PrinterSettings()
                If Not String.IsNullOrEmpty(_lastPrinter) Then
                    Dim previous As New PrinterSettings() With {.PrinterName = _lastPrinter}
                    If previous.IsValid Then settings = previous
                End If
                dlg.PrinterSettings = settings
                If dlg.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
                _lastPrinter = dlg.PrinterSettings.PrinterName
                Return New PrinterChoice(dlg.PrinterSettings.PrinterName, dlg.PrinterSettings.Copies)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("PdfPrinter.ChoosePrinter", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Prints one file once on the named printer and returns when the handler is done with it.
    ''' Risky boundary (Process, shell): logs and re-throws.
    ''' </summary>
    Public Shared Async Function PrintAsync(pdfPath As String, printerName As String) As Task
        Try
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not IO.File.Exists(pdfPath) Then
                Throw New IO.FileNotFoundException("Documentul de tipărit nu există pe disc.", pdfPath)
            End If
            If String.IsNullOrWhiteSpace(printerName) Then
                Throw New ArgumentException("Printer name is empty.", NameOf(printerName))
            End If
            Await Task.Run(Sub() PrintBlocking(pdfPath, printerName)).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfPrinter.PrintAsync", ex)
            Throw
        End Try
    End Function

    ' Off the UI thread. Transitive coverage: only reached through PrintAsync.
    Private Shared Sub PrintBlocking(pdfPath As String, printerName As String)
        Dim psi As New ProcessStartInfo(pdfPath) With {
            .Verb = "printto",
            .Arguments = """" & printerName & """",
            .UseShellExecute = True,
            .CreateNoWindow = True,
            .WindowStyle = ProcessWindowStyle.Hidden
        }
        AdobeHostLog.Write($"Tipărire din lista de documente: «{IO.Path.GetFileName(pdfPath)}» pe «{printerName}».")
        Using p As Process = Process.Start(psi)
            ' Nothing = the shell handed the file to a program that was already running; there is
            ' no process of ours to wait for.
            If p Is Nothing Then Return
            If Not p.WaitForExit(PRINT_TIMEOUT_MS) Then
                Throw New TimeoutException("Programul de PDF nu a terminat trimiterea la imprimantă în timp util.")
            End If
        End Using
    End Sub

End Class
