"""Conqueror's event schemas and outcome meaning for the shared log transport."""
import hashlib
import json
from dinorefurb_dosbox_session import EventSchema, OutcomeContract, EventLogSettings, LOG_FORMAT


PENDING_FIELDS = (
    'pending_operation', 'pending_screen_load', 'pending_archive_extraction',
    'pending_youth_answer', 'pending_youth_continue',
)
CONTRACT = OutcomeContract('conquer-native-rng-diagnostic', 1, {
    'schema': 'string', 'status': 'string', 'end_rng_state': ('integer', 'null'),
    'completed_youth_cycles': 'integer',
    **{field: 'boolean' for field in (*PENDING_FIELDS, 'pending_dubbing',
       'pending_dubbing_entry', 'dubbing_entry_complete', 'full_game_complete',
       'accepted_callers_complete')},
})
AGE_CONTRACT = OutcomeContract('conquer-native-rng-diagnostic', 2, {
    **CONTRACT.to_json()['fields'], 'youth_age_sha256': 'string',
})
UPDATE_CONTRACT = OutcomeContract('conquer-native-rng-diagnostic', 3, {
    **AGE_CONTRACT.to_json()['fields'], 'dubbing_trigger': 'string',
})
SCHEMAS = (
    EventSchema('seed', {'rule': 'string', 'seed': 'integer'}),
    EventSchema('draw', {'rule': 'string', 'reduction': 'string', 'bound': 'integer',
        'state_before': 'integer', 'state_after': 'integer', 'raw_result': 'integer',
        'result': 'integer'}),
)


def settings(modules, age_checked=False, update_checked=False):
    return EventLogSettings(UPDATE_CONTRACT if update_checked else AGE_CONTRACT if age_checked else CONTRACT, SCHEMAS, tuple(modules))


def outcome(report):
    """Absent presentation fields mean not applicable only under older journal schemas."""
    result = {field: report[field] for field in (*PENDING_FIELDS, 'schema', 'status',
              'full_game_complete', 'accepted_callers_complete')}
    result.update(end_rng_state=report.get('end_rng_state'),
                  completed_youth_cycles=report.get('completed_youth_cycles', 0))
    presentation = report['schema'] in ('conquer-native-rng-journal-v3', 'conquer-native-rng-journal-v4', 'conquer-native-rng-journal-v5')
    dubbing = report['schema'] in ('conquer-native-rng-journal-v2', 'conquer-native-rng-journal-v3', 'conquer-native-rng-journal-v4', 'conquer-native-rng-journal-v5')
    result['pending_dubbing'] = report['pending_dubbing'] if dubbing else False
    result['pending_dubbing_entry'] = report['pending_dubbing_entry'] if presentation else False
    result['dubbing_entry_complete'] = report['dubbing_entry_complete'] if presentation else False
    if report['schema'] in ('conquer-native-rng-journal-v4', 'conquer-native-rng-journal-v5'):
        result['youth_age_sha256'] = hashlib.sha256(json.dumps(
            report.get('youth_ages', []), sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if report['schema'] == 'conquer-native-rng-journal-v5':
        result['dubbing_trigger'] = report.get('dubbing_trigger', 'not-reached')
    return result


class EventLog:
    """Delegate durability and envelope checks to the owned session's log."""
    def __init__(self, runtime):
        self.session = runtime.owned

    def append(self, event):
        self.session.log_event(event['kind'], {key: value for key, value in event.items() if key != 'kind'})

    def finish(self, report):
        values = outcome(report)
        if 'failure' in report or 'diagnostic_failure' in report:
            # A transport/write refusal may already have ended the package log.
            if self.session.run_failure is None:
                self.session.fail_log(report.get('failure', 'Diagnostic failure'), values)
        else:
            self.session.finish_log(values)
