Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Reflection
Imports KBot.Common

''' <summary>
''' The computer an upload comes from (slice 0079), sent with every signature record as
''' <c>X-Statie</c>. The PUBLIC address and the logged-in account are NOT here: the server reads
''' them itself (request address, bearer session), so a client cannot put someone else's there.
''' Property names are the JSON keys (ASCII), shared with PYTHON/routes/forexe/pdf.py.
''' </summary>
Public NotInheritable Class StationInfo
    ''' <summary>This computer's IPv4 addresses on the local network, comma separated.</summary>
    Public Property ip_local As String = String.Empty
    Public Property calculator As String = String.Empty
    Public Property utilizator_windows As String = String.Empty
    Public Property sistem As String = String.Empty
    ''' <summary>K-BOT's FileVersion.</summary>
    Public Property versiune As String = String.Empty

    ''' <summary>
    ''' Read now. Never throws: a detail that cannot be read stays empty -- the signature record is
    ''' worth more than a complete description of the computer.
    ''' </summary>
    Public Shared Function Current() As StationInfo
        Dim info As New StationInfo()
        Try
            info.calculator = Environment.MachineName
            info.utilizator_windows = Environment.UserDomainName & "\" & Environment.UserName
            info.sistem = Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()
            Dim entry As Assembly = If(Assembly.GetEntryAssembly(), GetType(StationInfo).Assembly)
            info.versiune = If(FileVersionInfo.GetVersionInfo(entry.Location).FileVersion, String.Empty)
            info.ip_local = String.Join(", ", LocalAddresses())
        Catch ex As Exception
            GlobalErrorLog.Write("StationInfo.Current", ex)
        End Try
        Return info
    End Function

    ' Up, not loopback / tunnel, IPv4 only (what the operator's IT people recognise).
    Private Shared Function LocalAddresses() As List(Of String)
        Dim found As New List(Of String)()
        For Each nic As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
            If nic.OperationalStatus <> OperationalStatus.Up Then Continue For
            If nic.NetworkInterfaceType = NetworkInterfaceType.Loopback OrElse
               nic.NetworkInterfaceType = NetworkInterfaceType.Tunnel Then Continue For
            For Each a As UnicastIPAddressInformation In nic.GetIPProperties().UnicastAddresses
                If a.Address.AddressFamily <> AddressFamily.InterNetwork Then Continue For
                Dim text As String = a.Address.ToString()
                If Not found.Contains(text) Then found.Add(text)
            Next
        Next
        Return found
    End Function
End Class
