using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Devices
{
    public sealed class Relay
    {
        public enum RelayConfig { NormallyClosed, NormallyOpen };

        public RelayConfig Config { get; set; }

        public GpioPin GpioPin { get; set; }

        public GpioPinValue State { get; set; }

        public Relay(GpioController _gpioController,
                     int gpioId,
                     RelayConfig config,  
                     GpioPinValue state)
        {
            GpioPin = _gpioController.OpenPin(gpioId);
            GpioPin.SetDriveMode(GpioPinDriveMode.Output);
            GpioPin.Write(state);
            Config = config;
            State = state;
        }

        /// <summary>
        /// Close the relay.
        /// </summary>
        public void Close()
        {
            SetState(Config == RelayConfig.NormallyClosed ? GpioPinValue.Low : GpioPinValue.High);
        }

        /// <summary>
        /// Open the relay.
        /// </summary>
        public void Open()
        {
            SetState(Config == RelayConfig.NormallyOpen ? GpioPinValue.Low : GpioPinValue.High);
        }

        /// <summary>
        /// Set the relay state.
        /// </summary>
        /// <param name="relayState"></param>
        public void SetState(GpioPinValue relayState)
        {
            if (State != relayState)
            {
                GpioPin.Write(relayState);
                State = relayState;
            }
        }
    }
}
