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

               
        bool isEnabled;

        public StepDirection Direction { get; }
        public GpioPin DirectionPin { get; }
        public GpioPin EnablePin { get; }
        public GpioPin StepPin { get; }

        public Drv8825(int directionPin, int enablePin, int stepPin, StepDirection direction = StepDirection.Clockwise)
        {
            var gpioController = GpioController.GetDefault();

            DirectionPin = gpioController.OpenPin(directionPin);
            EnablePin = gpioController.OpenPin(enablePin);
            StepPin = gpioController.OpenPin(stepPin);
            Direction = direction;
            isEnabled = false;
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
