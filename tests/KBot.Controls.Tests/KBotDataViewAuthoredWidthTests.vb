Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028-05: cine deține lățimea unei coloane. Două reguli, amândouă găsite pe un caz real
''' (`IstoricView`, unde prima coloană ajunsese la `Width = 747` scris în `.Designer.vb`):
'''
'''  1. în DESIGNER trecerea de auto-dimensionare nu rulează deloc — altfel scrie `Width`, iar
'''     designerul serializează rezultatul ca și cum l-ar fi tastat operatorul;
'''  2. la RULARE trecerea pleacă mereu de la lățimea CERUTĂ de caller, nu de la ce a lăsat
'''     trecerea dinainte — o grilă îngustată temporar altfel ar păstra coloanele strâmtate.
''' </summary>
Public Class KBotDataViewAuthoredWidthTests

    ' Site minimal cu DesignMode = True — singurul semnal pe care KBotDesignTime îl citește aici.
    Private NotInheritable Class FakeDesignSite
        Implements ISite

        Public Property Name As String Implements ISite.Name
        Public ReadOnly Property Component As IComponent Implements ISite.Component
            Get
                Return Nothing
            End Get
        End Property
        Public ReadOnly Property Container As IContainer Implements ISite.Container
            Get
                Return Nothing
            End Get
        End Property
        Public ReadOnly Property DesignMode As Boolean Implements ISite.DesignMode
            Get
                Return True
            End Get
        End Property
        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            Return Nothing
        End Function
    End Class

    ' Aranjamentul din IstoricView: trei coloane fixe + ultima care se întinde.
    Private Shared Function ThreeFixedPlusFiller(dv As KBotDataView) As KBotDataView
        dv.ColumnFillMode = KBotFillMode.LastColumn
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.BeginUpdate()
        dv.AddColumn("clsf", "Clasificație", KBotColumnType.Text, 300)
        dv.AddColumn("tip", "Tip", KBotColumnType.Text, 200)
        dv.AddColumn("data", "Data", KBotColumnType.Text, 200)
        dv.AddColumn("desc", "Descriere", KBotColumnType.Text, 250)
        dv.AddRow()
        dv.EndUpdate()
        Return dv
    End Function

    <Fact>
    Public Sub InDesigner_ThePassNeverRuns_WidthsStayAsAuthored()
        Using dv As New KBotDataView()
            dv.Site = New FakeDesignSite()
            dv.Size = New Size(500, 300)               ' mult mai îngustă decât suma lățimilor
            dv.ApplyTheme(BuiltInSchemes.Classic())
            ThreeFixedPlusFiller(dv)

            ' Nici strâmtare, nici umplere: exact ce a autorat operatorul — altfel valorile
            ' calculate ar ajunge în .Designer.vb ca alegere a lui.
            Assert.Equal(300, dv.Column("clsf").Width)
            Assert.Equal(200, dv.Column("tip").Width)
            Assert.Equal(200, dv.Column("data").Width)
            Assert.Equal(250, dv.Column("desc").Width)
        End Using
    End Sub

    <Fact>
    Public Sub InDesigner_ToContentDoesNotMeasureEither()
        Using dv As New KBotDataView()
            dv.Site = New FakeDesignSite()
            dv.Size = New Size(1200, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 60)   ' grila e pe ToContent implicit
            Dim r = dv.AddRow()
            r("a") = New String("W"c, 60)                     ' conținut mult mai lat
            dv.EndUpdate()

            Assert.Equal(60, dv.Column("a").Width)
        End Using
    End Sub

    <Fact>
    Public Sub Shrink_IsNotDestructive_WideningRestoresTheAuthoredWidths()
        Using dv As New KBotDataView()
            dv.Size = New Size(500, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            ThreeFixedPlusFiller(dv)

            ' 500 < 950: strâmtarea a mușcat din ele (fără să coboare sub podea).
            Assert.True(dv.Column("tip").Width < 200, "pe îngust coloanele chiar se strâmtează")
            Assert.Equal(dv.ClientSize.Width, dv.Column("clsf").Width + dv.Column("tip").Width +
                                              dv.Column("data").Width + dv.Column("desc").Width)

            dv.Size = New Size(1400, 300)

            ' … iar la lărgire se întorc EXACT la ce a cerut caller-ul, nu rămân strâmte.
            Assert.Equal(300, dv.Column("clsf").Width)
            Assert.Equal(200, dv.Column("tip").Width)
            Assert.Equal(200, dv.Column("data").Width)
            Assert.True(dv.Column("desc").Width > 250, "ultima ia diferența")
            Assert.Equal(dv.ClientSize.Width, dv.Column("clsf").Width + dv.Column("tip").Width +
                                              dv.Column("data").Width + dv.Column("desc").Width)
        End Using
    End Sub

    <Fact>
    Public Sub AuthoredWidth_SurvivesRepeatedPasses()
        ' Aceeași lățime disponibilă => același rezultat, oricâte treceri s-ar face: trecerea e o
        ' funcție de (lățimi cerute, spațiu disponibil), nu de istoricul redimensionărilor.
        Using dv As New KBotDataView()
            dv.Size = New Size(1000, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            ThreeFixedPlusFiller(dv)
            Dim after1 As Integer = dv.Column("desc").Width

            dv.AutoSizeColumns()
            dv.AutoSizeColumns()

            Assert.Equal(300, dv.Column("clsf").Width)
            Assert.Equal(200, dv.Column("tip").Width)
            Assert.Equal(after1, dv.Column("desc").Width)
        End Using
    End Sub

    <Fact>
    Public Sub ADragBecomesTheNewAuthoredWidth()
        ' Lățimea trasă de operator e a lui: o trecere ulterioară n-o mai readuce la valoarea
        ' din designer (pentru asta există ResetColumnSizing).
        Using dv As New KBotDataView()
            dv.Size = New Size(1400, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            ThreeFixedPlusFiller(dv)

            dv.Column("tip").Width = 320                  ' ca și cum ar fi tras marginea
            dv.Column("tip").UserSized = True
            dv.AutoSizeColumns()

            Assert.Equal(320, dv.Column("tip").Width)
        End Using
    End Sub

End Class
