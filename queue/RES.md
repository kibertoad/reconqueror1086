# RES

Next ID: Q-RES-206

## Static

- Q-RES-177. RULE-RES-005: How does SETUP's expansion routine read literals in mode 1?
  Settles it: read 0001:45CF and the table it builds from CS:0x3601, and the
  literal path from 0001:4417 to 0001:44A8, every branch.
  Blocks: none.

- Q-RES-178. FMT-RES-119: Does `_SETUP.EXE` copy LANGUAGE.INF into the product directory?
  Settles it: read `_SETUP.EXE`'s file copies of LANGUAGE.INF.
  Tried: FND-RES-040 found the `Ident` `DirName` reader at 0004:4394, the
  other half of this item's first wording; the copy question is open.
  Blocks: none.

- Q-RES-189. FMT-RES-120: What does `PICKDEST`'s destination routine 0004:5EB4 show, and which choices return 2 and 7?
  Settles it: read 0004:5EB4 and every return value it can give.
  Blocks: none.

- Q-RES-190. FMT-RES-120: What does `COPY`'s copy routine do with the `[Files]`, `[Archives]` and `[Billboards]` data and `AnimationDLL`?
  Settles it: read 0004:626C, 0003:AF90, 0003:4ECC and 0002:30D8, every
  branch.
  Blocks: none.

- Q-RES-191. FMT-RES-120: How does `RUN` wait for a program (0004:D3D8), and what do 0002:A742 and 0004:C70A show?
  Settles it: read the three routines and the string resources 0xBB9 and
  0xBBA they name.
  Blocks: none.

- Q-RES-185. FMT-RES-120: What do the 23 `[Script]` commands the shipped script does not use, other than `WINDISKSPACE_LT`, do?
  Settles it: read their cases from the table at 0004:5C55, every branch.
  Blocks: none.

- Q-RES-186. FMT-RES-120: What does the dialog routine 0003:9EE6 show and return for `DIALOG`, and does it set script flags?
  Settles it: read 0003:9EE6 and its writes to the words at offset 0x56 of
  the loader's object.
  Blocks: none.

- Q-RES-187. FMT-RES-120: What does `TOGGLEGROUPON` change in the `[Files]` data?
  Settles it: read 0003:AEAA with offset 0x22 of the loader's object.
  Blocks: none.

- Q-RES-188. FMT-RES-120: When is the word at offset 0xAE of the object at DS:0FB4 set?
  Settles it: find every write to offset 0xAE of that object in
  `_SETUP.EXE` and read what decides it.
  Blocks: none.

- Q-RES-180. FMT-RES-120: What do 0004:3142 and 0003:C46A do with a dialog item's trailing fields and a `*` title?
  Settles it: read both routines and what they store.
  Blocks: none.

- Q-RES-181. FMT-RES-120: Which file do `_SETUP.EXE`'s variable-file reads of `Setup`, `Requirements` and `Ident` name?
  Settles it: trace the file argument of each such call FND-RES-032 lists to
  the string it was built from.
  Blocks: none.

- Q-RES-182. FMT-RES-120: What do the numeric fields of `[Archives]`, `[Files]` and `[Billboards]` records mean?
  Settles it: read the readers of the arrays at offsets 0x0A, 0x22 and 0x3A of
  the loader's object.
  Blocks: none.

- Q-RES-176. FMT-RES-118: What does SETUP's library routine at 0001:311E do with `unk_16` and `unk_18`?
  Settles it: read the routine at 0001:311E and the interrupt or import it reaches.
  Blocks: none.

- Q-RES-164. FMT-RES-014: Which shipped consumer opens the disc-root icons, and are those paths reachable?
  Settles it: Trace file-open references from media/application entry points. Blocks: Survey consumer reconciliation.

- Q-RES-165. FMT-RES-014: How does the consumer handle the differing image_size values?
  Settles it: Read size tests and every copy input, preserving FND-RES-020 variants. Blocks: Survey consumer reconciliation.

- Q-RES-166. FMT-RES-014: How does image selection handle zero directory hints and multiple images?
  Settles it: Trace display inputs, header interpretation and every fallback. Blocks: Survey consumer reconciliation.

- Q-RES-167. FMT-RES-014: What happens with absent, truncated or inconsistent icon files?
  Settles it: Read admission and failure paths without inferring them from stored files. Blocks: Survey consumer reconciliation.


