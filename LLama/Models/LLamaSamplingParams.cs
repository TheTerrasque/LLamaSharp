using LLama.Interfaces;
using System.Collections.Generic;
using System;

namespace LLama.Models
{
    public class LLamaSamplingParams : ILlamaSamplingParams
    {
        public Dictionary<Int32, float> logit_bias { get; set; } = new Dictionary<int, float>(); // logit bias for specific tokens
        public int top_k { get; set; } = 40; // <= 0 to use vocab size
        public float top_p { get; set; } = 0.95f; // 1.0 = disabled
        public float tfs_z { get; set; } = 1.00f; // 1.0 = disabled
        public float typical_p { get; set; } = 1.00f; // 1.0 = disabled
        public float temp { get; set; } = 0.80f; // 1.0 = disabled
        public float repeat_penalty { get; set; }= 1.10f; // 1.0 = disabled
        public int repeat_last_n { get; set; }= 64; // last n tokens to penalize (0 = disable penalty, -1 = context size)
        public float frequency_penalty { get; set; }= 0.00f; // 0.0 = disabled
        public float presence_penalty { get; set; }= 0.00f; // 0.0 = disabled
        public int mirostat { get; set; }= 0; // 0 = disabled, 1 = mirostat, 2 = mirostat 2.0
        public float mirostat_tau { get; set; }= 5.00f; // target entropy
        public float mirostat_eta { get; set; }= 0.10f; // learning rate
        public bool penalize_nl { get; set; } = true;
    }
}