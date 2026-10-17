import unittest
from tools.goatsim_evidence_export import build_export, ExportError


class KindTests(unittest.TestCase):
    def test_kind_retained(self):
        profile = {'class':'Templar','level':45}
        encounters = [{'start':'2026-10-17T10:00:00Z',
                       'end':'2026-10-17T10:00:10Z',
                       'events':[{'timestamp':'2026-10-17T10:00:00.100Z',
                                  'skill':'Strike','damage':120,'source_role':'self',
                                  'damage_kind':'direct'}]}]
        payload = build_export(profile, encounters, consent=True)
        self.assertEqual(payload['encounters'][0]['events'][0]['damage_kind'],'direct')
        encounters[0]['events'][0].pop('damage_kind')
        payload = build_export(profile, encounters, consent=True)
        self.assertEqual(payload['encounters'][0]['events'][0]['damage_kind'],'unknown')
