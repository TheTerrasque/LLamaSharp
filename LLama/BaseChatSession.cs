using System;
using System.Collections.Generic;
using System.Text;
using LLama.Interfaces;

namespace LLama {
    public class ChatHistoryEntry {
        public string Role { get; set; }
        public string Text { get; set; }
    }

    public class ChatMetadata {
        public string Prompt { get; set; } = "Prompt";
        public string User { get; set; } = "User";
        public string Assistant { get; set; } = "Assistant";

        public ChatMetadata SetPrompt(string v)
        {
            Prompt = v;
            return this;
        }

        public ChatMetadata SetUserName(string v)
        {
            User = v;
            return this;
        }

        public ChatMetadata SetAssistantName(string v)
        {
            Assistant = v;
            return this;
        }

        public ChatMetadata WithPromptFromFile(string filename) {
            Prompt = System.IO.File.ReadAllText(filename);
            return this;
        }
    }

    public class BaseChatSession {
        private ILanguageModel _model;
        private ChatMetadata _metadata;

        public List<ChatHistoryEntry> ChatHistory { get; } = new();
        public BaseChatSession(ILanguageModel model, ChatMetadata? metadata = null) {
            _model = model;
            if (metadata == null) metadata = new ChatMetadata();
            _metadata = metadata;

            if (_metadata.Prompt != "") {
                ChatHistory.Add(new ChatHistoryEntry() { Role = "", Text = _metadata.Prompt });
            }
        }

        string _formatChatHistory(List<ChatHistoryEntry> history) {
            StringBuilder sb = new();
            foreach (var entry in history) {
                if (entry.Role == "") {
                    sb.Append($"{entry.Text}\n");
                    continue;
                }
                sb.Append($"{entry.Role}: {entry.Text}\n");
            }
            sb.Append($"{_metadata.Assistant}: ");
            return sb.ToString();
        }

        public IEnumerable<string> Chat(string text) {
            ChatHistory.Add(new ChatHistoryEntry() { Role = "User", Text = text });
            string totalResponse = "";
            foreach (var response in _model.Generate(_formatChatHistory(ChatHistory))) {
                totalResponse += response;
                yield return response;
            }
            ChatHistory.Add(new ChatHistoryEntry() { Role = "Assistant", Text = totalResponse });
        }
    }
}