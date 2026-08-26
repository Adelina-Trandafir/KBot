Option Strict On
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

''' <summary>
''' One cell of a scraped FOREXE table, in the shape the site actually produced it.
'''
''' <para>
''' A cell is exactly one of three things, recursively: a scalar (text), an ordered list of
''' cells, or a named set of cells. That is the whole shape — there is no fourth case, and
''' nothing collapses into text on the way to the server.
''' </para>
''' <para>
''' WHY THIS TYPE EXISTS AT ALL. Until 26.08.2026 <c>JobResult.Tables</c> was
''' <c>Dictionary(Of String, List(Of Dictionary(Of String, String)))</c>, and
''' <c>ForexeRunner.TryParseTable</c> flattened every cell with <c>prop.Value.ToString()</c>.
''' For a scalar that is lossless. For a nested table it is not: the reception detail lines
''' arrived at the server as a piece of TEXT that happened to look like a list, and the
''' server had to keep a second parsing path alive to read them back. Two shapes on the wire
''' means two shapes to test, and the lossy one wins any time somebody forgets. Decision D-N:
''' structure travels.
''' </para>
''' <para>
''' Which columns are actually nested is not a guess — it is read off the workflow
''' definitions. A <c>ForEachVar</c> naming a field in <c>collectFields</c> that an inner
''' <c>ScrapeTable</c> also writes with <c>saveTo</c> produces a nested cell, and the
''' executor already preserves it (<c>BuildCollectedRow</c> does <c>JToken.Parse</c>).
''' Today that is <c>ListaReceptii.Detaliu</c> and <c>TabelIndicatori.BugetIndicator</c>.
''' </para>
''' <para>
''' POCO with factory methods and no I/O, so no Try/Catch (house rule). The one method that
''' can realistically throw — <see cref="Text"/> on a non-scalar — throws by design and says
''' which shape it actually found.
''' </para>
''' </summary>
<DebuggerDisplay("{DebugPreview(),nq}")>
Public NotInheritable Class CelulaTabel

    ''' <summary>Which of the three shapes this cell is.</summary>
    Public Enum Fel
        ''' <summary>A single value. <see cref="Text"/> is it.</summary>
        Scalar = 0
        ''' <summary>An ordered list of cells. <see cref="Lista"/> is it.</summary>
        Lista = 1
        ''' <summary>A named set of cells. <see cref="Obiect"/> is it.</summary>
        Obiect = 2
    End Enum

    Private ReadOnly _fel As Fel
    Private ReadOnly _text As String
    Private ReadOnly _lista As IReadOnlyList(Of CelulaTabel)
    Private ReadOnly _obiect As IReadOnlyDictionary(Of String, CelulaTabel)

    Private Sub New(felul As Fel, text As String,
                    lista As IReadOnlyList(Of CelulaTabel),
                    obiect As IReadOnlyDictionary(Of String, CelulaTabel))
        _fel = felul
        _text = text
        _lista = lista
        _obiect = obiect
    End Sub

    ''' <summary>The empty scalar. Shared because it is by far the commonest cell.</summary>
    Public Shared ReadOnly Gol As CelulaTabel = New CelulaTabel(Fel.Scalar, String.Empty, Nothing, Nothing)

    ''' <summary>A scalar cell. <c>Nothing</c> becomes the empty string, never a null cell.</summary>
    Public Shared Function DinText(valoare As String) As CelulaTabel
        If String.IsNullOrEmpty(valoare) Then Return Gol
        Return New CelulaTabel(Fel.Scalar, valoare, Nothing, Nothing)
    End Function

    ''' <summary>An ordered list of cells.</summary>
    Public Shared Function DinLista(valori As IEnumerable(Of CelulaTabel)) As CelulaTabel
        Dim copie As New List(Of CelulaTabel)()
        If valori IsNot Nothing Then
            For Each v As CelulaTabel In valori
                copie.Add(If(v, Gol))
            Next
        End If
        Return New CelulaTabel(Fel.Lista, Nothing, New ReadOnlyCollection(Of CelulaTabel)(copie), Nothing)
    End Function

    ''' <summary>A named set of cells. Key order is preserved as given.</summary>
    Public Shared Function DinObiect(valori As IEnumerable(Of KeyValuePair(Of String, CelulaTabel))) As CelulaTabel
        Dim copie As New Dictionary(Of String, CelulaTabel)(StringComparer.Ordinal)
        Dim ordine As New List(Of String)()
        If valori IsNot Nothing Then
            For Each kvp As KeyValuePair(Of String, CelulaTabel) In valori
                If Not copie.ContainsKey(kvp.Key) Then ordine.Add(kvp.Key)
                copie(kvp.Key) = If(kvp.Value, Gol)
            Next
        End If
        Return New CelulaTabel(Fel.Obiect, Nothing, Nothing, New OrdonatDictionary(copie, ordine))
    End Function

    ''' <summary>Which shape this cell is.</summary>
    Public ReadOnly Property Felul As Fel
        Get
            Return _fel
        End Get
    End Property

    ''' <summary>True when this is a scalar — the only case <see cref="Text"/> answers.</summary>
    Public ReadOnly Property EsteScalar As Boolean
        Get
            Return _fel = Fel.Scalar
        End Get
    End Property

    ''' <summary>True when this is an ordered list.</summary>
    Public ReadOnly Property EsteLista As Boolean
        Get
            Return _fel = Fel.Lista
        End Get
    End Property

    ''' <summary>True when this is a named set.</summary>
    Public ReadOnly Property EsteObiect As Boolean
        Get
            Return _fel = Fel.Obiect
        End Get
    End Property

    ''' <summary>
    ''' The scalar value. THROWS on a list or an object, deliberately: silently rendering a
    ''' nested cell as text is exactly the loss this type was introduced to stop, and a caller
    ''' that wants a readable rendering has <see cref="ToDebugString"/>.
    ''' </summary>
    Public ReadOnly Property Text As String
        Get
            If _fel <> Fel.Scalar Then
                Throw New InvalidOperationException(
                    $"Celula nu este un text simplu, ci {NumeFel(_fel)}. " &
                    "Citește «Lista» sau «Obiect», ori folosește «ToDebugString».")
            End If
            Return _text
        End Get
    End Property

    ''' <summary>The scalar value, or <paramref name="implicit_"/> when this is not a scalar.</summary>
    Public Function TextSau(implicit_ As String) As String
        If _fel <> Fel.Scalar Then Return implicit_
        Return _text
    End Function

    ''' <summary>The list elements. Empty on any other shape — never Nothing.</summary>
    Public ReadOnly Property Lista As IReadOnlyList(Of CelulaTabel)
        Get
            If _lista Is Nothing Then Return Array.Empty(Of CelulaTabel)()
            Return _lista
        End Get
    End Property

    ''' <summary>The named members. Empty on any other shape — never Nothing.</summary>
    Public ReadOnly Property Obiect As IReadOnlyDictionary(Of String, CelulaTabel)
        Get
            If _obiect Is Nothing Then Return New Dictionary(Of String, CelulaTabel)(StringComparer.Ordinal)
            Return _obiect
        End Get
    End Property

    ''' <summary>The named member, or Nothing when absent (or when this is not an object).</summary>
    Public Function Membru(nume As String) As CelulaTabel
        If _obiect Is Nothing OrElse nume Is Nothing Then Return Nothing
        Dim v As CelulaTabel = Nothing
        If _obiect.TryGetValue(nume, v) Then Return v
        Return Nothing
    End Function

    ''' <summary>Romanian name of a shape, for messages the operator reads.</summary>
    Public Shared Function NumeFel(felul As Fel) As String
        Select Case felul
            Case Fel.Lista : Return "o listă"
            Case Fel.Obiect : Return "un obiect"
            Case Else : Return "un text"
        End Select
    End Function

    ''' <summary>
    ''' The cell as an indented tree, for reading in a log or a watch window. NOT a wire
    ''' format and never parsed back — <see cref="CelulaTabelJsonConverter"/> owns the wire.
    ''' </summary>
    Public Function ToDebugString() As String
        Dim sb As New StringBuilder()
        Scrie(sb, 0)
        Return sb.ToString()
    End Function

    Private Sub Scrie(sb As StringBuilder, nivel As Integer)
        Dim pad As String = New String(" "c, nivel * 2)
        Select Case _fel
            Case Fel.Scalar
                sb.Append(_text)
            Case Fel.Lista
                sb.Append("[").Append(_lista.Count).Append("]")
                For i As Integer = 0 To _lista.Count - 1
                    sb.AppendLine().Append(pad).Append("  ").Append(i).Append(": ")
                    _lista(i).Scrie(sb, nivel + 1)
                Next
            Case Else
                sb.Append("{").Append(_obiect.Count).Append("}")
                For Each kvp As KeyValuePair(Of String, CelulaTabel) In _obiect
                    sb.AppendLine().Append(pad).Append("  ").Append(kvp.Key).Append(": ")
                    kvp.Value.Scrie(sb, nivel + 1)
                Next
        End Select
    End Sub

    ''' <summary>
    ''' One short line for the debugger's value column — counts first, then a preview, so a
    ''' nested cell can be told from a scalar without expanding anything.
    ''' </summary>
    Friend Function DebugPreview() As String
        Select Case _fel
            Case Fel.Scalar
                If _text.Length <= 40 Then Return """" & _text & """"
                Return """" & _text.Substring(0, 40) & "…"""
            Case Fel.Lista
                Return $"listă[{_lista.Count}]"
            Case Else
                Return $"obiect{{{String.Join(", ", _obiect.Keys.Take(4))}{If(_obiect.Count > 4, ", …", "")}}}"
        End Select
    End Function

    Public Overrides Function ToString() As String
        Return DebugPreview()
    End Function

    ''' <summary>
    ''' A dictionary that enumerates in insertion order. The wire form has to keep column
    ''' order — a JSON object whose keys reshuffle between the proposal and the save would
    ''' make two identical payloads look different to anyone reading them side by side.
    ''' </summary>
    Private NotInheritable Class OrdonatDictionary
        Implements IReadOnlyDictionary(Of String, CelulaTabel)

        Private ReadOnly _map As Dictionary(Of String, CelulaTabel)
        Private ReadOnly _ordine As List(Of String)

        Public Sub New(map As Dictionary(Of String, CelulaTabel), ordine As List(Of String))
            _map = map
            _ordine = ordine
        End Sub

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of String, CelulaTabel)).Count
            Get
                Return _ordine.Count
            End Get
        End Property

        Default Public ReadOnly Property Item(key As String) As CelulaTabel Implements IReadOnlyDictionary(Of String, CelulaTabel).Item
            Get
                Return _map(key)
            End Get
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, CelulaTabel).Keys
            Get
                Return _ordine
            End Get
        End Property

        Public ReadOnly Property Values As IEnumerable(Of CelulaTabel) Implements IReadOnlyDictionary(Of String, CelulaTabel).Values
            Get
                Return _ordine.Select(Function(k) _map(k))
            End Get
        End Property

        Public Function ContainsKey(key As String) As Boolean Implements IReadOnlyDictionary(Of String, CelulaTabel).ContainsKey
            Return _map.ContainsKey(key)
        End Function

        Public Function TryGetValue(key As String, ByRef value As CelulaTabel) As Boolean Implements IReadOnlyDictionary(Of String, CelulaTabel).TryGetValue
            Return _map.TryGetValue(key, value)
        End Function

        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, CelulaTabel)) Implements IEnumerable(Of KeyValuePair(Of String, CelulaTabel)).GetEnumerator
            Return _ordine.Select(Function(k) New KeyValuePair(Of String, CelulaTabel)(k, _map(k))).GetEnumerator()
        End Function

        Private Function GetEnumerator1() As Collections.IEnumerator Implements Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class

