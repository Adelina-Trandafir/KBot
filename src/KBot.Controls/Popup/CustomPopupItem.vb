Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' Un element al unui <see cref="CustomPopup"/>: fie un rând (cheie, text, pictogramă,
''' <see cref="Enabled"/>), fie un separator (<see cref="IsSeparator"/>), niciodată amândouă.
'''
''' UN SINGUR tip poartă ambele roluri, ca la <c>KBotNavItem</c> și din exact același motiv: două
''' tipuri într-o colecție ordonată ar cere un editor de colecții propriu, iar un editor propriu ar
''' cere un assembly de design-time compilat pe Microsoft.WinForms.Designer.SDK. Prețul e
''' <see cref="IsSeparator"/> ca steag și un <see cref="ToString"/> care face diferența vizibilă.
'''
''' <para><b>Textul poartă litera de acces.</b> «&amp;Salvează» se desenează «Salvează» cu S
''' subliniat și răspunde la tasta S; «&amp;&amp;» e un ampersand literal. Regula e a Windows-ului
''' și stă în <see cref="PopupMnemonic"/>.</para>
'''
''' Elementul NU deține <see cref="Image"/> și nu o eliberează niciodată — e a apelantului sau a
''' resurselor formularului, exact ca la <c>KBotNavItem.Image</c> și <c>KBotCaptionBar.IconImage</c>.
''' </summary>
Public NotInheritable Class CustomPopupItem

    ''' <summary>Constructor fără parametri — cerut de editorul de colecții al designerului.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Comoditate pentru cod: rând cu cheie și text.</summary>
    Public Sub New(key As String, text As String)
        Me.New(key, text, Nothing)
    End Sub

    ''' <summary>Comoditate pentru cod: rând cu cheie, text și pictogramă.</summary>
    Public Sub New(key As String, text As String, image As Image)
        ' «Me.» e OBLIGATORIU: VB e case-insensitive, parametrii ar umbri proprietățile și
        ' atribuirea necalificată ar scrie parametrul (capcana din feliile 0010 / 0019).
        Me.Key = key
        Me.Text = If(text, String.Empty)
        Me.Image = image
    End Sub

    ''' <summary>Separator: o linie fină, neselectabilă, care nu răspunde nici la mouse nici la taste.</summary>
    Public Shared Function Separator() As CustomPopupItem
        Return New CustomPopupItem() With {.IsSeparator = True}
    End Function

    <Category("K-BOT")>
    <Description("Identificatorul folosit de SelectedKey / ItemByKey. Trebuie să fie nevid și unic. Ignorat pe separatori.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Textul afișat. Un «&» marchează litera de acces («&Salvează» → S); «&&» e un ampersand literal.")>
    Public Property Text As String

    <Category("K-BOT")>
    <Description("Pictograma desenată la stânga textului. Ignorată pe separatori. Elementul nu o deține și nu o eliberează.")>
    Public Property Image As Image

    ' Fără perechea asta, editorul de colecții ar scrie «Item.Image = Nothing» pentru FIECARE
    ' element fără pictogramă. Un tip referință n-are <DefaultValue> utilizabil în VB (atributul
    ' primește un string convertit, iar Nothing nu e unul), deci ShouldSerialize/Reset e singurul
    ' fel de a spune «nesetat». Private: TypeDescriptor le găsește după nume, inclusiv nepublice.
    Private Function ShouldSerializeImage() As Boolean
        Return Image IsNot Nothing
    End Function

    Private Sub ResetImage()
        Image = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("False => rândul e desenat șters, nu se poate selecta și litera lui de acces nu răspunde (dar ocupă spațiu).")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT")>
    <Description("True => linie fină neselectabilă în locul unui rând. Cheia, textul și pictograma sunt ignorate.")>
    <DefaultValue(False)>
    Public Property IsSeparator As Boolean

    ''' <summary>Sac liber pentru apelant (nu e citit de popup).</summary>
    <Category("K-BOT")>
    <Description("Valoare liberă a apelantului; popup-ul nu o citește niciodată.")>
    Public Property Tag As Object

    ''' <summary>
    ''' Litera de acces derivată din <see cref="Text"/>, sau <see cref="PopupMnemonic.None"/>.
    ''' Un separator și un element dezactivat NU au literă de acces — altfel tasta ar «răspunde»
    ''' fără să se întâmple nimic, care e chiar no-op-ul tăcut interzis de regula casei.
    '''
    ''' Nici o literă care nu se poate TASTA nu contează (vezi <see cref="PopupMnemonic.IsTypable"/>):
    ''' «&amp;Închide» marchează «Î», care nu e nicio tastă, deci n-ar răspunde niciodată. Aici
    ''' răspunsul e cinstit — «nu are literă de acces» — în loc de una care pare să existe.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Mnemonic As Char
        Get
            If IsSeparator OrElse Not Enabled Then Return PopupMnemonic.None
            Dim litera As Char = PopupMnemonic.Extract(Text)
            If Not PopupMnemonic.IsTypable(litera) Then Return PopupMnemonic.None
            Return litera
        End Get
    End Property

    ''' <summary>Ce arată lista din stânga dialogului de colecții.</summary>
    Public Overrides Function ToString() As String
        If IsSeparator Then Return "──────── separator ────────"
        Dim cheie As String = If(String.IsNullOrWhiteSpace(Key), "<fără cheie>", Key)
        Dim eticheta As String = PopupMnemonic.Strip(If(Text, String.Empty))
        Dim litera As Char = Mnemonic
        Dim acces As String = If(litera = PopupMnemonic.None, String.Empty, " [" & litera & "]")
        Return cheie & " — «" & eticheta & "»" & acces & If(Enabled, String.Empty, " (dezactivat)")
    End Function

End Class

''' <summary>
''' Colecția ordonată din spatele <see cref="CustomPopup.Items"/>. Orice mutație cere popup-ului
''' să-și recalculeze geometria, ca o editare făcută după construcție (adaugă / șterge /
''' reordonează) să se vadă imediat.
'''
''' Validarea cheilor NU stă aici, din același motiv ca la <c>KBotNavItemCollection</c>: un editor
''' de colecții inserează elementul în clipa în care apeși «Add», cu mult înainte să fi tastat ceva
''' în el. Cheia se validează acolo unde e chiar folosită — <see cref="CustomPopup.SelectedKey"/>
''' și <see cref="CustomPopup.ItemByKey"/> ARUNCĂ pe o cheie necunoscută.
''' </summary>
Public NotInheritable Class CustomPopupItemCollection
    Inherits Collection(Of CustomPopupItem)

    ''' <summary>Popup-ul care deține colecția (Nothing pentru o instanță liberă).</summary>
    Friend Property Owner As CustomPopup

    ' Cei patru mutatori sunt PUNCTE DE INTRARE (codul îi cheamă direct), deci își poartă propriul
    ' Try/Catch. Clasificarea e «frontieră»: loghează și RE-ARUNCĂ — un meniu care pierde tăcut un
    ' element e chiar felul de eșec pe care regula casei îl interzice.
    Protected Overrides Sub InsertItem(index As Integer, item As CustomPopupItem)
        Try
            ArgumentNullException.ThrowIfNull(item)
            MyBase.InsertItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopupItemCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As CustomPopupItem)
        Try
            ArgumentNullException.ThrowIfNull(item)
            MyBase.SetItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopupItemCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopupItemCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopupItemCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

End Class

''' <summary>Elementul ales, plus poziția lui în colecție.</summary>
Public NotInheritable Class CustomPopupItemEventArgs
    Inherits EventArgs

    Public Sub New(item As CustomPopupItem, index As Integer)
        ' Câmpurile din spatele proprietăților ReadOnly: în VB proprietatea în sine nu se poate
        ' atribui nici măcar din constructor (spre deosebire de C#).
        _Item = item
        _Index = index
    End Sub

    ''' <summary>Elementul (niciodată Nothing pentru <c>ItemClicked</c>).</summary>
    Public ReadOnly Property Item As CustomPopupItem

    ''' <summary>Poziția lui în <see cref="CustomPopup.Items"/>.</summary>
    Public ReadOnly Property Index As Integer

End Class
