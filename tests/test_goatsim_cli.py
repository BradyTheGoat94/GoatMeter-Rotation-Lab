import unittest
from goatsim.cli import run_payload


class CliTests(unittest.TestCase):
    def setUp(self):
        self.payload = {
            "character": {
                "sourceUrl": "https://shugo.gg/character/global/nae-zikel/Bradyboi?tab=stats",
                "className": "Templar",
                "stats": {"attack": 100, "critChance": 0, "critMultiplier": 1.5}
            },
            "durationMs": 10000,
            "skills": [{"name": "Synthetic Test Hit", "potency": 1,
                        "cooldownMs": 0, "castMs": 1000}]
        }

    def test_end_to_end(self):
        result = run_payload(self.payload, trials=10)
        self.assertEqual(result["character"], "Bradyboi")
        self.assertEqual(result["results"]["meanDps"], 100)
        self.assertEqual(result["results"]["modelStatus"], "synthetic-unvalidated")

    def test_missing_skills(self):
        self.payload["skills"] = []
        with self.assertRaises(ValueError):
            run_payload(self.payload, trials=10)

    def test_invalid_duration(self):
        self.payload["durationMs"] = True
        with self.assertRaises(ValueError):
            run_payload(self.payload, trials=10)


if __name__ == "__main__":
    unittest.main()
