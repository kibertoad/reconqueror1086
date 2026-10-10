"""Guarded input at the two animations-disabled entry waits (FND-UI-026)."""
from rng_recording import RecordingError
from supported_pointer_input import FIELDS, primary_click_ready, queue_primary_click


POINTS = {0x19a8c: 'entry', 0x19bbf: 'text-wait', 0x19bcc: 'text-accepted',
          0x19c51: 'briefing-wait', 0x19c5e: 'briefing-accepted', 0x19c61: 'return'}


class DubbingEntryInput:
    def __init__(self):
        self.key = None
        self.phase = 'entry'
        self.queued = False
        self.completed = False

    def observe(self, runtime, mapping, pc, key, eax, request, rng_state):
        """Return a queued click, or None; never changes RNG or timing fields."""
        boundary = POINTS.get(pc)
        if boundary is None or request != {'screen': 6, 'draw': 0, 'mode': 1}:
            raise RecordingError('Dubbing presentation requires its prescribed screen request')
        if boundary == 'entry':
            if self.key is not None or self.completed or self.phase != 'entry':
                raise RecordingError('Repeated dubbing presentation entry')
            selector, offset = mapping.data_address(0x9adb8, 4)  # FND-SOUND-004
            if runtime.read(runtime.MemoryAddress.segmented(selector, offset), 4) != bytes(4):
                raise RecordingError('Dubbing presentation input requires animations disabled')
            self.key = key
            self.phase = 'text-wait'
            return None
        expected_key = self.key if boundary == 'return' else (
            (self.key[0], self.key[1] - 12) if self.key is not None else None)
        expected_phase = boundary.replace('-accepted', '-wait')
        if self.key is None or key != expected_key or expected_phase != self.phase:
            raise RecordingError('Dubbing presentation boundary/frame/order mismatch')
        if boundary.endswith('-wait'):
            if self.queued or not primary_click_ready(runtime, mapping):
                return None
            before = rng_state()
            if type(before) is not int:
                raise RecordingError('Dubbing presentation requires a recorded RNG state')
            click = queue_primary_click(runtime, mapping, x=10, y=10)
            if rng_state() != before:
                raise RecordingError('Dubbing presentation input changed RNG state')
            self.queued = True
            return dict(click, screen=6, stage=boundary)
        if boundary.endswith('-accepted'):
            selector, offset = mapping.data_address(FIELDS['pointer_count'], 4)  # FMT-BATTLE-002
            if not self.queued or eax != 1 or runtime.read(
                    runtime.MemoryAddress.segmented(selector, offset), 4) != bytes(4):
                raise RecordingError('Dubbing presentation input was not accepted and drained')
            self.queued = False
            self.phase = 'briefing-wait' if boundary == 'text-accepted' else 'return'
            return None
        self.completed = True
        self.key = None
        self.phase = 'completed'
        return None
