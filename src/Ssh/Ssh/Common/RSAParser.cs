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

namespace Microsoft.Azure.Commands.Ssh
{
    public class RSAParser
    {
        private const string algorithm = "ssh-rsa";

        public string Modulus { get; private set; }

        public string Exponent { get; private set; }

        public RSAParser(string publicKeyFileContents)
        {
            AssertPublicKeyFormat(publicKeyFileContents);
        }

        private void AssertPublicKeyFormat(string publicKey)
        {
            string[] keyParts = publicKey.Split(' ');

            if (keyParts.Length != 3)
            {
                throw new FormatException("Expected 3 space-separated parts to the public key (algorithm, key, comment)");
            }
        }

        private string GetModulus(string publicKey)
        {
            string key = publicKey.Split(' ')[1];
            Span<byte> keyBytes = Convert.FromBase64String(key);

            var str = BitConverter.ToString(keyBytes.ToArray());

            return null;
        }

        private string GetExponent(string publicKey)
        {
            return null;
        }
    }
}
