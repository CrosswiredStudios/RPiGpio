using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiGpio
{
    public sealed class Input
    {
        public GpioPin GpioPin { get; }

        public Input(GpioController gpioController, int gpioId, GpioPinDriveMode mode = GpioPinDriveMode.Input, long debounceTimeout = 20, bool debug = false)
        {
            GpioPin = gpioController.OpenPin(gpioId);
            GpioPin.DebounceTimeout = TimeSpan.FromMilliseconds(debounceTimeout);
            GpioPin.SetDriveMode(mode);
            GpioPin.ValueChanged += (pin, args) => {
                if(debug) Debug.WriteLine("RPiGpio Input: Value changed" + args.Edge);
            };
        }
    }
}
