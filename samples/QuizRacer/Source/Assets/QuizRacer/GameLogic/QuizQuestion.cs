/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace QuizRacer.GameLogic
{
    using UnityEngine;

    public class QuizQuestion
    {
        const int AnswerCount = 4;
        private string mQuestion;
        private string[] mAnswers = new string[AnswerCount];
        private int mRightAnswer = 0;
        private int mID;

        private static int QuestionID = 1;

        public QuizQuestion(string question, string rightAnswer, string[] wrongAnswers)
        {
            int i, j;

            mID = QuestionID++;

            mQuestion = question;

            // copy the wrong answers to our vector, using positions [1..AnswerCount-1]
            System.Array.Copy(wrongAnswers, 0, mAnswers, 1, AnswerCount - 1);

            // right answers goes in mAnswers[0]
            mAnswers[0] = rightAnswer;

            // shuffle the wrong answers
            for (i = 1; i < AnswerCount - 1; i++)
            {
                j = Random.Range(1, AnswerCount - 1);
                Swap(i, j);
            }

            // decide position of right answer
            mRightAnswer = Random.Range(0, AnswerCount);

            // place it there
            Swap(0, mRightAnswer);
        }

        private void Swap(int i, int j)
        {
            string temp = mAnswers[i];
            mAnswers[i] = mAnswers[j];
            mAnswers[j] = temp;
        }

        public static QuizQuestion GenerateQuestion()
        {
            int operation = Random.Range(0, 3);
            int a, b, c, result;
            string q = "";
            switch (operation)
            {
                case 0:
                // a + b x c
                    a = Random.Range(1, 10);
                    b = Random.Range(1, 10);
                    c = Random.Range(2, 5);
                    result = a + b * c;
                    q = a + " + (" + b + " x " + c + ")";
                    break;
                case 1:
                // a x b - c
                    a = Random.Range(2, 10);
                    b = Random.Range(2, 10);
                    c = Random.Range(0, a * b);
                    q = "(" + a + " x " + b + ") - " + c;
                    result = a * b - c;
                    break;
                case 2:
                // a/b + c
                    b = Random.Range(2, 8);
                    a = b * Random.Range(2, 8);
                    c = Random.Range(1, 5);
                    q = a + "/" + b + " + " + c;
                    result = a / b + c;
                    break;
                default:
                // a + b + c
                    a = Random.Range(2, 10);
                    b = Random.Range(2, 10);
                    c = Random.Range(2, 10);
                    result = a + b + c;
                    q = a + " + " + b + " + " + c;
                    break;
            }
            return new QuizQuestion(q + "?", result.ToString(),
                MakeWrongNumericAnswers(result));
        }

        private static string[] MakeWrongNumericAnswers(int rightNumber)
        {
            int wrongNumberCount = AnswerCount - 1;
            int numbersBelow = Random.Range(0, wrongNumberCount);
            int i;
            string[] wrongNumbers = new string[AnswerCount - 1];

            int n = rightNumber;
            for (i = 0; i < numbersBelow; i++)
            {
                n -= Random.Range(1, 4);
                wrongNumbers[i] = n.ToString();
            }
            n = rightNumber;
            for (; i < wrongNumberCount; i++)
            {
                n += Random.Range(1, 4);
                wrongNumbers[i] = n.ToString();
            }

            return wrongNumbers;
        }

        public string Question
        {
            get
            {
                return mQuestion;
            }
        }

        public string[] Answers
        {
            get
            {
                return mAnswers;
            }
        }

        public int RightAnswerIndex
        {
            get
            {
                return mRightAnswer;
            }
        }

        public string RightAnswer
        {
            get
            {
                return mAnswers[mRightAnswer];
            }
        }

        public int ID
        {
            get
            {
                return mID;
            }
        }
    }
}