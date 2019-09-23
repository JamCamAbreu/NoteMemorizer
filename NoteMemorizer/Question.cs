using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace NoteMemorizer
{

  /*
  <thing> one <areallylongwordthissucks>
  <thing> <two> two
  here's a sentence
  */


    public class Question
    {
        public Random r = new Random();
        public string answer { get; set; }
        public string processedQuestion { get; set; }
        const char REPLACE_CHAR = '_';


        public char[] splitSymbols = { ' ', '.', '{', '}', '(', ')', '[', ']', '"', '/', '<', '>', '-' };

        public int QuestionNumber { get; set; }

        public bool IsReviewQuestion;
        public void SetAsReviewQuestion() {
            IsReviewQuestion = true;
        }

        public Question(string question, string _answer, TestTaker.testType tt)
        {
            answer = _answer;
            switch (tt) {
                case TestTaker.testType.kewordsFull:
                case TestTaker.testType.keywordsPartial:
                case TestTaker.testType.keywordsFirstLetters:
                    processedQuestion = $"{question}\n\n{ParseKeyword(_answer, tt)}";
                    break;

                case TestTaker.testType.fullRandom:
                default:
                    processedQuestion = $"{question}\n\n{ParseFullRandom(_answer)}";
                    break;
            }
            IsReviewQuestion = false;
        }


        public void WriteStreamColor(string phrase, ConsoleColor color)
        {
            char[] splitSymbols = {' '};
            string[] words = phrase.Split(splitSymbols);

            int charSpeed = 80;

            foreach (var word in words)
            {
                if (word.Contains(TestTaker.KEYWORD_SYMBOL))
                {
                    string removeSymbol = word.Replace(TestTaker.KEYWORD_SYMBOL, "");
                    Console.ForegroundColor = color;

                    foreach (char c in removeSymbol)
                    {
                        System.Threading.Thread.Sleep(charSpeed);
                        if (Exam.SkipStream()) charSpeed = 1;
                        Console.Write(c);
                    }

                    System.Threading.Thread.Sleep(charSpeed * 2);
                    if (Exam.SkipStream()) charSpeed = 1;
                    Console.ResetColor();
                }
                else
                {
                    foreach (char c in word)
                    {
                        System.Threading.Thread.Sleep(charSpeed/4);
                        if (Exam.SkipStream()) charSpeed = 1;
                        Console.Write(c);
                    }
                }

                Console.Write(" ");
                if (Exam.SkipStream()) charSpeed = 1;
                System.Threading.Thread.Sleep(charSpeed/3);
            }
        }


        public string ParseFullRandom(string input) {
            if (input.Length < 10) { return input; }
            else
            {
                string[] words = input.Split(this.splitSymbols);
                if (words.Length < 2) { return input; }
                StringBuilder output = new StringBuilder(input);
                int amount = (int)(words.Length * 0.75);
                const int MAX_TRIES = 100;
                int tries = 0;
                int hidden = 0;
                int index;
                while (tries < MAX_TRIES && hidden < amount)
                {
                    index = r.Next(words.Length);
                    string curWord = words[index];
                    if (!curWord.Contains(REPLACE_CHAR) && curWord.Length > 1)
                    {
                        output.Replace(curWord, partialReplace(curWord));
                        hidden++;
                    }
                    tries++;
                }
                return output.ToString();
            } // end else
        }
        public string ParseKeyword(string input, TestTaker.testType tt) {
            string[] words = input.Split(this.splitSymbols);
            if (words.Length < 2) { return input; }
            StringBuilder output = new StringBuilder(input);
            foreach (string curWord in words)
            {
                if (!curWord.Contains(REPLACE_CHAR) && curWord.Length > 1 && curWord.Contains(TestTaker.KEYWORD_SYMBOL))
                {
                    if (tt == TestTaker.testType.kewordsFull)
                        output.Replace(curWord, fullReplace(curWord));
                    else if (tt == TestTaker.testType.keywordsPartial)
                        output.Replace(curWord, partialReplace(curWord));
                    else if (tt == TestTaker.testType.keywordsFirstLetters)
                        output.Replace(curWord, firstFewLettersOnly(curWord));
                    else
                        output.Replace(curWord, "ERROR 101");
                }
            }
            return output.ToString();
        }


        public string partialReplace(string input) {
            if (input.Length <= 3) return input;
            StringBuilder output = new StringBuilder(input);
            int amount = (int)(input.Length * 0.65);
            int tries = 0;
            int hidden = 0;
            int maxTries = input.Length * 2;
            int index;
            while (tries < maxTries && hidden < amount) {
                index = r.Next(1, input.Length);
                char curChar = input[index];
                if (curChar != REPLACE_CHAR && curChar != '\n') {
                    output[index] = REPLACE_CHAR;
                    hidden++;
                }
                tries++;
            }
            return output.ToString();
        }

        public string fullReplace(string input) {
            if (input.Length <= 3) return input;
            StringBuilder output = new StringBuilder(input);
            for (int i = 1; i < input.Length; i++) {
                if (output[i] != '\n') // don't replace newline chars
                    output[i] = REPLACE_CHAR;
            }
            return output.ToString();
        }

        public string firstFewLettersOnly(string input) {
            int startPos = (input.Length < 3) ? 0 : (int)(input.Length / 3);
            StringBuilder output = new StringBuilder(input);
            for (int i = startPos; i < input.Length; i++) {
                if (output[i] != '\n') // don't replace newline chars
                    output[i] = REPLACE_CHAR;
            }
            return output.ToString();
        }


    } // end question class





}
