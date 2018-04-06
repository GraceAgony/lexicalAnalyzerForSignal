using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace lab1
{
    class Table
    {
        public string Name;
        public Dictionary<string, Token> Tokens;
        private int _count;

        private string File { get; set; }
        private string CodesFile { get; set; }

        public class Location
        {
            public int Line { get; set; }
            public int Column { get; set; }
        }


        public class Token
        {
            public string Name { get; set; }
            public int Code { get; set; }
            public List<Location> Locations = new List<Location>();

            public void AddToList(int line, int column)
            {
                Locations.Add(new Location {Line = line, Column = column});
            }
        }
    

        public Table(string file, int displacement, string name, string codesFile)  
        {
            _count = displacement;
            Tokens = new Dictionary<string, Token>();
            Name = name;
            File = file;
            CodesFile = codesFile;
            if (!System.IO.File.Exists(file))
            {
                using (System.IO.File.CreateText(file))
                {
                }
            }

          
            
        }

        public Token Add(string token, int line, int column)
        {
            if (Tokens.ContainsKey(token))
            {
                Tokens[token].AddToList(line,column);
                WriteToCodesFile(Tokens[token].Code);
                return Tokens[token];
            }

            _count++;
            var locations = new List<Location>();
            var data = new Token {Name = token, Code = _count, Locations = locations};
            data.AddToList(line,column);
            Tokens.Add(token,data);
            WriteToCodesFile(_count);
            return Tokens[token];
        }

        public bool Exist(string token)
        {
            return Tokens.ContainsKey(token);
        }

        public void Output()
        {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                using (var sw = System.IO.File.AppendText(File))
                {              
                    sw.Write(json);
                }
               
        }


        private void WriteToCodesFile(int code)
        {
            using (TextWriter sw = new StreamWriter(CodesFile, true))
            {
                sw.Write($"{code}, ");
            }
        }
    }


   

}
