using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using kit_pse84_ai_streaming;
using kit_pse84_ai_streaming.Yolo;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Text;
using static Emgu.CV.ML.KNearest;

namespace ov7675
{
    public partial class MainForm : Form
    {
        private OV7675CDCReader cdcreader;

        private const int width = 320;
        private const int height = 240;

        private const int bytes_per_pix = 2;

        // TODO dynamic
        private const int samplesPerChirp = 128;

        private Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppRgb565);

        private BackgroundNNProcessor nnProcessor;

        private bool storeToC = false;
        private string storeToCFileName = string.Empty;

        private double lastPredictedValue = double.NaN;

        public MainForm()
        {
            InitializeComponent();

            cdcreader = new OV7675CDCReader();
            cdcreader.OnNewConnectionState += Cdcreader_OnNewConnectionState;
            cdcreader.OnNewOV7675 += Cdcreader_OnNewOV7675;
            cdcreader.OnNewNNOutputPacket += Cdcreader_OnNewNNOutputPacket;

            ov7675PictureBox.Image = bmp;

            nnProcessor = new BackgroundNNProcessor();
            nnProcessor.OnProcessedFrame += NnProcessor_OnProcessedFrame;
        }

        private void Cdcreader_OnNewNNOutputPacket(object sender, byte[] data)
        {
            StringBuilder str = new StringBuilder();
            bool doPrediction = true;

            for (int i = 0; i < data.Length; ++i)
            {
                if (data[i] == 0xFF)
                {
                    doPrediction = false;
                    str.AppendFormat("X");
                }
                else str.AppendFormat("{0};", data[i]);
            }
            psocEdgeNNOutputTextBox.Text = str.ToString();

            if (!doPrediction)
            {
                if (!double.IsNaN(lastPredictedValue))
                    predictionSignalsView.updateData(lastPredictedValue);
                return;
            }

            StringBuilder strX = new StringBuilder();
            if (data.Length == 8)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    strX.AppendFormat("{0}", data[i]);
                }

