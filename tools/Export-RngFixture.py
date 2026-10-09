"""Export only legal, address-free facts from the bounded startup journals."""
import argparse
import json
from pathlib import Path
import re
from rng_recording import replay_startup


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--experiment', required=True)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('journals', nargs='+', type=Path)
    args = parser.parse_args()
    if not re.fullmatch(r'EXP-RNG-[0-9]{3}', args.experiment):
        raise ValueError('Invalid experiment identifier')
    runs = []
    for path in args.journals:
        if path.stat().st_size > 1024 * 1024:
            raise ValueError('Journal exceeds the bounded case size')
        journal = json.loads(path.read_text())
        if journal.get('case_status') != 'passed' or journal.get('case') != 'startup-character-shifts':
            raise ValueError('Only a passed prescribed startup case can be exported')
        if journal.get('full_game_complete') is not False:
            raise ValueError('Startup fixture must not claim full-game coverage')
        events = journal['events']
        final = replay_startup(events)
        runs.append({'rng_state': events[0]['seed'], 'events': [],
                     'draws': [{'rule': event['rule'], 'bound': event['bound'], 'result': event['result']}
                               for event in events[1:]],
                     'end_state': [{'global': 'rng_state', 'value': final}]})
    if len({run['rng_state'] for run in runs}) != len(runs):
        raise ValueError('The repeated cases must have different recorded seeds')
    fixture = {'experiment': args.experiment, 'build': 'BLD-GOG-EN',
               'starting_state': {'kind': 'new-game', 'stage': 'startup-seed-return',
                                  'choices': {'MOVIE': 'OFF', 'CREDITS': 'OFF'}},
               'recording_xxh3': None, 'clock': 'draw_sequence', 'inputs': [], 'seeds': [],
               'runs': runs, 'comparison': {'outcome': 'ordered startup shift draws and final rng_state',
                                          'test': 'exact'}}
    args.output.write_text(json.dumps(fixture, indent=2) + '\n')
    print('Exported address-free startup fixture', args.experiment, 'runs', len(runs))


if __name__ == '__main__':
    main()
