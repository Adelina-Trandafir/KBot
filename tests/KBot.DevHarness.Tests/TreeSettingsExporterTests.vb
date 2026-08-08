Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.DevHarness
Imports Xunit

' Exportul de setări al playground-ului (felia 0027): controlul probat pe ecran → linii de
' designer. Ce se probează aici e exact ce poate strica un fișier .Designer.vb: forma
' literalilor și, mai ales, TĂCEREA pe proprietățile pe care le stăpânește tema.
Public Class TreeSettingsExporterTests

    <Fact>
    Sub Arbore_proaspat_nu_exporta_nicio_proprietate()
        Using tree As New AdvancedTreeControl()
            Dim text As String = TreeSettingsExporter.Build(tree, "Classic")
            Assert.DoesNotContain("Me.{TREE}.", text)
        End Using
    End Sub

    <Fact>
    Sub Proprietatile_schimbate_ajung_in_export()
        Using tree As New AdvancedTreeControl()
            tree.HeaderVisible = True
            tree.HeaderHeight = 40
            tree.HeaderCaption = "Arbore «probă»"
            tree.ItemHeight = 26

            Dim text As String = TreeSettingsExporter.Build(tree, "Modern")

            Assert.Contains("Me.{TREE}.HeaderVisible = True", text)
            Assert.Contains("Me.{TREE}.HeaderHeight = 40", text)
            Assert.Contains("Me.{TREE}.ItemHeight = 26", text)
            ' Ghilimelele din text se dublează, «» rămân ce sunt.
            Assert.Contains("Me.{TREE}.HeaderCaption = ""Arbore «probă»""", text)
        End Using
    End Sub

    ' Miezul feliei 0027: o culoare lăsată «auto» se rezolvă din temă la CITIRE, dar scrisă în
    ' designer ar îngheța acolo. ShouldSerialize* o ține afară; una aleasă explicit intră.
    <Fact>
    Sub Culorile_din_tema_nu_se_exporta_dar_cele_alese_da()
        Using tree As New AdvancedTreeControl()
            Assert.NotEqual(Color.Empty, tree.HeaderBackColor)      ' getterul dă culoarea rezolvată
            Assert.DoesNotContain("HeaderBackColor", TreeSettingsExporter.Build(tree, "Classic"))

            tree.HeaderBackColor = Color.Goldenrod
            Assert.Contains("Me.{TREE}.HeaderBackColor = System.Drawing.Color.Goldenrod",
                            TreeSettingsExporter.Build(tree, "Classic"))
        End Using
    End Sub

    ' BackColor/ForeColor sunt ale temei (ApplyTheme le rescrie) — nu pleacă niciodată în export.
    <Fact>
    Sub BackColor_si_ForeColor_raman_afara()
        Using tree As New AdvancedTreeControl()
            tree.BackColor = Color.Black
            tree.ForeColor = Color.White
            Dim text As String = TreeSettingsExporter.Build(tree, "Dark")
            Assert.DoesNotContain("Me.{TREE}.BackColor", text)
            Assert.DoesNotContain("Me.{TREE}.ForeColor", text)
        End Using
    End Sub

    <Fact>
    Sub Enum_font_si_padding_se_scriu_ca_in_designer()
        Using tree As New AdvancedTreeControl()
            tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientVertical
            tree.HeaderTextAlign = ContentAlignment.MiddleCenter
            tree.HeaderFont = New Font("Segoe UI", 10.0F, FontStyle.Bold Or FontStyle.Italic)
            tree.SearchClearButtonPadding = New Padding(4)

            Dim text As String = TreeSettingsExporter.Build(tree, "Classic")

            Assert.Contains("Me.{TREE}.HeaderBackStyle = KBot.Controls.AdvancedTreeControl.En_HeaderBackStyle.GradientVertical", text)
            Assert.Contains("Me.{TREE}.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter", text)
            Assert.Contains("Me.{TREE}.HeaderFont = New System.Drawing.Font(""Segoe UI"", 10.0!, " &
                            "CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), " &
                            "System.Drawing.GraphicsUnit.Point, CType(1, Byte))", text)
            Assert.Contains("Me.{TREE}.SearchClearButtonPadding = New System.Windows.Forms.Padding(4)", text)
        End Using
    End Sub

    ' O imagine nu are literal de designer: iese ca notă, nu ca linie de cod.
    <Fact>
    Sub Imaginile_ies_ca_nota_nu_ca_linie()
        Using tree As New AdvancedTreeControl()
            Using bmp As New Bitmap(16, 16)
                tree.HeaderSearchIcon = bmp
                Dim text As String = TreeSettingsExporter.Build(tree, "Classic")
                Assert.DoesNotContain("Me.{TREE}.HeaderSearchIcon =", text)
                Assert.Contains("' HeaderSearchIcon = imagine 16×16", text)
                Assert.Contains("HeaderSearchIconKey", text)
            End Using
        End Using
    End Sub

    <Fact>
    Sub Nodurile_de_designer_se_exporta_cu_membrii_nevizi()
        Using tree As New AdvancedTreeControl()
            tree.Nodes.Add(New TreeNodeDefinition("G1", "Grup 1") With {
                .ImageKey = "grup", .Expanded = True})
            tree.Nodes.Add(New TreeNodeDefinition("G1F1", "Frunza 1") With {
                .ParentKey = "G1", .Tag = "cod-1"})

            Dim text As String = TreeSettingsExporter.Build(tree, "Classic")

            Assert.Contains("Me.{TREE}.Nodes.Add(New KBot.Controls.TreeNodeDefinition() With " &
                            "{.Key = ""G1"", .Caption = ""Grup 1"", .ImageKey = ""grup"", .Expanded = True})", text)
            Assert.Contains("Me.{TREE}.Nodes.Add(New KBot.Controls.TreeNodeDefinition() With " &
                            "{.Key = ""G1F1"", .Caption = ""Frunza 1"", .ParentKey = ""G1"", .Tag = ""cod-1""})", text)
            ' Ce a rămas pe implicit nu se scrie.
            Assert.DoesNotContain(".HasCheckBox", text)
            Assert.DoesNotContain(".LazyNode", text)
        End Using
    End Sub

    <Fact>
    Sub Antetul_spune_tema_si_regula_de_aplicare()
        Using tree As New AdvancedTreeControl()
            Dim text As String = TreeSettingsExporter.Build(tree, "Modern")
            Assert.Contains("temă activă la export: Modern", text)
            Assert.Contains("CUM SE APLICĂ", text)
            Assert.Contains(TreeSettingsExporter.Placeholder, text)
        End Using
    End Sub

End Class
