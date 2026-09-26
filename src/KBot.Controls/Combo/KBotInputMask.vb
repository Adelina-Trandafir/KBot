Option Strict On
Imports System.Collections.Generic
Imports System.Text

''' <summary>
''' The input mask of <see cref="KBotComboBox.InputMask"/> (slice 0082): the operator types only
''' the characters that carry the value, and the literals between them (the dots of
''' <c>65.02.04.02.20.01.01</c>) are put in by the mask.
'''
''' <para><b>The mask language</b> is a small subset of <c>MaskedTextBox</c>'s, one character per
''' position:</para>
''' <list type="table">
''' <item><term><c>0</c></term><description>a digit</description></item>
''' <item><term><c>L</c></term><description>a letter</description></item>
''' <item><term><c>A</c></term><description>a letter or a digit</description></item>
''' <item><term><c>&amp;</c></term><description>any character except a space</description></item>
''' <item><term><c>\x</c></term><description>the character <c>x</c> as a literal (so <c>\0</c> is
''' a literal zero)</description></item>
''' <item><term>anything else</term><description>a literal, written by the mask</description></item>
''' </list>
'''
''' <para><b>The raw value</b> is the string of SLOT characters only (<c>65020402200101</c>). Every
''' edit is done on it and the text is rebuilt from it, so the literals can never drift. A literal
''' is written only when a slot character FOLLOWS it: typing <c>65</c> shows <c>65</c>, the next
''' digit shows <c>65.0</c>. There is no trailing literal to backspace over.</para>
'''
''' <para>Pure: no control, no I/O, so no Try/Catch (house rule).</para>
''' </summary>
Public NotInheritable Class KBotInputMask

    Private Enum SlotKind
        Literal
        Digit
        Letter
        LetterOrDigit
        AnyChar
    End Enum

    Private Structure Token
        Public Kind As SlotKind
        Public Literal As Char
    End Structure

    Private ReadOnly _tokens As New List(Of Token)()
    Private ReadOnly _slotCount As Integer

    ''' <summary>The mask exactly as it was given.</summary>
    Public ReadOnly Property Mask As String

    ''' <summary>How many characters the operator types for a full value.</summary>
    Public ReadOnly Property SlotCount As Integer
        Get
            Return _slotCount
        End Get
    End Property

    ''' <summary>Parses <paramref name="mask"/>. An empty mask, a mask with no slot at all and a
    ''' trailing lone <c>\</c> THROW: each would be a mask that silently does nothing.</summary>
    Public Sub New(mask As String)
        If String.IsNullOrEmpty(mask) Then
            Throw New ArgumentException("An input mask cannot be empty.", NameOf(mask))
        End If
        Me.Mask = mask

        Dim i As Integer = 0
        While i < mask.Length
            Dim c As Char = mask(i)
            Dim t As New Token()
            Select Case c
                Case "0"c : t.Kind = SlotKind.Digit
                Case "L"c : t.Kind = SlotKind.Letter
                Case "A"c : t.Kind = SlotKind.LetterOrDigit
                Case "&"c : t.Kind = SlotKind.AnyChar
                Case "\"c
                    If i = mask.Length - 1 Then
                        Throw New ArgumentException(
                            $"The input mask '{mask}' ends in an escape character with nothing to escape.",
                            NameOf(mask))
                    End If
                    i += 1
                    t.Kind = SlotKind.Literal
                    t.Literal = mask(i)
                Case Else
                    t.Kind = SlotKind.Literal
                    t.Literal = c
            End Select
            _tokens.Add(t)
            If t.Kind <> SlotKind.Literal Then _slotCount += 1
            i += 1
        End While

        If _slotCount = 0 Then
            Throw New ArgumentException(
                $"The input mask '{mask}' has no position to type into (use 0, L, A or &).", NameOf(mask))
        End If
    End Sub

    ''' <summary>Does slot number <paramref name="slotIndex"/> (0-based, literals not counted)
    ''' accept <paramref name="c"/>?</summary>
    Public Function Accepts(slotIndex As Integer, c As Char) As Boolean
        If slotIndex < 0 OrElse slotIndex >= _slotCount Then Return False
        Select Case SlotAt(slotIndex).Kind
            Case SlotKind.Digit : Return Char.IsDigit(c)
            Case SlotKind.Letter : Return Char.IsLetter(c)
            Case SlotKind.LetterOrDigit : Return Char.IsLetterOrDigit(c)
            Case SlotKind.AnyChar : Return Not Char.IsWhiteSpace(c) AndAlso Not Char.IsControl(c)
            Case Else : Return False
        End Select
    End Function

    ''' <summary>
    ''' The slot characters found in <paramref name="text"/>, in order. Literals of the mask that
    ''' are present are stepped over, absent ones are assumed, characters no slot accepts are
    ''' dropped, and whatever comes after the last slot is ignored -- so a pasted
    ''' <c>65020402200101</c>, a typed <c>65.02.04.02.20.01.01</c> and a list caption
    ''' <c>65.02.04.02.20.01.01 — Denumire</c> all give the same raw value.
    ''' </summary>
    Public Function ExtractRaw(text As String) As String
        Dim raw As New StringBuilder()
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Dim pos As Integer = 0
        For Each c As Char In text
            ' Literals in front of the next slot: consumed when typed, assumed when not.
            Dim consumed As Boolean = False
            While pos < _tokens.Count AndAlso _tokens(pos).Kind = SlotKind.Literal
                If _tokens(pos).Literal = c Then
                    pos += 1
                    consumed = True
                    Exit While
                End If
                pos += 1
            End While
            If consumed Then Continue For
            If pos >= _tokens.Count Then Exit For
            If Accepts(raw.Length, c) Then
                raw.Append(c)
                pos += 1
            End If
        Next
        Return raw.ToString()
    End Function

    ''' <summary>The text for a raw value: literals only in front of a slot character that
    ''' follows them. Characters past <see cref="SlotCount"/> are dropped.</summary>
    Public Function Format(raw As String) As String
        Dim sb As New StringBuilder()
        If String.IsNullOrEmpty(raw) Then Return String.Empty
        Dim ri As Integer = 0
        For Each t As Token In _tokens
            If ri >= raw.Length Then Exit For
            If t.Kind = SlotKind.Literal Then
                sb.Append(t.Literal)
            Else
                sb.Append(raw(ri))
                ri += 1
            End If
        Next
        Return sb.ToString()
    End Function

    ''' <summary>The text <paramref name="text"/> would show once the mask is applied to it.</summary>
    Public Function Normalize(text As String) As String
        Return Format(ExtractRaw(text))
    End Function

    ''' <summary>Is every slot filled?</summary>
    Public Function IsComplete(text As String) As Boolean
        Return ExtractRaw(text).Length = _slotCount
    End Function

    ''' <summary>How many slot characters lie before position <paramref name="textIndex"/> of a
    ''' text the mask produced.</summary>
    Public Function RawIndexFromTextIndex(formatted As String, textIndex As Integer) As Integer
        Dim limit As Integer = Math.Min(Math.Max(0, textIndex), If(formatted, String.Empty).Length)
        Dim n As Integer = 0
        For i As Integer = 0 To Math.Min(limit, _tokens.Count) - 1
            If _tokens(i).Kind <> SlotKind.Literal Then n += 1
        Next
        Return n
    End Function

    ''' <summary>Where the caret goes in the formatted text after <paramref name="rawIndex"/>
    ''' slot characters: right after the last of them (0 for none).</summary>
    Public Function TextIndexFromRawIndex(rawIndex As Integer) As Integer
        If rawIndex <= 0 Then Return 0
        Dim n As Integer = 0
        For i As Integer = 0 To _tokens.Count - 1
            If _tokens(i).Kind <> SlotKind.Literal Then
                n += 1
                If n = rawIndex Then Return i + 1
            End If
        Next
        Return _tokens.Count
    End Function

    ' =====================================================================
    ' EDITS -- what one keystroke does. Nothing = the keystroke is refused.
    ' =====================================================================

    ''' <summary>Types <paramref name="c"/> over the selection. Refused when the character does
    ''' not fit its slot, or when the value is already full.</summary>
    Public Function InsertChar(text As String, selStart As Integer, selLength As Integer,
                               c As Char) As KBotMaskEdit
        Dim raw As String = Nothing, rs As Integer, re As Integer
        Resolve(text, selStart, selLength, raw, rs, re)
        Dim kept As String = raw.Remove(rs, re - rs)
        If kept.Length >= _slotCount Then Return Nothing
        Dim candidate As String = kept.Insert(rs, c.ToString())
        If Not IsValidRaw(candidate) Then Return Nothing
        Return New KBotMaskEdit(Format(candidate), TextIndexFromRawIndex(rs + 1))
    End Function

    ''' <summary>Backspace: the selection, or the slot character before the caret (the
    ''' literals in between go with it).</summary>
    Public Function Backspace(text As String, selStart As Integer, selLength As Integer) As KBotMaskEdit
        Dim raw As String = Nothing, rs As Integer, re As Integer
        Resolve(text, selStart, selLength, raw, rs, re)
        If re > rs Then Return Remove(raw, rs, re)
        If rs = 0 Then Return Nothing
        Return Remove(raw, rs - 1, rs)
    End Function

    ''' <summary>Delete: the selection, or the slot character after the caret.</summary>
    Public Function DeleteForward(text As String, selStart As Integer, selLength As Integer) As KBotMaskEdit
        Dim raw As String = Nothing, rs As Integer, re As Integer
        Resolve(text, selStart, selLength, raw, rs, re)
        If re > rs Then Return Remove(raw, rs, re)
        If rs >= raw.Length Then Return Nothing
        Return Remove(raw, rs, rs + 1)
    End Function

    ''' <summary>Pastes <paramref name="pasted"/> over the selection: the characters that fit
    ''' the slots, in order, until the value is full. Literals in the pasted text are skipped.</summary>
    Public Function Paste(text As String, selStart As Integer, selLength As Integer,
                          pasted As String) As KBotMaskEdit
        Dim raw As String = Nothing, rs As Integer, re As Integer
        Resolve(text, selStart, selLength, raw, rs, re)
        Dim left As New StringBuilder(raw.Substring(0, rs))
        Dim right As String = raw.Substring(re)
        For Each p As Char In If(pasted, String.Empty)
            If left.Length + right.Length >= _slotCount Then Exit For
            If Accepts(left.Length, p) Then left.Append(p)
        Next
        Dim caretRaw As Integer = left.Length
        Dim combined As String = TrimToValid(left.ToString() & right)
        Return New KBotMaskEdit(Format(combined), TextIndexFromRawIndex(Math.Min(caretRaw, combined.Length)))
    End Function

    ' The raw value and the raw span of the selection. A text the mask did not produce (a list
    ' caption chosen from the drop-down, for one) is taken as its normalized form; a selection
    ' over it that is not empty means "all of it", anything else means "at the end".
    Private Sub Resolve(text As String, selStart As Integer, selLength As Integer,
                        ByRef raw As String, ByRef rs As Integer, ByRef re As Integer)
        Dim t As String = If(text, String.Empty)
        raw = ExtractRaw(t)
        Dim formatted As String = Format(raw)
        If Not String.Equals(formatted, t, StringComparison.Ordinal) Then
            If selLength > 0 Then
                rs = 0
                re = raw.Length
            Else
                rs = raw.Length
                re = raw.Length
            End If
            Return
        End If
        Dim a As Integer = Math.Max(0, Math.Min(selStart, formatted.Length))
        Dim b As Integer = Math.Max(a, Math.Min(selStart + Math.Max(0, selLength), formatted.Length))
        rs = RawIndexFromTextIndex(formatted, a)
        re = RawIndexFromTextIndex(formatted, b)
        ' A selection that starts ON a literal and covers no slot selects nothing raw -- fine.
    End Sub

    Private Function Remove(raw As String, rs As Integer, re As Integer) As KBotMaskEdit
        Dim rest As String = TrimToValid(raw.Remove(rs, re - rs))
        Return New KBotMaskEdit(Format(rest), TextIndexFromRawIndex(Math.Min(rs, rest.Length)))
    End Function

    ' After a deletion the characters on the right shift one slot to the left; on a mask that
    ' mixes kinds (L and 0, say) one of them may no longer fit. Cut at the first that does not.
    Private Function TrimToValid(raw As String) As String
        For i As Integer = 0 To raw.Length - 1
            If Not Accepts(i, raw(i)) Then Return raw.Substring(0, i)
        Next
        Return If(raw.Length > _slotCount, raw.Substring(0, _slotCount), raw)
    End Function

    Private Function IsValidRaw(raw As String) As Boolean
        If raw.Length > _slotCount Then Return False
        For i As Integer = 0 To raw.Length - 1
            If Not Accepts(i, raw(i)) Then Return False
        Next
        Return True
    End Function

    Private Function SlotAt(slotIndex As Integer) As Token
        Dim n As Integer = 0
        For Each t As Token In _tokens
            If t.Kind = SlotKind.Literal Then Continue For
            If n = slotIndex Then Return t
            n += 1
        Next
        Throw New ArgumentOutOfRangeException(NameOf(slotIndex))
    End Function
End Class

''' <summary>The result of one masked keystroke: the new text and where the caret goes.</summary>
Public NotInheritable Class KBotMaskEdit
    Public ReadOnly Property Text As String
    Public ReadOnly Property Caret As Integer

    Public Sub New(text As String, caret As Integer)
        Me.Text = If(text, String.Empty)
        Me.Caret = Math.Max(0, Math.Min(caret, Me.Text.Length))
    End Sub
End Class
