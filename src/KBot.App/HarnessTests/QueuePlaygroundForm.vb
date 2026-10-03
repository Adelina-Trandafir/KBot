#If DEBUG Then
Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe
Imports KBot.Theming

''' <summary>
''' Slice 0100-03 playground: the REAL <see cref="RobotQueueForm"/> over a simulated robot
''' (<see cref="FakeForexeRunner"/>), so the multi-thread grids can be seen working -- the running ones
''' on top, the waiting angajamente (and waiting tasks) below. A property grid sits on every grid and
''' every column of that window: what is changed shows at once, and «Salveaza» writes the result as the
''' designer lines Visual Studio would serialize (see <see cref="QueueDesignerExporter"/>), ready to be
''' written into <c>RobotQueueForm.Designer.vb</c>.
''' </summary>
''' <remarks>
''' Nothing leaves the PC and FOREXE is never touched. The controller writes its usual files for each
''' simulated download (answer, package, run log) under the made-up codes, as the queue bench does.
''' </remarks>
Public Class QueuePlaygroundForm

    ''' <summary>One thing the property grid can show: a grid of the window, or one column of a grid.</summary>
    Private NotInheritable Class Target
        Public Property Text As String = String.Empty
        Public Property Grid As KBotDataView
        Public Property Column As KBotDataColumn
        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _runner As FakeForexeRunner
    Private ReadOnly _controller As ForexeController
    Private ReadOnly _queue As RobotQueue
    Private ReadOnly _targets As New List(Of Target)()
    Private ReadOnly _images As New List(Of KeyValuePair(Of String, Image))()
    ' Which resource each column's button picture came from (only what was picked here; the rest is found by comparing).
    Private ReadOnly _imageNames As New Dictionary(Of KBotDataColumn, String)()
    Private ReadOnly _random As New Random()
    Private _queueForm As RobotQueueForm
    Private _loadingImage As Boolean
    Private _run As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="log">The harness run log.</param>
    Public Sub New(log As Action(Of String))
        InitializeComponent()
        _log = log
        _runner = New FakeForexeRunner()
        _controller = New ForexeController(_runner, New SessionContext(), Nothing, New ServerGate()) With {.Owner = Me}
        _queue = New RobotQueue(Function() _controller.WaitUntilIdleAsync(), Sub(k_text) Say("COADA  " & k_text))
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        Try
            MyBase.OnShown(e)
            If _queue Is Nothing Then Return
            LoadResourceImages()
            OpenQueueWindow()
            Say("Playground pornit. Robotul e SIMULAT; nimic nu pleaca de pe PC.")
            Say("Porneste o descarcare multipla: lista de sus (in lucru) si cea de jos (in asteptare) se umplu in fereastra cozii.")
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.OnShown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _queue IsNot Nothing Then
                _queue.SetPaused(True)
                _queue.CancelAll()
                If _controller.IsBusy Then _controller.Cancel()
            End If
            If _queueForm IsNot Nothing AndAlso Not _queueForm.IsDisposed Then _queueForm.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.OnFormClosing", ex)
        End Try
        MyBase.OnFormClosing(e)
    End Sub

    ' ── The real window ────────────────────────────────────────────────────────────

    ' Opens the real queue window to the right of the playground (or brings the open one forward) and
    ' points the property grid at its grids.
    Private Sub OpenQueueWindow()
        If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then
            _queueForm = New RobotQueueForm(_queue, _controller)
            AddHandler _queueForm.FormClosed, Sub()
                                                  _queueForm = Nothing
                                                  LoadTargets()
                                              End Sub
            _queueForm.StartPosition = FormStartPosition.Manual
            _queueForm.Location = New Point(Bounds.Right + 8, Bounds.Top + 40)
            _queueForm.Show(Me)
            LoadTargets()
        Else
            _queueForm.BringToFront()
        End If
    End Sub

    Private Sub BtnFereastra_Click(sender As Object, e As EventArgs) Handles btnFereastra.Click
        Try
            OpenQueueWindow()
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.btnFereastra_Click", ex)
        End Try
    End Sub

    ' ── The simulated run ─────────────────────────────────────────────────────────

    ' One queued task «Actualizare multipla (N)», shaped like the shell's (KbotForm.Parallel.vb): the controller
    ' downloads the codes on several tabs at once, with the simulated robot underneath.
    Private Async Sub BtnPorneste_Click(sender As Object, e As EventArgs) Handles btnPorneste.Click
        Try
            _runner.RunSeconds = CDbl(numDurata.Value)
            Dim k_count As Integer = CInt(numAng.Value)
            Dim k_tabs As Integer = CInt(numTaburi.Value)
            Dim k_requests As New List(Of ParallelNodeRequest)()
            For k_i As Integer = 1 To k_count
                k_requests.Add(New ParallelNodeRequest With {.Cod = NewCode()})
            Next
            _run += 1
            Say($"Pornesc {k_count} angajamente, cel mult {k_tabs} deodata.")
            Await _queue.RunAsync("proba|multi|" & _run.ToString(CultureInfo.InvariantCulture), $"Actualizare multipla ({k_count})",
                                  Function() _controller.DownloadNodesParallelAsync(
                                      k_requests, k_tabs, Function(k_cod, k_ct) Task.FromResult(Of IstoricInfo)(Nothing)))
            Say("Descarcarea multipla s-a incheiat.")
        Catch ex As RobotTaskDroppedException
            Say("Scoasa din coada: " & ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.btnPorneste_Click", ex)
            Say("EROARE: " & ex.Message)
        End Try
    End Sub

    ' A plain task behind the run: it shows in the lower grid next to the waiting angajamente.
    Private Async Sub BtnSarcina_Click(sender As Object, e As EventArgs) Handles btnSarcina.Click
        Try
            Dim k_cod As String = NewCode()
            Dim k_seconds As Integer = CInt(numDurata.Value)
            _run += 1
            Await _queue.RunAsync("proba|sarcina|" & _run.ToString(CultureInfo.InvariantCulture), $"Descarcare completa «{k_cod}»",
                                  Function() Task.Delay(k_seconds * 1000))
        Catch ex As RobotTaskDroppedException
            Say("Scoasa din coada: " & ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.btnSarcina_Click", ex)
            Say("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnOpreste_Click(sender As Object, e As EventArgs) Handles btnOpreste.Click
        Try
            _queue.CancelAll()
            If _controller.IsBusy Then _controller.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.btnOpreste_Click", ex)
        End Try
    End Sub

    ' An angajament code of the real shape: AAB + 8 letters / digits.
    Private Function NewCode() As String
        Const Alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789"
        Dim k_sb As New StringBuilder("AAB")
        For k_i As Integer = 1 To 8
            k_sb.Append(Alphabet(_random.Next(Alphabet.Length)))
        Next
        Return k_sb.ToString()
    End Function

    ' ── The property grid ─────────────────────────────────────────────────────────

    Private Sub LoadTargets()
        Try
            If IsDisposed OrElse Disposing Then Return
            _targets.Clear()
            cboTinta.Items.Clear()
            pg.SelectedObject = Nothing
            If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then Return
            AddGridTargets("gridDescarcari", _queueForm.gridDescarcari)
            AddGridTargets("gridAsteapta", _queueForm.gridAsteapta)
            cboTinta.Items.AddRange(_targets.ToArray())
            If cboTinta.Items.Count > 0 Then cboTinta.SelectedIndex = 0
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.LoadTargets", ex)
        End Try
    End Sub

    Private Sub AddGridTargets(k_name As String, k_grid As KBotDataView)
        _targets.Add(New Target With {.Text = k_name & "  (grila)", .Grid = k_grid})
        For Each k_col As KBotDataColumn In k_grid.Columns
            _targets.Add(New Target With {.Text = $"{k_name}  /  coloana «{k_col.Key}»", .Grid = k_grid, .Column = k_col})
        Next
    End Sub

    Private Sub CboTinta_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTinta.SelectedIndexChanged
        Try
            Dim k_target As Target = TryCast(cboTinta.SelectedItem, Target)
            If k_target Is Nothing Then Return
            pg.SelectedObject = If(k_target.Column IsNot Nothing, DirectCast(k_target.Column, Object), k_target.Grid)
            ShowImageOf(k_target)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.cboTinta_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' A change in the property grid is painted by the control itself; the grid is only asked to redraw.
    Private Sub Pg_PropertyValueChanged(s As Object, e As PropertyValueChangedEventArgs) Handles pg.PropertyValueChanged
        Try
            Dim k_target As Target = TryCast(cboTinta.SelectedItem, Target)
            If k_target Is Nothing Then Return
            k_target.Grid.Invalidate()
            ShowImageOf(k_target)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.pg_PropertyValueChanged", ex)
        End Try
    End Sub

    ' ── The button picture ─────────────────────────────────────────────────────────

    ' Every Image of the project's resources, by name (the same names the designer writes after
    ' «My.Resources.Resources.»).
    Private Sub LoadResourceImages()
        _images.Clear()
        Dim k_flags As BindingFlags = BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic
        For Each k_prop As PropertyInfo In GetType(My.Resources.Resources).GetProperties(k_flags)
            If Not GetType(Image).IsAssignableFrom(k_prop.PropertyType) Then Continue For
            Try
                Dim k_image As Image = TryCast(k_prop.GetValue(Nothing), Image)
                If k_image IsNot Nothing Then _images.Add(New KeyValuePair(Of String, Image)(k_prop.Name, k_image))
            Catch ex As Exception
                GlobalErrorLog.Write("QueuePlaygroundForm.LoadResourceImages", ex)
            End Try
        Next
        _images.Sort(Function(k_a, k_b) String.Compare(k_a.Key, k_b.Key, StringComparison.OrdinalIgnoreCase))
        cboImagine.Items.Clear()
        cboImagine.Items.Add("(fara poza)")
        For Each k_item As KeyValuePair(Of String, Image) In _images
            cboImagine.Items.Add(k_item.Key)
        Next
        cboImagine.SelectedIndex = 0
    End Sub

    ' The combo follows the selected column: its picture's resource name, or «(fara poza)». Only a column has one.
    Private Sub ShowImageOf(k_target As Target)
        _loadingImage = True
        Try
            cboImagine.Enabled = k_target.Column IsNot Nothing
            Dim k_name As String = If(k_target.Column Is Nothing, Nothing, NameOfImage(k_target.Column.ButtonImage))
            Dim k_index As Integer = If(k_name Is Nothing, 0, cboImagine.Items.IndexOf(k_name))
            cboImagine.SelectedIndex = Math.Max(0, k_index)
        Finally
            _loadingImage = False
        End Try
    End Sub

    Private Sub CboImagine_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboImagine.SelectedIndexChanged
        Try
            If _loadingImage Then Return
            Dim k_target As Target = TryCast(cboTinta.SelectedItem, Target)
            If k_target Is Nothing OrElse k_target.Column Is Nothing Then Return
            If cboImagine.SelectedIndex <= 0 Then
                k_target.Column.ButtonImage = Nothing
                _imageNames.Remove(k_target.Column)
            Else
                Dim k_pair As KeyValuePair(Of String, Image) = _images(cboImagine.SelectedIndex - 1)
                k_target.Column.ButtonImage = k_pair.Value
                _imageNames(k_target.Column) = k_pair.Key
            End If
            k_target.Grid.Invalidate()
            pg.Refresh()
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.cboImagine_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The resource an image came from: the very same object, or one of the same size and pixels.
    Private Function NameOfImage(k_image As Image) As String
        If k_image Is Nothing Then Return Nothing
        For Each k_item As KeyValuePair(Of String, Image) In _images
            If ReferenceEquals(k_item.Value, k_image) Then Return k_item.Key
        Next
        Dim k_bytes As Byte() = Nothing
        For Each k_item As KeyValuePair(Of String, Image) In _images
            If k_item.Value.Size <> k_image.Size Then Continue For
            If k_bytes Is Nothing Then k_bytes = PngBytes(k_image)
            If k_bytes.AsSpan().SequenceEqual(PngBytes(k_item.Value)) Then Return k_item.Key
        Next
        Return Nothing
    End Function

    Private Shared Function PngBytes(k_image As Image) As Byte()
        Using k_stream As New MemoryStream()
            k_image.Save(k_stream, Imaging.ImageFormat.Png)
            Return k_stream.ToArray()
        End Using
    End Function

    ' ── Save ──────────────────────────────────────────────────────────────────────

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then
                lblSalvat.Text = "Fereastra cozii e inchisa: deschide-o si apoi salveaza."
                Return
            End If
            Dim k_grids As New List(Of KeyValuePair(Of String, KBotDataView)) From {
                New KeyValuePair(Of String, KBotDataView)("gridDescarcari", _queueForm.gridDescarcari),
                New KeyValuePair(Of String, KBotDataView)("gridAsteapta", _queueForm.gridAsteapta)}
            Dim k_text As String = QueueDesignerExporter.Build(k_grids, AddressOf NameOfImage)
            txtExport.Text = k_text

            Dim k_folder As String = Path.Combine(AppContext.BaseDirectory, "Logs")
            Directory.CreateDirectory(k_folder)
            Dim k_path As String = Path.Combine(k_folder, "playground_coada_designer.txt")
            File.WriteAllText(k_path, k_text, New UTF8Encoding(False))
            Try
                Clipboard.SetText(k_text)
                lblSalvat.Text = "Salvat si copiat in clipboard: " & k_path
            Catch ex As Exception
                ' The clipboard can be held by another program: the file is the copy that counts.
                GlobalErrorLog.Write("QueuePlaygroundForm.btnSalveaza_Click", ex)
                lblSalvat.Text = "Salvat (clipboard-ul nu s-a putut folosi): " & k_path
            End Try
            Say("Salvat: " & k_path)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.btnSalveaza_Click", ex)
            lblSalvat.Text = "EROARE la salvare: " & ex.Message
        End Try
    End Sub

    Private Sub Say(k_text As String)
        Try
            _log?.Invoke(k_text)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.Say", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme = ThemeManager.Current
            Dim k_palette = k_scheme.Palette
            txtExport.BackColor = k_palette.SurfaceAltColor
            txtExport.ForeColor = k_palette.TextColor
            lblSalvat.ForeColor = k_palette.TextColor
            For Each k_button As Button In flpComenzi.Controls.OfType(Of Button)()
                ButtonStyles.ApplySecondary(k_button, k_scheme)
            Next
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            ButtonStyles.ApplySecondary(btnInchide, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class

''' <summary>
''' Writes the state of KBotDataView grids and columns as the lines Visual Studio's designer would
''' serialize: for every property, exactly the ones its own <c>ShouldSerialize*</c> says are set. A
''' colour or font left «from the theme» therefore stays out, and a freshly dropped column writes
''' nothing -- the trap <see cref="TreeSettingsExporter"/> documents. Variable names differ from
''' form to form (VS numbers its columns), so each block says which grid and which column KEY it is
''' and uses the placeholders {GRID} / {COL}.
''' </summary>
Friend NotInheritable Class QueueDesignerExporter

    Private Shared ReadOnly _inv As CultureInfo = CultureInfo.InvariantCulture

    Private Sub New()
    End Sub

    ''' <param name="k_nameOfImage">The resource name of an image (Nothing = not from the resources).</param>
    Friend Shared Function Build(k_grids As IEnumerable(Of KeyValuePair(Of String, KBotDataView)),
                                 k_nameOfImage As Func(Of Image, String)) As String
        Try
            Dim k_sb As New StringBuilder()
            k_sb.AppendLine($"' Playground «Coada robotului», salvat {Date.Now:dd.MM.yyyy HH:mm}")
            k_sb.AppendLine("' Ce ar scrie designerul pentru starea de acum. {GRID} = grila, {COL} = coloana cu cheia indicata")
            k_sb.AppendLine("' (variabila ei din Designer.vb se cauta dupa .Key). Ce lipseste aici = ramane cum e implicit.")
            k_sb.AppendLine()
            For Each k_pair As KeyValuePair(Of String, KBotDataView) In k_grids
                k_sb.AppendLine($"' ===== {k_pair.Key} (grila) =====")
                WriteProperties(k_sb, k_pair.Value, "{GRID}", True, k_nameOfImage)
                For Each k_col As KBotDataColumn In k_pair.Value.Columns
                    k_sb.AppendLine()
                    k_sb.AppendLine($"' ===== {k_pair.Key} / coloana Key = ""{k_col.Key}"" =====")
                    WriteProperties(k_sb, k_col, "{COL}", False, k_nameOfImage)
                Next
                k_sb.AppendLine()
            Next
            Return k_sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("QueueDesignerExporter.Build", ex)
            Throw
        End Try
    End Function

    ' k_kbotOnly: for a CONTROL only the properties of the K-BOT categories (layout, name and the rest are the form's).
    Private Shared Sub WriteProperties(k_sb As StringBuilder, k_obj As Object, k_prefix As String,
                                       k_kbotOnly As Boolean, k_nameOfImage As Func(Of Image, String))
        Dim k_props As List(Of PropertyDescriptor) =
            TypeDescriptor.GetProperties(k_obj).Cast(Of PropertyDescriptor)().
                OrderBy(Function(k_p) k_p.Name, StringComparer.Ordinal).ToList()
        For Each k_prop As PropertyDescriptor In k_props
            If k_prop.IsReadOnly Then Continue For
            Dim k_visibility As DesignerSerializationVisibilityAttribute =
                TryCast(k_prop.Attributes(GetType(DesignerSerializationVisibilityAttribute)), DesignerSerializationVisibilityAttribute)
            If k_visibility IsNot Nothing AndAlso k_visibility.Visibility <> DesignerSerializationVisibility.Visible Then Continue For
            If k_kbotOnly AndAlso Not k_prop.Category.StartsWith("K-BOT", StringComparison.Ordinal) Then Continue For
            If Not k_prop.ShouldSerializeValue(k_obj) Then Continue For

            Dim k_literal As String = Literal(k_prop.GetValue(k_obj), k_nameOfImage)
            If k_literal Is Nothing Then
                k_sb.AppendLine($"' {k_prefix}.{k_prop.Name}: tip {k_prop.PropertyType.Name} nesuportat de export (de scris de mana){If(TypeOf k_obj Is KBotDataColumn AndAlso k_prop.Name = "ButtonImage", "; poza nu e din resurse - alege-o din lista", String.Empty)}")
            Else
                k_sb.AppendLine($"{k_prefix}.{k_prop.Name} = {k_literal}")
            End If
        Next
    End Sub

    ' A value as VB source; Nothing = a type this exporter does not write.
    Private Shared Function Literal(k_value As Object, k_nameOfImage As Func(Of Image, String)) As String
        If k_value Is Nothing Then Return "Nothing"
        Dim k_text As String = TryCast(k_value, String)
        If k_text IsNot Nothing Then
            Return """" & k_text.Replace("""", """""").Replace(vbCrLf, """ & vbCrLf & """).Replace(vbLf, """ & vbLf & """) & """"
        End If
        If TypeOf k_value Is Boolean Then Return If(CBool(k_value), "True", "False")
        If TypeOf k_value Is Integer OrElse TypeOf k_value Is Long OrElse TypeOf k_value Is Short OrElse TypeOf k_value Is Byte Then
            Return Convert.ToString(k_value, _inv)
        End If
        If TypeOf k_value Is Single Then Return CSng(k_value).ToString("R", _inv) & "F"
        If TypeOf k_value Is Double Then Return CDbl(k_value).ToString("R", _inv)
        If TypeOf k_value Is Decimal Then Return CDec(k_value).ToString(_inv) & "D"
        If k_value.GetType().IsEnum Then Return EnumLiteral(k_value)
        If TypeOf k_value Is Padding Then
            Dim k_p As Padding = DirectCast(k_value, Padding)
            Return If(k_p.All >= 0, $"New Padding({k_p.All})", $"New Padding({k_p.Left}, {k_p.Top}, {k_p.Right}, {k_p.Bottom})")
        End If
        If TypeOf k_value Is Size Then Return $"New Size({DirectCast(k_value, Size).Width}, {DirectCast(k_value, Size).Height})"
        If TypeOf k_value Is Point Then Return $"New Point({DirectCast(k_value, Point).X}, {DirectCast(k_value, Point).Y})"
        If TypeOf k_value Is Color Then Return ColorLiteral(DirectCast(k_value, Color))
        If TypeOf k_value Is Font Then Return FontLiteral(DirectCast(k_value, Font))
        If TypeOf k_value Is Image Then
            Dim k_name As String = k_nameOfImage?.Invoke(DirectCast(k_value, Image))
            Return If(String.IsNullOrEmpty(k_name), Nothing, "My.Resources.Resources." & k_name)
        End If
        Return Nothing
    End Function

    Private Shared Function EnumLiteral(k_value As Object) As String
        Dim k_type As Type = k_value.GetType()
        Dim k_typeName As String = If(k_type.Namespace IsNot Nothing AndAlso k_type.Namespace.StartsWith("KBot", StringComparison.Ordinal),
                                      k_type.FullName.Replace("+"c, "."c), k_type.Name)
        Dim k_names As String() = k_value.ToString().Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries)
        Return String.Join(" Or ", k_names.Select(Function(k_n) k_typeName & "." & k_n))
    End Function

    Private Shared Function ColorLiteral(k_color As Color) As String
        If k_color.IsEmpty Then Return "Color.Empty"
        If k_color.IsSystemColor Then Return "SystemColors." & k_color.Name
        If k_color.IsNamedColor Then Return "Color." & k_color.Name
        Return $"Color.FromArgb({k_color.A}, {k_color.R}, {k_color.G}, {k_color.B})"
    End Function

    Private Shared Function FontLiteral(k_font As Font) As String
        Dim k_text As String = $"New Font(""{k_font.Name}"", {k_font.SizeInPoints.ToString("R", _inv)}F"
        If k_font.Style <> FontStyle.Regular Then
            k_text &= ", " & String.Join(" Or ", k_font.Style.ToString().Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries).
                                         Select(Function(k_n) "FontStyle." & k_n))
        End If
        Return k_text & ")"
    End Function
End Class
#End If
