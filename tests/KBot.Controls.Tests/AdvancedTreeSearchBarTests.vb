Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' English: the search bar of <see cref="AdvancedTreeControl"/>. The regression these tests pin
''' down: <c>SearchShow = True</c> set from a form designer used to do NOTHING — the only code that
''' opened the bar lived in <c>ResolveHeaderIcons</c>, the XML-builder path, which a designer-authored
''' form never calls. The bar is now opened by the property itself and re-applied on handle creation.
'''
''' The observable seam is the control's child collection: at runtime the open bar owns a real
''' TextBox (the search box) and a Label (the «Cautare:» caption). What these tests CANNOT prove is
''' how the bar LOOKS — including the design-time painted replica, which by definition only runs
''' inside the Visual Studio designer. That verdict is still owed on screen.
''' </summary>
Public Class AdvancedTreeSearchBarTests

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

    ' Caseta de căutare = singurul TextBox copil al arborelui.
    Private Shared Function SearchBox(tree As AdvancedTreeControl) As TextBox
        Return tree.Controls.OfType(Of TextBox)().FirstOrDefault()
    End Function

    Private Shared Function VisibleLabels(tree As AdvancedTreeControl) As Integer
        Return tree.Controls.OfType(Of Label)().Count(Function(l) l.Visible)
    End Function

    <Fact>
    Public Sub SearchShow_deschide_banda_fara_iconita_de_toggle()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       Assert.Null(SearchBox(tree))

                       tree.SearchShow = True

                       Dim box As TextBox = SearchBox(tree)
                       Assert.NotNull(box)
                       Assert.True(box.Visible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SearchShow_False_inchide_banda()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True
                       Assert.True(SearchBox(tree).Visible)

                       tree.SearchShow = False

                       Assert.False(SearchBox(tree).Visible)
                       Assert.Equal(0, VisibleLabels(tree))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Cu iconiță de căutare în antet banda NU se auto-deschide: iconița o comută. Regula veche
    ''' din ResolveHeaderIcons, păstrată — SearchShow înseamnă «permisă», nu «deschisă».
    ''' </summary>
    <Fact>
    Public Sub SearchShow_cu_iconita_de_antet_nu_deschide_banda()
        RunSta(Sub()
                   Using ico As New Bitmap(16, 16)
                       Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                           tree.HeaderSearchIcon = ico
                           tree.SearchShow = True

                           Assert.Null(SearchBox(tree))
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Ridicarea iconiței de toggle transformă banda în una permanentă: SearchShow e re-evaluat
    ''' din setterul iconiței, nu doar la construcție (ordinea din InitializeComponent nu e a noastră).
    ''' </summary>
    <Fact>
    Public Sub Stergerea_iconitei_de_antet_face_banda_permanenta()
        RunSta(Sub()
                   Using ico As New Bitmap(16, 16)
                       Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                           tree.HeaderSearchIcon = ico
                           tree.SearchShow = True
                           Assert.Null(SearchBox(tree))

                           tree.HeaderSearchIcon = Nothing

                           Assert.NotNull(SearchBox(tree))
                           Assert.True(SearchBox(tree).Visible)
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>Crearea handle-ului re-aplică SearchShow — banda rămâne deschisă, fără dubluri.</summary>
    <Fact>
    Public Sub Crearea_handle_ului_nu_dubleaza_banda()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True

                       Dim unused As IntPtr = tree.Handle       ' OnHandleCreated → ApplySearchShow

                       Assert.Single(tree.Controls.OfType(Of TextBox)())
                       Assert.True(SearchBox(tree).Visible)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Butonul ✕ se poate comuta și DUPĂ deschiderea benzii (playground-ul o face).</summary>
    <Fact>
    Public Sub SearchClearButton_comutat_dupa_deschidere_creeaza_butonul()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True
                       Dim inainte As Integer = tree.Controls.OfType(Of Label)().Count()

                       tree.SearchClearButton = True

                       Assert.Equal(inainte + 1, tree.Controls.OfType(Of Label)().Count())
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Design-time: banda NU-și creează controale copil (un TextBox viu în designer ar fura
    ''' click-urile) — se DESENEAZĂ. Testul montează arborele sub un părinte «sitat» în design
    ''' mode și verifică ambele jumătăți: zero copii noi și un paint care nu aruncă.
    ''' Ce NU dovedește: cum arată replica desenată. Verdictul vizual rămâne dator.
    ''' </summary>
    <Fact>
    Public Sub In_designer_banda_se_deseneaza_fara_controale_copil()
        RunSta(Sub()
                   Using gazda As New Panel() With {.Width = 400, .Height = 300}
                       gazda.Site = New FakeDesignSite()
                       Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                           gazda.Controls.Add(tree)

                           tree.HeaderVisible = True
                           tree.SearchShow = True
                           tree.SearchClearButton = True
                           tree.SearchDefaultText = "minim 3 caractere"

                           Assert.Empty(tree.Controls.OfType(Of TextBox)())
                           Assert.Empty(tree.Controls.OfType(Of Label)())

                           Using bmp As New Bitmap(tree.Width, tree.Height)
                               tree.DrawToBitmap(bmp, New Rectangle(0, 0, tree.Width, tree.Height))
                           End Using
                       End Using
                   End Using
               End Sub)
    End Sub

    ' Site minimal cu DesignMode = True: singurul lucru pe care InDesigner îl citește de la părinte.
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

    ''' <summary>
    ''' ESC golește caseta. Pe o casetă deja goală ESC ar închide banda — dar o bandă permanentă
    ''' (SearchShow fără iconiță de toggle) nu se închide, deci acolo ESC doar golește.
    ''' </summary>
    <Fact>
    Public Sub Escape_goleste_caseta_de_cautare()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True
                       Dim caseta As TextBox = SearchBox(tree)
                       caseta.Text = "angajament"

                       caseta.GetType()      ' KeyDown se ridică prin handler-ul intern al arborelui
                       SendKeyDown(caseta, Keys.Escape)

                       Assert.Equal(String.Empty, caseta.Text)
                       Assert.True(SearchBox(tree).Visible)   ' banda permanentă rămâne deschisă
                   End Using
               End Sub)
    End Sub

    ''' <summary>Butonul de golire își ia lățimea din imagine + SearchClearButtonPadding.</summary>
    <Fact>
    Public Sub Latimea_butonului_de_golire_include_padding_ul()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True
                       tree.SearchClearButton = True
                       tree.SearchClearButtonPadding = New Padding(6)

                       Dim buton As Label = tree.Controls.OfType(Of Label)().
                                                 Single(Function(l) l.Text = "✕" OrElse l.Image IsNot Nothing)
                       ' 18 (glifa implicită) + 6 stânga + 6 dreapta
                       Assert.Equal(18 + 12, buton.Width)

                       Using img As New Bitmap(24, 24)
                           tree.SearchClearButtonImage = img
                           Assert.Equal(24 + 12, buton.Width)
                           Assert.Same(img, buton.Image)
                           Assert.Equal(String.Empty, buton.Text)
                       End Using
                   End Using
               End Sub)
    End Sub

    ' TextBox nu expune OnKeyDown public: ridicăm evenimentul prin reflecție, ca handler-ul
    ' intern al arborelui (OnSearchTextBoxKeyDown) să ruleze exact ca la o tastă reală.
    Private Shared Sub SendKeyDown(target As Control, key As Keys)
        Dim m = GetType(Control).GetMethod("OnKeyDown",
                    Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        m.Invoke(target, New Object() {New KeyEventArgs(key)})
    End Sub

    ''' <summary>Eticheta benzii urmează SearchBarLabelText, inclusiv apariția/dispariția ei.</summary>
    <Fact>
    Public Sub SearchBarLabelText_gol_ascunde_eticheta()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl() With {.Width = 300, .Height = 200}
                       tree.SearchShow = True
                       Assert.Equal(1, VisibleLabels(tree))

                       tree.SearchBarLabelText = String.Empty
                       Assert.Equal(0, VisibleLabels(tree))

                       tree.SearchBarLabelText = "Filtru: "
                       Assert.Equal(1, VisibleLabels(tree))
                       Assert.Contains(tree.Controls.OfType(Of Label)(),
                                       Function(l) l.Visible AndAlso l.Text = "Filtru: ")
                   End Using
               End Sub)
    End Sub
End Class
