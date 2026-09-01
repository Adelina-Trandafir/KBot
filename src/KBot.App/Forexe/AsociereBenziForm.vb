Option Strict On
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' BENZILE DE AȘEZARE, LA MĂRIME (felia 0048-07).
'''
''' <para><b>Nu e o a doua funcție, e aceeași suprafață mai mare.</b> Banda strâmtă din
''' <see cref="AsociereForm"/> e compactă tocmai fiindcă trebuie să încapă douăzeci de benzi a
''' câte douăzeci de marcaje — și la scara aia nu mai încape text. Aici încape: denumirile
''' benzilor, sumele de lângă marcaje, datele pe axă. Datele sunt aceleași, construcția e aceeași
''' metodă, iar tragerea trece prin ACEIAȘI trei tratatori.</para>
'''
''' <para><b>De ce nu are date proprii.</b> Tabloul local — cine pe ce stă — trăiește într-un
''' singur loc, în dicționarele lui <see cref="AsociereForm"/>. Fereastra asta împrumută
''' construcția (<c>UmpleBenzile</c>) și tratatorii (<c>LeagaBanda</c>), deci nu există o a doua
''' regulă de așezare care s-ar putea abate de la prima. La închidere nu e nimic de împăcat:
''' n-au fost niciodată două tablouri.</para>
'''
''' <para><b>Fără buton de salvare</b>, deliberat. D-H spune o singură salvare, la sfârșit, iar
''' aceea stă în formularul-părinte. Un al doilea buton aici ar însemna două momente în care
''' pleacă ceva spre server, adică exact ce D-H a hotărât să nu existe.</para>
''' </summary>
Public Class AsociereBenziForm

    Private ReadOnly _parinte As AsociereForm

    ' Dicționarele benzii ASTEIA. Separate de cele ale formularului fiindcă marcajele sunt alte
    ' obiecte — aceleași fapte, altă suprafață — iar pasul de culori trebuie să le nimerească pe
    ' ale lui, nu pe ale ferestrei de dedesubt.
    Private ReadOnly _banda As New Dictionary(Of Integer, KBotLane)
    Private ReadOnly _marcaj As New Dictionary(Of Integer, KBotLaneMarker)

    Public Sub New(parinte As AsociereForm)
        If parinte Is Nothing Then Throw New ArgumentNullException(NameOf(parinte))
        InitializeComponent()
        _parinte = parinte
    End Sub

    Private Sub AsociereBenziForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ' Tratatorii părintelui MAI ÎNTÂI, ai noștri după: la o aruncare, ai lui scriu în
            ' tabloul local și reconstruiesc banda strâmtă, iar al nostru — care rulează după —
            ' reconstruiește suprafața asta din tabloul deja actualizat. Ordinea abonării e
            ' ordinea execuției, deci ea ține totul în picioare.
            _parinte.LeagaBanda(benziMari)
            AddHandler benziMari.MarkerDropped, AddressOf DupaAruncare
            Reincarca()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereBenziForm.AsociereBenziForm_Load", ex)
        End Try
    End Sub

    Private Sub Reincarca()
        _parinte.UmpleBenzile(benziMari, _banda, _marcaj)
    End Sub

    Private Sub DupaAruncare(sender As Object, e As LaneDropEventArgs)
        Try
            Reincarca()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereBenziForm.DupaAruncare", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Tema s-a schimbat, deci fiecare culoare pe care fereastra asta a copiat-o din paleta veche
    ''' e acum cea greșită.
    ''' </summary>
    ''' <remarks>
    ''' Tema ajunge singură la controale; ce nu ajunge e o culoare COPIATĂ din paletă într-o bandă
    ''' sau într-un marcaj — alea sunt valori, nu legături, și nimic nu se întoarce să le
    ''' corecteze. Reconstruirea le cere din nou pe toate. Aceeași grijă ca în
    ''' <c>AsociereForm.OnThemeChanged</c>.
    ''' </remarks>
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Reincarca()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereBenziForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    ''' <summary>
    ''' Tratatorii părintelui se desprind la închidere.
    ''' </summary>
    ''' <remarks>
    ''' Obligatoriu, nu igienă: <c>AsociereForm</c> trăiește mai mult decât fereastra asta, iar un
    ''' abonament rămas în urmă ar ține în viață o suprafață închisă și ar chema tratatori pe un
    ''' control aruncat la următoarea deschidere.
    ''' </remarks>
    Private Sub AsociereBenziForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            RemoveHandler benziMari.MarkerDropped, AddressOf DupaAruncare
            _parinte.DezleagaBanda(benziMari)
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereBenziForm.AsociereBenziForm_FormClosed", ex)
        End Try
    End Sub
End Class
