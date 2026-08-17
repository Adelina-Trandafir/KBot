Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Mărirea textului trebuie să ajungă și la fonturile pe care controalele NOASTRE le țin în
''' proprietăți proprii, nu doar la <c>Control.Font</c>.
'''
''' <para>Defectul reparat: toate coloanele grilei din vederi au <c>ColumnFont</c> pus în designer
''' (Calibri 9), iar <c>AppScaling.ApplyTextScale</c> nu atinge decât <c>Control.Font</c> — deci
''' rândurile creșteau cu cursorul, iar textul din celule rămânea la 9. Vezi
''' <c>AppScaling.ScaledFont</c>.</para>
''' </summary>
Public Class TextScaleFontsTests

    ' Fiecare test își pune scara înapoi pe 1: AppScaling e un modul, adică stare de proces.
    Private Shared Sub LaScara(scara As Single, proba As Action)
        AppScaling.SetTextScale(scara)
        Try
            proba()
        Finally
            AppScaling.SetTextScale(1.0F)
        End Try
    End Sub

    <Fact>
    Public Sub ColumnFont_fixat_creste_cu_marimea_textului()
        Using g As New KBotDataView()
            Dim col As New KBotDataColumn("cod", "Cod", KBotColumnType.Text, 100) With {
                .ColumnFont = New Font("Calibri", 9.0F, FontStyle.Bold)
            }
            g.Columns.Add(col)

            Assert.Equal(9.0F, g.CellFontFor(col).Size, 3)

            LaScara(1.5F, Sub()
                              Dim f As Font = g.CellFontFor(col)
                              Assert.Equal(13.5F, f.Size, 3)
                              ' Familia și stilul rămân cele autorite — se mărește doar mărimea.
                              Assert.Equal("Calibri", f.FontFamily.Name)
                              Assert.Equal(FontStyle.Bold, f.Style)
                          End Sub)

            ' Proprietatea publică rămâne LOGICĂ: designerul serializează 9, nu 13,5.
            Assert.Equal(9.0F, col.ColumnFont.Size, 3)
        End Using
    End Sub

    <Fact>
    Public Sub Fara_ColumnFont_celula_ia_fontul_ambient()
        Using g As New KBotDataView()
            Dim col As New KBotDataColumn("cod", "Cod", KBotColumnType.Text, 100)
            g.Columns.Add(col)
            ' Fontul ambient e mărit de AppScaling prin Control.Font, deci aici NU se mai înmulțește
            ' încă o dată — altfel s-ar dubla.
            LaScara(1.5F, Sub() Assert.Equal(g.Font.Size, g.CellFontFor(col).Size, 3))
        End Using
    End Sub

    <Fact>
    Public Sub HeaderFont_fixat_pe_grila_creste_cu_marimea_textului()
        Using g As New KBotDataView()
            g.HeaderFont = New Font("Calibri", 10.0F, FontStyle.Bold)
            LaScara(2.0F, Sub() Assert.Equal(20.0F, g.ResolvedHeaderFont().Size, 3))
            Assert.Equal(10.0F, g.HeaderFont.Size, 3)
        End Using
    End Sub

    <Fact>
    Public Sub HeaderFont_fixat_pe_arbore_creste_cu_marimea_textului()
        Using t As New AdvancedTreeControl()
            t.HeaderFont = New Font("Calibri", 10.0F, FontStyle.Bold)
            LaScara(1.5F, Sub() Assert.Equal(15.0F, t.HeaderFont.Size, 3))
            Assert.Equal(10.0F, t.HeaderFont.Size, 3)
        End Using
    End Sub

    <Fact>
    Public Sub SearchBarFont_NU_se_mareste_in_proprietate()
        ' Ajunge pe controale reale, care sunt mărite deja prin Control.Font — o mărire și aici ar
        ' da-o de două ori.
        Using t As New AdvancedTreeControl()
            t.SearchBarFont = New Font("Calibri", 10.0F)
            LaScara(1.5F, Sub() Assert.Equal(10.0F, t.SearchBarFont.Size, 3))
        End Using
    End Sub

    <Fact>
    Public Sub La_scara_1_fontul_fixat_e_chiar_obiectul_autorit()
        Using g As New KBotDataView()
            Dim autorit As New Font("Calibri", 9.0F)
            Dim col As New KBotDataColumn("cod", "Cod", KBotColumnType.Text, 100) With {.ColumnFont = autorit}
            g.Columns.Add(col)
            Assert.Same(autorit, g.CellFontFor(col))
        End Using
    End Sub

End Class
