﻿using Grayscale.Kifuwarazusa.Entities.Logging;

namespace Grayscale.P025_KifuLarabe.L00060_KifuParser
{
    public interface KifuParserA_Log
    {
        string Hint { get; }
        ILogTag LogTag { get; }
    }
}