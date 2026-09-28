Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>One node of a functional / economic code tree (see <see cref="ClassificationCodeTree"/>).</summary>
Public NotInheritable Class ClassificationCodeNode
    ''' <summary>«65», «6501» for groups; the six-digit code for a leaf.</summary>
    Public Property Id As String = String.Empty
    ''' <summary>The two digits this level adds: «65», «01», «01».</summary>
    Public Property Digits As String = String.Empty
    Public Property Caption As String = String.Empty
    ''' <summary>True for a code that can be chosen (the six-digit code in <see cref="Id"/>).</summary>
    Public Property IsLeaf As Boolean
    Public Property Children As New List(Of ClassificationCodeNode)()
    Friend Property SortKey As String = String.Empty
End Class

''' <summary>
''' Slice 0087 -- the functional / economic code lists turned into a three-level tree of two digits
''' each, exactly as the public registration page does it (PYTHON/static/js/inregistrare/
''' tree-builder.js), so the «add classifications» window offers the same leaves the server
''' accepts (routes/inregistrare/cerere.py, <c>_is_selectable</c>):
''' <list type="bullet">
''' <item><c>650000</c> is the NAME of level 1 <c>65</c>, never a choice;</item>
''' <item><c>650100</c> is a leaf while nothing hangs under <c>6501</c>, else the name of <c>6501</c>;</item>
''' <item><c>650101</c> is always a leaf.</item>
''' </list>
''' A level-1 group without a name or without children is dropped. Pure: no I/O, no Try/Catch.
''' </summary>
Public NotInheritable Class ClassificationCodeTree

    Private Sub New()
    End Sub

    Public Shared Function Build(codes As IEnumerable(Of CodeName),
                                 groupCaptions As IDictionary(Of String, String)) As List(Of ClassificationCodeNode)
        Dim captions As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Dim order As New List(Of String)()
        If codes IsNot Nothing Then
            For Each c As CodeName In codes
                Dim code As String = If(c.Code, String.Empty).Trim()
                If code.Length <> 6 OrElse captions.ContainsKey(code) Then Continue For
                captions(code) = If(c.Name, String.Empty).Trim()
                order.Add(code)
            Next
        End If

        Dim stemsWithChildren As New HashSet(Of String)(StringComparer.Ordinal)
        For Each code As String In order
            If Not code.EndsWith("00", StringComparison.Ordinal) Then stemsWithChildren.Add(code.Substring(0, 4))
        Next

        Dim nameFor As Func(Of String, String) =
            Function(prefix As String) As String
                Dim own As String = Nothing
                If captions.TryGetValue(prefix.PadRight(6, "0"c), own) AndAlso own.Length > 0 Then Return own
                Dim given As String = Nothing
                If groupCaptions IsNot Nothing AndAlso groupCaptions.TryGetValue(prefix, given) AndAlso
                   Not String.IsNullOrWhiteSpace(given) Then Return given.Trim()
                Return String.Empty
            End Function

        Dim tops As New Dictionary(Of String, ClassificationCodeNode)(StringComparer.Ordinal)
        Dim mids As New Dictionary(Of String, ClassificationCodeNode)(StringComparer.Ordinal)
        Dim topNode As Func(Of String, ClassificationCodeNode) =
            Function(prefix As String) As ClassificationCodeNode
                Dim n As ClassificationCodeNode = Nothing
                If Not tops.TryGetValue(prefix, n) Then
                    n = NewNode(prefix, prefix, nameFor(prefix), prefix, isLeaf:=False)
                    tops(prefix) = n
                End If
                Return n
            End Function
        Dim midNode As Func(Of String, ClassificationCodeNode) =
            Function(stem As String) As ClassificationCodeNode
                Dim n As ClassificationCodeNode = Nothing
                If Not mids.TryGetValue(stem, n) Then
                    n = NewNode(stem, stem.Substring(2), nameFor(stem), stem, isLeaf:=False)
                    mids(stem) = n
                    topNode(stem.Substring(0, 2)).Children.Add(n)
                End If
                Return n
            End Function

        For Each code As String In order
            If code.EndsWith("0000", StringComparison.Ordinal) Then Continue For
            Dim stem As String = code.Substring(0, 4)
            If code.EndsWith("00", StringComparison.Ordinal) Then
                If Not stemsWithChildren.Contains(stem) Then
                    topNode(code.Substring(0, 2)).Children.Add(
                        NewNode(code, code.Substring(2, 2), captions(code), code, isLeaf:=True))
                End If
                Continue For
            End If
            midNode(stem).Children.Add(NewNode(code, code.Substring(4), captions(code), code, isLeaf:=True))
        Next

        Dim kept As List(Of ClassificationCodeNode) =
            tops.Values.Where(Function(t) nameFor(t.Id).Length > 0 AndAlso t.Children.Count > 0).ToList()
        SortDeep(kept)
        Return kept
    End Function

    Private Shared Function NewNode(id As String, digits As String, caption As String, sortSource As String,
                                    isLeaf As Boolean) As ClassificationCodeNode
        Return New ClassificationCodeNode() With {
            .Id = id, .Digits = digits, .Caption = caption, .IsLeaf = isLeaf,
            .SortKey = sortSource.PadRight(6, "0"c)}
    End Function

    Private Shared Sub SortDeep(list As List(Of ClassificationCodeNode))
        list.Sort(Function(a, b) String.CompareOrdinal(a.SortKey, b.SortKey))
        For Each n As ClassificationCodeNode In list
            SortDeep(n.Children)
        Next
    End Sub

End Class
