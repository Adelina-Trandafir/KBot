# routes/avacont/introducere_date/operatii/NoteC.py
"""SLICE-AV08-04: the ONE place where the accounting notes are written (notes, documents, operations, debit / credit lines, payments and orders of payment,
links with received e-invoices, deletes) and where their rules live. Every window's route only reads the request, calls NoteC and answers.

The family, each file one job (all named NoteC_*):
    NoteC_tranzactie           the lock of the unit and the transaction of every write, the refusal (NoteCEroare), the constants
    NoteC_reguli               the rules: subunit, closed period / earlier year / journal JI, accounts of the chart and «Protectie operatii», partner, IBAN, delete
    NoteC_scriere              what writes: notes and their numbers, documents, operations, a template line made operation, e-invoice links, payment, deletes
    NoteC_plati                payments, orders of payment, allocations, what is left to pay
    NoteC_editor               «Nota manuala» / «Modifica operatie»
    NoteC_diverse              the save of «Operatii diverse»
    NoteC_facturi_plati        the save of «Facturi si plati»
    NoteC_contari              the save of the «Contari» of received e-invoices (evidence and payment)
    NoteC_contari_calcul       what the «Contari» page shows before the save, and what an invoice already has
    NoteC_contari_ajutatoare   the helpers of the «Contari»
This file only gathers their names: NoteC.X works for every X of the family.
"""
from .NoteC_tranzactie import (  # noqa: F401
    TREZORERIE_BIC, FEL_INCHIDERE, JURNAL_INCHIDERE, CENT, SQL_PLATI_PE_EVIDENTA, SQL_UNITATI_SURSEI, NoteCEroare, salvare)
from .NoteC_reguli import (  # noqa: F401
    subunitate_pentru, data_ultimei_inchideri, perioada_inchisa, este_document_inchidere, verifica_data_operatie, verifica_data_document, CONFIRMARE_LUNA, confirma, luna_noua, ajusteaza_data, cont_pentru, linii_sablon, conturi_nepotrivite, cont_din_subunitate, partener_al_sursei, cont_iban_partener, motiv_stergere)
from .NoteC_scriere import (  # noqa: F401
    numar_nota_urmator, nota_existenta, rezolva_nota, note_ale_subunitatii, creeaza_nota, creeaza_document, creeaza_operatie, verifica_echilibru, document_si_operatie, sterge_operatie, curata_goale, leaga_evidentiere, sume_evidentiate, actualizeaza_suma_evidentiata, salveaza_plata, sablonul, operatie_din_sablon, sterge_randuri)
from .NoteC_plati import (  # noqa: F401
    PLAJA_COMUNA, IMPLICITE, PlatiEroare, cent, citeste_setare, plaja_comuna, scrie_setare, numar_op_urmator, motiv_blocare, operatii_legate, evidentieri, alocat, primite_pe_evidentieri, in_limita, limita_comuna, rest_de_plata, are_receptie, primite_cu_receptie, creeaza_op, aloca_pe_evidentieri, aloca_pe_operatii, valori_pe_operatii, id_partener_linie, posteaza_ordin, sterge_plata)
from .NoteC_contari_ajutatoare import (  # noqa: F401
    CONTARI_SQL, MAX_FACTURI, MODURI, MIN_EXPLICATIE_OP, MAX_EXPLICATIE_OP, text_op, numar_document)
from .NoteC_contari_calcul import (  # noqa: F401
    calcul_contari, existente_contari)
from .NoteC_contari import (  # noqa: F401
    date_unitate, plata_e_facturi, salveaza_contari)
from .NoteC_diverse import (  # noqa: F401
    salveaza_operatii_diverse)
from .NoteC_facturi_plati import (  # noqa: F401
    salveaza_facturi_plati)
from .NoteC_editor import (  # noqa: F401
    PARTI, TIPURI_EDITEZ, TIPURI_OPERATIE_MANUALA, MESAJE_ALEGERE, ETICHETE, CONFIRMARE_DOCUMENT, refuz, id_exact, text_exact, data_exacta, confirma_cerere, document, detalii, verifica_rol, citeste_linii, valori, scrie_linii, verifica_protectia, luna_noua_cerere, document_dublat, antet_document, jurnalul, clasificatia, raspuns, aplica_nota, salveaza_nota_manuala)
from .NoteC_contari_ajutatoare import _clasificatii as clasificatii_cu_sabloane, _sabloane as sabloane_subunitatii  # noqa: F401,E402
