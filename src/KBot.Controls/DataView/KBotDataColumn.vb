Option Strict On
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' Modelul unei coloane <see cref="KBotDataView"/> (control NELEGAT de date). Controlul
''' deține colecția de coloane; caller-ul o construiește prin <c>AddColumn</c> și apoi
''' citește/scrie proprietățile de aici. Offset-ul X al coloanei e cache-uit de CONTROL
''' (nu se ține aici — depinde de scroll/freeze).
'''
''' English (slice 0025): also authorable from the Visual Studio property grid, through
''' <see cref="KBotDataView.Columns"/> and the stock collection dialog. That is why there is a
''' parameterless constructor and why <see cref="Key"/> / <see cref="ColumnType"/> are no longer
''' <c>ReadOnly</c> — the dialog creates an empty column first and fills it in afterwards. Both
''' setters are guarded once the column belongs to a grid that already holds rows.
''' </summary>
Public NotInheritable Class KBotDataColumn

    Private _key As String
    Private _columnType As KBotColumnType
    Private _minWidth As Integer = 40
    Private _maxWidth As Integer = Integer.MaxValue
    Private _width As Integer = 100

    ''' <summary>
    ''' English (slice 0025): the grid this column belongs to, set by
    ''' <c>KBotDataColumnCollection</c> on insert and cleared on removal. Friend: it is plumbing,
    ''' never a designer property. Nothing => the column is free-floating (the designer's case)
    ''' and the key/type guards do not apply.
    ''' </summary>
    Friend Property Owner As KBotDataView

    ''' <summary>
    ''' Identificator unic și stabil (folosit de API și de evenimente).
    '''
    ''' English (slice 0025): writable, but ONLY while the grid has no rows. Cell values are
    ''' stored per column key inside <see cref="KBotDataRow"/>'s dictionary, so renaming a key
    ''' under populated rows would orphan every stored cell and the column would silently paint
    ''' empty — a no-op that looks like data loss. That is an exception, not a silent skip.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Identificator unic și stabil al coloanei. Nu se poate schimba cât timp grila are rânduri.")>
    Public Property Key As String
        Get
            Return _key
        End Get
        Set(value As String)
            If String.Equals(_key, value, StringComparison.Ordinal) Then Return
            GuardModelChange(NameOf(Key))
            Dim oldKey As String = _key
            _key = value
            Owner?.OnColumnKeyChanged(oldKey, value)
        End Set
    End Property

    ''' <summary>
    ''' Tipul coloanei (determină pictarea/editarea).
    '''
    ''' English (slice 0025): writable under the same "no rows" rule as <see cref="Key"/> — the
    ''' painter and the editor branch on it, and an in-progress edit belongs to the OLD type.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Tipul coloanei — determină cum se pictează și cum se editează. Nu se poate schimba cât timp grila are rânduri.")>
    <DefaultValue(KBotColumnType.Text)>
    Public Property ColumnType As KBotColumnType
        Get
            Return _columnType
        End Get
        Set(value As KBotColumnType)
            If _columnType = value Then Return
            GuardModelChange(NameOf(ColumnType))
            _columnType = value
            ' O editare deschisă aparține tipului VECHI — se abandonează, nu se convertește.
            Owner?.CancelEdit()
        End Set
    End Property

    ' Cheia și tipul descriu FORMA datelor: se pot schimba doar pe o grilă fără rânduri.
    Private Sub GuardModelChange(propertyName As String)
        If Owner IsNot Nothing AndAlso Owner.RowCount > 0 Then
            Throw New InvalidOperationException(
                $"«{propertyName}» nu se poate schimba cât timp grila are rânduri: valorile celulelor sunt " &
                "păstrate pe cheia veche și ar rămâne orfane. Golește rândurile întâi (ClearRows).")
        End If
    End Sub

    ''' <summary>Textul din antet.</summary>
    <Category("K-BOT")>
    <Description("Textul afișat în banda de antet.")>
    Public Property HeaderText As String

    ''' <summary>Lățimea în pixeli. Nu coboară niciodată sub <see cref="MinWidth"/>.</summary>
    <Category("K-BOT")>
    <Description("Lățimea în pixeli. Se limitează întotdeauna la intervalul [MinWidth, MaxWidth].")>
    <DefaultValue(100)>
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
    <Category("K-BOT")>
    <Description("Lățimea minimă (px). Ridicarea ei împinge și Width.")>
    <DefaultValue(40)>
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
    <Category("K-BOT")>
    <Description("Lățimea maximă (px). Implicit nelimitată. Auto-dimensionarea și umplerea nu depășesc niciodată această valoare.")>
    <DefaultValue(Integer.MaxValue)>
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
    <Category("K-BOT")>
    <Description("False => coloana nu se pictează și nu ocupă spațiu.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    ''' <summary>
    ''' English (slice 0016): the column MAY be auto-hidden when the grid would otherwise need a
    ''' horizontal scrollbar. The fit pass hides auto-hideable columns (rightmost first) until
    ''' the rest fit or none remain; if none remain, the scrollbar appears normally. The fill
    ''' target (<c>ColumnFillMode</c> First/Last) is never auto-hidden — stretching wins over
    ''' hiding. Distinct from <see cref="Visible"/>, which stays the caller's explicit show/hide.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Coloana POATE fi ascunsă automat când grila n-ar încăpea fără bară orizontală (cea mai din dreapta prima).")>
    <DefaultValue(False)>
    Public Property AutoHide As Boolean = False

    ''' <summary>
    ''' English (slice 0016): set by the auto-hide pass when THIS column was hidden for lack of
    ''' room (never by the caller). Recomputed from scratch every layout, so a widened grid
    ''' brings the column back. Friend — the caller toggles <see cref="AutoHide"/>, not this.
    ''' </summary>
    Friend Property AutoHidden As Boolean

    ''' <summary>
    ''' English (slice 0016): whether the column is actually on screen right now — the caller
    ''' shows it (<see cref="Visible"/>) AND the fit pass has not auto-hidden it. Read-only:
    ''' the caller drives it through <see cref="Visible"/> / <see cref="AutoHide"/>.
    ''' Derived state: never shown in the property grid, never serialized.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsEffectivelyVisible As Boolean
        Get
            Return Visible AndAlso Not AutoHidden
        End Get
    End Property

    ''' <summary>Coloană înghețată (non-scrolling): se randează la stânga, înaintea zonei derulate.</summary>
    <Category("K-BOT")>
    <Description("Metadata de coloană înghețată. NOTĂ: mecanismul autoritar e KBotDataView.FrozenColumnCount.")>
    <DefaultValue(False)>
    Public Property Frozen As Boolean = False

    ''' <summary>Coloana nu intră niciodată în editare.</summary>
    <Category("K-BOT")>
    <Description("True => nicio celulă din coloană nu intră în editare.")>
    <DefaultValue(False)>
    Public Property [ReadOnly] As Boolean = False

    ''' <summary>Implicit True. False => întreaga coloană e ștearsă (gri) și inertă.</summary>
    <Category("K-BOT")>
    <Description("False => întreaga coloană e desenată ștearsă și nu răspunde la input.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    ''' <summary>Alinierea conținutului (se reutilizează enum-ul WinForms).</summary>
    <Category("K-BOT")>
    <Description("Alinierea conținutului în celulă.")>
    <DefaultValue(ContentAlignment.MiddleLeft)>
    Public Property TextAlign As ContentAlignment = ContentAlignment.MiddleLeft

    ''' <summary>Format .NET aplicat valorii la afișare (ex. „N2”, „dd.MM.yyyy”). Vid => ToString().</summary>
    <Category("K-BOT")>
    <Description("Format .NET aplicat valorii la afișare (ex. N2, dd.MM.yyyy). Vid => ToString().")>
    Public Property FormatString As String

    ''' <summary>
    ''' English (slice 0017-01): the aggregate this column contributes to the pinned totals row.
    ''' Default <see cref="KBotAggregate.None"/> (empty totals cell). Only meaningful when the
    ''' grid's <c>ShowTotalsRow</c> is True.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Ce agregat aduce coloana în rândul de totaluri. Contează doar când ShowTotalsRow e True.")>
    <DefaultValue(KBotAggregate.None)>
    Public Property Aggregate As KBotAggregate = KBotAggregate.None

    ''' <summary>
    ''' English (slice 0017-01): optional .NET format string for THIS column's aggregate value.
    ''' When empty, <see cref="KBotAggregate.Sum"/> / <see cref="KBotAggregate.Average"/> reuse
    ''' <see cref="FormatString"/>; <see cref="KBotAggregate.Count"/> ignores both and always
    ''' formats as a plain integer.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Format .NET pentru valoarea agregată. Vid => se reia FormatString (Count ignoră ambele).")>
    Public Property AggregateFormatString As String

    ''' <summary>
    ''' Sursa combo partajată pe coloană (override per-celulă prin evenimentul de formatare).
    ''' NU se serializează din designer: o listă de Object nu poate face rotunda prin
    ''' <c>InitializeComponent</c>, iar o sursă pe jumătate serializată e mai rea decât niciuna.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ComboItems As IList(Of Object)

    ''' <summary>
    ''' Grupul de exclusivitate pentru coloanele <see cref="KBotColumnType.OptionButton"/>.
    ''' Bifarea unei opțiuni le stinge pe celelalte din ACELAȘI RÂND care au același grup.
    ''' Vid => opțiunea e independentă (nu stinge nimic).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Grupul de exclusivitate al opțiunilor din același rând. Vid => opțiune independentă.")>
    Public Property OptionGroup As String

    ''' <summary>Minimul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    <Category("K-BOT")>
    <Description("Minimul barei de progres (doar pentru coloane ProgressBar).")>
    <DefaultValue(0.0R)>
    Public Property ProgressMin As Double = 0

    ''' <summary>Maximul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    <Category("K-BOT")>
    <Description("Maximul barei de progres (doar pentru coloane ProgressBar).")>
    <DefaultValue(100.0R)>
    Public Property ProgressMax As Double = 100

    ''' <summary>Redimensionabilă prin tragerea marginii din antet. Implicit True.</summary>
    <Category("K-BOT")>
    <Description("Redimensionabilă prin tragerea marginii din antet.")>
    <DefaultValue(True)>
    Public Property Resizable As Boolean = True

    ''' <summary>
    ''' Payload al caller-ului (nefolosit de control). NU se serializează din designer —
    ''' un Object arbitrar nu poate face rotunda prin <c>InitializeComponent</c>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>
    ''' English (slice 0025): parameterless constructor, required by the designer's collection
    ''' dialog — it creates the item first and fills in Key / ColumnType afterwards. A column
    ''' built this way is usable the moment it has a key.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>Cheia + tipul sunt fixate la creare; restul se lasă pe valorile implicite.</summary>
    Public Sub New(key As String, headerText As String, type As KBotColumnType, width As Integer)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        _key = key
        _columnType = type
        ' „Me.” e OBLIGATORIU: VB e case-insensitive, deci parametrul „headerText” ascunde
        ' proprietatea „HeaderText”, iar o atribuire nekalificată s-ar face parametrului.
        Me.HeaderText = If(headerText, String.Empty)
        _width = Math.Max(width, _minWidth)
    End Sub

    ''' <summary>Ce arată lista dialogului de colecție din designer.</summary>
    Public Overrides Function ToString() As String
        Dim shownKey As String = If(String.IsNullOrWhiteSpace(_key), "<fără cheie>", _key)
        Return shownKey & " — """ & If(HeaderText, String.Empty) & """ (" & _columnType.ToString() & ")"
    End Function

End Class
