Option Strict On

''' <summary>
''' The shape drawn on top of every data point of a series.
''' </summary>
''' <remarks>
''' <see cref="None"/> is not the same as a marker of size zero: without markers the chart has no
''' hit target, so hovering can no longer name a point and the floating label never opens. That is
''' a deliberate choice of the host, not a side effect of a small number.
''' </remarks>
Public Enum KBotChartMarkerStyle
    None = 0
    Circle = 1
    Square = 2
    Diamond = 3
End Enum

''' <summary>Which end of the header band the tab buttons sit on.</summary>
Public Enum KBotChartTabAlign
    Left = 0
    Right = 1
End Enum

''' <summary>
''' Where the value axis starts.
''' </summary>
''' <remarks>
''' <see cref="FromZero"/> tells the truth about magnitude — two chains whose values differ by a
''' factor of ten look like they differ by a factor of ten. <see cref="FromMinimum"/> tells the
''' truth about movement — a chain that grows by one percent still shows a visible slope. Neither
''' is right in general, so the host picks.
''' </remarks>
Public Enum KBotChartValueAxisMode
    FromZero = 0
    FromMinimum = 1
End Enum
