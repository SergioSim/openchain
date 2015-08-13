﻿using System;

namespace OpenChain
{
    /// <summary>
    /// Represents a transaction affecting the data store.
    /// </summary>
    public class Transaction
    {
        public Transaction(ByteString mutation, DateTime timestamp, ByteString transactionMetadata)
        {
            if (mutation == null)
                throw new ArgumentNullException(nameof(mutation));

            if (transactionMetadata == null)
                throw new ArgumentNullException(nameof(transactionMetadata));

            this.Mutation = mutation;
            this.Timestamp = timestamp;
            this.TransactionMetadata = transactionMetadata;
        }

        /// <summary>
        /// Gets the binary representation of the mutation applied by this transaction.
        /// </summary>
        public ByteString Mutation { get; }

        /// <summary>
        /// Gets the timestamp of the transaction.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the metadata associated with the transaction.
        /// </summary>
        public ByteString TransactionMetadata { get; }
    }
}