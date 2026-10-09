"""Native entry/return breakpoint recorder; accepted caller coverage is explicit.

FND-RNG-002/003/004, FND-PERSON-003, FND-TALK-003, FND-STRATEGY-043 support the current policies.
This transport does not claim full-game caller coverage or actual rebuild replay.
"""
import dataclasses
import json
from pathlib import Path
from rng_journal import Journal, replay
from live_mapping import descriptor, tables_from_diagnostic
from rng_recording import RecordingError, canonical_pc


def record(runtime, mapping, entry_ids, output, maximum_draws=30):
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
    controls = []
    for start, end in ((0x6b3eb, 0x6b423), (0x24c38, 0x24c4c), (0x1a14c, 0x1a1ef), (0x43670, 0x436e0)):
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
    count = 0
    shift_ordinal = 0
    report = {'schema': 'conquer-native-rng-journal-v1', 'status': 'incomplete',
              'full_game_complete': False, 'accepted_callers_complete': False,
              'events': journal.events}
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
                    bound = {0x1624d: 1, 0x16259: 8, 0x43678: 7}.get(outer_pc)
                    if bound is None:
                        raise RecordingError('Unowned inclusive-helper caller; stopped before draw')
                    if words[2] != bound:
                        raise RecordingError('Native helper argument differs from its direct reading')
                    if outer_pc == 0x43678:
                        # FND-STRATEGY-043 settles this caller's actual argument.
                        # RULE-STRATEGY-012 remains disputed; record 7 faithfully
                        # without asserting the selection tables' final meaning.
                        rule = 'RULE-STRATEGY-012'
                    else:
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
                frame = None
            elif kind == 'draw-return':
                if pc != 0x6b412 or frame is None or frame['phase'] != 'raw' or frame['key'] != key:
                    raise RecordingError('Raw return/frame mismatch')
                journal.finish_raw(state(), int(registers.general['eax'], 16))
                frame['phase'] = 'bound'
            else:
                if frame is None or frame['phase'] != 'bound' or frame['bound_key'] != key or frame['bound_pc'] != pc:
                    raise RecordingError('Bounded result/frame mismatch')
                result_register = 'edx' if kind == 'prompt-result' else 'eax'
                if journal.pending['rule'] == 'RULE-PERSON-002':
                    shift_ordinal += 1
                journal.finish_bound(state(), int(registers.general[result_register], 16))
                frame = None
                count += 1
                if count == maximum_draws:
                    report['end_rng_state'] = journal.complete()
                    if replay(journal.events) != report['end_rng_state']:
                        raise RecordingError('Complete journal replay differs')
                    report['status'] = 'bounded-limit-reached'
                    return report
            operation = runtime.agent.continue_(runtime.session.id)
            try:
                runtime.wait(operation, seconds=30)
            except TimeoutError:
                # Keep observing this same live operation; do not issue another
                # continuation simply because its first observation expired.
                runtime.wait(operation, seconds=30)
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
        report['pending_operation'] = journal.pending is not None
        (output / 'native-rng-journal.json').write_text(json.dumps(report, indent=2))
