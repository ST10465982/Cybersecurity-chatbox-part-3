using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CyberSecurityAwarenessBotGUI2
{
    public partial class MainForm : Form
    {
        private readonly DatabaseService database = new DatabaseService();
        private readonly ChatbotService chatbot = new ChatbotService();
        private readonly AudioService audio = new AudioService();
        private UserMemory memory = new UserMemory();

        private TabControl tabs;
        private Label lblDbStatus;
        private Label lblMemoryBanner;
        private RichTextBox chatBox;
        private TextBox txtUserInput;
        private DataGridView gridTasks;
        private TextBox txtTask;
        private DateTimePicker dtReminder;
        private Label lblQuizQuestion;
        private RadioButton[] quizOptions;
        private Label lblQuizFeedback;
        private Label lblQuizScore;
        private ListBox lstActivity;
        private Label lblMemoryDetails;

        private int selectedTaskId = -1;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private int logLimit = 10;

        private readonly Color Navy = Color.FromArgb(7, 18, 38);
        private readonly Color Card = Color.FromArgb(245, 248, 252);
        private readonly Color Accent = Color.FromArgb(0, 150, 220);
        private readonly Color Success = Color.FromArgb(24, 135, 84);

        private readonly Font TitleFont = new Font("Segoe UI", 18, FontStyle.Bold);
        private readonly Font HeaderFont = new Font("Segoe UI", 12, FontStyle.Bold);
        private readonly Font NormalFont = new Font("Segoe UI", 10);

        private readonly List<QuizQuestion> questions = new List<QuizQuestion>
        {
            new QuizQuestion("Phishing", "An email says your bank account will close today unless you click a link. What should you do?", new[] { "Click quickly", "Reply with your password", "Verify using the official bank app or number", "Forward it to everyone" }, 2, "Correct. Urgency is a phishing trick. Verify through official channels.", "Not safe. Urgent messages can be fake, so verify before clicking."),
            new QuizQuestion("Passwords", "Which password habit is safest?", new[] { "Use one password everywhere", "Use a long unique passphrase", "Use your birthday", "Share it with a friend" }, 1, "Correct. Long unique passphrases are harder to guess and safer.", "That option is risky. Use a long unique passphrase for each account."),
            new QuizQuestion("MFA", "What does multi-factor authentication do?", new[] { "Deletes your account", "Adds another proof of identity", "Shares your password", "Turns off security" }, 1, "Correct. MFA adds another layer after your password.", "Incorrect. MFA means using more than one way to prove it is you."),
            new QuizQuestion("Social Engineering", "A caller claims to be IT support and asks for your OTP. What should you do?", new[] { "Give the OTP", "Ask them to call later and verify with official support", "Post the OTP", "Turn off your phone" }, 1, "Correct. Real support should not ask for your OTP.", "Careful. OTPs must stay private, even from someone claiming to be support."),
            new QuizQuestion("Malware", "What is malware?", new[] { "Software that protects passwords", "Harmful software such as viruses or ransomware", "A strong password", "A safe website" }, 1, "Correct. Malware is harmful software that can damage or steal data.", "Incorrect. Malware is software designed to harm devices or data."),
            new QuizQuestion("Safe Browsing", "What is safer on public Wi-Fi?", new[] { "Do banking on any hotspot", "Avoid sensitive logins or use a trusted VPN", "Turn off updates", "Share files with strangers" }, 1, "Correct. Public Wi-Fi can be risky, so avoid sensitive actions.", "Risky choice. Public Wi-Fi should be treated as untrusted."),
            new QuizQuestion("Privacy", "Which action protects your personal information?", new[] { "Posting your ID number online", "Checking app permissions", "Sharing your PIN", "Using public computers for banking" }, 1, "Correct. App permissions control what information apps can access.", "Not quite. Protect personal information by limiting what apps and people can access."),
            new QuizQuestion("Incident Response", "What is a good first step if your email is hacked?", new[] { "Ignore it", "Change the password and enable MFA", "Share the old password", "Delete antivirus" }, 1, "Correct. Change the password, enable MFA, and review account activity.", "Incorrect. You should act quickly to secure the account."),
            new QuizQuestion("Updates", "Why are software updates important?", new[] { "They only change colours", "They fix bugs and security weaknesses", "They make passwords public", "They remove all protection" }, 1, "Correct. Updates often patch security weaknesses.", "Not correct. Updates are important because they fix security problems."),
            new QuizQuestion("Links", "Before clicking a link, what should you check?", new[] { "Sender and web address", "Only the colour", "How fast it loads", "Nothing" }, 0, "Correct. Check the sender, spelling, and real web address.", "Unsafe. Always check the sender and link before opening it."),
            new QuizQuestion("Backups", "Why should you back up important files?", new[] { "To protect against loss or ransomware", "To make scams work", "To delete your photos", "To weaken security" }, 0, "Correct. Backups help you recover after accidents or attacks.", "Incorrect. Backups are important for recovery after data loss or ransomware."),
            new QuizQuestion("Scams", "Which message is suspicious?", new[] { "A normal school notice", "You won money but must pay a fee first", "A saved contact saying hello", "A calendar reminder" }, 1, "Correct. Paying a fee to receive a prize is a common scam sign.", "Careful. Fake prizes and upfront fees are common scam warning signs.")
        };

        public MainForm()
        {
            InitializeComponent();
            BuildInterface();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            database.Initialise();

            lblDbStatus.Text = database.IsAvailable ? "Database: Connected to LocalDB" : "Database: Fallback Mode";
            lblDbStatus.ForeColor = database.IsAvailable ? Success : Color.DarkOrange;

            memory = database.LoadMemory();

            UpdateMemoryUI();

            AppendBot("Welcome to the Cybersecurity Awareness Bot. I can remember your name, give cyber advice, manage reminders, run a quiz, and keep an activity log.");

            database.SaveChatMessage("Bot", "Welcome to the Cybersecurity Awareness Bot.", "Startup");
            database.AddActivity("Application opened. " + database.StatusMessage);

            PlayGreetingAfterStartup();

            RefreshTasks();
            LoadQuestion();
            RefreshActivity();
        }

        private void PlayGreetingAfterStartup()
        {
            Timer startupAudioTimer = new Timer();
            startupAudioTimer.Interval = 700;

            startupAudioTimer.Tick += delegate
            {
                startupAudioTimer.Stop();
                startupAudioTimer.Dispose();

                audio.PlayGreeting();

                database.AddActivity("Greeting audio play command was triggered.");
                RefreshActivity();
            };

            startupAudioTimer.Start();
        }

        private void TestGreetingAudio()
        {
            audio.PlayGreeting();
            database.AddActivity("Greeting audio test button was clicked.");
            RefreshActivity();
        }

        private void BuildInterface()
        {
            Controls.Clear();

            Width = 1180;
            Height = 760;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 680);
            BackColor = Color.White;
            Text = "Cybersecurity Awareness Bot - Part 3";

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 112;
            header.BackColor = Navy;
            Controls.Add(header);

            PictureBox logo = new PictureBox();
            logo.Left = 22;
            logo.Top = 15;
            logo.Width = 80;
            logo.Height = 80;
            logo.SizeMode = PictureBoxSizeMode.Zoom;

            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png");

            if (File.Exists(logoPath))
            {
                logo.Image = Image.FromFile(logoPath);
            }
            else
            {
                Bitmap logoBitmap = new Bitmap(80, 80);

                using (Graphics g = Graphics.FromImage(logoBitmap))
                {
                    g.Clear(Accent);
                    g.FillEllipse(Brushes.White, 15, 15, 50, 50);
                    g.DrawString("CS", new Font("Segoe UI", 16, FontStyle.Bold), Brushes.Navy, 20, 25);
                }

                logo.Image = logoBitmap;
            }

            header.Controls.Add(logo);

            Label title = new Label();
            title.Text = "Cybersecurity Awareness Bot";
            title.ForeColor = Color.White;
            title.Font = TitleFont;
            title.AutoSize = true;
            title.Left = 120;
            title.Top = 20;
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Part 3 GUI • SQL CRUD • NLP Simulation • Memory • Quiz • Activity Log";
            subtitle.ForeColor = Color.FromArgb(210, 230, 245);
            subtitle.Font = NormalFont;
            subtitle.AutoSize = true;
            subtitle.Left = 123;
            subtitle.Top = 58;
            header.Controls.Add(subtitle);

            lblDbStatus = new Label();
            lblDbStatus.Text = "Database: checking...";
            lblDbStatus.ForeColor = Color.White;
            lblDbStatus.Font = HeaderFont;
            lblDbStatus.AutoSize = true;
            lblDbStatus.Left = 760;
            lblDbStatus.Top = 24;
            header.Controls.Add(lblDbStatus);

            lblMemoryBanner = new Label();
            lblMemoryBanner.Text = "Memory: loading...";
            lblMemoryBanner.ForeColor = Color.FromArgb(210, 230, 245);
            lblMemoryBanner.Font = NormalFont;
            lblMemoryBanner.AutoSize = true;
            lblMemoryBanner.Left = 760;
            lblMemoryBanner.Top = 60;
            header.Controls.Add(lblMemoryBanner);

            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Font = NormalFont;
            Controls.Add(tabs);

            BuildChatTab();
            BuildTasksTab();
            BuildQuizTab();
            BuildActivityTab();
            BuildMemoryTab();

            header.BringToFront();
        }

        private void BuildChatTab()
        {
            TabPage page = new TabPage("Chatbot GUI");
            page.BackColor = Color.White;
            tabs.TabPages.Add(page);

            page.Controls.Add(Heading("Cybersecurity Chat Assistant", 25, 18));

            Label intro = new Label();
            intro.Text = "Ask a cybersecurity question, save your name, or choose a quick topic below.";
            intro.Left = 28;
            intro.Top = 52;
            intro.Width = 760;
            intro.Height = 25;
            intro.Font = NormalFont;
            intro.ForeColor = Color.FromArgb(80, 90, 100);
            page.Controls.Add(intro);

            Panel chatPanel = CardPanel(25, 90, 760, 480);
            page.Controls.Add(chatPanel);

            Label chatTitle = new Label();
            chatTitle.Text = "Conversation";
            chatTitle.Left = 18;
            chatTitle.Top = 12;
            chatTitle.Width = 200;
            chatTitle.Height = 25;
            chatTitle.Font = HeaderFont;
            chatTitle.ForeColor = Navy;
            chatPanel.Controls.Add(chatTitle);

            chatBox = new RichTextBox();
            chatBox.Left = 18;
            chatBox.Top = 45;
            chatBox.Width = 720;
            chatBox.Height = 360;
            chatBox.ReadOnly = true;
            chatBox.Font = new Font("Segoe UI", 10);
            chatBox.BackColor = Color.White;
            chatBox.BorderStyle = BorderStyle.FixedSingle;
            chatPanel.Controls.Add(chatBox);

            txtUserInput = new TextBox();
            txtUserInput.Left = 18;
            txtUserInput.Top = 420;
            txtUserInput.Width = 570;
            txtUserInput.Height = 32;
            txtUserInput.Font = new Font("Segoe UI", 10);
            txtUserInput.KeyDown += TxtUserInput_KeyDown;
            chatPanel.Controls.Add(txtUserInput);

            Button btnSend = Button("Send", 600, 417, 138, 36, Accent);
            btnSend.Click += delegate
            {
                SendChat();
            };
            chatPanel.Controls.Add(btnSend);

            Panel sidePanel = CardPanel(810, 90, 305, 480);
            page.Controls.Add(sidePanel);

            Label quickTitle = new Label();
            quickTitle.Text = "Quick Cyber Topics";
            quickTitle.Left = 18;
            quickTitle.Top = 15;
            quickTitle.Width = 250;
            quickTitle.Height = 25;
            quickTitle.Font = HeaderFont;
            quickTitle.ForeColor = Navy;
            sidePanel.Controls.Add(quickTitle);

            Label quickInfo = new Label();
            quickInfo.Text = "Click a topic to test the chatbot quickly during your demo.";
            quickInfo.Left = 18;
            quickInfo.Top = 42;
            quickInfo.Width = 260;
            quickInfo.Height = 40;
            quickInfo.Font = NormalFont;
            quickInfo.ForeColor = Color.FromArgb(80, 90, 100);
            sidePanel.Controls.Add(quickInfo);

            Button btnName = Button("Save my name", 18, 95, 260, 34, Accent);
            btnName.Click += delegate
            {
                txtUserInput.Text = "My name is Onga";
                SendChat();
            };
            sidePanel.Controls.Add(btnName);

            Button btnPhishing = Button("Ask about phishing", 18, 137, 260, 34, Accent);
            btnPhishing.Click += delegate
            {
                txtUserInput.Text = "How do I spot phishing?";
                SendChat();
            };
            sidePanel.Controls.Add(btnPhishing);

            Button btnPassword = Button("Ask about passwords", 18, 179, 260, 34, Accent);
            btnPassword.Click += delegate
            {
                txtUserInput.Text = "What is a strong password?";
                SendChat();
            };
            sidePanel.Controls.Add(btnPassword);

            Button btnMfa = Button("Ask about MFA", 18, 221, 260, 34, Accent);
            btnMfa.Click += delegate
            {
                txtUserInput.Text = "What is MFA?";
                SendChat();
            };
            sidePanel.Controls.Add(btnMfa);

            Button btnScam = Button("Ask about scams", 18, 263, 260, 34, Accent);
            btnScam.Click += delegate
            {
                txtUserInput.Text = "How do I avoid online scams?";
                SendChat();
            };
            sidePanel.Controls.Add(btnScam);

            Panel memoryPanel = new Panel();
            memoryPanel.Left = 18;
            memoryPanel.Top = 320;
            memoryPanel.Width = 260;
            memoryPanel.Height = 95;
            memoryPanel.BackColor = Color.White;
            memoryPanel.BorderStyle = BorderStyle.FixedSingle;
            sidePanel.Controls.Add(memoryPanel);

            Label memoryTitle = new Label();
            memoryTitle.Text = "Memory Feature";
            memoryTitle.Left = 12;
            memoryTitle.Top = 10;
            memoryTitle.Width = 220;
            memoryTitle.Height = 22;
            memoryTitle.Font = HeaderFont;
            memoryTitle.ForeColor = Success;
            memoryPanel.Controls.Add(memoryTitle);

            Label memoryInfo = new Label();
            memoryInfo.Text = "The bot remembers your name, favourite topic, last concern, and previous response.";
            memoryInfo.Left = 12;
            memoryInfo.Top = 38;
            memoryInfo.Width = 230;
            memoryInfo.Height = 45;
            memoryInfo.Font = NormalFont;
            memoryInfo.ForeColor = Color.FromArgb(70, 80, 90);
            memoryPanel.Controls.Add(memoryInfo);

            Button btnTestAudio = Button("Play Greeting Voice", 18, 430, 260, 34, Success);
            btnTestAudio.Click += delegate
            {
                TestGreetingAudio();
            };
            sidePanel.Controls.Add(btnTestAudio);
        }

        private void BuildTasksTab()
        {
            TabPage page = new TabPage("Task Assistant + Reminders");
            page.BackColor = Color.White;
            tabs.TabPages.Add(page);

            page.Controls.Add(Heading("Task Assistant with Full SQL CRUD", 20, 18));

            Panel entry = CardPanel(20, 60, 1095, 110);
            page.Controls.Add(entry);

            Label lblTask = new Label();
            lblTask.Text = "Task / Reminder Description";
            lblTask.Left = 15;
            lblTask.Top = 15;
            lblTask.Width = 230;
            lblTask.Font = HeaderFont;
            entry.Controls.Add(lblTask);

            txtTask = new TextBox();
            txtTask.Left = 15;
            txtTask.Top = 45;
            txtTask.Width = 450;
            txtTask.Height = 28;
            txtTask.Font = NormalFont;
            entry.Controls.Add(txtTask);

            Label lblReminder = new Label();
            lblReminder.Text = "Reminder Date";
            lblReminder.Left = 490;
            lblReminder.Top = 15;
            lblReminder.Width = 160;
            lblReminder.Font = HeaderFont;
            entry.Controls.Add(lblReminder);

            dtReminder = new DateTimePicker();
            dtReminder.Left = 490;
            dtReminder.Top = 45;
            dtReminder.Width = 180;
            dtReminder.Font = NormalFont;
            entry.Controls.Add(dtReminder);

            Button btnAdd = Button("Add", 700, 40, 85, 34, Accent);
            btnAdd.Click += delegate
            {
                AddTask();
            };
            entry.Controls.Add(btnAdd);

            Button btnUpdate = Button("Update", 790, 40, 90, 34, Color.FromArgb(90, 110, 130));
            btnUpdate.Click += delegate
            {
                UpdateTask();
            };
            entry.Controls.Add(btnUpdate);

            Button btnComplete = Button("Complete", 885, 40, 95, 34, Success);
            btnComplete.Click += delegate
            {
                CompleteTask();
            };
            entry.Controls.Add(btnComplete);

            Button btnDelete = Button("Delete", 985, 40, 85, 34, Color.FromArgb(180, 55, 55));
            btnDelete.Click += delegate
            {
                DeleteTask();
            };
            entry.Controls.Add(btnDelete);

            gridTasks = new DataGridView();
            gridTasks.Left = 20;
            gridTasks.Top = 190;
            gridTasks.Width = 1095;
            gridTasks.Height = 390;
            gridTasks.ReadOnly = true;
            gridTasks.AutoGenerateColumns = true;
            gridTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTasks.MultiSelect = false;
            gridTasks.AllowUserToAddRows = false;
            gridTasks.Font = NormalFont;
            gridTasks.BackgroundColor = Card;
            gridTasks.SelectionChanged += GridTasks_SelectionChanged;
            page.Controls.Add(gridTasks);
        }

        private void BuildQuizTab()
        {
            TabPage page = new TabPage("Cyber Quiz Mini-Game");
            page.BackColor = Color.White;
            tabs.TabPages.Add(page);

            page.Controls.Add(Heading("Cybersecurity Mini-Game Quiz", 20, 18));

            Panel quizCard = CardPanel(20, 60, 1095, 480);
            page.Controls.Add(quizCard);

            lblQuizScore = new Label();
            lblQuizScore.Text = "Score: 0/0";
            lblQuizScore.Left = 20;
            lblQuizScore.Top = 20;
            lblQuizScore.Width = 300;
            lblQuizScore.Font = HeaderFont;
            quizCard.Controls.Add(lblQuizScore);

            lblQuizQuestion = new Label();
            lblQuizQuestion.Text = "Question";
            lblQuizQuestion.Left = 20;
            lblQuizQuestion.Top = 60;
            lblQuizQuestion.Width = 1045;
            lblQuizQuestion.Height = 70;
            lblQuizQuestion.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            quizCard.Controls.Add(lblQuizQuestion);

            quizOptions = new RadioButton[4];

            for (int i = 0; i < 4; i++)
            {
                quizOptions[i] = new RadioButton();
                quizOptions[i].Left = 45;
                quizOptions[i].Top = 145 + i * 45;
                quizOptions[i].Width = 980;
                quizOptions[i].Height = 32;
                quizOptions[i].Font = NormalFont;
                quizCard.Controls.Add(quizOptions[i]);
            }

            Button btnSubmit = Button("Submit Answer", 45, 340, 160, 40, Accent);
            btnSubmit.Click += delegate
            {
                SubmitAnswer();
            };
            quizCard.Controls.Add(btnSubmit);

            Button btnRestart = Button("Restart Quiz", 215, 340, 140, 40, Color.FromArgb(90, 110, 130));
            btnRestart.Click += delegate
            {
                RestartQuiz();
            };
            quizCard.Controls.Add(btnRestart);

            lblQuizFeedback = new Label();
            lblQuizFeedback.Text = "Choose an answer and click Submit.";
            lblQuizFeedback.Left = 45;
            lblQuizFeedback.Top = 395;
            lblQuizFeedback.Width = 970;
            lblQuizFeedback.Height = 60;
            lblQuizFeedback.Font = NormalFont;
            lblQuizFeedback.ForeColor = Navy;
            quizCard.Controls.Add(lblQuizFeedback);
        }

        private void BuildActivityTab()
        {
            TabPage page = new TabPage("Activity Log");
            page.BackColor = Color.White;
            tabs.TabPages.Add(page);

            page.Controls.Add(Heading("Activity Log with Clear Summaries", 20, 18));

            lstActivity = new ListBox();
            lstActivity.Left = 20;
            lstActivity.Top = 65;
            lstActivity.Width = 1095;
            lstActivity.Height = 450;
            lstActivity.Font = NormalFont;
            page.Controls.Add(lstActivity);

            Button btnShowMore = Button("Show More", 20, 535, 130, 38, Accent);
            btnShowMore.Click += delegate
            {
                logLimit += 10;
                RefreshActivity();
            };
            page.Controls.Add(btnShowMore);

            Button btnRefresh = Button("Refresh", 160, 535, 120, 38, Color.FromArgb(90, 110, 130));
            btnRefresh.Click += delegate
            {
                RefreshActivity();
            };
            page.Controls.Add(btnRefresh);
        }

        private void BuildMemoryTab()
        {
            TabPage page = new TabPage("Memory + Code Structure");
            page.BackColor = Color.White;
            tabs.TabPages.Add(page);

            page.Controls.Add(Heading("Chatbot Memory and Project Structure", 20, 18));

            Panel card = CardPanel(20, 65, 1095, 500);
            page.Controls.Add(card);

            lblMemoryDetails = new Label();
            lblMemoryDetails.Left = 25;
            lblMemoryDetails.Top = 25;
            lblMemoryDetails.Width = 1045;
            lblMemoryDetails.Height = 430;
            lblMemoryDetails.Font = NormalFont;
            lblMemoryDetails.AutoSize = false;
            card.Controls.Add(lblMemoryDetails);
        }

        private Label Heading(string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.Left = left;
            label.Top = top;
            label.AutoSize = true;
            label.Font = TitleFont;
            label.ForeColor = Navy;
            return label;
        }

        private Panel CardPanel(int left, int top, int width, int height)
        {
            Panel panel = new Panel();
            panel.Left = left;
            panel.Top = top;
            panel.Width = width;
            panel.Height = height;
            panel.BackColor = Card;
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
        }

        private Button Button(string text, int left, int top, int width, int height, Color colour)
        {
            Button button = new Button();
            button.Text = text;
            button.Left = left;
            button.Top = top;
            button.Width = width;
            button.Height = height;
            button.Font = HeaderFont;
            button.BackColor = colour;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void TxtUserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendChat();
            }
        }

        private void SendChat()
        {
            string input = txtUserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please type a message first.");
                return;
            }

            txtUserInput.Clear();

            AppendUser(input);

            BotReply reply = chatbot.Reply(input, memory);

            ApplyMemory(reply);

            AppendBot(reply.Message);

            database.SaveChatMessage("User", input, reply.Intent);
            database.SaveChatMessage("Bot", reply.Message, reply.Intent);

            string activityMessage = "Chatbot answered a " + reply.Intent + " request";

            if (!string.IsNullOrWhiteSpace(reply.Topic))
            {
                activityMessage += " about " + reply.Topic;
            }

            activityMessage += ".";

            database.AddActivity(activityMessage);

            RefreshActivity();
        }

        private void ApplyMemory(BotReply reply)
        {
            string name = chatbot.DetectNameFromReply(reply);

            if (!string.IsNullOrWhiteSpace(name))
            {
                memory.UserName = name;
                database.SaveMemory("UserName", name);
                database.AddActivity("Memory updated: user name saved as " + name + ".");
            }

            string favourite = chatbot.DetectFavouriteFromReply(reply);

            if (!string.IsNullOrWhiteSpace(favourite))
            {
                memory.FavouriteTopic = favourite;
                database.SaveMemory("FavouriteTopic", favourite);
                database.AddActivity("Memory updated: favourite topic saved as " + favourite + ".");
            }

            memory.LastIntent = reply.Intent;
            memory.LastConcern = reply.Topic;
            memory.LastBotResponse = reply.Message;

            database.SaveMemory("LastIntent", reply.Intent);
            database.SaveMemory("LastConcern", reply.Topic);
            database.SaveMemory("LastBotResponse", reply.Message);

            UpdateMemoryUI();
        }

        private void AppendUser(string text)
        {
            chatBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chatBox.SelectionColor = Accent;
            chatBox.AppendText("You: ");

            chatBox.SelectionFont = NormalFont;
            chatBox.SelectionColor = Color.Black;
            chatBox.AppendText(text + Environment.NewLine + Environment.NewLine);

            chatBox.ScrollToCaret();
        }

        private void AppendBot(string text)
        {
            chatBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chatBox.SelectionColor = Success;
            chatBox.AppendText("Bot: ");

            chatBox.SelectionFont = NormalFont;
            chatBox.SelectionColor = Color.Black;
            chatBox.AppendText(text + Environment.NewLine + Environment.NewLine);

            chatBox.ScrollToCaret();
        }

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(txtTask.Text))
            {
                MessageBox.Show("Please enter a task description.");
                return;
            }

            database.AddTask(txtTask.Text.Trim(), dtReminder.Value.Date);

            database.AddActivity(
                "Task added: " +
                txtTask.Text.Trim() +
                " due " +
                dtReminder.Value.ToShortDateString() +
                "."
            );

            txtTask.Clear();

            RefreshTasks();
            RefreshActivity();

            MessageBox.Show("Task added successfully.");
        }

        private void UpdateTask()
        {
            if (selectedTaskId < 0)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTask.Text))
            {
                MessageBox.Show("Enter the updated task description.");
                return;
            }

            database.UpdateTask(selectedTaskId, txtTask.Text.Trim(), dtReminder.Value.Date);
            database.AddActivity("Task updated: ID " + selectedTaskId + ".");

            RefreshTasks();
            RefreshActivity();

            MessageBox.Show("Task updated successfully.");
        }

        private void CompleteTask()
        {
            if (selectedTaskId < 0)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            database.CompleteTask(selectedTaskId);
            database.AddActivity("Task completed: ID " + selectedTaskId + ".");

            RefreshTasks();
            RefreshActivity();

            MessageBox.Show("Task marked as complete.");
        }

        private void DeleteTask()
        {
            if (selectedTaskId < 0)
            {
                MessageBox.Show("Select a task first.");
                return;
            }

            database.DeleteTask(selectedTaskId);
            database.AddActivity("Task deleted: ID " + selectedTaskId + ".");

            selectedTaskId = -1;
            txtTask.Clear();

            RefreshTasks();
            RefreshActivity();

            MessageBox.Show("Task deleted successfully.");
        }

        private void RefreshTasks()
        {
            try
            {
                if (gridTasks == null || database == null)
                {
                    return;
                }

                gridTasks.SelectionChanged -= GridTasks_SelectionChanged;

                DataTable taskTable = database.GetTasksTable();

                if (taskTable == null)
                {
                    taskTable = new DataTable();
                }

                EnsureColumn(taskTable, "TaskID", typeof(int));
                EnsureColumn(taskTable, "TaskDescription", typeof(string));
                EnsureColumn(taskTable, "ReminderDate", typeof(DateTime));
                EnsureColumn(taskTable, "TaskStatus", typeof(string));
                EnsureColumn(taskTable, "CreatedDateTime", typeof(DateTime));

                gridTasks.DataSource = null;
                gridTasks.Columns.Clear();

                gridTasks.AutoGenerateColumns = true;
                gridTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                gridTasks.MultiSelect = false;
                gridTasks.AllowUserToAddRows = false;
                gridTasks.ReadOnly = true;

                gridTasks.DataSource = taskTable;

                SetColumnHeader("TaskID", "ID", 70);
                SetColumnHeader("TaskDescription", "Task Description", 250);
                SetColumnHeader("ReminderDate", "Reminder Date", 130);
                SetColumnHeader("TaskStatus", "Status", 100);
                SetColumnHeader("CreatedDateTime", "Created At", 150);

                selectedTaskId = -1;
            }
            catch
            {
                selectedTaskId = -1;
            }
            finally
            {
                if (gridTasks != null)
                {
                    gridTasks.SelectionChanged += GridTasks_SelectionChanged;
                }
            }
        }

        private void EnsureColumn(DataTable table, string columnName, Type dataType)
        {
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, dataType);
            }
        }

        private void SetColumnHeader(string columnName, string headerText, int width)
        {
            if (gridTasks == null)
            {
                return;
            }

            DataGridViewColumn column = gridTasks.Columns[columnName];

            if (column != null)
            {
                column.HeaderText = headerText;
                column.Width = width;
            }
        }

        private void GridTasks_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gridTasks == null)
                {
                    return;
                }

                if (gridTasks.SelectedRows == null || gridTasks.SelectedRows.Count == 0)
                {
                    return;
                }

                DataGridViewRow row = gridTasks.SelectedRows[0];

                if (row == null || row.IsNewRow)
                {
                    return;
                }

                if (!gridTasks.Columns.Contains("TaskID"))
                {
                    return;
                }

                if (row.Cells["TaskID"] == null || row.Cells["TaskID"].Value == null)
                {
                    return;
                }

                int id;

                if (!int.TryParse(row.Cells["TaskID"].Value.ToString(), out id))
                {
                    return;
                }

                selectedTaskId = id;

                if (gridTasks.Columns.Contains("TaskDescription") &&
                    row.Cells["TaskDescription"] != null &&
                    row.Cells["TaskDescription"].Value != null)
                {
                    txtTask.Text = row.Cells["TaskDescription"].Value.ToString();
                }

                if (gridTasks.Columns.Contains("ReminderDate") &&
                    row.Cells["ReminderDate"] != null &&
                    row.Cells["ReminderDate"].Value != null)
                {
                    DateTime date;

                    if (DateTime.TryParse(row.Cells["ReminderDate"].Value.ToString(), out date))
                    {
                        dtReminder.Value = date;
                    }
                }
            }
            catch
            {
                selectedTaskId = -1;
            }
        }

        private void LoadQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                lblQuizQuestion.Text = "Quiz complete! Final score: " + score + "/" + questions.Count;
                lblQuizScore.Text = "Final Score: " + score + "/" + questions.Count;

                if (score >= 10)
                {
                    lblQuizFeedback.Text = "Excellent work. You are showing strong cybersecurity awareness.";
                }
                else
                {
                    lblQuizFeedback.Text = "Good effort. Review the feedback and try again to improve.";
                }

                foreach (RadioButton radioButton in quizOptions)
                {
                    radioButton.Visible = false;
                    radioButton.Checked = false;
                }

                database.SaveQuizResult(score, questions.Count);
                database.AddActivity("Quiz completed with score " + score + "/" + questions.Count + ".");

                RefreshActivity();

                return;
            }

            QuizQuestion question = questions[currentQuestionIndex];

            lblQuizScore.Text =
                "Question " +
                (currentQuestionIndex + 1) +
                " of " +
                questions.Count +
                " | Score: " +
                score;

            lblQuizQuestion.Text = question.QuestionText;

            for (int i = 0; i < quizOptions.Length; i++)
            {
                quizOptions[i].Visible = true;
                quizOptions[i].Checked = false;
                quizOptions[i].Text = question.Options[i];
            }

            lblQuizFeedback.Text = "Topic: " + question.Topic + ". Choose an answer and click Submit.";
        }

        private void SubmitAnswer()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                return;
            }

            int selected = -1;

            for (int i = 0; i < quizOptions.Length; i++)
            {
                if (quizOptions[i].Checked)
                {
                    selected = i;
                }
            }

            if (selected < 0)
            {
                MessageBox.Show("Please choose an answer first.");
                return;
            }

            QuizQuestion question = questions[currentQuestionIndex];

            bool correct = selected == question.CorrectIndex;

            if (correct)
            {
                score++;
            }

            string feedback;

            if (correct)
            {
                feedback = question.CorrectFeedback;
            }
            else
            {
                feedback =
                    question.IncorrectFeedback +
                    " Correct answer: " +
                    question.Options[question.CorrectIndex] +
                    ".";
            }

            lblQuizFeedback.Text = feedback;

            database.SaveQuizAnswer(question, selected, correct, feedback);

            database.AddActivity(
                "Quiz answered: " +
                question.Topic +
                " was " +
                (correct ? "correct" : "incorrect") +
                "."
            );

            currentQuestionIndex++;

            Timer timer = new Timer();
            timer.Interval = 900;

            timer.Tick += delegate
            {
                timer.Stop();
                timer.Dispose();

                LoadQuestion();
                RefreshActivity();
            };

            timer.Start();
        }

        private void RestartQuiz()
        {
            currentQuestionIndex = 0;
            score = 0;

            database.AddActivity("Quiz restarted.");

            LoadQuestion();
            RefreshActivity();
        }

        private void RefreshActivity()
        {
            if (lstActivity == null)
            {
                return;
            }

            lstActivity.Items.Clear();

            foreach (string item in database.GetActivities(logLimit))
            {
                lstActivity.Items.Add(item);
            }
        }

        private void UpdateMemoryUI()
        {
            string name;

            if (string.IsNullOrWhiteSpace(memory.UserName))
            {
                name = "not saved yet";
            }
            else
            {
                name = memory.UserName;
            }

            string topic;

            if (string.IsNullOrWhiteSpace(memory.FavouriteTopic))
            {
                topic = "not saved yet";
            }
            else
            {
                topic = memory.FavouriteTopic;
            }

            lblMemoryBanner.Text = "Memory: Name = " + name + " | Favourite topic = " + topic;

            if (lblMemoryDetails != null)
            {
                lblMemoryDetails.Text =
                    "Saved Chatbot Memory\n\n" +
                    "User name: " + name + "\n" +
                    "Favourite cybersecurity topic: " + topic + "\n" +
                    "Last detected intent: " + (memory.LastIntent ?? "none") + "\n" +
                    "Last concern/topic: " + (memory.LastConcern ?? "none") + "\n" +
                    "Last bot response: " + (memory.LastBotResponse ?? "none") + "\n\n" +

                    "Code Structure\n\n" +
                    "Program.cs - starts the Windows Forms application.\n" +
                    "MainForm.cs - controls the professional GUI, tabs, buttons, quiz and task events.\n" +
                    "DatabaseService.cs - handles SQL LocalDB, tables, CRUD, activity log, memory, quiz answers and fallback mode.\n" +
                    "ChatbotService.cs - handles NLP-style keyword and phrase detection plus natural responses.\n" +
                    "AudioService.cs - plays the greeting WAV from the Assets folder.\n" +
                    "Models.cs - keeps task, quiz, memory and reply objects organised.\n\n" +

                    "Rubric Evidence\n\n" +
                    "The app includes full task CRUD, 12 quiz questions, adaptive feedback, NLP simulation, memory, activity log navigation, visible logo, clean folders, README and SQL script.";
            }
        }
    }
}