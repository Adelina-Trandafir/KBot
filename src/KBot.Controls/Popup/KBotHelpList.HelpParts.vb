Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' Slice 000T-07: the popup's list as seen by the help. One part, <c>tutorials</c>: the header row
''' whose <see cref="KBotHelpRow.Key"/> is «tutorials» and the tutorial rows under it. It is drawn
''' without the mouse (no demo); it is Empty while the list does not hold that block (a search is
''' showing, or the login has no tutorials).
''' </summary>
Partial Public NotInheritable Class KBotHelpList
    Implements IKBotHelpParts

    ''' <summary>The key of the header row that opens the block of tutorials.</summary>
    Public Const PartTutorials As String = "tutorials"

    ''' <summary>
    ''' Where <paramref name="k_part"/> is, in client coordinates. A block that is scrolled out of view is
    ''' brought into view first (as much of it as fits), so the ring never points at nothing.
    ''' </summary>
    Public Function HelpPartBounds(k_part As String) As Rectangle Implements IKBotHelpParts.HelpPartBounds
        CheckPart(k_part)
        Dim k_first As Integer = _slots.FindIndex(Function(k_slot) k_slot.Row.Kind = KBotHelpRowKind.Header AndAlso
                                                                    String.Equals(k_slot.Row.Key, PartTutorials, StringComparison.Ordinal))
        If k_first < 0 Then Return Rectangle.Empty
        Dim k_last As Integer = k_first
        While k_last + 1 < _slots.Count AndAlso _slots(k_last + 1).Row.Kind = KBotHelpRowKind.Tour
            k_last += 1
        End While
        Dim k_top As Integer = _slots(k_first).Bounds.Top
        Dim k_bottom As Integer = _slots(k_last).Bounds.Bottom
        If _barOn Then
            If k_bottom - k_top > ClientSize.Height OrElse k_top < _scroll.Value Then
                _scroll.Value = k_top
            ElseIf k_bottom > _scroll.Value + ClientSize.Height Then
                _scroll.Value = k_bottom - ClientSize.Height
            End If
        End If
        Dim k_block As New Rectangle(0, k_top - Offset, ContentWidth(), k_bottom - k_top)
        k_block.Intersect(ClientRectangle)
        Return If(k_block.Width > 0 AndAlso k_block.Height > 0, k_block, Rectangle.Empty)
    End Function

    Public Sub SetHelpPartDemo(k_part As String, k_show As Boolean) Implements IKBotHelpParts.SetHelpPartDemo
        CheckPart(k_part)
    End Sub

    Private Shared Sub CheckPart(k_part As String)
        If Not String.Equals(k_part, PartTutorials, StringComparison.Ordinal) Then
            Throw New ArgumentException("Unknown help list part '" & k_part & "'.", NameOf(k_part))
        End If
    End Sub

End Class
