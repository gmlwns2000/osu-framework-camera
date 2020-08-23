// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace osu.Framework.Input
{
    public class CameraManager
    {
        private readonly List<DsDevice> cameraDevices;
        private readonly List<string> cameraDeviceNames;
        public IEnumerable<string> CameraDeviceNames => cameraDeviceNames;

        public CameraManager()
        {
            cameraDevices = EnumerateAllDevices().ToList();
            cameraDeviceNames = cameraDevices.Select(d => d.Name).ToList();
        }

        protected virtual IEnumerable<DsDevice> EnumerateAllDevices() => DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
    }
}
