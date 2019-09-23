using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace NoteMemorizer
{

    public class Exam
    {
        public Dictionary<string, Section> sections = new Dictionary<string, Section>();
        public Random randomGenerator = new Random();
        public Question currentQuestion { get; set; }
        public Section currentSection { get; set; }
        public int totalQuestions { get; set; }

        Stack<Section> prevSections = new Stack<Section>();
        Stack<Section> forwardSections = new Stack<Section>();

        Stack<Question> prevQuestions = new Stack<Question>();
        Stack<Question> forwardQuestions = new Stack<Question>();

        ReviewBag reviewQuestions = new ReviewBag();
        int QuestionsCompleted { get; set; }
        double chanceIncreaser;

        public int NumberQuestionsThisSession { get; set; }

        public int NumberQuestionsForReview() {
            return reviewQuestions.Count();
        }

        private Section _getPreviousSection()
        {
            if (prevSections.Count > 0)
                return prevSections.Pop();
            else
                return null;
        }

        private Section _getForwardOrCurrentSection()
        {
            if (forwardSections.Count > 0)
                return forwardSections.Pop();
            else
                return null;
        }


        public void CompleteQuestion(Question q) {
            // Remove all instances of q in review bag
            reviewQuestions.Purge(q);

            // increment counter
            QuestionsCompleted++;
        }

        public void GrabReviewQuestion() {
            if (reviewQuestions.Count() > 0) {
                prevQuestions.Push(currentQuestion);
                currentQuestion = reviewQuestions.Grab();
            }
            else {
                Forward();
            }
        }

        public void AddReviewQuestion(Question q) {
            reviewQuestions.Add(q);
        }

        public bool HasForwardQuestions()
        {
            return forwardQuestions.Count > 0;
        }

        public bool HasPreviousQuestions()
        {
            return prevQuestions.Count > 0;
        }

        private Question _getPreviousQuestion()
        {
            if (prevQuestions.Count > 0)
                return prevQuestions.Pop();
            else
                return null;
        }

        private Question _getForwardOrCurrentQuestion()
        {
            if (forwardQuestions.Count > 0)
                return forwardQuestions.Pop();
            else
                return null;
        }

        public bool Back()
        {
            Question pq = _getPreviousQuestion();
            Section ps = _getPreviousSection();
            if (pq != null)
            {
                forwardQuestions.Push(currentQuestion);
                currentQuestion = pq;
                forwardSections.Push(currentSection);
                currentSection = ps;
                return true;
            }

            // We went all the way back
            else
                return false;
        }

        public bool Forward()
        {

            Question fq = _getForwardOrCurrentQuestion();
            Section fs = _getForwardOrCurrentSection();
            if (fq != null)
            {
                prevQuestions.Push(currentQuestion);
                currentQuestion = fq;
                prevSections.Push(currentSection);
                currentSection = fs;
                return true;
            }

            // No more questions
            else
                return false;
        }

        public int NumForwardQuestions()
        {
            return forwardQuestions.Count();
        }

        public int asked { get; set; }
        public TestTaker.testType examType { get; set; }

        // CONSTRUCTOR:
        public Exam()
        {
            asked = 0;
            totalQuestions = 0;
            QuestionsCompleted = 0;
            chanceIncreaser = 1.0;
            NumberQuestionsThisSession = 1;
        }

        public void addSection(string section)
        {
            if (!sections.ContainsKey(section))
            {
                Section s = new Section(section);
                sections.Add(s.topic, s);
            }
        }


        public void addQuestion(string section, string question, string answer)
        {
            if (!sections.ContainsKey(section)) { addSection(section); }
            Question q = new Question(question, answer, this.examType);
            sections[section].add(q);
            totalQuestions++;
        }

        public Question _getNewQuestion()
        {
            if (sections.Count <= 0) { return null; }
            // pick random section (that has questions still)
            Question cur = null;
            while (cur == null && sections.Count > 0) {
                var i = randomGenerator.Next(sections.Count);
                string t = sections.ElementAt(i).Key;
                // Remove section if it doesn't have any questions:
                if (!sections[t].hasQuestions()) { sections.Remove(t); }
                else {
                    cur = sections[t].next();
                    currentSection = sections[t];
                }
            }

            if (cur != null)
            {
                asked++;
                cur.QuestionNumber = asked;
            }

            return cur;
        }

        public bool GetNewQuestion()
        {
            // store previous question, if existed:
            if (currentQuestion != null)
            {
                prevQuestions.Push(currentQuestion); // add to previous questions
                prevSections.Push(currentSection); // add to previous section
            }



            // Check for review questions:
            Question q;
            double chancePerc = (100.00 * ((double)QuestionsCompleted / (double)NumberQuestionsThisSession)/2) + chanceIncreaser + reviewQuestions.Count()*3;
            int minimum = randomGenerator.Next(0, 100);

            if (reviewQuestions.Count() > 0)
            {
            }


            if ((asked >= NumberQuestionsThisSession) || (reviewQuestions.Count() > 0 && chancePerc > minimum)) {
                q = reviewQuestions.Grab();
                if (q == null)
                    return false;

                q.SetAsReviewQuestion();
                chanceIncreaser = 1.0; // reset

                List<string> message = new List<string>();
                message.Add($"Review Question incoming");
                StreamString(message.ToArray(), ConsoleColor.Magenta);
            }
            else {
               q = _getNewQuestion();

                // Increase chance of getting review question each time NOT shown
                if (reviewQuestions.Count() > 0) {
                    if (chanceIncreaser == 1.0)
                        chanceIncreaser = 5;
                    else {
                        chanceIncreaser *= 2.0;
                    }
                }
            }

            if (q == null) { return false; }
            currentQuestion = q;
            return true;
        }




        public void StreamString(string[] lines, ConsoleColor color)
        {
            const int CHARACTER_SPEED_MS = 10;
            const int LINE_SPEED_MS = 40;
            Console.ForegroundColor = color;

            foreach (var line in lines)
            {
                Console.Write("\n");
                foreach (char c in line)
                {
                    System.Threading.Thread.Sleep(CHARACTER_SPEED_MS);
                    Console.Write(c);
                }
                System.Threading.Thread.Sleep(LINE_SPEED_MS);
            }

            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(250);
                Console.Write(".");
            }
            System.Threading.Thread.Sleep(300);

            Console.ResetColor();
        }




    }
}
