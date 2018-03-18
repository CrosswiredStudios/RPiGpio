using System;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace RPiGpio.Controllers
{
    public enum MotorState { Off, On }
    public enum MotorDirection { Clockwise, CounterClockwise }

    /// <summary>
    /// A motor in relation to the L298N motor driver.
    /// </summary>
    public class Motor
    {
        GpioPin enx;
        MotorDirection direction;
        GpioPin pin1, pin2;
        MotorState state;

        public MotorDirection Direction
        {
            get => direction;
            set
            {
                if (direction == value) return;
                direction = value;
                UpdateState();
            }
        }
        public MotorState State
        {
            get => state;
            set
            {
                if (state == value) return;
                state = value;
                UpdateState();
            }
        }

        /// <summary>
        /// A motor in relation to the L298N motor driver
        /// </summary>
        /// <param name="pin1">Reference to direction pin 1.</param>
        /// <param name="pin2">Reference to direction pin 2.</param>
        /// <param name="direction">The starting direction</param>
        /// <param name="state">The starting state</param>
        /// <param name="enx">Reference to the engage pin.</param>
        public Motor(GpioPin pin1, GpioPin pin2, MotorDirection direction = MotorDirection.Clockwise, MotorState state = MotorState.Off, GpioPin enx = null)
        {
            this.direction = direction;            

            this.enx = enx;
            this.enx?.Write(GpioPinValue.High);
            this.enx?.SetDriveMode(GpioPinDriveMode.Output);

            this.pin1 = pin1;
            this.pin1?.Write(GpioPinValue.Low);
            this.pin1?.SetDriveMode(GpioPinDriveMode.Output);

            this.pin2 = pin2;
            this.pin2?.Write(GpioPinValue.Low);
            this.pin2?.SetDriveMode(GpioPinDriveMode.Output);

            State = state;
        }

        /// <summary>
        /// Flips the correct bits based on the direction and on/off state.
        /// </summary>
        void UpdateState()
        {
            if (state == MotorState.On)
            {
                pin1?.Write(direction == MotorDirection.Clockwise ? GpioPinValue.High : GpioPinValue.Low);
                pin2?.Write(direction == MotorDirection.Clockwise ? GpioPinValue.Low : GpioPinValue.High);
            }
            else
            {
                pin1?.Write(GpioPinValue.Low);
                pin2?.Write(GpioPinValue.Low);
            }
        }
    }

    /// <summary>
    /// The L298N dual motor driver.
    /// </summary>
    public class L298n
    {
        public enum MotorOutput { OneTwo, ThreeFour }

        GpioController gpioController;
        public Motor Motor1 { get; private set; }
        public Motor Motor2 { get; private set; }
        bool initiated;

        /// <summary>
        /// The L298N dual motor driver.
        /// </summary>
        /// <param name="gpioController">GpioController reference</param>
        public L298n(GpioController gpioController)
        {
            this.gpioController = gpioController;
            initiated = false;
        }

        /// <summary>
        /// Initiates the L298N.
        /// </summary>
        /// <param name="in1">Directional input 1 gpio pin</param>
        /// <param name="in2">Directional input 2 gpio pin</param>
        /// <param name="in3">Directional input 3 gpio pin</param>
        /// <param name="in4">Directional input 4 gpio pin</param>
        /// <param name="ena">Speed output A gpio pin</param>
        /// <param name="enb">Speed output B gpio pin</param>
        public void Initiate(int in1, int in2, int in3 = -1, int in4 = -1, int ena = -1, int enb = -1)
        {
            try
            {
                Debug.Write($"Initiating L298N motor 1: ");
                if (gpioController.TryOpenPin(in1, GpioSharingMode.Exclusive, out GpioPin pin, out GpioOpenStatus openStatus))
                {
                    if (gpioController.TryOpenPin(in2, GpioSharingMode.Exclusive, out GpioPin pin2, out GpioOpenStatus openStatus2))
                    {
                        Motor1 = new Motor(pin, pin2);
                        Debug.WriteLine($"Success");
                    }
                    else
                    {
                        Debug.WriteLine($"Fail");
                    }
                }
                else
                    Debug.WriteLine($"Fail");

                if (in3 != -1 && in4 != -1)
                {
                    Debug.Write($"Initiating L298N motor 2: ");
                    if (gpioController.TryOpenPin(in3, GpioSharingMode.Exclusive, out GpioPin pin3, out GpioOpenStatus openStatus3))
                    {
                        if (gpioController.TryOpenPin(in4, GpioSharingMode.Exclusive, out GpioPin pin4, out GpioOpenStatus openStatus4))
                        {
                            Motor2 = new Motor(pin3, pin4);
                            Debug.WriteLine($"Success");
                        }
                        else
                            Debug.WriteLine($"Fail");
                    }
                    else
                    {
                        Debug.WriteLine($"Fail");
                    }
                }

                initiated = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not initiate the L298N. {ex}");
            }
        }        
    }
}
