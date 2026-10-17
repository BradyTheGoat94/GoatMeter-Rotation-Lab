import unittest
from tools.goatsim_evidence_export import build_export, ExportError


class TimestampTests(unittest.TestCase):
    def test_outside_window_is_rejected(self):
        profile = {'class':'Templar','level':45}
        encounters = [{'start':'2026-10-17T10:00:00Z',
                       'end':'2026-10-17T10:00:10Z',
                       'events':[{'timestamp':'2026-10-17T10:01:00Z',
                                  'skill':'Strike','damage':120,'source_role':'self'}]}]
        with self.assertRaises(ExportError):
            build_export(profile, encounters, consent=True)
