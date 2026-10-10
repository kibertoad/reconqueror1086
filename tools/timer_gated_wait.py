"""Owned breakpoint orchestration; readiness and boundary proof belong to callers.

No guest field is written. Other evidence/RNG breakpoints remain installed.
After any RPC failure the caller must fail the run and close its owned session;
pending remains true when a temporary breakpoint may still exist.
"""
from rng_recording import RecordingError


def _address(value):
    if (not isinstance(value, tuple) or len(value) != 2 or
            any(type(item) is not int for item in value) or
            not 0 <= value[0] <= 0xffff or not 0 <= value[1] <= 0xffffffff):
        raise RecordingError('Timer wait requires a bounded selector/offset pair')
    return value


class TimerGatedWait:
    def __init__(self, runtime, hooks, timer_address):
        self.runtime, self.hooks = runtime, hooks
        self.timer_address = _address(timer_address)
        self.active = None
        self.faulted = False

    @property
    def pending(self):
        return self.active is not None

    def _stopped(self):
        if self.faulted:
            raise RecordingError('Failed timer wait must close its owned session')
        if self.runtime.session.state != 'stopped':
            raise RecordingError('Timer wait requires the stopped owned session')

    def _rpc(self, operation, *arguments):
        try:
            return operation(*arguments)
        except BaseException:
            self.faulted = True
            raise

    def _new_hook(self, address):
        hook = self._rpc(self.runtime.agent.create_execution_breakpoint,
                         self.runtime.session.id, *address)
        if type(hook.id) is not str or not hook.id or hook.id in self.hooks:
            self.faulted = True
            raise RecordingError('Breakpoint identity is not a new owned identity')
        return hook

    def arm(self, poll_id, poll_address, poll_kind):
        """Caller has verified that only timer readiness currently blocks input."""
        self._stopped()
        poll_address = _address(poll_address)
        if self.pending or self.hooks.get(poll_id) != poll_kind or not poll_kind:
            raise RecordingError('Timer wait requires an inactive, owned poll breakpoint')
        if poll_address == self.timer_address:
            raise RecordingError('Timer and poll boundaries must differ')
        # A transport failure can follow successful guest-side creation.
        # Mark pending before the RPC, even when its identity is not known yet.
        self.active = dict(timer_id=None, poll_address=poll_address,
                           poll_kind=poll_kind)
        hook = self._new_hook(self.timer_address)
        self.hooks[hook.id] = 'timer-gated-wait'
        self.active['timer_id'] = hook.id
        # Track the new owned hook before any mutation that could fail.
        self._rpc(self.runtime.agent.delete_breakpoint, self.runtime.session.id, poll_id)
        del self.hooks[poll_id]

    def restore(self, timer_id):
        """Caller observed readiness at this exact owned timer boundary."""
        self._stopped()
        if (not self.pending or timer_id != self.active['timer_id'] or
                self.hooks.get(timer_id) != 'timer-gated-wait'):
            raise RecordingError('Timer wait restore requires its active owned hook')
        hook = self._new_hook(self.active['poll_address'])
        self.hooks[hook.id] = self.active['poll_kind']
        self._rpc(self.runtime.agent.delete_breakpoint, self.runtime.session.id, timer_id)
        del self.hooks[timer_id]
        self.active = None
        return hook.id
