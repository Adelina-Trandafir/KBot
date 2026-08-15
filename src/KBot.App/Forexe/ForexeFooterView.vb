Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Banda FOREXE din subsolul shell-ului (felia 0034): butonul «Conectare», starea conexiunii,
''' progresul, certificatul ales, ULTIMA linie de stare și butonul care deschide consola.
''' Înlocuiește <c>lblProgram</c> și înghite ce arăta <c>lblForexe</c>.
'''
''' <para>Suprafață PROASTĂ: nu vorbește cu robotul, ci doar cu <see cref="ForexeController"/> —
''' se abonează la evenimentele lui și îi cheamă intențiile. Descărcările NU pornesc de aici,
''' ci din arbore.</para>
'''
''' <para>Implementează <see cref="IThemedControl"/> pentru că are copii proprii: fără asta
''' <c>ThemeManager.Traverse</c> ar recurge în ei și regulile generice pe tip le-ar rescrie
''' culorile (vezi regula casei — a mușcat deja de două ori).</para>
''' </summary>
Public Class ForexeFooterView
    Implements IThemedControl

    Private _controller As ForexeController

    ''' <summary>Operatorul a cerut consola FOREXE (butonul de extindere).</summary>
    Public Event ExpandRequested As EventHandler

    Public Sub New()
        InitializeComponent()
        ' Starea de repaus, pusă AICI și nu în designer: pe suprafața de proiectare cele două
        ' rămân vizibile (altfel operatorul n-ar mai avea ce apuca), iar la rulare pornesc
        ' ascunse. Fiind andocate la stânga, ascunse nu ocupă deloc lățime.
        pbProgress.Visible = False
        lblCert.Visible = False
    End Sub

    ''' <summary>
    ''' Leagă banda de coordonator. Controalele stau în designer, deci coordonatorul nu poate
    ''' veni prin constructor — shell-ul îl dă după creare, o singură dată.
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
            GlobalErrorLog.Write("ForexeFooterView.Bind", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Desface abonamentele (chemat din Dispose — coordonatorul e singleton și
    ''' trăiește mai mult decât banda, deci un abonament rămas ar ține controlul în viață).</summary>
    Friend Sub Dezleaga()
        Try
            If _controller Is Nothing Then Return
            RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged
            RemoveHandler _controller.ProgressChanged, AddressOf Controller_ProgressChanged
            RemoveHandler _controller.StatusChanged, AddressOf Controller_StatusChanged
            _controller = Nothing
        Catch ex As Exception
            ' Frontieră de eliberare: nu rearuncăm din Dispose.
            GlobalErrorLog.Write("ForexeFooterView.Dezleaga", ex)
        End Try
    End Sub

    ' ── Evenimentele coordonatorului (pot veni de pe firul robotului) ────

    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            PeFirulDeUI(AddressOf ActualizeazaStarea)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeFooterView.Controller_StateChanged", ex)
        End Try
    End Sub

    Private Sub Controller_ProgressChanged(sender As Object, procent As Integer)
        Try
            PeFirulDeUI(Sub() pbProgress.Value = Math.Max(0, Math.Min(100, procent)))
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeFooterView.Controller_ProgressChanged", ex)
        End Try
    End Sub

    Private Sub Controller_StatusChanged(sender As Object, stare As String)
        Try
            PeFirulDeUI(Sub() lblStatus.Text = If(stare, String.Empty))
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeFooterView.Controller_StatusChanged", ex)
        End Try
    End Sub

    ' Tot ce depinde de starea coordonatorului, într-un singur loc.
    Private Sub ActualizeazaStarea()
        If _controller Is Nothing Then Return
        Dim conectat As Boolean = _controller.IsConnected
        Dim ocupat As Boolean = _controller.IsBusy

        btnConectare.Enabled = Not ocupat AndAlso Not conectat
        lblConexiune.Text = If(conectat, "● Forexe: conectat", "● Forexe: neconectat")

        Dim p = ThemeManager.Current.Palette
        lblConexiune.ForeColor = If(conectat, p.SuccessColor, p.TextDimColor)

        ' Certificatul: eticheta apare DOAR după ce s-a ales unul. Fără certificat n-are ce
        ' spune, iar un «Certificat: —» permanent e zgomot, nu informație.
        Dim cert As String = _controller.CertificateName
        Dim areCert As Boolean = Not String.IsNullOrEmpty(cert)
        lblCert.Visible = areCert
        If areCert Then lblCert.Text = "Certificat: " & cert

        ' Bara de progres se vede DOAR cât rulează ceva; în repaus se retrage la zero și dispare
        ' (fiind andocată la stânga, nu lasă gol în urmă).
        pbProgress.Visible = ocupat
        If Not ocupat Then pbProgress.Value = 0
    End Sub

    ' ── Butoane ──────────────────────────────────────────────────────────

    Private Async Sub BtnConectare_Click(sender As Object, e As EventArgs) Handles btnConectare.Click
        Try
            If _controller Is Nothing Then Return
            Await _controller.ConnectAsync()
        Catch ex As Exception
            ' Frontieră de UI (async Sub): nu poate rearunca — logăm și spunem de ce.
            GlobalErrorLog.Write("ForexeFooterView.btnConectare_Click", ex)
            MessageBox.Show(Me, "Conectarea la FOREXE a eșuat: " & ex.Message, "FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub BtnExtinde_Click(sender As Object, e As EventArgs) Handles btnExtinde.Click
        Try
            RaiseEvent ExpandRequested(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeFooterView.btnExtinde_Click", ex)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p = scheme.Palette

            ' Banda stă pe cardul de status al shell-ului: transparentă, ca linia lui să se vadă.
            BackColor = Color.Transparent

            lblCert.ForeColor = p.TextDimColor
            lblStatus.ForeColor = p.TextDimColor

            ButtonStyles.ApplyPrimary(btnConectare, scheme)
            ButtonStyles.ApplySecondary(btnExtinde, scheme)

            ' Bara de progres e ea însăși IThemedControl, dar banda ASTA e la rândul ei una:
            ' ThemeManager nu recurge în copiii unui IThemedControl, deci schema trebuie
            ' împinsă mai departe cu mâna. Fără rândul ăsta bara ar rămâne pe culorile
            ' implicite la fiecare comutare (exact capcana din regulile casei).
            pbProgress.ApplyTheme(scheme)

            ' Culoarea pastilei de conexiune depinde de STARE, nu doar de schemă.
            ActualizeazaStarea()
            Invalidate()
        Catch ex As Exception
            ' Frontieră de UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("ForexeFooterView.ApplyTheme", ex)
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
