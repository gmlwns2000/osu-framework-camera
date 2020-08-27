// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using DirectShowLib;
using osu.Framework.Bindables;
using osu.Framework.Threading;

namespace osu.Framework.Input
{
    public class CameraManager : IDisposable
    {
        private ImmutableList<DsDevice> cameraDevices = ImmutableList<DsDevice>.Empty;
        private ImmutableList<string> cameraDeviceNames = ImmutableList<string>.Empty;
        private readonly DsDeviceUpdateComparer updateComparer = new DsDeviceUpdateComparer();
        private readonly GameThread thread;
        private Scheduler scheduler => thread.Scheduler;
        private Scheduler eventScheduler => EventScheduler ?? scheduler;
        public Scheduler EventScheduler;
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        public IEnumerable<string> CameraDeviceNames => cameraDeviceNames;
        public event Action<string> OnNewDevice;
        public event Action<string> OnLostDevice;
        public readonly Bindable<string> CameraDevice = new Bindable<string>();

        public CameraManager(GameThread updateThread)
        {
            thread = updateThread;

            scheduler.Add(() =>
            {
                new Thread(() =>
                {
                    while (!cancelSource.Token.IsCancellationRequested)
                    {
                        syncCameraDevices();
                        Thread.Sleep(1000);
                    }
                })
                {
                    IsBackground = true
                }.Start();
            });
        }

        protected virtual IEnumerable<DsDevice> EnumerateAllDevices() => DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

        private void syncCameraDevices()
        {
            var updatedCameraDevices = EnumerateAllDevices().ToImmutableList();
            if (cameraDevices.SequenceEqual(updatedCameraDevices, updateComparer))
                return;

            cameraDevices = updatedCameraDevices;

            var oldDeviceNames = cameraDeviceNames;
            var newDeviceNames = cameraDeviceNames = cameraDevices.Select(d => d.Name).ToImmutableList();

            var newDevices = newDeviceNames.Except(oldDeviceNames).ToList();
            var lostDevices = oldDeviceNames.Except(newDeviceNames).ToList();

            if (newDevices.Count > 0 || lostDevices.Count > 0)
            {
                eventScheduler.Add(delegate
                {
                    foreach (var d in newDevices)
                        OnNewDevice?.Invoke(d);
                    foreach (var d in lostDevices)
                        OnLostDevice?.Invoke(d);
                });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            cancelSource.Cancel();

            OnNewDevice = null;
            OnLostDevice = null;
        }

        ~CameraManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private class DsDeviceUpdateComparer : IEqualityComparer<DsDevice>
        {
            public bool Equals([AllowNull] DsDevice x, [AllowNull] DsDevice y) => x.DevicePath == y.DevicePath;

            public int GetHashCode(DsDevice obj) => obj.Name.GetHashCode();
        }
    }
}
