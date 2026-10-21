Option Strict On
Imports System.Drawing
Imports System.Reflection

''' <summary>
''' The pictures of the ribbon buttons (slice ME01-05). They are K-BOT's own icons, linked into this program as embedded
''' resources named <c>icon.&lt;name&gt;</c> (see KBotAdmin.vbproj). Loaded once each; an unknown name is a mistake in the
''' code, so it raises instead of drawing a button without a picture.
''' </summary>
Public Module AdminIcons

    Private ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private ReadOnly _sync As New Object()

    ''' <summary>The icon with this name (no file extension), the same instance every time.</summary>
    Public Function [Get](k_name As String) As Image
        Try
            SyncLock _sync
                Dim k_image As Image = Nothing
                If _cache.TryGetValue(k_name, k_image) Then Return k_image
                Using k_stream As IO.Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("icon." & k_name)
                    If k_stream Is Nothing Then Throw New ArgumentException("Iconita '" & k_name & "' nu e inregistrata in KBotAdmin.vbproj.", NameOf(k_name))
                    Using k_raw As Image = Image.FromStream(k_stream)
                        k_image = New Bitmap(k_raw)   ' detached from the stream, which is closed on leaving the Using
                    End Using
                End Using
                _cache(k_name) = k_image
                Return k_image
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("AdminIcons.Get", ex)
            Throw
        End Try
    End Function

End Module
