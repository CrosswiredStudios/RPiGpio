using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiGpio.Controllers
{
    public class L298n
    {
        public enum MotorOutput { OneTwo, ThreeFour }
        public enum RotationDirection { Clockwise, CounterClockwise }

        RotationDirection direction;
        GpioController gpioController;
        GpioPin ena, enb, in1, in2, in3, in4;
        bool initiated;

        public L298n(GpioController gpioController)
        {
            direction = RotationDirection.Clockwise;
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
                Debug.Write($"Initiating L298N input 1 on pin {in1}: ");
                if (gpioController.TryOpenPin(in1, GpioSharingMode.Exclusive, out GpioPin pin, out GpioOpenStatus openStatus))
                {
                    Debug.WriteLine($"Success");
                    this.in1 = pin;
                    this.in1.Write(GpioPinValue.Low);
                    this.in1.SetDriveMode(GpioPinDriveMode.Output);
                }
                else
                {
                    Debug.WriteLine($"Fail");
                }

                Debug.Write($"Initiating L298N input 2 on pin {in2}: ");
                if (gpioController.TryOpenPin(in2, GpioSharingMode.Exclusive, out GpioPin pin2, out GpioOpenStatus openStatus2))
                {
                    Debug.WriteLine($"Success");
                    this.in2 = pin2;
                    this.in2.Write(GpioPinValue.Low);
                    this.in2.SetDriveMode(GpioPinDriveMode.Output);
                }
                else
                {
                    Debug.WriteLine($"Fail");
                }

                if (in3 != -1)
                {
                    Debug.Write($"Initiating L298N input 3 on pin {in3}: ");
                    if (gpioController.TryOpenPin(in3, GpioSharingMode.Exclusive, out GpioPin pin3, out GpioOpenStatus openStatus3))
                    {
                        Debug.WriteLine($"Success");
                        this.in3 = pin3;
                        this.in3.Write(GpioPinValue.Low);
                        this.in3.SetDriveMode(GpioPinDriveMode.Output);
                    }
                    else
                    {
                        Debug.WriteLine($"Fail");
                    }
                }
                

                if (in4 != -1)
                {
                    Debug.Write($"Initiating L298N input 4 on pin {in4}: ");
                    if (gpioController.TryOpenPin(in4, GpioSharingMode.Exclusive, out GpioPin pin4, out GpioOpenStatus openStatus4))
                    {
                        Debug.WriteLine($"Success");
                        this.in4 = pin4;
                        this.in4.Write(GpioPinValue.Low);
                        this.in4.SetDriveMode(GpioPinDriveMode.Output);
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

        public void SetDirection(MotorOutput output, RotationDirection direction)
        {
            try
            {
                if(initiated)
                {
                    Debug.WriteLine($"Setting motor { (output == MotorOutput.OneTwo ? "A" : "B") } to {direction.ToString()}.");

                    switch (output)
                    {
                        case MotorOutput.OneTwo:
                            in1.Write(direction == RotationDirection.Clockwise ? GpioPinValue.High : GpioPinValue.Low);
                            in2.Write(direction == RotationDirection.Clockwise ? GpioPinValue.Low : GpioPinValue.High);
                            break;
                        case MotorOutput.ThreeFour:
                            in3.Write(direction == RotationDirection.Clockwise ? GpioPinValue.High : GpioPinValue.Low);
                            in4.Write(direction == RotationDirection.Clockwise ? GpioPinValue.Low : GpioPinValue.High);
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine($"Cannot set direction before initializing L298N.");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Could not set direction for the L298N. {ex}");
            }
        }
    }
}
