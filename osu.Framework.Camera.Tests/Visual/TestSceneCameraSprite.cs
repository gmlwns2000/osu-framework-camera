// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Camera;
using osu.Framework.Testing;
using osuTK;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Bindables;

namespace osu.Framework.Camera.Tests.Visual
{
    public class TestSceneCameraSprite : TestScene
    {
        private CameraSprite display;
        private BasicDropdown<string> deviceList;
        private Bindable<string> device = new Bindable<string>();

        [Resolved]
        private CameraManager camera { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                deviceList = new BasicDropdown<string>
                {
                    Width = 300,
                    Margin = new MarginPadding(10),
                    Items = camera.CameraDeviceNames
                },
                display = new CameraSprite
                {
                    Size = new Vector2(512),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });

            camera.OnNewDevice += updateDevices;
            camera.OnLostDevice += updateDevices;

            device.BindTo(deviceList.Current);
            device.ValueChanged += (v) => display.CameraID = camera.CameraDeviceNames.ToList().IndexOf(v.NewValue);
        }

        private void updateDevices(string name)
        {
            deviceList.Items = camera.CameraDeviceNames;
        }
    }
}