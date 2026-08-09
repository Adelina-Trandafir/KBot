Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Culorile, măsurile și bucățile de text cu care <see cref="TreeNodeFlyout"/> pictează un nod.
''' Le calculează ARBORELE (el știe regulile de selecție/hover și geometria rândului), fereastra
''' doar le folosește — altfel aceleași reguli ar trăi în două locuri.
'''
''' Fonturile și pictograma sunt ÎMPRUMUTATE: fereastra nu le deține și nu le eliberează.
''' </summary>
Friend NotInheritable Class TreeNodeFlyoutStyle
    Public Fill As Color
    Public Border As Color
    Public Radius As Integer
    ''' <summary>Înălțimea rândului = exact <c>ItemHeight</c> al arborelui.</summary>
    Public ItemHeight As Integer
    ''' <summary>Dreptunghiul iconiței din stânga, în coordonatele rândului. Gol = fără iconiță.</summary>
    Public IconRect As Rectangle
    Public Icon As Image
    ''' <summary>X-ul de la care începe textul — ACELAȘI ca în arbore, ca nodul să pară că se desface.</summary>
    Public TextX As Integer
    Public Parts As List(Of AdvancedTreeControl.RichTextPart)
End Class

''' <summary>
''' Nodul plutitor al arborelui STRÂNS (subsol, felia butonului de strângere): fereastra care iese
''' spre dreapta când cursorul stă pe un nod al unui <see cref="AdvancedTreeControl"/> strâns la
''' <c>MinimumCollapsedWidth</c>. Sora lui <c>KBotNavFlyout</c> din bara de navigare, cu aceleași
''' trucuri și din aceleași motive.
'''
''' NU e un <c>ToolTip</c>: desenează RÂNDUL ÎNTREG — aceeași iconiță, la același X, același fundal
''' de hover/selecție, același text îmbogățit — cu culorile arborelui, primite gata calculate în
''' <see cref="TreeNodeFlyoutStyle"/>. Fereastra pleacă exact din dreptunghiul rândului strâns și
''' crește DOAR spre dreapta, deci cursorul vede un nod care se desfășoară, nu o etichetă lipită.
'''
''' <para><b>Fereastră care nu se bagă în seamă.</b> <c>WS_EX_NOACTIVATE</c> +
''' <see cref="ShowWithoutActivation"/> (nu fură focusul), <c>WS_EX_TOOLWINDOW</c> (fără buton în
''' bara de activități) și <c>HTTRANSPARENT</c> pe <c>WM_NCHITTEST</c> — mouse-ul TRECE PRIN ea la
''' arborele de dedesubt. Ultima parte nu e cosmetică: fereastra acoperă chiar rândul peste care
''' stă cursorul, deci fără ea hover-ul s-ar pierde în clipa în care apare, ceea ce ar ascunde-o,
''' ceea ce ar readuce hover-ul… la infinit.</para>
''' </summary>
Friend NotInheritable Class TreeNodeFlyout
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTTRANSPARENT As Integer = -1
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    Private _style As TreeNodeFlyoutStyle

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' Fără autoscalare: arborele ne dă Bounds în px DEJA scalați; o a doua ajustare ar muta
        ' fereastra de pe rândul din care trebuie să pară că iese.
        AutoScaleMode = AutoScaleMode.None
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW
            Return cp
        End Get
    End Property

    ' Regula casei lasă WndProc pe plasa globală Application.ThreadException: un Try/Catch aici ar
    ' risca să rupă contractul de mesaje al ferestrei.
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = WM_NCHITTEST Then m.Result = New IntPtr(HTTRANSPARENT)
    End Sub

    ''' <summary>Ce se pictează. Se cheamă la fiecare cadru al desfășurării.</summary>
    Friend Sub SetContent(style As TreeNodeFlyoutStyle)
        _style = style
        If style IsNot Nothing Then BackColor = style.Fill
        Invalidate()
    End Sub

    ' Colțurile rotunjite se taie din FEREASTRĂ, nu doar din desen: altfel în colțuri s-ar vedea
    ' dreptunghiul ferestrei peste arborele de dedesubt.
    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        Try
            Dim radius As Integer = If(_style Is Nothing, 0, _style.Radius)
            If radius <= 0 OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then
                Region = Nothing
                Return
            End If
            Using path As GraphicsPath = ThemeShapes.RoundedRect(
                    New Rectangle(0, 0, ClientSize.Width, ClientSize.Height), radius)
                Region = New Region(path)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TreeNodeFlyout.OnSizeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim st As TreeNodeFlyoutStyle = _style
            If st Is Nothing Then Return
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' -1 pe fiecare latură ca și conturul să intre în fereastră, nu pe jumătate în afara ei.
            Dim r As New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 1), Math.Max(1, ClientSize.Height - 1))
            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, Math.Max(1, st.Radius))
                Using b As New SolidBrush(st.Fill)
                    g.FillPath(b, path)
                End Using
                ' Conturul e ce o desprinde de arborele de dedesubt — fundalul singur ar face-o să
                ' pară o pată de aceeași culoare cu rândul.
                Using pen As New Pen(st.Border)
                    g.DrawPath(pen, path)
                End Using
            End Using

            ' Iconița: la ACELAȘI X ca în arbore, adică exact peste locul unde e deja desenată.
            ' Asta e ce face desfășurarea să pară a nodului, nu a unei etichete de alături.
            If st.Icon IsNot Nothing AndAlso Not st.IconRect.IsEmpty Then
                g.DrawImage(st.Icon, st.IconRect)
            End If

            If st.Parts Is Nothing OrElse st.Parts.Count = 0 Then Return

            ' Textul: aceleași bucăți îmbogățite ca în rând, desenate una după alta de la TextX.
            ' Cât timp desfășurarea e la început fereastra e mai îngustă decât textul — se taie
            ' singur pe marginea ferestrei, fără caz special.
            Dim fmt As StringFormat = StringFormat.GenericTypographic
            fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
            Dim cx As Single = st.TextX
            For Each part In st.Parts
                Dim sz As SizeF = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
                If part.HasBackColor Then
                    Using b As New SolidBrush(part.BackColor)
                        g.FillRectangle(b, cx, 0, sz.Width, st.ItemHeight)
                    End Using
                End If
                Using b As New SolidBrush(part.ForeColor)
                    g.DrawString(part.Text, part.Font, b, cx,
                                 (st.ItemHeight - part.Font.Height) / 2.0F, fmt)
                End Using
                cx += sz.Width
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TreeNodeFlyout.OnPaint", ex)
        End Try
    End Sub

End Class
