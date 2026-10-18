Imports System.IO
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>What a file of the source folder is, judged by its NAME only (decided by the operator, 12.10.2026).</summary>
Public Enum VrcFileKind
    ''' <summary>tz_&lt;year&gt;.accdb / .mdb: the exercise of a Materiale or Alimente gestiune.</summary>
    Year = 0
    ''' <summary>tz_2100.accdb / .mdb: 2100 is NOT a year, it marks a special base (Obiecte de inventar or Mijloace fixe).</summary>
    Special = 1
    ''' <summary>A name that is not tz_&lt;4 digits&gt;: listed but not selected by default.</summary>
    Other = 2
End Enum

''' <summary>One candidate file of the source folder.</summary>
Public NotInheritable Class VrcSourceFile

    Public Const SpecialMarker As Integer = 2100

    Private Shared ReadOnly YearName As New Regex("^tz_(\d{4})$", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Public Property Path As String = String.Empty
    Public Property Kind As VrcFileKind
    ''' <summary>The exercise year for <see cref="VrcFileKind.Year"/>; 0 otherwise (2100 is never a year).</summary>
    Public Property Year As Integer
    Public Property SizeBytes As Long
    Public Property Selected As Boolean

    Public ReadOnly Property FileName As String
        Get
            Return IO.Path.GetFileName(Path)
        End Get
    End Property

    ''' <summary>"accdb" or "mdb".</summary>
    Public ReadOnly Property Format As String
        Get
            Return IO.Path.GetExtension(Path).TrimStart("."c).ToLowerInvariant()
        End Get
    End Property

    ''' <summary>
    ''' Classifies one file by name. Returns Nothing for the files that must never be selected: <c>tz.mdb</c> / <c>tz.accdb</c>
    ''' (the plain name without an underscore) and every file that is not an Access base.
    ''' </summary>
    Public Shared Function FromPath(k_path As String) As VrcSourceFile
        Dim k_extension As String = IO.Path.GetExtension(k_path).ToLowerInvariant()
        If k_extension <> ".accdb" AndAlso k_extension <> ".mdb" Then Return Nothing
        Dim k_name As String = IO.Path.GetFileNameWithoutExtension(k_path)
        If String.Equals(k_name, "tz", StringComparison.OrdinalIgnoreCase) Then Return Nothing   ' tz.mdb: never taken
        Dim k_file As New VrcSourceFile() With {.Path = k_path, .SizeBytes = New FileInfo(k_path).Length}
        Dim k_match = YearName.Match(k_name)
        If k_match.Success Then
            Dim k_number As Integer = Integer.Parse(k_match.Groups(1).Value, Globalization.CultureInfo.InvariantCulture)
            If k_number = SpecialMarker Then
                k_file.Kind = VrcFileKind.Special
            Else
                k_file.Kind = VrcFileKind.Year
                k_file.Year = k_number
            End If
            k_file.Selected = True
        ElseIf k_name.StartsWith("tz_", StringComparison.OrdinalIgnoreCase) Then
            k_file.Kind = VrcFileKind.Other        ' e.g. tz_Alimente: listed, not selected, the operator may tick it
            k_file.Selected = False
        Else
            Return Nothing                         ' not part of the source set
        End If
        Return k_file
    End Function

    ''' <summary>
    ''' A file the operator picked BY HAND: the choice is respected even when the name is not tz_*. Only tz.mdb / tz.accdb and
    ''' files that are not Access bases are refused (Nothing). A name outside the tz_ pattern becomes <see cref="VrcFileKind.Other"/>, ticked.
    ''' </summary>
    Public Shared Function FromChosenPath(k_path As String) As VrcSourceFile
        Dim k_file As VrcSourceFile = FromPath(k_path)
        If k_file IsNot Nothing Then
            k_file.Selected = True
            Return k_file
        End If
        Dim k_extension As String = IO.Path.GetExtension(k_path).ToLowerInvariant()
        If k_extension <> ".accdb" AndAlso k_extension <> ".mdb" Then Return Nothing
        If String.Equals(IO.Path.GetFileNameWithoutExtension(k_path), "tz", StringComparison.OrdinalIgnoreCase) Then Return Nothing
        Return New VrcSourceFile() With {.Path = k_path, .SizeBytes = New FileInfo(k_path).Length, .Kind = VrcFileKind.Other, .Selected = True}
    End Function

    ''' <summary>
    ''' All candidate files of <paramref name="k_folder"/> (that folder only, both .accdb and .mdb, a year in both formats
    ''' gives two rows). <paramref name="k_ignored"/> receives the names that were skipped on purpose.
    ''' </summary>
    Public Shared Function Scan(k_folder As String, k_ignored As List(Of String)) As List(Of VrcSourceFile)
        Try
            Dim k_result As New List(Of VrcSourceFile)()
            For Each k_path In Directory.EnumerateFiles(k_folder)
                Dim k_extension As String = IO.Path.GetExtension(k_path).ToLowerInvariant()
                If k_extension <> ".accdb" AndAlso k_extension <> ".mdb" Then Continue For
                Dim k_file As VrcSourceFile = FromPath(k_path)
                If k_file Is Nothing Then
                    k_ignored?.Add(IO.Path.GetFileName(k_path))
                Else
                    k_result.Add(k_file)
                End If
            Next
            Return k_result.OrderBy(Function(k_f) If(k_f.Kind = VrcFileKind.Year, 0, If(k_f.Kind = VrcFileKind.Special, 1, 2))) _
                           .ThenBy(Function(k_f) k_f.Year) _
                           .ThenBy(Function(k_f) k_f.FileName, StringComparer.OrdinalIgnoreCase).ToList()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcSourceFile.Scan", ex)
            Throw
        End Try
    End Function

End Class
