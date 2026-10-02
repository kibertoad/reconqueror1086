# RES

Next ID: Q-RES-120

## Static

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

- Q-RES-007. FMT-RES-005: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-009. FMT-RES-007: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-010. FMT-RES-008: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-011. FMT-RES-009: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-012. FMT-RES-010: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-013. FMT-RES-011: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-014. FMT-RES-012: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-015. FMT-RES-013: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-016. FMT-RES-014: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

- Q-RES-017. FMT-RES-015: What layout, if any, is shared by these listed candidates?
  Settles it: inspect bounded file signatures and the relevant readers; record the
  parsing syntax or field layout, splitting the entry if the files differ before
  making claims about them. Blocks: Survey data-family reconciliation.

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
