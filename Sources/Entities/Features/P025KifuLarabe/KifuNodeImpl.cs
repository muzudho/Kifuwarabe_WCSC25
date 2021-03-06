﻿using System;
using System.Text;
using Finger = ProjectDark.NamedInt.StrictNamedInt0; //スプライト番号

namespace Grayscale.Kifuwarazusa.Entities.Features
{
    public class KifuNodeImpl : NodeImpl<ShootingStarlightable, KyokumenWrapper>, KifuNode
    {

        public Playerside Tebanside { get { return this.tebanside; } }
        private Playerside tebanside;

        public static Playerside GetReverseTebanside(Playerside tebanside1)
        {
            Playerside side2;
            switch (tebanside1)
            {
                case Playerside.P1: side2 = Playerside.P2; break;
                case Playerside.P2: side2 = Playerside.P1; break;
                case Playerside.Empty: side2 = Playerside.Empty; break;
                default: throw new Exception($"未定義のプレイヤーサイド [{tebanside1}]");
            }

            return side2;
        }



        public KyHyoka KyHyoka { get { return this.kyHyoka; } }
        /// <summary>
        /// 枝専用。
        /// </summary>
        /// <param name="branchKyHyoka"></param>
        public void SetBranchKyHyoka(KyHyoka branchKyHyoka)
        {
            this.kyHyoka = branchKyHyoka;
        }
        private KyHyoka kyHyoka;


        public KifuNodeImpl(ShootingStarlightable shootingStarlightable, KyokumenWrapper kyokumenWrapper, Playerside tebanside)
            : base(shootingStarlightable, kyokumenWrapper)
        {
            this.kyHyoka = new KyHyokaImpl();
            this.tebanside = tebanside;
        }

        public void AppdendNextNodes(Node<ShootingStarlightable, KyokumenWrapper> hubNode)
        {

            hubNode.Foreach_NextNodes((string key, Node<ShootingStarlightable, KyokumenWrapper> node, ref bool toBreak) =>
            {
                if (!this.ContainsKey_NextNodes(key))
                {
                    this.Add_NextNode(key, node);
                }
            });
        }


        /// <summary>
        /// 棋譜に符号を追加します。
        /// 
        /// KifuIO を通して使ってください。
        /// 
        /// ①コマ送り用。
        /// ②「成り」フラグの更新用。
        /// ③マウス操作用
        /// 
        /// カレントノードは変更しません。
        /// </summary>
        public void AppendChildA_New(
            Node<ShootingStarlightable, KyokumenWrapper> newNode
            )
        {
            System.Diagnostics.Debug.Assert(!this.ContainsKey_NextNodes(Util_Sky.ToSfenMoveText(newNode.Key)));

            // SFENをキーに、次ノードを増やします。
            this.Add_NextNode(Util_Sky.ToSfenMoveText(newNode.Key), newNode);

            newNode.PreviousNode = this;

        }


        /// <summary>
        /// 指し手一覧を、駒毎に分けます。
        /// </summary>
        /// <param name="hubNode">指し手一覧</param>
        /// <param name="logTag"></param>
        /// <returns>駒毎の、全指し手</returns>
        public Maps_OneAndMulti<Finger, ShootingStarlightable> SplitMoveByKoma(Node<ShootingStarlightable, KyokumenWrapper> hubNode)
        {
            SkyConst src_Sky = this.Value.ToKyokumenConst;


            Maps_OneAndMulti<Finger, ShootingStarlightable> enable_teMap = new Maps_OneAndMulti<Finger, ShootingStarlightable>();


            hubNode.Foreach_NextNodes((string key, Node<ShootingStarlightable, KyokumenWrapper> nextNode, ref bool toBreak) =>
            {
                ShootingStarlightable nextMove = nextNode.Key;

                Finger figKoma = Util_Sky.Fingers_AtMasuNow(src_Sky, Util_Koma.AsKoma(nextMove.LongTimeAgo).Masu).ToFirst();

                enable_teMap.AddOverwrite(figKoma, nextMove);
            });

            return enable_teMap;
        }

        public string Json_NextNodes_MultiSky(
            string memo,
            string hint,
            int tesumi_yomiGenTeban_forLog//読み進めている現在の手目済
            )
        {
            StringBuilder sb = new StringBuilder();

            this.Foreach_NextNodes((string key, Node<ShootingStarlightable, KyokumenWrapper> node, ref bool toBreak) =>
            {
                sb.AppendLine(Util_Sky.Json_1Sky(
                    node.Value.ToKyokumenConst,
                    memo + "：" + key,
                    hint + "_SF解1",
                    tesumi_yomiGenTeban_forLog
                    ));// 局面をテキストで作成
            });

            return sb.ToString();
        }


