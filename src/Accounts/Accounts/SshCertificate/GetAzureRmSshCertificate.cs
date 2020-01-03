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
using System.IO;
using System.Management.Automation;
using System.Text;
using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.Profile
{
    [Cmdlet("Get", AzureRMConstants.AzureRMPrefix + "SshCertificate")]
    [OutputType(typeof(string))]
    public class GetAzureRmSshCertificateCommand : AzureRMCmdlet
    {
        private RMProfileClient _client;

        [Parameter(Mandatory = true, HelpMessage = "The file containing your public key")]
        [ValidateNotNullOrEmpty]
        public string PublicKeyFile { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _client = new RMProfileClient(DefaultProfile as AzureRmProfile);
        }

        public override void ExecuteCmdlet()
        {
            string modulus = File.ReadAllText(PublicKeyFile).Split(' ')[1];
            string kty = "RSA";
            string n = modulus;
            string e = "AQAB";
            string kid = modulus.GetHashCode().ToString();

            Dictionary<string, string> jwk = new Dictionary<string, string>
            {
                { "kty", kty },
                { "n", n },
                { "e", e },
            };

            string jwkStrJson = JsonConvert.SerializeObject(jwk);

            var context = DefaultContext;
            string cert = _client.GetSSHCertificate(context.Account, context.Environment, context.Tenant.Id, jwkStrJson, kid, null, true, (str) => { });
            WriteObject(cert);
        }
    }
}
