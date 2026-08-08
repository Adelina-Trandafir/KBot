Option Strict On
Imports System.ComponentModel

''' <summary>
''' Suprafața de DESIGNER a arborelui: o listă de imagini și o colecție de noduri, amândouă
''' editabile din IDE.
'''
''' <see cref="NodeImages"/> e un <see cref="ImageList"/> obișnuit — se pune pe formular, se
''' încarcă pozele prin editorul lui (care le scrie în .resx) și se leagă de arbore. Așa se
''' «importă imagini prin designer» fără niciun editor propriu de tipuri.
'''
''' <see cref="Nodes"/> ține definiții PLATE (<see cref="TreeNodeDefinition"/>): legătura de
''' părinte e o cheie, iconițele sunt chei în <see cref="NodeImages"/>. La orice schimbare —
''' din designer sau din cod — <see cref="RebuildFromDefinitions"/> reconstruiește nodurile
''' vii din <c>Items</c>.
'''
''' CINE PE CINE: definițiile sunt sursa DOAR cât timp colecția e nevidă. Un arbore umplut la
''' rulare prin <c>AddItem</c> / XML-ul FOREXE lasă <see cref="Nodes"/> gol, deci reconstrucția
''' nu se declanșează niciodată și nu are ce să șteargă — vederile existente rămân neatinse.
''' </summary>
Partial Public Class AdvancedTreeControl

    Private ReadOnly _nodeDefinitions As New TreeNodeDefinitionCollection()
    Private _nodeImages As ImageList = Nothing
    Private _rebuildingDefinitions As Boolean = False

    ''' <summary>Nodurile scrise în designer. Gol = arborele se umple la rulare, ca până acum.</summary>
    <Category("K-BOT Arbore - Noduri")>
    <Description("Nodurile autorite în designer (cheie, text, cheia părintelui, chei de imagini).")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Nodes As TreeNodeDefinitionCollection
        Get
            Return _nodeDefinitions
        End Get
    End Property

    ''' <summary>
    ''' Sursa de imagini pentru cheile de iconițe (noduri ȘI antet). Arborele NU deține lista
    ''' și nu o eliberează niciodată — aparține formularului, exact ca la <c>KBotNavItem.Image</c>.
    ''' </summary>
    <Category("K-BOT Arbore - Noduri")>
    <Description("Lista de imagini din care se rezolvă cheile de iconițe (noduri și antet).")>
    <DefaultValue(GetType(ImageList), Nothing)>
    Public Property NodeImages As ImageList
        Get
            Return _nodeImages
        End Get
        Set(value As ImageList)
            If ReferenceEquals(_nodeImages, value) Then Return
            _nodeImages = value
            ResolveHeaderIconsFromNodeImages()
            RebuildFromDefinitions()
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Imaginea unei chei din <see cref="NodeImages"/>; Nothing dacă lipsește.</summary>
    Public Function NodeImage(key As String) As Image
        Try
            If _nodeImages Is Nothing OrElse String.IsNullOrEmpty(key) Then Return Nothing
            Dim idx As Integer = _nodeImages.Images.IndexOfKey(key)
            If idx < 0 Then Return Nothing
            Return _nodeImages.Images(idx)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.NodeImage", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Reconstruiește <c>Items</c> din <see cref="Nodes"/>. No-op când colecția e goală, ca un
    ''' arbore umplut la rulare să nu fie golit de o colecție de designer neatinsă.
    ''' </summary>
    Friend Sub RebuildFromDefinitions()
        Try
            If _rebuildingDefinitions Then Return
            If _nodeDefinitions.Count = 0 Then Return
            _rebuildingDefinitions = True
            Try
                Items.Clear()
                Dim dupaCheie As New Dictionary(Of String, TreeItem)(StringComparer.Ordinal)

                ' Treceri repetate: la fiecare tură construim nodurile al căror părinte există
                ' deja și le amânăm pe celelalte — o definiție își poate declara părintele MAI
                ' JOS în colecție. Parcurgerea e ÎNAINTE și amânatele își păstrează ordinea,
                ' fiindcă ordinea din colecție e ordinea de afișare între frați.
                Dim ramase As New List(Of TreeNodeDefinition)(_nodeDefinitions)
                Dim progres As Boolean = True
                While ramase.Count > 0 AndAlso progres
                    progres = False
                    Dim amanate As New List(Of TreeNodeDefinition)()
                    For Each def As TreeNodeDefinition In ramase
                        Dim parinte As TreeItem = Nothing
                        If Not String.IsNullOrEmpty(def.ParentKey) AndAlso
                           Not dupaCheie.TryGetValue(def.ParentKey, parinte) Then
                            amanate.Add(def)
                            Continue For
                        End If
                        MaterializeNode(def, parinte, dupaCheie)
                        progres = True
                    Next
                    ramase = amanate
                End While

                ' Ce a rămas are un ParentKey care nu există (sau o buclă): urcă la rădăcină,
                ' ca nodul să fie VIZIBIL și greșeala să sară în ochi, nu să dispară în tăcere.
                For Each def In ramase
                    MaterializeNode(def, Nothing, dupaCheie)
                Next
            Finally
                _rebuildingDefinitions = False
            End Try

            RefreshScrollVisibility()
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.RebuildFromDefinitions", ex)
            Throw
        End Try
    End Sub

    ' Ordinea din colecție e ordinea între frați: AddItem adaugă la coada părintelui.
    Private Sub MaterializeNode(def As TreeNodeDefinition,
                                parinte As TreeItem,
                                dupaCheie As Dictionary(Of String, TreeItem))
        Dim inchis As Image = NodeImage(def.ImageKey)
        Dim deschis As Image = If(String.IsNullOrEmpty(def.OpenImageKey), inchis, NodeImage(def.OpenImageKey))
        Dim cheie As String = If(String.IsNullOrEmpty(def.Key), Guid.NewGuid().ToString(), def.Key)

        Dim nod As TreeItem = AddItem(cheie, If(def.Caption, String.Empty), parinte,
                                      inchis, deschis, NodeImage(def.RightImageKey),
                                      def.Tag, def.Expanded, def.LazyNode)
        nod.HasCheckBox = def.HasCheckBox
        If Not String.IsNullOrEmpty(def.Tooltip) Then nod.Tooltip = def.Tooltip
        dupaCheie(cheie) = nod
    End Sub

    ''' <summary>
    ''' Rezolvă cheile de iconițe de antet din <see cref="NodeImages"/>. Cheia câștigă doar dacă
    ''' găsește o imagine — o iconiță aleasă direct în designer nu e ștearsă de o cheie greșită.
    ''' </summary>
    Friend Sub ResolveHeaderIconsFromNodeImages()
        If _nodeImages Is Nothing Then Return
        Dim img As Image

        img = NodeImage(_headerLeftIconKey)
        If img IsNot Nothing Then _headerLeftIcon = img

        img = NodeImage(_headerRightIconKey)
        If img IsNot Nothing Then _headerRightIcon = img

        img = NodeImage(_headerSearchIconKey)
        If img IsNot Nothing Then
            _headerSearchIcon = img
            ApplySearchShow()      ' o iconiță de toggle schimbă regimul benzii de căutare
        End If
    End Sub

End Class
