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

using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Azure.Management.Network;
using System.IO;
using System.Linq;

namespace Microsoft.Azure.Commands.Ssh
{
    public abstract class SshBaseCmdlet : AzureRMCmdlet
    {
        private ComputeClient computeClient;
        private NetworkClient networkClient;
        private RMProfileClient profileClient;

        public ComputeClient ComputeClient
        {
            get
            {
                if (computeClient == null)
                {
                    computeClient = new ComputeClient(DefaultProfile.DefaultContext);
                }

                return computeClient;
            }

            set { computeClient = value; }
        }

        public NetworkClient NetworkClient
        {
            get
            {
                if (networkClient == null)
                {
                    networkClient = new NetworkClient(DefaultProfile.DefaultContext);
                }
                return networkClient;
            }

            set { networkClient = value; }
        }

        public RMProfileClient ProfileClient
        {
            get
            {
                if (profileClient == null)
                {
                    profileClient = new RMProfileClient(DefaultProfile as AzureRmProfile);
                }
                return profileClient;
            }

            set { profileClient = value; }
        }

        public IVirtualMachinesOperations VirtualMachineClient
        {
            get
            {
                return ComputeClient.ComputeManagementClient.VirtualMachines;
            }
        }

        public INetworkInterfacesOperations NetworkInterfaceClient
        {
            get
            {
                return NetworkClient.NetworkManagementClient.NetworkInterfaces;
            }
        }

        public IPublicIPAddressesOperations PublicIpAddressClient
        {
            get
            {
                return NetworkClient.NetworkManagementClient.PublicIPAddresses;
            }
        }

        public string GetCertificateFileName(string publicKeyFileName)
        {
            string directoryName = Path.GetFullPath(Path.GetDirectoryName(publicKeyFileName));
            string certFileName = Path.GetFileNameWithoutExtension(publicKeyFileName) + ".cer";
            return Path.Combine(directoryName, certFileName);
        }

        public string GetFirstPublicIp(VirtualMachine vm)
        {
            string publicIpAddress = null;

            foreach (var nicReference in vm.NetworkProfile.NetworkInterfaces)
            {
                string nicRg = AzureIdUtilities.GetResourceGroup(nicReference.Id);
                string nicName = AzureIdUtilities.GetResourceName(nicReference.Id);

                var nic = this.NetworkClient.NetworkManagementClient.NetworkInterfaces.GetWithHttpMessagesAsync(
                    nicRg, nicName).GetAwaiter().GetResult();

                var publicIps = nic.Body.IpConfigurations.Where(ipconfig => ipconfig.PublicIPAddress != null).Select(ipconfig => ipconfig.PublicIPAddress);
                foreach (var ip in publicIps)
                {
                    var ipRg = AzureIdUtilities.GetResourceGroup(ip.Id);
                    var ipName = AzureIdUtilities.GetResourceName(ip.Id);
                    var ipAddress = this.NetworkClient.NetworkManagementClient.PublicIPAddresses.GetWithHttpMessagesAsync(
                        ipRg, ipName).GetAwaiter().GetResult().Body;

                    publicIpAddress = ipAddress.IpAddress;

                    if (!string.IsNullOrEmpty(publicIpAddress))
                    {
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(publicIpAddress))
                {
                    break;
                }
            }

            return publicIpAddress;
        }
    }
}
