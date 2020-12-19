﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grayscale.P027MoveGen.L00025_MovableMove;
using Grayscale.P025_KifuLarabe.L00012_Atom;
using Grayscale.P025_KifuLarabe.L00025_Struct;

namespace Grayscale.P027MoveGen.L050_MovableMove
{
    public class MmGenjo_MovableMasuImpl : MmGenjo_MovableMasu
    {
        public bool IsHonshogi { get { return this.isHonshogi; } }
        private bool isHonshogi;

        public SkyConst Src_Sky { get { return this.src_Sky; } }
        private SkyConst src_Sky;

        public Playerside Pside_genTeban3 { get { return this.pside_genTeban3; } }
        private Playerside pside_genTeban3;

        public bool IsAiteban { get { return this.isAiteban; } }
        private bool isAiteban;

        public MmGenjo_MovableMasuImpl(
            bool isHonshogi,
            SkyConst src_Sky,
            Playerside pside_genTeban3,
            bool isAiteban
            )
        {
            this.isHonshogi = isHonshogi;
            this.src_Sky = src_Sky;
            this.pside_genTeban3 = pside_genTeban3;
            this.isAiteban = isAiteban;
        }
    }
}