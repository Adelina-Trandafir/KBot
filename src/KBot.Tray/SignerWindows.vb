Option Strict On
Imports System.Windows.Forms
Imports KBot.App
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The signer edition (slice TRAY-02): the windows where the user sees and signs the documents, opened by the tray itself.
''' They are the windows of K-BOT (<see cref="DirectorForm"/> for a director, <see cref="DeSemnatRestransForm"/> for a Secretar or an
''' Administrator), taken from <c>KBot.App.dll</c>; there is no <c>KBot.App.exe</c> next to the tray, so nothing else of K-BOT can be
''' opened from here. They talk to the server through the tray's own session (<see cref="TrayChecker"/>).
''' </summary>
Friend NotInheritable Class SignerWindows

    Private Const Caption As String = "K-BOT — documente de semnat"

    Private ReadOnly _checker As TrayChecker
    Private _open As Boolean
    Private _director As DirectorForm

    Friend Sub New(k_checker As TrayChecker)
        ArgumentNullException.ThrowIfNull(k_checker)
        _checker = k_checker
    End Sub

    ''' <summary>Is a signing window open? The tray does not read the list or sign out underneath it.</summary>
    Friend ReadOnly Property IsOpen As Boolean
        Get
            Return _open
        End Get
    End Property

    ''' <summary>
    ''' Opens the window of the signed-in role, with the revision <paramref name="k_idrev"/> picked when it is not 0 (the click on a row
    ''' of the message above the clock). A window already open is only pointed at that revision. Returns when the window is closed.
    ''' UI boundary: logged and shown.
    ''' </summary>
    Friend Sub Open(k_idrev As Integer)
        Try
            TrayLog.Write("window", $"open requested, revision {k_idrev}, already open={_open}")
            If _open Then
                If _director IsNot Nothing AndAlso k_idrev > 0 Then _director.SelecteazaRevizia(k_idrev)
                Return
            End If
            Dim k_session As SessionContext = _checker.Session
            If Not k_session.IsAuthenticated Then
                TrayLog.Write("window", "not opened: the tray has no signed-in session")
                KBotMessage.Show("Autentificați-vă mai întâi: faceți clic pe iconiță.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _open = True
            Try
                TrayLog.Write("window", $"role {k_session.Role} on unit {k_session.DbName}")
                If DeSemnatRestransForm.EsteRolRestrans(k_session) Then
                    TrayLog.Write("window", "opening the restricted window (Secretar / Administrator)")
                    Using k_form As New DeSemnatRestransForm(_checker.Api, k_session, AddressOf NewSignIn)
                        k_form.ShowDialog()
                    End Using
                ElseIf DirectorForm.EsteDirector(k_session) Then
                    TrayLog.Write("window", "opening the director's window")
                    Using k_form As New DirectorForm(_checker.Api, k_session, _checker.Auth, AddressOf NewSignIn)
                        k_form.IdrevInitial = k_idrev
                        _director = k_form
                        k_form.ShowDialog()
                    End Using
                Else
                    TrayLog.Write("window", "not opened: the role has no list in this program")
                    KBotMessage.Show("Rolul dvs. nu are documente de semnat în această aplicație.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Finally
                _director = Nothing
                _open = False
                TrayLog.Write("window", "the window was closed")
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("SignerWindows.Open", ex)
            KBotMessage.Show("Fereastra documentelor de semnat nu a putut fi deschisă. Detalii în jurnalul de erori.", Caption,
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' The sign-in window the signing windows open when the session expired or a document of another unit asks for it: the same window as
    ' K-BOT's, but without the K-BOT update service (Nothing) -- it only signs in, on the tray's session.
    Private Function NewSignIn() As LoginForm
        Return New LoginForm(_checker.Auth, _checker.Session, Nothing)
    End Function
End Class
