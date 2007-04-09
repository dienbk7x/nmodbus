using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Modbus.IO;
using Modbus.Message;
using Modbus.Util;
using log4net;

namespace Modbus.Device
{
	/// <summary>
	/// Modbus serial slave device.
	/// </summary>
	public class ModbusSerialSlave : ModbusSlave
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(ModbusSerialSlave));

		private ModbusSerialSlave(byte unitID, ModbusTransport transport)
			: base(unitID, transport)
		{
		}

		/// <summary>
		/// Modbus ASCII slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateAscii(byte unitID, SerialPort serialPort)
		{
			return new ModbusSerialSlave(unitID, new ModbusAsciiTransport(new SerialPortAdapter(serialPort)));
		}

		/// <summary>
		/// Modbus RTU slave factory method.
		/// </summary>
		public static ModbusSerialSlave CreateRtu(byte unitID, SerialPort serialPort)
		{
			return new ModbusSerialSlave(unitID, new ModbusRtuTransport(new SerialPortAdapter(serialPort)));
		}

		/// <summary>
		/// Start slave listening for requests.
		/// </summary>
		public override void Listen()
		{
			while (true)
			{
				try
				{
					// use transport to retrieve raw message frame from stream
					byte[] frame = Transport.ReadRequest();

					// build request from frame
					IModbusMessage request = ModbusMessageFactory.CreateModbusRequest(frame);
					_log.InfoFormat("RX: {0}", StringUtil.Join(", ", request.MessageFrame));

					// only service requests addressed to this particular slave
					if (request.SlaveAddress != UnitID)
					{
						_log.DebugFormat("NModbus Slave {0} ignoring request intended for NModbus Slave {1}", UnitID, request.SlaveAddress);
						continue;
					}

					// perform action
					IModbusMessage response = ApplyRequest(request);

					// write response
					_log.InfoFormat("TX: {0}", StringUtil.Join(", ", response.MessageFrame));
					Transport.Write(response);

				}
				catch (Exception e)
				{
					// TODO explicitly catch timeout exception
					_log.ErrorFormat("Exception encountered while listening for requests - {0}", e.Message);
				}
			}
		}
	}
}
