using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI;

namespace RPiGpio.Sensors
{
    public struct KnownColor
    {
        public Color Color;
        public string Name;

        public KnownColor(Color color, string name)
        {
            Color = color;
            Name = name;
        }
    };

    public class ColorData
    {
        public UInt16 Red { get; set; }
        public UInt16 Green { get; set; }
        public UInt16 Blue { get; set; }
        public UInt16 Alpha { get; set; }
    }

    public class RgbData
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
    }

    /// <summary>
    /// RBGa Color sensor
    /// </summary>
    public class Tcs34725
    {
        enum Tcs34725Gain
        {
            TCS34725_GAIN_1X = 0x00,   // No gain 
            TCS34725_GAIN_4X = 0x01,   // 2x gain
            TCS34725_GAIN_16X = 0x02,  // 16x gain
            TCS34725_GAIN_60X = 0x03   // 60x gain 
        };

        enum Tcs34725IntegrationTime
        {
            TCS34725_INTEGRATIONTIME_2_4MS = 0xFF,   //2.4ms - 1 cycle    - Max Count: 1024
            TCS34725_INTEGRATIONTIME_24MS = 0xF6,    //24ms  - 10 cycles  - Max Count: 10240
            TCS34725_INTEGRATIONTIME_50MS = 0xEB,    //50ms  - 20 cycles  - Max Count: 20480 
            TCS34725_INTEGRATIONTIME_101MS = 0xD5,   //101ms - 42 cycles  - Max Count: 43008
            TCS34725_INTEGRATIONTIME_154MS = 0xC0,   //154ms - 64 cycles  - Max Count: 65535 
            TCS34725_INTEGRATIONTIME_700MS = 0x00    //700ms - 256 cycles - Max Count: 65535 
        };

        public enum LedState { On, Off };

        //Address values set according to the datasheet: http://www.adafruit.com/datasheets/TCS34725.pdf
        const bool debug = false;
        const string I2CControllerName = "I2C1"; //String for the friendly name of the I2C bus (default for raspberry pi)
        const byte TCS34725_Address = 0x29;
        const byte TCS34725_ENABLE = 0x00;
        const byte TCS34725_ENABLE_PON = 0x01; //Power on: 1 activates the internal oscillator, 0 disables it
        const byte TCS34725_ENABLE_AEN = 0x02; //RGBC Enable: 1 actives the ADC, 0 disables it 
        const byte TCS34725_ID = 0x12;
        const byte TCS34725_CDATAL = 0x14;  //Alpha channel data 
        const byte TCS34725_CDATAH = 0x15;
        const byte TCS34725_RDATAL = 0x16;  //Red channel data
        const byte TCS34725_RDATAH = 0x17;
        const byte TCS34725_GDATAL = 0x18;  //Green channel data
        const byte TCS34725_GDATAH = 0x19;
        const byte TCS34725_BDATAL = 0x1A;  //Blue channel data */
        const byte TCS34725_BDATAH = 0x1B;
        const byte TCS34725_ATIME = 0x01;   //Integration time
        const byte TCS34725_CONTROL = 0x0F; //Set the gain level for the sensor
        const byte TCS34725_COMMAND_BIT = 0x80; // Have to | addresses with this value when asking for values

        private List<KnownColor> colorList;
        I2cDevice colorSensor = null;
        LedState currentLedState;
        GpioController gpio;
        bool initialized = false;
        GpioPin ledGpioPin;
        int ledControlPin;
        string[] limitColorList;
        Tcs34725Gain tcs34725Gain;
        Tcs34725IntegrationTime tcs34725IntegrationTime;

        public LedState CurrentLedState
        {
            get => currentLedState;
            set
            {
                if (ledGpioPin != null)
                {
                    //Set the GPIO pin value to the new value
                    GpioPinValue newValue = (value == LedState.On ? GpioPinValue.High : GpioPinValue.Low);
                    ledGpioPin.Write(newValue);
                    //Update the LED state variable
                    currentLedState = value;
                }
            }
        }

        public Tcs34725(int ledControlPin = 12)
        {
            this.ledControlPin = ledControlPin;
            limitColorList = new string[] {"Moccasin", "NavajoWhite", "DarkOliveGreen", "SeaGreen", "Turquoise", "Tomato"};
            colorList = new List<KnownColor>();
            currentLedState = LedState.On;
            tcs34725Gain = Tcs34725Gain.TCS34725_GAIN_16X;
            tcs34725IntegrationTime = Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_101MS;
        }

        /// <summary>
        /// Build a list of colors
        /// </summary>
        void BuildColorsList()
        {
            //Read the all the known colors from Windows.UI.Colors
            foreach (PropertyInfo property in typeof(Colors).GetProperties())
            {
                KnownColor temp = new KnownColor((Color)property.GetValue(null), property.Name);
                colorList.Add(temp);

                ////Select the colors in the limited colors list
                //if (limitColorList.Contains(property.Name))
                //{
                //    KnownColor temp = new KnownColor((Color)property.GetValue(null), property.Name);
                //    colorList.Add(temp);
                //}
            }
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            Debug.WriteLine("TCS34725: Initializing...");

            try
            {
                //Instantiate the I2CConnectionSettings using the device address of the TCS34725
                I2cConnectionSettings settings = new I2cConnectionSettings(TCS34725_Address);

                //Set the I2C bus speed of connection to fast mode
                settings.BusSpeed = I2cBusSpeed.FastMode;

                //Use the I2CBus device selector to create an advanced query syntax string
                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);

                //Use the Windows.Devices.Enumeration.DeviceInformation class to create a 
                //collection using the advanced query syntax string
                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);

                //Instantiate the the TCS34725 I2C device using the device id of the I2CBus 
                //and the I2CConnectionSettings
                colorSensor = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                //Create a default GPIO controller
                gpio = GpioController.GetDefault();

                try
                {
                    //Open the LED control pin using the GPIO controller
                    ledGpioPin = gpio.OpenPin(ledControlPin);

                    //Set the pin to output
                    ledGpioPin.SetDriveMode(GpioPinDriveMode.Output);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Tcs34725: Unable to open pin {ledGpioPin} for LED control. {ex}");
                }
                
                //Initialize the known color list
                BuildColorsList();

                //initialized = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }

        }

        async Task Begin()
        {
            Debug.WriteLine("TCS34725::Begin");
            byte[] WriteBuffer = new byte[] { TCS34725_ID | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            //Read and check the device signature
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("TCS34725 Signature: " + ReadBuffer[0].ToString());

            if (ReadBuffer[0] != 0x44)
            {
                Debug.WriteLine("TCS34725::Begin Signature Mismatch.");
                return;
            }

            //Set the initalize variable to true
            initialized = true;

            //Set the default integration time
            SetIntegrationTime(tcs34725IntegrationTime);

            //Set default gain
            SetGain(tcs34725Gain);

            //Note: By default the device is in power down mode on bootup so need to enable it.
            //await Enable(); *************
        }

        //Method to write the gain value to the control register
        private async void SetGain(Tcs34725Gain gain)
        {
            if (!initialized) await Begin();
            tcs34725Gain = gain;
            byte[] WriteBuffer = new byte[] { TCS34725_CONTROL | TCS34725_COMMAND_BIT,
                    (byte)tcs34725Gain };
            colorSensor.Write(WriteBuffer);
        }

        //Method to write the integration time value to the ATIME register
        private async void SetIntegrationTime(Tcs34725IntegrationTime integrationTime)
        {
            if (!initialized) await Begin();
            tcs34725IntegrationTime = integrationTime;
            byte[] WriteBuffer = new byte[] { TCS34725_ATIME | TCS34725_COMMAND_BIT,
                    (byte)tcs34725IntegrationTime };
            colorSensor.Write(WriteBuffer);
        }

        //Method to enable the sensor
        public async Task Enable()
        {
            Debug.WriteLine("TCS34725::enable");
            if (!initialized) await Begin();

            byte[] WriteBuffer = new byte[] { 0x00, 0x00 };

            //Enable register 
            WriteBuffer[0] = TCS34725_ENABLE | TCS34725_COMMAND_BIT;

            //Send power on
            WriteBuffer[1] = TCS34725_ENABLE_PON;
            colorSensor.Write(WriteBuffer);

            //Pause between commands
            await Task.Delay(3);

            //Send ADC Enable
            WriteBuffer[1] = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            colorSensor.Write(WriteBuffer);
        }

        //Method to disable the sensor
        public async Task Disable()
        {
            Debug.WriteLine("TCS34725::disable");
            if (!initialized) await Begin();

            byte[] WriteBuffer = new byte[] { TCS34725_ENABLE | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            //Read the enable buffer
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);

            //Turn the device off to save power by reversing the on conditions
            byte onState = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            byte offState = (byte)~onState;
            offState &= ReadBuffer[0];
            byte[] OffBuffer = new byte[] { TCS34725_ENABLE, offState };

            //Write the off buffer
            colorSensor.Write(OffBuffer);
        }

        //Method to get the 16-bit color from 2 8-bit buffers
        UInt16 ColorFromBuffer(byte[] buffer)
        {
            UInt16 color = 0x00;

            color = buffer[1];
            color <<= 8;
            color |= buffer[0];

            return color;
        }

        //Method to read the raw color data
        public async Task<ColorData> GetColorData()
        {
            //Create an object to store the raw color data
            ColorData colorData = new ColorData();

            //Make sure the I2C device is initialized
            if (!initialized) await Begin();

            byte[] WriteBuffer = new byte[] { 0x00 };
            byte[] ReadBuffer = new byte[] { 0x00, 0x00 };

            try
            {
                //Read and store the clear data
                WriteBuffer[0] = TCS34725_CDATAL | TCS34725_COMMAND_BIT;
                colorSensor.WriteRead(WriteBuffer, ReadBuffer);
                colorData.Alpha = ColorFromBuffer(ReadBuffer);

                //Read and store the red data
                WriteBuffer[0] = TCS34725_RDATAL | TCS34725_COMMAND_BIT;
                colorSensor.WriteRead(WriteBuffer, ReadBuffer);
                colorData.Red = ColorFromBuffer(ReadBuffer);

                //Read and store the green data
                WriteBuffer[0] = TCS34725_GDATAL | TCS34725_COMMAND_BIT;
                colorSensor.WriteRead(WriteBuffer, ReadBuffer);
                colorData.Green = ColorFromBuffer(ReadBuffer);

                //Read and store the blue data
                WriteBuffer[0] = TCS34725_BDATAL | TCS34725_COMMAND_BIT;
                colorSensor.WriteRead(WriteBuffer, ReadBuffer);
                colorData.Blue = ColorFromBuffer(ReadBuffer);

                //Output the raw data to the debug console
                if(debug)
                Debug.WriteLine("Raw Data - red: {0}, green: {1}, blue: {2}, clear: {3}",
                    colorData.Red, colorData.Green, colorData.Blue, colorData.Alpha);
            }
            catch
            {
            }
            //Return the data
            return colorData;
        }

        //Method to read the RGB data
        public async Task<RgbData> GetRgbData()
        {
            //Create an object to store the raw color data
            RgbData rgbData = new RgbData();

            //First get the raw color data
            ColorData colorData = await GetColorData();
            //Check if clear data is received
            if (colorData.Alpha > 0)
            {
                //Find the RGB values from the raw data using the clear data as reference
                rgbData.Red = (colorData.Red * 255 / colorData.Alpha);
                rgbData.Blue = (colorData.Blue * 255 / colorData.Alpha);
                rgbData.Green = (colorData.Green * 255 / colorData.Alpha);
            }
            //Write the RGB values to the debug console
            if(debug)
            Debug.WriteLine("RGB Data - red: {0}, green: {1}, blue: {2}", rgbData.Red, rgbData.Green, rgbData.Blue);

            //Return the data
            return rgbData;
        }

        //Method to find the approximate color
        public async Task<string> GetColorString()
        {
            //Create an object to store the raw color data
            RgbData rgbData = await GetRgbData();
            //Create a variable to store the closest color. Black by default.
            KnownColor closestColor = colorList[0];
            //Create a variable to store the minimum Euclidean distance between 2 colors
            double minDiff = double.MaxValue;

            //For every known color, check the Euclidean distance and store the minimum distance
            foreach (var color in colorList)
            {
                Color colorValue = color.Color;
                double diff = Math.Pow((colorValue.R - rgbData.Red), 2) +
                              Math.Pow((colorValue.G - rgbData.Green), 2) +
                              Math.Pow((colorValue.B - rgbData.Blue), 2);
                diff = (int)Math.Sqrt(diff);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestColor = color;
                }
            }

            //Write the approximate color to the debug console
            if(debug)
            Debug.WriteLine("Approximate color: " + closestColor.Name + " - "
                            + closestColor.Color.ToString());

            //Return the approximate color
            return closestColor.Name;
        }
    }
}
