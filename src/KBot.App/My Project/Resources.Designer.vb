'------------------------------------------------------------------------------
' Accesor puternic-tipizat pentru resursele proiectului. Scris de mână (generatorul
' VS „VbMyResourcesResXFileCodeGenerator” nu rulează sub dotnet build).
'
' DOUĂ forme de acces, amândouă obligatorii:
'   1. Clasa „Resources” de la nivelul rădăcinii => KBot.App.Resources.calendar.
'      Aceasta e forma pe care o EMITE designer-ul din Visual Studio când pui o imagine
'      pe o proprietate din property grid („Project resource file”). Dacă tipul nu există,
'      MainForm.Designer.vb nu mai compilează (BC30456).
'   2. Modulul din Namespace My.Resources, care ridică membrii la nivel de namespace
'      => My.Resources.kbot_64. Forma folosită de codul scris de mână (LoginForm,
'      MainForm, InternalInfoForm, FxIcons).
'
' Numele bazei ResourceManager („KBot.App.Resources”) trebuie să coincidă cu LogicalName
' setat pe EmbeddedResource în KBot.App.vbproj.
'
' Cheia trimisă lui GetObject e numele DIN .resx (cu cratime/puncte), nu identificatorul
' VB curățat — exact ca în codul generat de VS.
'------------------------------------------------------------------------------
Option Strict On
Option Explicit On

''' <summary>
''' Resursele proiectului, în forma pe care o generează/consumă designer-ul VS
''' (KBot.App.Resources.<i>nume</i>).
''' </summary>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated(),
 Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),
 Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>
Friend NotInheritable Class Resources

    Private Shared resourceMan As Global.System.Resources.ResourceManager
    Private Shared resourceCulture As Global.System.Globalization.CultureInfo

    Private Sub New()
        ' Doar membri Shared — nu se instanțiază.
    End Sub

    ''' <summary>ResourceManager cache-uit pentru resursele acestui proiect.</summary>
    Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
        Get
            If Object.ReferenceEquals(resourceMan, Nothing) Then
                Dim temp As Global.System.Resources.ResourceManager =
                    New Global.System.Resources.ResourceManager("KBot.App.Resources", GetType(Resources).Assembly)
                resourceMan = temp
            End If
            Return resourceMan
        End Get
    End Property

    ''' <summary>Cultura folosită la căutarea resurselor.</summary>
    Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
        Get
            Return resourceCulture
        End Get
        Set(value As Global.System.Globalization.CultureInfo)
            resourceCulture = value
        End Set
    End Property

    ''' <summary>Bitmap după numele exact din .resx.</summary>
    Private Shared Function Bmp(numeResx As String) As Global.System.Drawing.Bitmap
        Dim obj As Object = ResourceManager.GetObject(numeResx, resourceCulture)
        Return CType(obj, Global.System.Drawing.Bitmap)
    End Function

    ''' <summary>Sigla K-BOT 64px.</summary>
    Friend Shared ReadOnly Property kbot_64() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("kbot_64")
        End Get
    End Property

    ''' <summary>Pictogramă „calendar” (nav: Istoric).</summary>
    Friend Shared ReadOnly Property calendar() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("calendar")
        End Get
    End Property

    ''' <summary>Pictogramă „database” (nav: Rezervări).</summary>
    Friend Shared ReadOnly Property database() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("database")
        End Get
    End Property

    ''' <summary>Pictogramă „binvoice” (nav: Recepții).</summary>
    Friend Shared ReadOnly Property binvoice() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("binvoice")
        End Get
    End Property

    ''' <summary>Pictogramă „credit-card” (nav: Plăți).</summary>
    Friend Shared ReadOnly Property credit_card() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("credit-card")
        End Get
    End Property

    ''' <summary>Pictogramă „application”.</summary>
    Friend Shared ReadOnly Property application() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("application")
        End Get
    End Property

    ''' <summary>Pictogramă fișier „temporary” (nav: Doc. Fundamentare).</summary>
    Friend Shared ReadOnly Property Umut_Pulat_Tulliana_2_File_temporary_32() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("Umut-Pulat-Tulliana-2-File-temporary.32")
        End Get
    End Property

    ''' <summary>Pictogramă fișier „locked” (nav: Ordonanțare).</summary>
    Friend Shared ReadOnly Property Umut_Pulat_Tulliana_2_File_locked_32() As Global.System.Drawing.Bitmap
        Get
            Return Bmp("Umut-Pulat-Tulliana-2-File-locked.32")
        End Get
    End Property

End Class

Namespace My.Resources

    ''' <summary>
    ''' Aceleași resurse, ridicate în My.Resources (forma folosită de codul scris de mână).
    ''' Toate proprietățile deleagă către clasa KBot.App.Resources — o singură sursă de adevăr.
    ''' </summary>
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated(),
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>
    Friend Module Resources

        ''' <summary>ResourceManager cache-uit pentru resursele acestui proiect.</summary>
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                Return Global.KBot.App.Resources.ResourceManager
            End Get
        End Property

        ''' <summary>Cultura folosită la căutarea resurselor.</summary>
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return Global.KBot.App.Resources.Culture
            End Get
            Set(value As Global.System.Globalization.CultureInfo)
                Global.KBot.App.Resources.Culture = value
            End Set
        End Property

        ''' <summary>Sigla K-BOT 64px.</summary>
        Friend ReadOnly Property kbot_64() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.kbot_64
            End Get
        End Property

        ''' <summary>Pictogramă „calendar” (nav: Istoric).</summary>
        Friend ReadOnly Property calendar() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.calendar
            End Get
        End Property

        ''' <summary>Pictogramă „database” (nav: Rezervări).</summary>
        Friend ReadOnly Property database() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.database
            End Get
        End Property

        ''' <summary>Pictogramă „binvoice” (nav: Recepții).</summary>
        Friend ReadOnly Property binvoice() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.binvoice
            End Get
        End Property

        ''' <summary>Pictogramă „credit-card” (nav: Plăți).</summary>
        Friend ReadOnly Property credit_card() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.credit_card
            End Get
        End Property

        ''' <summary>Pictogramă „application”.</summary>
        Friend ReadOnly Property application() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.application
            End Get
        End Property

        ''' <summary>Pictogramă fișier „temporary” (nav: Doc. Fundamentare).</summary>
        Friend ReadOnly Property Umut_Pulat_Tulliana_2_File_temporary_32() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.Umut_Pulat_Tulliana_2_File_temporary_32
            End Get
        End Property

        ''' <summary>Pictogramă fișier „locked” (nav: Ordonanțare).</summary>
        Friend ReadOnly Property Umut_Pulat_Tulliana_2_File_locked_32() As Global.System.Drawing.Bitmap
            Get
                Return Global.KBot.App.Resources.Umut_Pulat_Tulliana_2_File_locked_32
            End Get
        End Property

    End Module

End Namespace
