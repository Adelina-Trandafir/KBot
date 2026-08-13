Option Strict On
Imports System.Collections
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Rândurile și coloanele FIXE ale unui <see cref="TableLayoutPanel"/>, ținute pe măsura schemei
''' active (slice 0030).
'''
''' <para><b>Problema, pe scurt.</b> Designerul din Visual Studio nu știe nimic despre motorul de
''' teme: tot ce se așază acolo se așază pe schema CLASSIC, adică pe controale de sistem, fără
''' umplutura și fără fontul unei scheme. Un rând de 40px e ales uitându-te la un buton de 32px cu
''' <c>Padding = 0</c>. Vine schema Modern, care cere aer în jurul textului
''' (<c>ControlPadding = 12,8,12,8</c>) și un alt font — <c>ModernRenderer</c> crește butonul ca
''' să-i încapă și umplutura, și textul (vezi <c>FitHeightToPaddingAndText</c>) — și butonul de 32
''' devine 48 înăuntrul unui rând care a rămas 40. Rezultatul e ce se vede pe ecran: controale
''' tăiate, înghesuite unul în altul, cu textul pe jumătate.</para>
'''
''' <para><b>Regula, într-o propoziție:</b> un rând fix crește cu EXACT surplusul pe care conținutul
''' lui îl cere peste măsura AUTORATĂ, și se întoarce la măsura autorată când surplusul dispare.
''' Nu «rândul devine cât conținutul» — atunci aerul ales de operator între controale s-ar pierde la
''' prima comutare de schemă; ci «rândul autorat, plus cât nu încape».</para>
'''
''' <para><b>De ce baza e instantaneul autorat și nu mărimea de acum.</b> Fără
''' <see cref="Capture"/>, a doua comutare de schemă ar măsura peste rezultatul primeia și rândurile
''' ar crește la fiecare trecere prin Modern, fără să se mai întoarcă niciodată. E același tipar ca
''' înălțimea autorată a unui buton din <c>ModernRenderer</c> și ca lățimea autorată a unei coloane
''' de grilă: schema are voie să CREASCĂ o măsură cât să încapă conținutul, n-are voie s-o
''' REscrie.</para>
'''
''' <para><b>Ce NU atinge:</b> rândurile <c>Percent</c> și <c>AutoSize</c> — ele urmăresc deja
''' conținutul sau spațiul rămas, deci n-au nimic de aflat de aici; și orice control care se întinde
''' peste mai multe rânduri (<c>RowSpan &gt; 1</c>), fiindcă surplusul lui nu se poate pune în
''' seama unui rând anume.</para>
'''
''' <para>Apelul e idempotent și ieftin: <see cref="Capture"/> reține o singură dată,
''' <see cref="Fit"/> scrie doar când măsura chiar se schimbă (altfel ar cere un layout degeaba la
''' fiecare comutare).</para>
''' </summary>
Public Module ThemeTableFit

    ' Instantaneul măsurilor AUTORATE, per tabel. ConditionalWeakTable: nu ține tabelul în viață și
    ' nu cere nicio golire la închiderea formularului — exact ca registrul de butoane din
    ' ModernRenderer.
    Private NotInheritable Class TableBaseline
        Public Rows As Single()
        Public Columns As Single()
        ''' <summary>Rândurile strânse la zero de apelant (vezi <see cref="SetRowCollapsed"/>).</summary>
        Public ReadOnly Stranse As New HashSet(Of Integer)()
    End Class

    Private ReadOnly _tables As New ConditionalWeakTable(Of TableLayoutPanel, TableBaseline)()

    ''' <summary>
    ''' Reține măsurile autorate ale rândurilor și coloanelor. Se cheamă IMEDIAT după
    ''' <c>InitializeComponent</c>, înainte ca vreo schemă să apuce să scrie ceva — după, valoarea
    ''' autorată nu mai există. Idempotent: doar prima trecere reține.
    ''' </summary>
    Public Sub Capture(tlp As TableLayoutPanel)
        If tlp Is Nothing Then Return
        Dim b As TableBaseline = Nothing
        If _tables.TryGetValue(tlp, b) Then Return
        b = New TableBaseline With {
            .Rows = CitesteMasurile(tlp.RowStyles),
            .Columns = CitesteMasurile(tlp.ColumnStyles)}
        _tables.Add(tlp, b)
    End Sub

    ' Măsurile unui set de stiluri, ca tablou (Percent/AutoSize se rețin ca atare — Fit le sare).
    Private Function CitesteMasurile(styles As IList) As Single()
        Dim m(Math.Max(0, styles.Count - 1)) As Single
        For i As Integer = 0 To styles.Count - 1
            Dim rs As RowStyle = TryCast(styles(i), RowStyle)
            If rs IsNot Nothing Then
                m(i) = rs.Height
            Else
                m(i) = DirectCast(styles(i), ColumnStyle).Width
            End If
        Next
        Return m
    End Function

    ''' <summary>
    ''' Re-măsoară tabelul pe schema activă: fiecare rând/coloană <c>Absolute</c> primește măsura
    ''' AUTORATĂ plus surplusul cerut de cel mai lacom control din el. Fără instantaneu autorat
    ''' (nimeni n-a chemat <see cref="Capture"/>) nu face nimic — n-ar avea față de ce să măsoare.
    ''' </summary>
    Public Sub Fit(tlp As TableLayoutPanel)
        Try
            If tlp Is Nothing Then Return
            Dim b As TableBaseline = Nothing
            If Not _tables.TryGetValue(tlp, b) Then Return

            Dim nrR As Integer = tlp.RowStyles.Count
            Dim nrC As Integer = tlp.ColumnStyles.Count
            Dim surplusRanduri(Math.Max(0, nrR - 1)) As Single
            Dim surplusColoane(Math.Max(0, nrC - 1)) As Single

            ' NU se citește «c.Visible» nicăieri aici, și nu din lene: getter-ul răspunde despre
            ' LANȚUL DE PĂRINȚI, deci pe un formular nearătat (bancul de probă, orice test headless)
            ' TOT ce e în tabel raportează False și măsurarea ar ieși goală. Un rând care chiar
            ' trebuie să dispară se strânge explicit, prin SetRowCollapsed.
            For Each c As Control In tlp.Controls
                If c Is Nothing Then Continue For
                Dim cerut As Size = CatCere(c)

                ' Un control întins peste mai multe rânduri nu-și poate pune surplusul în seama
                ' unui rând anume — se sare, cu totul, pe axa aceea.
                If tlp.GetRowSpan(c) = 1 Then
                    Dim r As Integer = tlp.GetRow(c)
                    If EAbsolut(tlp.RowStyles, r, b.Rows) Then
                        surplusRanduri(r) = Math.Max(surplusRanduri(r), cerut.Height - b.Rows(r))
                    End If
                End If

                If tlp.GetColumnSpan(c) = 1 Then
                    Dim col As Integer = tlp.GetColumn(c)
                    If EAbsolut(tlp.ColumnStyles, col, b.Columns) Then
                        surplusColoane(col) = Math.Max(surplusColoane(col), cerut.Width - b.Columns(col))
                    End If
                End If
            Next

            For r As Integer = 0 To nrR - 1
                If Not EAbsolut(tlp.RowStyles, r, b.Rows) Then Continue For
                Dim vrea As Single = If(b.Stranse.Contains(r), 0.0F,
                                        b.Rows(r) + Math.Max(0.0F, surplusRanduri(r)))
                If tlp.RowStyles(r).Height <> vrea Then tlp.RowStyles(r).Height = vrea
            Next

            For col As Integer = 0 To nrC - 1
                If Not EAbsolut(tlp.ColumnStyles, col, b.Columns) Then Continue For
                Dim vrea As Single = b.Columns(col) + Math.Max(0.0F, surplusColoane(col))
                If tlp.ColumnStyles(col).Width <> vrea Then tlp.ColumnStyles(col).Width = vrea
            Next
        Catch ex As Exception
            ' Boundary de temă (se cheamă din OnThemeChanged): loghează + ÎNGHITE. O măsurare
            ' căzută lasă tabelul pe măsurile de dinainte, adică pe cele autorate — urât, nu rupt.
            GlobalErrorLog.Write("ThemeTableFit.Fit", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Strânge la zero (sau redeschide) un rând fix — și o ține așa peste orice re-măsurare.
    '''
    ''' <para>Într-un tabel cu rânduri fixe, un control pus pe <c>Visible = False</c> lasă în urmă
    ''' o gaură cât rândul: butonul dispare, banda goală rămâne. Aici se spune cealaltă jumătate,
    ''' explicit — și explicit fiindcă <c>Control.Visible</c> nu poate fi întrebat de nimeni:
    ''' getter-ul răspunde despre lanțul de părinți, deci pe un formular încă nearătat răspunde
    ''' False pentru tot ce e în el.</para>
    ''' </summary>
    Public Sub SetRowCollapsed(tlp As TableLayoutPanel, rowIndex As Integer, collapsed As Boolean)
        Dim b As TableBaseline = Nothing
        If tlp Is Nothing OrElse Not _tables.TryGetValue(tlp, b) Then Return
        If collapsed Then b.Stranse.Add(rowIndex) Else b.Stranse.Remove(rowIndex)
    End Sub

    ' Stilul de la indexul dat e Absolute (și avem instantaneu pentru el)?
    Private Function EAbsolut(styles As IList, index As Integer, baseline As Single()) As Boolean
        If index < 0 OrElse index >= styles.Count OrElse index >= baseline.Length Then Return False
        Dim rs As RowStyle = TryCast(styles(index), RowStyle)
        If rs IsNot Nothing Then Return rs.SizeType = SizeType.Absolute
        Return DirectCast(styles(index), ColumnStyle).SizeType = SizeType.Absolute
    End Function

    ''' <summary>
    ''' Cât loc cere un control ACUM, cu tot cu marginile lui.
    '''
    ''' <para>Pe axa pe care controlul e ÎNTINS de andocare mărimea lui e a celulei, nu a lui:
    ''' acolo se întoarce 0, adică «nu cer nimic în plus». Altfel s-ar măsura chiar rezultatul
    ''' rândului și fiecare trecere ar crește rândul cu propria lui înălțime.</para>
    '''
    ''' <para>Pe axele rămase întrebarea se pune lui <c>GetPreferredSize</c>, NU lui
    ''' <c>Height</c>/<c>Width</c>, și asta e miezul lucrului. <c>ModernRenderer</c> chiar crește
    ''' butonul ca să-i încapă umplutura schemei — dar înăuntrul unui <c>TableLayoutPanel</c>
    ''' creșterea aia nu se vede niciodată: motorul de așezare al tabelului taie orice control
    ''' andocat la dreptunghiul celulei, la primul layout de după. Un buton de 56px într-o celulă
    ''' de 40 raportează 40, deci o măsurare care s-ar uita la mărimea lui ar afla mereu că totul
    ''' încape perfect — și rândul n-ar crește nici o dată. Mărimea PREFERATĂ, în schimb, se
    ''' calculează din text + umplutură + chenare, adică din chiar lucrurile pe care le schimbă
    ''' schema, și nu are cum să fie tăiată de nimeni.</para>
    ''' </summary>
    Private Function CatCere(c As Control) As Size
        Dim inaltDinCelula As Boolean = c.Dock = DockStyle.Fill OrElse
                                        c.Dock = DockStyle.Left OrElse c.Dock = DockStyle.Right
        Dim latDinCelula As Boolean = c.Dock = DockStyle.Fill OrElse
                                      c.Dock = DockStyle.Top OrElse c.Dock = DockStyle.Bottom

        Dim preferat As Size = c.GetPreferredSize(Size.Empty)
        Dim h As Integer = If(inaltDinCelula, 0, preferat.Height + c.Margin.Vertical)
        Dim w As Integer = If(latDinCelula, 0, preferat.Width + c.Margin.Horizontal)
        Return New Size(w, h)
    End Function

    ''' <summary>
    ''' Măsura autorată a unui rând, sau <c>-1</c> dacă tabelul n-a fost fotografiat. Poartă de
    ''' verificare headless: creșterea și întoarcerea la loc nu se pot proba altfel decât comparând
    ''' cu baza, iar baza e privată.
    ''' </summary>
    Public Function DebugAuthoredRow(tlp As TableLayoutPanel, index As Integer) As Single
        Dim b As TableBaseline = Nothing
        If tlp Is Nothing OrElse Not _tables.TryGetValue(tlp, b) Then Return -1.0F
        If index < 0 OrElse index >= b.Rows.Length Then Return -1.0F
        Return b.Rows(index)
    End Function

End Module
