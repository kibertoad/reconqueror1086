# RES

Next ID: Q-RES-171

## Static

- Q-RES-169. FMT-RES-015: How does LANGUAGE handle repeated InstallDoneTitle?
  Settles it: trace lookup order and continuation after a match. Blocks: INF reconciliation.
  Tried: FND-RES-022 establishes the repeated token, not lookup semantics.

- Q-RES-170. FMT-RES-015: Which SIERRA reader distinguishes command-shaped equals regions and comma-bearing data?
  Settles it: identify the reader and trace dispatch/token boundaries. Blocks: INF reconciliation.
  Tried: FND-RES-022 retains all token classes; raw separators do not settle their roles.

- Q-RES-168. FMT-RES-015: Which encoding interprets AUTORUN.INF non-ASCII bytes?
  Settles it: trace the consuming decoder or establish a relevant format source
  and compare its interpretation with the complete file. Blocks: INF reconciliation.
  Tried: FND-RES-021 records four high bytes but cannot choose a code page.

- Q-RES-164. FMT-RES-014: Which shipped consumer opens the disc-root icons, and are those paths reachable?
  Settles it: Trace file-open references from media/application entry points. Blocks: Survey consumer reconciliation.

- Q-RES-165. FMT-RES-014: How does the consumer handle the differing image_size values?
  Settles it: Read size tests and every copy input, preserving FND-RES-020 variants. Blocks: Survey consumer reconciliation.

- Q-RES-166. FMT-RES-014: How does image selection handle zero directory hints and multiple images?
  Settles it: Trace display inputs, header interpretation and every fallback. Blocks: Survey consumer reconciliation.

- Q-RES-167. FMT-RES-014: What happens with absent, truncated or inconsistent icon files?
  Settles it: Read admission and failure paths without inferring them from stored files. Blocks: Survey consumer reconciliation.


- Q-RES-158. FMT-RES-013: Which shipped reader consumes CD-root INSTALL.HLP, and is that path reachable?
  Settles it: trace filename references from installation/media entry points into the reader. Blocks: Survey data-family reconciliation.

- Q-RES-159. FMT-RES-013: What roles do the backslash-prefixed regions have?
  Settles it: trace prefix recognition and every dispatch path using the marker remainder. Blocks: Survey data-family reconciliation.

- Q-RES-160. FMT-RES-013: Does marker lookup retain or normalize trailing whitespace?
  Settles it: read token boundaries and comparison inputs. Blocks: Survey data-family reconciliation.

- Q-RES-161. FMT-RES-013: How does the reader handle repeated complete marker remainders?
  Settles it: read search order and behavior after a match. Blocks: Survey data-family reconciliation.

- Q-RES-162. FMT-RES-013: How does the reader treat 0x1A and the following CRLF?
  Settles it: read control-byte recognition and end-of-input branches. Blocks: Survey data-family reconciliation.

- Q-RES-163. FMT-RES-013: How does the reader handle tabs within text regions?
  Settles it: trace tab recognition through tokenization and presentation decisions. Blocks: Survey data-family reconciliation.


- Q-RES-157. FMT-RES-001: Which consumer selects the disc-root C1086.GOB copy
  versus the installed copy? Settles it: trace the archive path construction and
  every relevant caller through the file-open decision. Blocks: Survey runtime-use
  reconciliation.

- Q-RES-151. FMT-RES-011: Which shipped interpreter consumes CD-root INSTALL.DAT, and is that path reachable?
  Settles it: trace file references from installation/media entry points into the reader. Blocks: Survey data-family reconciliation.

- Q-RES-152. FMT-RES-011: How does the interpreter tokenize at-sign identifiers and match their casing?
  Settles it: read token boundaries and the dispatch comparison. Blocks: Survey data-family reconciliation.

- Q-RES-153. FMT-RES-011: How does the interpreter recognize double-slash comments and handle quotes within them?
  Settles it: read comment recognition, extent and interaction with the quote lexer. Blocks: Survey data-family reconciliation.

- Q-RES-154. FMT-RES-011: What quoted-string and backslash grammar does the interpreter use?
  Settles it: read delimiter, escaping and continuation branches. Blocks: Survey data-family reconciliation.

- Q-RES-155. FMT-RES-011: What grammar handles nonempty regions without at-sign or double-slash prefixes?
  Settles it: read the paths accepting these regions and their surrounding lexical state. Blocks: Survey data-family reconciliation.

- Q-RES-156. FMT-RES-011: How does the interpreter handle the final unterminated region?
  Settles it: read final-line consumption and end-of-file success/error branches. Blocks: Survey data-family reconciliation.


- Q-RES-138. FMT-RES-010: Which shipped reader consumes CD-root RESOURCE.CFG, and is that path reachable?
  Settles it: trace file references from installation/media entry points through the relevant reader. Blocks: Survey data-family reconciliation.

- Q-RES-139. FMT-RES-010: Does the reader require fixed key/separator alignment or accept other spacing?
  Settles it: read its token-boundary and whitespace handling. Blocks: Survey data-family reconciliation.

