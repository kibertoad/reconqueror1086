"""Synthetic native stops using FND-RNG-003 and FND-UI-001 entry identities."""
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch
from native_rng_recorder import record
from rng_recording import RecordingError


def runtime_and_mapping(screen_state=123, screen_number=0, record_pointer=0x300000, object_screen=0):
    value = SimpleNamespace()
    value.phase = 0
    value.MemoryAddress = SimpleNamespace(segmented=lambda segment, offset: (segment, offset),
                                         physical=lambda offset: ('physical', offset))
    value.session = SimpleNamespace(id='owned', state='stopped')
    value.session.stop_reason = SimpleNamespace(kind='breakpoint', breakpoint_id='seed')
    value.agent = Mock()
    value.agent.create_execution_breakpoint.side_effect = [SimpleNamespace(id=f'hook-{i}') for i in range(6)]
    value.agent.execute_command.return_value.raw_output = 'synthetic'
    code = SimpleNamespace(selector=0x180, resolve=lambda offset: offset)
    data = SimpleNamespace(selector=0x188)
    mapping = SimpleNamespace(code=code, data=data, code_base=0x10000, code_size=0x80000,
                              code_address=lambda address, length=1: (0x180, address),
                              data_address=lambda address, length=1: (0x188, address))
    def read(address, length):
        if address == (0x188, 0x9e044):
            return (0 if value.phase == 0 else 123 if value.phase == 1 else screen_state).to_bytes(4, 'little')
        if address == (0x188, 0x1000):
            return b''.join(word.to_bytes(4, 'little') for word in (0x24c32, 123, 0))
        if address == (0x188, 0x1004):
            return b''.join(word.to_bytes(4, 'little') for word in (screen_number, 1, 0))
        if address == (0x188, 0xafe50):  # FND-UI-017; pointers below are synthetic.
            return record_pointer.to_bytes(4, 'little')
        if address == (0x188, 0x300000):
            return b''.join(word.to_bytes(4, 'little') for word in (0x300100, screen_number, *([0xffffffff] * 4)))
        if address == (0x188, 0x300100):
            block = bytearray(184)
            block[96:100] = object_screen.to_bytes(4, 'little')
            return bytes(block)
        return bytes(length)
    value.read = read
    value.registers = lambda: SimpleNamespace(
        general={'esp': '0x1000'}, segments={'cs': '0x180', 'ds': '0x188', 'ss': '0x188'},
        cpu_mode='protected', instruction_pointer=hex((0x6b413, 0x6b422, 0x595c0, 0x5965f)[value.phase]))
    def wait(operation):
        value.phase += 1
        value.session.stop_reason.breakpoint_id = ('seed', 'hook-1', 'hook-4', 'hook-5')[value.phase]
    value.wait_until_stopped = wait
    return value, mapping


class ScreenBoundaryTests(unittest.TestCase):
    def run_sequence(self, after=False, **options):
        runtime, mapping = runtime_and_mapping(**options)
        with patch('native_rng_recorder.tables_from_diagnostic', return_value=(0, 16)), \
             patch('native_rng_recorder.descriptor', side_effect=[mapping.code, mapping.data] * 4), \
             patch('native_rng_recorder.EventLog'), \
             patch('native_rng_recorder.Path.write_text'):
            return record(runtime, mapping, ('draw', 'seed'), 'unused-output',
                          stop_at_screen=not after, stop_after_screen=after)

    def test_loaded_screen_checks_object_history_and_rng(self):
        report = self.run_sequence(after=True)
        self.assertEqual(report['status'], 'screen-load-return-reached')
        self.assertEqual(report['screen_observation']['history'], [0, -1, -1, -1, -1])
        self.assertEqual(report['screen_observation']['screen_id'], 0)
        self.assertFalse(report['full_game_complete'])
        self.assertFalse(report['pending_screen_load'])

    def test_loaded_screen_rejects_wrong_object_identity(self):
        with self.assertRaisesRegex(RecordingError, 'Loaded screen identity/history differs'):
            self.run_sequence(after=True, object_screen=1)

    def test_loaded_screen_rejects_null_record(self):
        with self.assertRaisesRegex(RecordingError, 'outside configured guest RAM'):
            self.run_sequence(after=True, record_pointer=0)

    def test_loaded_screen_rejects_unrecorded_rng_change(self):
        with self.assertRaisesRegex(RecordingError, 'Loaded-screen RNG state differs'):
            self.run_sequence(after=True, screen_state=124)

    def test_screen_entry_is_a_diagnostic_boundary_not_full_game_completion(self):
        report = self.run_sequence()
        self.assertEqual(report['status'], 'screen-load-entry-reached')
        self.assertEqual(report['screen_request'], {'screen': 0, 'draw': 1, 'mode': 0})
        self.assertEqual(report['end_rng_state'], 123)
        self.assertFalse(report['full_game_complete'])
        self.assertFalse(report['accepted_callers_complete'])

    def test_unrecorded_state_change_at_screen_boundary_fails(self):
        with self.assertRaisesRegex(RecordingError, 'Screen-boundary RNG state differs'):
            self.run_sequence(screen_state=124)

    def test_unregistered_screen_fails(self):
        with self.assertRaisesRegex(RecordingError, 'Unregistered requested screen'):
            self.run_sequence(screen_number=25)


if __name__ == '__main__':
    unittest.main()
