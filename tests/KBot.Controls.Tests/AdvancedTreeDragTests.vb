Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' TRAGEREA unui nod peste altul (felia 0048-04, decizia D-K).
'''
''' Ce se ține fix aici, fără ecran: implicitul e STINS (cele nouă vederi care folosesc deja
''' arborele nu capătă comportament nou), un arbore neatins nu scrie NICIO linie de tragere în
''' formularul gazdă, <c>AllowDrop</c> urmează comutatorul, iar <b>refuzul e implicitul</b> —
''' o gazdă care uită să răspundă la <c>NodeDragOver</c> nu lasă să treacă nimic.
'''
''' Ce NU se poate acoperi aici: bucla modală a sistemului. <c>DoDragDrop</c> nu se poate porni
''' fără mouse real, deci pornirea tragerii se verifică prin pragul care o precede, nu prin ea
''' însăși. Consemnat ca atare.
''' </summary>
Public Class AdvancedTreeDragTests

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

    Private Shared Function Arbore() As AdvancedTreeControl
        Dim tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
        Dim grup As AdvancedTreeControl.TreeItem = tree.AddItem("R1", "Recepție 1", Nothing, pExpanded:=True)
        tree.AddItem("H1", "Instantaneu 1", grup)
        tree.AddItem("H2", "Instantaneu 2", grup)
        Return tree
    End Function

    ' ── Serializare: tragerea nefolosită e invizibilă pentru designer ────────────

    <Fact>
    Public Sub ArboreNeatins_NuScrieNicioLinieDeTragere()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(tree)
                       For Each nume As String In {"DragEnabled", "DragHighlightColor", "DragForbiddenColor"}
                           Assert.False(props(nume).ShouldSerializeValue(tree),
                                        $"«{nume}» ar fi scrisă în .Designer.vb fără ca cineva să o fi ales.")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub OCuloareAleasaInDesigner_SeSerializeaza_SiCastiga()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.DragHighlightColor = Color.Magenta
                       Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)("DragHighlightColor")
                       Assert.True(pd.ShouldSerializeValue(tree))
                       Assert.Equal(Color.Magenta, tree.DragHighlightColor)

                       ' Reset o duce înapoi la «din temă», deci iar invizibilă pentru designer.
                       pd.ResetValue(tree)
                       Assert.False(pd.ShouldSerializeValue(tree))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CulorileImplicite_NuSuntEmpty_CiVinDinTema()
        ' `Color.Empty` = «din temă» pe DINĂUNTRU; pe DINAFARĂ proprietatea trebuie să răspundă
        ' cu o culoare adevărată, altfel desenul ar picta cu Empty.
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Assert.NotEqual(Color.Empty, tree.DragHighlightColor)
                       Assert.NotEqual(Color.Empty, tree.DragForbiddenColor)
                   End Using
               End Sub)
    End Sub

    ' ── Comutatorul ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Implicit_TragereaEStinsa_SiAllowDropEFals()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Assert.False(tree.DragEnabled)
                       Assert.False(tree.AllowDrop)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AllowDrop_UrmeazaComutatorul()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.DragEnabled = True
                       Assert.True(tree.AllowDrop)
                       tree.DragEnabled = False
                       Assert.False(tree.AllowDrop)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub FaraComutator_ApasareaNuArmeazaNimic()
        ' Pragul se măsoară abia la mișcare; dacă nimic nu s-a armat, mișcarea nu are ce porni.
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("X", "X")
                       tree.ArmDrag(nod, New Point(10, 10), MouseButtons.Left)
                       Assert.False(tree.MaybeBeginDrag(New Point(400, 400), MouseButtons.Left))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SubPrag_NuSePorneste()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       tree.DragEnabled = True
                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("X", "X")
                       tree.ArmDrag(nod, New Point(50, 50), MouseButtons.Left)
                       ' Un pixel de tremur nu e o tragere.
                       Assert.False(tree.MaybeBeginDrag(New Point(51, 50), MouseButtons.Left))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ButonulDrept_NuArmeazaTragerea()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       tree.DragEnabled = True
                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("X", "X")
                       tree.ArmDrag(nod, New Point(50, 50), MouseButtons.Right)
                       Assert.False(tree.MaybeBeginDrag(New Point(400, 400), MouseButtons.Right))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub GazdaCarePuneCancel_OpresteTragereaInaintePornirii()
        ' Aici se opresc rândurile care nu sunt de mutat: rădăcinile de recepție (D-J) și
        ' instantaneele blocate de ordonanțări sau plăți.
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       tree.DragEnabled = True
                       Dim intrebat As AdvancedTreeControl.TreeItem = Nothing
                       AddHandler tree.NodeDragStarting,
                           Sub(s As Object, e As TreeDragStartEventArgs)
                               intrebat = e.Item
                               e.Cancel = True
                           End Sub

                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("X", "X")
                       tree.ArmDrag(nod, New Point(50, 50), MouseButtons.Left)
                       ' Nu s-a intrat în bucla modală, deci funcția răspunde False — dar
                       ' evenimentul A FOST ridicat, adică refuzul a fost al gazdei.
                       Assert.False(tree.MaybeBeginDrag(New Point(400, 400), MouseButtons.Left))
                       Assert.Same(nod, intrebat)
                   End Using
               End Sub)
    End Sub

    ' ── Contractul evenimentelor ────────────────────────────────────────────────

    <Fact>
    Public Sub RefuzulEImplicitul_LaValidareaAruncarii()
        ' O gazdă care uită să răspundă nu are voie să lase să treacă tot. Sub F12 o asociere
        ' greșită e tăcută și permanentă, deci implicitul trebuie să fie cel care nu strică.
        Dim e As New TreeDragOverEventArgs(Nothing, Nothing)
        Assert.False(e.Allow)
        Assert.Equal(String.Empty, e.Motiv)
    End Sub

    <Fact>
    Public Sub PornireaNuEAnulataDinOficiu()
        Dim e As New TreeDragStartEventArgs(Nothing)
        Assert.False(e.Cancel)
    End Sub

    <Fact>
    Public Sub ArgumenteleDucSursaSiTinta()
        Dim a As New AdvancedTreeControl.TreeItem() With {.Key = "A"}
        Dim b As New AdvancedTreeControl.TreeItem() With {.Key = "B"}
        Dim over As New TreeDragOverEventArgs(a, b)
        Assert.Same(a, over.Source)
        Assert.Same(b, over.Target)
        Dim drop As New TreeDropEventArgs(a, b)
        Assert.Same(a, drop.Source)
        Assert.Same(b, drop.Target)
    End Sub

    <Fact>
    Public Sub StingereaComutatorului_StergeStareaDeTragere()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       tree.DragEnabled = True
                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("X", "X")
                       tree.ArmDrag(nod, New Point(50, 50), MouseButtons.Left)
                       tree.DragEnabled = False
                       ' Armarea a fost anulată odată cu comutatorul.
                       tree.DragEnabled = True
                       Assert.False(tree.MaybeBeginDrag(New Point(400, 400), MouseButtons.Left))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NiciunNodTras_InAfaraTragerii()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = Arbore()
                       Assert.Null(tree.DraggedItem)
                       tree.CancelDrag()
                       Assert.Null(tree.DraggedItem)
                   End Using
               End Sub)
    End Sub
End Class
