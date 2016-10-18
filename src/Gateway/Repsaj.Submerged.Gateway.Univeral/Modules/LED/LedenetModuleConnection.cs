using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System.Net.Sockets;
using System.Net;
using Repsaj.Submerged.GatewayApp.Universal.Models.ConfigurationModels;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.UI;
using System.Text.RegularExpressions;
using Repsaj.Submerged.GatewayApp.Universal.Control.LED;
using Repsaj.Submerged.Gateway.Common.Log;
using Repsaj.Submerged.GatewayApp.Universal.Commands;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.LED
{
    class LedenetModuleConnection : GenericModuleConnectionBase, IModuleConnection
    {
        public override string ModuleType
        {
            get
            {
                return ModuleTypes.LEDENET;
            }
        }

        private LedenetModuleConfiguration _config;
        private string _deviceAddress;
        int _groupPort = 48899;
        int _devicePort = 5577;
        Socket _socket;

        UdpClient _udp;
        RGBWValue[] _program;

        bool isOn;

        ThreadPoolTimer _colorUpdateTimer;
        ThreadPoolTimer _discoverTimeout;

        public LedenetModuleConnection(string name, LedenetModuleConfiguration config) : base (name)
        {
            this._config = config;
        }

        public async Task Init()
        {
            SetModuleStatus(ModuleConnectionStatus.Connecting);

            try
            {
                await DiscoverDevice();

                // connect the socket to the endpoint
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(this._deviceAddress), _devicePort);
                _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(endpoint);

                // request a status update from the device to make sure we're in sync
                RefreshState();

                // calculate the colors per minute to send to the controller
                _program = RGBWHelper.CalculateProgram(this._config.PointsInTime);

                // start the timer which will update the controller every minute
                StartTimer();

                SetModuleStatus(ModuleConnectionStatus.Connected);
            }
            catch (ObjectDisposedException)
            {
                // this was a timeout connection error 
                LogEventSource.Log.Warn("Initializing Ledenet module failed due to connection timeout.");
                SetModuleStatus(ModuleConnectionStatus.Disconnected);
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Initializing Ledenet module failed: " + ex.ToString());
                SetModuleStatus(ModuleConnectionStatus.Disconnected);

                // clean up the socket when needed, garbage collection
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        public async Task Reconnect()
        {
            await Init();
        }  

        void StartTimer()
        {
            _colorUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, new TimeSpan(0, 1, 0));
        }

        void StopTimer()
        {
            _colorUpdateTimer.Cancel();
            _colorUpdateTimer = null;
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            UpdateLED();
        }

        private void UpdateLED()
        { 
            // all times are stored in UTC by default, so always use the UTC number of minutes.
            int time = (int)DateTime.UtcNow.TimeOfDay.TotalMinutes;

            try
            {
                // get the color for this time
                RGBWValue color = _program[time];

                if (color.NoOutput() && isOn)
                {
                    TurnOff();
                }
                else if (!color.NoOutput())
                {
                    if (!isOn)
                        TurnOn();

                    SetRgb(color);
                }
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Exception trying to set color on Ledenet module: " + ex.ToString());
            }
        }

        async Task DiscoverDevice()
        {
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, _groupPort);

            // the broadcast message is a fixed one, do not change
            string msg = "HF-A11ASSISTHREAD";
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);

            // start the timeout timer which will handle a connection timeout
            _discoverTimeout = ThreadPoolTimer.CreateTimer(TimeoutTimer_Tick, new TimeSpan(0, 0, 30));

            _udp = new UdpClient();
            await _udp.SendAsync(msgBytes, msgBytes.Length, broadcastEndPoint);

            do
            {
                // all active devices will reply, select the one we need 
                UdpReceiveResult receiveResult = await _udp.ReceiveAsync();
                _discoverTimeout.Cancel();

                string returnData = Encoding.ASCII.GetString(receiveResult.Buffer);

                if (returnData.Contains(this._config.Device))
                    _deviceAddress = returnData.Substring(0, returnData.IndexOf(','));

            } while (_deviceAddress == null);
        }

        private void TimeoutTimer_Tick(ThreadPoolTimer timer)
        {
            if (_udp != null)
            {
                _udp.Dispose();
                _udp = null;
            }

            SetModuleStatus(ModuleConnectionStatus.Disconnected);
        }

        byte[] ReadRaw(int byte_count = 1024)
        {
            byte[] buffer = new byte[byte_count];
            _socket.Receive(buffer);
            return buffer;
        }

        byte[] ReadResponse(int expected)
        {
            var remaining = expected;
            var rx = new List<byte>();

            while (remaining > 0)
            {
                var chunk = ReadRaw(remaining);
                remaining -= chunk.Length;
                rx.AddRange(chunk);
            }

            return rx.ToArray();
        }

        string DetermineMode(byte level, byte pattern)
        {
            string mode = "unknown";

            if (pattern == 0x61 || pattern == 0x62)
            {
                if (level != 0)
                    return "ww";
                else
                    return "color";
            }
            else if (pattern == 0x60)
            {
                return "custom";
            }

            // did not implement patterns since Submerged doesn't use these
            return mode;
        }

        void RefreshState()
        {
            List<byte> msg = new List<byte>();

            msg.Add(0x81);
            msg.Add(0x8a);
            msg.Add(0x8b);

            SendPacket(msg);
            byte[] rx = ReadResponse(14);

            var power_state = rx[2];
            var power_str = "Unknown power state";

            if (power_state == 0x23)
            {
                this.isOn = true;
                power_str = "ON ";
            }
            else if (power_state == 0x24)
            {
                this.isOn = false;
                power_str = "OFF";
            }

            var pattern = rx[3];
            var ww_level = rx[9];
            var mode = DetermineMode(ww_level, pattern);
            var delay = rx[5];
        }

        void TurnOn(bool on = true)
        {
            List<byte> msg = new List<byte>();
            if (on)
            {
                msg.AddRange(new byte[] { 0x71, 0x23, 0x0f });
            }
            else
            {
                msg.AddRange(new byte[] { 0x71, 0x24, 0x0f });
            }

            SendPacket(msg);
            this.isOn = on;
        }

        void TurnOff()
        {
            TurnOn(false);
        }

        public static byte ComputeAdditionChecksum(byte[] data)
        {
            byte sum = 0;
            unchecked // Let overflow occur without exceptions
            {
                foreach (byte b in data)
                {
                    sum += b;
                }
            }
            return sum;
        }

        void SendPacket(List<byte> data)
        {
            try
            {
                // add the checksum to the package as last byte
                var checksum = ComputeAdditionChecksum(data.ToArray());
                data.Add(checksum);

                byte[] buffer = data.ToArray();
                int result = _socket.Send(buffer, buffer.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Failure sending a packet to Ledenet module: " + ex.ToString());
            }
        }

        void SetRgb(RGBWValue color)
        {
            SetRgb(color.R, color.G, color.B, color.W);
        }

        /// <summary>
        /// Sets the connected controller to the request value
        /// </summary>
        /// <param name="r">Value for RED (0 - 255)</param>
        /// <param name="g">Value for GREEN (0 - 255)</param>
        /// <param name="b">Value for BLUE (0 - 255)</param>
        /// <param name="w">Value for WHITE (0 - 255)</param>
        /// <param name="persist">When true, the controller will persist this value upon rebooting / losing power</param>
        void SetRgb(byte r, byte g, byte b, byte w, bool persist = true)
        {
            List<byte> msg = new List<byte>();

            if (persist)
                msg.Add(0x31);
            else
                msg.Add(0x41);

            msg.Add(r);         // Red
            msg.Add(g);         // Green
            msg.Add(b);         // Blue
            msg.Add(w);         // White

            msg.Add(0x00);
            msg.Add(0x0f);

            SendPacket(msg);            
        }

        public void Dispose()
        {
            if (this._colorUpdateTimer != null)
            {
                this._colorUpdateTimer.Cancel();
                this._colorUpdateTimer = null;
            }

            if (this._discoverTimeout != null)
            {
                this._discoverTimeout.Cancel();
                this._discoverTimeout = null;
            }
        }
        
        public Task ProcessCommand(dynamic command)
        {
            if (command.Action == "TestProgram")
            {
                return TestProgram();
            }

            return Task.FromResult(0);
        }

        private async Task TestProgram()
        {
            // if there's no program or its empty, just return
            if (!(_program?.Length > 0))
                return;

            // stop the timer so it does not interfere
            StopTimer();

            List<RGBWValue> trimmedProgram = new List<RGBWValue>(_program);

            // trim off all rgbw(0,0,0,0) values from the beginning and end of the list
            while (trimmedProgram.First().Equals(RGBWHelper.Blackout))
                trimmedProgram.RemoveAt(0);
            while (trimmedProgram.Last().Equals(RGBWHelper.Blackout))
                trimmedProgram.RemoveAt(trimmedProgram.Count - 1);

            // start with a blackout
            SetRgb(0, 0, 0, 0);
            await Task.Delay(new TimeSpan(0, 0, 1));

            // the entire playback should half a minute, calcute the milliseconds delay between steps
            int delay = 30000 / trimmedProgram.Count;

            // start playback of the current program in fast mode (complete program in 1 minute)
            for (int i = 0; i < trimmedProgram.Count; i++)
            {
                await Task.Delay(new TimeSpan(0, 0, 0, 0, delay));
                RGBWValue value = trimmedProgram[i];
                SetRgb(value);
            }

            // delay for another second because it looks nice
            await Task.Delay(new TimeSpan(0, 0, 1));

            // send the values for NOW again and restart the timer
            UpdateLED();
            StartTimer();
        }
    }
}
