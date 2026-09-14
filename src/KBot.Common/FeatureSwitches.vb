Option Strict On

''' <summary>
''' Comutatoarele INTERNE ale aplicației: un singur loc care spune ce e aprins și ce nu.
'''
''' <para>Nu e configurație de operator și nu se citește (încă) din niciun fișier — sunt valori de
''' cod, aici ca să existe UN loc de schimbat, nu zece <c>If</c>-uri împrăștiate. Când se va decide
''' de unde vin cu adevărat — rol de utilizator, manifest de actualizare, cheie de configurare —
''' se schimbă IMPLEMENTAREA proprietăților de aici, iar apelanții rămân neatinși. Ăsta e tot rostul
''' clasei.</para>
''' </summary>
Public Module FeatureSwitches

    ''' <summary>
    ''' Are operatorul acces la vizualizatorul de jurnale (meniul butonului de opțiuni din bara de
    ''' titlu a shell-ului, rândul «Arată jurnal»)?
    '''
    ''' <para><b>Azi: mereu True</b> — oricine poate deschide jurnalele. Când e False, meniul NU se
    ''' deschide deloc: fiind singurul rând, un meniu cu el stins ar fi o fereastră goală care se
    ''' agață de buton degeaba. Vezi <c>MainForm.CapBar_OptionButtonClick</c>.</para>
    ''' </summary>
    Public ReadOnly Property VizualizatorJurnaleActiv As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Does the reception picker open with EVERY reception already ticked?
    '''
    ''' <para><b>Today: always True</b> — asked for that way by the operator (10.09.2026). The
    ''' ordinary press on a node is «bring me everything», so a form that opened with nothing
    ''' ticked would turn the ordinary press into a chore before it does anything at all. False
    ''' makes the same form open empty, i.e. «bring me nothing until I say which». The switch is
    ''' here because that choice was asked for as CONFIGURABLE later, not because it is in doubt
    ''' now — see <c>SelectieReceptiiForm</c> and <c>MainForm.AlegeReceptiileDeReimprospatat</c>.</para>
    ''' </summary>
    Public ReadOnly Property ReceptiiBifateLaDeschidere As Boolean
        Get
            Return True
        End Get
    End Property

End Module
