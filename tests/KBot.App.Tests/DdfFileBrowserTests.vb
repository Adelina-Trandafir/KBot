Option Strict On
Imports System
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.App

' Headless STA tests for DdfFileBrowser (slice 0020-04): the grid fills from a real temp tree,
' a missing root shows a message naming the path, and selecting a row raises FileActivated with
' the file's full path (the single-preview cross-link, plan §7).
Public Class DdfFileBrowserTests

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function GridOf(b As DdfFileBrowser) As KBotDataView
        Return DirectCast(FindByName(b, "grid"), KBotDataView)
    End Function

    Private Shared Function LabelVisible(b As DdfFileBrowser) As Boolean
        Dim l = FindByName(b, "lblEmpty")
        Return l IsNot Nothing AndAlso l.Visible
    End Function

    Private Shared Function MakeTree() As String
        Dim root As String = Path.Combine(Path.GetTempPath(), "kbot_fb_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(Path.Combine(root, "GENERAL"))
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_0_A100.PDF"), "x")
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_1_A100.PDF"), "xx")
        File.WriteAllText(Path.Combine(root, "GENERAL", "DDF_NR_4_REV_0_A100.xml"), "<form1/>")
        Return root
    End Function

    <Fact>
    Public Sub SetContext_FillsGrid_FromDisk()
        RunSta(Sub()
                   Dim root = MakeTree()
                   Try
                       Using b As New DdfFileBrowser()
                           b.SetContext(root, "A100")
                           Dim g = GridOf(b)
                           Assert.Equal(2, g.RowCount)          ' două PDF-uri, .xml exclus
                           Assert.True(g.Visible)
                           Assert.False(LabelVisible(b))
                       End Using
                   Finally
                       Directory.Delete(root, True)
                   End Try
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_MissingRoot_ShowsMessageNamingThePath()
        RunSta(Sub()
                   Dim ghost As String = Path.Combine(Path.GetTempPath(), "kbot_ghost_" & Guid.NewGuid().ToString("N"))
                   Using b As New DdfFileBrowser()
                       b.SetContext(ghost, "A100")
                       Assert.True(LabelVisible(b))
                       Dim lbl = DirectCast(FindByName(b, "lblEmpty"), Label)
                       Assert.Contains(ghost, lbl.Text)         ' mesajul numește calea configurată
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_ExistingRootNoFiles_ShowsNoDocumentsMessage()
        RunSta(Sub()
                   Dim root As String = Path.Combine(Path.GetTempPath(), "kbot_empty_" & Guid.NewGuid().ToString("N"))
                   Directory.CreateDirectory(root)
                   Try
                       Using b As New DdfFileBrowser()
                           b.SetContext(root, "A100")
                           Assert.True(LabelVisible(b))
                       End Using
                   Finally
                       Directory.Delete(root, True)
                   End Try
               End Sub)
    End Sub

    <Fact>
    Public Sub SelectingRow_RaisesFileActivated_WithFullPath()
        RunSta(Sub()
                   Dim root = MakeTree()
                   Try
                       Using b As New DdfFileBrowser()
                           Dim activated As String = Nothing
                           AddHandler b.FileActivated, Sub(p) activated = p
                           b.SetContext(root, "A100")

                           ' Setarea rândului curent ridică SelectionChanged -> FileActivated.
                           GridOf(b).CurrentRowIndex = 0
                           Assert.NotNull(activated)
                           Assert.EndsWith("_A100.PDF", activated, StringComparison.OrdinalIgnoreCase)
                           Assert.True(File.Exists(activated))
                       End Using
                   Finally
                       Directory.Delete(root, True)
                   End Try
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_ClearsAndShowsPrompt()
        RunSta(Sub()
                   Using b As New DdfFileBrowser()
                       b.SetContext("C:\whatever", "")
                       Assert.True(LabelVisible(b))
                       Assert.Equal(0, GridOf(b).RowCount)
                   End Using
               End Sub)
    End Sub

End Class
