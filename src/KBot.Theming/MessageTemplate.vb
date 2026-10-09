Option Strict On
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' The message templates of the message catalog (<c>Config\mesaje_catalog.json</c>): plain text with
''' <c>{expression}</c> holes for the parts the program fills in at run time, <c>{{</c> / <c>}}</c> for
''' a literal brace. The catalog stores, for each call, the template the CODE has (<c>origText</c>) and
''' the one the operator edited (<c>text</c>).
'''
''' <para><b>Why no call site changed.</b> A call builds its text itself (<c>"..." &amp; ex.Message</c>).
''' <see cref="Match"/> lays the code's template over the text the call really produced and reads the
''' value of every hole back out of it; <see cref="Fill"/> then puts those values into the edited
''' template. The hole is identified by its expression (<c>{ex.Message}</c>), so the editor may move
''' it, repeat it or drop it. Anything that does not fit falls back to the call's own text.</para>
''' </summary>
Friend NotInheritable Class MessageTemplate

    Private Sub New()
    End Sub

    ''' <summary>One piece of a template: literal text, or the expression of a hole.</summary>
    Friend Structure Token
        Public IsHole As Boolean
        Public Value As String
    End Structure

    Friend Shared Function Tokenize(k_template As String) As List(Of Token)
        Dim k_tokens As New List(Of Token)()
        Dim k_text As String = If(k_template, String.Empty)
        Dim k_lit As New StringBuilder()
        Dim k_i As Integer = 0
        While k_i < k_text.Length
            Dim k_c As Char = k_text(k_i)
            Dim k_next As Char = If(k_i + 1 < k_text.Length, k_text(k_i + 1), ControlChars.NullChar)
            If k_c = "{"c AndAlso k_next = "{"c Then
                k_lit.Append("{"c) : k_i += 2
            ElseIf k_c = "}"c AndAlso k_next = "}"c Then
                k_lit.Append("}"c) : k_i += 2
            ElseIf k_c = "{"c Then
                Dim k_end As Integer = HoleEnd(k_text, k_i)
                Dim k_expr As String = If(k_end > k_i, Normalize(k_text.Substring(k_i + 1, k_end - k_i - 1)), String.Empty)
                If k_expr.Length > 0 Then
                    If k_lit.Length > 0 Then
                        k_tokens.Add(New Token() With {.IsHole = False, .Value = k_lit.ToString()})
                        k_lit.Clear()
                    End If
                    k_tokens.Add(New Token() With {.IsHole = True, .Value = k_expr})
                    k_i = k_end + 1
                Else
                    k_lit.Append("{"c) : k_i += 1
                End If
            Else
                k_lit.Append(k_c) : k_i += 1
            End If
        End While
        If k_lit.Length > 0 Then k_tokens.Add(New Token() With {.IsHole = False, .Value = k_lit.ToString()})
        Return k_tokens
    End Function

    ' Index of the "}" that closes the "{" at k_start (strings inside the expression are skipped), or -1.
    Private Shared Function HoleEnd(k_text As String, k_start As Integer) As Integer
        Dim k_depth As Integer = 0
        Dim k_j As Integer = k_start
        While k_j < k_text.Length
            Dim k_c As Char = k_text(k_j)
            If k_c = """"c Then
                k_j += 1
                While k_j < k_text.Length
                    If k_text(k_j) = """"c Then
                        If k_j + 1 < k_text.Length AndAlso k_text(k_j + 1) = """"c Then
                            k_j += 2
                            Continue While
                        End If
                        Exit While
                    End If
                    k_j += 1
                End While
            ElseIf k_c = "{"c Then
                k_depth += 1
            ElseIf k_c = "}"c Then
                k_depth -= 1
                If k_depth = 0 Then Return k_j
            End If
            k_j += 1
        End While
        Return -1
    End Function

    Private Shared Function Normalize(k_expr As String) As String
        Return Regex.Replace(k_expr, "\s+", " ").Trim()
    End Function

    ''' <summary>The text with every line break as a single LF, which is how templates keep them.</summary>
    Friend Shared Function Unify(k_text As String) As String
        Return If(k_text, String.Empty).Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
    End Function

    ''' <summary>
    ''' Lays <paramref name="k_template"/> over <paramref name="k_actual"/>. True when the literal parts
    ''' fit; the value of each hole is then added to <paramref name="k_values"/> (the first value of a
    ''' repeated expression stays).
    ''' </summary>
    Friend Shared Function Match(k_template As String, k_actual As String,
                                 k_values As Dictionary(Of String, String)) As Boolean
        Dim k_tokens As List(Of Token) = Tokenize(k_template)
        Dim k_text As String = Unify(k_actual)
        If Not k_tokens.Any(Function(k_t) k_t.IsHole) Then
            Dim k_flat As New StringBuilder()
            For Each k_t As Token In k_tokens
                k_flat.Append(k_t.Value)
            Next
            Return String.Equals(k_flat.ToString(), k_text, StringComparison.Ordinal)
        End If

        Dim k_pattern As New StringBuilder("^")
        Dim k_holes As New List(Of String)()
        For Each k_t As Token In k_tokens
            If k_t.IsHole Then
                k_pattern.Append("([\s\S]*?)")
                k_holes.Add(k_t.Value)
            Else
                k_pattern.Append(Regex.Escape(k_t.Value))
            End If
        Next
        k_pattern.Append("$")
        Try
            Dim k_m As System.Text.RegularExpressions.Match =
                Regex.Match(k_text, k_pattern.ToString(), RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250))
            If Not k_m.Success Then Return False
            For k_i As Integer = 0 To k_holes.Count - 1
                If Not k_values.ContainsKey(k_holes(k_i)) Then k_values(k_holes(k_i)) = k_m.Groups(k_i + 1).Value
            Next
            Return True
        Catch ex As RegexMatchTimeoutException
            Return False   ' an over-long text: the call keeps its own wording
        End Try
    End Function

    ''' <summary>The template with every hole replaced by its value; a hole without a value stays visible.</summary>
    Friend Shared Function Fill(k_template As String, k_values As Dictionary(Of String, String)) As String
        Dim k_out As New StringBuilder()
        For Each k_t As Token In Tokenize(k_template)
            If Not k_t.IsHole Then
                k_out.Append(k_t.Value)
            Else
                Dim k_value As String = Nothing
                If k_values.TryGetValue(k_t.Value, k_value) Then
                    k_out.Append(k_value)
                Else
                    k_out.Append("{"c).Append(k_t.Value).Append("}"c)
                End If
            End If
        Next
        Return k_out.ToString()
    End Function

End Class
