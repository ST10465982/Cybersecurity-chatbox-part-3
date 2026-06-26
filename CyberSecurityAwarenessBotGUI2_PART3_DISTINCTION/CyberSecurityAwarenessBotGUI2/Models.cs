using System;

namespace CyberSecurityAwarenessBotGUI2
{
    public class CyberTask
    {
        public int TaskID { get; set; }
        public string TaskDescription { get; set; }
        public DateTime ReminderDate { get; set; }
        public string TaskStatus { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

    public class QuizQuestion
    {
        public string Topic { get; set; }
        public string QuestionText { get; set; }
        public string[] Options { get; set; }
        public int CorrectIndex { get; set; }
        public string CorrectFeedback { get; set; }
        public string IncorrectFeedback { get; set; }

        public QuizQuestion(string topic, string questionText, string[] options, int correctIndex, string correctFeedback, string incorrectFeedback)
        {
            Topic = topic;
            QuestionText = questionText;
            Options = options;
            CorrectIndex = correctIndex;
            CorrectFeedback = correctFeedback;
            IncorrectFeedback = incorrectFeedback;
        }
    }

    public class UserMemory
    {
        public string UserName { get; set; }
        public string FavouriteTopic { get; set; }
        public string LastIntent { get; set; }
        public string LastConcern { get; set; }
        public string LastBotResponse { get; set; }
    }

    public class BotReply
    {
        public string Message { get; set; }
        public string Intent { get; set; }
        public string Topic { get; set; }
    }
}
