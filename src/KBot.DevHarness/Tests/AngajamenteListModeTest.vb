Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Controls

' Offline test for the AdvancedTreeControl flat-list mode (Part 3): ConfigureListMode +
' AddItem + per-node Cells/icons/bold/tag, exactly as MainForm.PopulateAngajamenteList
' drives it. Asserts the public node state and forces a real paint (DrawToBitmap) so a
' rendering regression in the static-column path throws here instead of on-screen.
Public NotInheritable Class AngajamenteListModeTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "AdvancedTree — mod listă angajamente (ConfigureListMode + celule + paint)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Angajamente"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Private Structure Row
        Public Cod As String
        Public Descriere As String
        Public HasSurse As Boolean
    End Structure

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        Dim rows As New List(Of Row) From {
            New Row With {.Cod = "2026-0001", .Descriere = "Achiziție echipamente IT", .HasSurse = True},
            New Row With {.Cod = "2026-0002", .Descriere = "Servicii mentenanță", .HasSurse = False},
            New Row With {.Cod = "2026-0003", .Descriere = "Angajament anulat", .HasSurse = False}
        }

        Dim created As New List(Of Image)()
        Dim tree As New AdvancedTreeControl()
        Try
            ' Configurare identică cu MainForm.ConfigureAngajamenteList.
            tree.LeftIconSize = New Size(24, 24)
            tree.RightIconSize = New Size(24, 24)
            tree.ItemHeight = 32
            tree.ShowRightIconOnHover = True
            Dim cols As New List(Of ColumnDef) From {
                New ColumnDef With {
                    .Name = "CodAngajament", .Header = "CodAngajament", .Width = 140,
                    .ColType = En_ColType.ColType_Text, .Align = En_ColAlign.ColAlign_Left,
                    .Format = "", .HeaderBackColor = Color.Empty, .HeaderForeColor = Color.Empty,
                    .HeaderAlign = En_ColAlign.ColAlign_Inherit}
            }
            tree.ConfigureListMode(cols)

            If Not tree.TreeListView Then
                Return Task.FromResult(HarnessTestResult.Failed("ConfigureListMode nu a activat TreeListView."))
            End If

            For Each r In rows
                Dim statusImg As Image = New Bitmap(24, 24)   ' placeholder (FxIcons e în KBot.App)
                Dim refreshImg As Image = New Bitmap(24, 24)
                created.Add(statusImg)
                created.Add(refreshImg)

                Dim caption As String = r.Descriere.Trim().ToUpperInvariant()
                Dim node As AdvancedTreeControl.TreeItem =
                    tree.AddItem("D_" & r.Cod, caption,
                                 pLeftIconClosed:=statusImg, pRightIcon:=refreshImg, pTag:=r.Cod)
                node.Cells("CodAngajament") = New AdvancedTreeControl.TreeItem.CellData With {.Value = r.Cod}
                node.Bold = r.HasSurse
                node.Tooltip = r.Descriere
                node.ShowRightIconOnHover = True
            Next

            ' --- assert public node state ---
            If tree.Items.Count <> rows.Count Then
                Return Task.FromResult(HarnessTestResult.Failed(
                    $"Items.Count={tree.Items.Count}, așteptat {rows.Count}."))
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim node As AdvancedTreeControl.TreeItem = tree.Items(i)
                Dim r As Row = rows(i)
                If node.Level <> 0 Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Nod «{r.Cod}» nu e la nivel 0 (Level={node.Level})."))
                End If
                If node.Caption <> r.Descriere.ToUpperInvariant() Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Caption greșit pentru «{r.Cod}»: {node.Caption}."))
                End If
                Dim cell As AdvancedTreeControl.TreeItem.CellData = Nothing
                If Not node.Cells.TryGetValue("CodAngajament", cell) OrElse cell Is Nothing OrElse cell.Value <> r.Cod Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Celula CodAngajament lipsă/greșită pentru «{r.Cod}»."))
                End If
                If node.Bold <> r.HasSurse Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Bold={node.Bold} pentru «{r.Cod}», așteptat {r.HasSurse}."))
                End If
                If Not String.Equals(TryCast(node.Tag, String), r.Cod, StringComparison.Ordinal) Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Tag greșit pentru «{r.Cod}»."))
                End If
                If node.LeftIconClosed Is Nothing OrElse node.RightIcon Is Nothing Then
                    Return Task.FromResult(HarnessTestResult.Failed($"Iconiță lipsă pentru «{r.Cod}»."))
                End If
                If Not node.ShowRightIconOnHover Then
                    Return Task.FromResult(HarnessTestResult.Failed($"ShowRightIconOnHover=False pentru «{r.Cod}»."))
                End If
            Next

            ' --- force a real paint (offscreen) — throws here if the paint path regresses ---
            Using host As New Form()
                host.Size = New Size(600, 400)
                tree.Dock = DockStyle.Fill
                host.Controls.Add(tree)
                host.CreateControl()
                Using bmp As New Bitmap(Math.Max(1, tree.Width), Math.Max(1, tree.Height))
                    tree.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                End Using
            End Using

            Return Task.FromResult(HarnessTestResult.Passed(
                $"{rows.Count} rânduri; TreeListView activ; celule/iconițe/bold/tag corecte; paint OK."))
        Catch ex As Exception
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        Finally
            tree.Dispose()
            For Each img In created
                img.Dispose()
            Next
        End Try
    End Function
End Class
