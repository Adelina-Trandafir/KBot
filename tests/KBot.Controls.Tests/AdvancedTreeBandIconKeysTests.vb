Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' The SECOND picker for every icon the tree draws outside its nodes — header, footer, collapse
''' button, search clear button. Each of them has always had an <c>Image</c> property, which the
''' designer's image picker fills by writing the picture into the host form's .resx; each of them
''' now also has a <c>*Key</c> property that points at a picture already held by the
''' <see cref="ImageList"/> bound to <c>NodeImages</c>, offered as a drop-down by
''' <see cref="TreeImageKeyConverter"/>.
'''
''' The rule worth pinning: a key only wins when it FINDS a picture, so a stale or misspelled key
''' never wipes an icon the operator picked directly.
''' </summary>
Public Class AdvancedTreeBandIconKeysTests

    ' Every non-node icon, as the pair the operator sees in the property grid.
    Private Shared ReadOnly Bands As String()() = {
        New String() {"HeaderLeftIconKey", "HeaderLeftIcon"},
        New String() {"HeaderRightIconKey", "HeaderRightIcon"},
        New String() {"HeaderSearchIconKey", "HeaderSearchIcon"},
        New String() {"FooterLeftIconKey", "FooterLeftIcon"},
        New String() {"FooterRightIconKey", "FooterRightIcon"},
        New String() {"FooterCollapseExpandedImageKey", "FooterCollapseExpandedImage"},
        New String() {"FooterCollapseCollapsedImageKey", "FooterCollapseCollapsedImage"},
        New String() {"SearchClearButtonImageKey", "SearchClearButtonImage"}
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

    ' The bitmaps are NOT disposed here — see AdvancedTreeDesignerNodesTests: until its native
    ' handle exists, the ImageList holds the ORIGINAL image, so a bitmap freed right after Add
    ' makes the first read throw. The ImageList owns them, and the test owns the ImageList.
    Private Shared Function NewImageList() As ImageList
        Dim il As New ImageList() With {.ImageSize = New Size(16, 16)}
        il.Images.Add("frunza", SolidBitmap(Color.SteelBlue))
        il.Images.Add("grup", SolidBitmap(Color.Goldenrod))
        Return il
    End Function

    Private Shared Function SolidBitmap(culoare As Color) As Bitmap
        Dim bmp As New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(culoare)
        End Using
        Return bmp
    End Function

    Private Shared Sub SetProp(tree As AdvancedTreeControl, name As String, value As Object)
        Dim pi As PropertyInfo = GetType(AdvancedTreeControl).GetProperty(name)
        Assert.True(pi IsNot Nothing, $"Proprietatea {name} lipseste")
        pi.SetValue(tree, value)
    End Sub

    Private Shared Function GetProp(tree As AdvancedTreeControl, name As String) As Object
        Dim pi As PropertyInfo = GetType(AdvancedTreeControl).GetProperty(name)
        Assert.True(pi IsNot Nothing, $"Proprietatea {name} lipseste")
        Return pi.GetValue(tree)
    End Function

    ''' <summary>The key written BEFORE the list still resolves — the designer picks its own order.</summary>
    <Fact>
    Public Sub Every_band_icon_resolves_when_the_key_is_written_first()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       For Each band As String() In Bands
                           Using tree As New AdvancedTreeControl()
                               SetProp(tree, band(0), "grup")
                               tree.NodeImages = il
                               Assert.True(GetProp(tree, band(1)) IsNot Nothing,
                                           $"{band(1)} a ramas gol dupa {band(0)}")
                           End Using
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>The list written BEFORE the key resolves too.</summary>
    <Fact>
    Public Sub Every_band_icon_resolves_when_the_list_is_bound_first()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       For Each band As String() In Bands
                           Using tree As New AdvancedTreeControl()
                               tree.NodeImages = il
                               SetProp(tree, band(0), "grup")
                               Assert.True(GetProp(tree, band(1)) IsNot Nothing,
                                           $"{band(1)} a ramas gol dupa {band(0)}")
                           End Using
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' A key that finds nothing leaves the picture chosen with the image picker alone. Without
    ''' this, a renamed ImageList key would silently blank a band the operator had already set.
    ''' </summary>
    <Fact>
    Public Sub A_key_that_finds_nothing_does_not_wipe_the_picked_image()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       For Each band As String() In Bands
                           Using tree As New AdvancedTreeControl()
                               Using ales As Bitmap = SolidBitmap(Color.Firebrick)
                                   SetProp(tree, band(1), ales)
                                   tree.NodeImages = il
                                   SetProp(tree, band(0), "cheie-inexistenta")
                                   Assert.Same(ales, GetProp(tree, band(1)))
                               End Using
                           End Using
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' The path the property grid actually takes: the converter attached to the property offers
    ''' the bound list's keys as a drop-down, and stays non-exclusive so a key that will only
    ''' exist at runtime can still be typed.
    ''' </summary>
    <Fact>
    Public Sub The_key_picker_offers_the_keys_of_the_bound_image_list()
        RunSta(Sub()
                   Using il As ImageList = NewImageList()
                       Using tree As New AdvancedTreeControl()
                           tree.NodeImages = il
                           For Each band As String() In Bands
                               Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)(band(0))
                               Assert.NotNull(pd)
                               Assert.IsType(Of TreeImageKeyConverter)(pd.Converter)

                               Dim ctx As New FakeContext(tree)
                               Assert.True(pd.Converter.GetStandardValuesSupported(ctx))
                               Assert.False(pd.Converter.GetStandardValuesExclusive(ctx))

                               Dim oferite As String() = pd.Converter.GetStandardValues(ctx).
                                   Cast(Of String)().ToArray()
                               Assert.Contains("frunza", oferite)
                               Assert.Contains("grup", oferite)
                               Assert.Equal(String.Empty, oferite(0))   ' «fara iconita» prima
                           Next
                       End Using
                   End Using
               End Sub)
    End Sub

    ''' <summary>No bound list = just the empty key; the grid must not throw over it.</summary>
    <Fact>
    Public Sub The_key_picker_is_empty_without_a_bound_image_list()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)("HeaderLeftIconKey")
                       Dim oferite As String() = pd.Converter.GetStandardValues(New FakeContext(tree)).
                           Cast(Of String)().ToArray()
                       Assert.Single(oferite)
                       Assert.Equal(String.Empty, oferite(0))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' The designer stays quiet about an untouched key — the ShouldSerialize trap, checked the
    ''' way Visual Studio checks it.
    ''' </summary>
    <Fact>
    Public Sub An_untouched_key_writes_no_designer_line()
        RunSta(Sub()
                   Using tree As New AdvancedTreeControl()
                       For Each band As String() In Bands
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(tree)(band(0))
                           Assert.False(pd.ShouldSerializeValue(tree), $"{band(0)} se serializeaza degeaba")
                           SetProp(tree, band(0), "grup")
                           Assert.True(pd.ShouldSerializeValue(tree), $"{band(0)} nu se serializeaza dupa ce e setata")
                       Next
                   End Using
               End Sub)
    End Sub

    ''' <summary>The least a type converter needs to be asked for standard values.</summary>
    Private NotInheritable Class FakeContext
        Implements ITypeDescriptorContext

        Private ReadOnly _instance As Object

        Public Sub New(instance As Object)
            _instance = instance
        End Sub

        Public ReadOnly Property Container As IContainer Implements ITypeDescriptorContext.Container
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Instance As Object Implements ITypeDescriptorContext.Instance
            Get
                Return _instance
            End Get
        End Property

        Public ReadOnly Property PropertyDescriptor As PropertyDescriptor Implements ITypeDescriptorContext.PropertyDescriptor
            Get
                Return Nothing
            End Get
        End Property

        Public Sub OnComponentChanged() Implements ITypeDescriptorContext.OnComponentChanged
        End Sub

        Public Function OnComponentChanging() As Boolean Implements ITypeDescriptorContext.OnComponentChanging
            Return True
        End Function

        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            Return Nothing
        End Function
    End Class

End Class
