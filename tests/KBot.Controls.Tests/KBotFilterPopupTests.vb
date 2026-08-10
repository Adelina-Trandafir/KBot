Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports Xunit
Imports KBot.Controls

''' <summary>
''' <c>KBotFilterPopup</c> — meniul de filtrare al unei coloane (slice 0028-03). Ca la
''' <c>CustomPopupTests</c>, aici stă tot ce se poate ține fix FĂRĂ ecran: fereastra e doar
''' randare, dar deciziile ei — ce filtru iese la «OK», ce înseamnă «toate bifate», ce rămâne în
''' listă după o căutare — sunt funcții care se pot chema direct.
'''
''' Contractul apărat mai ales: <b>«tot bifat» NU e un filtru.</b> Dacă ar fi, coloana ar rămâne
''' marcată ca filtrată pentru totdeauna, fără să ascundă vreun rând.
''' </summary>
Public Class KBotFilterPopupTests

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

    Private Shared ReadOnly Valori As New List(Of String) From {"", "Ana", "Barbu", "Cezar"}

    Private Shared Function Meniu(Optional filtru As KBotColumnFilter = Nothing,
                                  Optional tip As KBotValueType = KBotValueType.Text) As KBotFilterPopup
        Return New KBotFilterPopup("nume", "Nume", tip, Valori, filtru, KBotSortDirection.None)
    End Function

    <Fact>
    Public Sub ItMeasuresItself_WithoutBeingShown()
        ' Prinde orice cădere în așezare — singurul drum prin care meniul se poate strica tăcut
        ' până când îl vede cineva pe ecran.
        RunSta(Sub()
                   Using p = Meniu()
                       Dim s = p.DebugMeasure()
                       Assert.True(s.Width > 0)
                       Assert.True(s.Height > 0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub WithoutAFilter_EverythingStartsChecked()
        ' Starea «nefiltrat», nu una goală pe care operatorul ar trebui s-o repare.
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.Equal(Valori.Count, p.DebugCheckedCount())
                       Assert.False(p.BuildFilter().IsActive)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub UncheckingOne_ProducesARealFilter()
        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugToggleValue("Barbu")
                       Dim f = p.BuildFilter()
                       Assert.True(f.IsActive)
                       Assert.NotNull(f.SelectedValues)
                       Assert.DoesNotContain("Barbu", f.SelectedValues)
                       Assert.Contains("Ana", f.SelectedValues)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RecheckingItBack_DisarmsTheFilterAgain()
        ' «Tot bifat» trebuie să se întoarcă la «fără filtru», altfel antetul ar rămâne aprins.
        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugToggleValue("Barbu")
                       Assert.True(p.BuildFilter().IsActive)
                       p.DebugToggleValue("Barbu")
                       Assert.False(p.BuildFilter().IsActive)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnExistingFilter_IsLoadedIntoTheCheckboxes()
        RunSta(Sub()
                   Dim existent As New KBotColumnFilter("nume") With {
                       .SelectedValues = New HashSet(Of String)({"Ana"}, StringComparer.CurrentCultureIgnoreCase)}
                   Using p = Meniu(existent)
                       Assert.Equal(1, p.DebugCheckedCount())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnExistingCondition_SurvivesTheRoundTrip()
        ' Bifele și condiția sunt două jumătăți ale aceluiași filtru: atingerea uneia nu are voie
        ' s-o piardă pe cealaltă.
        RunSta(Sub()
                   Dim existent As New KBotColumnFilter("nume") With {
                       .Condition = KBotFilterOperator.Contains, .Operand1 = "an"}
                   Using p = Meniu(existent)
                       p.DebugToggleValue("Cezar")
                       Dim f = p.BuildFilter()
                       Assert.Equal(KBotFilterOperator.Contains, f.Condition)
                       Assert.Equal("an", f.Operand1)
                       Assert.NotNull(f.SelectedValues)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Search_NarrowsTheList_WithoutTouchingTheChecks()
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.Equal(Valori.Count, p.DebugShownCount())
                       p.DebugSearch("ana")
                       Assert.Equal(1, p.DebugShownCount())
                       ' Căutarea ascunde, nu debifează.
                       Assert.Equal(Valori.Count, p.DebugCheckedCount())
                       p.DebugSearch("")
                       Assert.Equal(Valori.Count, p.DebugShownCount())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheBlankValue_GetsALabel_ButKeepsItsEmptyKey()
        ' Un rând complet gol în listă arată ca un rând stricat; eticheta e a interfeței, iar
        ' modelul rămâne pe textul vid (altfel o coloană care conține chiar «(Necompletate)» s-ar
        ' filtra împreună cu goalele).
        Assert.Equal("(Necompletate)", KBotFilterPopup.EtichetaValorii(""))
        Assert.Equal("Ana", KBotFilterPopup.EtichetaValorii("Ana"))

        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugToggleValue(KBotFilterEngine.CheieGol)
                       Assert.DoesNotContain(KBotFilterEngine.CheieGol, p.BuildFilter().SelectedValues)
                   End Using
               End Sub)
    End Sub

End Class
