Option Strict On
Imports System.Diagnostics
Imports System.IO

''' <summary>
''' Rotația jurnalelor client: ISTORIC, nu distrugere.
'''
''' Aceeași politică pe care serverul o are deja prin <c>RotatingFileHandler</c> din
''' <c>utils/logger.py</c> — 10 MB, cinci generații — ca să existe O SINGURĂ regulă peste ambele
''' jumătăți ale sistemului, nu două. La depășire fișierul viu devine <c>.1</c>, <c>.1</c> devine
''' <c>.2</c> și așa mai departe; a șasea generație nu există niciodată. Consumul maxim pe familie
''' de fișiere e mărginit și previzibil: 60 MB.
'''
''' Se cheamă ÎNAINTE de fiecare adăugare. Nu în vizualizator: un vizualizator ar aplica limita
''' doar când îl deschide cineva, adică exact atunci când nu mai contează.
'''
''' <para><b>Nu aruncă NICIODATĂ.</b> Orice eșec (fișier ținut deschis de alt proces, drepturi,
''' disc plin) se scrie pe <see cref="Trace"/> și întoarce False, ca adăugarea care a declanșat
''' verificarea să se întâmple oricum. O problemă de rotație nu are voie să coste linia care a
''' provocat-o.</para>
''' </summary>
Public Module LogRotation

    ''' <summary>Pragul implicit: 10 MB, ca pe server.</summary>
    Public Const MaxBytes As Long = 10L * 1024L * 1024L

    ''' <summary>Câte generații de arhivă se păstrează: <c>.1</c> … <c>.5</c>.</summary>
    Public Const BackupCount As Integer = 5

    ''' <summary>
    ''' Rotește <paramref name="filePath"/> dacă a depășit <paramref name="maxBytes"/>.
    ''' Întoarce True DOAR dacă rotația chiar s-a făcut.
    '''
    ''' După o rotație reușită fișierul viu NU mai există pe disc — apelantul îl recreează prin
    ''' adăugarea lui obișnuită (<c>File.AppendAllText</c> creează fișierul lipsă).
    ''' </summary>
    Public Function Roll(filePath As String,
                         Optional maxBytes As Long = MaxBytes,
                         Optional backupCount As Integer = BackupCount) As Boolean
        Try
            If String.IsNullOrWhiteSpace(filePath) Then Return False

            ' Parametri fără sens: NU ștergem nimic. Refuzul e mereu mai bun decât o distrugere
            ' de istoric pornită dintr-o valoare greșită.
            If maxBytes <= 0L Then
                Trace.WriteLine("LogRotation: maxBytes <= 0 pentru " & filePath & " — nu se rotește.")
                Return False
            End If
            If backupCount < 1 Then
                Trace.WriteLine("LogRotation: backupCount < 1 pentru " & filePath & " — nu se rotește (istoricul nu se distruge).")
                Return False
            End If

            Dim info As New FileInfo(filePath)
            If Not info.Exists Then Return False
            If info.Length <= maxBytes Then Return False

            ' Cea mai veche generație iese din istoric. De aici încolo orice eșec lasă lucrurile
            ' într-o stare parțial rotită, dar NICIODATĂ cu fișierul viu pierdut: mutarea lui e
            ' ultimul pas.
            Dim oldest As String = filePath & "." & backupCount.ToString(Globalization.CultureInfo.InvariantCulture)
            If File.Exists(oldest) Then File.Delete(oldest)

            ' .4 -> .5, .3 -> .4, … , .1 -> .2   (de la coadă spre cap, ca să nu suprascriem)
            For generation As Integer = backupCount - 1 To 1 Step -1
                Dim src As String = filePath & "." & generation.ToString(Globalization.CultureInfo.InvariantCulture)
                If Not File.Exists(src) Then Continue For
                Dim dst As String = filePath & "." & (generation + 1).ToString(Globalization.CultureInfo.InvariantCulture)
                If File.Exists(dst) Then File.Delete(dst)
                File.Move(src, dst)
            Next

            ' Fișierul viu devine .1. Doar redenumire — costul NU depinde de mărimea fișierului.
            Dim first As String = filePath & ".1"
            If File.Exists(first) Then File.Delete(first)
            File.Move(filePath, first)

            Return True
        Catch ex As Exception
            ' SINK TERMINAL, ca GlobalErrorLog: rotația e un serviciu al scrierii, nu invers.
            ' Dacă a eșuat, linia care urmează trebuie totuși scrisă.
            Trace.WriteLine("LogRotation.Roll a eșuat pentru " & If(filePath, "<null>") & ": " & ex.Message)
            Return False
        End Try
    End Function

End Module
