Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' PICTOGRAMELE DE ANTET ale <see cref="KBotDataView"/> (slice 0028-02): fiecare coloană poate
''' purta o pictogramă la stânga titlului și una la dreapta lui — sora, pe coloană, a perechii
''' de capăt din antetul/subsolul lui <c>AdvancedTreeControl</c>. Cea din DREAPTA e cea care se
''' apasă (<see cref="KBotDataView.HeaderRightIconClicked"/>: filtru, sortare, meniu de coloană);
''' cea din stânga e un semn, nu un buton.
'''
''' <para><b>Ordinea sacrificiului, când coloana se îngustează.</b> Titlul se taie ÎNTÂI (elipsă,
''' până la lățime zero), apoi cade pictograma din STÂNGA. Cea din dreapta nu se sacrifică
''' niciodată: e singura care poate fi apăsată, iar un buton care dispare exact când coloana e
''' îngustă e un buton care lipsește tocmai când ai nevoie de el.</para>
'''
''' <para><b>De aceea coloana are o podea de lățime.</b> <see cref="KBotDataColumn.EffectiveMinWidth"/>
''' nu coboară sub cât cer pictogramele + spațiile dintre ele, deci nicio trecere de
''' auto-dimensionare, de umplere sau de strâmtare nu poate ajunge să le suprapună. Podeaua bate
''' inclusiv <c>MaxWidth</c> — un plafon mai mic decât pictogramele ar fi o cerere imposibilă.</para>
'''
''' <para><b>DPI.</b> Mărimile pictogramelor sunt în pixeli, exact ca la arbore: ce se vede în
''' designer e ce se desenează. Spațiile (<see cref="KBotDataColumn.HeaderIconPad"/> /
''' <see cref="KBotDataColumn.HeaderIconGap"/>) se scalează cu DPI-ul, deci pe un ecran la 150%
''' podeaua rămâne cu câțiva pixeli sub necesar — se plătește din text, care oricum se taie primul,
''' niciodată din pictograme.</para>
''' </summary>
Partial Class KBotDataView

    ' Coloana a cărei pictogramă din dreapta e sub cursor (Nothing = niciuna). Se ține pe CHEIE,
    ' nu pe referință: o coloană poate fi înlocuită în colecție între două mișcări de mouse.
    Private _hotHeaderIconKey As String = Nothing

    ''' <summary>
    ''' Pictograma din dreapta unui antet de coloană a fost apăsată. Argumentele poartă cheia
    ''' coloanei ȘI dreptunghiul pictogramei, ca gazda să-și poată așeza meniul sub ea.
    ''' </summary>
    Public Event HeaderRightIconClicked As EventHandler(Of KBotColumnEventArgs)

    ''' <summary>Așezarea pieselor dintr-o celulă de antet (pictograme + titlu).</summary>
    Friend Structure HeaderCellLayout
        ''' <summary>Pictograma din stânga; goală = nu există sau n-a mai încăput.</summary>
        Public LeftIcon As Rectangle
        ''' <summary>Pictograma din dreapta; goală = nu există.</summary>
        Public RightIcon As Rectangle
        ''' <summary>Pictograma de FILTRARE, la capătul din dreapta; goală = nu se arată.</summary>
        Public FilterIcon As Rectangle
        ''' <summary>Ce a rămas pentru titlu; lățime 0 = titlul nu se mai scrie deloc.</summary>
        Public Text As Rectangle
    End Structure

    ''' <summary>
    ''' Așezarea pieselor dintr-o celulă de antet. Funcție PURĂ: o folosesc pictarea, hit-testul
    ''' și testele — o a doua formulă ar însemna o pictogramă care se desenează unde nu se apasă.
    '''
    ''' <para><paramref name="filterSize"/> (slice 0028-03) e mărimea pictogramei de filtrare, goală
    ''' dacă nu se arată pe coloana asta. Ea stă la CAPĂTUL din dreapta, iar
    ''' <see cref="KBotDataColumn.HeaderRightIcon"/> se mută la stânga ei: filtrul e o funcție a
    ''' grilei, aceeași pe toate coloanele, deci trebuie să cadă mereu în același loc — o
    ''' pictogramă care își schimbă poziția de la o coloană la alta e o pictogramă pe care o cauți
    ''' de fiecare dată.</para>
    '''
    ''' <para><paramref name="textPad"/> e retragerea TITLULUI de la marginea celulei, și e mai mică
    ''' decât cea a pictogramelor (<paramref name="pad"/>). Erau una singură, iar antetul stătea
    ''' retras cu 8px de fiecare parte — mai mult decât celulele din corp (6px), și, mai rău,
    ''' 16px furați din lățimea la care se rupe un titlu pe mai multe linii. Retragerea mică se
    ''' aplică doar pe latura unde NU s-a așezat nicio pictogramă; acolo unde e una, titlul se
    ''' oprește oricum la spațiul dinaintea ei. <c>-1</c> = aceeași cu cea a pictogramelor.</para>
    ''' </summary>
    Friend Shared Function ComputeHeaderCellLayout(col As KBotDataColumn, cellRect As Rectangle,
                                                   pad As Integer, gap As Integer,
                                                   Optional filterSize As Size = Nothing,
                                                   Optional textPad As Integer = -1) As HeaderCellLayout
        Dim rez As New HeaderCellLayout()
        If col Is Nothing Then Return rez
        If textPad < 0 Then textPad = pad

        Dim stanga As Integer = cellRect.Left + pad
        Dim dreapta As Integer = cellRect.Right - pad

        ' 0) Pictograma de FILTRARE ia capătul din dreapta, înaintea tuturor: ca și cea din
        '    dreapta, se apasă, deci nu se sacrifică niciodată.
        If filterSize.Width > 0 AndAlso filterSize.Height > 0 Then
            rez.FilterIcon = New Rectangle(dreapta - filterSize.Width,
                                           cellRect.Top + (cellRect.Height - filterSize.Height) \ 2,
                                           filterSize.Width, filterSize.Height)
            dreapta = rez.FilterIcon.Left - gap
        End If

        ' 1) Pictograma din DREAPTA se așază apoi — nici ea nu se sacrifică.
        If col.HeaderRightIcon IsNot Nothing Then
            Dim s As Size = col.HeaderRightIconSize
            rez.RightIcon = New Rectangle(dreapta - s.Width,
                                          cellRect.Top + (cellRect.Height - s.Height) \ 2,
                                          s.Width, s.Height)
            dreapta = rez.RightIcon.Left - gap
        End If

        ' 2) Pictograma din STÂNGA: doar dacă mai încape ÎNTREAGĂ. Jumătate de pictogramă nu e
        '    o pictogramă mai mică, e una greșită.
        If col.HeaderLeftIcon IsNot Nothing Then
            Dim s As Size = col.HeaderLeftIconSize
            If dreapta - stanga >= s.Width Then
                rez.LeftIcon = New Rectangle(stanga,
                                             cellRect.Top + (cellRect.Height - s.Height) \ 2,
                                             s.Width, s.Height)
                stanga = rez.LeftIcon.Right + gap
            End If
        End If

        ' 3) Titlul ia ce a rămas — poate fi zero, fiindcă el se taie primul. Pe laturile unde nu
        '    s-a așezat nicio pictogramă își ia PROPRIA retragere, cea mică: acolo nu are de ce să
        '    stea la distanță de pictograme inexistente.
        Dim textStanga As Integer = If(rez.LeftIcon.IsEmpty, cellRect.Left + textPad, stanga)
        Dim textDreapta As Integer = If(rez.RightIcon.IsEmpty AndAlso rez.FilterIcon.IsEmpty,
                                        cellRect.Right - textPad, dreapta)
        rez.Text = New Rectangle(textStanga, cellRect.Top,
                                 Math.Max(0, textDreapta - textStanga), cellRect.Height)
        Return rez
    End Function

    ' Așezarea unei celule de antet cu spațiile scalate la DPI-ul controlului.
    Private Function HeaderLayoutFor(col As KBotDataColumn, cellRect As Rectangle) As HeaderCellLayout
        Return ComputeHeaderCellLayout(col, cellRect,
                                       ScaleDpi(KBotDataColumn.HeaderIconPad),
                                       ScaleDpi(KBotDataColumn.HeaderIconGap),
                                       FilterIconSizeFor(col),
                                       ScaleDpi(KBotDataColumn.HeaderTextPad))
    End Function

    ''' <summary>
    ''' Cât spațiu cer pictogramele unei coloane, dincolo de titlu — îl adună trecerea de
    ''' auto-dimensionare, ca titlul să nu se taie tocmai după o măsurare „la conținut”.
    ''' </summary>
    Private Function HeaderIconsExtent(col As KBotDataColumn) As Integer
        Dim gap As Integer = ScaleDpi(KBotDataColumn.HeaderIconGap)
        Dim total As Integer = 0
        If col.HeaderLeftIcon IsNot Nothing Then total += col.HeaderLeftIconSize.Width + gap
        If col.HeaderRightIcon IsNot Nothing Then total += col.HeaderRightIconSize.Width + gap
        Dim filtru As Size = FilterIconSizeFor(col)
        If filtru.Width > 0 Then total += filtru.Width + gap
        Return total
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTARE (acoperită tranzitiv de Try-ul din OnPaint)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Desenează pictogramele unei celule de antet și întoarce dreptunghiul rămas titlului.
    ''' Fundalul de hover se desenează SUB pictogramă, doar pentru cea din dreapta: e singura
    ''' care răspunde la apăsare, iar o evidențiere sub ceva inert ar minți.
    ''' </summary>
    Private Function DrawHeaderIcons(g As Graphics, col As KBotDataColumn, cellRect As Rectangle) As Rectangle
        Dim asezare As HeaderCellLayout = HeaderLayoutFor(col, cellRect)

        ' Pictograma de filtrare (slice 0028-03) — vezi partiala .FilterIcon.
        DrawColumnFilterIcon(g, col, asezare.FilterIcon)

        If Not asezare.RightIcon.IsEmpty Then
            If IsHeaderRightIconHot(col) Then
                Using b As New SolidBrush(HeaderIconHoverResolved(col))
                    Using path As GraphicsPath = RoundedRect(Rectangle.Inflate(asezare.RightIcon, 3, 3), ScaleDpi(3))
                        g.FillPath(b, path)
                    End Using
                End Using
            End If
            g.DrawImage(col.HeaderRightIcon, asezare.RightIcon)
        End If

        If Not asezare.LeftIcon.IsEmpty Then g.DrawImage(col.HeaderLeftIcon, asezare.LeftIcon)

        Return asezare.Text
    End Function

    ' Coloana e cea survolată? Comparație pe cheie (vezi _hotHeaderIconKey).
    Private Function IsHeaderRightIconHot(col As KBotDataColumn) As Boolean
        If _hotHeaderIconKey Is Nothing OrElse col Is Nothing Then Return False
        Return String.Equals(_hotHeaderIconKey, col.Key, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Culoarea de hover a pictogramei: cea fixată pe coloană, altfel o spălare din culoarea de
    ''' text a antetului — adică din TEMĂ, ca peste tot (<c>Color.Empty</c> = „automat”).
    ''' </summary>
    Friend Function HeaderIconHoverResolved(col As KBotDataColumn) As Color
        If col IsNot Nothing AndAlso col.HeaderRightIconHoverColor <> Color.Empty Then
            Return col.HeaderRightIconHoverColor
        End If
        Return Color.FromArgb(40, HeaderForeResolved())
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' HIT-TEST + HOVER (chemate din partiala .Input)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Coloana a cărei pictogramă din dreapta e sub punct (Nothing = niciuna), împreună cu
    ''' dreptunghiul ei. Banda înghețată se caută prima, fiindcă ea se pictează PESTE cea derulată.
    ''' </summary>
    Friend Function HeaderIconTarget(pt As Point, ByRef iconRect As Rectangle) As KBotDataColumn
        iconRect = Rectangle.Empty
        ' Înălțimea EFECTIVĂ, nu cea din designer: sub o coloană cu titlul pe mai multe linii banda
        ' e mai înaltă, iar pictogramele stau centrate în ea — căutate în vechea bandă, ele s-ar
        ' desena într-un loc și s-ar apăsa în altul.
        Dim bandH As Integer = HeaderBandHeight()
        If bandH <= 0 Then Return Nothing
        If pt.Y < 0 OrElse pt.Y >= bandH Then Return Nothing

        For Each cl In _frozenLayout
            Dim r As Rectangle = HeaderLayoutFor(cl.Column, New Rectangle(cl.X, 0, cl.Column.Width, bandH)).RightIcon
            If Not r.IsEmpty AndAlso r.Contains(pt) Then
                iconRect = r
                Return cl.Column
            End If
        Next

        ' Sub banda înghețată nu se mai caută: acolo coloanele derulate sunt acoperite.
        If pt.X < _frozenBandWidth OrElse pt.X >= ViewportWidth() Then Return Nothing

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            Dim r As Rectangle = HeaderLayoutFor(cl.Column,
                New Rectangle(_frozenBandWidth + cl.X - hOffset, 0, cl.Column.Width, bandH)).RightIcon
            If Not r.IsEmpty AndAlso r.Contains(pt) Then
                iconRect = r
                Return cl.Column
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Actualizează starea de hover a pictogramelor de antet. Întoarce True dacă punctul e chiar
    ''' peste una — atunci apelantul pune cursorul de mână și nu mai caută nimic altceva acolo.
    ''' </summary>
    Friend Function UpdateHeaderIconHover(pt As Point) As Boolean
        Dim r As Rectangle = Rectangle.Empty
        Dim col As KBotDataColumn = HeaderIconTarget(pt, r)
        Dim cheie As String = If(col Is Nothing, Nothing, col.Key)
        If Not String.Equals(cheie, _hotHeaderIconKey, StringComparison.Ordinal) Then
            _hotHeaderIconKey = cheie
            Invalidate()
        End If
        Return col IsNot Nothing
    End Function

    ''' <summary>Stinge hover-ul pictogramelor de antet (cursorul a plecat din control).</summary>
    Friend Sub ClearHeaderIconHover()
        If _hotHeaderIconKey Is Nothing Then Return
        _hotHeaderIconKey = Nothing
        Invalidate()
    End Sub

    ''' <summary>
    ''' Apăsare peste o pictogramă de antet. Întoarce True dacă a fost consumată — atunci grila
    ''' nu mai pornește o redimensionare și nu mai mută selecția.
    ''' </summary>
    Friend Function HandleHeaderIconMouseDown(pt As Point) As Boolean
        Dim r As Rectangle = Rectangle.Empty
        Dim col As KBotDataColumn = HeaderIconTarget(pt, r)
        If col Is Nothing Then Return False
        If Not KBotDesignTime.IsDesignTime(Me) Then
            RaiseEvent HeaderRightIconClicked(Me, New KBotColumnEventArgs(col.Key, r))
        End If
        Return True
    End Function

    ''' <summary>
    ''' Chemată de <see cref="KBotDataColumn"/> când i s-au schimbat pictogramele: lățimile au o
    ''' podea nouă, deci se re-măsoară totul. Boundary: loghează + înghite — o schimbare de
    ''' pictogramă n-are voie să arunce dintr-un setter de designer.
    ''' </summary>
    Friend Sub OnColumnIconsChanged()
        Try
            If _initializing Then Return
            LayoutChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnColumnIconsChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Dreptunghiul pictogramei din dreapta unei coloane, în coordonate client (gol = nu se vede).
    ''' Friend: poarta de verificare headless a testelor — hit-testul nu se poate proba cu mouse-ul.
    ''' </summary>
    Friend Function DebugHeaderRightIconRect(colKey As String) As Rectangle
        RecalcColumnLayout()
        Dim bandH As Integer = HeaderBandHeight()
        For Each cl In _frozenLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return HeaderLayoutFor(cl.Column, New Rectangle(cl.X, 0, cl.Column.Width, bandH)).RightIcon
            End If
        Next
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If String.Equals(cl.Column.Key, colKey, StringComparison.Ordinal) Then
                Return HeaderLayoutFor(cl.Column,
                    New Rectangle(_frozenBandWidth + cl.X - hOffset, 0, cl.Column.Width, bandH)).RightIcon
            End If
        Next
        Return Rectangle.Empty
    End Function

End Class
