Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Culorile, fonturile și măsurile cu care <see cref="KBotNavFlyout"/> pictează un buton. Le
''' calculează BARA (ea știe regulile de selecție/hover/dezactivat și ea face scalarea la DPI),
''' fereastra doar le folosește — altfel logica de temă ar trăi în două locuri.
'''
''' Toate măsurile sunt deja în px SCALAȚI. Fonturile și pictograma sunt ÎMPRUMUTATE: fereastra
''' nu le deține și nu le eliberează niciodată (sunt ale barei, respectiv ale apelantului).
''' </summary>
Friend NotInheritable Class KBotNavFlyoutStyle
    Public Fill As Color
    Public Border As Color
    Public TextColor As Color
    Public BadgeFill As Color
    Public BadgeText As Color
    Public Radius As Integer
    ''' <summary>Intensitatea gradientului „modern" (0..100; 0 = umplere plată). Vezi <c>ThemeShapes.FillModern</c>.</summary>
    Public GradientStrength As Integer
    ''' <summary>Lățimea benzii pictogramei = exact lățimea butonului din bara strânsă.</summary>
    Public RailWidth As Integer
    Public IconSide As Integer
    Public PadX As Integer
    Public BadgeHeight As Integer
    Public CaptionFont As Font
    Public BadgeFont As Font
End Class

