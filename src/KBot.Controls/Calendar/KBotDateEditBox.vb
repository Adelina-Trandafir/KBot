Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The edit box inside <see cref="KBotDatePicker"/>: a <c>TextBox</c> that can be ANY height
''' and still shows its one line of text vertically centred.
'''
''' <para><b>Why a subclass at all.</b> A single-line <c>TextBox</c> cannot be made taller than
''' its font: with <c>AutoSize</c> off it accepts the height but draws the text at the top, and
''' the Windows edit control ignores <c>EM_SETRECT</c> unless it is multiline. So the box is
''' multiline — that is what frees its height, in the designer and at runtime — and the
''' formatting rectangle (<c>EM_SETRECT</c>) is moved to a one-line band in the middle of the
''' client area, which is where the text and the caret then live. Enter never reaches the edit
''' control (the picker suppresses it), so the box never grows a second line.</para>
'''
''' <para><b>The placeholder is drawn here, not by the base.</b> The framework draws
''' <c>PlaceholderText</c> at the TOP of a multiline box, which would leave the hint a line above
''' where the typed text sits. The base property is shadowed and kept empty, and the hint is
''' painted after <c>WM_PAINT</c> inside the same band the text uses, in the text colour pulled
''' halfway towards the background (C1: derived, never a stored grey).</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Public NotInheritable Class KBotDateEditBox
    Inherits TextBox

    Private Const WM_PAINT As Integer = &HF
    Private Const WM_SIZE As Integer = &H5
    Private Const WM_PRINT As Integer = &H317
    Private Const WM_PRINTCLIENT As Integer = &H318
    Private Const EM_SETRECT As Integer = &HB3
    Private Const EM_GETMARGINS As Integer = &HD4

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=False)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr,
                                        ByRef lParam As RECT) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=False)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr,
                                        lParam As IntPtr) As IntPtr
    End Function

    Private _placeholder As String = String.Empty

    Public Sub New()
        MyBase.Multiline = True
        MyBase.WordWrap = False
        MyBase.AcceptsReturn = False
        MyBase.AcceptsTab = False
        MyBase.BorderStyle = BorderStyle.None
    End Sub

    ''' <summary>
    ''' The hint shown while the box is empty and unfocused. Shadows the base property on
    ''' purpose: the base one stays empty so the framework never paints the hint at the top of
    ''' the box; this one is painted in the centred band (see the class summary).
    ''' </summary>
    <Category("K-BOT Date")>
    <Description("Text shown while the box is empty.")>
    <DefaultValue("")>
    Public Shadows Property PlaceholderText As String
        Get
            Return _placeholder
        End Get
        Set(value As String)
            Dim v As String = If(value, String.Empty)
            If _placeholder = v Then Return
            _placeholder = v
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Multiline is what frees the height, so it is not a choice: hidden from the grid and
    ''' pinned True. Writing False is refused rather than quietly ignored (C3).
    ''' </summary>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property Multiline As Boolean
        Get
            Return MyBase.Multiline
        End Get
        Set(value As Boolean)
            If Not value Then
                Throw New ArgumentException("Caseta de dată rămâne multilinie: așa își poate schimba înălțimea.", NameOf(value))
            End If
            MyBase.Multiline = True
        End Set
    End Property

    ''' <summary>The band the text is laid out in: one line tall, centred in the client area.</summary>
    Private Function TextBand() As Rectangle
        Dim client As Rectangle = ClientRectangle
        Dim line As Integer = LineHeight()
        If line <= 0 OrElse line >= client.Height Then Return client
        Dim top As Integer = client.Top + (client.Height - line) \ 2
        Return New Rectangle(client.Left, top, client.Width, line)
    End Function

    ' Measured on the control's own device context, so a per-monitor DPI change is honoured
    ' rather than the primary screen's answer from Font.Height.
    Private Function LineHeight() As Integer
        Try
            Using g As Graphics = CreateGraphics()
                Return CInt(Math.Ceiling(Font.GetHeight(g)))
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDateEditBox.LineHeight", ex)
            Return Font.Height
        End Try
    End Function

    ''' <summary>
    ''' Pushes the centred band into the edit control's formatting rectangle. The edit control
    ''' resets that rectangle on every size change, so this runs after each <c>WM_SIZE</c>, after
    ''' the handle is created and after a font change. Interop reached from layout: logs and
    ''' returns, a refused message must not take the form down.
    ''' </summary>
    Public Sub UpdateFormattingRect()
        Try
            If Not IsHandleCreated Then Return
            Dim band As Rectangle = TextBand()
            Dim r As New RECT With {
                .Left = band.Left,
                .Top = band.Top,
                .Right = Math.Max(band.Left + 1, band.Right),
                .Bottom = Math.Max(band.Top + 1, band.Bottom)
            }
            SendMessage(Handle, EM_SETRECT, IntPtr.Zero, r)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDateEditBox.UpdateFormattingRect", ex)
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        UpdateFormattingRect()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        UpdateFormattingRect()
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        If _placeholder.Length > 0 Then Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        If _placeholder.Length > 0 Then Invalidate()
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        If _placeholder.Length > 0 Then Invalidate()
    End Sub

    ' WndProc stays bare (house rule): the window's message contract is not wrapped, the net is
    ' Application.ThreadException. The one call that can throw, the hint painting, wraps itself.
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        Select Case m.Msg
            Case WM_SIZE
                UpdateFormattingRect()
            Case WM_PAINT
                If ShouldDrawPlaceholder() Then
                    Using g As Graphics = CreateGraphics()
                        DrawPlaceholder(g)
                    End Using
                End If
            Case WM_PRINT, WM_PRINTCLIENT
                ' DrawToBitmap and print previews come through here, with the target DC in
                ' wParam; without this branch the hint is on screen but missing from every
                ' rendering of the field.
                If ShouldDrawPlaceholder() AndAlso m.WParam <> IntPtr.Zero Then
                    Using g As Graphics = Graphics.FromHdc(m.WParam)
                        DrawPlaceholder(g)
                    End Using
                End If
        End Select
    End Sub

    Private Function ShouldDrawPlaceholder() As Boolean
        Return _placeholder.Length > 0 AndAlso Not Focused AndAlso TextLength = 0 AndAlso IsHandleCreated
    End Function

    ' The hint sits exactly where the typed text would: the centred band, past the edit
    ' control's own left margin (EM_GETMARGINS, low word), in the text colour pulled halfway
    ' towards the background so it reads as a hint on every scheme.
    Private Sub DrawPlaceholder(g As Graphics)
        Try
            Dim band As Rectangle = TextBand()
            Dim margins As Integer = SendMessage(Handle, EM_GETMARGINS, IntPtr.Zero, IntPtr.Zero).ToInt32()
            Dim leftMargin As Integer = margins And &HFFFF
            Dim area As New Rectangle(band.Left + leftMargin, band.Top,
                                      Math.Max(0, band.Width - leftMargin), band.Height)
            If area.Width <= 0 OrElse area.Height <= 0 Then Return
            Dim colour As Color = ThemeShapes.Blend(ForeColor, BackColor, 0.5)
            ' Same horizontal alignment as the typed text, so the hint sits where the text will.
            Dim align As TextFormatFlags
            Select Case TextAlign
                Case HorizontalAlignment.Center : align = TextFormatFlags.HorizontalCenter
                Case HorizontalAlignment.Right : align = TextFormatFlags.Right
                Case Else : align = TextFormatFlags.Left
            End Select
            TextRenderer.DrawText(g, _placeholder, Font, area, colour, BackColor,
                                  TextFormatFlags.NoPadding Or align Or
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                  TextFormatFlags.EndEllipsis)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDateEditBox.DrawPlaceholder", ex)
        End Try
    End Sub

End Class