- Q-RES-140. FMT-RES-010: Does the reader match key casing exactly or without case distinctions?
  Settles it: read its key comparison and caller-supplied key tokens. Blocks: Survey data-family reconciliation.

- Q-RES-141. FMT-RES-010: How does the consumer interpret the directory value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-142. FMT-RES-010: How does the consumer interpret the videoDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-143. FMT-RES-010: How does the consumer interpret the cd value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-144. FMT-RES-010: How does the consumer interpret the joyDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-145. FMT-RES-010: How does the consumer interpret the memoryDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-146. FMT-RES-010: How does the consumer interpret the minCPU value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-147. FMT-RES-010: How does the consumer interpret the minDOS value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-148. FMT-RES-010: How does the consumer interpret the mode value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-149. FMT-RES-010: How does the consumer interpret the mouseDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.

- Q-RES-150. FMT-RES-010: How does the consumer interpret the smartDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.


- Q-RES-135. FMT-RES-009: Which shipped consumer reads AUTOPLAY.BMP, and is that path reachable?
  Settles it: trace filename references from the shipped media entry points through the bitmap reader. Blocks: Survey data-family reconciliation.

- Q-RES-136. FMT-RES-009: Which bitmap header and extent constraints does the consumer check, and what is its malformed-input behavior?
  Settles it: read every relevant guard, access and error path in that consumer. Blocks: Survey data-family reconciliation.

- Q-RES-137. FMT-RES-009: How does the consumer position and orient the decoded image?
  Settles it: trace storage-to-display mapping, coordinate inputs and presentation calls. Blocks: Survey data-family reconciliation.


- Q-RES-132. FMT-RES-008: Which shipped interpreter or wrapper consumes the CD-root batch files, and which paths are reachable?
  Settles it: trace the installation/media entry points and their batch-file accesses. Blocks: Survey data-family reconciliation.

- Q-RES-133. FMT-RES-008: How does the consumer handle the terminal 0x1A in README.BAT?
  Settles it: read its text termination branch and the caller that supplies this file. Blocks: Survey data-family reconciliation.

- Q-RES-134. FMT-RES-008: How does the consumer handle the final unterminated line in INSTALL.BAT?
  Settles it: read its line termination and final-line handling with the relevant caller. Blocks: Survey data-family reconciliation.


- Q-RES-124. FMT-RES-007: Which loader reads the counted containers and selects records, and what malformed-count/length behavior does it have?
  Settles it: trace filename references through every relevant loader read and selection path. Blocks: Survey data-family reconciliation.

- Q-RES-125. FMT-RES-007: Does name lookup compare a zero-terminated prefix or the whole stored name region?
  Settles it: trace the lookup comparison and its length/termination inputs. Blocks: Survey data-family reconciliation.

- Q-RES-126. FMT-RES-007: What is the runtime meaning of driver_unk_00?
  Settles it: trace every header read of this field and its downstream uses.
  Blocks: Survey data-family reconciliation.

- Q-RES-127. FMT-RES-007: What is the runtime meaning of driver_unk_28?
  Settles it: trace every header read of this field and its downstream uses.
  Blocks: Survey data-family reconciliation.

- Q-RES-128. FMT-RES-007: What is the runtime meaning of driver_unk_record_20?
  Settles it: trace every record read of this field and its downstream uses.
  Blocks: Survey data-family reconciliation.

- Q-RES-129. FMT-RES-007: What is the runtime meaning of driver_unk_record_28?
  Settles it: trace every record read of this field and its downstream uses.
  Blocks: Survey data-family reconciliation.

- Q-RES-130. FMT-RES-007: What is the runtime meaning of driver_unk_record_2c?
  Settles it: trace every record read of this field and its downstream uses.
  Blocks: Survey data-family reconciliation.

- Q-RES-131. FMT-RES-007: Which payload layouts occur in the three containers?
  Settles it: identify each payload family through its reader or executable mapping,
  splitting incompatible layouts. Blocks: Survey data-family reconciliation.

- Q-RES-001. FMT-RES-001: What, if anything, occupies the bytes before FNT6.PCX in SKIRMISH.RES?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-RES-003. FMT-RES-002: What is the layout of compression kind 3? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-RES-004. RULE-RES-001: What does `fn_000491D4` decode, and what do `fn_00048090`, `fn_0004830C` and
  `fn_00049124` produce beyond the formats FMT-RES-003 and FMT-RES-004 give? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-RES-005. RULE-RES-004: Is it true that any call loads a picture, layout, song or sound with
  mode 0? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-RES-006. RULE-RES-004: Is it true that the shipped game ever reads a `.LOW` file? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.


- Q-RES-017. FMT-RES-015: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

  Tried: FND-RES-021 checked every file byte and retained all line classes.
  No single strict bracket/key grammar covers the candidates. Next evidence:
  consumer references and token handling, or an independent format-source reading.
  Second attempt: FND-RES-022 adds token-family and repeated-token evidence.
  Remains Static for reader identification; do not repeat raw shape counting without
  new reader evidence.

