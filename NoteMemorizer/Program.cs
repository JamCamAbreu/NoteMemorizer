using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{

    class Program
    {

        public enum keyCommand
        {

            FORWARD_ONE = 0,
            BACKWARD_ONE,
            NEXT,
            ESCAPE,
            RESTART_QUESTION
        }

        static void Main(string[] args)
        {

            bool playAgain;
            int numQuestions;
            do
            {
                printTitle();
                printWelcome();

                TestTaker t;
                bool foundFile;
                do
                {
                    string fileName = askFileName();
                    t = loadNotes($"{fileName}", out foundFile);
                    if (!foundFile)
                    {
                        Console.WriteLine("\nCould not find file specified.");
                        Console.WriteLine("Please choose a file listed or copy desired text file to 'noteFiles' directory");
                        Console.WriteLine("Press enter to continue...");
                        Console.ReadLine();

                        Console.Clear();
                        printTitle();
                        printWelcome();
                    }
                } while (!foundFile);

                numQuestions = askHowManyQuestions(t);

                printInstructions();

                keyCommand command;
                bool SolvedEnough = false;
                bool newQuestionExists = t.exam.GetNewQuestion(); // first question
                bool stillHasReviewQuestions = false;
                do
                {
                    do
                    {
                        // Ask the question
                        command = askQuestion(t);

                        // Pressed Left Arrow:
                        if (command == keyCommand.BACKWARD_ONE)
                            t.Back();

                        // Pressed Right Arrow:
                        else if (command == keyCommand.FORWARD_ONE)
                            t.Forward();

                        // Pressed Enter, but is reviewing question in past
                        else if (command == keyCommand.NEXT && t.HasForwardQuestions())
                        {
                            t.Forward();
                            command = keyCommand.FORWARD_ONE;
                        }

                        // Pressed Left or Up arrow while looking at answer (restarts question)
                        else if (command == keyCommand.RESTART_QUESTION) {  /* do nothing */ }


                    } while (command != keyCommand.ESCAPE && command != keyCommand.NEXT);

                    // End prematurely
                    if (command == keyCommand.ESCAPE)
                        break;


                    SolvedEnough = (t.exam.asked >= numQuestions);
                    newQuestionExists = t.exam.GetNewQuestion();
                    stillHasReviewQuestions = t.exam.NumberQuestionsForReview() > 0;
                } while (!SolvedEnough || newQuestionExists || stillHasReviewQuestions);



                playAgain = testComplete(t);

            } while (playAgain);
        }


        public static TestTaker loadNotes(string fileName, out bool foundFile)
        {
            StringBuilder file = new StringBuilder(fileName);
            TestTaker t = new TestTaker();
            askType(t);
            t.exam.StreamString(new string[] { "Loading file" }, ConsoleColor.Gray);
            if (!fileName.Contains('.'))
            {
                file.Append(".txt");
            }
            bool success = t.parseFile($"{file}");
            if (!success)
            {
                foundFile = false;
                WriteColor("failure.\n", ConsoleColor.Red);
                System.Threading.Thread.Sleep(300);
            }
            else
            {
                foundFile = true;
                WriteColor("success!\n", ConsoleColor.Green);
                System.Threading.Thread.Sleep(300);
            }
            return t;
        }

        public static void printTitle()
        {
            ConsoleColor main = ConsoleColor.Yellow;
            ConsoleColor secondary = ConsoleColor.DarkYellow;

            WriteColorAlternating("+------------------------------------------------------------------", main, secondary);
            WriteColor("+\n", main);
            WriteColor("|  ", secondary);
            WriteColor("Note Memorizer", main);
            WriteColor(" | ", secondary);
            WriteColor("An Educational Project by James Cameron Abreu", main);
            WriteColor("  |\n", secondary);
            WriteColorAlternating("+------------------------------------------------------------------", main, secondary);
            WriteColor("+\n", main);
            Console.WriteLine();
        }

        public static void printWelcome()
        {
            WriteColor("Welcome to Note Memorizer!\n", ConsoleColor.White);
            WriteColor("Let's begin with a few questions about your test.\n", ConsoleColor.White);
            Console.WriteLine();
        }


        public static void printInstructions()
        {
            Console.WriteLine();
            WriteColor("Your test will now begin!\n", ConsoleColor.Cyan);
            Console.WriteLine("[Press enter to continue]");
            Console.ReadLine();
        }

        public static keyCommand askQuestion(TestTaker t)
        {

            // TITLE
            Console.Clear();
            printTitle();

            // REVIEW QUESTION INDICATOR
            if (t.exam.currentQuestion.IsReviewQuestion)
            {
                WriteColor("                 *---------------------*\n", ConsoleColor.Magenta);
                WriteColor("                 | ~[REVIEW QUESTION]~ |\n", ConsoleColor.Magenta);
                WriteColor("                 *---------------------*\n", ConsoleColor.Magenta);
                Console.WriteLine();
            }

            // HEADER
            string trimmedSectionName = (t.exam.currentSection.topic).Replace(TestTaker.TOPIC_SYMBOL, "");
            int sectionNum = t.exam.currentSection.howManyTotal() - t.exam.currentSection.howManyLeft();

            int maxSize = 68 - 2;

            int fullStringSize = $"SECTION:{trimmedSectionName} [{sectionNum} out of {t.exam.currentSection.howManyTotal()}]".Count();
            string dashes = "";
            for (int i = 0; i < maxSize - fullStringSize - 1; i += 2)
                dashes += "-";

            bool addDash = (fullStringSize + 2 + dashes.Count() * 2) < 68 ? true : false;

            WriteColor(dashes + " ", ConsoleColor.White);
            WriteColor($"SECTION:{trimmedSectionName}", ConsoleColor.White);
            WriteColor($" [", ConsoleColor.White);
            WriteColor(sectionNum.ToString(), ConsoleColor.Cyan);
            WriteColor($" out of ", ConsoleColor.White);
            WriteColor($"{t.exam.currentSection.howManyTotal()}", ConsoleColor.Cyan);
            WriteColor($"]", ConsoleColor.White);
            WriteColor(" " + dashes, ConsoleColor.White);
            if (addDash)
                WriteColor("-", ConsoleColor.White);
            Console.WriteLine();

            // Question
            int questionNum = t.exam.currentQuestion.QuestionNumber;
            string reviewIndicator = t.exam.currentQuestion.IsReviewQuestion ? " (review) " : " ";
            if (t.HasPreviousQuestions())
            {
                Console.Write("<<--- ["); WriteColor("previous", ConsoleColor.DarkCyan); Console.Write("]    ");
            }
            else
                Console.Write("                      ");

            Console.Write($"Question ");
            WriteColor($"{questionNum}", ConsoleColor.Cyan);
            WriteColor($"{reviewIndicator}", ConsoleColor.Magenta);
            Console.Write($"out of ");
            WriteColor($"{t.GetNumQuestionsSession()}", ConsoleColor.Cyan);
            Console.Write($": ");

            if (t.HasForwardQuestions())
            {
                Console.Write("    ["); WriteColor("forward", ConsoleColor.DarkCyan); Console.Write("] --->>\n");
            }
            else
                Console.Write("\n");

            // Questions for Review:
            Console.Write("                    ");
            Console.Write($"Review Questions: [");
            if (t.exam.NumberQuestionsForReview() > 0)
                WriteColor(t.exam.NumberQuestionsForReview().ToString(), ConsoleColor.Magenta);
            else
                Console.Write(t.exam.NumberQuestionsForReview().ToString());
            Console.Write("]\n");

            // QUESTION
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            t.exam.currentQuestion.WriteSymbolColor(t.exam.currentQuestion.processedQuestion, ConsoleColor.Yellow);
            Console.WriteLine();
            Console.WriteLine("[Press enter to continue]");

            // REVEAL ANSWER?
            Console.WriteLine();
            var key = Console.ReadKey(false).Key;

            if (key == ConsoleKey.Escape)
                return keyCommand.ESCAPE;


            if (key != ConsoleKey.LeftArrow && key != ConsoleKey.RightArrow)
            {
                Console.WriteLine();
                Console.WriteLine("[ANSWER]:");
                Console.WriteLine();
                t.exam.currentQuestion.WriteSymbolColor(t.exam.currentQuestion.answer, ConsoleColor.Green);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press [Backspace] to review later");
                Console.WriteLine("Press [Enter] to complete this question");
                Console.WriteLine();
                key = Console.ReadKey(false).Key;
            }

            // Add to review questions:
            if (key == ConsoleKey.Backspace)
            {
                t.exam.AddReviewQuestion(t.exam.currentQuestion);

                List<string> message = new List<string>();
                message.Add($"Adding Question for Review");
                t.exam.StreamString(message.ToArray(), ConsoleColor.Magenta);

            }
            if (key == ConsoleKey.Enter && !t.HasForwardQuestions())
            {
                t.exam.CompleteQuestion(t.exam.currentQuestion);
            }

            // Restart question:
            if (key == ConsoleKey.UpArrow)
                return (keyCommand.RESTART_QUESTION);

            // KEYBOARD LOGIC
            if (key == ConsoleKey.LeftArrow)
                return keyCommand.BACKWARD_ONE;

            else if (key == ConsoleKey.RightArrow)
                return keyCommand.FORWARD_ONE;

            else if (key == ConsoleKey.Escape)
                return keyCommand.ESCAPE;

            else
                return keyCommand.NEXT;
        }

        public static bool testComplete(TestTaker t)
        {
            Console.Clear();
            printTitle();
            Console.WriteLine("Thanks for playing!");
            string answer;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Would you like to play again? (yes/no)");
                Console.Write("Your response: ");
                answer = Console.ReadLine();
            } while (!answer.ToLower().Contains('y') && !answer.ToLower().Contains('n'));

            Console.Clear();
            if (answer.ToLower().Contains('y')) return true;
            else return false;
        }


        public static List<string> getFileNames()
        {
            List<string> fileNames = Directory
                .GetFiles(@"noteFiles\", "*.txt", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .ToList();
            return fileNames;
        }

        public static void listFileNames(List<string> fileNames, int maxListings)
        {
            WriteColor("Files found:\n", ConsoleColor.White);
            int it = 1;
            foreach (var name in fileNames)
            {
                Console.WriteLine($"\t{name}");
                it++;
                if (it > maxListings) break;
            }
            if (it > maxListings)
            {
                Console.WriteLine("...");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public static string askFileName()
        {
            WriteColor("Please enter the name of the file you would like to use for input\n", ConsoleColor.Yellow);
            List<string> fileNames = getFileNames();
            listFileNames(fileNames, 8);
            string userInput = null;
            do
            {
                WriteColor("File: ", ConsoleColor.Yellow);
                userInput = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(userInput));
            return userInput;
        }

        public static int askHowManyQuestions(TestTaker t)
        {
            Console.WriteLine();
            Console.Write($"These notes contain ");
            WriteColor($"{t.exam.totalQuestions}", ConsoleColor.Cyan);
            Console.WriteLine($" questions.");
            WriteColor($"How many would you like to practice?\n", ConsoleColor.Yellow);

            ConsoleColor highlight = ConsoleColor.Magenta;

            Console.Write($"\t- Press [");
            WriteColor($"enter", highlight);
            Console.Write($"] for all\n");

            Console.Write($"\t- Otherwise [");
            WriteColor($"enter a number", highlight);
            Console.Write($"] and press [");
            WriteColor($"enter", highlight);
            Console.Write($"]\n");
            Console.WriteLine();
            int num = t.exam.totalQuestions; // default
            string userInput = null;
            bool parseSuccess = true;
            do
            {
                WriteColor("Your choice: ", ConsoleColor.Yellow);
                userInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(userInput))
                    parseSuccess = int.TryParse(userInput, out num);
            } while (!parseSuccess || (num > t.exam.totalQuestions) || (num < 1));

            if (num <= 0 || string.IsNullOrWhiteSpace(userInput))
                num = t.exam.totalQuestions;

            t.SetNumQuestionsSession(num);

            return num;
        }

        public static void askType(TestTaker t)
        {
            bool goodKey;
            string key;
            ConsoleColor titleColor = ConsoleColor.Cyan;
            ConsoleColor recommendedColor = ConsoleColor.Green;
            Console.WriteLine();
            WriteColor("Which type of test will you take?\n", ConsoleColor.Yellow);
            Console.Write("\t["); WriteColor("1", titleColor); Console.Write("] ");
            WriteColor("Keywords Partial", titleColor);
            Console.Write(" (keywords will be partially hidden)\n");

            Console.Write("\t["); WriteColor("2", titleColor); Console.Write("] ");
            WriteColor("Keywords Full Blank", titleColor);
            Console.Write(" (keywords will be fully hidden)\n");

            Console.Write("\t["); WriteColor("3", recommendedColor); Console.Write("] ");
            WriteColor("Keywords First Letters", recommendedColor);
            Console.Write(" (only first few letters of keyword revealed)");
            WriteColor(" -- (Recommended)\n", recommendedColor);

            Console.Write("\t["); WriteColor("4", titleColor); Console.Write("] ");
            WriteColor("Full Random", titleColor);
            Console.Write(" (all words in answer will be randomly hidden)\n");

            do
            {
                WriteColor("Your choice: ", ConsoleColor.Yellow);
                key = Console.ReadLine();
                goodKey = (key == "1" || key == "2" || key == "3" || key == "4");
                if (!goodKey)
                {
                    Console.WriteLine();
                    WriteColor("Please provide your choice by entering 1, 2, 3, or 4 on your keyboard\n", ConsoleColor.Red);
                }
            } while (!goodKey);
            int keyNum = int.Parse(key);
            t.setExamType((TestTaker.testType)keyNum);
            return;
        }


        public static void WriteColor(string phrase, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(phrase);
            Console.ResetColor();
        }

        public static void WriteColorAlternating(string phrase, ConsoleColor color1, ConsoleColor color2)
        {
            bool mainColor = true;
            Console.ForegroundColor = color1;
            foreach (var c in phrase)
            {
                Console.Write(c);

                if (mainColor)
                {
                    Console.ForegroundColor = color2;
                    mainColor = false;
                }
                else {
                    Console.ForegroundColor = color1;
                    mainColor = true;
                }
            }
            Console.ResetColor();
        }


    } // end main class


}


