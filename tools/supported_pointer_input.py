"""Controlled queue input, using FMT-BATTLE-002 and FND-BATTLE-023 only.

This models a supported-state experiment, not physical mouse delivery.
The caller must first establish an appropriate observable input boundary.
"""
import hashlib
import struct
from rng_recording import RecordingError


# FND-BATTLE-023 and the corresponding glossary terms locate these fields.
FIELDS = {'pointer_events': 0xb0530, 'pointer_count': 0x9df38,
          'pointer_clock': 0xa6830, 'primary_release_time': 0x9df44,
          'long_press_limit': 0x9df4c, 'double_click_limit': 0x9df50}


def primary_click_ready(runtime, mapping):
    """Observe readiness without changing queue or timer fields (FND-BATTLE-023)."""
    if runtime.session.state != 'stopped':
        raise RecordingError('Pointer readiness requires a stopped owned guest')
    def word(name):
        selector, offset = mapping.data_address(FIELDS[name], 4)
        return int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4), 'little')
    return (word('pointer_count') == 0 and word('long_press_limit') != 0 and
            ((word('pointer_clock') - word('primary_release_time')) & 0xffffffff) >= word('double_click_limit'))


def queue_primary_click(runtime, mapping, x=0, y=0):
    if runtime.session.state != 'stopped':
        raise RecordingError('Supported pointer input requires a stopped owned guest')
    registers = runtime.registers()
    if registers.cpu_mode != 'protected' or any(
            int(registers.segments[name], 16) != mapping.data.selector for name in ('ds', 'ss')):
        raise RecordingError('Supported pointer input requires the verified data/stack selectors')
    if any(type(value) is not int or not -2147483648 <= value <= 2147483647 for value in (x, y)):
        raise RecordingError('Pointer coordinates must be signed 32-bit integers')
    def address(name, length, displacement=0):
        selector, offset = mapping.data_address(FIELDS[name] + displacement, length)
        return runtime.MemoryAddress.segmented(selector, offset)
    def word(name):
        return int.from_bytes(runtime.read(address(name, 4), 4), 'little')
    count_address = address('pointer_count', 4)
    old_count = runtime.read(count_address, 4)
    if int.from_bytes(old_count, 'little') != 0:
        raise RecordingError('Pointer queue must be empty before controlled input')
    clock, release = word('pointer_clock'), word('primary_release_time')
    long_limit, double_limit = word('long_press_limit'), word('double_click_limit')
    if long_limit == 0 or ((clock - release) & 0xffffffff) < double_limit:
        raise RecordingError('Observed pointer timing cannot produce the prescribed short click')
    queue_address = address('pointer_events', 40)
    old_queue = runtime.read(queue_address, 40)
    expected = bytearray(old_queue)
    for displacement, kind in ((0, 0), (20, 1)):
        data = struct.pack('<Iiii', clock, x, y, kind)
        expected[displacement:displacement + 16] = data
        runtime.agent.write_memory(runtime.session.id, address('pointer_events', 16, displacement), data,
                                   expected_sha256=hashlib.sha256(old_queue[displacement:displacement + 16]).hexdigest())
    # Publish only after both events are written; unk_10 in each record is untouched.
    runtime.agent.write_memory(runtime.session.id, count_address, struct.pack('<I', 2),
                               expected_sha256=hashlib.sha256(old_count).hexdigest())
    if runtime.read(queue_address, 40) != bytes(expected) or word('pointer_count') != 2:
        raise RecordingError('Supported pointer input readback differs from the prescribed fields')
    return {'action': 'primary-short-click', 'x': x, 'y': y, 'time': clock, 'event_count': 2}
