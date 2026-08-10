Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' MENIUL DE FILTRARE al unei coloane <see cref="KBotDataView"/> (slice 0028-03) — echivalentul
''' săgeții din antetul unei foi de date Access, cu aceleași patru etaje, în aceeași ordine:
'''
''' <list type="number">
''' <item><description>SORTAREA (crescător / descrescător), sus, fiindcă e ce se cere cel mai
''' des;</description></item>
''' <item><description><b>Șterge filtrul</b> din coloană — stins cât timp coloana n-are
''' filtru;</description></item>
''' <item><description>submeniul de CONDIȚII («Filtre text / numerice / de dată»), care deschide
''' apoi <see cref="KBotFilterConditionDialog"/>;</description></item>
''' <item><description>lista de VALORI BIFABILE, cu «(Selectează tot)» și o casetă de căutare,
''' iar dedesubt OK / Anulează.</description></item>
''' </list>
'''
''' <para><b>E o FEREASTRĂ desenată de noi</b>, ca <c>CustomPopup</c> și din același motiv: un
''' <c>ContextMenuStrip</c> cu un <c>CheckedListBox</c> în el ar rămâne două dreptunghiuri albe sub
''' o schemă întunecată. Singurul control-copil adevărat e caseta de căutare
''' (<c>KBotTextField</c>) — text tastat cere un control care știe să primească taste — iar ea e
''' <c>IThemedControl</c>, deci traversarea temei n-o calcă (regula care a mușcat de două ori:
''' vezi comentariul lui <c>IThemedControl</c>).</para>
'''
''' <para><b>Ce alege operatorul se predă la OK, nu pe loc.</b> Popup-ul lucrează pe o COPIE a
''' filtrului (<see cref="KBotColumnFilter.Clone"/>) și ridică <see cref="FilterAccepted"/> abia la
''' apăsarea OK; «Anulează» și Esc nu lasă nimic în urmă. Sortarea, în schimb, se aplică IMEDIAT și
''' închide meniul — ea nu e o alegere de confirmat, e o comandă, exact ca în Access.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Partial Friend NotInheritable Class KBotFilterPopup
    Inherits Form
    Implements IThemedControl

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    ' Măsuri logice (px @96dpi), scalate la DPI la fiecare recalculare.
    Private Const PadXLogical As Integer = 6
    Private Const PadYLogical As Integer = 4
    Private Const RowAirLogical As Integer = 10      ' înălțimea rândului = fontul + atât
    Private Const SeparatorLogical As Integer = 7
    Private Const CheckBoxLogical As Integer = 14
    Private Const CheckGapLogical As Integer = 7
    Private Const SearchHeightLogical As Integer = 26
    Private Const ButtonHeightLogical As Integer = 26
    Private Const ButtonWidthLogical As Integer = 84
    Private Const MaxListRowsLogical As Integer = 10 ' câte valori se văd fără derulare
    Private Const BorderThickness As Integer = 1

    ''' <summary>Felul unui rând din partea de MENIU (cea care nu derulează).</summary>
    Private Enum MenuRowKind
        SortAscending
        SortDescending
        ClearFilter
        Conditions
        Separator
    End Enum

    Private Structure MenuRow
        Public Kind As MenuRowKind
        Public Text As String
        Public Enabled As Boolean
        Public Bounds As Rectangle
    End Structure

    ' ── Ce filtrăm ───────────────────────────────────────────────────────────────
    Private ReadOnly _columnKey As String
    Private ReadOnly _columnCaption As String
    Private ReadOnly _valueType As KBotValueType
    Private ReadOnly _values As New List(Of String)()          ' textele distincte, în ordine
    Private ReadOnly _checked As HashSet(Of String)
    Private ReadOnly _working As KBotColumnFilter

    ' ── Așezare ──────────────────────────────────────────────────────────────────
    Private ReadOnly _menu As New List(Of MenuRow)()
    Private ReadOnly _shown As New List(Of Integer)()          ' indici în _values care trec de căutare
    Private _searchRect As Rectangle
    Private _listRect As Rectangle
    Private _selectAllRect As Rectangle
    Private _okRect As Rectangle
    Private _cancelRect As Rectangle
    Private _listScroll As Integer = 0
    Private _layoutDirty As Boolean = True

    ' ── Stare de interacțiune ────────────────────────────────────────────────────
    ' Rândul survolat: indexul din _menu (>= 0), sau unul din codurile de mai jos.
    Private Const HotNone As Integer = -1
    Private Const HotSelectAll As Integer = -2
    Private Const HotOk As Integer = -3
    Private Const HotCancel As Integer = -4
    Private _hotMenu As Integer = HotNone
    Private _hotValue As Integer = -1                          ' index în _shown
    Private _suppressDeactivate As Boolean = False
    Private _closing As Boolean = False

    Private WithEvents txtSearch As KBotTextField

    ' ── Culori, toate din temă (vezi ApplyTheme) ─────────────────────────────────
    Private _cBack As Color = SystemColors.Window
    Private _cBorder As Color = SystemColors.ControlDark
    Private _cText As Color = SystemColors.ControlText
    Private _cDisabled As Color = SystemColors.GrayText
    Private _cHighlightBack As Color = SystemColors.Highlight
    Private _cHighlightText As Color = SystemColors.HighlightText
    Private _cSeparator As Color = SystemColors.ControlLight
    Private _cAccent As Color = SystemColors.Highlight
    Private _cAccentText As Color = SystemColors.HighlightText
    Private _cButtonFace As Color = SystemColors.Control
    Private _cButtonBorder As Color = SystemColors.ControlDark

    ''' <summary>
    ''' Operatorul a apăsat OK: filtrul din argument e cel de așezat pe coloană (poate fi inactiv,
    ''' adică «fără filtru»).
    ''' </summary>
    Friend Event FilterAccepted As EventHandler(Of KBotFilterAcceptedEventArgs)

    ''' <summary>Operatorul a cerut o sortare. Se aplică imediat, iar meniul se închide.</summary>
    Friend Event SortRequested As EventHandler(Of KBotSortRequestedEventArgs)

    ''' <summary>
    ''' Construiește meniul pentru o coloană: titlul afișat, tipul valorilor, valorile distincte
    ''' (deja formatate, în ordinea de sortare) și filtrul curent (<c>Nothing</c> = niciunul).
    ''' </summary>
    Friend Sub New(columnKey As String, columnCaption As String, valueType As KBotValueType,
                   distinctValues As IEnumerable(Of String), currentFilter As KBotColumnFilter,
                   currentSort As KBotSortDirection)
        _columnKey = columnKey
        _columnCaption = If(columnCaption, String.Empty)
        _valueType = valueType
        If distinctValues IsNot Nothing Then _values.AddRange(distinctValues)
        _working = If(currentFilter Is Nothing, New KBotColumnFilter(columnKey), currentFilter.Clone())
        _currentSort = currentSort

        ' Bifele pornesc de la filtrul existent; fără filtru, tot ce există e bifat — adică starea
        ' «nefiltrat», nu una goală pe care operatorul ar trebui s-o repare cu «Selectează tot».
        If _working.SelectedValues Is Nothing Then
            _checked = New HashSet(Of String)(_values, StringComparer.CurrentCultureIgnoreCase)
        Else
            _checked = New HashSet(Of String)(_working.SelectedValues, StringComparer.CurrentCultureIgnoreCase)
        End If

        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' Fără autoscalare: totul se calculează în px DEJA scalați, iar o a doua ajustare a
        ' formularului ar muta meniul de sub pictograma pe care s-a apăsat.
        AutoScaleMode = AutoScaleMode.None
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        KeyPreview = True

        txtSearch = New KBotTextField() With {.PlaceholderText = "Caută…"}
        Controls.Add(txtSearch)
        AddHandler txtSearch.InnerTextBox.TextChanged, AddressOf OnSearchChanged
        AddHandler txtSearch.FieldKeyDown, AddressOf OnSearchKeyDown

        RebuildShown()
        ' Meniul se tematizează SINGUR: fiind o fereastră de sine stătătoare, nu-l prinde
        ' traversarea gazdei (același raționament ca la CustomPopup).
        ApplyTheme(ThemeManager.Current)
    End Sub

    Private ReadOnly _currentSort As KBotSortDirection

    ''' <summary>Cheia coloanei pentru care s-a deschis meniul.</summary>
    Friend ReadOnly Property ColumnKey As String
        Get
            Return _columnKey
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW      ' fără buton în bara de activități
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW   ' umbra pe care o are orice meniu
            Return cp
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' TEMĂ
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Ia culorile schemei active. Boundary de temă: loghează + ÎNGHITE — o excepție aici ar rupe
    ''' traversarea pentru tot formularul de dedesubt.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _cBack = p.SurfaceAltColor
            _cBorder = p.BorderColor
            _cText = p.TextColor
            _cDisabled = p.DisabledTextColor
            _cHighlightBack = p.AccentColor
            _cHighlightText = p.AccentTextColor
            _cSeparator = p.BorderColor
            _cAccent = p.AccentColor
            _cAccentText = p.AccentTextColor
            _cButtonFace = p.ButtonBackColor
            _cButtonBorder = p.ButtonBorderColor
            BackColor = _cBack
            ForeColor = _cText
            txtSearch?.ApplyTheme(scheme)
            _layoutDirty = True
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.ApplyTheme", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' AȘEZARE
    ' ══════════════════════════════════════════════════════════════════════════

    ' Rândurile de meniu, în ordinea Access. Se reconstruiesc la fiecare recalculare, fiindcă
    ' «Șterge filtrul» își schimbă starea de activare odată cu filtrul.
    Private Sub RebuildMenuRows()
        _menu.Clear()
        _menu.Add(New MenuRow With {.Kind = MenuRowKind.SortAscending,
                                    .Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Ascending),
                                    .Enabled = True})
        _menu.Add(New MenuRow With {.Kind = MenuRowKind.SortDescending,
                                    .Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Descending),
                                    .Enabled = True})
        _menu.Add(New MenuRow With {.Kind = MenuRowKind.Separator})
        _menu.Add(New MenuRow With {.Kind = MenuRowKind.ClearFilter,
                                    .Text = $"Șterge filtrul din «{_columnCaption}»",
                                    .Enabled = _working.IsActive})

        ' Coloanele logice n-au submeniu de condiții: cele două căsuțe din listă spun deja tot
        ' ce se poate spune despre o bifă (vezi KBotFilterEngine.AllowedOperators).
        If KBotFilterEngine.AllowedOperators(_valueType).Length > 0 Then
            _menu.Add(New MenuRow With {.Kind = MenuRowKind.Conditions,
                                        .Text = KBotFilterEngine.ConditionMenuCaption(_valueType) & "  ▸",
                                        .Enabled = True})
        End If

        _menu.Add(New MenuRow With {.Kind = MenuRowKind.Separator})
    End Sub

    ' Valorile care trec de caseta de căutare (toate, dacă e goală).
    Private Sub RebuildShown()
        _shown.Clear()
        Dim cautat As String = If(txtSearch Is Nothing, String.Empty, txtSearch.Text.Trim())
        For i As Integer = 0 To _values.Count - 1
            If cautat.Length = 0 OrElse
               EtichetaValorii(_values(i)).IndexOf(cautat, StringComparison.CurrentCultureIgnoreCase) >= 0 Then
                _shown.Add(i)
            End If
        Next
        _listScroll = 0
    End Sub

    ''' <summary>
    ''' Ce SCRIE pe rândul unei valori. Golul are o etichetă a lui — un rând complet gol în listă
    ''' arată ca un rând stricat, iar operatorul trebuie să poată bifa anume celulele necompletate.
    ''' </summary>
    Friend Shared Function EtichetaValorii(value As String) As String
        If String.IsNullOrEmpty(value) Then Return "(Necompletate)"
        Return value
    End Function

    Private Function Sc(logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(Me, logical)
    End Function

    Private Function RowHeight() As Integer
        Return TextRenderer.MeasureText("Wg", Font).Height + Sc(RowAirLogical)
    End Function

    ' Recalculează toate dreptunghiurile și mărimea ferestrei.
    Private Sub Recalc()
        If Not _layoutDirty Then Return
        RebuildMenuRows()

        Dim padX As Integer = Sc(PadXLogical)
        Dim padY As Integer = Sc(PadYLogical)
        Dim rowH As Integer = RowHeight()
        Dim latime As Integer = MeasureNaturalWidth()

        Dim y As Integer = BorderThickness + padY

        For i As Integer = 0 To _menu.Count - 1
            Dim r As MenuRow = _menu(i)
            Dim h As Integer = If(r.Kind = MenuRowKind.Separator, Sc(SeparatorLogical), rowH)
            r.Bounds = New Rectangle(BorderThickness, y, latime - 2 * BorderThickness, h)
            _menu(i) = r
            y += h
        Next

        ' Caseta de căutare.
        Dim searchH As Integer = Sc(SearchHeightLogical)
        _searchRect = New Rectangle(BorderThickness + padX, y, latime - 2 * (BorderThickness + padX), searchH)
        y += searchH + padY

        ' «(Selectează tot)» — rând fix, deasupra listei care derulează.
        _selectAllRect = New Rectangle(BorderThickness, y, latime - 2 * BorderThickness, rowH)
        y += rowH

        ' Lista propriu-zisă, plafonată la MaxListRows rânduri.
        Dim randuriVizibile As Integer = Math.Min(_shown.Count, MaxListRowsLogical)
        _listRect = New Rectangle(BorderThickness, y, latime - 2 * BorderThickness, randuriVizibile * rowH)
        y += _listRect.Height + padY

        ' Butoanele, aliniate la dreapta.
        Dim btnW As Integer = Sc(ButtonWidthLogical)
        Dim btnH As Integer = Sc(ButtonHeightLogical)
        _cancelRect = New Rectangle(latime - BorderThickness - padX - btnW, y, btnW, btnH)
        _okRect = New Rectangle(_cancelRect.Left - padX - btnW, y, btnW, btnH)
        y += btnH + padY + BorderThickness

        Size = New Size(latime, y)
        If txtSearch IsNot Nothing Then txtSearch.Bounds = _searchRect
        ClampScroll()
        _layoutDirty = False
    End Sub

    ' Lățimea naturală: cât cere cel mai lat rând (meniu sau valoare), între o podea și un plafon.
    Private Function MeasureNaturalWidth() As Integer
        Dim padX As Integer = Sc(PadXLogical)
        Dim gutter As Integer = Sc(CheckBoxLogical) + Sc(CheckGapLogical)
        Dim maxim As Integer = 0

        For Each r In _menu
            If r.Kind = MenuRowKind.Separator Then Continue For
            maxim = Math.Max(maxim, TextRenderer.MeasureText(r.Text, Font).Width)
        Next
        For Each i In _shown
            maxim = Math.Max(maxim, TextRenderer.MeasureText(EtichetaValorii(_values(i)), Font).Width + gutter)
        Next
        maxim = Math.Max(maxim, TextRenderer.MeasureText("(Selectează tot)", Font).Width + gutter)
        ' Cele două butoane trebuie să încapă una lângă alta, orice ar scrie în listă.
        maxim = Math.Max(maxim, 2 * Sc(ButtonWidthLogical) + padX)

        Dim total As Integer = maxim + 2 * (BorderThickness + padX) + Sc(CheckGapLogical)
        Return Math.Max(Sc(220), Math.Min(total, Sc(420)))
    End Function

    Private Sub ClampScroll()
        Dim maxim As Integer = Math.Max(0, _shown.Count - MaxListRowsLogical)
        _listScroll = Math.Max(0, Math.Min(_listScroll, maxim))
    End Sub

    ''' <summary>Câte rânduri de valori încap în fereastra listei.</summary>
    Private Function ListWindow() As Integer
        Return Math.Min(_shown.Count, MaxListRowsLogical)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' DESCHIDERE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Deschide meniul sub un dreptunghi din interiorul gazdei (coordonate client) — pictograma de
    ''' filtru pe care s-a apăsat. Când nu încape dedesubt sau spre dreapta, se răstoarnă peste
    ''' celelalte două laturi ale pictogramei, ca orice meniu de sistem.
    ''' </summary>
    Friend Sub ShowBelow(anchor As Control, anchorRect As Rectangle)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            _layoutDirty = True
            Recalc()

            Dim sus As Point = anchor.PointToScreen(New Point(anchorRect.Left, anchorRect.Top))
            Dim la As New Point(sus.X, sus.Y + anchorRect.Height)
            Dim zona As Rectangle = Screen.FromPoint(la).WorkingArea

            If la.X + Width > zona.Right Then la.X = Math.Max(zona.Left, sus.X + anchorRect.Width - Width)
            If la.Y + Height > zona.Bottom Then la.Y = Math.Max(zona.Top, sus.Y - Height)
            Location = la

            Show(anchor.FindForm())
            Activate()
            txtSearch?.Focus()
        Catch ex As Exception
            ' Punct de intrare (creare de fereastră, geometrie de ecran) => loghează și RE-ARUNCĂ.
            GlobalErrorLog.Write("KBotFilterPopup.ShowBelow", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        Try
            MyBase.OnDeactivate(e)
            ' Cât timp ține deschis un copil (submeniul de condiții), pierderea activării nu
            ' înseamnă că operatorul a dat clic în altă parte — înseamnă că se uită la copil.
            If _suppressDeactivate OrElse _closing Then Return
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnDeactivate", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' ACȚIUNI
    ' ══════════════════════════════════════════════════════════════════════════

    ' Aplică sortarea cerută și închide — sortarea e o comandă, nu o alegere de confirmat.
    Private Sub CereSortare(direction As KBotSortDirection)
        _closing = True
        RaiseEvent SortRequested(Me, New KBotSortRequestedEventArgs(_columnKey, direction))
        Close()
    End Sub

    ''' <summary>
    ''' Filtrul pe care l-ar preda un «OK» apăsat ACUM. Separat de <see cref="AcceptaFiltrul"/> ca
    ''' regula de mai jos să poată fi probată fără ecran — meniul e o fereastră, deciziile lui nu.
    ''' </summary>
    Friend Function BuildFilter() As KBotColumnFilter
        Dim rezultat As New KBotColumnFilter(_columnKey) With {
            .Condition = _working.Condition,
            .Operand1 = _working.Operand1,
            .Operand2 = _working.Operand2}

        ' TOATE valorile bifate = nicio restricție de listă. Fără regula asta, un filtru „bifat
        ' tot” ar rămâne activ pentru totdeauna și antetul ar arăta coloana ca filtrată degeaba.
        If _checked.Count < _values.Count Then
            rezultat.SelectedValues = New HashSet(Of String)(_checked, StringComparer.CurrentCultureIgnoreCase)
        End If

        Return rezultat
    End Function

    ' Predă filtrul construit din starea curentă și închide.
    Private Sub AcceptaFiltrul()
        _closing = True
        RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(BuildFilter()))
        Close()
    End Sub

    ' ── Porți de verificare headless (convenția Debug* a casei) ──────────────────

    ''' <summary>Câte valori distincte are lista (după căutare).</summary>
    Friend Function DebugShownCount() As Integer
        Return _shown.Count
    End Function

    ''' <summary>Câte valori sunt bifate acum.</summary>
    Friend Function DebugCheckedCount() As Integer
        Return _checked.Count
    End Function

    ''' <summary>Comută bifa unei valori după TEXTUL ei — drumul pe care l-ar face un clic.</summary>
    Friend Sub DebugToggleValue(displayText As String)
        Dim i As Integer = _values.IndexOf(displayText)
        If i < 0 Then Throw New ArgumentException($"Valoare inexistentă în listă: «{displayText}».", NameOf(displayText))
        ComutaValoarea(_shown.IndexOf(i))
    End Sub

    ''' <summary>Scrie în caseta de căutare, ca și cum ar fi tastat operatorul.</summary>
    Friend Sub DebugSearch(text As String)
        txtSearch.Text = text
    End Sub

    ''' <summary>Așază geometria și întoarce mărimea la care a ieșit fereastra.</summary>
    Friend Function DebugMeasure() As Size
        _layoutDirty = True
        Recalc()
        Return Size
    End Function

    ' Ridică filtrul coloanei pe loc (rândul «Șterge filtrul»).
    Private Sub StergeFiltrul()
        _closing = True
        RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(New KBotColumnFilter(_columnKey)))
        Close()
    End Sub

    ' Bifează / debifează toate valorile ARĂTATE (adică cele care trec de căutare). Peste o listă
    ' căutată, «Selectează tot» care ar atinge și valorile nevăzute ar fi o comandă care face mai
    ' mult decât se vede pe ecran.
    Private Sub ComutaToate()
        Dim toateBifate As Boolean = ToateAratateBifate()
        For Each i In _shown
            If toateBifate Then
                _checked.Remove(_values(i))
            Else
                _checked.Add(_values(i))
            End If
        Next
        Invalidate()
    End Sub

    Private Function ToateAratateBifate() As Boolean
        For Each i In _shown
            If Not _checked.Contains(_values(i)) Then Return False
        Next
        Return _shown.Count > 0
    End Function

    ' Comută bifa unei valori (index în _shown).
    Private Sub ComutaValoarea(shownIndex As Integer)
        If shownIndex < 0 OrElse shownIndex >= _shown.Count Then Return
        Dim v As String = _values(_shown(shownIndex))
        If _checked.Contains(v) Then
            _checked.Remove(v)
        Else
            _checked.Add(v)
        End If
        Invalidate()
    End Sub

    ' Deschide submeniul de condiții. Cât timp e sus, deactivate-ul NU închide meniul-părinte.
    Private Sub DeschideConditii(anchorRow As Rectangle)
        Dim operatori As KBotFilterOperator() = KBotFilterEngine.AllowedOperators(_valueType)
        If operatori.Length = 0 Then Return

        Dim meniu As New CustomPopup()
        For Each op In operatori
            meniu.Items.Add(New CustomPopupItem(op.ToString(), KBotFilterEngine.OperatorCaption(op, _valueType)))
        Next

        _suppressDeactivate = True
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs)
                Dim ales As KBotFilterOperator
                If [Enum].TryParse(Of KBotFilterOperator)(ev.Item.Key, ales) Then AplicaConditia(ales)
            End Sub
        AddHandler meniu.FormClosed,
            Sub(s As Object, ev As FormClosedEventArgs)
                _suppressDeactivate = False
                ' Dacă alegerea din submeniu n-a închis meniul-părinte (operatorul a apăsat Esc),
                ' focusul se întoarce aici — altfel ar rămâne o fereastră vizibilă și moartă.
                If Not _closing AndAlso Not IsDisposed Then Activate()
            End Sub

        meniu.ShowBelow(Me, anchorRow)
    End Sub

    ' Cere operanzii (dacă îi are) și așază condiția pe filtrul de lucru.
    Private Sub AplicaConditia(op As KBotFilterOperator)
        If KBotFilterEngine.OperandCount(op) = 0 Then
            _working.Condition = op
            _working.Operand1 = Nothing
            _working.Operand2 = Nothing
            AcceptaFiltrul()
            Return
        End If

        ' Dialogul e MODAL, deci meniul se dă la o parte întâi: două ferestre suprapuse, dintre
        ' care una cere o valoare, sunt o fereastră în plus peste ce a cerut operatorul.
        Hide()
        _suppressDeactivate = True
        Dim dlg As New KBotFilterConditionDialog(op, _valueType, _columnCaption,
                                                 _working.Operand1, _working.Operand2)
        Try
            If dlg.ShowDialog(Owner) = DialogResult.OK Then
                _working.Condition = op
                _working.Operand1 = dlg.Operand1
                _working.Operand2 = dlg.Operand2
                AcceptaFiltrul()
            Else
                _closing = True
                Close()
            End If
        Finally
            dlg.Dispose()
            _suppressDeactivate = False
        End Try
    End Sub

    Private Sub OnSearchChanged(sender As Object, e As EventArgs)
        Try
            RebuildShown()
            _hotValue = -1
            _layoutDirty = True
            Recalc()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnSearchChanged", ex)
        End Try
    End Sub

    Private Sub OnSearchKeyDown(sender As Object, e As KeyEventArgs)
        Try
            Select Case e.KeyCode
                Case Keys.Escape
                    e.SuppressKeyPress = True
                    Close()
                Case Keys.Enter
                    e.SuppressKeyPress = True
                    AcceptaFiltrul()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnSearchKeyDown", ex)
        End Try
    End Sub

End Class

''' <summary>Argumentele lui <c>KBotFilterPopup.FilterAccepted</c>.</summary>
Friend NotInheritable Class KBotFilterAcceptedEventArgs
    Inherits EventArgs

    Public Sub New(filter As KBotColumnFilter)
        Me.Filter = filter
    End Sub

    ''' <summary>Filtrul de așezat pe coloană; inactiv înseamnă «ridică filtrul».</summary>
    Public ReadOnly Property Filter As KBotColumnFilter

End Class

''' <summary>Argumentele lui <c>KBotFilterPopup.SortRequested</c>.</summary>
Friend NotInheritable Class KBotSortRequestedEventArgs
    Inherits EventArgs

    Public Sub New(columnKey As String, direction As KBotSortDirection)
        Me.ColumnKey = columnKey
        Me.Direction = direction
    End Sub

    Public ReadOnly Property ColumnKey As String
    Public ReadOnly Property Direction As KBotSortDirection

End Class
