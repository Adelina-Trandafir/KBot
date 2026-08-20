# Offline unit tests for the slice 0044 router (routes/migrare/routing.py), the
# server-side port of KBot.Migrator's RowRouter.
#   python -m pytest tests/test_migrare_routing.py
#
# No config.py, no MariaDB, no Access file: the maps are built by hand, which is
# the whole point of keeping the routing rule separate from the reading.
#
# The rules under test are the ones that lose data when they are wrong:
#   * FX_Extrase_F fans out to SEVERAL databases, on purpose;
#   * a row with two parents that DISAGREE is a hard error, not a fallback;
#   * a key that resolves nowhere is rejected with a reason, never dropped silently.

import pytest

from routes.migrare import routing, tables


def maps():
    m = routing.RoutingMaps()
    m.unit_to_dc = {48: "000_DEMO", 75: "005_CEVM", 121: "005_CEVM"}
    m.angajament = {"aab-001": "000_DEMO", "aab-002": "005_CEVM"}
    m.rezervare = {10: "000_DEMO"}
    m.receptie_r = {20: "000_DEMO"}
    m.receptie_h = {30: "000_DEMO", 31: "005_CEVM"}
    m.extras_file = {40: {"000_DEMO", "005_CEVM"}, 41: {"000_DEMO"}}
    return m


def router(table_name):
    return routing.RowRouter(tables.by_name(table_name), maps())


# --- DC propriu, apoi IdUnitate ----------------------------------------------

def test_dc_propriu_bate_idunitate():
    dcs, reject = router("FX_Angajamente").route(
        {"CodAngajament": "AAB-001", "DC": "005_CEVM", "IdUnitate": 48})
    assert (dcs, reject) == (["005_CEVM"], None)


def test_fara_dc_propriu_se_cade_pe_idunitate():
    dcs, reject = router("FX_Angajamente").route(
        {"CodAngajament": "AAB-001", "IdUnitate": 48})
    assert (dcs, reject) == (["000_DEMO"], None)


def test_idunitate_necunoscut_e_respins_cu_motiv():
    dcs, reject = router("FX_Angajamente").route(
        {"CodAngajament": "AAB-009", "IdUnitate": 999})
    assert dcs == []
    assert "999" in reject and "[Cai]" in reject


# --- prin angajament ----------------------------------------------------------

def test_copilul_urmeaza_angajamentul_indiferent_de_litere():
    # Codurile din Access vin cu majuscule amestecate; harta e pe litere mici.
    dcs, reject = router("FX_Istoric").route({"ID": 1, "CodAngajament": "AaB-002"})
    assert (dcs, reject) == (["005_CEVM"], None)


def test_angajamentul_inexistent_e_respins_nu_pierdut():
    dcs, reject = router("FX_Istoric").route({"ID": 2, "CodAngajament": "ZZZ-999"})
    assert dcs == []
    assert "ZZZ-999" in reject


def test_codangajament_lipsa_e_respins():
    dcs, reject = router("FX_Istoric").route({"ID": 3})
    assert dcs == []
    assert "CodAngajament" in reject


# --- prin rezervare -----------------------------------------------------------

def test_imaginea_urmeaza_rezervarea():
    dcs, reject = router("FX_Rezervarii_IMG").route({"IDRZC": 1, "IDRZ": 10})
    assert (dcs, reject) == (["000_DEMO"], None)


def test_rezervarea_inexistenta_e_respinsa():
    dcs, reject = router("FX_Rezervarii_IMG").route({"IDRZC": 2, "IDRZ": 77})
    assert dcs == []
    assert "FX_Rezervari" in reject


# --- doi parinti --------------------------------------------------------------

def test_primul_parinte_castiga_cand_amandoi_sunt_de_acord():
    dcs, reject = router("FX_Receptii_IMG").route({"IDRDC": 1, "IDRR": 20, "IDRH": 30})
    assert (dcs, reject) == (["000_DEMO"], None)


def test_retragerea_pe_al_doilea_parinte_cand_primul_lipseste():
    dcs, reject = router("FX_Receptii_IMG").route({"IDRDC": 2, "IDRH": 31})
    assert (dcs, reject) == (["005_CEVM"], None)


def test_parinti_care_nu_sunt_de_acord_opresc_migrarea():
    # Nu retragere si nici ghiceala: legaturile din Access se contrazic, iar o
    # alegere ar muta randul in baza gresita.
    with pytest.raises(routing.RoutingError) as info:
        router("FX_Receptii_IMG").route({"IDRDC": 3, "IDRR": 20, "IDRH": 31})
    assert "nu sunt de acord" in str(info.value)


def test_niciun_parinte_rezolvabil_e_respins():
    dcs, reject = router("FX_Receptii_IMG").route({"IDRDC": 4, "IDRR": 88, "IDRH": 99})
    assert dcs == []
    assert "IDRR" in reject and "IDRH" in reject


def test_ordinea_parintilor_difera_intre_cele_doua_tabele():
    assert tables.by_name("FX_Receptii_IMG").route_column == "IDRR"
    assert tables.by_name("FX_Receptii_Plati").route_column == "IDRH"


# --- multiplicarea intentionata ----------------------------------------------

def test_fisierul_de_extras_ajunge_in_toate_bazele_lui():
    dcs, reject = router("FX_Extrase_F").route({"IDEXF": 40})
    assert reject is None
    assert dcs == ["000_DEMO", "005_CEVM"]


def test_extrasul_fara_antet_e_respins():
    dcs, reject = router("FX_Extrase_F").route({"IDEXF": 42})
    assert dcs == []
    assert "FX_Extrase_H" in reject


# --- setul de tabele ----------------------------------------------------------

def test_setul_are_sasesprezece_tabele_in_ordinea_de_scriere():
    assert len(tables.ALL) == 16
    nume = [t.name for t in tables.ALL]
    # Parintii inaintea copiilor: fiecare regula depinde de tabelul de dinaintea ei.
    assert nume.index("FX_Angajamente") == 0
    assert nume.index("FX_Rezervari") < nume.index("FX_Rezervarii_IMG")
    assert nume.index("FX_Receptii_R") < nume.index("FX_Receptii_IMG")
    assert nume.index("FX_Receptii_H") < nume.index("FX_Receptii_Plati")


def test_tabel_necunoscut_arunca_nu_intoarce_tacut_nimic():
    with pytest.raises(KeyError):
        tables.by_name("FX_Parteneri")


def test_cheia_primara_e_raportata_pentru_lista_de_respinse():
    r = router("FX_Istoric")
    assert r.primary_key_of({"ID": 17}) == "17"
    assert r.primary_key_of({}) == "?"
