using Conqueror.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Conqueror.Game;

public sealed class SmackerMoviePlayer : IDisposable
{
    private readonly SmackerMovieStream _source;
    private readonly SmackerMovie _movie;
    private readonly SmackerVideoDecoder _videoDecoder;
    private readonly byte[] _compressedFrame;
    private readonly byte[] _indices;
    private readonly byte[] _rgba;
    private readonly SmackerAudioTrack? _audioTrack;
    private readonly DynamicSoundEffectInstance? _audio;
    private byte[] _palette = new byte[768];
    private int _nextFrame;
    private TimeSpan _elapsed;
    private bool _paused;
    private bool _disposed;

    public Texture2D Texture { get; }
    public bool IsComplete { get; private set; }
    public int CurrentFrameIndex => _nextFrame - 1;

    public SmackerMoviePlayer(GraphicsDevice graphicsDevice, Stream source, float volume = 1f)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(source);
        _source = new SmackerMovieStream(source);
        _movie = _source.Movie;
        _videoDecoder = SmackerVideoDecoder.FromTreeData(_movie, _source.TreeData);
        _compressedFrame = new byte[_source.MaximumFrameLength];
        _indices = new byte[checked(_movie.Width * _movie.Height)];
        _rgba = new byte[checked(_indices.Length * 4)];
        Texture = new Texture2D(graphicsDevice, _movie.Width, _movie.Height, false, SurfaceFormat.Color);
        _audioTrack = _movie.AudioTracks.FirstOrDefault();
        if (_audioTrack is { IsCompressed: true, Is16Bit: false, IsStereo: false })
        {
            _audio = new DynamicSoundEffectInstance(_audioTrack.SampleRate, AudioChannels.Mono);
            _audio.Volume = Math.Clamp(volume, 0, 1);
        }
        DecodeNextFrame();
    }

    public void Update(TimeSpan elapsed)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsComplete || _paused || elapsed < TimeSpan.Zero) return;
        _elapsed += elapsed;
        while (_elapsed >= _movie.FrameDuration && !IsComplete)
        {
            _elapsed -= _movie.FrameDuration;
            if (_nextFrame < _movie.Frames.Count) DecodeNextFrame();
            else IsComplete = true;
        }
    }

    public void Skip()
    {
        if (_disposed) return;
        IsComplete = true;
        _audio?.Stop();
    }

    public void Pause()
    {
        if (_disposed || _paused) return;
        _paused = true;
        if (_audio?.State == SoundState.Playing) _audio.Pause();
    }

    public void Resume()
    {
        if (_disposed || !_paused) return;
        _paused = false;
        if (_audio is { State: SoundState.Paused }) _audio.Resume();
    }

    public void SetVolume(float volume)
    {
        if (_disposed) return;
        if (_audio is not null) _audio.Volume = Math.Clamp(volume, 0, 1);
    }

    private void DecodeNextFrame()
    {
        var descriptor = _movie.Frames[_nextFrame];
        var frameLength = _source.ReadFrame(_nextFrame, _compressedFrame);
        var framePayload = _compressedFrame.AsSpan(0, frameLength);
        var frame = SmackerMovieDecoder.DecodeFramePayload(_movie, _nextFrame, framePayload, _palette);
        _palette = frame.Palette;
        _videoDecoder.DecodeFrame(framePayload.Slice(frame.Video.Offset, frame.Video.Length), _indices,
            descriptor.IsKeyFrame);
        for (var index = 0; index < _indices.Length; index++)
        {
            var color = _indices[index] * 3;
            var target = index * 4;
            _rgba[target] = _palette[color];
            _rgba[target + 1] = _palette[color + 1];
            _rgba[target + 2] = _palette[color + 2];
            _rgba[target + 3] = 255;
        }
        Texture.SetData(_rgba);

        if (_audio is not null)
        {
            var audioTrack = _audioTrack!;
            foreach (var packet in frame.AudioPackets.Where(packet => packet.TrackIndex == audioTrack.Index))
            {
                var decoded = SmackerAudioDecoder.Decode(
                    framePayload.Slice(packet.Data.Offset, packet.Data.Length), audioTrack);
                _audio.SubmitBuffer(decoded.ToPcm16LittleEndian());
            }
            if (!_paused && _audio.PendingBufferCount > 0 && _audio.State != SoundState.Playing) _audio.Play();
        }
        _nextFrame++;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _audio?.Dispose();
        Texture.Dispose();
        _source.Dispose();
    }
}
