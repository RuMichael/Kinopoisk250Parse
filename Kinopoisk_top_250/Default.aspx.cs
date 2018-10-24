using AngleSharp.Parser.Html;
using Kinopoisk_top_250.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kinopoisk_top_250
{       // работает только через Internet Explorer =)
    public partial class Default : System.Web.UI.Page
    {
        List<KnFilm> repository = new List<KnFilm>();
        Table table = new Table();
        
        protected void Page_Load(object sender, EventArgs e)
        {            
            table.HorizontalAlign = HorizontalAlign.Center;
            table.Width = new Unit("70%");
            table.CssClass = "table_background";

            #region // шапка таблицы


            TableHeaderRow hRow = new TableHeaderRow();
            TableHeaderCell hCell;

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Позиция" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Название" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Дата релиза" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Рейтинг" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Количество голосов" });
            hRow.Cells.Add(hCell);

            table.Rows.Add(hRow);
            #endregion

            PlaceHolder1.Controls.Add(table);
        }

        public KnFilm ClearStr(string str)  // записываем номер, название, дату и проверяем указано ли оригинальное название фильма
        {
            string s = "";
            for (int i = 0; i < str.Length; i++)
                if (Char.IsLetterOrDigit(str[i]) || Char.IsPunctuation(str[i]) || str[i] == ' ')
                    s += str[i];
            while (s.Contains("  "))
                s = s.Remove(s.IndexOf("  "), 1);
            string number = s.Remove(s.IndexOf('.'));
            s = s.Remove(0, s.IndexOf('.') + 1);
            string name = s.Remove(s.IndexOf('('));
            s = s.Remove(0, s.IndexOf('(') + 1);
            string data = s.Remove(s.IndexOf(')'));
            s = s.Remove(0, s.IndexOf(')') + 1);
            s = s.Remove(s.IndexOf(')') + 1);
            return new KnFilm { Number = int.Parse(number), Name = name, Data = int.Parse(data), Orig = s.Length > 16 ? true : false};
        }
                      
        protected void BParse_Click(object sender, EventArgs e)     // парсим страницу кинопоиска
        {
            repository.Clear();

            #region // скачиваем страницу топ 250 кинопоиска

            string HtmlText;
            HttpWebRequest myHttwebrequest = (HttpWebRequest)HttpWebRequest.Create("https://www.kinopoisk.ru/top/");
            HttpWebResponse myHttpWebresponse = (HttpWebResponse)myHttwebrequest.GetResponse();
            StreamReader strm = new StreamReader(myHttpWebresponse.GetResponseStream(), Encoding.GetEncoding(1251));
            HtmlText = strm.ReadToEnd();
            #endregion

            #region     // парсим страницу

            var parser = new HtmlParser();
            var rty = parser.Parse(HtmlText);
            var items = (rty).QuerySelectorAll("tr"); // номер и год
            var items_name = (rty).QuerySelectorAll("span"); // имя
            var items_rating = (rty).QuerySelectorAll("a"); // рейтинг
            #endregion

            #region // формируем коллекцию из 250 фильмов

            foreach (var item in items)
                if (item.Id != null && item.Id.Contains("top250_place") && item.TextContent.Contains("("))
                    repository.Add(ClearStr(item.TextContent));
            #endregion

            #region     //записываем оригинальное имя и кол-во проголосовавших в колекцию фильмов

            List<string> orig_name = new List<string>();
            List<string> vote = new List<string>();
            foreach (var item in items_name)
            {
                if (item.ClassName != null && item.ClassName.Contains("text-grey"))
                    orig_name.Add(item.TextContent);
                if (item.ClassName == null && item.InnerHtml.Contains("nbsp"))
                    vote.Add(item.TextContent);
            }

            int j = 0;
            foreach (var item in repository)
                if (item.Orig)
                {
                    item.Name = orig_name[j];
                    item.Orig = false;
                    j++;
                }

            j = 0;
            foreach (var item in repository)
            {
                string vote_s = "";
                for (int i = 0; i < vote[j].Length; i++)
                    if (Char.IsNumber(vote[j][i]))
                        vote_s += vote[j][i];

                item.Vote = int.Parse(vote_s);
                j++;
            }
            #endregion

            #region // записываем рейтинг в коллекцию фильмов

            j = 0;
            foreach (var item in items_rating)
                if (item.ClassName != null && item.ClassName.Contains("continue"))
                {
                    repository[j].Rating = double.Parse(item.TextContent.Replace('.', ','));
                    j++;
                }
            #endregion

            BToSql.Enabled = true;
            BFind.Enabled = true;
            CBFrom.Enabled = true;
            CBTo.Enabled = true;

            PrintTable();
            CBElement();
            Button knopka = (Button)sender;
            if (knopka.ID.Contains("BToSql"))
            {
                using (KnContext context = new KnContext())
                {
                    if (context.Films.ToList().Count == 0)
                        context.Films.AddRange(repository);
                    else
                    {
                        List<KnFilm> rip = context.Films.ToList();
                        context.Films.RemoveRange(rip);
                        context.Films.AddRange(repository);
                    }
                    context.SaveChanges();
                }
            }
        }

        protected void BToSql_Click(object sender, EventArgs e)
        {
            BParse_Click(sender, e);
        }

        protected void BLoadSql_Click(object sender, EventArgs e)
        {            
            using (KnContext context = new KnContext())
            {
                if (context.Films.ToList().Count != 0)
                    repository = context.Films.ToList();
                else
                {
                    LExp.Text = "Бд пустая";
                    return;
                }
            }
            PrintTable();
            CBElement();

            BFind.Enabled = true;
            CBFrom.Enabled = true;
            CBTo.Enabled = true;
        }

        public void PrintTable()    // вывести топ 250 на страницу
        {
            table.Rows.Clear();

            table.HorizontalAlign = HorizontalAlign.Center;
            table.Width = new Unit("70%");
            table.CssClass = "table_background";

            #region // шапка таблицы


            TableHeaderRow hRow = new TableHeaderRow();
            TableHeaderCell hCell;

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Позиция" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Название" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Дата релиза" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Рейтинг" });
            hRow.Cells.Add(hCell);

            hCell = new TableHeaderCell();
            hCell.Controls.Add(new Label { Text = "Количество голосов" });
            hRow.Cells.Add(hCell);

            table.Rows.Add(hRow);
            #endregion

            #region     //тело таблицы
            

            TableRow row;
            TableCell cell;
            foreach (var item in repository)
            {
                row = new TableRow();
                row.CssClass = "tableCell-1 ";

                cell = new TableCell();
                cell.Controls.Add(new Label { Text = item.Number.ToString() });
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Controls.Add(new Label { Text = item.Name });
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Controls.Add(new Label { Text = item.Data.ToString() });
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Controls.Add(new Label { Text = item.Rating.ToString() });
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Controls.Add(new Label { Text = item.Vote.ToString() });
                row.Cells.Add(cell);

                table.Rows.Add(row);
                #endregion

            }
        }       

        public void CBElement()     // заполнение дат в DropDownList
        {
            CBFrom.Items.Clear();
            CBTo.Items.Clear();

            List<int> x = new List<int>();
            int z = 0;
            foreach (var item in repository)
                x.Add(item.Data);
            x.Sort();
            foreach (var item in x)
                if (z == 0)
                {
                    z = item;
                    CBFrom.Items.Add(item.ToString());
                    CBTo.Items.Add(item.ToString());
                }
                else
                    if (z != item)
                {
                    z = item;
                    CBFrom.Items.Add(item.ToString());
                    CBTo.Items.Add(item.ToString());
                }
        }

        protected void BFind_Click(object sender, EventArgs e)
        {
            if (int.Parse(CBFrom.SelectedItem.Text) > int.Parse(CBTo.SelectedItem.Text))
                LExp.Text = "неверно заданы параметры поиска!";
            else
            {
                using (KnContext contex = new KnContext())
                {
                    List<KnFilm> kns = new List<KnFilm>();
                    foreach (var item in contex.Films.ToList())
                        if (item.Data >= int.Parse(CBFrom.SelectedItem.Text) && item.Data <= int.Parse(CBTo.SelectedItem.Text))
                            kns.Add(item);

                    var a = kns.ToArray();
                    for (int i = 0; i <a.Length-1; i++)                    
                        for (int j = 0; j < a.Length-i-1; j++)                        
                            if (a[j].Rating<a[j+1].Rating)
                            {
                                KnFilm x = a[j + 1];
                                a[j + 1] = a[j];
                                a[j] = x;
                            }
                    for (int i = 0; i < 10 && i < a.Length; i++)                    
                        repository.Add(a[i]);
                    LExp.Text = "";
                    PrintTable();         
                }
            }
        }
    }
}