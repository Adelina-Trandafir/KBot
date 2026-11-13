-- SLICE-VC14-04. Aplicati manual pe AVACONT_SURSA (si pe fiecare baza VERCON), INAINTE de publicarea codului.
-- Anularea unui NIR / FCS / BC / BR / BP / BT / BS / BU / lista de alimente nu mai lasa documentul in tabele ca «anulat»:
-- continutul lui (antet + linii + legaturile) se copiaza aici, ca JSON, apoi randurile se sterg din tabelele VC_.
-- Numarul documentului se elibereaza. Tabelul nu are chei straine (supravietuieste oricarei stergeri) si nu este citit
-- de nicio macheta VERCON: este doar locul unde se pastreaza.
-- Colatie: utf8mb3 / utf8mb3_general_ci, ca restul bazei.
CREATE TABLE IF NOT EXISTS VC_ArhivaAnulate (
    IdArhiva    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    SubunitId   INT UNSIGNED NOT NULL,
    GestiuneId  INT UNSIGNED NOT NULL,
    ExercitiuId INT UNSIGNED NOT NULL,
    Familie     VARCHAR(10)  NOT NULL COMMENT 'nir, storno, bc, br, lista, bp, bt, bs, bu',
    IdDocument  BIGINT       NOT NULL COMMENT 'cheia documentului in momentul anularii (IdIntrare, IdBonConsum, IdLista, IDBP/IDBT/IDBC, ID-ul BU)',
    Numar       VARCHAR(60)  NULL     COMMENT 'numarul asa cum il citea operatorul (BC 13, NIR 707,1, ...)',
    DataDoc     DATETIME     NULL,
    Motiv       VARCHAR(255) NULL,
    AnulatDe    VARCHAR(255) NULL,
    AnulatLa    DATETIME     NULL,
    Continut    LONGTEXT     NOT NULL COMMENT 'JSON: {tabela: [randuri]} cu tot ce s-a sters',
    CreatedAt   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt   DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdArhiva),
    KEY ix_ArhivaAnulate_doc (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
