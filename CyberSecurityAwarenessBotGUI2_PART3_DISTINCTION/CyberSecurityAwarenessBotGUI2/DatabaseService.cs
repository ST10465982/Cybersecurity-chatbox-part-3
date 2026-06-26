using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CyberSecurityAwarenessBotGUI2
{
    public class DatabaseService
    {
        private const string DatabaseName = "CyberSecurityBotDB";
        private const string MasterConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=5";
        private const string AppConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=CyberSecurityBotDB;Integrated Security=True;Connect Timeout=5";

        private readonly List<CyberTask> fallbackTasks = new List<CyberTask>();
        private readonly List<string> fallbackLog = new List<string>();
        private readonly UserMemory fallbackMemory = new UserMemory();
        private int nextTaskId = 1;

        public bool IsAvailable { get; private set; }
        public string StatusMessage { get; private set; }

        public void Initialise()
        {
            try
            {
                using (SqlConnection master = new SqlConnection(MasterConnectionString))
                {
                    master.Open();
                    using (SqlCommand cmd = new SqlCommand("IF DB_ID(@db) IS NULL EXEC('CREATE DATABASE " + DatabaseName + "')", master))
                    {
                        cmd.Parameters.AddWithValue("@db", DatabaseName);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection conn = new SqlConnection(AppConnectionString))
                {
                    conn.Open();
                    ExecuteNonQuery(conn, SchemaSql());
                }

                IsAvailable = true;
                StatusMessage = "Database connected: full CRUD is saving to LocalDB.";
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                StatusMessage = "Database fallback mode active. LocalDB message: " + ex.Message;
            }
        }

        private string SchemaSql()
        {
            return @"
IF OBJECT_ID('dbo.UserMemory','U') IS NULL
BEGIN
 CREATE TABLE dbo.UserMemory(
  MemoryID INT IDENTITY(1,1) PRIMARY KEY,
  MemoryKey VARCHAR(100) NOT NULL UNIQUE,
  MemoryValue VARCHAR(1000) NULL,
  UpdatedDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
IF OBJECT_ID('dbo.ChatMessages','U') IS NULL
BEGIN
 CREATE TABLE dbo.ChatMessages(
  MessageID INT IDENTITY(1,1) PRIMARY KEY,
  Sender VARCHAR(30) NOT NULL,
  MessageText VARCHAR(2000) NOT NULL,
  Intent VARCHAR(100) NULL,
  MessageDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
IF OBJECT_ID('dbo.ReminderTasks','U') IS NULL
BEGIN
 CREATE TABLE dbo.ReminderTasks(
  TaskID INT IDENTITY(1,1) PRIMARY KEY,
  TaskDescription VARCHAR(500) NOT NULL,
  ReminderDate DATE NOT NULL,
  TaskStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',
  CreatedDateTime DATETIME NOT NULL DEFAULT GETDATE(),
  UpdatedDateTime DATETIME NULL,
  CompletedDateTime DATETIME NULL
 );
END;
IF OBJECT_ID('dbo.QuizResults','U') IS NULL
BEGIN
 CREATE TABLE dbo.QuizResults(
  ResultID INT IDENTITY(1,1) PRIMARY KEY,
  Score INT NOT NULL,
  TotalQuestions INT NOT NULL,
  PercentageScore DECIMAL(5,2) NOT NULL,
  CompletedDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
IF OBJECT_ID('dbo.QuizAnswers','U') IS NULL
BEGIN
 CREATE TABLE dbo.QuizAnswers(
  AnswerID INT IDENTITY(1,1) PRIMARY KEY,
  Topic VARCHAR(100) NOT NULL,
  QuestionText VARCHAR(1000) NOT NULL,
  SelectedAnswer VARCHAR(500) NOT NULL,
  CorrectAnswer VARCHAR(500) NOT NULL,
  WasCorrect BIT NOT NULL,
  Feedback VARCHAR(1000) NOT NULL,
  AnsweredDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;
IF OBJECT_ID('dbo.ActivityLog','U') IS NULL
BEGIN
 CREATE TABLE dbo.ActivityLog(
  LogID INT IDENTITY(1,1) PRIMARY KEY,
  ActivityDescription VARCHAR(1000) NOT NULL,
  ActivityDateTime DATETIME NOT NULL DEFAULT GETDATE()
 );
END;";
        }

        private void ExecuteNonQuery(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn)) cmd.ExecuteNonQuery();
        }

        private void Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(AppConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private DataTable Query(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(AppConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                adapter.Fill(table);
            }
            return table;
        }

        public void SaveChatMessage(string sender, string text, string intent)
        {
            if (!IsAvailable) return;
            Execute("INSERT INTO dbo.ChatMessages(Sender, MessageText, Intent) VALUES(@s,@m,@i)",
                new SqlParameter("@s", sender), new SqlParameter("@m", text), new SqlParameter("@i", (object)intent ?? DBNull.Value));
        }

        public void AddActivity(string description)
        {
            string item = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " - " + description;
            if (!IsAvailable)
            {
                fallbackLog.Insert(0, item);
                return;
            }
            Execute("INSERT INTO dbo.ActivityLog(ActivityDescription) VALUES(@d)", new SqlParameter("@d", description));
        }

        public List<string> GetActivities(int limit)
        {
            List<string> list = new List<string>();
            if (!IsAvailable)
            {
                for (int i = 0; i < fallbackLog.Count && i < limit; i++) list.Add(fallbackLog[i]);
                return list;
            }
            DataTable t = Query("SELECT TOP (@n) ActivityDateTime, ActivityDescription FROM dbo.ActivityLog ORDER BY LogID DESC", new SqlParameter("@n", limit));
            foreach (DataRow r in t.Rows) list.Add(Convert.ToDateTime(r["ActivityDateTime"]).ToString("yyyy-MM-dd HH:mm") + " - " + r["ActivityDescription"]);
            return list;
        }

        public UserMemory LoadMemory()
        {
            if (!IsAvailable) return fallbackMemory;
            UserMemory memory = new UserMemory();
            DataTable t = Query("SELECT MemoryKey, MemoryValue FROM dbo.UserMemory");
            foreach (DataRow r in t.Rows)
            {
                string key = Convert.ToString(r["MemoryKey"]);
                string value = Convert.ToString(r["MemoryValue"]);
                if (key == "UserName") memory.UserName = value;
                if (key == "FavouriteTopic") memory.FavouriteTopic = value;
                if (key == "LastIntent") memory.LastIntent = value;
                if (key == "LastConcern") memory.LastConcern = value;
                if (key == "LastBotResponse") memory.LastBotResponse = value;
            }
            return memory;
        }

        public void SaveMemory(string key, string value)
        {
            if (!IsAvailable)
            {
                if (key == "UserName") fallbackMemory.UserName = value;
                if (key == "FavouriteTopic") fallbackMemory.FavouriteTopic = value;
                if (key == "LastIntent") fallbackMemory.LastIntent = value;
                if (key == "LastConcern") fallbackMemory.LastConcern = value;
                if (key == "LastBotResponse") fallbackMemory.LastBotResponse = value;
                return;
            }
            Execute(@"MERGE dbo.UserMemory AS target
USING (SELECT @k AS MemoryKey, @v AS MemoryValue) AS source
ON target.MemoryKey = source.MemoryKey
WHEN MATCHED THEN UPDATE SET MemoryValue = source.MemoryValue, UpdatedDateTime = GETDATE()
WHEN NOT MATCHED THEN INSERT(MemoryKey, MemoryValue) VALUES(source.MemoryKey, source.MemoryValue);",
                new SqlParameter("@k", key), new SqlParameter("@v", (object)value ?? DBNull.Value));
        }

        public void AddTask(string description, DateTime reminderDate)
        {
            if (!IsAvailable)
            {
                fallbackTasks.Add(new CyberTask { TaskID = nextTaskId++, TaskDescription = description, ReminderDate = reminderDate, TaskStatus = "Pending", CreatedDateTime = DateTime.Now });
                return;
            }
            Execute("INSERT INTO dbo.ReminderTasks(TaskDescription, ReminderDate, TaskStatus) VALUES(@d,@r,'Pending')",
                new SqlParameter("@d", description), new SqlParameter("@r", reminderDate.Date));
        }

        public void UpdateTask(int taskId, string description, DateTime reminderDate)
        {
            if (!IsAvailable)
            {
                CyberTask task = fallbackTasks.Find(t => t.TaskID == taskId);
                if (task != null) { task.TaskDescription = description; task.ReminderDate = reminderDate; }
                return;
            }
            Execute("UPDATE dbo.ReminderTasks SET TaskDescription=@d, ReminderDate=@r, UpdatedDateTime=GETDATE() WHERE TaskID=@id",
                new SqlParameter("@d", description), new SqlParameter("@r", reminderDate.Date), new SqlParameter("@id", taskId));
        }

        public void CompleteTask(int taskId)
        {
            if (!IsAvailable)
            {
                CyberTask task = fallbackTasks.Find(t => t.TaskID == taskId);
                if (task != null) task.TaskStatus = "Completed";
                return;
            }
            Execute("UPDATE dbo.ReminderTasks SET TaskStatus='Completed', CompletedDateTime=GETDATE(), UpdatedDateTime=GETDATE() WHERE TaskID=@id", new SqlParameter("@id", taskId));
        }

        public void DeleteTask(int taskId)
        {
            if (!IsAvailable)
            {
                fallbackTasks.RemoveAll(t => t.TaskID == taskId);
                return;
            }
            Execute("DELETE FROM dbo.ReminderTasks WHERE TaskID=@id", new SqlParameter("@id", taskId));
        }

        public DataTable GetTasksTable()
        {
            if (IsAvailable)
                return Query("SELECT TaskID, TaskDescription, ReminderDate, TaskStatus, CreatedDateTime FROM dbo.ReminderTasks ORDER BY ReminderDate ASC, TaskID DESC");

            DataTable t = new DataTable();
            t.Columns.Add("TaskID", typeof(int));
            t.Columns.Add("TaskDescription", typeof(string));
            t.Columns.Add("ReminderDate", typeof(DateTime));
            t.Columns.Add("TaskStatus", typeof(string));
            t.Columns.Add("CreatedDateTime", typeof(DateTime));
            foreach (CyberTask task in fallbackTasks) t.Rows.Add(task.TaskID, task.TaskDescription, task.ReminderDate, task.TaskStatus, task.CreatedDateTime);
            return t;
        }

        public void SaveQuizAnswer(QuizQuestion q, int selectedIndex, bool correct, string feedback)
        {
            if (!IsAvailable) return;
            Execute(@"INSERT INTO dbo.QuizAnswers(Topic, QuestionText, SelectedAnswer, CorrectAnswer, WasCorrect, Feedback)
VALUES(@t,@q,@s,@c,@w,@f)",
                new SqlParameter("@t", q.Topic), new SqlParameter("@q", q.QuestionText),
                new SqlParameter("@s", q.Options[selectedIndex]), new SqlParameter("@c", q.Options[q.CorrectIndex]),
                new SqlParameter("@w", correct), new SqlParameter("@f", feedback));
        }

        public void SaveQuizResult(int score, int total)
        {
            if (!IsAvailable) return;
            decimal percent = total == 0 ? 0 : Math.Round((decimal)score / total * 100, 2);
            Execute("INSERT INTO dbo.QuizResults(Score, TotalQuestions, PercentageScore) VALUES(@s,@t,@p)",
                new SqlParameter("@s", score), new SqlParameter("@t", total), new SqlParameter("@p", percent));
        }
    }
}
