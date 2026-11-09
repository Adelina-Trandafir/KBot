Option Strict On
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0126-04: the frame of the trees of the «Clasificatii» window (both pages). A tree has no frame of its own by default (its
''' <c>BorderColor</c> is «Transparent»), while the grids next to it have one in the colour of their band lines, so the trees looked
''' borderless. This gives them the SAME frame colour the grids resolve (<c>KBotDataView.BorderColorResolved</c> = the palette's
''' <c>BorderColor</c>). Written from the palette at every theme change, so it follows the scheme.
''' </summary>
Friend NotInheritable Class ClasificatiiTreeStyle

    Private Sub New()
    End Sub

    ''' <summary>Puts the frame on the tree, in the colour the grids of this scheme use for their frame.</summary>
    Friend Shared Sub ApplyFrame(k_tree As AdvancedTreeControl, k_scheme As ThemeScheme)
        ArgumentNullException.ThrowIfNull(k_tree)
        If k_scheme Is Nothing Then Return
        k_tree.BorderColor = k_scheme.OuterBorder(k_scheme.Palette.BorderColor)   ' SLICE-0130
    End Sub

End Class
