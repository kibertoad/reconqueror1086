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
    value.agent.create_execution_breakpoint.side_effect = lambda session, selector, offset: SimpleNamespace(id=f'hook-{offset:x}')
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
        value.session.stop_reason.breakpoint_id = ('seed', 'hook-6b422', 'hook-595c0', 'hook-5965f')[value.phase]
    value.wait_until_stopped = wait
    return value, mapping


class ScreenBoundaryTests(unittest.TestCase):
    def startup_sequence(self, corrupt_stack=False, extraction=False, corrupt_archive=False,
                         startup_click=False, wrong_callback=False):
        runtime, mapping = runtime_and_mapping()
        pcs = (0x6b413, 0x6b422, 0x2ac08, 0x2ac86, 0x2ad54, 0x595c0, 0x5965f)  # FND-UI-018/017
        ids = ('seed', 'hook-6b422', 'hook-2ac08', 'hook-2ac86', 'hook-2ad54', 'hook-595c0', 'hook-5965f')
        if startup_click:
            pcs = pcs[:3] + (0x5b790,) + pcs[3:]  # FND-UI-019
            ids = ids[:3] + ('hook-5b790',) + ids[3:]
            original_read = runtime.read
            def read(address, length):
                if address == (0x188, 0x1000 - 104):
                    return b''.join(word.to_bytes(4, 'little') for word in
                                    (0x2ac61, 6144, 0 if wrong_callback else 0x5bb34))
                return original_read(address, length)
            runtime.read = read
        if extraction:
            pcs = pcs[:2] + (0x49ba8, 0x49c78) + pcs[2:]  # FND-SAVE-003
            ids = ids[:2] + ('hook-49ba8', 'hook-49c78') + ids[2:]
        original_registers = runtime.registers
        def registers():
            saved_phase = runtime.phase
            runtime.phase = min(saved_phase, 3)
            result = original_registers()
            runtime.phase = saved_phase
            result.instruction_pointer = hex(pcs[saved_phase])
            result.general['eax'] = '0x100'
            if corrupt_archive and pcs[saved_phase] == 0x49c78:
                result.general['esp'] = '0x1004'
            if pcs[saved_phase] in (0x2ac86, 0x2ad54):
                result.general['esp'] = hex(0x1000 - (88 if corrupt_stack else 92))
            elif pcs[saved_phase] == 0x5b790:
                result.general['esp'] = hex(0x1000 - 104)
            return result
        runtime.registers = registers
        def wait(operation):
            runtime.phase += 1
            runtime.session.stop_reason.breakpoint_id = ids[runtime.phase]
        runtime.wait_until_stopped = wait
        with patch('native_rng_recorder.tables_from_diagnostic', return_value=(0, 16)), \
             patch('native_rng_recorder.descriptor', side_effect=[mapping.code, mapping.data] * len(pcs)), \
             patch('native_rng_recorder.EventLog'), patch('native_rng_recorder.Path.write_text'), \
             patch('native_rng_recorder.queue_primary_click', return_value={'action': 'primary-short-click'}) as click:
            try:
                result = record(runtime, mapping, ('draw', 'seed'), 'unused-output',
                                stop_after_screen=True, startup_checkpoints=True, startup_click=startup_click)
            except Exception:
                if wrong_callback:
                    click.assert_not_called()
                raise
            if startup_click:
                click.assert_called_once_with(runtime, mapping)
                runtime.agent.delete_breakpoint.assert_called_once_with('owned', 'hook-5b790')
            return result

    def test_startup_checkpoints_continue_to_verified_screen(self):
        report = self.startup_sequence()
        self.assertEqual([item['boundary'] for item in report['startup_checkpoints']],
                         ['preparation-entry', 'animation-test', 'preparation-epilogue'])
        self.assertEqual(report['status'], 'screen-load-return-reached')
        self.assertFalse(report['full_game_complete'])

    def test_startup_checkpoint_rejects_changed_stack(self):
        with self.assertRaisesRegex(RecordingError, 'Startup preparation stack differs'):
            self.startup_sequence(corrupt_stack=True)

    def test_archive_progress_is_separate_from_rng_and_screen_completion(self):
        report = self.startup_sequence(extraction=True)
        observed = report['startup_checkpoints'][:2]
        self.assertEqual([item['boundary'] for item in observed], ['archive-entry', 'archive-return'])
        self.assertEqual(observed[1]['returned_length'], 256)
        self.assertFalse(report['pending_archive_extraction'])
        self.assertEqual(len(report['events']), 1)
        self.assertFalse(report['full_game_complete'])

    def test_archive_progress_rejects_wrong_return_frame(self):
        with self.assertRaisesRegex(RecordingError, 'Archive extraction return/frame mismatch'):
            self.startup_sequence(extraction=True, corrupt_archive=True)

    def test_supported_click_only_runs_at_the_verified_startup_wait(self):
        report = self.startup_sequence(startup_click=True)
        self.assertEqual(report['supported_input'], [{'action': 'primary-short-click'}])
        self.assertEqual(report['status'], 'screen-load-return-reached')
        self.assertEqual(len(report['events']), 1)

    def test_startup_click_rejects_a_different_callback_before_writing(self):
        with self.assertRaisesRegex(RecordingError, 'Startup wait arguments differ'):
            self.startup_sequence(startup_click=True, wrong_callback=True)

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