End Class


''' <summary>
''' One row of a scraped table: an ordered set of named cells.
'''
''' <para>
''' Exists as a named type — rather than a bare <c>Dictionary(Of String, CelulaTabel)</c> —
''' so it can carry a <see cref="DebuggerDisplayAttribute"/>. Stopping on a row and seeing
''' <c>7 coloane · Tip=«Plata ces» · Detaliu=listă[3]</c> without calling anything is the
''' point: a nested cell has to be visible AS nested, at a glance, or the next person to
''' read this code will assume it is text again.
''' </para>
''' <para>Key order is insertion order — the column order the site produced.</para>
''' </summary>
<DebuggerDisplay("{DebugPreview(),nq}")>
Public NotInheritable Class RandTabel
    Implements IReadOnlyDictionary(Of String, CelulaTabel)

    Private ReadOnly _map As New Dictionary(Of String, CelulaTabel)(StringComparer.Ordinal)
    Private ReadOnly _ordine As New List(Of String)()

    Public Sub New()
    End Sub

    Public Sub New(celule As IEnumerable(Of KeyValuePair(Of String, CelulaTabel)))
        If celule Is Nothing Then Return
        For Each kvp As KeyValuePair(Of String, CelulaTabel) In celule
            Pune(kvp.Key, kvp.Value)
        Next
    End Sub

    ''' <summary>
    ''' Collection-initialiser shape: <c>New RandTabel From {{"Tip", "Plata ces"}}</c>.
    ''' A plain string is a SCALAR cell — the commonest case by far, and spelling it out
    ''' at every call site would bury the nested ones it exists to make visible.
    ''' </summary>
    Public Sub Add(coloana As String, text As String)
        Pune(coloana, CelulaTabel.DinText(text))
    End Sub

    ''' <summary>Collection-initialiser shape for a cell of any shape.</summary>
    Public Sub Add(coloana As String, valoare As CelulaTabel)
        Pune(coloana, valoare)
    End Sub

    ''' <summary>Sets a cell, appending the column the first time it is seen.</summary>
    Public Sub Pune(coloana As String, valoare As CelulaTabel)
        If coloana Is Nothing Then Throw New ArgumentNullException(NameOf(coloana))
        If Not _map.ContainsKey(coloana) Then _ordine.Add(coloana)
        _map(coloana) = If(valoare, CelulaTabel.Gol)
    End Sub

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of String, CelulaTabel)).Count
        Get
            Return _ordine.Count
        End Get
    End Property

    Default Public ReadOnly Property Item(coloana As String) As CelulaTabel Implements IReadOnlyDictionary(Of String, CelulaTabel).Item
        Get
            Return _map(coloana)
        End Get
    End Property

    Public ReadOnly Property Keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, CelulaTabel).Keys
        Get
            Return _ordine
        End Get
    End Property

    Public ReadOnly Property Values As IEnumerable(Of CelulaTabel) Implements IReadOnlyDictionary(Of String, CelulaTabel).Values
        Get
            Return _ordine.Select(Function(k) _map(k))
        End Get
    End Property

    Public Function ContainsKey(coloana As String) As Boolean Implements IReadOnlyDictionary(Of String, CelulaTabel).ContainsKey
        Return coloana IsNot Nothing AndAlso _map.ContainsKey(coloana)
    End Function

    Public Function TryGetValue(coloana As String, ByRef value As CelulaTabel) As Boolean Implements IReadOnlyDictionary(Of String, CelulaTabel).TryGetValue
        If coloana Is Nothing Then
            value = Nothing
            Return False
        End If
        Return _map.TryGetValue(coloana, value)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, CelulaTabel)) Implements IEnumerable(Of KeyValuePair(Of String, CelulaTabel)).GetEnumerator
        Return _ordine.Select(Function(k) New KeyValuePair(Of String, CelulaTabel)(k, _map(k))).GetEnumerator()
    End Function

    Private Function GetEnumerator1() As Collections.IEnumerator Implements Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Friend Function DebugPreview() As String
        Dim capete As String = String.Join(" · ",
            _ordine.Take(3).Select(Function(k) k & "=" & _map(k).DebugPreview()))
        Return $"{_ordine.Count} coloane · {capete}{If(_ordine.Count > 3, " · …", "")}"
    End Function

    Public Overrides Function ToString() As String
        Return DebugPreview()
    End Function
End Class


''' <summary>
''' One scraped table: an ordered list of <see cref="RandTabel"/>.
'''
''' <para>
''' A named type for the same reason <see cref="RandTabel"/> is one — a
''' <see cref="DebuggerDisplayAttribute"/> showing the row count AND which columns are
''' nested, so «this table has a list inside it» is visible before anything is expanded.
''' </para>
''' </summary>
<DebuggerDisplay("{DebugPreview(),nq}")>
Public NotInheritable Class TabelRezultat
    Implements IReadOnlyList(Of RandTabel)

    Private ReadOnly _randuri As New List(Of RandTabel)()

    Public Sub New()
    End Sub

    Public Sub New(randuri As IEnumerable(Of RandTabel))
        If randuri Is Nothing Then Return
        For Each r As RandTabel In randuri
            If r IsNot Nothing Then _randuri.Add(r)
        Next
    End Sub

    Public Sub Adauga(rand As RandTabel)
        If rand Is Nothing Then Throw New ArgumentNullException(NameOf(rand))
        _randuri.Add(rand)
    End Sub

    ''' <summary>Collection-initialiser shape: <c>New TabelRezultat From {rand1, rand2}</c>.</summary>
    Public Sub Add(rand As RandTabel)
        Adauga(rand)
    End Sub

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of RandTabel).Count
        Get
            Return _randuri.Count
        End Get
    End Property

    Default Public ReadOnly Property Item(index As Integer) As RandTabel Implements IReadOnlyList(Of RandTabel).Item
        Get
            Return _randuri(index)
        End Get
    End Property

    Public Function GetEnumerator() As IEnumerator(Of RandTabel) Implements IEnumerable(Of RandTabel).GetEnumerator
        Return _randuri.GetEnumerator()
    End Function

    Private Function GetEnumerator1() As Collections.IEnumerator Implements Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    ''' <summary>
    ''' The columns carrying a list or an object in at least one row. This is the answer to
    ''' «what is actually nested here», computed from the data rather than from a list
    ''' somebody maintains by hand.
    ''' </summary>
    Public Function ColoaneImbricate() As IReadOnlyList(Of String)
        Dim vazute As New List(Of String)()
        For Each rand As RandTabel In _randuri
            For Each kvp As KeyValuePair(Of String, CelulaTabel) In rand
                If Not kvp.Value.EsteScalar AndAlso Not vazute.Contains(kvp.Key) Then
                    vazute.Add(kvp.Key)
                End If
            Next
        Next
        Return vazute
    End Function

    Friend Function DebugPreview() As String
        Dim imbricate As IReadOnlyList(Of String) = ColoaneImbricate()
        If imbricate.Count = 0 Then Return $"{_randuri.Count} rânduri"
        Return $"{_randuri.Count} rânduri · imbricate: {String.Join(", ", imbricate)}"
    End Function

    Public Overrides Function ToString() As String
        Return DebugPreview()
    End Function
