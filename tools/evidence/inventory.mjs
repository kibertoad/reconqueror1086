// One path segment that every supported OS can store and Git can check out unchanged.
const portableSegment = (x) => !!x && x !== "." && x !== ".." && !/[<>:"\\|?*\x00-\x1F]/.test(x) && !/[. ]$/.test(x) && !/^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)/i.test(x);

export function inventoryPath(build, manifest) {
  if (!/^BLD-[A-Z][A-Z0-9.-]*$/.test(build)) throw new Error("Invalid build ID");
  if (typeof manifest !== "string") throw new Error("Invalid manifest path");
  const disc = /^(CD\d*):(.+)$/.exec(manifest), path = disc ? `@${disc[1]}/${disc[2]}` : manifest;
  const parts = path.split("/");
  if (!parts.every(portableSegment) || (!disc && parts[0].startsWith("@"))) throw new Error("Manifest path cannot be represented portably");
  return `coverage/${build}/${path}.tsv`;
}
