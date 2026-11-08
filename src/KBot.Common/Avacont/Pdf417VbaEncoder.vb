Imports System.Collections.Generic
Imports System.Globalization
Imports System.Numerics
Imports System.Text

Namespace Avacont
    Partial Public NotInheritable Class Pdf417VbaEncoder
        Private Const DataColumns As Integer = 6
        Private Const CorrectionLevel As Integer = 3
        Private Const MaxRows As Integer = 90
        Private Const Modulus As Integer = 929

        Private Const TextMode As Integer = 900
        Private Const ByteMode As Integer = 901
        Private Const NumericMode As Integer = 902

        Private Const Upper As Integer = 1
        Private Const Lower As Integer = 2
        Private Const Mixed As Integer = 4
        Private Const Punctuation As Integer = 8

        Private Shared ReadOnly CorrectionCoefficients As Integer() = {
            274, 562, 232, 755, 599, 524, 801, 132, 295, 116, 442, 428, 295, 42, 176, 65
        }

        Private Shared ReadOnly TextCharacters As String() = {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ ",
            "abcdefghijklmnopqrstuvwxyz ",
            "0123456789&" & vbCr & vbTab & ",:#-.$/+%*=^" & ChrW(0) & " ",
            ";<>@[\]_`~!" & vbCr & vbTab & ",:" & vbLf & "-.$/""|*()?{}'"
        }

        Private Structure Block
            Public Start As Integer
            Public Length As Integer
            Public Mode As Integer
        End Structure

        Private Sub New()
        End Sub

        ' Reproduce the default pdf417$ call in the Access OP reports. Each output row
        ' consists of Code PDF417 glyphs and ends in CRLF, including the last row.
        Public Shared Function Encode(text As String, ansiEncoding As Encoding) As String
            If String.IsNullOrEmpty(text) Then Return String.Empty
            ArgumentNullException.ThrowIfNull(ansiEncoding)
            If Not ansiEncoding.IsSingleByte Then
                Throw New ArgumentException("PDF417 Access necesită o pagină de cod ANSI cu un octet per caracter.", NameOf(ansiEncoding))
            End If

            ' VBA calls Asc on each UTF-16 code unit individually.
            Dim characters = text.ToCharArray()
            Dim bytes(characters.Length - 1) As Byte
            For index = 0 To characters.Length - 1
                If ansiEncoding.GetBytes(characters, index, 1, bytes, index) <> 1 Then
                    Throw New ArgumentException("Fiecare caracter trebuie să producă un octet ANSI.", NameOf(ansiEncoding))
                End If
            Next

            Dim blocks = SplitBlocks(bytes)
            Dim words As New List(Of Integer) From {0}
            For index = 0 To blocks.Count - 1
                Dim part = blocks(index)
                Select Case part.Mode
                    Case TextMode
                        If index > 0 Then words.Add(TextMode)
                        AppendText(words, bytes, part)
                    Case NumericMode
                        words.Add(NumericMode)
                        For start = part.Start To part.Start + part.Length - 1 Step 44
                            Dim length = Math.Min(44, part.Start + part.Length - start)
                            Dim number = BigInteger.Parse("1" & Encoding.ASCII.GetString(bytes, start, length), CultureInfo.InvariantCulture)
                            AppendBase900(words, number)
                        Next
                    Case ByteMode
                        AppendBytes(words, bytes, part)
                End Select
            Next

            Dim correctionCount = CorrectionCoefficients.Length
            Dim total = words.Count + correctionCount
            Dim rows = Math.Max(3, (total + DataColumns - 1) \ DataColumns)
            If rows > MaxRows Then
                Throw New ArgumentException("Textul depășește capacitatea PDF417 la 6 coloane și 90 de rânduri.", NameOf(text))
            End If

            While words.Count < rows * DataColumns - correctionCount
                words.Add(TextMode)
            End While
            words(0) = words.Count
            AppendCorrection(words)
            Return EncodeRows(words, rows)
        End Function

        Private Shared Function SplitBlocks(bytes As Byte()) As List(Of Block)
            Dim blocks As New List(Of Block)
            Dim start = 0
            While start < bytes.Length
                Dim mode = Classify(bytes(start))
                Dim finish = start + 1
                While finish < bytes.Length AndAlso Classify(bytes(finish)) = mode
                    finish += 1
                End While
                blocks.Add(New Block With {.Start = start, .Length = finish - start, .Mode = mode})
                start = finish
            End While

            ' Reclassify in order. A block sees any changes already made to its predecessor.
            For index = 0 To blocks.Count - 1
                Dim part = blocks(index)
                If part.Mode <> NumericMode Then Continue For

                Dim before = If(index = 0, 0, blocks(index - 1).Mode)
                Dim after = If(index = blocks.Count - 1, 0, blocks(index + 1).Mode)
                If (after = TextMode AndAlso (before = 0 OrElse before = TextMode) AndAlso part.Length < 8) OrElse
                   (before = TextMode AndAlso after = 0 AndAlso part.Length < 7) OrElse
                   (before = TextMode AndAlso after = ByteMode AndAlso part.Length < 5) Then
                    part.Mode = TextMode
                ElseIf (before = ByteMode AndAlso after = ByteMode AndAlso part.Length < 4) OrElse
                       (part.Length = 1 AndAlso ((before = 0 AndAlso after = ByteMode) OrElse
                                                (before = ByteMode AndAlso after = 0))) Then
                    part.Mode = ByteMode
                End If
                blocks(index) = part
            Next

            blocks = MergeAdjacent(blocks)
            For index = 1 To blocks.Count - 1
                Dim part = blocks(index)
                If part.Mode <> TextMode Then Continue For

                Dim beforeIsByte = blocks(index - 1).Mode = ByteMode
                Dim last = index = blocks.Count - 1
                Dim afterIsByte = Not last AndAlso blocks(index + 1).Mode = ByteMode
                Dim byteThreshold = If(last, If(beforeIsByte, 2, 0),
                    If(beforeIsByte AndAlso afterIsByte, 5, If(beforeIsByte OrElse afterIsByte, 3, 0)))
                If part.Length < byteThreshold Then
                    part.Mode = ByteMode
                    blocks(index) = part
                End If
            Next
            Return MergeAdjacent(blocks)
        End Function

        Private Shared Function Classify(value As Byte) As Integer
            Select Case value
                Case 48 To 57
                    Return NumericMode
                Case 9, 10, 13, 32 To 126
                    Return TextMode
                Case Else
                    Return ByteMode
            End Select
        End Function

        Private Shared Function MergeAdjacent(blocks As List(Of Block)) As List(Of Block)
            Dim merged As New List(Of Block)
            For Each part In blocks
                If merged.Count > 0 AndAlso merged(merged.Count - 1).Mode = part.Mode Then
                    Dim previous = merged(merged.Count - 1)
                    previous.Length += part.Length
                    merged(merged.Count - 1) = previous
                Else
                    merged.Add(part)
                End If
            Next
            Return merged
        End Function

        Private Shared Sub AppendText(words As List(Of Integer), bytes As Byte(), part As Block)
            Dim submode = Upper
            Dim values As New List(Of Integer)
            Dim last = part.Start + part.Length - 1
            For index = part.Start To last
                Dim character = ChrW(bytes(index))
                Dim mask = TextMask(character)
                If (mask And submode) = 0 Then
                    Dim common = If(index = last, 0, mask And TextMask(ChrW(bytes(index + 1))))
                    If common = 0 AndAlso submode = Lower AndAlso (mask And Upper) <> 0 Then
                        values.Add(27)
                        values.Add(TextCharacters(0).IndexOf(character))
                        Continue For
                    End If
                    If common = 0 AndAlso (mask And Punctuation) <> 0 Then
                        values.Add(29)
                        values.Add(TextCharacters(3).IndexOf(character))
                        Continue For
                    End If

                    Dim available = If(common = 0, mask, common)
                    Dim nextMode = available And -available
                    AppendTextLatch(values, submode, nextMode)
                    submode = nextMode
                End If
                Dim table = BitOperations.TrailingZeroCount(CUInt(submode))
                values.Add(TextCharacters(table).IndexOf(character))
            Next

            If values.Count Mod 2 <> 0 Then values.Add(29)
            For index = 0 To values.Count - 1 Step 2
                words.Add(values(index) * 30 + values(index + 1))
            Next
        End Sub

        Private Shared Function TextMask(character As Char) As Integer
            Dim mask = 0
            For index = 0 To TextCharacters.Length - 1
                If TextCharacters(index).IndexOf(character) >= 0 Then mask = mask Or (1 << index)
            Next
            Return mask
        End Function

        Private Shared Sub AppendTextLatch(values As List(Of Integer), current As Integer, target As Integer)
            If current = Punctuation Then
                values.Add(29)
                current = Upper
                If target = Upper Then Return
            End If
            If target = Punctuation Then
                If current <> Mixed Then values.Add(28)
                values.Add(25)
            ElseIf target = Lower Then
                values.Add(27)
            Else
                If current = Lower AndAlso target = Upper Then values.Add(28)
                values.Add(28)
            End If
        End Sub

        Private Shared Sub AppendBytes(words As List(Of Integer), bytes As Byte(), part As Block)
            If part.Length = 1 Then
                words.Add(913)
                words.Add(bytes(part.Start))
                Return
            End If

            words.Add(If(part.Length Mod 6 = 0, 924, ByteMode))
            Dim index = part.Start
            Dim finish = index + part.Length
            While finish - index >= 6
                Dim number As Long = 0
                For ofst = 0 To 5
                    number = (number << 8) Or CLng(bytes(index + ofst))
                Next
                AppendBase900(words, number)
                index += 6
            End While
            While index < finish
                words.Add(bytes(index))
                index += 1
            End While
        End Sub

        Private Shared Sub AppendBase900(words As List(Of Integer), number As BigInteger)
            Dim start = words.Count
            Do
                Dim remainder As BigInteger
                number = BigInteger.DivRem(number, 900, remainder)
                words.Add(CInt(remainder))
            Loop Until number.IsZero
            words.Reverse(start, words.Count - start)
        End Sub

        Private Shared Sub AppendCorrection(words As List(Of Integer))
            Dim correction(CorrectionCoefficients.Length - 1) As Integer
            Dim last = correction.Length - 1
            For Each word In words
                Dim factor = (word + correction(last)) Mod Modulus
                For index = last To 0 Step -1
                    Dim prior = If(index = 0, 0, correction(index - 1))
                    correction(index) = (prior + Modulus - (factor * CorrectionCoefficients(index)) Mod Modulus) Mod Modulus
                Next
            Next
            For index = last To 0 Step -1
                words.Add((Modulus - correction(index)) Mod Modulus)
            Next
        End Sub

        Private Shared Function EncodeRows(words As List(Of Integer), rows As Integer) As String
            Dim indicators = {(rows - 1) \ 3, CorrectionLevel * 3 + (rows - 1) Mod 3, DataColumns - 1}
            Dim result As New StringBuilder
            For row = 0 To rows - 1
                Dim cluster = row Mod 3
                Dim group = (row \ 3) * 30
                Dim table = GlyphTables(cluster)
                result.Append("+*")
                result.Append(table, (group + indicators(cluster)) * 3, 3).Append("*"c)
                For column = 0 To DataColumns - 1
                    result.Append(table, words(row * DataColumns + column) * 3, 3).Append("*"c)
                Next
                result.Append(table, (group + indicators((cluster + 2) Mod 3)) * 3, 3).Append("*"c)
                result.Append("-"c).Append(vbCrLf)
            Next
            Return result.ToString()
        End Function
    End Class
End Namespace
