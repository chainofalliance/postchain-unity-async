using System.Linq;
using System.Text;
using System;

namespace Chromia.Postchain.Ft3
{
    public static class Util
    {
        public static byte[] HexStringToBuffer(string text)
        {
            return Enumerable.Range(0, text.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(text.Substring(x, 2), 16))
                            .ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        public static string AuthTypeToString(AuthType type)
        {
            switch (type)
            {
                case AuthType.SingleSig:
                    return "S";
                case AuthType.MultiSig:
                    return "M";
                default:
                    return "";
            }
        }

        public static string FlagTypeToString(FlagsType type)
        {
            switch (type)
            {
                case FlagsType.Account:
                    return "A";
                case FlagsType.Transfer:
                    return "T";
                default:
                    return "";
            }
        }

        public static FlagsType StringToFlagType(string type)
        {
            switch (type)
            {
                case "A":
                    return FlagsType.Account;
                case "T":
                    return FlagsType.Transfer;
                default:
                    return FlagsType.None;
            }
        }

        public static AuthType StringToAuthType(string type)
        {
            switch (type)
            {
                case "S":
                    return AuthType.SingleSig;
                case "M":
                    return AuthType.MultiSig;
                default:
                    return AuthType.None;
            }
        }
    }
}
