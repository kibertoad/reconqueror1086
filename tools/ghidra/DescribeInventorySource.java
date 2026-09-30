// Metadata only: identity, language, mapping and recognized function count.
// @category CleanRoom
import ghidra.app.script.GhidraScript;
import ghidra.program.model.mem.MemoryBlock;
public class DescribeInventorySource extends GhidraScript {
    public void run() throws Exception {
        println("SOURCE_SHA256=" + currentProgram.getExecutableSHA256());
        println("FORMAT=" + currentProgram.getExecutableFormat());
        println("LANGUAGE=" + currentProgram.getLanguageID());
        println("FUNCTIONS=" + currentProgram.getFunctionManager().getFunctionCount());
        MemoryBlock[] blocks = currentProgram.getMemory().getBlocks();
        println("BLOCK_COUNT=" + blocks.length);
        if (blocks.length > 32) throw new IllegalArgumentException("Use ReportMemoryBlocks.java page/name for larger maps.");
        for (MemoryBlock block : blocks)
            println("BLOCK=" + block.getStart() + ".." + block.getEnd() + "; executable=" + block.isExecute());
    }
}
