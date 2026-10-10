"""Native entry/return breakpoint recorder; accepted caller coverage is explicit.

FND-RNG-002/003/004, FND-PERSON-003, FND-TALK-003, FND-STRATEGY-043 support the current policies.
FND-UI-001 supports the optional screen-loading diagnostic boundary.
FND-UI-002/017 support the initial loaded-screen record checks.
This transport does not claim full-game caller coverage or actual rebuild replay.
"""
import dataclasses
import json
import os
from pathlib import Path
from rng_journal import Journal, replay
from live_mapping import descriptor, tables_from_diagnostic
from rng_recording import RecordingError, canonical_pc
from supported_pointer_input import queue_primary_click, primary_click_ready


class EventLog:
    """Append only completed rule events; retain them if capture is interrupted."""
    def __init__(self, path):
        self.file = Path(path).open('x', encoding='utf-8', newline='\n')

    def append(self, event):
        self.file.write(json.dumps(event, separators=(',', ':'), allow_nan=False) + '\n')
        self.file.flush()
        os.fsync(self.file.fileno())

    def close(self):
        self.file.close()


def record(runtime, mapping, entry_ids, output, maximum_draws=30, stop_at_screen=False, stop_after_screen=False,
           startup_checkpoints=False, startup_click=False, continue_after_screen=False, title_click=False,
           screen_checkpoints=False, stop_after_screen_id=None, new_game_click=False, generation_click=False,
           youth_answer=False, youth_continue=False):
    if stop_at_screen and stop_after_screen:
        raise ValueError('Choose one screen diagnostic boundary')
    if startup_checkpoints and not stop_after_screen:
        raise ValueError('Startup checkpoints require the loaded-screen diagnostic')
    if startup_click and not startup_checkpoints:
        raise ValueError('Startup click requires verified startup checkpoints')
    if continue_after_screen and not stop_after_screen:
        raise ValueError('Continuation requires a verified loaded-screen boundary')
    if title_click and not continue_after_screen:
        raise ValueError('Title input requires guarded recording continuation')
    if screen_checkpoints and not continue_after_screen:
        raise ValueError('Screen checkpoints require guarded recording continuation')
    if new_game_click and not screen_checkpoints:
        raise ValueError('New-game input requires verified screen checkpoints')
    if generation_click and not new_game_click:
        raise ValueError('Generation input requires the guarded new-game sequence')
    if youth_answer and (not generation_click or stop_after_screen_id == 3):
        raise ValueError('Youth answer requires generation input and continuation beyond screen three')
    if youth_continue and not youth_answer:
        raise ValueError('Youth Continue requires the guarded answer stage')
    if stop_after_screen_id is not None and (not screen_checkpoints or
            type(stop_after_screen_id) is not int or not 0 <= stop_after_screen_id <= 24):
        raise ValueError('Screen target requires checkpoints and a registered screen identifier')
    if type(maximum_draws) is not int or not 1 <= maximum_draws <= 100000:
        raise ValueError('Invalid recording draw limit')
    output = Path(output)
    journal = Journal()
    hooks = {entry_ids[0]: 'draw-entry', entry_ids[1]: 'seed-entry'}
    for address, name in ((0x6b412, 'draw-return'), (0x6b422, 'seed-return'),
                          (0x24c4b, 'inclusive-return'), (0x1a18d, 'prompt-result')):
        selector, offset = mapping.code_address(address)
        hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
        hooks[hook.id] = name
    if stop_at_screen or stop_after_screen:
        # FND-UI-001 names both screen-loading entries and their arguments.
        # This boundary precedes loading/setup; it does not mean menu-ready.
        for address in ((0x595c0,) if stop_after_screen and not screen_checkpoints else (0x595c0, 0x596c0)):
            selector, offset = mapping.code_address(address)
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = 'screen-entry'
    if stop_after_screen:
        selector, offset = mapping.code_address(0x5965f)  # FND-UI-017
        hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
        hooks[hook.id] = 'screen-return'
        if screen_checkpoints:
            selector, offset = mapping.code_address(0x5975e)  # FND-UI-020
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = 'screen-return'
    startup_points = {0x2ac08: 'preparation-entry', 0x2ac86: 'animation-test',
                      0x2ad54: 'preparation-epilogue'}  # FND-UI-018
    if startup_checkpoints:
        for address in startup_points:
            selector, offset = mapping.code_address(address)
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = 'startup-checkpoint'
        for address, name in ((0x49ba8, 'archive-entry'), (0x49c78, 'archive-return')):  # FND-SAVE-003
            selector, offset = mapping.code_address(address)
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = name
    for address, name in ((0x445c1, 'scaled-return'), (0x5b454, 'sound-result')):
        # FND-RNG-002 / FND-SOUND-008 locate the completed reductions.
        selector, offset = mapping.code_address(address)
        hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
        hooks[hook.id] = name
    if startup_click:
        selector, offset = mapping.code_address(0x5b790)  # FND-UI-019
        hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
        hooks[hook.id] = 'startup-wait'
    if youth_answer:
        # FND-PERSON-005: first answer entry and final return.
        for address, name in ((0x14b38, 'answer-entry'), (0x14cdd, 'answer-return')):
            selector, offset = mapping.code_address(address)
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = name
    if youth_continue:
        # FND-UI-022: both restored-stack returns of the Continue callback.
        for address, name in ((0x15054, 'continue-entry'), (0x150a7, 'continue-return'),
                              (0x151f5, 'continue-return')):
            selector, offset = mapping.code_address(address)
            hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
            hooks[hook.id] = name
    controls = []
    if youth_continue:
        for start, end in ((0x15054, 0x151f6), (0x63114, 0x63267)):
            selector, offset = mapping.code_address(start, end - start)
            address = runtime.MemoryAddress.segmented(selector, offset)
            controls.append((address, runtime.read(address, end - start)))  # FND-UI-022 / FND-BATTLE-023
    if youth_answer:
        selector, offset = mapping.code_address(0x14b38, 0x14cde - 0x14b38)
        address = runtime.MemoryAddress.segmented(selector, offset)
        controls.append((address, runtime.read(address, 0x14cde - 0x14b38)))  # FND-PERSON-005
    code_ranges = [(0x6b3eb, 0x6b423), (0x24c38, 0x24c4c), (0x1a14c, 0x1a1ef), (0x43670, 0x436e0)]
    code_ranges.extend(((0x445b4, 0x445c2), (0x4f2ac, 0x4f2d6), (0x5b418, 0x5b470)))
    # FND-PERSON-004: initial, rerolled and continued dilemma-selection draws.
    code_ranges.extend(((0x148c0, 0x14902), (0x15054, 0x150e9), (0x15228, 0x152a5)))
    if stop_at_screen or stop_after_screen:
        code_ranges.extend(((0x595c0, 0x59660), (0x596c0, 0x5975f)))
    if stop_after_screen:
        code_ranges.extend(((0x59ce4, 0x59d11), (0x63d20, 0x63d3e),
                            (0x59fa0, 0x5a180), (0x5a2a4, 0x5a414)))
    if startup_checkpoints:
        code_ranges.append((0x2ac08, 0x2ad5b))  # FND-UI-018
        code_ranges.append((0x49ba8, 0x49c79))  # FND-SAVE-003
    if startup_click:
        # FND-UI-019 / FND-BATTLE-023: input boundary and queue consumers.
        code_ranges.extend(((0x5b790, 0x5b7bd), (0x5bb34, 0x5bb49),
                            (0x24d14, 0x24d54), (0x63098, 0x63102), (0x63114, 0x63267)))
    for start, end in code_ranges:
        selector, offset = mapping.code_address(start, end - start)
        address = runtime.MemoryAddress.segmented(selector, offset)
        controls.append((address, runtime.read(address, end - start)))
    selector, state_offset = mapping.data_address(0x9e044, 4)
    state_address = runtime.MemoryAddress.segmented(selector, state_offset)
    def state():
        return int.from_bytes(runtime.read(state_address, 4), 'little')
    def canonical_return(value):
        if not mapping.code_base <= value < mapping.code_base + mapping.code_size:
            raise RecordingError('Saved return lies outside the verified code object')
        return value - mapping.code_base + 0x10000
    frame = None
    screen_frame = None
    initial_screen_seen = False
    new_game_click_done = False
    generation_click_done = False
    youth_answer_queued = False
    answer_key = None
    continue_key = None
    continue_queued = False
    readiness_hook = None
    startup_key = None
    archive_key = None
    archive_ordinal = 0
    count = 0
    shift_ordinal = 0
    report = {'schema': 'conquer-native-rng-journal-v1', 'status': 'incomplete',
              'full_game_complete': False, 'accepted_callers_complete': False,
              'events': journal.events}
    event_log = EventLog(output / 'native-rng-events.jsonl')
    try:
        while True:
            stop = runtime.session.stop_reason
            if runtime.session.state != 'stopped' or stop.kind != 'breakpoint' or stop.breakpoint_id not in hooks:
                raise RecordingError('Unmodelled native stop while recording')
            diagnostic = runtime.agent.execute_command(runtime.session.id, 'CPU').raw_output
            table_base, table_length = tables_from_diagnostic(diagnostic)
            table = runtime.read(runtime.MemoryAddress.physical(table_base), table_length)
            if descriptor(table, mapping.code.selector) != mapping.code or descriptor(table, mapping.data.selector) != mapping.data:
                raise RecordingError('Native descriptor identity changed during recording')
            if any(runtime.read(address, len(control)) != control for address, control in controls):
                raise RecordingError('Original RNG/owner code changed during recording')
            registers = runtime.registers()
            pc = canonical_pc(registers, mapping)
            if any(int(registers.segments[name], 16) != mapping.data.selector for name in ('ds', 'ss')):
                raise RecordingError('Unmodelled data/stack descriptor change')
            stack_pointer = int(registers.general['esp'], 16)
            key = (int(registers.segments['ss'], 16), stack_pointer)
            kind = hooks[stop.breakpoint_id]
            if kind == 'continue-readiness':
                if pc != 0x63114 or frame is not None or screen_frame is not None or answer_key is not None:
                    raise RecordingError('Continue readiness boundary/frame mismatch')
                # FND-UI-017 / FND-UI-002: retain the verified youth screen identity.
                selector, offset = mapping.data_address(0xafe50, 4)
                pointer = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4), 'little')
                if pointer != report['screen_observation']['record_pointer']:
                    raise RecordingError('Continue readiness screen record changed')
                record_bytes = runtime.read(runtime.MemoryAddress.segmented(selector, pointer), 24)
                object_pointer = int.from_bytes(record_bytes[:4], 'little')
                if object_pointer != report['screen_observation']['object_pointer'] or int.from_bytes(record_bytes[4:8], 'little') != 3:
                    raise RecordingError('Continue readiness requires the verified youth screen')
                if primary_click_ready(runtime, mapping):
                    current = journal.complete()
                    if state() != current or replay(journal.events) != current:
                        raise RecordingError('Continue readiness RNG state differs from journal')
                    click = queue_primary_click(runtime, mapping, x=500, y=150)  # SCR-UI-004 / FND-UI-021
                    if state() != current:
                        raise RecordingError('RNG state changed while queuing Continue')
                    report.setdefault('supported_input', []).append(dict(click, screen=3, stage='continue'))
                    (output / 'supported-input.json').write_text(json.dumps(report['supported_input'], indent=2) + '\n')
                    continue_queued = True
                    runtime.agent.delete_breakpoint(runtime.session.id, readiness_hook)
                    del hooks[readiness_hook]
                    readiness_hook = None
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind in ('continue-entry', 'continue-return'):
                if not continue_queued or frame is not None or screen_frame is not None or answer_key is not None:
                    raise RecordingError('Continue boundary lacks prescribed input or has an unfinished operation')
                if kind == 'continue-entry':
                    if pc != 0x15054 or continue_key is not None:
                        raise RecordingError('Unexpected or reentrant Continue entry')
                    continue_key = key
                else:
                    if pc not in (0x150a7, 0x151f5) or key != continue_key:
                        raise RecordingError('Continue return/frame mismatch')
                    report['end_rng_state'] = journal.complete()
                    if state() != report['end_rng_state'] or replay(journal.events) != report['end_rng_state']:
                        raise RecordingError('Continue RNG state differs from journal')
                    continue_key = None
                    report['status'] = 'youth-continue-return-reached'
                    return report
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind in ('answer-entry', 'answer-return'):
                if not youth_answer_queued or frame is not None or screen_frame is not None or archive_key is not None:
                    raise RecordingError('Youth answer boundary lacks prescribed input or has an unfinished operation')
                if kind == 'answer-entry':
                    if pc != 0x14b38 or answer_key is not None:
                        raise RecordingError('Unexpected or reentrant youth answer entry')
                    answer_key = key
                else:
                    if pc != 0x14cdd or answer_key != key:
                        raise RecordingError('Youth answer return/frame mismatch')
                    report['end_rng_state'] = journal.complete()
                    if state() != report['end_rng_state'] or replay(journal.events) != report['end_rng_state']:
                        raise RecordingError('Youth answer RNG state differs from journal')
                    answer_key = None
                    report['status'] = 'youth-answer-return-reached'
                    if not youth_continue:
                        return report
                    selector, offset = mapping.code_address(0x63114)  # FND-BATTLE-023
                    hook = runtime.agent.create_execution_breakpoint(runtime.session.id, selector, offset)
                    readiness_hook = hook.id
                    hooks[hook.id] = 'continue-readiness'
                    report['status'] = 'incomplete'
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind == 'startup-wait':
                if pc != 0x5b790 or frame is not None or archive_key is not None or screen_frame is not None or startup_key is None:
                    raise RecordingError('Startup input boundary/frame mismatch')
                if key != (startup_key[0], startup_key[1] - 104):
                    raise RecordingError('Startup wait stack differs from its supported caller')
                arguments = runtime.read(runtime.MemoryAddress.segmented(key[0], key[1]), 12)
                words = [int.from_bytes(arguments[i:i + 4], 'little') for i in (0, 4, 8)]
                if canonical_return(words[0]) != 0x2ac61 or words[1] != 6144 or words[2] != mapping.code_address(0x5bb34)[1]:
                    raise RecordingError('Startup wait arguments differ from their supported reading')
                current_state = journal.complete()
                if state() != current_state or replay(journal.events) != current_state:
                    raise RecordingError('Startup input RNG state differs before supported writes')
                observation = queue_primary_click(runtime, mapping)
                report.setdefault('supported_input', []).append(observation)
                (output / 'supported-input.json').write_text(json.dumps(report['supported_input'], indent=2))
                if state() != current_state:
                    raise RecordingError('RNG state changed during supported pointer writes')
                runtime.agent.delete_breakpoint(runtime.session.id, stop.breakpoint_id)
                del hooks[stop.breakpoint_id]
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind in ('archive-entry', 'archive-return'):
                if frame is not None:
                    raise RecordingError('Archive checkpoint overlaps a pending recorded operation')
                if kind == 'archive-entry':
                    if pc != 0x49ba8 or archive_key is not None:
                        raise RecordingError('Archive extraction entry/frame mismatch')
                    archive_key = key
                    archive_ordinal += 1
                elif pc != 0x49c78 or archive_key != key:
                    raise RecordingError('Archive extraction return/frame mismatch')
                current_state = journal.complete()
                if state() != current_state or replay(journal.events) != current_state:
                    raise RecordingError('Archive checkpoint RNG state differs from recorded events')
                observation = {'boundary': kind, 'ordinal': archive_ordinal, 'rng_state': current_state}
                if kind == 'archive-return':
                    observation['returned_length'] = int(registers.general['eax'], 16)
                    archive_key = None
                report.setdefault('startup_checkpoints', []).append(observation)
                (output / 'startup-checkpoints.json').write_text(json.dumps(report['startup_checkpoints'], indent=2))
                print('Verified startup checkpoint:', observation, flush=True)
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind == 'startup-checkpoint':
                if pc not in startup_points or frame is not None or screen_frame is not None:
                    raise RecordingError('Startup checkpoint/frame mismatch')
                if pc == 0x2ac08:
                    if startup_key is not None:
                        raise RecordingError('Repeated startup preparation requires separate evidence')
                    startup_key = key
                elif startup_key is None or key != (startup_key[0], startup_key[1] - 92):
                    raise RecordingError('Startup preparation stack differs from its reading')
                current_state = journal.complete()
                if state() != current_state or replay(journal.events) != current_state:
                    raise RecordingError('Startup checkpoint RNG state differs from recorded events')
                selector, offset = mapping.data_address(0x9adb8, 4)  # FND-SOUND-004
                animation_flag = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4), 'little')
                observation = {'boundary': startup_points[pc], 'animation_flag': animation_flag,
                               'rng_state': current_state}
                report.setdefault('startup_checkpoints', []).append(observation)
                (output / 'startup-checkpoints.json').write_text(json.dumps(report['startup_checkpoints'], indent=2))
                print('Verified startup checkpoint:', observation, flush=True)
                runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                continue
            if kind == 'screen-return':
                if frame is not None or archive_key is not None or screen_frame is None or \
                        pc != screen_frame['return_pc'] or screen_frame['key'] != key:
                    raise RecordingError('Loaded-screen return/frame mismatch')
                selector, offset = mapping.data_address(0xafe50, 4)  # FND-UI-017
                pointer = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4), 'little')
                def heap_read(pointer, length):
                    if not 0 < pointer <= 16 * 1024 * 1024 - length:
                        raise RecordingError('Screen pointer lies outside configured guest RAM')
                    return runtime.read(runtime.MemoryAddress.segmented(mapping.data.selector, pointer), length)
                record_bytes = heap_read(pointer, 24)
                object_pointer = int.from_bytes(record_bytes[:4], 'little')
                object_bytes = heap_read(object_pointer, 184)  # FND-UI-002
                screen_id = int.from_bytes(object_bytes[96:100], 'little')
                history = [int.from_bytes(record_bytes[i:i + 4], 'little', signed=True) for i in range(4, 24, 4)]
                if screen_id != screen_frame['request']['screen'] or history[0] != screen_id:
                    raise RecordingError('Loaded screen identity/history differs from request')
                report['end_rng_state'] = journal.complete()
                if state() != report['end_rng_state'] or replay(journal.events) != report['end_rng_state']:
                    raise RecordingError('Loaded-screen RNG state differs from recorded events')
                report['status'] = 'screen-load-return-reached'
                report['screen_request'] = screen_frame['request']
                report['screen_observation'] = {'record_pointer': pointer, 'object_pointer': object_pointer,
                                                'screen_id': screen_id, 'history': history}
                screen_frame = None
                if screen_checkpoints:
                    observation = {'request': report['screen_request'],
                                   'observation': report['screen_observation'],
                                   'rng_state': report['end_rng_state']}
                    report.setdefault('screen_loads', []).append(observation)
                    (output / 'screen-load-checkpoints.json').write_text(
                        json.dumps(report['screen_loads'], indent=2) + '\n')
                    print('Verified loaded screen:', screen_id, flush=True)
                    if screen_id == stop_after_screen_id:
                        report['status'] = 'screen-target-return-reached'
                        return report
                if continue_after_screen:
                    report['status'] = 'incomplete'
                    if not initial_screen_seen:
                        (output / 'screen-load-checkpoint.json').write_text(
                            json.dumps({'request': report['screen_request'],
                                        'observation': report['screen_observation'],
                                        'rng_state': report['end_rng_state']}, indent=2) + '\n')
                    if title_click and not initial_screen_seen:
                        if screen_id != 0:
                            raise RecordingError('Prescribed title input requires screen zero')
                        # SCR-UI-001: the title's full-screen region accepts a
                        # primary click. The queue helper enforces field/timing guards.
                        click = queue_primary_click(runtime, mapping, x=10, y=10)
                        report.setdefault('supported_input', []).append(dict(click, screen=0))
                        (output / 'supported-input.json').write_text(
                            json.dumps(report['supported_input'], indent=2) + '\n')
                        if state() != report['end_rng_state']:
                            raise RecordingError('RNG state changed while queuing title input')
                    if new_game_click and screen_id == 1 and not new_game_click_done:
                        # SCR-UI-002 / FND-UI-012: region 3 starts a new game.
                        click = queue_primary_click(runtime, mapping, x=100, y=350)
                        report.setdefault('supported_input', []).append(dict(click, screen=1))
                        (output / 'supported-input.json').write_text(
                            json.dumps(report['supported_input'], indent=2) + '\n')
                        if state() != report['end_rng_state']:
                            raise RecordingError('RNG state changed while queuing new-game input')
                        new_game_click_done = True
                    if generation_click and screen_id == 2 and not generation_click_done:
                        # SCR-UI-003: region 0 opens youth generation.
                        click = queue_primary_click(runtime, mapping, x=200, y=250)
                        report.setdefault('supported_input', []).append(dict(click, screen=2))
                        (output / 'supported-input.json').write_text(
                            json.dumps(report['supported_input'], indent=2) + '\n')
                        if state() != report['end_rng_state']:
                            raise RecordingError('RNG state changed while queuing generation input')
                        generation_click_done = True
                    if youth_answer and screen_id == 3 and not youth_answer_queued:
                        # SCR-UI-004 / FND-PERSON-005: first answer rectangle.
                        click = queue_primary_click(runtime, mapping, x=100, y=350)
                        report.setdefault('supported_input', []).append(dict(click, screen=3))
                        (output / 'supported-input.json').write_text(
                            json.dumps(report['supported_input'], indent=2) + '\n')
                        if state() != report['end_rng_state']:
                            raise RecordingError('RNG state changed while queuing youth answer')
                        youth_answer_queued = True
                    # These are one-time startup observations, not policies for
                    # subsequent screen transitions or archive requests.
                    for hook_id, hook_kind in list(hooks.items()):
                        if hook_kind in ('startup-checkpoint', 'archive-entry', 'archive-return') or \
                                (not screen_checkpoints and hook_kind in ('screen-entry', 'screen-return')):
                            runtime.agent.delete_breakpoint(runtime.session.id, hook_id)
                            del hooks[hook_id]
                    initial_screen_seen = True
                    runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                    continue
                return report
            if kind == 'screen-entry':
                if pc not in (0x595c0, 0x596c0) or frame is not None:
                    raise RecordingError('Screen boundary/frame mismatch')
                arguments = runtime.read(runtime.MemoryAddress.segmented(key[0], key[1] + 4), 12)
                screen, draw, mode = (int.from_bytes(arguments[i:i + 4], 'little') for i in (0, 4, 8))
                if not 0 <= screen <= 24:
                    raise RecordingError('Unregistered requested screen at diagnostic boundary')
                if stop_after_screen:
                    if screen_frame is not None:
                        raise RecordingError('Recursive screen loading requires separate evidence')
                    screen_frame = {'key': key, 'return_pc': 0x5965f if pc == 0x595c0 else 0x5975e,
                                    'request': {'screen': screen, 'draw': draw, 'mode': mode}}
                    runtime.wait_until_stopped(runtime.agent.continue_(runtime.session.id))
                    continue
                report['end_rng_state'] = journal.complete()
                if state() != report['end_rng_state'] or replay(journal.events) != report['end_rng_state']:
                    raise RecordingError('Screen-boundary RNG state differs from recorded events')
                report['status'] = 'screen-load-entry-reached'
                report['screen_request'] = {'screen': screen, 'draw': draw, 'mode': mode}
                return report
            if kind.endswith('entry'):
                if pc != (0x6b413 if kind == 'seed-entry' else 0x6b3f1):
                    raise RecordingError('Native entry breakpoint/map mismatch')
                if frame is not None:
                    raise RecordingError('Reentrant RNG invocation requires separate evidence')
                stack = runtime.read(runtime.MemoryAddress.segmented(key[0], key[1]), 12)
                words = [int.from_bytes(stack[i:i + 4], 'little') for i in (0, 4, 8)]
                return_pc = canonical_return(words[0])
                if kind == 'seed-entry':
                    rule = {0x24c32: 'RULE-RNG-001', 0x1a178: 'RULE-TALK-001'}.get(return_pc)
                    if rule is None or (not journal.events and return_pc != 0x24c32):
                        raise RecordingError('Unowned seed caller; stopped before its state write')
                    journal.begin_seed(rule, words[1], state())
                    frame = {'key': key, 'return_pc': return_pc, 'phase': 'seed'}
                elif return_pc == 0x24c3d:
                    outer_pc = canonical_return(words[1])
                    policy = {0x1624d: (1, 'RULE-PERSON-002'), 0x16259: (8, 'RULE-PERSON-002'),
                              0x43678: (7, 'RULE-STRATEGY-012'),
                              0x148f6: (4, 'RULE-PERSON-003'), 0x15299: (4, 'RULE-PERSON-003'),
                              0x150db: (4, 'RULE-PERSON-004')}.get(outer_pc)
                    if policy is None:
                        raise RecordingError('Unowned inclusive-helper caller; stopped before draw')
                    bound, rule = policy
                    if words[2] != bound:
                        raise RecordingError('Native helper argument differs from its direct reading')
                    # FND-STRATEGY-043 settles its caller's argument; recording
                    # 7 does not settle RULE-STRATEGY-012's disputed interpretation.
                    if rule == 'RULE-PERSON-002':
                        index = int(registers.general['esi'], 16)
                        if shift_ordinal == 30 and index == 0 and bound == 1:
                            shift_ordinal = 0
                        if index != (shift_ordinal // 2) * 4 or shift_ordinal >= 30 or bound != (1 if shift_ordinal % 2 == 0 else 8):
                            raise RecordingError('Character-shift bound/index/order differs from its supported reading')
                        rule = 'RULE-PERSON-002'
                    journal.begin_draw(rule, 'inclusive', bound, state())
                    frame = {'key': key, 'return_pc': return_pc, 'phase': 'raw',
                             'bound_key': (key[0], key[1] + 4), 'bound_pc': 0x24c4b,
                             'outer_pc': outer_pc}
                elif return_pc == 0x445b9:
                    # FND-ASSAULT-031: this exact caller passes 200 for the hit check.
                    if canonical_return(words[1]) != 0x4f2cf or words[2] != 200:
                        raise RecordingError('Unowned scaled-helper caller or unexpected hit-check bound')
                    journal.begin_draw('RULE-ASSAULT-023', 'scaled', 200, state())
                    frame = {'key': key, 'return_pc': return_pc, 'phase': 'raw',
                             'bound_key': (key[0], key[1] + 4), 'bound_pc': 0x445c1}
                elif return_pc == 0x5b44d:
                    # FND-SOUND-008: only the exhausted ten-voice scan reaches this call.
                    if int(registers.general['esi'], 16) != 10:
                        raise RecordingError('Busy-voice divisor differs from its supported reading')
                    journal.begin_draw('RULE-SOUND-002', 'remainder', 10, state())
                    frame = {'key': key, 'return_pc': return_pc, 'phase': 'raw',
                             'bound_key': (key[0], key[1] + 4), 'bound_pc': 0x5b454}
                elif return_pc == 0x1a183:
                    # FND-TALK-003: EDI carries the packed node word; its high
                    # byte is the signed prompt count used by the reduction.
                    packed = int(registers.general['edi'], 16)
                    bound = packed >> 24
                    if not 2 <= bound <= 127:
                        raise RecordingError('Unsupported prompt count before native draw')
                    journal.begin_draw('RULE-TALK-001', 'remainder', bound, state())
                    frame = {'key': key, 'return_pc': return_pc, 'phase': 'raw',
                             'bound_key': (key[0], key[1] + 4), 'bound_pc': 0x1a18d}
                else:
                    raise RecordingError('Unowned raw draw caller; stopped before draw')
            elif kind == 'seed-return':
                if pc != 0x6b422 or frame is None or frame['phase'] != 'seed' or frame['key'] != key:
                    raise RecordingError('Seed return/frame mismatch')
                journal.finish_seed(state())
                event_log.append(journal.events[-1])
                frame = None
            elif kind == 'draw-return':
                if pc != 0x6b412 or frame is None or frame['phase'] != 'raw' or frame['key'] != key:
                    raise RecordingError('Raw return/frame mismatch')
                journal.finish_raw(state(), int(registers.general['eax'], 16))
                frame['phase'] = 'bound'
            else:
                if frame is None or frame['phase'] != 'bound' or frame['bound_key'] != key or frame['bound_pc'] != pc:
                    raise RecordingError('Bounded result/frame mismatch')
                if kind == 'sound-result' and int(registers.general['esi'], 16) != 10:
                    raise RecordingError('Busy-voice divisor changed before reduction')
                result_register = 'edx' if kind in ('prompt-result', 'sound-result') else 'eax'
                if journal.pending['rule'] == 'RULE-PERSON-002':
                    shift_ordinal += 1
                journal.finish_bound(state(), int(registers.general[result_register], 16))
                event_log.append(journal.events[-1])
                frame = None
                count += 1
                if count == maximum_draws:
                    report['end_rng_state'] = journal.complete()
                    if replay(journal.events) != report['end_rng_state']:
                        raise RecordingError('Complete journal replay differs')
                    report['status'] = 'bounded-limit-reached'
                    return report
            operation = runtime.agent.continue_(runtime.session.id)
            runtime.wait_until_stopped(operation)
    except Exception as error:
        report['failure'] = str(error)
        try:
            if runtime.agent.status(runtime.session.id).state == 'running':
                runtime.wait(runtime.agent.pause(runtime.session.id))
            diagnostic = runtime.registers()
            (output / 'native-rng-failure-registers.json').write_text(json.dumps(dataclasses.asdict(diagnostic), indent=2))
            with (output / 'native-rng-failure-memory.bin').open('wb') as memory:
                for base in range(0, 16 * 1024 * 1024, 65536):
                    memory.write(runtime.read(runtime.MemoryAddress.physical(base), 65536))
        except Exception as diagnostic_error:
            report['diagnostic_failure'] = type(diagnostic_error).__name__
        raise
    finally:
        event_log.close()
        report['pending_operation'] = journal.pending is not None
        report['pending_screen_load'] = screen_frame is not None
        report['pending_archive_extraction'] = archive_key is not None
        report['pending_youth_answer'] = answer_key is not None
        report['pending_youth_continue'] = continue_key is not None
        (output / 'native-rng-journal.json').write_text(json.dumps(report, indent=2))