        public ISfenPosition1 ToRO_Kyokumen1()
        {
            ISfenPosition1 ro_Kyokumen1 = new SfenPosition1Impl();

            SkyConst src_Sky = this.Value.ToKyokumenConst;

            for (int suji = 1; suji < 10; suji++)
            {
                for (int dan = 1; dan < 10; dan++)
                {
                    Finger koma0 = Util_Sky.Fingers_AtMasuNow(
                        src_Sky, Util_Masu.OkibaSujiDanToMasu(Okiba.ShogiBan, suji, dan)
                        ).ToFirst();

                    if (Fingers.Error_1 != koma0)
                    {
                        RO_Star_Koma koma1 = Util_Koma.AsKoma(src_Sky.StarlightIndexOf(koma0).Now);

                        ro_Kyokumen1.Ban[suji][dan] = KomaSyurui14Array.SfenText(
                            Haiyaku184Array.Syurui(koma1.Haiyaku),
                            koma1.Pside
                            );
                    }
                }
            }


            int mK = 0;
            int mR = 0;
            int mB = 0;
            int mG = 0;
            int mS = 0;
            int mN = 0;
            int mL = 0;
            int mP = 0;

            int mk = 0;
            int mr = 0;
            int mb = 0;
            int mg = 0;
            int ms = 0;
            int mn = 0;
            int ml = 0;
            int mp = 0;
            this.GetMoti(
                src_Sky,
                out mK,
                out mR,
                out mB,
                out mG,
                out mS,
                out mN,
                out mL,
                out mP,

                out mk,
                out mr,
                out mb,
                out mg,
                out ms,
                out mn,
                out ml,
                out mp
                );


            ro_Kyokumen1.MotiP1[0] = mR;
            ro_Kyokumen1.MotiP1[1] = mB;
            ro_Kyokumen1.MotiP1[2] = mG;
            ro_Kyokumen1.MotiP1[3] = mS;
            ro_Kyokumen1.MotiP1[4] = mN;
            ro_Kyokumen1.MotiP1[5] = mL;
            ro_Kyokumen1.MotiP1[6] = mP;

            ro_Kyokumen1.MotiP2[0] = mr;
            ro_Kyokumen1.MotiP2[1] = mb;
            ro_Kyokumen1.MotiP2[2] = mg;
            ro_Kyokumen1.MotiP2[3] = ms;
            ro_Kyokumen1.MotiP2[4] = mn;
            ro_Kyokumen1.MotiP2[5] = ml;
            ro_Kyokumen1.MotiP2[6] = mp;

            return ro_Kyokumen1;
        }


        public string ToSfenstring(Playerside pside)
        {
            SkyConst src_Sky = this.Value.ToKyokumenConst;

            StringBuilder sb = new StringBuilder();
            sb.Append("sfen ");

            for (int dan = 1; dan <= 9; dan++)
            {
                int spaceCount = 0;

                for (int suji = 9; suji >= 1; suji--)
                {
                    // 将棋盤上のどこかにある駒？
                    Finger koma0 = Util_Sky.Fingers_AtMasuNow(
                        src_Sky, Util_Masu.OkibaSujiDanToMasu(Okiba.ShogiBan, suji, dan)
                        ).ToFirst();

                    if (Fingers.Error_1 != koma0)
                    {
                        if (0 < spaceCount)
                        {
                            sb.Append(spaceCount);
                            spaceCount = 0;
                        }


                        RO_Star_Koma koma1 = Util_Koma.AsKoma(src_Sky.StarlightIndexOf(koma0).Now);



                        sb.Append(KomaSyurui14Array.SfenText(
                            Haiyaku184Array.Syurui(koma1.Haiyaku),
                            koma1.Pside
                            ));
                    }
                    else
                    {
                        spaceCount++;
                    }

                }

                if (0 < spaceCount)
                {
                    sb.Append(spaceCount);
                    spaceCount = 0;
                }

                if (dan != 9)
                {
                    sb.Append("/");
                }
            }

            sb.Append(" ");

            //------------------------------------------------------------
            // 先後
            //------------------------------------------------------------
            switch (pside)
            {
                case Playerside.P2:
                    sb.Append("w");
                    break;
                default:
                    sb.Append("b");
                    break;
            }

            sb.Append(" ");

            //------------------------------------------------------------
            // 持ち駒
            //------------------------------------------------------------
            {
                int mK = 0;
                int mR = 0;
                int mB = 0;
                int mG = 0;
                int mS = 0;
                int mN = 0;
                int mL = 0;
                int mP = 0;

                int mk = 0;
                int mr = 0;
                int mb = 0;
                int mg = 0;
                int ms = 0;
                int mn = 0;
                int ml = 0;
                int mp = 0;
                this.GetMoti(
                    src_Sky,
                    out mK,
                    out mR,
                    out mB,
                    out mG,
                    out mS,
                    out mN,
                    out mL,
                    out mP,

                    out mk,
                    out mr,
                    out mb,
                    out mg,
                    out ms,
                    out mn,
                    out ml,
                    out mp
                    );



                if (0 == mK + mR + mB + mG + mS + mN + mL + mP + mk + mr + mb + mg + ms + mn + ml + mp)
                {
                    sb.Append("-");
                }
                else
                {
                    if (0 < mK)
                    {
                        if (1 < mK)
                        {
                            sb.Append(mK);
                        }
                        sb.Append("K");
                    }

                    if (0 < mR)
                    {
                        if (1 < mR)
                        {
                            sb.Append(mR);
                        }
                        sb.Append("R");
                    }

                    if (0 < mB)
                    {
                        if (1 < mB)
                        {
                            sb.Append(mB);
                        }
                        sb.Append("B");
                    }

                    if (0 < mG)
                    {
                        if (1 < mG)
                        {
                            sb.Append(mG);
                        }
                        sb.Append("G");
                    }

                    if (0 < mS)
                    {
                        if (1 < mS)
                        {
                            sb.Append(mS);
                        }
                        sb.Append("S");
                    }

                    if (0 < mN)
                    {
                        if (1 < mN)
                        {
                            sb.Append(mN);
                        }
                        sb.Append("N");
                    }

                    if (0 < mL)
                    {
                        if (1 < mL)
                        {
                            sb.Append(mL);
                        }
                        sb.Append("L");
                    }

                    if (0 < mP)
                    {
                        if (1 < mP)
                        {
                            sb.Append(mP);
                        }
                        sb.Append("P");
                    }

                    if (0 < mk)
                    {
                        if (1 < mk)
                        {
                            sb.Append(mk);
                        }
                        sb.Append("k");
                    }

                    if (0 < mr)
                    {
                        if (1 < mr)
                        {
                            sb.Append(mr);
                        }
                        sb.Append("r");
                    }

                    if (0 < mb)
                    {
                        if (1 < mb)
                        {
                            sb.Append(mb);
                        }
                        sb.Append("b");
                    }

                    if (0 < mg)
                    {
                        if (1 < mg)
                        {
                            sb.Append(mg);
                        }
                        sb.Append("g");
                    }

                    if (0 < ms)
                    {
                        if (1 < ms)
                        {
                            sb.Append(ms);
                        }
                        sb.Append("s");
                    }

                    if (0 < mn)
                    {
                        if (1 < mn)
                        {
                            sb.Append(mn);
                        }
                        sb.Append("n");
                    }

                    if (0 < ml)
                    {
                        if (1 < ml)
                        {
                            sb.Append(ml);
                        }
                        sb.Append("l");
                    }

                    if (0 < mp)
                    {
                        if (1 < mp)
                        {
                            sb.Append(mp);
                        }
                        sb.Append("p");
                    }
                }

            }

            // 手目
            sb.Append(" 1");

            return sb.ToString();
        }

