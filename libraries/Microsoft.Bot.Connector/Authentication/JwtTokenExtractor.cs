﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    public class JwtTokenExtractor
    {
        /// <summary>
        /// The endorsements validator delegate.
        /// </summary>
        /// <param name="endorsements">The endorsements used for validation.</param>
        /// <returns>true if validation passes; false otherwise.</returns>
        public delegate bool EndorsementsValidator(string[] endorsements);

        /// <summary>
        /// Cache for OpenIdConnect configuration managers (one per metadata URL)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _openIdMetadataCache =
            new ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Cache for Endorsement configuration managers (one per metadata URL)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, string[]>>> _endorsementsCache =
            new ConcurrentDictionary<string, ConfigurationManager<IDictionary<string, string[]>>>();

        /// <summary>
        /// Token validation parameters for this instance
        /// </summary>
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// OpenIdConnect configuration manager for this instance
        /// </summary>
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        /// <summary>
        /// Endorsements configuration manager for this instance
        /// </summary>
        private readonly ConfigurationManager<IDictionary<string, string[]>> _endorsementsData;

        /// <summary>
        /// Allowed signing algorithms
        /// </summary>
        private readonly string[] _allowedSigningAlgorithms;

        /// <summary>
        /// Delegate for validating endorsements extracted from JwtToken
        /// </summary>
        private readonly EndorsementsValidator _validator;
        
        /// <summary>
        /// Extracts relevant data from JWT Tokens
        /// </summary>
        /// <param name="httpClient">As part of validating JWT Tokens, endorsements need to be feteched from
        /// sources specified by the relevant security URLs. This HttpClient is used to allow for resource
        /// pooling around those retrievals. As those resources require TLS sharing the HttpClient is 
        /// important to overall perfomance.</param>
        /// <param name="tokenValidationParameters"></param>
        /// <param name="metadataUrl"></param>
        /// <param name="allowedSigningAlgorithms"></param>
        /// <param name="validator"></param>
        public JwtTokenExtractor(HttpClient httpClient, TokenValidationParameters tokenValidationParameters, string metadataUrl, string[] allowedSigningAlgorithms, EndorsementsValidator validator)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();
            _tokenValidationParameters.RequireSignedTokens = true;
            _allowedSigningAlgorithms = allowedSigningAlgorithms;
            _validator = validator;

            _openIdMetadata = _openIdMetadataCache.GetOrAdd(metadataUrl, key =>
            {
                return new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl, new OpenIdConnectConfigurationRetriever());
            });

            _endorsementsData = _endorsementsCache.GetOrAdd(metadataUrl, key =>
            {
                var retriever = new EndorsementsRetriever(httpClient);
                return new ConfigurationManager<IDictionary<string, string[]>>(metadataUrl, retriever, retriever);
            });
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(HttpRequestMessage request)
        {
            if (request.Headers.Authorization != null)
                return await GetIdentityAsync(
                    request.Headers.Authorization.Scheme,
                    request.Headers.Authorization.Parameter).ConfigureAwait(false);

            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader)
        {
            if (authorizationHeader == null)
                return null;

            string[] parts = authorizationHeader?.Split(' ');
            if (parts.Length == 2)
                return await GetIdentityAsync(parts[0], parts[1]).ConfigureAwait(false);

            return null;
        }

        public static string ExtractBearerTokenFromAuthHeader(string authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                throw new ArgumentException(nameof(authorizationHeader));

            string[] parts = authorizationHeader.Split(' ');
            if (parts.Length != 2)
            {
                // The Auth Header must have exactly 2 parts:
                // "Bearer [jwtEncodedString]"
                throw new InvalidOperationException($"Authorization Header has '{parts.Length} Parts. Expected value is 2.");
            }

            string scheme = parts[0];
            if (scheme != "Bearer")
            {
                // The Auth Header must have exactly 2 parts:
                // "Bearer [jwtEncodedString]"
                throw new InvalidOperationException("Incorrect Scheme. Only 'Bearer' is supported."); 
            }

            return parts[1]; 
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter)
        {
            // No header in correct scheme or no token
            if (scheme != "Bearer" || string.IsNullOrEmpty(parameter))
                return null;

            // Issuer isn't allowed? No need to check signature
            if (!HasAllowedIssuer(parameter))
                return null;

            try
            {
                ClaimsPrincipal claimsPrincipal = await ValidateTokenAsync(parameter).ConfigureAwait(false);
                return claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Invalid token. " + e.ToString());
                throw;
            }
        }

        private bool HasAllowedIssuer(string jwtToken)
        {
            JwtSecurityToken token = new JwtSecurityToken(jwtToken);
            if (_tokenValidationParameters.ValidIssuer != null && _tokenValidationParameters.ValidIssuer == token.Issuer)
                return true;

            if ((_tokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>()).Contains(token.Issuer))
                return true;

            return false;
        }
              
        private async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken)
        {
            // _openIdMetadata only does a full refresh when the cache expires every 5 days
            OpenIdConnectConfiguration config = null;
            try
            {
                config = await _openIdMetadata.GetConfigurationAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error refreshing OpenId configuration: {e}");

                // No config? We can't continue
                if (config == null)
                    throw;
            }

            // Update the signing tokens from the last refresh
            _tokenValidationParameters.IssuerSigningKeys = config.SigningKeys;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out SecurityToken parsedToken);
                var parsedJwtToken = parsedToken as JwtSecurityToken;

                if (_validator != null)
                {
                    string keyId = (string)parsedJwtToken?.Header?[AuthenticationConstants.KeyIdHeader];
                    var endorsements = await _endorsementsData.GetConfigurationAsync();
                    if (!string.IsNullOrEmpty(keyId) && endorsements.ContainsKey(keyId))
                    {
                        if (!_validator(endorsements[keyId]))
                        {
                            throw new UnauthorizedAccessException($"Could not validate endorsement for key: {keyId} with endorsements: {string.Join(",", endorsements[keyId])}");
                        }
                    }
                }

                if (_allowedSigningAlgorithms != null)
                {
                    string algorithm = parsedJwtToken?.Header?.Alg;
                    if (!_allowedSigningAlgorithms.Contains(algorithm))
                    {
                        throw new UnauthorizedAccessException($"Token signing algorithm '{algorithm}' not in allowed list");
                    }
                }
                return principal;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                string keys = string.Join(", ", ((config?.SigningKeys) ?? Enumerable.Empty<SecurityKey>()).Select(t => t.KeyId));
                Trace.TraceError("Error finding key for token. Available keys: " + keys);
                throw;
            }
        }
    }
}