﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Grayscale.Kifuwarazusa.Entities.Features;
using Grayscale.Kifuwarazusa.Entities.Features.Gui;
using Grayscale.Kifuwarazusa.Entities.Logging;
using Finger = ProjectDark.NamedInt.StrictNamedInt0; //スプライト番号

namespace Grayscale.Kifuwarazusa.UseCases.Features
{
    public class Util_InServer
    {
        /// <summary>
        /// 「棋譜ツリーのカレントノード」の差替え、
        /// および
        /// 「ＧＵＩ用局面データ」との同期。
        /// 
        /// (1) 駒をつまんでいるときに、マウスの左ボタンを放したとき。
        /// (2) 駒の移動先の升の上で、マウスの左ボタンを放したとき。
        /// (3) 成る／成らないダイアログボックスが出たときに、マウスの左ボタンを押下したとき。
        /// </summary>
        /// <param name="kifu"></param>
        /// <param name="newNode"></param>
        public static void SetCurNode_Srv(
            IRoomViewModel roomViewModel,
            Node<ShootingStarlightable, KyokumenWrapper> newNode
            )
        {
            //ShogiGui shogiGui = (ShogiGui)shogiGui_Base;
            Debug.Assert(null != newNode, "新規ノードがヌル。");

            roomViewModel.GameViewModel.Kifu.CurNode = newNode;

            roomViewModel.GameViewModel.SetGuiSky(newNode.Value.ToKyokumenConst);
            roomViewModel.GameViewModel.GuiTesumi = Util_InServer.CountCurTesumi2(roomViewModel);
            roomViewModel.GameViewModel.GuiPside = Util_InServer.CurPside(roomViewModel);
        }

        public static int CountCurTesumi2(IRoomViewModel roomViewModel)
        {
            return roomViewModel.GameViewModel.Kifu.CountTesumi(roomViewModel.GameViewModel.Kifu.CurNode);
        }

        public static Playerside CurPside(IRoomViewModel roomViewModel)
        {
            return roomViewModel.GameViewModel.Kifu.CountPside(roomViewModel.GameViewModel.Kifu.CurNode);
        }

