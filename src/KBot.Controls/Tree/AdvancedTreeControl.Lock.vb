Imports System.ComponentModel

Partial Public Class AdvancedTreeControl

    ' Slice 0078-08: while True, a click (or arrow key) cannot change the selected row -- the view
    ' behind the tree is still opening the document of the row selected before.
    Private _selectionLocked As Boolean
    ' The press that started while locked: its release is swallowed too, even if the lock ended
    ' in between (otherwise the release alone would raise NodeMouseUp on a row never selected).
    Private _pressWhileLocked As Boolean

    ''' <summary>
    ''' Slice 0078-08: rows cannot be selected while True -- mouse presses, double clicks and
    ''' navigation keys on rows are ignored and the cursor shows the application is working.
    ''' Scrolling, the header and the footer keep working. Set at run time only.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectionLocked As Boolean
        Get
            Return _selectionLocked
        End Get
        Set(value As Boolean)
            If _selectionLocked = value Then Return
            _selectionLocked = value
            Me.Cursor = If(value, Cursors.AppStarting, Cursors.Default)
        End Set
    End Property

End Class
