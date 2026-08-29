Option Strict On
Imports System.Drawing
Imports System.Drawing.Imaging
Imports KBot.Common

''' <summary>
''' Recolours a monochrome icon to a palette colour, keeping its alpha — the shape stays exactly
''' as drawn, only the ink changes.
'''
''' <para><b>Why a colour matrix and not a set of per-theme icon files.</b> Shipping one bitmap
''' per scheme means every new scheme needs a new set of artwork, and the icon a control asks for
''' has to be looked up by key rather than simply held as an <c>Image</c> property. The matrix
''' works on any monochrome bitmap already in <c>Resources.resx</c>, so nothing about how a
''' control receives its icon changes.</para>
'''
''' <para><b>Results are cached.</b> Tinting inside a paint handler shows as lag on a long list;
''' the cache is keyed on (image identity, colour, size) and is emptied by <see cref="Clear"/>
''' whenever the scheme changes, which is the only moment the answers can go stale.</para>
''' </summary>
Public Module IconTint

    Private ReadOnly _cache As New Dictionary(Of String, Bitmap)()
    Private ReadOnly _gate As New Object()

    ''' <summary>Number of tinted bitmaps currently held. Exists so a test can prove the cache does not grow without bound.</summary>
    Public ReadOnly Property CacheCount As Integer
        Get
            SyncLock _gate
                Return _cache.Count
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Drops every cached bitmap and disposes it. Called on a scheme change: the cached results
    ''' are keyed on a colour that no longer belongs to the active scheme.
    ''' </summary>
    Public Sub Clear()
        SyncLock _gate
            For Each bmp As Bitmap In _cache.Values
                Try
                    bmp.Dispose()
                Catch ex As Exception
                    ' A bitmap still bound to a Graphics can refuse to dispose. Dropping the
                    ' reference is enough; the GC finishes the job.
                    GlobalErrorLog.Write("IconTint.Clear", ex)
                End Try
            Next
            _cache.Clear()
        End SyncLock
    End Sub

    ''' <summary>
    ''' <paramref name="source"/> drawn in <paramref name="color"/>, at its own size.
    ''' Returns <c>Nothing</c> for a <c>Nothing</c> source. The returned bitmap belongs to the
    ''' cache — the CALLER MUST NOT DISPOSE IT.
    ''' </summary>
    Public Function Tint(source As Image, color As Color) As Image
        If source Is Nothing Then Return Nothing
        Return Tint(source, color, source.Width, source.Height)
    End Function

    ''' <summary>
    ''' <paramref name="source"/> drawn in <paramref name="color"/>, scaled to
    ''' <paramref name="width"/> x <paramref name="height"/>. The returned bitmap belongs to the
    ''' cache — the CALLER MUST NOT DISPOSE IT. On any failure the ORIGINAL image comes back:
    ''' an icon in the wrong colour reads better than a hole where an icon should be.
    ''' </summary>
    Public Function Tint(source As Image, color As Color, width As Integer, height As Integer) As Image
        If source Is Nothing Then Return Nothing
        If width <= 0 OrElse height <= 0 Then Return source

        Dim key As String = $"{source.GetHashCode()}|{color.ToArgb()}|{width}x{height}"
        SyncLock _gate
            Dim hit As Bitmap = Nothing
            If _cache.TryGetValue(key, hit) Then Return hit
        End SyncLock

        Try
            Dim tinted As Bitmap = Build(source, color, width, height)
            SyncLock _gate
                ' Another thread may have built the same key while this one was drawing.
                Dim hit As Bitmap = Nothing
                If _cache.TryGetValue(key, hit) Then
                    tinted.Dispose()
                    Return hit
                End If
                _cache(key) = tinted
            End SyncLock
            Return tinted
        Catch ex As Exception
            GlobalErrorLog.Write("IconTint.Tint", ex)
            Return source
        End Try
    End Function

    ' The matrix zeroes the source RGB and adds the target colour back through the fourth row,
    ' leaving alpha (row 4, column 4 = 1) exactly as the artwork drew it. That is what keeps the
    ' anti-aliased edge of the glyph intact instead of turning it into a hard cutout.
    Private Function Build(source As Image, color As Color, width As Integer, height As Integer) As Bitmap
        Dim result As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Try
            Using g As Graphics = Graphics.FromImage(result)
                g.Clear(Color.Transparent)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

                Dim m As New ColorMatrix(New Single()() {
                    New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                    New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                    New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                    New Single() {0.0F, 0.0F, 0.0F, 1.0F, 0.0F},
                    New Single() {color.R / 255.0F, color.G / 255.0F, color.B / 255.0F, 0.0F, 1.0F}
                })

                Using attrs As New ImageAttributes()
                    attrs.SetColorMatrix(m)
                    g.DrawImage(source, New Rectangle(0, 0, width, height),
                                0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attrs)
                End Using
            End Using
            Return result
        Catch
            result.Dispose()
            Throw
        End Try
    End Function

End Module
