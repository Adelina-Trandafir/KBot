Option Strict On
Imports System.Drawing

''' <summary>
''' Modelul unei coloane <see cref="KBotDataView"/> (control NELEGAT de date). Controlul
''' deține colecția de coloane; caller-ul o construiește prin <c>AddColumn</c> și apoi
''' citește/scrie proprietățile de aici. Offset-ul X al coloanei e cache-uit de CONTROL
''' (nu se ține aici — depinde de scroll/freeze).
''' </summary>
Public NotInheritable Class KBotDataColumn

    ''' <summary>Identificator unic și stabil (folosit de API și de evenimente). Ne-modificabil.</summary>
    Public ReadOnly Property Key As String

    ''' <summary>Tipul coloanei — fixat la creare (determină pictarea/editarea).</summary>
    Public ReadOnly Property ColumnType As KBotColumnType

    ''' <summary>Textul din antet.</summary>
    Public Property HeaderText As String

    Private _minWidth As Integer = 40
    Private _maxWidth As Integer = Integer.MaxValue
    Private _width As Integer

    ''' <summary>Lățimea în pixeli. Nu coboară niciodată sub <see cref="MinWidth"/>.</summary>
    Public Property Width As Integer
        Get
            Return _width
        End Get
        Set(value As Integer)
            ' English (slice 0013): clamp to [MinWidth, MaxWidth] on every write so the
            ' auto-size / fill / shrink passes can assign freely and let the model enforce
            ' the bounds. MaxWidth is never below MinWidth (see the MaxWidth setter).
            _width = Math.Min(Math.Max(value, _minWidth), _maxWidth)
        End Set
    End Property

    ''' <summary>Lățimea minimă (px). Implicit 40. Ridicarea ei împinge și <see cref="Width"/>.</summary>
    Public Property MinWidth As Integer
        Get
            Return _minWidth
        End Get
        Set(value As Integer)
            _minWidth = Math.Max(0, value)
            ' English (slice 0013): keep the invariant MinWidth <= MaxWidth, then re-clamp Width.
            If _maxWidth < _minWidth Then _maxWidth = _minWidth
            If _width < _minWidth Then _width = _minWidth
            If _width > _maxWidth Then _width = _maxWidth
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0013): maximum width in pixels. Default <see cref="Integer.MaxValue"/>
    ''' (uncapped). Auto-sizing and fill modes never grow a column past this. Kept at or above
    ''' <see cref="MinWidth"/>; lowering it re-clamps <see cref="Width"/>.
    ''' </summary>
    Public Property MaxWidth As Integer
        Get
            Return _maxWidth
        End Get
        Set(value As Integer)
            _maxWidth = Math.Max(value, _minWidth)
            If _width > _maxWidth Then _width = _maxWidth
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0013): set when the operator has dragged this column's edge. A
    ''' <see cref="KBotAutoSizeMode.ToContent"/> pass leaves such a column alone, but fill /
    ''' shrink still applies to it. Cleared by <c>KBotDataView.ResetColumnSizing</c>.
    ''' </summary>
    Friend Property UserSized As Boolean

    ''' <summary>Vizibilă. Implicit True. False => coloana nu se pictează și nu ocupă spațiu.</summary>
    Public Property Visible As Boolean = True

    ''' <summary>Coloană înghețată (non-scrolling): se randează la stânga, înaintea zonei derulate.</summary>
    Public Property Frozen As Boolean = False

    ''' <summary>Coloana nu intră niciodată în editare.</summary>
    Public Property [ReadOnly] As Boolean = False

    ''' <summary>Implicit True. False => întreaga coloană e ștearsă (gri) și inertă.</summary>
    Public Property Enabled As Boolean = True

    ''' <summary>Alinierea conținutului (se reutilizează enum-ul WinForms).</summary>
    Public Property TextAlign As ContentAlignment = ContentAlignment.MiddleLeft

    ''' <summary>Format .NET aplicat valorii la afișare (ex. „N2”, „dd.MM.yyyy”). Vid => ToString().</summary>
    Public Property FormatString As String

    ''' <summary>Sursa combo partajată pe coloană (override per-celulă prin evenimentul de formatare).</summary>
    Public Property ComboItems As IList(Of Object)

    ''' <summary>
    ''' Grupul de exclusivitate pentru coloanele <see cref="KBotColumnType.OptionButton"/>.
    ''' Bifarea unei opțiuni le stinge pe celelalte din ACELAȘI RÂND care au același grup.
    ''' Vid => opțiunea e independentă (nu stinge nimic).
    ''' </summary>
    Public Property OptionGroup As String

    ''' <summary>Minimul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    Public Property ProgressMin As Double = 0

    ''' <summary>Maximul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    Public Property ProgressMax As Double = 100

    ''' <summary>Redimensionabilă prin tragerea marginii din antet. Implicit True.</summary>
    Public Property Resizable As Boolean = True

    ''' <summary>Payload al caller-ului (nefolosit de control).</summary>
    Public Property Tag As Object

    ''' <summary>Cheia + tipul sunt fixate la creare; restul se lasă pe valorile implicite.</summary>
    Public Sub New(key As String, headerText As String, type As KBotColumnType, width As Integer)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        _Key = key
        _ColumnType = type
        ' „Me.” e OBLIGATORIU: VB e case-insensitive, deci parametrul „headerText” ascunde
        ' proprietatea „HeaderText”, iar o atribuire nekalificată s-ar face parametrului.
        Me.HeaderText = If(headerText, String.Empty)
        _width = Math.Max(width, _minWidth)
    End Sub

End Class
