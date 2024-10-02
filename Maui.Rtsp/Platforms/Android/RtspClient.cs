using Android.Views;
using Com.Alexvas.Utils;
using Java.Util.Concurrent.Atomic;
using Maui.Rtsp.AndroidLib;

namespace Maui.Rtsp.Platforms.Android
{
    public class RtspClient
    {
        private Com.Alexvas.Rtsp.RtspClient localClient;
        private AtomicBoolean rtspStopped;
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public async Task<bool> StartStreaming(SurfaceView surfaceView)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Uri uri = new Uri(this.Url);
                    var socket = NetUtils.CreateSocketAndConnect(uri.Host, 554, 5000);

                    rtspStopped = new AtomicBoolean(false);
                    var listener = new RtspListener(surfaceView.Holder.Surface, surfaceView.Width, surfaceView.Height);

                    localClient = new Com.Alexvas.Rtsp.RtspClient.Builder(socket, this.Url, rtspStopped, listener)
                        .RequestVideo(true)
                        .RequestAudio(false)
                        .WithDebug(true)
                        .WithCredentials(this.Username, this.Password)
                        .Build();
                    localClient.Execute();

                    NetUtils.CloseSocket(socket);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting RTSP stream: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task StopStreaming()
        {
            try
            {
                if (localClient != null && rtspStopped != null)
                {

                    localClient = null;
                    rtspStopped.Set(true);

                    // Wait a second to be sre the streaming stopped
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping RTSP stream: {ex.Message}");
            }
            finally
            {
                localClient = null;
                rtspStopped = null;
            }
        }

        public void Dispose()
        {
            localClient?.Dispose();
        }
    }
}