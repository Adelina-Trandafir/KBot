Imports System.Windows.Forms
Imports System.Text.Json.Serialization

''' <summary>Strategia de randare a butoanelor pentru o schemă.</summary>
Public Enum ButtonRenderStyle
    ''' <summary>Buton system (UseVisualStyleBackColor) — nu se pictează nimic.</summary>
    [System] = 0
    ''' <summary>FlatStyle.Flat cu culori din paletă (tema dark actuală).</summary>
    Flat = 1
    ''' <summary>Owner-drawn modern: colțuri rotunjite, hover/pressed pictat.</summary>
    ModernOwnerDrawn = 2
End Enum

''' <summary>
''' „Mai mult decât culori”: flagurile care descriu comportamentul vizual al unei
''' scheme, independent de paletă. Serializabil JSON (editorul viitor le scrie direct).
''' </summary>
Public NotInheritable Class ThemeStyleOptions

    ''' <summary>True (Classic) => sari peste orice pictură custom, deferă la SystemColors.</summary>
    Public Property UseSystemColors As Boolean = True

    ''' <summary>Button/Tab FlatStyle.Flat.</summary>
    Public Property FlatControls As Boolean = False

    ''' <summary>Strategia de randare a butoanelor.</summary>
    Public Property ButtonRender As ButtonRenderStyle = ButtonRenderStyle.System

    ''' <summary>Rază colț în px logici @96dpi; scalată la DPI la pictare. 0 = pătrat.</summary>
    Public Property CornerRadius As Integer = 0

    ''' <summary>Numele fontului de bază (fallback „Segoe UI” dacă lipsește).</summary>
    Public Property BaseFontName As String = "Segoe UI"

    ''' <summary>Dimensiunea fontului de bază (pt). 0 sau negativ => nu schimbă fontul.</summary>
    Public Property BaseFontSize As Single = 0F

    ''' <summary>Padding intern pentru inputuri/butoane (serializat ca 4 întregi).</summary>
    Public Property ControlPadding As PaddingDto = New PaddingDto()

    ''' <summary>Inel/underline accent pe focus la inputuri.</summary>
    Public Property FocusAccent As Boolean = False

    ''' <summary>Bară de titlu dark (DWM attr 20).</summary>
    Public Property DarkTitleBar As Boolean = False

    ''' <summary>Owner-draw pe header-ele de tab (reutilizează OnDrawTab).</summary>
    Public Property OwnerDrawTabs As Boolean = False

    ''' <summary>Padding-ul efectiv ca <see cref="Padding"/> WinForms.</summary>
    <JsonIgnore> Public ReadOnly Property PaddingValue As Padding
        Get
            Return ControlPadding.ToPadding()
        End Get
    End Property

End Class

''' <summary>
''' DTO serializabil pentru <see cref="Padding"/> (structul WinForms nu are un
''' contract JSON stabil). Toate valorile în px logici.
''' </summary>
Public NotInheritable Class PaddingDto
    Public Property Left As Integer = 0
    Public Property Top As Integer = 0
    Public Property Right As Integer = 0
    Public Property Bottom As Integer = 0

    Public Sub New()
    End Sub

    Public Sub New(all As Integer)
        Left = all : Top = all : Right = all : Bottom = all
    End Sub

    Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
        Me.Left = left : Me.Top = top : Me.Right = right : Me.Bottom = bottom
    End Sub

    Public Function ToPadding() As Padding
        Return New Padding(Left, Top, Right, Bottom)
    End Function
End Class
