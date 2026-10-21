Option Strict On

''' <summary>
''' A ready-made <see cref="IKBotComboGroupedItem"/>: the face text (<see cref="Text"/>, what <c>ToString()</c> and the typed search use), the group, the shorter
''' text of its row and any value the host wants back (<see cref="Tag"/>, e.g. an account id). Two items are equal when group, text and tag are.
''' </summary>
Public NotInheritable Class KBotComboGroupedItem
    Implements IKBotComboGroupedItem

    Public Sub New(k_text As String, k_group As String, k_rowText As String, k_tag As Object)
        Me.Text = If(k_text, String.Empty)
        Me.GroupCaption = If(k_group, String.Empty)
        Me.RowCaption = If(k_rowText, String.Empty)
        Me.Tag = k_tag
    End Sub

    ''' <summary>What the face shows and the typed search matches.</summary>
    Public ReadOnly Property Text As String

    ''' <summary>The header the item is listed under.</summary>
    Public ReadOnly Property GroupCaption As String Implements IKBotComboGroupedItem.GroupCaption

    ''' <summary>What the row shows under its header; empty = <see cref="Text"/>.</summary>
    Public ReadOnly Property RowCaption As String Implements IKBotComboGroupedItem.RowCaption

    ''' <summary>The host's value for this item.</summary>
    Public ReadOnly Property Tag As Object

    Public Overrides Function ToString() As String
        Return Text
    End Function

    Public Overrides Function Equals(k_other As Object) As Boolean
        Dim k_item As KBotComboGroupedItem = TryCast(k_other, KBotComboGroupedItem)
        If k_item Is Nothing Then Return False
        Return String.Equals(Text, k_item.Text, StringComparison.Ordinal) AndAlso
               String.Equals(GroupCaption, k_item.GroupCaption, StringComparison.Ordinal) AndAlso
               Object.Equals(Tag, k_item.Tag)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return HashCode.Combine(Text, GroupCaption, Tag)
    End Function
End Class
