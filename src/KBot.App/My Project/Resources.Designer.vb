'------------------------------------------------------------------------------
' Accesor puternic-tipizat pentru resursele proiectului. Scris de mână (generatorul
' VS „VbMyResourcesResXFileCodeGenerator” nu rulează sub dotnet build). Modulul din
' Namespace My.Resources ridică membrii la nivel de namespace => My.Resources.kbot_64.
' Numele bazei ResourceManager („KBot.App.Resources”) trebuie să coincidă cu LogicalName
' setat pe EmbeddedResource în KBot.App.vbproj.
'------------------------------------------------------------------------------
Option Strict On
Option Explicit On

Namespace My.Resources

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated(),
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>
    Friend Module Resources

        Private resourceMan As Global.System.Resources.ResourceManager
        Private resourceCulture As Global.System.Globalization.CultureInfo

        ''' <summary>ResourceManager cache-uit pentru resursele acestei clase.</summary>
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager =
                        New Global.System.Resources.ResourceManager("KBot.App.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property

        ''' <summary>Cultura folosită la căutarea resurselor pentru acest tip.</summary>
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set(value As Global.System.Globalization.CultureInfo)
                resourceCulture = value
            End Set
        End Property

        ''' <summary>Sigla K-BOT 64px (placeholder — poate fi înlocuit pe loc).</summary>
        Friend ReadOnly Property kbot_64() As Global.System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("kbot_64", resourceCulture)
                Return CType(obj, Global.System.Drawing.Bitmap)
            End Get
        End Property

    End Module

End Namespace
