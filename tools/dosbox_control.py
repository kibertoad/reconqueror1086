"""Serialized loopback client for DOSBox-X's documented debugger channel.

DOSBox-X is the TCP client; this controller listens before it starts. This
module does not start, attach to or stop any emulator or game on its own.
"""
import socket


class DebuggerError(RuntimeError):
    pass


class DebuggerControl:
    def __init__(self, port=0, timeout=15):
        self.listener = socket.socket()
        self.listener.bind(('127.0.0.1', port))
        self.listener.listen(1)
        self.listener.settimeout(timeout)
        self.port = self.listener.getsockname()[1]
        self.connection = None
        self.stream = None
        self.sequence = 0

    def accept(self):
        self.connection, _ = self.listener.accept()
        self.connection.settimeout(self.listener.gettimeout())
        self.stream = self.connection.makefile('rb')

    def request(self, command):
        if '\n' in command or '\r' in command:
            raise ValueError('Control request must occupy one line')
        if self.stream is None:
            raise DebuggerError('No DOSBox-X connection')
        self.sequence += 1
        identity = str(self.sequence)
        self.connection.sendall(f'REQ {identity} {command}\n'.encode('ascii'))
        first = self.stream.readline(65537).rstrip(b'\r\n')
        if first not in (f'BEGIN {identity} OK'.encode(), f'BEGIN {identity} ERR'.encode()):
            raise DebuggerError('Malformed debugger response framing')
        lines, length = [], 0
        while True:
            line = self.stream.readline(65537)
            if not line or len(line) > 65536:
                raise DebuggerError('Truncated or oversized debugger response')
            if line.rstrip(b'\r\n') == f'END {identity}'.encode():
                break
            length += len(line)
            if length > 4 * 1024 * 1024:
                raise DebuggerError('Debugger response exceeds limit')
            lines.append(line.decode('utf-8', errors='replace').rstrip('\r\n'))
        if first.endswith(b' ERR'):
            raise DebuggerError('\n'.join(lines))
        return '\n'.join(lines)

    def execute(self, command):
        return self.request('EXEC ' + command)

    def close(self):
        if self.stream is not None:
            self.stream.close()
        if self.connection is not None:
            self.connection.close()
        self.listener.close()

    def __enter__(self):
        return self

    def __exit__(self, *_):
        self.close()
