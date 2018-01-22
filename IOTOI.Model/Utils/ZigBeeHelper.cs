using System;
using System.Collections.Generic;
using System.Text;

namespace IOTOI.Model.Utils
{
    public class ZigBeeHelper
    {
        // supported data types
        private const int ZCL_TYPE_CODE_LENGTH = sizeof(byte);

        public const byte UNKNOWN_TYPE = 0xFF;
        public const byte BOOLEAN_TYPE = 0x10;
        public const byte BITMAP_8_BIT_TYPE = 0x18;
        public const byte BITMAP_16_BIT_TYPE = 0x19;
        public const byte UINT8_TYPE = 0x20;
        public const byte UINT16_TYPE = 0x21;
        public const byte UINT32_TYPE = 0x23;
        public const byte INT8_TYPE = 0x28;
        public const byte INT16_TYPE = 0x29;
        public const byte INT32_TYPE = 0x2B;
        public const byte ENUMERATION_8_BIT_TYPE = 0x30;
        public const byte ENUMERATION_16_BIT_TYPE = 0x31;
        public const byte CHAR_STRING_TYPE = 0x42;
        public const byte IEEE_ADDRESS_TYPE = 0xF0;

        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                    (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                    (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                    (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                    (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public static Int16 Int16FromZigBeeFrame(byte[] buffer, int offset)
        {
            return (Int16)UInt16FromZigBeeFrame(buffer, offset);
        }
        public static UInt16 UInt16FromZigBeeFrame(byte[] buffer, int offset)
        {
            // numbers are little endian in ZigBee frames (ZDO and ZCL) 
            UInt16 value = BitConverter.ToUInt16(buffer, offset);
            if (!BitConverter.IsLittleEndian)
            {
                value = ReverseBytes(value);
            }
            return value;
        }

        public static Int32 Int32FromZigBeeFrame(byte[] buffer, int offset)
        {
            return (Int32)UInt32FromZigBeeFrame(buffer, offset);
        }

        public static UInt32 UInt32FromZigBeeFrame(byte[] buffer, int offset)
        {
            // numbers are little endian in ZigBee frames (ZDO and ZCL) 
            UInt32 value = BitConverter.ToUInt32(buffer, offset);
            if (!BitConverter.IsLittleEndian)
            {
                value = ReverseBytes(value);
            }
            return value;
        }

        public static UInt64 UInt64FromZigBeeFrame(byte[] buffer, int offset)
        {
            // numbers are little endian in ZigBee frames (ZDO and ZCL) 
            UInt64 value = BitConverter.ToUInt64(buffer, offset);
            if (!BitConverter.IsLittleEndian)
            {
                value = ReverseBytes(value);
            }
            return value;
        }
        public static bool GetValue(byte type, ref byte[] buffer, out object value)
        {
            value = null;
            int offset = 0;
            switch (type)
            {
                case BOOLEAN_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(bool))
                        {
                            bool tempVal = Convert.ToBoolean(buffer[offset]);
                            value = tempVal;
                            offset += sizeof(bool);
                        }
                    }
                    break;

                case CHAR_STRING_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(byte))
                        {
                            int length = Convert.ToInt32(buffer[offset]);
                            if (length != 0 &&
                                buffer.Length >= (offset + (length + 1) * sizeof(byte)))
                            {
                                offset += sizeof(byte);
                                String tempVal = Encoding.UTF8.GetString(buffer, offset, length);
                                value = tempVal;
                                offset += (length + 1) * sizeof(byte);
                            }
                        }
                    }
                    break;

                case INT8_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(sbyte))
                        {
                            sbyte tempVal = (sbyte)buffer[offset];
                            value = tempVal;
                            offset += sizeof(sbyte);
                        }
                    }
                    break;

                case ENUMERATION_8_BIT_TYPE:        // expected fall through
                case BITMAP_8_BIT_TYPE:             // expected fall through
                case UINT8_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(byte))
                        {
                            byte tempVal = buffer[offset];
                            value = tempVal;
                            offset += sizeof(byte);
                        }
                    }
                    break;

                case INT16_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(Int16))
                        {
                            value = Int16FromZigBeeFrame(buffer, offset);
                            offset += sizeof(Int16);
                        }
                    }
                    break;

                case ENUMERATION_16_BIT_TYPE:        // expected fall through
                case BITMAP_16_BIT_TYPE:             // expected fall through
                case UINT16_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(UInt16))
                        {
                            value = UInt16FromZigBeeFrame(buffer, offset);
                            offset += sizeof(UInt16);
                        }
                    }
                    break;

                case INT32_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(Int32))
                        {
                            value = Int32FromZigBeeFrame(buffer, offset);
                            offset += sizeof(Int32);
                        }
                    }
                    break;

                case UINT32_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(UInt32))
                        {
                            value = UInt32FromZigBeeFrame(buffer, offset);
                            offset += sizeof(UInt32);
                        }
                    }
                    break;

                case IEEE_ADDRESS_TYPE:
                    {
                        if (buffer.Length >= offset + sizeof(UInt64))
                        {
                            value = UInt64FromZigBeeFrame(buffer, offset);
                            offset += sizeof(UInt64);
                        }
                    }
                    break;
            }

            if (value != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
