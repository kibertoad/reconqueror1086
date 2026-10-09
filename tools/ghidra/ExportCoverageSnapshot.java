// Metadata only: body ranges and the executable-region denominator audit.
// Run headless with -readOnly -noanalysis; original names and bytes are never exported.
// @category CleanRoom
import ghidra.app.script.GhidraScript;
import ghidra.framework.Application;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.mem.MemoryBlock;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

public class ExportCoverageSnapshot extends GhidraScript {
    private String spans(AddressSetView set) {
        StringJoiner result = new StringJoiner(";");
        for (AddressRange range : set.getAddressRanges())
            result.add(range.getMinAddress().toString() + ":" + range.getLength());
        return result.toString();
    }
    private void write(Path directory, String name, String text) throws Exception {
        Files.writeString(directory.resolve(name), text, StandardCharsets.UTF_8,
            StandardOpenOption.CREATE_NEW);
    }
    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length != 2 || !args[1].matches("[0-9a-f]{64}"))
            throw new IllegalArgumentException("New output directory and expected source SHA-256 required.");
        if (!args[1].equalsIgnoreCase(currentProgram.getExecutableSHA256()))
            throw new IllegalArgumentException("Database source SHA-256 mismatch.");
        Path destination = Path.of(args[0]).toAbsolutePath().normalize();
        if (Files.exists(destination)) throw new IllegalArgumentException("Output already exists.");
        Files.createDirectories(destination.getParent());
        Path temporary = Files.createTempDirectory(destination.getParent(), "coverage-partial-");
        try {
            AddressSet bodies = new AddressSet();
            StringBuilder functions = new StringBuilder("start\tsize\tranges\n");
            StringBuilder anomalies = new StringBuilder("start\treason\n");
            long count = 0;
            FunctionIterator iterator = currentProgram.getFunctionManager().getFunctions(true);
            Address previous = null;
            while (iterator.hasNext()) {
                monitor.checkCancelled();
                Function function = iterator.next();
                if (function.isExternal()) continue;
                Address start = function.getEntryPoint();
                AddressSetView body = function.getBody();
                if (previous != null && start.compareTo(previous) <= 0)
                    throw new IllegalStateException("Unordered or duplicate function entry.");
                previous = start;
                if (body.isEmpty() || !body.contains(start))
                    throw new IllegalStateException("Empty body or entry outside its body at " + start);
                if (currentProgram.getListing().getInstructionAt(start) == null)
                    anomalies.append(start).append("\tno decoded instruction at entry\n");
                bodies.add(body);
                functions.append(start).append('\t').append(body.getNumAddresses()).append('\t')
                    .append(spans(body)).append('\n');
                count++;
            }
            AddressSet instructions = new AddressSet(), data = new AddressSet(), executable = new AddressSet();
            Listing listing = currentProgram.getListing();
            InstructionIterator instructionIterator = listing.getInstructions(true);
            while (instructionIterator.hasNext()) {
                monitor.checkCancelled();
                Instruction unit = instructionIterator.next();
                instructions.add(unit.getMinAddress(), unit.getMaxAddress());
            }
            DataIterator dataIterator = listing.getDefinedData(true);
            while (dataIterator.hasNext()) {
                monitor.checkCancelled();
                Data unit = dataIterator.next();
                data.add(unit.getMinAddress(), unit.getMaxAddress());
            }
            if (!instructions.intersect(data).isEmpty())
                throw new IllegalStateException("Instruction/data interpretations overlap.");
            StringBuilder regions = new StringBuilder("start\tsize\tinitialized\tinstructions\tdata\tundefined\tinstructions_outside\tdata_outside\tundefined_outside\n");
            StringBuilder unresolved = new StringBuilder("kind\tranges\n");
            for (MemoryBlock block : currentProgram.getMemory().getBlocks()) {
                if (!block.isExecute()) continue;
                AddressSet region = new AddressSet(block.getStart(), block.getEnd());
                executable.add(region);
                AddressSet ins = instructions.intersect(region), dat = data.intersect(region);
                AddressSet undef = region.subtract(ins).subtract(dat);
                if (ins.getNumAddresses() + dat.getNumAddresses() + undef.getNumAddresses() != block.getSize())
                    throw new IllegalStateException("Region partition does not equal block size.");
                regions.append(block.getStart()).append('\t').append(block.getSize()).append('\t')
                    .append(block.isInitialized()).append('\t').append(ins.getNumAddresses()).append('\t')
                    .append(dat.getNumAddresses()).append('\t').append(undef.getNumAddresses()).append('\t')
                    .append(ins.subtract(bodies).getNumAddresses()).append('\t')
                    .append(dat.subtract(bodies).getNumAddresses()).append('\t')
                    .append(undef.subtract(bodies).getNumAddresses()).append('\n');
                unresolved.append("instructions_outside\t").append(spans(ins.subtract(bodies))).append('\n');
                unresolved.append("undefined_outside\t").append(spans(undef.subtract(bodies))).append('\n');
            }
            write(temporary, "functions.tsv", functions.toString());
            write(temporary, "regions.tsv", regions.toString());
            write(temporary, "anomalies.tsv", anomalies.toString());
            write(temporary, "unresolved.tsv", unresolved.toString());
            write(temporary, "metadata.tsv", "source_sha256\t" + currentProgram.getExecutableSHA256()
                + "\nanalysis_tool\tGhidra " + Application.getApplicationVersion()
                + "\nlanguage\t" + currentProgram.getLanguageID()
                + "\nfunctions\t" + count + "\nbody_bytes_unique\t" + bodies.getNumAddresses()
                + "\nbody_bytes_outside_executable\t" + bodies.subtract(executable).getNumAddresses() + "\n");
            Files.move(temporary, destination);
            println("COVERAGE_EXPORT_COMPLETE functions=" + count + " uniqueBodyBytes=" + bodies.getNumAddresses());
        } finally {
            if (Files.exists(temporary)) {
                try (var children = Files.list(temporary)) {
                    for (Path child : children.toList()) Files.delete(child);
                }
                Files.delete(temporary);
            }
        }
    }
}
