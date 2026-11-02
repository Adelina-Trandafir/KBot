Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' The editor's vertical scroll bar: a themed KBotScrollBar standing beside the editing surface
' (the native bar is a window Windows paints itself, so no palette colour could reach it).
' The native horizontal bar, if the operator asks for one through ScrollBars, stays native.
Partial Public Class KBotRichTextEditor

    Private _scrollBars As RichTextBoxScrollBars = RichTextBoxScrollBars.Vertical
    Private _syncingBar As Boolean

    ''' <summary>True when our vertical bar is wanted at all (any mode that has a vertical bar).</summary>
    Private Function WantsVerticalBar() As Boolean
        Return _scrollBars = RichTextBoxScrollBars.Vertical OrElse _scrollBars = RichTextBoxScrollBars.ForcedVertical OrElse
               _scrollBars = RichTextBoxScrollBars.Both OrElse _scrollBars = RichTextBoxScrollBars.ForcedBoth
    End Function

    ''' <summary>True when the vertical bar stays even with nothing to scroll.</summary>
    Private Function ForcesVerticalBar() As Boolean
        Return _scrollBars = RichTextBoxScrollBars.ForcedVertical OrElse _scrollBars = RichTextBoxScrollBars.ForcedBoth
    End Function

    ''' <summary>What the native box is left with: only the horizontal bar, when one was asked for.</summary>
    Private Function NativeScrollBars() As RichTextBoxScrollBars
        Select Case _scrollBars
            Case RichTextBoxScrollBars.Horizontal, RichTextBoxScrollBars.Both
                Return RichTextBoxScrollBars.Horizontal
            Case RichTextBoxScrollBars.ForcedHorizontal, RichTextBoxScrollBars.ForcedBoth
                Return RichTextBoxScrollBars.ForcedHorizontal
            Case Else
                Return RichTextBoxScrollBars.None
        End Select
    End Function

    ''' <summary>Width of the bar in device px when it takes part in the layout, else 0.</summary>
    Private Function VerticalBarWidthPx() As Integer
        Return If(vBar.Visible, Sc(KBotScrollBar.GrosimeImplicita), 0)
    End Function

    ''' <summary>Rereads the editing surface and puts the bar on it (range, position, visibility).
    ''' UI boundary: logs and swallows.</summary>
    Private Sub SyncVerticalBar()
        Try
            If _syncingBar OrElse _layingOut OrElse rtb Is Nothing OrElse Not rtb.IsHandleCreated Then Return
            _syncingBar = True
            Try
                ' A bar that appears / disappears changes the width of the text, so the surface is
                ' laid out again and read a second time (it settles: a narrower surface is only taller).
                If ReadIntoVerticalBar() Then
                    RebuildLayout()
                    ReadIntoVerticalBar()
                End If
            Finally
                _syncingBar = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.SyncVerticalBar", ex)
        End Try
    End Sub

    ''' <summary>Puts range, position and visibility on the bar; True when its visibility changed.</summary>
    Private Function ReadIntoVerticalBar() As Boolean
        Dim k_show As Boolean = False
        If WantsVerticalBar() AndAlso rtb.Visible Then
                    Dim k_pad As Padding = ScPadding(_editorPadding)
                    Dim k_scrollY As Integer = rtb.ScrollY
                    Dim k_viewport As Integer = Math.Max(1, rtb.ClientSize.Height - k_pad.Top - k_pad.Bottom)
                    ' Document height = distance from the top of the document to the end of the last
                    ' line: its client Y, the scroll offset, minus the padding the first line starts at.
                    Dim k_lastLine As Integer = Math.Max(1, rtb.Font.Height)
                    Dim k_endY As Integer = rtb.GetPositionFromCharIndex(rtb.TextLength).Y
                    Dim k_content As Integer = Math.Max(k_viewport, k_endY + k_scrollY - k_pad.Top + k_lastLine)
                    vBar.SmallChange = k_lastLine
                    vBar.SetRange(0, k_content - 1, k_viewport, k_scrollY)
                    k_show = ForcesVerticalBar() OrElse vBar.IsScrollable
        End If
        If vBar.Visible = k_show Then Return False
        vBar.Visible = k_show
        Return True
    End Function

    ' The operator moved the bar: scroll the surface to it, then read back where it really went
    ' (the surface clamps), so the thumb never stays beside the text.
    Private Sub VBar_Scroll(sender As Object, e As ScrollEventArgs) Handles vBar.Scroll
        Try
            If _syncingBar OrElse Not rtb.IsHandleCreated Then Return
            rtb.ScrollY = vBar.Value
            _syncingBar = True
            Try
                vBar.Value = rtb.ScrollY
            Finally
                _syncingBar = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.VBar_Scroll", ex)
        End Try
    End Sub

    Private Sub Rtb_WheelTurned(sender As Object, e As MouseEventArgs) Handles rtb.WheelTurned
        Try
            If Not vBar.Visible Then Return
            Dim k_lines As Integer = SystemInformation.MouseWheelScrollLines
            If k_lines <= 0 Then k_lines = 3
            Dim k_notches As Integer = e.Delta \ SystemInformation.MouseWheelScrollDelta
            If k_notches = 0 Then Return
            rtb.ScrollY = Math.Max(0, Math.Min(vBar.MaxValue, rtb.ScrollY - k_notches * k_lines * vBar.SmallChange))
            SyncVerticalBar()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.Rtb_WheelTurned", ex)
        End Try
    End Sub

    Private Sub Rtb_ViewChanged(sender As Object, e As EventArgs) Handles rtb.ViewChanged
        SyncVerticalBar()
    End Sub

    ' New text or a changed font changes the document height.
    Private Sub Rtb_TextChangedBar(sender As Object, e As EventArgs) Handles rtb.TextChanged
        SyncVerticalBar()
    End Sub
End Class
