using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Maui.Rtsp.Controls;
using Maui.Rtsp.Platforms.Android;
using Microsoft.Maui.Handlers;

namespace Maui.Rtsp.Handlers
{
    public class CameraViewHandler : ViewHandler <ICameraView, SurfaceView>
    {
        private RtspClient rtspClient;
        private RtspListener rtspListener;

        public static IPropertyMapper<ICameraView, CameraViewHandler> PropertyMapper = new PropertyMapper<ICameraView, CameraViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(ICameraView.Url)] = MapUrl,
            [nameof(ICameraView.User)] = MapUser,
            [nameof(ICameraView.Password)] = MapPassword
        };
        public CameraViewHandler() : base(PropertyMapper)
        {
        }
        public CameraViewHandler(IPropertyMapper mapper) : base(mapper ?? PropertyMapper)
        {
        }
        protected override SurfaceView CreatePlatformView()
        {
            var context = Context;
            var surfaceView = new SurfaceView(context);

            surfaceView.Holder.AddCallback(new SurfaceCallbackImplementation(this));
            rtspListener = new RtspListener(surfaceView.Holder.Surface, 0, 0);

            return surfaceView;
        }
        protected override void ConnectHandler(SurfaceView platformView)
        {
            base.ConnectHandler(platformView);
            rtspClient = new RtspClient();
            UpdateUrl();
            UpdateUser();
            UpdatePassword();
        }
        protected override void DisconnectHandler(SurfaceView platformView)
        {
            platformView.Holder.RemoveCallback(new SurfaceCallbackImplementation(this));
            rtspClient?.Dispose();
            rtspClient = null;
            rtspListener?.Dispose();
            rtspListener = null;
            base.DisconnectHandler(platformView);
        }
        public static void MapUrl(CameraViewHandler handler, ICameraView view)
        {
            handler.UpdateUrl();
            _ = handler.RestartRtspAsync();
        }

        public static void MapUser(CameraViewHandler handler, ICameraView view)
        {
            handler.UpdateUser();
        }

        public static void MapPassword(CameraViewHandler handler, ICameraView view)
        {
            handler.UpdatePassword();
        }

        private void UpdateUrl()
        {
            if (VirtualView != null)
            {
                rtspClient.Url = VirtualView.Url ?? "";
            }
        }

        private void UpdateUser()
        {
            if (VirtualView != null && VirtualView is CameraView cameraView)
            {
                rtspClient.Username = cameraView.User ?? "";
            }
        }

        private void UpdatePassword()
        {
            if (VirtualView != null && VirtualView is CameraView cameraView)
            {
                rtspClient.Password = cameraView.Password ?? "";
            }
        }

        public async Task RestartRtspAsync()
        {
            if (rtspClient != null)
            {
                // Stop streaming
                await rtspClient.StopStreaming();

                // Update the data
                UpdateUrl();
                UpdateUser();
                UpdatePassword();

                // Restart
                await rtspClient.StartStreaming(PlatformView);
            }
        }
        private class SurfaceCallbackImplementation : Java.Lang.Object, ISurfaceHolderCallback
        {
            private readonly CameraViewHandler _handler;

            public SurfaceCallbackImplementation(CameraViewHandler handler)
            {
                _handler = handler;
            }

            public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
            {
            }

            public void SurfaceCreated(ISurfaceHolder holder)
            {
                if (_handler.rtspClient != null)
                {
                    System.Threading.Tasks.Task.Run(async () =>
                    {
                        await _handler.rtspClient.StartStreaming(_handler.PlatformView);
                    });
                }
            }

            public void SurfaceDestroyed(ISurfaceHolder holder)
            {
            }
        }
    }
}