''' <summary>
''' Eticheta plutitoare a barei strânse (0025-07): fereastra care iese spre dreapta când cursorul
''' stă pe un buton al unei <see cref="KBotNavList"/> strânse la pictograme.
'''
''' NU e un <c>ToolTip</c>. Un ToolTip e galben, nescalabil și netematizabil, apare unde vrea el și
''' arată textul ca pe o notă lipită; ăsta desenează BUTONUL ÎNTREG — aceeași pictogramă, același
''' fundal (selectat/hover), același font semibold pe cel selectat, aceeași pastilă de badge — cu
''' culorile din schema activă, primite gata calculate în <see cref="KBotNavFlyoutStyle"/>.
'''
''' Trucul care-l face să arate ca o desfășurare, nu ca o etichetă lipită alături: fereastra
''' pleacă exact din dreptunghiul butonului STRÂNS și crește spre dreapta, iar pictograma se
''' desenează centrată în prima bandă (<see cref="KBotNavFlyoutStyle.RailWidth"/>), adică fix
''' peste locul unde e deja pictată în bară. Cursorul vede un buton care se desfășoară, nu două
''' pictograme una lângă alta.
'''
''' <para><b>Fereastră care nu se bagă în seamă.</b> <c>WS_EX_NOACTIVATE</c> +
''' <see cref="ShowWithoutActivation"/> (nu fură focusul de la formularul de dedesubt),
''' <c>WS_EX_TOOLWINDOW</c> (fără buton de bară de activități) și <c>HTTRANSPARENT</c> pe
''' <c>WM_NCHITTEST</c> — mouse-ul TRECE PRIN ea la bara de dedesubt. Ultima parte nu e cosmetică:
''' eticheta acoperă chiar butonul peste care stă cursorul, deci fără ea hover-ul s-ar pierde în
''' clipa în care apare eticheta, care s-ar ascunde, ceea ce ar readuce hover-ul… la infinit.</para>
''' </summary>
Friend NotInheritable Class KBotNavFlyout
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTTRANSPARENT As Integer = -1
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80

    Private _caption As String = String.Empty
    Private _image As Image
    Private _badge As Integer
    Private _itemEnabled As Boolean = True
    Private _style As KBotNavFlyoutStyle

    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' Fără autoscalare: bara ne dă Bounds în px DEJA scalați: dacă formularul le-ar mai ajusta
        ' o dată, eticheta n-ar mai cădea peste butonul din care trebuie să pară că iese.
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

    ''' <summary>
    ''' Ce se pictează. Se cheamă la fiecare cadru al desfășurării — deci nu alocă nimic și nu
    ''' repictează decât dacă s-a schimbat ceva.
    ''' </summary>
    Friend Sub SetContent(caption As String, image As Image, badge As Integer,
                          itemEnabled As Boolean, style As KBotNavFlyoutStyle)
        _caption = If(caption, String.Empty)
        _image = image
        _badge = badge
        _itemEnabled = itemEnabled
        _style = style
        If style IsNot Nothing Then BackColor = style.Fill
        Invalidate()
    End Sub

    ' Colțurile rotunjite se taie din FEREASTRĂ, nu doar din desen: altfel în colțuri s-ar vedea
    ' dreptunghiul ferestrei peste vederea de dedesubt. Setter-ul Region eliberează regiunea veche.
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
            GlobalErrorLog.Write("KBotNavFlyout.OnSizeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim st As KBotNavFlyoutStyle = _style
            If st Is Nothing Then Return
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' -1 pe fiecare latură ca și conturul să intre în fereastră, nu pe jumătate în afara ei.
            Dim r As New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 1), Math.Max(1, ClientSize.Height - 1))
            ' EXACT aceeași umplere ca a butonului din bară (ThemeShapes.FillModern, aceeași rază,
            ' aceeași intensitate): eticheta trebuie să fie butonul care se desfășoară, deci un
            ' gradient care nu se potrivește ar strica iluzia mai rău decât lipsa lui.
            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, st.Radius)
                ThemeShapes.FillModern(g, path, r, st.Fill, st.GradientStrength)
                ' Conturul e ce o desprinde de vederea de dedesubt — fundalul singur ar face-o să
                ' pară o pată de aceeași culoare cu bara.
                Using pen As New Pen(st.Border)
                    g.DrawPath(pen, path)
                End Using
            End Using

            ' Pictograma: centrată în PRIMA bandă, adică exact peste locul unde bara strânsă o
            ' desenează deja. Asta e ce face desfășurarea să pară a butonului, nu a unei etichete.
            If _image IsNot Nothing AndAlso st.IconSide > 0 Then
                Dim side As Integer = Math.Min(st.IconSide, ClientSize.Height - st.PadX \ 2)
                If side > 0 Then
                    Dim dest As New Rectangle(Math.Max(0, (st.RailWidth - side) \ 2),
                                              (ClientSize.Height - side) \ 2, side, side)
                    KBotNavList.DrawItemImage(g, _image, dest, _itemEnabled)
                End If
            End If

            ' Pastila badge-ului, la dreapta, înaintea textului (ea îi taie din lățime).
            Dim textRight As Integer = ClientSize.Width - st.PadX
            If _badge > 0 Then
                Dim badgeText As String = _badge.ToString()
                Dim ts As Size = TextRenderer.MeasureText(g, badgeText, st.BadgeFont)
                Dim bh As Integer = st.BadgeHeight
                Dim bw As Integer = Math.Max(bh, ts.Width + st.PadX)
                Dim br As New Rectangle(ClientSize.Width - bw - st.PadX \ 2,
                                        (ClientSize.Height - bh) \ 2, bw, bh)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(br, bh \ 2)
                    Using b As New SolidBrush(st.BadgeFill)
                        g.FillPath(b, path)
                    End Using
                End Using
                TextRenderer.DrawText(g, badgeText, st.BadgeFont, br, st.BadgeText,
                    TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                textRight = br.Left - st.PadX \ 3
            End If

            ' Textul începe unde se termină banda pictogramei. Cât timp desfășurarea e la început
            ' lățimea utilă e zero sau negativă — se taie singur, fără caz special.
            Dim textLeft As Integer = st.RailWidth
            Dim tr As New Rectangle(textLeft, 0, Math.Max(0, textRight - textLeft), ClientSize.Height)
            If tr.Width > 0 Then
                TextRenderer.DrawText(g, _caption, st.CaptionFont, tr, st.TextColor,
                    TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavFlyout.OnPaint", ex)
        End Try
    End Sub

End Class
