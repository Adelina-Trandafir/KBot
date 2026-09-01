Option Strict On
Imports System.Drawing

''' <summary>
''' THE automatic colour set of K-BOT — the sequence a series, a point or a lane is drawn in when
''' nobody gave it a colour.
''' </summary>
''' <remarks>
''' <para><b>Why it is a module and not a method on one control.</b> Two surfaces show the SAME
''' facts side by side: a chart of how a value moved, and lanes of where those values were placed.
''' A mark and the line it belongs to have to carry the same colour, or the operator cannot pair
''' them by eye — which is the one job colour does here that nothing else can do. Two independent
''' implementations of "the n-th colour" would agree on the day they were written and drift on the
''' first theme change. So there is one.</para>
'''
''' <para><b>Never red.</b> Red is what this application spends on something being wrong, so a set
''' that hands it out by turn teaches the operator to stop reading it. The hues live strictly
''' between <see cref="HueFirst"/> and <see cref="HueLast"/>: the red wedge around zero is not
''' "skipped over", it is outside the range, so nothing can land there whatever the accent of the
''' active scheme happens to be. A host that means "this one is bad" writes the colour itself,
''' which is the only way red should ever reach these controls.</para>
'''
''' <para><b>Why not the palette's accents.</b> The first version handed out the five accents of
''' the scheme and then repeated them a step lighter. At four or five lines that reads fine; at the
''' dozen a real commitment has, "a step lighter" is not a difference an eye can hold, and two of
''' them looked like one. So the HUE is what moves, by the golden fraction of the range per step:
''' consecutive indexes land as far apart as a sequence can put them, and they keep landing far
''' apart however long the list gets — that is the property this particular irrational number is
''' picked for. Lightness alternates in three steps on top of it, so even two indexes that
''' eventually come back to a similar hue still differ in weight.</para>
'''
''' <para>Nothing here is a hardcoded colour: the starting hue, the saturation and the middle
''' lightness are read off the ACTIVE scheme's accent, so the whole set turns with the theme. The
''' literals are limits — how dark is still readable on this background — which is the same kind of
''' number <c>ThemeShapes.Lighten</c> already takes.</para>
'''
''' <para>Any index is accepted, negative ones included, so a caller never has to bound-check a
''' loop.</para>
''' </remarks>
Friend Module KBotAutoPalette

    ''' <summary>First hue of the set, just past the red wedge.</summary>
    Private Const HueFirst As Double = 30.0

    ''' <summary>Last hue of the set, just before the red wedge starts again.</summary>
    Private Const HueLast As Double = 330.0

    ''' <summary>
    ''' The step, as a fraction of the hue range: the golden ratio, chosen because it is the number
    ''' a sequence stays furthest from repeating with.
    ''' </summary>
    Private Const GoldenStep As Double = 0.6180339887498949

    ''' <summary>
    ''' The <paramref name="index"/>-th colour of the set, derived from the scheme's
    ''' <paramref name="accent"/>. <paramref name="isDark"/> lifts the whole set so a mid-dark
    ''' line does not sink into a dark plot.
    ''' </summary>
    Friend Function ColorAt(accent As Color, isDark As Boolean, index As Integer) As Color
        Dim i As Integer = Math.Abs(index)

        ' Where in the allowed arc the active accent sits: the set starts from the scheme's own
        ' colour instead of from a fixed hue, and walks on from there.
        Dim anchor As Double = Math.Min(HueLast, Math.Max(HueFirst, CDbl(accent.GetHue())))
        Dim span As Double = HueLast - HueFirst
        Dim t As Double = (anchor - HueFirst) / span + i * GoldenStep
        t -= Math.Floor(t)
        Dim hue As Double = HueFirst + t * span

        Dim sat As Double = Math.Min(0.85, Math.Max(0.55, CDbl(accent.GetSaturation())))
        Dim middle As Double = CDbl(accent.GetBrightness())
        If isDark Then
            middle = Math.Min(0.72, Math.Max(0.58, middle + 0.12))
        Else
            middle = Math.Min(0.52, Math.Max(0.34, middle))
        End If
        Dim steps() As Double = {0.0, -0.1, 0.1}
        Dim light As Double = Math.Min(0.82, Math.Max(0.24, middle + steps(i Mod steps.Length)))

        Return FromHsl(hue, sat, light)
    End Function

    ''' <summary>
    ''' Hue (0..360), saturation and lightness (0..1) back to a colour. Deliberately HSL — the same
    ''' space <see cref="Color.GetHue"/>, <c>GetSaturation</c> and <c>GetBrightness</c> report in,
    ''' so a palette colour taken apart by those and put together again here comes out unchanged.
    ''' </summary>
    Friend Function FromHsl(hue As Double, sat As Double, light As Double) As Color
        Dim chroma As Double = (1.0 - Math.Abs(2.0 * light - 1.0)) * sat
        Dim turn As Double = ((hue Mod 360.0) + 360.0) Mod 360.0
        Dim sector As Double = turn / 60.0
        Dim second As Double = chroma * (1.0 - Math.Abs((sector Mod 2.0) - 1.0))
        Dim bottom As Double = light - chroma / 2.0
        Dim r, g, b As Double
        Select Case CInt(Math.Floor(sector))
            Case 0
                r = chroma : g = second : b = 0.0
            Case 1
                r = second : g = chroma : b = 0.0
            Case 2
                r = 0.0 : g = chroma : b = second
            Case 3
                r = 0.0 : g = second : b = chroma
            Case 4
                r = second : g = 0.0 : b = chroma
            Case Else
                r = chroma : g = 0.0 : b = second
        End Select
        Return Color.FromArgb(Channel(r + bottom), Channel(g + bottom), Channel(b + bottom))
    End Function

    ''' <summary>One 0..1 channel to its 0..255 byte, clamped.</summary>
    Private Function Channel(value As Double) As Integer
        Return Math.Min(255, Math.Max(0, CInt(Math.Round(value * 255.0))))
    End Function
End Module
