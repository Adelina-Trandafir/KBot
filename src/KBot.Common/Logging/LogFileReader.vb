Option Strict On
Imports System.IO
Imports System.Text

''' <summary>Rezultatul citirii unei ferestre dintr-un fișier de jurnal.</summary>
Public NotInheritable Class LogReadResult

    Public Sub New(text As String, wasTruncated As Boolean, fileLengthBytes As Long)
        Me.Text = If(text, String.Empty)
        Me.WasTruncated = wasTruncated
        Me.FileLengthBytes = fileLengthBytes
    End Sub

    ''' <summary>Textul citit (ultimii N octeți, decodați UTF-8).</summary>
    Public ReadOnly Property Text As String

    ''' <summary>True dacă fișierul era mai mare decât fereastra și începutul lui NU e în <see cref="Text"/>.</summary>
    Public ReadOnly Property WasTruncated As Boolean

    ''' <summary>Mărimea REALĂ a fișierului pe disc, nu a ferestrei citite.</summary>
    Public ReadOnly Property FileLengthBytes As Long

End Class

''' <summary>
''' Citește ultima parte a unui fișier de jurnal. Trei decizii, toate obligatorii:
'''
''' <list type="number">
''' <item><b><c>FileShare.ReadWrite</c>.</b> <c>RunLogger</c> își ține fișierul DESCHIS, cu
''' <c>AutoFlush</c>, cât timp rulează bancul de probă. Un cititor care nu permite partajarea la
''' scriere eșuează exact pe jurnalul rulării curente — cazul obișnuit, nu unul marginal. Asta e și
''' motivul pentru care nu se folosește <c>File.ReadAllText</c>.</item>
''' <item><b>Doar ultimii octeți.</b> Un fișier de 10 MB nu se încarcă întreg ca să i se vadă coada.
''' Se sare la <c>Length - fereastră</c> și se ARUNCĂ prima linie parțială, ca analizoarelor să nu
''' le ajungă niciodată o jumătate de linie.</item>
''' <item><b>Fără BOM.</b> Scriitorii scriu UTF-8 CU BOM; lăsat în text, BOM-ul ar sta lipit de
''' primul caracter al primei linii și ar strica orice potrivire de antet.</item>
''' </list>
''' </summary>
Public Module LogFileReader

    ''' <summary>Fereastra implicită: ultimii 5 MB.</summary>
    Public Const DefaultWindowBytes As Long = 5L * 1024L * 1024L

    ''' <summary>
    ''' Citește ultimii <paramref name="windowBytes"/> octeți din fișier.
    ''' Aruncă la I/O (fișier lipsă, drepturi) — apelantul e o graniță care trebuie să afle.
    ''' </summary>
    Public Function ReadTail(filePath As String,
                             Optional windowBytes As Long = DefaultWindowBytes) As LogReadResult
        If String.IsNullOrWhiteSpace(filePath) Then
            Throw New ArgumentException("Calea fișierului de jurnal nu poate fi goală.", NameOf(filePath))
        End If
        If windowBytes <= 0L Then
            Throw New ArgumentOutOfRangeException(NameOf(windowBytes), "Fereastra de citire trebuie să fie pozitivă.")
        End If

        ' FileShare.ReadWrite: vezi nota de clasă — jurnalul rulării curente e ținut deschis.
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim length As Long = fs.Length
            Dim truncated As Boolean = length > windowBytes
            Dim start As Long = Math.Max(0L, length - windowBytes)
            fs.Seek(start, SeekOrigin.Begin)

            Dim count As Integer = CInt(Math.Min(CLng(Integer.MaxValue), length - start))
            Dim buffer As Byte() = New Byte(Math.Max(count - 1, 0)) {}
            Dim read As Integer = 0
            While read < count
                Dim n As Integer = fs.Read(buffer, read, count - read)
                If n <= 0 Then Exit While
                read += n
            End While

            Dim offset As Integer = 0
            ' BOM: doar dacă am citit chiar de la începutul fișierului.
            If start = 0L AndAlso read >= 3 AndAlso
               buffer(0) = &HEF AndAlso buffer(1) = &HBB AndAlso buffer(2) = &HBF Then
                offset = 3
            End If

            Dim text As String = New UTF8Encoding(False).GetString(buffer, offset, read - offset)

            ' Fereastră mutată: prima linie e aproape sigur tăiată la mijloc. O aruncăm.
            If truncated Then
                Dim firstBreak As Integer = text.IndexOf(ControlChars.Lf)
                text = If(firstBreak >= 0, text.Substring(firstBreak + 1), String.Empty)
            End If

            Return New LogReadResult(text, truncated, length)
        End Using
    End Function

End Module
