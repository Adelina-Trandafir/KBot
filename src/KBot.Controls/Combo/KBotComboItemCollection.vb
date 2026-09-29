Option Strict On
Imports System.Collections

''' <summary>
''' The items of a <see cref="KBotComboBox"/>: a plain list of objects, shown by their caption
''' (<see cref="KBotComboBox.CaptionSelector"/> or <c>ToString()</c>). Every change is reported to
''' the owner so the selection stays on the same item (an insert before it shifts the index, a
''' removal of it clears the selection) -- the same contract as <c>ComboBox.ObjectCollection</c>,
''' without any data binding behind it.
''' </summary>
Public NotInheritable Class KBotComboItemCollection
    Implements IList

    Private ReadOnly _owner As KBotComboBox
    Private ReadOnly _items As New List(Of Object)()

    Friend Sub New(owner As KBotComboBox)
        ArgumentNullException.ThrowIfNull(owner)
        _owner = owner
    End Sub

    ''' <summary>The item at <paramref name="index"/>. Nothing is refused.</summary>
    Default Public Property Item(index As Integer) As Object Implements IList.Item
        Get
            Return _items(index)
        End Get
        Set(value As Object)
            ArgumentNullException.ThrowIfNull(value)
            _items(index) = value
            _owner.OnItemReplaced(index)
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection.Count
        Get
            Return _items.Count
        End Get
    End Property

    ''' <summary>Adds one item at the end; returns its index.</summary>
    Public Function Add(value As Object) As Integer Implements IList.Add
        ArgumentNullException.ThrowIfNull(value)
        _items.Add(value)
        Dim index As Integer = _items.Count - 1
        _owner.OnItemInserted(index)
        Return index
    End Function

    ''' <summary>Adds every element of <paramref name="values"/> at the end (one notification).</summary>
    Public Sub AddRange(values As IEnumerable)
        ArgumentNullException.ThrowIfNull(values)
        Dim added As New List(Of Object)()
        For Each v As Object In values
            ArgumentNullException.ThrowIfNull(v, NameOf(values))
            added.Add(v)
        Next
        If added.Count = 0 Then Return
        _items.AddRange(added)
        _owner.OnItemsAppended()
    End Sub

    Public Sub Insert(index As Integer, value As Object) Implements IList.Insert
        ArgumentNullException.ThrowIfNull(value)
        _items.Insert(index, value)
        _owner.OnItemInserted(index)
    End Sub

    Public Sub Remove(value As Object) Implements IList.Remove
        Dim index As Integer = _items.IndexOf(value)
        If index >= 0 Then RemoveAt(index)
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList.RemoveAt
        _items.RemoveAt(index)
        _owner.OnItemRemoved(index)
    End Sub

    Public Sub Clear() Implements IList.Clear
        If _items.Count = 0 Then Return
        _items.Clear()
        _owner.OnItemsCleared()
    End Sub

    Public Function Contains(value As Object) As Boolean Implements IList.Contains
        Return _items.Contains(value)
    End Function

    Public Function IndexOf(value As Object) As Integer Implements IList.IndexOf
        Return _items.IndexOf(value)
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        CType(_items, ICollection).CopyTo(array, index)
    End Sub

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return _items.GetEnumerator()
    End Function

    Public ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
        Get
            Return False
        End Get
    End Property

    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
        Get
            Return False
        End Get
    End Property

    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
        Get
            Return CType(_items, ICollection).SyncRoot
        End Get
    End Property
End Class
