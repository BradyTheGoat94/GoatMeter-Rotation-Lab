import unittest
from tools.goatsim_evidence_export import build_export
from tools.goatsim_beta import run_prototype


class IntegrationTests(unittest.TestCase):
    def test_export_then_simulate(self):
        profile = {'class':'Templar','level':45}
        encounters = [{'start':'2026-10-17T10:00:00Z',
                       'end':'2026-10-17T10:00:10Z',
                       'events':[{'timestamp':'2026-10-17T10:00:00.100Z',
                                  'skill':'Strike','damage':120,'source_role':'self',
                                  'damage_kind':'direct'}]}]
        exported = build_export(profile, encounters, consent=True)
        result = run_prototype(exported, [{'name':'Strike','cooldown_ms':1000}],
                               duration_ms=3000)
        self.assertEqual(result['estimated_dps'], 120)
