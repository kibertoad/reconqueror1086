"""Independent synthetic flow examples; no original bytes or addresses."""
import unittest
from call_survey import walk_calls


class CallSurveyTests(unittest.TestCase):
    def test_jump_over_inline_data_reaches_call(self):
        # Jump over three undecodable bytes, then call the synthetic target.
        code = bytes.fromhex('eb03 ffff0f e8f6000000 c3')
        calls, unresolved = walk_calls(code, 0x1000, 0x1000, [(0x1000, 0x100b)], {0x1100: 'target'})
        self.assertEqual({0x1005: 'target'}, calls)
        self.assertEqual([], unresolved)

    def test_conditional_paths_both_followed_and_loop_terminates(self):
        code = bytes.fromhex('7405 e8f9000000 e8f4000000 ebfe')
        calls, unresolved = walk_calls(code, 0x1000, 0x1000, [(0x1000, 0x100e)], {0x1100: 'target'})
        self.assertEqual({0x1002: 'target', 0x1007: 'target'}, calls)
        self.assertEqual([], unresolved)

    def test_far_body_fragment_followed_only_through_edge(self):
        code = bytes.fromhex('eb06 ffffffffffff e8f3000000 c3')
        calls, unresolved = walk_calls(code, 0x1000, 0x1000,
                                      [(0x1000, 0x1002), (0x1008, 0x100e)], {0x1100: 'target'})
        self.assertEqual({0x1008: 'target'}, calls)
        self.assertEqual([], unresolved)

    def test_indirect_jump_and_external_edge_are_unresolved(self):
        for code, reason in [(bytes.fromhex('ffe0'), 'indirect branch'),
                             (bytes.fromhex('eb7f'), 'edge outside')]:
            calls, unresolved = walk_calls(code, 0x1000, 0x1000,
                                          [(0x1000, 0x1000 + len(code))], {})
            self.assertEqual({}, calls)
            self.assertIn(reason, unresolved[0]['reason'])

    def test_invalid_bounds_and_budget_fail(self):
        with self.assertRaisesRegex(ValueError, 'range'):
            walk_calls(b'\xc3', 0x1000, 0x1000, [(0x1000, 0x1002)], {})
        with self.assertRaisesRegex(ValueError, 'budget'):
            walk_calls(bytes.fromhex('9090c3'), 0x1000, 0x1000, [(0x1000, 0x1003)], {}, budget=1)


if __name__ == '__main__':
    unittest.main()
