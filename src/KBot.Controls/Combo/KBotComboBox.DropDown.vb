Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The list part of <see cref="KBotComboBox"/> (slice 0094): ONE window
''' (<see cref="KBotComboList"/>) for both ways of choosing. The arrow (or F4 / Alt+Down) opens it
''' with every item and the selected one highlighted; typing with <see cref="FindAsYouType"/>
''' re-fills it with the matching items only; with <see cref="OfferNewItem"/> it shows the «new
''' item» row when there is nothing else to show.
'''
''' <para>It closes on a click anywhere outside it and outside the box (a message filter watches
''' the mouse presses of the whole thread while it is open), when the field is left, and when the
''' host form moves, resizes or is deactivated.</para>
''' </summary>
Partial Public Class KBotComboBox

    Private _list As KBotComboList                ' created on first use
    Private _listHost As Form                     ' the form whose Move/Deactivate close the list
    Private _openedByArrow As Boolean = False     ' the whole list, not a search result
    Private _outsideClickFilter As OutsideClickFilter
    Private _maxDropDownItems As Integer = 8
    Private _dropDownWidth As Integer = 0
    Private _dropDownAnchorProvider As Func(Of Rectangle)

    ''' <summary>The list opened.</summary>
    <Category("K-BOT Combo")>
    <Description("The list opened.")>
    Public Event DropDown As EventHandler

    ''' <summary>The list closed.</summary>
    <Category("K-BOT Combo")>
    <Description("The list closed.")>
    Public Event DropDownClosed As EventHandler

    ''' <summary>How many rows the list shows before it scrolls.</summary>
    <Category("K-BOT Combo")>
    <Description("How many rows the list shows before it scrolls. Minimum 1.")>
    <DefaultValue(8)>
    Public Property MaxDropDownItems As Integer
        Get
            Return _maxDropDownItems
        End Get
        Set(value As Integer)
            If value < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(value), value, "MaxDropDownItems must be at least 1.")
            End If
            _maxDropDownItems = value
        End Set
    End Property

    ''' <summary>Minimum width of the list, logical px (scaled at runtime). 0 = as wide as the box.</summary>
    <Category("K-BOT Combo")>
    <Description("Minimum width of the list, px @96dpi. 0 = as wide as the box.")>
    <DefaultValue(0)>
    Public Property DropDownWidth As Integer
        Get
            Return _dropDownWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value), value, "DropDownWidth cannot be negative.")
            End If
            _dropDownWidth = value
        End Set
    End Property

    ''' <summary>
    ''' The SCREEN rectangle the list opens under, and inside which a click does not close it.
    ''' Nothing = the box itself. The grid gives the whole cell, so the list lines up with the cell
    ''' and a click on the cell's chevron toggles it instead of closing and re-opening it.
    ''' </summary>
    Friend Property DropDownAnchorProvider As Func(Of Rectangle)
        Get
            Return _dropDownAnchorProvider
        End Get
        Set(value As Func(Of Rectangle))
            _dropDownAnchorProvider = value
        End Set
    End Property

    ''' <summary>Is the list open? Setting True opens the WHOLE list; False closes it.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DroppedDown As Boolean
        Get
            Return _list IsNot Nothing AndAlso Not _list.IsDisposed AndAlso
                   _list.Visible AndAlso _list.RowCount > 0
        End Get
        Set(value As Boolean)
            Try
                If value Then
                    If Not Enabled OrElse Not Visible Then Return
                    If Not ContainsFocus Then Focus()
                    ShowFullList()
                Else
                    CloseDropDown()
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("KBotComboBox.DroppedDown", ex)
            End Try
        End Set
    End Property

    ' The box's rectangle on screen (or the one the host asked for).
    Private Function AnchorRect() As Rectangle
        If _dropDownAnchorProvider IsNot Nothing Then Return _dropDownAnchorProvider()
        Return RectangleToScreen(ClientRectangle)
    End Function

    Private Function ListMinWidth() As Integer
        Return If(_dropDownWidth > 0, ThemeShapes.ScaleDpi(Me, _dropDownWidth), 0)
    End Function

    ' The whole list, the selected item (or, while typing, the first item starting with the text)
    ' highlighted and scrolled into view. Nothing to show: the «new item» row, or nothing.
    Private Sub ShowFullList()
        If _items.Count = 0 Then
            If OffersNewItem Then
                _openedByArrow = True
                ShowNewItemRow()
            End If
            Return
        End If
        Dim highlight As Integer = _selectedIndex
        If _editable AndAlso (highlight < 0 OrElse
                              Not String.Equals(CaptionOf(_items(highlight)), Text, StringComparison.CurrentCultureIgnoreCase)) Then
            Dim exact As Integer = FindStringExact(Text)
            highlight = If(exact >= 0, exact, FirstPrefixMatch(Text))
        End If
        Dim captions As List(Of String) = AllCaptions()
        Dim rows As New List(Of KBotComboListRow)(captions.Count)
        For i As Integer = 0 To captions.Count - 1
            rows.Add(New KBotComboListRow(i, captions(i)))
        Next
        _openedByArrow = True
        ShowRows(rows, highlight)
    End Sub

    ' Only the «new item» row.
    Private Sub ShowNewItemRow()
        ShowRows(New List(Of KBotComboListRow) From {KBotComboListRow.NewItem(_offerNewItemText)}, 0)
    End Sub

    ' Opens (or re-fills) the list window. highlight = the ROW to highlight, -1 = none.
    Private Sub ShowRows(rows As List(Of KBotComboListRow), highlight As Integer)
        If Not IsHandleCreated OrElse Not Visible Then Return
        Dim wasOpen As Boolean = DroppedDown
        If _list Is Nothing OrElse _list.IsDisposed Then
            _list = New KBotComboList(Me)
            AddHandler _list.RowChosen, AddressOf List_RowChosen
            AddHandler _list.NewItemChosen, AddressOf List_NewItemChosen
        End If
        HookListHost()
        _list.ShowRows(rows, _listHost, AnchorRect(), ListMinWidth(), highlight)
        If _outsideClickFilter Is Nothing Then
            _outsideClickFilter = New OutsideClickFilter(Me)
            Application.AddMessageFilter(_outsideClickFilter)
        End If
        If Not wasOpen Then
            Invalidate()
            RaiseEvent DropDown(Me, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>Closes the list, if it is open.</summary>
    Private Sub CloseDropDown()
        Try
            _openedByArrow = False
            If _outsideClickFilter IsNot Nothing Then
                Application.RemoveMessageFilter(_outsideClickFilter)
                _outsideClickFilter = Nothing
            End If
            If _list Is Nothing OrElse _list.IsDisposed OrElse Not _list.Visible Then Return
            _list.Hide()
            Invalidate()
            RaiseEvent DropDownClosed(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.CloseDropDown", ex)
        End Try
    End Sub

    ' The list is placed in screen coordinates: when the box moves (a grid scrolling its editor,
    ' a layout pass), the list follows.
    Private Sub RepositionDropDown()
        Try
            If Not DroppedDown Then Return
            _list.Reposition(AnchorRect(), ListMinWidth())
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.RepositionDropDown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' After every edit the operator makes: with <see cref="FindAsYouType"/> and enough typed,
    ''' the list shows the matching rows; nothing matches and <see cref="OfferNewItem"/> applies =
    ''' the «new item» row. Otherwise a list opened by the arrow stays open with the first item
    ''' starting with the text highlighted, and a search result closes.
    ''' </summary>
    Private Sub AfterOperatorEdit()
        Try
            If Not _editable Then Return
            Dim typed As String = Text
            If SignificantLength() >= _findAfterNChars AndAlso (_findAsYouType OrElse OffersNewItem) Then
                Dim captions As List(Of String) = AllCaptions()
                Dim hits As List(Of Integer) = FindMatches(captions, typed, _findFirstGroupCount)
                If hits.Count = 0 Then
                    If OffersNewItem Then ShowNewItemRow() Else CloseDropDown()
                    Return
                End If
                If _findAsYouType Then
                    Dim rows As New List(Of KBotComboListRow)(hits.Count)
                    For Each i As Integer In hits
                        rows.Add(New KBotComboListRow(i, captions(i)))
                    Next
                    Dim keepArrow As Boolean = _openedByArrow
                    ShowRows(rows, 0)
                    _openedByArrow = keepArrow
                    Return
                End If
            End If
            If _openedByArrow AndAlso DroppedDown Then
                ShowFullList()
            Else
                CloseDropDown()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.AfterOperatorEdit", ex)
        End Try
    End Sub

    Private Sub List_RowChosen(itemIndex As Integer)
        Try
            AcceptRow(itemIndex)
            If _editable Then _edit.Focus() Else Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.List_RowChosen", ex)
        End Try
    End Sub

    Private Sub List_NewItemChosen()
        Try
            RequestNewItem()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.List_NewItemChosen", ex)
        End Try
    End Sub

    ' The form that owns the list window; its Move/Resize/Deactivate close the list.
    Private Sub HookListHost()
        Dim host As Form = TryCast(TopLevelControl, Form)
        If ReferenceEquals(host, _listHost) Then Return
        UnhookListHost()
        _listHost = host
        If _listHost Is Nothing Then Return
        AddHandler _listHost.Move, AddressOf ListHost_Changed
        AddHandler _listHost.Resize, AddressOf ListHost_Changed
        AddHandler _listHost.Deactivate, AddressOf ListHost_Changed
    End Sub

    Private Sub UnhookListHost()
        If _listHost Is Nothing Then Return
        RemoveHandler _listHost.Move, AddressOf ListHost_Changed
        RemoveHandler _listHost.Resize, AddressOf ListHost_Changed
        RemoveHandler _listHost.Deactivate, AddressOf ListHost_Changed
        _listHost = Nothing
    End Sub

    Private Sub ListHost_Changed(sender As Object, e As EventArgs)
        ' Slice 0000-31: held open (help capture tool); closes when the guard is released.
        If KBot.Theming.KBotPopupGuard.KeepOpen Then
            KBot.Theming.KBotPopupGuard.CloseOnRelease(AddressOf CloseDropDown)
            Return
        End If
        CloseDropDown()
    End Sub

    ''' <summary>
    ''' A mouse press somewhere in the application while the list is open. On the list, on the box
    ''' or inside the anchor (the grid's cell) it is theirs; anywhere else it closes the list and
    ''' goes on to wherever it was aimed.
    ''' </summary>
    Private Sub OnMousePressAnywhere(hwnd As IntPtr)
        Try
            If Not DroppedDown Then Return
            Dim target As Control = Control.FromChildHandle(hwnd)
            If target IsNot Nothing Then
                If ReferenceEquals(target, _list) OrElse _list.Contains(target) Then Return
                If ReferenceEquals(target, Me) OrElse Contains(target) Then Return
            End If
            If _dropDownAnchorProvider IsNot Nothing AndAlso AnchorRect().Contains(MousePosition) Then Return
            If KBot.Theming.KBotPopupGuard.KeepOpen Then
                KBot.Theming.KBotPopupGuard.CloseOnRelease(AddressOf CloseDropDown)
                Return
            End If
            CloseDropDown()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnMousePressAnywhere", ex)
        End Try
    End Sub

    ''' <summary>Watches the thread's mouse presses while the list is open. Never eats one.</summary>
    Private NotInheritable Class OutsideClickFilter
        Implements IMessageFilter

        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_RBUTTONDOWN As Integer = &H204
        Private Const WM_MBUTTONDOWN As Integer = &H207
        Private Const WM_NCLBUTTONDOWN As Integer = &HA1
        Private Const WM_NCRBUTTONDOWN As Integer = &HA4
        Private Const WM_NCMBUTTONDOWN As Integer = &HA7

        Private ReadOnly _owner As KBotComboBox

        Public Sub New(owner As KBotComboBox)
            _owner = owner
        End Sub

        Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Select Case m.Msg
                Case WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN,
                     WM_NCLBUTTONDOWN, WM_NCRBUTTONDOWN, WM_NCMBUTTONDOWN
                    _owner.OnMousePressAnywhere(m.HWnd)
            End Select
            Return False
        End Function
    End Class

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        If Not Visible Then CloseDropDown()
    End Sub

    Protected Overrides Sub OnParentChanged(e As EventArgs)
        MyBase.OnParentChanged(e)
        CloseDropDown()
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        CloseDropDown()
        MyBase.OnHandleDestroyed(e)
    End Sub

    ''' <summary>The list is a window of its own: it goes with the box.</summary>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                CloseDropDown()
                UnhookListHost()
                If _list IsNot Nothing Then
                    RemoveHandler _list.RowChosen, AddressOf List_RowChosen
                    RemoveHandler _list.NewItemChosen, AddressOf List_NewItemChosen
                    _list.Dispose()
                    _list = Nothing
                End If
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
