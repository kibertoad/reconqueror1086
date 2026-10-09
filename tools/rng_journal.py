"""Address-free recorder state machine, separate from native breakpoint transport.

RULE-RNG-001 defines the recurrence and reductions. Callers must supply an
evidence-backed owner; this module never infers a rule from an address.
"""
import re
from rng_recording import RecordingError, integer


def reduced(raw, reduction, bound):
    integer(raw, 0, 32767, 'raw result')
    integer(bound, 0, 0x7fffffff, 'bound')
    if reduction == 'inclusive':
        return raw % (bound + 1)
    if reduction == 'scaled' and bound > 0:
        return ((raw * bound) & 0xffffffff) >> 15
    if reduction == 'remainder' and bound > 0:
        return raw % bound
    if reduction == 'raw' and bound == 32768:
        return raw
    raise RecordingError('Unsupported reduction or bound')


def owner(rule):
    if not isinstance(rule, str) or not re.fullmatch(r'RULE-[A-Z]+-[0-9]{3}', rule):
        raise RecordingError('A named rule owner is required')
    return rule


class Journal:
    def __init__(self):
        self.events = []
        self.state = None
        self.pending = None

    def begin_seed(self, rule, seed, observed):
        if self.pending is not None:
            raise RecordingError('Reentrant RNG state write requires separate evidence')
        integer(observed, 0, 0xffffffff, 'observed state')
        if self.state is not None and observed != self.state:
            raise RecordingError('Unrecorded state change before seed')
        self.pending = {'kind': 'seed', 'rule': owner(rule),
                        'seed': integer(seed, 0, 0xffffffff, 'seed')}

    def finish_seed(self, observed):
        if self.pending is None or self.pending['kind'] != 'seed':
            raise RecordingError('Seed return without a matching entry')
        integer(observed, 0, 0xffffffff, 'observed seed state')
        if observed != self.pending['seed']:
            raise RecordingError('Native seed store diverged')
        self.state = observed
        self.events.append(self.pending)
        self.pending = None

    def begin_draw(self, rule, reduction, bound, observed):
        if self.pending is not None:
            raise RecordingError('Reentrant RNG state write requires separate evidence')
        if self.state is None:
            raise RecordingError('Draw before the initial recorded seed')
        integer(observed, 0, 0xffffffff, 'observed draw state')
        if observed != self.state:
            raise RecordingError('Unrecorded state change before draw')
        reduced(0, reduction, bound)
        self.pending = {'kind': 'draw', 'rule': owner(rule), 'reduction': reduction,
                        'bound': bound, 'state_before': observed}

    def finish_raw(self, observed, result):
        event = self.pending
        if event is None or event['kind'] != 'draw' or 'raw_result' in event:
            raise RecordingError('Raw return without a matching unfinished draw')
        integer(observed, 0, 0xffffffff, 'observed state')
        integer(result, 0, 32767, 'native raw result')
        expected = (event['state_before'] * 0x41c64e6d + 0x3039) & 0xffffffff
        if (observed, result) != (expected, (expected >> 16) & 32767):
            raise RecordingError('Native raw result/state diverged')
        event.update(state_after=observed, raw_result=result)
        self.state = observed

    def finish_bound(self, observed, result):
        event = self.pending
        if event is None or 'raw_result' not in event:
            raise RecordingError('Bounded return without a verified raw draw')
        integer(observed, 0, 0xffffffff, 'observed state')
        integer(result, 0, 0xffffffff, 'native bounded result')
        if observed != self.state:
            raise RecordingError('Unrecorded state write inside reduction')
        if result != reduced(event['raw_result'], event['reduction'], event['bound']):
            raise RecordingError('Native bounded result diverged')
        event['result'] = result
        self.events.append(event)
        self.pending = None

    def complete(self):
        if self.pending is not None or self.state is None:
            raise RecordingError('Recording ends with an incomplete operation or no seed')
        return self.state


def replay(events):
    if not isinstance(events, list) or not 1 <= len(events) <= 1000000:
        raise RecordingError('Invalid journal event count')
    journal = Journal()
    for event in events:
        if not isinstance(event, dict):
            raise RecordingError('Invalid journal event')
        if event.get('kind') == 'seed' and set(event) == {'kind', 'rule', 'seed'}:
            journal.begin_seed(event['rule'], event['seed'], journal.state or 0)
            journal.finish_seed(event['seed'])
        elif event.get('kind') == 'draw' and set(event) == {
                'kind', 'rule', 'reduction', 'bound', 'state_before',
                'state_after', 'raw_result', 'result'}:
            journal.begin_draw(event['rule'], event['reduction'], event['bound'], event['state_before'])
            journal.finish_raw(event['state_after'], event['raw_result'])
            journal.finish_bound(event['state_after'], event['result'])
        else:
            raise RecordingError('Unexpected journal fields; addresses and diagnostics are forbidden')
    return journal.complete()
