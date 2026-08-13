Option Strict On
Imports System.ComponentModel
Imports System.Linq
Imports System.Threading
Imports Xunit
Imports KBot.Controls

''' <summary>
''' MARGINILE ARBORELUI ca proprietăți de designer (categoria «K-BOT Arbore - Paddings»).
'''
''' Erau constante private; acum sunt proprietăți. Testul păzește trei lucruri:
'''  1. toate stau în categoria cerută (operatorul le vrea într-un singur loc în grilă);
'''  2. valorile implicite sunt EXACT cele ale fostelor constante — o margine schimbată din
'''     greșeală la mutare ar fi mișcat tot desenul;
'''  3. un control proaspăt scos din Toolbox NU serializează niciuna (regula casei: zero linii
'''     în .Designer.vb). Se verifică prin <c>TypeDescriptor</c>, adică pe drumul pe care merge
'''     chiar Visual Studio, nu apelând ShouldSerializeX direct.
''' </summary>
Public Class AdvancedTreePaddingsTests

    Private Const CATEGORIE As String = "K-BOT Arbore - Paddings"

    ' Numele proprietății → valoarea implicită (fosta constantă)
    Private Shared ReadOnly Implicite As New Dictionary(Of String, Integer) From {
        {"PaddingTreeStart", 10},
        {"PaddingSelectionLeft", 4},
        {"PaddingTreeTop", 5},
        {"PaddingTreeEnd", 4},
        {"PaddingExpanderGap", 12},
        {"PaddingTreeLineHMargin", 4},
        {"PaddingCheckBoxGap", 8},
        {"PaddingIconGap", 16},
        {"PaddingSeparatorGap", 8},
        {"PaddingTooltipIconHit", 3},
        {"RightIconRightPadding", 6}
    }

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

    <Fact>
    Public Sub Toate_marginile_sunt_proprietati_vizibile_in_categoria_Paddings()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim props = TypeDescriptor.GetProperties(tree)
                       For Each nume In Implicite.Keys
                           Dim pd = props(nume)
                           Assert.True(pd IsNot Nothing, $"Proprietatea «{nume}» lipsește din grilă.")
                           Assert.True(pd.IsBrowsable, $"«{nume}» nu e vizibilă în grilă.")
                           Assert.Equal(CATEGORIE, pd.Category)
                       Next
                       ' Și marginea benzii de căutare (Padding, nu Integer) stă în aceeași categorie.
                       Assert.Equal(CATEGORIE, props("SearchClearButtonPadding").Category)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Valorile_implicite_sunt_cele_ale_fostelor_constante()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim props = TypeDescriptor.GetProperties(tree)
                       For Each kv In Implicite
                           Assert.Equal(kv.Value, CInt(props(kv.Key).GetValue(tree)))
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Un_arbore_proaspat_nu_serializeaza_nicio_margine()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim props = TypeDescriptor.GetProperties(tree)
                       For Each nume In Implicite.Keys
                           Assert.False(props(nume).ShouldSerializeValue(tree),
                                        $"«{nume}» s-ar scrie în .Designer.vb fără ca operatorul s-o fi atins.")
                       Next
                       Assert.False(props("SearchClearButtonPadding").ShouldSerializeValue(tree))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub O_margine_schimbata_de_operator_se_serializeaza_si_se_poate_reseta()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim props = TypeDescriptor.GetProperties(tree)
                       Dim pd = props("PaddingExpanderGap")
                       pd.SetValue(tree, 30)
                       Assert.Equal(30, tree.PaddingExpanderGap)
                       Assert.True(pd.ShouldSerializeValue(tree))
                       pd.ResetValue(tree)
                       Assert.Equal(12, tree.PaddingExpanderGap)
                       Assert.False(pd.ShouldSerializeValue(tree))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Marginile_negative_sunt_prinse_la_zero()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       tree.PaddingTreeStart = -5
                       tree.PaddingIconGap = -1
                       Assert.Equal(0, tree.PaddingTreeStart)
                       Assert.Equal(0, tree.PaddingIconGap)
                   End Using
               End Sub)
    End Sub

End Class
