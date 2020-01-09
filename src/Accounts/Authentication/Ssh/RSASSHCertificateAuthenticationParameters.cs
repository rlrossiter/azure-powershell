﻿// ----------------------------------------------------------------------------------
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
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.Common.Authentication.Ssh
{
    public class RSASSHCertificateAuthenticationParameters : SSHCertificateAuthenticationParameters
    {
        private string jwk;
        private const string rsaExponent = "AQAB";

        public override SshKeyType KeyType => SshKeyType.RSA;
        public override string Jwk => jwk;

        public RSASSHCertificateAuthenticationParameters(string modulus)
        {
            Dictionary<string, string> jwkDict = new Dictionary<string, string>
            {
                { KeyTypeKey, SshKeyType.RSA.ToString() },
                { ModulusKey, modulus },
                { ExponentKey, rsaExponent }
            };

            jwk = JsonConvert.SerializeObject(jwkDict);
        }
    }
}
