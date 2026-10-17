import unittest
from goatsim.snapshot import import_snapshot


class SnapshotTests(unittest.TestCase):
    def setUp(self):
        self.data = {"sourceUrl": "https://shugo.gg/character/global/nae-zikel/Bradyboi?tab=stats",
                     "className": "Templar",
                     "stats": {"attack": 100, "critChance": 0.2, "critMultiplier": 1.5}}

    def test_import(self):
        snapshot = import_snapshot(self.data)
        self.assertEqual(snapshot.character_name, "Bradyboi")
        self.assertEqual(snapshot.profile.attack, 100)
        self.assertEqual(snapshot.provenance, "manual-unverified")

    def test_missing_stat(self):
        self.data["stats"].pop("attack")
        with self.assertRaises(ValueError):
            import_snapshot(self.data)

    def test_bad_fraction(self):
        self.data["stats"]["critChance"] = 20
        with self.assertRaises(ValueError):
            import_snapshot(self.data)

    def test_bad_numeric(self):
        for value in (True, "100", float("nan"), float("inf")):
            self.data["stats"]["attack"] = value
            with self.assertRaises(ValueError):
                import_snapshot(self.data)


if __name__ == "__main__":
    unittest.main()
