﻿//  Copyright (c) 2018 Demerzel Solutions Limited
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

using Nethermind.Core;
using Nethermind.Dirichlet.Numerics;
using Nethermind.Specs.Forks;
using Nethermind.State;

namespace Nethermind.Consensus.AuRa.Contracts
{
    public class SystemContract : Contract
    {
        protected SystemContract(Address contractAddress) : base(contractAddress)
        {
        }
        
        public void EnsureSystemAccount(IStateProvider stateProvider)
        {
            if (!stateProvider.AccountExists(Address.SystemUser))
            {
                stateProvider.CreateAccount(Address.SystemUser, UInt256.Zero);
                stateProvider.Commit(Homestead.Instance);
            }
        }

        protected Transaction GenerateSystemTransaction(byte[] transactionData, long gasLimit = long.MaxValue, UInt256? nonce = null) => GenerateTransaction(transactionData, Address.SystemUser, gasLimit, nonce);
    }
}