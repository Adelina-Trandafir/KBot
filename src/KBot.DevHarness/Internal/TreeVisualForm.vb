Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Controls

' Proba vizuală pentru AdvancedTreeControl: arbore populat hardcodat, cu checkbox-uri
' și icoane dreapta, plus butoanele Pass/Fail. Evenimentele native ale controlului
' se loghează prin delegatul primit din test (context.Log).
Public NotInheritable Class TreeVisualForm
    Inherits Form

    Private ReadOnly _log As Action(Of String)
    Private _rightIcon As Bitmap

    Public Sub New(log As Action(Of String))
        _log = log
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "AdvancedTreeControl — proba vizuală"
        Me.Width = 520
        Me.Height = 560
        Me.StartPosition = FormStartPosition.CenterParent

        ' Mică iconiță dreapta (16x16) ca evenimentul RightIconClicked să fie accesibil.
        _rightIcon = New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(_rightIcon)
            g.Clear(Color.SteelBlue)
        End Using

        Dim tree As New AdvancedTreeControl() With {.Dock = DockStyle.Fill, .CheckBoxes = True}

        ' Două noduri-părinte, fiecare cu câteva frunze; toate cu checkbox (propagare părinte→copii).
        Dim grupA As AdvancedTreeControl.TreeItem =
            tree.AddItem("A", "Grup A — Indicatori", Nothing, pExpanded:=True)
        grupA.HasCheckBox = True
        Dim a1 As AdvancedTreeControl.TreeItem = tree.AddItem("A1", "Indicator 1", grupA, pRightIcon:=_rightIcon)
        a1.HasCheckBox = True
        Dim a2 As AdvancedTreeControl.TreeItem = tree.AddItem("A2", "Indicator 2", grupA, pRightIcon:=_rightIcon)
        a2.HasCheckBox = True
        Dim a3 As AdvancedTreeControl.TreeItem = tree.AddItem("A3", "Indicator 3", grupA, pRightIcon:=_rightIcon)
        a3.HasCheckBox = True

        Dim grupB As AdvancedTreeControl.TreeItem =
            tree.AddItem("B", "Grup B — Angajamente", Nothing, pExpanded:=True)
        grupB.HasCheckBox = True
        Dim b1 As AdvancedTreeControl.TreeItem = tree.AddItem("B1", "Angajament 1", grupB, pRightIcon:=_rightIcon)
        b1.HasCheckBox = True
        Dim b2 As AdvancedTreeControl.TreeItem = tree.AddItem("B2", "Angajament 2", grupB, pRightIcon:=_rightIcon)
        b2.HasCheckBox = True

        ' Evenimentele publice ale controlului → log.
        AddHandler tree.NodeChecked, AddressOf OnNodeChecked
        AddHandler tree.NodeRadioSelected, AddressOf OnNodeRadioSelected
        AddHandler tree.RightIconClicked, AddressOf OnRightIconClicked
        AddHandler tree.NodeDoubleClicked, AddressOf OnNodeDoubleClicked

        ' Butoane Pass/Fail
        Dim pnl As New FlowLayoutPanel() With {.Dock = DockStyle.Bottom, .Height = 40, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(6)}
        Dim btnFail As New Button() With {.Text = "Fail", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim btnPass As New Button() With {.Text = "Pass", .DialogResult = DialogResult.OK, .AutoSize = True}
        pnl.Controls.Add(btnFail)
        pnl.Controls.Add(btnPass)
        Me.AcceptButton = btnPass
        Me.CancelButton = btnFail

        Me.Controls.Add(tree)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub OnNodeChecked(pNode As AdvancedTreeControl.TreeItem)
        _log("checked: " & pNode.Key & " (" & pNode.Caption & ") -> " & pNode.CheckState.ToString())
    End Sub

    Private Sub OnNodeRadioSelected(nodeOn As AdvancedTreeControl.TreeItem, nodeOff As AdvancedTreeControl.TreeItem)
        _log("radio: " & If(nodeOn IsNot Nothing, nodeOn.Key, "<none>") &
             " (off: " & If(nodeOff IsNot Nothing, nodeOff.Key, "<none>") & ")")
    End Sub

    Private Sub OnRightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs)
        _log("right-icon: " & pNode.Key & " (" & pNode.Caption & ")")
    End Sub

    Private Sub OnNodeDoubleClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs)
        _log("double-click: " & pNode.Key & " (" & pNode.Caption & ")")
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _rightIcon?.Dispose()
            _rightIcon = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
