Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Temă» (slice 0072): the same content as <c>ThemeOptionsForm</c> (slice 0036), as a page
''' of the settings window -- the scheme and its 23 colour slots + style options in a
''' property grid, and the application scaling underneath. The logic is the same, line
''' for line where it could be, so the two surfaces cannot drift apart in behaviour:
'''
''' <para><b>Choosing a scheme ACTIVATES it</b> (what you edit is what you see);
''' <b>effect at once, save explicitly</b> (nothing reaches the disk before «Salvează»);
''' «Restaurează implicit» deletes the personalised file and puts the compiled scheme
''' back; <b>scaling is not part of the scheme</b> and writes to <c>theme.json</c> on its
''' own, through <see cref="AppScaling"/>.</para>
'''
''' <para>Unsaved scheme edits are asked about when the settings window closes
''' (<see cref="CanClose"/>) -- the page's counterpart of the form's OnFormClosing.</para>
''' </summary>
Public Class SetariTemaView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Private _suppress As Boolean
    Private _dirty As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "tema"
        End Get
    End Property

    Public Sub Activated() Implements ISetariView.Activated
        Try
            IncarcaModurileDeScalare()
            IncarcaScalarea()
            ' The grid is bound to the LIVE scheme; rebinding would drop unsaved edits, so it
            ' happens only the first time, or when the active scheme changed elsewhere.
            If grid.SelectedObject Is Nothing OrElse Not _dirty Then IncarcaSchemele()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.Activated", ex)
        End Try
    End Sub

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Try
            If Not _dirty Then Return True
            Dim raspuns As DialogResult = KBotMessage.Show(FindForm(),
                $"Schema «{BuiltInSchemes.DisplayName(ThemeManager.Current.Name)}» are modificări nesalvate. Le salvez?",
                "Modificări nesalvate", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning)
            Select Case raspuns
                Case DialogResult.Yes
                    ThemeManager.SaveScheme(ThemeManager.Current)
                    _dirty = False
                    Return True
                Case DialogResult.Cancel
                    Return False
                Case Else
                    ' «Nu»: the edits stay on screen until restart but never reach the disk.
                    Return True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.CanClose", ex)
            Return True
        End Try
    End Function

    ' ---------------- scheme ----------------

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
            GlobalErrorLog.Write("SetariTemaView.IncarcaSchemele", ex)
            Throw
        End Try
    End Sub

    ' The ACTIVE scheme goes in the grid, not the list item: AvailableSchemes builds new
    ' instances on every call, so an item would be an unedited copy.
    Private Sub LeagaGrila()
        grid.SelectedObject = New SchemeOptionsProxy(ThemeManager.Current, AddressOf DupaModificare)
        _dirty = False
        ActualizeazaStareaSchemei()
    End Sub

    Private Sub DupaModificare()
        Try
            _dirty = True
            ThemeManager.Refresh()
            ActualizeazaStareaSchemei()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.DupaModificare", ex)
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

    Private Sub CboScheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScheme.SelectedIndexChanged
        Try
            If _suppress Then Return
            Dim item As SchemeItem = TryCast(cboScheme.SelectedItem, SchemeItem)
            If item Is Nothing Then Return

            ' Switching schemes DROPS unsaved edits (the list makes new instances); ask first.
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
            RaiseEvent StatusChanged($"Schema activă: {item}.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.CboScheme_SelectedIndexChanged", ex)
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
        Return KBotMessage.Show(FindForm(),
            $"Schema «{BuiltInSchemes.DisplayName(ThemeManager.Current.Name)}» are modificări nesalvate. Le pierzi?",
            "Modificări nesalvate", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes
    End Function

    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Dim schema As ThemeScheme = ThemeManager.Current
            ThemeManager.SaveScheme(schema)
            _dirty = False
            ActualizeazaStareaSchemei()
            RaiseEvent StatusChanged($"Salvat: {IO.Path.GetFileName(ThemeStore.SchemeFilePath(schema.Name))}.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.BtnSave_Click", ex)
            ShowError("Salvarea schemei a eșuat.", ex)
        End Try
    End Sub

    Private Sub BtnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            Dim nume As String = ThemeManager.Current.Name
            If KBotMessage.Show(FindForm(),
                    $"Schema «{BuiltInSchemes.DisplayName(nume)}» revine la valorile din program, iar personalizarea salvată se șterge. Continui?",
                    "Restaurează implicit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim revenita As ThemeScheme = ThemeManager.ResetScheme(nume)
            _dirty = False
            IncarcaSchemele()
            RaiseEvent StatusChanged(If(revenita Is Nothing,
                                        $"Schema «{nume}» a fost ștearsă.",
                                        $"«{BuiltInSchemes.DisplayName(nume)}» readusă la valorile din program."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.BtnReset_Click", ex)
            ShowError("Restaurarea schemei a eșuat.", ex)
        End Try
    End Sub

    ' ---------------- scaling ----------------

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
            trkTextScale.Minimum = CInt(Math.Round(AppScaling.MinTextScale * 100))
            trkTextScale.Maximum = CInt(Math.Round(AppScaling.MaxTextScale * 100))
            trkTextScale.Value = ProcenteDinScara(AppScaling.TextScale)
            rdoFitScaled.Checked = (ThemeFormFit.Baseline = FormFitBaseline.Scaled)
            rdoFitRaw.Checked = (ThemeFormFit.Baseline = FormFitBaseline.DesignerRaw)
        Finally
            _suppress = False
        End Try
        ActualizeazaDisponibilitateaFactorului()
        ActualizeazaEticheta()
    End Sub

    Private Sub RdoFitBaseline_CheckedChanged(sender As Object, e As EventArgs) _
            Handles rdoFitScaled.CheckedChanged, rdoFitRaw.CheckedChanged
        Try
            If _suppress Then Return
            Dim rdo As RadioButton = TryCast(sender, RadioButton)
            If rdo Is Nothing OrElse Not rdo.Checked Then Return
            Dim wanted As FormFitBaseline = If(rdo Is rdoFitRaw, FormFitBaseline.DesignerRaw, FormFitBaseline.Scaled)
            If wanted = ThemeFormFit.Baseline Then Return
            ThemeFormFit.Baseline = wanted
            RaiseEvent StatusChanged(If(wanted = FormFitBaseline.Scaled,
                                        "Baza ferestrelor urmează scalarea.",
                                        "Baza ferestrelor rămâne cea din designer."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.RdoFitBaseline_CheckedChanged", ex)
            ShowError("Schimbarea bazei ferestrelor a eșuat.", ex)
        End Try
    End Sub

    Private Function ProcenteDinScara(scara As Single) As Integer
        Dim p As Integer = CInt(Math.Round(scara * 100))
        Return Math.Max(trkTextScale.Minimum, Math.Min(trkTextScale.Maximum, p))
    End Function

    Private Sub ActualizeazaEticheta()
        lblTextScaleValue.Text = trkTextScale.Value.ToString(Globalization.CultureInfo.CurrentCulture) & "%"
    End Sub

    Private Sub TrkTextScale_ValueChanged(sender As Object, e As EventArgs) Handles trkTextScale.ValueChanged
        ActualizeazaEticheta()
    End Sub

    ' The size is applied at the END of the gesture (mouse up / key up / focus lost), never
    ' per tick: applying rescales every window, this one included, and the thumb would move
    ' under the finger -- see ThemeOptionsForm for the full reasoning.
    Private Sub TrkTextScale_GestFinalizat(sender As Object, e As EventArgs) _
            Handles trkTextScale.MouseUp, trkTextScale.KeyUp, trkTextScale.Leave
        AplicaMarimeaTextului()
    End Sub

    Private Sub AplicaMarimeaTextului()
        Try
            If _suppress Then Return
            Dim ceruta As Single = trkTextScale.Value / 100.0F
            If Math.Abs(ceruta - AppScaling.TextScale) < 0.0001F Then Return
            AppScaling.SetTextScale(ceruta)
            RaiseEvent StatusChanged($"Mărime text: {trkTextScale.Value}%.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.AplicaMarimeaTextului", ex)
            ShowError("Schimbarea mărimii textului a eșuat.", ex)
        End Try
    End Sub

    Private Sub ActualizeazaDisponibilitateaFactorului()
        Dim manual As Boolean = (AppScaling.Mode = ScalingMode.Manual)
        numScalingFactor.Enabled = manual
        lblScalingFactor.Enabled = manual
    End Sub

    Private Sub CboScalingMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScalingMode.SelectedIndexChanged
        AplicaScalarea()
    End Sub

    Private Sub NumScalingFactor_ValueChanged(sender As Object, e As EventArgs) Handles numScalingFactor.ValueChanged
        AplicaScalarea()
    End Sub

    Private Sub AplicaScalarea()
        Try
            If _suppress Then Return
            Dim item As ScalingItem = TryCast(cboScalingMode.SelectedItem, ScalingItem)
            If item Is Nothing Then Return
            AppScaling.Configure(item.Mode, CSng(numScalingFactor.Value))
            ActualizeazaDisponibilitateaFactorului()
            RaiseEvent StatusChanged($"Scalare: {item} (factor {AppScaling.FactorFor(Me):0.00}).")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.AplicaScalarea", ex)
            ShowError("Schimbarea scalării a eșuat.", ex)
        End Try
    End Sub

    Private Sub ChkDpiUnaware_CheckedChanged(sender As Object, e As EventArgs) Handles chkDpiUnaware.CheckedChanged
        Try
            If _suppress Then Return
            AppScaling.DpiUnaware = chkDpiUnaware.Checked
            RaiseEvent StatusChanged("Setare salvată — are efect la următoarea pornire a aplicației.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.ChkDpiUnaware_CheckedChanged", ex)
            ShowError("Salvarea setării a eșuat.", ex)
        End Try
    End Sub

    ' ---------------- theme / messages ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyTop.BackColor = p.SurfaceAltColor
            tlyScaling.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblScheme, lblSchemeState, lblScalingMode, lblScalingFactor,
                                                      lblTextScale, lblFitBaseline, lblScalingHint}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            lblTextScaleValue.ForeColor = p.TextColor
            grid.BackColor = p.SurfaceColor
            grid.ViewBackColor = p.InputBackColor
            grid.ViewForeColor = p.InputTextColor
            grid.LineColor = p.BorderColor
            grid.CategoryForeColor = p.TextColor
            grid.HelpBackColor = p.SurfaceColor
            grid.HelpForeColor = p.TextDimColor
            ButtonStyles.ApplySecondary(btnReset, scheme)
            ButtonStyles.ApplyPrimary(btnSave, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariTemaView.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub ShowError(title As String, ex As Exception)
        RaiseEvent StatusChanged(title)
        KBotMessage.Show(FindForm(), $"{title}{Environment.NewLine}{ex.Message}", "Opțiuni de temă",
                         MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>List item: the scheme + its Romanian label.</summary>
    Private NotInheritable Class SchemeItem
        Public ReadOnly Property Scheme As ThemeScheme

        Public Sub New(scheme As ThemeScheme)
            Me.Scheme = scheme
        End Sub

        Public Overrides Function ToString() As String
            Return BuiltInSchemes.DisplayName(Scheme.Name)
        End Function
    End Class

    ''' <summary>List item for the scaling mode.</summary>
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
