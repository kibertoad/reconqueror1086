"""Address-free RNG journal and bounded startup recording case.

Evidence: RULE-RNG-001, RULE-PERSON-002, FND-RNG-002/003/004,
FND-PERSON-003. This case is an intermediate recorder check, not full-game
coverage. Unknown callers, interrupt paths and state changes fail closed.
"""
import json
from pathlib import Path


class RecordingError(RuntimeError):
    pass


def integer(value, low, high, label):
    if type(value) is not int or not low <= value <= high:
        raise RecordingError(f'Invalid {label}')
    return value


def replay_startup(events, require_complete=True):
    """Replay the supported startup draw order from the spec, without addresses."""
    if not isinstance(events, list) or not 1 <= len(events) <= 31 or (require_complete and len(events) != 31):
        raise RecordingError('Expected one seed and thirty startup shift draws')
    seed = events[0]
    if set(seed) != {'kind', 'rule', 'seed'} or seed['kind'] != 'seed' or seed['rule'] != 'RULE-RNG-001':
        raise RecordingError('Invalid initial seed event')
    state = integer(seed['seed'], 0, 0xffffffff, 'seed')
    for ordinal, event in enumerate(events[1:]):
        if set(event) != {'kind', 'rule', 'parameter', 'field_index', 'range', 'bound',
                          'result', 'raw_result', 'state_before', 'state_after'}:
            raise RecordingError('Unexpected event field; diagnostics must stay outside the journal')
        parameter, bound = ('negative', 1) if ordinal % 2 == 0 else ('amount', 8)
        integer(event['field_index'], 0, 14, 'field index')
        integer(event['bound'], 0, 0x7fffffff, 'bound')
        if (event['kind'], event['rule'], event['parameter'], event['field_index'],
            event['range'], event['bound']) != ('draw', 'RULE-PERSON-002', parameter,
                                               ordinal // 2, 'inclusive', bound):
            raise RecordingError('Startup owner, parameter, bound or draw ordering diverged')
        for name, high in (('raw_result', 32767), ('result', bound),
                           ('state_before', 0xffffffff), ('state_after', 0xffffffff)):
            integer(event[name], 0, high, name)
        expected = (state * 0x41c64e6d + 0x3039) & 0xffffffff
        raw = (expected >> 16) & 32767
        if (event['state_before'], event['state_after'], event['raw_result'], event['result']) != (
                state, expected, raw, raw % (bound + 1)):
            raise RecordingError(f'RNG replay diverged at draw {ordinal}')
        state = expected
    return state


def canonical_pc(registers, mapping):
    if registers.cpu_mode != 'protected' or int(registers.segments['cs'], 16) != mapping.code.selector:
        raise RecordingError('Unmodelled mode/selector during bounded RNG recording')
    linear = mapping.code.resolve(int(registers.instruction_pointer, 16))
    if not mapping.code_base <= linear < mapping.code_base + mapping.code_size:
        raise RecordingError('Instruction lies outside the verified code object')
    return linear - mapping.code_base + 0x10000


def step_to_return(runtime, mapping, return_address, allowed, maximum):
    for _ in range(maximum):
        registers = runtime.registers()
        pc = canonical_pc(registers, mapping)
        if not any(start <= pc < end for start, end in allowed):
            raise RecordingError('Interrupt or unexpected instruction path needs separate instrumentation')
        runtime.session, registers = runtime.agent.step(runtime.session.id)
        if int(registers.instruction_pointer, 16) == return_address:
            canonical_pc(registers, mapping)
            return registers
    raise RecordingError('Bounded RNG call did not reach its saved return address')


def record_startup_shifts(runtime, mapping, entry_ids, seed, output):
    """Capture all thirty startup shifts; reject every other native RNG caller."""
    events = [{'kind': 'seed', 'rule': 'RULE-RNG-001', 'seed': seed}]
    output = Path(output)
    report = {'schema': 'conquer-rng-journal-v1', 'case': 'startup-character-shifts',
              'case_status': 'incomplete', 'full_game_complete': False,
              'starting_state': {'build': 'BLD-GOG-EN', 'stage': 'startup-seed-return', 'rng_state': seed},
              'events': events}
    state_selector, state_offset = mapping.data_address(0x9e044, 4)
    state_address = runtime.MemoryAddress.segmented(state_selector, state_offset)
    owners = {0x1624d: ('negative', 1), 0x16259: ('amount', 8)}
    try:
        for ordinal in range(30):
            runtime.wait(runtime.agent.continue_(runtime.session.id), seconds=30)
            stop = runtime.session.stop_reason
            if runtime.session.state != 'stopped' or stop.kind != 'breakpoint' or stop.breakpoint_id != entry_ids[0]:
                raise RecordingError('Unexpected seed, exit, diagnostic or stop while recording startup draws')
            registers = runtime.registers()
            if canonical_pc(registers, mapping) != 0x6b3f1:
                raise RecordingError('Native draw stop disagrees with the validated map')
            if any(int(registers.segments[name], 16) != mapping.data.selector for name in ('ds', 'ss')):
                raise RecordingError('Data/stack selector changed during recording')
            stack = runtime.read(runtime.MemoryAddress.segmented(
                mapping.data.selector, int(registers.general['esp'], 16)), 12)
            raw_return, outer_return, bound = (int.from_bytes(stack[i:i + 4], 'little') for i in (0, 4, 8))
            _, helper_return = mapping.code_address(0x24c3d)
            if raw_return != helper_return:
                raise RecordingError('Unowned raw RNG caller; stop before executing the draw')
            caller = outer_return - mapping.code_base + 0x10000
            if caller not in owners:
                raise RecordingError('Unowned inclusive-helper caller; stop before executing the draw')
            parameter, expected_bound = owners[caller]
            field_offset = int(registers.general['esi'], 16)
            if bound != expected_bound or field_offset % 4 or field_offset // 4 != ordinal // 2:
                raise RecordingError('Native attribute index or argument disagrees with supported startup order')
            before = int.from_bytes(runtime.read(state_address, 4), 'little')
            expected_before = seed if not events[1:] else events[-1]['state_after']
            if before != expected_before:
                raise RecordingError('Unrecorded RNG state change before the next draw')
            after_raw = step_to_return(runtime, mapping, raw_return, [(0x6b3eb, 0x6b413)], 32)
            after = int.from_bytes(runtime.read(state_address, 4), 'little')
            raw_result = int(after_raw.general['eax'], 16)
            after_helper = step_to_return(runtime, mapping, outer_return, [(0x24c38, 0x24c4c)], 16)
            if int.from_bytes(runtime.read(state_address, 4), 'little') != after:
                raise RecordingError('Unrecorded RNG state change inside the inclusive helper')
            events.append({'kind': 'draw', 'rule': 'RULE-PERSON-002', 'parameter': parameter,
                           'field_index': ordinal // 2, 'range': 'inclusive', 'bound': bound,
                           'result': int(after_helper.general['eax'], 16), 'raw_result': raw_result,
                           'state_before': before, 'state_after': after})
            replay_startup(events, require_complete=False)
        report['end_rng_state'] = replay_startup(events)
        report['case_status'] = 'passed'
        return report
    except Exception as error:
        report['failure'] = str(error)
        # Guest diagnostics are separate and local-only; none of these addresses
        # or bytes are passed to the address-free event journal.
        try:
            diagnostics = runtime.registers()
            import dataclasses
            (output / 'recording-failure-registers.json').write_text(json.dumps(dataclasses.asdict(diagnostics), indent=2))
            if runtime.session.state == 'stopped':
                with (output / 'recording-failure-memory.bin').open('wb') as memory:
                    for base in range(0, 16 * 1024 * 1024, 65536):
                        memory.write(runtime.read(runtime.MemoryAddress.physical(base), 65536))
        except Exception as diagnostic_error:
            report['diagnostic_failure'] = type(diagnostic_error).__name__
        raise
    finally:
        (output / 'rng-journal.json').write_text(json.dumps(report, indent=2))
