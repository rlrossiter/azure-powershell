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
using System.IO;
using System.Management.Automation;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Factories;
using Microsoft.Azure.Commands.Common.Authentication.Ssh;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Commands.Ssh.Models;

namespace Microsoft.Azure.Commands.Ssh
{
    [Cmdlet("Get", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "VMSshConfig")]
    [OutputType(typeof(PSSshConfigEntry))]
    public class GetAzureVMSshConfigCommand : SshBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipeline = true)]
        [ResourceNameCompleter("Microsoft.Compute/virtualMachines", "ResourceGroupName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string PublicKeyFile { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 3,
            ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string PrivateKeyFile { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var result = this.VirtualMachineClient.GetWithHttpMessagesAsync(
                this.ResourceGroupName, this.Name).GetAwaiter().GetResult();

            string publicIpAddress = GetFirstPublicIp(result.Body);

            if (string.IsNullOrEmpty(publicIpAddress))
            {
                throw new PSArgumentException($"VM {this.Name} does not have a public IP address to SSH to.");
            }

            string certFileName = GetCertificateFileName(this.PublicKeyFile);
            RSAParser rsa = new RSAParser(BitConverter.IsLittleEndian);
            rsa.ParseKey(File.ReadAllText(PublicKeyFile));

            var factory = AzureSession.Instance.AuthenticationFactory as AuthenticationFactory;
            var credentials = factory.GetClientCertificateCredentials(new RSASSHCertificateAuthenticationParameters(rsa.Modulus, rsa.Exponent), DefaultContext);
            File.WriteAllText(certFileName, credentials.Certificate);

            PSSshConfigEntry entry = new PSSshConfigEntry
            {
                Host = this.ResourceGroupName + "-" + this.Name,
                HostName = publicIpAddress,
                CertificateFile = certFileName,
                IdentityFile = this.PrivateKeyFile
            };
            WriteObject(entry);

            PSSshConfigEntry ipEntry = new PSSshConfigEntry
            {
                Host = publicIpAddress,
                HostName = publicIpAddress,
                CertificateFile = certFileName,
                IdentityFile = this.PrivateKeyFile
            };
            WriteObject(ipEntry);
        }
    }
}