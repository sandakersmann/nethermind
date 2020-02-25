//  Copyright (c) 2018 Demerzel Solutions Limited
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
using System.Diagnostics;
using System.Text;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Dirichlet.Numerics;

namespace Nethermind.Core
{
    [DebuggerDisplay("{Hash}, Value: {Value}, To: {To}, Gas: {GasLimit}")]
    public class Transaction
    {
        public const int BaseTxGasCost = 21000;
        
        private readonly bool _isSystem;

        public Transaction() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isSystem"></param>
        /// <remarks>ctor based genesis allocations are treated as system transactions.</remarks>
        public Transaction(bool isSystem)
        {
            _isSystem = isSystem;
        }

        public UInt256 Nonce { get; set; }
        public UInt256 GasPrice { get; set; }
        public long GasLimit { get; set; }
        public Address To { get; set; }
        public UInt256 Value { get; set; }
        public byte[] Data { get; set; }
        public Address SenderAddress { get; set; }
        public Signature Signature { get; set; }
        public bool IsSigned => Signature != null;
        public bool IsContractCreation => To == null;
        public bool IsMessageCall => To != null;
        public Keccak Hash { get; set; }
        public PublicKey DeliveredBy { get; set; } // tks: this is added so we do not send the pending tx back to original sources, not used yet
        public UInt256 Timestamp { get; set; }

        public string ToShortString()
        {
            return $"[TX: from {SenderAddress} to {To} with data {Data?.ToHexString() ?? Data?.ToHexString()}, gas price {GasPrice} and limit {GasLimit}, nonce {Nonce}]";
        }
        
        public string ToString(string indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{indent}Gas Price: {GasPrice}");
            builder.AppendLine($"{indent}Gas Limit: {GasLimit}");
            builder.AppendLine($"{indent}To: {To}");
            builder.AppendLine($"{indent}Nonce: {Nonce}");
            builder.AppendLine($"{indent}Value: {Value}");
            builder.AppendLine($"{indent}Data: {(Data ?? Array.Empty<byte>()).ToHexString()}");
            builder.AppendLine($"{indent}Hash: {Hash}");
            return builder.ToString();
        }

        public override string ToString() => ToString(string.Empty);

        public bool IsSystem() => SenderAddress == Address.SystemUser || _isSystem;
    }
}