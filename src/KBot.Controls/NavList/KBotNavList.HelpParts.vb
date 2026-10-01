Option Strict On
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the nav list as seen by the guided tours (<see cref="IKBotHelpParts"/>).
''' Parts: <c>item:&lt;Key&gt;</c> (one button, by its <see cref="KBotNavItem.Key"/>) and
''' <c>collapse</c> (the corner button). All are drawn without the mouse: no demo.
''' </summary>
Partial Public NotInheritable Class KBotNavList
    Implements IKBotHelpParts

    Private Const ItemPartPrefix As String = "item:"

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

End Class
