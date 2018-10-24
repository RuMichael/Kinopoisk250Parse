using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kinopoisk_top_250.Core
{
    public class KnFilm : BaseID
    {
        public static int countOrig = 0;
        bool orig;

        public int Number { get; set; }     // позиция

        public string Name { get; set; }    // название

        public int Data { get; set; }       // дата релиза

        public double Rating { get; set; }  // рейтинг

        public int Vote { get; set; }       // кол-во проголосовавших

        public bool Orig
        {
            get { return orig; }
            set
            {
                orig = value;
                if (value)
                    countOrig++;
            }
        }      //костыль для проверки  оригинального имени  

        public KnFilm()
        {

        }

    }
}