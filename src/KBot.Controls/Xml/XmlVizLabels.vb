Option Strict On
Imports System.Collections.Generic
Imports System.Xml.Linq

''' <summary>
''' Slice 00EF-36 -- the Romanian names of the elements of an e-invoice (UBL 2.1 as CIUS-RO uses it) for the MAP of <see cref="XmlViz"/>
''' (the text shows the XML as it is). The operators do not know what «Party» or «TaxSubtotal» is: the map says «Date furnizor»,
''' «Subtotal TVA».
'''
''' <para>A name is looked up by its parent AND its own local name first (<c>ID</c> is the invoice number under the root, the IBAN under
''' <c>PayeeFinancialAccount</c>, the line number under <c>InvoiceLine</c>), then by its own name alone; an element nobody knows keeps
''' its name. The prefixes cac / cbc play no part (see <see cref="XmlViz"/>).</para>
''' </summary>
Friend NotInheritable Class XmlVizLabels

    Private Sub New()
    End Sub

    ' "Parent/Local" (more specific, looked up first) or "Local".
    Private Shared ReadOnly _names As Dictionary(Of String, String) = Build()

    ''' <summary>The label of an element for the map: its Romanian name, or its own local name when it is not known.</summary>
    Friend Shared Function LabelOf(k_el As XElement) As String
        ArgumentNullException.ThrowIfNull(k_el)
        Dim k_local As String = k_el.Name.LocalName
        Dim k_parent As String = If(k_el.Parent Is Nothing, "#root", k_el.Parent.Name.LocalName)
        Dim k_label As String = Nothing
        If _names.TryGetValue(k_parent & "/" & k_local, k_label) Then Return k_label
        If _names.TryGetValue(k_local, k_label) Then Return k_label
        Return k_local
    End Function

    Private Shared Function Build() As Dictionary(Of String, String)
        Dim d As New Dictionary(Of String, String)(StringComparer.Ordinal)

        ' ---- the document
        d("Invoice") = "Factură"
        d("CreditNote") = "Notă de credit"
        d("Invoice/ID") = "Număr factură"
        d("CreditNote/ID") = "Număr factură"
        d("UBLVersionID") = "Versiune UBL"
        d("CustomizationID") = "Specificație (CIUS-RO)"
        d("ProfileID") = "Profil de proces"
        d("IssueDate") = "Data emiterii"
        d("DueDate") = "Data scadenței"
        d("InvoiceTypeCode") = "Tip factură (cod)"
        d("CreditNoteTypeCode") = "Tip notă de credit (cod)"
        d("Note") = "Notă"
        d("TaxPointDate") = "Data exigibilității TVA"
        d("DocumentCurrencyCode") = "Moneda facturii"
        d("TaxCurrencyCode") = "Moneda pentru TVA"
        d("AccountingCost") = "Referință contabilă"
        d("BuyerReference") = "Referința cumpărătorului"

        ' ---- period, references
        d("InvoicePeriod") = "Perioada facturată"
        d("StartDate") = "Data de început"
        d("EndDate") = "Data de sfârșit"
        d("DescriptionCode") = "Cod moment exigibilitate"
        d("OrderReference") = "Comanda"
        d("OrderReference/ID") = "Număr comandă"
        d("SalesOrderID") = "Număr comandă vânzare"
        d("BillingReference") = "Factura anterioară"
        d("InvoiceDocumentReference") = "Factură referită"
        d("InvoiceDocumentReference/ID") = "Număr factură referită"
        d("DespatchDocumentReference") = "Aviz de expediție"
        d("ReceiptDocumentReference") = "Document de recepție"
        d("ContractDocumentReference") = "Contract"
        d("OriginatorDocumentReference") = "Document inițiator"
        d("ProjectReference") = "Proiect"
        d("AdditionalDocumentReference") = "Document suplimentar"
        d("DocumentTypeCode") = "Tip document (cod)"
        d("DocumentDescription") = "Descrierea documentului"
        d("Attachment") = "Atașament"
        d("EmbeddedDocumentBinaryObject") = "Fișier atașat (conținut)"
        d("ExternalReference") = "Referință externă"
        d("URI") = "Adresă (URI)"

        ' ---- the parties
        d("AccountingSupplierParty") = "Furnizor"
        d("AccountingCustomerParty") = "Client"
        d("PayeeParty") = "Beneficiarul plății"
        d("TaxRepresentativeParty") = "Reprezentant fiscal"
        d("AccountingSupplierParty/Party") = "Date furnizor"
        d("AccountingCustomerParty/Party") = "Date client"
        d("PayeeParty/Party") = "Date beneficiar"
        d("Party") = "Parte"
        d("EndpointID") = "Adresă electronică"
        d("PartyIdentification") = "Identificator parte"
        d("PartyIdentification/ID") = "Identificator"
        d("PartyName") = "Denumire comercială"
        d("PartyName/Name") = "Denumire"
        d("PostalAddress") = "Adresă"
        d("StreetName") = "Strada"
        d("AdditionalStreetName") = "Strada (adițional)"
        d("CityName") = "Localitate"
        d("PostalZone") = "Cod poștal"
        d("CountrySubentity") = "Județ"
        d("AddressLine") = "Linie de adresă"
        d("Line") = "Adresă"
        d("Country") = "Țara"
        d("IdentificationCode") = "Cod țară"
        d("PartyTaxScheme") = "Înregistrare TVA"
        d("PartyTaxScheme/CompanyID") = "Cod de TVA"
        d("PartyLegalEntity") = "Entitate juridică"
        d("RegistrationName") = "Denumire juridică"
        d("PartyLegalEntity/CompanyID") = "Număr de înregistrare / CUI"
        d("CompanyLegalForm") = "Forma juridică"
        d("Contact") = "Persoană de contact"
        d("Telephone") = "Telefon"
        d("ElectronicMail") = "E-mail"
        d("TaxScheme") = "Regim fiscal"
        d("TaxScheme/ID") = "Cod regim fiscal"

        ' ---- delivery and payment
        d("Delivery") = "Livrare"
        d("ActualDeliveryDate") = "Data livrării"
        d("DeliveryLocation") = "Locul livrării"
        d("DeliveryLocation/ID") = "Identificator loc"
        d("DeliveryLocation/Address") = "Adresa livrării"
        d("DeliveryParty") = "Destinatar"
        d("PaymentMeans") = "Mijloc de plată"
        d("PaymentMeansCode") = "Mijloc de plată (cod)"
        d("PaymentID") = "Referință plată"
        d("PayeeFinancialAccount") = "Cont bancar al beneficiarului"
        d("PayeeFinancialAccount/ID") = "IBAN"
        d("PayeeFinancialAccount/Name") = "Titularul contului"
        d("FinancialInstitutionBranch") = "Banca"
        d("FinancialInstitutionBranch/ID") = "Cod bancă (BIC)"
        d("CardAccount") = "Card"
        d("PaymentMandate") = "Mandat de plată"
        d("PaymentTerms") = "Condiții de plată"
        d("PaymentTerms/Note") = "Descrierea condițiilor"

        ' ---- allowances / charges, VAT, totals
        d("AllowanceCharge") = "Reducere / taxă suplimentară"
        d("ChargeIndicator") = "Este taxă suplimentară (altfel reducere)"
        d("AllowanceChargeReasonCode") = "Motiv (cod)"
        d("AllowanceChargeReason") = "Motiv"
        d("MultiplierFactorNumeric") = "Procent"
        d("Amount") = "Valoare"
        d("BaseAmount") = "Valoarea de bază"
        d("TaxTotal") = "Total TVA"
        d("TaxAmount") = "Valoare TVA"
        d("TaxSubtotal") = "Subtotal TVA"
        d("TaxableAmount") = "Baza de impozitare"
        d("TaxCategory") = "Categorie TVA"
        d("TaxCategory/ID") = "Cod categorie TVA"
        d("ClassifiedTaxCategory") = "Categorie TVA"
        d("ClassifiedTaxCategory/ID") = "Cod categorie TVA"
        d("Percent") = "Cotă TVA (%)"
        d("TaxExemptionReasonCode") = "Motiv scutire (cod)"
        d("TaxExemptionReason") = "Motiv scutire"
        d("LegalMonetaryTotal") = "Totaluri"
        d("LineExtensionAmount") = "Total linii (fără TVA)"
        d("TaxExclusiveAmount") = "Total fără TVA"
        d("TaxInclusiveAmount") = "Total cu TVA"
        d("AllowanceTotalAmount") = "Total reduceri"
        d("ChargeTotalAmount") = "Total taxe suplimentare"
        d("PrepaidAmount") = "Suma plătită anticipat"
        d("PayableRoundingAmount") = "Rotunjire"
        d("PayableAmount") = "Suma de plată"

        ' ---- the lines
        d("InvoiceLine") = "Linia facturii"
        d("CreditNoteLine") = "Linia notei de credit"
        d("InvoiceLine/ID") = "Nr. linie"
        d("CreditNoteLine/ID") = "Nr. linie"
        d("InvoiceLine/Note") = "Notă la linie"
        d("CreditNoteLine/Note") = "Notă la linie"
        d("InvoicedQuantity") = "Cantitate"
        d("CreditedQuantity") = "Cantitate creditată"
        d("InvoiceLine/LineExtensionAmount") = "Valoare linie (fără TVA)"
        d("CreditNoteLine/LineExtensionAmount") = "Valoare linie (fără TVA)"
        d("OrderLineReference") = "Linia comenzii"
        d("LineID") = "Număr linie comandă"
        d("DocumentReference") = "Document referit"
        d("Item") = "Articol"
        d("Item/Description") = "Descriere"
        d("Item/Name") = "Denumire"
        d("BuyersItemIdentification") = "Cod articol (cumpărător)"
        d("SellersItemIdentification") = "Cod articol (furnizor)"
        d("StandardItemIdentification") = "Cod standard (GTIN)"
        d("BuyersItemIdentification/ID") = "Cod"
        d("SellersItemIdentification/ID") = "Cod"
        d("StandardItemIdentification/ID") = "Cod"
        d("OriginCountry") = "Țara de origine"
        d("CommodityClassification") = "Clasificare (CPV)"
        d("ItemClassificationCode") = "Cod clasificare (CPV)"
        d("AdditionalItemProperty") = "Proprietate a articolului"
        d("AdditionalItemProperty/Name") = "Proprietate"
        d("Value") = "Valoare"
        d("Price") = "Preț"
        d("PriceAmount") = "Preț unitar (fără TVA)"
        d("BaseQuantity") = "Cantitatea de bază a prețului"

        ' ---- generic, last resort
        d("ID") = "Identificator"
        d("Name") = "Denumire"
        d("Description") = "Descriere"
        d("Quantity") = "Cantitate"
        Return d
    End Function

End Class
