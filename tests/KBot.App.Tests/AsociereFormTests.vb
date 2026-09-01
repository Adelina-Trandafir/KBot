Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.App

''' <summary>
''' EDITORUL DE LEGĂTURI RECEPȚIE ▸ INSTANTANEU (felia 0048-04), fără ecran.
'''
''' Ce se ține fix aici e chiar ce nu poate atinge niciun test de server: proiecția locală
''' (tabloul de pe ecran între trageri), care comenzi pleacă de fapt pe fir, și cele DOUĂ
''' feluri de refuz — <b>legătura înghețată</b> (nu se poate nici măcar porni tragerea) și
''' <b>vetoul de plasare</b> (se poate trage, dar nu ACOLO).
'''
''' <para>Regula pe care o apără cele mai multe: se trimit DOAR comenzile care diferă de ce
''' s-a citit. O legătură neatinsă nu se rescrie, fiindcă aici tăcerea înseamnă «las-o cum e» —
''' spre deosebire de ingestie, unde ar fi fost o alegere ascunsă și de-aia acolo acoperirea e
''' obligatorie.</para>
'''
''' <para>Totul rulează pe un fir STA dedicat: crearea unui Form instalează un
''' <c>WindowsFormsSynchronizationContext</c>, deci continuările au nevoie de o buclă pompată.
''' Același tipar ca la <c>ReceptiiViewTests</c>.</para>
''' </summary>
Public Class AsociereFormTests

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw New Xunit.Sdk.XunitException(failure.ToString())
    End Sub

    ' ── Datele de probă ──────────────────────────────────────────────────────

    Private Shared Function Rec(idrr As Integer, dataR As Date, suma As Double,
                                ParamArray indicatori As String()) As ReceptiePropusa
        Dim r As New ReceptiePropusa() With {.Idrr = idrr, .DataR = dataR, .SumaAntet = suma,
                                             .Descriere = "Plata factura"}
        For Each c As String In indicatori
            r.Rhr.Add(New LinieReceptie() With {.CodIndicator = c, .CodAi = "A-" & c,
                                                .CreditBugetar = 10502.19, .Valoare = suma})
        Next
        Return r
    End Function

    Private Shared Function Inst(idrh As Integer, dataH As Date, total As Double,
                                 Optional idrr As Integer = 0,
                                 Optional blocat As Boolean = False,
                                 Optional indicatori As String() = Nothing) As InstantaneuLegat
        Dim i As New InstantaneuLegat() With {.Idrh = idrh, .Idrr = idrr, .DataH = dataH,
                                              .Total = total, .Descriere = "Plata factura",
                                              .Blocat = blocat}
        If blocat Then i.Motive.Add("Angajamentul are 3 plăți începând cu 28.02.2026.")
        For Each c As String In If(indicatori, New String() {"AAB"})
            i.Linii.Add(New LinieInstantaneu() With {.CodIndicator = c, .CodAi = "A-" & c, .Valoare = total})
        Next
        Return i
    End Function

    ' Tabloul standard: DOUĂ recepții, un instantaneu așezat pe fiecare, unul neașezat.
    ' Instantaneul recepției 1 e BLOCAT — el e subiectul testelor despre refuz.
    Private Shared Function StandardStare() As AsociereStare
        Dim s As New AsociereStare() With {.CodAngajament = "A100", .Amprenta = "amp1"}
        s.Receptii.Add(Rec(1, New Date(2026, 1, 1), 100.0, "AAB"))
        s.Receptii.Add(Rec(2, New Date(2026, 2, 1), 200.0, "AAB"))
        s.Instantanee.Add(Inst(11, New Date(2026, 1, 19, 10, 0, 0), 100.0, idrr:=1, blocat:=True))
        s.Instantanee.Add(Inst(21, New Date(2026, 2, 16, 10, 0, 0), 200.0, idrr:=2))
        s.Instantanee.Add(Inst(31, New Date(2026, 3, 10, 10, 0, 0), 200.0, idrr:=0))
        Return s
    End Function

    ' ── Ajutoare de reflexie ─────────────────────────────────────────────────

    Private Shared Function Formular(api As AsociereFakeApi) As AsociereForm
        Return New AsociereForm(api, "A100", Function(op) op(), Function(op) op())
    End Function

    ''' <summary>
    ''' Formularul își încarcă tabloul în <c>Shown</c>; testele nu îl arată, deci cheamă direct
    ''' încărcarea și pompează bucla până se așază continuările.
    ''' </summary>
    Private Shared Sub Incarca(f As AsociereForm)
        Dim m = f.GetType().GetMethod("ReincarcaAsync",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        Dim t As Task = CType(m.Invoke(f, Nothing), Task)
        For i As Integer = 0 To 200
            Application.DoEvents()
            If t.IsCompleted Then Exit For
            Thread.Sleep(1)
        Next
        Assert.True(t.IsCompleted, "Încărcarea nu s-a terminat.")
    End Sub

    Private Shared Function Comenzi(f As AsociereForm) As List(Of ComandaAsociere)
        Dim m = f.GetType().GetMethod("Comenzi",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        Return CType(m.Invoke(f, Nothing), List(Of ComandaAsociere))
    End Function

    ''' <summary>
    ''' Unul dintre cei doi arbori ai formularului, dupa numele din designer.
    '''
    ''' <para>Se cauta INTAI proprietatea, apoi campul cu underscore: un <c>Friend WithEvents</c>
    ''' din VB nu e un camp cu numele acela, ci o PROPRIETATE peste un camp ascuns numit
    ''' <c>_treeLant</c>. Un <c>GetField(nume)</c> simplu raspunde Nothing, si raspunde asa in
    ''' tacere.</para>
    ''' </summary>
    Private Shared Function Arbore(f As AsociereForm, nume As String) As AdvancedTreeControl
        Dim flags = Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance
        Dim prop = f.GetType().GetProperty(nume, flags)
        If prop IsNot Nothing Then Return CType(prop.GetValue(f), AdvancedTreeControl)
        Dim fld = f.GetType().GetField("_" & nume, flags)
        If fld Is Nothing Then fld = f.GetType().GetField(nume, flags)
        Assert.NotNull(fld)
        Return CType(fld.GetValue(f), AdvancedTreeControl)
    End Function

    Private Shared Function Cauta(items As List(Of AdvancedTreeControl.TreeItem), cheie As String) _
        As AdvancedTreeControl.TreeItem
        For Each it As AdvancedTreeControl.TreeItem In items
            If String.Equals(it.Key, cheie, StringComparison.Ordinal) Then Return it
            Dim hit = Cauta(it.Children, cheie)
            If hit IsNot Nothing Then Return hit
        Next
        Return Nothing
    End Function

    Private Shared Function Nod(tree As AdvancedTreeControl, cheie As String) As AdvancedTreeControl.TreeItem
        Return Cauta(tree.Items, cheie)
    End Function

    Private Shared Function IntreabaPornirea(f As AsociereForm, nod As AdvancedTreeControl.TreeItem) _
        As TreeDragStartEventArgs
        Dim e As New TreeDragStartEventArgs(nod)
        f.GetType().GetMethod("Tree_NodeDragStarting",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).
            Invoke(f, New Object() {Nothing, e})
        Return e
    End Function

    Private Shared Function IntreabaAruncarea(f As AsociereForm, metoda As String,
                                              sursa As AdvancedTreeControl.TreeItem,
                                              tinta As AdvancedTreeControl.TreeItem) As TreeDragOverEventArgs
        Dim e As New TreeDragOverEventArgs(sursa, tinta)
        f.GetType().GetMethod(metoda,
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).
            Invoke(f, New Object() {Nothing, e})
        Return e
    End Function

    Private Shared Sub Arunca(f As AsociereForm, metoda As String,
                              sursa As AdvancedTreeControl.TreeItem,
                              tinta As AdvancedTreeControl.TreeItem)
        f.GetType().GetMethod(metoda,
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).
            Invoke(f, New Object() {Nothing, New TreeDropEventArgs(sursa, tinta)})
    End Sub

    Private Shared Sub Meniu(f As AsociereForm, inst As InstantaneuLegat, cheie As String)
        f.GetType().GetMethod("AplicaComandaDeMeniu",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).
            Invoke(f, New Object() {inst, cheie})
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Proiecția
    ' ══════════════════════════════════════════════════════════════════════════

    <Fact>
    Public Sub Incarcarea_AsazaFiecareInstantaneuUndeIlAreServerul()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Assert.Equal(New String() {"A100"}, api.CoduriCerute.ToArray())

                       Dim lant = Arbore(f, "treeLant")
                       Assert.NotNull(Nod(lant, "H:11"))
                       Assert.NotNull(Nod(lant, "H:21"))
                       ' Cel neașezat NU e în arborele lanțurilor.
                       Assert.Null(Nod(lant, "H:31"))
                       Assert.NotNull(Nod(Arbore(f, "treeLibere"), "H:31"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub FaraNiciOSchimbare_NuPleacaNicioComanda()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Assert.Empty(Comenzi(f))
                   End Using
               End Sub)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Refuzul: legătura înghețată
    ' ══════════════════════════════════════════════════════════════════════════

    <Fact>
    Public Sub OLegaturaBlocata_RamaneVizibila_DarNuSePoateTrage()
        ' Chiar cerința operatorului: nodul EXISTĂ în arbore, doar tragerea lui e oprită.
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       ' `randul`, nu `nod`: VB e insensibil la litere mari/mici, deci o
                       ' variabila numita `nod` ar umbri functia `Nod()` chiar in propria ei
                       ' initializare.
                       Dim randul = Nod(Arbore(f, "treeLant"), "H:11")
                       Assert.NotNull(randul)
                       Assert.True(IntreabaPornirea(f, randul).Cancel)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub OLegaturaLibera_PoateFiTrasa()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Assert.False(IntreabaPornirea(f, Nod(Arbore(f, "treeLant"), "H:21")).Cancel)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RadacinaDeReceptie_NuSeTrage()
        ' D-J: rădăcinile de recepție nu sunt de mutat.
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Assert.True(IntreabaPornirea(f, Nod(Arbore(f, "treeLant"), "R:1")).Cancel)
                   End Using
               End Sub)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Refuzul: vetourile de plasare
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' F13 a fost RETRAS pe 31.08.2026 și e acum doar un semn — testul îl urmează.
    ''' </summary>
    ''' <remarks>
    ''' Rescris, nu șters: regula n-a dispărut, a coborât. <c>DataR</c> nu spune când a apărut
    ''' recepția — e un câmp pe care omul îl tastează pe site și îl poate schimba după aceea, iar
    ''' <c>FX_Receptii_R</c> nu are nicio coloană cu momentul creării (F29). Un refuz clădit pe
    ''' el poate opri o plasare corectă. Aruncarea trece; observația o poartă rândul.
    ''' </remarks>
    <Fact>
    Public Sub F13Retras_ORecepțieMaiNouaDecatInstantaneul_ETotusiPrimita()
        ' Recepția 2 e creată în februarie; instantaneul 41 e din ianuarie.
        RunSta(Sub()
                   Dim stare As AsociereStare = StandardStare()
                   stare.Instantanee.Add(Inst(41, New Date(2026, 1, 5, 8, 0, 0), 200.0))
                   Dim api As New AsociereFakeApi() With {.Stare = stare}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim e = IntreabaAruncarea(f, "TreeLant_NodeDragOver",
                                                 Nod(Arbore(f, "treeLibere"), "H:41"),
                                                 Nod(Arbore(f, "treeLant"), "R:2"))
                       Assert.True(e.Allow)
                       Assert.Equal(String.Empty, e.Motiv)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub F14_UnIndicatorPeCareRecepțiaNuIlAre_ERefuzat()
        RunSta(Sub()
                   Dim stare As AsociereStare = StandardStare()
                   stare.Instantanee.Add(Inst(51, New Date(2026, 3, 5, 8, 0, 0), 200.0,
                                              indicatori:=New String() {"AA9"}))
                   Dim api As New AsociereFakeApi() With {.Stare = stare}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim e = IntreabaAruncarea(f, "TreeLant_NodeDragOver",
                                                 Nod(Arbore(f, "treeLibere"), "H:51"),
                                                 Nod(Arbore(f, "treeLant"), "R:2"))
                       Assert.False(e.Allow)
                       Assert.Contains("AA9", e.Motiv)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub OPlasareValida_EPermisa()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim e = IntreabaAruncarea(f, "TreeLant_NodeDragOver",
                                                 Nod(Arbore(f, "treeLibere"), "H:31"),
                                                 Nod(Arbore(f, "treeLant"), "R:2"))
                       Assert.True(e.Allow)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AceeasiRecepție_ERefuzataCuUnMotivCareOSpune()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim e = IntreabaAruncarea(f, "TreeLant_NodeDragOver",
                                                 Nod(Arbore(f, "treeLant"), "H:21"),
                                                 Nod(Arbore(f, "treeLant"), "R:2"))
                       Assert.False(e.Allow)
                       Assert.Contains("deja pe această recepție", e.Motiv)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub UnInstantaneuDejaNeasezat_NuSeMaiPoateDesprinde()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim e = IntreabaAruncarea(f, "TreeLibere_NodeDragOver",
                                                 Nod(Arbore(f, "treeLibere"), "H:31"),
                                                 Nod(Arbore(f, "treeLibere"), "LIBERE"))
                       Assert.False(e.Allow)
                       Assert.Contains("deja neașezat", e.Motiv)
                   End Using
               End Sub)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Ce pleacă pe fir
    ' ══════════════════════════════════════════════════════════════════════════

    <Fact>
    Public Sub OAsezare_DaExactOComandaAsociat()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Arunca(f, "TreeLant_NodeDropped",
                              Nod(Arbore(f, "treeLibere"), "H:31"),
                              Nod(Arbore(f, "treeLant"), "R:2"))

                       Dim una = Assert.Single(Comenzi(f))
                       Assert.Equal(31, una.Idrh)
                       Assert.Equal(ActiuneAsociere.Asociat, una.Actiune)
                       Assert.Equal(2, una.Idrr)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ODesprindere_DaComandaDesprins_NuIgnorat()
        ' Cele două scriu ALTCEVA în bază (Sters = 0 față de Sters = 1), deci nu se confundă:
        ' «desprins» = încă nu știu unde; «ignorat» = nu consemnează nicio schimbare.
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Arunca(f, "TreeLibere_NodeDropped",
                              Nod(Arbore(f, "treeLant"), "H:21"),
                              Nod(Arbore(f, "treeLibere"), "LIBERE"))

                       Dim una = Assert.Single(Comenzi(f))
                       Assert.Equal(21, una.Idrh)
                       Assert.Equal(ActiuneAsociere.Desprins, una.Actiune)
                       Assert.Equal(0, una.Idrr)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MutareaInapoiUndeEra_NuMaiLasaNicioComanda()
        ' Proiecția locală se compară cu tabloul CITIT, nu cu pasul dinainte.
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Arunca(f, "TreeLibere_NodeDropped",
                              Nod(Arbore(f, "treeLant"), "H:21"),
                              Nod(Arbore(f, "treeLibere"), "LIBERE"))
                       Assert.Single(Comenzi(f))

                       Arunca(f, "TreeLant_NodeDropped",
                              Nod(Arbore(f, "treeLibere"), "H:21"),
                              Nod(Arbore(f, "treeLant"), "R:2"))
                       Assert.Empty(Comenzi(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MarcajulFaraSchimbare_DaComandaIgnorat()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim inst = CType(Nod(Arbore(f, "treeLibere"), "H:31").Tag, InstantaneuLegat)
                       Meniu(f, inst, "ignora")

                       Dim una = Assert.Single(Comenzi(f))
                       Assert.Equal(ActiuneAsociere.Ignorat, una.Actiune)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub OCheieDeMeniuNecunoscuta_NuSchimbaNimic()
        ' Metoda e o graniță de UI: loghează și înghite, deci nu aruncă — dar nu are voie să
        ' fi schimbat nimic. Fără implicit tăcut.
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Dim inst = CType(Nod(Arbore(f, "treeLibere"), "H:31").Tag, InstantaneuLegat)
                       Meniu(f, inst, "ceva_inventat")
                       Assert.Empty(Comenzi(f))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Salvarea_TrimiteAmprentaCitita_SiDoarComenzileSchimbate()
        RunSta(Sub()
                   Dim api As New AsociereFakeApi() With {.Stare = StandardStare()}
                   Using f As AsociereForm = Formular(api)
                       Incarca(f)
                       Arunca(f, "TreeLant_NodeDropped",
                              Nod(Arbore(f, "treeLibere"), "H:31"),
                              Nod(Arbore(f, "treeLant"), "R:2"))

                       Dim m = f.GetType().GetMethod("btnSalveaza_Click",
                           Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                       m.Invoke(f, New Object() {Nothing, EventArgs.Empty})
                       For i As Integer = 0 To 200
                           Application.DoEvents()
                           If api.Salvari.Count > 0 Then Exit For
                           Thread.Sleep(1)
                       Next

                       Dim trimise = Assert.Single(api.Salvari)
                       Assert.Equal("amp1", api.AmprentaPrimita)
                       Assert.Equal(31, Assert.Single(trimise).Idrh)
                       Assert.True(f.SAuSalvatModificari)
                   End Using
               End Sub)
    End Sub
End Class
