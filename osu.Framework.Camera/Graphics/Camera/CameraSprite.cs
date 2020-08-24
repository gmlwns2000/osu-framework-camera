// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Framework.Graphics.Camera
{
    public class CameraSprite : Sprite
    {
        private Task cameraLoopTask;
        private CancellationTokenSource cameraLoopTaskCanellationTokenSource;
        private VideoCapture capture;
        private readonly Mat image;

        private int cameraID = -1;
        public int CameraID
        {
            get => cameraID;
            set
            {
                if (cameraID == value) return;

                if (IsLoaded)
                    stopRecording();

                cameraID = value;
                capture?.Dispose();
                capture = new VideoCapture(cameraID);

                if (IsLoaded)
                    startRecording();
            }
        }

        public byte[] TextureData { get; private set; }

        public CameraSprite(int cameraID = 0)
        {
            CameraID = cameraID;
            image = new Mat();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            startRecording();
        }

        private void cameraLoop(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (CameraID == -1)
                    continue;

                capture?.Read(image);

                if (!(image?.Empty() ?? true))
                {
                    TextureData = image.ToBytes();
                    Texture = Texture.FromStream(new MemoryStream(TextureData));
                }
            }
        }

        private void startRecording()
        {
            cameraLoopTaskCanellationTokenSource = new CancellationTokenSource();
            cameraLoopTask = Task.Factory.StartNew(() => cameraLoop(cameraLoopTaskCanellationTokenSource.Token), cameraLoopTaskCanellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void stopRecording()
        {
            cameraLoopTaskCanellationTokenSource.Cancel();
            cameraLoopTask.Wait();
            cameraLoopTask.Dispose();
            cameraLoopTaskCanellationTokenSource.Dispose();

            TextureData = null;
            cameraLoopTask = null;
            cameraLoopTaskCanellationTokenSource = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            stopRecording();

            capture?.Dispose();
            image?.Dispose();
        }
    }
}