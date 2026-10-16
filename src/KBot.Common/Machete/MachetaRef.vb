Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks

''' <summary>
''' The identity of one opening of a «macheta» (a form of the application): its type plus the
''' arguments that make it THIS one -- <c>ddf</c> with <c>cod=AAB2EF77C9X</c>, a view with the
''' angajament and the year. Two references with the same type and arguments are the same macheta.
''' No UI here: the registry and the history work for any application built on K-BOT's shell.
''' </summary>
Public NotInheritable Class MachetaRef

    Private ReadOnly _args As New SortedDictionary(Of String, String)(StringComparer.Ordinal)

    Public Sub New(k_typeKey As String, Optional k_args As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing)
        If String.IsNullOrWhiteSpace(k_typeKey) Then Throw New ArgumentException("A macheta reference needs a type key.", NameOf(k_typeKey))
        TypeKey = k_typeKey
        If k_args IsNot Nothing Then
            For Each k_pair As KeyValuePair(Of String, String) In k_args
                If String.IsNullOrWhiteSpace(k_pair.Key) Then Throw New ArgumentException("A macheta argument has no name.", NameOf(k_args))
                _args(k_pair.Key) = If(k_pair.Value, String.Empty)
            Next
        End If
    End Sub

    ''' <summary>Convenience: <c>MachetaRef.Create("ddf", "cod", "AAB2", "an", "2026")</c> -- name, value, name, value...</summary>
    Public Shared Function Create(k_typeKey As String, ParamArray k_pairs As String()) As MachetaRef
        Dim k_list As New List(Of KeyValuePair(Of String, String))()
        If k_pairs IsNot Nothing Then
            If k_pairs.Length Mod 2 <> 0 Then Throw New ArgumentException("Macheta arguments come as name, value pairs.", NameOf(k_pairs))
            For k_i As Integer = 0 To k_pairs.Length - 1 Step 2
                k_list.Add(New KeyValuePair(Of String, String)(k_pairs(k_i), k_pairs(k_i + 1)))
            Next
        End If
        Return New MachetaRef(k_typeKey, k_list)
    End Function

    Public ReadOnly Property TypeKey As String

    Public ReadOnly Property Args As IReadOnlyDictionary(Of String, String)
        Get
            Return _args
        End Get
    End Property

    ''' <summary>The value of an argument; empty when the reference has none of that name.</summary>
    Public Function Arg(k_name As String) As String
        Dim k_value As String = Nothing
        Return If(_args.TryGetValue(k_name, k_value), k_value, String.Empty)
    End Function

    ''' <summary>Stable text form (<c>type?a=1&amp;b=2</c>, arguments in name order): the key of the history.</summary>
    Public ReadOnly Property Id As String
        Get
            If _args.Count = 0 Then Return TypeKey
            Return TypeKey & "?" & String.Join("&", _args.Select(Function(k_p) k_p.Key & "=" & k_p.Value))
        End Get
    End Property

    Public Overrides Function Equals(k_other As Object) As Boolean
        Dim k_ref As MachetaRef = TryCast(k_other, MachetaRef)
        Return k_ref IsNot Nothing AndAlso String.Equals(Id, k_ref.Id, StringComparison.Ordinal)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return StringComparer.Ordinal.GetHashCode(Id)
    End Function

    Public Overrides Function ToString() As String
        Return Id
    End Function
End Class

''' <summary>
''' A kind of macheta, registered once: how it is called, and how it is opened again from a
''' reference. The opener answers True when the macheta was actually shown.
''' </summary>
Public NotInheritable Class MachetaType

    Private ReadOnly _titleFor As Func(Of MachetaRef, String)

    Public Sub New(k_key As String, k_title As String, k_iconKey As String,
                   k_opener As Func(Of MachetaRef, Task(Of Boolean)),
                   Optional k_titleFor As Func(Of MachetaRef, String) = Nothing,
                   Optional k_inHistory As Boolean = True)
        If String.IsNullOrWhiteSpace(k_key) Then Throw New ArgumentException("A macheta type needs a key.", NameOf(k_key))
        ArgumentNullException.ThrowIfNull(k_opener)
        Key = k_key
        Title = If(k_title, k_key)
        IconKey = k_iconKey
        Opener = k_opener
        _titleFor = k_titleFor
        InHistory = k_inHistory
    End Sub

    Public ReadOnly Property Key As String
    ''' <summary>The name of the kind («Extrase de cont», «Sumar»).</summary>
    Public ReadOnly Property Title As String
    ''' <summary>A key the UI maps to an icon; free text for the registry.</summary>
    Public ReadOnly Property IconKey As String
    Public ReadOnly Property Opener As Func(Of MachetaRef, Task(Of Boolean))

    ''' <summary>
    ''' False = never listed in the history of machete (slice 0113-01: the modal windows -- editors, links, charts -- are answered
    ''' and gone; they are not machete one comes back to).
    ''' </summary>
    Public ReadOnly Property InHistory As Boolean

    ''' <summary>The line shown in the history for one reference («Sumar · AAB2EF77C9X»).</summary>
    Public Function TitleFor(k_ref As MachetaRef) As String
        Return If(_titleFor Is Nothing, Title, _titleFor(k_ref))
    End Function
End Class

''' <summary>One line of the history, as stored (public settable properties: it is serialized).</summary>
Public NotInheritable Class MachetaEntry

    Public Property Id As String = String.Empty
    Public Property TypeKey As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Args As Dictionary(Of String, String) = New Dictionary(Of String, String)()
    Public Property Moment As Date

    ''' <summary>True while the macheta is open (slice 0113-01). Open entries come first, in the order they were opened; closed ones follow, the one closed last at the end.</summary>
    Public Property IsOpen As Boolean

    Public Function ToRef() As MachetaRef
        Return New MachetaRef(TypeKey, Args)
    End Function
End Class
