Option Strict On
Imports Microsoft.Win32
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Forțează, o singură dată pe sesiune, interfața CLASICĂ a Adobe (<c>bEnableAv2 = 0</c>) și pune
''' valoarea la loc la ieșirea din aplicație.
'''
''' ═══ ESTE OPRIT ȘI TREBUIE SĂ RĂMÂNĂ OPRIT PÂNĂ CÂND CINEVA DECIDE ALTFEL ═══
''' <see cref="ENFORCE_LEGACY_UI"/> este <c>False</c>. Cu el pe <c>False</c> clasa nu citește și nu
''' scrie nimic — <see cref="EnsureApplied"/> iese imediat.
'''
''' DE CE E OPRIT. Felia 0024-01 a decis explicit contrariul acestui mecanism: codul livrat NU scrie
''' preferințe Adobe, fiindcă <c>bEnableAv2</c> schimbă Adobe-ul operatorului pentru ORICE PDF ar
''' deschide, inclusiv în afara K-BOT, iar previzualizarea se descurcă fără: <see cref="AdobeUiDetector"/>
''' recunoaște generația din arborele de ferestre și <see cref="AdobeViewerProfiles"/> aplică profilul
''' potrivit. Mecanismul e scris, testat și lăsat inert ca decizia să fie o CONSTANTĂ de comutat, nu
''' un cod de scris sub presiune.
'''
''' CE FACE CÂND E PORNIT, exact:
'''  * o singură dată pe sesiune, la prima încorporare — niciodată per document;
'''  * dacă valoarea e DEJA 0, nu face absolut nimic: nicio scriere, niciun instantaneu;
'''  * altfel: instantaneu prin <see cref="RegistrySnapshotSet"/>, scrie 0, notează vechi → nou;
'''  * NU omoară niciun Adobe străin și NU pune nicio întrebare. Bancul are voie să omoare, aplicația
'''    livrată nu.
'''
''' CONSECINȚĂ DE CONSEMNAT: o instanță Adobe străină care se închide mai târziu poate rescrie
''' valoarea. Asta afectează doar o sesiune viitoare — instanța noastră citește preferința la
''' pornirea ei, iar noi am scris înainte să o pornim.
'''
''' LA RESTAURARE, o valoare ABSENTĂ se restaurează prin ȘTERGERE, niciodată prin scrierea lui 0
''' (regula fixată de <c>RegistrySnapshotSetTests.Absent_RestoresToDeletion_NotToZero</c>).
''' </summary>
Friend NotInheritable Class AdobeUiPreference

    ''' <summary>Comutatorul. Vezi remarcile clasei înainte de a-l pune pe True.</summary>
    Private Const ENFORCE_LEGACY_UI As Boolean = False

    Private Sub New()
    End Sub

    Private Shared ReadOnly _lock As New Object()
    Private Shared _applied As Boolean = False
    Private Shared _snapshot As RegistrySnapshotSet
    Private Shared _reg As IRegistryAccess

    ''' <summary>Adevărat după ce sesiunea a trecut o dată prin <see cref="EnsureApplied"/>.</summary>
    Friend Shared ReadOnly Property WasApplied As Boolean
        Get
            Return _applied
        End Get
    End Property

    ''' <summary>
    ''' Aplică preferința dacă e cazul. Idempotentă: apelurile următoare din aceeași sesiune ies
    ''' imediat. <paramref name="registry"/> există pentru teste; în producție rămâne Nothing.
    ''' </summary>
    Friend Shared Sub EnsureApplied(log As Action(Of String), Optional registry As IRegistryAccess = Nothing)
        If Not ENFORCE_LEGACY_UI Then Return
        Try
            SyncLock _lock
                If _applied Then Return
                _applied = True

                _reg = If(registry, New WinRegistryAccess())

                ' AdobeHiveResolver e PUR: sondarea o face apelantul, exact ca în banc.
                Dim resolution As AdobeHiveResolution = AdobeHiveResolver.Resolve(
                    readerHiveExists:=_reg.KeyExists(AdobeRegistryConstants.AvGeneralReader),
                    acrobatHiveExists:=_reg.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat),
                    exePath:=AdobeReaderHost.ResolveAdobePath())

                Dim hive As String = resolution.AvGeneralPath
                If String.IsNullOrEmpty(hive) Then
                    log?.Invoke("Nu am găsit cheia AVGeneral a Adobe — nu forțez interfața clasică.")
                    Return
                End If

                Dim current As RegistryValueSnapshot =
                    _reg.Read(hive, AdobeRegistryConstants.ValEnableAv2)

                ' Deja clasic: nu atingem nimic. Un instantaneu inutil ar fi o restaurare inutilă.
                If current.Presence <> RegPresence.Absent AndAlso IsZero(current.Value) Then
                    log?.Invoke($"Interfața Adobe e deja clasică ({AdobeRegistryConstants.ValEnableAv2}=0) — nu scriu nimic.")
                    Return
                End If

                _snapshot = New RegistrySnapshotSet(_reg)
                _snapshot.Capture(hive, AdobeRegistryConstants.ValEnableAv2)
                _reg.Write(hive, AdobeRegistryConstants.ValEnableAv2, RegistryValueKind.DWord, 0)

                Dim before As String = If(current.Presence = RegPresence.Absent,
                                          "absentă", Convert.ToString(current.Value))
                log?.Invoke($"Am forțat interfața clasică Adobe: {hive}\{AdobeRegistryConstants.ValEnableAv2} " &
                            $"{before} → 0. Valoarea se restaurează la ieșirea din aplicație.")
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUiPreference.EnsureApplied", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Pune valoarea la loc. De apelat la ieșirea din aplicație. Sigură când nu s-a scris nimic.
    ''' </summary>
    Friend Shared Sub Restore(log As Action(Of String))
        If Not ENFORCE_LEGACY_UI Then Return
        Try
            SyncLock _lock
                If _snapshot Is Nothing OrElse _snapshot.Count = 0 Then Return
                _snapshot.RestoreAll()
                log?.Invoke("Am restaurat preferința Adobe la valoarea dinaintea sesiunii.")
                _snapshot = Nothing
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUiPreference.Restore", ex)
        End Try
    End Sub

    ' Registry-ul poate întoarce Integer, Long sau String pentru același DWORD, în funcție de cine
    ' l-a scris. Comparăm valoarea, nu tipul.
    Private Shared Function IsZero(value As Object) As Boolean
        If value Is Nothing Then Return False
        Dim text As String = Convert.ToString(value)
        Dim parsed As Long
        Return Long.TryParse(text, parsed) AndAlso parsed = 0
    End Function

End Class
