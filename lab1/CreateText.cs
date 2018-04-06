using System.Collections.Generic;
using System.IO;
using System.Linq;
using static lab1.Program.LexAnalyzer;
using static lab1.Table;

namespace lab1
{
    class CreateText
    {
        private string TextFile { get; set; }
        private string CodesFile { get; set; }

        private Dictionary<Token, int> Tokens;

        private readonly Table _simpleDelimitersTable;
        private readonly Table _multiCharacterDelimitersTable;
        private readonly Table _keyWordsTable;
        private readonly Table _constantsTable;
        private readonly Table _identifiersTable;
        private readonly ErrorsList _errorsTable;

        public CreateText(string textFile, string codesFile, Table simpleDelimitersTable, Table multiCharacterDelimitersTable,
            Table keyWordsTable, Table constantsTable, Table identifiersTable, ErrorsList errorsTable)
        {
            TextFile = textFile;
            CodesFile = codesFile;
            _simpleDelimitersTable = simpleDelimitersTable;
            _multiCharacterDelimitersTable = multiCharacterDelimitersTable;
            _keyWordsTable = keyWordsTable;
            _constantsTable = constantsTable;
            _identifiersTable = identifiersTable;
            _errorsTable = errorsTable;
            Tokens = new Dictionary<Token, int>();
        }

        public void ReadFile()
        {

            using (TextReader sr = new StreamReader(CodesFile))
            {
                while (sr.Peek().GetHashCode() != -1)
                {
                    string currentCode = "";

                    while ((char) sr.Peek() != ',')
                    {
                        currentCode += (char) sr.Read();
                    }

                    sr.Read();
                    sr.Read();
                    int code = int.Parse(currentCode);

                    if (code >= SimpleDelimitersOffset && code < MultiCharacterDelimitersOffset)
                    {
                        Search(_simpleDelimitersTable, code);
                    }
                    else if (code >= MultiCharacterDelimitersOffset && code < KeyWordsOffset)
                    {
                        Search(_multiCharacterDelimitersTable, code);
                    }
                    else if (code >= KeyWordsOffset && code < ConstantsOffset)
                    {
                        Search(_keyWordsTable, code);
                    }
                    else if (code >= ConstantsOffset && code < IdentifiersOffset)
                    {
                        Search(_constantsTable, code);
                    }
                    else if (code >= IdentifiersOffset)
                    {
                        Search(_identifiersTable, code);
                    }
                }
              WriteErrors();
            }
        }

        private void WriteErrors()
        {
            using (TextWriter sw = new StreamWriter(TextFile, true))
            {
                foreach (ErrorsList.Error err in _errorsTable)
                {
                    sw.WriteLine($"\n{err.Text}");
                }
                
            }
        }

        private  void Search(Table table, int code)
        {
            IEnumerable<Token> tokens = from t in table.Tokens
                where t.Value.Code == code
                select t.Value;

            var currentToken = tokens.FirstOrDefault();
            if (currentToken == null)
            {
                return;
            }

            if (!Tokens.ContainsKey(currentToken))
            {
                Tokens.Add(currentToken, 0);
            }  
            WriteToFile(currentToken, Tokens[currentToken]);
            Tokens[currentToken]++;
        }

        public void WriteToFile(Token token, int locationNumber)
        {
            using (TextWriter sw = new StreamWriter(TextFile, true))
            {
                sw.WriteLine($"{token.Locations[locationNumber].Line, 4}  {token.Locations[locationNumber].Column, 4}  {token.Code, 6}   {token.Name, 10}");
            }
        }
    }
}
