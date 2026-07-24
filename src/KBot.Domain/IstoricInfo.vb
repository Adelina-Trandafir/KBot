' POCO-urile vederii Istoric (felia 0022) — echivalentul lui frmFX_ISTORIC.
'
' Sursa: FX_Istoric, printr-un cititor brut (GET /api/forexe/istoric). Serverul NU pre-formeaza
' nimic: intoarce un rand per inregistrare FX_Istoric + ierarhia de clasificatii pentru meniul
' de filtrare, intr-un SINGUR drum dus-intors. Clientul (IstoricView) modeleaza grila, cele trei
' meniuri (Clasificatii / TipRand / DataFX) si filtrarea LOCAL, fara alte cereri.
' Modele pure (fara logica de I/O) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' Un rand FX_Istoric al angajamentului. Coloanele de bani vin deja 0-ate de server.
''' <see cref="DataFx"/> pastreaza componenta de timp (§2.3) — randurile de Istoric sunt
''' evenimente cu ora; grila afiseaza dd.MM.yyyy, dar filtrul pe zi are nevoie de data intreaga.
''' <see cref="IdClsf"/> este id-ul ACCESS (= Clasificatii.IdClsfAcc), cheia meniului de
''' clasificatii — INVERS fata de DDF, unde IdClsf tinea PK-ul nomenclatorului (§2.5).
''' </summary>
Public NotInheritable Class IstoricRand
    ''' <summary>Cheia primara FX_Istoric — purtata de evenimentul <c>RandSchimbat</c>.</summary>
    Public Property Id As Integer
    ''' <summary>Momentul evenimentului (datetime, cu ora). Baza filtrului pe lună/zi.</summary>
    Public Property DataFx As Date?
    ''' <summary>Clasificatia denormalizata (coloana TEXT FX_Istoric.Clsf), citita direct.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Id-ul de clasificatie ACCESS — cheia segmentului de filtrare pe clasificatie.</summary>
    Public Property IdClsf As Integer
    ''' <summary>Tipul rândului (Rez_Initiala, Rez_Definitiva+, PLATA_PLATA, Receptie…) —
    ''' segmentul de filtrare TipRand, grupat pe prefixele Rez_ / Plata_.</summary>
    Public Property TipRand As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property CodAI As String = String.Empty
    ''' <summary>Descrierea evenimentului — panoul de detaliu (stânga).</summary>
    Public Property Descriere As String = String.Empty
    ''' <summary>Observațiile evenimentului — panoul de detaliu (dreapta).</summary>
    Public Property Observatii As String = String.Empty
    Public Property ValRezervareI As Double
    Public Property ValRezervareD As Double
    Public Property ValRezervareAnt As Double
    ''' <summary>Diferența de rezervare — coloană totalizată (Sum).</summary>
    Public Property ValRezervareDif As Double
    Public Property ValAngLeg As Double
    ''' <summary>Recepția — coloană totalizată (Sum).</summary>
    Public Property ValReceptie As Double
    ''' <summary>Plata — coloană totalizată (Sum).</summary>
    Public Property ValPlata As Double
    Public Property IdTrezor As String = String.Empty
    Public Property Doc As String = String.Empty
    ''' <summary>Revizia (IDREV) — poate lipsi (multe randuri de istoric nu au revizie).</summary>
    Public Property Idrev As Integer?
End Class

''' <summary>
''' O intrare din ierarhia de filtrare pe clasificatie (§2.2), asamblata pe server din
''' <c>Clasificatii</c> + nomenclatoarele <c>AVACONT_COMUN</c>, deduplicata pe IdClsfAcc.
''' <see cref="IdClsf"/> = id-ul ACCESS (Clasificatii.IdClsfAcc), care se potriveste cu
''' <see cref="IstoricRand.IdClsf"/>. Captiunile celor trei niveluri (Subcapitol / Articol /
''' Alineat) construiesc meniul ierarhic.
''' </summary>
Public NotInheritable Class IstoricClasificatie
    ''' <summary>Id-ul ACCESS al clasificatiei — cheia de filtrare a segmentului Clsf.</summary>
    Public Property IdClsf As Integer
    ''' <summary>Codul complet al clasificatiei (Capitol.Subcapitol.Articol.Alineat).</summary>
    Public Property Clsf As String = String.Empty
    Public Property Capitol As String = String.Empty
    Public Property Subcapitol As String = String.Empty
    Public Property Articol As String = String.Empty
    Public Property Alineat As String = String.Empty
    ''' <summary>Captionul nivelului Subcapitol al meniului.</summary>
    Public Property DenSubcapitol As String = String.Empty
    ''' <summary>Captionul nivelului Articol al meniului.</summary>
    Public Property DenArticol As String = String.Empty
    ''' <summary>Captionul nivelului Alineat (frunza) al meniului.</summary>
    Public Property DenAlineat As String = String.Empty
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/istoric: randurile FX_Istoric + ierarhia de
''' clasificatii pentru meniul de filtrare. Ambele liste pot fi goale — un angajament fara
''' istoric e legitim (raspuns 200, nu 404). <see cref="Cod"/> = angajamentul cerut.
''' </summary>
Public NotInheritable Class IstoricInfo
    Public Property Cod As String = String.Empty
    Public Property Randuri As New List(Of IstoricRand)()
    Public Property Clasificatii As New List(Of IstoricClasificatie)()
End Class
