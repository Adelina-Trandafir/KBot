Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Formularul de pornire al migratorului: adresa serverului, cheia API, și proba
''' că amândouă sunt bune — lista bazelor de unitate de pe MariaDB.
'''
''' De ce ecranul ăsta există: fără el, prima greșeală de cheie sau de adresă ar
''' apărea abia la prima operație lungă, după ce operatorul a ales deja DC-ul,
''' anul și fișierul. Aici e o cerere mică, iar răspunsul ei e chiar lista din
''' care se alege pe ecranul următor.
'''
''' Clientul construit aici trece mai departe la <see cref="MigratorForm"/>, care
''' îl și eliberează.
''' </summary>
Public Class ConnectForm

    ''' <summary>Variabila de mediu din care se preia cheia, dacă e pusă.</summary>
    Public Const ApiKeyEnvVar As String = "KBOT_SEED_API_KEY"

    Private _client As MigrareApiClient
    Private _baze As List(Of BazaInfo)
    Private _busy As Boolean

    ''' <summary>Clientul conectat. Valid doar după DialogResult.OK.</summary>
    Public ReadOnly Property Client As MigrareApiClient
        Get
            Return _client
        End Get
    End Property

    ''' <summary>Bazele citite la conectare. Valide doar după DialogResult.OK.</summary>
    Public ReadOnly Property Baze As List(Of BazaInfo)
        Get
            Return _baze
        End Get
    End Property

    Public Sub New()
        InitializeComponent()

        Try
            txtServer.Text = ApiOptions.DefaultBaseUrl
            Dim key As String = Environment.GetEnvironmentVariable(ApiKeyEnvVar)
            If Not String.IsNullOrWhiteSpace(key) Then txtCheie.Text = key
        Catch ex As Exception
            ' Granita UI (constructor de formular): un throw ar impiedica deschiderea.
            GlobalErrorLog.Write("ConnectForm.New", ex)
        End Try
    End Sub

    Private Async Sub btnConecteaza_Click(sender As Object, e As EventArgs) Handles btnConecteaza.Click
        Try
            If _busy Then Return
            If String.IsNullOrWhiteSpace(txtServer.Text) Then
                lblStare.Text = "Completează adresa serverului."
                Return
            End If
            If String.IsNullOrWhiteSpace(txtCheie.Text) Then
                lblStare.Text = "Completează cheia API. Rutele de migrare nu folosesc token bearer."
                Return
            End If

            SetBusy(True, "Se conectează…")
            DisposeClient()
            lstBaze.Items.Clear()
            btnContinua.Enabled = False

            Dim client As New MigrareApiClient(txtCheie.Text.Trim(), txtServer.Text.Trim())
            Dim baze As List(Of BazaInfo) = Await client.GetBazeAsync()

            _client = client
            _baze = baze

            For Each b As BazaInfo In baze
                lstBaze.Items.Add(b)
            Next

            If baze.Count = 0 Then
                lblStare.Text = "Conectat, dar serverul nu vede nicio bază de unitate. " &
                                "Verifică ce cont folosește API-ul."
            Else
                Dim incomplete As Integer = 0
                For Each b As BazaInfo In baze
                    If Not b.Complet Then incomplete += 1
                Next
                lblStare.Text = "Conectat la " & client.BaseUrl & ". " &
                                baze.Count.ToString() & " baze de unitate" &
                                If(incomplete > 0,
                                   ", dintre care " & incomplete.ToString() &
                                   " fără toate tabelele FX_ (migrarea nu creează tabele).",
                                   ", toate cu tabelele FX_ instalate.")
                btnContinua.Enabled = True
                AcceptButton = btnContinua
            End If

        Catch ex As Exception
            GlobalErrorLog.Write("ConnectForm.btnConecteaza_Click", ex)
            DisposeClient()
            _baze = Nothing
            btnContinua.Enabled = False
            lblStare.Text = "Conectarea a eșuat: " & ex.Message
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub btnContinua_Click(sender As Object, e As EventArgs) Handles btnContinua.Click
        Try
            If _client Is Nothing Then Return
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ConnectForm.btnContinua_Click", ex)
        End Try
    End Sub

    Private Sub Camp_TextChanged(sender As Object, e As EventArgs) _
            Handles txtServer.TextChanged, txtCheie.TextChanged
        Try
            ' Datele s-au schimbat dupa conectare: legatura de dinainte nu mai
            ' raspunde pentru ele, deci nu se merge mai departe pe ea.
            If _client Is Nothing Then Return
            DisposeClient()
            _baze = Nothing
            lstBaze.Items.Clear()
            btnContinua.Enabled = False
            AcceptButton = btnConecteaza
            lblStare.Text = "Datele s-au schimbat — conectează-te din nou."
        Catch ex As Exception
            GlobalErrorLog.Write("ConnectForm.Camp_TextChanged", ex)
        End Try
    End Sub

    Private Sub ConnectForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            ' Daca nu s-a mers mai departe, clientul nu are cine sa-l elibereze.
            If DialogResult <> DialogResult.OK Then DisposeClient()
        Catch ex As Exception
            GlobalErrorLog.Write("ConnectForm.ConnectForm_FormClosed", ex)
        End Try
    End Sub

    Private Sub DisposeClient()
        If _client Is Nothing Then Return
        _client.Dispose()
        _client = Nothing
    End Sub

    Private Sub SetBusy(busy As Boolean, message As String)
        _busy = busy
        btnConecteaza.Enabled = Not busy
        txtServer.Enabled = Not busy
        txtCheie.Enabled = Not busy
        If message IsNot Nothing Then lblStare.Text = message
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

End Class
