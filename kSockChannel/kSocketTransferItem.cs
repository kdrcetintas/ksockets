///-----------------------------------------------------------------
///   Namespace:        kdrcts.kSockChannel
///   Class:            kSocketTransferItem
///   Description:      Has and object class definition for data sending / receiving at kSocketChannel
///   Author:           @kdrcetintas
///   Date:             2018-05-28
///   Version:          1.0
///   Notes:            Enjoy it.
///   Revision History:
///

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace kdrcts.kSockChannel
{
    /// <summary>
    /// DataReceived event
    /// </summary>
    public class kSocketTransferItem
    {
        /// <summary>
        /// A custom byte array for usable at split data receiving actions
        /// </summary>
        public static readonly byte[] TransferSplitData = new byte[] { 60, 115, 111, 99, 107, 69, 79, 70, 62 };

        /// <summary>
        /// 
        /// </summary>
        public Socket TransferSocket = null;

        public Guid TransferKey;

        public int TransferSize;

        public byte[] TempData = null;

        public List<byte> TransferedData = new List<byte>();

        public kSocketTransferItem(int BufferSize = 1024)
        {
            this.TransferKey = Guid.NewGuid();
            this.TransferSize = BufferSize;
            this.TempData = new byte[BufferSize];
        }
    }
}