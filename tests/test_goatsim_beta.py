import unittest
from tools.goatsim_beta import validate_priority, PrototypeError

class BetaTests(unittest.TestCase):
    def test_requires_cooldown(self):
        with self.assertRaises(PrototypeError):
            validate_priority([{'name':'Strike'}])
