using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace APRS
{
    public sealed class PacketInfo
    {
        private static readonly Regex _AltitudeRegex = new Regex(@"A\=(\d\d\d\d\d\d)", RegexOptions.Compiled);

        private static readonly Dictionary<string, MicEMessageType> _CustomMicEMessageTypeMap = new Dictionary<string, MicEMessageType>();
        private static readonly Dictionary<char, DataType> _DataTypeMap = new Dictionary<char, DataType>();
        private static readonly Dictionary<char, Symbol> _PrimarySymbolTableSymbolMap = new Dictionary<char, Symbol>();
        private static readonly Dictionary<char, Symbol> _SecondarySymbolTableSymbolMap = new Dictionary<char, Symbol>();
        private static readonly Dictionary<string, MicEMessageType> _StandardMicEMessageTypeMap = new Dictionary<string, MicEMessageType>();
        private static readonly Dictionary<char, SymbolTable> _SymbolTableMap = new Dictionary<char, SymbolTable>();

        static PacketInfo()
        {
            _DataTypeMap[(char) 0x1c] = DataType.CurrentMicERev0;
            _DataTypeMap[(char) 0x1d] = DataType.OldMicERev0;
            _DataTypeMap['!'] = DataType.PositionWithoutTimestampNoAprsMessaging;
            _DataTypeMap['#'] = DataType.PeetBrosUiiWxStation;
            _DataTypeMap['$'] = DataType.RawGpsDataOrUltimeter2000;
            _DataTypeMap['%'] = DataType.AgreloDfJrMicroFinder;
            _DataTypeMap['\''] = DataType.OldMicE;
            _DataTypeMap[')'] = DataType.Item;
            _DataTypeMap['*'] = DataType.PeetBrosUiiWxStation;
            _DataTypeMap['+'] = DataType.ShelterDataWithTime;
            _DataTypeMap[','] = DataType.InvalidOrTestData;
            _DataTypeMap['.'] = DataType.SpaceWeather;
            _DataTypeMap['/'] = DataType.PositionWithTimestampNoAprsMessaging;
            _DataTypeMap[':'] = DataType.Message;
            _DataTypeMap[';'] = DataType.Object;
            _DataTypeMap['<'] = DataType.StationCapabilities;
            _DataTypeMap['='] = DataType.PositionWithoutTimestampWithAprsMessaging;
            _DataTypeMap['>'] = DataType.Status;
            _DataTypeMap['?'] = DataType.Query;
            _DataTypeMap['@'] = DataType.PositionWithTimestampWithAprsMessaging;
            _DataTypeMap['T'] = DataType.TelemetryData;
            _DataTypeMap['['] = DataType.MaidenheadGridLocatorBeacon;
            _DataTypeMap['_'] = DataType.WeatherReportWithoutPosition;
            _DataTypeMap['`'] = DataType.CurrentMicE;
            _DataTypeMap['{'] = DataType.UserDefinedAprsPacketFormat;
            _DataTypeMap['}'] = DataType.ThirdPartyTraffic;

            _StandardMicEMessageTypeMap["111"] = MicEMessageType.OffDuty;
            _StandardMicEMessageTypeMap["110"] = MicEMessageType.EnRoute;
            _StandardMicEMessageTypeMap["101"] = MicEMessageType.InService;
            _StandardMicEMessageTypeMap["100"] = MicEMessageType.Returning;
            _StandardMicEMessageTypeMap["011"] = MicEMessageType.Committed;
            _StandardMicEMessageTypeMap["010"] = MicEMessageType.Special;
            _StandardMicEMessageTypeMap["001"] = MicEMessageType.Priority;

            _CustomMicEMessageTypeMap["111"] = MicEMessageType.Custom0;
            _CustomMicEMessageTypeMap["110"] = MicEMessageType.Custom1;
            _CustomMicEMessageTypeMap["101"] = MicEMessageType.Custom2;
            _CustomMicEMessageTypeMap["100"] = MicEMessageType.Custom3;
            _CustomMicEMessageTypeMap["011"] = MicEMessageType.Custom4;
            _CustomMicEMessageTypeMap["010"] = MicEMessageType.Custom5;
            _CustomMicEMessageTypeMap["001"] = MicEMessageType.Custom6;

            _SymbolTableMap['/'] = SymbolTable.Primary;
            _SymbolTableMap['\\'] = SymbolTable.Secondary;

            _PrimarySymbolTableSymbolMap['\''] = Symbol.Aircraft;
            _SecondarySymbolTableSymbolMap['`'] = Symbol.Aircraft;
        }

        public PacketInfo(string rawData)
        {
            ReceivedDate = DateTime.UtcNow;

            //Parse out the callsign
            var match = Regex.Match(rawData, @"^([^\>]*)\>(.*)$");
            Callsign = match.Groups[1].Value;
            rawData = match.Groups[2].Value;


            match = Regex.Match(rawData, @"^([^\:]*)\:(.*)$");
            StationRoute = new ReadOnlyCollection<string>(match.Groups[1].Value.Split(','));
            rawData = match.Groups[2].Value;

            DataType dataType;
            if (!_DataTypeMap.TryGetValue(rawData[0], out dataType))
                throw new ArgumentException("Unsupported data type in raw data", "rawData");

            DataType = dataType;


            switch (DataType)
            {
                case DataType.CurrentMicE:
                {
                    Latitude latitude;
                    short longitudeOffset;
                    LongitudeHemisphere longitudeHemisphere;
                    MicEMessageType micEMessageType;

                    decodeMicEDestinationAddress(
                        StationRoute[0],
                        out latitude,
                        out longitudeOffset,
                        out longitudeHemisphere,
                        out micEMessageType);

                    Latitude = latitude;
                    MicEMessageType = micEMessageType;

                    var longitudeDegrees = (short) (rawData[1] - 28 + longitudeOffset);
                    if (180 <= longitudeDegrees && longitudeDegrees <= 189)
                        longitudeDegrees -= 80;
                    else if (190 <= longitudeDegrees && longitudeDegrees <= 199)
                        longitudeDegrees -= 190;

                    Longitude = new Longitude(
                        longitudeDegrees,
                        (short) ((rawData[2] - 28)%60),
                        (rawData[3] - 28)*0.6,
                        longitudeHemisphere,
                        latitude.Ambiguity);

                    var speedCourseSharedByte = (rawData[5] - 28);
                    Speed = Speed.FromKnots(((rawData[4] - 28)*10 + (int) Math.Floor(speedCourseSharedByte/10.0))%800);
                    Direction = new Heading((short) (((speedCourseSharedByte%10*100) + (rawData[6] - 28))%400), 0, 0);

                    SymbolTable = _SymbolTableMap[rawData[8]];
                    Symbol = (SymbolTable == SymbolTable.Primary ? _PrimarySymbolTableSymbolMap : _SecondarySymbolTableSymbolMap)[rawData[7]];

                    if (rawData.Length > 12 && rawData[12] == '}')
                    {
                        Altitude = Altitude.FromMetersAboveBaseline(convertFromBase91(rawData.Substring(9, 3)));
                    }
                }
                    break;
                case DataType.PositionWithoutTimestampWithAprsMessaging:
                case DataType.PositionWithoutTimestampNoAprsMessaging:
                    Latitude = new Latitude(Convert.ToInt16(rawData.Substring(1, 2)), Convert.ToDouble(rawData.Substring(3, 5)), rawData[8] == 'N' ? LatitudeHemisphere.North : LatitudeHemisphere.South);
                    SymbolTable = _SymbolTableMap[rawData[9]];
                    Longitude = new Longitude(Convert.ToInt16(rawData.Substring(10, 3)), Convert.ToDouble(rawData.Substring(13, 5)), rawData[18] == 'E' ? LongitudeHemisphere.East : LongitudeHemisphere.West);
                    Symbol = (SymbolTable == SymbolTable.Primary ? _PrimarySymbolTableSymbolMap : _SecondarySymbolTableSymbolMap)[rawData[19]];
                    rawData = rawData.Substring(20);
                    if (Regex.IsMatch(rawData, @"^\d\d\d\\\d\d\d"))
                    {
                        Direction = new Heading(Convert.ToInt16(rawData.Substring(0, 3)), 0, 0);
                        Speed = Speed.FromKnots(Convert.ToDouble(rawData.Substring(4, 3)));
                        rawData = rawData.Substring(7);
                    }
                    if (_AltitudeRegex.IsMatch(rawData))
                        Altitude = Altitude.FromFeetAboveSeaLevel(Convert.ToInt32(_AltitudeRegex.Match(rawData).Groups[1].Value));
                    break;
                case DataType.PositionWithTimestampWithAprsMessaging:
                case DataType.PositionWithTimestampNoAprsMessaging:
                    if (Regex.IsMatch(rawData.Substring(1), @"^(\d\d\d\d\d\d)h"))
                    {
                        rawData = rawData.Substring(8);
                    }
                    else
                    {
                        return;
                    }
                    Latitude = new Latitude(Convert.ToInt16(rawData.Substring(0, 2)), Convert.ToDouble(rawData.Substring(2, 5)), rawData[7] == 'N' ? LatitudeHemisphere.North : LatitudeHemisphere.South);
                    SymbolTable = _SymbolTableMap[rawData[8]];
                    Longitude = new Longitude(Convert.ToInt16(rawData.Substring(9, 3)), Convert.ToDouble(rawData.Substring(12, 5)), rawData[17] == 'E' ? LongitudeHemisphere.East : LongitudeHemisphere.West);
                    Symbol = (SymbolTable == SymbolTable.Primary ? _PrimarySymbolTableSymbolMap : _SecondarySymbolTableSymbolMap)[rawData[18]];
                    Direction = new Heading(Convert.ToInt16(rawData.Substring(19, 3)), 0, 0);
                    Speed = Speed.FromKnots(Convert.ToDouble(rawData.Substring(23, 3)));
                    rawData = rawData.Substring(26);
                    if (_AltitudeRegex.IsMatch(rawData))
                        Altitude = Altitude.FromFeetAboveSeaLevel(Convert.ToInt32(_AltitudeRegex.Match(rawData).Groups[1].Value));
                    break;
            }
        }

        public string Callsign { get; private set; }
        public ReadOnlyCollection<string> StationRoute { get; private set; }
        public DataType DataType { get; private set; }
        public Latitude Latitude { get; private set; }
        public Longitude Longitude { get; private set; }
        public Altitude Altitude { get; private set; }
        public Heading Direction { get; private set; }
        public Speed Speed { get; private set; }
        public SymbolTable SymbolTable { get; private set; }
        public Symbol Symbol { get; private set; }
        public MicEMessageType MicEMessageType { get; private set; }
        public DateTime ReceivedDate { get; private set; }

        private static int convertFromBase91(string data)
        {
            var returnValue = 0;

            for (var i = 0; i < data.Length; i++)
                returnValue += Convert.ToInt32(Math.Pow(91, data.Length - i - 1))*(data[i] - 33);

            return returnValue;
        }

        private static void decodeMicEDestinationAddress(
            string data,
            out Latitude latitude,
            out short longitudeOffset,
            out LongitudeHemisphere longitudeHemisphere,
            out MicEMessageType micEMessageType)
        {
            if (string.IsNullOrEmpty(data) || data.Length != 6)
                throw new ArgumentException("Data must be a six character string", "data");

            var sbLatitude = new StringBuilder();
            var sbMicEMessageCode = new StringBuilder();

            var isStandardMessage = false;
            var isCustomMessage = false;
            var latitudeHemisphere = default(LatitudeHemisphere);
            //latitude = null;
            longitudeOffset = 0;
            longitudeHemisphere = default(LongitudeHemisphere);
            //micEMessageType = MicEMessageType.Unknown;

            for (var p = 0; p < 6; p++)
            {
                var c = data[p];
                if (c >= '0' && c <= '9' || c == 'L')
                {
                    if (c == 'L')
                        sbLatitude.Append(" ");
                    else
                        sbLatitude.Append(c - '0');

                    if (p < 3)
                        sbMicEMessageCode.Append('0');
                    else
                        switch (p)
                        {
                            case 3:
                                latitudeHemisphere = LatitudeHemisphere.South;
                                break;
                            case 4:
                                longitudeOffset = 0;
                                break;
                            case 5:
                                longitudeHemisphere = LongitudeHemisphere.East;
                                break;
                        }
                }
                else if (c >= 'A' && c <= 'K')
                {
                    if (c == 'K')
                        sbLatitude.Append(" ");
                    else
                        sbLatitude.Append(c - 'A');

                    if (p < 3)
                    {
                        sbMicEMessageCode.Append(1);
                        isCustomMessage = true;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid data: " + data);
                    }
                }
                else if (c >= 'P' && c <= 'Z')
                {
                    if (c == 'Z')
                        sbLatitude.Append(" ");
                    else
                        sbLatitude.Append(c - 'P');

                    if (p < 3)
                    {
                        sbMicEMessageCode.Append(1);
                        isStandardMessage = true;
                    }
                    else
                        switch (p)
                        {
                            case 3:
                                latitudeHemisphere = LatitudeHemisphere.North;
                                break;
                            case 4:
                                longitudeOffset = 100;
                                break;
                            case 5:
                                longitudeHemisphere = LongitudeHemisphere.West;
                                break;
                        }
                }
                else
                {
                    throw new ArgumentException("Invalid data: " + data);
                }
            }

            short degrees, minutes;
            double seconds;
            short latitudeAmbiguity;
            parseLatitudeValue(sbLatitude.ToString(), out degrees, out minutes, out seconds, out latitudeAmbiguity);

            latitude = new Latitude(degrees, minutes, seconds, latitudeHemisphere, latitudeAmbiguity);

            if (isStandardMessage && isCustomMessage)
            {
                micEMessageType = MicEMessageType.Unknown;
            }
            else if (isStandardMessage)
            {
                if (!_StandardMicEMessageTypeMap.TryGetValue(sbMicEMessageCode.ToString(), out micEMessageType))
                    throw new InvalidOperationException("Invalid MicE Message Code: " + sbMicEMessageCode);
            }
            else if (isCustomMessage)
            {
                if (!_CustomMicEMessageTypeMap.TryGetValue(sbMicEMessageCode.ToString(), out micEMessageType))
                    throw new InvalidOperationException("Invalid MicE Message Code: " + sbMicEMessageCode);
            }
            else
            {
                micEMessageType = MicEMessageType.Emergency;
            }
        }

        private static void parseLatitudeValue(string data, out short degrees, out short minutes, out double seconds, out short ambiguity)
        {
            ambiguity = 0;
            for (var i = data.Length - 1; i >= 0; i--)
            {
                if (data[i] != ' ')
                    break;
                ambiguity++;
            }

            degrees = Convert.ToInt16(data.Substring(0, 2).Replace(' ', '0'));
            minutes = Convert.ToInt16(data.Substring(2, 2).Replace(' ', '0'));
            seconds = Convert.ToDouble("0." + data.Substring(4).Replace(' ', '0'))*60;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Packet Information Received {0} UTC\n", ReceivedDate);
            sb.AppendFormat("        Callsign: {0}\n", Callsign);
            //sb.AppendFormat("   Station Route: {0}\n", string.Join(", ", ));
            sb.AppendFormat("        DataType: {0}\n", DataType);
            sb.AppendFormat("        Latitude: {0}\n", Latitude);
            sb.AppendFormat("       Longitude: {0}\n", Longitude);
            sb.AppendFormat("        Altitude: {0}\n", Altitude);
            sb.AppendFormat("          Course: {0}\n", Direction);
            sb.AppendFormat("           Speed: {0}\n", Speed);
            sb.AppendFormat("    Symbol Table: {0}\n", SymbolTable);
            sb.AppendFormat("          Symbol: {0}\n", Symbol);

            if (DataType == DataType.CurrentMicE)
                sb.AppendFormat("   Mic E Message: {0}\n", MicEMessageType);


            return sb.ToString();
        }
    }
}