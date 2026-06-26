Wrapper XFA writer ca librărie. De importat: AdobeUtils.vb (iTextSharp:
  PdfReader / PdfStamper / XfaForm / AcroFields) + restul logicii din XFA_WRITTER.

DE TĂIAT: Sub Main / parsarea argumentelor CLI (devine librărie).

Expune o metodă publică, ex.: GenereazaPdf(xmlConfigPath, templatePdfPath, outputPdfPath, docType).
Deschiderea PDF-ului = Process.Start(file) cu aplicația default.

PACHET PDF = punct deschis — NU adăuga nimic acum.
