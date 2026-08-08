Option Strict On
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' English: the two designer-authored surfaces added for the operator — <c>NodeImages</c>
''' (a plain <see cref="ImageList"/>, so pictures are imported through the IDE's own editor) and
''' the <c>Nodes</c> collection of flat <see cref="TreeNodeDefinition"/> records that the control
''' materialises into live <c>Items</c>.
'''
''' The rule worth pinning: definitions are the source ONLY while the collection is non-empty, so
''' the existing views — which fill the tree at runtime through <c>AddItem</c> / the FOREXE XML —
''' are never wiped by an untouched designer collection.
''' </summary>
Public Class AdvancedTreeDesignerNodesTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    ' ATENȚIE: bitmap-urile NU se eliberează aici. Până i se creează handle-ul nativ,
    ' ImageList păstrează REFERINȚA la imaginea originală, deci un Bitmap eliberat imediat
    ' după Add face ca prima citire să arunce «Parameter is not valid». Le ține ImageList-ul,
    ' iar ImageList-ul e eliberat de test.
    Private Shared Function NewImageList() As ImageList
        Dim il As New ImageList() With {.ImageSize = New Size(16, 16)}
        il.Images.Add("frunza", SolidBitmap(Color.SteelBlue))
        il.Images.Add("grup", SolidBitmap(Color.Goldenrod))
        Return il
    End Function

    Private Shared Function SolidBitmap(culoare As Color) As Bitmap
        Dim bmp As New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(culoare)
        End Using
        Return bmp
    End Function

    <Fact>
    Public Sub Nodes_construieste_ierarhia_dupa_ParentKey()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.Nodes.Add(New TreeNodeDefinition("a", "Grup A"))
                       tree.Nodes.Add(New TreeNodeDefinition("a1", "Frunza 1") With {.ParentKey = "a"})
                       tree.Nodes.Add(New TreeNodeDefinition("a2", "Frunza 2") With {.ParentKey = "a"})

                       Assert.Single(tree.Items)
                       Assert.Equal("a", tree.Items(0).Key)
                       Assert.Equal(2, tree.Items(0).Children.Count)
                       Assert.Equal("a1", tree.Items(0).Children(0).Key)
                       Assert.Equal(1, tree.Items(0).Children(0).Level)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un părinte declarat MAI JOS în colecție trebuie să funcționeze (două treceri).</summary>
    <Fact>
    Public Sub Nodes_accepta_parintele_declarat_mai_jos()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.Nodes.Add(New TreeNodeDefinition("copil", "Copil") With {.ParentKey = "parinte"})
                       tree.Nodes.Add(New TreeNodeDefinition("parinte", "Părinte"))

                       Assert.Single(tree.Items)
                       Assert.Equal("parinte", tree.Items(0).Key)
                       Assert.Single(tree.Items(0).Children)
                       Assert.Equal("copil", tree.Items(0).Children(0).Key)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Un ParentKey inexistent urcă nodul la rădăcină — vizibil, nu dispărut în tăcere.</summary>
    <Fact>
    Public Sub ParentKey_negasit_urca_nodul_la_radacina()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.Nodes.Add(New TreeNodeDefinition("orfan", "Orfan") With {.ParentKey = "nu-exista"})

                       Assert.Single(tree.Items)
                       Assert.Equal("orfan", tree.Items(0).Key)
                       Assert.Equal(0, tree.Items(0).Level)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Cheile_de_imagini_se_rezolva_din_NodeImages()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       Using tree As New AdvancedTreeControl()
                           tree.NodeImages = il
                           tree.Nodes.Add(New TreeNodeDefinition("a", "Grup") With {
                               .ImageKey = "grup", .RightImageKey = "frunza"})

                           Assert.NotNull(tree.Items(0).LeftIconClosed)
                           Assert.NotNull(tree.Items(0).RightIcon)
                           Assert.Null(tree.NodeImage("cheie-inexistenta"))
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cheia de iconiță de antet se rezolvă din NodeImages, indiferent de ordinea în care
    ''' designerul scrie cele două proprietăți (cheia întâi sau lista întâi).
    ''' </summary>
    <Fact>
    Public Sub Cheia_de_iconita_de_antet_se_rezolva_in_ambele_ordini()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       Using cheiaIntai As New AdvancedTreeControl()
                           cheiaIntai.HeaderLeftIconKey = "grup"
                           cheiaIntai.NodeImages = il
                           Assert.NotNull(cheiaIntai.HeaderLeftIcon)
                       End Using
                       Using listaIntai As New AdvancedTreeControl()
                           listaIntai.NodeImages = il
                           listaIntai.HeaderLeftIconKey = "grup"
                           Assert.NotNull(listaIntai.HeaderLeftIcon)
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Contractul care protejează vederile existente: o colecție de designer neatinsă NU
    ''' golește un arbore umplut la rulare.
    ''' </summary>
    <Fact>
    Public Sub Colectia_goala_nu_atinge_arborele_umplut_la_rulare()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.AddItem("r", "Rădăcină din cod")
                       Assert.Single(tree.Items)

                       tree.NodeImages = NewImageList()      ' declanșează RebuildFromDefinitions

                       Assert.Single(tree.Items)
                       Assert.Equal("r", tree.Items(0).Key)
                   End Using
               End Sub)
    End Sub
End Class
