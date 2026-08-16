Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Obiectul pe care îl editează PropertyGrid-ul din <see cref="ThemeOptionsForm"/>: o
''' <see cref="ThemeScheme"/> ÎNTREAGĂ — cele 23 de sloturi de culoare ale paletei plus toate
''' opțiunile de stil.
'''
''' <para><b>Diferența față de <see cref="ControlStyleProxy"/></b>, cu care se confundă ușor:
''' acela editează UN CONTROL de pe un formular anume («fă butonul ăsta roșu»), ăsta editează
''' SCHEMA («ce înseamnă „Modern"»). Primul produce un fișier de excepții legat de o suprafață,
''' al doilea rescrie tema însăși, pentru toate ferestrele deodată.</para>
'''
''' <para><b>De ce un proxy și nu schema direct în grilă.</b> Trei motive, toate practice:
''' paleta ține culorile ca ȘIRURI hex, iar un operator vrea selectorul de culoare, nu să
''' tasteze «#2D2D30»; sloturile trebuie grupate pe categorii cu nume românești, ceea ce cere
''' atribute; și fiecare scriere trebuie să declanșeze repictarea, altfel ai edita în orb.</para>
'''
''' <para><b>Efectul e imediat, dar NU se salvează singur.</b> Fiecare setare cheamă
''' <c>ThemeManager.Refresh</c>, deci ecranul urmează cursorul; pe disc nu se scrie nimic până la
''' «Salvează». Așa se poate umbla liniștit prin culori fără să rămână ceva stricat dacă
''' operatorul se răzgândește — închide fereastra fără să salveze și repornirea aduce înapoi
''' schema de dinainte.</para>
''' </summary>
Public NotInheritable Class SchemeOptionsProxy

    Private ReadOnly _scheme As ThemeScheme
    Private ReadOnly _onChanged As Action

    ''' <param name="scheme">Schema editată — se modifică pe loc (schemele sunt mutabile prin design).</param>
    ''' <param name="onChanged">Chemat după fiecare scriere reușită: repictarea + marcarea „nesalvat”.</param>
    Public Sub New(scheme As ThemeScheme, onChanged As Action)
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        _scheme = scheme
        _onChanged = onChanged
    End Sub

    ''' <summary>Schema editată (formularul o folosește la salvare).</summary>
    <Browsable(False)>
    Public ReadOnly Property Scheme As ThemeScheme
        Get
            Return _scheme
        End Get
    End Property

    ' ── 1. Schemă ────────────────────────────────────────────────────────────────

    <Category("1. Schemă")>
    <DisplayName("Nume")>
    <Description("Numele schemei. E și CHEIA sub care se salvează fișierul și se ține minte alegerea, deci nu se schimbă de aici.")>
    Public ReadOnly Property Nume As String
        Get
            Return _scheme.Name
        End Get
    End Property

    <Category("1. Schemă")>
    <DisplayName("Schemă întunecată")>
    <Description("Spune motorului că schema e închisă la culoare: de aici vin bara de titlu DWM și varianta «DarkMode» a listelor/combo-urilor de sistem. Nu schimbă singură nicio culoare din paletă.")>
    Public Property Intunecata As Boolean
        Get
            Return _scheme.IsDark
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.IsDark = value, "Intunecata")
        End Set
    End Property

    ' ── 2. Suprafețe și text ─────────────────────────────────────────────────────

    <Category("2. Suprafețe și text")>
    <DisplayName("Suprafață")>
    <Description("Fundalul formularelor și al panourilor obișnuite.")>
    Public Property Suprafata As Color
        Get
            Return _scheme.Palette.SurfaceColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Surface = ColorHex.ToHex(value), "Suprafata")
        End Set
    End Property

    <Category("2. Suprafețe și text")>
    <DisplayName("Suprafață secundară")>
    <Description("Fundalul suprafețelor de tip card și al filei active.")>
    Public Property SuprafataAlt As Color
        Get
            Return _scheme.Palette.SurfaceAltColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.SurfaceAlt = ColorHex.ToHex(value), "SuprafataAlt")
        End Set
    End Property

    <Category("2. Suprafețe și text")>
    <DisplayName("Text")>
    <Description("Culoarea textului obișnuit.")>
    Public Property TextCuloare As Color
        Get
            Return _scheme.Palette.TextColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Text = ColorHex.ToHex(value), "Text")
        End Set
    End Property

    <Category("2. Suprafețe și text")>
    <DisplayName("Text estompat")>
    <Description("Textul secundar: explicații, stări, rânduri de subsol.")>
    Public Property TextEstompat As Color
        Get
            Return _scheme.Palette.TextDimColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.TextDim = ColorHex.ToHex(value), "TextDim")
        End Set
    End Property

    <Category("2. Suprafețe și text")>
    <DisplayName("Contur")>
    <Description("Liniile de despărțire și chenarele.")>
    Public Property Contur As Color
        Get
            Return _scheme.Palette.BorderColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Border = ColorHex.ToHex(value), "Border")
        End Set
    End Property

    ' ── 3. Câmpuri de introducere ────────────────────────────────────────────────

    <Category("3. Câmpuri")>
    <DisplayName("Fundal câmp")>
    <Description("Fundalul casetelor de text, al listelor și al combo-urilor.")>
    Public Property FundalCamp As Color
        Get
            Return _scheme.Palette.InputBackColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.InputBack = ColorHex.ToHex(value), "InputBack")
        End Set
    End Property

    <Category("3. Câmpuri")>
    <DisplayName("Text câmp")>
    <Description("Culoarea textului din câmpuri.")>
    Public Property TextCamp As Color
        Get
            Return _scheme.Palette.InputTextColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.InputText = ColorHex.ToHex(value), "InputText")
        End Set
    End Property

    <Category("3. Câmpuri")>
    <DisplayName("Contur câmp")>
    <Description("Chenarul câmpurilor.")>
    Public Property ConturCamp As Color
        Get
            Return _scheme.Palette.InputBorderColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.InputBorder = ColorHex.ToHex(value), "InputBorder")
        End Set
    End Property

    ' ── 4. Butoane ───────────────────────────────────────────────────────────────

    <Category("4. Butoane")>
    <DisplayName("Fundal buton")>
    <Description("Fundalul butoanelor obișnuite.")>
    Public Property FundalButon As Color
        Get
            Return _scheme.Palette.ButtonBackColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.ButtonBack = ColorHex.ToHex(value), "ButtonBack")
        End Set
    End Property

    <Category("4. Butoane")>
    <DisplayName("Contur buton")>
    <Description("Chenarul butoanelor.")>
    Public Property ConturButon As Color
        Get
            Return _scheme.Palette.ButtonBorderColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.ButtonBorder = ColorHex.ToHex(value), "ButtonBorder")
        End Set
    End Property

    <Category("4. Butoane")>
    <DisplayName("Buton sub cursor")>
    <Description("Fundalul butonului survolat.")>
    Public Property ButonHover As Color
        Get
            Return _scheme.Palette.ButtonHoverColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.ButtonHover = ColorHex.ToHex(value), "ButtonHover")
        End Set
    End Property

    <Category("4. Butoane")>
    <DisplayName("Buton apăsat")>
    <Description("Fundalul butonului în timpul apăsării.")>
    Public Property ButonApasat As Color
        Get
            Return _scheme.Palette.ButtonPressedColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.ButtonPressed = ColorHex.ToHex(value), "ButtonPressed")
        End Set
    End Property

    <Category("4. Butoane")>
    <DisplayName("Text buton")>
    <Description("Culoarea textului de pe butoane.")>
    Public Property TextButon As Color
        Get
            Return _scheme.Palette.ButtonTextColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.ButtonText = ColorHex.ToHex(value), "ButtonText")
        End Set
    End Property

    ' ── 5. Accent ────────────────────────────────────────────────────────────────

    <Category("5. Accent")>
    <DisplayName("Accent")>
    <Description("Culoarea principală de accent: selecție, evidențiere, elementul activ.")>
    Public Property Accent As Color
        Get
            Return _scheme.Palette.AccentColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Accent = ColorHex.ToHex(value), "Accent")
        End Set
    End Property

    <Category("5. Accent")>
    <DisplayName("Text pe accent")>
    <Description("Culoarea textului așezat PESTE accent — de citit, nu de asortat.")>
    Public Property TextPeAccent As Color
        Get
            Return _scheme.Palette.AccentTextColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.AccentText = ColorHex.ToHex(value), "AccentText")
        End Set
    End Property

    <Category("5. Accent")>
    <DisplayName("Accent sub cursor")>
    <Description("Varianta survolată a accentului.")>
    Public Property AccentHover As Color
        Get
            Return _scheme.Palette.AccentHoverColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.AccentHover = ColorHex.ToHex(value), "AccentHover")
        End Set
    End Property

    ' ── 6. File ──────────────────────────────────────────────────────────────────

    <Category("6. File")>
    <DisplayName("Accent filă")>
    <Description("Dunga de sub fila activă. Se vede doar cu «Desenează filele» aprins.")>
    Public Property AccentFila As Color
        Get
            Return _scheme.Palette.TabAccentColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.TabAccent = ColorHex.ToHex(value), "TabAccent")
        End Set
    End Property

    <Category("6. File")>
    <DisplayName("Filă inactivă")>
    <Description("Fundalul filelor neselectate. Se vede doar cu «Desenează filele» aprins.")>
    Public Property FilaInactiva As Color
        Get
            Return _scheme.Palette.TabInactiveColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.TabInactive = ColorHex.ToHex(value), "TabInactive")
        End Set
    End Property

    ' ── 7. Stări ─────────────────────────────────────────────────────────────────

    <Category("7. Stări")>
    <DisplayName("Eroare")>
    <Description("Culoarea mesajelor de eroare.")>
    Public Property Eroare As Color
        Get
            Return _scheme.Palette.ErrorColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.[Error] = ColorHex.ToHex(value), "Error")
        End Set
    End Property

    <Category("7. Stări")>
    <DisplayName("Succes")>
    <Description("Culoarea confirmărilor.")>
    Public Property Succes As Color
        Get
            Return _scheme.Palette.SuccessColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Success = ColorHex.ToHex(value), "Success")
        End Set
    End Property

    <Category("7. Stări")>
    <DisplayName("Avertisment")>
    <Description("Culoarea avertismentelor.")>
    Public Property Avertisment As Color
        Get
            Return _scheme.Palette.WarningColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.Warning = ColorHex.ToHex(value), "Warning")
        End Set
    End Property

    <Category("7. Stări")>
    <DisplayName("Inel de focus")>
    <Description("Culoarea inelului de focus. Se vede doar cu «Accent pe focus» aprins.")>
    Public Property InelFocus As Color
        Get
            Return _scheme.Palette.FocusRingColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.FocusRing = ColorHex.ToHex(value), "FocusRing")
        End Set
    End Property

    <Category("7. Stări")>
    <DisplayName("Text dezactivat")>
    <Description("Culoarea textului din controalele stinse.")>
    Public Property TextDezactivat As Color
        Get
            Return _scheme.Palette.DisabledTextColor
        End Get
        Set(value As Color)
            Apply(Sub() _scheme.Palette.DisabledText = ColorHex.ToHex(value), "DisabledText")
        End Set
    End Property

    ' ── 8. Stil ──────────────────────────────────────────────────────────────────

    <Category("8. Stil")>
    <DisplayName("Culori de sistem")>
    <Description("Aprins, motorul NU pictează nimic din paletă și lasă culorile Windows (așa e făcut «Clasic»). Restul culorilor de mai sus rămân fără efect pe controalele obișnuite.")>
    Public Property CuloriDeSistem As Boolean
        Get
            Return _scheme.Style.UseSystemColors
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.UseSystemColors = value, "UseSystemColors")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Păstrează culorile din designer")>
    <Description("Aprins, motorul pune înapoi culorile autorite în designer în loc să scrie paleta (așa e făcut «Colorat»). Controalele K-BOT își iau în continuare culorile interne din paletă.")>
    Public Property PastreazaCuloriDesigner As Boolean
        Get
            Return _scheme.Style.PreserveDesignerColors
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.PreserveDesignerColors = value, "PreserveDesignerColors")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Controale plate")>
    <Description("Butoanele și filele fără relief.")>
    Public Property ControalePlate As Boolean
        Get
            Return _scheme.Style.FlatControls
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.FlatControls = value, "FlatControls")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Randarea butoanelor")>
    <Description("System = butonul Windows; Flat = plat, cu culorile paletei; ModernOwnerDrawn = desenat de noi, cu colțuri rotunjite și survolare pictată.")>
    Public Property RandareButoane As ButtonRenderStyle
        Get
            Return _scheme.Style.ButtonRender
        End Get
        Set(value As ButtonRenderStyle)
            Apply(Sub() _scheme.Style.ButtonRender = value, "ButtonRender")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Rază colț")>
    <Description("Raza colțurilor rotunjite, în pixeli logici la 96 dpi (se scalează la pictare). 0 = colț drept.")>
    Public Property RazaColt As Integer
        Get
            Return _scheme.Style.CornerRadius
        End Get
        Set(value As Integer)
            If value < 0 Then Throw New ArgumentException("Raza colțului nu poate fi negativă.")
            Apply(Sub() _scheme.Style.CornerRadius = value, "CornerRadius")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Accent pe focus")>
    <Description("Inel/subliniere de accent pe câmpul care are focusul.")>
    Public Property AccentPeFocus As Boolean
        Get
            Return _scheme.Style.FocusAccent
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.FocusAccent = value, "FocusAccent")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Bară de titlu întunecată")>
    <Description("Cere Windows-ului bara de titlu întunecată (DWM). Se vede doar pe ferestrele CU chenar de sistem.")>
    Public Property BaraTitluIntunecata As Boolean
        Get
            Return _scheme.Style.DarkTitleBar
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.DarkTitleBar = value, "DarkTitleBar")
        End Set
    End Property

    <Category("8. Stil")>
    <DisplayName("Desenează filele")>
    <Description("Antetele de filă sunt pictate de noi, cu culorile din categoria «File», în locul celor de sistem.")>
    Public Property DeseneazaFilele As Boolean
        Get
            Return _scheme.Style.OwnerDrawTabs
        End Get
        Set(value As Boolean)
            Apply(Sub() _scheme.Style.OwnerDrawTabs = value, "OwnerDrawTabs")
        End Set
    End Property

    ' ── 9. Font și spațiere ──────────────────────────────────────────────────────

    <Category("9. Font și spațiere")>
    <DisplayName("Font de bază")>
    <Description("Fontul aplicat pe formular; copiii îl moștenesc dacă n-au unul propriu. Un font lipsă de pe mașină cade elegant pe cel implicit.")>
    <TypeConverter(GetType(InstalledFontNameConverter))>
    Public Property FontDeBaza As String
        Get
            Return _scheme.Style.BaseFontName
        End Get
        Set(value As String)
            Apply(Sub() _scheme.Style.BaseFontName = value, "BaseFontName")
        End Set
    End Property

    <Category("9. Font și spațiere")>
    <DisplayName("Dimensiune font (pt)")>
    <Description("Dimensiunea fontului de bază, în puncte. 0 = nu atinge fontul formularelor.")>
    Public Property DimensiuneFont As Single
        Get
            Return _scheme.Style.BaseFontSize
        End Get
        Set(value As Single)
            If value < 0F OrElse value > 72F Then Throw New ArgumentException(
                "Dimensiunea fontului trebuie să fie între 0 (nu atinge) și 72.")
            Apply(Sub() _scheme.Style.BaseFontSize = value, "BaseFontSize")
        End Set
    End Property

    <Category("9. Font și spațiere")>
    <DisplayName("Spațiere internă")>
    <Description("Spațiul din interiorul câmpurilor și al butoanelor, în pixeli logici la 96 dpi.")>
    Public Property Spatiere As Padding
        Get
            Return _scheme.Style.ControlPadding.ToPadding()
        End Get
        Set(value As Padding)
            Apply(Sub() _scheme.Style.ControlPadding =
                      New PaddingDto(value.Left, value.Top, value.Right, value.Bottom), "ControlPadding")
        End Set
    End Property

    ''' <summary>
    ''' Frontieră de editare. O proprietate din PropertyGrid nu are voie să arunce pentru un
    ''' motiv INTERN (grila arată un dialog urât și rămâne pe o valoare inconsistentă), deci
    ''' scrierea se loghează și se înghite. Validările de mai sus, în schimb, aruncă ÎNAINTE de a
    ''' ajunge aici: acolo greșeala e a operatorului și trebuie să i se spună.
    ''' </summary>
    Private Sub Apply(action As Action, slot As String)
        Try
            action()
            If _onChanged IsNot Nothing Then _onChanged()
        Catch ex As Exception
            GlobalErrorLog.Write($"SchemeOptionsProxy.Set({slot})", ex)
        End Try
    End Sub