        /// <summary>
        /// １手進む
        /// 
        /// ＜棋譜読込用＞＜マウス入力非対応＞
        /// 
        /// 「棋譜並べモード」と「vsコンピューター対局」でも、これを使いますが、
        /// 「棋譜並べモード」では送られてくる SFEN が「position startpos moves 8c8d」とフルであるのに対し、
        /// 「vsコンピューター対局」では、送られてくる SFEN が「8c8d」だけです。
        /// 
        /// それにより、処理の流れが異なります。
        /// 
        /// </summary>
        public static bool ReadLine_TuginoItteSusumu_Srv(
            ref string inputLine,
            IRoomViewModel roomViewModel,
            out bool toBreak,
            string hint
            ,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            bool successful = false;
            KifuParserA_Impl kifuParserA_Impl = new KifuParserA_Impl();
            KifuParserA_Result result = new KifuParserA_ResultImpl();
            KifuParserA_Genjo genjo = new KifuParserA_GenjoImpl(inputLine);

            try
            {

                kifuParserA_Impl.Delegate_OnChangeSky_Im_Srv = Util_InServer.OnChangeSky_Im_Srv;


                if (kifuParserA_Impl.State is KifuParserA_StateA0_Document)
                {
                    // 最初はここ

                    Logger.Trace("... ...");
                    Logger.Trace("ｻｲｼｮﾊｺｺ☆　：　" + memberName + "." + sourceFilePath + "." + sourceLineNumber);
                    inputLine = kifuParserA_Impl.Execute_Step(
                        ref result,
                        roomViewModel,
                        genjo
                        );

                    Debug.Assert(result.Out_newNode_OrNull == null, "ここでノードに変化があるのはおかしい。");

                    if (genjo.ToBreak)
                    {
                        goto gt_EndMethod;
                    }
                    // （１）position コマンドを処理しました。→startpos
                    // （２）日本式棋譜で、何もしませんでした。→moves
                }

                if (kifuParserA_Impl.State is KifuParserA_StateA1_SfenPosition)
                {
                    //------------------------------------------------------------
                    // このブロックでは「position ～ moves 」まで一気に(*1)を処理します。
                    //------------------------------------------------------------
                    //
                    //          *1…初期配置を作るということです。
                    // 

                    {
                        string message = "ﾂｷﾞﾊ　ﾋﾗﾃ　ﾏﾀﾊ　ｼﾃｲｷｮｸﾒﾝ　ｦ　ｼｮﾘｼﾀｲ☆ inputLine=[" + inputLine + "]";
                        Logger.Trace(message);

                        inputLine = kifuParserA_Impl.Execute_Step(
                            ref result,
                            roomViewModel,
                            genjo
                            );
                        Debug.Assert(result.Out_newNode_OrNull == null, "ここでノードに変化があるのはおかしい。");


                        if (genjo.ToBreak)
                        {
                            goto gt_EndMethod;
                        }
                    }


                    {
                        Logger.Trace("ﾂｷﾞﾊ　ﾑｰﾌﾞｽ　ｦ　ｼｮﾘｼﾀｲ☆");
                        inputLine = kifuParserA_Impl.Execute_Step(
                            ref result,
                            roomViewModel,
                            genjo
                            );
                        Debug.Assert(result.Out_newNode_OrNull == null, "ここでノードに変化があるのはおかしい。");


                        if (genjo.ToBreak)
                        {
                            goto gt_EndMethod;
                        }
                        // moves を処理しました。
                        // ここまでで、「position ～ moves 」といった記述が入力されていたとすれば、入力欄から削除済みです。
                    }


                    // →moves
                }

                //
                // 対COMP戦で関係があるのはここです。
                //
                if (kifuParserA_Impl.State is KifuParserA_StateA2_SfenMoves)
                {
                    Logger.Trace("ﾂｷﾞﾊ　ｲｯﾃ　ｼｮﾘｼﾀｲ☆");
                    inputLine = kifuParserA_Impl.Execute_Step(
                        ref result,
                        roomViewModel,
                        genjo
                        );

                    if (null != result.Out_newNode_OrNull)
                    {
                        Util_InServer.SetCurNode_Srv(roomViewModel, result.Out_newNode_OrNull);
                    }

                    if (genjo.ToBreak)
                    {
                        goto gt_EndMethod;
                    }


                    // １手を処理した☆？
                }


                successful = true;
            }
            catch (Exception ex)
            {
                Util_Message.Show(ex.GetType().Name + "：" + ex.Message);
                toBreak = true;
                successful = false;
            }

        gt_EndMethod:
            toBreak = genjo.ToBreak;
            return successful;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kifu"></param>
        /// <param name="restText"></param>
        /// <param name="startposImporter"></param>
        /// <param name="logTag"></param>
        public static void OnChangeSky_Im_Srv(
            IRoomViewModel roomViewModel,
            StartposImporter startposImporter,
            KifuParserA_Genjo genjo
            )
        {
            // SFENの初期配置の書き方(*1)を元に、駒を並べます。
            // 
            //     *1…「position startpos moves 7g7f 3c3d 2g2f」といった書き方。
            //     

            //------------------------------
            // 駒の配置
            //------------------------------
            {
                // 先後
                Playerside nextTebanside;
                if (startposImporter.RO_SfenStartpos.PsideIsBlack)
                {
                    // 黒は先手。
                    nextTebanside = Converter04.AlternatePside(Playerside.P1);//FIXME:逆か？
                }
                else
                {
                    // 白は後手。
                    nextTebanside = Converter04.AlternatePside(Playerside.P2);//FIXME:逆か？
                }

                ShootingStarlightable move = Util_Sky.NullObjectMove;//ルートなので

                SkyConst src_Sky_New = startposImporter.ToSky();
                Node<ShootingStarlightable, KyokumenWrapper> newNode =
                        new KifuNodeImpl(
                            move,
                            new KyokumenWrapper(new SkyConst(src_Sky_New)),
                            nextTebanside
                        );

                Util_InServer.SetCurNode_Srv(roomViewModel, newNode);// GUIに通知するだけ。
            }


            //------------------------------
            // 先後
            //------------------------------
            if (startposImporter.RO_SfenStartpos.PsideIsBlack)
            {
                // 黒は先手。
                roomViewModel.GameViewModel.Kifu.SetProperty(KifuTreeImpl.PropName_FirstPside, Converter04.AlternatePside(Playerside.P1));//FIXME:逆か？
            }
            else
            {
                // 白は後手。
                roomViewModel.GameViewModel.Kifu.SetProperty(KifuTreeImpl.PropName_FirstPside, Converter04.AlternatePside(Playerside.P2));//FIXME:逆か？
            }

            // 駒袋に表示されている駒を、駒台に表示させます。
            {
                // 駒袋に表示されている駒を、駒台へ表示されるように移します。
                List<PieceType> syuruiList = new List<PieceType>();
                List<int> countList = new List<int>();
                List<Playerside> psideList = new List<Playerside>();

                //------------------------------------------------------------------------------------------------------------------------
                // 移動計画作成
                //------------------------------------------------------------------------------------------------------------------------

                //------------------------------
                // ▲王
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1K)
                {
                    syuruiList.Add(PieceType.K);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1K);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mK=" + ro_SfenStartpos.Moti1K);
                }

                //------------------------------
                // ▲飛
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1R)
                {
                    syuruiList.Add(PieceType.R);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1R);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mR=" + ro_SfenStartpos.Moti1R);
                }

