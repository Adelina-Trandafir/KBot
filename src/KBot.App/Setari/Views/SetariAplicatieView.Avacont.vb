Option Strict On
Imports KBot.Common

' Slice AV08-01 - the «AVACONT» tab of «Setari > Aplicatie»: the per-user settings of the accounting windows. For now one tick, «Protectie operatii»
' (the Access setting Scheme._MNC.C5, forced on by the form Note): the accounting editor keeps the accounts to the classification of the operation.
Partial Public Class SetariAplicatieView

    Private Sub IncarcaAvacont()
        Dim k_before As Boolean = _suppress
        _suppress = True
        Try
            chkProtectieOperatii.Checked = AppSettings.Current.AvacontProtectieOperatii
        Finally
            _suppress = k_before
        End Try
    End Sub

    Private Sub ChkProtectieOperatii_CheckedChanged(sender As Object, e As EventArgs) Handles chkProtectieOperatii.CheckedChanged
        SalveazaComutator(Sub(k_s) k_s.AvacontProtectieOperatii = chkProtectieOperatii.Checked,
                          If(chkProtectieOperatii.Checked,
                             "Operațiile contabile primesc doar conturile potrivite clasificației alese.",
                             "Operațiile contabile pot primi orice cont al subunității."))
    End Sub
End Class
