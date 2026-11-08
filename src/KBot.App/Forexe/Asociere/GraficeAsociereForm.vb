Option Strict On
Imports KBot.Common

''' <summary>
''' GRAFICELE ȘI BENZILE, ÎN FEREASTRA LOR (felia 0061).
'''
''' <para><b>De ce au plecat din formular.</b> Cerința operatorului din 10.09.2026: ecranul de
''' asociere e un ecran de LUCRU — doi arbori și, sub fiecare, indicatorii rândului ales. Graficul
''' și benzile sunt de PRIVIT: se deschid când vrei să te uiți, și atunci merită toată fereastra,
''' nu un sfert de ecran furat celor patru suprafețe cu care se lucrează.</para>
'''
''' <para><b>Arborele recepțiilor vine cu ele.</b> Cererea operatorului din 10.09.2026: graficul se
''' desface pe recepția ALEASĂ, deci într-o fereastră fără arbore nu se putea alege nimic — se
''' vedea ce fusese ales înainte de apăsarea butonului, și atât. Arborele e și celălalt capăt al
''' culorilor (fiecare punct poartă culoarea rândului lui) și al clicului pe punct, care
''' selectează rândul. La BENZI se strânge: acolo nu se alege, se trage.</para>
'''
''' <para><b>Nu are controale proprii, și nu din economie.</b> Și panoul cu cele trei suprafețe, și
''' arborele sunt declarate în <see cref="AsociereForm"/> și se MUTĂ aici la deschidere. Tabloul
''' local — cine pe ce stă — trăiește într-un singur loc, iar tratatorii graficului, ai benzilor și
''' ai arborelui sunt scriși acolo, lângă el. O a doua pereche de suprafețe ar însemna un al doilea
''' set de culori, de repere și de reguli de tragere, care s-ar putea abate de la primul. Aceeași
''' hotărâre ca la <see cref="AsociereBenziForm"/>, dusă până la capăt.</para>
'''
''' <para><b>Se închide întorcând ce a împrumutat.</b> Panoul și arborele se întorc în formular și
''' fereastra se poate deschide din nou fără ca nimic să fie reconstruit.</para>
''' </summary>
Public Class GraficeAsociereForm

    Private ReadOnly _parinte As AsociereForm

    ' Cele două controale împrumutate și locurile din care au fost luate. Locurile sunt
    ' obligatorii: fără ele, închiderea ar trebui să GHICEASCĂ unde se pun înapoi.
    Private ReadOnly _panou As Control
    Private ReadOnly _acasa As Control
    Private ReadOnly _arbore As Control
    Private ReadOnly _acasaArbore As Control

    Public Sub New(parinte As AsociereForm, panou As Control, acasa As Control,
                   arbore As Control, acasaArbore As Control)
        If parinte Is Nothing Then Throw New ArgumentNullException(NameOf(parinte))
        If panou Is Nothing Then Throw New ArgumentNullException(NameOf(panou))
        If acasa Is Nothing Then Throw New ArgumentNullException(NameOf(acasa))
        If arbore Is Nothing Then Throw New ArgumentNullException(NameOf(arbore))
        If acasaArbore Is Nothing Then Throw New ArgumentNullException(NameOf(acasaArbore))
        InitializeComponent()
        _parinte = parinte
        _panou = panou
        _acasa = acasa
        _arbore = arbore
        _acasaArbore = acasaArbore
    End Sub

    Private Sub GraficeAsociereForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            _acasa.Controls.Remove(_panou)
            splitGrafice.Panel2.Controls.Add(_panou)
            _panou.Dock = DockStyle.Fill
            _panou.Visible = True

            _acasaArbore.Controls.Remove(_arbore)
            splitGrafice.Panel1.Controls.Add(_arbore)
            _arbore.Dock = DockStyle.Fill
            _arbore.Visible = True

            ' Starea de pornire nu se ghicește: formularul-părinte știe pe care dintre cele două
            ' vederi stă banda de nume, iar arborele e de folos doar la una din ele.
            AratArborele(Not _parinte.VedereaEsteBenzi)
        Catch ex As Exception
            ' Graniță de UI: se loghează și se merge mai departe cu o fereastră goală, în loc să
            ' cadă deschiderea peste tot formularul de asociere.
            GlobalErrorLog.Write("GraficeAsociereForm.GraficeAsociereForm_Load", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arată sau strânge arborele recepțiilor. Îl cheamă <see cref="AsociereForm"/> de fiecare
    ''' dată când banda de nume trece de pe grafic pe benzi și înapoi.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Strâns, nu ascuns.</b> <c>Panel1Collapsed</c> ia și despărțitorul, deci benzile
    ''' primesc toată fereastra; cu <c>Visible = False</c> ar fi rămas o coloană goală și o dungă
    ''' de tras în stânga lor.</para>
    ''' <para>Lățimea aleasă de operator NU se pierde: <c>SplitterDistance</c> rămâne ce era cât
    ''' panoul e strâns, deci întoarcerea pe grafic aduce arborele la loc, la fel de lat.</para>
    ''' </remarks>
    Public Sub AratArborele(vizibil As Boolean)
        Try
            splitGrafice.Panel1Collapsed = Not vizibil
        Catch ex As Exception
            GlobalErrorLog.Write("GraficeAsociereForm.AratArborele", ex)
        End Try
    End Sub

    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    ''' <summary>
    ''' Panoul se întoarce ACASĂ ascuns, arborele se întoarce la vedere.
    ''' </summary>
    ''' <remarks>
    ''' Obligatoriu, nu igienă: amândouă sunt controale ale formularului-părinte, iar
    ''' <c>Form.Dispose</c> aruncă tot ce mai are în <c>Controls</c>. Lăsate aici, ar fi aruncate
    ''' împreună cu fereastra, iar a doua apăsare a butonului ar deschide o fereastră peste
    ''' controale moarte — și ecranul de lucru ar rămâne fără arborele lui.
    ''' </remarks>
    Private Sub GraficeAsociereForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            splitGrafice.Panel2.Controls.Remove(_panou)
            _panou.Visible = False
            _acasa.Controls.Add(_panou)
            ' Înapoi în spatele celorlalți: panoul e ascuns, dar dacă ar sta deasupra în ordinea
            ' Z, ar fura andocarea celor de sub el la următoarea aranjare.
            _panou.SendToBack()

            ' Arborele, în schimb, se întoarce LA VEDERE: acasă e jumătatea de sus a stângii, și
            ' de acolo a plecat vizibil.
            splitGrafice.Panel1.Controls.Remove(_arbore)
            _acasaArbore.Controls.Add(_arbore)
            _arbore.Dock = DockStyle.Fill
            _arbore.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("GraficeAsociereForm.GraficeAsociereForm_FormClosed", ex)
        End Try
    End Sub
End Class
