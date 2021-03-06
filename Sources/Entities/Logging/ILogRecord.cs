﻿using Grayscale.Kifuwarazusa.Entities.Configuration;

namespace Grayscale.Kifuwarazusa.Entities.Logging
{
    public interface ILogRecord
    {
        /// <summary>
        /// 出力先ファイル。
        /// </summary>
        IResFile LogFile { get; }

        /// <summary>
        /// ログ出力の有無。
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// タイムスタンプの有無。
        /// </summary>
        bool TimeStampPrintable { get; }
    }
}
