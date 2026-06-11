using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ov7675
{
    public class OV7675CDCReader
    {
        private const byte TYPE_RADAR = 1;
        private const byte TYPE_UCAM = 2;
        private const byte TYPE_THERMOSENSOR = 3;

        private const int WORKER_THERMOSENSOR_DATA = 1;

        private const int WORKER_OV7675_PACKET = 10;
        private const int WORKER_NN_OUTPUT_PACKET = 12;

        public const int COM_OVERHEAD = 8;

        private int[] POSSIBLE_LENGTHS = new int[] { 153600, 4096 };

        public enum ConnectionState
        {
            Iddle,
            Connected,
            Error
        }

        public delegate void OnNewConnectionStateEventHandler(object sender, ConnectionState state);
        public event OnNewConnectionStateEventHandler? OnNewConnectionState;

        public delegate void OnNewOV7675PacketEventHandler(object sender, byte[] data);
        public event OnNewOV7675PacketEventHandler? OnNewOV7675;

        public delegate void OnNewNNOutputPacketEventHandler(object sender, byte[] data);
        public event OnNewNNOutputPacketEventHandler? OnNewNNOutputPacket;

        /// <summary>
        /// Serial port used for the communication
        /// </summary>
        private SerialPort? port;

        /// <summary>
        /// Background worker enabling background operations
        /// </summary>
        private BackgroundWorker? worker;

        private bool stopRequest = false;

        private object sync = new object();

        /// <summary>
        /// Store CRC table for fast computation
        /// </summary>
        private byte[] crcTable;

        public OV7675CDCReader()
        {
            crcTable = GenerateTable();
        }

        public void SetPortName(string portName)
        {
            try
            {
                port = new SerialPort
                {
                    BaudRate = 1000000,
                    DataBits = 8,
                    Handshake = Handshake.None,
                    Parity = Parity.None,
                    PortName = portName,
                    StopBits = StopBits.One,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                port.Open();
            }
            catch (Exception)
            {
                OnNewConnectionState?.Invoke(this, ConnectionState.Error);
                return;
            }

            OnNewConnectionState?.Invoke(this, ConnectionState.Connected);

            CreateBackgroundWorker();
            if (worker != null) worker.RunWorkerAsync();
        }

        public void Disconnect()
        {
            lock(sync)
            {
                stopRequest = true;
            }
        }

        private void CreateBackgroundWorker()
        {
            if (this.worker != null)
            {
                this.worker.CancelAsync();
            }

            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += Worker_DoWork;
            this.worker.ProgressChanged += Worker_ProgressChanged;
            this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        /// <summary>
        /// Compute CRC
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex">Start index (included)</param>
        /// <param name="stopIndex">Stop index (excluded)</param>
        /// <returns></returns>
        private static byte CrcStd(byte[] buffer, int startIndex, int stopIndex)
        {
            byte crc = 0x15;
            int j = 0;
            for (j = startIndex; j < stopIndex; ++j)
            {
                byte tmp = (byte)(buffer[j] ^ crc);
                int i = 0;
                for (i = 0; i < 8; ++i)
                {
                    byte lsb = (byte)(tmp & 0x01);
                    tmp >>= 1;
                    if (lsb != 0) tmp ^= 0x8C; //polynome 0x8C
                }
                crc = (byte)tmp;
            }

            return crc;
        }

        public byte[] GenerateTable()
        {
            byte[] csTable = new byte[256];
            byte poly = 0x8c;
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    byte lsb = (byte)(temp & 0x01);
                    temp >>= 1;
                    if (lsb != 0) temp ^= poly;
                }
                csTable[i] = (byte)temp;
            }
            return csTable;
        }

        private byte Crc(byte[] buff, int startIndex, int stopIndex)
        {
            byte c = 0x15;
            for (int j = startIndex; j < stopIndex; ++j)
            {
                c = crcTable[c ^ buff[j]];
            }
            return c;
        }

        private int readHeader(byte[] header, out byte counter, out int packetLength, out byte crc)
        {
            // Default
            packetLength = -1;
            crc = 0;
            counter = 0;

            // Header is COM_OVERHEAD bytes
            if ((header == null) || (header.Length != COM_OVERHEAD)) return -1;
            if (port == null) return -2;

            int readIndex = 0;
            int remaining = COM_OVERHEAD;

            try
            {
                for (; ; )
                {
                    int read_bytes = port.Read(header, readIndex, remaining);
                    readIndex += read_bytes;
                    remaining -= read_bytes;
                    if (remaining == 0) break;
                }
            }
            catch(Exception)
            {
                return -3;
            }

            // Check content
            if ((header[0] != 0x55) || (header[1] != 0x55)) return -4;

            // Get counter
            counter = header[2];

            // Get length
            packetLength = BitConverter.ToInt32(header, 3);

            // TODO check if length is plausible or not

            // Get CRC
            crc = header[7];

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private int readPacket(byte[] packet)
        {
            if (port == null) return -1;
            if ((packet == null) ||(packet.Length == 0))  return -2; 

            int readIndex = 0;
            int remaining = packet.Length;
            try
            {
                for (; ; )
                {
                    // TODO: add try/catch here
                    int read_bytes = port.Read(packet, readIndex, remaining);
                    readIndex += read_bytes;
                    remaining -= read_bytes;
                    if (remaining == 0) break;
                }
            }
            catch(Exception)
            {
                return -3;
            }

            return 0;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (sender == null) return;
            if (port == null) return;

            BackgroundWorker worker = (BackgroundWorker)sender;

            // Stop
            byte[] stopBuffer = new byte[1] { 50 };
            port.Write(stopBuffer, 0, 1);

            System.Threading.Thread.Sleep(500);

            for (; ; )
            {
                string cnt = port.ReadExisting();
                if (cnt.Length == 0) break;
            }

            // Start and read
            byte[] startBuffer = new byte[1] { 49 };
            port.Write(startBuffer, 0, 1);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            byte[] headerBuffer = new byte[COM_OVERHEAD];

            for(; ;)
            {
                bool doStop = false;
                lock(sync)
                {
                    if (stopRequest)
                    {
                        doStop = true;
                        stopRequest = false;

                        port.Write(stopBuffer, 0, 1);

                        port.Close();
                        return;
                    }
                }

                if (doStop)
                {
                    return;
                }

                // Reader header buffer
                byte packetCounter = 0;
                int packetLength = 0;
                byte crc = 0;

                int retval = readHeader(headerBuffer, out packetCounter, out packetLength, out crc);
                if (retval != 0)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("readHeader returns {0}", retval));

                    port.Write(stopBuffer, 0, 1);
                    System.Threading.Thread.Sleep(500);

                    for (; ; )
                    {
                        string cnt = port.ReadExisting();
                        if (cnt.Length == 0) break;
                    }

                    port.Write(startBuffer, 0, 1);

                    continue;
                }

                // Allocate memory
                byte[] packetBuffer = new byte[packetLength];

                // Read packet
                retval = readPacket(packetBuffer);
                if (retval != 0)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("readPacket returns {0}", retval));

                    port.Write(stopBuffer, 0, 1);
                    System.Threading.Thread.Sleep(500);

                    for (; ; )
                    {
                        string cnt = port.ReadExisting();
                        if (cnt.Length == 0) break;
                    }

                    port.Write(startBuffer, 0, 1);

                    continue;
                }

                // check CRC
                byte computedCRC = Crc(packetBuffer, 0, packetLength);
                if (computedCRC != crc)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("CRC mismatch. Computed {0} but got {1}", computedCRC, crc));

                    port.Write(stopBuffer, 0, 1);
                    System.Threading.Thread.Sleep(500);

                    for (; ; )
                    {
                        string cnt = port.ReadExisting();
                        if (cnt.Length == 0) break;
                    }

                    port.Write(startBuffer, 0, 1);

                    continue;
                }

                if ((packetLength == 153600) || (packetLength == 614400))
                {
                    // Pic
                    worker.ReportProgress(WORKER_OV7675_PACKET, packetBuffer);
                }
                else if (packetLength == 8)
                {
                    worker.ReportProgress(WORKER_NN_OUTPUT_PACKET, packetBuffer);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("other {0}", packetLength));

                    port.Write(stopBuffer, 0, 1);
                    System.Threading.Thread.Sleep(500);

                    for (; ; )
                    {
                        string cnt = port.ReadExisting();
                        if (cnt.Length == 0) break;
                    }

                    port.Write(startBuffer, 0, 1);

                    continue;
                }
            }
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            switch(e.ProgressPercentage)
            {
                case WORKER_OV7675_PACKET:
                    if (e.UserState != null)
                    {
                        OnNewOV7675?.Invoke(this, (byte[])e.UserState);
                    }
                    break;
                case WORKER_NN_OUTPUT_PACKET:
                    if (e.UserState != null)
                    {
                        OnNewNNOutputPacket?.Invoke(this, (byte[])e.UserState);
                    }
                    break;
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            OnNewConnectionState?.Invoke(this, ConnectionState.Iddle);
        }
    }
}
