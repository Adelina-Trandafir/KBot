Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Editorul de stiluri (felia 0028, punctul 3): o fereastră-unealtă care ia o SUPRAFAȚĂ deja
''' deschisă — fereastra gazdă sau una dintre vederile ei — îi enumeră controalele și lasă
''' operatorul să le schimbe fundalul, textul, hover-ul, conturul, accentul, culorile de selecție
''' și fontul, cu efect IMEDIAT pe ecran. La final, alegerile se salvează ca fișier JSON.
'''
''' Trei lucruri de știut despre ea:
'''
'''  1. **Nu e modală.** Trebuie să poți vedea efectul pe fereastra din spate în timp ce alegi
'''     culoarea; un dialog modal ar acoperi exact ce încerci să reglezi. De aceea se deschide cu
'''     <c>Show(owner)</c> (vezi <see cref="ShowFor"/>), nu cu <c>ShowDialog</c>.
'''  2. **Editează instanțe vii, nu fișierul de designer.** Ce se vede se pierde la repornire —
'''     până când felia următoare va citi fișierul salvat la pornire. Cerința operatorului spune
'''     explicit «later, not in this slice», deci AICI NU SE CITEȘTE NIMIC LA PORNIRE.
'''  3. **Fiecare suprafață are setul ei.** Alegerile se țin într-un dicționar pe numele
'''     suprafeței, deci comutarea între MainForm și o vedere nu pierde ce s-a lucrat.
'''
''' Arborele se oprește la controalele <c>IThemedControl</c>, exact ca traversarea din
''' <c>ThemeManager</c>: copiii lor sunt interni (banda de căutare a arborelui, casetele grilei),
''' n-au fost autoriți în designer și nu sunt ai operatorului. Vederile apar ca frunze în arborele
''' ferestrei și ca suprafețe separate în lista de sus — sunt gazde în sine.
''' </summary>
Public Class ThemeEditorForm

    Private ReadOnly _host As Form
    Private ReadOnly _sets As New Dictionary(Of String, ThemeOverrideSet)(StringComparer.Ordinal)
    Private _scopes As New List(Of ThemeScope)()
    Private _scope As ThemeScope
    Private _suppressScopeEvents As Boolean = False

    ''' <param name="host">Fereastra pe care o inspectăm. Nu se modifică decât prin editare.</param>
    Public Sub New(host As Form)
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        InitializeComponent()
        _host = host
    End Sub

    ''' <summary>
    ''' Deschide editorul pentru fereastra dată, ne-modal, deținut de ea. Dacă e deja deschis
    ''' pentru aceeași fereastră, îl aduce în față în loc să deschidă un al doilea — două editoare
    ''' pe aceleași controale ar avea fiecare setul lui și s-ar suprascrie reciproc la salvare.
    ''' </summary>
    Public Shared Function ShowFor(host As Form) As ThemeEditorForm
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        Try
            For Each f As Form In host.OwnedForms
                Dim existing As ThemeEditorForm = TryCast(f, ThemeEditorForm)
                If existing IsNot Nothing Then
                    existing.BringToFront()
                    existing.Activate()
                    Return existing
                End If
            Next

            Dim editor As New ThemeEditorForm(host)
            editor.Show(host)
            Return editor
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.ShowFor", ex)
            Throw
        End Try
    End Function

    Protected Overrides Sub OnLoad(e As EventArgs)
        Try
            MyBase.OnLoad(e)   ' KBotThemedForm: înregistrare + aplicarea temei pe editor
            ReloadScopes()
        Catch ex As Exception
            ' Frontieră UI (Load): un throw ar rupe deschiderea ferestrei.
            GlobalErrorLog.Write("ThemeEditorForm.OnLoad", ex)
        End Try
    End Sub

    ' ---------------- suprafețe ----------------

    ''' <summary>Recompune lista de suprafețe (vederile se creează leneș — de aici butonul).</summary>
    Private Sub ReloadScopes()
        Try
            Dim previous As String = If(_scope Is Nothing, Nothing, _scope.ScopeName)
            _scopes = ThemeScope.Collect(_host)

            _suppressScopeEvents = True
            cboScope.Items.Clear()
            For Each s In _scopes
                cboScope.Items.Add(s)
            Next
            _suppressScopeEvents = False

            Dim index As Integer = 0
            If previous IsNot Nothing Then
                For i As Integer = 0 To _scopes.Count - 1
                    If String.Equals(_scopes(i).ScopeName, previous, StringComparison.Ordinal) Then
                        index = i
                        Exit For
                    End If
                Next
            End If
            If cboScope.Items.Count > 0 Then cboScope.SelectedIndex = index
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.ReloadScopes", ex)
            Throw
        End Try
    End Sub

    Private Sub cboScope_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScope.SelectedIndexChanged
        Try
            If _suppressScopeEvents Then Return
            _scope = TryCast(cboScope.SelectedItem, ThemeScope)
            BuildTree()
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.cboScope_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            ReloadScopes()
            SetStatus($"{_scopes.Count} suprafețe găsite.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.btnRefresh_Click", ex)
        End Try
    End Sub

    ' ---------------- arborele de controale ----------------

    Private Sub BuildTree()
        Try
            treeControls.BeginUpdate()
            Try
                treeControls.Nodes.Clear()
                grid.SelectedObject = Nothing
                If _scope Is Nothing Then Return

                Dim root As New TreeNode(NodeLabel(_scope.Root)) With {.Tag = _scope.Root}
                treeControls.Nodes.Add(root)
                AddChildren(root, _scope.Root)
                root.ExpandAll()
                treeControls.SelectedNode = root
            Finally
                treeControls.EndUpdate()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' Recursie oprită în două locuri, ambele intenționat:
    '  · IThemedControl — copiii lui sunt interni, nu ai operatorului (vezi ThemeManager.Traverse);
    '  · UserControl    — e o suprafață în sine, editabilă din lista de sus.
    Private Sub AddChildren(parentNode As TreeNode, parent As Control)
        For Each child As Control In parent.Controls
            Dim node As New TreeNode(NodeLabel(child)) With {.Tag = child}
            parentNode.Nodes.Add(node)

            Dim isBoundary As Boolean = TypeOf child Is IThemedControl OrElse
                                        (TypeOf child Is UserControl AndAlso Not ReferenceEquals(child, _scope.Root))
            If Not isBoundary Then AddChildren(node, child)
        Next
    End Sub

    Private Shared Function NodeLabel(ctrl As Control) As String
        Dim name As String = If(String.IsNullOrWhiteSpace(ctrl.Name), "(fără nume)", ctrl.Name)
        Return $"{name}  —  {ctrl.GetType().Name}"
    End Function

    Private Sub treeControls_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles treeControls.AfterSelect
        Try
            Dim ctrl As Control = TryCast(e.Node?.Tag, Control)
            If ctrl Is Nothing OrElse _scope Is Nothing Then
                grid.SelectedObject = Nothing
                Return
            End If

            Dim path As String = ControlPath.Build(_scope.Root, ctrl)
            If path Is Nothing Then
                grid.SelectedObject = Nothing
                SetStatus("Controlul nu mai e sub suprafața curentă.")
                Return
            End If

            Dim entry = CurrentSet().GetOrCreate(path, ctrl.GetType().FullName)
            grid.SelectedObject = New ControlStyleProxy(ctrl, entry)
            SetStatus($"{CurrentSet().TouchedCount} controale cu alegeri.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.treeControls_AfterSelect", ex)
        End Try
    End Sub

    ' ---------------- setul curent ----------------

    Private Function CurrentSet() As ThemeOverrideSet
        Dim key As String = If(_scope Is Nothing, String.Empty, _scope.ScopeName)
        Dim found As ThemeOverrideSet = Nothing
        If Not _sets.TryGetValue(key, found) Then
            found = New ThemeOverrideSet With {
                .Name = key,
                .Scope = key,
                .BaseScheme = ThemeManager.Current.Name
            }
            _sets(key) = found
        End If
        Return found
    End Function

    ' ---------------- resetare ----------------

    Private Sub btnResetControl_Click(sender As Object, e As EventArgs) Handles btnResetControl.Click
        Try
            Dim proxy As ControlStyleProxy = TryCast(grid.SelectedObject, ControlStyleProxy)
            If proxy Is Nothing Then
                SetStatus("Selectează întâi un control.")
                Return
            End If
            proxy.ResetAll()
            grid.Refresh()
            SetStatus($"«{proxy.Nume}» readus la valorile din designer.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.btnResetControl_Click", ex)
            ShowError("Resetarea controlului a eșuat.", ex)
        End Try
    End Sub

    Private Sub btnResetAll_Click(sender As Object, e As EventArgs) Handles btnResetAll.Click
        Try
            If _scope Is Nothing Then Return
            If MessageBox.Show(Me,
                    $"Se șterg toate alegerile pentru «{_scope.ScopeName}» și controalele revin la valorile din designer. Continui?",
                    "Reset tot", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim n As Integer = ResetScope()
            SetStatus($"{n} controale readuse la valorile din designer.")
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.btnResetAll_Click", ex)
            ShowError("Resetarea suprafeței a eșuat.", ex)
        End Try
    End Sub

    ' Resetăm doar controalele care CHIAR au o intrare — restul n-au fost atinse niciodată, iar o
    ' restaurare oarbă a întregii ierarhii ar sterge și ce a scris tema pe drum.
    Private Function ResetScope() As Integer
        Dim styleSet As ThemeOverrideSet = CurrentSet()
        Dim n As Integer = 0
        For Each entry In styleSet.Entries
            Dim ctrl As Control = ControlPath.Resolve(_scope.Root, entry.Path)
            If ctrl Is Nothing Then Continue For
            Dim proxy As New ControlStyleProxy(ctrl, entry)
            proxy.ResetAll()
            n += 1
        Next
        styleSet.Entries.Clear()
        grid.SelectedObject = Nothing
        BuildTree()
        Return n
    End Function

    ' ---------------- salvare / încărcare ----------------

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If _scope Is Nothing Then Return
            Dim styleSet As ThemeOverrideSet = CurrentSet()
            styleSet.Prune()
            If styleSet.Entries.Count = 0 Then
                SetStatus("Nu există nicio alegere de salvat.")
                Return
            End If

            Dim defaultPath As String = ThemeOverrideStore.DefaultPathFor(_scope.ScopeName)
            Directory.CreateDirectory(ThemeOverrideStore.OverridesFolder)

            Using dlg As New SaveFileDialog()
                dlg.Title = "Salvează stilurile"
                dlg.Filter = "Fișiere de stiluri (*.json)|*.json|Toate fișierele (*.*)|*.*"
                dlg.DefaultExt = "json"
                dlg.InitialDirectory = ThemeOverrideStore.OverridesFolder
                dlg.FileName = Path.GetFileName(defaultPath)
                dlg.OverwritePrompt = True
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

                ThemeOverrideStore.Save(styleSet, dlg.FileName)
                SetStatus($"Salvat: {Path.GetFileName(dlg.FileName)} ({styleSet.Entries.Count} controale).")
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.btnSave_Click", ex)
            ShowError("Salvarea a eșuat.", ex)
        End Try
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Try
            If _scope Is Nothing Then Return

            Using dlg As New OpenFileDialog()
                dlg.Title = "Încarcă stiluri"
                dlg.Filter = "Fișiere de stiluri (*.json)|*.json|Toate fișierele (*.*)|*.*"
                dlg.InitialDirectory = ThemeOverrideStore.OverridesFolder
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

                Dim loaded As ThemeOverrideSet = ThemeOverrideStore.LoadFile(dlg.FileName)
                If loaded Is Nothing Then
                    SetStatus("Fișierul nu a putut fi citit.")
                    Return
                End If

                ' Avertisment, nu refuz: un set autorit pe altă suprafață poate fi util aici (căile
                ' care nu se potrivesc sunt sărite oricum de aplicator).
                If Not String.Equals(loaded.Scope, _scope.ScopeName, StringComparison.OrdinalIgnoreCase) Then
                    If MessageBox.Show(Me,
                            $"Fișierul a fost autorit pentru «{loaded.Scope}», iar suprafața curentă e «{_scope.ScopeName}». Îl aplic oricum?",
                            "Suprafață diferită", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                End If

                Dim applied As Integer = ThemeOverrideApplier.Apply(_scope.Root, loaded)
                _sets(_scope.ScopeName) = loaded
                BuildTree()
                SetStatus($"Aplicate {applied} din {loaded.Entries.Count} intrări.")
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.btnLoad_Click", ex)
            ShowError("Încărcarea a eșuat.", ex)
        End Try
    End Sub

    ' ---------------- temă / mesaje ----------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p = ThemeManager.Current.Palette
            lblStatus.ForeColor = p.TextDimColor
            grid.BackColor = p.SurfaceColor
            grid.ViewBackColor = p.InputBackColor
            grid.ViewForeColor = p.InputTextColor
            grid.LineColor = p.BorderColor
            grid.CategoryForeColor = p.TextColor
            grid.HelpBackColor = p.SurfaceColor
            grid.HelpForeColor = p.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeEditorForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub SetStatus(text As String)
        lblStatus.Text = If(text, String.Empty)
    End Sub

    Private Sub ShowError(title As String, ex As Exception)
        SetStatus(title)
        MessageBox.Show(Me, $"{title}{Environment.NewLine}{ex.Message}", "Editor de stiluri",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Class
