import unittest
from goatsim.prototype import Profile, Skill
from goatsim.monte_carlo import estimate_dps


class MonteCarloTests(unittest.TestCase):
    def setUp(self):
        self.profile = Profile(100, 0.5, 2)
        self.skills = [Skill("Test", 1, 0, 1000)]

    def test_reproducible(self):
        a = estimate_dps(self.profile, self.skills, 10000, 100, 42)
        self.assertEqual(a, estimate_dps(self.profile, self.skills, 10000, 100, 42))

    def test_percentiles_ordered(self):
        r = estimate_dps(self.profile, self.skills, 10000)
        self.assertLessEqual(r["minDps"], r["p05Dps"])
        self.assertLessEqual(r["p05Dps"], r["medianDps"])
        self.assertLessEqual(r["medianDps"], r["p95Dps"])
        self.assertLessEqual(r["p95Dps"], r["maxDps"])

    def test_no_crit_zero_variance(self):
        r = estimate_dps(Profile(100, 0, 2), self.skills, 10000)
        self.assertEqual(r["p05Dps"], 100)
        self.assertEqual(r["p95Dps"], 100)

    def test_invalid_trials(self):
        for count in (0, 1, 10001, 2.5, "100"):
            with self.assertRaises(ValueError):
                estimate_dps(self.profile, self.skills, 10000, count)


if __name__ == "__main__":
    unittest.main()
