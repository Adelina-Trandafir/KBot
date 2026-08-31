Option Strict On
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms

''' <summary>
''' ETICHETELE BUTOANELOR arborelui (felia 0035): fiecare zonă apăsabilă din antet și din subsol
''' își poate spune, la survolare, la ce folosește.
'''
''' <para><b>De ce nu un <c>ToolTip</c> din WinForms.</b> Butoanele astea nu sunt controale — sunt
''' zone pictate în interiorul arborelui. <c>ToolTip</c> extinde CONTROALE, deci n-ar avea ce
''' extinde aici, iar dacă i s-ar da tot arborele, ar arăta același text peste toată suprafața
''' lui. Eticheta K-BOT (<see cref="KBotToolTip"/>) are, tocmai pentru asta,
''' <see cref="KBotToolTip.ShowAt"/>: se cheamă cu un conținut și o poziție, exact atunci când
''' survolarea intră într-o zonă anume.</para>
'''
''' <para><b>Textele sunt pe MAI MULTE RÂNDURI</b> (editor multilinie în grila de proprietăți) și
''' acceptă marcajele de text îmbogățit — primul rând poate fi un titlu îngroșat, restul o
''' explicație. Un buton fără text nu arată nimic: tăcerea e implicitul.</para>
'''
''' <para><b>Eticheta arborelui e a LUI</b> (<see cref="ButtonTooltip"/>), nu una împrumutată de
''' la formular: așa butoanele de arbore pot arăta altfel decât cele de formular, ceea ce e chiar
''' cerința — mai multe înfățișări pe același ecran. Cine vrea una singură peste tot îi poate
''' copia stilul.</para>
''' </summary>
Partial Public Class AdvancedTreeControl

    ' Componenta care arată etichetele butoanelor. Se creează leneș, la prima nevoie, și
    ' NICIODATĂ în designer: ar însemna o fereastră deschisă în Visual Studio.
    Private _butonTooltip As KBotToolTip

    ' Ce buton e „în etichetă" acum. Fără el, fiecare pixel de mișcare peste același buton ar
    ' reprograma apariția, iar eticheta n-ar apărea niciodată.
    Private _tipButonCurent As String = Nothing
    Private ReadOnly _tipContinut As New KBotToolTipContent()

    ''' <summary>
    ''' Eticheta plutitoare cu care arborele își explică butoanele. Se poate îmbrăca din grila de
    ''' proprietăți (<c>ButtonTooltip.Style.…</c>) — culori, antet, subsol, linie despărțitoare.
    ''' </summary>
    <Category("K-BOT: Buttons")>
    <Description("Eticheta plutitoare care explică butoanele din antet și din subsol.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property ButtonTooltip As KBotToolTip
        Get
            If _butonTooltip Is Nothing Then _butonTooltip = New KBotToolTip()
            Return _butonTooltip
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' TEXTELE — unul pentru fiecare buton desenat
    ' ══════════════════════════════════════════════════════════════════════════

    Private _tipHeaderSearchIcon As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta iconiței de căutare din antet (mai multe rânduri; acceptă <b>, <color=#…>).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property HeaderSearchIconTooltip As String
        Get
            Return _tipHeaderSearchIcon
        End Get
        Set(value As String)
            _tipHeaderSearchIcon = If(value, String.Empty)
        End Set
    End Property

    Private _tipHeaderRightIcon As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta iconiței din dreapta antetului (mai multe rânduri).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property HeaderRightIconTooltip As String
        Get
            Return _tipHeaderRightIcon
        End Get
        Set(value As String)
            _tipHeaderRightIcon = If(value, String.Empty)
        End Set
    End Property

    Private _tipFooterLeftIcon As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta iconiței din stânga subsolului (mai multe rânduri).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property FooterLeftIconTooltip As String
        Get
            Return _tipFooterLeftIcon
        End Get
        Set(value As String)
            _tipFooterLeftIcon = If(value, String.Empty)
        End Set
    End Property

    Private _tipFooterRightIcon As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta iconiței din dreapta subsolului (mai multe rânduri).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property FooterRightIconTooltip As String
        Get
            Return _tipFooterRightIcon
        End Get
        Set(value As String)
            _tipFooterRightIcon = If(value, String.Empty)
        End Set
    End Property

    ''' <summary>
    ''' Eticheta butonului de strângere. Are DOUĂ texte, fiindcă butonul are două înțelesuri:
    ''' „strânge" cât e desfăcut și „desfă" cât e strâns. Un singur text ar minți jumătate din timp.
    ''' </summary>
    Private _tipCollapseButton As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta butonului de strângere, cât arborele e DESFĂCUT (mai multe rânduri).")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property CollapseButtonTooltip As String
        Get
            Return _tipCollapseButton
        End Get
        Set(value As String)
            _tipCollapseButton = If(value, String.Empty)
        End Set
    End Property

    Private _tipExpandButton As String = String.Empty
    <Category("K-BOT: Buttons")>
    <Description("Eticheta butonului de strângere, cât arborele e STRÂNS. Gol = același text ca la desfăcut.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property ExpandButtonTooltip As String
        Get
            Return _tipExpandButton
        End Get
        Set(value As String)
            _tipExpandButton = If(value, String.Empty)
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' AFIȘAREA
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Cere eticheta butonului identificat prin <paramref name="cheie"/> (o etichetă internă
    ''' stabilă, nu textul). Aceeași cheie de două ori la rând nu face nimic — eticheta rămâne
    ''' unde e. O cheie nouă sau <c>Nothing</c> stinge ce era.
    ''' </summary>
    Private Sub ShowButtonTip(cheie As String, text As String)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If String.Equals(cheie, _tipButonCurent, StringComparison.Ordinal) Then Return
            _tipButonCurent = cheie

            If String.IsNullOrEmpty(cheie) OrElse String.IsNullOrEmpty(text) Then
                _butonTooltip?.HideNow()
                Return
            End If

            _tipContinut.Text = text
            ButtonTooltip.ShowAt(Me, _tipContinut, Cursor.Position)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ShowButtonTip", ex)
        End Try
    End Sub

    ''' <summary>Stinge eticheta de buton (cursorul a plecat de pe orice buton).</summary>
    Friend Sub HideButtonTip()
        ShowButtonTip(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Decide, din stările de survolare deja calculate, ce etichetă se cuvine. Un singur loc: cele
    ''' patru iconițe și butonul de strângere nu pot fi survolate în același timp, deci nici nu
    ''' pot cere două etichete deodată.
    ''' </summary>
    Friend Sub RefreshButtonTip()
        Try
            If _headerSearchIconHover Then
                ShowButtonTip("hdr.search", _tipHeaderSearchIcon)
            ElseIf _headerRightIconHover Then
                ShowButtonTip("hdr.right", _tipHeaderRightIcon)
            ElseIf _footerLeftIconHover Then
                ShowButtonTip("ftr.left", _tipFooterLeftIcon)
            ElseIf _footerRightIconHover Then
                ShowButtonTip("ftr.right", _tipFooterRightIcon)
            ElseIf _footerButtonHover Then
                Dim t As String = If(_collapsed AndAlso Not String.IsNullOrEmpty(_tipExpandButton),
                                     _tipExpandButton, _tipCollapseButton)
                ShowButtonTip(If(_collapsed, "ftr.expand", "ftr.collapse"), t)
            Else
                HideButtonTip()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.RefreshButtonTip", ex)
        End Try
    End Sub

End Class
