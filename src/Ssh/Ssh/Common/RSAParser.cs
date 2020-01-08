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

using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Ssh
{
    public class RSAParser
    {
        public const string KeyType = "RSA";
        public const string Exponent = "AQAB";

        private const string KeyTypeKey = "kty";
        private const string ModulusKey = "n";
        private const string ExponentKey = "e";

        private string publicKey;
        private string modulus;
        private string keyId;

        public string Modulus
        {
            get
            {
                if (string.IsNullOrEmpty(modulus))
                {
                    modulus = GetModulus(publicKey);
                }

                return modulus;
            }
        }

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

        public Dictionary<string, string> Jwk
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { KeyTypeKey, KeyType },
                    { ModulusKey, Modulus },
                    { ExponentKey, Exponent }
                };
            }
        }

        public RSAParser(string publicKeyFileContents)
        {
            AssertPublicKeyFormat(publicKeyFileContents);
            this.publicKey = publicKeyFileContents;
        }

        private void AssertPublicKeyFormat(string publicKey)
        {
            return;
        }

        private string GetModulus(string publicKey)
        {
            return publicKey.Split(' ')[1];
        }

        private string GetKeyId(string modulus)
        {
            return modulus.GetHashCode().ToString();
        }
    }
}
