Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' SELECTORUL DE TEMĂ al barei de titlu: un al doilea buton cu pictogramă, așezat imediat la
''' stânga cutiei de control (minimizare/maximizare, iar în lipsa lor la stânga închiderii), care
''' desfășoară meniul de scheme.
'''
''' <para><b>De ce în bară și nu în gazdă.</b> Până acum meniul îl construia MainForm, pe
''' <c>OptionButtonClick</c>: gazda trebuia să știe de <c>ThemeManager</c>, de literele de acces, de
''' pictogramele schemelor, de <c>CustomPopup.ClosedJustNow</c> și de editorul de stiluri — vreo
''' sută de rânduri pe care al doilea formular cu bară de titlu ar fi trebuit să le COPIEZE. Aici
''' sunt scrise o dată: o gazdă aprinde <see cref="KBotCaptionBar.ShowThemeButton"/> și atât.</para>
'''
''' <para><b>Gazda nu trebuie să facă nimic după alegere.</b> <c>ThemeManager.SetScheme</c> difuzează
''' schema peste toate formularele deschise, deci fiecare se re-tematizează singur. Evenimentul
''' <see cref="KBotCaptionBar.ThemeSchemeChanged"/> există pentru ce e ÎN PLUS față de asta (o
''' pictogramă care depinde de schemă, un desen propriu), nu ca să reaplice cineva tema.</para>
''' </summary>
Partial Public NotInheritable Class KBotCaptionBar

    ''' <summary>
    ''' Cheia rândului «Stiluri...». NU e numele unei scheme și nici nu poate fi confundată cu unul:
    ''' o schemă de utilizator chiar s-ar putea numi «Stiluri», iar atunci alegerea ei ar deschide
    ''' editorul în loc să comute tema.
    ''' </summary>
    Private Const ThemeEditorKey As String = "@ThemeEditor"

    ''' <summary>
    ''' Cheia rândului «Opțiuni temă...» — fereastra care reglează SCHEMA (culori, stil, scalare),
    ''' spre deosebire de «Stiluri...», care pune excepții pe controale anume. Aceeași grijă ca la
    ''' cheia de mai sus: un «@» în față, ca să nu poată fi confundată cu numele unei scheme.
    ''' </summary>
    Private Const ThemeOptionsKey As String = "@ThemeOptions"

    ''' <summary>
    ''' Cheia rândului-CURSOR pentru mărimea textului. Aceeași grijă ca la celelalte două: un «@»
    ''' în față, ca să nu poată fi confundată cu numele unei scheme.
    ''' </summary>
    Private Const TextScaleKey As String = "@TextScale"

    ''' <summary>
    ''' Cheia rândului bifabil «Font din temă». Aceeași grijă ca la celelalte: un «@» în față, ca
    ''' să nu poată fi confundată cu numele unei scheme.
    ''' </summary>
    Private Const ThemeFontKey As String = "@ThemeFont"

    ' Bifa desenată pentru rândul de mai sus, ținută pe culoarea cu care a fost făcută. Se reface
    ' când se schimbă schema (deci culoarea textului) și NU se eliberează — o imagine pe care
    ' tocmai o desenează un meniu n-are voie să dispară sub el (aceeași alegere ca în FontBaseline).
    Private Shared _bifa As Bitmap
    Private Shared _bifaCuloare As Integer

    ''' <summary>
    ''' Bifa rândului «Font din temă», în culoarea textului din schema curentă — sau Nothing când
    ''' comutatorul e stins. Un rând fără pictogramă nu iese din coloană: jgheabul de pictograme e
    ''' al MENIULUI, iar un element fără imagine își lasă slotul gol (vezi CustomPopup.Painting).
    ''' </summary>
    Private Shared Function BifaPentru(pornit As Boolean) As Image
        If Not pornit Then Return Nothing
        Dim culoare As Color = ThemeManager.Current.Palette.TextColor
        If _bifa IsNot Nothing AndAlso _bifaCuloare = culoare.ToArgb() Then Return _bifa

        ' 16x16 e mărimea celorlalte pictograme ale meniului; popup-ul o scalează la ImageSize.
        Dim bmp As New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)
            Using pen As New Pen(culoare, 2.0F)
                pen.StartCap = Drawing2D.LineCap.Round
                pen.EndCap = Drawing2D.LineCap.Round
                pen.LineJoin = Drawing2D.LineJoin.Round
                g.DrawLines(pen, New Point() {New Point(3, 8), New Point(6, 12), New Point(13, 4)})
            End Using
        End Using
        _bifa = bmp
        _bifaCuloare = culoare.ToArgb()
        Return _bifa
    End Function

    ' Ridicat cât ține deschiderea meniului de temă, ca sinkul comun IPopupAnchor.SetPopupOpen să
    ' știe CARE buton s-a desfășurat. Vezi comentariul de acolo.
    Private _themeMenuOpening As Boolean = False

    ' ── Proprietăți ───────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Arată butonul de temă. UN SINGUR comutator: pictograma, meniul, literele de acces și
    ''' comutarea schemei vin cu el.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Arată butonul de temă (la stânga cutiei de control) și meniul lui de scheme. Implicit False.")>
    <DefaultValue(False)>
    Public Property ShowThemeButton As Boolean
        Get
            Return _showThemeButton
        End Get
        Set(value As Boolean)
            If value = _showThemeButton Then Return
            _showThemeButton = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Rândul «Opțiuni temă...» — fereastra care reglează schema (culori, stil, scalare) — se
    ''' poate stinge, cu comutatorul LUI. Deliberat separat de <see cref="ShowThemeEditor"/>:
    ''' cele două unelte fac lucruri diferite (una schimbă tema, cealaltă pune excepții pe
    ''' controale), deci o fereastră care o vrea pe una n-are de ce s-o capete și pe cealaltă.
    ''' </summary>
    ''' <summary>
    ''' Arată rândul-CURSOR pentru mărimea textului, în capul meniului.
    '''
    ''' <para>Stă SUS, deasupra schemelor, dintr-un motiv practic: e singurul rând care nu închide
    ''' meniul, deci e și singurul pe care operatorul îl folosește de mai multe ori la rând. Pus
    ''' jos, ar fi trebuit căutat de fiecare dată sub o listă care crește cu fiecare schemă
    ''' salvată.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Arată cursorul «Mărime text» în capul meniului de temă. Implicit True.")>
    <DefaultValue(True)>
    Public Property ShowTextScaleSlider As Boolean
        Get
            Return _showTextScaleSlider
        End Get
        Set(value As Boolean)
            _showTextScaleSlider = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Arată rândul «Opțiuni temă...» — culorile, stilul și scalarea schemei. Implicit True.")>
    <DefaultValue(True)>
    Public Property ShowThemeOptions As Boolean
        Get
            Return _showThemeOptions
        End Get
        Set(value As Boolean)
            _showThemeOptions = value
        End Set
    End Property

    ''' <summary>
    ''' Rândul «Stiluri...», editorul de excepții pe controale, se poate stinge: e o unealtă
    ''' de reglaj, nu o alegere de zi cu zi, deci nu are ce căuta pe fiecare fereastră.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Arată ultimul rând al meniului de temă — «Stiluri...», editorul de stiluri. Implicit True.")>
    <DefaultValue(True)>
    Public Property ShowThemeEditor As Boolean
        Get
            Return _showThemeEditor
        End Get
        Set(value As Boolean)
            _showThemeEditor = value
        End Set
    End Property

    ''' <summary>
    ''' Pictograma butonului. Lăsată goală = cea din resursele K-BOT (<c>switch_theme</c>), ca
    ''' butonul să arate cum trebuie fără nicio reglare. Vezi <see cref="EffectiveThemeButtonImage"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Pictograma butonului de temă. Goală = pictograma implicită din resursele K-BOT.")>
    Public Property ThemeButtonImage As Image
        Get
            ' Se întoarce alegerea OPERATORULUI, nu pictograma efectivă: altfel designerul ar
            ' îngheța implicitul în formular (regula ShouldSerialize din CLAUDE.md).
            Return _themeButtonImage
        End Get
        Set(value As Image)
            _themeButtonImage = value
            Invalidate()
        End Set
    End Property

    ' Private: TypeDescriptor le găsește după nume, inclusiv nepublice (vezi CustomPopupItem).
    Private Function ShouldSerializeThemeButtonImage() As Boolean
        Return _themeButtonImage IsNot Nothing
    End Function

    Private Sub ResetThemeButtonImage()
        ThemeButtonImage = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("Padding-ul pentru pictograma butonului de temă.")>
    <DefaultValue(2)>
    Public Property ThemeButtonPadding As Integer
        Get
            Return _themeButtonPadding
        End Get
        Set(value As Integer)
            If value = _themeButtonPadding Then Return
            _themeButtonPadding = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Ca la <see cref="TintOptionButtonImage"/>: pictograma se recolorează cu culoarea celorlalte
    ''' glife, deci urmează schema. Se stinge pentru o pictogramă cu adevărat colorată.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Recolorează pictograma butonului de temă cu culoarea glifelor (deci urmează tema). Stinge-l pentru o pictogramă colorată.")>
    <DefaultValue(True)>
    Public Property TintThemeButtonImage As Boolean
        Get
            Return _tintThemeButtonImage
        End Get
        Set(value As Boolean)
            If value = _tintThemeButtonImage Then Return
            _tintThemeButtonImage = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Butonul de temă e APRINS cât timp meniul lui e deschis. Stare pură de rulare — n-o pune
    ''' nimeni de mână, o ridică și o coboară <see cref="CustomPopup"/> prin <see cref="IPopupAnchor"/>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ThemeButtonActive As Boolean
        Get
            Return _themeButtonActive
        End Get
    End Property

    ''' <summary>
    ''' Dreptunghiul butonului de temă, în coordonatele CLIENT ale barei —
    ''' <see cref="Rectangle.Empty"/> când butonul e ascuns. Același contract ca la
    ''' <see cref="OptionButtonBounds"/>: desenul, hit-testul și oricine altcineva citesc ACEEAȘI
    ''' funcție, ca nimic să nu rămână în urmă când se stinge un buton vecin.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ThemeButtonBounds As Rectangle
        Get
            If Not _showThemeButton Then Return Rectangle.Empty
            Return ThemeButtonRect()
        End Get
    End Property

    ''' <summary>
    ''' S-a comutat schema din meniul butonului de temă. Ridicat DUPĂ ce
    ''' <c>ThemeManager.SetScheme</c> a aplicat și a difuzat schema — deci gazda nu are de reaplicat
    ''' nimic, doar de reglat ce ține de ea.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Ridicat după ce s-a comutat schema din meniul butonului de temă.")>
    Public Event ThemeSchemeChanged As EventHandler(Of ThemeSchemeChangedEventArgs)

    ' ── Meniul ────────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Desfășoară meniul de teme sub butonul de temă. Public fiindcă nu doar butonul îl poate
    ''' cere (o scurtătură de tastatură a gazdei, o comandă de meniu) — dar atunci butonul trebuie
    ''' să fie vizibil: fără el n-are de unde ieși meniul.
    '''
    ''' Meniul e un <see cref="CustomPopup"/>, nu un <c>ContextMenuStrip</c>, exact din motivul
    ''' pentru care controlul acela există: fața unui ContextMenuStrip o desenează
    ''' <c>ToolStripRenderer</c>, deci meniul CU CARE SE ALEGE TEMA ar rămâne o fâșie de sistem,
    ''' albă pe schemele întunecate.
    '''
    ''' Schema ACTIVĂ lipsește din listă: e deja aplicată, deci ar fi un rând care nu face nimic.
    ''' Numele sunt cele românești (<c>BuiltInSchemes.DisplayName</c>) — cheia rămâne numele
    ''' englezesc, cel cu care se persistă și se rezolvă înapoi.
    ''' </summary>
    Public Sub ShowThemeMenu()
        Try
            If Not _showThemeButton Then Return
            ' Al doilea clic pe buton ÎNCHIDE meniul: apăsarea l-a închis deja (a activat
            ' fereastra de dedesubt), deci fără garda asta l-am redeschide instantaneu.
            If CustomPopup.ClosedJustNow Then Return

            Dim ancora As Rectangle = ThemeButtonBounds
            If ancora.IsEmpty Then Return

            Dim elemente As List(Of CustomPopupItem) = ConstruiesteElementeleMeniului()
            ' Gardă defensivă. De la felia 0052 rândul «Font din temă» e mereu acolo, deci lista nu
            ' mai poate fi goală — dar un meniu gol tot n-are ce să arate, iar construirea listei
            ' nu e treaba acestei metode.
            If elemente.Count = 0 Then Return

            ' Nicio selecție inițială: rândul «curent» lipsește din listă tocmai fiindcă e curent,
            ' deci n-are ce fi evidențiat. NU în «Using»: arătat nemodal, popup-ul se eliberează
            ' singur la închidere.
            Dim meniu As New CustomPopup(elemente)
            AddHandler meniu.ItemClicked, AddressOf ThemeMenu_ItemClicked
            ' COMMITTED, nu Changed: rescrierea fonturilor întregii aplicații e lucru greu, iar
            ' făcută la fiecare pixel al tragerii ea reașază toate ferestrele — meniul pierdea
            ' activarea și se închidea singur la prima mișcare.
            AddHandler meniu.SliderValueCommitted, AddressOf ThemeMenu_SliderValueChanged
            Try
                _themeMenuOpening = True
                meniu.ShowBelow(Me, ancora)
            Finally
                ' Consumat de SetPopupOpen la aprindere; coborât și aici, ca o deschidere care a
                ' crăpat înainte de sink să nu lase steagul ridicat pentru meniul următor.
                _themeMenuOpening = False
            End Try
        Catch ex As Exception
            ' Frontieră UI (drumul vine din OnMouseClick): logăm și înghițim.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.ShowThemeMenu", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Rândurile meniului: schemele alegibile, apoi — dacă <see cref="ShowThemeEditor"/> e aprins —
    ''' un separator și «Stiluri...».
    '''
    ''' Ajutor chemat DOAR din <see cref="ShowThemeMenu"/>, care e deja înfășurat. <c>Friend</c>
    ''' fiindcă e ȘI cusătura de test: conținutul meniului se poate ține fix fără ecran, altfel
    ''' regula «schema activă lipsește» și comutatorul <see cref="ShowThemeEditor"/> n-ar putea fi
    ''' verificate decât cu ochii.
    ''' </summary>
    Friend Function ConstruiesteElementeleMeniului() As List(Of CustomPopupItem)
        Dim elemente As New List(Of CustomPopupItem)()
        Dim folosite As New List(Of Char)()

        ' Cursorul de mărime, în CAP. Valoarea e citită din AppScaling la fiecare deschidere, deci
        ' meniul arată mereu mărimea reală, chiar dacă a fost schimbată din fereastra de opțiuni.
        '
        ' Ascuns cât timp tema NU scrie fontul formularului (felia 0052): mărirea textului trece
        ' tocmai prin scrierea fontului pe formular, deci cu comutatorul stins cursorul ar fi tras
        ' degeaba pe jumătate din ferestre. Proprietatea ShowTextScaleSlider rămâne a
        ' OPERATORULUI — o sting șapte formulare din designer — deci se citesc amândouă, nu se
        ' derivă una din cealaltă.
        If _showTextScaleSlider AndAlso ThemeManager.WritesFormFont Then
            elemente.Add(CustomPopupItem.Slider(TextScaleKey, "Mărime text",
                                                CInt(Math.Round(AppScaling.MinTextScale * 100)),
                                                CInt(Math.Round(AppScaling.MaxTextScale * 100)),
                                                CInt(Math.Round(AppScaling.TextScale * 100))))
        End If

        ' Comutatorul fontului, sub cursor. Bifat = tema scrie fontul de bază peste cel pe care
        ' formularul îl are deja din constructor. Fiind ACELAȘI font, stingerea nu mișcă nimic pe
        ' ecran — vezi ThemeManager.WritesFormFont pentru de ce rândul există totuși.
        elemente.Add(New CustomPopupItem(ThemeFontKey,
                                         CuLiteraDeAcces("Font din temă", folosite),
                                         BifaPentru(ThemeManager.WritesFormFont)))
        elemente.Add(CustomPopupItem.Separator())

        For Each s As ThemeScheme In ThemeManager.AvailableSchemes
            If s Is Nothing Then Continue For
            If String.Equals(s.Name, ThemeManager.Current.Name, StringComparison.OrdinalIgnoreCase) Then Continue For
            elemente.Add(New CustomPopupItem(s.Name,
                                             CuLiteraDeAcces(BuiltInSchemes.DisplayName(s.Name), folosite),
                                             IconaSchemei(s)))
        Next

        ' Separatorul aparține GRUPULUI de unelte, nu unei unelte anume: se pune o dată, dacă
        ' rămâne măcar una aprinsă ȘI are ce despărți (un meniu care începe cu o linie e o linie
        ' degeaba).
        ' …și niciodată DOI la rând: cu o singură schemă alegibilă (sau niciuna), separatorul
        ' cursorului de mai sus ar fi rămas lipit de ăsta.
        If (_showThemeOptions OrElse _showThemeEditor) AndAlso elemente.Count > 0 AndAlso
           Not elemente(elemente.Count - 1).IsSeparator Then
            elemente.Add(CustomPopupItem.Separator())
        End If

        ' Ordinea celor două unelte NU e întâmplătoare: «Opțiuni temă...» reglează SCHEMA
        ' (culorile, stilul, scalarea) și e ce caută operatorul în nouă din zece cazuri;
        ' «Stiluri...» pune excepții pe controale anume ale unei ferestre și e unealta rară.
        ' Numele sunt scrise cât să se deosebească fără să fie nevoie să le încerci pe rând.
        If _showThemeOptions Then
            elemente.Add(New CustomPopupItem(ThemeOptionsKey,
                                             CuLiteraDeAcces("Opțiuni temă...", folosite),
                                             My.Resources.Resources.switch_theme))
        End If

        If _showThemeEditor Then
            elemente.Add(New CustomPopupItem(ThemeEditorKey,
                                             CuLiteraDeAcces("Stiluri...", folosite),
                                             My.Resources.Resources.ThemeEditor))
        End If

        Return elemente
    End Function

    ''' <summary>
    ''' Alegerea din meniu: ori editorul de stiluri, ori comutarea schemei. Frontieră de UI
    ''' (răspuns la un clic) — logăm și înghițim.
    ''' </summary>
    Private Sub ThemeMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If e Is Nothing OrElse e.Item Is Nothing Then Return

            If String.Equals(e.Item.Key, ThemeFontKey, StringComparison.Ordinal) Then
                ' Setterul persistă, repune fontul din designer pe ferestrele deschise și ridică
                ' ThemeChanged — aici nu mai e nimic de făcut.
                ThemeManager.WritesFormFont = Not ThemeManager.WritesFormFont
                Return
            End If

            If String.Equals(e.Item.Key, ThemeOptionsKey, StringComparison.Ordinal) Then
                DeschideOptiunileDeTema()
                Return
            End If

            If String.Equals(e.Item.Key, ThemeEditorKey, StringComparison.Ordinal) Then
                DeschideEditorulDeStiluri()
                Return
            End If

            ' Cheia elementului E numele schemei, deci drumul înapoi trece prin ResolveByName. O
            ' schemă care a dispărut între deschiderea meniului și clic (fișier de utilizator șters)
            ' se SEMNALEAZĂ — un meniu care nu face nimic e chiar no-op-ul tăcut interzis de casă.
            Dim aleasa As ThemeScheme = ThemeManager.ResolveByName(e.Item.Key)
            If aleasa Is Nothing Then
                GlobalErrorLog.Write("KBotCaptionBar.ThemeMenu_ItemClicked",
                                     New InvalidOperationException(
                                         "Schema de temă «" & e.Item.Key & "» nu mai există."))
                Return
            End If

            ThemeManager.SetScheme(aleasa)
            RaiseEvent ThemeSchemeChanged(Me, New ThemeSchemeChangedEventArgs(aleasa))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.ThemeMenu_ItemClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deschide editorul de stiluri pentru fereastra care ține bara. Eșecul se ARATĂ: operatorul
    ''' tocmai a cerut o fereastră, iar «nu s-a întâmplat nimic» n-ar avea nicio explicație.
    '''
    ''' Ajutor chemat DOAR din <see cref="ThemeMenu_ItemClicked"/>, care e deja înfășurat.
    ''' </summary>
    Private Sub DeschideEditorulDeStiluri()
        Dim gazda As Form = FindForm()
        If gazda Is Nothing Then Throw New InvalidOperationException(
            "Bara de titlu nu e pe niciun formular — editorul de stiluri n-are ce suprafață să inspecteze.")
        Try
            ThemeEditorForm.ShowFor(gazda)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.DeschideEditorulDeStiluri", ex)
            MessageBox.Show(gazda, "Nu s-a putut deschide editorul de stiluri: " & ex.Message,
                            "Stiluri", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' S-a TERMINAT de tras cursorul de mărime — la ridicarea butonului sau a tastei, nu la
    ''' fiecare pixel.
    '''
    ''' <para><b>De ce nu la fiecare pas.</b> Aplicarea rescrie fonturile întregii aplicații și
    ''' reașază toate ferestrele deschise. Făcută în timpul tragerii, era de nefolosit ca viteză
    ''' și — mai rău — reactiva fereastra de dedesubt, iar meniul se închidea singur pe
    ''' <c>Deactivate</c>. Cu munca la sfârșitul gestului, tragerea decurge netulburată: cifra de
    ''' pe șină se mișcă în timp real (aceea e ieftină, o desenează meniul), iar aplicația se
    ''' redimensionează o dată, când ridici degetul.</para>
    '''
    ''' Frontieră de UI (drumul vine dintr-un mesaj de mouse): logăm și înghițim.
    ''' </summary>
    Private Sub ThemeMenu_SliderValueChanged(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If e Is Nothing OrElse e.Item Is Nothing Then Return
            If Not String.Equals(e.Item.Key, TextScaleKey, StringComparison.Ordinal) Then Return
            AppScaling.SetTextScale(e.Item.SliderValue / 100.0F)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.ThemeMenu_SliderValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deschide fereastra de opțiuni ale temei pentru fereastra care ține bara. Ca la sora ei de
    ''' mai sus, eșecul se ARATĂ: operatorul tocmai a cerut o fereastră.
    '''
    ''' Ajutor chemat DOAR din <see cref="ThemeMenu_ItemClicked"/>, care e deja înfășurat.
    ''' </summary>
    Private Sub DeschideOptiunileDeTema()
        Dim gazda As Form = FindForm()
        If gazda Is Nothing Then Throw New InvalidOperationException(
            "Bara de titlu nu e pe niciun formular — opțiunile de temă n-au de cine să fie deținute.")
        Try
            ThemeOptionsForm.ShowFor(gazda)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.DeschideOptiunileDeTema", ex)
            MessageBox.Show(gazda, "Nu s-au putut deschide opțiunile de temă: " & ex.Message,
                            "Opțiuni de temă", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── Ajutoare ──────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Pictograma efectivă a butonului: cea aleasă de operator, iar în lipsa ei cea implicită din
    ''' resursele K-BOT. Despărțirea de <see cref="ThemeButtonImage"/> e chiar regula
    ''' ShouldSerialize: implicitul nu trebuie să treacă vreodată drept alegere.
    ''' </summary>
    Private Shared Function EffectiveThemeButtonImageCore(chosen As Image) As Image
        If chosen IsNot Nothing Then Return chosen
        Return My.Resources.Resources.switch_theme
    End Function

    Private Function EffectiveThemeButtonImage() As Image
        Return EffectiveThemeButtonImageCore(_themeButtonImage)
    End Function

    ''' <summary>
    ''' Pastila colorată a schemei. O schemă de utilizator (venită din AppData) n-are pictogramă a
    ''' ei, deci primește pastila neutră — un rând fără pictogramă ar sări din coloana celorlalte.
    ''' </summary>
    Private Shared Function IconaSchemei(scheme As ThemeScheme) As Image
        Select Case scheme.Name.Trim().ToLowerInvariant()
            Case "classic" : Return My.Resources.Resources.ThemeClassic
            Case "dark" : Return My.Resources.Resources.ThemeDark
            Case "modern" : Return My.Resources.Resources.ThemeModern
            Case "colorful" : Return My.Resources.Resources.ThemeColorful
            Case Else : Return My.Resources.Resources.ThemeClassic
        End Select
    End Function

    ''' <summary>
    ''' Pune un «&amp;» înaintea primei litere încă nefolosite din nume, ca fiecare rând să aibă
    ''' litera lui de acces. Numele schemelor vin și din fișiere de utilizator, nu dintr-o listă
    ''' fixă, deci marcajul nu poate fi scris de mână nicăieri.
    '''
    ''' Litera trebuie să fie și TASTABILĂ, nu doar liberă (<see cref="PopupMnemonic.IsTypable"/>).
    ''' În română asta nu e o subtilitate: «Întunecat» ar fi marcat «Î», care nu e nicio tastă, deci
    ''' sublinierea ar promite o scurtătură inexistentă. Se sare peste ea și iese «Î&amp;ntunecat».
    '''
    ''' Când toate literele sunt deja luate, numele rămâne nemarcat: un nume fără marcaj e mai
    ''' cinstit decât unul care pare al altcuiva.
    ''' </summary>
    Friend Shared Function CuLiteraDeAcces(nume As String, folosite As List(Of Char)) As String
        If String.IsNullOrEmpty(nume) Then Return String.Empty
        ' Un «&» din numele schemei e text, nu marcaj: se dublează ÎNAINTE de a căuta locul
        ' marcajului, ca poziția găsită să fie deja cea din șirul livrat meniului.
        Dim escapat As String = nume.Replace("&", "&&")
        For i As Integer = 0 To escapat.Length - 1
            If Not PopupMnemonic.IsTypable(escapat(i)) Then Continue For
            Dim litera As Char = Char.ToUpperInvariant(escapat(i))
            If folosite.Contains(litera) Then Continue For
            folosite.Add(litera)
            Return escapat.Insert(i, "&")
        Next
        Return escapat
    End Function

End Class
