using System;

namespace QrCodeRecovery.Helpers
{
    public static class Base32Utils
    {
        public static string StringToBase32(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return ToBase32String(bytes);
        }

        public static string ToBase32String(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentNullException(nameof(input));

            int charCount = (int)Math.Ceiling(input.Length / 5d) * 8;
            char[] returnArray = new char[charCount];

            byte nextChar = 0, bitsRemaining = 5;
            int arrayIndex = 0;

            foreach (byte b in input)
            {
                nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
                returnArray[arrayIndex++] = ValueToChar(nextChar);

                if (bitsRemaining < 4)
                {
                    nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                    returnArray[arrayIndex++] = ValueToChar(nextChar);
                    bitsRemaining += 5;
                }

                bitsRemaining -= 3;
                nextChar = (byte)((b << bitsRemaining) & 31);
            }

            if (arrayIndex != charCount)
            {
                returnArray[arrayIndex++] = ValueToChar(nextChar);
                while (arrayIndex != charCount)
                    returnArray[arrayIndex++] = '=';
            }

            return new string(returnArray);
        }

        public static byte[] FromBase32String(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            input = input.TrimEnd('=');
            int byteCount = input.Length * 5 / 8;
            byte[] returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int arrayIndex = 0;

            foreach (char c in input)
            {
                int cValue = CharToValue(c);
                int mask;
                if (bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            if (arrayIndex != byteCount)
                returnArray[arrayIndex] = curByte;

            return returnArray;
        }

        private static int CharToValue(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return c - 'A';

            if (c >= '2' && c <= '7')
                return c - '2' + 26;

            if (c >= 'a' && c <= 'z')
                return c - 'a';

            throw new ArgumentException("Character is not a Base32 character.", nameof(c));
        }

        private static char ValueToChar(byte b)
        {
            if (b < 26)
                return (char)(b + 65);

            if (b < 32)
                return (char)(b + 24);

            throw new ArgumentException("Byte is not a valid Base32 value.", nameof(b));
        }
    }
}
