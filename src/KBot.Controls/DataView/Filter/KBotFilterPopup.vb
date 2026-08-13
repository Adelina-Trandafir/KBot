Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' MENIUL DE COLOANĂ al unei <see cref="KBotDataView"/> — echivalentul săgeții din antetul unei foi
''' de date Access, cu TREI FILE alese dintr-un <see cref="KBotNavList"/> orizontal (slice 0030):
'''
''' <list type="number">
''' <item><description><b>Sortare</b> — crescător / descrescător, plus «resetează sortarea»;</description></item>
''' <item><description><b>Filtrare</b> — lista de valori bifabile cu «(Selectează tot)» și o casetă
''' de căutare, submeniul de CONDIȚII («Filtre text / numerice / de dată») și «Șterge filtrul»;</description></item>
''' <item><description><b>Grupare</b> — opțiunile de nivel din felia 0029 pentru COLOANA aceasta,
''' plus ierarhia de niveluri a grilei. Fila se vede numai dacă grila are
''' <see cref="KBotDataView.EnableGrouping"/> aprins.</description></item>
''' </list>
'''
''' <para><b>Meniul e AUTORAT ÎN DESIGNER.</b> Până la felia 0028-06 era o fereastră desenată
''' integral de noi (≈400 de linii de pictură plus tot atâtea de hit-test și geometrie), fiindcă un
''' <c>ContextMenuStrip</c> cu un <c>CheckedListBox</c> ar fi rămas două dreptunghiuri albe pe o
''' schemă întunecată. Motivul acela a dispărut între timp: <c>ThemeManager</c> are reguli pe tip
''' pentru <c>CheckedListBox</c>, <c>CheckBox</c>, <c>Button</c> și <c>Panel</c> (inclusiv tema
''' nativă a barelor de derulare), iar <see cref="KBotThemedForm"/> le aplică singur. Deci TOATE
''' controalele stau în <c>KBotFilterPopup.Designer.vb</c>, ca la orice formular al casei, iar
''' fișierul acesta ține DOAR comportamentul.</para>
'''
''' <para><b>Ce rămâne al rulării, și de ce:</b> textele care depind de tipul coloanei (sortarea se
''' numește «A → Z» pe text și «de la mic la mare» pe numere), starea de activare a lui «Șterge
''' filtrul», existența butonului de condiții (coloanele logice n-au submeniu),
''' <b>conținutul listei de valori</b> și <b>conținutul filei de grupare</b> — valorile distincte
''' ale unei coloane și nivelurile de grupare ale grilei nu există la proiectare. Controalele care
''' le arată, în schimb, sunt ale designerului, ca tot restul.</para>
'''
''' <para><b>Trei feluri de a preda o hotărâre, și diferența contează:</b></para>
''' <list type="bullet">
''' <item><description><b>FILTRUL se predă la OK.</b> Popup-ul lucrează pe o COPIE
''' (<see cref="KBotColumnFilter.Clone"/>) și ridică <see cref="FilterAccepted"/> abia la apăsarea
''' OK; «Anulează» și Esc nu lasă nimic în urmă.</description></item>
''' <item><description><b>SORTAREA se aplică imediat ȘI închide meniul</b> — nu e o alegere de
''' confirmat, e o comandă, exact ca în Access.</description></item>
''' <item><description><b>GRUPAREA se aplică imediat, dar NU închide meniul.</b> E tot o comandă
''' (grila se rearanjează pe loc, iar operatorul vede rezultatul în spate), numai că are șapte
''' opțiuni: o filă care s-ar închide la prima bifă ar trebui redeschisă de șase ori.</description></item>
''' </list>
''' </summary>
<ToolboxItem(False)>
Partial Friend NotInheritable Class KBotFilterPopup

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    ''' <summary>Câte valori se văd fără derulare — restul, la scroll (lista are bara ei).</summary>
    Private Const MaxListRows As Integer = 10

    ''' <summary>Sub atât nu coboară fereastra, oricât de scurtă ar fi fila activă.</summary>
    Private Const MinHeight As Integer = 160

    ' Rândul butonului de condiții din tlyFiltrare — se strânge la zero pe coloanele logice.
    Private Const RandConditii As Integer = 4

    ' ── Ce filtrăm ───────────────────────────────────────────────────────────────
    Private ReadOnly _columnKey As String
    Private ReadOnly _columnCaption As String
    Private ReadOnly _valueType As KBotValueType
    Private ReadOnly _values As New List(Of String)()          ' textele distincte, în ordine
    Private ReadOnly _checked As HashSet(Of String)
    Private ReadOnly _working As KBotColumnFilter
    Private ReadOnly _currentSort As KBotSortDirection

    ' Grila care a deschis meniul — DOAR ca să se poată citi starea de grupare (nivelurile active și
    ' cel al coloanei acesteia). Nimic nu se scrie prin ea: hotărârile ies pe evenimente, ca
    ' filtrul și sortarea. Nothing = meniu fără gazdă (teste, bancul de probă) => fără filă de
    ' grupare.
    Private ReadOnly _grid As KBotDataView

    ' Indicii din _values care trec de căutare — adică exact ce e în lstValori, în aceeași ordine.
    Private ReadOnly _shown As New List(Of Integer)()

    ' Cât timp e True, evenimentele controalelor sunt ecoul nostru, nu al operatorului.
    Private _syncing As Boolean = False
    Private _suppressDeactivate As Boolean = False
    Private _closing As Boolean = False

    ' False până la sfârșitul constructorului: EndInit-ul barei de file ridică SelectionChanged din
    ' mijlocul lui InitializeComponent, cu mult înainte ca vreun câmp de mai sus să existe.
    Private _construit As Boolean = False

    ' Fila deschisă. Ținută ÎNTR-UN CÂMP, nu citită din «pnlX.Visible», și asta nu e o preferință:
    ' getter-ul lui Control.Visible răspunde despre LANȚUL DE PĂRINȚI, deci pe un formular încă
    ' nearătat toate cele trei file raportează False — măsurarea ar croi mereu fereastra pe aceeași
    ' filă, iar orice probă headless ar măsura altceva decât ce se vede pe ecran.
    Private _fila As String = "filtrare"

    ''' <summary>
    ''' Operatorul a apăsat OK: filtrul din argument e cel de așezat pe coloană (poate fi inactiv,
    ''' adică «fără filtru»).
    ''' </summary>
    Friend Event FilterAccepted As EventHandler(Of KBotFilterAcceptedEventArgs)

    ''' <summary>Operatorul a cerut o sortare. Se aplică imediat, iar meniul se închide.</summary>
    Friend Event SortRequested As EventHandler(Of KBotSortRequestedEventArgs)

    ''' <summary>
    ''' Operatorul a schimbat gruparea coloanei. Se aplică imediat, dar meniul RĂMÂNE deschis —
    ''' vezi rezumatul clasei.
    ''' </summary>
    Friend Event GroupingRequested As EventHandler(Of KBotGroupingRequestedEventArgs)

    ''' <summary>
    ''' Construiește meniul pentru o coloană: titlul afișat, tipul valorilor, valorile distincte
    ''' (deja formatate, în ordinea de sortare), filtrul curent (<c>Nothing</c> = niciunul) și
    ''' sensul de sortare al coloanei. Grila e opțională: fără ea meniul n-are filă de grupare.
    ''' </summary>
    Friend Sub New(columnKey As String, columnCaption As String, valueType As KBotValueType,
                   distinctValues As IEnumerable(Of String), currentFilter As KBotColumnFilter,
                   currentSort As KBotSortDirection, Optional grid As KBotDataView = Nothing)
        InitializeComponent()

        ' Instantaneul măsurilor AUTORATE, ÎNAINTE de orice atingere a temei (vezi ThemeTableFit):
        ' după prima scriere a schemei, valoarea aleasă cu ochiul în designer nu mai există.
        ThemeTableFit.Capture(tlySortare)
        ThemeTableFit.Capture(tlyFiltrare)
        ThemeTableFit.Capture(tlyGrupare)

        _columnKey = columnKey
        _columnCaption = If(columnCaption, String.Empty)
        _valueType = valueType
        _grid = grid
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

        AplicaTexteleDependenteDeColoana()
        RebuildShown()
        PopuleazaGruparea()
        _construit = True
        AplicaFila(navFile.SelectedKey)
    End Sub

    ''' <summary>Cheia coloanei pentru care s-a deschis meniul.</summary>
    Friend ReadOnly Property ColumnKey As String
        Get
            Return _columnKey
        End Get
    End Property

    ''' <summary>Fila deschisă acum: «sortare», «filtrare» sau «grupare».</summary>
    Friend ReadOnly Property FilaCurenta As String
        Get
            Return _fila
        End Get
    End Property

    ' Grila oferă operatorului fila de grupare? (Fără gazdă, niciodată.)
    Private ReadOnly Property AreGrupare As Boolean
        Get
            Return _grid IsNot Nothing AndAlso _grid.EnableGrouping
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
    ''' Culorile SEMANTICE, cele pe care regulile generice pe tip n-au de unde să le știe: chenarul
    ''' meniului (marginea formularului), liniile despărțitoare, suprafața pe care stau rândurile și
    ''' RÂNDURILE DE MENIU. Restul — cele două butoane de comandă, bifele, listele, barele lor de
    ''' derulare, bara de file — vine de la <c>ThemeManager.Apply</c>, prin
    ''' <see cref="KBotThemedForm"/>.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor                ' rama de 1px = Padding-ul formularului
            pnlCorp.BackColor = p.SurfaceAltColor
            pnlFile.BackColor = p.SurfaceAltColor
            pnlSortare.BackColor = p.SurfaceAltColor
            pnlFiltrare.BackColor = p.SurfaceAltColor
            pnlGrupare.BackColor = p.SurfaceAltColor
            pnlSensGrup.BackColor = p.SurfaceAltColor
            pnlButoane.BackColor = p.SurfaceAltColor
            tlySortare.BackColor = p.SurfaceAltColor
            tlyFiltrare.BackColor = p.SurfaceAltColor
            tlyGrupare.BackColor = p.SurfaceAltColor
            picCauta.BackColor = p.SurfaceAltColor
            navFile.BackColor = p.SurfaceAltColor

            ' Liniile despărțitoare sunt PANOURI, nu etichete: regula generică de Label pune
            ' BackColor = Transparent, adică o linie de 1px care nu se mai vede deloc.
            sepNav.BackColor = p.BorderColor
            sepSortare.BackColor = p.BorderColor
            sepFiltrare.BackColor = p.BorderColor
            sepGrupare.BackColor = p.BorderColor

            lstValori.BackColor = p.SurfaceAltColor  ' listele continuă suprafața meniului
            lstValori.ForeColor = p.TextColor
            lstNiveluri.BackColor = p.SurfaceAltColor
            lstNiveluri.ForeColor = p.TextColor
            lblNiveluri.ForeColor = p.TextDimColor

            ' Comenzile de meniu sunt RÂNDURI, nu butoane (vezi AplicaRandDeMeniu). Roșul lui
            ' «Șterge filtrul» vine din paletă, nu din designer: e culoarea de avertizare a schemei
            ' active, nu un Firebrick scris o dată.
            AplicaRandDeMeniu(btnSortAsc, p, p.TextColor)
            AplicaRandDeMeniu(btnSortDesc, p, p.TextColor)
            AplicaRandDeMeniu(btnSortClear, p, p.ErrorColor)
            AplicaRandDeMeniu(btnConditii, p, p.TextColor)
            AplicaRandDeMeniu(btnStergeFiltru, p, p.ErrorColor)

            ' O schemă poate cere alt aer în jurul textului (Modern: 12,8,12,8) și alt font, iar
            ' designerul a autorat totul pe Classic. Rândurile de tabel se re-măsoară aici, apoi
            ' fereastra se re-măsoară peste ele.
            AjusteazaInaltimea()
        Catch ex As Exception
            ' Boundary de temă: loghează + ÎNGHITE — o excepție aici ar rupe comutarea de schemă.
            GlobalErrorLog.Write("KBotFilterPopup.OnThemeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Un RÂND DE MENIU: plat, fără chenar, pe toată lățimea, în culoarea suprafeței pe care stă —
    ''' hover-ul e singurul lucru care-l scoate în relief, exact ca într-un meniu de sistem.
    '''
    ''' <para><b>De ce nu-l lăsăm pe seama regulii generice de buton.</b> Schema Modern randează
    ''' orice <c>Button</c> owner-drawn: îi taie colțurile cu un <c>Region</c> de rază 8 și-i pune
    ''' fundalul de buton (<c>#F3F3F3</c>). Pe un rând lat cât meniul, prin decupaje se vedea
    ''' suprafața de dedesubt (<c>#FFFFFF</c>), deci meniul arăta ca niște pastile gri lipite pe o
    ''' foaie albă, nu ca o listă de comenzi. <c>DetachButton</c> scoate Region-ul și, pe drum,
    ''' redă marginea și înălțimea AUTORATE — schema modernă le mărise ca să încapă umplutura ei.
    ''' Celelalte scheme nu rotunjesc nimic, iar apelul e idempotent: rândul iese la fel peste
    ''' tot.</para>
    ''' </summary>
    Private Sub AplicaRandDeMeniu(b As Button, p As ThemePalette, culoareText As Color)
        ModernRenderer.DetachButton(b)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = p.SurfaceAltColor
        b.ForeColor = culoareText
        b.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
        b.FlatAppearance.MouseDownBackColor = p.ButtonPressedColor
        b.UseVisualStyleBackColor = False
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' FILELE
    ' ══════════════════════════════════════════════════════════════════════════

    ' Arată fila cerută și le ascunde pe celelalte două. O cheie necunoscută cade pe «filtrare» —
    ' asta e fila pentru care se apasă pâlnia din antet.
    Private Sub AplicaFila(cheie As String)
        Dim grupare As Boolean = String.Equals(cheie, "grupare", StringComparison.Ordinal) AndAlso AreGrupare
        Dim sortare As Boolean = String.Equals(cheie, "sortare", StringComparison.Ordinal)
        _fila = If(grupare, "grupare", If(sortare, "sortare", "filtrare"))

        pnlSortare.Visible = sortare
        pnlGrupare.Visible = grupare
        pnlFiltrare.Visible = Not sortare AndAlso Not grupare

        ' Butoanele OK / Anulează sunt ale FILTRULUI: el e singurul care se predă la sfârșit.
        ' Sortarea și gruparea s-au aplicat deja când operatorul a apăsat, deci pe filele lor
        ' bara de jos ar arăta ca și cum ar mai fi ceva de confirmat.
        pnlButoane.Visible = Not sortare AndAlso Not grupare

        AjusteazaInaltimea()
    End Sub

    Private Sub NavFile_SelectionChanged(key As String) Handles navFile.SelectionChanged
        Try
            If Not _construit Then Return
            AplicaFila(key)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.NavFile_SelectionChanged", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' CE SE AȘAZĂ LA RULARE (restul e în .Designer.vb)
    ' ══════════════════════════════════════════════════════════════════════════

    ' Textele care depind de TIPUL coloanei și starea care depinde de filtrul curent.
    Private Sub AplicaTexteleDependenteDeColoana()
        btnSortAsc.Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Ascending) &
                          SemnulSortarii(KBotSortDirection.Ascending)
        btnSortDesc.Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Descending) &
                           SemnulSortarii(KBotSortDirection.Descending)
        btnSortClear.Enabled = _currentSort <> KBotSortDirection.None
        btnStergeFiltru.Text = $"Șterge filtrul din «{_columnCaption}»"
        btnStergeFiltru.Enabled = _working.IsActive

        ' Fila de grupare există doar dacă grila o oferă (KBotDataView.EnableGrouping).
        navFile.SetItemVisible("grupare", AreGrupare)

        ' Coloanele logice n-au submeniu de condiții: cele două căsuțe din listă spun deja tot ce se
        ' poate spune despre o bifă (vezi KBotFilterEngine.AllowedOperators). Butonul se ASCUNDE
        ' ȘI rândul lui se strânge la zero — altfel ar rămâne o bandă goală în mijlocul filei.
        Dim areConditii As Boolean = KBotFilterEngine.AllowedOperators(_valueType).Length > 0
        btnConditii.Visible = areConditii
        ThemeTableFit.SetRowCollapsed(tlyFiltrare, RandConditii, Not areConditii)
        If areConditii Then btnConditii.Text = KBotFilterEngine.ConditionMenuCaption(_valueType) & "  ▸"
    End Sub

    ' Sensul activ e marcat, ca operatorul să vadă pe ce e sortată deja coloana.
    Private Function SemnulSortarii(direction As KBotSortDirection) As String
        Return If(_currentSort = direction, "   ✓", String.Empty)
    End Function

    ''' <summary>
    ''' Ce SCRIE pe rândul unei valori. Golul are o etichetă a lui — un rând complet gol în listă
    ''' arată ca un rând stricat, iar operatorul trebuie să poată bifa anume celulele necompletate.
    ''' </summary>
    Friend Shared Function EtichetaValorii(value As String) As String
        If String.IsNullOrEmpty(value) Then Return "(Necompletate)"
        Return value
    End Function

    ' Umple lista cu valorile care trec de căutare (toate, dacă e goală) și pune bifele la zi.
    Private Sub RebuildShown()
        _shown.Clear()
        Dim cautat As String = txtCauta.Text.Trim()
        For i As Integer = 0 To _values.Count - 1
            If cautat.Length = 0 OrElse
               EtichetaValorii(_values(i)).Contains(cautat, StringComparison.CurrentCultureIgnoreCase) Then
                _shown.Add(i)
            End If
        Next

        _syncing = True
        Try
            lstValori.BeginUpdate()
            lstValori.Items.Clear()
            For Each i In _shown
                lstValori.Items.Add(EtichetaValorii(_values(i)), _checked.Contains(_values(i)))
            Next
            lstValori.EndUpdate()
        Finally
            _syncing = False
        End Try

        ActualizeazaSelecteazaTot()
    End Sub

    ' Bifa de sus arată starea celor ARĂTATE: toate / niciuna / unele (a treia stare).
    Private Sub ActualizeazaSelecteazaTot()
        Dim bifate As Integer = 0
        For Each i In _shown
            If _checked.Contains(_values(i)) Then bifate += 1
        Next

        _syncing = True
        Try
            If _shown.Count > 0 AndAlso bifate = _shown.Count Then
                chkSelecteazaTot.CheckState = CheckState.Checked
            ElseIf bifate = 0 Then
                chkSelecteazaTot.CheckState = CheckState.Unchecked
            Else
                chkSelecteazaTot.CheckState = CheckState.Indeterminate
            End If
        Finally
            _syncing = False
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' MĂSURA FERESTREI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Singura măsură rămasă în cod: ÎNĂLȚIMEA ferestrei. Lățimea și toate celelalte mărimi sunt
    ''' ale designerului — o fereastră care se re-măsoară singură pe lățime ar face inutil tot ce
    ''' așază operatorul acolo.
    '''
    ''' <para>Formula e una singură, pentru toate cele trei file: <b>rama corpului</b> (navigația,
    ''' linia, bara de butoane, marginile — adică tot ce nu e fila) plus <b>cât cere fila
    ''' activă</b>. Se măsoară DUPĂ un layout, nu înainte: pe un arbore de controale încă neașezat,
    ''' înălțimile citite sunt cele scrise de designer, iar diferența față de ele s-ar aduna la
    ''' fiecare apel.</para>
    ''' </summary>
    Private Sub AjusteazaInaltimea()
        ' Rândurile fixe se pun întâi pe măsura schemei (umplutura și fontul ei), altfel fereastra
        ' s-ar croi pe niște rânduri care se schimbă imediat după.
        ThemeTableFit.Fit(tlySortare)
        ThemeTableFit.Fit(tlyFiltrare)
        ThemeTableFit.Fit(tlyGrupare)
        PerformLayout()

        Dim cere As Integer = InaltimeaFilei()
        If cere <= 0 Then Return
        Dim rama As Integer = ClientSize.Height - pnlFile.Height
        Dim dorit As Integer = Math.Max(MinHeight, rama + cere)
        If ClientSize.Height = dorit Then Return
        ClientSize = New Size(ClientSize.Width, dorit)
        PerformLayout()
    End Sub

    ' Cât cere fila activă: rândurile ei FIXE, plus cât vrea lista ei elastică (rândul Percent).
    Private Function InaltimeaFilei() As Integer
        Select Case _fila
            Case "sortare"
                Return RanduriFixe(tlySortare)
            Case "grupare"
                Return RanduriFixe(tlyGrupare) + CatVreaLista(lstNiveluri, lstNiveluri.Items.Count)
            Case Else
                Return RanduriFixe(tlyFiltrare) + CatVreaLista(lstValori, _shown.Count)
        End Select
    End Function

    ' Suma rândurilor Absolute ale unui tabel (cele Percent sunt ale listei elastice).
    Private Shared Function RanduriFixe(tlp As TableLayoutPanel) As Integer
        Dim total As Single = 0
        For i As Integer = 0 To tlp.RowStyles.Count - 1
            Dim rs As RowStyle = tlp.RowStyles(i)
            If rs.SizeType = SizeType.Absolute Then total += rs.Height
        Next
        Return CInt(Math.Ceiling(total))
    End Function

    ' Cât loc vrea o listă ca să arate câte rânduri are, până la MaxListRows (cu marginile ei).
    Private Shared Function CatVreaLista(lb As ListBox, elemente As Integer) As Integer
        Dim randuri As Integer = Math.Max(1, Math.Min(elemente, MaxListRows))
        Return randuri * lb.ItemHeight + lb.Margin.Vertical
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
            AjusteazaInaltimea()

            Dim sus As Point = anchor.PointToScreen(New Point(anchorRect.Left, anchorRect.Top))
            Location = LocFataDe(sus, anchorRect)

            Dim inainte As Integer = Height
            Show(anchor.FindForm())

            ' Tema se aplică abia ACUM (KBotThemedForm.OnLoad), iar odată cu ea rândurile fixe își
            ' primesc măsura schemei — pe Modern meniul poate ieși mai înalt decât cel așezat cu
            ' două rânduri mai sus. Se re-verifică o singură dată: un meniu care iese pe sub
            ' marginea de jos a ecranului nu se mai poate citi până la capăt.
            If Height <> inainte Then Location = LocFataDe(sus, anchorRect)

            Activate()
            txtCauta.Focus()
        Catch ex As Exception
            ' Punct de intrare (creare de fereastră, geometrie de ecran) => loghează și RE-ARUNCĂ.
            GlobalErrorLog.Write("KBotFilterPopup.ShowBelow", ex)
            Throw
        End Try
    End Sub

    ' Colțul din stânga-sus al meniului față de pictograma apăsată (coordonate de ECRAN). Când nu
    ' încape dedesubt sau spre dreapta, se răstoarnă peste celelalte două laturi ale pictogramei,
    ' ca orice meniu de sistem.
    Private Function LocFataDe(susEcran As Point, anchorRect As Rectangle) As Point
        Dim la As New Point(susEcran.X, susEcran.Y + anchorRect.Height)
        Dim zona As Rectangle = Screen.FromPoint(la).WorkingArea
        If la.X + Width > zona.Right Then la.X = Math.Max(zona.Left, susEcran.X + anchorRect.Width - Width)
        If la.Y + Height > zona.Bottom Then la.Y = Math.Max(zona.Top, susEcran.Y - Height)
        Return la
    End Function

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

    ' Esc închide fără să lase nimic în urmă; Enter predă filtrul. KeyPreview e pus în designer, ca
    ' cele două taste să funcționeze indiferent ce control are focusul.
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            Select Case e.KeyCode
                Case Keys.Escape
                    e.SuppressKeyPress = True
                    Close()
                Case Keys.Enter
                    e.SuppressKeyPress = True
                    ' Enter confirmă FILTRUL. Pe celelalte două file nu e nimic de confirmat
                    ' (s-au aplicat deja), deci tasta doar închide meniul.
                    If String.Equals(_fila, "filtrare", StringComparison.Ordinal) Then
                        AcceptaFiltrul()
                    Else
                        Close()
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnKeyDown", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' EVENIMENTELE CONTROALELOR
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub BtnSortAsc_Click(sender As Object, e As EventArgs) Handles btnSortAsc.Click
        CereSortare(KBotSortDirection.Ascending)
    End Sub

    Private Sub BtnSortDesc_Click(sender As Object, e As EventArgs) Handles btnSortDesc.Click
        CereSortare(KBotSortDirection.Descending)
    End Sub

    Private Sub BtnSortClear_Click(sender As Object, e As EventArgs) Handles btnSortClear.Click
        CereSortare(KBotSortDirection.None)
    End Sub

    Private Sub BtnStergeFiltru_Click(sender As Object, e As EventArgs) Handles btnStergeFiltru.Click
        Try
            _closing = True
            RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(New KBotColumnFilter(_columnKey)))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.BtnStergeFiltru_Click", ex)
        End Try
    End Sub

    Private Sub BtnConditii_Click(sender As Object, e As EventArgs) Handles btnConditii.Click
        Try
            ' Ancora se cere în coordonatele CORPULUI: butonul stă cu trei părinți mai jos (tabel,
            ' filă, gazda filelor), deci Bounds-ul lui e față de tabel, nu față de pnlCorp.
            DeschideConditii(pnlCorp.RectangleToClient(btnConditii.Parent.RectangleToScreen(btnConditii.Bounds)))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.BtnConditii_Click", ex)
        End Try
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        AcceptaFiltrul()
    End Sub

    Private Sub BtnAnuleaza_Click(sender As Object, e As EventArgs) Handles btnAnuleaza.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.BtnAnuleaza_Click", ex)
        End Try
    End Sub

    Private Sub TxtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            RebuildShown()
            AjusteazaInaltimea()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.TxtCauta_TextChanged", ex)
        End Try
    End Sub

    ' Bifează / debifează toate valorile ARĂTATE (adică cele care trec de căutare). Peste o listă
    ' căutată, «Selectează tot» care ar atinge și valorile nevăzute ar fi o comandă care face mai
    ' mult decât se vede pe ecran.
    Private Sub ChkSelecteazaTot_Click(sender As Object, e As EventArgs) Handles chkSelecteazaTot.Click
        Try
            If _syncing Then Return
            ComutaToate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.ChkSelecteazaTot_Click", ex)
        End Try
    End Sub

    ' ItemCheck vine ÎNAINTE ca lista să-și schimbe starea, deci se citește e.NewValue, nu bifa.
    '
    ' Clauza «Handles» de mai jos NU e decorativă și n-are voie să dispară: fără ea, bifele puse cu
    ' mouse-ul nu ajung niciodată în _checked. Meniul arată corect, dar filtrul predat la OK e cel
    ' de dinaintea oricărui clic — iar «debifează tot, apoi bifează una» (drumul obișnuit) predă un
    ' set GOL, adică o grilă goală. S-a pierdut o dată exact așa, la o redenumire.
    Private Sub LstValori_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstValori.ItemCheck
        Try
            If _syncing Then Return
            If e.Index < 0 OrElse e.Index >= _shown.Count Then Return
            Dim v As String = _values(_shown(e.Index))
            If e.NewValue = CheckState.Checked Then
                _checked.Add(v)
            Else
                _checked.Remove(v)
            End If
            ActualizeazaSelecteazaTot()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.LstValori_ItemCheck", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' FILA DE GRUPARE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Umple fila de grupare din starea REALĂ a grilei: nivelul așezat pe coloana aceasta (dacă
    ''' există) și ierarhia de niveluri, în ordinea ei. Nimic din ce se vede aici nu e scris în
    ''' designer în afară de controale — cheile, titlurile și sensurile sunt ale grilei.
    ''' </summary>
    Private Sub PopuleazaGruparea()
        _syncing = True
        Try
            chkGrupeaza.Text = $"Grupează după «{_columnCaption}»"

            Dim n As KBotGroupLevel = If(_grid Is Nothing, Nothing, _grid.GroupLevelFor(_columnKey))
            chkGrupeaza.Checked = n IsNot Nothing
            rbGrupDesc.Checked = n IsNot Nothing AndAlso n.SortDirection = KBotSortDirection.Descending
            rbGrupCresc.Checked = Not rbGrupDesc.Checked
            chkGrupAntet.Checked = If(n Is Nothing, True, n.ShowHeader)
            chkGrupSubsol.Checked = If(n Is Nothing, True, n.ShowFooter)
            chkGrupAgregate.Checked = If(n Is Nothing, False, n.ShowHeaderAggregates)
            chkGrupStrangere.Checked = If(n Is Nothing, True, n.Collapsible)
            chkGrupPornitStrans.Checked = If(n Is Nothing, False, n.CollapsedByDefault)

            ActualizeazaNivelurile()
            ActiveazaOptiunileGrupare()
        Finally
            _syncing = False
        End Try
    End Sub

    ' Ierarhia grilei, o linie pe nivel, cu coloana curentă marcată — altfel operatorul nu are de
    ' unde ști pe al câtelea etaj a nimerit ce tocmai a bifat.
    Private Sub ActualizeazaNivelurile()
        lstNiveluri.BeginUpdate()
        Try
            lstNiveluri.Items.Clear()
            If _grid Is Nothing Then Return
            Dim niveluri As IReadOnlyList(Of KBotGroupLevel) = _grid.ActiveLevels()
            If niveluri.Count = 0 Then
                lstNiveluri.Items.Add("(grila nu e grupată)")
                Return
            End If
            For i As Integer = 0 To niveluri.Count - 1
                Dim nv As KBotGroupLevel = niveluri(i)
                Dim titlu As String = TitluColoanei(nv.ColumnKey)
                Dim sens As String = If(nv.SortDirection = KBotSortDirection.Descending,
                                        "descrescător", "crescător")
                Dim aici As String = If(String.Equals(nv.ColumnKey, _columnKey, StringComparison.Ordinal),
                                        "   ← coloana aceasta", String.Empty)
                lstNiveluri.Items.Add($"{i + 1}. {titlu} ({sens}){aici}")
            Next
        Finally
            lstNiveluri.EndUpdate()
        End Try
    End Sub

    ' Titlul coloanei din grilă; pe o cheie fără titlu rămâne cheia (mai bine decât un rând gol).
    Private Function TitluColoanei(colKey As String) As String
        Try
            Dim col As KBotDataColumn = _grid.Column(colKey)
            If Not String.IsNullOrWhiteSpace(col.HeaderText) Then Return col.HeaderText
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.TitluColoanei", ex)
        End Try
        Return colKey
    End Function

    ' Fără grupare pe coloană, opțiunile ei n-au despre ce vorbi: se sting, nu se ascund — un rând
    ' care dispare și reapare la fiecare bifă face fila să sară sub cursor.
    Private Sub ActiveazaOptiunileGrupare()
        Dim pornit As Boolean = chkGrupeaza.Checked
        rbGrupCresc.Enabled = pornit
        rbGrupDesc.Enabled = pornit
        chkGrupAntet.Enabled = pornit
        chkGrupSubsol.Enabled = pornit
        chkGrupAgregate.Enabled = pornit AndAlso chkGrupAntet.Checked
        chkGrupStrangere.Enabled = pornit AndAlso chkGrupAntet.Checked
        chkGrupPornitStrans.Enabled = chkGrupStrangere.Enabled AndAlso chkGrupStrangere.Checked
    End Sub

    ' Orice atingere din fila de grupare cere aceeași lucrare: se compune nivelul din controale și
    ' se predă gazdei. Un singur handler pentru toate șapte — șapte handlere identice ar fi șapte
    ' locuri în care se poate uita un rând.
    Private Sub OptiuniGrupare_Changed(sender As Object, e As EventArgs) _
        Handles chkGrupeaza.CheckedChanged, rbGrupCresc.CheckedChanged, rbGrupDesc.CheckedChanged,
                chkGrupAntet.CheckedChanged, chkGrupSubsol.CheckedChanged,
                chkGrupAgregate.CheckedChanged, chkGrupStrangere.CheckedChanged,
                chkGrupPornitStrans.CheckedChanged
        Try
            If _syncing OrElse Not _construit Then Return
            ActiveazaOptiunileGrupare()
            CereGruparea()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OptiuniGrupare_Changed", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Nivelul pe care l-ar preda fila de grupare ACUM (<c>Nothing</c> = coloana nu mai grupează).
    ''' Separat de <see cref="CereGruparea"/> ca regula să poată fi probată fără ecran.
    '''
    ''' <para>Nivelul EXISTENT se refolosește, nu se înlocuiește cu unul nou: pe el pot sta culori
    ''' și fonturi puse din designer (<c>HeaderBackColor</c>, <c>FooterFont</c>…), iar o bifă din
    ''' meniu n-are voie să le șteargă.</para>
    ''' </summary>
    Friend Function BuildGroupLevel() As KBotGroupLevel
        If Not chkGrupeaza.Checked Then Return Nothing

        Dim n As KBotGroupLevel = If(_grid Is Nothing, Nothing, _grid.GroupLevelFor(_columnKey))
        If n Is Nothing Then n = New KBotGroupLevel()

        n.ColumnKey = _columnKey
        n.SortDirection = If(rbGrupDesc.Checked, KBotSortDirection.Descending, KBotSortDirection.Ascending)
        n.ShowHeader = chkGrupAntet.Checked
        n.ShowFooter = chkGrupSubsol.Checked
        n.ShowHeaderAggregates = chkGrupAgregate.Checked
        n.Collapsible = chkGrupStrangere.Checked
        n.CollapsedByDefault = chkGrupPornitStrans.Checked
        Return n
    End Function

    ' Predă gruparea și RĂMÂNE deschis (vezi rezumatul clasei), apoi își reface propria ierarhie:
    ' nivelul tocmai adăugat trebuie să se vadă în listă, altfel operatorul nu are nicio confirmare.
    Private Sub CereGruparea()
        RaiseEvent GroupingRequested(Me, New KBotGroupingRequestedEventArgs(_columnKey, BuildGroupLevel()))
        _syncing = True
        Try
            ActualizeazaNivelurile()
        Finally
            _syncing = False
        End Try
        AjusteazaInaltimea()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' ACȚIUNI
    ' ══════════════════════════════════════════════════════════════════════════

    ' Aplică sortarea cerută și închide — sortarea e o comandă, nu o alegere de confirmat.
    Private Sub CereSortare(direction As KBotSortDirection)
        Try
            _closing = True
            RaiseEvent SortRequested(Me, New KBotSortRequestedEventArgs(_columnKey, direction))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.CereSortare", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Filtrul pe care l-ar preda un «OK» apăsat ACUM. Separat de <c>AcceptaFiltrul</c> ca regula
    ''' de mai jos să poată fi probată fără ecran — meniul e o fereastră, deciziile lui nu.
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
        Try
            _closing = True
            RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(BuildFilter()))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.AcceptaFiltrul", ex)
        End Try
    End Sub

    Private Sub ComutaToate()
        Dim toateBifate As Boolean = ToateAratateBifate()
        For Each i In _shown
            If toateBifate Then
                _checked.Remove(_values(i))
            Else
                _checked.Add(_values(i))
            End If
        Next
        SincronizeazaBifeleListei()
        ActualizeazaSelecteazaTot()
    End Sub

    Private Function ToateAratateBifate() As Boolean
        For Each i In _shown
            If Not _checked.Contains(_values(i)) Then Return False
        Next
        Return _shown.Count > 0
    End Function

    ' Pune bifele din listă pe starea modelului (fără a trece prin ItemCheck-ul operatorului).
    Private Sub SincronizeazaBifeleListei()
        _syncing = True
        Try
            For poz As Integer = 0 To _shown.Count - 1
                lstValori.SetItemChecked(poz, _checked.Contains(_values(_shown(poz))))
            Next
        Finally
            _syncing = False
        End Try
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
                If Not [Enum].TryParse(Of KBotFilterOperator)(ev.Item.Key, ales) Then Return
                ' Submeniul se DĂ LA O PARTE ÎNTÂI. CustomPopup ridică ItemClicked ÎNAINTE de
                ' Close (vezi CustomPopup.CloseWith), iar dialogul de condiție e modal: fără
                ' rândul de mai jos, meniul ar rămâne pe ecran, viu și inutil, până la închiderea
                ' dialogului. Close-ul lui vine oricum, imediat ce ne întoarcem de aici.
                meniu.Hide()
                AplicaConditia(ales)
            End Sub
        AddHandler meniu.FormClosed,
            Sub(s As Object, ev As FormClosedEventArgs)
                _suppressDeactivate = False
                ' Dacă alegerea din submeniu n-a închis meniul-părinte (operatorul a apăsat Esc),
                ' focusul se întoarce aici — altfel ar rămâne o fereastră vizibilă și moartă.
                If Not _closing AndAlso Not IsDisposed Then Activate()
            End Sub

        ' Ancora e în coordonatele CORPULUI (vezi BtnConditii_Click).
        meniu.ShowBelow(pnlCorp, anchorRow)
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
        ' Garda se pune ÎNAINTE de Hide: ascunderea ferestrei active mută activarea pe altcineva,
        ' adică ridică OnDeactivate — care altfel ar închide meniul chiar acum.
        _suppressDeactivate = True
        Hide()
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
        Dim poz As Integer = _shown.IndexOf(i)
        If poz < 0 Then Throw New ArgumentException($"Valoarea «{displayText}» nu e în lista arătată acum.", NameOf(displayText))
        Dim v As String = _values(i)
        If Not _checked.Remove(v) Then
            _checked.Add(v)
        End If
        SincronizeazaBifeleListei()
        ActualizeazaSelecteazaTot()
    End Sub

    ''' <summary>Scrie în caseta de căutare, ca și cum ar fi tastat operatorul.</summary>
    Friend Sub DebugSearch(text As String)
        txtCauta.Text = text
    End Sub

    ''' <summary>Trece pe o filă, ca un clic pe bara de sus.</summary>
    Friend Sub DebugSelectTab(key As String)
        navFile.SelectedKey = key
    End Sub

    ''' <summary>Ce scrie pe rândurile listei de niveluri (fila de grupare).</summary>
    Friend Function DebugLevelLines() As String()
        Dim linii(lstNiveluri.Items.Count - 1) As String
        For i As Integer = 0 To lstNiveluri.Items.Count - 1
            linii(i) = CStr(lstNiveluri.Items(i))
        Next
        Return linii
    End Function

    ''' <summary>Așază geometria și întoarce mărimea la care a ieșit fereastra.</summary>
    Friend Function DebugMeasure() As Size
        AjusteazaInaltimea()
        PerformLayout()
        Return Size
    End Function
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

''' <summary>Argumentele lui <c>KBotFilterPopup.GroupingRequested</c> (slice 0030).</summary>
Friend NotInheritable Class KBotGroupingRequestedEventArgs
    Inherits EventArgs

    Public Sub New(columnKey As String, level As KBotGroupLevel)
        Me.ColumnKey = columnKey
        Me.Level = level
    End Sub

    ''' <summary>Coloana a cărei grupare s-a schimbat.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>Nivelul de așezat pe coloană; <c>Nothing</c> = coloana nu mai grupează.</summary>
    Public ReadOnly Property Level As KBotGroupLevel

End Class
