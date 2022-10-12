using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Kinect;
using Coding4Fun.Kinect.WinForm;

namespace Laberinto
{
    public partial class FormKinect : Form
    {
        private KinectSensor sensor;
        public FormKinect()
        {
            InitializeComponent();
        }

        
        private void BtnStart_Click(object sender, EventArgs e)
        {
            

            if (btnStart.Text == "Start")
            {
                if (KinectSensor.KinectSensors.Count > 0)
                {
                    this.btnStart.Text = "Stop";
                    sensor = KinectSensor.KinectSensors[0];
                    KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                }

                sensor.Start();
                this.lblConID.Text = sensor.DeviceConnectionId;
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                //sensor.ColorFrameReady += Sensor_ColorFrameReady;
                sensor.DepthStream.Enable();
                //sensort.setDepthClipping(bDepthClippingNear, bDepthClippingFar);
                //sensor.DepthStream.Range = DepthRange.Near;
                sensor.AllFramesReady += Sensor_AllFramesReady;
                sensor.SkeletonStream.Enable();
                sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            }
            else
            {
                if(sensor != null && sensor.IsRunning)
                {
                    sensor.Stop();
                    this.btnStart.Text = "Start";
                    this.pbStream.Image = null;
                }
            }
        }

        private void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
                if (frame != null)
                {
                    pbStream.Image = CreateBitmapFromSensor(frame);
                }
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;
                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                var TrackedSkeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                if (TrackedSkeleton == null)
                    return;

                var position = TrackedSkeleton.Joints[JointType.HandRight].Position;
                var coordinateMapper = new CoordinateMapper(sensor);
                var colorPoint = coordinateMapper.MapSkeletonPointToColorPoint(position, ColorImageFormat.InfraredResolution640x480Fps30);
                this.lblPosition.Text = string.Format("Hand position X: {0}, Y: {1}", colorPoint.X, colorPoint.Y);

                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(colorPoint.X * 3, colorPoint.Y * 3);
            }
        }


        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.lblStatus.Text = sensor.Status.ToString();
        }

        private Bitmap CreateBitmapFromSensor(ColorImageFrame frame)
        {

            var pixelData = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixelData);

            CrazyEffect(pixelData);
            return pixelData.ToBitmap(frame.Width, frame.Height);

            //var stride = frame.Width * frame.BytesPerPixel;

            //var bmpFrame = new Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //var bmpData = bmpFrame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmpFrame.PixelFormat);

            //System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, bmpData.Scan0, frame.PixelDataLength);

            //bmpFrame.UnlockBits(bmpData);
            //return bmpFrame;
        }

        private void CrazyEffect(byte[] pixelData)
        {
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                var b = pixelData[i];
                var g = pixelData[i + 1];
                var r = pixelData[i + 2];

                r = (byte) (b * 2);
                g = (byte) (r / 0.5);
                b = (byte)(r * g + b);

                pixelData[i] = r;
                pixelData[i + 1] = b;
                pixelData[i + 2] = g;
            }
        }

        private void pictureBox17_MouseEnter(object sender, EventArgs e)
        {
            MessageBox.Show("Golpeaste la pared, Perdiste");
        }

        private void pictureBox19_MouseEnter(object sender, EventArgs e)
        {
            MessageBox.Show("¡Lo lograste!");
        }
    }
}
