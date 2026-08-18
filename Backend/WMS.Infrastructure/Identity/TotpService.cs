using System;
using System.Security.Cryptography;
using System.Text;
using WMS.Application.Common.Interfaces;

namespace WMS.Infrastructure.Identity
{
    public class TotpService : ITotpService
    {
        private static readonly string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public string GenerateSecret()
        {
            byte[] bytes = new byte[20]; // 160 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return ToBase32String(bytes);
        }

        public string GetQrCodeUrl(string email, string secret)
        {
            return $"otpauth://totp/WMS:{Uri.EscapeDataString(email)}?secret={secret}&issuer=WMS";
        }

        public bool VerifyCode(string secret, string code, int timeToleranceStep = 1)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6 || !int.TryParse(code, out _))
                return false;

            byte[] secretBytes;
            try
            {
                secretBytes = FromBase32String(secret);
            }
            catch
            {
                return false;
            }

            long currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

            for (int i = -timeToleranceStep; i <= timeToleranceStep; i++)
            {
                long step = currentStep + i;
                if (CalculateTotp(secretBytes, step) == code)
                {
                    return true;
                }
            }

            return false;
        }

        private static string CalculateTotp(byte[] secret, long step)
        {
            byte[] stepBytes = BitConverter.GetBytes(step);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(stepBytes);
            }

            byte[] challenge = new byte[8];
            Array.Copy(stepBytes, challenge, 8);

            using (var hmac = new HMACSHA1(secret))
            {
                byte[] hash = hmac.ComputeHash(challenge);
                int offset = hash[hash.Length - 1] & 0x0F;
                int binary = ((hash[offset] & 0x7F) << 24)
                           | ((hash[offset + 1] & 0xFF) << 16)
                           | ((hash[offset + 2] & 0xFF) << 8)
                           | (hash[offset + 3] & 0xFF);

                int otp = binary % 1000000;
                return otp.ToString("D6");
            }
        }

        private static string ToBase32String(byte[] data)
        {
            StringBuilder result = new StringBuilder((data.Length + 7) * 8 / 5);
            int val = 0;
            int bits = 0;

            for (int i = 0; i < data.Length; i++)
            {
                val = (val << 8) | data[i];
                bits += 8;
                while (bits >= 5)
                {
                    bits -= 5;
                    result.Append(Base32Alphabet[(val >> bits) & 31]);
                }
            }

            if (bits > 0)
            {
                result.Append(Base32Alphabet[(val << (5 - bits)) & 31]);
            }

            return result.ToString();
        }

        private static byte[] FromBase32String(string input)
        {
            input = input.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(input))
                return [];

            int bits = 0;
            int val = 0;
            var result = new System.Collections.Generic.List<byte>();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                int charVal = Base32Alphabet.IndexOf(c);
                if (charVal < 0)
                    throw new ArgumentException("Invalid Base32 character.");

                val = (val << 5) | charVal;
                bits += 5;
                if (bits >= 8)
                {
                    bits -= 8;
                    result.Add((byte)((val >> bits) & 255));
                }
            }

            return result.ToArray();
        }
    }
}