- Q-RES-018. FMT-RES-016: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-019. FMT-RES-017: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-020. FMT-RES-018: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-021. FMT-RES-019: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-022. FMT-RES-020: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-023. FMT-RES-021: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-024. FMT-RES-022: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-025. FMT-RES-023: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-026. FMT-RES-024: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-027. FMT-RES-025: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-028. FMT-RES-026: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-029. FMT-RES-027: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-030. FMT-RES-028: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-031. FMT-RES-029: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-032. FMT-RES-030: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-033. FMT-RES-031: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-034. FMT-RES-032: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-035. FMT-RES-033: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-036. FMT-RES-034: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-037. FMT-RES-035: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-038. FMT-RES-036: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-039. FMT-RES-037: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-040. FMT-RES-038: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-041. FMT-RES-039: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-042. FMT-RES-040: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-043. FMT-RES-041: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-044. FMT-RES-042: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-045. FMT-RES-043: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-046. FMT-RES-044: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-047. FMT-RES-045: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-048. FMT-RES-046: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-049. FMT-RES-047: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-050. FMT-RES-048: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-051. FMT-RES-049: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-052. FMT-RES-050: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-053. FMT-RES-051: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-054. FMT-RES-052: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-055. FMT-RES-053: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-056. FMT-RES-054: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-057. FMT-RES-055: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-058. FMT-RES-056: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-059. FMT-RES-057: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-060. FMT-RES-058: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-061. FMT-RES-059: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-062. FMT-RES-060: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-063. FMT-RES-061: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-064. FMT-RES-062: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-065. FMT-RES-063: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-066. FMT-RES-064: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-067. FMT-RES-065: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-068. FMT-RES-066: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-069. FMT-RES-067: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-070. FMT-RES-068: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-071. FMT-RES-069: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-072. FMT-RES-070: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-073. FMT-RES-071: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-074. FMT-RES-072: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-075. FMT-RES-073: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-076. FMT-RES-074: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-077. FMT-RES-075: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-078. FMT-RES-076: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-079. FMT-RES-077: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-080. FMT-RES-078: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-081. FMT-RES-079: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-082. FMT-RES-080: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-083. FMT-RES-081: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-084. FMT-RES-082: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-085. FMT-RES-083: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-086. FMT-RES-084: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-087. FMT-RES-085: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-088. FMT-RES-086: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-089. FMT-RES-087: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-090. FMT-RES-088: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-091. FMT-RES-089: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-092. FMT-RES-090: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-093. FMT-RES-091: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-094. FMT-RES-092: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-095. FMT-RES-093: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-096. FMT-RES-094: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-097. FMT-RES-095: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-098. FMT-RES-096: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-099. FMT-RES-097: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-100. FMT-RES-098: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-101. FMT-RES-099: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-102. FMT-RES-100: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-103. FMT-RES-101: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-104. FMT-RES-102: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-105. FMT-RES-103: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-106. FMT-RES-104: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-107. FMT-RES-105: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-108. FMT-RES-106: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-109. FMT-RES-107: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-110. FMT-RES-108: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-111. FMT-RES-109: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-112. FMT-RES-110: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-113. FMT-RES-111: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-114. FMT-RES-112: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-115. FMT-RES-113: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-116. FMT-RES-114: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-117. FMT-RES-115: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-118. FMT-RES-006: Does the shipped wrapper convert INDEX timestamps with 75 subdivisions per second?
  Settles it: statically locate the installed wrapper's cue consumer and read the
  complete timestamp conversion and its callers, distinguishing observed file
  arithmetic from the reader's computation. Blocks: complete reading of FMT-RES-006.

- Q-RES-119. FMT-RES-006: Does the shipped wrapper reject INDEX timestamps with seconds 60 or greater?
  Settles it: read the timestamp parser's bounds and failure paths through its
  callers; distinguish explicit rejection from unchecked conversion or clamping.
  Blocks: complete reading of FMT-RES-006.

- Q-RES-120. FMT-RES-005: Does the shipped wrapper reject entirely-zero records before the cue's first audio start?
  Settles it: read the installed wrapper's raw record reader and its callers,
  separating physical indexing from header checks and the treatment of the
  observed zero run. Blocks: complete reading of FMT-RES-005.

- Q-RES-121. FMT-RES-005: What is the sample representation of the cue-labelled audio span?
  Settles it: trace the shipped wrapper's audio reader and conversion into its
  output channels, recording widths, ordering and byte order with independent
  source-data checks. Blocks: Survey audio member-format coverage.

- Q-RES-122. FMT-RES-116: Which stored trailer regions does the shipped wrapper validate?
  Settles it: follow every trailer read and its error propagation in the raw
  record consumer; distinguish unused bytes from verified checksums or codes.
  Blocks: complete reading of FMT-RES-116.

- Q-RES-123. FMT-RES-116: Does the shipped wrapper use stored address components as physical locators?
  Settles it: read record selection and offset formation in the consumer and its
  callers, checking how the observed address-repeating block is treated.
  Blocks: complete reading of FMT-RES-116.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-RES-002. FMT-RES-002: What was `unk_24` intended to hold? Settles it: a contemporary design
  note, erratum or author statement addressing the intent; executable behavior alone does not
  prove intent. Blocks: none.

## Blocked

None.
