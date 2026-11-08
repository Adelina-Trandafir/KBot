Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Descărcări multiple» (slice 0100, moved here from «Aplicație» by slice 0100-02): several angajamente
''' downloaded at once, one FOREXE tab each -- the switch, how many tabs, the update of the old angajamente
''' on connection, and «actualizează implicit toate recepțiile».
'''
''' <para><b>The server decides whether the page exists and how far it goes.</b> The settings window shows
''' the page only while <see cref="ServerSettings.MultithreadAllowed"/> (the server's <c>Setari.Multithread</c>)
''' is on, and the number of tabs can never go above <see cref="AppSettings.DownloadThreadsCeiling"/> (the
''' server's <c>Setari.Multithread_Max</c>, at most 10). The operator's own choices still save to
''' <see cref="AppSettings"/> as they change, like every other page.</para>
''' </summary>
Public Class SetariMultithreadView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    ' Guards the change handlers while the page fills its controls from the stores.
    Private _suppress As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "multithread"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            Incarca()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariMultithreadView.Activated", ex)
        End Try
    End Sub

    ' The stores can change under the page (the server's answer after a change of unit): follow them while
    ' the page has a window. Both are static, so the subscriptions end with the handle.
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        AddHandler ServerSettings.Changed, AddressOf Setarile_Changed
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        RemoveHandler ServerSettings.Changed, AddressOf Setarile_Changed
        MyBase.OnHandleDestroyed(e)
    End Sub

    Private Sub Setarile_Changed(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf Incarca))
            Else
                Incarca()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariMultithreadView.Setarile_Changed", ex)
        End Try
    End Sub

    ' ---------------- fill ----------------

    Private Sub Incarca()
        Dim before As Boolean = _suppress
        _suppress = True
        Try
            Dim s As AppSettings = AppSettings.Current
            Dim tavan As Integer = AppSettings.DownloadThreadsCeiling
            chkMultiThread.Checked = s.MultiThreadDownloads
            txtFire.Text = s.DownloadThreadsInEffect.ToString(Globalization.CultureInfo.InvariantCulture)
            chkAutoVechi.Checked = s.AutoUpdateOnConnect
            txtZile.Text = s.AutoUpdateDaysInEffect.ToString(Globalization.CultureInfo.InvariantCulture)
            chkToateReceptiile.Checked = s.UpdateAllReceptiiByDefault
            lblFire.Text = "Numărul de taburi deodată (1–" & tavan & ")"
            lblServer.Text = "Serverul îngăduie cel mult " & tavan & If(tavan = 1, " tab", " taburi") &
                             " deodată. Fiecare angajament se descarcă pe un tab FOREXE al lui."
            ActualizeazaDisponibilitatea()
        Finally
            _suppress = before
        End Try
    End Sub

    ' The main switch is enabled only while the server allows multi-thread (the page is not even shown
    ' otherwise -- this guards the moment between the server's change and the window following it). What
    ' hangs off the switch is enabled only while the switch is on; the days field also needs its own box.
    ' Kept visible (not hidden) so the operator sees what the switch brings.
    Private Sub ActualizeazaDisponibilitatea()
        Dim permis As Boolean = ServerSettings.MultithreadAllowed
        Dim pornit As Boolean = permis AndAlso chkMultiThread.Checked
        chkMultiThread.Enabled = permis
        lblFire.Enabled = pornit
        txtFire.Enabled = pornit
        chkAutoVechi.Enabled = pornit
        chkToateReceptiile.Enabled = pornit
        lblZile.Enabled = pornit AndAlso chkAutoVechi.Checked
        txtZile.Enabled = pornit AndAlso chkAutoVechi.Checked
    End Sub

    ''' <summary>
    ''' One saver for every control: copies the current store, applies the change, writes it (which makes
    ''' it Current and raises Changed), and reports on the band. A write that fails is said, and the page
    ''' is put back to what the store still holds.
    ''' </summary>
    Private Sub Salveaza(aplica As Action(Of AppSettings), mesaj As String)
        Try
            If _suppress Then Return
            Dim copie As AppSettings = AppSettings.Current.Clone()
            aplica(copie)
            copie.Save()
            RaiseEvent StatusChanged(mesaj)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariMultithreadView.Salveaza", ex)
            RaiseEvent StatusChanged("Setarea nu a putut fi salvată: " & ex.Message)
            Incarca()
        End Try
    End Sub

    ' ---------------- handlers ----------------

    Private Sub ChkMultiThread_CheckedChanged(sender As Object, e As EventArgs) Handles chkMultiThread.CheckedChanged
        ActualizeazaDisponibilitatea()
        Salveaza(Sub(s) s.MultiThreadDownloads = chkMultiThread.Checked,
                 If(chkMultiThread.Checked,
                    "Descărcarea pe mai multe taburi FOREXE e pornită (cel mult " & AppSettings.Current.DownloadThreadsInEffect & " deodată).",
                    "Descărcările merg din nou una câte una."))
    End Sub

    Private Sub ChkAutoVechi_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoVechi.CheckedChanged
        ActualizeazaDisponibilitatea()
        Salveaza(Sub(s) s.AutoUpdateOnConnect = chkAutoVechi.Checked,
                 If(chkAutoVechi.Checked,
                    "La conectare se actualizează angajamentele neactualizate de " & AppSettings.Current.AutoUpdateDaysInEffect & " zile.",
                    "La conectare nu se mai actualizează nimic singur."))
    End Sub

    Private Sub ChkToateReceptiile_CheckedChanged(sender As Object, e As EventArgs) Handles chkToateReceptiile.CheckedChanged
        Salveaza(Sub(s) s.UpdateAllReceptiiByDefault = chkToateReceptiile.Checked,
                 If(chkToateReceptiile.Checked,
                    "Cât timp descărcarea pe mai multe taburi e pornită, se citesc toate recepțiile, fără întrebare.",
                    "Alegerea recepțiilor se face ca până acum."))
    End Sub

    ' The two numbers save when the field is left or on Enter -- not on every keystroke.
    Private Sub TxtFire_Leave(sender As Object, e As EventArgs) Handles txtFire.Leave
        SalveazaNumarul(txtFire, "numărul de taburi", AppSettings.DownloadThreadsMin, AppSettings.DownloadThreadsCeiling,
                        Function(s) s.DownloadThreadsInEffect, Sub(s, n) s.DownloadThreads = n,
                        "Se descarcă cel mult {0} angajamente deodată.")
    End Sub

    Private Sub TxtZile_Leave(sender As Object, e As EventArgs) Handles txtZile.Leave
        SalveazaNumarul(txtZile, "numărul de zile", 1, AppSettings.AutoUpdateDaysMax,
                        Function(s) s.AutoUpdateDaysInEffect, Sub(s, n) s.AutoUpdateDays = n,
                        "La conectare se actualizează angajamentele neactualizate de {0} zile.")
    End Sub

    Private Sub Campuri_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtFire.FieldKeyDown, txtZile.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.SuppressKeyPress = True
            If ReferenceEquals(sender, txtFire) Then
                TxtFire_Leave(sender, EventArgs.Empty)
            Else
                TxtZile_Leave(sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariMultithreadView.Campuri_FieldKeyDown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Validates one whole-number field and saves it. Out of range or not a number -> the band says why and
    ''' the field goes back to the stored value.
    ''' </summary>
    Private Sub SalveazaNumarul(field As KBotTextField, ce As String, minim As Integer, maxim As Integer,
                                read As Func(Of AppSettings, Integer), write As Action(Of AppSettings, Integer),
                                mesajFormat As String)
        Try
            If _suppress Then Return
            Dim stored As Integer = read(AppSettings.Current)
            Dim raw As String = If(field.Text, String.Empty).Trim()
            Dim asked As Integer
            If Not Integer.TryParse(raw, Globalization.NumberStyles.None,
                                    Globalization.CultureInfo.InvariantCulture, asked) OrElse
               asked < minim OrElse asked > maxim Then
                RaiseEvent StatusChanged("Pentru " & ce & " trebuie un număr între " & minim & " și " & maxim &
                                         ". A rămas " & stored & ".")
                Incarca()
                Return
            End If
            If asked = stored Then Return
            Salveaza(Sub(s) write(s, asked), String.Format(mesajFormat, asked))
        Catch ex As Exception
            ' Reached from Leave / KeyDown only: UI boundary, log and swallow.
            GlobalErrorLog.Write("SetariMultithreadView.SalveazaNumarul", ex)
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            For Each t As Control In New Control() {tlyPagina, tlyOptiuni}
                t.BackColor = p.SurfaceAltColor
            Next
            For Each caption As Label In New Label() {lblFire, lblZile, lblServer}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("SetariMultithreadView.ApplyTheme", ex)
        End Try
    End Sub

End Class
