Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Obiectul pe care îl editează PropertyGrid-ul din <see cref="ThemeEditorForm"/>. Stă între
''' controlul VIU și intrarea din fișier: fiecare setare face DOUĂ lucruri — o aplică pe control
''' (previzualizare imediată) și o reține în <see cref="ControlStyleOverride"/> (ce se salvează).
'''
''' De ce un proxy și nu controlul însuși în grilă: un <c>Control</c> real ar expune vreo sută de
''' proprietăți, majoritatea nepotrivite pentru un editor de stiluri (Dock, TabIndex, Anchor —
''' orice atinsă acolo strică aranjarea, nu culorile). Proxy-ul expune EXACT sloturile pe care le
''' cunoaște formatul de fișier.
'''
''' Onestitate: sloturile hover/contur/accent/selecție se aplică doar controalelor care expun o
''' proprietate cu numele potrivit (vezi tabelele din <see cref="ThemeOverrideApplier"/>). Pe un
''' control obișnuit ele se SALVEAZĂ, dar nu se văd. Proprietatea read-only
''' <see cref="SloturiAplicabile"/> spune negru pe alb care dintre ele au efect pe ținta curentă,
''' ca operatorul să nu creadă că a schimbat ceva ce nu s-a schimbat.
''' </summary>
Public NotInheritable Class ControlStyleProxy

    Private ReadOnly _target As Control
    Private ReadOnly _entry As ControlStyleOverride

    Public Sub New(target As Control, entry As ControlStyleOverride)
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        _target = target
        _entry = entry
    End Sub

    ''' <summary>Controlul viu editat (folosit de formular pentru reset/evidențiere).</summary>
    <Browsable(False)>
    Public ReadOnly Property Target As Control
        Get
            Return _target
        End Get
    End Property

    ''' <summary>Intrarea din fișier care acumulează alegerile.</summary>
    <Browsable(False)>
    Public ReadOnly Property Entry As ControlStyleOverride
        Get
            Return _entry
        End Get
    End Property

    ' ── Identitate (read-only, doar ca operatorul să știe pe ce lucrează) ─────────

    <Category("1. Control")>
    <Description("Numele controlului în formular.")>
    <DisplayName("Nume")>
    Public ReadOnly Property Nume As String
        Get
            Return If(String.IsNullOrWhiteSpace(_target.Name), "(fără nume)", _target.Name)
        End Get
    End Property

    <Category("1. Control")>
    <Description("Tipul controlului.")>
    <DisplayName("Tip")>
    Public ReadOnly Property Tip As String
        Get
            Return _target.GetType().Name
        End Get
    End Property

    <Category("1. Control")>
    <Description("Calea în ierarhie — cheia sub care se salvează alegerile în fișierul JSON.")>
    <DisplayName("Cale")>
    Public ReadOnly Property Cale As String
        Get
            Return _entry.Path
        End Get
    End Property

    <Category("1. Control")>
    <Description("Care dintre sloturile suplimentare au efect pe ACEST control. Restul se salvează, dar nu se văd.")>
    <DisplayName("Sloturi aplicabile")>
    Public ReadOnly Property SloturiAplicabile As String
        Get
            Dim found As New List(Of String)()
            AddIfSupported(found, "Hover", ThemeOverrideApplier.HoverColorNames)
            AddIfSupported(found, "Contur", ThemeOverrideApplier.BorderColorNames)
            AddIfSupported(found, "Accent", ThemeOverrideApplier.AccentColorNames)
            AddIfSupported(found, "Selecție fundal", ThemeOverrideApplier.SelectionBackColorNames)
            AddIfSupported(found, "Selecție text", ThemeOverrideApplier.SelectionForeColorNames)
            If found.Count = 0 Then Return "(niciunul — doar fundal, text și font)"
            Return String.Join(", ", found)
        End Get
    End Property

    ' ── Culorile de bază: există pe ORICE control ────────────────────────────────

    <Category("2. Culori")>
    <Description("Fundalul controlului.")>
    <DisplayName("Fundal (BackColor)")>
    Public Property Fundal As Color
        Get
            Return _target.BackColor
        End Get
        Set(value As Color)
            Apply(Sub()
                      _target.BackColor = value
                      _entry.BackColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Fundal")
        End Set
    End Property

    <Category("2. Culori")>
    <Description("Culoarea textului.")>
    <DisplayName("Text (ForeColor)")>
    Public Property Text As Color
        Get
            Return _target.ForeColor
        End Get
        Set(value As Color)
            Apply(Sub()
                      _target.ForeColor = value
                      _entry.ForeColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Text")
        End Set
    End Property

    ' ── Sloturi suplimentare: doar pe controalele care le expun ──────────────────

    <Category("3. Culori suplimentare")>
    <Description("Fundalul sub cursor. Are efect doar dacă apare în «Sloturi aplicabile».")>
    <DisplayName("Hover")>
    Public Property Hover As Color
        Get
            Return ThemeOverrideApplier.ReadColor(_target, ThemeOverrideApplier.HoverColorNames)
        End Get
        Set(value As Color)
            Apply(Sub()
                      ThemeOverrideApplier.TrySetColor(_target, ThemeOverrideApplier.HoverColorNames, value)
                      _entry.HoverColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Hover")
        End Set
    End Property

    <Category("3. Culori suplimentare")>
    <Description("Conturul controlului. Are efect doar dacă apare în «Sloturi aplicabile».")>
    <DisplayName("Contur")>
    Public Property Contur As Color
        Get
            Return ThemeOverrideApplier.ReadColor(_target, ThemeOverrideApplier.BorderColorNames)
        End Get
        Set(value As Color)
            Apply(Sub()
                      ThemeOverrideApplier.TrySetColor(_target, ThemeOverrideApplier.BorderColorNames, value)
                      _entry.BorderColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Contur")
        End Set
    End Property

    <Category("3. Culori suplimentare")>
    <Description("Culoarea de accent. Are efect doar dacă apare în «Sloturi aplicabile».")>
    <DisplayName("Accent")>
    Public Property Accent As Color
        Get
            Return ThemeOverrideApplier.ReadColor(_target, ThemeOverrideApplier.AccentColorNames)
        End Get
        Set(value As Color)
            Apply(Sub()
                      ThemeOverrideApplier.TrySetColor(_target, ThemeOverrideApplier.AccentColorNames, value)
                      _entry.AccentColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Accent")
        End Set
    End Property

    <Category("3. Culori suplimentare")>
    <Description("Fundalul rândului selectat. Are efect doar dacă apare în «Sloturi aplicabile».")>
    <DisplayName("Selecție - fundal")>
    Public Property SelectieFundal As Color
        Get
            Return ThemeOverrideApplier.ReadColor(_target, ThemeOverrideApplier.SelectionBackColorNames)
        End Get
        Set(value As Color)
            Apply(Sub()
                      ThemeOverrideApplier.TrySetColor(_target, ThemeOverrideApplier.SelectionBackColorNames, value)
                      _entry.SelectionBackColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Selecție fundal")
        End Set
    End Property

    <Category("3. Culori suplimentare")>
    <Description("Textul rândului selectat. Are efect doar dacă apare în «Sloturi aplicabile».")>
    <DisplayName("Selecție - text")>
    Public Property SelectieText As Color
        Get
            Return ThemeOverrideApplier.ReadColor(_target, ThemeOverrideApplier.SelectionForeColorNames)
        End Get
        Set(value As Color)
            Apply(Sub()
                      ThemeOverrideApplier.TrySetColor(_target, ThemeOverrideApplier.SelectionForeColorNames, value)
                      _entry.SelectionForeColor = ControlStyleOverride.FromColor(value)
                  End Sub, "Selecție text")
        End Set
    End Property

    ' ── Font ─────────────────────────────────────────────────────────────────────

    <Category("4. Font")>
    <Description("Fontul controlului. Copiii care nu au font propriu îl moștenesc.")>
    <DisplayName("Font")>
    Public Property Font As Font
        Get
            Return _target.Font
        End Get
        Set(value As Font)
            Apply(Sub()
                      _target.Font = value
                      _entry.SetFont(value)
                  End Sub, "Font")
        End Set
    End Property

    ''' <summary>
    ''' Șterge toate alegerile pentru acest control și pune controlul înapoi pe valorile din
    ''' designer (instantaneul <see cref="DesignerBaseline"/>). Sloturile suplimentare se întorc
    ''' pe <c>Color.Empty</c>, care în contractul controalelor K-BOT înseamnă «din temă».
    ''' </summary>
    Public Sub ResetAll()
        Try
            _entry.BackColor = Nothing
            _entry.ForeColor = Nothing
            _entry.HoverColor = Nothing
            _entry.BorderColor = Nothing
            _entry.AccentColor = Nothing
            _entry.SelectionBackColor = Nothing
            _entry.SelectionForeColor = Nothing
            _entry.SetFont(Nothing)

            ForceEmpty(ThemeOverrideApplier.HoverColorNames)
            ForceEmpty(ThemeOverrideApplier.BorderColorNames)
            ForceEmpty(ThemeOverrideApplier.AccentColorNames)
            ForceEmpty(ThemeOverrideApplier.SelectionBackColorNames)
            ForceEmpty(ThemeOverrideApplier.SelectionForeColorNames)

            ' Instantaneul e cel de la PRIMA traversare, adică valorile autorite în designer.
            DesignerBaseline.Restore(_target)
            _target.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ControlStyleProxy.ResetAll", ex)
            Throw
        End Try
    End Sub

    ' TrySetColor refuză Color.Empty (acolo Empty înseamnă «neatins»); la reset vrem exact
    ' contrariul — să SCRIEM Empty, ca proprietatea să redevină «din temă».
    Private Sub ForceEmpty(candidates As String())
        Dim pd As PropertyDescriptor = ThemeOverrideApplier.FindColorProperty(_target, candidates)
        If pd IsNot Nothing Then pd.SetValue(_target, Color.Empty)
    End Sub

    Private Sub AddIfSupported(into As List(Of String), label As String, candidates As String())
        If ThemeOverrideApplier.FindColorProperty(_target, candidates) IsNot Nothing Then into.Add(label)
    End Sub

    ' Frontieră de editare: o proprietate din grilă nu are voie să arunce în PropertyGrid (ar
    ' produce un dialog de eroare urât și ar lăsa grila într-o stare inconsistentă). Logăm.
    Private Sub Apply(action As Action, slot As String)
        Try
            action()
            _target.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write($"ControlStyleProxy.Set({slot})", ex)
        End Try
    End Sub

End Class
