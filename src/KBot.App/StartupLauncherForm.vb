Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Fereastra de pornire (Debug): înlocuiește dialogul Da/Nu cu care se alegea până acum între
''' autentificare și bancul de probă.
'''
''' <para>Un <c>MessageBox</c> cu două butoane putea purta exact două opțiuni, iar textul lor era
''' «Da» și «Nu» — adică alegerea trebuia explicată într-un paragraf deasupra. Aici pornirile sunt
''' rânduri într-un <c>KBotNavList</c>: se citesc dintr-o privire, se adaugă altele fără să se
''' schimbe nimic altceva, și fereastra e tematizată ca restul aplicației (dialogul de sistem nu
''' era).</para>
'''
''' <para>Rezultatul se citește din <see cref="Alegere"/> după <c>ShowDialog</c>:
''' <c>DialogResult.OK</c> = una din cheile <see cref="KEY_APLICATIE"/> / <see cref="KEY_BANC"/> /
''' <see cref="KEY_JURNALE"/>; orice altceva înseamnă că operatorul a renunțat și procesul trebuie
''' să iasă fără să deschidă nimic.</para>
''' </summary>
Public Class StartupLauncherForm

    ''' <summary>Aplicația reală: autentificare, apoi shell-ul (MainForm).</summary>
    Public Const KEY_APLICATIE As String = "aplicatie"

    ''' <summary>Bancul de probă (DevHarnessForm).</summary>
    Public Const KEY_BANC As String = "banc"

    ''' <summary>Vizualizatorul de jurnale, singur — fără autentificare și fără shell.</summary>
    Public Const KEY_JURNALE As String = "jurnale"

    ' Pornirile, în ordinea afișării. Un rând nou se adaugă AICI și nicăieri altundeva; dispecerul
    ' din Program.Main aruncă pe o cheie pe care n-o cunoaște, deci o pornire uitată se vede imediat.
    Private Shared ReadOnly PORNIRI As (Key As String, Text As String)() = {
        (KEY_APLICATIE, "Aplicația — autentificare, apoi K-BOT"),
        (KEY_BANC, "Banc de probă — teste și playground-uri"),
        (KEY_JURNALE, "Jurnale — vizualizatorul de jurnale")}

    ''' <summary>Cheia pornirii alese, sau <c>Nothing</c> dacă fereastra a fost respinsă.</summary>
    Public ReadOnly Property Alegere As String

    Public Sub New()
        InitializeComponent()
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' Iconița e cosmetică — lipsa ei nu are voie să oprească pornirea aplicației.
            GlobalErrorLog.Write("StartupLauncherForm.New", ex)
        End Try

        For Each p In PORNIRI
            navPorniri.AddItem(p.Key, p.Text)
        Next
        ' Prima pornire e cea obișnuită și e selectată din start: cine apasă Enter fără să citească
        ' primește aplicația, nu bancul de probă.
        navPorniri.SelectedKey = KEY_APLICATIE
        _Alegere = KEY_APLICATIE
    End Sub

    Private Sub navPorniri_SelectionChanged(key As String) Handles navPorniri.SelectionChanged
        Try
            _Alegere = key
            btnPorneste.Enabled = Not String.IsNullOrEmpty(key)
        Catch ex As Exception
            GlobalErrorLog.Write("StartupLauncherForm.navPorniri_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub btnPorneste_Click(sender As Object, e As EventArgs) Handles btnPorneste.Click
        Try
            If String.IsNullOrEmpty(_Alegere) Then Return
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("StartupLauncherForm.btnPorneste_Click", ex)
        End Try
    End Sub

    ' Renunțarea trebuie să lase Alegere GOL: altfel un apelant care se uită doar la proprietate,
    ' fără să verifice DialogResult, ar porni ultima pornire survolată.
    Private Sub btnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        _Alegere = Nothing
    End Sub

End Class
