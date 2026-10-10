"""Validate a terminal native diagnostic, without claiming gameplay completeness."""
import argparse
import json
from pathlib import Path

from rng_journal import replay
from rng_recording import RecordingError


TERMINAL_STATUSES = frozenset({
    'bounded-limit-reached', 'screen-load-entry-reached',
    'screen-load-return-reached', 'screen-target-return-reached',
    'youth-answer-return-reached', 'youth-continue-return-reached',
    'youth-sequence-return-reached',
})
PENDING_FIELDS = (
    'pending_operation', 'pending_screen_load', 'pending_archive_extraction',
    'pending_youth_answer', 'pending_youth_continue',
)
MAX_BYTES = 256 * 1024 * 1024


def read_bounded(path):
    with path.open('rb') as stream:
        content = stream.read(MAX_BYTES + 1)
    if len(content) > MAX_BYTES:
        raise RecordingError('Recording file exceeds validation size limit')
    return content.decode('utf-8')


def verify(directory, expected_status):
    if expected_status not in TERMINAL_STATUSES:
        raise RecordingError('Expected status must name a terminal diagnostic boundary')
    directory = Path(directory)
    report = json.loads(read_bounded(directory / 'native-rng-journal.json'))
    if not isinstance(report, dict) or report.get('schema') != 'conquer-native-rng-journal-v1':
        raise RecordingError('Unexpected native journal schema')
    if report.get('status') != expected_status:
        raise RecordingError('Native journal did not reach the requested diagnostic boundary')
    if 'failure' in report or 'diagnostic_failure' in report:
        raise RecordingError('Native journal records a failure')
    if any(report.get(name) is not False for name in PENDING_FIELDS):
        raise RecordingError('Native journal has a pending or unreported operation')
    if any(report.get(name) is not False for name in
           ('full_game_complete', 'accepted_callers_complete')):
        raise RecordingError('Diagnostic must not claim full-game or caller completeness')
    durable = [json.loads(line) for line in
               read_bounded(directory / 'native-rng-events.jsonl').splitlines()]
    if durable != report.get('events'):
        raise RecordingError('Durable events differ from native journal order or contents')
    # RULE-RNG-001: numeric replay checks recorded state transitions/reductions;
    # this is not replay against the rebuild or proof of every original draw.
    final_state = replay(durable)
    # Python equality treats True and 1 (and integral floats) alike; validate
    # the journal's event types independently as well as comparing the lists.
    if replay(report['events']) != final_state:
        raise RecordingError('Native journal replay differs from durable events')
    if type(report.get('end_rng_state')) is not int or report['end_rng_state'] != final_state:
        raise RecordingError('Native journal final state differs from numeric replay')
    return {'status': expected_status, 'events': len(durable),
            'end_rng_state': final_state, 'full_game_complete': False,
            'accepted_callers_complete': False}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--expect-status', required=True, choices=sorted(TERMINAL_STATUSES))
    args = parser.parse_args()
    try:
        result = verify(args.directory, args.expect_status)
    except (OSError, ValueError, RecordingError) as error:
        parser.exit(1, f'Native diagnostic verification failed: {error}\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
