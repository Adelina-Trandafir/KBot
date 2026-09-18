Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' THE FIT (slice 0062, kept): every Absolute row/column is written as its SCALED authored
''' measure plus exactly the surplus the greediest child in it asks for, and 0 when collapsed.
'''
''' <para><b>Why the base is the authored measure and not the current one.</b> Without the
''' snapshot the second scheme switch would measure over the result of the first and the rows
''' would grow at every pass through Modern, never coming back. Same pattern as the authored
''' height of a button in <c>ModernRenderer</c> and the authored width of a grid column: a scheme
''' may GROW a measure to fit the content, it may not REWRITE it.</para>
'''
''' <para><b>What it never touches:</b> Percent and AutoSize styles (they already follow the
''' content or the remaining space) and any child spanning several rows/columns (its surplus
''' cannot be charged to one of them).</para>
'''
''' <para>Writes only when the value changes: a style write asks for a layout, and a theme pass
''' over a form with thirty tables must not cost thirty layouts for nothing.</para>
''' </summary>
Partial Public Class KBotTableLayoutPanel

    ''' <summary>
    ''' Rewrites every Absolute style: <c>scaled authored + surplus</c>, or 0 when collapsed.
    ''' Called from <c>ApplyMetricScale</c> only, after the snapshot exists and the padding is
    ''' in place.
    ''' </summary>
    Private Sub Refit()
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If ColumnStyles.Count = 0 AndAlso RowStyles.Count = 0 Then Return
            EnsureBaseline()

            Dim nrC As Integer = ColumnStyles.Count
            Dim nrR As Integer = RowStyles.Count
            Dim baseC As Single() = New Single(Math.Max(0, nrC - 1)) {}
            Dim baseR As Single() = New Single(Math.Max(0, nrR - 1)) {}
            For col As Integer = 0 To nrC - 1
                baseC(col) = SX(_logicalCols(col))
            Next
            For row As Integer = 0 To nrR - 1
                baseR(row) = SY(_logicalRows(row))
            Next

            Dim surplusC As Single() = New Single(Math.Max(0, nrC - 1)) {}
            Dim surplusR As Single() = New Single(Math.Max(0, nrR - 1)) {}
            If _autoFitToTheme Then
                For Each c As Control In Controls
                    If c Is Nothing Then Continue For
                    Dim wants As Size = Demand(c)
                    If GetColumnSpan(c) = 1 Then
                        Dim col As Integer = GetColumn(c)
                        If IsAbsolute(ColumnStyles, col) Then
                            surplusC(col) = Math.Max(surplusC(col), wants.Width - baseC(col))
                        End If
                    End If
                    If GetRowSpan(c) = 1 Then
                        Dim row As Integer = GetRow(c)
                        If IsAbsolute(RowStyles, row) Then
                            surplusR(row) = Math.Max(surplusR(row), wants.Height - baseR(row))
                        End If
                    End If
                Next
            End If

            SuspendLayout()
            Try
                For col As Integer = 0 To nrC - 1
                    If Not IsAbsolute(ColumnStyles, col) Then Continue For
                    Dim want As Single = If(_collapsedCols.Contains(col), 0F, baseC(col) + Math.Max(0F, surplusC(col)))
                    If Math.Abs(ColumnStyles(col).Width - want) > 0.01F Then ColumnStyles(col).Width = want
                    _lastCols(col) = want
                Next
                For row As Integer = 0 To nrR - 1
                    If Not IsAbsolute(RowStyles, row) Then Continue For
                    Dim want As Single = If(_collapsedRows.Contains(row), 0F, baseR(row) + Math.Max(0F, surplusR(row)))
                    If Math.Abs(RowStyles(row).Height - want) > 0.01F Then RowStyles(row).Height = want
                    _lastRows(row) = want
                Next
            Finally
                ResumeLayout(True)
            End Try
        Catch ex As Exception
            ' Theme/scale boundary: a failed fit leaves the table on its previous measures.
            GlobalErrorLog.Write("KBotTableLayoutPanel.Refit", ex)
        End Try
    End Sub

    ''' <summary>
    ''' What a child asks for NOW, margins included -- never its Width/Height: the table clips a
    ''' docked child to its cell, so a 56px button in a 40px cell reports 40 and would never ask
    ''' for more (slice 0030). A leaf answers through its own <c>GetPreferredSize</c>; a container
    ''' is measured by its content (<see cref="ThemeFormFit.ContentDemand"/>), because its
    ''' <c>GetPreferredSize</c> would echo the cell it sits in and the row would never come back
    ''' from a growth. Neither reads the cell. The margins are the child's, in device pixels as
    ''' the platform scaled them.
    ''' </summary>
    Private Shared Function Demand(c As Control) As Size
        Dim d As Size = ThemeFormFit.ContentDemand(c)
        Return New Size(d.Width + c.Margin.Horizontal, d.Height + c.Margin.Vertical)
    End Function

    ''' <summary>
    ''' The table's demand, from CONTENT only: every Absolute row/column at Max(its live style,
    ''' what its children ask), every Percent/AutoSize one at what its children ask (0 when they
    ''' are containers with nothing fixed inside), plus the cell gaps and the padding. It never
    ''' reads the table's own size, so it can be asked before the first fit (the 0030 failure: a
    ''' 56px button in a 40px cell reported 40) and it is stable after it. Children spanning
    ''' several cells are not attributed to any of them.
    '''
    ''' <para>With <see cref="AutoFitToTheme"/> off the base answer is returned: the size is the
    ''' designer's business then.</para>
    ''' </summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            If Not _autoFitToTheme OrElse KBotDesignTime.IsDesignTime(Me) Then Return MyBase.GetPreferredSize(proposedSize)
            Dim nrC As Integer = ColumnStyles.Count
            Dim nrR As Integer = RowStyles.Count
            If nrC = 0 OrElse nrR = 0 Then Return MyBase.GetPreferredSize(proposedSize)

            Dim wantC As Single() = New Single(nrC - 1) {}
            Dim wantR As Single() = New Single(nrR - 1) {}
            For Each c As Control In Controls
                If c Is Nothing Then Continue For
                Dim d As Size = Demand(c)
                If GetColumnSpan(c) = 1 Then
                    Dim col As Integer = GetColumn(c)
                    If col >= 0 AndAlso col < nrC Then wantC(col) = Math.Max(wantC(col), d.Width)
                End If
                If GetRowSpan(c) = 1 Then
                    Dim row As Integer = GetRow(c)
                    If row >= 0 AndAlso row < nrR Then wantR(row) = Math.Max(wantR(row), d.Height)
                End If
            Next

            Dim gap As Integer = NativeBorderWidth()
            Dim pad As Padding = MyBase.Padding
            Dim w As Single = pad.Horizontal + gap * (nrC + 1)
            For col As Integer = 0 To nrC - 1
                If _collapsedCols.Contains(col) Then Continue For
                w += If(ColumnStyles(col).SizeType = SizeType.Absolute, Math.Max(ColumnStyles(col).Width, wantC(col)), wantC(col))
            Next
            Dim h As Single = pad.Vertical + gap * (nrR + 1)
            For row As Integer = 0 To nrR - 1
                If _collapsedRows.Contains(row) Then Continue For
                h += If(RowStyles(row).SizeType = SizeType.Absolute, Math.Max(RowStyles(row).Height, wantR(row)), wantR(row))
            Next
            Return New Size(CInt(Math.Ceiling(w)), CInt(Math.Ceiling(h)))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

End Class