- Q-RES-162. FMT-RES-013: Does INST.EXE's text-mode read stop at the 0x1A byte, keeping it out of the last entry's text?
  Settles it: read the run-time `fgets` and text-mode read path under 1000:4543 for 0x1A handling. Blocks: Survey data-family reconciliation.
  Tried: FND-RES-053 reads the dictionary parser; the run-time read below it was not read.

- Q-RES-163. FMT-RES-013: How does INST.EXE show tab bytes in a dictionary text?
  Settles it: trace the texts INSTALL.HLP holds into the routines that draw them. Blocks: Survey data-family reconciliation.
  Tried: FND-RES-053 shows the dictionary keeps tabs; no display routine was read.

- Q-RES-204. FMT-RES-013: Which INST.EXE code looks up the keys of INSTALL.HLP, and with what keys?
  Settles it: find every caller of 2852:0B71 and 2852:0A8B and how each forms its key. Blocks: Survey data-family reconciliation.

- Q-RES-205. FMT-RES-013: Which flag does each caller of 2852:0B71 pass, so that a missing key ends the run or gives an empty text?
  Settles it: read the flag argument at every call of 2852:0B71. Blocks: Survey data-family reconciliation.


- Q-RES-157. FMT-RES-001: Which consumer selects the disc-root C1086.GOB copy
  versus the installed copy? Settles it: trace the archive path construction and
  every relevant caller through the file-open decision. Blocks: Survey runtime-use
  reconciliation.

- Q-RES-192. FMT-RES-010: Does INST.EXE reach its read of `resource.cfg` when `INSTALL.BAT` starts it as `inst.exe -f`?
  Settles it: resolve the eleven indirect calls FND-RES-045 lists in the
  callees of 20F3:029C before 20F3:03D6, and read each target for paths that
  end the run with the arguments `-f`. Blocks: Survey data-family reconciliation.
  Tried: FND-RES-045 reads the command line (+0x44) and the script load
  (+0x54) and walks the direct calls of all six callees; along direct calls
  the run ends only on failures, but the indirect calls are not followed.

- Q-RES-141. FMT-RES-010: How does the consumer interpret the directory value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which copies the value to
  object+0x194 and to the string at +0x18 of the object at +0x1EB; the uses of
  that field are not read.

- Q-RES-142. FMT-RES-010: How does the consumer interpret the videoDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 finds that INST.EXE's reader passes this key to the
  objects listed at object+2 (count at +0x192) through vtable entry +0x2C of
  each. Which objects the list holds (2487:0047 passes nine to vtable entry
  +0x00) and what they do are not read.

- Q-RES-143. FMT-RES-010: How does the consumer interpret the cd value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which sets the word at +0x1FB to
  1 for `yes`; the uses of that field are not read.

- Q-RES-144. FMT-RES-010: How does the consumer interpret the joyDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 finds that INST.EXE's reader passes this key to the
  objects listed at object+2 (count at +0x192) through vtable entry +0x2C of
  each. Which objects the list holds (2487:0047 passes nine to vtable entry
  +0x00) and what they do are not read.

- Q-RES-145. FMT-RES-010: How does the consumer interpret the memoryDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 finds that INST.EXE's reader passes this key to the
  objects listed at object+2 (count at +0x192) through vtable entry +0x2C of
  each. Which objects the list holds (2487:0047 passes nine to vtable entry
  +0x00) and what they do are not read.

- Q-RES-146. FMT-RES-010: How does the consumer interpret the minCPU value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which copies the value to the
  string at +0x203; the uses of that field are not read.

- Q-RES-147. FMT-RES-010: How does the consumer interpret the minDOS value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which converts the value through
  1000:414A and keeps the low word at +0x207; the uses of that field are not
  read.

- Q-RES-148. FMT-RES-010: How does the consumer interpret the mode value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which copies the value to the
  string at +0x1FF; the uses of that field are not read.

- Q-RES-149. FMT-RES-010: How does the consumer interpret the mouseDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 finds that INST.EXE's reader passes this key to the
  objects listed at object+2 (count at +0x192) through vtable entry +0x2C of
  each. Which objects the list holds (2487:0047 passes nine to vtable entry
  +0x00) and what they do are not read.

