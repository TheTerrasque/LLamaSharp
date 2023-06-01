using System;
using System.Collections.Generic;
namespace LLama.Interfaces
{
    public interface ILlamaSamplingParams
    {
        /// <summary>
        /// logit bias for specific tokens
        /// </summary>
        public Dictionary<Int32, float> logit_bias { get; set; } // logit bias for specific tokens
        /// <summary>
        ///  0 or lower to use vocab size
        /// </summary>
        public int top_k { get; set; } // <= 0 to use vocab size
        /// <summary>
        /// 1.0 = disabled
        /// </summary>
        public float top_p { get; set; } // 1.0 = disabled
        /// <summary>
        /// 1.0 = disabled
        /// </summary>
        public float tfs_z { get; set; } // 1.0 = disabled
        /// <summary>
        /// 1.0 = disabled
        /// </summary>
        public float typical_p { get; set; } // 1.0 = disabled
        /// <summary>
        /// 1.0 = disabled
        /// </summary>
        public float temp { get; set; } // 1.0 = disabled
        /// <summary>
        /// 1.0 = disabled
        /// </summary>
        public float repeat_penalty { get; set; } // 1.0 = disabled
        /// <summary>
        /// last n tokens to penalize (0 = disable penalty, -1 = context size)
        /// </summary>
        public int repeat_last_n { get; set; } // last n tokens to penalize (0 = disable penalty, -1 = context size)
        /// <summary>
        /// frequency penalty coefficient
        /// 0.0 = disabled
        /// </summary>
        public float frequency_penalty { get; set; } // 0.0 = disabled
        /// <summary>
        /// presence penalty coefficient
        /// 0.0 = disabled
        /// </summary>
        public float presence_penalty { get; set; } // 0.0 = disabled
        /// <summary>
        /// Mirostat uses tokens instead of words.
        /// algorithm described in the paper https://arxiv.org/abs/2007.14966.
        /// 0 = disabled, 1 = mirostat, 2 = mirostat 2.0
        /// </summary>
        public int mirostat { get; set; } // 0 = disabled, 1 = mirostat, 2 = mirostat 2.0
        /// <summary>
        /// target entropy
        /// </summary>
        public float mirostat_tau { get; set; } // target entropy
        /// <summary>
        /// learning rate
        /// </summary>
        public float mirostat_eta { get; set; } // learning rate
        /// <summary>
        /// consider newlines as a repeatable token
        /// </summary>
        public bool penalize_nl { get; set; } // consider newlines as a repeatable token
    }
}