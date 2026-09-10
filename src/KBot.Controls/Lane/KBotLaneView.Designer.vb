Option Strict On
Imports System.ComponentModel
Imports System.Windows.Forms

''' <summary>
''' The DESIGNER half of <see cref="KBotLaneView"/>. Under the house rule every child control and
''' every component a control owns is declared HERE, in <c>InitializeComponent</c>, and never
''' conjured up in the middle of the code that happens to need it first.
'''
''' <para>Two of them, and no more: the vertical scrollbar, and the floating label.</para>
'''
''' <list type="bullet">
''' <item><description><c>vScroll</c> — the only real child window on the surface. Everything else
''' the operator sees (bands, rails, markers, end marks, the enlarge button, the dated lines) is
''' PAINTED, so it has no control behind it and nothing to declare here. Vertical only: the
''' horizontal axis is time, and a time axis that scrolls off the edge has stopped being a
''' comparison between lanes.</description></item>
''' <item><description><c>markerTip</c> — the floating label for markers, lanes, guides and the
''' enlarge button. A <see cref="KBotToolTip"/> and never <c>System.Windows.Forms.ToolTip</c>
''' (CONTROLS.md C8): the things it names are painted regions, not controls, so there is nothing
''' for the stock tooltip to extend. It is a <see cref="Component"/>, so it lives in
''' <c>components</c> and is disposed with it.</description></item>
''' </list>
'''
''' <para><b>Why the tooltip is built here rather than on first hover.</b> It used to be created
''' lazily, which meant the first label the operator asked for paid for building a window, and a
''' control read purely with the property grid never built one at all. Building it here costs
''' nothing at design time — a <see cref="KBotToolTip"/> does not open a window until something
''' calls <c>ShowAt</c>, and this control never calls it under
''' <c>KBotDesignTime.IsDesignTime</c>.</para>
'''
''' <para><b>Positions are NOT set here.</b> The scrollbar is placed by <c>LayoutScrollBar</c> on
''' every layout pass, because where it goes depends on the band height, the margins and whether
''' the stack of lanes is taller than the surface — none of which exist yet at this point. What
''' the designer owns is that the field EXISTS and is wired up; what the layout owns is where it
''' sits.</para>
''' </summary>
Partial Public NotInheritable Class KBotLaneView

    ''' <summary>The component container (standard designer contract).</summary>
    Private components As IContainer

    ''' <summary>
    ''' The vertical scrollbar. Hidden until the stack of lanes is taller than the surface, and
    ''' hidden always under the designer.
    ''' </summary>
    Friend WithEvents vScroll As VScrollBar

    ''' <summary>The floating label for every painted region of this surface.</summary>
    Friend WithEvents markerTip As KBotToolTip

    Private Sub InitializeComponent()
        components = New Container()
        vScroll = New VScrollBar()
        markerTip = New KBotToolTip(components)
        SuspendLayout()
        '
        ' vScroll — laid out by LayoutScrollBar; only the range and the state belong here
        '
        vScroll.Minimum = 0
        vScroll.Maximum = 0
        vScroll.Visible = False
        '
        ' KBotLaneView
        '
        Controls.Add(vScroll)
        ResumeLayout(False)
    End Sub

    ''' <summary>
    ''' Frees the GDI+ objects the painter caches, then the components.
    ''' </summary>
    ''' <remarks>
    ''' The scratch pens and brushes exist so that a repaint does not allocate a thousand of
    ''' them; the price of that is that they outlive the paint pass and have to be freed by hand.
    ''' <c>KBotLaneLog.Flush</c> goes here too, so a Debug run that ends by closing its window
    ''' leaves a complete journal behind rather than losing the last batch.
    ''' </remarks>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                KBotLaneLog.Flush()
                _borderPen?.Dispose()
                _laneLinePen?.Dispose()
                _separatorPen?.Dispose()
                _fillBrush?.Dispose()
                _backBrush?.Dispose()
                _edgePen?.Dispose()
                _detailPen?.Dispose()
                _derivedHeaderFont?.Dispose()
                _derivedAxisFont?.Dispose()
                components?.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
