Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' OPȚIUNILE TEMEI (felia 0036): fereastra din care se reglează schema însăși — cele 23 de
''' sloturi de culoare, opțiunile de stil (randarea butoanelor, raza colțului, fontul de bază,
''' spațierea) — plus scalarea aplicației.
'''
''' <para><b>Nu e „Stiluri..."</b>, deși se deschid amândouă din același buton. <c>ThemeEditorForm</c>
''' pune EXCEPȚII pe controale anume ale unei ferestre («butonul ăsta, de pe formularul ăsta,
''' roșu»). Aici se schimbă tema, adică ce înseamnă «Modern» pentru toate ferestrele deodată.
''' Confuzia dintre ele e ușor de făcut, de aceea rândurile de meniu sunt scrise diferit.</para>
'''
''' <para><b>Alegerea schemei din listă o și ACTIVEAZĂ.</b> Editorul arată efectul pe ferestrele
''' din spate, deci ce editezi trebuie să fie ce vezi; o listă din care ai putea edita o schemă
''' inactivă ar picta pe ecran altceva decât valorile de sub cursor.</para>
'''
''' <para><b>Efect imediat, salvare explicită.</b> Fiecare valoare atinsă se vede pe loc, dar pe
''' disc nu ajunge nimic până la «Salvează» — atunci schema se scrie în
''' <c>…\AVACONT\Themes\&lt;Nume&gt;.json</c> și de la pornirea următoare ACELA e «Modern».
''' «Restaurează implicit» șterge fișierul și repune schema compilată; codul sursă nu se atinge
''' niciodată, deci nu există drum fără întoarcere.</para>
'''
''' <para><b>Scalarea nu ține de schemă</b> și scrie separat, în <c>theme.json</c> — vezi panoul
''' de jos și <see cref="AppScaling"/>. E o proprietate a ecranului pe care lucrează operatorul;
''' pusă în schemă, o trecere de la «Modern» la «Întunecat» ar redimensiona tăcut aplicația.</para>
''' </summary>
Public Class ThemeOptionsForm

    Private ReadOnly _host As Form
    Private _suppress As Boolean = False
    Private _dirty As Boolean = False

    ''' <param name="host">Fereastra din care s-a cerut editorul — doar ca proprietar al ferestrei.</param>
    Public Sub New(host As Form)
        InitializeComponent()
        _host = host
    End Sub

    ''' <summary>
    ''' Deschide fereastra ne-modal, deținută de gazdă. Dacă e deja deschisă pentru aceeași
    ''' gazdă, o aduce în față: două ferestre pe aceeași schemă ar edita același obiect din două
    ''' locuri, iar ce salvează una ar contrazice ce arată cealaltă.
    ''' </summary>
    Public Shared Function ShowFor(host As Form) As ThemeOptionsForm
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        Try
            For Each f As Form In host.OwnedForms
                Dim existing As ThemeOptionsForm = TryCast(f, ThemeOptionsForm)
                If existing IsNot Nothing Then
                    existing.BringToFront()
                    existing.Activate()
                    Return existing
                End If
            Next

            Dim editor As New ThemeOptionsForm(host)
            editor.Show(host)
            Return editor
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.ShowFor", ex)
            Throw
        End Try
    End Function

    Protected Overrides Sub OnLoad(e As EventArgs)
        Try
            MyBase.OnLoad(e)   ' KBotThemedForm: înregistrare + aplicarea temei
            IncarcaModurileDeScalare()
            IncarcaScalarea()
            IncarcaSchemele()
        Catch ex As Exception
            ' Frontieră UI (Load): un throw ar rupe deschiderea ferestrei.
            GlobalErrorLog.Write("ThemeOptionsForm.OnLoad", ex)
        End Try
    End Sub

    ' ---------------- schema ----------------

    ''' <summary>
    ''' Umple lista de scheme și o selectează pe cea activă. Elementele sunt
    ''' <see cref="SchemeItem"/>, nu <c>ThemeScheme</c>: schema nu-și suprascrie <c>ToString</c>,
    ''' iar în listă trebuie să apară numele ROMÂNESC — cheia rămâne cea englezească.
    ''' </summary>
    Private Sub IncarcaSchemele()
        Try
            _suppress = True
            Try
                cboScheme.Items.Clear()
                Dim index As Integer = 0
                Dim scheme As List(Of ThemeScheme) = New List(Of ThemeScheme)(ThemeManager.AvailableSchemes)
                For i As Integer = 0 To scheme.Count - 1
                    cboScheme.Items.Add(New SchemeItem(scheme(i)))
                    If String.Equals(scheme(i).Name, ThemeManager.Current.Name, StringComparison.OrdinalIgnoreCase) Then
                        index = i
                    End If
                Next
                If cboScheme.Items.Count > 0 Then cboScheme.SelectedIndex = index
            Finally
                _suppress = False
            End Try

            LeagaGrila()
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.IncarcaSchemele", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Pune în grilă schema ACTIVĂ — nu elementul din listă. Sunt același obiect după
    ''' <c>SetScheme</c>, dar <c>AvailableSchemes</c> construiește instanțe NOI la fiecare apel
    ''' (schemele built-in se fabrică din cod), deci un element vechi din listă ar fi o copie
    ''' needitată, iar modificările s-ar duce în gol.
    ''' </summary>
    Private Sub LeagaGrila()
        grid.SelectedObject = New SchemeOptionsProxy(ThemeManager.Current, AddressOf DupaModificare)
        _dirty = False
        ActualizeazaStareaSchemei()
    End Sub

    ' Chemat de proxy după fiecare valoare atinsă: repictăm tot și marcăm nesalvat.
    Private Sub DupaModificare()
        Try
            _dirty = True
            ThemeManager.Refresh()
            ActualizeazaStareaSchemei()
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.DupaModificare", ex)
        End Try
    End Sub

    Private Sub ActualizeazaStareaSchemei()
        Dim personalizata As Boolean = IO.File.Exists(ThemeStore.SchemeFilePath(ThemeManager.Current.Name))
        If _dirty Then
            lblSchemeState.Text = "Modificată — nesalvată."
        ElseIf personalizata Then
            lblSchemeState.Text = "Personalizată (salvată în AppData)."
        Else
            lblSchemeState.Text = "Valorile din program."
        End If
        btnReset.Enabled = personalizata OrElse _dirty
    End Sub

    Private Sub cboScheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScheme.SelectedIndexChanged
        Try
            If _suppress Then Return
            Dim item As SchemeItem = TryCast(cboScheme.SelectedItem, SchemeItem)
            If item Is Nothing Then Return

            ' Trecerea pe altă schemă ARUNCĂ modificările nesalvate: schema editată e un obiect
            ' viu, iar lista fabrică instanțe noi, deci nu există unde să fie ținute deoparte.
            ' Un avertisment e mai ieftin decât o pierdere tăcută.
            If _dirty AndAlso Not ConfirmaPierderea() Then
                _suppress = True
                Try
                    SelecteazaSchemaActiva()
                Finally
                    _suppress = False
                End Try
                Return
            End If

            ThemeManager.SetScheme(item.Scheme)
            LeagaGrila()
            SetStatus($"Schema activă: {item}.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.cboScheme_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub SelecteazaSchemaActiva()
        For i As Integer = 0 To cboScheme.Items.Count - 1
            Dim item As SchemeItem = TryCast(cboScheme.Items(i), SchemeItem)
            If item IsNot Nothing AndAlso
               String.Equals(item.Scheme.Name, ThemeManager.Current.Name, StringComparison.OrdinalIgnoreCase) Then
                cboScheme.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    Private Function ConfirmaPierderea() As Boolean
        Return MessageBox.Show(Me,
            $"Schema «{BuiltInSchemes.DisplayName(ThemeManager.Current.Name)}» are modificări nesalvate. Le pierzi?",
            "Modificări nesalvate", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes
    End Function

    ' ---------------- salvare / restaurare ----------------

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Dim schema As ThemeScheme = ThemeManager.Current
            ThemeManager.SaveScheme(schema)
            _dirty = False
            ActualizeazaStareaSchemei()
            SetStatus($"Salvat: {IO.Path.GetFileName(ThemeStore.SchemeFilePath(schema.Name))}.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.btnSave_Click", ex)
            ShowError("Salvarea schemei a eșuat.", ex)
        End Try
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            Dim nume As String = ThemeManager.Current.Name
            If MessageBox.Show(Me,
                    $"Schema «{BuiltInSchemes.DisplayName(nume)}» revine la valorile din program, iar personalizarea salvată se șterge. Continui?",
                    "Restaurează implicit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim revenita As ThemeScheme = ThemeManager.ResetScheme(nume)
            _dirty = False
            IncarcaSchemele()

            If revenita Is Nothing Then
                ' O schemă pur de utilizator a dispărut cu totul; ThemeManager a comutat deja pe
                ' cea implicită, iar lista tocmai s-a refăcut fără ea.
                SetStatus($"Schema «{nume}» a fost ștearsă.")
            Else
                SetStatus($"«{BuiltInSchemes.DisplayName(nume)}» readusă la valorile din program.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.btnReset_Click", ex)
            ShowError("Restaurarea schemei a eșuat.", ex)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            MyBase.OnFormClosing(e)
            If Not _dirty Then Return

            Dim raspuns As DialogResult = MessageBox.Show(Me,
                $"Schema «{BuiltInSchemes.DisplayName(ThemeManager.Current.Name)}» are modificări nesalvate. Le salvez?",
                "Modificări nesalvate", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning)

            Select Case raspuns
                Case DialogResult.Yes
                    ThemeManager.SaveScheme(ThemeManager.Current)
                    _dirty = False
                Case DialogResult.Cancel
                    e.Cancel = True
                Case Else
                    ' «Nu»: modificările rămân pe ecran până la repornire, dar nu ajung pe disc.
                    ' Nu se restaurează nimic aici — asta ar cere o copie a schemei de dinainte,
                    ' iar operatorul tocmai a spus că nu vrea să le păstreze, nu că vrea înapoi
                    ' ce era. Repornirea le duce oricum.
            End Select
        Catch ex As Exception
            ' Frontieră UI: un throw din închidere ar lăsa fereastra blocată.
            GlobalErrorLog.Write("ThemeOptionsForm.OnFormClosing", ex)
        End Try
    End Sub

    ' ---------------- scalare ----------------

    Private Sub IncarcaModurileDeScalare()
        _suppress = True
        Try
            cboScalingMode.Items.Clear()
            cboScalingMode.Items.Add(New ScalingItem(ScalingMode.Automatic, "Automat (după ecran)"))
            cboScalingMode.Items.Add(New ScalingItem(ScalingMode.Fixed100, "Fix 100% (ca în designer)"))
            cboScalingMode.Items.Add(New ScalingItem(ScalingMode.Manual, "Manual (factorul de alături)"))
        Finally
            _suppress = False
        End Try
    End Sub

    Private Sub IncarcaScalarea()
        _suppress = True
        Try
            For i As Integer = 0 To cboScalingMode.Items.Count - 1
                Dim item As ScalingItem = TryCast(cboScalingMode.Items(i), ScalingItem)
                If item IsNot Nothing AndAlso item.Mode = AppScaling.Mode Then
                    cboScalingMode.SelectedIndex = i
                    Exit For
                End If
            Next
            numScalingFactor.Value = CDec(AppScaling.ManualFactor)
            chkDpiUnaware.Checked = AppScaling.DpiUnaware

            ' Cursorul lucrează în PROCENTE întregi; AppScaling ține o fracție. Limitele vin tot
            ' de acolo, ca să nu existe două păreri despre cât de mare poate fi textul.
            trkTextScale.Minimum = CInt(Math.Round(AppScaling.MinTextScale * 100))
            trkTextScale.Maximum = CInt(Math.Round(AppScaling.MaxTextScale * 100))
            trkTextScale.Value = ProcenteDinScara(AppScaling.TextScale)
        Finally
            _suppress = False
        End Try
        ActualizeazaDisponibilitateaFactorului()
        ActualizeazaEticheta()
    End Sub

    ' Procentele întregi, ținute în interiorul șinei — o valoare din fișier ușor în afara
    ' limitelor ar face TrackBar-ul să arunce, iar asta ar rupe deschiderea ferestrei.
    Private Function ProcenteDinScara(scara As Single) As Integer
        Dim p As Integer = CInt(Math.Round(scara * 100))
        Return Math.Max(trkTextScale.Minimum, Math.Min(trkTextScale.Maximum, p))
    End Function

    Private Sub ActualizeazaEticheta()
        lblTextScaleValue.Text = trkTextScale.Value.ToString(Globalization.CultureInfo.CurrentCulture) & "%"
    End Sub

    ''' <summary>
    ''' Cât se trage, se mișcă DOAR eticheta cu procentul — aceea e ieftină. Mărimea propriu-zisă
    ''' se aplică la sfârșitul gestului (vezi <see cref="AplicaMarimeaTextului"/>).
    ''' </summary>
    Private Sub trkTextScale_ValueChanged(sender As Object, e As EventArgs) Handles trkTextScale.ValueChanged
        ActualizeazaEticheta()
    End Sub

    ''' <summary>
    ''' Sfârșitul gestului: ridicarea butonului de mouse, ridicarea tastei, sau pierderea focusului
    ''' (cineva a plecat de pe cursor cu Tab, fără să ridice nimic pe el).
    '''
    ''' <para><b>De ce nu la fiecare pas.</b> Aplicarea rescrie fonturile întregii aplicații și
    ''' reașază toate ferestrele — inclusiv pe ASTA, deci cursorul s-ar muta sub deget în timp ce
    ''' îl tragi. La un `TrackBar` asta înseamnă că ținta se plimbă; în meniu era și mai rău, acolo
    ''' popup-ul se închidea singur. Aceeași regulă în amândouă locurile: previzualizare ieftină în
    ''' timpul gestului, lucrul greu la capătul lui.</para>
    ''' </summary>
    Private Sub trkTextScale_GestFinalizat(sender As Object, e As EventArgs) _
            Handles trkTextScale.MouseUp, trkTextScale.KeyUp, trkTextScale.Leave
        AplicaMarimeaTextului()
    End Sub

    Private Sub AplicaMarimeaTextului()
        Try
            If _suppress Then Return
            Dim ceruta As Single = trkTextScale.Value / 100.0F
            If Math.Abs(ceruta - AppScaling.TextScale) < 0.0001F Then Return   ' n-a mișcat nimeni nimic
            AppScaling.SetTextScale(ceruta)
            SetStatus($"Mărime text: {trkTextScale.Value}%.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.AplicaMarimeaTextului", ex)
            ShowError("Schimbarea mărimii textului a eșuat.", ex)
        End Try
    End Sub

    ' Factorul are înțeles doar pe modul «Manual»; lăsat activ pe celelalte, ar părea o valoare
    ' care face ceva.
    Private Sub ActualizeazaDisponibilitateaFactorului()
        Dim manual As Boolean = (AppScaling.Mode = ScalingMode.Manual)
        numScalingFactor.Enabled = manual
        lblScalingFactor.Enabled = manual
    End Sub

    Private Sub cboScalingMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScalingMode.SelectedIndexChanged
        AplicaScalarea()
    End Sub

    Private Sub numScalingFactor_ValueChanged(sender As Object, e As EventArgs) Handles numScalingFactor.ValueChanged
        AplicaScalarea()
    End Sub

    Private Sub AplicaScalarea()
        Try
            If _suppress Then Return
            Dim item As ScalingItem = TryCast(cboScalingMode.SelectedItem, ScalingItem)
            If item Is Nothing Then Return

            AppScaling.Configure(item.Mode, CSng(numScalingFactor.Value))
            ActualizeazaDisponibilitateaFactorului()
            SetStatus($"Scalare: {item} (factor {AppScaling.FactorFor(Me):0.00}).")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.AplicaScalarea", ex)
            ShowError("Schimbarea scalării a eșuat.", ex)
        End Try
    End Sub

    Private Sub chkDpiUnaware_CheckedChanged(sender As Object, e As EventArgs) Handles chkDpiUnaware.CheckedChanged
        Try
            If _suppress Then Return
            AppScaling.DpiUnaware = chkDpiUnaware.Checked
            SetStatus("Setare salvată — are efect la următoarea pornire a aplicației.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.chkDpiUnaware_CheckedChanged", ex)
            ShowError("Salvarea setării a eșuat.", ex)
        End Try
    End Sub

    ' ---------------- temă / mesaje ----------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p = ThemeManager.Current.Palette
            lblStatus.ForeColor = p.TextDimColor
            lblSchemeState.ForeColor = p.TextDimColor
            lblScalingHint.ForeColor = p.TextDimColor
            lblTextScaleValue.ForeColor = p.TextColor
            grid.BackColor = p.SurfaceColor
            grid.ViewBackColor = p.InputBackColor
            grid.ViewForeColor = p.InputTextColor
            grid.LineColor = p.BorderColor
            grid.CategoryForeColor = p.TextColor
            grid.HelpBackColor = p.SurfaceColor
            grid.HelpForeColor = p.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOptionsForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub SetStatus(text As String)
        lblStatus.Text = If(text, String.Empty)
    End Sub

    Private Sub ShowError(title As String, ex As Exception)
        SetStatus(title)
        MessageBox.Show(Me, $"{title}{Environment.NewLine}{ex.Message}", "Opțiuni de temă",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>Element de listă: schema + eticheta ei românească.</summary>
    Private NotInheritable Class SchemeItem
        Public ReadOnly Property Scheme As ThemeScheme

        Public Sub New(scheme As ThemeScheme)
            Me.Scheme = scheme
        End Sub

        Public Overrides Function ToString() As String
            Return BuiltInSchemes.DisplayName(Scheme.Name)
        End Function
    End Class

    ''' <summary>Element de listă pentru modul de scalare.</summary>
    Private NotInheritable Class ScalingItem
        Public ReadOnly Property Mode As ScalingMode
        Private ReadOnly _label As String

        Public Sub New(mode As ScalingMode, label As String)
            Me.Mode = mode
            _label = label
        End Sub

        Public Overrides Function ToString() As String
            Return _label
        End Function
    End Class

End Class
