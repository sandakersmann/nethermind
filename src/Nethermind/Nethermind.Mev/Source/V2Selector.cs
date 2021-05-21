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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Nethermind.Core;
using Nethermind.Evm;
using Nethermind.Int256;
using Nethermind.Mev.Data;
using Nethermind.Mev.Execution;

namespace Nethermind.Mev.Source
{
    public class V2Selector : IBundleSource
    {
        private readonly ISimulatedBundleSource _simulatedBundleSource;
        private readonly int _maxMergedBundles;
        

        public V2Selector(
            ISimulatedBundleSource simulatedBundleSource,
            int maxMergedBundles)
        {
            _simulatedBundleSource = simulatedBundleSource;
            _maxMergedBundles = maxMergedBundles;
        }
        
        public async Task<IEnumerable<MevBundle>> GetBundles(BlockHeader parent, UInt256 timestamp, long gasLimit, CancellationToken token = default)
        {
            ICollection<MevBundle> includedBundles = Enumerable.Empty<MevBundle>().ToList();
            long totalGasUsed = 0;
            
            IEnumerable<SimulatedMevBundle> simulatedBundles = await _simulatedBundleSource.GetBundles(parent, timestamp, gasLimit, token);
            foreach (SimulatedMevBundle simulatedBundle in simulatedBundles.OrderByDescending(bundle => bundle.BundleAdjustedGasPrice))
            {
                if (includedBundles.Count < _maxMergedBundles)
                {
                    if (simulatedBundle.GasUsed <= gasLimit - totalGasUsed)
                    {
                        includedBundles.Add(simulatedBundle.Bundle);
                        totalGasUsed += simulatedBundle.GasUsed;
                    }
                }
                else
                {
                    break;
                }
            }

            return includedBundles.Any() ? includedBundles : Enumerable.Empty<MevBundle>();
        }
    }
}
