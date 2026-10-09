"""Bounded direct-call lead discovery; unresolved control flow stays explicit."""
from capstone import Cs, CS_ARCH_X86, CS_MODE_32, CS_OP_IMM


def walk_calls(code, base, entry, ranges, targets, budget=100000):
    if not isinstance(code, bytes) or not code or type(budget) is not int or budget < 1:
        raise ValueError('Invalid code or instruction budget')
    spans = sorted(ranges)
    if not spans or any(not base <= a < b <= base + len(code) for a, b in spans):
        raise ValueError('Body range exceeds code object')
    if any(left[1] > right[0] for left, right in zip(spans, spans[1:])):
        raise ValueError('Overlapping body ranges')
    def containing(address):
        return next((span for span in spans if span[0] <= address < span[1]), None)
    if not containing(entry):
        raise ValueError('Entry lies outside its body ranges')
    decoder = Cs(CS_ARCH_X86, CS_MODE_32)
    decoder.detail = True
    pending, seen, occupied, calls, unresolved = [entry], set(), {}, {}, []
    while pending:
        address = pending.pop()
        if address in seen:
            continue
        if len(seen) >= budget:
            raise ValueError('Instruction budget exhausted')
        seen.add(address)
        span = containing(address)
        if not span:
            unresolved.append({'address': address, 'reason': 'edge outside body ranges'})
            continue
        if address in occupied and occupied[address] != address:
            unresolved.append({'address': address, 'reason': 'edge into decoded instruction'})
            continue
        instructions = list(decoder.disasm(code[address - base:span[1] - base], address, count=1))
        if not instructions:
            unresolved.append({'address': address, 'reason': 'undecodable instruction'})
            continue
        instruction = instructions[0]
        if any(byte in occupied and occupied[byte] != address
               for byte in range(address, address + instruction.size)):
            unresolved.append({'address': address, 'reason': 'overlapping instruction interpretation'})
            continue
        for byte in range(address, address + instruction.size):
            occupied[byte] = address
        mnemonic = instruction.mnemonic
        direct = instruction.operands[0].imm if instruction.operands and instruction.operands[0].type == CS_OP_IMM else None
        if mnemonic == 'call':
            if direct in targets:
                calls[address] = targets[direct]
            elif direct is None:
                unresolved.append({'address': address, 'reason': 'indirect call target not followed'})
        if mnemonic in ('ret', 'retf', 'iret', 'iretd', 'hlt', 'ud2'):
            continue
        branch = mnemonic.startswith('j') or mnemonic.startswith('loop')
        if branch:
            if direct is None:
                unresolved.append({'address': address, 'reason': 'indirect branch target not followed'})
            else:
                pending.append(direct & 0xffffffff)
            if mnemonic == 'jmp':
                continue
        following = address + instruction.size
        if containing(following):
            pending.append(following)
        else:
            unresolved.append({'address': following, 'reason': 'fallthrough outside body ranges'})
    return calls, unresolved
