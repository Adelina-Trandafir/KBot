Option Strict On
Imports System.Drawing
Imports System.Text.Json.Serialization

''' <summary>
''' O suprascriere de stil pentru UN control, identificat prin <see cref="Path"/>.
'''
''' Contractul e „totul e opțional”: fiecare slot e <c>Nothing</c> (sau 0, pentru mărimea
''' fontului) cât timp operatorul nu l-a atins. Un slot neatins NU se scrie în JSON și NU se
''' aplică la rulare — asta separă «am ales negru» de «n-am ales nimic», exact distincția pe care
''' o face și <c>Color.Empty</c> în controalele K-BOT. Culorile se stochează hex ("#RRGGBB"),
''' la fel ca în <see cref="ThemePalette"/>, ca fișierul să rămână citibil de om.
'''
''' <see cref="HoverColor"/>, <see cref="BorderColor"/>, <see cref="AccentColor"/> și perechea
''' de selecție NU sunt proprietăți de <c>Control</c>: se aplică doar controalelor K-BOT care
''' expun o proprietate publică cu același nume (potrivire prin reflexie — vezi
''' <c>ControlStyleProxy</c>). Pe un control obișnuit rămân în fișier fără efect, ceea ce e de
''' preferat pierderii lor: un buton system de azi poate deveni un KBot* mâine.
''' </summary>
Public NotInheritable Class ControlStyleOverride

    ''' <summary>Calea în ierarhia gazdei, ex. „pnlRoot/pnlHeader/cboAn”. Cheia de potrivire.</summary>
    Public Property Path As String = String.Empty

    ''' <summary>Numele tipului controlului la momentul salvării — diagnostic, nu criteriu de potrivire.</summary>
    Public Property TypeName As String = String.Empty

    ' ── Culori (hex "#RRGGBB"; Nothing = neatinsă) ────────────────────────────
    Public Property BackColor As String = Nothing
    Public Property ForeColor As String = Nothing
    Public Property HoverColor As String = Nothing
    Public Property BorderColor As String = Nothing
    Public Property AccentColor As String = Nothing
    Public Property SelectionBackColor As String = Nothing
    Public Property SelectionForeColor As String = Nothing

    ' ── Font (numele gol / mărimea 0 = neatins) ───────────────────────────────
    Public Property FontName As String = Nothing
    Public Property FontSize As Single = 0F
    Public Property FontStyle As String = Nothing

    ''' <summary>Nu conține nicio alegere — se poate omite complet din fișier.</summary>
    <JsonIgnore>
    Public ReadOnly Property IsEmpty As Boolean
        Get
            Return String.IsNullOrWhiteSpace(BackColor) AndAlso
                   String.IsNullOrWhiteSpace(ForeColor) AndAlso
                   String.IsNullOrWhiteSpace(HoverColor) AndAlso
                   String.IsNullOrWhiteSpace(BorderColor) AndAlso
                   String.IsNullOrWhiteSpace(AccentColor) AndAlso
                   String.IsNullOrWhiteSpace(SelectionBackColor) AndAlso
                   String.IsNullOrWhiteSpace(SelectionForeColor) AndAlso
                   Not HasFont
        End Get
    End Property

    ''' <summary>Fontul e considerat ales doar dacă are ȘI nume, ȘI mărime pozitivă.</summary>
    <JsonIgnore>
    Public ReadOnly Property HasFont As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(FontName) AndAlso FontSize > 0F
        End Get
    End Property

    ''' <summary>Fontul reconstruit, sau Nothing dacă nu a fost ales. Un nume invalid dă Nothing.</summary>
    Public Function ToFont() As Font
        If Not HasFont Then Return Nothing
        Try
            Return New Font(FontName, FontSize, ParseStyle(FontStyle))
        Catch
            ' Font indisponibil pe mașina asta: mai bine niciun font decât o excepție la pictare.
            Return Nothing
        End Try
    End Function

    ''' <summary>Scrie fontul în cele trei sloturi (Nothing = șterge alegerea).</summary>
    Public Sub SetFont(f As Font)
        If f Is Nothing Then
            FontName = Nothing
            FontSize = 0F
            FontStyle = Nothing
            Return
        End If
        FontName = f.FontFamily.Name
        FontSize = f.Size
        FontStyle = f.Style.ToString()
    End Sub

    ''' <summary>Citește un slot de culoare; <c>Color.Empty</c> dacă nu a fost ales sau e invalid.</summary>
    Public Shared Function ToColor(hex As String) As Color
        If String.IsNullOrWhiteSpace(hex) Then Return Color.Empty
        Try
            Return ColorHex.FromHex(hex)
        Catch
            Return Color.Empty
        End Try
    End Function

    ''' <summary>Scrie un slot de culoare; <c>Color.Empty</c> șterge alegerea (Nothing).</summary>
    Public Shared Function FromColor(c As Color) As String
        If c = Color.Empty Then Return Nothing
        Return ColorHex.ToHex(c)
    End Function

    Private Shared Function ParseStyle(value As String) As FontStyle
        If String.IsNullOrWhiteSpace(value) Then Return Drawing.FontStyle.Regular
        Dim parsed As Object = Nothing
        If [Enum].TryParse(GetType(Drawing.FontStyle), value, True, parsed) AndAlso parsed IsNot Nothing Then
            Return CType(parsed, Drawing.FontStyle)
        End If
        Return Drawing.FontStyle.Regular
    End Function

End Class
