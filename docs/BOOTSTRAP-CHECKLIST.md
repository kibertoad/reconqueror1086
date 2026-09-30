# Existing-project migration checklist

- [x] Preserve project names, importer paths, package IDs and installer GUID.
- [x] Review agent policy and preserve canonical push and orphan-audit rules.
- [x] Record the owner decision removing separate plan approvals.
- [x] Adopt pinned offline rules/checker and toolkit reporter licenses.
- [x] Establish latest official patch provenance before executable analysis
      (SRC-PATCH-CATALOG; owned source hash verified).
- [x] Complete migration validation and record the observed results.

This is an existing game project, so the template rename/bootstrap operation is
not run over the checkout. Configuration verification checks its existing identity.
