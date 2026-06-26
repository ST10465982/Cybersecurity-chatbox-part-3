using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBotGUI2
{
    public class ChatbotService
    {
        private readonly Dictionary<string, string[]> topicKeywords = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Phishing", new[] { "phishing", "fake email", "email scam", "link", "urgent", "verify account", "suspicious email" } },
            { "Passwords", new[] { "password", "passphrase", "login", "credential", "reset" } },
            { "MFA", new[] { "mfa", "2fa", "two factor", "multi factor", "otp", "authenticator" } },
            { "Malware", new[] { "malware", "virus", "ransomware", "spyware", "trojan", "infected" } },
            { "Privacy", new[] { "privacy", "personal information", "data", "popia", "share online" } },
            { "Safe Browsing", new[] { "browser", "website", "public wifi", "wi-fi", "download", "safe online" } },
            { "Social Engineering", new[] { "social engineering", "trick", "scam call", "pretext", "impersonate" } },
            { "Incident Response", new[] { "hacked", "breach", "stolen", "compromised", "someone accessed" } }
        };

        public BotReply Reply(string input, UserMemory memory)
        {
            string text = (input ?? "").Trim();
            string lower = text.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(text))
                return Make("Please type a question first. You can ask about phishing, passwords, malware, privacy, MFA, scams, or public Wi-Fi.", "Empty", null);

            string fav = ExtractFavouriteTopic(lower);
            if (!string.IsNullOrWhiteSpace(fav))
                return Make("Got it. I will remember that you are interested in " + fav + ". I can give you extra tips and quiz practice around that topic.", "RememberFavouriteTopic", fav);

            string name = ExtractName(lower);
            if (!string.IsNullOrWhiteSpace(name))
            {
                string cleanName = ToTitle(name);
                return Make("Nice to meet you, " + cleanName + ". I will remember your name and use it when I help you with cybersecurity questions.", "RememberName", "Memory");
            }

            if (ContainsAny(lower, "hello", "hi", "hey", "good morning", "good afternoon", "good evening"))
            {
                string greetingName = string.IsNullOrWhiteSpace(memory.UserName) ? "" : ", " + memory.UserName;
                return Make("Hello" + greetingName + "! I am your Cybersecurity Awareness Bot. Ask me anything about online safety, or open the Quiz and Task Assistant tabs.", "Greeting", null);
            }

            if (ContainsAny(lower, "quiz", "test me", "questions", "practice"))
                return Make("Open the Cyber Quiz tab when you are ready. It has more than 10 questions, instant feedback, score tracking, and it saves your results.", "QuizHelp", "Quiz");

            if (ContainsAny(lower, "task", "remind", "reminder", "todo", "to do"))
                return Make("Use the Task Assistant tab to add, update, complete, or delete cybersecurity reminders. Example: 'Change Gmail password' with a reminder date.", "TaskHelp", "Tasks");

            string topic = DetectTopic(lower);
            if (topic != null)
                return Make(ResponseForTopic(topic, memory), "CyberAdvice", topic);

            if (ContainsAny(lower, "thank", "thanks", "appreciate"))
                return Make("You are welcome. Keep practising safe online habits: think before clicking, protect your passwords, and verify anything suspicious.", "Thanks", null);

            return Make("I understand you are asking about online safety. Try asking it like: 'How do I spot phishing?', 'What is a strong password?', 'What must I do if I was hacked?', or 'How do I stay safe on public Wi-Fi?'.", "GeneralHelp", null);
        }

        public string DetectNameFromReply(BotReply reply)
        {
            if (reply.Intent != "RememberName") return null;
            string marker = "Nice to meet you, ";
            int start = reply.Message.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return null;
            start += marker.Length;
            int end = reply.Message.IndexOf('.', start);
            return end > start ? reply.Message.Substring(start, end - start) : null;
        }

        public string DetectFavouriteFromReply(BotReply reply)
        {
            return reply.Intent == "RememberFavouriteTopic" ? reply.Topic : null;
        }

        private BotReply Make(string message, string intent, string topic)
        {
            return new BotReply { Message = message, Intent = intent, Topic = topic };
        }

        private string DetectTopic(string lower)
        {
            foreach (KeyValuePair<string, string[]> pair in topicKeywords)
                if (ContainsAny(lower, pair.Value)) return pair.Key;
            return null;
        }

        private string ResponseForTopic(string topic, UserMemory memory)
        {
            string name = string.IsNullOrWhiteSpace(memory.UserName) ? "" : memory.UserName + ", ";
            switch (topic)
            {
                case "Phishing":
                    return name + "phishing is when attackers use fake emails, SMSs, calls, or websites to steal details. Check the sender, do not rush, avoid unknown links, and verify through the real company website or official number.";
                case "Passwords":
                    return name + "use long passphrases, avoid reusing passwords, and never share them. A password manager is safer than writing passwords everywhere or using the same one for every account.";
                case "MFA":
                    return name + "multi-factor authentication adds another proof after your password, like an authenticator app or approval prompt. It protects you even if someone guesses or steals your password.";
                case "Malware":
                    return name + "malware is harmful software. Avoid unknown downloads, keep your device updated, use antivirus, and do not open suspicious attachments.";
                case "Privacy":
                    return name + "protect privacy by sharing less personal information online, checking app permissions, using strong account settings, and being careful with documents, ID numbers, and banking details.";
                case "Safe Browsing":
                    return name + "safe browsing means checking website addresses, avoiding strange downloads, using HTTPS sites, updating your browser, and not doing sensitive logins on untrusted public Wi-Fi.";
                case "Social Engineering":
                    return name + "social engineering tricks people using fear, urgency, trust, or curiosity. Slow down, verify the person, and never share passwords, OTPs, or banking PINs.";
                case "Incident Response":
                    return name + "if you think an account was hacked, change the password, enable MFA, sign out of other sessions, check recovery details, and report suspicious activity to the service provider.";
                default:
                    return "Cybersecurity is about protecting devices, accounts, data, and people from online threats.";
            }
        }

        private bool ContainsAny(string lower, params string[] words)
        {
            return words.Any(w => lower.Contains(w));
        }

        private string ExtractName(string lower)
        {
            string[] markers = { "my name is ", "i am ", "i'm ", "call me " };
            foreach (string marker in markers)
            {
                int i = lower.IndexOf(marker, StringComparison.Ordinal);
                if (i >= 0)
                {
                    string after = lower.Substring(i + marker.Length).Trim();
                    string[] parts = after.Split(new[] { ' ', '.', ',', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && parts[0].Length > 1) return parts[0];
                }
            }
            return null;
        }

        private string ExtractFavouriteTopic(string lower)
        {
            if (!(lower.Contains("interested in") || lower.Contains("favourite") || lower.Contains("favorite") || lower.Contains("like learning"))) return null;
            foreach (string topic in topicKeywords.Keys)
            {
                if (lower.Contains(topic.ToLowerInvariant())) return topic;
            }
            return null;
        }

        private string ToTitle(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return word;
            return char.ToUpper(word[0]) + (word.Length > 1 ? word.Substring(1).ToLowerInvariant() : "");
        }
    }
}
