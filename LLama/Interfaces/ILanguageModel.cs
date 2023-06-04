using System.Collections.Generic;
using System.Threading;

namespace LLama.Interfaces
{
    public interface ILanguageModel
    {
        public IEnumerable<string> Generate(string text, CancellationToken? ct=null, ILlamaSamplingParams? samplingParams = null);
        public byte[] SaveState();
        public void LoadState(byte[] state);
        public int CountTokens(string text);
    }
}