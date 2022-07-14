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

using Nethermind.Core;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Logging;
using Nethermind.State;

namespace Nethermind.Consensus.Processing
{
    internal static class TransactionProcessorAdapterExtensions
    {
        public static void ProcessTransaction(this ITransactionProcessorAdapter transactionProcessor,
            Block block,
            Transaction currentTx,
            BlockReceiptsTracer receiptsTracer,
            ProcessingOptions processingOptions,
            IStateProvider stateProvider,
            ILogger logger)
        {
            logger.Info($"ProcessTransaction before if");
            if (processingOptions.ContainsFlag(ProcessingOptions.DoNotVerifyNonce))
            {
                logger.Info($"ProcessTransaction inside if");
                currentTx.Nonce = stateProvider.GetNonce(currentTx.SenderAddress);
            }
            else
            {
                logger.Info($"ProcessTransaction inside else");
            }
            logger.Info($"ProcessTransaction after if else");

            receiptsTracer.StartNewTxTrace(currentTx);
            logger.Info($"ProcessTransaction after StartNewTxTrace");

            transactionProcessor.Execute(currentTx, block.Header, receiptsTracer);
            logger.Info($"ProcessTransaction after Execute");

            receiptsTracer.EndTxTrace();
            logger.Info($"ProcessTransaction after EndTxTrace");

        }
    }
}
