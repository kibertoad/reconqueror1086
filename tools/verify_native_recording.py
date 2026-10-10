"""Validate a terminal native diagnostic, without claiming gameplay completeness."""
import argparse
import json
from pathlib import Path

from rng_journal import replay
from rng_recording import RecordingError
from native_event_log import CONTRACT, LOG_FORMAT, outcome
from dinorefurb_dosbox_session import read_event_log, LogRejected


TERMINAL_STATUSES = frozenset({
    'bounded-limit-reached', 'screen-load-entry-reached',
    'screen-load-return-reached', 'screen-target-return-reached',
    'youth-answer-return-reached', 'youth-continue-return-reached',
    'youth-sequence-return-reached',
    'dubbing-return-reached',
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


def verify(directory, expected_status, expected_youth_cycles=None):
    if expected_status not in TERMINAL_STATUSES:
        raise RecordingError('Expected status must name a terminal diagnostic boundary')
    if expected_youth_cycles is not None:
        if type(expected_youth_cycles) is not int or not 1 <= expected_youth_cycles <= 6:
            raise RecordingError('Expected youth cycles must be an integer from one to six')
        cycle_status = ('youth-continue-return-reached' if expected_youth_cycles == 1
                        else 'youth-sequence-return-reached')
        if expected_status != cycle_status and not (expected_status == 'dubbing-return-reached' and expected_youth_cycles == 6):
            raise RecordingError('Expected youth cycles require the corresponding Continue boundary')
    directory = Path(directory)
    report = json.loads(read_bounded(directory / 'native-rng-journal.json'))
    if not isinstance(report, dict) or report.get('schema') not in (
            'conquer-native-rng-journal-v1', 'conquer-native-rng-journal-v2', 'conquer-native-rng-journal-v3'):
        raise RecordingError('Unexpected native journal schema')
    if report.get('status') != expected_status:
        raise RecordingError('Native journal did not reach the requested diagnostic boundary')
    if 'failure' in report or 'diagnostic_failure' in report:
        raise RecordingError('Native journal records a failure')
    if any(report.get(name) is not False for name in PENDING_FIELDS):
        raise RecordingError('Native journal has a pending or unreported operation')
    if report['schema'] == 'conquer-native-rng-journal-v1' and 'pending_dubbing' in report:
        raise RecordingError('Dubbing state requires schema v2')
    if report['schema'] in ('conquer-native-rng-journal-v2', 'conquer-native-rng-journal-v3') and report.get('pending_dubbing') is not False:
        raise RecordingError('Native journal has a pending or unreported dubbing operation')
    if report['schema'] != 'conquer-native-rng-journal-v3' and any(
            field in report for field in ('pending_dubbing_entry', 'dubbing_entry_complete')):
        raise RecordingError('Dubbing entry state requires schema v3')
    if report['schema'] == 'conquer-native-rng-journal-v3':
        if report.get('pending_dubbing_entry') is not False or type(report.get('dubbing_entry_complete')) is not bool:
            raise RecordingError('Native journal has pending or unreported dubbing entry state')
        if expected_youth_cycles == 6 and report['dubbing_entry_complete'] is not True:
            raise RecordingError('Prescribed traversal did not complete dubbing entry input')
    if expected_status == 'dubbing-return-reached' and (
            report['schema'] not in ('conquer-native-rng-journal-v2', 'conquer-native-rng-journal-v3') or expected_youth_cycles != 6):
        raise RecordingError('Dubbing verification requires schema v2 and six prescribed cycles')
    if any(report.get(name) is not False for name in
           ('full_game_complete', 'accepted_callers_complete')):
        raise RecordingError('Diagnostic must not claim full-game or caller completeness')
    if 'event_log_format' in report:
        if report['event_log_format'] != LOG_FORMAT:
            raise RecordingError('Unexpected shared event-log format')
        try:
            log = read_event_log(directory / 'session' / 'events.jsonl', outcome(report))
        except LogRejected as error:
            raise RecordingError(f'Shared event log refused: {error}') from error
        if log.contract.to_json() != CONTRACT.to_json():
            raise RecordingError('Unexpected native outcome contract')
        durable = [dict(event.data, kind=event.kind) for event in log.events]
    else:
        # Historical captures retain their recorded unwrapped transport contract.
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
    result = {'status': expected_status, 'events': len(durable),
              'end_rng_state': final_state, 'full_game_complete': False,
              'accepted_callers_complete': False}
    if expected_youth_cycles is not None:
        completed = report.get('completed_youth_cycles')
        if type(completed) is not int or completed != expected_youth_cycles:
            raise RecordingError('Native journal did not complete the requested youth cycles')
        # FND-UI-022 / RULE-PERSON-004: the sixth Continue replaces youth
        # with dubbing; earlier Continue returns retain the youth screen.
        expected_screen = (11 if expected_status == 'dubbing-return-reached' else
                           6 if expected_youth_cycles == 6 else 3)
        observation = report.get('screen_observation')
        if not isinstance(observation, dict):
            raise RecordingError('Youth cycle endpoint has no screen observation')
        screen = observation.get('screen_id')
        history = observation.get('history')
        if type(screen) is not int or screen != expected_screen or not isinstance(history, list) or \
                len(history) != 5 or type(history[0]) is not int or history[0] != expected_screen:
            raise RecordingError('Youth cycle endpoint screen identity/history differs')
        result.update(completed_youth_cycles=completed, screen_id=screen)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--expect-status', required=True, choices=sorted(TERMINAL_STATUSES))
    parser.add_argument('--expect-youth-cycles', type=int, choices=range(1, 7),
                        help='Require the prescribed cycle count and youth/dubbing endpoint')
    args = parser.parse_args()
    try:
        result = verify(args.directory, args.expect_status, args.expect_youth_cycles)
    except (OSError, ValueError, RecordingError) as error:
        parser.exit(1, f'Native diagnostic verification failed: {error}\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
