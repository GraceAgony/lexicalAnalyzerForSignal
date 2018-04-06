using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace lab1
{
    class Program
    {

        public class LexAnalyzer
        {
            public string ProgramFile { get; set; }

            private readonly Table _simpleDelimitersTable;
            private readonly Table _multiCharacterDelimitersTable;
            private readonly Table _keyWordsTable;
            private readonly Table _constantsTable;
            private readonly Table _identifiersTable;
            private readonly ErrorsList _errorsTable;

            public const int SimpleDelimitersOffset = 0;
            public const int MultiCharacterDelimitersOffset = 300;
            public const int KeyWordsOffset = 400;
            public const int ConstantsOffset = 500;
            public const int IdentifiersOffset = 1000;

            public  string ResultJson = @"\result.json";
            public string Result = @"\result.txt";
            public string CodesFile = @"\codesFile.csv";
            public  string Folder = @"D:\labs\ipz\lab1Files";

           
            private readonly char[] _ws = {' ', '\r', '\n' , '\t', '\v', '\f'};
            private readonly string[] _keyWords = { "PROCEDURE","BEGIN","END","INTEGER","FLOAT","CONST" };
            private readonly char[] _simpleDelim = {';',':','=','(',')'};
            private readonly string[] _multiDelim = { };

            private static char _currentChar;
            public static int CurrentLine { get; set; }
            public static int CurrentColumn { get; set; }

            private static StreamReader _sr;

            private CreateText createText;

            public LexAnalyzer()
            {
                using (var fs = File.Open(Folder+ResultJson, FileMode.Open))
                {
                    fs.SetLength(0);
                }
                using (var fs1 = File.Open(Folder + CodesFile, FileMode.Open))
                {
                    fs1.SetLength(0);
                }
                using (var fs2 = File.Open(Folder + Result, FileMode.Open))
                {
                    fs2.SetLength(0);
                }

                _simpleDelimitersTable = new Table(Folder+ResultJson,SimpleDelimitersOffset, "SimpleDelimiters", Folder + CodesFile);
                _multiCharacterDelimitersTable = new Table(Folder+ResultJson, MultiCharacterDelimitersOffset, "MultiCharacterDelimiters", Folder + CodesFile);
                _keyWordsTable = new Table(Folder+ResultJson,KeyWordsOffset, "KeyWords", Folder + CodesFile);
                _constantsTable = new Table(Folder+ResultJson, ConstantsOffset, "Constants", Folder + CodesFile);
                _identifiersTable = new Table(Folder+ResultJson,IdentifiersOffset, "Identifiers", Folder + CodesFile);
                _errorsTable = new ErrorsList();
                createText = new CreateText(Folder + Result, Folder+CodesFile, _simpleDelimitersTable, _multiCharacterDelimitersTable,
                                             _keyWordsTable, _constantsTable, _identifiersTable, _errorsTable);
            }

            public void AnalyzeFile(string file)
            {
                using (_sr = new StreamReader(file))
                {
                   _currentChar = (char)_sr.Read();
                    CurrentLine = 1;
                    CurrentColumn = 1;
                    while (_currentChar.GetHashCode() != -1)
                    {
                        if (!IsItIdentifierOrKeyWord())
                        {
                            if (!IsItConstant())
                            {
                                if (!IsItMultiCharacterDelimiter())
                                {
                                    if (!IsItComment())
                                    {
                                        if (!IsItWs())
                                        {
                                            if (!IsItSimpleDelimiter())
                                            {
                                               _errorsTable.Add(CurrentLine, CurrentColumn,
                                                   $"Lexer : Error (line {CurrentLine}, column {CurrentColumn}): " +
                                                   $"Illegal character '{_currentChar}' detected", _currentChar.ToString());
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public bool IsItSimpleDelimiter()
            {
                if (!Array.Exists(_simpleDelim, element => element == _currentChar))
                {
                    return false;
                }

                _simpleDelimitersTable.Add(_currentChar.ToString(),CurrentLine,CurrentColumn);
                _currentChar = (char)_sr.Read();
                CurrentColumn++;
                return true;
            }

            public bool IsItMultiCharacterDelimiter()
            {
                if (_sr.Peek() == -1)
                {
                    return false;
                }
                var token = _currentChar + _sr.Peek().ToString();
                if (!Array.Exists(_multiDelim, element => element == token.ToString()))
                {
                    return false;
                }
                _currentChar = (char)_sr.Read();
                CurrentColumn++;
                _currentChar = (char)_sr.Read();
                CurrentColumn++;
                _multiCharacterDelimitersTable.Add(token, CurrentLine, CurrentColumn - 1);
                return true;
            }

            public bool IsItConstant()
            {
                if ((_currentChar < '0' || _currentChar > '9') && _currentChar!= '-')
                {
                    return false;
                }
                var token = _currentChar.ToString();
                var next = _sr.Peek();
                var length = 1;
                while (next != -1 && next >= '0' && next <='9')
                {
                    _currentChar = (char) _sr.Read();
                    CurrentColumn++;
                    length++;
                    token += _currentChar;
                    next = _sr.Peek();
                }

                _currentChar = (char)_sr.Read();
                CurrentColumn++;
                length++;
                _constantsTable.Add(token,CurrentLine,CurrentColumn-length+1);
                return true;

            }

            public bool IsItIdentifierOrKeyWord()
            {
                if ((_currentChar < 'A' || _currentChar > 'Z') && _currentChar!='#')
                {
                    return false;
                }
                var token = _currentChar.ToString();
                var next = _sr.Peek();
                var length = 1;


                if (_currentChar == '#')
                {
                    if (next != '`')
                    {
                        return false;
                    }

                    _currentChar = (char) _sr.Read();
                    length++;
                    CurrentColumn++;
                    token += _currentChar;
                    next = _sr.Peek();
                    int numberCount = 0;
                    int letterCount = 0;

                    while (next != -1 && ((next >= 'A' && next <= 'Z') || (next >= '0' && next <= '9')))
                    {
                        if (next >= '0' && next <= '9')
                        {
                            numberCount++;
                        }

                        if (next >= 'A' && next <= 'Z')
                        {
                            if (numberCount < 2)
                            {
                                return false;
                            }

                            letterCount++;
                        }

                        _currentChar = (char)_sr.Read();
                        length++;
                        CurrentColumn++;
                        token += _currentChar;
                        next = _sr.Peek();
                    }

                    if (letterCount < 1)
                    {
                        return false;
                    }

                    _currentChar = (char)_sr.Read();
                    length++;
                    CurrentColumn++;
                    if (Array.Exists(_keyWords, element => element == token))
                    {
                        _keyWordsTable.Add(token, CurrentLine, CurrentColumn - length + 1);
                    }
                    else
                    {
                        _identifiersTable.Add(token, CurrentLine, CurrentColumn - length + 1);
                    }
                    return true;

                }



                while (next != -1 && ((next>='A'&& next<='Z')||(next>='0' && next<='9')))
                {
                    _currentChar = (char) _sr.Read();
                    length++;
                    CurrentColumn++;
                    token += _currentChar;
                    next = _sr.Peek();
                }

                _currentChar = (char) _sr.Read();
                length++;
                CurrentColumn++;
                if (Array.Exists(_keyWords, element => element == token))
                {
                    _keyWordsTable.Add(token,CurrentLine,CurrentColumn-length+1);
                }
                else
                {
                    _identifiersTable.Add(token,CurrentLine,CurrentColumn-length+1);
                }
                return true;
            }

            public bool IsItWs()
            {
                if (!Array.Exists(_ws, element => element == _currentChar))
                {
                    return false;
                }
                _currentChar = (char) _sr.Read();
                if (_currentChar != '\n')
                {
                    CurrentColumn++;
                }
                else
                {
                    CurrentLine++;
                    CurrentColumn = 0;
                }

                return true;

            }

            public bool IsItComment()
            {
                if (_sr.Peek() == -1)
                {
                    return false;
                }          
                if (!((_currentChar=='(')&&((char)_sr.Peek() == '*')))
                {
                    return false;
                }
                _currentChar = (char)_sr.Read();
                CurrentColumn++;
                 while (_sr.Peek()!=-1)
                {
                    if ((_currentChar=='*')&&((char)_sr.Peek()==')'))
                    {
                        _currentChar = (char)_sr.Read();
                        CurrentColumn++;
                        _currentChar = (char)_sr.Read();
                        CurrentColumn++;
                        return true;
                    }
                    _currentChar = (char)_sr.Read();
                    if (_currentChar != '\n')
                    {
                        CurrentColumn++;
                    }
                    else
                    {
                        CurrentLine++;
                        CurrentColumn = 0;
                    }
                    
                }
                _currentChar = (char)_sr.Read();
                _errorsTable.Add(CurrentLine, CurrentColumn,$"Parse: Error (line {CurrentLine}, column {CurrentColumn}): " +
                                                            $"unexpected end of file", "(*");
                return true;
            }

            public void WriteResult()
            {
                _simpleDelimitersTable.Output();           
                _multiCharacterDelimitersTable.Output();
                _keyWordsTable.Output();
                _constantsTable.Output();
                _identifiersTable.Output();
                _errorsTable.Output(Folder+ResultJson);
                createText.ReadFile();
            }

        }

        private static void Main()
        {
            var lexAn = new LexAnalyzer();
            lexAn.AnalyzeFile("D:/labs/ipz/lab1Files/test2.txt");
            lexAn.WriteResult();
           
        }
    }
}
