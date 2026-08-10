Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Pictura meniului: rama, rândurile, banda de pictograme, sublinierea literei de acces.
''' Totul owner-drawn — de aici vine singura însușire pe care <c>ContextMenuStrip</c> n-o are,
''' anume că se schimbă la culoare odată cu schema.
''' </summary>
Partial Public Class CustomPopup

    ''' <summary>
    ''' Frontieră de pictură: un <c>Throw</c> de aici ar dărâma procesul, deci se loghează și se
    ''' înghite (regula casei). Ajutoarele chemate DOAR de aici nu-și mai poartă propriul Try.
    ''' </summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' -1 pe fiecare latură ca și conturul să intre în fereastră, nu pe jumătate în afara ei.
            Dim rama As New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 1), Math.Max(1, ClientSize.Height - 1))
            Using path As GraphicsPath = ThemeShapes.RoundedRect(rama, EffectiveRadius())
                Using b As New SolidBrush(EffectiveBackColor)
                    g.FillPath(b, path)
                End Using
                ' Conturul e ce desprinde meniul de fereastra de dedesubt — fundalul singur l-ar
                ' face să pară o pată de aceeași culoare cu formularul.
                Using pen As New Pen(EffectiveBorderColor)
                    g.DrawPath(pen, path)
                End Using
            End Using

            If Items.Count = 0 Then Return

            Dim gutter As Integer = IconGutter()
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, PadXLogical)
            Dim vizor As New Rectangle(0, 0, ClientSize.Width, ClientSize.Height)

            For i As Integer = 0 To Items.Count - 1
                Dim r As Rectangle = RowBounds(i)
                If r.IsEmpty Then Continue For
                r.Offset(0, -ScrollOffset)
                ' Meniul poate fi mai înalt decât ecranul (vezi ScrollBy): rândurile ieșite din
                ' vizor nu se mai desenează deloc.
                If r.Bottom <= vizor.Top OrElse r.Top >= vizor.Bottom Then Continue For

                If Items(i).IsSeparator Then
                    DrawSeparator(g, r, padX)
                Else
                    DrawRow(g, i, r, padX, gutter)
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnPaint", ex)
        End Try
    End Sub

    ' Un rând: fundalul (doar cel evidențiat îl are), pictograma, textul.
    Private Sub DrawRow(g As Graphics, index As Integer, r As Rectangle, padX As Integer, gutter As Integer)
        Dim it As CustomPopupItem = Items(index)
        Dim evidentiat As Boolean = (index = SelectedIndex)

        Dim fore As Color
        If Not it.Enabled Then
            fore = EffectiveDisabledForeColor
        ElseIf evidentiat Then
            fore = EffectiveHighlightForeColor
        Else
            fore = EffectiveItemForeColor
        End If

        If evidentiat Then
            ' Aceeași umplere ca a butoanelor din KBotNavList (ThemeShapes.FillModern): nicio
            ' culoare nouă, doar două nuanțe derivate din culoarea de evidențiere a schemei.
            Dim inset As Integer = ThemeShapes.ScaleDpi(Me, 2)
            Dim h As Rectangle = Rectangle.Inflate(r, -inset, 0)
            Dim raza As Integer = Math.Max(0, EffectiveRadius() - 1)
            Using path As GraphicsPath = ThemeShapes.RoundedRect(h, raza)
                ThemeShapes.FillModern(g, path, h, EffectiveHighlightBackColor, ItemGradient)
            End Using
        End If

        If gutter > 0 AndAlso it.Image IsNot Nothing Then
            Dim side As Integer = ThemeShapes.ScaleDpi(Me, ImageSize)
            Dim dest As New Rectangle(r.Left + padX, r.Top + (r.Height - side) \ 2, side, side)
            ' Aceeași desaturare pentru elementele dezactivate ca în bara de navigare — ajutorul
            ' e Friend Shared pe KBotNavList tocmai ca să nu existe două matrice de culoare.
            KBotNavList.DrawItemImage(g, it.Image, dest, it.Enabled)
        End If

        Dim textLeft As Integer = r.Left + padX + gutter
        Dim tr As New Rectangle(textLeft, r.Top, Math.Max(0, r.Right - padX - textLeft), r.Height)
        If tr.Width <= 0 Then Return
        ' EndEllipsis peste steagurile de măsurare: taie doar ce oricum n-ar fi încăput în
        ' MaximumPopupWidth, deci nu schimbă lățimea calculată pentru textele care încap.
        TextRenderer.DrawText(g, If(it.Text, String.Empty), Font, tr, fore,
                              MeasureFlags() Or TextFormatFlags.EndEllipsis)
    End Sub

    ' Separatorul: o linie fină pe mijlocul slotului, retrasă de la margini ca să grupeze
    ' rândurile, nu să taie meniul în două.
    Private Sub DrawSeparator(g As Graphics, r As Rectangle, padX As Integer)
        Dim y As Integer = r.Top + r.Height \ 2
        Using pen As New Pen(EffectiveSeparatorColor)
            g.DrawLine(pen, r.Left + padX, y, r.Right - padX, y)
        End Using
    End Sub

End Class
