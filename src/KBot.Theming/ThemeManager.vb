Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Motorul central de teme (înlocuiește miezul vechiului KBotTheme). Palette+Style
''' driven, nu „If dark”. Punctul unic de intrare: <see cref="Apply"/>. Comutare live
''' prin <see cref="SetScheme"/> + evenimentul <see cref="ThemeChanged"/>.
'''
''' Difuzarea la comutare: reaplică pe reuniunea (registru de formulare tematizate ∪
''' Application.OpenForms), deduplicată — deci și formularele legacy (ne-migrate) se
''' re-tematizează, exact ca vechiul SetTheme. Formularele KBotThemedForm rulează
''' apoi DOAR OnThemeChanged() din handler-ul de eveniment (Apply deja s-a executat
''' în difuzare), evitând dublul Apply.
''' </summary>
Public Module ThemeManager

    Private _current As ThemeScheme = BuiltInSchemes.Classic()
    Private ReadOnly _userSchemes As New List(Of ThemeScheme)()
    Private ReadOnly _forms As New List(Of WeakReference(Of Form))()
    Private _initialized As Boolean = False

    ''' <summary>Schema activă curentă.</summary>
    Public ReadOnly Property Current As ThemeScheme
        Get
            Return _current
        End Get
    End Property

    ''' <summary>
    ''' Schemele alegibile: cele built-in, plus cele de utilizator descoperite în AppData.
    '''
    ''' <para><b>Un fișier cu numele unei scheme built-in o ÎNLOCUIEȘTE, nu se adaugă lângă ea</b>
    ''' (felia 0036). Așa se persistă editarea lui «Modern»: editorul scrie
    ''' <c>…\Themes\Modern.json</c>, iar de la pornirea următoare acela E «Modern». Regula veche —
    ''' concatenare oarbă — ar fi produs DOUĂ rânduri «Modern» în meniul de teme, iar
    ''' <c>ResolveByName</c> ar fi întors mereu primul, adică exact pe cel needitat.</para>
    '''
    ''' <para>Ștergerea fișierului readuce schema compilată: codul sursă nu se atinge niciodată,
    ''' deci «Restaurează implicit» n-are cum să eșueze pe jumătate.</para>
    ''' </summary>
    Public ReadOnly Property AvailableSchemes As IReadOnlyList(Of ThemeScheme)
        Get
            Return MergeSchemes(BuiltInSchemes.All(), _userSchemes)
        End Get
    End Property

    ''' <summary>
    ''' Built-in-urile, cu cele omonime din <paramref name="user"/> puse peste, apoi restul
    ''' schemelor de utilizator în ordinea lor. Pură — de aceea e și cusătura de test.
    ''' </summary>
    Friend Function MergeSchemes(builtIn As IReadOnlyList(Of ThemeScheme),
                                 user As IReadOnlyList(Of ThemeScheme)) As IReadOnlyList(Of ThemeScheme)
        Dim result As New List(Of ThemeScheme)()
        Dim consumed As New List(Of ThemeScheme)()

        For Each b As ThemeScheme In builtIn
            Dim replacement As ThemeScheme = Nothing
            For Each u As ThemeScheme In user
                If u IsNot Nothing AndAlso String.Equals(u.Name, b.Name, StringComparison.OrdinalIgnoreCase) Then
                    replacement = u
                    Exit For
                End If
            Next
            If replacement Is Nothing Then
                result.Add(b)
            Else
                result.Add(replacement)
                consumed.Add(replacement)
            End If
        Next

        For Each u As ThemeScheme In user
            If u IsNot Nothing AndAlso Not consumed.Contains(u) Then result.Add(u)
        Next

        Return result
    End Function

    ''' <summary>Ridicat DUPĂ ce Current s-a schimbat (Apply deja difuzat).</summary>
    Public Event ThemeChanged As EventHandler

    ''' <summary>
    ''' Încarcă schema persistată (sau default = Classic) + schemele utilizator din
    ''' AppData. Idempotent — apelabil o singură dată la pornire.
    ''' </summary>
    Public Sub Initialize()
        If _initialized Then Return
        _initialized = True

        ' Scalarea ÎNAINTE de orice altceva: e citită de fiecare control la prima pictare, deci
        ' trebuie să fie deja așezată când se construiește primul formular (felia 0036).
        ThemeStore.LoadScaling()

        ' Scheme utilizator — inclusiv fișierele care SUPRASCRIU o schemă built-in editată din
        ' fereastra de opțiuni. Un fișier corupt e sărit + logat, nu crapă pornirea.
        _userSchemes.Clear()
        _userSchemes.AddRange(ThemeStore.LoadUserSchemes())

        ' Numele schemei active persistat; fallback documentat = Classic.
        Dim activeName As String = ThemeStore.LoadActiveName()
        Dim resolved As ThemeScheme = ResolveByName(activeName)
        _current = If(resolved, BuiltInSchemes.Classic())
    End Sub

    ''' <summary>Rezolvă un nume la o schemă (built-in sau utilizator); Nothing dacă nu există.</summary>
    ''' <remarks>
    ''' Public de la felia 0028: selectorul de temă din MainForm ține în listă NUMELE schemelor
    ''' (ThemeScheme nu suprascrie ToString) și are nevoie de drumul invers nume → schemă.
    ''' Testele îl folosesc și pentru contractul de fallback (nume necunoscut → Nothing → Classic).
    ''' </remarks>
    Public Function ResolveByName(name As String) As ThemeScheme
        If String.IsNullOrWhiteSpace(name) Then Return Nothing
        For Each s In AvailableSchemes
            If String.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) Then
                Return s
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>Aplică schema curentă unui control/formular (recursiv).</summary>
    Public Sub Apply(ctrl As Control)
        If ctrl Is Nothing Then Return
        ctrl.SuspendLayout()
        Try
            Traverse(ctrl)
        Finally
            ctrl.ResumeLayout(True)
        End Try

        ' Bară de titlu dark/light (DWM). Contract log-once în NativeMethods.
        Dim f As Form = TryCast(ctrl, Form)
        If f IsNot Nothing Then
            NativeMethods.SetTitleBarDark(f, _current.Style.DarkTitleBar)
            ' Formulare fără chenar (card borderless): cerem colțuri rotunjite DWM.
            ' NativeMethods e Friend — cererea trebuie să pornească de aici, ca și dark-ul.
            If f.FormBorderStyle = FormBorderStyle.None Then
                NativeMethods.SetRoundedCorners(f, True)
            End If
        End If

        ' MĂRIMEA TEXTULUI, la SFÂRȘIT (felia 0036-01). Ordinea nu e o preferință: ApplyBaseFont
        ' tocmai a scris fontul schemei pe formular, iar sub «Colorat» PreserveDesignerColors a
        ' restaurat fonturile autorite — amândouă ar fi șters mărirea dacă ar fi rulat DUPĂ ea.
        ' Aici, orice ar fi scris tema devine baza din care se înmulțește (vezi FontBaseline).
        AppScaling.ApplyTextScale(ctrl)
    End Sub

    ''' <summary>Setează schema activă, o persistă, o difuzează și ridică ThemeChanged.</summary>
    Public Sub SetScheme(scheme As ThemeScheme)
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        _current = scheme
        ThemeStore.SaveActive(scheme.Name)

        ' Difuzare: registru ∪ OpenForms, deduplicat pe identitate de referință.
        For Each f As Form In CollectTargets()
            Apply(f)
        Next

        RaiseEvent ThemeChanged(Nothing, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Re-difuzează schema ACTIVĂ, fără s-o schimbe și fără s-o persiste. O folosește fereastra
    ''' de opțiuni după fiecare valoare atinsă: schema e un obiect MUTABIL, deci editarea ei a
    ''' avut deja loc — ce lipsește e doar repictarea ferestrelor.
    '''
    ''' Despărțită de <see cref="SetScheme"/> tocmai ca previzualizarea să nu scrie pe disc la
    ''' fiecare mișcare de cursor prin selectorul de culoare.
    ''' </summary>
    Public Sub Refresh()
        Try
            For Each f As Form In CollectTargets()
                Apply(f)
            Next
            RaiseEvent ThemeChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeManager.Refresh", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Persistă o schemă editată ca fișier de utilizator și o pune în lista celor alegibile
    ''' (înlocuind versiunea cu același nume, built-in sau nu). Dacă e chiar schema activă, ecranul
    ''' se reîmprospătează.
    '''
    ''' Frontieră de I/O: loghează ȘI aruncă — operatorul tocmai a apăsat «Salvează».
    ''' </summary>
    Public Sub SaveScheme(scheme As ThemeScheme)
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        Try
            ThemeStore.SaveScheme(scheme)

            For i As Integer = _userSchemes.Count - 1 To 0 Step -1
                If String.Equals(_userSchemes(i).Name, scheme.Name, StringComparison.OrdinalIgnoreCase) Then
                    _userSchemes.RemoveAt(i)
                End If
            Next
            _userSchemes.Add(scheme)

            If String.Equals(scheme.Name, _current.Name, StringComparison.OrdinalIgnoreCase) Then
                _current = scheme
                Refresh()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeManager.SaveScheme", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' «Readu schema asta la implicitul ei»: șterge fișierul de utilizator și, dacă numele e al
    ''' unei scheme built-in, o repune pe cea compilată. Întoarce schema rezultată — Nothing dacă
    ''' era o schemă pur de utilizator, care prin ștergere a dispărut cu totul.
    '''
    ''' Când dispare CHIAR schema activă, se comută pe implicita documentată; altfel aplicația ar
    ''' rămâne pictată cu o schemă care nu mai există nicăieri.
    ''' </summary>
    Public Function ResetScheme(schemeName As String) As ThemeScheme
        If String.IsNullOrWhiteSpace(schemeName) Then Throw New ArgumentException(
            "Numele schemei e obligatoriu.", NameOf(schemeName))
        Try
            ThemeStore.DeleteScheme(schemeName)

            For i As Integer = _userSchemes.Count - 1 To 0 Step -1
                If String.Equals(_userSchemes(i).Name, schemeName, StringComparison.OrdinalIgnoreCase) Then
                    _userSchemes.RemoveAt(i)
                End If
            Next

            Dim rezultat As ThemeScheme = ResolveByName(schemeName)   ' built-in-ul, dacă există
            If String.Equals(schemeName, _current.Name, StringComparison.OrdinalIgnoreCase) Then
                SetScheme(If(rezultat, ResolveByName(BuiltInSchemes.DefaultSchemeName)))
            End If
            Return rezultat
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeManager.ResetScheme", ex)
            Throw
        End Try
    End Function

    ' Reuniunea formularelor tematizate înregistrate și a celor deschise (legacy incluse).
    Private Function CollectTargets() As List(Of Form)
        Dim targets As New List(Of Form)()
        PurgeDeadForms()
        For Each wr In _forms
            Dim f As Form = Nothing
            If wr.TryGetTarget(f) AndAlso f IsNot Nothing AndAlso Not targets.Contains(f) Then
                targets.Add(f)
            End If
        Next
        For Each f As Form In Application.OpenForms
            If f IsNot Nothing AndAlso Not targets.Contains(f) Then
                targets.Add(f)
            End If
        Next
        Return targets
    End Function

    ''' <summary>Înregistrează un formular tematizat (apelat de KBotThemedForm).</summary>
    Friend Sub RegisterForm(f As Form)
        If f Is Nothing Then Return
        PurgeDeadForms()
        For Each wr In _forms
            Dim existing As Form = Nothing
            If wr.TryGetTarget(existing) AndAlso existing Is f Then Return
        Next
        _forms.Add(New WeakReference(Of Form)(f))
    End Sub

    ''' <summary>Dezînregistrează un formular tematizat.</summary>
    Friend Sub UnregisterForm(f As Form)
        If f Is Nothing Then Return
        For i As Integer = _forms.Count - 1 To 0 Step -1
            Dim existing As Form = Nothing
            If Not _forms(i).TryGetTarget(existing) OrElse existing Is f Then
                _forms.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub PurgeDeadForms()
        For i As Integer = _forms.Count - 1 To 0 Step -1
            Dim existing As Form = Nothing
            If Not _forms(i).TryGetTarget(existing) OrElse existing Is Nothing Then
                _forms.RemoveAt(i)
            End If
        Next
    End Sub

    ' =========================================================================
    ' TRAVERSARE RECURSIVĂ (port verbatim din KBotTheme, cu excepțiile SplitContainer/TabControl)
    ' =========================================================================
    Private Sub Traverse(ctrl As Control)
        ' PRIMA operație pe orice control: instantaneul valorilor din designer. Trebuie luat
        ' înainte de orice scriere a temei — după, valoarea autorită nu mai există (vezi
        ' DesignerBaseline). Idempotent: doar prima trecere reține ceva.
        CaptureBaseline(ctrl)

        Dim preserve As Boolean = _current.Style.PreserveDesignerColors

        ' Controalele auto-tematizate își aplică singure culorile ȘI NU se recurge în
        ' ele cu regulile GENERICE — altfel regula de Panel ar repicta suprafața, iar recursia ar
        ' strica TextBox-ul intern din KBotTextField.
        Dim themed As IThemedControl = TryCast(ctrl, IThemedControl)
        If themed IsNot Nothing Then
            themed.ApplyTheme(_current)
            ' …iar sub «Colorful» punem înapoi cele trei proprietăți ambientale peste ce-a scris
            ' ApplyTheme: interiorul controlului rămâne al schemei, suprafața rămâne a designerului.
            If preserve Then DesignerBaseline.Restore(ctrl)
            ' …DAR un control auto-tematizat poate GĂZDUI la rândul lui controale auto-tematizate,
            ' iar acelea trebuie să-și primească schema. Cazul e regula, nu excepția: toate cele
            ' șase vederi reale (Sumar, Rezervări, Recepții, Plăți, DDF, Istoric) sunt
            ' IThemedControl ȘI țin un KBotDataView înăuntru, deci oprirea seacă de aici lăsa
            ' NETEMATIZATĂ grila din fiecare — pe bancul de probă, unde grila stă direct pe
            ' formular, aceeași grilă se colora corect, ceea ce făcea diferența greu de citit.
            ApplyToNestedThemed(ctrl)
            Return
        End If

        If preserve Then
            PreserveDesigner(ctrl)
        Else
            StyleControl(ctrl)
        End If

        If TypeOf ctrl Is SplitContainer Then
            Dim sc = DirectCast(ctrl, SplitContainer)
            For Each child As Control In sc.Panel1.Controls
                Traverse(child)
            Next
            For Each child As Control In sc.Panel2.Controls
                Traverse(child)
            Next
            Return
        End If

        If TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            For Each tp As TabPage In tc.TabPages
                Traverse(tp)
            Next
            Return
        End If

        For Each child As Control In ctrl.Controls
            Traverse(child)
        Next
    End Sub

    ''' <summary>
    ''' Duce schema la controalele auto-tematizate din INTERIORUL unui control auto-tematizat, fără
    ''' să aplice vreo regulă generică pe drum.
    '''
    ''' <para>Asta e diferența față de <see cref="Traverse"/>, și e toată povestea: aici nu se
    ''' stilizează nimic „după tip”, se doar PREDĂ schema celor care știu singure ce să facă cu ea.
    ''' De aceea coborârea e sigură exact acolo unde <c>Traverse</c> n-avea voie să meargă —
    ''' <c>TextBox</c>-ul intern al lui <c>KBotTextField</c> sau al benzii de căutare a arborelui nu
    ''' e <c>IThemedControl</c>, deci nu e atins, în timp ce grila dintr-o vedere îl este și îl
    ''' primește.</para>
    ''' </summary>
    Private Sub ApplyToNestedThemed(container As Control)
        For Each child As Control In container.Controls
            Dim themed As IThemedControl = TryCast(child, IThemedControl)
            If themed IsNot Nothing Then
                ' Instantaneul de designer se ia și aici, înaintea oricărei scrieri — altfel
                ' «Colorful» n-ar avea ce restaura pentru un control imbricat.
                CaptureBaseline(child)
                themed.ApplyTheme(_current)
                If _current.Style.PreserveDesignerColors Then DesignerBaseline.Restore(child)
            End If
            ApplyToNestedThemed(child)
        Next
    End Sub

    ' =========================================================================
    ' «COLORFUL» — schema care restaurează în loc să scrie (Style.PreserveDesignerColors)
    ' =========================================================================

    ''' <summary>
    ''' Instantaneul de designer al controlului, plus cazul special al lui SplitContainer.
    ''' Panourile lui NU trec niciodată prin <see cref="Traverse"/> (recursia sare direct la copiii
    ''' lor), dar <see cref="StylePalette"/> LE SCRIE — deci dacă nu le-am fotografia AICI, prima
    ''' lor fotografie ar fi luată abia sub «Colorful», adică deja peste culoarea temei anterioare.
    ''' Exact asta a prins testul «Colorful_RestoresSplitContainerPanels_Too».
    ''' </summary>
    Private Sub CaptureBaseline(ctrl As Control)
        DesignerBaseline.Capture(ctrl)
        Dim sc As SplitContainer = TryCast(ctrl, SplitContainer)
        If sc IsNot Nothing Then
            DesignerBaseline.Capture(sc.Panel1)
            DesignerBaseline.Capture(sc.Panel2)
        End If
    End Sub

    ''' <summary>
    ''' Pune controlul înapoi pe valorile din designer. Ordinea contează: întâi DESPRINDEM
    ''' cârligele de pictură lăsate de o schemă anterioară — un buton rămas owner-drawn se
    ''' pictează singur cu paleta veche și ar face restaurarea lui BackColor complet invizibilă,
    ''' la fel inelul de focus de pe inputuri și owner-draw-ul de pe tab-uri — și abia apoi
    ''' restaurăm instantaneul.
    ''' </summary>
    Private Sub PreserveDesigner(ctrl As Control)
        Dim btn As Button = TryCast(ctrl, Button)
        If btn IsNot Nothing Then ModernRenderer.DetachButton(btn)

        If TypeOf ctrl Is ComboBox OrElse TypeOf ctrl Is TextBox OrElse TypeOf ctrl Is MaskedTextBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
        End If

        Dim tc As TabControl = TryCast(ctrl, TabControl)
        If tc IsNot Nothing Then SetupTabOwnerDraw(tc, False)

        ' Panourile unui SplitContainer nu trec niciodată prin Traverse (recursia sare direct la
        ' copiii lor), deci nu s-ar restaura niciodată — dar StylePalette LE SCRIE. Le tratăm aici.
        Dim sc As SplitContainer = TryCast(ctrl, SplitContainer)
        If sc IsNot Nothing Then
            DesignerBaseline.Capture(sc.Panel1)
            DesignerBaseline.Capture(sc.Panel2)
            DesignerBaseline.Restore(sc.Panel1)
            DesignerBaseline.Restore(sc.Panel2)
        End If

        DesignerBaseline.Restore(ctrl)
    End Sub

    ' =========================================================================
    ' STILIZARE PER CONTROL — comută pe UseSystemColors, altfel merge pe paletă
    ' =========================================================================
    Private Sub StyleControl(ctrl As Control)
        If _current.Style.UseSystemColors Then
            StyleSystem(ctrl)
        Else
            StylePalette(ctrl, _current)
        End If
    End Sub

    ' ─────────────────── PALETĂ (Dark / Modern / scheme viitoare) ─────────────
    Private Sub StylePalette(ctrl As Control, scheme As ThemeScheme)
        Dim p As ThemePalette = scheme.Palette
        Dim st As ThemeStyleOptions = scheme.Style
        Dim listTheme As String = If(scheme.IsDark, "DarkMode_Explorer", "Explorer")
        Dim comboTheme As String = If(scheme.IsDark, "DarkMode_CFD", "Explorer")

        If TypeOf ctrl Is Form Then
            ctrl.BackColor = p.SurfaceColor
            ApplyBaseFont(ctrl, st)

        ElseIf TypeOf ctrl Is SplitContainer Then
            ctrl.BackColor = p.SurfaceColor
            DirectCast(ctrl, SplitContainer).Panel1.BackColor = p.SurfaceColor
            DirectCast(ctrl, SplitContainer).Panel2.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            tc.BackColor = p.SurfaceColor
            SetupTabOwnerDraw(tc, st.OwnerDrawTabs)

        ElseIf TypeOf ctrl Is TabPage Then
            ctrl.BackColor = p.SurfaceAltColor

        ElseIf IsCard(ctrl) Then
            ctrl.BackColor = p.SurfaceAltColor

        ElseIf TypeOf ctrl Is TableLayoutPanel Then
            ctrl.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is Panel Then
            ctrl.BackColor = p.SurfaceColor

        ElseIf TypeOf ctrl Is GroupBox Then
            ctrl.BackColor = p.SurfaceColor
            ctrl.ForeColor = p.TextColor

        ElseIf TypeOf ctrl Is Label Then
            ctrl.ForeColor = p.TextColor
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is CheckBox Then
            Dim chk = DirectCast(ctrl, CheckBox)
            If chk.Appearance = Appearance.Button Then
                chk.FlatStyle = FlatStyle.Flat
                chk.BackColor = p.ButtonBackColor
                chk.ForeColor = p.ButtonTextColor
                chk.FlatAppearance.BorderColor = p.ButtonBorderColor
                chk.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
                chk.FlatAppearance.CheckedBackColor = p.AccentColor
                chk.UseVisualStyleBackColor = False
            Else
                ctrl.ForeColor = p.TextColor
                ctrl.BackColor = Color.Transparent
            End If

        ElseIf TypeOf ctrl Is RadioButton Then
            ctrl.ForeColor = p.TextColor
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is Button Then
            Dim btn = DirectCast(ctrl, Button)
            If btn.Tag?.ToString() = "ThemeToggle" Then
                UpdateToggleButton(btn)
            ElseIf Not IsAccentButton(btn) Then
                If st.ButtonRender = ButtonRenderStyle.ModernOwnerDrawn Then
                    ModernRenderer.ApplyButton(btn, scheme)
                Else
                    ModernRenderer.DetachButton(btn)
                    btn.FlatStyle = FlatStyle.Flat
                    btn.BackColor = p.ButtonBackColor
                    btn.ForeColor = p.ButtonTextColor
                    btn.FlatAppearance.BorderColor = p.ButtonBorderColor
                    btn.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
                    btn.UseVisualStyleBackColor = False
                End If
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                NativeMethods.ApplyWindowTheme(ctrl, listTheme)
            Else
                ctrl.BackColor = p.InputBackColor
                ctrl.ForeColor = p.InputTextColor
                NativeMethods.ApplyWindowTheme(ctrl, listTheme)
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, listTheme)

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, listTheme)

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = p.InputBackColor
            tv.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(tv, listTheme)

        ElseIf TypeOf ctrl Is ComboBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            NativeMethods.ApplyWindowTheme(ctrl, comboTheme)
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is TextBox Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor
            If st.FocusAccent Then ModernRenderer.AttachFocusAccent(ctrl, scheme) Else ModernRenderer.DetachFocusAccent(ctrl)

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = p.InputBackColor
            ctrl.ForeColor = p.InputTextColor

        ElseIf TypeOf ctrl Is ProgressBar Then
            ' ProgressBar — lăsăm stilul system (ca înainte).

        End If
    End Sub

    ' ─────────────────── SISTEM (Classic; port verbatim din StyleLight) ───────
    Private Sub StyleSystem(ctrl As Control)
        If TypeOf ctrl Is Form Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is SplitContainer Then
            ctrl.BackColor = SystemColors.Control
            DirectCast(ctrl, SplitContainer).Panel1.BackColor = SystemColors.Control
            DirectCast(ctrl, SplitContainer).Panel2.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is TabControl Then
            Dim tc = DirectCast(ctrl, TabControl)
            tc.BackColor = SystemColors.Control
            SetupTabOwnerDraw(tc, False)

        ElseIf TypeOf ctrl Is TabPage Then
            ctrl.BackColor = SystemColors.Control
            DirectCast(ctrl, TabPage).UseVisualStyleBackColor = True

        ElseIf IsCard(ctrl) Then
            ctrl.BackColor = Color.White

        ElseIf TypeOf ctrl Is TableLayoutPanel Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is Panel Then
            ctrl.BackColor = SystemColors.Control

        ElseIf TypeOf ctrl Is GroupBox Then
            ctrl.BackColor = SystemColors.Control
            ctrl.ForeColor = SystemColors.ControlText

        ElseIf TypeOf ctrl Is Label Then
            ctrl.ForeColor = SystemColors.ControlText
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is CheckBox Then
            Dim chk = DirectCast(ctrl, CheckBox)
            If chk.Appearance = Appearance.Button Then
                chk.FlatStyle = FlatStyle.Flat
                chk.BackColor = SystemColors.Control
                chk.ForeColor = SystemColors.ControlText
                chk.FlatAppearance.BorderColor = SystemColors.ControlDark
                chk.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight
                chk.FlatAppearance.CheckedBackColor = SystemColors.Highlight
                chk.UseVisualStyleBackColor = False
            Else
                ctrl.ForeColor = SystemColors.ControlText
                ctrl.BackColor = Color.Transparent
                chk.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf ctrl Is RadioButton Then
            ctrl.ForeColor = SystemColors.ControlText
            ctrl.BackColor = Color.Transparent

        ElseIf TypeOf ctrl Is Button Then
            Dim btn = DirectCast(ctrl, Button)
            If btn.Tag?.ToString() = "ThemeToggle" Then
                UpdateToggleButton(btn)
            ElseIf Not IsAccentButton(btn) Then
                ModernRenderer.DetachButton(btn)
                btn.FlatStyle = FlatStyle.Standard
                btn.BackColor = SystemColors.Control
                btn.ForeColor = SystemColors.ControlText
                btn.UseVisualStyleBackColor = True
            End If

        ElseIf TypeOf ctrl Is RichTextBox Then
            If ctrl.Tag?.ToString() = "SyntaxRTB" Then
                NativeMethods.ApplyWindowTheme(ctrl, "Explorer")
            Else
                ctrl.BackColor = Color.White
                ctrl.ForeColor = SystemColors.WindowText
                NativeMethods.ApplyWindowTheme(ctrl, "Explorer")
            End If

        ElseIf TypeOf ctrl Is CheckedListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is ListBox Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is TreeView Then
            Dim tv = DirectCast(ctrl, TreeView)
            tv.BackColor = SystemColors.Window
            tv.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(tv, "Explorer")

        ElseIf TypeOf ctrl Is ComboBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText
            NativeMethods.ApplyWindowTheme(ctrl, "Explorer")

        ElseIf TypeOf ctrl Is MaskedTextBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is TextBox Then
            ModernRenderer.DetachFocusAccent(ctrl)
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        ElseIf TypeOf ctrl Is NumericUpDown Then
            ctrl.BackColor = SystemColors.Window
            ctrl.ForeColor = SystemColors.WindowText

        End If
    End Sub

    ' Aplică fontul de bază al schemei pe formular (copiii moștenesc fontul ambiant).
    ' „Segoe UI Variable Text” lipsă => GDI cade elegant pe fontul default (fără excepție).
    Private Sub ApplyBaseFont(ctrl As Control, st As ThemeStyleOptions)
        If String.IsNullOrWhiteSpace(st.BaseFontName) OrElse st.BaseFontSize <= 0F Then Return
        Try
            ctrl.Font = New Font(st.BaseFontName, st.BaseFontSize, ctrl.Font.Style)
            ' Fontul schemei e noua BAZĂ pentru mărirea textului (felia 0036-01). Fără linia asta,
            ' mărirea s-ar înmulți peste fontul deja mărit al schemei precedente.
            FontBaseline.Rebase(ctrl)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeManager.ApplyBaseFont", ex)
        End Try
    End Sub

    ' =========================================================================
    ' TABCONTROL OWNER DRAW (port din KBotTheme; culorile din paleta curentă)
    ' =========================================================================
    Private Sub SetupTabOwnerDraw(tc As TabControl, ownerDraw As Boolean)
        RemoveHandler tc.DrawItem, AddressOf OnDrawTab
        If ownerDraw Then
            tc.DrawMode = TabDrawMode.OwnerDrawFixed
            AddHandler tc.DrawItem, AddressOf OnDrawTab
        Else
            tc.DrawMode = TabDrawMode.Normal
        End If
        tc.Invalidate()
    End Sub

    Private Sub OnDrawTab(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim tc = DirectCast(sender, TabControl)
        If e.Index >= tc.TabPages.Count Then Return
        Dim tp = tc.TabPages(e.Index)
        Dim isSelected = (e.Index = tc.SelectedIndex)
        Dim p As ThemePalette = _current.Palette

        Dim bgColor = If(isSelected, p.SurfaceAltColor, p.TabInactiveColor)
        Using bg As New SolidBrush(bgColor)
            e.Graphics.FillRectangle(bg, e.Bounds)
        End Using

        If isSelected Then
            Using accent As New SolidBrush(p.TabAccentColor)
                e.Graphics.FillRectangle(accent,
                    New Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2))
            End Using
        End If

        Using txt As New SolidBrush(p.TextColor)
            Dim sf As New StringFormat() With {
                .Alignment = StringAlignment.Center,
                .LineAlignment = StringAlignment.Center,
                .FormatFlags = StringFormatFlags.NoWrap
            }
            e.Graphics.DrawString(tp.Text, tc.Font, txt,
                                  RectangleF.op_Implicit(e.Bounds), sf)
        End Using
    End Sub

    ' =========================================================================
    ' BUTON TOGGLE TEMĂ (port; „dark” = schema curentă e dark)
    ' =========================================================================
    Private Sub UpdateToggleButton(btn As Button)
        Dim p As ThemePalette = _current.Palette
        If _current.IsDark Then
            btn.Text = "☀️"
            btn.FlatStyle = FlatStyle.Flat
            btn.BackColor = p.ButtonBackColor
            btn.ForeColor = p.ButtonTextColor
            btn.FlatAppearance.BorderColor = p.ButtonBorderColor
            btn.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
            btn.UseVisualStyleBackColor = False
        Else
            btn.Text = "🌙"
            btn.FlatStyle = FlatStyle.Standard
            btn.BackColor = SystemColors.Control
            btn.ForeColor = SystemColors.ControlText
            btn.UseVisualStyleBackColor = True
        End If
    End Sub

    ' =========================================================================
    ' HELPER: „card” = Panel/TableLayoutPanel marcat Tag="Card" — suprafață SurfaceAlt
    ' (convenția Tag e deja pattern-ul casei: vezi „ThemeToggle” / „SyntaxRTB”).
    ' =========================================================================
    Private Function IsCard(ctrl As Control) As Boolean
        If TypeOf ctrl IsNot Panel AndAlso TypeOf ctrl IsNot TableLayoutPanel Then Return False
        Return ctrl.Tag IsNot Nothing AndAlso String.Equals(ctrl.Tag.ToString(), "Card", StringComparison.Ordinal)
    End Function

    ' =========================================================================
    ' HELPER: buton cu culoare funcțională (verde/roșu/galben) — NU se re-tematizează
    ' =========================================================================
    Private Function IsAccentButton(btn As Button) As Boolean
        If btn.UseVisualStyleBackColor Then Return False
        Dim c = btn.BackColor
        Return c.GetSaturation() > 0.25F AndAlso
               c <> _current.Palette.ButtonBackColor AndAlso
               c <> Color.Transparent AndAlso
               c.ToArgb() <> SystemColors.Control.ToArgb()
    End Function

End Module