                double volume = Double.Parse(strX.ToString());
                predictionSignalsView.updateData(volume);
                lastPredictedValue = volume;
            }
        }

        private void NnProcessor_OnProcessedFrame(object sender, Bitmap bmp)
        {
            debugPictureBox.Image = bmp;
        }

        private static void StoreCFileTask(byte[] data, string path)
        {
            System.Diagnostics.Debug.WriteLine("Storing to C file...");
            FileGenerator.GenerateContent(path, data);
            System.Diagnostics.Debug.WriteLine("Done");
        }

        private void Cdcreader_OnNewOV7675(object sender, byte[] data)
        {
            if (storeToC)
            {
                storeToC = false;
                Task.Run(() => StoreCFileTask((byte[])data.Clone(), storeToCFileName));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int data_count = width * height * bytes_per_pix;
            if (data_count != data.Length)
            {
                Debug.WriteLine("Problem...");
                return;
            }

            // Bytes to Bitmap
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly,
                bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, data_count);
            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);

            // Clone it before updating (avoid some memory problems)
            Bitmap toProcess = (Bitmap)bmp.Clone();

            // Image src of ov7675PictureBox is bmp
            ov7675PictureBox.Invalidate();

            nnProcessor.Feed(toProcess);

            //// Convert from RGB565 to 24 bits per pixel RGB
            //// required for Open CV
            //startTime = Stopwatch.GetTimestamp();
            //using (Graphics gr = Graphics.FromImage(bmp24))
            //{
            //    gr.DrawImage(bmp, new Rectangle(0, 0, bmp24.Width, bmp24.Height));
            //}
            //Debug.WriteLine(string.Format("Conversion to 24bits {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));


            //// Convert Bitmap to Mat
            //Mat srcMat;
            //startTime = Stopwatch.GetTimestamp();
            //{
            //    srcMat = BitmapExtension.ToMat(bmp24);
            //}
            //Debug.WriteLine(string.Format("Bitmap -> Mat {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));

            //// Resize to 320x320
            //Mat resized;
            //startTime = Stopwatch.GetTimestamp();
            //{
            //    resized = new Mat();
            //    CvInvoke.CopyMakeBorder(srcMat, resized, 40, 40, 0, 0, BorderType.Constant, new MCvScalar(0, 0, 0));
            //}
            //Debug.WriteLine(string.Format("Resize to 320x320 {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));
            //// debugPictureBox.Image = resized.ToBitmap();

            ////startTime = Stopwatch.GetTimestamp();
            ////{
            ////    var inputTensor = Yolo.ExtractPixelsGood(resized.ToBitmap());
            ////}
            ////Debug.WriteLine(string.Format("Yolo.ExtractPixels {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));

            //if (refCounter == 0)
            //{
            //    refCounter = 1;

            //    List<YoloPrediction> predictions;
            //    DenseTensor<float> inputTensor;

            //    startTime = Stopwatch.GetTimestamp();
            //    {
            //        inputTensor = Yolo.ExtractPixels(resized.ToBitmap());
            //    }
            //    Debug.WriteLine(string.Format("Yolo.ExtractPixels {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));

            //    startTime = Stopwatch.GetTimestamp();
            //    {
            //        using var results = session.Run([NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)]);
            //        var outputTensor = results.First().AsTensor<float>();
            //        Debug.WriteLine($"Output shape: [{outputTensor.Dimensions[0]}, {outputTensor.Dimensions[1]}, {outputTensor.Dimensions[2]}]");
            //        predictions = Yolo.ParseResults(outputTensor);
            //        predictions = Yolo.Suppress(predictions);
            //    }
            //    Debug.WriteLine(string.Format("Inference takes {0}ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds));

            //    foreach (var prediction in predictions)
            //    {
            //        Rectangle detectedArea = new Rectangle((int)prediction.rectangle.X,
            //            (int)prediction.rectangle.Y,
            //            (int)prediction.rectangle.Width,
            //            (int)prediction.rectangle.Height);
            //        CvInvoke.Rectangle(resized, detectedArea, new MCvScalar(0, 255, 0), 2);

            //        Debug.WriteLine(string.Format("{0}:{1}:{2}:{3}", detectedArea.X, detectedArea.Y, detectedArea.Width, detectedArea.Height));
            //        Debug.WriteLine(string.Format("Class: {0} ", prediction.label));

            //        CvInvoke.PutText(resized,
            //            prediction.label,
            //            new Point((int)prediction.rectangle.X, (int)prediction.rectangle.Y - 5),
            //            FontFace.HersheyPlain, 2.0, new Bgr(Color.Green).MCvScalar, 2);

            //    }

            //    debugPictureBox.Image = resized.ToBitmap();

            //}
            //else
            //{
            //    refCounter--;
            //}

            //    return;


            //if (refCounter == 0)
            //{

            //    Mat resized1f2da = new Mat();
            //    CvInvoke.Imdecode(data, ImreadModes.Unchanged, resized1f2da);

            //    debugPictureBox.Image = resized1f2da.ToBitmap();

            //    return;

            //    // Mat resized2 = new Mat();
            //    // CvInvoke.CopyMakeBorder(bmp, resized2, 40, 40, 0, 0, BorderType.Constant, new MCvScalar(0, 0, 0));

            //    // Apply model
            //    Mat img = BitmapExtension.ToMat(bmp);
            //    Mat resized1 = new Mat();
            //    // CvInvoke.CvtColor(img, resized1, ColorConversion. )
            //    //Mat resized = new Mat();

            //    string _inputName = session.InputMetadata.Keys.First();

            //    // Resize it (compatible with the model)
            //    // from 320x240 to 320 x 320 -> add 40 and 40 margin
            //    CvInvoke.CopyMakeBorder(img, resized, 40, 40, 0, 0, BorderType.Constant, new MCvScalar(0, 0, 0));

            //    var inputTensor = Yolo.ExtractPixels(resized.ToBitmap());

            //    using var results = session.Run([NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)]);

            //    var outputTensor = results.First().AsTensor<float>();

            //    Debug.WriteLine($"Output shape: [{outputTensor.Dimensions[0]}, {outputTensor.Dimensions[1]}, {outputTensor.Dimensions[2]}]");

            //    List<YoloPrediction> predictions = Yolo.ParseResults(outputTensor);

            //    predictions = Yolo.Suppress(predictions);
            //    foreach (var prediction in predictions)
            //    {
            //        Rectangle detectedArea = new Rectangle((int)prediction.rectangle.X,
            //            (int)prediction.rectangle.Y,
            //            (int)prediction.rectangle.Width,
            //            (int)prediction.rectangle.Height);
            //        CvInvoke.Rectangle(resized, detectedArea, new MCvScalar(0, 255, 0), 2);

            //        Debug.WriteLine(string.Format("{0}:{1}:{2}:{3}", detectedArea.X, detectedArea.Y, detectedArea.Width, detectedArea.Height));
            //        Debug.WriteLine(string.Format("Class: {0} ", prediction.label));

            //        CvInvoke.PutText(resized,
            //            prediction.label,
            //            new Point((int)prediction.rectangle.X, (int)prediction.rectangle.Y - 5),
            //            FontFace.HersheyPlain, 2.0, new Bgr(Color.Green).MCvScalar, 2);

            //    }

            //    ov7675PictureBox.Image = resized.ToBitmap();

            //    refCounter = 10;
            //}
            //else
            //{
            //    refCounter--;
            //}
        }

        private void Cdcreader_OnNewConnectionState(object sender, OV7675CDCReader.ConnectionState state)
        {
            // System.Diagnostics.Debug.WriteLine("CDC New connection state: " + state.ToString());
            switch (state)
            {
                case OV7675CDCReader.ConnectionState.Connected:
                    connectButton.Enabled = false;
                    comPortComboBox.Enabled = false;
                    disconnectButton.Enabled = true;
                    statusTextBox.Text = "Connected";
                    statusTextBox.BackColor = Color.Green;
                    break;

                case OV7675CDCReader.ConnectionState.Error:
                    connectButton.Enabled = true;
                    comPortComboBox.Enabled = true;
                    disconnectButton.Enabled = false;
                    statusTextBox.Text = "Error";
                    statusTextBox.BackColor = Color.Red;
                    break;

                case OV7675CDCReader.ConnectionState.Iddle:
                    connectButton.Enabled = true;
                    comPortComboBox.Enabled = true;
                    disconnectButton.Enabled = false;
                    statusTextBox.Text = "Iddle";
                    statusTextBox.BackColor = Color.Gray;
                    break;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load the possible com ports
            string[] serialPorts = SerialPort.GetPortNames();
            comPortComboBox.DataSource = serialPorts;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if ((comPortComboBox.SelectedIndex < 0) || (comPortComboBox.SelectedIndex >= comPortComboBox.Items.Count)) return;

            var selectedItem = comPortComboBox.Items[comPortComboBox.SelectedIndex];
            if (selectedItem != null)
            {
                string? portName = selectedItem.ToString();
                if (portName != null)
                {
                    cdcreader.SetPortName(portName);
                    connectButton.Enabled = false;
                    comPortComboBox.Enabled = false;
                }
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            cdcreader.Disconnect();
            disconnectButton.Enabled = false;
        }


        private void savePictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "*";
            dlg.DefaultExt = "bmp";
            dlg.ValidateNames = true;
            dlg.Filter = "Bitmap Image (.bmp)|*.bmp";

            if (dlg.ShowDialog() != DialogResult.OK) return;

            ov7675PictureBox.Image.Save(dlg.FileName);
        }

        private void storeForCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Store in a text file that can be imported inside a project
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "*";
            dlg.DefaultExt = "c";
            dlg.ValidateNames = true;
            dlg.Filter = "C file (.c)|*.c";

            if (dlg.ShowDialog() != DialogResult.OK) return;

            storeToCFileName = dlg.FileName;
            storeToC = true;
        }
    }
}
