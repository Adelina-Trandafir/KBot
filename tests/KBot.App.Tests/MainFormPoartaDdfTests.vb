Option Strict On
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports Xunit
Imports KBot.Domain

''' <summary>
''' The «ddf» entry of the vertical navigation, after a DDF write.
'''
''' <para>The bug this pins: <c>ApplyViewGating</c> reads <c>AngajamentTreeInfo.AreDDF</c>,
''' which is filled ONCE, when the tree is loaded. Adding the first document of an angajament
''' (revision 0) writes <c>FX_Angajamente.IDDF</c> on the server, but nothing told the shell,
''' so the entry stayed hidden and the document the operator had just saved could not be
''' opened until the next year/SS change reloaded the whole tree.</para>
'''
''' <para><c>_currentInfo</c> and <c>ActualizeazaPoartaDdf</c> are private to the shell and
''' there is no seam to reach them by -- the state is the form's own. Reflection is the price
''' of testing the shell at all; the alternative was leaving the gate untested, which is how
''' the defect got in.</para>
''' </summary>
Public Class MainFormPoartaDdfTests

    ' MainForm is a WinForms form: build it on an STA thread. Its constructor only runs
    ' InitializeComponent plus field assignments, so the six dependencies can be Nothing.
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

    Private Const LEGATURI As BindingFlags =
        BindingFlags.Instance Or BindingFlags.NonPublic

    Private Shared Sub PuneSelectia(f As KBOT, info As AngajamentTreeInfo)
        Dim camp As FieldInfo = GetType(KBOT).GetField("_currentInfo", LEGATURI)
        Assert.NotNull(camp)
        camp.SetValue(f, info)
    End Sub

    Private Shared Sub Poarta(f As KBOT, cod As String, iddf As Integer, sters As Boolean)
        Dim m As MethodInfo = GetType(KBOT).GetMethod("ActualizeazaPoartaDdf", LEGATURI)
        Assert.NotNull(m)
        m.Invoke(f, New Object() {cod, iddf, sters})
    End Sub

    Private Shared Function DdfEVizibil(f As KBOT) As Boolean
        Return f.navViews.Items.First(Function(i) i.Key = "ddf").Visible
    End Function

    ''' <summary>An angajament with everything but a document -- the state before the first
    ''' revision 0 is saved.</summary>
    Private Shared Function FaraDdf(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {
            .CodAngajament = cod,
            .AreRezervari = True,
            .AreDDF = False,
            .IDDF = Nothing}
    End Function

    <Fact>
    Public Sub Salvarea_PrimuluiDocument_AprindeIntrareaDdf()
        RunSta(Sub()
                   Using f As New KBOT(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim info As AngajamentTreeInfo = FaraDdf("AN-1")
                       PuneSelectia(f, info)
                       f.navViews.SetItemVisible("ddf", False)

                       Poarta(f, "AN-1", iddf:=4242, sters:=False)

                       Assert.True(info.AreDDF)
                       ' IDDF is what the flag MEANS on the server, so it travels with it.
                       Assert.Equal(4242L, info.IDDF)
                       Assert.True(DdfEVizibil(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StergereaDocumentului_StingeIntrareaDdf()
        RunSta(Sub()
                   Using f As New KBOT(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim info As AngajamentTreeInfo = FaraDdf("AN-1")
                       info.AreDDF = True
                       info.IDDF = 4242L
                       PuneSelectia(f, info)
                       f.navViews.SetItemVisible("ddf", True)

                       Poarta(f, "AN-1", iddf:=0, sters:=True)

                       Assert.False(info.AreDDF)
                       Assert.Null(info.IDDF)
                       Assert.False(DdfEVizibil(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StergereaUneiSingureRevizii_NuAtingePoarta()
        ' The server reports `document_sters = False` there: the document stands even when the
        ' revision deleted was its last. Nothing to say -> nothing touched.
        RunSta(Sub()
                   Using f As New KBOT(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim info As AngajamentTreeInfo = FaraDdf("AN-1")
                       info.AreDDF = True
                       info.IDDF = 4242L
                       PuneSelectia(f, info)
                       f.navViews.SetItemVisible("ddf", True)

                       Poarta(f, "AN-1", iddf:=0, sters:=False)

                       Assert.True(info.AreDDF)
                       Assert.Equal(4242L, info.IDDF)
                       Assert.True(DdfEVizibil(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ScriereaPeAltAngajament_NuAtingeSelectiaCurenta()
        ' The operator can move off the node between the write and the refresh. Flipping the
        ' flag on somebody else's angajament is worse than leaving it stale.
        RunSta(Sub()
                   Using f As New KBOT(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                       Dim info As AngajamentTreeInfo = FaraDdf("AN-1")
                       PuneSelectia(f, info)
                       f.navViews.SetItemVisible("ddf", False)

                       Poarta(f, "AN-2", iddf:=4242, sters:=False)

                       Assert.False(info.AreDDF)
                       Assert.Null(info.IDDF)
                       Assert.False(DdfEVizibil(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub FaraNodSelectat_NuCrapa()
        RunSta(Sub()
                   Using f As New KBOT(Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                       PuneSelectia(f, Nothing)
                       Poarta(f, "AN-1", iddf:=4242, sters:=False)
                   End Using
               End Sub)
    End Sub

End Class
