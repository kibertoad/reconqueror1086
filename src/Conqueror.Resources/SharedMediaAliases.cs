// FMT-MEDIA-003, FMT-MEDIA-005 and FMT-MEDIA-006 resource contracts use the shared toolkit readers.
// RULE-MEDIA-005 presentation and timing remain in Game; these aliases only select bounded media APIs.
// PLACEHOLDER: RULE-MEDIA-003 the shared PCX reader keeps the header width rather than drawing odd-width padding.
global using IndexedImage = ScientificMethod.LegacyFormats.IndexedImage;
global using PcxImage = ScientificMethod.LegacyFormats.PcxImage;
global using RawIndexedImageDecoder = ScientificMethod.LegacyFormats.RawIndexedImageDecoder;
global using PcxDecoder = ScientificMethod.LegacyFormats.PcxDecoder;
global using SmackerAudioTrack = ScientificMethod.LegacyFormats.SmackerAudioTrack;
global using SmackerFrame = ScientificMethod.LegacyFormats.SmackerFrame;
global using SmackerDataSegment = ScientificMethod.LegacyFormats.SmackerDataSegment;
global using SmackerAudioPacket = ScientificMethod.LegacyFormats.SmackerAudioPacket;
global using SmackerFrameLayout = ScientificMethod.LegacyFormats.SmackerFrameLayout;
global using SmackerTreeSizes = ScientificMethod.LegacyFormats.SmackerTreeSizes;
global using SmackerMovie = ScientificMethod.LegacyFormats.SmackerMovie;
global using SmackerMovieDecoder = ScientificMethod.LegacyFormats.SmackerMovieDecoder;
global using SmackerAudioBuffer = ScientificMethod.LegacyFormats.SmackerAudioBuffer;
global using SmackerAudioDecoder = ScientificMethod.LegacyFormats.SmackerAudioDecoder;
global using SmackerVideoDecoder = ScientificMethod.LegacyFormats.SmackerVideoDecoder;
global using SmackerMovieStream = ScientificMethod.LegacyFormats.SmackerMovieStream;

// Host cue/bin and ISO import readers; RULE-SOUND-004 track selection remains with the importer.
global using CueTrack = ScientificMethod.LegacyFormats.CueTrack;
global using IsoFile = ScientificMethod.LegacyFormats.IsoFile;
global using CueSheet = ScientificMethod.LegacyFormats.CueSheet;
global using CddaWave = ScientificMethod.LegacyFormats.CddaWave;
global using RawMode1Image = ScientificMethod.LegacyFormats.RawMode1Image;
global using Iso9660 = ScientificMethod.LegacyFormats.Iso9660;
