Option Strict On
Imports System.ComponentModel
Imports System.Drawing

' Slice 0097-09: optional row typography; existing popups retain their ambient font.
Partial Public Class CustomPopup
    Private _fontSizeOffset As Single
    Private _rowFont As Font
    Private _italicRowFont As Font

    <Category("K-BOT Popup"), DefaultValue(0.0F)>
    <Description("Extra points added to the anchor font for command rows. Zero keeps the ambient font.")>
    Public Property FontSizeOffset As Single
        Get
            Return _fontSizeOffset
        End Get
        Set(value As Single)
            If Single.IsNaN(value) OrElse Single.IsInfinity(value) Then Throw New ArgumentOutOfRangeException(NameOf(value))
            value = Math.Max(0.0F, Math.Min(24.0F, value))
            If value = _fontSizeOffset Then Return
            _fontSizeOffset = value
            DisposeRowFonts()
            InvalidateLayout()
        End Set
    End Property

    Private Function RowFont(Optional italic As Boolean = False) As Font
        If _fontSizeOffset = 0.0F AndAlso Not italic Then Return Font
        If _rowFont Is Nothing Then
            _rowFont = New Font(Font.FontFamily, Font.SizeInPoints + _fontSizeOffset, Font.Style, GraphicsUnit.Point)
        End If
        If Not italic Then Return _rowFont
        If _italicRowFont Is Nothing Then _italicRowFont = New Font(_rowFont, _rowFont.Style Or FontStyle.Italic)
        Return _italicRowFont
    End Function

    Private Sub DisposeRowFonts()
        _italicRowFont?.Dispose()
        _italicRowFont = Nothing
        _rowFont?.Dispose()
        _rowFont = Nothing
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then DisposeRowFonts()
        MyBase.Dispose(disposing)
    End Sub
End Class
