CyberSecurityAwarenessBotGUI2 - Part 3 Distinction Build
========================================================

Student submission project for Cybersecurity Awareness Chatbot Part 3.
This version was built to match the highest rubric column by including GUI design, SQL integration, chatbot memory, NLP simulation, task reminders, quiz mini-game and activity logging.

HOW TO RUN
----------
1. Unzip the folder.
2. Open CyberSecurityAwarenessBotGUI2.sln in Visual Studio.
3. Build the solution.
4. Press Start.
5. The application will try to connect to SQL Server LocalDB automatically.
6. If LocalDB is unavailable on the computer, the app still opens using fallback mode, but for marking the SQL script is included in the Database folder.

FOLDER STRUCTURE
----------------
CyberSecurityAwarenessBotGUI2.sln
CyberSecurityAwarenessBotGUI2/
  Program.cs
  MainForm.cs
  MainForm.Designer.cs
  DatabaseService.cs
  ChatbotService.cs
  AudioService.cs
  Models.cs
  App.config
  Assets/
    logo.png
    greeting.wav
Database/
  CyberSecurityBot_Part3.sql
README.txt

FEATURES MAPPED TO THE RUBRIC
-----------------------------

1. Task Assistant Database Integration [15 Marks]
- Full CRUD is implemented for reminder tasks.
- Add task: saves TaskDescription and ReminderDate.
- Read/load task: DataGridView loads all tasks from the database.
- Update task: selected task can be edited.
- Complete task: status changes to Completed.
- Delete task: task is removed from the database.
- SQL table used: ReminderTasks.

2. Cybersecurity Mini-Game Quiz with GUI [15 Marks]
- Includes 12 cybersecurity questions.
- Covers phishing, passwords, MFA, social engineering, malware, safe browsing, privacy, incident response, updates, links, backups and scams.
- Gives different feedback for correct and incorrect answers.
- Tracks score and saves quiz answers/results.
- SQL tables used: QuizAnswers and QuizResults.

3. NLP Simulation with GUI Interaction [10 Marks]
- Detects different user phrases, not only one exact keyword.
- Recognises greetings, names, favourite topics, phishing, passwords, malware, privacy, MFA, public Wi-Fi, scams, hacked accounts and task/quiz requests.
- Replies naturally and adapts using memory.
- Example phrases:
  - My name is Onga
  - I am interested in phishing
  - How do I spot phishing?
  - Someone hacked my account
  - What is MFA?

4. Activity Log Feature with GUI [10 Marks]
- Every important action is logged with a timestamp.
- Logs app start, chatbot replies, memory updates, task CRUD, quiz answers, quiz restart and quiz completion.
- Shows 10 actions first and has Show More navigation.
- SQL table used: ActivityLog.

5. Combining Parts 1, 2 and 3 [10 Marks]
- Greeting audio is included in Assets/greeting.wav.
- GUI chatbot integrates memory, audio, quiz, tasks and activity log.
- Design is consistent across tabs.
- Logo is visible in the top header.

6. Correct Submission [5 Marks]
- Clean folder structure.
- Required code files included.
- Assets included.
- SQL script included.
- README includes setup and usage instructions.

7. GitHub and Release Tags [10 Marks]
For final submission, upload this folder to GitHub and make sure you have:
- At least 6 clear commits.
- At least 3 release tags, for example:
  - v1.0-part1-chatbot
  - v2.0-part2-gui
  - v3.0-part3-final
- Add release notes for each tag.
- Submit your GitHub link on ARC.

SUGGESTED COMMIT MESSAGES
-------------------------
1. Initial project structure and Windows Forms setup
2. Add chatbot GUI and greeting audio
3. Add SQL database service and task CRUD
4. Add cybersecurity quiz mini-game
5. Add NLP simulation and chatbot memory
6. Add activity log, logo, README and final polish

DEMO SCRIPT
-----------
1. Open the app and show the logo and greeting audio.
2. Go to Chatbot GUI and type: My name is Onga.
3. Type: I am interested in phishing.
4. Type: How do I spot phishing?
5. Go to Task Assistant and add a reminder such as: Change Gmail password.
6. Update the task, complete it, and delete a test task to show CRUD.
7. Go to Cyber Quiz and answer several questions to show feedback and score.
8. Go to Activity Log and show the logged actions.
9. Go to Memory + Code Structure to show saved memory and organised code structure.

IMPORTANT
---------
The app automatically creates CyberSecurityBotDB when SQL Server LocalDB is available.
If your lecturer wants to see the SQL manually, open Database/CyberSecurityBot_Part3.sql in SQL Server Management Studio and run it.
