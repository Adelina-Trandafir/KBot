Option Strict On
Imports System.Security.Cryptography.X509Certificates
Imports KBot.Common
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' «FOREXE» (slice 0072): what the robot is doing now, the certificate it will propose
''' next time (and the button that forgets it), the docked-browser toolbar switch, and the
''' folders it reads and writes (read-only here -- they are edited on «Aplicație»).
'''
''' <para>Reads the state from <see cref="ForexeController"/> and follows its
''' <c>StateChanged</c> while the page is alive, so a connection made while the window is
''' open shows up without reopening it.</para>
''' </summary>
Public Class SetariForexeView
    Implements ISetariView, IThemedContainer

    Private ReadOnly _controller As ForexeController

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Private _suppress As Boolean

    Public Sub New(controller As ForexeController)
        ArgumentNullException.ThrowIfNull(controller)
        InitializeComponent()
        _controller = controller
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
        ' The controller is a singleton and outlives the page: unhook, or it keeps us alive.
        AddHandler Me.Disposed, Sub() RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "forexe"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            ActualizeazaStarea()
            ActualizeazaCertificatulMemorat()
            _suppress = True
            Try
                chkHideChrome.Checked = AppSettings.Current.ForexeHideBrowserChrome
            Finally
                _suppress = False
            End Try
            ' The labels ellipsize a long path; the full one is in the tooltip.
            AratatCalea(lblWorkflows, KBotPaths.FolderWorkflows)
            AratatCalea(lblRezultate, KBotPaths.FolderRezultateWorkflow)
            AratatCalea(lblExtrase, KBotPaths.FolderExtrase)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.Activated", ex)
        End Try
    End Sub

    Private Sub AratatCalea(eticheta As Label, cale As String)
        eticheta.Text = cale
        tips.SetToolTipHeader(eticheta, "Folder")
        tips.SetToolTipText(eticheta, cale)
    End Sub

    ' ---------------- state ----------------

    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ActualizeazaStarea))
            Else
                ActualizeazaStarea()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.Controller_StateChanged", ex)
        End Try
    End Sub

    Private Sub ActualizeazaStarea()
        Try
            Dim conectat As Boolean = _controller.IsConnected
            lblConexiune.Text = If(conectat,
                                   If(_controller.IsBusy, "Conectat — o operație este în curs", "Conectat"),
                                   "Neconectat")
            Dim p As ThemePalette = ThemeManager.Current.Palette
            lblConexiune.ForeColor = If(conectat, p.SuccessColor, p.TextDimColor)
            Dim nume As String = _controller.CertificateName
            lblCertificat.Text = If(String.IsNullOrEmpty(nume), "— (se alege la conectare)", nume)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.ActualizeazaStarea", ex)
        End Try
    End Sub

    ' ---------------- remembered certificate ----------------

    Private Sub ActualizeazaCertificatulMemorat()
        Try
            Dim cert As X509Certificate2 = CertificateService.LoadLastUsedCertificate()
            If cert Is Nothing Then
                lblCertMemorat.Text = "Niciunul — se alege la prima conectare."
                btnUitaCertificat.Enabled = False
                Return
            End If
            Using cert
                lblCertMemorat.Text = cert.GetNameInfo(X509NameType.SimpleName, False) &
                                      " · expiră " & cert.NotAfter.ToString("dd.MM.yyyy", Globalization.CultureInfo.InvariantCulture)
            End Using
            btnUitaCertificat.Enabled = True
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.ActualizeazaCertificatulMemorat", ex)
            lblCertMemorat.Text = "Nu s-a putut citi certificatul memorat. Detalii în jurnalul de erori."
            btnUitaCertificat.Enabled = True
        End Try
    End Sub

    Private Sub BtnUitaCertificat_Click(sender As Object, e As EventArgs) Handles btnUitaCertificat.Click
        Try
            If KBotMessage.Show(FindForm(),
                    "Certificatul memorat se șterge de pe acest calculator; la următoarea conectare îl alegi din nou. Continui?",
                    "Uită certificatul", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim sters As Boolean = CertificateService.ForgetLastUsedCertificate()
            ActualizeazaCertificatulMemorat()
            RaiseEvent StatusChanged(If(sters, "Certificatul memorat a fost șters.", "Nu era niciun certificat memorat."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.BtnUitaCertificat_Click", ex)
            RaiseEvent StatusChanged("Certificatul nu a putut fi șters: " & ex.Message)
        End Try
    End Sub

    ' ---------------- browser ----------------

    Private Sub ChkHideChrome_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideChrome.CheckedChanged
        Try
            If _suppress Then Return
            Dim copie As AppSettings = AppSettings.Current.Clone()
            copie.ForexeHideBrowserChrome = chkHideChrome.Checked
            copie.Save()
            RaiseEvent StatusChanged(If(chkHideChrome.Checked,
                                        "Bara browserului rămâne în afara panoului. Se aplică de la următoarea lucrare.",
                                        "Bara browserului se vede în panou. Se aplică de la următoarea lucrare."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.ChkHideChrome_CheckedChanged", ex)
            RaiseEvent StatusChanged("Setarea nu a putut fi salvată: " & ex.Message)
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyStare.BackColor = p.SurfaceAltColor
            tlyCertificat.BackColor = p.SurfaceAltColor
            tlyBrowser.BackColor = p.SurfaceAltColor
            tlyFoldere.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblConexiuneCaption, lblCertificatCaption, lblCertMemoratCaption,
                                                      lblWorkflowsCaption, lblRezultateCaption, lblExtraseCaption, lblFoldereHint}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnUitaCertificat, scheme)
            ' The connection colour depends on the STATE, not only on the scheme.
            ActualizeazaStarea()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForexeView.ApplyTheme", ex)
        End Try
    End Sub

End Class
