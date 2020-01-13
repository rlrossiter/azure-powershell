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

using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Azure.Commands.Ssh.Test
{
    public class RSAParserTests
    {
        private static string rsaAglorithm = "ssh-rsa";
        private static string comment = "comment";

        private ITestOutputHelper _output;

        public RSAParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestCorrectKey()
        {
            string dummyKey = GetDummyKey();
            string pubKeyString = BuildPublicKeyString(rsaAglorithm, dummyKey, comment);

            RSAParser parser = new RSAParser(BitConverter.IsLittleEndian);
            parser.ParseKey(pubKeyString);
            string modulus = parser.Modulus;
            string exponent = parser.Exponent;

            string expectedModulus = "AK8C2iIVXn13yRodSHzZTLluyjmDr9dZCS7r77N9Y2ifmanLxItZPZAqJNR" +
                "5a4zzrA6YGFwVs7/w+jCi/xtbNlGSA0qfDI84vc+ne7Kr2ZoZthwZ45LvnuFmB2Yky6XL+ju0acm7TDYR" +
                "+nx5l0abPUVk6Lin6zn4RYAvyM4IXBcS5mb6aOwIislWV9noCu+6dD81L7aV3hzzGzEf+yYAkfDfjBcjXjd" +
                "3K2M6ZAs3wBsaEP3bsKc0twV0nZ52pMELlTtHD+HLmFpz3FW38xwrTRuoBU6HPEhzGTomLJoxJoKBogvDWzo" +
                "iJGIlwX+8IfYVgiHNEU9s61gUFzVIfveszPc=";

            Assert.Equal("AQAB", exponent);
            Assert.Equal(expectedModulus, modulus);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestWrongAlgorithm()
        {
            string dummyKey = GetDummyKey();
            string pubKeyString = BuildPublicKeyString("ssh-dsa", dummyKey, comment);

            RSAParser parser = new RSAParser(BitConverter.IsLittleEndian);

            Assert.ThrowsAny<ArgumentException>(() => parser.ParseKey(pubKeyString));
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestNotEnoughKeyParts()
        {
            RSAParser parser = new RSAParser(BitConverter.IsLittleEndian);

            Assert.ThrowsAny<ArgumentException>(() => parser.ParseKey(rsaAglorithm));
        }

        private string BuildPublicKeyString(string algorithm, string b64EncodedKey, string comment)
        {
            return string.Format("{0} {1} {2}", algorithm, b64EncodedKey, comment);
        }

        private string GetDummyKey()
        {
            string consolePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Resources", "DummyKey.txt");
            string vsPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "DummyKey.txt");

            string pathToUse = File.Exists(consolePath) ? consolePath : vsPath;
            return File.ReadAllText(pathToUse);
        }
    }
}
