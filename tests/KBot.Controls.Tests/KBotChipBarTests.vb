Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit

''' <summary>
''' Contractul lui <see cref="KBotChipBar"/> (felia 0031-03), testele 12–18 din
''' <c>docs/PLAN_LogViewer.md</c> §10: colecția conduce așezarea, cheile greșite aruncă, bifarea și
''' evenimentul ei, pragul <c>MinimumRequiredChecked</c>, <c>BeginInit</c>/<c>EndInit</c> și
''' tastatura.
'''
''' Ce NU pot dovedi testele astea: dus-întorsul prin designer-ul real din Visual Studio (butonul
''' «…», dialogul de colecție, liniile scrise în <c>*.Designer.vb</c>, chenarul roșu pictat pe
''' suprafața de design). Sunt verificări manuale, scrise în worklog ca NERULATE — aceeași
''' limitare recunoscută la felia 0025 pentru <c>KBotNavList</c>.
''' </summary>
Public Class KBotChipBarTests

    ' Controlul e WinForms: se creează pe un fir STA, ca suitele surori.
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

    ' O bară cu dimensiune reală, ca RecalcLayout să dea sloturi nevide.
    Private Shared Function NewSizedBar() As KBotChipBar
        Dim bar As New KBotChipBar()
        bar.Size = New Size(400, 32)
        Return bar
    End Function

    Private Shared Function CentreOf(bar As KBotChipBar, index As Integer) As Point
        Dim r As Rectangle = bar.DebugBounds(index)
        Return New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)
    End Function

    ' ── 12. Colecția invalidează așezarea; Nothing se refuză ──────────────────

    <Fact>
    Public Sub Colectia_AdaugareStergere_ReasazaJetoanele()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.Chips.Add(New KBotChip("a", "Alfa"))
                       bar.Chips.Add(New KBotChip("b", "Beta"))
                       Dim first As Rectangle = bar.DebugBounds(0)
                       Dim second As Rectangle = bar.DebugBounds(1)
                       Assert.True(first.Width > 0)
                       ' Al doilea jeton stă la DREAPTA primului, pe același rând.
                       Assert.True(second.Left >= first.Right)
                       Assert.Equal(first.Top, second.Top)

                       bar.Chips.RemoveAt(0)
                       ' După ștergere, jetonul rămas s-a mutat la începutul rândului.
                       Assert.Equal(0, bar.DebugBounds(0).Left)

                       bar.Chips.Clear()
                       Assert.Equal(0, bar.Chips.Count)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Colectia_RefuzaNothing()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       Assert.Throws(Of ArgumentNullException)(Sub() bar.Chips.Add(Nothing))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Jetonul_Ascuns_NuPrimesteSlot()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       bar.AddChip("b", "Beta")
                       bar.SetChipVisible("a", False)
                       Assert.Equal(Rectangle.Empty, bar.DebugBounds(0))
                       ' Cel vizibil urcă în locul lui, la începutul rândului.
                       Assert.Equal(0, bar.DebugBounds(1).Left)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Jetoanele_TrecPeRandulUrmator_CandNuMaiIncap()
        RunSta(Sub()
                   Using bar = New KBotChipBar()
                       ' Bară îngustă dinadins: al doilea jeton nu mai are unde să încapă.
                       bar.Size = New Size(90, 64)
                       bar.AddChip("a", "Alfa")
                       bar.AddChip("b", "Beta")
                       Dim first As Rectangle = bar.DebugBounds(0)
                       Dim second As Rectangle = bar.DebugBounds(1)
                       Assert.True(second.Top > first.Top)
                       Assert.Equal(0, second.Left)
                       Assert.Equal(2, bar.DebugRowCount())
                       ' Înălțimea cerută acoperă amândouă rândurile.
                       Assert.True(bar.PreferredBarHeight > first.Height)
                   End Using
               End Sub)
    End Sub

    ' ── 13. Chei vide / duplicate / necunoscute ───────────────────────────────

    <Fact>
    Public Sub AddChip_RefuzaCheiaVidaSiDuplicata()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       Assert.Throws(Of ArgumentException)(Sub() bar.AddChip("", "Gol"))
                       Assert.Throws(Of ArgumentException)(Sub() bar.AddChip("   ", "Spații"))
                       Assert.Throws(Of ArgumentException)(Sub() bar.AddChip("a", "Din nou"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Setterele_ArunkaPeCheieNecunoscuta()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       Assert.Throws(Of ArgumentException)(Sub() bar.SetChecked("x", True))
                       Assert.Throws(Of ArgumentException)(Sub() bar.SetBadge("x", 3))
                       Assert.Throws(Of ArgumentException)(Sub() bar.SetChipEnabled("x", False))
                       Assert.Throws(Of ArgumentException)(Sub() bar.SetChipVisible("x", False))
                       Assert.Throws(Of ArgumentException)(Sub() bar.IsChecked("x"))
                       ' Și cheia vidă e o cheie greșită, nu «nimic».
                       Assert.Throws(Of ArgumentException)(Sub() bar.SetChecked("", True))
                   End Using
               End Sub)
    End Sub

    ' ── 14. CheckedKeys, CheckAll, UncheckAll ─────────────────────────────────

    <Fact>
    Public Sub CheckedKeys_UrmeazaStarea()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       bar.AddChip("b", "Beta", True)
                       bar.AddChip("c", "Gama")
                       Assert.Equal(New String() {"b"}, bar.CheckedKeys.ToArray())

                       bar.SetChecked("a", True)
                       Assert.Equal(New String() {"a", "b"}, bar.CheckedKeys.ToArray())

                       bar.CheckAll()
                       Assert.Equal(New String() {"a", "b", "c"}, bar.CheckedKeys.ToArray())

                       bar.UncheckAll()
                       Assert.Empty(bar.CheckedKeys)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CheckedKeys_NuRaporteazaJetoaneleAscunse()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa", True)
                       bar.AddChip("b", "Beta", True)
                       bar.SetChipVisible("a", False)
                       Assert.Equal(New String() {"b"}, bar.CheckedKeys.ToArray())
                   End Using
               End Sub)
    End Sub

    ' ── 15. CheckedChanged: o dată pe schimbare REALĂ, niciodată pe una redundantă ──

    <Fact>
    Public Sub CheckedChanged_SeRidicaDoarLaSchimbareReala()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       Dim raised As New List(Of String)()
                       AddHandler bar.CheckedChanged, Sub(k As String) raised.Add(k)

                       bar.SetChecked("a", True)
                       Assert.Equal(New String() {"a"}, raised.ToArray())

                       ' Aceeași valoare din nou: niciun eveniment.
                       bar.SetChecked("a", True)
                       Assert.Single(raised)

                       bar.SetChecked("a", False)
                       Assert.Equal(2, raised.Count)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CheckAll_RidicaEvenimentulNumaiPentruCeleSchimbate()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa", True)
                       bar.AddChip("b", "Beta")
                       Dim raised As New List(Of String)()
                       AddHandler bar.CheckedChanged, Sub(k As String) raised.Add(k)
                       bar.CheckAll()
                       ' «a» era deja bifat.
                       Assert.Equal(New String() {"b"}, raised.ToArray())
                   End Using
               End Sub)
    End Sub

    ' ── 16. MinimumRequiredChecked = 1 ────────────────────────────────────────

    <Fact>
    Public Sub PragulDeUnu_RefuzaStingereaUltimuluiJeton()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa", True)
                       bar.AddChip("b", "Beta")
                       bar.MinimumRequiredChecked = 1
                       Dim raised As New List(Of String)()
                       AddHandler bar.CheckedChanged, Sub(k As String) raised.Add(k)

                       ' Click pe singurul jeton bifat: refuzat, fără excepție și fără eveniment.
                       bar.DebugClickAt(CentreOf(bar, 0))
                       Assert.True(bar.IsChecked("a"))
                       Assert.Empty(raised)
                       ' Refuzul se VEDE: jetonul clipește.
                       Assert.Equal(0, bar.DebugFlashIndex())

                       ' Cu două bifate, stingerea uneia e permisă.
                       bar.DebugClickAt(CentreOf(bar, 1))
                       Assert.True(bar.IsChecked("b"))
                       bar.DebugClickAt(CentreOf(bar, 0))
                       Assert.False(bar.IsChecked("a"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Pragul_NuOpresteApelurileDeApi()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa", True)
                       bar.MinimumRequiredChecked = 1
                       ' SetChecked e un apel de API, nu un gest: pragul nu i se aplică (vezi
                       ' documentația proprietății).
                       bar.SetChecked("a", False)
                       Assert.False(bar.IsChecked("a"))
                   End Using
               End Sub)
    End Sub

    ' ── 17. BeginInit / EndInit ───────────────────────────────────────────────

    <Fact>
    Public Sub BeginEndInit_ValideazaLaSfarsit()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       Dim init As ISupportInitialize = bar
                       init.BeginInit()
                       bar.Chips.Add(New KBotChip("a", "Alfa", True))
                       bar.Chips.Add(New KBotChip("b", "Beta"))
                       init.EndInit()

                       Assert.Equal(New String() {"a"}, bar.CheckedKeys.ToArray())
                       Assert.True(bar.DebugBounds(0).Width > 0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EndInit_ArunkaPeCheiDuplicateSauVide()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       Dim init As ISupportInitialize = bar
                       init.BeginInit()
                       bar.Chips.Add(New KBotChip("a", "Alfa"))
                       bar.Chips.Add(New KBotChip("a", "Alfa din nou"))
                       Assert.Throws(Of ArgumentException)(Sub() init.EndInit())
                   End Using

                   Using bar2 = NewSizedBar()
                       Dim init2 As ISupportInitialize = bar2
                       init2.BeginInit()
                       bar2.Chips.Add(New KBotChip("", "Fără cheie"))
                       Assert.Throws(Of ArgumentException)(Sub() init2.EndInit())
                   End Using
               End Sub)
    End Sub

    ' ── 18. Tastatură ─────────────────────────────────────────────────────────

    <Fact>
    Public Sub Spatiul_ComutaJetonulCuFocus()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       bar.AddChip("b", "Beta")
                       ' Fără focus mutat explicit, Space aprinde primul jeton selectabil.
                       bar.DebugKeyDown(Keys.Space)
                       Assert.True(bar.IsChecked("a"))

                       bar.DebugKeyDown(Keys.Right)
                       Assert.Equal(1, bar.DebugFocusIndex())
                       bar.DebugKeyDown(Keys.Space)
                       Assert.True(bar.IsChecked("b"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Sagetile_SarJetoaneleAscunseSiDezactivate()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       bar.AddChip("b", "Beta")
                       bar.AddChip("c", "Gama")
                       bar.AddChip("d", "Delta")
                       bar.SetChipVisible("b", False)
                       bar.SetChipEnabled("c", False)

                       bar.DebugKeyDown(Keys.Right)      ' focus pe «a»
                       Assert.Equal(0, bar.DebugFocusIndex())
                       bar.DebugKeyDown(Keys.Right)      ' sare «b» (ascuns) și «c» (dezactivat)
                       Assert.Equal(3, bar.DebugFocusIndex())
                       ' Fără wrap: dincolo de ultimul jeton focusul rămâne pe loc.
                       bar.DebugKeyDown(Keys.Right)
                       Assert.Equal(3, bar.DebugFocusIndex())

                       bar.DebugKeyDown(Keys.Left)
                       Assert.Equal(0, bar.DebugFocusIndex())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClicPeJetonDezactivat_NuFaceNimic()
        RunSta(Sub()
                   Using bar = NewSizedBar()
                       bar.AddChip("a", "Alfa")
                       bar.SetChipEnabled("a", False)
                       Dim raised As New List(Of String)()
                       AddHandler bar.CheckedChanged, Sub(k As String) raised.Add(k)
                       bar.DebugClickAt(CentreOf(bar, 0))
                       Assert.False(bar.IsChecked("a"))
                       Assert.Empty(raised)
                   End Using
               End Sub)
    End Sub

End Class
