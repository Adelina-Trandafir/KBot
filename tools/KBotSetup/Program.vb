Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Windows.Forms

''' <summary>
''' Self-extractor K-BOT. Structura fisierului produs de publish-debug.ps1:
'''     [ stub .exe ] [ payload .zip ] [ Int64 lungime-payload ] [ magic 8 octeti ]
''' La rulare citeste propriul .exe, localizeaza payload-ul dupa footer si il
''' dezarhiveaza in C:\KBOT (sau in folderul dat ca prim argument), suprascriind tot.
''' Fara prompturi pe succes; pe eroare arata un MessageBox si iese cu cod 1
''' (zero exceptii inghitite, conform principiilor K-BOT).
''' </summary>
Module Program

    Private Const DefaultTarget As String = "C:\KBOT"
    Private Const Magic As String = "KBOTSFX1"   ' exact 8 octeti ASCII
    Private Const FooterLen As Integer = 16      ' 8 (Int64 lungime) + 8 (magic)

    <STAThread>
    Sub Main(argv As String())
        Try
            Dim target As String = DefaultTarget
            If argv IsNot Nothing AndAlso argv.Length >= 1 AndAlso Not String.IsNullOrWhiteSpace(argv(0)) Then
                target = argv(0)
            End If

            Dim selfPath As String = Environment.ProcessPath
            If String.IsNullOrEmpty(selfPath) Then
                Throw New InvalidOperationException("Nu pot determina calea propriului executabil (Environment.ProcessPath gol).")
            End If

            Dim payload As Byte() = ReadAppendedPayload(selfPath)
            ExtractOverwrite(payload, target)

            ' Nicio configurare API la instalare (Felia 1 auth): nu mai exista cheie
            ' client-side, iar adresa serverului e hardcodata in app (KBOT_API_BASE_URL
            ' ramane doar ca override de dev). Nu se provisioneaza niciun secret.

            ' Succes: iesire silentioasa (fara prompt), cod 0.
            Environment.Exit(0)

        Catch ex As Exception
            MessageBox.Show(
                "Instalarea K-BOT a esuat:" & Environment.NewLine & Environment.NewLine & ex.ToString(),
                "K-BOT Setup", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Environment.Exit(1)
        End Try
    End Sub

    ''' <summary>Extrage octetii payload-ului (.zip) atasati la finalul propriului .exe.</summary>
    Private Function ReadAppendedPayload(selfPath As String) As Byte()
        Using fs As New FileStream(selfPath, FileMode.Open, FileAccess.Read)
            If fs.Length < FooterLen Then
                Throw New InvalidDataException("Fisier setup invalid (mai mic decat footer-ul).")
            End If

            fs.Seek(-FooterLen, SeekOrigin.End)
            Dim footer(FooterLen - 1) As Byte
            ReadExactly(fs, footer, 0, FooterLen)

            Dim magicBytes As Byte() = Encoding.ASCII.GetBytes(Magic)
            For i As Integer = 0 To 7
                If footer(8 + i) <> magicBytes(i) Then
                    Throw New InvalidDataException("Marker SFX absent — acest .exe nu contine payload K-BOT.")
                End If
            Next

            Dim zipLen As Long = BitConverter.ToInt64(footer, 0)
            If zipLen <= 0 OrElse zipLen > fs.Length - FooterLen Then
                Throw New InvalidDataException($"Lungime payload invalida: {zipLen}.")
            End If

            Dim offset As Long = fs.Length - FooterLen - zipLen
            fs.Seek(offset, SeekOrigin.Begin)
            Dim buffer(CInt(zipLen) - 1) As Byte
            ReadExactly(fs, buffer, 0, CInt(zipLen))
            Return buffer
        End Using
    End Function

    ''' <summary>Citire garantata a exact <paramref name="count"/> octeti (Stream.Read poate returna partial).</summary>
    Private Sub ReadExactly(s As Stream, buf As Byte(), off As Integer, count As Integer)
        Dim total As Integer = 0
        While total < count
            Dim n As Integer = s.Read(buf, off + total, count - total)
            If n <= 0 Then Throw New EndOfStreamException("Sfarsit neasteptat la citirea payload-ului.")
            total += n
        End While
    End Sub

    ''' <summary>Dezarhiveaza payload-ul in <paramref name="destDir"/>, suprascriind fisierele existente.</summary>
    Private Sub ExtractOverwrite(payload As Byte(), destDir As String)
        Dim destFull As String = Path.GetFullPath(destDir)
        Directory.CreateDirectory(destFull)
        Dim destPrefix As String = destFull.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar

        Using ms As New MemoryStream(payload, writable:=False)
            Using archive As New ZipArchive(ms, ZipArchiveMode.Read)
                For Each entry As ZipArchiveEntry In archive.Entries
                    Dim entryPath As String = Path.GetFullPath(Path.Combine(destFull, entry.FullName))

                    ' Protectie zip-slip: intrarea trebuie sa ramana sub destinatie.
                    If Not entryPath.StartsWith(destPrefix, StringComparison.OrdinalIgnoreCase) Then
                        Throw New IOException("Intrare zip in afara folderului tinta: " & entry.FullName)
                    End If

                    If String.IsNullOrEmpty(entry.Name) Then
                        ' Intrare de director.
                        Directory.CreateDirectory(entryPath)
                    Else
                        Dim dir As String = Path.GetDirectoryName(entryPath)
                        If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)
                        entry.ExtractToFile(entryPath, overwrite:=True)
                    End If
                Next
            End Using
        End Using
    End Sub

End Module
