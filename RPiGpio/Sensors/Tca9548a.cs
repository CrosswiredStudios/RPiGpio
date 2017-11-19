using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace RPiGpio.Sensors
{
    /// <summary>
    /// This class wraps the functionality of the TCA9548A I2C Multiplexer. 
    /// It was created with the intention of being used on the Raspberry Pi 3 on Windows IoT.
    /// </summary>
    public static class Tca9548a
    {
        const bool debug = false;
        const string I2CControllerName = "I2C1";

        static readonly Dictionary<byte, I2cDevice> multiplexers = new Dictionary<byte, I2cDevice>();

        /// <summary>
        /// Attempts to initialize the multiplexer
        /// </summary>
        /// <param name="multiplexerAddress">The address of the multiplexer. Default for Tca9548a is 0x70.</param>
        public static async Task Initialize(byte multiplexerAddress = 0x70)
        {
            Debug.WriteLine($"Initiating Multiplexer at address {multiplexerAddress}");

            try
            {
                // Error checking
                if (multiplexers.ContainsKey(multiplexerAddress))
                {
                    Debug.WriteLine($"A Tca9548a multiplexer has already been initiated at that address ({multiplexerAddress})");
                    return;
                }

                // Create a I2C settings object
                var settings = new I2cConnectionSettings(multiplexerAddress)
                {
                    BusSpeed = I2cBusSpeed.FastMode
                };

                // Get the device information for the I2C controller
                var aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                var dis = await DeviceInformation.FindAllAsync(aqs);

                // Initiate the sensor
                var tca9548a = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                // Add it to the dictionary
                multiplexers.Add(multiplexerAddress, tca9548a);

                Debug.WriteLine($"Successfully initiated the Tca9548a multiplexer at address: {multiplexerAddress}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to initiate the Tca9548a multiplexer: {e}");
            }
        }

        /// <summary>
        /// Select a multiplex channel
        /// </summary>
        /// <param name="multiplexChannel">The channel to select (0x01 - 0x07)</param>
        public static void SelectAddress(byte multiplexChannel)
        {
            SelectAddress(0x70, multiplexChannel);
        }

        /// <summary>
        /// Select a multiplex channel on a multiplexer.
        /// </summary>
        /// <param name="multiplexerAddress">The adress of the multiplexer to change (0x70 - 0x77)</param>
        /// <param name="multiplexChannel">The multiplex channel to connect to (0 off, 1 - 7 for SCx/SDx)</param>
        public static void SelectAddress(byte multiplexerAddress, byte multiplexChannel)
        {
            try
            {
                if(debug)
                    Debug.WriteLine($"Setting multiplexer {multiplexerAddress} to id {multiplexChannel}.");
                if (!multiplexers.ContainsKey(multiplexerAddress))
                {
                    Debug.WriteLine($"Multiplexer at {multiplexerAddress} not found. Please initialize the multiplexer before using it.");
                    return;
                }
                if (multiplexChannel > 0x07)
                {
                    Debug.WriteLine($"Tca9548a.SelectAddress({multiplexChannel}) is not valid. Only 0 - 7 is supported by this device.");
                    return;
                }

                var writeBuffer = new byte[] { (byte)(1 << multiplexChannel) };
                multiplexers[multiplexerAddress].Write(writeBuffer);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to select ({multiplexerAddress}, {multiplexChannel}): {e}");
            }
        }
    }
}