- Q-RES-150. FMT-RES-010: How does the consumer interpret the smartDrv value token?
  Settles it: trace this key through value parsing and every relevant consumer use.
  Blocks: Survey data-family reconciliation.
  Tried: FND-RES-054 reads INST.EXE's reader, which sets the word at +0x1FD to
  1 for `yes`; the uses of that field are not read.


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
  New lead: FND-RES-023 locates AUTORUN/LANGUAGE filename bytes in AUTOPLAY
  and two SIERRA filename occurrences in SETUP. Trace their executable references
  independently into file-open inputs; no reachable consumer is proved yet.
  Mapping: FND-RES-024 distinguishes AUTOPLAY PE32 and SETUP NE; trace the
  profile-import argument routes in the former and segment/relocation references
  in the latter. Import presence alone is not a call.
  FND-RES-025 completes SETUP relocation-table inspection: the selector chain
  needs instruction/offset provenance, not another direct filename-fixup search.
  Next tooling prerequisite: correctly mapped launcher function inventories.
  Third attempt, with the mapped AUTOPLAY reading: FND-RES-026 settles AUTORUN.INF
  (now FMT-RES-117) and shows AUTOPLAY reads only an installed LANGUAGE.INF's
  `Ident`/`Title` and never CONQUER.INF or SIERRA.INF. Next: SETUP's selector
  provenance with its NE inventory, and INST.EXE, for the three remaining files.
  Fourth attempt: FND-RES-029 reads SETUP's SIERRA.INF use (two `Setup` keys
  through the profile routines) and finds it starts `_SETUP.EXE`; the other
  readers are behind Q-RES-174 and INST.EXE.
  Sixth attempt: FND-RES-033 reads SIERRA.INF's loader (FMT-RES-120); only
  CONQUER.INF is left in FMT-RES-015, and INST.EXE is the lead for it.
  Fifth attempt: FND-RES-032 splits LANGUAGE.INF out (FMT-RES-119) and locates
  SIERRA.INF's loader in `_SETUP.EXE` (Q-RES-170); CONQUER.INF's reader is
  still to be found, with INST.EXE the lead.
  Seventh attempt, with INST.EXE unpacked: FND-RES-034 finds CONQUER.INF's
  name written whole only in INSTALL.DAT's `@exists` test, a DOS find-first
  in CONFIG.EXE that reads nothing from the file; INST.EXE names neither it
  nor INSTALL.DAT. What is left is a reader that builds the name at run time
  or keeps it compressed inside another file; take this up again only with
  such a lead.

- Q-RES-199. FMT-RES-016: Which values do INSTALL.SCR's parameters `%1` to `%7` hold when the script runs?
  Settles it: trace every write to the object fields +0x1E6, +0x23E, +0x1DE, +0x194, +0x1AA and +0x1AE and to DS:5557 before vtable entry +0x30 is called. Blocks: Survey data-family reconciliation.
  Tried: FND-RES-047 finds that `space` writes the free megabytes into +0x1AE
  of the object at DS:1A04; FND-RES-050 finds the run loop called on the
  object at DS:55D8, which DS:1A04 points to (FND-RES-054). FND-RES-052
  settles `%1` and `%2`; the string fields behind `%3` to `%6` are written
  through string routines in 20F3, 2487 and 2821 that are not read.

- Q-RES-202. FMT-RES-016: What do INST.EXE's checks before the script, vtable entries +0x10 and +0x28 of the object at DS:55D8, decide, and which drive letters do the bytes at +0x1E6 and +0x23E hold?
  Settles it: read 20F3:1021 and 20F3:1D07 with every branch, and every write to +0x1E6 and +0x23E. Blocks: Survey data-family reconciliation.
  Tried: FND-RES-052 settles the drive letters at +0x1E6 and +0x23E; +0x10
  (20F3:1021) and +0x28 (20F3:1D07) are not read.

- Q-RES-203. FMT-RES-016: What does the shipped `pick` line's fourth label buffer, which no word fills, hold when a key with a low byte of 0 is pressed?
  Settles it: read the stack writes that the calls before 1C17:0D8F leave at that buffer's offset, or replay the call with the harness from a recorded stack. Blocks: Survey data-family reconciliation.

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

- Q-RES-172. FMT-RES-117: Which program reads the `autorun` section of AUTORUN.INF, and how?
  Settles it: a Windows 95 AutoRun reference for the `OPEN` and `ICON` keys, compared
  with the shipped values. AUTOPLAY does not read it (FND-RES-026). Blocks: none.

- Q-RES-002. FMT-RES-002: What was `unk_24` intended to hold? Settles it: a contemporary design
  note, erratum or author statement addressing the intent; executable behavior alone does not
  prove intent. Blocks: none.

## Blocked

None.
