Option Strict On
Imports System.IO
Imports System.Text

''' <summary>
''' Writes the structure dump of AVACONT_SURSA over the file the schema pages already read (<c>MariaDB_Schema\AVACONT_SURSA.sql</c>, see
''' <see cref="SchemaScanner"/>), so «Schema bazelor» shows the structure a query just made. It only replaces a file that is already there; the
''' file it replaced is kept next to it as <c>AVACONT_SURSA.anterior.sql</c> (one generation), because the new text is structure only and the old
''' Navicat dump also held a few rows of data.
''' </summary>
Public NotInheritable Class SourceDumpFile

    Public Const FileName As String = "AVACONT_SURSA.sql"
    Public Const PreviousFileName As String = "AVACONT_SURSA.anterior.sql"

    Private Sub New()
    End Sub

    ''' <summary>Replaces the dump in <paramref name="k_folder"/> with <paramref name="k_text"/>; returns the path written.</summary>
    Public Shared Function Save(k_folder As String, k_text As String) As String
        Try
            Dim k_path As String = Path.Combine(k_folder, FileName)
            If Not File.Exists(k_path) Then
                Throw New AdminException("Nu există " & k_path & ". Dump-ul se rescrie doar acolo unde există deja.")
            End If
            Dim k_temp As String = k_path & ".nou"
            File.WriteAllText(k_temp, k_text, New UTF8Encoding(False))
            File.Replace(k_temp, k_path, Path.Combine(k_folder, PreviousFileName))
            Return k_path
        Catch ex As Exception
            GlobalErrorLog.Write("SourceDumpFile.Save", ex)
            Throw
        End Try
    End Function

End Class