End Class


''' <summary>
''' The bridge between the typed tables and JSON.
'''
''' <para>
''' WHY A BRIDGE AND NOT A <c>JsonConverter</c>. <c>JsonConverter(Of T).Read</c> takes a
''' <c>ByRef Utf8JsonReader</c>, and <c>Utf8JsonReader</c> is a <c>ref struct</c> — a shape
''' the VB.NET compiler refuses outright (<c>BC30668</c>, «types with embedded references are
''' not supported»). No VB project can author a System.Text.Json converter, full stop. So the
''' conversion is explicit and happens at the serialisation boundaries: the HTTP request, the
''' local JSON dump, and the association folder.
''' </para>
''' <para>
''' The result is a real <see cref="JsonNode"/> tree, so what goes on the wire is a genuine
''' JSON array of objects — never a string that happens to contain JSON.
''' </para>
''' </summary>
Public NotInheritable Class TabeleJson

    Private Sub New()
    End Sub

    ''' <summary>The tables as a <c>JsonObject</c>: table name ▸ array of row objects.</summary>
    Public Shared Function Catre(tabele As IReadOnlyDictionary(Of String, TabelRezultat)) As JsonObject
        Dim root As New JsonObject()
        If tabele Is Nothing Then Return root
        For Each tabel As KeyValuePair(Of String, TabelRezultat) In tabele
            Dim randuri As New JsonArray()
            If tabel.Value IsNot Nothing Then
                For Each rand As RandTabel In tabel.Value
                    Dim obj As New JsonObject()
                    For Each celula As KeyValuePair(Of String, CelulaTabel) In rand
                        obj(celula.Key) = CatreCelula(celula.Value)
                    Next
                    randuri.Add(obj)
                Next
            End If
            root(tabel.Key) = randuri
        Next
        Return root
    End Function

    ''' <summary>One cell as JSON: scalar ▸ string, list ▸ array, named set ▸ object.</summary>
    Public Shared Function CatreCelula(celula As CelulaTabel) As JsonNode
        If celula Is Nothing Then Return JsonValue.Create(String.Empty)
        Select Case celula.Felul
            Case CelulaTabel.Fel.Lista
                Dim arr As New JsonArray()
                For Each element As CelulaTabel In celula.Lista
                    arr.Add(CatreCelula(element))
                Next
                Return arr
            Case CelulaTabel.Fel.Obiect
                Dim obj As New JsonObject()
                For Each kvp As KeyValuePair(Of String, CelulaTabel) In celula.Obiect
                    obj(kvp.Key) = CatreCelula(kvp.Value)
                Next
                Return obj
            Case Else
                Return JsonValue.Create(celula.TextSau(String.Empty))
        End Select
    End Function

    ''' <summary>The inverse of <see cref="Catre"/>, for reading a saved payload back.</summary>
    Public Shared Function Din(nod As JsonNode) As Dictionary(Of String, TabelRezultat)
        Dim out As New Dictionary(Of String, TabelRezultat)(StringComparer.Ordinal)
        Dim root As JsonObject = TryCast(nod, JsonObject)
        If root Is Nothing Then Return out

        For Each tabel As KeyValuePair(Of String, JsonNode) In root
            Dim tinta As New TabelRezultat()
            Dim randuri As JsonArray = TryCast(tabel.Value, JsonArray)
            If randuri IsNot Nothing Then
                For Each nodRand As JsonNode In randuri
                    Dim obj As JsonObject = TryCast(nodRand, JsonObject)
                    If obj Is Nothing Then Continue For
                    Dim rand As New RandTabel()
                    For Each kvp As KeyValuePair(Of String, JsonNode) In obj
                        rand.Pune(kvp.Key, DinCelula(kvp.Value))
                    Next
                    tinta.Adauga(rand)
                Next
            End If
            out(tabel.Key) = tinta
        Next
        Return out
    End Function

    ''' <summary>One JSON node back into a cell.</summary>
    ''' <remarks>
    ''' Numbers and booleans come back as their TEXT, because that is what the scraper
    ''' produced and what every consumer parses (<c>parse_amount</c> and friends). A numeric
    ''' cell invented here would give the same column two shapes again — exactly what D-N
    ''' removed. <c>null</c> becomes the empty scalar: to a scraped table a missing value and
    ''' an empty one are the same thing, and a Nothing would only move the crash further away.
    ''' </remarks>
    Public Shared Function DinCelula(nod As JsonNode) As CelulaTabel
        If nod Is Nothing Then Return CelulaTabel.Gol

        Dim arr As JsonArray = TryCast(nod, JsonArray)
        If arr IsNot Nothing Then
            Dim elemente As New List(Of CelulaTabel)()
            For Each element As JsonNode In arr
                elemente.Add(DinCelula(element))
            Next
            Return CelulaTabel.DinLista(elemente)
        End If

        Dim obj As JsonObject = TryCast(nod, JsonObject)
        If obj IsNot Nothing Then
            Dim membri As New List(Of KeyValuePair(Of String, CelulaTabel))()
            For Each kvp As KeyValuePair(Of String, JsonNode) In obj
                membri.Add(New KeyValuePair(Of String, CelulaTabel)(kvp.Key, DinCelula(kvp.Value)))
            Next
            Return CelulaTabel.DinObiect(membri)
        End If

        Return CelulaTabel.DinText(nod.ToString())
    End Function
End Class
