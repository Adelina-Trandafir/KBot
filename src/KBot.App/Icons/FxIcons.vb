Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Text
Imports KBot.Forexe   ' KBotTheme.IsDark

<Assembly: InternalsVisibleTo("KBot.App.Tests")>   ' testarea mapării pure StatusIconName

''' <summary>
''' Mapează starea unui angajament la iconița de status și oferă iconița de refresh,
''' oglindind mdl_FX_PopulareTree.Angajament_Iconita_Dreapta. Iconițele vin din
''' resursele embedded (My Project\Resources.resx, la 24px pentru lista MainForm).
''' Fără no-op tăcut: o iconiță lipsă din resurse ridică o excepție clară.
''' </summary>
Public NotInheritable Class FxIcons

    Private Sub New()
    End Sub

    ' Cache pe nume de resursă — imaginile embedded sunt imutabile și partajate.
    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ' Perechea (substring căutat în Stare, numele de bază al iconiței). Ordinea CONTEAZĂ:
    ' primul match câștigă (identic cu Select Case True din VBA). "definitivare" e după
    ' "initial" ca în legacy.
    Private Shared ReadOnly _map As (Token As String, Icon As String)() = {
        ("derulare", "FX_GREEN"),
        ("anulat", "FX_RED"),
        ("reziliat", "FX_RED"),
        ("suspendat", "FX_ORANGE"),
        ("initial", "FX_GRAY"),
        ("arhivat", "FX_BLUE"),
        ("manual", "FX_WHITE"),
        ("definitivare", "FX_BLUE")
    }

    ''' <summary>
    ''' Iconița de status (24px) pentru o valoare Stare. Match pe substring,
    ''' case-insensitive ȘI diacritic-insensitive ("În derulare" -> derulare -> FX_GREEN).
    ''' Fallback (necunoscut / gol) -> FX_GRAY.
    ''' </summary>
    Public Shared Function StatusIcon(stare As String) As Image
        Return Load(StatusIconName(stare) & "_24")
    End Function

    ''' <summary>
    ''' Numele de bază al iconiței de status (fără sufixul de dimensiune) pentru o Stare —
    ''' maparea pură (fără resurse), expusă pentru testare. Fallback -> "FX_GRAY".
    ''' </summary>
    Friend Shared Function StatusIconName(stare As String) As String
        Dim norm As String = Normalize(stare)
        For Each pair In _map
            If norm.Contains(pair.Token) Then Return pair.Icon
        Next
        Return "FX_GRAY"
    End Function

    ''' <summary>Iconița de refresh (24px), variantă adaptată temei active (clar/închis).</summary>
    Public Shared Function RefreshIcon() As Image
        Return Load(If(KBotTheme.IsDark, "FX_REFRESH_DARK_24", "FX_REFRESH_24"))
    End Function

    ' Lower-invariant + strip diacritice (FormD -> elimină semnele non-spacing).
    Private Shared Function Normalize(value As String) As String
        If String.IsNullOrEmpty(value) Then Return String.Empty
        Dim decomposed As String = value.Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(decomposed.Length)
        For Each ch As Char In decomposed
            If CharUnicodeInfo.GetUnicodeCategory(ch) <> UnicodeCategory.NonSpacingMark Then
                sb.Append(ch)
            End If
        Next
        Return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant()
    End Function

    ' Încarcă (o singură dată) o imagine embedded după numele resursei. Ridică
    ' excepție dacă lipsește — resursele sunt un invariant de build, nu date runtime.
    Private Shared Function Load(resourceName As String) As Image
        SyncLock _sync
            Dim cached As Image = Nothing
            If _cache.TryGetValue(resourceName, cached) Then Return cached

            Dim obj As Object = My.Resources.ResourceManager.GetObject(resourceName, My.Resources.Culture)
            Dim img As Image = TryCast(obj, Image)
            If img Is Nothing Then
                Throw New InvalidOperationException(
                    $"Iconița «{resourceName}» lipsește din resursele embedded (My Project\Resources.resx).")
            End If
            _cache(resourceName) = img
            Return img
        End SyncLock
    End Function

End Class
