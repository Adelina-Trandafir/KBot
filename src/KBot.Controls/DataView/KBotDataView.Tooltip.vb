Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' ETICHETA PLUTITOARE de celulă a <see cref="KBotDataView"/> (slice 0028): fereastra care
''' arată textul ÎNTREG al unei celule al cărei conținut nu încape în lățimea coloanei.
'''
''' <para><b>Doar când chiar nu încape.</b> Grila măsoară textul deja formatat (același pe care
''' l-ar picta, prin aceeași trecere de <c>CellFormatting</c>) cu fontul lui și îl compară cu
''' lățimea utilă a celulei. O etichetă care apare peste un text care se citea și așa e zgomot —
''' și, mai rău, acoperă exact celula pe care operatorul o citea.</para>
'''
''' <para>Decizia («pentru care celulă, cu ce text, unde») se ia AICI și rămâne calculabilă fără
''' ecran — de aceea <see cref="CellTooltipTextFor"/> e o funcție pură, verificabilă headless.
''' Fereastra (<c>KBotCellTooltipWindow</c>) e doar randare, ca la <c>TreeNodeFlyout</c>.</para>
''' </summary>
Partial Class KBotDataView

    Private ReadOnly _cellTooltipOptions As New KBotCellTooltipOptions()
    Private _tipWindow As KBotCellTooltipWindow
    Private _tipTimer As Timer
    ' Ținta pentru care s-a pornit așteptarea (-1 / Nothing = niciuna).
    Private _tipRowIndex As Integer = -1
    Private _tipColKey As String = Nothing

    ''' <summary>
    ''' Obiectul de etichetă al grilei: pornit/stins, întârziere, lățime maximă, culori, font,
    ''' rotunjire. Culorile goale și fontul nesetat înseamnă «din temă», ca peste tot în K-BOT.
    ''' Se autorizează și din grila de proprietăți (obiect imbricat, extensibil).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Eticheta plutitoare pentru celulele al căror text nu încape în coloană.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property CellTooltip As KBotCellTooltipOptions
        Get
            Return _cellTooltipOptions
        End Get
    End Property

    ' Chemată din constructor: obiectul de setări trebuie să știe cui să-i ceară închiderea
    ' etichetei când e stins din designer sau din cod.
    Private Sub WireCellTooltip()
        _cellTooltipOptions.Owner = Me
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' DECIZIA — pură, deci verificabilă fără ecran
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Textul pe care l-ar arăta eticheta pentru o celulă, sau <c>Nothing</c> dacă nu i se cuvine
    ''' niciuna (text vid, text care încape, coloană fără text propriu).
    '''
    ''' Friend: e poarta de verificare headless a regulii «doar ce nu încape» — testele n-au cum
    ''' să plimbe un mouse, dar pot întreba direct.
    ''' </summary>
    Friend Function CellTooltipTextFor(colKey As String, rowIndex As Integer) As String
        Try
            If rowIndex < 0 OrElse rowIndex >= _rows.Count Then Return Nothing
            Dim col As KBotDataColumn = Nothing
            If colKey Is Nothing OrElse Not _columnIndex.TryGetValue(colKey, col) Then Return Nothing

            ' Doar coloanele care poartă TEXT propriu. O bifă, un buton radio sau o bară de
            ' progres n-au ce tăia, iar butonul își desenează eticheta centrată, pe fața lui.
            If col.ColumnType <> KBotColumnType.Text AndAlso col.ColumnType <> KBotColumnType.Combo Then
                Return Nothing
            End If

            Dim row As KBotDataRow = _rows(rowIndex)
            Dim value As Object = row(colKey)

            ' Aceeași trecere de formatare ca la pictare, pe argumentele de INTEROGARE (nu pe
            ' cele ale unei pictări în curs): eticheta trebuie să arate exact ce s-ar fi văzut,
            ' inclusiv un text pus de handler peste valoarea brută.
            ' Fontul e cel al coloanei (altfel al grilei), ca la pictare: eticheta se aprinde
            ' pentru ce NU încape, iar «cât încape» depinde de fontul cu care se scrie.
            _probeCellArgs.Reset(col, row, rowIndex, value, FormatValue(value, col),
                                 BackColor, ForeColor, CellFontFor(col), col.TextAlign,
                                 col.Enabled AndAlso row.Enabled)
            RaiseEvent CellFormatting(Me, _probeCellArgs)

            Dim text As String = _probeCellArgs.Text
            If String.IsNullOrEmpty(text) Then Return Nothing
            If Not TextOverflowsCell(text, _probeCellArgs.Font, col) Then Return Nothing
            Return text
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CellTooltipTextFor", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Textul depășește lățimea utilă a celulei? Lățimea utilă e cea din <c>DrawTextCell</c>
    ''' (aceleași margini), minus zona chevronului la coloanele Combo — altfel eticheta n-ar
    ''' apărea taman pentru textele ascunse SUB săgeata de derulare.
    ''' </summary>
    Private Function TextOverflowsCell(text As String, font As Font, col As KBotDataColumn) As Boolean
        Dim padX As Integer = ScaleDpi(6)
        Dim disponibil As Integer = col.WidthPx - 2 * padX
        If col.ColumnType = KBotColumnType.Combo Then disponibil -= ScaleDpi(16)
        If disponibil <= 0 Then Return True
        Dim latime As Integer = MeasureText(text, If(font, Me.Font))
        Return latime > disponibil
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' URMĂRIREA CURSORULUI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Chemată din <c>OnMouseMove</c> cu celula de sub cursor (cheie Nothing / index -1 = nicio
    ''' celulă). Ținta nouă repornește așteptarea; aceeași țintă nu face nimic — altfel eticheta
    ''' ar clipi la fiecare pixel de mișcare peste aceeași celulă.
    ''' </summary>
    Private Sub UpdateCellTooltip(colKey As String, rowIndex As Integer)
        If Not _cellTooltipOptions.Enabled OrElse KBotDesignTime.IsDesignTime(Me) Then
            CancelCellTooltip()
            Return
        End If
        If rowIndex = _tipRowIndex AndAlso String.Equals(colKey, _tipColKey, StringComparison.Ordinal) Then Return

        CancelCellTooltip()
        If rowIndex < 0 OrElse colKey Is Nothing Then Return
        ' Se măsoară ACUM, nu la expirarea cronometrului: dacă textul încape, nu pornim nimic.
        If CellTooltipTextFor(colKey, rowIndex) Is Nothing Then Return

        _tipRowIndex = rowIndex
        _tipColKey = colKey
        If _tipTimer Is Nothing Then
            _tipTimer = New Timer()
            AddHandler _tipTimer.Tick, AddressOf OnCellTooltipTick
        End If
        _tipTimer.Interval = _cellTooltipOptions.Delay
        _tipTimer.Stop()
        _tipTimer.Start()
    End Sub

    ' Boundary UI (tick de cronometru): loghează și ÎNGHITE.
    Private Sub OnCellTooltipTick(sender As Object, e As EventArgs)
        Try
            _tipTimer?.Stop()
            ShowCellTooltip()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnCellTooltipTick", ex)
        End Try
    End Sub

    ' Scoate fereastra lângă celulă. Transitiv acoperită de boundary-ul tick-ului.
    Private Sub ShowCellTooltip()
        Dim text As String = CellTooltipTextFor(_tipColKey, _tipRowIndex)
        If text Is Nothing Then
            CancelCellTooltip()
            Return
        End If

        Dim cellRect As Rectangle = CellRectangle(_tipColKey, _tipRowIndex)
        If cellRect.IsEmpty Then
            CancelCellTooltip()
            Return
        End If

        Dim col As KBotDataColumn = _columnIndex(_tipColKey)
        ' Fontul etichetei: cel fixat pe opțiuni (mărit, ca orice font ținut de noi), altfel chiar
        ' fontul CELULEI — care a trecut deja prin CellFontFor, deci e mărit la rândul lui.
        Dim font As Font = If(Marit(_cellTooltipOptions.Font), If(_probeCellArgs.Font, Me.Font))
        Dim maxTextW As Integer = Math.Max(40, _cellTooltipOptions.MaxWidth - 2 * KBotCellTooltipWindow.PaddingSize.Width)
        Dim masura As Size = TextRenderer.MeasureText(text, font,
                                                      New Size(maxTextW, Integer.MaxValue),
                                                      TextFormatFlags.WordBreak)
        Dim latime As Integer = masura.Width + 2 * KBotCellTooltipWindow.PaddingSize.Width
        Dim inaltime As Integer = masura.Height + 2 * KBotCellTooltipWindow.PaddingSize.Height

        If _tipWindow Is Nothing Then _tipWindow = New KBotCellTooltipWindow()
        _tipWindow.SetContent(text, font, TooltipForeColor(), TooltipBackColor(),
                              TooltipBorderColor(), _cellTooltipOptions.CornerRadius)

        ' Sub celulă, aliniată la stânga ei; dacă n-are loc, deasupra / împinsă în ecran.
        Dim ancora As Point = PointToScreen(New Point(cellRect.Left, cellRect.Bottom + 1))
        Dim zona As Rectangle = Screen.FromControl(Me).WorkingArea
        Dim x As Integer = Math.Max(zona.Left, Math.Min(ancora.X, zona.Right - latime))
        Dim y As Integer = ancora.Y
        If y + inaltime > zona.Bottom Then
            y = PointToScreen(New Point(cellRect.Left, cellRect.Top)).Y - inaltime - 1
        End If
        y = Math.Max(zona.Top, Math.Min(y, zona.Bottom - inaltime))

        _tipWindow.Bounds = New Rectangle(x, y, latime, inaltime)
        If Not _tipWindow.Visible Then _tipWindow.Show()
    End Sub

    ''' <summary>
    ''' Închide eticheta și uită ținta. Friend: o cheamă și <c>KBotCellTooltipOptions.Enabled</c>
    ''' când e stinsă, ca eticheta deja ieșită să nu rămână agățată pe ecran.
    ''' </summary>
    Friend Sub CancelCellTooltip()
        Try
            _tipTimer?.Stop()
            _tipRowIndex = -1
            _tipColKey = Nothing
            If _tipWindow IsNot Nothing AndAlso _tipWindow.Visible Then _tipWindow.Hide()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CancelCellTooltip", ex)
        End Try
    End Sub

    ' Eliberarea ferestrei + cronometrului (chemată din Dispose).
    Private Sub DisposeCellTooltip()
        If _tipTimer IsNot Nothing Then
            RemoveHandler _tipTimer.Tick, AddressOf OnCellTooltipTick
            _tipTimer.Dispose()
            _tipTimer = Nothing
        End If
        _tipWindow?.Dispose()
        _tipWindow = Nothing
    End Sub

    ' ── Culorile etichetei: din setări dacă operatorul le-a pus, altfel din temă ──

    Private Function TooltipBackColor() As Color
        If _cellTooltipOptions.BackColor <> Color.Empty Then Return _cellTooltipOptions.BackColor
        Return _cHeaderBack
    End Function

    Private Function TooltipForeColor() As Color
        If _cellTooltipOptions.ForeColor <> Color.Empty Then Return _cellTooltipOptions.ForeColor
        Return _cCellText
    End Function

    Private Function TooltipBorderColor() As Color
        If _cellTooltipOptions.BorderColor <> Color.Empty Then Return _cellTooltipOptions.BorderColor
        Return _cHeaderSep
    End Function

    ''' <summary>
    ''' Dreptunghiul (client) al unei celule, sau gol dacă nu e pe ecran. Ține cont de banda
    ''' înghețată și de derularea orizontală, exact ca pictarea.
    ''' </summary>
    Friend Function CellRectangle(colKey As String, rowIndex As Integer) As Rectangle
        If rowIndex < 0 OrElse rowIndex >= _rows.Count Then Return Rectangle.Empty
        ' Index de MODEL (așa vine din input); un rând filtrat afară n-are dreptunghi.
        Dim y As Integer = RowTopForModel(rowIndex)
        If y = Integer.MinValue Then Return Rectangle.Empty

        For Each cl In _frozenLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return New Rectangle(cl.X, y, cl.Column.WidthPx, _rowHeight)
            End If
        Next

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return New Rectangle(_frozenBandWidth + cl.X - hOffset, y, cl.Column.WidthPx, _rowHeight)
            End If
        Next

        Return Rectangle.Empty
    End Function

End Class
