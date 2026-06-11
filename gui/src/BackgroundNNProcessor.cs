using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using kit_pse84_ai_streaming.Yolo;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kit_pse84_ai_streaming
{
    public class BackgroundNNProcessor
    {
        public delegate void OnProcessedFrameEventHandler(object sender, Bitmap bmp);
        public event OnProcessedFrameEventHandler? OnProcessedFrame;

        /// <summary>
        /// Background worker enabling background operations
        /// </summary>
        private BackgroundWorker? worker;

        private Bitmap? bitmap;
        private object sync = new object();

        private const int WORKER_REPORT_BMP = 10;

        public void Feed(Bitmap toProcess)
        {
            lock(sync)
            {
                bitmap = toProcess;
            }
        }

        public BackgroundNNProcessor()
        {
            CreateBackgroundWorkerAndStart();
        }

        /// <summary>
        /// Create and start the background worker
        /// </summary>
        private void CreateBackgroundWorkerAndStart()
        {
            if (worker != null)
            {
                return;
            }

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork; ;
            worker.ProgressChanged += Worker_ProgressChanged; ;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (sender == null) return;

            const int WIDTH = 320;
            const int HEIGHT = 240;
            Bitmap bmp24 = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb);
            Mat resized = new Mat();

            // var session = new InferenceSession("models\\best.onnx");
            var session = new InferenceSession("models\\best_bw.onnx");
            var _inputName = session.InputMetadata.Keys.First();

            BackgroundWorker worker = (BackgroundWorker)sender;
            for(; ;)
            {
                Bitmap? toProcess = null;

                lock(sync)
                {
                    if (bitmap != null) toProcess = (Bitmap)bitmap.Clone();
                    bitmap = null;
                }

                if (toProcess == null)
                {
                    Thread.Sleep(10);
                    continue;
                }

                // Convert from RGB565 to 24 bits per pixel RGB
                // required for Open CV
                using (Graphics gr = Graphics.FromImage(bmp24))
                {
                    gr.DrawImage(toProcess, new Rectangle(0, 0, bmp24.Width, bmp24.Height));
                }


                // Convert Bitmap to Mat
                Mat srcMat1 = BitmapExtension.ToMat(bmp24);
                Mat srcMat2 = new Mat();
                CvInvoke.CvtColor(srcMat1, srcMat2, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                Mat srcMat = new Mat();

                // Adaptive threshold
                CvInvoke.AdaptiveThreshold(srcMat2, srcMat, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 11, 3);

                // Crop (as in C)
                CvInvoke.Rectangle(srcMat, new Rectangle(0, 0, 30, 240), new MCvScalar(255, 255, 255), -1);
                CvInvoke.Rectangle(srcMat, new Rectangle(310, 0, 10, 240), new MCvScalar(255, 255, 255), -1);
                CvInvoke.Rectangle(srcMat, new Rectangle(0, 0, 320, 130), new MCvScalar(255, 255, 255), -1);
                CvInvoke.Rectangle(srcMat, new Rectangle(0, 230, 320, 10), new MCvScalar(255, 255, 255), -1);

                // Resize to 320x320
                resized = new Mat();
                CvInvoke.CopyMakeBorder(srcMat, resized, 40, 40, 0, 0, BorderType.Constant, new MCvScalar(0, 0, 0));

                var inputTensor = Yolo.Yolo.ExtractPixels(resized.ToBitmap());

                using var results = session.Run([NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)]);
                var outputTensor = results.First().AsTensor<float>();
                var predictions = Yolo.Yolo.ParseResults(outputTensor);
                predictions = Yolo.Yolo.Suppress(predictions);

                // Convert back to colored image for display
                Mat displayMat = new Mat();
                CvInvoke.CvtColor(resized, displayMat, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);

                foreach (var prediction in predictions)
                {
                    Rectangle detectedArea = new Rectangle((int)prediction.rectangle.X,
                        (int)prediction.rectangle.Y,
                        (int)prediction.rectangle.Width,
                        (int)prediction.rectangle.Height);
                    // CvInvoke.Rectangle(resized, detectedArea, new MCvScalar(0, 255, 0), 2);
                    CvInvoke.Rectangle(displayMat, detectedArea, new MCvScalar(0, 255, 0), 2);

                    //Debug.WriteLine(string.Format("{0}:{1}:{2}:{3}", detectedArea.X, detectedArea.Y, detectedArea.Width, detectedArea.Height));
                    //Debug.WriteLine(string.Format("Class: {0} ", prediction.label));

                    //CvInvoke.PutText(resized,
                    //    prediction.label,
                    //    new Point((int)prediction.rectangle.X, (int)prediction.rectangle.Y - 5),
                    //    FontFace.HersheyPlain, 2.0, new Bgr(Color.Green).MCvScalar, 2);
                    CvInvoke.PutText(displayMat,
                        prediction.label,
                        new Point((int)prediction.rectangle.X, (int)prediction.rectangle.Y - 5),
                        FontFace.HersheyPlain, 2.0, new Bgr(Color.Green).MCvScalar, 2);
                }

                //worker.ReportProgress(WORKER_REPORT_BMP, resized.ToBitmap());
                worker.ReportProgress(WORKER_REPORT_BMP, displayMat.ToBitmap());
            }
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            switch(e.ProgressPercentage)
            {
                case WORKER_REPORT_BMP:
                    if (e.UserState != null)
                    {
                        OnProcessedFrame?.Invoke(this, (Bitmap)e.UserState);
                    }
                    break;
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            // Do something?
        }
    }
}
