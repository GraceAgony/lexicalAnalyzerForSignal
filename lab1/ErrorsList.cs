using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace lab1
{
    class ErrorsList : IEnumerable
    {
        public class Error
        {
            public int Line { get; set; }
            public int Column { get; set; }
            public string Text { get; set; }
            public string Symbol { get; set; }
        }

        public List<Error> Errors = new List<Error>();

        public void Add(int line, int column, string text, string symbol)
        {
            Errors.Add(new Error {Line = line, Column = column, Text = text, Symbol = symbol});
        }

        public void Output(string file)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = File.AppendText(file))
            {
                sw.Write(json);
            }
        }


        public IEnumerator GetEnumerator()
        {
            return Errors.GetEnumerator();
        }
    }
}
