"""Synthetic ownership/failure controls; no original or emulator is used."""
from types import SimpleNamespace
import unittest
from rng_recording import RecordingError
from timer_gated_wait import TimerGatedWait


class Agent:
    def __init__(self):
        self.calls=[]
        self.serial=0
        self.fail=None
        self.override_id=None

    def create_execution_breakpoint(self, session, selector, offset):
        self.calls.append(('create',session,selector,offset))
        if self.fail == 'create':
            raise RuntimeError('create failed')
        self.serial += 1
        return SimpleNamespace(id=self.override_id or f'temporary-{self.serial}')

    def delete_breakpoint(self,session,identity):
        self.calls.append(('delete',session,identity))
        if self.fail == 'delete':
            raise RuntimeError('delete failed')


class TimerWaitTests(unittest.TestCase):
    def setUp(self):
        self.runtime=SimpleNamespace(session=SimpleNamespace(id='owned',state='stopped'),agent=Agent())
        self.hooks={'rng':'draw','seed':'seed','poll':'presentation'}
        self.wait=TimerGatedWait(self.runtime,self.hooks,(8,0x200))

    def arm(self):
        self.wait.arm('poll',(8,0x100),'presentation')

    def test_success_preserves_rng_hooks_and_rearms_original_boundary(self):
        self.arm()
        self.assertTrue(self.wait.pending)
        self.assertEqual(self.hooks,{'rng':'draw','seed':'seed','temporary-1':'timer-gated-wait'})
        restored=self.wait.restore('temporary-1')
        self.assertFalse(self.wait.pending)
        self.assertEqual(self.hooks,{'rng':'draw','seed':'seed',restored:'presentation'})
        self.assertEqual(self.runtime.agent.calls,[
            ('create','owned',8,0x200),('delete','owned','poll'),
            ('create','owned',8,0x100),('delete','owned','temporary-1')])

    def test_running_guest_and_foreign_hook_are_refused_without_rpc(self):
        self.runtime.session.state='running'
        with self.assertRaises(RecordingError):self.arm()
        self.runtime.session.state='stopped'
        with self.assertRaises(RecordingError):self.wait.arm('foreign',(8,0x100),'presentation')
        self.assertEqual(self.runtime.agent.calls,[])

    def test_duplicate_arm_and_foreign_restore_are_refused(self):
        self.arm()
        before=list(self.runtime.agent.calls)
        with self.assertRaises(RecordingError):self.arm()
        with self.assertRaises(RecordingError):self.wait.restore('rng')
        self.assertEqual(self.runtime.agent.calls,before)
        self.assertTrue(self.wait.pending)

    def test_create_failure_keeps_original_poll(self):
        self.runtime.agent.fail='create'
        with self.assertRaises(RuntimeError):self.arm()
        self.assertTrue(self.wait.pending)
        self.assertTrue(self.wait.faulted)
        with self.assertRaises(RecordingError):self.wait.restore('temporary-1')
        self.assertEqual(self.hooks,{'rng':'draw','seed':'seed','poll':'presentation'})

    def test_corrupt_creation_identity_never_removes_an_existing_hook(self):
        self.runtime.agent.override_id='rng'
        with self.assertRaises(RecordingError):self.arm()
        self.assertTrue(self.wait.pending)
        self.assertTrue(self.wait.faulted)
        self.assertEqual(self.hooks,{'rng':'draw','seed':'seed','poll':'presentation'})
        self.assertFalse(any(call[0]=='delete' for call in self.runtime.agent.calls))

    def test_poll_delete_failure_tracks_temporary_hook_for_failed_run_cleanup(self):
        self.runtime.agent.fail='delete'
        with self.assertRaises(RuntimeError):self.arm()
        self.assertTrue(self.wait.pending)
        self.assertEqual(self.hooks['temporary-1'],'timer-gated-wait')
        self.assertEqual(self.hooks['rng'],'draw')

    def test_restore_create_failure_remains_pending(self):
        self.arm()
        self.runtime.agent.fail='create'
        with self.assertRaises(RuntimeError):self.wait.restore('temporary-1')
        self.assertTrue(self.wait.pending)
        self.assertNotIn('poll',self.hooks)

    def test_restore_delete_failure_tracks_both_hooks_and_remains_pending(self):
        self.arm()
        self.runtime.agent.fail='delete'
        with self.assertRaises(RuntimeError):self.wait.restore('temporary-1')
        self.assertTrue(self.wait.pending)
        self.assertEqual(self.hooks['temporary-1'],'timer-gated-wait')
        self.assertEqual(self.hooks['temporary-2'],'presentation')

    def test_invalid_and_identical_boundaries_are_refused(self):
        for address in ((True,1),(8,-1),(0x10000,1),(8,0x100000000),[8,1]):
            with self.subTest(address=address):
                with self.assertRaises(RecordingError):TimerGatedWait(self.runtime,self.hooks,address)
        with self.assertRaises(RecordingError):self.wait.arm('poll',(8,0x200),'presentation')
        self.assertEqual(self.runtime.agent.calls,[])


if __name__=='__main__':unittest.main()
