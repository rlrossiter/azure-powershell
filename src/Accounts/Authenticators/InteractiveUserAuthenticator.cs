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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Hyak.Common;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Identity.Client.SSHCertificates;
using Newtonsoft.Json;

namespace Microsoft.Azure.PowerShell.Authenticators
{
    /// <summary>
    /// Authenticator for user interactive authentication
    /// </summary>
    public class InteractiveUserAuthenticator : DelegatingAuthenticator
    {
        public override Task<IAccessToken> Authenticate(AuthenticationParameters parameters, CancellationToken cancellationToken)
        {
            var interactiveParameters = parameters as InteractiveParameters;
            var onPremise = interactiveParameters.Environment.OnPremise;
            var authenticationClientFactory = interactiveParameters.AuthenticationClientFactory;
            IPublicClientApplication publicClient = null;
            var resource = interactiveParameters.Environment.GetEndpoint(interactiveParameters.ResourceId);
            var scopes = new string[] { string.Format(AuthenticationHelpers.DefaultScope, resource) };
            TcpListener listener = null;
            var replyUrl = string.Empty;
            var port = 8399;
            try
            {
                while (++port < 9000)
                {
                    try
                    {
                        listener = new TcpListener(IPAddress.Loopback, port);
                        listener.Start();
                        replyUrl = string.Format("http://localhost:{0}", port);
                        listener.Stop();
                        break;
                    }
                    catch (Exception ex)
                    {
                        interactiveParameters.PromptAction(string.Format("Port {0} is taken with exception '{1}'; trying to connect to the next port.", port, ex.Message));
                        listener?.Stop();
                    }
                }

                if (!string.IsNullOrEmpty(replyUrl))
                {
                    var clientId = AuthenticationHelpers.PowerShellClientId;
                    var authority = onPremise ?
                                        interactiveParameters.Environment.ActiveDirectoryAuthority :
                                        AuthenticationHelpers.GetAuthority(parameters.Environment, parameters.TenantId);
                    TracingAdapter.Information(string.Format("[InteractiveUserAuthenticator] Creating IPublicClientApplication - ClientId: '{0}', Authority: '{1}', ReplyUrl: '{2}' UseAdfs: '{3}'", clientId, authority, replyUrl, onPremise));
                    publicClient = authenticationClientFactory.CreatePublicClient(clientId: clientId, authority: authority, redirectUri: replyUrl, useAdfs: onPremise);
                    TracingAdapter.Information(string.Format("[InteractiveUserAuthenticator] Calling AcquireTokenInteractive - Scopes: '{0}'", string.Join(",", scopes)));

                    var interactiveResponse = publicClient.AcquireTokenInteractive(scopes)
                        .WithCustomWebUi(new CustomWebUi())
                        .ExecuteAsync(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return AuthenticationResultToken.GetAccessTokenAsync(interactiveResponse);
                }
            }
            catch
            {
                interactiveParameters.PromptAction("Unable to authenticate using interactive login. Defaulting back to device code flow.");
            }

            return null;
        }

        public override Task<IAccessToken> AuthenticateSSH(AuthenticationParameters parameters, string jwk, string kid, CancellationToken cancellationToken)
        {
            var interactiveParameters = parameters as InteractiveParameters;
            var onPremise = interactiveParameters.Environment.OnPremise;
            var authenticationClientFactory = interactiveParameters.AuthenticationClientFactory;
            IPublicClientApplication publicClient = null;
            var resource = interactiveParameters.Environment.GetEndpoint(interactiveParameters.ResourceId);
            var scopes = new string[] { string.Format(AuthenticationHelpers.DefaultScope, resource) };
            TcpListener listener = null;
            var replyUrl = string.Empty;
            var port = 8399;
            try
            {
                while (++port < 9000)
                {
                    try
                    {
                        listener = new TcpListener(IPAddress.Loopback, port);
                        listener.Start();
                        replyUrl = string.Format("http://localhost:{0}", port);
                        listener.Stop();
                        break;
                    }
                    catch (Exception ex)
                    {
                        interactiveParameters.PromptAction(string.Format("Port {0} is taken with exception '{1}'; trying to connect to the next port.", port, ex.Message));
                        listener?.Stop();
                    }
                }

                if (!string.IsNullOrEmpty(replyUrl))
                {
                    var clientId = AuthenticationHelpers.PowerShellClientId;
                    var authority = onPremise ?
                                        interactiveParameters.Environment.ActiveDirectoryAuthority :
                                        AuthenticationHelpers.GetAuthority(parameters.Environment, parameters.TenantId);
                    TracingAdapter.Information(string.Format("[InteractiveUserAuthenticator] Creating IPublicClientApplication - ClientId: '{0}', Authority: '{1}', ReplyUrl: '{2}' UseAdfs: '{3}'", clientId, authority, replyUrl, onPremise));
                    publicClient = authenticationClientFactory.CreatePublicClient(clientId: clientId, authority: authority, redirectUri: replyUrl, useAdfs: onPremise);
                    TracingAdapter.Information(string.Format("[InteractiveUserAuthenticator] Calling AcquireTokenInteractive - Scopes: '{0}'", string.Join(",", scopes)));

                    var interactiveResponse = publicClient.AcquireTokenInteractive(scopes)
                        .WithCustomWebUi(new CustomWebUi())
                        .WithSSHCertificateAuthenticationScheme(jwk, kid)
                        .ExecuteAsync(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return AuthenticationResultToken.GetAccessTokenAsync(interactiveResponse);
                }
            }
            catch
            {
                interactiveParameters.PromptAction("Unable to authenticate using interactive login. Defaulting back to device code flow.");
            }

            return null;
        }

        public override bool CanAuthenticate(AuthenticationParameters parameters)
        {
            return (parameters as InteractiveParameters) != null;
        }
    }
}
