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
''' <para><b>Slice 0028-06: meniul e AUTORAT ÎN DESIGNER.</b> Până aici era o fereastră desenată
''' integral de noi (≈400 de linii de pictură plus tot atâtea de hit-test și geometrie), fiindcă un
''' <c>ContextMenuStrip</c> cu un <c>CheckedListBox</c> ar fi rămas două dreptunghiuri albe pe o
''' schemă întunecată. Motivul acela a dispărut între timp: <c>ThemeManager</c> are reguli pe tip
''' pentru <c>CheckedListBox</c>, <c>CheckBox</c>, <c>Button</c> și <c>Panel</c> (inclusiv tema
''' nativă a barelor de derulare), iar <see cref="KBotThemedForm"/> le aplică singur. Deci acum
''' TOATE controalele stau în <c>KBotFilterPopup.Designer.vb</c>, ca la orice formular al casei, iar
''' fișierul acesta ține DOAR comportamentul.</para>
'''
''' <para><b>Ce rămâne al rulării, și de ce:</b> textele care depind de tipul coloanei (sortarea se
''' numește «A → Z» pe text și «de la mic la mare» pe numere), starea de activare a lui «Șterge
''' filtrul», existența butonului de condiții (coloanele logice n-au submeniu) și
''' <b>conținutul listei de valori</b> — valorile distincte ale unei coloane nu există la
''' proiectare. Controlul care le arată, în schimb, e al designerului, ca tot restul.</para>
'''
''' <para><b>Ce alege operatorul se predă la OK, nu pe loc.</b> Popup-ul lucrează pe o COPIE a
''' filtrului (<see cref="KBotColumnFilter.Clone"/>) și ridică <see cref="FilterAccepted"/> abia la
''' apăsarea OK; «Anulează» și Esc nu lasă nimic în urmă. Sortarea, în schimb, se aplică IMEDIAT și
''' închide meniul — ea nu e o alegere de confirmat, e o comandă, exact ca în Access.</para>
''' </summary>
<ToolboxItem(False)>
Partial Friend NotInheritable Class KBotFilterPopup

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    ''' <summary>Câte valori se văd fără derulare — restul, la scroll (lista are bara ei).</summary>
    Private Const MaxListRows As Integer = 10

    ' ── Ce filtrăm ───────────────────────────────────────────────────────────────
    Private ReadOnly _columnKey As String
    Private ReadOnly _columnCaption As String
    Private ReadOnly _valueType As KBotValueType
    Private ReadOnly _values As New List(Of String)()          ' textele distincte, în ordine
    Private ReadOnly _checked As HashSet(Of String)
    Private ReadOnly _working As KBotColumnFilter
    Private ReadOnly _currentSort As KBotSortDirection

    ' Indicii din _values care trec de căutare — adică exact ce e în lstValori, în aceeași ordine.
    Private ReadOnly _shown As New List(Of Integer)()

    ' Cât timp e True, evenimentele controalelor sunt ecoul nostru, nu al operatorului.
    Private _syncing As Boolean = False
    Private _suppressDeactivate As Boolean = False
    Private _closing As Boolean = False

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
        InitializeComponent()

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

        AplicaTexteleDependenteDeColoana()
        RebuildShown()
        AjusteazaInaltimea()
    End Sub

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
    ''' Culorile SEMANTICE, cele pe care regulile generice pe tip n-au de unde să le știe: chenarul
    ''' meniului (marginea formularului), cele două linii despărțitoare, suprafața pe care stau
    ''' rândurile și cele patru RÂNDURI DE MENIU de sus. Restul — cele două butoane de comandă,
    ''' bifa, lista, bara ei de derulare — vine de la <c>ThemeManager.Apply</c>, prin
    ''' <see cref="KBotThemedForm"/>.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor                ' rama de 1px = Padding-ul formularului
            pnlCorp.BackColor = p.SurfaceAltColor
            pnlButoane.BackColor = p.SurfaceAltColor
            sepSortare.BackColor = p.BorderColor
            sepConditii.BackColor = p.BorderColor
            lstValori.BackColor = p.SurfaceAltColor  ' lista continuă suprafața meniului
            lstValori.ForeColor = p.TextColor

            ' Cele patru comenzi de sus sunt RÂNDURI, nu butoane (vezi AplicaRandDeMeniu). Roșul lui
            ' «Șterge filtrul» vine din paletă, nu din designer: e culoarea de avertizare a schemei
            ' active, nu un Firebrick scris o dată.
            AplicaRandDeMeniu(btnSortAsc, p, p.TextColor)
            AplicaRandDeMeniu(btnSortDesc, p, p.TextColor)
            AplicaRandDeMeniu(btnStergeFiltru, p, p.ErrorColor)
            AplicaRandDeMeniu(btnConditii, p, p.TextColor)

            ' O schemă poate cere alt aer în jurul textului (Modern: 12,8,12,8), iar butoanele de
            ' comandă cresc atunci ca să încapă și umplutura, și textul (vezi ModernRenderer).
            ' Rândurile de mai sus și-au primit înapoi înălțimea autorată, dar bifa, caseta de
            ' căutare și bara de butoane pot încă să se miște: fereastra se re-măsoară aici, ca
            ' numărul de rânduri arătate să rămână cel de dinainte.
            PerformLayout()
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
    ''' fundalul de buton (<c>#F3F3F3</c>). Pe un rând lat cât meniul, prin cele patru decupaje se
    ''' vedea suprafața de dedesubt (<c>#FFFFFF</c>), deci meniul arăta ca patru pastile gri lipite
    ''' pe o foaie albă, nu ca o listă de comenzi. <c>DetachButton</c> scoate Region-ul și, pe drum,
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
    ' CE SE AȘAZĂ LA RULARE (restul e în .Designer.vb)
    ' ══════════════════════════════════════════════════════════════════════════

    ' Textele care depind de TIPUL coloanei și starea care depinde de filtrul curent.
    Private Sub AplicaTexteleDependenteDeColoana()
        btnSortAsc.Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Ascending) &
                          SemnulSortarii(KBotSortDirection.Ascending)
        btnSortDesc.Text = KBotFilterEngine.SortCaption(_valueType, KBotSortDirection.Descending) &
                           SemnulSortarii(KBotSortDirection.Descending)
        btnStergeFiltru.Text = $"Șterge filtrul din «{_columnCaption}»"
        btnStergeFiltru.Enabled = _working.IsActive

        ' Coloanele logice n-au submeniu de condiții: cele două căsuțe din listă spun deja tot ce se
        ' poate spune despre o bifă (vezi KBotFilterEngine.AllowedOperators). Butonul se ASCUNDE,
        ' deci și înălțimea lui dispare — un buton stins ar fi un rând care nu duce nicăieri.
        Dim areConditii As Boolean = KBotFilterEngine.AllowedOperators(_valueType).Length > 0
        btnConditii.Visible = areConditii
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
               EtichetaValorii(_values(i)).IndexOf(cautat, StringComparison.CurrentCultureIgnoreCase) >= 0 Then
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

    ''' <summary>
    ''' Singura măsură rămasă în cod: ÎNÂLȚIMEA ferestrei, ca lista să arate câte rânduri are
    ''' (până la <see cref="MaxListRows"/>). Lățimea și toate celelalte mărimi sunt ale
    ''' designerului — o fereastră care se re-măsoară singură pe lățime ar face inutil tot ce
    ''' așază operatorul acolo.
    ''' </summary>
    Private Sub AjusteazaInaltimea()
        Dim randuri As Integer = Math.Max(1, Math.Min(_shown.Count, MaxListRows))
        Dim listaVrea As Integer = randuri * lstValori.ItemHeight
        Dim delta As Integer = listaVrea - lstValori.Height
        If delta = 0 Then Return
        ClientSize = New Size(ClientSize.Width, Math.Max(lstValori.ItemHeight, ClientSize.Height + delta))
    End Sub

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
            Dim la As New Point(sus.X, sus.Y + anchorRect.Height)
            Dim zona As Rectangle = Screen.FromPoint(la).WorkingArea

            If la.X + Width > zona.Right Then la.X = Math.Max(zona.Left, sus.X + anchorRect.Width - Width)
            If la.Y + Height > zona.Bottom Then la.Y = Math.Max(zona.Top, sus.Y - Height)
            Location = la

            Show(anchor.FindForm())
            Activate()
            txtCauta.Focus()
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
                    AcceptaFiltrul()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnKeyDown", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' EVENIMENTELE CONTROALELOR
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub btnSortAsc_Click(sender As Object, e As EventArgs) Handles btnSortAsc.Click
        CereSortare(KBotSortDirection.Ascending)
    End Sub

    Private Sub btnSortDesc_Click(sender As Object, e As EventArgs) Handles btnSortDesc.Click
        CereSortare(KBotSortDirection.Descending)
    End Sub

    Private Sub btnStergeFiltru_Click(sender As Object, e As EventArgs) Handles btnStergeFiltru.Click
        Try
            _closing = True
            RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(New KBotColumnFilter(_columnKey)))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.btnStergeFiltru_Click", ex)
        End Try
    End Sub

    Private Sub btnConditii_Click(sender As Object, e As EventArgs) Handles btnConditii.Click
        Try
            DeschideConditii(btnConditii.Bounds)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.btnConditii_Click", ex)
        End Try
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        AcceptaFiltrul()
    End Sub

    Private Sub btnAnuleaza_Click(sender As Object, e As EventArgs) Handles btnAnuleaza.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.btnAnuleaza_Click", ex)
        End Try
    End Sub

    Private Sub txtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            RebuildShown()
            AjusteazaInaltimea()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.txtCauta_TextChanged", ex)
        End Try
    End Sub

    ' Bifează / debifează toate valorile ARĂTATE (adică cele care trec de căutare). Peste o listă
    ' căutată, «Selectează tot» care ar atinge și valorile nevăzute ar fi o comandă care face mai
    ' mult decât se vede pe ecran.
    Private Sub chkSelecteazaTot_Click(sender As Object, e As EventArgs) Handles chkSelecteazaTot.Click
        Try
            If _syncing Then Return
            ComutaToate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.chkSelecteazaTot_Click", ex)
        End Try
    End Sub

    ' ItemCheck vine ÎNAINTE ca lista să-și schimbe starea, deci se citește e.NewValue, nu bifa.
    Private Sub lstValori_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstValori.ItemCheck
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
            GlobalErrorLog.Write("KBotFilterPopup.lstValori_ItemCheck", ex)
        End Try
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

        ' Ancora e în coordonatele CORPULUI, nu ale ferestrei: butonul stă în pnlCorp.
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
        If _checked.Contains(v) Then
            _checked.Remove(v)
        Else
            _checked.Add(v)
        End If
        SincronizeazaBifeleListei()
        ActualizeazaSelecteazaTot()
    End Sub

    ''' <summary>Scrie în caseta de căutare, ca și cum ar fi tastat operatorul.</summary>
    Friend Sub DebugSearch(text As String)
        txtCauta.Text = text
    End Sub

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
