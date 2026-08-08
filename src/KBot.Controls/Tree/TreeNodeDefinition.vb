Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Un nod AUTORIT ÎN DESIGNER. Nu e același lucru cu <c>AdvancedTreeControl.TreeItem</c>: acela e
''' nodul viu (părinte/copii ca referințe, stare de expandare, celule, iconițe rezolvate), construit
''' la rulare din API sau din XML-ul FOREXE. Aici avem forma PLATĂ, serializabilă, pe care o poate
''' scrie designerul: legătura de părinte e o CHEIE, nu o referință, iar imaginile sunt chei în
''' <see cref="AdvancedTreeControl.NodeImages"/>, nu obiecte Image.
'''
''' De ce plat: o ierarhie de referințe nu trece prin editorul standard de colecții, iar un editor
''' propriu ar cere un assembly de design-time compilat pe Microsoft.WinForms.Designer.SDK — exact
''' prețul pe care felia 0025 a refuzat să-l plătească pentru <see cref="KBotNavItem"/>. Ordinea din
''' colecție e ordinea de afișare între frați; un ParentKey care nu se găsește urcă nodul la rădăcină.
''' </summary>
Public NotInheritable Class TreeNodeDefinition

    ''' <summary>Constructor fără parametri — cerut de editorul de colecții al designerului.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Comoditate pentru cod: nod rădăcină cu cheie și text.</summary>
    Public Sub New(key As String, caption As String)
        ' «Me.» e OBLIGATORIU: VB e case-insensitive, parametrii ar umbri proprietățile
        ' și atribuirea necalificată ar scrie parametrul (capcana din feliile 0010 / 0019).
        Me.Key = key
        Me.Caption = If(caption, String.Empty)
    End Sub

    <Category("K-BOT")>
    <Description("Identificatorul nodului. Trebuie să fie nevid și unic în colecție.")>
    Public Property Key As String

    <Category("K-BOT")>
    <Description("Textul afișat (acceptă mini-html-ul arborelui și separatorul ~~~).")>
    Public Property Caption As String

    <Category("K-BOT")>
    <Description("Cheia nodului părinte; gol = nod rădăcină. O cheie negăsită urcă nodul la rădăcină.")>
    Public Property ParentKey As String

    <Category("K-BOT")>
    <Description("Cheia imaginii din NodeImages pentru nodul închis (și implicit pentru cel deschis).")>
    Public Property ImageKey As String

    <Category("K-BOT")>
    <Description("Cheia imaginii din NodeImages pentru nodul deschis; gol = aceeași ca ImageKey.")>
    Public Property OpenImageKey As String

    <Category("K-BOT")>
    <Description("Cheia imaginii din NodeImages pentru iconița din dreapta rândului.")>
    Public Property RightImageKey As String

    <Category("K-BOT")>
    <Description("Textul tooltip-ului nodului.")>
    Public Property Tooltip As String

    <Category("K-BOT")>
    <Description("Valoarea Tag a nodului (căutabilă când SearchIn include Tag).")>
    Public Property Tag As String

    <Category("K-BOT")>
    <Description("Nodul pornește expandat.")>
    <DefaultValue(False)>
    Public Property Expanded As Boolean

    <Category("K-BOT")>
    <Description("Nodul primește checkbox (când CheckBoxes e activ pe arbore).")>
    <DefaultValue(False)>
    Public Property HasCheckBox As Boolean

    <Category("K-BOT")>
    <Description("Nodul se încarcă leneș: expandarea ridică RequestLazyLoad.")>
    <DefaultValue(False)>
    Public Property LazyNode As Boolean

    ''' <summary>Ce arată lista din stânga dialogului de colecții.</summary>
    Public Overrides Function ToString() As String
        Dim cheie As String = If(String.IsNullOrWhiteSpace(Key), "<fără cheie>", Key)
        Dim parinte As String = If(String.IsNullOrWhiteSpace(ParentKey), "rădăcină", "sub " & ParentKey)
        Return cheie & " — """ & If(Caption, String.Empty) & """ (" & parinte & ")"
    End Function

End Class

''' <summary>
''' Colecția ordonată din spatele <c>AdvancedTreeControl.Nodes</c>. Orice mutație cere arborelui
''' să-și reconstruiască nodurile vii, ca o editare din designer (adaugă / șterge / reordonează)
''' să se vadă imediat, fără să aștepte un resize.
'''
''' Validarea cheilor NU stă aici, din același motiv ca la <see cref="KBotNavItemCollection"/>:
''' editorul de colecții inserează elementul în clipa în care apeși «Add», cu mult înainte să fi
''' tastat ceva în el.
''' </summary>
Public NotInheritable Class TreeNodeDefinitionCollection
    Inherits Collection(Of TreeNodeDefinition)

    ''' <summary>Arborele care deține colecția (Nothing pentru o instanță liberă).</summary>
    Friend Property Owner As AdvancedTreeControl

    ' Cei patru mutatori sunt PUNCTE DE INTRARE (designerul îi cheamă din InitializeComponent,
    ' codul îi cheamă direct), deci își poartă propriul Try/Catch: loghează și RE-ARUNCĂ.
    Protected Overrides Sub InsertItem(index As Integer, item As TreeNodeDefinition)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            Owner?.RebuildFromDefinitions()
        Catch ex As Exception
            LogUnlessDesignTime("TreeNodeDefinitionCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As TreeNodeDefinition)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.SetItem(index, item)
            Owner?.RebuildFromDefinitions()
        Catch ex As Exception
            LogUnlessDesignTime("TreeNodeDefinitionCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.RebuildFromDefinitions()
        Catch ex As Exception
            LogUnlessDesignTime("TreeNodeDefinitionCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.RebuildFromDefinitions()
        Catch ex As Exception
            LogUnlessDesignTime("TreeNodeDefinitionCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' Un fișier de log scris din interiorul devenv.exe e, în cel mai bun caz, zgomot.
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub

End Class
