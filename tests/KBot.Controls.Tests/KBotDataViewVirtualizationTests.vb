Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Poarta de verificare a VIRTUALIZĂRII (slice 0010-02), headless: forțăm o pictare
''' sincronă cu DrawToBitmap și citim câte rânduri de date a atins efectiv OnPaint
''' (<c>DebugLastPaintedDataRows</c>, vizibil prin InternalsVisibleTo).
'''
''' Dovada esențială: numărul de rânduri pictate NU depinde de RowCount.
''' </summary>
Public Class KBotDataViewVirtualizationTests

    Private Const ViewW As Integer = 800
    Private Const ViewH As Integer = 400

    ' Construiește o grilă de dimensiune fixă cu rowCount × 20 coloane și o pictează o dată.
    Private Shared Function PaintedRowsFor(rowCount As Integer) As Integer
        Using dv As New KBotDataView()
            dv.Size = New Size(ViewW, ViewH)
            dv.ApplyTheme(BuiltInSchemes.Classic())

            dv.BeginUpdate()
            For c As Integer = 0 To 19
                dv.AddColumn("c" & c.ToString(), "Coloana " & c.ToString(), KBotColumnType.Text, 90)
            Next
            For r As Integer = 0 To rowCount - 1
                Dim row = dv.AddRow()
                row("c0") = "rand " & r.ToString()
            Next
            dv.EndUpdate()

            Using bmp As New Bitmap(ViewW, ViewH)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, ViewW, ViewH))
            End Using

            Return dv.DebugLastPaintedDataRows
        End Using
    End Function

    <Fact>
    Public Sub Paints_OnlyVisibleRows_NotAllFiveThousand()
        Dim painted As Integer = PaintedRowsFor(5000)
        Assert.True(painted > 0, "trebuie pictat cel puțin un rând")
        ' Fereastra de 400px la RowHeight=28 ține ~13 rânduri; +2 marja de virtualizare.
        Assert.True(painted <= 20, $"s-au pictat {painted} rânduri — virtualizarea nu funcționează")
    End Sub

    <Fact>
    Public Sub PaintedRowCount_IsIndependentOfRowCount()
        ' Dovada că e O(fereastră), nu O(rânduri): de 10× mai multe rânduri, aceeași muncă.
        Assert.Equal(PaintedRowsFor(5000), PaintedRowsFor(50000))
    End Sub

    <Fact>
    Public Sub EmptyGrid_PaintsNoDataRows()
        Using dv As New KBotDataView()
            dv.Size = New Size(ViewW, ViewH)
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            Using bmp As New Bitmap(ViewW, ViewH)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, ViewW, ViewH))
            End Using
            Assert.Equal(0, dv.DebugLastPaintedDataRows)
        End Using
    End Sub

End Class
