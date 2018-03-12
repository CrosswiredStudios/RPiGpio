using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiGpio.Controllers
{
    public class Drv8825
    {
        public enum StepDirection { Clockwise, CounterClockwise };
        public enum StepMode { Full, Half, Quarter, Eigth, Sixteenth, ThirtySecond}
        
        bool isEnabled;

        public StepDirection Direction { get; private set; }
        public GpioPin DirectionPin { get; }
        public GpioPin EnablePin { get; }
        public StepMode Mode { get; private set; }
        public float StepAngle { get; }
        public GpioPin StepPin { get; }

        public Drv8825(int directionPin, int enablePin, int stepPin)
        {
            var gpioController = GpioController.GetDefault();

            DirectionPin = gpioController.OpenPin(directionPin);
            DirectionPin.Write(GpioPinValue.Low);
            DirectionPin.SetDriveMode(GpioPinDriveMode.Output);
            
            EnablePin = gpioController.OpenPin(enablePin);
            EnablePin.Write(GpioPinValue.Low);
            EnablePin.SetDriveMode(GpioPinDriveMode.Output);
            
            
            StepPin = gpioController.OpenPin(stepPin);
            StepPin.Write(GpioPinValue.Low);
            StepPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void Initialize(StepDirection direction = StepDirection.Clockwise, StepMode stepMode = StepMode.Full, bool isEnabled = true)
        {
            Direction = direction;
            IsEnabled = isEnabled;
            Mode = stepMode;
            //StepperMotor = stepperMotor;
        }

        public bool IsEnabled
        {
            get => IsEnabled;
            set
            {

            }
        }

        /// <summary>
        /// Steps the motor in the current direction
        /// </summary>
        /// <param name="numberOfSteps">The number of steps to take</param>
        public void Step(int numberOfSteps)
        {

        }
    }
}
