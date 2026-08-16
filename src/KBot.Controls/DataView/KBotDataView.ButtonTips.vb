Option Strict On
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' ETICHETELE BUTOANELOR grilei (felia 0035) — perechea lui
''' <c>AdvancedTreeControl.ButtonTips.vb</c>, cu aceeași motivare: butoanele din antetul unei
''' coloane nu sunt controale, sunt zone pictate, deci un <c>ToolTip</c> din WinForms n-are ce
''' extinde. Se folosește <see cref="KBotToolTip.ShowAt"/>, chemat din aceleași funcții care
''' urmăresc deja survolarea.
'''
''' <para><b>Ce se explică.</b> Titlul coloanei, pictograma din dreapta lui, pictograma de
''' filtrare și butonul de strângere. Textele stau pe coloană
''' (<see cref="KBotDataColumn.HeaderTooltip"/> și surorile ei), fiindcă ele diferă de la o
''' coloană la alta; doar filtrul are și o etichetă COMUNĂ pe grilă — filtrul face același lucru
''' peste tot, deci de obicei nu se scrie de zece ori.</para>
'''
''' <para><b>Prioritatea la survolare</b> e cea a butoanelor: pictograma de filtrare bate
''' pictograma din dreapta, care bate titlul. Cursorul e peste una singură, dar dreptunghiurile
''' se pot atinge pe margine, iar butonul trebuie să câștige mereu în fața etichetei de sub el.</para>
''' </summary>
Partial Class KBotDataView

    Private _butonTooltip As KBotToolTip
    Private _tipButonCurent As String = Nothing
    Private ReadOnly _tipContinut As New KBotToolTipContent()

    ''' <summary>
    ''' Eticheta plutitoare cu care grila își explică butoanele de antet. Se îmbracă din grila de
    ''' proprietăți (<c>ButtonTooltip.Style.…</c>), independent de etichetele formularului — de
    ''' aceea două suprafețe de pe același ecran pot arăta diferit.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Eticheta plutitoare care explică butoanele din antetul coloanelor.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property ButtonTooltip As KBotToolTip
        Get
            If _butonTooltip Is Nothing Then _butonTooltip = New KBotToolTip()
            Return _butonTooltip
        End Get
    End Property

    Private _filterIconTooltip As String = String.Empty
    ''' <summary>
    ''' Eticheta COMUNĂ a pictogramei de filtrare. O coloană poate să o suprascrie prin
    ''' <see cref="KBotDataColumn.FilterIconTooltip"/>.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Eticheta comună a pictogramei de filtrare (mai multe rânduri). O coloană o poate suprascrie.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property FilterIconTooltip As String
        Get
            Return _filterIconTooltip
        End Get
        Set(value As String)
            _filterIconTooltip = If(value, String.Empty)
        End Set
    End Property

    Private _collapseButtonTooltip As String = String.Empty
    ''' <summary>Eticheta butonului de strângere, cât grila e DESFĂCUTĂ.</summary>
    <Category("K-BOT: Footer")>
    <Description("Eticheta butonului de strângere, cât grila e desfăcută (mai multe rânduri).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property CollapseButtonTooltip As String
        Get
            Return _collapseButtonTooltip
        End Get
        Set(value As String)
            _collapseButtonTooltip = If(value, String.Empty)
        End Set
    End Property

    Private _expandButtonTooltip As String = String.Empty
    ''' <summary>Eticheta aceluiași buton cât grila e STRÂNSĂ. Gol = același text.</summary>
    <Category("K-BOT: Footer")>
    <Description("Eticheta butonului de strângere, cât grila e strânsă. Gol = același text ca la desfăcut.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property ExpandButtonTooltip As String
        Get
            Return _expandButtonTooltip
        End Get
        Set(value As String)
            _expandButtonTooltip = If(value, String.Empty)
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' AFIȘAREA
    ' ══════════════════════════════════════════════════════════════════════════

    ' Aceeași cheie de două ori la rând nu face nimic: altfel fiecare pixel de mișcare peste
    ' același buton ar reprograma apariția, iar eticheta n-ar ieși niciodată.
    Private Sub ShowButtonTip(cheie As String, text As String)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If String.Equals(cheie, _tipButonCurent, StringComparison.Ordinal) Then Return
            _tipButonCurent = cheie

            If String.IsNullOrEmpty(cheie) OrElse String.IsNullOrEmpty(text) Then
                _butonTooltip?.HideNow()
                Return
            End If

            _tipContinut.Text = text
            ButtonTooltip.ShowAt(Me, _tipContinut, Cursor.Position)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ShowButtonTip", ex)
        End Try
    End Sub

    ''' <summary>Stinge eticheta de buton (cursorul a plecat de pe orice buton).</summary>
    Friend Sub HideButtonTip()
        ShowButtonTip(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Decide ce etichetă se cuvine pentru punctul dat din bandă de antet. Un singur loc pentru
    ''' toate cele trei zone, ca prioritatea (filtru &gt; pictogramă &gt; titlu) să fie scrisă o
    ''' dată.
    ''' </summary>
    Friend Sub RefreshHeaderTip(pt As Point)
        Try
            Dim r As Rectangle = Rectangle.Empty

            Dim colFiltru As KBotDataColumn = FilterIconTarget(pt, r)
            If colFiltru IsNot Nothing Then
                Dim t As String = If(String.IsNullOrEmpty(colFiltru.FilterIconTooltip),
                                     _filterIconTooltip, colFiltru.FilterIconTooltip)
                ShowButtonTip("flt:" & colFiltru.Key, t)
                Return
            End If

            Dim colIcon As KBotDataColumn = HeaderIconTarget(pt, r)
            If colIcon IsNot Nothing Then
                ShowButtonTip("ico:" & colIcon.Key, colIcon.HeaderRightIconTooltip)
                Return
            End If

            Dim colTitlu As KBotDataColumn = ColumnAtHeaderPoint(pt)
            If colTitlu IsNot Nothing AndAlso Not String.IsNullOrEmpty(colTitlu.HeaderTooltip) Then
                ShowButtonTip("hdr:" & colTitlu.Key, colTitlu.HeaderTooltip)
                Return
            End If

            HideButtonTip()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.RefreshHeaderTip", ex)
        End Try
    End Sub

    ''' <summary>Eticheta butonului de strângere (cele două înțelesuri ale lui).</summary>
    Friend Sub RefreshCollapseTip(hover As Boolean)
        Try
            If Not hover Then
                If _tipButonCurent IsNot Nothing AndAlso _tipButonCurent.StartsWith("clp", StringComparison.Ordinal) Then
                    HideButtonTip()
                End If
                Return
            End If
            Dim strans As Boolean = Collapsed
            Dim t As String = If(strans AndAlso Not String.IsNullOrEmpty(_expandButtonTooltip),
                                 _expandButtonTooltip, _collapseButtonTooltip)
            ShowButtonTip(If(strans, "clp.expand", "clp.collapse"), t)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.RefreshCollapseTip", ex)
        End Try
    End Sub

    ' Coloana peste al cărei ANTET stă punctul (fără pictograme). Nothing dacă punctul nu e în
    ' banda de antet sau nu cade pe nicio coloană vizibilă.
    ' Aceleași două benzi de așezare pe care le parcurge și HeaderIconTarget — banda înghețată
    ' întâi, fiindcă ea acoperă coloanele derulate de sub ea.
    Private Function ColumnAtHeaderPoint(pt As Point) As KBotDataColumn
        Dim bandH As Integer = HeaderBandHeight()
        If bandH <= 0 OrElse pt.Y < 0 OrElse pt.Y >= bandH Then Return Nothing

        For Each cl In _frozenLayout
            If pt.X >= cl.X AndAlso pt.X < cl.X + cl.Column.WidthPx Then Return cl.Column
        Next

        If pt.X < _frozenBandWidth OrElse pt.X >= ViewportWidth() Then Return Nothing

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            Dim stanga As Integer = _frozenBandWidth + cl.X - hOffset
            If pt.X >= stanga AndAlso pt.X < stanga + cl.Column.WidthPx Then Return cl.Column
        Next
        Return Nothing
    End Function

    ''' <summary>Eliberează eticheta de buton (chemat din Dispose-ul grilei).</summary>
    Private Sub DisposeButtonTips()
        _butonTooltip?.Dispose()
        _butonTooltip = Nothing
    End Sub

End Class
