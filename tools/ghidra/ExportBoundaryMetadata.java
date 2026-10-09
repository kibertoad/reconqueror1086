// Metadata-only boundary review. No bytes, mnemonics or operands are exported.
// @category CleanRoom
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;

public class ExportBoundaryMetadata extends GhidraScript {
    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length < 3 || !args[1].matches("[0-9a-f]{64}"))
            throw new IllegalArgumentException("New output file, source SHA-256 and addresses required");
        if (!args[1].equalsIgnoreCase(currentProgram.getExecutableSHA256()))
            throw new IllegalArgumentException("Database source identity mismatch");
        StringBuilder result = new StringBuilder("requested\tinstruction_start\tinstruction_size\tflow\n");
        for (int i=2; i<args.length; i++) {
            monitor.checkCancelled();
            Address address = currentProgram.getAddressFactory().getAddress(args[i]);
            if (address==null || !currentProgram.getMemory().contains(address))
                throw new IllegalArgumentException("Unmapped address: "+args[i]);
            Instruction instruction = currentProgram.getListing().getInstructionContaining(address);
            if (instruction==null) throw new IllegalArgumentException("No decoded instruction: "+address);
            result.append(address).append('\t').append(instruction.getMinAddress()).append('\t')
                .append(instruction.getLength()).append('\t').append(instruction.getFlowType()).append('\n');
        }
        Files.writeString(Path.of(args[0]),result,StandardCharsets.UTF_8,StandardOpenOption.CREATE_NEW);
        println("BOUNDARY_EXPORT_COMPLETE addresses="+(args.length-2));
    }
}
