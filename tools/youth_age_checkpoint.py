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
