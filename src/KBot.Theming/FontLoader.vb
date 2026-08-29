Option Strict On
Imports System.Drawing
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.InteropServices
Imports KBot.Common

''' <summary>
''' Loads the font families shipped next to the executable (…\Fonts\*.ttf) so a scheme may name
''' one that the machine has never had installed.
'''
''' <para><b>The trap this class exists for.</b> A <see cref="PrivateFontCollection"/> produces a
''' <see cref="FontFamily"/> that only <c>Graphics.DrawString</c> can resolve.
''' <c>TextRenderer.DrawText</c> is GDI, not GDI+, and CANNOT see it — and K-BOT draws through
''' both (23 call sites go through TextRenderer, 18 through DrawString). Registering the file on
''' only one of the two paths leaves roughly half the application silently on the fallback font,
''' with nothing in any log to say why. So both are done here: the collection for GDI+, and
''' <c>AddFontMemResourceEx</c> for the process-wide GDI table.</para>
'''
''' <para><b>A missing font is not a reason to refuse to start.</b> No folder, no file, an
''' unreadable file, or a family that does not resolve after registration: each one is logged
''' through <see cref="GlobalErrorLog"/> and the scheme falls back to whatever GDI picks — which
''' is what happened before this class existed. <see cref="Initialize"/> never throws.</para>
''' </summary>
Public Module FontLoader

    Private Const FontsSubfolder As String = "Fonts"

    ' Keeps GDI+ families alive for the life of the process. Collected, the FontFamily objects
    ' handed out from it become invalid mid-paint.
    Private ReadOnly _collection As New PrivateFontCollection()

    ' Handles from AddFontMemResourceEx, plus the unmanaged buffers they were built from. Both
    ' must outlive every Font drawn from them, so they are simply never released — the process
    ' ending is the only point at which they stop being needed.
    Private ReadOnly _memHandles As New List(Of IntPtr)()
    Private ReadOnly _buffers As New List(Of IntPtr)()

    Private ReadOnly _loaded As New List(Of String)()
    Private _initialized As Boolean = False

    <DllImport("gdi32.dll", ExactSpelling:=True, SetLastError:=True)>
    Private Function AddFontMemResourceEx(pbFont As IntPtr, cbFont As UInteger,
                                          pdv As IntPtr, ByRef pcFonts As UInteger) As IntPtr
    End Function

    ''' <summary>Family names successfully registered, in load order. Empty until <see cref="Initialize"/> runs.</summary>
    Public ReadOnly Property LoadedFamilies As IReadOnlyList(Of String)
        Get
            Return _loaded
        End Get
    End Property

    ''' <summary>
    ''' Registers every .ttf/.otf under <c>&lt;AppDir&gt;\Fonts</c>. Idempotent — the second call
    ''' is a no-op, so it is safe to call from both <c>Program.Main</c> and a test setup.
    ''' Must run BEFORE the first form is constructed: a form built earlier has already resolved
    ''' its font and will not re-resolve it.
    ''' </summary>
    Public Sub Initialize()
        If _initialized Then Return
        _initialized = True
        Try
            Dim folder As String = FontsFolder()
            If Not Directory.Exists(folder) Then Return          ' nothing shipped — not an error
            For Each file As String In Directory.EnumerateFiles(folder, "*.*")
                Dim ext As String = Path.GetExtension(file).ToLowerInvariant()
                If ext <> ".ttf" AndAlso ext <> ".otf" Then Continue For
                LoadFile(file)
            Next
        Catch ex As Exception
            ' Boundary, but deliberately NOT rethrown: a font folder is not worth a failed start.
            GlobalErrorLog.Write("FontLoader.Initialize", ex)
        End Try
    End Sub

    ''' <summary>…\Fonts, next to the running assembly.</summary>
    Public Function FontsFolder() As String
        Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FontsSubfolder)
    End Function

    ''' <summary>
    ''' True if <paramref name="familyName"/> can actually be drawn — either installed on the
    ''' machine or registered from <c>…\Fonts</c>. This is the check a caller wants before
    ''' believing a scheme's font name: GDI resolves an unknown name to the default silently, so
    ''' a typo is otherwise invisible.
    ''' </summary>
    Public Function IsAvailable(familyName As String) As Boolean
        If String.IsNullOrWhiteSpace(familyName) Then Return False
        Try
            For Each fam As FontFamily In FontFamily.Families
                If String.Equals(fam.Name, familyName, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
            For Each fam As FontFamily In _collection.Families
                If String.Equals(fam.Name, familyName, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("FontLoader.IsAvailable", ex)
            Return False
        End Try
    End Function

    ' One file, both registration paths. A failure on one file is logged and the next file is
    ' still tried — half a family is better than none, and the log says which half is missing.
    Private Sub LoadFile(filePath As String)
        Try
            Dim bytes As Byte() = File.ReadAllBytes(filePath)
            If bytes.Length = 0 Then
                GlobalErrorLog.Write("FontLoader.LoadFile",
                    New InvalidDataException($"Empty font file: {filePath}"))
                Return
            End If

            Dim buffer As IntPtr = Marshal.AllocCoTaskMem(bytes.Length)
            Marshal.Copy(bytes, 0, buffer, bytes.Length)
            _buffers.Add(buffer)

            ' (1) GDI+ — what Graphics.DrawString resolves against.
            _collection.AddMemoryFont(buffer, bytes.Length)

            ' (2) GDI — what TextRenderer.DrawText resolves against. Without this, every
            ' TextRenderer call site falls back to the default font.
            Dim installed As UInteger = 0
            Dim handle As IntPtr = AddFontMemResourceEx(buffer, CUInt(bytes.Length), IntPtr.Zero, installed)
            If handle = IntPtr.Zero OrElse installed = 0UI Then
                GlobalErrorLog.Write("FontLoader.LoadFile",
                    New InvalidOperationException(
                        $"AddFontMemResourceEx failed for {Path.GetFileName(filePath)} " &
                        $"(code {Marshal.GetLastWin32Error()}). The font will only be visible on the GDI+ path."))
            Else
                _memHandles.Add(handle)
            End If

            RecordFamilies()
        Catch ex As Exception
            GlobalErrorLog.Write($"FontLoader.LoadFile({Path.GetFileName(filePath)})", ex)
        End Try
    End Sub

    Private Sub RecordFamilies()
        For Each fam As FontFamily In _collection.Families
            If Not _loaded.Contains(fam.Name) Then _loaded.Add(fam.Name)
        Next
    End Sub

End Module
