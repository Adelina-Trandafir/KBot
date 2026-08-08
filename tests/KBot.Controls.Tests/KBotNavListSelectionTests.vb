Option Strict On
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Theming

''' <summary>
''' <see cref="KBotNavList.SelectedKey"/> and the difference between «no selection» and «wrong key».
'''
''' WHY THIS SUITE EXISTS. The WinForms designer serialises a String property left as Nothing by
''' emitting <c>navSub.SelectedKey = Nothing</c>. The setter used to reject that, so the first time
''' Visual Studio regenerated <c>DdfView.Designer.vb</c> the line began throwing from
''' <c>InitializeComponent</c> — the DDF view stopped constructing, and clicking DDF in the nav list
''' did nothing at all. Nothing in the build or the 710 tests noticed, because no test set the
''' property the way the designer does.
'''
''' Any control that ends up in a designer has to tolerate what the designer writes about it.
''' </summary>
Public Class KBotNavListSelectionTests

    ' The control is a WinForms control: create it on an STA thread, like the harness layout tests.
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

    Private Shared Function NewList() As KBotNavList
        Dim nav As New KBotNavList()
        nav.AddItem("sumar", "Sumar")
        nav.AddItem("ddf", "DDF")
        Return nav
    End Function

    <Fact>
    Public Sub SettingNothing_MeansNoSelection_AndDoesNotThrow()
        ' This is the exact line the designer emits.
        RunSta(Sub()
                   Using nav = NewList()
                       nav.SelectedKey = "ddf"
                       nav.SelectedKey = Nothing
                       Assert.True(String.IsNullOrEmpty(nav.SelectedKey))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SettingEmptyString_AlsoMeansNoSelection()
        RunSta(Sub()
                   Using nav = NewList()
                       nav.SelectedKey = "sumar"
                       nav.SelectedKey = ""
                       Assert.True(String.IsNullOrEmpty(nav.SelectedKey))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnUnknownKey_STILL_Throws()
        ' The house rule is unchanged: a wrong key is a mistake and must be loud. Only «absent» was
        ' ever meant to be a state rather than an error.
        RunSta(Sub()
                   Using nav = NewList()
                       Assert.Throws(Of ArgumentException)(Sub() nav.SelectedKey = "nu-exista")
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClearingRaisesSelectionChangedOnce_AndOnlyWhenSomethingWasSelected()
        RunSta(Sub()
                   Using nav = NewList()
                       Dim raised As Integer = 0
                       AddHandler nav.SelectionChanged, Sub(k As String) raised += 1

                       nav.SelectedKey = "ddf"          ' 1: a real change
                       nav.SelectedKey = Nothing        ' 2: cleared
                       nav.SelectedKey = Nothing        ' already clear -> no event
                       Assert.Equal(2, raised)
                   End Using
               End Sub)
    End Sub

End Class