End Class

''' <summary>
''' Lista fonturilor instalate, ca listă derulantă în PropertyGrid. Fără el, «Font de bază» ar fi
''' o casetă de text în care numele se scrie din memorie — iar un nume greșit nu dă nicio eroare,
''' fontul pur și simplu nu se schimbă (GDI cade elegant pe cel implicit), deci greșeala ar fi
''' invizibilă. <c>IsStandardValuesExclusive</c> rămâne False: un font instalat pe mașina
''' clientului, dar nu pe asta, trebuie totuși să se poată tasta.
''' </summary>
Public NotInheritable Class InstalledFontNameConverter
    Inherits StringConverter

    Public Overrides Function GetStandardValuesSupported(context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive(context As ITypeDescriptorContext) As Boolean
        Return False
    End Function

    Public Overrides Function GetStandardValues(context As ITypeDescriptorContext) As StandardValuesCollection
        Try
            Dim names As New List(Of String)()
            For Each fam As FontFamily In FontFamily.Families
                names.Add(fam.Name)
            Next
            names.Sort(StringComparer.CurrentCultureIgnoreCase)
            Return New StandardValuesCollection(names)
        Catch ex As Exception
            ' Enumerarea fonturilor poate pica pe o mașină cu profil GDI stricat. O listă goală e
            ' un răspuns bun (caseta rămâne editabilă); o excepție ar rupe grila.
            GlobalErrorLog.Write("InstalledFontNameConverter.GetStandardValues", ex)
            Return New StandardValuesCollection(New String() {})
        End Try
    End Function

End Class
