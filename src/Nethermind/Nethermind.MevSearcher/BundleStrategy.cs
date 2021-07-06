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
// 

using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.MevSearcher.Data;
using Nethermind.State;

namespace Nethermind.MevSearcher
{
    public class BundleStrategy : IBundleStrategy
    {
        private readonly IMevSearcherConfig _mevSearcherConfig;
        private readonly IStateProvider _stateProvider;
        private readonly ISigner _signer;

        public BundleStrategy(IMevSearcherConfig mevSearcherConfig, IStateProvider stateProvider, ISigner signer)
        {
            _mevSearcherConfig = mevSearcherConfig;
            _stateProvider = stateProvider;
            _signer = signer;
        }

        private Address Address => _signer.Address;
        
        public bool ProcessTransaction(Transaction transaction, out MevBundle bundle)
        {
            bundle = null;
            return false;
        }
    }
}