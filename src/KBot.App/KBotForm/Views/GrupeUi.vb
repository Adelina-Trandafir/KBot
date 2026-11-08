Option Strict On
Imports System.Collections.Generic
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Small helpers shared by the groups window (<see cref="GrupeForm"/>) and the main tree's group
''' menu (slice 0008-02): the #RRGGBB colour of a group, its colour swatch icon, the order of the
''' groups.
''' </summary>
Friend Module GrupeUi

    ''' <summary>A group colour (#RRGGBB) as a <see cref="Color"/>; black when it cannot be read.</summary>
    Friend Function HexToColor(k_hex As String) As Color
        Try
            If String.IsNullOrWhiteSpace(k_hex) Then Return Color.Black
            Dim k_text As String = k_hex.Trim()
            If k_text.Length <> 7 OrElse k_text(0) <> "#"c Then Return Color.Black
            Dim k_rgb As Integer
            If Not Integer.TryParse(k_text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, k_rgb) Then
                Return Color.Black
            End If
            Return Color.FromArgb(255, (k_rgb >> 16) And &HFF, (k_rgb >> 8) And &HFF, k_rgb And &HFF)
        Catch ex As Exception
            GlobalErrorLog.Write("GrupeUi.HexToColor", ex)
            Return Color.Black
        End Try
    End Function

    ''' <summary>The #RRGGBB text of a colour (upper case), as the server stores it.</summary>
    Friend Function ColorToHex(k_color As Color) As String
        Return $"#{k_color.R:X2}{k_color.G:X2}{k_color.B:X2}"
    End Function

    ''' <summary>
    ''' A small filled square in the group's colour, for a menu row or a tree row. The caller owns it
    ''' and disposes it when the row goes away.
    ''' </summary>
    Friend Function Swatch(k_color As Color) As Bitmap
        Dim k_bmp As New Bitmap(16, 16)
        Try
            Using k_g As Graphics = Graphics.FromImage(k_bmp)
                k_g.SmoothingMode = SmoothingMode.AntiAlias
                Using k_fill As New SolidBrush(k_color)
                    k_g.FillRectangle(k_fill, 1, 1, 13, 13)
                End Using
                Using k_pen As New Pen(Color.FromArgb(120, 0, 0, 0))
                    k_g.DrawRectangle(k_pen, 1, 1, 13, 13)
                End Using
            End Using
            Return k_bmp
        Catch ex As Exception
            k_bmp.Dispose()
            GlobalErrorLog.Write("GrupeUi.Swatch", ex)
            Throw
        End Try
    End Function

    ''' <summary>The groups the way the operator reads them: alphabetical, Romanian order, case ignored.</summary>
    Friend Function Ordonate(k_grupe As IEnumerable(Of GrupaInfo)) As List(Of GrupaInfo)
        Dim k_cmp As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)
        Return k_grupe.OrderBy(Function(k_g) k_g.Denumire, k_cmp).ThenBy(Function(k_g) k_g.IdGr).ToList()
    End Function

End Module
