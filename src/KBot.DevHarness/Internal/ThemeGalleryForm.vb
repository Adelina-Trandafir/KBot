Imports System
Imports System.Windows.Forms
Imports KBot.Theming

' Proba vizuală a motorului de teme: un exemplar din fiecare tip de control tematizat,
' plus butoanele Classic/Dark/Modern (comutare LIVE) și Pass/Fail. Fiind KBotThemedForm,
' se re-tematizează singur la fiecare SetScheme. Evenimentele/comutările se loghează prin
' delegatul primit din test (context.Log).
Public NotInheritable Class ThemeGalleryForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme

    ' preset: schema cu care se deschide galeria (Nothing => schema activă curentă).
    ' Folosit de probele per-temă (Classic/Dark/Modern) din harness.
    Public Sub New(log As Action(Of String), Optional preset As ThemeScheme = Nothing)
        _log = log
        _originalScheme = ThemeManager.Current   ' de restaurat la închidere (proba nu trebuie să repersiste alegerea operatorului)
        If preset IsNot Nothing AndAlso Not String.Equals(preset.Name, ThemeManager.Current.Name, StringComparison.OrdinalIgnoreCase) Then
            _log("deschide galeria pe schema → " & preset.Name)
            ThemeManager.SetScheme(preset)       ' OnLoad-ul bazei va aplica schema curentă (= preset)
        End If
        InitializeComponent()
        SeedSampleData()
    End Sub

    ' Restaurează schema activă dinainte de probă, ca galeria să nu schimbe tema aplicației.
    Private Sub OnClosedRestore(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
            ThemeManager.SetScheme(_originalScheme)
        End If
    End Sub

    ' Date hardcodate în controalele-mostră (controalele sunt în .Designer.vb).
    Private Sub SeedSampleData()
        cboSample.Items.AddRange(New Object() {"Alfa", "Beta", "Gamma"})
        cboSample.SelectedIndex = 0
        lstSample.Items.AddRange(New Object() {"Rând 1", "Rând 2", "Rând 3"})
        clbSample.Items.AddRange(New Object() {"Opțiune A", "Opțiune B", "Opțiune C"})
        clbSample.SetItemChecked(0, True)
        Dim root = tvSample.Nodes.Add("Rădăcină")
        root.Nodes.Add("Copil 1")
        root.Nodes.Add("Copil 2")
        root.Expand()
        chkSample.Checked = True
        optSample.Checked = True
    End Sub

    ' Re-citește starea vizibilă după fiecare comutare de schemă (base o cheamă după Apply).
    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        If lblActive IsNot Nothing Then
            lblActive.Text = "activ: " & ThemeManager.Current.Name
        End If
    End Sub

    Private Sub OnClassic(sender As Object, e As EventArgs) Handles btnClassic.Click
        Switch(BuiltInSchemes.Classic())
    End Sub

    Private Sub OnDark(sender As Object, e As EventArgs) Handles btnDark.Click
        Switch(BuiltInSchemes.Dark())
    End Sub

    Private Sub OnModern(sender As Object, e As EventArgs) Handles btnModern.Click
        Switch(BuiltInSchemes.Modern())
    End Sub

    Private Sub Switch(scheme As ThemeScheme)
        _log("comută schema → " & scheme.Name)
        ThemeManager.SetScheme(scheme)
    End Sub

End Class
