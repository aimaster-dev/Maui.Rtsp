using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Rtsp.Controls
{
    public interface ICameraView : IView
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
    public class CameraView : View, ICameraView
    {
        public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(CameraView), default(string));
        public static readonly BindableProperty UserProperty = BindableProperty.Create(nameof(User), typeof(string), typeof(CameraView), default(string));
        public static readonly BindableProperty PasswordProperty = BindableProperty.Create(nameof(Password), typeof(string), typeof(CameraView), default(string));
        public string Url
        {
            get => (string)GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }
        public string User
        {
            get => (string)GetValue(UserProperty);
            set => SetValue(UserProperty, value);
        }
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }
    }
}
