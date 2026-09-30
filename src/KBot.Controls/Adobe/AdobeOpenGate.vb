Option Strict On
Imports System.Collections.Generic
Imports KBot.Common

''' <summary>
''' Slice 0078-08 (operator, 30.09.2026): «while a document is still opening in Adobe, no other row
''' may be clicked». Sending a second document to Adobe while the first is still loading produced
''' random Adobe errors and left Adobe processes behind.
'''
''' One gate for the whole application, because the click that sends a new document can come from
''' several trees (the main angajamente tree, the DDF / ORD / CAB-note trees). A preview that sends a
''' document to Adobe <see cref="Enter"/>s with itself as the owner and <see cref="Leave"/>s when
''' Adobe reports the document done (<see cref="AdobeReaderHost.DocumentReady"/>), when showing it
''' failed, or when it is cleared. The gate is busy while any owner is inside. UI thread only.
''' </summary>
Public NotInheritable Class AdobeOpenGate

    Private Shared ReadOnly _owners As New HashSet(Of Object)()

    ''' <summary>Raised (UI thread) when the gate goes from free to busy (True) or back (False).</summary>
    Public Shared Event BusyChanged As Action(Of Boolean)

    Private Sub New()
    End Sub

    ''' <summary>A document is being opened in Adobe right now.</summary>
    Public Shared ReadOnly Property IsBusy As Boolean
        Get
            Return _owners.Count > 0
        End Get
    End Property

    ''' <summary><paramref name="owner"/> starts opening a document. Entering twice is one entry.</summary>
    Public Shared Sub Enter(owner As Object)
        If owner Is Nothing Then Throw New ArgumentNullException(NameOf(owner))
        If _owners.Add(owner) AndAlso _owners.Count = 1 Then Notify(True)
    End Sub

    ''' <summary><paramref name="owner"/> is done (or gave up). Safe when it never entered.</summary>
    Public Shared Sub Leave(owner As Object)
        If owner Is Nothing Then Return
        If _owners.Remove(owner) AndAlso _owners.Count = 0 Then Notify(False)
    End Sub

    ''' <summary>
    ''' Locks <paramref name="tree"/>'s rows while the gate is busy (<see cref="AdvancedTreeControl.SelectionLocked"/>)
    ''' for as long as the tree lives.
    ''' </summary>
    Public Shared Sub LockWhileOpening(tree As AdvancedTreeControl)
        If tree Is Nothing Then Throw New ArgumentNullException(NameOf(tree))
        Dim handler As Action(Of Boolean) = Sub(busy) tree.SelectionLocked = busy
        AddHandler BusyChanged, handler
        AddHandler tree.Disposed, Sub(s, e) RemoveHandler BusyChanged, handler
        tree.SelectionLocked = IsBusy
    End Sub

    ' A tree that fails to lock must not break the preview that raised the change: logged only.
    Private Shared Sub Notify(busy As Boolean)
        Try
            RaiseEvent BusyChanged(busy)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeOpenGate.Notify", ex)
        End Try
    End Sub

End Class