class OwnedReductionTests(unittest.TestCase):
    def run_draw(self, sound=False, bound=200, caller=0x4f2cf, divisor=10):
        runtime, mapping = runtime_and_mapping()
        raw_return = 0x5b44d if sound else 0x445b9  # FND-SOUND-008 / FND-RNG-002
        endpoint = 0x5b454 if sound else 0x445c1
        pcs = (0x6b413, 0x6b422, 0x6b3f1, 0x6b412, endpoint)
        original_read = runtime.read
        def read(address, length):
            if address == (0x188, 0x9e044):
                return (0 if runtime.phase == 0 else 1 if runtime.phase < 3 else 1103527590).to_bytes(4, 'little')
            if address == (0x188, 0x1000):
                words = (0x24c32, 1, 0) if runtime.phase == 0 else (raw_return, caller, bound)
                return b''.join(word.to_bytes(4, 'little') for word in words)
            return original_read(address, length)
        runtime.read = read
        runtime.registers = lambda: SimpleNamespace(
            general={'esp': '0x1004' if runtime.phase == 4 else '0x1000',
                     'eax': hex(102 if runtime.phase == 4 else 16838),
                     'edx': '0x8', 'esi': hex(divisor)},
            segments={'cs': '0x180', 'ds': '0x188', 'ss': '0x188'},
            cpu_mode='protected', instruction_pointer=hex(pcs[runtime.phase]))
        ids = ('seed', 'hook-6b422', 'draw', 'hook-6b412', f'hook-{endpoint:x}')
        def wait(operation):
            runtime.phase += 1
            runtime.session.stop_reason.breakpoint_id = ids[runtime.phase]
        runtime.wait_until_stopped = wait
        with patch('native_rng_recorder.tables_from_diagnostic', return_value=(0, 16)), \
             patch('native_rng_recorder.descriptor', side_effect=[mapping.code, mapping.data] * 5), \
             patch('native_rng_recorder.EventLog'), patch('native_rng_recorder.Path.write_text'):
            return record(runtime, mapping, ('draw', 'seed'), 'unused-output', maximum_draws=1)

    def test_hit_check_records_the_scaled_result(self):
        report = self.run_draw()
        event = report['events'][-1]
        self.assertEqual((event['rule'], event['reduction'], event['bound'], event['result']),
                         ('RULE-ASSAULT-023', 'scaled', 200, 102))
        self.assertFalse(report['full_game_complete'])

    def test_busy_voice_records_remainder_register_instead_of_quotient(self):
        event = self.run_draw(sound=True)['events'][-1]
        self.assertEqual((event['rule'], event['reduction'], event['bound'], event['result']),
                         ('RULE-SOUND-002', 'remainder', 10, 8))

    def test_scaled_helper_rejects_other_callers_and_bounds(self):
        for options in ({'caller': 0x4f2d0}, {'bound': 201}):
            with self.subTest(options=options), self.assertRaisesRegex(RecordingError, 'Unowned scaled-helper'):
                self.run_draw(**options)

    def test_busy_voice_rejects_wrong_divisor_before_draw(self):
        with self.assertRaisesRegex(RecordingError, 'Busy-voice divisor differs'):
            self.run_draw(sound=True, divisor=9)


if __name__ == '__main__':
    unittest.main()
