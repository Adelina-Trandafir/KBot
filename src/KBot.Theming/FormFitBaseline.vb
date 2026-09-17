Option Strict On

''' <summary>
''' What a themed form's "base size" means once the application scale changes (slice 0062).
'''
''' <para>The base is the client size a form has right after <c>InitializeComponent</c> -- with
''' the active scheme's font already on it (the base constructor assigns it) and the platform's
''' font autoscale already applied. <see cref="ThemeFormFit"/> never lets a form shrink below it;
''' the two values here only differ once the scale (<c>AppScaling</c>, DPI) changes at runtime.
''' At a fixed scale they describe the same number of pixels.</para>
'''
''' <para><see cref="Scaled"/> is deliberately 0: a <c>theme.json</c> written before the slice has
''' no key, a missing key deserializes to zero, and zero must land on the wanted default.</para>
''' </summary>
Public Enum FormFitBaseline
    ''' <summary>The base follows the scale cursor: captured size x (scale now / scale at capture).</summary>
    Scaled = 0
    ''' <summary>The base is frozen in raw pixels, exactly as captured.</summary>
    DesignerRaw = 1
End Enum
