Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Partea de GEOMETRIE a <see cref="KBotDataView"/>: benzile (antet / coloane înghețate /
''' corp derulat), offset-urile X ale coloanelor, matematica de virtualizare (înălțime fixă
''' de rând => aritmetică întreagă) și barele de derulare.
'''
''' Regula de aur a virtualizării: se pictează DOAR rândurile vizibile, deci costul unei
''' pictări nu depinde de <c>RowCount</c> (5.000 sau 500.000 — la fel).
''' </summary>
Partial Class KBotDataView

    ' Offset-ul X al unei coloane în banda ei (înghețată sau derulată).
    Private Structure ColLayout
        Public Column As KBotDataColumn
        Public X As Integer
    End Structure

    Private ReadOnly _frozenLayout As New List(Of ColLayout)()
    Private ReadOnly _scrollLayout As New List(Of ColLayout)()
    Private _frozenBandWidth As Integer = 0
    Private _scrollBandWidth As Integer = 0

    ' Gardă de reintrare: schimbarea vizibilității barelor declanșează layout.
    Private _inLayout As Boolean = False

    ' ── Recalcul coloane ────────────────────────────────────────────────────────

    ''' <summary>
    ''' Reconstruiește offset-urile X pe cele două benzi. Pur (nu atinge controale), deci
    ''' e sigur de apelat și din pictare.
    ''' </summary>
    Private Sub RecalcColumnLayout()
        _frozenLayout.Clear()
        _scrollLayout.Clear()
        Dim frozenX As Integer = 0
        Dim scrollX As Integer = 0
        Dim visibleIndex As Integer = 0

        For Each c In _columns
            If Not c.Visible Then Continue For
            If visibleIndex < _frozenColumnCount Then
                _frozenLayout.Add(New ColLayout With {.Column = c, .X = frozenX})
                frozenX += c.Width
            Else
                _scrollLayout.Add(New ColLayout With {.Column = c, .X = scrollX})
                scrollX += c.Width
            End If
            visibleIndex += 1
        Next

        _frozenBandWidth = frozenX
        _scrollBandWidth = scrollX
    End Sub

    ' ── Geometrie ───────────────────────────────────────────────────────────────

    ''' <summary>Înălțimea efectivă a benzii de antet (0 dacă e ascunsă).</summary>
    Private Function HeaderBandHeight() As Integer
        Return If(_showHeader, _headerHeight, 0)
    End Function

    ''' <summary>Lățimea zonei utile (client minus bara verticală, dacă e vizibilă).</summary>
    Private Function ViewportWidth() As Integer
        Return Math.Max(0, ClientSize.Width - If(vScroll.Visible, vScroll.Width, 0))
    End Function

    ''' <summary>Înălțimea zonei de date (client minus antet minus bara orizontală).</summary>
    Private Function ViewportHeight() As Integer
        Return Math.Max(0, ClientSize.Height - HeaderBandHeight() - If(hScroll.Visible, hScroll.Height, 0))
    End Function

    ''' <summary>Offset-ul vertical curent, în pixeli.</summary>
    Private Function VScrollOffset() As Integer
        Return If(vScroll.Visible, vScroll.Value, 0)
    End Function

    ''' <summary>Offset-ul orizontal curent al benzii derulate, în pixeli.</summary>
    Private Function HScrollOffset() As Integer
        Return If(hScroll.Visible, hScroll.Value, 0)
    End Function

    ' ── Virtualizare (înălțime fixă => aritmetică întreagă) ─────────────────────

    ''' <summary>Primul rând vizibil, dedus din offset-ul în pixeli.</summary>
    Private Function FirstVisibleRow() As Integer
        If _rowHeight <= 0 Then Return 0
        Return Math.Max(0, VScrollOffset() \ _rowHeight)
    End Function

    ''' <summary>Câte rânduri încap în zona de date (+2 pentru rândurile tăiate sus/jos).</summary>
    Private Function VisibleRowCount() As Integer
        If _rowHeight <= 0 Then Return 0
        Return (ViewportHeight() \ _rowHeight) + 2
    End Function

    ''' <summary>Ultimul rând care trebuie pictat (limitat la ultimul rând real).</summary>
    Private Function LastVisibleRow() As Integer
        Return Math.Min(_rows.Count - 1, FirstVisibleRow() + VisibleRowCount())
    End Function

    ''' <summary>Y-ul (client) al unui rând, ținând cont de antet și de derulare.</summary>
    Private Function RowTop(rowIndex As Integer) As Integer
        Return HeaderBandHeight() + rowIndex * _rowHeight - VScrollOffset()
    End Function

    ' ── Bare de derulare ────────────────────────────────────────────────────────

    ' Legarea evenimentelor (apelată din constructor, după InitializeComponent).
    Private Sub WireScrollBars()
        AddHandler vScroll.ValueChanged, AddressOf OnScrollValueChanged
        AddHandler hScroll.ValueChanged, AddressOf OnScrollValueChanged
    End Sub

    Private Sub OnScrollValueChanged(sender As Object, e As EventArgs)
        Try
            ' Derularea comite editarea deschisă: un editor real care plutește peste o celulă
            ' care tocmai a ieșit din fereastră ar rămâne agățat în aer.
            If _editing Then CommitEdit()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnScrollValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Recalculează coloanele și reconfigurează barele de derulare. Gardă de reintrare:
    ''' comutarea vizibilității unei bare schimbă spațiul disponibil pentru cealaltă.
    ''' </summary>
    Private Sub UpdateLayout()
        If _inLayout Then Return
        _inLayout = True
        Try
            ' English (slice 0013): size the columns first, then compute offsets and scrollbars.
            ' The pass no-ops while _updateDepth > 0 and runs once from EndUpdate.
            PerformAutoSize()
            RecalcColumnLayout()
            UpdateScrollBars()
        Finally
            _inLayout = False
        End Try
    End Sub

    ' Decide vizibilitatea/valorile barelor. Cele două se influențează reciproc, deci
    ' evaluăm în două treceri (bara verticală mănâncă lățime, cea orizontală înălțime).
    Private Sub UpdateScrollBars()
        Dim vw As Integer = SystemInformation.VerticalScrollBarWidth
        Dim hh As Integer = SystemInformation.HorizontalScrollBarHeight
        Dim headerH As Integer = HeaderBandHeight()

        Dim contentH As Integer = _rows.Count * _rowHeight
        Dim totalColsW As Integer = _frozenBandWidth + _scrollBandWidth

        Dim availW As Integer = ClientSize.Width
        Dim availH As Integer = Math.Max(0, ClientSize.Height - headerH)

        Dim needV As Boolean = contentH > availH
        If needV Then availW = Math.Max(0, availW - vw)

        Dim needH As Boolean = totalColsW > availW
        If needH Then
            availH = Math.Max(0, availH - hh)
            ' A doua trecere: pierderea de înălțime poate cere acum și bara verticală.
            If Not needV AndAlso contentH > availH Then
                needV = True
                availW = Math.Max(0, availW - vw)
                needH = totalColsW > availW
            End If
        End If

        ' Verticală.
        If needV Then
            vScroll.Bounds = New Rectangle(ClientSize.Width - vw, headerH, vw, availH)
            ConfigureScrollBar(vScroll, contentH, availH, _rowHeight)
        End If
        If vScroll.Visible <> needV Then vScroll.Visible = needV
        If Not needV Then vScroll.Value = 0

        ' Orizontală — derulează DOAR banda ne-înghețată.
        If needH Then
            hScroll.Bounds = New Rectangle(0, ClientSize.Height - hh, availW, hh)
            Dim scrollViewport As Integer = Math.Max(0, availW - _frozenBandWidth)
            ConfigureScrollBar(hScroll, _scrollBandWidth, scrollViewport, Math.Max(1, _rowHeight))
        End If
        If hScroll.Visible <> needH Then hScroll.Visible = needH
        If Not needH Then hScroll.Value = 0
    End Sub

    ' Setează intervalul unei bare. Semantica WinForms: valoarea maximă atinsă efectiv este
    ' Maximum - LargeChange + 1, deci Maximum = conținut - 1 și LargeChange = fereastra.
    Private Shared Sub ConfigureScrollBar(bar As ScrollBar, contentSize As Integer,
                                          viewportSize As Integer, smallChange As Integer)
        Dim viewport As Integer = Math.Max(1, viewportSize)
        bar.Minimum = 0
        bar.Maximum = Math.Max(0, contentSize - 1)
        bar.LargeChange = viewport
        bar.SmallChange = Math.Max(1, smallChange)
        ' Clamp: după micșorarea conținutului, Value poate depăși noul maxim util.
        Dim maxValue As Integer = Math.Max(0, contentSize - viewport)
        If bar.Value > maxValue Then bar.Value = maxValue
    End Sub

    ''' <summary>Derulează astfel încât rândul dat să fie complet vizibil.</summary>
    Public Sub EnsureVisible(rowIndex As Integer)
        Try
            If rowIndex < 0 OrElse rowIndex >= _rows.Count Then Return
            If Not vScroll.Visible Then Return

            Dim viewH As Integer = ViewportHeight()
            Dim top As Integer = rowIndex * _rowHeight
            Dim bottom As Integer = top + _rowHeight
            Dim current As Integer = vScroll.Value
            Dim target As Integer = current

            If top < current Then
                target = top                              ' iese pe sus
            ElseIf bottom > current + viewH Then
                target = bottom - viewH                   ' iese pe jos
            End If

            Dim maxValue As Integer = Math.Max(0, (_rows.Count * _rowHeight) - viewH)
            target = Math.Max(0, Math.Min(target, maxValue))
            If target <> current Then vScroll.Value = target
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.EnsureVisible", ex)
            Throw
        End Try
    End Sub

    ' ── Rotița mouse-ului ───────────────────────────────────────────────────────

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            Dim notches As Integer = e.Delta \ 120
            If notches = 0 Then Return

            ' Shift + rotiță => derulare orizontală (convenție Windows).
            Dim bar As ScrollBar = If((ModifierKeys And Keys.Shift) = Keys.Shift, CType(hScroll, ScrollBar), CType(vScroll, ScrollBar))
            If bar Is Nothing OrElse Not bar.Visible Then Return

            Dim linesPerNotch As Integer = Math.Max(1, SystemInformation.MouseWheelScrollLines)
            Dim delta As Integer = notches * linesPerNotch * bar.SmallChange
            Dim maxValue As Integer = Math.Max(0, bar.Maximum - bar.LargeChange + 1)
            bar.Value = Math.Max(0, Math.Min(bar.Value - delta, maxValue))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseWheel", ex)
        End Try
    End Sub

    ' ── Ajutor DPI (ThemeShapes e Friend în KBot.Theming, invizibil de aici) ─────

    ' Scalează o valoare logică (px @96dpi) la DPI-ul controlului. Fallback 96 înainte de handle.
    Private Function ScaleDpi(logical As Integer) As Integer
        Dim dpi As Integer = 96
        Try
            dpi = DeviceDpi
        Catch
            dpi = 96
        End Try
        Return CInt(Math.Round(logical * dpi / 96.0))
    End Function

End Class
