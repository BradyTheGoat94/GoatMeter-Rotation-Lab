import unittest
from tools.goatsim_beta import validate_priority, PrototypeError

class BetaTests(unittest.TestCase):
    def test_requires_cooldown(self):
        with self.assertRaises(PrototypeError):
            validate_priority([{'name':'Strike'}])

    def test_deterministic_and_isolated(self):
        from tools.goatsim_beta import run_prototype
        evidence = {'schema':'goatsim-export-v1',
                    'character':{'class':'Templar','level':45},
                    'encounters':[{'events':[
                        {'skill':'Strike','damage':120,'damage_kind':'direct','source_role':'self'},
                        {'skill':'Strike','damage':999999,'damage_kind':'direct','source_role':'other'}]}]}
        priority = [{'name':'Strike','cooldown_ms':1000}]
        first = run_prototype(evidence, priority, duration_ms=3000)
        self.assertEqual(first, run_prototype(evidence, priority, duration_ms=3000))
        self.assertEqual(first['estimated_dps'], 120)
        self.assertEqual(first['evidence']['excluded_or_untyped_hits'], 1)
        self.assertEqual([e['time_ms'] for e in first['events']], [0,1000,2000])

    def test_untyped_rejected(self):
        from tools.goatsim_beta import run_prototype
        evidence = {'schema':'goatsim-export-v1',
                    'character':{'class':'Templar'},
                    'encounters':[{'events':[{'skill':'Strike','damage':10,
                                             'source_role':'self'}]}]}
        with self.assertRaises(PrototypeError):
            run_prototype(evidence, [{'name':'Strike','cooldown_ms':0}])

    def test_unsupported_condition_rejected(self):
        with self.assertRaises(PrototypeError):
            validate_priority([{'name':'Strike','cooldown_ms':0,'requires':'proc'}])
