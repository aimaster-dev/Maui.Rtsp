using Android.Views;
using Com.Alexvas.Rtsp.Codec;
using Com.Alexvas.Utils;
using Java.Interop;
using static Com.Alexvas.Rtsp.Codec.FrameQueue;

namespace Maui.Rtsp.Platforms.Android
{
    public class RtspListener : Java.Lang.Object, Com.Alexvas.Rtsp.RtspClient.IRtspClientListener, VideoDecodeThread.IVideoDecoderListener
    {
        public bool RtspStopped { get; set; }

        private VideoFrameQueue videoFrameQueue;
        private AudioFrameQueue audioFrameQueue;
        private string videoMimeType = "";
        private string audioMimeType = "";
        private int audioSampleRate = 0;
        private int audioChannelCount = 0;
        private byte[] audioCodecConfig = null;
        VideoDecodeThread videoDecodeThread;
        Surface _surface;
        int _width;
        int _height;

        public RtspListener(Surface surface, int width, int height)
        {
            _surface = surface;
            _width = width;
            _height = height;
            videoFrameQueue = new VideoFrameQueue(100);
            audioFrameQueue = new AudioFrameQueue(100);
        }

        public void Dispose()
        {
        }

        public void Disposed()
        {
        }

        public void DisposeUnlessReferenced()
        {
        }

        public void Finalized()
        {
        }

        public void OnRtspAudioSampleReceived(byte[] data, int offset, int length, long timestamp)
        {
        }

        public void OnRtspConnected(Com.Alexvas.Rtsp.RtspClient.SdpInfo sdpInfo)
        {
            if (sdpInfo.VideoTrack != null)
            {
                videoFrameQueue.Clear();

                switch (sdpInfo.VideoTrack.VideoCodec)
                {
                    case Com.Alexvas.Rtsp.RtspClient.VideoCodecH264:
                        videoMimeType = "video/avc";
                        break;
                    case Com.Alexvas.Rtsp.RtspClient.VideoCodecH265:
                        videoMimeType = "video/hevc";
                        break;
                }

                if (sdpInfo.AudioTrack != null)
                {
                    switch (sdpInfo.AudioTrack.AudioCodec)
                    {
                        case Com.Alexvas.Rtsp.RtspClient.AudioCodecAac:
                            audioMimeType = "audio/mp4a-latm";
                            break;
                    }
                }
            }

            var sps = sdpInfo.VideoTrack.Sps;
            var pps = sdpInfo.VideoTrack.Pps;

            if (sps != null && pps != null)
            {
                var data = new byte[sps.Count + pps.Count];
                sps.CopyTo(data, 0);
                pps.CopyTo(data, sps.Count);

                var videoFrame = new VideoFrame(
                    codecType: VideoCodecType.H264,
                    isKeyframe: true,
                    data: data,
                    offset: 0,
                    length: data.Length,
                    timestampMs: 0,
                    capturedTimestampMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                );
                videoFrameQueue.Push(videoFrame);
            }

            if (sdpInfo.AudioTrack != null)
            {
                audioFrameQueue.Clear();

                switch (sdpInfo.AudioTrack.AudioCodec)
                {
                    case Com.Alexvas.Rtsp.RtspClient.AudioCodecAac:
                        audioMimeType = "audio/mp4a-latm";
                        break;
                }

                audioSampleRate = sdpInfo.AudioTrack.SampleRateHz;
                audioChannelCount = sdpInfo.AudioTrack.Channels;
                audioCodecConfig = sdpInfo.AudioTrack.Config.ToArray();
            }

            OnRtspClientConnected();
        }

        public void OnRtspClientConnected()
        {
            if (!string.IsNullOrWhiteSpace(videoMimeType))
            {
                videoDecodeThread = new VideoDecodeThread(
                    _surface,
                    videoMimeType,
                    _width,
                    _height,
                    0, // This is the rotation
                    videoFrameQueue,
                    this, // IVideoDecoderListener
                          // or Hardware, depending on your needs
                    VideoDecodeThread.DecoderType.Software
                );
                videoDecodeThread.Start();
            }
        }

        public void OnRtspConnecting()
        {
        }

        public void OnRtspDisconnected()
        {
            RtspStopped = true;
        }

        public void OnRtspFailed(string message)
        {
            RtspStopped = true;
        }

        public void OnRtspFailedUnauthorized()
        {
        }

        public void OnRtspVideoNalUnitReceived(byte[] data, int offset, int length, long timestamp)
        {
            var nals = new List<VideoCodecUtils.NalUnit>();
            var numNals = VideoCodecUtils.GetH264NalUnits(data, offset, length - 1, nals);

            if (length > 0)
            {
                bool isKeyframe = nals.Exists(nal => nal.Type == 5); // For H.264, NAL type 5 is IDR (keyframe)
                var videoFrame = new VideoFrame(
                    codecType: VideoCodecType.H264,
                    isKeyframe: isKeyframe,
                    data: data,
                    offset: offset,
                    length: length,
                    timestampMs: timestamp,
                    capturedTimestampMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                );
                videoFrameQueue.Push(videoFrame);
            }
        }

        public void SetJniIdentityHashCode(int value)
        {
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
        }

        public void SetPeerReference(JniObjectReference reference)
        {
        }

        public void UnregisterFromRuntime()
        {
        }

        public void OnRtspDisconnecting()
        {
        }

        public void OnVideoDecoderFormatChanged(int width, int height) { }
        public void OnVideoDecoderFirstFrameDecoded() { }
        public void OnVideoDecoderFailed(string message) { }
        public void OnVideoDecoderFirstFrameRendered() { }
        public void OnVideoDecoderStarted() { }
        public void OnVideoDecoderStopped() { }
    }
}