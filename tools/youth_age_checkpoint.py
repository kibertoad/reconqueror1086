"""Read-only row-zero AGE observation through FND-PERSON-001/003's table."""
from rng_recording import RecordingError


def read_youth_age(runtime, mapping):
    selector, offset = mapping.data_address(0x9a650, 4)  # FND-PERSON-003
    table = int.from_bytes(runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4), 'little')

    def read(pointer, length):
        if pointer % 4 or not 0 < pointer <= 16 * 1024 * 1024 - length:
            raise RecordingError('Character table pointer outside configured guest RAM')
        return runtime.read(runtime.MemoryAddress.segmented(mapping.data.selector, pointer), length)

    header = read(table, 16)
    attributes = int.from_bytes(header[8:12], 'little')
    characters = int.from_bytes(header[12:16], 'little')
    if not 19 <= attributes <= 65536 or not 1 <= characters <= 65536:
        raise RecordingError('Character table counts cannot locate row-zero AGE')
    rows = int.from_bytes(header[:4], 'little')
    row = int.from_bytes(read(rows, 4), 'little')
    record = read(row, 84)
    values = int.from_bytes(record[80:84], 'little')
    # Validate the values pointer as well as the selected field address.
    read(values, 4)
    return int.from_bytes(read(values + 18 * 4, 4), 'little', signed=True)


def validate_youth_ages(rows, completed=None):
    """RULE-PERSON-004/007: validate the observed fresh March traversal prefix."""
    expected = [('youth-screen-return', 0, 13)]
    for cycle in range(5):
        expected.extend((boundary, cycle, age) for boundary, age in (
            ('answer-entry', 13 + cycle), ('answer-return', 14 + cycle),
            ('continue-entry', 14 + cycle), ('continue-return', 14 + cycle)))
    if not isinstance(rows, list) or not rows or len(rows) > len(expected):
        raise RecordingError('Youth AGE boundary sequence is missing or too long')
    for row, target in zip(rows, expected):
        if not isinstance(row, dict) or any(type(row.get(field)) is not int for field in
                ('completed_cycles', 'age', 'rng_state')) or not 0 <= row['rng_state'] <= 0xffffffff or \
                (row.get('boundary'), row['completed_cycles'], row['age']) != target:
            raise RecordingError('Youth AGE boundary sequence differs from prescribed March traversal')
    if completed is not None and (type(completed) is not int or not 0 <= completed <= 5 or
                                  len(rows) != 1 + 4 * completed):
        raise RecordingError('Youth AGE boundary sequence does not cover completed cycles')
