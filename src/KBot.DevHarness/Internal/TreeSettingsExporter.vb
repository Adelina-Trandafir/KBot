Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Traduce starea RUNTIME a unui <see cref="AdvancedTreeControl"/> în linii de designer VB.NET,
''' gata de pus în <c>InitializeComponent()</c> pe orice formular. Perechea de la capătul celălalt
''' al playground-ului: acolo se probează combinația pe ecran, aici se scoate din ea exact ce
''' trebuie scris în <c>.Designer.vb</c>.
'''
''' DE CE UN FORMAT DE COD, nu JSON/INI: ținta nu e o altă rulare a bancului, ci un fișier de
''' designer. O linie <c>Me.{TREE}.HeaderHeight = 26</c> se aplică fără traducere și se citește la
''' fel de ușor de om și de mașină. Numele arborelui din formularul-țintă nu e cunoscut aici, deci
''' se scrie <see cref="Placeholder"/> și se înlocuiește la aplicare.
'''
''' CE NU SE EXPORTĂ, INTENȚIONAT:
'''   * proprietățile pe care <c>ShouldSerialize*</c> le declară «neatinse» — adică toate culorile
'''     lăsate <c>Color.Empty</c> și fonturile nesetate. Scrise în designer, valoarea REZOLVATĂ din
'''     tema activă la export ar îngheța acolo pentru totdeauna, exact capcana din CLAUDE.md care a
'''     hardcodat paleta luminoasă în cinci fișiere de designer înainte de felia 0027;
'''   * <c>BackColor</c>/<c>ForeColor</c>: le dă tema prin <c>ApplyTheme</c> cât timp nu-s fixate în
'''     designer, iar la rulare sunt MEREU diferite de implicitul WinForms — exportate, ar fixa
'''     tema de la export pe formularul-țintă;
'''   * layout-ul (Dock/Location/Size/Anchor/Name): ține de formularul gazdă, nu de arbore.
''' </summary>
Public NotInheritable Class TreeSettingsExporter

    ''' <summary>Locul ținut pentru numele arborelui din formularul-țintă.</summary>
    Public Const Placeholder As String = "{TREE}"

    ' Proprietăți cu categorie K-BOT tratate separat sau deliberat omise (vezi rezumatul clasei).
    Private Shared ReadOnly _omise As String() = {"BackColor", "ForeColor", "Nodes", "NodeImages"}

    Private Shared ReadOnly _inv As CultureInfo = CultureInfo.InvariantCulture

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Construiește textul exportului. <paramref name="numeTema"/> intră doar în antet, ca o
    ''' urmă a contextului în care a fost aleasă combinația.
    ''' </summary>
    Public Shared Function Build(tree As AdvancedTreeControl, numeTema As String) As String
        Try
            If tree Is Nothing Then Throw New ArgumentNullException(NameOf(tree))

            Dim sb As New StringBuilder()
            ScrieAntet(sb, numeTema)

            ' Etalonul: un arbore proaspăt. Metadatele (DefaultValue / ShouldSerialize*) prind
            ' aproape tot, dar proprietățile fără nici una — *IconSize, *GradientEndColor — s-ar
            ' scrie mereu. Comparate cu etalonul, tac atâta timp cât nimeni nu le-a mișcat.
            Dim etalon As AdvancedTreeControl = Nothing
            Try
                etalon = New AdvancedTreeControl()
            Catch ex As Exception
                GlobalErrorLog.Write("TreeSettingsExporter.Build/etalon", ex)
                etalon = Nothing        ' fără etalon rămânem doar pe metadate
            End Try

            Try
                Dim peCategorii As New SortedDictionary(Of String, List(Of String))(StringComparer.Ordinal)
                Dim note As New List(Of String)()

                For Each pd As PropertyDescriptor In TypeDescriptor.GetProperties(tree)
                    If Not EsteExportabila(pd) Then Continue For
                    If Not pd.ShouldSerializeValue(tree) Then Continue For

                    Dim valoare As Object = pd.GetValue(tree)
                    If etalon IsNot Nothing AndAlso Egale(valoare, pd.GetValue(etalon)) Then Continue For

                    ' O imagine nu are literal: e o resursă a formularului-țintă.
                    Dim img As Image = TryCast(valoare, Image)
                    If img IsNot Nothing Then
                        note.Add(NotaImagine(pd.Name, img))
                        Continue For
                    End If

                    Dim lista As List(Of String) = Nothing
                    If Not peCategorii.TryGetValue(pd.Category, lista) Then
                        lista = New List(Of String)()
                        peCategorii.Add(pd.Category, lista)
                    End If
                    lista.Add($"Me.{Placeholder}.{pd.Name} = {Literal(valoare)}")
                Next

                For Each pereche In peCategorii
                    pereche.Value.Sort(StringComparer.Ordinal)
                    sb.AppendLine()
                    sb.AppendLine($"'—— {pereche.Key} ——")
                    For Each linie In pereche.Value
                        sb.AppendLine(linie)
                    Next
                Next

                ScrieNoduri(sb, tree)
                ScrieNote(sb, tree, note)
            Finally
                etalon?.Dispose()
            End Try

            sb.AppendLine()
            sb.AppendLine("' ── sfârșit export ──")
            Return sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("TreeSettingsExporter.Build", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Scrie exportul în <c>&lt;AppDir&gt;\Exports\tree-designer-&lt;marcaj&gt;.vb.txt</c> și
    ''' întoarce calea completă. UTF-8 CU BOM, ca diacriticele să supraviețuiască în Notepad.
    ''' </summary>
    Public Shared Function Save(continut As String) As String
        Try
            Dim dosar As String = Path.Combine(AppContext.BaseDirectory, "Exports")
            Directory.CreateDirectory(dosar)
            Dim cale As String = Path.Combine(
                dosar, "tree-designer-" & DateTime.Now.ToString("yyyyMMdd-HHmmss", _inv) & ".vb.txt")
            File.WriteAllText(cale, continut, New UTF8Encoding(True))
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("TreeSettingsExporter.Save", ex)
            Throw
        End Try
    End Function

    ' ── Selecția proprietăților ──────────────────────────────────────────────────
    Private Shared Function EsteExportabila(pd As PropertyDescriptor) As Boolean
        If pd.IsReadOnly Then Return False
        If pd.SerializationVisibility = DesignerSerializationVisibility.Hidden Then Return False
        If Array.IndexOf(_omise, pd.Name) >= 0 Then Return False
        Dim cat As String = pd.Category
        Return cat IsNot Nothing AndAlso cat.StartsWith("K-BOT", StringComparison.Ordinal)
    End Function

    Private Shared Function Egale(a As Object, b As Object) As Boolean
        If a Is Nothing Then Return b Is Nothing
        Return a.Equals(b)
    End Function

    ' ── Secțiuni de text ─────────────────────────────────────────────────────────
    Private Shared Sub ScrieAntet(sb As StringBuilder, numeTema As String)
        sb.AppendLine("' ══════════════════════════════════════════════════════════════════════════")
        sb.AppendLine("' K-BOT — export setări AdvancedTreeControl (TreePlaygroundForm, KBot.DevHarness)")
        sb.AppendLine("' Generat: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", _inv) &
                      " • temă activă la export: " & If(numeTema, "necunoscută"))
        sb.AppendLine("' ══════════════════════════════════════════════════════════════════════════")
        sb.AppendLine("' CUM SE APLICĂ: înlocuiește " & Placeholder & " cu numele arborelui din")
        sb.AppendLine("' formularul-țintă și pune liniile în InitializeComponent(), în blocul acelui")
        sb.AppendLine("' control. Liniile care încep cu apostrof sunt NOTE — cer o acțiune manuală.")
        sb.AppendLine("'")
        sb.AppendLine("' CE NU E AICI, INTENȚIONAT:")
        sb.AppendLine("'   • layout-ul (Dock/Location/Size/Anchor/Name) — ține de formularul gazdă;")
        sb.AppendLine("'   • culorile lăsate «auto» (Color.Empty) și fonturile nesetate — ShouldSerialize*")
        sb.AppendLine("'     le sare, ca tema să rămână stăpână pe ele (regula din CLAUDE.md);")
        sb.AppendLine("'   • BackColor/ForeColor — le dă tema prin ApplyTheme; se fixează doar în designer.")
        sb.AppendLine("' Ce NU apare mai jos a rămas pe implicit: nu-l scrie.")
    End Sub

    Private Shared Sub ScrieNoduri(sb As StringBuilder, tree As AdvancedTreeControl)
        sb.AppendLine()
        If tree.Nodes.Count = 0 Then
            sb.AppendLine("' Nodes: gol — arborele s-a umplut la rulare (AddItem / XML FOREXE), nu din")
            sb.AppendLine("' designer. Nu se exportă noduri.")
            Return
        End If

        sb.AppendLine($"'—— Noduri de designer ({tree.Nodes.Count} definiții) ——")
        sb.AppendLine("' Cheile de imagini se rezolvă din ImageList-ul legat la NodeImages (vezi notele).")
        For Each def As TreeNodeDefinition In tree.Nodes
            sb.AppendLine($"Me.{Placeholder}.Nodes.Add(New KBot.Controls.TreeNodeDefinition() With {{{MembriiNodului(def)}}})")
        Next
    End Sub

    Private Shared Function MembriiNodului(def As TreeNodeDefinition) As String
        Dim m As New List(Of String) From {
            ".Key = " & Sir(def.Key),
            ".Caption = " & Sir(def.Caption)
        }
        AdaugaSirNevid(m, "ParentKey", def.ParentKey)
        AdaugaSirNevid(m, "ImageKey", def.ImageKey)
        AdaugaSirNevid(m, "OpenImageKey", def.OpenImageKey)
        AdaugaSirNevid(m, "RightImageKey", def.RightImageKey)
        AdaugaSirNevid(m, "Tag", def.Tag)
        AdaugaSirNevid(m, "Tooltip", def.Tooltip)
        If def.Expanded Then m.Add(".Expanded = True")
        If def.HasCheckBox Then m.Add(".HasCheckBox = True")
        If def.LazyNode Then m.Add(".LazyNode = True")
        Return String.Join(", ", m)
    End Function

    Private Shared Sub AdaugaSirNevid(m As List(Of String), nume As String, valoare As String)
        If String.IsNullOrEmpty(valoare) Then Return
        m.Add("." & nume & " = " & Sir(valoare))
    End Sub

    Private Shared Sub ScrieNote(sb As StringBuilder, tree As AdvancedTreeControl, note As List(Of String))
        Dim lista As ImageList = tree.NodeImages
        If lista IsNot Nothing AndAlso lista.Images.Count > 0 Then
            Dim chei As New List(Of String)()
            For Each cheie As String In lista.Images.Keys
                chei.Add(cheie)
            Next
            note.Insert(0, $"' NodeImages: ImageList cu {lista.Images.Count} imagini " &
                           $"({lista.ImageSize.Width}×{lista.ImageSize.Height}), chei: {String.Join(", ", chei)}." &
                           $" Pune un ImageList pe formular cu ACELEAȘI chei și leagă-l:" &
                           $" Me.{Placeholder}.NodeImages = Me.<numeleListei>")
        End If

        If note.Count = 0 Then Return
        sb.AppendLine()
        sb.AppendLine("'—— De făcut manual (imagini) ——")
        For Each n In note
            sb.AppendLine(n)
        Next
    End Sub

    Private Shared Function NotaImagine(nume As String, img As Image) As String
        Dim sfat As String
        If nume.StartsWith("Header", StringComparison.Ordinal) Then
            sfat = $"alege-o în designer (selectorul de imagini o scrie în .resx) sau setează " &
                   $"Me.{Placeholder}.{nume}Key = ""<cheie din NodeImages>"""
        Else
            sfat = "alege-o în designer (selectorul de imagini o scrie în .resx)"
        End If
        Return $"' {nume} = imagine {img.Width}×{img.Height} — nu are literal de designer; {sfat}."
    End Function

    ' ── Literali VB ──────────────────────────────────────────────────────────────
    ''' <summary>Valoarea ca literal VB.NET, în forma pe care o scrie designerul.</summary>
    Friend Shared Function Literal(v As Object) As String
        If v Is Nothing Then Return "Nothing"
        If TypeOf v Is Boolean Then Return If(CBool(v), "True", "False")
        If TypeOf v Is String Then Return Sir(CStr(v))
        If v.GetType().IsEnum Then Return EnumLit(v)
        If TypeOf v Is Color Then Return CuloareLit(DirectCast(v, Color))
        If TypeOf v Is Font Then Return FontLit(DirectCast(v, Font))
        If TypeOf v Is Size Then
            Dim s As Size = DirectCast(v, Size)
            Return $"New System.Drawing.Size({s.Width}, {s.Height})"
        End If
        If TypeOf v Is Point Then
            Dim p As Point = DirectCast(v, Point)
            Return $"New System.Drawing.Point({p.X}, {p.Y})"
        End If
        If TypeOf v Is Padding Then Return PaddingLit(DirectCast(v, Padding))
        If TypeOf v Is Single Then Return CSng(v).ToString("0.0###", _inv) & "!"
        If TypeOf v Is Double Then Return CDbl(v).ToString("0.0###", _inv) & "R"
        If TypeOf v Is Integer Then Return CInt(v).ToString(_inv)
        Return Convert.ToString(v, _inv)
    End Function

    Private Shared Function Sir(s As String) As String
        If s Is Nothing Then Return """"""
        Dim t As String = s.Replace("""", """""")
        t = t.Replace(vbCrLf, """ & vbCrLf & """)
        t = t.Replace(vbLf, """ & vbLf & """)
        t = t.Replace(vbCr, """ & vbCr & """)
        Return """" & t & """"
    End Function

    Private Shared Function EnumLit(v As Object) As String
        Dim numeTip As String = v.GetType().FullName.Replace("+"c, "."c)
        Dim text As String = v.ToString()
        ' O combinație de flag-uri se scrie „A, B" — în VB e „A Or B".
        If text.Contains(", ") Then
            Dim parti As String() = text.Split(New String() {", "}, StringSplitOptions.None)
            For i As Integer = 0 To parti.Length - 1
                parti(i) = numeTip & "." & parti(i)
            Next
            Return "CType((" & String.Join(" Or ", parti) & "), " & numeTip & ")"
        End If
        ' O valoare fără nume (numerică) nu se poate scrie ca membru.
        If text.Length > 0 AndAlso (Char.IsDigit(text(0)) OrElse text(0) = "-"c) Then
            Return "CType(" & text & ", " & numeTip & ")"
        End If
        Return numeTip & "." & text
    End Function

    Private Shared Function CuloareLit(c As Color) As String
        If c.IsEmpty Then Return "System.Drawing.Color.Empty"
        If c.IsSystemColor Then Return "System.Drawing.SystemColors." & c.Name
        If c.IsNamedColor Then Return "System.Drawing.Color." & c.Name
        Dim comp As String = $"CType({c.R}, Byte), CType({c.G}, Byte), CType({c.B}, Byte)"
        If c.A = 255 Then Return $"System.Drawing.Color.FromArgb({comp})"
        Return $"System.Drawing.Color.FromArgb(CType({c.A}, Byte), {comp})"
    End Function

    Private Shared Function FontLit(f As Font) As String
        Return $"New System.Drawing.Font(""{f.Name}"", {f.Size.ToString("0.0###", _inv)}!, " &
               $"{StilFont(f.Style)}, System.Drawing.GraphicsUnit.{f.Unit}, CType({f.GdiCharSet}, Byte))"
    End Function

    Private Shared Function StilFont(stil As FontStyle) As String
        If stil = FontStyle.Regular Then Return "System.Drawing.FontStyle.Regular"
        Dim parti As New List(Of String)()
        For Each f As FontStyle In New FontStyle() {FontStyle.Bold, FontStyle.Italic,
                                                    FontStyle.Underline, FontStyle.Strikeout}
            If (stil And f) = f Then parti.Add("System.Drawing.FontStyle." & f.ToString())
        Next
        If parti.Count = 1 Then Return parti(0)
        Return "CType((" & String.Join(" Or ", parti) & "), System.Drawing.FontStyle)"
    End Function

    Private Shared Function PaddingLit(p As Padding) As String
        If p.Left = p.Top AndAlso p.Top = p.Right AndAlso p.Right = p.Bottom Then
            Return $"New System.Windows.Forms.Padding({p.Left})"
        End If
        Return $"New System.Windows.Forms.Padding({p.Left}, {p.Top}, {p.Right}, {p.Bottom})"
    End Function

End Class
