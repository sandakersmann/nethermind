//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Nethermind.Core.Extensions;
using Nethermind.JsonRpc;
using Nethermind.Serialization.Json;

namespace Nethermind.Overseer.Test.JsonRpc
{
    public class JsonRpcClient : IJsonRpcClient
    {
        private readonly string _host;
        private readonly string _methodPrefix = "ndm_";
        private readonly HttpClient _client;

        public JsonRpcClient(string host)
        {
            _host = host;
            _client = new HttpClient
            {
                BaseAddress = new Uri(host)
            };
        }
        public JsonRpcClient(string host, string hexSecret) : this(host)
        {
            hexSecret = hexSecret.Trim();
            if (!Regex.IsMatch(hexSecret, @"^(0x)?[0-9a-fA-F]{64}$"))
            {
                throw new FormatException("JWT secret should be a 64 digit hexadecimal number");
            }
            byte[] decodedSecret = Bytes.FromHexString(hexSecret);
            var securityKey = new SymmetricSecurityKey(decodedSecret);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = signingCredentials,
                Expires = DateTime.Now.AddYears(1)
            };
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.CreateEncodedJwt(tokenDescriptor);
            _client.DefaultRequestHeaders.Authorization = new
                AuthenticationHeaderValue("Bearer", token);
        }

        public Task<JsonRpcResponse<T>> PostAsync<T>(string method)
            => PostAsync<T>(method, Array.Empty<object>());

        public async Task<JsonRpcResponse<T>> PostAsync<T>(string method, object[] @params)
        {
            string methodToCall = method.Contains("_") ? method : $"{_methodPrefix}{method}";
            Console.WriteLine($"Sending {methodToCall} to {_host}");
            var request = new JsonRpcRequest(methodToCall, @params);
            var payload = GetPayload(request);
            string errorMessage = null;
            HttpResponseMessage response = await _client.PostAsync("/", payload).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    errorMessage = t.Exception.Unwrap().Message;
                    return null;
                }
                else if (t.IsCanceled)
                {
                    return null;
                }

                return t.Result;
            });

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                var result = new JsonRpcResponse<T>();
                result.Error = new JsonRpcResponse<T>.ErrorResponse(ErrorCodes.InternalError, errorMessage);
                return result;
            }

            return await response.Content.ReadAsStringAsync()
                .ContinueWith(t => new EthereumJsonSerializer().Deserialize<JsonRpcResponse<T>>(t.Result));
        }

        private StringContent GetPayload(JsonRpcRequest request)
            => new StringContent(new EthereumJsonSerializer().Serialize(request), Encoding.UTF8, "application/json");

        private class JsonRpcRequest
        {
            public string JsonRpc { get; set; }
            public int Id { get; set; }
            public string Method { get; set; }
            public object[] Params { get; set; }

            public JsonRpcRequest(string method, object[] @params) : this("2.0", 1, method, @params)
            {
            }

            public JsonRpcRequest(string jsonRpc, int id, string method, object[] @params)
            {
                JsonRpc = jsonRpc;
                Id = id;
                Method = method;
                Params = @params;
            }
        }

    }
}
