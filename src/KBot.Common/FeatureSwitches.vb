Option Strict On

''' <summary>
''' Comutatoarele INTERNE ale aplicației: un singur loc care spune ce e aprins și ce nu.
'''
''' <para>Until slice 0072 these were code values. They now come from <see cref="AppSettings"/>
''' (the operator's «Setări» window), but callers keep reading them FROM HERE -- which is the
''' whole point of the class: the implementation changed, the callers did not.</para>
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
            ' Slice 0072: the operator's own switch (settings window, «Aplicație»); default True.
            Return AppSettings.Current.LogViewerEnabled
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
            ' Slice 0072: configurable at last, from the settings window; default True.
            Return AppSettings.Current.ReceptiiCheckedOnOpen
        End Get
    End Property

End Module
