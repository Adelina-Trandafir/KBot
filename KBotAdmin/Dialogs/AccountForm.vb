Option Strict On

''' <summary>
''' Adds an account to a database (e-mail, database, K-BOT role); with the e-mail fixed and the database fixed too it
''' changes the role of an existing link; with the e-mail fixed and <c>k_grant</c> it gives that existing account
''' access to one more database (slice ME01).
''' </summary>
Public Class AccountForm

    Private ReadOnly _roles As IReadOnlyList(Of String)
    Private ReadOnly _databases As IReadOnlyList(Of String)

    ''' <summary>The e-mail typed (or fixed).</summary>
    Public ReadOnly Property Un As String
        Get
            Return txtUn.Text.Trim()
        End Get
    End Property

    ''' <summary>The database chosen (or fixed).</summary>
    Public ReadOnly Property Db As String
        Get
            Return If(cmbDb.SelectedIndex >= 0, _databases(cmbDb.SelectedIndex), String.Empty)
        End Get
    End Property

    ''' <summary>The K-BOT role chosen, as the server names it (e.g. Contabil).</summary>
    Public ReadOnly Property Rol As String
        Get
            Return If(cmbRol.SelectedIndex >= 0, _roles(cmbRol.SelectedIndex), String.Empty)
        End Get
    End Property

    ''' <param name="k_roles">The K-BOT role names the server knows.</param>
    ''' <param name="k_databases">The databases to choose from.</param>
    ''' <param name="k_fixedUn">Empty for a new link; the e-mail for a role change.</param>
    ''' <param name="k_preselectDb">The database to preselect (and to fix, for a role change).</param>
    ''' <param name="k_currentRole">The role to preselect.</param>
    ''' <param name="k_grant">With an e-mail: the database stays choosable (the account gets access to one more database).</param>
    Public Sub New(k_roles As IReadOnlyList(Of String), k_databases As IReadOnlyList(Of String),
                   k_fixedUn As String, k_preselectDb As String, k_currentRole As String,
                   Optional k_grant As Boolean = False)
        InitializeComponent()
        Icon = AdminIcon.Value
        _roles = k_roles
        _databases = k_databases
        For Each k_name As String In k_roles
            cmbRol.Items.Add(k_name)
        Next
        For Each k_name As String In k_databases
            cmbDb.Items.Add(k_name)
        Next
        Dim k_index As Integer = k_databases.ToList().FindIndex(Function(k_d) String.Equals(k_d, k_preselectDb, StringComparison.OrdinalIgnoreCase))
        cmbDb.SelectedIndex = If(k_index >= 0, k_index, If(k_databases.Count > 0, 0, -1))
        Dim k_roleIndex As Integer = k_roles.ToList().FindIndex(Function(k_r) String.Equals(k_r, k_currentRole, StringComparison.OrdinalIgnoreCase))
        cmbRol.SelectedIndex = If(k_roleIndex >= 0, k_roleIndex, If(k_roles.Count > 0, 0, -1))
        If k_fixedUn <> String.Empty Then
            txtUn.Text = k_fixedUn
            txtUn.ReadOnly = True
            cmbDb.Enabled = k_grant
            If k_grant Then
                Text = "Dă acces la o bază"
                lblInfo.Text = "Se acordă accesul și se sincronizează MariaDB pentru toate bazele contului. Drepturile ADECHIT / VERCON se dau separat."
            Else
                Text = "Schimbă rolul K-BOT"
                lblInfo.Text = "Se schimbă rolul K-BOT și se sincronizează privilegiile MariaDB. Contabil: toate datele bazelor lui, fără administrare de server sau structură."
            End If
        Else
            Text = "Adaugă cont la bază"
            lblInfo.Text = "Contul trebuie să existe deja în MariaDB. Legătura și rolul K-BOT determină și privilegiile SQL; drepturile vechi, mai largi, se retrag."
        End If
    End Sub

    Private Sub BtnOk_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If Un = String.Empty OrElse Not Un.Contains("@", StringComparison.Ordinal) Then
                KBotMessage.Show("Scrieți adresa de e-mail a contului.", "Cont", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Db = String.Empty OrElse Rol = String.Empty Then
                KBotMessage.Show("Alegeți baza și rolul.", "Cont", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AccountForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
