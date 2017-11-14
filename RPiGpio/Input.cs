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

        public Input(GpioController gpioController, int gpioId)
        {
            GpioPin = gpioController.OpenPin(gpioId);
            GpioPin.SetDriveMode(GpioPinDriveMode.Input);
            GpioPin.ValueChanged += (pin, args) => {
                Debug.WriteLine("RPiGpio Input: Value changed" + args.Edge);
            };
        }
    }
}
