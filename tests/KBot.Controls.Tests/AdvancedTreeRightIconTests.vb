Option Strict On
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' ICONIȚA DIN DREAPTA a nodului și locul pe care i-l ia textului.
'''
''' Regula, cerută de operator după prima probă pe ecran: «showrighticononhover SHOULDN'T reserve
''' the space by default. if it's disabled, then DON'T RESERVE THE SPACE! when hovered and the
''' space is NOT reserved, the text of the node will become narrower to fit the icon».
'''
''' Deci: o iconiță PERMANENTĂ ia mereu locul (textul n-are voie să treacă pe sub ea); una
''' HOVER-ONLY nu ia nimic cât nodul nu e survolat — ăsta e tot rostul lui «hover-only» — și
''' îngustează textul abia când apare. Rezervarea permanentă rămâne disponibilă, dar ca alegere
''' EXPLICITĂ (<c>ReserveRightIconSpace</c>), nu ca implicit.
''' </summary>
Public Class AdvancedTreeRightIconTests

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


    ' Regula cerută de operator: hover-only NU rezervă locul. Textul folosește toată lățimea și
    ' se îngustează abia când iconița apare. Rezervarea permanentă rămâne disponibilă, dar e o
    ' alegere explicită (ReserveRightIconSpace), nu implicitul.

    Private Shared Function ArboreCuIconitaDreapta(hoverOnly As Boolean) As AdvancedTreeControl
        Dim tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
        tree.ShowRightIconOnHover = hoverOnly
        Dim nod As AdvancedTreeControl.TreeItem =
            tree.AddItem("N1", "Nod cu iconiță la dreapta", Nothing, Nothing, Nothing,
                         New Bitmap(16, 16))
        Return tree
    End Function

    <Fact>
    Public Sub Iconita_permanenta_isi_ia_locul_intotdeauna()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuIconitaDreapta(hoverOnly:=False)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.Items(0)
                       Dim asteptat As Integer = tree.RightIconSize.Width + tree.RightIconRightPadding
                       ' Nesurvolat sau survolat, iconița e pe ecran: textul nu trece pe sub ea.
                       Assert.Equal(asteptat, tree.RightIconGutter(nod))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Hover_only_nu_rezerva_locul_dar_ingusteaza_textul_la_survolare()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuIconitaDreapta(hoverOnly:=True)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.Items(0)
                       Dim latimeIconita As Integer = tree.RightIconSize.Width + tree.RightIconRightPadding

                       ' Nesurvolat: textul are TOATĂ lățimea — ăsta e rostul lui «hover-only».
                       Assert.Equal(0, tree.RightIconGutter(nod))

                       ' Survolat: iconița apare, deci textul se îngustează exact cu ea.
                       tree.DebugSetHoveredItem(nod)
                       Assert.Equal(latimeIconita, tree.RightIconGutter(nod))

                       tree.DebugSetHoveredItem(Nothing)
                       Assert.Equal(0, tree.RightIconGutter(nod))
                   End Using
               End Sub)
    End Sub

    ''' <summary>Cine chiar vrea text nemișcat cere locul fix — dar o cere explicit.</summary>
    <Fact>
    Public Sub ReserveRightIconSpace_tine_locul_fix_si_pentru_hover_only()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuIconitaDreapta(hoverOnly:=True)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.Items(0)
                       tree.ReserveRightIconSpace = True
                       Dim asteptat As Integer = tree.RightIconSize.Width + tree.RightIconRightPadding

                       Assert.Equal(asteptat, tree.RightIconGutter(nod))     ' nesurvolat
                       tree.DebugSetHoveredItem(nod)
                       Assert.Equal(asteptat, tree.RightIconGutter(nod))     ' survolat — la fel
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_nod_fara_iconita_nu_ia_niciun_loc()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Dim nod As AdvancedTreeControl.TreeItem = tree.AddItem("N1", "Fără iconiță")
                       tree.ReserveRightIconSpace = True
                       Assert.Equal(0, tree.RightIconGutter(nod))
                       Assert.Equal(0, tree.RightIconGutter(Nothing))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Steagul per-nod ridică hover-only pentru nodul lui, chiar dacă globalul e stins — deci și
    ''' regula de rezervare trebuie să-l urmeze.
    ''' </summary>
    <Fact>
    Public Sub Steagul_per_nod_conduce_rezervarea_la_fel_ca_globalul()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuIconitaDreapta(hoverOnly:=False)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.Items(0)
                       nod.ShowRightIconOnHover = True
                       Assert.True(tree.IsRightIconHoverOnly(nod))
                       Assert.Equal(0, tree.RightIconGutter(nod))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Banda de coloane (TreeListView) NU urmează hover-ul, cu bună știință: o geometrie pe tot
    ''' controlul care se re-așază la fiecare trecere a cursorului ar fi de nefolosit.
    ''' </summary>
    <Fact>
    Public Sub Banda_de_coloane_nu_se_reaseaza_la_hover()
        RunSta(Sub()
                   Using tree As AdvancedTreeControl = ArboreCuIconitaDreapta(hoverOnly:=True)
                       Dim nod As AdvancedTreeControl.TreeItem = tree.Items(0)
                       Assert.Equal(0, tree.ReservedRightIconWidth())

                       tree.ReserveRightIconSpace = True
                       Dim asteptat As Integer = tree.RightIconSize.Width + tree.RightIconRightPadding
                       Assert.Equal(asteptat, tree.ReservedRightIconWidth())
                       tree.DebugSetHoveredItem(nod)
                       Assert.Equal(asteptat, tree.ReservedRightIconWidth())
                   End Using
               End Sub)
    End Sub

End Class
