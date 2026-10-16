Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-34 -- the INPUT rows of a <see cref="CustomPopup"/>: a <b>combo</b> (a list to pick from, no typing;
''' <see cref="CustomPopupItem.Combo"/>) and a <b>text box</b> (<see cref="CustomPopupItem.TextInput"/>). A row of that kind is a
''' caption line with the real KBot control under it, so the popup stays one window and one theme. Picking a row of the combo, or
''' Enter in the text box, closes the popup and raises <see cref="ItemClicked"/> for THAT item, which then carries the answer
''' (<see cref="CustomPopupItem.ComboSelectedIndex"/> / <see cref="CustomPopupItem.InputText"/>) -- the same way a row is chosen.
'''
''' <para>Keys: while an input has the focus every key is the input's own (letters, digits, arrows), except Esc, which closes the
''' popup as always. An input row is never highlighted and has no access letter.</para>
''' </summary>
Partial Public Class CustomPopup

    Private Const InputMinWidthLogical As Integer = 190
    Private Const InputGapLogical As Integer = 4

    Private ReadOnly _inputControls As New Dictionary(Of CustomPopupItem, Control)()
    Private _fixingInput As Boolean

    ' Makes the real controls of the input items (once each; not at design time, nothing is sited there).
    Private Sub EnsureInputControls()
        If KBotDesignTime.IsDesignTime(Me) Then Return
        For Each k_item As CustomPopupItem In _items
            If Not k_item.IsInput OrElse _inputControls.ContainsKey(k_item) Then Continue For
            Dim k_control As Control
            If k_item.IsCombo Then
                Dim k_combo As New KBotComboBox() With {.Editable = False}
                For Each k_row As String In k_item.ComboRows
                    k_combo.Items.Add(k_row)
                Next
                If k_item.ComboSelectedIndex >= 0 AndAlso k_item.ComboSelectedIndex < k_combo.Items.Count Then
                    k_combo.SelectedIndex = k_item.ComboSelectedIndex
                End If
                Dim k_comboItem As CustomPopupItem = k_item
                AddHandler k_combo.SelectedIndexChanged, Sub(k_s, k_e) InputComboPicked(k_comboItem, k_combo)
                k_control = k_combo
            Else
                Dim k_box As New KBotTextBox() With {
                    .PlaceholderText = k_item.InputPlaceholder, .MaxLength = k_item.InputMaxLength, .Text = k_item.InputText}
                Dim k_textItem As CustomPopupItem = k_item
                AddHandler k_box.FieldKeyDown, Sub(k_s, k_e) InputKeyDown(k_textItem, k_box, k_e)
                AddHandler k_box.TextChanged, Sub(k_s, k_e) InputTextChanged(k_textItem, k_box)
                k_control = k_box
            End If
            k_control.Enabled = k_item.Enabled
            _inputControls(k_item) = k_control
            Controls.Add(k_control)
            Dim k_themed As IThemedControl = TryCast(k_control, IThemedControl)
            If k_themed IsNot Nothing Then k_themed.ApplyTheme(ThemeManager.Current)
        Next
    End Sub

    ' The height the real control of an input row wants at the popup's font (the combo forces its own).
    Private Function InputControlHeight(k_item As CustomPopupItem) As Integer
        EnsureInputControls()
        Dim k_control As Control = Nothing
        If Not _inputControls.TryGetValue(k_item, k_control) Then
            Return Font.Height + ThemeShapes.ScaleDpi(ScaleRef, 14)   ' design time: no control
        End If
        Dim k_combo As KBotComboBox = TryCast(k_control, KBotComboBox)
        If k_combo IsNot Nothing Then Return k_combo.PreferredHeight
        Return k_control.GetPreferredSize(Size.Empty).Height
    End Function

    ''' <summary>The height of an input row: the caption line, a small gap, the control.</summary>
    Friend Function InputRowHeight(k_item As CustomPopupItem) As Integer
        Return EffectiveRowHeight() + ThemeShapes.ScaleDpi(ScaleRef, InputGapLogical) + InputControlHeight(k_item)
    End Function

    ''' <summary>The width an input row asks for (so a popup of inputs is never narrower than the control needs).</summary>
    Friend Function InputMinWidth() As Integer
        Return ThemeShapes.ScaleDpi(ScaleRef, InputMinWidthLogical)
    End Function

    ' The real controls follow the rows (the content scrolls under the window): placed under the caption line of the row.
    Private Sub PlaceInputControls()
        If _inputControls.Count = 0 Then Return
        Dim k_padX As Integer = ThemeShapes.ScaleDpi(ScaleRef, PadXLogical)
        Dim k_lineH As Integer = EffectiveRowHeight() + ThemeShapes.ScaleDpi(ScaleRef, InputGapLogical)
        For k_i As Integer = 0 To _items.Count - 1
            Dim k_control As Control = Nothing
            If Not _inputControls.TryGetValue(_items(k_i), k_control) Then Continue For
            If k_i >= _rows.Length Then Continue For
            Dim k_row As Rectangle = _rows(k_i)
            Dim k_h As Integer = Math.Max(1, k_row.Height - k_lineH)
            k_control.Bounds = New Rectangle(k_row.Left + k_padX, k_row.Top + k_lineH - _scroll,
                                             Math.Max(1, k_row.Width - 2 * k_padX), k_h)
            k_control.Visible = True
        Next
    End Sub

    ' The caption line of an input row (the control itself is a child window).
    Private Sub DrawInputRow(k_g As Graphics, k_index As Integer, k_row As Rectangle, k_padX As Integer)
        Dim k_item As CustomPopupItem = Items(k_index)
        Dim k_fore As Color = If(k_item.Enabled, EffectiveItemForeColor, EffectiveDisabledForeColor)
        Dim k_lineH As Integer = EffectiveRowHeight()
        Dim k_gutter As Integer = IconGutter()
        ' The icon (if the item has one) sits on the caption line, in the same band as the icons of the plain rows.
        If k_gutter > 0 AndAlso k_item.Image IsNot Nothing Then
            Dim k_side As Integer = ThemeShapes.ScaleDpi(ScaleRef, ImageSize)
            Dim k_dest As New Rectangle(k_row.Left + k_padX, k_row.Top + (k_lineH - k_side) \ 2, k_side, k_side)
            KBotNavList.DrawItemImage(k_g, k_item.Image, k_dest, k_item.Enabled)
        End If
        Dim k_text As New Rectangle(k_row.Left + k_padX + k_gutter, k_row.Top,
                                    Math.Max(0, k_row.Width - 2 * k_padX - k_gutter), k_lineH)
        If k_text.Width <= 0 Then Return
        TextRenderer.DrawText(k_g, If(k_item.Text, String.Empty), Font, k_text, k_fore,
                              MeasureFlags() Or TextFormatFlags.EndEllipsis)
    End Sub

    ' The focus goes to the first input when the popup opens (a text box first: that is where one types).
    Private Sub FocusFirstInput()
        Try
            Dim k_first As Control = Nothing
            For Each k_item As CustomPopupItem In _items
                Dim k_control As Control = Nothing
                If Not k_item.IsInput OrElse Not _inputControls.TryGetValue(k_item, k_control) Then Continue For
                If k_item.IsTextInput Then
                    k_first = k_control
                    Exit For
                End If
                If k_first Is Nothing Then k_first = k_control
            Next
            k_first?.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.FocusFirstInput", ex)
        End Try
    End Sub

    ' True while one of the input controls has the keyboard: its keys are its own.
    Private Function InputHasFocus() As Boolean
        If _inputControls.Count = 0 Then Return False
        For Each k_control As Control In _inputControls.Values
            If k_control.ContainsFocus Then Return True
        Next
        Return False
    End Function

    ' UI boundary (list event): logs and swallows.
    Private Sub InputComboPicked(k_item As CustomPopupItem, k_combo As KBotComboBox)
        Try
            If _closing OrElse k_combo.SelectedIndex < 0 Then Return
            k_item.ComboSelectedIndex = k_combo.SelectedIndex
            CloseWith(k_item, _items.IndexOf(k_item))
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.InputComboPicked", ex)
        End Try
    End Sub

    ' UI boundary (key event): logs and swallows. Enter answers; Esc is the popup's (ProcessCmdKey).
    Private Sub InputKeyDown(k_item As CustomPopupItem, k_box As KBotTextBox, e As KeyEventArgs)
        Try
            If e.KeyCode <> Keys.Enter OrElse _closing Then Return
            e.Handled = True
            e.SuppressKeyPress = True
            k_item.InputText = k_box.Text.Trim()
            CloseWith(k_item, _items.IndexOf(k_item))
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.InputKeyDown", ex)
        End Try
    End Sub

    ' UI boundary (text event): logs and swallows. DigitsOnly keeps only 0-9.
    Private Sub InputTextChanged(k_item As CustomPopupItem, k_box As KBotTextBox)
        Try
            If _fixingInput OrElse Not k_item.InputDigitsOnly Then Return
            Dim k_clean As String = New String(k_box.Text.Where(Function(k_c) k_c >= "0"c AndAlso k_c <= "9"c).ToArray())
            If String.Equals(k_clean, k_box.Text, StringComparison.Ordinal) Then Return
            _fixingInput = True
            Try
                k_box.Text = k_clean
            Finally
                _fixingInput = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.InputTextChanged", ex)
        End Try
    End Sub

End Class
