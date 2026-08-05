Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text

''' <summary>
''' Jurnalul de LUCRU al gazdei Adobe: <c>&lt;AppDir&gt;\Logs\adobe_preview.log</c>.
'''
''' Separat de <see cref="KBot.Common.GlobalErrorLog"/>, care primește excepții. Aici ajunge ce s-a
''' DECIS și de ce: profilul folosit, marcajul care a decis generația, dreptunghiul cerut față de
''' cel obținut, fiecare fereastră plutitoare acceptată sau respinsă. Fără el, o previzualizare
''' stricată după un update Adobe nu are nicio urmă de citit — exact situația din care a ieșit
''' bancul de probă al feliei 0023.
'''
''' Sink terminal, ca <c>GlobalErrorLog</c>: dacă nici acest fișier nu se poate scrie, mesajul
''' pleacă pe <see cref="Trace"/> și NU se aruncă mai departe.
''' </summary>
Public Module AdobeHostLog

    Private ReadOnly _gate As New Object()

    ''' <summary>Numele fișierului, lângă executabil, sub <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "adobe_preview.log"

    ''' <summary>Scrie o linie cu marcaj de timp. Nu aruncă niciodată.</summary>
    Public Sub Write(line As String)
        Try
            Dim dir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
            Directory.CreateDirectory(dir)
            SyncLock _gate
                File.AppendAllText(Path.Combine(dir, FileNameOnly),
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & line & Environment.NewLine,
                                   New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            Trace.WriteLine("AdobeHostLog terminal failure: " & terminalEx.Message)
        End Try
    End Sub

End Module
