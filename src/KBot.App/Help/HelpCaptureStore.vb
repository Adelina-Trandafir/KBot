Option Strict On
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports KBot.Common

''' <summary>
''' Where help screenshots are written and read (slice 0000-02).
'''
''' <para><b>Always</b> into <c>&lt;AppDir&gt;\Help\img\</c>, which is where the help window reads
''' them, so a new picture shows at once. <b>Also</b> into the source folder
''' <c>src\KBot.App\HelpContent\img\</c> when K-BOT runs from a build inside the repository (found
''' by walking up from the exe), so on the developer's PC the picture lands straight in the code.
''' On a client's PC there is no repository: the pictures stay in <c>C:\KBOT\Help\img\</c> (the
''' updater never deletes files it did not ship) and are copied back by hand; «Deschide dosarul»
''' opens that folder.</para>
''' </summary>
Public NotInheritable Class HelpCaptureStore

    Private ReadOnly _runtimeFolder As String
    Private ReadOnly _sourceFolder As String

    Public Sub New(library As HelpLibrary)
        _runtimeFolder = Path.Combine(library.Root, "img")
        _sourceFolder = FindSourceFolder()
    End Sub

    ''' <summary>The folder the help reads pictures from.</summary>
    Public ReadOnly Property RuntimeFolder As String
        Get
            Return _runtimeFolder
        End Get
    End Property

    ''' <summary>The repository's picture folder, or Nothing when K-BOT does not run from a repository build.</summary>
    Public ReadOnly Property SourceFolder As String
        Get
            Return _sourceFolder
        End Get
    End Property

    Public Function PathFor(capture As HelpCapture) As String
        Return Path.Combine(_runtimeFolder, capture.Id & ".png")
    End Function

    Public Function Exists(capture As HelpCapture) As Boolean
        Return File.Exists(PathFor(capture))
    End Function

    ''' <summary>When the picture was taken, or Nothing.</summary>
    Public Function TakenAt(capture As HelpCapture) As DateTime?
        Dim p As String = PathFor(capture)
        If Not File.Exists(p) Then Return Nothing
        Return File.GetLastWriteTime(p)
    End Function

    ''' <summary>Writes the picture as PNG to every folder it belongs in; returns the paths written.</summary>
    Public Function Save(capture As HelpCapture, picture As Bitmap) As List(Of String)
        Try
            Dim written As New List(Of String)()
            For Each folder As String In {_runtimeFolder, _sourceFolder}
                If String.IsNullOrEmpty(folder) Then Continue For
                Directory.CreateDirectory(folder)
                Dim target As String = Path.Combine(folder, capture.Id & ".png")
                picture.Save(target, ImageFormat.Png)
                written.Add(target)
            Next
            Return written
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureStore.Save", ex)
            Throw
        End Try
    End Function

    ' <repo>\src\KBot.App\HelpContent\img, looked for above the exe (bin\Debug\net8.0-windows\ is
    ' four levels under src\KBot.App). Nothing outside a repository build.
    Private Shared Function FindSourceFolder() As String
        Dim dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
        For i As Integer = 0 To 8
            If dir Is Nothing Then Exit For
            Dim candidate As String = Path.Combine(dir.FullName, "HelpContent")
            If Directory.Exists(candidate) AndAlso File.Exists(Path.Combine(dir.FullName, "KBot.App.vbproj")) Then
                Return Path.Combine(candidate, "img")
            End If
            dir = dir.Parent
        Next
        Return Nothing
    End Function

End Class
