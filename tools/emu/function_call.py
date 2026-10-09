"""Isolated Unicorn calls. No OS, timer, input, video or audio is emulated."""
import struct
import capstone
import unicorn
from unicorn import x86_const as reg


class CallError(RuntimeError):
    pass


def call(objects, entry, arguments=(), writes=(), instruction_limit=100000):
    if not 1 <= instruction_limit <= 1000000:
        raise ValueError('Instruction limit must be between 1 and 1000000')
    cpu = unicorn.Uc(unicorn.UC_ARCH_X86, unicorn.UC_MODE_32)
    stack, stack_size, stop = 0x2000000, 0x10000, 0x3000000
    spans = []
    for obj in objects:
        base, size = obj['base'], obj['size']
        if base % 4096 or size <= 0 or size > 16 * 1024 * 1024:
            raise CallError('Invalid object mapping')
        cpu.mem_map(base, (size + 4095) & ~4095)
        cpu.mem_write(base, bytes(obj['image']))
        permissions = unicorn.UC_PROT_READ
        if obj['flags'] & 2:
            permissions |= unicorn.UC_PROT_WRITE
        if obj['flags'] & 4:
            permissions |= unicorn.UC_PROT_EXEC
        cpu.mem_protect(base, (size + 4095) & ~4095, permissions)
        spans.append((base, base + size))
    if not any(o['flags'] & 4 and o['base'] <= entry < o['base'] + o['size'] for o in objects):
        raise CallError('Entry outside executable object')
    cpu.mem_map(stack, stack_size)
    cpu.mem_map(stop, 4096, unicorn.UC_PROT_READ | unicorn.UC_PROT_EXEC)
    spans.extend([(stack, stack + stack_size)])
    for address, data in writes:
        if not any(o['flags'] & 2 and o['base'] <= address and address + len(data) <= o['base'] + o['size']
                   for o in objects):
            raise CallError('Initial write outside writable state object')
        cpu.mem_write(address, data)
    if len(arguments) > 64:
        raise CallError('Too many arguments')
    sp = stack + stack_size - 4 * (len(arguments) + 1)
    cpu.mem_write(sp, struct.pack('<' + 'I' * (len(arguments) + 1), stop,
                                  *[value & 0xffffffff for value in arguments]))
    cpu.reg_write(reg.UC_X86_REG_ESP, sp)
    cpu.reg_write(reg.UC_X86_REG_EFLAGS, 2)
    decoder = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_32)
    trace, changed = [], set()

    def instruction(uc, address, size, _):
        if not any(a <= address and address + size <= b for a, b in spans):
            raise CallError(f'Instruction outside loaded image at {address:#x}')
        item = next(decoder.disasm(bytes(uc.mem_read(address, size)), address), None)
        if item is None:
            raise CallError(f'Undecodable instruction at {address:#x}')
        mnemonic = item.mnemonic.split()[-1]
        if mnemonic in ('in', 'out', 'insb', 'insw', 'insd', 'outsb', 'outsw', 'outsd',
                        'hlt', 'syscall', 'sysenter', 'rdtsc', 'rdtscp'):
            raise CallError(f'Unmodelled hardware/service instruction {mnemonic} at {address:#x}')
        trace.append(address)

    def interrupt(uc, number, _):
        raise CallError(f'Unmodelled interrupt {number:#x} at {uc.reg_read(reg.UC_X86_REG_EIP):#x}')

    def memory(uc, access, address, size, value, _):
        if not any(a <= address and address + size <= b for a, b in spans):
            raise CallError(f'Memory access outside loaded image at {address:#x}, size {size}')
        if access == unicorn.UC_MEM_WRITE:
            changed.update(range(address, address + size))

    cpu.hook_add(unicorn.UC_HOOK_CODE, instruction)
    cpu.hook_add(unicorn.UC_HOOK_INTR, interrupt)
    cpu.hook_add(unicorn.UC_HOOK_MEM_READ | unicorn.UC_HOOK_MEM_WRITE, memory)
    try:
        cpu.emu_start(entry, stop, count=instruction_limit)
    except unicorn.UcError as error:
        raise CallError(f'Unicorn stopped at {cpu.reg_read(reg.UC_X86_REG_EIP):#x}: {error}') from error
    if cpu.reg_read(reg.UC_X86_REG_EIP) != stop:
        raise CallError('Instruction budget exhausted before return')
    return {'result': cpu.reg_read(reg.UC_X86_REG_EAX), 'trace': trace,
            'writes': {a: bytes(cpu.mem_read(a, 1))[0] for a in sorted(changed)},
            'read': lambda address, size: bytes(cpu.mem_read(address, size))}
