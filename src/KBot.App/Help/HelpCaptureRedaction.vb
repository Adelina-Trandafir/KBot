Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: what a help capture blurs before it is saved, so a picture taken on a real
''' database does not carry anyone's data into the manual. Blurred, as the operator asked
''' (30.09.2026) -- the TEXT only, never a whole bar or menu:
''' <list type="bullet">
''' <item>the login's user name (the e-mail) and the unit's name, wherever they are written;</item>
''' <item>the names of the other units in the caption bar's unit list;</item>
''' <item>anything that starts with «RO» followed by digits (an IBAN, a fiscal code with RO);</item>
''' <item>13 digits in a row (a CNP);</item>
''' <item>(operator's yes, same day) a fiscal code WITHOUT «RO» (2-10 digits), but only in a field or
''' column that is one -- its name or header says «cod fiscal», «CUI» or «CIF» -- so ordinary numbers
''' stay sharp;</item>
''' <item>(same) any e-mail address and any Romanian phone number (07.., 02.., 03.., +40 / 0040).</item>
''' </list>
''' Any other category is added only after asking the operator.
''' Where the text is: a control's <c>Text</c> for ordinary controls (labels, boxes, buttons), the
''' items of a list box, and <see cref="IKBotCaptureRedaction"/> for the controls that paint their
''' own text (tree, grid, caption bar, menus). What K-BOT cannot read -- a PDF in Adobe, the FOREXE
''' page, a picture loaded from disk -- is not blurred.
''' </summary>
Friend NotInheritable Class HelpCaptureRedaction

    ' RO + a digit, then letters / digits (single spaces allowed, as IBANs are often written).
    Private Shared ReadOnly AccountPattern As New Regex("(?<![A-Za-z0-9])RO ?\d(?: ?[0-9A-Z]){5,31}(?![A-Za-z0-9])",
                                                        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly CnpPattern As New Regex("(?<!\d)\d{13}(?!\d)", RegexOptions.CultureInvariant)
    Private Shared ReadOnly EmailPattern As New Regex("[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.CultureInvariant)
    ' +40 / 0040 / 0, then 2, 3 or 7 and eight more digits; spaces, dots or dashes between groups.
    Private Shared ReadOnly PhonePattern As New Regex("(?<![\d+])(?:\+40|0040|0)[ .\-]?[237](?:[ .\-]?\d){8}(?!\d)", RegexOptions.CultureInvariant)
    ' A classification code: seven dot-separated pairs of digits (e.g. 20.01.01.03.00.00.00).
    Private Shared ReadOnly ClassificationPattern As New Regex("(?<![\d.])\d{2}(?:\.\d{2}){6}(?![\d.])", RegexOptions.CultureInvariant)
    ' A field or column that holds a fiscal code, once its name / header is split and lower-cased.
    Private Shared ReadOnly FiscalContextPattern As New Regex("\b(cod ?fiscal|cui|cif)\b", RegexOptions.CultureInvariant)
    ' What a fiscal code looks like on its own (with or without RO).
    Private Shared ReadOnly FiscalValuePattern As New Regex("^\s*(?:RO\s?)?\d{2,10}\s*$", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    ' Names shorter than this are not searched for: «AB» would blur half the screen.
    Private Const MinTermLength As Integer = 3

    Private ReadOnly _terms As New List(Of String)()

    Public Sub New(session As SessionContext)
        If session IsNot Nothing Then
            AddTerm(session.OperatorName)
            AddTerm(session.NumeUnitate)
        End If
        ' The operator's other units, as the caption bar's unit list shows them.
        For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
            For Each bar As KBot.Controls.KBotCaptionBar In OfType(Of KBot.Controls.KBotCaptionBar)(f)
                For Each t As String In bar.SelectorTexts
                    AddTerm(t)
                Next
            Next
        Next
    End Sub

    Private Sub AddTerm(term As String)
        Dim t As String = If(term, String.Empty).Trim()
        If t.Length >= MinTermLength AndAlso Not _terms.Contains(t, StringComparer.OrdinalIgnoreCase) Then _terms.Add(t)
    End Sub

    ''' <summary>
    ''' True when <paramref name="text"/> holds any of the things listed on the class.
    ''' <paramref name="context"/>: what the text is (a column's key and header, a control's name),
    ''' empty when unknown; it matters only for the fiscal code without «RO».
    ''' </summary>
    Public Function IsSensitive(context As String, text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then Return False
        ' Budget classifications are public and must stay readable (the phone pattern would take them).
        Dim scanned As String = ClassificationPattern.Replace(text, " ")
        If AccountPattern.IsMatch(scanned) OrElse CnpPattern.IsMatch(scanned) OrElse
           EmailPattern.IsMatch(scanned) OrElse PhonePattern.IsMatch(scanned) Then Return True
        If FiscalValuePattern.IsMatch(text) AndAlso IsFiscalContext(context) Then Return True
        For Each t As String In _terms
            If text.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next
        Return False
    End Function

    ' «txtCodFiscal», «COD_FISCAL», «Cod fiscal», «CUI» -> "txt cod fiscal", "cod fiscal", "cui".
    Private Shared Function IsFiscalContext(context As String) As Boolean
        If String.IsNullOrWhiteSpace(context) Then Return False
        Dim split As String = Regex.Replace(context, "(?<=[a-z])(?=[A-Z])", " ").Replace("_"c, " "c).ToLowerInvariant()
        Return FiscalContextPattern.IsMatch(split) OrElse split.Contains("codfiscal")
    End Function

    ''' <summary>
    ''' Every place on screen (screen coordinates) where a visible K-BOT window shows sensitive
    ''' text now. <paramref name="skip"/>: windows left out (the capture list itself).
    ''' </summary>
    Public Function ScreenRegions(skip As ICollection(Of IntPtr)) As List(Of Rectangle)
        Try
            Dim result As New List(Of Rectangle)()
            For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
                If Not f.Visible OrElse f.WindowState = FormWindowState.Minimized Then Continue For
                If skip IsNot Nothing AndAlso skip.Contains(f.Handle) Then Continue For
                Walk(f, result)
            Next
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureRedaction.ScreenRegions", ex)
            Throw
        End Try
    End Function

    Private Sub Walk(c As Control, into As List(Of Rectangle))
        If Not c.Visible OrElse c.Width <= 0 OrElse c.Height <= 0 Then Return
        Dim painter As IKBotCaptureRedaction = TryCast(c, IKBotCaptureRedaction)
        If painter IsNot Nothing Then
            For Each r As Rectangle In painter.SensitiveRegions(AddressOf IsSensitive)
                If r.Width > 0 AndAlso r.Height > 0 Then into.Add(c.RectangleToScreen(r))
            Next
        ElseIf TypeOf c Is ListBox Then
            Dim list As ListBox = DirectCast(c, ListBox)
            For i As Integer = 0 To list.Items.Count - 1
                If Not IsSensitive(list.Name, list.GetItemText(list.Items(i))) Then Continue For
                Dim r As Rectangle = list.GetItemRectangle(i)
                r.Intersect(list.ClientRectangle)
                If Not r.IsEmpty Then into.Add(list.RectangleToScreen(r))
            Next
        ElseIf ShowsItsText(c) AndAlso IsSensitive(c.Name, c.Text) Then
            Dim r As Rectangle = TextBounds(c)
            If Not r.IsEmpty Then into.Add(c.RectangleToScreen(r))
        End If
        For Each child As Control In c.Controls
            Walk(child, into)
        Next
    End Sub

    ' Containers do not write their Text on screen (a form's is its taskbar title; the caption bar
    ' paints the visible one), so only the controls that do are read.
    Private Shared Function ShowsItsText(c As Control) As Boolean
        Return Not (TypeOf c Is Form OrElse TypeOf c Is UserControl OrElse TypeOf c Is Panel OrElse
                    TypeOf c Is TabPage OrElse TypeOf c Is SplitContainer OrElse TypeOf c Is TableLayoutPanel OrElse
                    TypeOf c Is FlowLayoutPanel OrElse TypeOf c Is WebBrowser)
    End Function

    ' Where the text is inside the control (client coordinates): for a label, the measured text
    ' placed by its alignment; for a text box, the line(s) it writes from the left; for anything
    ' else (buttons, combos) the inside of the control, a little in from its edge.
    Private Shared Function TextBounds(c As Control) As Rectangle
        Dim client As Rectangle = c.ClientRectangle
        If TypeOf c Is Label Then
            Dim lbl As Label = DirectCast(c, Label)
            Dim flags As TextFormatFlags = TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix
            Dim sz As Size = TextRenderer.MeasureText(lbl.Text, lbl.Font, New Size(Math.Max(1, client.Width), Integer.MaxValue), flags)
            sz = New Size(Math.Min(sz.Width, client.Width), Math.Min(sz.Height, client.Height))
            Dim x As Integer = client.Left
            Dim y As Integer = client.Top
            Select Case lbl.TextAlign
                Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                    x += (client.Width - sz.Width) \ 2
                Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                    x += client.Width - sz.Width
            End Select
            Select Case lbl.TextAlign
                Case ContentAlignment.MiddleLeft, ContentAlignment.MiddleCenter, ContentAlignment.MiddleRight
                    y += (client.Height - sz.Height) \ 2
                Case ContentAlignment.BottomLeft, ContentAlignment.BottomCenter, ContentAlignment.BottomRight
                    y += client.Height - sz.Height
            End Select
            Return New Rectangle(x, y, sz.Width, sz.Height)
        End If
        If TypeOf c Is TextBoxBase AndAlso Not DirectCast(c, TextBoxBase).Multiline Then
            Dim w As Integer = Math.Min(client.Width - 2, TextRenderer.MeasureText(c.Text, c.Font).Width)
            Dim h As Integer = Math.Min(client.Height, c.Font.Height + 2)
            Return New Rectangle(1, (client.Height - h) \ 2, Math.Max(0, w), h)
        End If
        Return Rectangle.Inflate(client, -2, -2)
    End Function

    ''' <summary>
    ''' Blurs <paramref name="regions"/> (screen coordinates) on <paramref name="shot"/>, a picture
    ''' whose (0,0) is the screen point <paramref name="origin"/>. Returns how many were blurred.
    ''' The blur is strong enough that no letter can be read back: each area is shrunk to a
    ''' sixth and stretched back, twice.
    ''' </summary>
    Public Shared Function Blur(shot As Bitmap, origin As Point, regions As IEnumerable(Of Rectangle)) As Integer
        Try
            Dim count As Integer = 0
            Dim bounds As New Rectangle(Point.Empty, shot.Size)
            Using g As Graphics = Graphics.FromImage(shot)
                g.InterpolationMode = InterpolationMode.HighQualityBilinear
                g.PixelOffsetMode = PixelOffsetMode.HighQuality
                For Each screenRect As Rectangle In regions
                    Dim r As Rectangle = Rectangle.Inflate(screenRect, 2, 2)
                    r.Offset(-origin.X, -origin.Y)
                    r.Intersect(bounds)
                    If r.Width < 2 OrElse r.Height < 2 Then Continue For
                    For pass As Integer = 1 To 2
                        Using part As Bitmap = shot.Clone(r, shot.PixelFormat)
                            Using small As New Bitmap(Math.Max(1, r.Width \ 6), Math.Max(1, r.Height \ 6))
                                Using gs As Graphics = Graphics.FromImage(small)
                                    gs.InterpolationMode = InterpolationMode.HighQualityBilinear
                                    gs.DrawImage(part, New Rectangle(Point.Empty, small.Size))
                                End Using
                                g.DrawImage(small, r)
                            End Using
                        End Using
                    Next
                    count += 1
                Next
            End Using
            Return count
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureRedaction.Blur", ex)
            Throw
        End Try
    End Function

    Private Shared Iterator Function OfType(Of T As Control)(root As Control) As IEnumerable(Of T)
        If TypeOf root Is T Then Yield DirectCast(root, T)
        For Each child As Control In root.Controls
            For Each hit As T In OfType(Of T)(child)
                Yield hit
            Next
        Next
    End Function

End Class
