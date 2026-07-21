Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru proprietatea <c>ScrollByColumn</c>: derularea orizontală se aliniază la
''' marginile coloanelor. Grila e fixată pe dimensionare MANUALĂ (AutoSize/Fill = None) ca
''' lățimile — deci și marginile de coloană — să fie deterministe.
'''
''' Setup: fereastră 200px lată, 5 coloane × 100px => banda derulată are marginile
''' 0/100/200/300/400 (fără coloane înghețate); bara orizontală devine vizibilă (500 > 200)
''' cu maximul util 300 (Maximum 499 − LargeChange 200 + 1).
''' </summary>
Public Class KBotDataViewScrollByColumnTests

    Private Shared Function WideGrid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None    ' păstrează lățimile exact
        dv.ColumnFillMode = KBotFillMode.None
        dv.Size = New Size(200, 200)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        For i As Integer = 0 To 4
            dv.AddColumn("c" & i.ToString(), "C" & i.ToString(), KBotColumnType.Text, 100)
        Next
        dv.AddRow()
        dv.AddRow()
        Return dv
    End Function

    <Fact>
    Public Sub DefaultsToPixelScrolling()
        Using dv = WideGrid()
            Assert.False(dv.ScrollByColumn)
            Assert.True(dv.hScroll.Visible, "bara orizontală trebuie să fie vizibilă la 500 > 200")

            dv.hScroll.Value = 130
            Assert.Equal(130, dv.hScroll.Value)          ' pixel cu pixel, fără aliniere
        End Using
    End Sub

    <Fact>
    Public Sub ScrollingForward_SnapsUpToNextColumnEdge()
        Using dv = WideGrid()
            dv.ScrollByColumn = True
            dv.hScroll.Value = 130                        ' între marginile 100 și 200
            Assert.Equal(200, dv.hScroll.Value)          ' creștere => marginea următoare
        End Using
    End Sub

    <Fact>
    Public Sub ScrollingBackward_SnapsDownToPreviousColumnEdge()
        Using dv = WideGrid()
            dv.ScrollByColumn = True
            dv.hScroll.Value = 200                        ' aliniat pe marginea coloanei 2
            dv.hScroll.Value = 170                        ' scădere, între 100 și 200
            Assert.Equal(100, dv.hScroll.Value)          ' scădere => marginea precedentă
        End Using
    End Sub

    <Fact>
    Public Sub SmallForwardStep_StillAdvancesAFullColumn()
        Using dv = WideGrid()
            dv.ScrollByColumn = True
            dv.hScroll.Value = 1                          ' un pas mic de la 0
            Assert.Equal(100, dv.hScroll.Value)          ' nu se lipește de 0
        End Using
    End Sub

    <Fact>
    Public Sub SnappedValueNeverExceedsUsefulMaximum()
        Using dv = WideGrid()
            dv.ScrollByColumn = True
            dv.hScroll.Value = 290                        ' aproape de capăt; ceil(290)=300
            Assert.Equal(300, dv.hScroll.Value)          ' 300 = maximul util, tot o margine
            Assert.True(dv.hScroll.Value <= dv.hScroll.Maximum - dv.hScroll.LargeChange + 1)
        End Using
    End Sub

    <Fact>
    Public Sub EnablingSnapsTheCurrentPixelOffset()
        Using dv = WideGrid()
            dv.hScroll.Value = 130                        ' derulare pixel cu pixel (încă off)
            Assert.Equal(130, dv.hScroll.Value)
            dv.ScrollByColumn = True                      ' activarea aliniază pe loc
            Assert.Equal(200, dv.hScroll.Value)
        End Using
    End Sub

    <Fact>
    Public Sub AlreadyOnAnEdge_DoesNotMove()
        Using dv = WideGrid()
            dv.ScrollByColumn = True
            dv.hScroll.Value = 100
            Assert.Equal(100, dv.hScroll.Value)          ' deja pe margine => neschimbat
        End Using
    End Sub

End Class
