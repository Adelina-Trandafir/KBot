Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Consola FOREXE (felia 0034): jurnalul COMPLET al robotului, progresul, ultima linie de
''' stare, anularea, «arată browserul» și deschiderea fișierului de jurnal. Nu are butoane de
''' descărcare — descărcările se comandă din arbore (subsol = lista, iconița din dreapta unui
''' nod = angajamentul acela).
'''
''' <para>Se creează O SINGURĂ DATĂ și NU se distruge: închiderea doar o ascunde. Motivul e
''' <c>RichTextBoxLogger</c>, care primește <see cref="rtbLog"/> la construcție și îl ține cât
''' trăiește aplicația — o fereastră distrusă i-ar lăsa un control eliberat în mână.</para>
''' </summary>
Public Class ForexeConsoleForm

    Private _controller As ForexeController
    ' Calea fișierului de jurnal, pentru butonul «Deschide jurnalul».
    Private _caleJurnal As String = String.Empty

    Public Sub New()
        InitializeComponent()
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' Iconița e cosmetică — absența ei nu împiedică deschiderea ferestrei.
            GlobalErrorLog.Write("ForexeConsoleForm.New", ex)
        End Try
    End Sub

    ''' <summary>Caseta de jurnal — ținta <c>RichTextBoxLogger</c>-ului atașat runner-ului.</summary>
    Public ReadOnly Property LogBox As RichTextBox
        Get
            Return rtbLog
        End Get
    End Property

    ''' <summary>Calea fișierului de jurnal, arătată de butonul «Deschide jurnalul».</summary>
    Public Property CaleJurnal As String
        Get
            Return _caleJurnal
        End Get
        Set(value As String)
            _caleJurnal = If(value, String.Empty)
        End Set
    End Property

    ''' <summary>
    ''' Leagă fereastra de coordonator. Controalele sunt puse în designer, deci nu pot primi
    ''' coordonatorul prin constructor — se leagă după creare, o singură dată.
    ''' </summary>
    Public Sub Bind(controller As ForexeController)
        Try
            ArgumentNullException.ThrowIfNull(controller)
            If _controller IsNot Nothing Then Return   ' o singură legare

            _controller = controller
            AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
            AddHandler _controller.ProgressChanged, AddressOf Controller_ProgressChanged
            AddHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
            ActualizeazaStarea()
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.Bind", ex)
            Throw
        End Try
    End Sub

    ' ── Evenimentele coordonatorului ─────────────────────────────────────
    ' Toate trei pot veni de pe firul robotului: trecem pe firul de UI înainte de a atinge
    ' vreun control. Frontieră de UI => logăm și înghițim.

    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            PeFirulDeUI(AddressOf ActualizeazaStarea)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.Controller_StateChanged", ex)
        End Try
    End Sub

    Private Sub Controller_ProgressChanged(sender As Object, procent As Integer)
        Try
            PeFirulDeUI(Sub() pbProgress.Value = Math.Max(0, Math.Min(100, procent)))
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.Controller_ProgressChanged", ex)
        End Try
    End Sub

    Private Sub Controller_StatusChanged(sender As Object, stare As String)
        Try
            PeFirulDeUI(Sub() lblStatus.Text = If(stare, String.Empty))
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.Controller_StatusChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Consola se leagă la coordonator din <c>MainForm_Load</c>, dar se ARATĂ abia când operatorul
    ''' apasă butonul din bandă — iar între cele două momente fereastra n-are handle, deci
    ''' <see cref="PeFirulDeUI"/> aruncă TOATE evenimentele coordonatorului (nu are unde să le
    ''' împingă). De aceea starea se recitește la fiecare aducere la vedere: altfel consola arăta
    ''' pentru totdeauna starea de la legare — «neconectat», cu «Arată browserul» stins — chiar
    ''' dacă shell-ul era de mult conectat.
    ''' </summary>
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        Try
            If Visible Then ActualizeazaStarea()
        Catch ex As Exception
            ' Frontieră de UI: logăm și înghițim.
            GlobalErrorLog.Write("ForexeConsoleForm.OnVisibleChanged", ex)
        End Try
    End Sub

    ' Starea butoanelor + certificatul, dintr-un singur loc.
    Private Sub ActualizeazaStarea()
        If _controller Is Nothing Then Return
        Dim conectat As Boolean = _controller.IsConnected
        Dim ocupat As Boolean = _controller.IsBusy

        btnAnulare.Enabled = ocupat
        btnAfiseazaBrowser.Enabled = conectat
        ' Eticheta spune ce FACE apăsarea, nu ce se vede acum.
        btnAfiseazaBrowser.Text = If(conectat AndAlso _controller.IsBrowserVisible,
                                     "Ascunde browserul", "Arată browserul")
        Dim cert As String = _controller.CertificateName
        lblCert.Text = "Certificat: " & If(String.IsNullOrEmpty(cert), "—", cert)

        ' Linia de stare și progresul se iau tot de la coordonator, nu doar din evenimente:
        ' altfel o consolă deschisă târziu ar porni goală, deși robotul lucrează de zece minute.
        lblStatus.Text = If(_controller.LastStatus.Length > 0, _controller.LastStatus, "În așteptare...")
        pbProgress.Value = If(ocupat, Math.Max(0, Math.Min(100, _controller.LastPercent)), 0)
    End Sub

    ' ── Butoane ──────────────────────────────────────────────────────────

    Private Sub BtnAnulare_Click(sender As Object, e As EventArgs) Handles btnAnulare.Click
        Try
            _controller?.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.btnAnulare_Click", ex)
            MessageBox.Show(Me, "Anularea nu a putut fi cerută: " & ex.Message, "Consolă FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub BtnAfiseazaBrowser_Click(sender As Object, e As EventArgs) Handles btnAfiseazaBrowser.Click
        Try
            If _controller Is Nothing Then Return
            ' Comutare, nu doar «arată»: browserul pornește ASCUNS (stealth), deci operatorul
            ' trebuie să-l poată pune la loc după ce s-a uitat la el.
            Await _controller.ToggleBrowserAsync()
        Catch ex As Exception
            ' Frontieră de UI (async Sub): nu poate rearunca — logăm și spunem de ce.
            GlobalErrorLog.Write("ForexeConsoleForm.btnAfiseazaBrowser_Click", ex)
            MessageBox.Show(Me, "Browserul nu a putut fi adus în față: " & ex.Message, "Consolă FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub BtnAfiseazaLog_Click(sender As Object, e As EventArgs) Handles btnAfiseazaLog.Click
        Try
            If String.IsNullOrEmpty(_caleJurnal) OrElse Not File.Exists(_caleJurnal) Then
                MessageBox.Show(Me, "Fișierul de jurnal nu există (încă).", "Consolă FOREXE",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Process.Start(New ProcessStartInfo(_caleJurnal) With {.UseShellExecute = True})
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.btnAfiseazaLog_Click", ex)
            MessageBox.Show(Me, "Jurnalul nu a putut fi deschis: " & ex.Message, "Consolă FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Închiderea ASCUNDE fereastra, nu o distruge: <c>rtbLog</c> este ținta logger-ului
    ''' FOREXE pe toată durata aplicației. O fereastră distrusă i-ar lăsa în mână un control
    ''' eliberat, iar prima linie de jurnal de după aceea ar cădea.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Hide()
                Return
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeConsoleForm.OnFormClosing", ex)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim schema = ThemeManager.Current
            Dim p = schema.Palette

            ' Fundalul formularului ESTE conturul de 1px al ferestrei (vezi LoginForm/MainForm).
            BackColor = p.BorderColor

            ' Jurnalul e o suprafață de citit: fundal de intrare, text pe culoarea de bază.
            rtbLog.BackColor = p.InputBackColor
            rtbLog.ForeColor = p.TextColor
            ' Culorile pe nivel ale logger-ului urmează schema (dark vs light).
            RichTextBoxLogger.SetColorScheme(schema.IsDark)

            lblCert.ForeColor = p.TextDimColor
            lblStatus.ForeColor = p.TextColor

            ButtonStyles.ApplyPrimary(btnAnulare, schema)
            ButtonStyles.ApplySecondary(btnAfiseazaBrowser, schema)
            ButtonStyles.ApplySecondary(btnAfiseazaLog, schema)
        Catch ex As Exception
            ' Frontieră de UI (cascada de temă/paint): logăm și înghițim.
            GlobalErrorLog.Write("ForexeConsoleForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' Trecerea pe firul de UI: evenimentele coordonatorului vin de pe firul robotului.
    Private Sub PeFirulDeUI(actiune As Action)
        If IsDisposed OrElse Disposing Then Return
        If Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(actiune)
        Else
            actiune()
        End If
    End Sub

End Class