                //------------------------------
                // ▲角
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1B)
                {
                    syuruiList.Add(PieceType.B);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1B);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mB=" + ro_SfenStartpos.Moti1B);
                }

                //------------------------------
                // ▲金
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1G)
                {
                    syuruiList.Add(PieceType.G);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1G);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mG=" + ro_SfenStartpos.Moti1G);
                }

                //------------------------------
                // ▲銀
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1S)
                {
                    syuruiList.Add(PieceType.S);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1S);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mS=" + ro_SfenStartpos.Moti1S);
                }

                //------------------------------
                // ▲桂
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1N)
                {
                    syuruiList.Add(PieceType.N);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1N);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mN=" + ro_SfenStartpos.Moti1N);
                }

                //------------------------------
                // ▲香
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1L)
                {
                    syuruiList.Add(PieceType.L);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1L);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mL=" + ro_SfenStartpos.Moti1L);
                }

                //------------------------------
                // ▲歩
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti1P)
                {
                    syuruiList.Add(PieceType.P);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti1P);
                    psideList.Add(Playerside.P1);
                    //System.C onsole.WriteLine("mP=" + ro_SfenStartpos.Moti1P);
                }

                //------------------------------
                // △王
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2k)
                {
                    syuruiList.Add(PieceType.K);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2k);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mk=" + ro_SfenStartpos.Moti2k);
                }

                //------------------------------
                // △飛
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2r)
                {
                    syuruiList.Add(PieceType.R);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2r);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mr=" + ro_SfenStartpos.Moti2r);
                }

                //------------------------------
                // △角
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2b)
                {
                    syuruiList.Add(PieceType.B);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2b);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mb=" + ro_SfenStartpos.Moti2b);
                }

                //------------------------------
                // △金
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2g)
                {
                    syuruiList.Add(PieceType.G);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2g);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mg=" + ro_SfenStartpos.Moti2g);
                }

                //------------------------------
                // △銀
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2s)
                {
                    syuruiList.Add(PieceType.S);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2s);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("ms=" + ro_SfenStartpos.Moti2s);
                }

                //------------------------------
                // △桂
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2n)
                {
                    syuruiList.Add(PieceType.N);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2n);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mn=" + ro_SfenStartpos.Moti2n);
                }

                //------------------------------
                // △香
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2l)
                {
                    syuruiList.Add(PieceType.L);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2l);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("ml=" + ro_SfenStartpos.Moti2l);
                }

                //------------------------------
                // △歩
                //------------------------------
                if (0 < startposImporter.RO_SfenStartpos.Moti2p)
                {
                    syuruiList.Add(PieceType.P);
                    countList.Add(startposImporter.RO_SfenStartpos.Moti2p);
                    psideList.Add(Playerside.P2);
                    //System.C onsole.WriteLine("mp=" + ro_SfenStartpos.Moti2p);
                }




                //------------------------------------------------------------------------------------------------------------------------
                // 移動
                //------------------------------------------------------------------------------------------------------------------------
                for (int i = 0; i < syuruiList.Count; i++)
                {
                    Playerside itaruPside;   //(至)先後
                    Okiba itaruOkiba;   //(至)置き場

                    if (Playerside.P2 == psideList[i])
                    {
                        // 宛：後手駒台
                        itaruPside = Playerside.P2;
                        itaruOkiba = Okiba.Gote_Komadai;
                    }
                    else
                    {
                        // 宛：先手駒台
                        itaruPside = Playerside.P1;
                        itaruOkiba = Okiba.Sente_Komadai;
                    }


                    //------------------------------
                    // 駒を、駒袋から駒台に移動させます。
                    //------------------------------
                    {
                        SkyBuffer buffer_Sky = new SkyBuffer(roomViewModel.GameViewModel.GuiSkyConst);

                        Fingers komas = Util_Sky.Fingers_ByOkibaSyuruiNow(new SkyConst(buffer_Sky), Okiba.KomaBukuro, syuruiList[i]);
                        int moved = 1;
                        foreach (Finger koma in komas.Items)
                        {
                            // 駒台の空いている枡
                            SyElement akiMasu = KifuIO.GetKomadaiKomabukuroSpace(itaruOkiba, new SkyConst(buffer_Sky));

                            buffer_Sky.AddOverwriteStarlight(koma, new RO_MotionlessStarlight(
                                //koma,
                                new RO_Star_Koma(
                                itaruPside,
                                akiMasu,
                                syuruiList[i]
                                )));

                            if (countList[i] <= moved)
                            {
                                break;
                            }

                            moved++;
                        }
                        roomViewModel.GameViewModel.SetGuiSky(new SkyConst(buffer_Sky));
                    }

                }
            }
        }

        /// <summary>
        /// [巻戻し]ボタン
        /// </summary>
        public static bool Makimodosi_Srv(
            out Finger movedKoma,
            out Finger foodKoma,
            out string fugoJStr,
            IRoomViewModel roomViewModel
            )
        {
            bool successful = false;

            //------------------------------
            // 棋譜から１手削ります
            //------------------------------
            Node<ShootingStarlightable, KyokumenWrapper> removeeLeaf = roomViewModel.GameViewModel.Kifu.CurNode;

            if (removeeLeaf.PreviousNode == null)
            {
                // ルート
                fugoJStr = "×";
                movedKoma = Fingers.Error_1;
                foodKoma = Fingers.Error_1;
                goto gt_EndMethod;
            }


            //------------------------------
            // 符号
            //------------------------------
            //
            // 移動前と、移動後では、駒が変わっていることがあります。
            // 例えば、「▲２二角成」なら　移動前は角、移動後は馬です。
            //
            // そこで[巻戻し]ボタンでは、移動前の駒に従って、「進んできた動きとは逆の動き」を行います。

            RO_Star_Koma koma = Util_Koma.AsKoma(removeeLeaf.Key.LongTimeAgo);

            fugoJStr = JFugoCreator15Array.ItemMethods[
                (int)Haiyaku184Array.Syurui(koma.Haiyaku)
            ](
                removeeLeaf.Key,
                new KyokumenWrapper(roomViewModel.GameViewModel.GuiSkyConst)
            ).ToText_UseDou(removeeLeaf);
            //MessageBox.Show("[巻戻し]符号＝" + fugoJStr, "デバッグ");

            //------------------------------
            // 前の手に戻します
            //------------------------------
            //Finger movedKoma;
            //Finger foodKoma = Fingers.Error_1;//取られた駒
            foodKoma = Fingers.Error_1;//取られた駒
            bool isMakimodosi = true;

            Node<ShootingStarlightable, KyokumenWrapper> out_newNode_OrNull;
            KifuIO.Ittesasi(
                removeeLeaf.Key,
                roomViewModel.GameViewModel.Kifu,
                isMakimodosi,
                out movedKoma,
                out foodKoma,
                out out_newNode_OrNull
                );

            successful = true;


        gt_EndMethod:
            return successful;
        }

        /// <summary>
        /// [コマ送り]ボタン
        /// 
        /// vsコンピューター対局でも、タイマーによって[コマ送り]が実行されます。
        /// 
        /// </summary>
        public static bool Komaokuri_Srv(
            ref string inputLine,
            IRoomViewModel roomViewModel
            ,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {

            bool toBreak = false;
            Util_InServer.ReadLine_TuginoItteSusumu_Srv(
                ref inputLine,
                roomViewModel,
                out toBreak,
                "hint"
                );

            return true;
        }

        /// <summary>
        /// 駒を動かします(1)。マウスボタンが押下されたとき。
        /// 
        /// 成る、成らない関連。
        /// 
        /// </summary>
        public static void Komamove1a_50Srv(
            out bool torareruKomaAri,
            out RO_Star_Koma koma_Food_after,
            Starlight dst,
            Finger btnTumandeiruKoma_Koma,
            RO_Star_Koma koma1,
            IRoomViewModel roomViewModel
            )
        {
            Finger btnKoma_Food_Koma;

            torareruKomaAri = false;


            // 取られることになる駒のボタン
            btnKoma_Food_Koma = Util_Sky.Fingers_AtMasuNow(roomViewModel.GameViewModel.GuiSkyConst, koma1.Masu).ToFirst();
            if (Fingers.Error_1 == btnKoma_Food_Koma)
            {
                koma_Food_after = null;
                btnKoma_Food_Koma = Fingers.Error_1;
                goto gt_EndBlock1;
            }




            //>>>>> 取る駒があったとき
            torareruKomaAri = true;

            PieceType koma_Food_pre_Syurui = Util_Koma.AsKoma(roomViewModel.GameViewModel.GuiSkyConst.StarlightIndexOf(btnKoma_Food_Koma).Now).Syurui;


            // その駒は、駒置き場に移動させます。
            SyElement akiMasu;
            switch (koma1.Pside)
            {
                case Playerside.P2:

                    akiMasu = KifuIO.GetKomadaiKomabukuroSpace(Okiba.Gote_Komadai, roomViewModel.GameViewModel.GuiSkyConst);
                    if (Masu_Honshogi.Error != akiMasu)
                    {
                        // 駒台に空きスペースがありました。
                        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                        koma_Food_after = new RO_Star_Koma(
                            Playerside.P2,
                            akiMasu,//駒台へ
                            KomaSyurui14Array.NarazuCaseHandle(koma_Food_pre_Syurui)
                        );
                    }
                    else
                    {
                        // エラー：　駒台に空きスペースがありませんでした。
                        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        koma_Food_after = new RO_Star_Koma(
                            Playerside.P2,
                            Util_Masu.OkibaSujiDanToMasu(
                                Okiba.Gote_Komadai,
                                Util_Koma.CTRL_NOTHING_PROPERTY_SUJI,
                                Util_Koma.CTRL_NOTHING_PROPERTY_DAN
                            ),
                            KomaSyurui14Array.NarazuCaseHandle(koma_Food_pre_Syurui)
                        );
                    }

                    break;

                case Playerside.P1://thru
                default:

                    akiMasu = KifuIO.GetKomadaiKomabukuroSpace(Okiba.Sente_Komadai, roomViewModel.GameViewModel.GuiSkyConst);
                    if (Masu_Honshogi.Error != akiMasu)
                    {
                        // 駒台に空きスペースがありました。
                        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        koma_Food_after = new RO_Star_Koma(
                            Playerside.P1,
                            akiMasu,//駒台へ
                            KomaSyurui14Array.NarazuCaseHandle(koma_Food_pre_Syurui)
                        );
                    }
                    else
                    {
                        // エラー：　駒台に空きスペースがありませんでした。
                        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        koma_Food_after = new RO_Star_Koma(
                            Playerside.P1,
                            Util_Masu.OkibaSujiDanToMasu(
                                Okiba.Sente_Komadai,
                                Util_Koma.CTRL_NOTHING_PROPERTY_SUJI,
                                Util_Koma.CTRL_NOTHING_PROPERTY_DAN
                            ),
                            KomaSyurui14Array.NarazuCaseHandle(koma_Food_pre_Syurui)
                        );
                    }

                    break;
            }



        gt_EndBlock1:


            if (torareruKomaAri)
            {
                {
                    SkyBuffer buffer_Sky1;

                    // 取られる動き
                    buffer_Sky1 = new SkyBuffer(roomViewModel.GameViewModel.GuiSkyConst);
                    buffer_Sky1.AddOverwriteStarlight(
                        btnKoma_Food_Koma,
                        new RO_MotionlessStarlight(
                            //btnKoma_Food_Koma,
                            koma_Food_after
                        )
                    );

                    roomViewModel.GameViewModel.SetGuiSky(new SkyConst(buffer_Sky1));
                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                    // 棋譜は変更された。
                    // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                }

                //------------------------------
                // 成りは解除。
                //------------------------------
                switch (Util_Masu.GetOkiba(koma_Food_after.Masu))
                {
                    case Okiba.Sente_Komadai://thru
                    case Okiba.Gote_Komadai:
                        // 駒台へ移動しました
                        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                        SkyBuffer buffer_Sky2 = new SkyBuffer(roomViewModel.GameViewModel.GuiSkyConst);
                        buffer_Sky2.AddOverwriteStarlight(
                            btnKoma_Food_Koma,
                            new RO_MotionlessStarlight(
                                //btnKoma_Food_Koma,
                                koma_Food_after
                            )
                        );

                        roomViewModel.GameViewModel.SetGuiSky(new SkyConst(buffer_Sky2));
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
                        // 棋譜は変更された。
                        // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

                        break;
                }
            }

            SkyBuffer buffer_Sky = new SkyBuffer(roomViewModel.GameViewModel.GuiSkyConst);
            buffer_Sky.AddOverwriteStarlight(btnTumandeiruKoma_Koma, dst);
            roomViewModel.GameViewModel.SetGuiSky(new SkyConst(buffer_Sky));
            // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
            // 棋譜は変更された。
            // ■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        }


    }


}
