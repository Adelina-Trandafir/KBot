Option Strict On
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports KBot.Common

''' <summary>One job of a print queue, as the spooler reports it (slice 0099). POCO.</summary>
Friend NotInheritable Class SpoolerJob
    Public Property Printer As String = ""
    Public Property JobId As Integer
    ''' <summary>The name the printing application gave the job (Adobe: the file name).</summary>
    Public Property Document As String = ""
    Public Property User As String = ""
    Public Property TotalPages As Integer
End Class

''' <summary>One pass over the print queues of this computer (slice 0099). POCO.</summary>
Friend NotInheritable Class SpoolerScan
    ''' <summary>False when the printers themselves could not be listed: the pass says nothing.</summary>
    Public Property PrintersListed As Boolean
    ''' <summary>Win32 error of the failed listing (0 when listed).</summary>
    Public Property ListError As Integer
    ''' <summary>The queues that were read, by printer name. A queue that is not here was NOT read.</summary>
    Public ReadOnly Property Queues As New Dictionary(Of String, List(Of SpoolerJob))(StringComparer.OrdinalIgnoreCase)
    ''' <summary>The printers whose queue could not be read, with the Win32 error.</summary>
    Public ReadOnly Property Failed As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
End Class

''' <summary>
''' The Windows print spooler, as much of it as <see cref="AdobePrintWatcher"/> needs (slice 0099):
''' the jobs of every queue of this computer, and a wake-up when a job is added to a local queue.
'''
''' Plain winspool calls, no package: <c>System.Printing</c> would bring WPF into KBot.Controls and
''' <c>System.Management</c> is not referenced. Every pointer the spooler hands back is read with
''' <see cref="Marshal"/> out of a buffer this class allocates and frees itself.
'''
''' NEVER called on the UI thread: a queue on a print server that does not answer can hold a call
''' for seconds.
''' </summary>
Friend NotInheritable Class PrintSpooler

    Private Sub New()
    End Sub

    Private Const PRINTER_ENUM_LOCAL As UInteger = &H2UI
    Private Const PRINTER_ENUM_CONNECTIONS As UInteger = &H4UI
    Private Const ERROR_INSUFFICIENT_BUFFER As Integer = 122
    ''' <summary>A job was added to a queue.</summary>
    Private Const PRINTER_CHANGE_ADD_JOB As UInteger = &H100UI
    ' EnumJobs wants a count; a queue longer than this is read up to here.
    Private Const MaxJobs As UInteger = 512UI
    ' The buffer size can grow between the sizing call and the real one.
    Private Const SizeRetries As Integer = 3
    Private Shared ReadOnly InvalidHandle As New IntPtr(-1)

    ' Strings are read from the buffer by hand (IntPtr fields): the marshaller must not try to free
    ' pointers that live inside the buffer.
    <StructLayout(LayoutKind.Sequential)>
    Private Structure PRINTER_INFO_4
        Public pPrinterName As IntPtr
        Public pServerName As IntPtr
        Public Attributes As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure SYSTEMTIME
        Public wYear As UShort
        Public wMonth As UShort
        Public wDayOfWeek As UShort
        Public wDay As UShort
        Public wHour As UShort
        Public wMinute As UShort
        Public wSecond As UShort
        Public wMilliseconds As UShort
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure JOB_INFO_1
        Public JobId As UInteger
        Public pPrinterName As IntPtr
        Public pMachineName As IntPtr
        Public pUserName As IntPtr
        Public pDocument As IntPtr
        Public pDatatype As IntPtr
        Public pStatus As IntPtr
        Public Status As UInteger
        Public Priority As UInteger
        Public Position As UInteger
        Public TotalPages As UInteger
        Public PagesPrinted As UInteger
        Public Submitted As SYSTEMTIME
    End Structure

    <DllImport("winspool.drv", EntryPoint:="EnumPrintersW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function EnumPrinters(flags As UInteger, name As String, level As UInteger,
                                         pPrinterEnum As IntPtr, cbBuf As UInteger,
                                         ByRef pcbNeeded As UInteger, ByRef pcReturned As UInteger) As Boolean
    End Function

    <DllImport("winspool.drv", EntryPoint:="OpenPrinterW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function OpenPrinter(pPrinterName As String, ByRef phPrinter As IntPtr, pDefault As IntPtr) As Boolean
    End Function

    <DllImport("winspool.drv", SetLastError:=True)>
    Private Shared Function ClosePrinter(hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.drv", EntryPoint:="EnumJobsW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function EnumJobs(hPrinter As IntPtr, firstJob As UInteger, noJobs As UInteger, level As UInteger,
                                     pJob As IntPtr, cbBuf As UInteger,
                                     ByRef pcbNeeded As UInteger, ByRef pcReturned As UInteger) As Boolean
    End Function

    <DllImport("winspool.drv", SetLastError:=True)>
    Private Shared Function FindFirstPrinterChangeNotification(hPrinter As IntPtr, fdwFilter As UInteger,
                                                               fdwOptions As UInteger,
                                                               pPrinterNotifyOptions As IntPtr) As IntPtr
    End Function

    <DllImport("winspool.drv", SetLastError:=True)>
    Private Shared Function FindNextPrinterChangeNotification(hChange As IntPtr, ByRef pdwChange As UInteger,
                                                              pPrinterNotifyOptions As IntPtr,
                                                              ppPrinterNotifyInfo As IntPtr) As Boolean
    End Function

    <DllImport("winspool.drv", SetLastError:=True)>
    Private Shared Function FindClosePrinterChangeNotification(hChange As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' Reads every queue of this computer: its own printers and the network printers the user is
    ''' connected to. A queue that cannot be read is reported in <see cref="SpoolerScan.Failed"/>
    ''' and the pass goes on. Boundary (interop): log and rethrow.
    ''' </summary>
    Public Shared Function Scan() As SpoolerScan
        Try
            Dim result As New SpoolerScan()
            Dim listError As Integer
            Dim printers As List(Of String) = PrinterNames(listError)
            If printers Is Nothing Then
                result.ListError = listError
                Return result
            End If
            result.PrintersListed = True
            For Each printer As String In printers
                Dim jobsError As Integer
                Dim jobs As List(Of SpoolerJob) = JobsOf(printer, jobsError)
                If jobs Is Nothing Then
                    result.Failed(printer) = jobsError
                Else
                    result.Queues(printer) = jobs
                End If
            Next
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("PrintSpooler.Scan", ex)
            Throw
        End Try
    End Function

    ' Level 4 = names only, read from the registry: no printer and no print server is contacted.
    ' Nothing (and the Win32 error) when the list cannot be read. Reached from Scan (wrapped).
    Private Shared Function PrinterNames(ByRef lastError As Integer) As List(Of String)
        lastError = 0
        Dim flags As UInteger = PRINTER_ENUM_LOCAL Or PRINTER_ENUM_CONNECTIONS
        For attempt As Integer = 1 To SizeRetries
            Dim needed As UInteger = 0
            Dim returned As UInteger = 0
            If EnumPrinters(flags, Nothing, 4UI, IntPtr.Zero, 0UI, needed, returned) Then Return New List(Of String)()
            lastError = Marshal.GetLastWin32Error()
            If lastError <> ERROR_INSUFFICIENT_BUFFER OrElse needed = 0UI Then Return Nothing

            Dim buffer As IntPtr = Marshal.AllocHGlobal(CInt(needed))
            Try
                If EnumPrinters(flags, Nothing, 4UI, buffer, needed, needed, returned) Then
                    lastError = 0
                    Dim names As New List(Of String)()
                    Dim size As Integer = Marshal.SizeOf(Of PRINTER_INFO_4)()
                    For i As Integer = 0 To CInt(returned) - 1
                        Dim info As PRINTER_INFO_4 = Marshal.PtrToStructure(Of PRINTER_INFO_4)(IntPtr.Add(buffer, i * size))
                        Dim name As String = Marshal.PtrToStringUni(info.pPrinterName)
                        If Not String.IsNullOrEmpty(name) Then names.Add(name)
                    Next
                    Return names
                End If
                lastError = Marshal.GetLastWin32Error()
                If lastError <> ERROR_INSUFFICIENT_BUFFER Then Return Nothing
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try
        Next
        Return Nothing
    End Function

    ' The jobs waiting in one queue. Nothing (and the Win32 error) when it cannot be read.
    ' Reached from Scan (wrapped).
    Private Shared Function JobsOf(printer As String, ByRef lastError As Integer) As List(Of SpoolerJob)
        lastError = 0
        Dim handle As IntPtr = IntPtr.Zero
        If Not OpenPrinter(printer, handle, IntPtr.Zero) OrElse handle = IntPtr.Zero Then
            lastError = Marshal.GetLastWin32Error()
            Return Nothing
        End If
        Try
            For attempt As Integer = 1 To SizeRetries
                Dim needed As UInteger = 0
                Dim returned As UInteger = 0
                If EnumJobs(handle, 0UI, MaxJobs, 1UI, IntPtr.Zero, 0UI, needed, returned) Then Return New List(Of SpoolerJob)()
                lastError = Marshal.GetLastWin32Error()
                If lastError <> ERROR_INSUFFICIENT_BUFFER OrElse needed = 0UI Then Return Nothing

                Dim buffer As IntPtr = Marshal.AllocHGlobal(CInt(needed))
                Try
                    If EnumJobs(handle, 0UI, MaxJobs, 1UI, buffer, needed, needed, returned) Then
                        lastError = 0
                        Dim jobs As New List(Of SpoolerJob)()
                        Dim size As Integer = Marshal.SizeOf(Of JOB_INFO_1)()
                        For i As Integer = 0 To CInt(returned) - 1
                            Dim info As JOB_INFO_1 = Marshal.PtrToStructure(Of JOB_INFO_1)(IntPtr.Add(buffer, i * size))
                            jobs.Add(New SpoolerJob With {
                                .Printer = printer,
                                .JobId = CInt(info.JobId And &H7FFFFFFFUI),
                                .Document = If(Marshal.PtrToStringUni(info.pDocument), ""),
                                .User = If(Marshal.PtrToStringUni(info.pUserName), ""),
                                .TotalPages = CInt(info.TotalPages And &H7FFFFFFFUI)})
                        Next
                        Return jobs
                    End If
                    lastError = Marshal.GetLastWin32Error()
                    If lastError <> ERROR_INSUFFICIENT_BUFFER Then Return Nothing
                Finally
                    Marshal.FreeHGlobal(buffer)
                End Try
            Next
            Return Nothing
        Finally
            ClosePrinter(handle)
        End Try
    End Function

    ''' <summary>
    ''' Asks the spooler of THIS computer to signal when a job is added to one of its queues.
    ''' Returns the waitable handle (IntPtr.Zero when refused, with the Win32 error) and, in
    ''' <paramref name="server"/>, the print server handle that must stay open as long as the
    ''' notification is used. Both are given back to <see cref="CloseAddJobNotification"/>.
    ''' Boundary (interop): log and rethrow.
    ''' </summary>
    Public Shared Function OpenAddJobNotification(ByRef server As IntPtr, ByRef lastError As Integer) As IntPtr
        Try
            server = IntPtr.Zero
            lastError = 0
            ' A null name = the local print server: one notification for all its queues.
            If Not OpenPrinter(Nothing, server, IntPtr.Zero) OrElse server = IntPtr.Zero Then
                lastError = Marshal.GetLastWin32Error()
                server = IntPtr.Zero
                Return IntPtr.Zero
            End If
            Dim change As IntPtr = FindFirstPrinterChangeNotification(server, PRINTER_CHANGE_ADD_JOB, 0UI, IntPtr.Zero)
            If change = IntPtr.Zero OrElse change = InvalidHandle Then
                lastError = Marshal.GetLastWin32Error()
                ClosePrinter(server)
                server = IntPtr.Zero
                Return IntPtr.Zero
            End If
            Return change
        Catch ex As Exception
            GlobalErrorLog.Write("PrintSpooler.OpenAddJobNotification", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Takes the signalled change and arms the notification for the next one. False (and the Win32
    ''' error) when the notification is no longer usable. Boundary (interop): log and rethrow.
    ''' </summary>
    Public Shared Function AcknowledgeChange(change As IntPtr, ByRef lastError As Integer) As Boolean
        Try
            lastError = 0
            Dim what As UInteger = 0
            If FindNextPrinterChangeNotification(change, what, IntPtr.Zero, IntPtr.Zero) Then Return True
            lastError = Marshal.GetLastWin32Error()
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("PrintSpooler.AcknowledgeChange", ex)
            Throw
        End Try
    End Function

    ''' <summary>Closes what <see cref="OpenAddJobNotification"/> opened. Safe with zero handles.</summary>
    Public Shared Sub CloseAddJobNotification(change As IntPtr, server As IntPtr)
        Try
            If change <> IntPtr.Zero AndAlso change <> InvalidHandle Then FindClosePrinterChangeNotification(change)
            If server <> IntPtr.Zero Then ClosePrinter(server)
        Catch ex As Exception
            GlobalErrorLog.Write("PrintSpooler.CloseAddJobNotification", ex)
            Throw
        End Try
    End Sub

End Class
