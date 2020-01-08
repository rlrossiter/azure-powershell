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

using System.Text;

namespace Microsoft.Azure.Commands.Ssh.Models
{
    public class PSSshConfigEntry
    {
        public string Host { get; set; }

        public string HostName { get; set; }

        public string CertificateFile { get; set; }

        public string IdentityFile { get; set; }

        public string ConfigString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Format("Host {0}", this.Host));
                builder.AppendLine(string.Format("\tHostName {0}", this.HostName));
                builder.AppendLine(string.Format("\tCertificateFile {0}", this.CertificateFile));
                builder.Append(string.Format("\tIdentityFile {0}", this.IdentityFile));
                return builder.ToString();
            }
        }
    }
}
