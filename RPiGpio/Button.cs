using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiGpio
{
    public class Button
    {
        public enum ButtonState
        {
            Pressed,
            Released
        };

        GpioPin buttonPin;
        readonly Subject<Unit> whenButtonPressed = new Subject<Unit>();
        readonly Subject<Unit> whenButtonReleased = new Subject<Unit>();
        readonly Subject<GpioPinValue> whenStateChanged = new Subject<GpioPinValue>();

        public ButtonState State { get; private set; }
        public IObservable<Unit> WhenButtonPressed;
        public IObservable<Unit> WhenButtonReleased;
        public IObservable<GpioPinValue> WhenStateChanged => whenStateChanged;

        public Button(int pin)
        {
            // Get the gpio controller
            var gpio = GpioController.GetDefault();

            // Try to open the pin
            gpio.TryOpenPin(pin, GpioSharingMode.Exclusive, out GpioPin gpioPin, out GpioOpenStatus openStatus);

            // If unseuccessful return
            if (gpioPin == null) return;

            // Set the pin
            buttonPin = gpioPin;
            buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            // Subscribe to changes
            buttonPin.ValueChanged += (s, e) =>
            {
                var state = buttonPin.Read();
                State = state == GpioPinValue.High ? ButtonState.Pressed : ButtonState.Released;

                whenStateChanged.OnNext(state);

                if(state == GpioPinValue.High)
                    whenButtonPressed.OnNext(Unit.Default);
                else
                    whenButtonReleased.OnNext(Unit.Default);
            };
        }
    }
}
