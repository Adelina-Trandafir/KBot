Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports KBot.Common

''' <summary>
''' Which Adobe is installed, as far as its executable says (slice 0078-12). Adobe's file version
''' starts with the year of the release line (Reader DC 2024 = 24.x), so «before 2024» is a major
''' below <see cref="FirstNewMajor"/>. That is the line the operator draws between the hosted
''' window working as designed and the older Acrobat (DC 2019 / 2020) where the form's scripts fail
''' again and the Save As flow differs. HEURISTIC: it reads a number, it does not probe behaviour.
''' </summary>
Public NotInheritable Class AdobeProductInfo

    ''' <summary>First file-version major of the line the hosted window was built against (2024).</summary>
    Public Const FirstNewMajor As Integer = 24

    ''' <summary>The executable that was read, or Nothing when none was found.</summary>
    Public ReadOnly Property Path As String
    Public ReadOnly Property ExeName As String
    ''' <summary>The file version text («19.12.20035.332343»), or empty.</summary>
    Public ReadOnly Property FileVersion As String
    ''' <summary>First number of the file version; 0 when it could not be read.</summary>
    Public ReadOnly Property Major As Integer
    Public ReadOnly Property Description As String

    Public Sub New(k_path As String, k_fileVersion As String, k_description As String)
        Path = k_path
        ExeName = If(String.IsNullOrEmpty(k_path), "", System.IO.Path.GetFileName(k_path))
        FileVersion = If(k_fileVersion, "")
        Description = If(k_description, "")
        Major = ParseMajor(FileVersion)
    End Sub

    ''' <summary>The version could be read (a major above zero).</summary>
    Public ReadOnly Property IsKnown As Boolean
        Get
            Return Major > 0
        End Get
    End Property

    ''' <summary>Known, and older than the 2024 line.</summary>
    Public ReadOnly Property IsBefore2024 As Boolean
        Get
            Return IsKnown AndAlso Major < FirstNewMajor
        End Get
    End Property

    ''' <summary>First file-version major of the line the operator recommends (Reader 2025 = 25.x).</summary>
    Public Const RecommendedReaderMajor As Integer = 25

    ''' <summary>
    ''' The Adobe the operator recommends: release line 2025 or newer. Judged by the version ONLY:
    ''' since 2024 the free Reader installs as «Adobe Acrobat» with the same Acrobat.exe and the same
    ''' file description as Pro (verified on the operator's PC, 06.10.2026), so the executable does not
    ''' say free or paid. Where the version is not known (nothing installed, unreadable) the answer is
    ''' False, and the caller decides whether that deserves a word. HEURISTIC: it reads a number.
    ''' </summary>
    Public ReadOnly Property IsRecommended As Boolean
        Get
            Return IsKnown AndAlso Major >= RecommendedReaderMajor
        End Get
    End Property

    ''' <summary>Pure: the number before the first dot of a version text; 0 when there is none.</summary>
    Public Shared Function ParseMajor(k_version As String) As Integer
        If String.IsNullOrWhiteSpace(k_version) Then Return 0
        Dim head As String = k_version.Trim()
        Dim dot As Integer = head.IndexOf("."c)
        If dot > 0 Then head = head.Substring(0, dot)
        Dim value As Integer
        If Integer.TryParse(head, value) AndAlso value > 0 Then Return value
        Return 0
    End Function

    ''' <summary>
    ''' Reads the executable's version resource. Never throws: a diagnostic must not stop a document
    ''' from opening, so a failure is logged and the result is simply «unknown».
    ''' </summary>
    Public Shared Function Read(k_path As String) As AdobeProductInfo
        If String.IsNullOrWhiteSpace(k_path) OrElse Not File.Exists(k_path) Then
            Return New AdobeProductInfo(Nothing, Nothing, Nothing)
        End If
        Try
            Dim info As FileVersionInfo = FileVersionInfo.GetVersionInfo(k_path)
            Return New AdobeProductInfo(k_path, info.FileVersion, info.FileDescription)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeProductInfo.Read", ex)
            Return New AdobeProductInfo(k_path, Nothing, Nothing)
        End Try
    End Function

    ''' <summary>One Romanian line for the working log and the bench.</summary>
    Public Function Describe() As String
        If String.IsNullOrEmpty(Path) Then Return "nu am găsit niciun produs Adobe instalat"
        If Not IsKnown Then Return $"{ExeName} (versiunea nu a putut fi citită) — {Path}"
        Dim line As String = If(IsBefore2024, "ANTERIOR lui 2024 (fluxul «vechi»)", "linia 2024 sau mai nouă")
        Return $"{ExeName} {FileVersion} — {line} — {Path}"
    End Function

End Class
