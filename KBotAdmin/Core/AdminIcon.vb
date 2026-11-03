Option Strict On
Imports System.Drawing
Imports System.Reflection

''' <summary>
''' The KBotAdmin icon (slice ME01): the blue rounded square of K-BOT's window icon with an X instead of the K.
''' The file <c>KBotAdmin.ico</c> is the exe icon (the taskbar) AND is embedded, so every window and the tray
''' icon use the same one.
''' </summary>
Public Module AdminIcon

    Private _value As Icon

    ''' <summary>The icon for forms and the tray (loaded once).</summary>
    Public ReadOnly Property Value As Icon
        Get
            If _value Is Nothing Then
                Try
                    Using k_stream As IO.Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KBotAdmin.ico")
                        If k_stream IsNot Nothing Then _value = New Icon(k_stream)
                    End Using
                Catch ex As Exception
                    GlobalErrorLog.Write("AdminIcon.Value", ex)
                End Try
            End If
            Return _value
        End Get
    End Property

    ''' <summary>The icon as a small picture, for the caption bar of the borderless main window.</summary>
    Public Function Small() As Image
        Dim k_icon As Icon = Value
        If k_icon Is Nothing Then Return Nothing
        Using k_sized As New Icon(k_icon, New Size(16, 16))
            Return k_sized.ToBitmap()
        End Using
    End Function

End Module
