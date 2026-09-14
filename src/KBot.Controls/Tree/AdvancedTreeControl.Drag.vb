Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' TRAGEREA UNUI NOD PESTE ALTUL (felia 0048-04, decizia D-K).
'''
''' <para><b>De ce e în control și nu în vedere.</b> D-K din
''' <c>docs/FUNDAMENT_Asociere_Receptii.md</c>: tragerea aparține arborelui, nu formularului de
''' asociere. Consecința care contează e că <b>vetoul ajunge la validarea aruncării</b>, nu îngropat
''' în tratarea ei: cine ascultă <see cref="NodeDragOver"/> spune «da» sau «nu, uite de ce»
''' ÎNAINTE ca operatorul să dea drumul mouse-ului, iar arborele arată răspunsul pe loc. Un veto
''' descoperit după aruncare ar fi un mesaj de eroare; unul descoperit în timpul tragerii e un
''' cursor care spune nu.</para>
'''
''' <para><b>Se folosește <c>DoDragDrop</c> din WinForms</b>, nu o urmărire proprie a mouse-ului.
''' Motivul e cursorul: bucla modală a sistemului dă gratis cursorul «nu se poate» și oprește
''' tragerea corect la ESC sau la pierderea ferestrei. Efectul secundar e binevenit —
''' <c>MouseUp</c> nu mai ajunge la control după o tragere, deci <c>ClickDelayTimer</c> nu mai
''' pornește și aruncarea nu se mai citește ȘI ca un clic.</para>
'''
''' <para><b>Nimic nu se mișcă singur.</b> Controlul NU mută nodul: ridică
''' <see cref="NodeDropped"/> și atât. Arborele de asociere e o proiecție a datelor de pe server —
''' dacă frunza s-ar muta local, ecranul ar arăta o legătură pe care nimeni nu a scris-o încă.</para>
'''
''' <para><b>Implicit e stins.</b> <see cref="DragEnabled"/> = False, deci cele nouă vederi care
''' folosesc deja arborele nu capătă un comportament nou fără să-l ceară.</para>
''' </summary>
Partial Public Class AdvancedTreeControl

    ' ── Starea tragerii ──────────────────────────────────────────────────────────────
    ' Punctul apăsării, ca să știm când depășim pragul de tragere al sistemului. Fără prag,
    ' orice clic cu un pixel de tremur ar porni o tragere.
    Private _dragOrigin As Point = Point.Empty
    Private _dragCandidate As TreeItem = Nothing

    ' Nodul tras și nodul de sub cursor, cât ține tragerea. Ambele Nothing în afara ei.
    Private _dragSource As TreeItem = Nothing

    ''' <summary>
    ''' Rândurile care se trag ACUM, când sunt mai multe (vezi partiala .MultiSelect).
    '''
    ''' <para><b>De ce e Shared.</b> Bucla de tragere a sistemului e MODALĂ și e una singură pe
    ''' proces: în orice clipă se trage cel mult un lucru, deci un câmp comun nu poate fi citit
    ''' de o a doua tragere. Iar tragerea ÎNTRE doi arbori are nevoie exact de asta: arborele
    ''' care primește nu e cel care a pornit tragerea, deci câmpurile lui de instanță sunt goale,
    ''' și el trebuie totuși să afle că vin cinci rânduri, nu unul.</para>
    '''
    ''' <para>Obiectul de date rămâne ce era — rândul APĂSAT — ca tot ce citea de acolo înainte
    ''' să citească la fel și acum.</para>
    ''' </summary>
    Private Shared _draggedGroup As List(Of TreeItem) = Nothing
    Private _dropTarget As TreeItem = Nothing
    Private _dropAllowed As Boolean = False
    Private _dropMotiv As String = String.Empty

    ' Ce țintă e „în etichetă" acum. Fără el, fiecare pixel de mișcare peste același rând ar
    ' reprograma apariția, iar motivul refuzului n-ar apărea niciodată.
    Private _dropTipTinta As TreeItem = Nothing
    Private ReadOnly _dropTipContinut As New KBotToolTipContent()

    Private _autoDragHighlight As Color = Color.FromArgb(&H00, &H7A, &HCC)
    Private _autoDragForbidden As Color = Color.FromArgb(&HBE, &H1E, &H1E)

    ' ══════════════════════════════════════════════════════════════════════════
    ' Proprietăți
    ' ══════════════════════════════════════════════════════════════════════════

    Private _dragEnabled As Boolean = False
    <Category("K-BOT: DND")>
    <Description("Permite tragerea unui nod peste altul. Controlul nu mută nimic singur — doar ridică evenimentele.")>
    <DefaultValue(False)>
    Public Property DragEnabled As Boolean
        Get
            Return _dragEnabled
        End Get
        Set(value As Boolean)
            If _dragEnabled = value Then Return
            _dragEnabled = value
            ' AllowDrop e ce face controlul să primească DragOver/DragDrop. Se pune aici, nu în
            ' constructor: un arbore care nu trage nu are de ce să fie țintă de aruncare pentru
            ' altcineva (un fișier tras din Explorer, de pildă).
            If Not KBotDesignTime.IsDesignTime(Me) Then Me.AllowDrop = value
            If Not value Then CancelDrag()
        End Set
    End Property

    ''' <summary>
    ''' Culoarea chenarului de pe rândul peste care se poate arunca.
    ''' <c>Color.Empty</c> = din temă (accentul).
    ''' </summary>
    Private _dragHighlightColor As Color = Color.Empty
    <Category("K-BOT: DND")>
    <Description("Chenarul rândului care poate primi nodul. Empty = din temă.")>
    Public Property DragHighlightColor As Color
        Get
            Return If(_dragHighlightColor.IsEmpty, _autoDragHighlight, _dragHighlightColor)
        End Get
        Set(value As Color)
            _dragHighlightColor = value
            Me.Invalidate()
        End Set
    End Property
    ' Fără perechea asta, Visual Studio ar scrie culoarea REZOLVATĂ în .Designer.vb, iar valoarea
    ' înghețată s-ar citi pe veci ca o alegere deliberată a operatorului (regula casei).
    Private Function ShouldSerializeDragHighlightColor() As Boolean
        Return Not _dragHighlightColor.IsEmpty
    End Function
    Private Sub ResetDragHighlightColor()
        _dragHighlightColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Culoarea rândului care NU poate primi nodul. <c>Color.Empty</c> = din temă (eroarea).
    ''' </summary>
    Private _dragForbiddenColor As Color = Color.Empty
    <Category("K-BOT: DND")>
    <Description("Chenarul rândului care refuză nodul. Empty = din temă.")>
    Public Property DragForbiddenColor As Color
        Get
            Return If(_dragForbiddenColor.IsEmpty, _autoDragForbidden, _dragForbiddenColor)
        End Get
        Set(value As Color)
            _dragForbiddenColor = value
            Me.Invalidate()
        End Set
    End Property
    Private Function ShouldSerializeDragForbiddenColor() As Boolean
        Return Not _dragForbiddenColor.IsEmpty
    End Function
    Private Sub ResetDragForbiddenColor()
        _dragForbiddenColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>Nodul care se trage acum, sau <c>Nothing</c>. Doar de citit.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DraggedItem As TreeItem
        Get
            Return _dragSource
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' Evenimente
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Un nod e pe cale să fie tras. Gazda spune dacă are voie.
    '''
    ''' <para>Aici se opresc rândurile care nu sunt de mutat — rădăcinile de recepție (D-J) și
    ''' instantaneele blocate de ordonanțări sau plăți: <b>vizibile, dar nu de mutat</b>.</para>
    ''' </summary>
    Public Event NodeDragStarting(sender As Object, e As TreeDragStartEventArgs)

    ''' <summary>
    ''' Cursorul e peste un rând în timpul tragerii. Gazda spune dacă aruncarea e permisă și,
    ''' dacă nu, DE CE — motivul apare ca etichetă plutitoare, în română.
    '''
    ''' <para>Aici ajung vetourile F13 (data), F14 (indicatorii) și F16 (mulțimile doar cresc):
    ''' operatorul le vede înainte de a da drumul mouse-ului, nu după.</para>
    ''' </summary>
    Public Event NodeDragOver(sender As Object, e As TreeDragOverEventArgs)

    ''' <summary>
    ''' Nodul a fost aruncat pe o țintă care a răspuns «da». Controlul NU mută nimic — gazda
    ''' decide ce înseamnă mutarea și reîncarcă arborele când datele s-au schimbat.
    ''' </summary>
    Public Event NodeDropped(sender As Object, e As TreeDropEventArgs)

    ' ══════════════════════════════════════════════════════════════════════════
    ' Pornirea — chemată din OnMouseDown / OnMouseMove
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Reține de unde s-ar putea porni o tragere. Nu pornește nimic încă.</summary>
    Friend Sub ArmDrag(it As TreeItem, p As Point, buton As MouseButtons)
        If Not _dragEnabled OrElse buton <> MouseButtons.Left Then
            _dragCandidate = Nothing
            Return
        End If
        _dragCandidate = it
        _dragOrigin = p
    End Sub

    ''' <summary>
    ''' Pornește tragerea dacă mouse-ul s-a depărtat destul. Întoarce True dacă a tras (și deci
    ''' restul lui <c>OnMouseMove</c> nu mai are ce căuta: bucla modală s-a terminat abia acum).
    ''' </summary>
    Friend Function MaybeBeginDrag(p As Point, buton As MouseButtons) As Boolean
        If Not _dragEnabled Then Return False
        If _dragCandidate Is Nothing Then Return False
        If (buton And MouseButtons.Left) <> MouseButtons.Left Then
            _dragCandidate = Nothing
            Return False
        End If

        Dim prag As Size = SystemInformation.DragSize
        If Math.Abs(p.X - _dragOrigin.X) < prag.Width AndAlso
           Math.Abs(p.Y - _dragOrigin.Y) < prag.Height Then Return False

        Dim it As TreeItem = _dragCandidate
        _dragCandidate = Nothing

        ' Apăsarea a devenit tragere, deci strângerea grupului la un rând NU se mai face: exact
        ' asta e gestul prin care se trag mai multe deodată (vezi .MultiSelect).
        _pendingSingleSelect = Nothing

        Dim start As New TreeDragStartEventArgs(it)
        start.SetItems(DragGroupFor(it))
        RaiseEvent NodeDragStarting(Me, start)
        If start.Cancel Then Return False

        ' Gazda poate SCOATE rânduri din grup (unul înghețat printre cele bune, de pildă). Dacă
        ' le scoate pe toate, nu mai e nimic de tras — și nu e o eroare, e un răspuns.
        Dim grup As New List(Of TreeItem)()
        For Each rand As TreeItem In start.Items
            If rand IsNot Nothing Then grup.Add(rand)
        Next
        If grup.Count = 0 Then Return False

        _dragSource = it
        _dropTarget = Nothing
        _dropAllowed = False
        _dropMotiv = String.Empty
        _draggedGroup = grup
        Try
            ' Bucla modală a sistemului. Se întoarce abia la aruncare, la ESC sau la pierderea
            ' ferestrei — toate trei ies pe același drum, prin CancelDrag de mai jos.
            Me.DoDragDrop(it, DragDropEffects.Move)
        Finally
            _draggedGroup = Nothing
            CancelDrag()
        End Try
        Return True
    End Function

    ''' <summary>Stinge orice urmă de tragere. Sigură de chemat oricând.</summary>
    Friend Sub CancelDrag()
        ' Fantomele ies pe ACEST drum, oricare ar fi fost sfârșitul tragerii — aruncare, ESC sau
        ' ieșirea din fereastră. Un al doilea drum ar fi unul care se poate uita.
        ClearDropPreview()
        Dim eraCeva As Boolean = _dragSource IsNot Nothing OrElse _dropTarget IsNot Nothing
        _dragSource = Nothing
        _dragCandidate = Nothing
        _dropTarget = Nothing
        _dropAllowed = False
        _dropMotiv = String.Empty
        _dropTipTinta = Nothing
        _butonTooltip?.HideNow()
        If eraCeva Then Me.Invalidate()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Ținta — suprascrierile de aruncare
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnDragEnter(drgevent As DragEventArgs)
        Try
            MyBase.OnDragEnter(drgevent)
            drgevent.Effect = DragDropEffects.None
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnDragEnter", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Nodul tras, luat DIN OBIECTUL DE DATE, nu din câmpul propriu.
    '''
    ''' <para>Diferența contează la tragerea ÎNTRE doi arbori: <c>_dragSource</c> e completat
    ''' doar pe controlul care a pornit tragerea, iar arborele-țintă e altul și l-ar vedea gol.
    ''' Obiectul de date e singurul lucru pe care îl văd amândoi, și el răspunde la fel și când
    ''' sursa și ținta sunt același arbore.</para>
    '''
    ''' <para>Tragerea nu iese din proces, deci nodul călătorește ca referință și nu are nevoie
    ''' să fie serializabil.</para>
    ''' </summary>
    Private Shared Function ItemDinDate(date_ As IDataObject) As TreeItem
        If date_ Is Nothing Then Return Nothing
        If Not date_.GetDataPresent(GetType(TreeItem)) Then Return Nothing
        Return TryCast(date_.GetData(GetType(TreeItem)), TreeItem)
    End Function

    ''' <summary>
    ''' TOATE rândurile care se trag: grupul, dacă apăsarea a pornit dintr-unul, altfel rândul
    ''' din obiectul de date, singur.
    '''
    ''' <para>Grupul se ia în seamă numai dacă îl CONȚINE pe cel apăsat. Altfel o selecție
    ''' rămasă de la un gest de dinainte s-ar lipi de o tragere care n-are nimic cu ea.</para>
    ''' </summary>
    Private Shared Function ItemsDinDate(date_ As IDataObject) As List(Of TreeItem)
        Dim rezultat As New List(Of TreeItem)()
        Dim apasat As TreeItem = ItemDinDate(date_)
        If apasat Is Nothing Then Return rezultat

        Dim grup As List(Of TreeItem) = _draggedGroup
        If grup IsNot Nothing AndAlso grup.Count > 0 Then
            For Each rand As TreeItem In grup
                If rand Is apasat Then
                    rezultat.AddRange(grup)
                    Return rezultat
                End If
            Next
        End If

        rezultat.Add(apasat)
        Return rezultat
    End Function

    ''' <summary>Este rândul acesta printre cele care se trag?</summary>
    Private Shared Function EstePrintre(lista As List(Of TreeItem), it As TreeItem) As Boolean
        If lista Is Nothing OrElse it Is Nothing Then Return False
        For Each rand As TreeItem In lista
            If rand Is it Then Return True
        Next
        Return False
    End Function

    Protected Overrides Sub OnDragOver(drgevent As DragEventArgs)
        Try
            MyBase.OnDragOver(drgevent)
            drgevent.Effect = DragDropEffects.None
            If Not _dragEnabled Then Return

            Dim surse As List(Of TreeItem) = ItemsDinDate(drgevent.Data)
            If surse.Count = 0 Then Return
            Dim sursa As TreeItem = surse(0)

            Dim p As Point = Me.PointToClient(New Point(drgevent.X, drgevent.Y))
            ' Cursorul poate sta chiar pe un rând-fantomă pus de previzualizare. Fantoma nu e un
            ' nod al datelor, dar locul ei înseamnă «rădăcina asta» — vezi partiala .DropPreview.
            Dim tinta As TreeItem = RandReal(HitTestItem(p))

            ' Un nod nu se poate arunca pe el însuși — nici pe vreunul din grupul care se trage.
            If EstePrintre(surse, tinta) Then tinta = Nothing

            Dim permis As Boolean = False
            Dim motiv As String = String.Empty
            If tinta IsNot Nothing Then
                Dim args As New TreeDragOverEventArgs(sursa, tinta)
                args.SetSources(surse)
                RaiseEvent NodeDragOver(Me, args)
                permis = args.Allow
                motiv = If(args.Motiv, String.Empty)
            End If

            If tinta IsNot _dropTarget OrElse permis <> _dropAllowed Then
                _dropTarget = tinta
                _dropAllowed = permis
                _dropMotiv = motiv
                Me.Invalidate()
            Else
                _dropMotiv = motiv
            End If

            drgevent.Effect = If(permis, DragDropEffects.Move, DragDropEffects.None)
            ArataMotivulRefuzului(tinta, permis)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnDragOver", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDragLeave(e As EventArgs)
        Try
            MyBase.OnDragLeave(e)
            ' Cursorul a plecat din arbore: previzualizarea vorbea despre o aruncare care nu se
            ' mai întâmplă aici.
            ClearDropPreview()
            If _dropTarget IsNot Nothing Then
                _dropTarget = Nothing
                _dropAllowed = False
                Me.Invalidate()
            End If
            _dropTipTinta = Nothing
            _butonTooltip?.HideNow()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnDragLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDragDrop(drgevent As DragEventArgs)
        Try
            MyBase.OnDragDrop(drgevent)
            ' Din obiectul de date, nu din câmp: la o tragere între doi arbori, ținta n-a
            ' pornit tragerea și n-are ce să aibă în `_dragSource`.
            Dim surse As List(Of TreeItem) = ItemsDinDate(drgevent.Data)
            Dim tinta As TreeItem = _dropTarget
            Dim permis As Boolean = _dropAllowed
            CancelDrag()

            If Not permis OrElse surse.Count = 0 OrElse tinta Is Nothing Then Return
            Dim args As New TreeDropEventArgs(surse(0), tinta)
            args.SetSources(surse)
            RaiseEvent NodeDropped(Me, args)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnDragDrop", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Eticheta cu motivul refuzului
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Arată, lângă cursor, DE CE nu se poate arunca aici. Numai la refuz și numai o dată per
    ''' rând: fără paza pe <c>_dropTipTinta</c>, fiecare pixel de mișcare ar reprograma apariția
    ''' și eticheta n-ar apuca să apară niciodată.
    ''' </summary>
    Private Sub ArataMotivulRefuzului(tinta As TreeItem, permis As Boolean)
        If permis OrElse tinta Is Nothing OrElse String.IsNullOrWhiteSpace(_dropMotiv) Then
            If _dropTipTinta IsNot Nothing Then
                _dropTipTinta = Nothing
                _butonTooltip?.HideNow()
            End If
            Return
        End If
        If tinta Is _dropTipTinta Then Return

        _dropTipTinta = tinta
        _dropTipContinut.HeaderText = "Nu se poate aici"
        _dropTipContinut.Text = _dropMotiv
        _dropTipContinut.FooterText = Nothing

        Dim y As Integer = GetItemY(tinta)
        If y < 0 Then Return
        Dim pozitie As Point = Me.PointToScreen(New Point(SX(24), y + _itemHeight))
        ButtonTooltip.ShowAt(Me, _dropTipContinut, pozitie)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Desenul — chemat din OnPaint, după antet/subsol
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Chenarul de pe rândul-țintă: accentul când primește, culoarea de eroare când refuză.
    '''
    ''' <para>Se uită la <c>_dropTarget</c>, NU la <c>_dragSource</c>: la o tragere între doi
    ''' arbori, cel care desenează ținta e arborele care nu a pornit tragerea.</para>
    '''
    ''' <para>Se desenează și la refuz, deliberat. Un rând care nu răspunde deloc s-ar citi ca
    ''' «arborele nu m-a văzut»; unul cu chenar roșu spune «te-am văzut, și nu aici» — iar
    ''' eticheta de alături spune de ce.</para>
    ''' </summary>
    Private Sub DrawDropTarget(g As Graphics)
        If _dropTarget Is Nothing Then Return

        Dim y As Integer = GetItemY(_dropTarget)
        If y < 0 Then Return

        Dim headerOff As Integer = TotalHeaderOffset
        Dim zonaNoduri As Integer = Math.Max(0, Me.Height - headerOff - FooterOffset)
        If zonaNoduri <= 0 Then Return

        Dim oldClip = g.Clip.Clone()
        Try
            g.SetClip(New Rectangle(0, headerOff, Me.Width, zonaNoduri))

            Dim latime As Integer = Math.Max(1, Me.Width - If(_vScroll.Visible, _vScroll.Width, 0) - SX(2))
            Dim r As New Rectangle(SX(1), y, latime - SX(2), _itemHeight - 1)
            Dim culoare As Color = If(_dropAllowed, DragHighlightColor, DragForbiddenColor)

            ' Un văl subțire sub chenar: la 150% un chenar de un pixel se pierde pe un rând înalt.
            Using umplere As New SolidBrush(Color.FromArgb(40, culoare))
                g.FillRectangle(umplere, r)
            End Using
            Using pen As New Pen(culoare, CSng(SY(2)))
                pen.Alignment = PenAlignment.Inset
                g.DrawRectangle(pen, r)
            End Using
        Finally
            g.Clip = oldClip
        End Try
    End Sub
End Class

''' <summary>Argumentele lui <see cref="AdvancedTreeControl.NodeDragStarting"/>.</summary>
Public NotInheritable Class TreeDragStartEventArgs
    Inherits EventArgs

    Public Sub New(item As AdvancedTreeControl.TreeItem)
        Me.Item = item
        _items = New List(Of AdvancedTreeControl.TreeItem)()
        If item IsNot Nothing Then _items.Add(item)
    End Sub

    ''' <summary>Nodul pe care operatorul a început să-l tragă.</summary>
    Public ReadOnly Property Item As AdvancedTreeControl.TreeItem

    Private ReadOnly _items As List(Of AdvancedTreeControl.TreeItem)

    ''' <summary>
    ''' TOATE rândurile care pleacă în tragere — grupul selectat, dacă apăsarea a pornit
    ''' dintr-unul, altfel doar <see cref="Item"/>.
    '''
    ''' <para>Lista se poate MODIFICA: gazda scoate din ea rândurile pe care nu le lasă să
    ''' plece (unul înghețat printre cele bune) în loc să refuze tot gestul. Golită de tot,
    ''' tragerea nu mai pornește.</para>
    ''' </summary>
    Public ReadOnly Property Items As IList(Of AdvancedTreeControl.TreeItem)
        Get
            Return _items
        End Get
    End Property

    ''' <summary>Umplut de control înainte de a ridica evenimentul.</summary>
    Friend Sub SetItems(items As IEnumerable(Of AdvancedTreeControl.TreeItem))
        _items.Clear()
        If items Is Nothing Then Return
        For Each it As AdvancedTreeControl.TreeItem In items
            If it IsNot Nothing Then _items.Add(it)
        Next
    End Sub

    ''' <summary>Pune-l pe True ca nodul să NU poată fi tras.</summary>
    Public Property Cancel As Boolean
End Class

''' <summary>Argumentele lui <see cref="AdvancedTreeControl.NodeDragOver"/>.</summary>
Public NotInheritable Class TreeDragOverEventArgs
    Inherits EventArgs

    Public Sub New(source As AdvancedTreeControl.TreeItem, target As AdvancedTreeControl.TreeItem)
        Me.Source = source
        Me.Target = target
    End Sub

    ''' <summary>Nodul tras — cel APĂSAT, când se trag mai multe.</summary>
    Public ReadOnly Property Source As AdvancedTreeControl.TreeItem

    Private _sources As IReadOnlyList(Of AdvancedTreeControl.TreeItem) = Nothing

    ''' <summary>
    ''' TOATE rândurile care se trag. Când e unul singur, e o listă cu <see cref="Source"/> în ea
    ''' — deci gazda poate scrie o singură dată bucla și acoperă amândouă cazurile.
    ''' </summary>
    Public ReadOnly Property Sources As IReadOnlyList(Of AdvancedTreeControl.TreeItem)
        Get
            If _sources IsNot Nothing Then Return _sources
            If Source Is Nothing Then Return Array.Empty(Of AdvancedTreeControl.TreeItem)()
            Return New AdvancedTreeControl.TreeItem() {Source}
        End Get
    End Property

    ''' <summary>Umplut de control înainte de a ridica evenimentul.</summary>
    Friend Sub SetSources(items As IReadOnlyList(Of AdvancedTreeControl.TreeItem))
        _sources = items
    End Sub

    ''' <summary>Nodul de sub cursor.</summary>
    Public ReadOnly Property Target As AdvancedTreeControl.TreeItem

    ''' <summary>
    ''' Implicit False: <b>refuzul e implicitul</b>. O gazdă care uită să răspundă nu lasă să
    ''' treacă nimic, în loc să lase să treacă tot.
    ''' </summary>
    Public Property Allow As Boolean

    ''' <summary>
    ''' De ce nu se poate — în română, gata de arătat operatorului. Ignorat când
    ''' <see cref="Allow"/> e True.
    ''' </summary>
    Public Property Motiv As String = String.Empty
End Class

''' <summary>Argumentele lui <see cref="AdvancedTreeControl.NodeDropped"/>.</summary>
Public NotInheritable Class TreeDropEventArgs
    Inherits EventArgs

    Public Sub New(source As AdvancedTreeControl.TreeItem, target As AdvancedTreeControl.TreeItem)
        Me.Source = source
        Me.Target = target
    End Sub

    ''' <summary>Nodul tras — cel APĂSAT, când s-au tras mai multe.</summary>
    Public ReadOnly Property Source As AdvancedTreeControl.TreeItem

    Private _sources As IReadOnlyList(Of AdvancedTreeControl.TreeItem) = Nothing

    ''' <summary>
    ''' TOATE rândurile aruncate. Una singură când s-a tras unul singur — vezi
    ''' <see cref="TreeDragOverEventArgs.Sources"/>.
    ''' </summary>
    Public ReadOnly Property Sources As IReadOnlyList(Of AdvancedTreeControl.TreeItem)
        Get
            If _sources IsNot Nothing Then Return _sources
            If Source Is Nothing Then Return Array.Empty(Of AdvancedTreeControl.TreeItem)()
            Return New AdvancedTreeControl.TreeItem() {Source}
        End Get
    End Property

    ''' <summary>Umplut de control înainte de a ridica evenimentul.</summary>
    Friend Sub SetSources(items As IReadOnlyList(Of AdvancedTreeControl.TreeItem))
        _sources = items
    End Sub

    ''' <summary>Nodul pe care a fost aruncat.</summary>
    Public ReadOnly Property Target As AdvancedTreeControl.TreeItem
End Class
