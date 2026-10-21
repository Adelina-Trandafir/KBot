Option Strict On

''' <summary>
''' An item of <see cref="KBotComboBox"/> that belongs to a GROUP (slice 00EF-46, the accounts of Vercon under their gestiune). The list shows one
''' collapsible header row per group, in the order the groups first appear, and the items of the group under it. The item itself stays a plain
''' selectable item: <see cref="KBotComboBox.SelectedIndex"/>, <c>FindStringExact</c>, typing and the keys work on items only; the headers exist in the
''' LIST window, not in <see cref="KBotComboBox.Items"/>.
''' </summary>
Public Interface IKBotComboGroupedItem

    ''' <summary>The group this item is listed under (the header's text). Empty = no group: the item is listed first, plain.</summary>
    ReadOnly Property GroupCaption As String

    ''' <summary>What the row shows UNDER its header (shorter than the face text, because the header already says the group).
    ''' Empty = the same as the face text.</summary>
    ReadOnly Property RowCaption As String
End Interface
