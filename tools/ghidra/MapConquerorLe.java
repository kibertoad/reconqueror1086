// Inventory-only mapping for the fingerprinted BLD-GOG-EN LE payload.
// @category CleanRoom
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.mem.MemoryBlock;
import java.io.ByteArrayInputStream;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.file.Files;
import java.nio.file.Path;
import java.security.MessageDigest;
import java.util.HexFormat;

public class MapConquerorLe extends GhidraScript {
    private static final String SHA = "5d7231758766204ad061e6b82cf2f0e0cbe28899b35d095f13e4aad75c8b79d6";
    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length != 2) throw new IllegalArgumentException("Source executable and documented neutral-entry TSV required.");
        byte[] bytes = Files.readAllBytes(Path.of(args[0]));
        String hash = HexFormat.of().formatHex(MessageDigest.getInstance("SHA-256").digest(bytes));
        if (bytes.length != 919107 || !SHA.equals(hash)) throw new IllegalArgumentException("Wrong BLD-GOG-EN source fingerprint.");
        if (!currentProgram.getLanguageID().toString().equals("x86:LE:32:default"))
            throw new IllegalArgumentException("Use BinaryLoader with x86:LE:32:default.");
        ByteBuffer data = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN);
        int module = 0x26654, header = 0x290fc;
        if (data.getShort(module) != 0x5a4d || module + data.getInt(module + 0x3c) != header
                || data.getShort(header) != 0x454c || data.getInt(header + 0x44) != 2
                || data.getInt(header + 0x28) != 4096)
            throw new IllegalArgumentException("Unexpected bound-module/object layout.");
        int pages = Math.addExact(module, data.getInt(header + 0x80));
        if (pages != 0x4c254) throw new IllegalArgumentException("Unexpected module-relative page mapping.");
        int table = Math.addExact(header, data.getInt(header + 0x40));
        // The analyzer import is disposable. Never run this against an owner's
        // existing project: it replaces the BinaryLoader's whole-file block.
        for (MemoryBlock block : currentProgram.getMemory().getBlocks())
            currentProgram.getMemory().removeBlock(block, monitor);
        for (int i = 0; i < 2; i++) {
            int row = table + i * 24;
            int size = data.getInt(row), base = data.getInt(row + 4), flags = data.getInt(row + 8);
            int firstPage = data.getInt(row + 12), pageCount = data.getInt(row + 16);
            if (size != (i == 0 ? 0x7cb9e : 0x23670) || base != (i == 0 ? 0x10000 : 0x90000)
                    || firstPage != (i == 0 ? 1 : 126) || pageCount != (i == 0 ? 125 : 24))
                throw new IllegalArgumentException("Unexpected object range.");
            int offset = Math.addExact(pages, Math.multiplyExact(firstPage - 1, 4096));
            int stored = i == 0 ? size : (pageCount - 1) * 4096 + data.getInt(header + 0x2c);
            if (offset < 0 || stored > bytes.length - offset) throw new IllegalArgumentException("Truncated object bytes.");
            MemoryBlock block = currentProgram.getMemory().createInitializedBlock("LE_object_" + (i + 1),
                toAddr(base), new ByteArrayInputStream(bytes, offset, stored), stored, monitor, false);
            block.setRead(true);
            block.setWrite((flags & 2) != 0);
            block.setExecute((flags & 4) != 0);
            if (stored < size) {
                MemoryBlock bss = currentProgram.getMemory().createUninitializedBlock("LE_zero_fill_" + (i + 1),
                    toAddr(base + stored), size - stored, false);
                bss.setRead(true);
                bss.setWrite((flags & 2) != 0);
                bss.setExecute(false);
            }
        }
        int entryObject = data.getInt(header + 0x18), entryOffset = data.getInt(header + 0x1c);
        if (entryObject != 1 || entryOffset != 0x60e64) throw new IllegalArgumentException("Unexpected entry point.");
        Address entry = toAddr(0x10000L + entryOffset);
        currentProgram.getSymbolTable().addExternalEntryPoint(entry);
        disassemble(entry);
        createFunction(entry, null);
        for (String line : Files.readAllLines(Path.of(args[1]))) {
            if (line.isBlank()) continue;
            long value = Long.parseUnsignedLong(line, 16);
            if (value < 0x10000 || value >= 0x8cb9e) throw new IllegalArgumentException("Documented entry outside code object.");
            Address address = toAddr(value);
            disassemble(address);
            if (getFunctionAt(address) == null) createFunction(address, null);
        }
        println("Mapped fingerprinted LE objects; relocation operands remain raw, so indirect targets require separate review.");
    }
}
