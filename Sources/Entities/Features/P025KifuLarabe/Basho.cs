﻿namespace Grayscale.Kifuwarazusa.Entities.Features
{

    /// <summary>
    /// マス番号の一時代替。
    /// </summary>
    public class Basho : SyElement
    {

        public string Word { get { return this.word; } }
        private string word;


        public int number;
        public int MasuNumber
        {
            get
            {
                return this.number;
            }
        }

        public Basho(int number)
        {
            this.word = "升" + number;
            this.number = number;
        }

        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            Basho obj2 = (Basho)obj;

            return this.MasuNumber == obj2.MasuNumber;
        }

        public override int GetHashCode()
        {
            return this.MasuNumber;
        }

    }
}