        private void GetMoti(
            SkyConst src_Sky,
            out int mK,
            out int mR,
            out int mB,
            out int mG,
            out int mS,
            out int mN,
            out int mL,
            out int mP,

            out int mk,
            out int mr,
            out int mb,
            out int mg,
            out int ms,
            out int mn,
            out int ml,
            out int mp
        )
        {
            mK = 0;
            mR = 0;
            mB = 0;
            mG = 0;
            mS = 0;
            mN = 0;
            mL = 0;
            mP = 0;

            mk = 0;
            mr = 0;
            mb = 0;
            mg = 0;
            ms = 0;
            mn = 0;
            ml = 0;
            mp = 0;

            // 先手
            Fingers komasS = Util_Sky.Fingers_ByOkibaNow(src_Sky, Okiba.Sente_Komadai);
            foreach (Finger figKoma in komasS.Items)
            {
                RO_Star_Koma koma = Util_Koma.AsKoma(src_Sky.StarlightIndexOf(figKoma).Now);

                PieceType syurui = KomaSyurui14Array.NarazuCaseHandle(Haiyaku184Array.Syurui(koma.Haiyaku));
                if (PieceType.K == syurui)
                {
                    mK++;
                }
                else if (PieceType.R == syurui)
                {
                    mR++;
                }
                else if (PieceType.B == syurui)
                {
                    mB++;
                }
                else if (PieceType.G == syurui)
                {
                    mG++;
                }
                else if (PieceType.S == syurui)
                {
                    mS++;
                }
                else if (PieceType.N == syurui)
                {
                    mN++;
                }
                else if (PieceType.L == syurui)
                {
                    mL++;
                }
                else if (PieceType.P == syurui)
                {
                    mP++;
                }
                else
                {
                }
            }



            // 後手
            Fingers komasG = Util_Sky.Fingers_ByOkibaNow(src_Sky, Okiba.Gote_Komadai);
            foreach (Finger figKoma in komasG.Items)
            {
                RO_Star_Koma koma = Util_Koma.AsKoma(src_Sky.StarlightIndexOf((int)figKoma).Now);


                PieceType syurui = KomaSyurui14Array.NarazuCaseHandle(Haiyaku184Array.Syurui(koma.Haiyaku));

                if (PieceType.K == syurui)
                {
                    mk++;
                }
                else if (PieceType.R == syurui)
                {
                    mr++;
                }
                else if (PieceType.B == syurui)
                {
                    mb++;
                }
                else if (PieceType.G == syurui)
                {
                    mg++;
                }
                else if (PieceType.S == syurui)
                {
                    ms++;
                }
                else if (PieceType.N == syurui)
                {
                    mn++;
                }
                else if (PieceType.L == syurui)
                {
                    ml++;
                }
                else if (PieceType.P == syurui)
                {
                    mp++;
                }
                else
                {
                }
            }

        }
    }
}
