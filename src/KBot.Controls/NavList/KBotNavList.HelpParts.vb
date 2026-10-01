Option Strict On
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the nav list as seen by the guided tours (<see cref="IKBotHelpParts"/>).
''' Parts: <c>item:&lt;Key&gt;</c> (one button, by its <see cref="KBotNavItem.Key"/>) and
''' <c>collapse</c> (the corner button). All are drawn without the mouse: no demo.
''' </summary>
Partial Public NotInheritable Class KBotNavList
    Implements IKBotHelpParts, IKBotHelpReveal

    Private Const ItemPartPrefix As String = "item:"

    ' Slice 0000-30: buttons the tour showed although the app hides them (no data, no connection).
    Private ReadOnly _helpRevealed As New List(Of KBotNavItem)()

    Public Function HelpPartBounds(part As String) As Rectangle Implements IKBotHelpParts.HelpPartBounds
        If String.Equals(part, "collapse", StringComparison.Ordinal) Then Return CollapseButtonRect()
        If part Is Nothing OrElse Not part.StartsWith(ItemPartPrefix, StringComparison.Ordinal) Then
            Throw New ArgumentException("Unknown nav list help part '" & part & "'.", NameOf(part))
        End If
        Dim key As String = part.Substring(ItemPartPrefix.Length)
        Dim item As KBotNavItem = _items.FirstOrDefault(Function(it) String.Equals(it.Key, key, StringComparison.OrdinalIgnoreCase))
        If item Is Nothing Then Throw New ArgumentException("The nav list '" & Name & "' has no item '" & key & "'.", NameOf(part))
        If Not item.Visible OrElse item.IsSeparator Then Return Rectangle.Empty
        If Not _layoutValid Then RecalcLayout()
        Return item.Bounds
    End Function

    Public Sub SetHelpPartDemo(part As String, show As Boolean) Implements IKBotHelpParts.SetHelpPartDemo
        HelpPartBounds(part)   ' validates the name; every part is always drawn
    End Sub

    ''' <summary>
    ''' Slice 0000-30: a button the app hides now (the angajament has no such data, FOREXE is not
    ''' connected, advanced options are off) is shown until <see cref="HelpRevealEnd"/>.
    ''' </summary>
    Public Function HelpReveal(target As Control, part As String, ByRef area As Rectangle, ByRef note As String) As Boolean Implements IKBotHelpReveal.HelpReveal
        If Not ReferenceEquals(target, Me) OrElse part Is Nothing OrElse
           Not part.StartsWith(ItemPartPrefix, StringComparison.Ordinal) Then Return False
        Dim key As String = part.Substring(ItemPartPrefix.Length)
        Dim item As KBotNavItem = _items.FirstOrDefault(Function(it) String.Equals(it.Key, key, StringComparison.OrdinalIgnoreCase))
        If item Is Nothing OrElse item.IsSeparator OrElse item.Visible Then Return False
        item.Visible = True
        _helpRevealed.Add(item)
        InvalidateLayout()
        Return True
    End Function

    Public Sub HelpRevealEnd() Implements IKBotHelpReveal.HelpRevealEnd
        If _helpRevealed.Count = 0 Then Return
        For Each item As KBotNavItem In _helpRevealed
            item.Visible = False
        Next
        _helpRevealed.Clear()
        InvalidateLayout()
    End Sub

End Class
