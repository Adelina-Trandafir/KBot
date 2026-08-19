Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' Jurnalul unei rulări: <c>&lt;AppDir&gt;\Logs\migrare_&lt;DC&gt;_&lt;stamp&gt;.log</c> și
''' <c>..._respinse.csv</c>, câte o pereche pentru fiecare DC selectat.
'''
''' Notă asupra respinselor: un rând respins e, prin definiție, unul a cărui cheie de rutare
''' NU se rezolvă în niciun DC — deci nu se poate atribui unui DC anume. Ca fiecare fișier să
''' fie complet citit singur, respinsele se scriu în CSV-ul FIECĂRUI DC selectat. La fel și
''' secțiunile globale ale jurnalului (verificarea artefactelor, construirea hărților).
'''
''' Scrierile pe disc sunt de graniță: logăm și re-aruncăm. <see cref="Line"/> e evenimentul
''' pe care formularul îl folosește pentru coada live.
''' </summary>
Public NotInheritable Class RunLog
    Implements IDisposable

    Public Event Line(text As String)

    Private ReadOnly _dcs As List(Of String)
    Private ReadOnly _logs As New Dictionary(Of String, StreamWriter)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _rejects As New Dictionary(Of String, StreamWriter)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _gate As New Object()
    Private _disposed As Boolean

    Public Sub New(dcs As IEnumerable(Of String))
        Try
            _dcs = New List(Of String)(dcs)
            Dim stamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            LogPaths.EnsureLogsDirectory()

            For Each dc As String In _dcs
                Dim safe As String = SafeName(dc)
                Dim logPath As String = LogPaths.Combine("migrare_" & safe & "_" & stamp & ".log")
                Dim rejPath As String = LogPaths.Combine("migrare_" & safe & "_" & stamp & "_respinse.csv")

                Dim lw As New StreamWriter(logPath, False, New UTF8Encoding(True)) With {.AutoFlush = True}
                _logs.Add(dc, lw)

                Dim rw As New StreamWriter(rejPath, False, New UTF8Encoding(True)) With {.AutoFlush = True}
                rw.WriteLine("tabel;cheie;motiv")
                _rejects.Add(dc, rw)
            Next

        Catch ex As Exception
            GlobalErrorLog.Write("RunLog.New", ex)
            Throw
        End Try
    End Sub

    ''' <summary>O linie care privește toată rularea — merge în jurnalul fiecărui DC.</summary>
    Public Sub Write(text As String)
        Try
            Dim stamped As String = DateTime.Now.ToString("HH:mm:ss") & "  " & text
            SyncLock _gate
                For Each w As StreamWriter In _logs.Values
                    w.WriteLine(stamped)
                Next
            End SyncLock
            RaiseEvent Line(stamped)
        Catch ex As Exception
            GlobalErrorLog.Write("RunLog.Write", ex)
            Throw
        End Try
    End Sub

    ''' <summary>O linie care privește un singur DC.</summary>
    Public Sub WriteFor(dc As String, text As String)
        Try
            Dim stamped As String = DateTime.Now.ToString("HH:mm:ss") & "  [" & dc & "] " & text
            SyncLock _gate
                Dim w As StreamWriter = Nothing
                If _logs.TryGetValue(dc, w) Then w.WriteLine(stamped)
            End SyncLock
            RaiseEvent Line(stamped)
        Catch ex As Exception
            GlobalErrorLog.Write("RunLog.WriteFor", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Un rând respins: tabel, cheie primară, motiv. Nu se pierde nimic tăcut.</summary>
    Public Sub Reject(table As String, primaryKey As String, reason As String)
        Try
            Dim line As String = Csv(table) & ";" & Csv(primaryKey) & ";" & Csv(reason)
            SyncLock _gate
                For Each w As StreamWriter In _rejects.Values
                    w.WriteLine(line)
                Next
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("RunLog.Reject", ex)
            Throw
        End Try
    End Sub

    Private Shared Function Csv(value As String) As String
        Dim s As String = If(value, "")
        If s.IndexOfAny(New Char() {";"c, """"c, ControlChars.Cr, ControlChars.Lf}) >= 0 Then
            Return """" & s.Replace("""", """""") & """"
        End If
        Return s
    End Function

    ''' <summary>Un DC are forma 045_CTER, dar numele de fișier nu se bazează pe asta.</summary>
    Private Shared Function SafeName(dc As String) As String
        Dim sb As New StringBuilder()
        For Each c As Char In If(dc, "")
            If Char.IsLetterOrDigit(c) OrElse c = "_"c OrElse c = "-"c Then sb.Append(c) Else sb.Append("_"c)
        Next
        If sb.Length = 0 Then sb.Append("necunoscut")
        Return sb.ToString()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        Try
            SyncLock _gate
                For Each w As StreamWriter In _logs.Values
                    w.Dispose()
                Next
                For Each w As StreamWriter In _rejects.Values
                    w.Dispose()
                Next
                _logs.Clear()
                _rejects.Clear()
            End SyncLock
        Catch ex As Exception
            ' Dispose: nu re-aruncăm dintr-o eliberare de resurse — ar masca eroarea originală
            ' a blocului Using care ne-a chemat.
            GlobalErrorLog.Write("RunLog.Dispose", ex)
        End Try
    End Sub

End Class
