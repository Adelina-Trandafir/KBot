Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru logica non-vizuală adusă de slice 0010-03: exclusivitatea grupurilor de
''' opțiuni și scalarea barei de progres. Pictarea propriu-zisă se validează în harness.
''' </summary>
Public Class KBotDataViewColumnTypeTests

    ' ── Exclusivitatea OptionGroup ──────────────────────────────────────────────

    Private Shared Function GridWithOptions() As KBotDataView
        Dim dv As New KBotDataView()
        dv.AddColumn("a", "A", KBotColumnType.OptionButton, 40).OptionGroup = "g1"
        dv.AddColumn("b", "B", KBotColumnType.OptionButton, 40).OptionGroup = "g1"
        dv.AddColumn("c", "C", KBotColumnType.OptionButton, 40).OptionGroup = "g2"
        dv.AddColumn("liber", "Liber", KBotColumnType.OptionButton, 40)   ' fără grup
        dv.AddRow()
        dv.AddRow()
        Return dv
    End Function

    <Fact>
    Public Sub SetOption_ClearsSiblingsInSameGroupAndRow()
        Using dv = GridWithOptions()
            dv.SetOptionValue("a", 0, True)
            dv.SetOptionValue("b", 0, True)          ' trebuie s-o stingă pe „a”
            Assert.Equal(False, dv("a", 0))
            Assert.Equal(True, dv("b", 0))
        End Using
    End Sub

    <Fact>
    Public Sub SetOption_DoesNotTouchOtherGroups()
        Using dv = GridWithOptions()
            dv.SetOptionValue("c", 0, True)          ' grupul g2
            dv.SetOptionValue("a", 0, True)          ' grupul g1
            Assert.Equal(True, dv("c", 0))           ' g2 rămâne neatins
            Assert.Equal(True, dv("a", 0))
        End Using
    End Sub

    <Fact>
    Public Sub SetOption_DoesNotTouchOtherRows()
        Using dv = GridWithOptions()
            dv.SetOptionValue("a", 0, True)
            dv.SetOptionValue("b", 1, True)          ' alt rând, același grup
            Assert.Equal(True, dv("a", 0))           ' rândul 0 nu e afectat
            Assert.Equal(True, dv("b", 1))
        End Using
    End Sub

    <Fact>
    Public Sub SetOption_UngroupedIsIndependent()
        Using dv = GridWithOptions()
            dv.SetOptionValue("liber", 0, True)
            dv.SetOptionValue("a", 0, True)
            Assert.Equal(True, dv("liber", 0))       ' fără grup => nu se stinge
        End Using
    End Sub

    <Fact>
    Public Sub SetOption_RejectsNonOptionColumn()
        Using dv As New KBotDataView()
            dv.AddColumn("t", "T", KBotColumnType.Text, 40)
            dv.AddRow()
            Assert.Throws(Of ArgumentException)(Sub() dv.SetOptionValue("t", 0, True))
            Assert.Throws(Of ArgumentException)(Sub() dv.SetOptionValue("inexistent", 0, True))
        End Using
    End Sub

    ' ── Scalarea barei de progres ───────────────────────────────────────────────

    <Fact>
    Public Sub ProgressFraction_ScalesAndClamps()
        Dim col As New KBotDataColumn("p", "P", KBotColumnType.ProgressBar, 100)   ' 0..100 implicit
        Assert.Equal(0.0, KBotDataView.ProgressFraction(0, col))
        Assert.Equal(0.5, KBotDataView.ProgressFraction(50, col))
        Assert.Equal(1.0, KBotDataView.ProgressFraction(100, col))
        Assert.Equal(0.0, KBotDataView.ProgressFraction(-20, col))    ' sub minim => 0
        Assert.Equal(1.0, KBotDataView.ProgressFraction(999, col))    ' peste maxim => 1
        Assert.Equal(0.0, KBotDataView.ProgressFraction(Nothing, col))
    End Sub

    <Fact>
    Public Sub ProgressFraction_HonoursCustomRange()
        Dim col As New KBotDataColumn("p", "P", KBotColumnType.ProgressBar, 100)
        col.ProgressMin = 10
        col.ProgressMax = 20
        Assert.Equal(0.0, KBotDataView.ProgressFraction(10, col))
        Assert.Equal(0.5, KBotDataView.ProgressFraction(15, col))
        Assert.Equal(1.0, KBotDataView.ProgressFraction(20, col))
    End Sub

    <Fact>
    Public Sub ProgressFraction_DegenerateRangeIsZero()
        Dim col As New KBotDataColumn("p", "P", KBotColumnType.ProgressBar, 100)
        col.ProgressMin = 5
        col.ProgressMax = 5              ' interval nul => nu împărțim la zero
        Assert.Equal(0.0, KBotDataView.ProgressFraction(5, col))
    End Sub

    ' ── Toate cele șase tipuri se pictează fără excepție ────────────────────────

    <Fact>
    Public Sub AllSixColumnTypes_PaintWithoutThrowing()
        Using dv As New KBotDataView()
            dv.Size = New Size(700, 300)
            dv.ApplyTheme(BuiltInSchemes.Dark())
            dv.AddColumn("txt", "Text", KBotColumnType.Text, 100)
            dv.AddColumn("cmb", "Combo", KBotColumnType.Combo, 100)
            dv.AddColumn("chk", "Check", KBotColumnType.CheckBox, 60)
            dv.AddColumn("opt", "Option", KBotColumnType.OptionButton, 60)
            dv.AddColumn("btn", "Buton", KBotColumnType.Button, 100)
            dv.AddColumn("prg", "Progres", KBotColumnType.ProgressBar, 120)

            Dim row = dv.AddRow()
            row("txt") = "abc"
            row("cmb") = "optiune"
            row("chk") = True
            row("opt") = True
            row("prg") = 42

            Using bmp As New Bitmap(700, 300)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, 700, 300))
            End Using

            ' Dacă OnPaint ar fi aruncat, l-ar fi înghițit — deci verificăm că a pictat rândul.
            Assert.Equal(1, dv.DebugLastPaintedDataRows)
        End Using
    End Sub

End Class
