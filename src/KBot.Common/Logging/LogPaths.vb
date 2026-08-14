Option Strict On
Imports System.IO

''' <summary>
''' Singurul loc care răspunde la întrebarea «unde stau jurnalele»: <c>&lt;AppDir&gt;\Logs</c>.
'''
''' Înainte de felia 0031, fiecare scriitor își compunea singur calea — <c>GlobalErrorLog</c> și
''' <c>AdobeHostLog</c> ajungeau la ACEEAȘI cale prin cod duplicat, iar <c>TreeLogger</c> scria
''' lângă executabil, în AFARA folderului <c>Logs\</c>. Vizualizatorul de jurnale are nevoie de un
''' singur director de citit, deci calea se calculează într-un singur loc.
'''
''' Nu aruncă la citirea căii; <see cref="EnsureLogsDirectory"/> poate arunca (I/O), fiindcă un
''' apelant care nu poate crea directorul TREBUIE să afle.
''' </summary>
Public Module LogPaths

    ''' <summary>Numele folderului, lângă executabil.</summary>
    Public Const FolderName As String = "Logs"

    ''' <summary>
    ''' Calea folderului de jurnale. NU creează nimic pe disc — o poate apela și un cititor
    ''' care doar vrea să știe unde să se uite.
    ''' </summary>
    Public Function LogsDirectory() As String
        Return Path.Combine(AppContext.BaseDirectory, FolderName)
    End Function

    ''' <summary>
    ''' Calea folderului de jurnale, creat dacă lipsește. Aruncă dacă nu se poate crea
    ''' (disc plin / fără drepturi) — apelantul e un scriitor, iar un scriitor care crede că
    ''' a scris când nu a scris e mai rău decât unul care se plânge.
    ''' </summary>
    Public Function EnsureLogsDirectory() As String
        Dim dir As String = LogsDirectory()
        Directory.CreateDirectory(dir)
        Return dir
    End Function

    ''' <summary>Calea completă a unui fișier din folderul de jurnale. Nu atinge discul.</summary>
    Public Function Combine(fileName As String) As String
        If String.IsNullOrWhiteSpace(fileName) Then
            Throw New ArgumentException("Numele fișierului de jurnal nu poate fi gol.", NameOf(fileName))
        End If
        Return Path.Combine(LogsDirectory(), fileName)
    End Function

End Module
