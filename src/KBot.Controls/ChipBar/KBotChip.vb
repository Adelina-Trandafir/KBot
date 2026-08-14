Option Strict On
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' Un jeton («chip») al lui <see cref="KBotChipBar"/> — fratele MULTI-SELECT al lui
''' <c>KBotNavItem</c>: cheie, text, o pastilă de număr opțională, <c>Enabled</c>, <c>Visible</c>
''' și, spre deosebire de o navigație, o stare <see cref="Checked"/> proprie fiecărui jeton.
'''
''' Nu există separator aici (bara e o linie de jetoane, nu o coloană de butoane grupate), deci
''' tipul e simplu: un jeton e mereu un jeton.
'''
''' <see cref="AccentOverride"/> există pentru un singur motiv concret: jetonul ERORI trebuie să
''' fie roșu și cel de AVERTISMENTE chihlimbariu. <b>Culoarea o dă APELANTUL</b>
''' (<c>Palette.ErrorColor</c> / <c>Palette.WarningColor</c>) — controlul nu numește nicio culoare,
''' exact ca restul casei, iar la schimbarea schemei apelantul o re-dă (vezi
''' <c>LogViewerForm.OnThemeChanged</c>).
''' </summary>
Public NotInheritable Class KBotChip

    ''' <summary>Constructor fără parametri — cerut de dialogul de colecție al designer-ului.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Comoditate pentru cod: un jeton nebifat.</summary>
    Public Sub New(key As String, text As String)
        Me.New(key, text, False)
    End Sub

    ''' <summary>Comoditate pentru cod: jeton cu starea de bifare dată.</summary>
    Public Sub New(key As String, text As String, checked As Boolean)
        ' «Me.» e OBLIGATORIU: VB e case-insensitive, deci parametrii umbresc proprietățile, iar o
        ' atribuire necalificată ar scrie parametrul în el însuși (capcana din feliile 0010 / 0019).
        Me.Key = key
        Me.Text = If(text, String.Empty)
        Me.Checked = checked
    End Sub

    <Category("K-BOT")>
    <Description("Identificatorul folosit de SetChecked / IsChecked / SetBadge / SetChipEnabled / SetChipVisible. Trebuie să fie nevid și unic.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Textul scris pe jeton.")>
    Public Property Text As String

    <Category("K-BOT")>
    <Description("True => jetonul e bifat (desenat pe accent). Bara e multi-select: oricâte jetoane pot fi bifate deodată.")>
    <DefaultValue(False)>
    Public Property Checked As Boolean

    <Category("K-BOT")>
    <Description("Numărul din pastila din dreapta textului. 0 = pastila nu se desenează.")>
    <DefaultValue(0)>
    Public Property Count As Integer

    <Category("K-BOT")>
    <Description("False => jetonul e desenat șters și nu se poate bifa (dar ocupă spațiu).")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("False => jetonul nu ocupă spațiu, nu se pictează, nu se poate bifa și e sărit de navigarea cu tastatura.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    ''' <summary>
    ''' Culoarea de fundal a jetonului BIFAT, când nu e cea a schemei. <c>Color.Empty</c> (implicit)
    ''' = accentul schemei active.
    '''
    ''' <b>Controlul nu numește nicio culoare</b>: valoarea vine de la apelant, din paletă
    ''' (<c>Palette.ErrorColor</c> pentru jetonul de erori, <c>Palette.WarningColor</c> pentru
    ''' avertismente). Deci la comutarea schemei apelantul trebuie s-o RE-DEA — o culoare pusă azi
    ''' rămâne pusă, iar bara nu o poate deriva singură.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Fundalul jetonului bifat, dat de apelant din paletă (ex. ErrorColor). Gol = accentul schemei. Se re-dă la schimbarea temei.")>
    Public Property AccentOverride As Color = Color.Empty

    ' Ca la KBotNavItem.Image: un tip fără <DefaultValue> utilizabil își spune «nesetat» prin
    ' ShouldSerialize/Reset, altfel designer-ul scrie «KBotChipN.AccentOverride = Color.Empty» pe
    ' fiecare jeton care n-a fost atins. Private dinadins — TypeDescriptor le găsește după nume,
    ' inclusiv nepublice, și nu fac parte din API-ul jetonului.
    Private Function ShouldSerializeAccentOverride() As Boolean
        Return AccentOverride <> Color.Empty
    End Function

    Private Sub ResetAccentOverride()
        AccentOverride = Color.Empty
    End Sub

    ''' <summary>
    ''' Slotul calculat de <c>KBotChipBar.RecalcLayout</c> (<see cref="Rectangle.Empty"/> când
    ''' jetonul e ascuns). FRIEND, nu Public: e stare derivată, recalculată la fiecare așezare —
    ''' designer-ul n-are voie s-o vadă și cu atât mai puțin s-o serializeze (ca
    ''' <c>KBotNavItem.Bounds</c>).
    ''' </summary>
    Friend Property Bounds As Rectangle

    ''' <summary>Ce se vede în lista dialogului de colecție — cheia, textul și starea de bifare.</summary>
    Public Overrides Function ToString() As String
        Dim shownKey As String = If(String.IsNullOrWhiteSpace(Key), "<fără cheie>", Key)
        Return shownKey & " — """ & If(Text, String.Empty) & """" & If(Checked, " [✓]", String.Empty)
    End Function

End Class
