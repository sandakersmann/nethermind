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

using System;
using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Logging;
using Nethermind.State;

namespace Nethermind.Consensus.Processing
{
    public partial class BlockProcessor
    {
        public class BlockValidationTransactionsExecutor : IBlockProcessor.IBlockTransactionsExecutor
        {
            private readonly ITransactionProcessorAdapter _transactionProcessor;
            private readonly IStateProvider _stateProvider;
            private readonly ILogger _logger;

            public BlockValidationTransactionsExecutor(ITransactionProcessor transactionProcessor, IStateProvider stateProvider, ILogManager logManager)
                : this(new ExecuteTransactionProcessorAdapter(transactionProcessor), stateProvider)
            {
                _logger = logManager.GetClassLogger();
            }

            public BlockValidationTransactionsExecutor(ITransactionProcessorAdapter transactionProcessor, IStateProvider stateProvider)
            {
                _transactionProcessor = transactionProcessor;
                _stateProvider = stateProvider;
            }

            public event EventHandler<TxProcessedEventArgs>? TransactionProcessed;

            public TxReceipt[] ProcessTransactions(Block block, ProcessingOptions processingOptions, BlockReceiptsTracer receiptsTracer, IReleaseSpec spec)
            {
                _logger.Info($"ProcessTransactions processing block with block.Transactions.Length: {block.Transactions.Length}");

                for (int i = 0; i < block.Transactions.Length; i++)
                {
                    Transaction currentTx = block.Transactions[i];
                    _logger.Info($"ProcessTransactions before ProcessTransaction {i}");
                    ProcessTransaction(block, currentTx, i, receiptsTracer, processingOptions);
                    _logger.Info($"ProcessTransactions after ProcessTransaction {i}");
                }
                return receiptsTracer.TxReceipts.ToArray();
            }

            private void ProcessTransaction(Block block, Transaction currentTx, int index, BlockReceiptsTracer receiptsTracer, ProcessingOptions processingOptions)
            {
                _logger.Info($"ProcessTransaction before _transactionProcessor.ProcessTransaction");
                try
                {
                    _transactionProcessor.ProcessTransaction(block, currentTx, receiptsTracer, processingOptions,
                        _stateProvider, _logger);
                }
                catch (Exception ex)
                {
                    _logger.Info($"_transactionProcessor.ProcessTransaction exception: {ex}");
                }
                _logger.Info($"ProcessTransaction after _transactionProcessor.ProcessTransaction");

                TransactionProcessed?.Invoke(this, new TxProcessedEventArgs(index, currentTx, receiptsTracer.TxReceipts[index]));
                _logger.Info($"ProcessTransaction after invoking ProcessTx event");
            }
        }
    }
}
