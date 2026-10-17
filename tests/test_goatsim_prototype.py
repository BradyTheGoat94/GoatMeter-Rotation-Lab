import unittest
from goatsim.prototype import Profile, Skill, parse_shugo_character_url, simulate


class PrototypeTests(unittest.TestCase):
    def test_profile_url(self):
        p = parse_shugo_character_url("https://shugo.gg/character/global/nae-zikel/Bradyboi?tab=stats")
        self.assertEqual(p["characterName"], "Bradyboi")

    def test_reject_host_spoof(self):
        for url in ("https://shugo.gg.evil.test/character/global/a/b",
                    "http://shugo.gg/character/global/a/b",
                    "https://shugo.gg/character/global/a/%2F"):
            with self.assertRaises(ValueError):
                parse_shugo_character_url(url)

    def test_missing_stat(self):
        with self.assertRaises(ValueError):
            simulate(Profile(None, 0, 1), [Skill("A", 1, 1000, 1000)], 2000)

    def test_repeatability_and_cooldown(self):
        p = Profile(100, 0.25, 2)
        s = [Skill("A", 1, 2000, 1000)]
        a = simulate(p, s, 5000, 7)
        self.assertEqual(a, simulate(p, s, 5000, 7))
        self.assertEqual([x["atMs"] for x in a["timeline"]], [1000, 3000, 5000])

    def test_resource(self):
        a = simulate(Profile(100, 0, 1, 2), [Skill("A", 1, 0, 1000, 1)], 5000)
        self.assertEqual(len(a["timeline"]), 2)
        self.assertEqual(a["remainingResource"], 0)

    def test_no_cast_beyond_end(self):
        a = simulate(Profile(100, 0, 1), [Skill("A", 1, 0, 1500)], 1000)
        self.assertEqual(a["dps"], 0)


if __name__ == "__main__":
    unittest.main()
