// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Commands.Ssh
{
    public class RSAParser
    {
        public const string KeyType = "RSA";
        public const string RSAAlgorithm = "ssh-rsa";

        private string keyId;
        private Func<byte[], byte[]> correctEndianness;

        public string Modulus { get; private set; }
        public string Exponent { get; private set; }

        public string KeyId
        {
            get
            {
                if (string.IsNullOrEmpty(keyId))
                {
                    keyId = GetKeyId(Modulus);
                }

                return keyId;
            }
        }

        public RSAParser(bool isSystemLittleEndian)
        {
            if (isSystemLittleEndian)
            {
                correctEndianness = (bytes) => bytes.Reverse().ToArray();
            }
            else
            {
                correctEndianness = (bytes) => bytes;
            }
        }

        public void ParseKey(string publicKey)
        {
            string[] keyParts = publicKey.Split(' ');

            if (keyParts.Length < 2)
            {
                throw new ArgumentException("Public key needs at least 2 parts (algorithm, encodedKey)");
            }

            if (!string.Equals(keyParts[0], RSAAlgorithm))
            {
                throw new ArgumentException("Public key is not encoded with algorithm ssh-rsa");
            }

            string pubkey = keyParts[1];
            Span<byte> keyBytes = Convert.FromBase64String(pubkey);

            IEnumerable<byte[]> fields = GetRSAFields(pubkey);
            string algorithm = Encoding.ASCII.GetString(fields.ElementAt(0));
            Exponent = Convert.ToBase64String(fields.ElementAt(1));
            Modulus = Convert.ToBase64String(fields.ElementAt(2));
        }

        IEnumerable<byte[]> GetRSAFields(string pubkey)
        {
            byte[] keyBytes = Convert.FromBase64String(pubkey);

            int read = 0;
            while (read < keyBytes.Length)
            {
                Span<byte> lengthBytes = new Span<byte>(keyBytes, read, 4);
                byte[] lengthByteArray = correctEndianness(lengthBytes.ToArray());
                int length = Convert.ToInt32(BitConverter.ToUInt32(lengthByteArray, 0));

                read += 4;
                ArraySegment<byte> field = new ArraySegment<byte>(keyBytes, read, length);
                read += length;
                yield return field.ToArray();
            }
        }

        private string GetKeyId(string modulus)
        {
            return modulus.GetHashCode().ToString();
        }
    }
}
