using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoteMemorizer
{



    public class TestTaker
    {

        public enum testType { keywordsPartial = 1, kewordsFull = 2, fullRandom = 4, keywordsFirstLetters = 3 }

        public const string QUESTION_SYMBOL = "#";
        public const string ANSWER_SYMBOL = "*";
        public static string TOPIC_SYMBOL = "~";
        public static string KEYWORD_SYMBOL = "^";


        public bool HasPreviousQuestions()
        {
            return exam.HasPreviousQuestions();
        }

        public bool HasForwardQuestions()
        {
            return exam.HasForwardQuestions();
        }

        public void SkipAllForwardQuestions()
        {
            while (exam.HasForwardQuestions())
            {
                exam.Forward();
            }
        }

        public bool Back()
        {
            if (exam.HasPreviousQuestions())
            {
                exam.Back();
                return true;
            }
            else
                return false;
        }

        public bool Forward()
        {
            if (exam.HasForwardQuestions())
            {
                exam.Forward();
                return true;
            }
            else
                return false;
        }

        public Exam exam = new Exam();

        private int numQuestionsDesired { get; set; }
        public int GetNumQuestionsSession() {
            return numQuestionsDesired;
        }
        public void SetNumQuestionsSession(int num) {
            numQuestionsDesired = num;
            if (exam != null) {
                exam.NumberQuestionsThisSession = num;
            }
        }

        public bool isSymbol(string word)
        {
            if (isAnswer(word))
                return true;
            else if (isTopic(word))
                return true;
            else if (isQuestion(word))
                return true;
            else
                return false;
        }

        public bool isQuestion(string word) { return (word == QUESTION_SYMBOL); }
        public bool isAnswer(string word) { return (word == ANSWER_SYMBOL); }
        public bool isTopic(string word) { return (word == TOPIC_SYMBOL); }

        public void setExamType(testType _type) {
            if (exam != null) {
                exam.examType = _type;
            }
        }

        public bool parseFile(string fileName) {

            try
            {
                string[] lines = System.IO.File.ReadAllLines($@"noteFiles\{fileName}");
                lines = lines.Select(l => l.Replace('\t', ' ')).ToArray();
                string curSection = "~ Unsorted / Miscellaneous"; // just in case user forgets first section
                string curQuestion = "";
                string curAnswer = "";
                foreach (string line in lines)
                {
                    if (!String.IsNullOrWhiteSpace(line)) {

                        string[] words = line.Split(' ');
                        string firstWordLine = "";

                        foreach (string word in words) {
                            if (!String.IsNullOrWhiteSpace(word))
                            {
                                firstWordLine = word;
                                break;
                            }
                        }

                        // TOPIC:
                        if (isTopic(firstWordLine)) {
                            curSection = line; // the whole line
                            exam.addSection(line); // only adds if didn't exist

                            // wrap up previous question:
                            if (curQuestion.Length > 0 && curAnswer.Length > 0) {
                                exam.addQuestion(curSection, curQuestion, curAnswer);
                                curAnswer = "";
                                curQuestion = "";
                            }
                        } // end topic

                        // QUESTION
                        else if (isQuestion(firstWordLine)) {
                            if (curQuestion.Length > 0) {
                                if (curAnswer.Length > 0) {
                                    exam.addQuestion(curSection, curQuestion, curAnswer);
                                    curAnswer = "";
                                    curQuestion = $"{line}\n";
                                }
                                else {
                                    // no answer to previous question,
                                    // ignore previous question
                                    curQuestion = $"{line}\n";
                                }
                            }
                            else {
                                curQuestion = $"{line}\n";
                            }
                        }

                        // ANSWER:
                        else if (isAnswer(firstWordLine)) {
                            // We don't want more than one answer per question,
                            // However, extra lines can be added to answers
                            if (curQuestion.Length > 0) {
                                if (curAnswer.Length == 0) {
                                    curAnswer = $"{line}\n";
                                }
                                else {
                                    curAnswer += $"\n{line}\n";
                                }
                            }
                        } // end answer

                        // OTHER
                        else {

                            // first try to add extra lines to answer
                            if (curAnswer.Length > 0) {
                                curAnswer += $"{line}\n";
                            }

                            // if not, try to add it to the question
                            else if (curQuestion.Length > 0) {
                                curQuestion += $"{line}\n";
                            }
                        }

                    } //end if line is not blank
                } // end foreach

                // Finalize (any left over questions?)
                if (curAnswer.Length > 0) {
                    exam.addQuestion(curSection, curQuestion, curAnswer);
                }

            } // end try
            catch
            {
                return false;
            }

            var sections = exam.sections.Count;
            var totalQuestions = exam.totalQuestions;

            return true;
        }



    }



}


