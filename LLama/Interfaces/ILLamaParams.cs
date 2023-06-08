
namespace LLama.Interfaces
{
    public interface ILLamaParams
    {
        /// <summary>
        /// Model context size
        /// </summary>
        public int n_ctx { get; set; }
        /// <summary>
        /// Number of layers to run in VRAM / GPU memory
        /// </summary>
        public int n_gpu_layers { get; set; }
        /// <summary>
        /// Seed for the random number generator
        /// </summary>
        public int seed { get; set; }
        /// <summary>
        /// Use f16 instead of f32 for memory kv
        /// </summary>
        public bool memory_f16 { get; set; }
        /// <summary>
        /// Use mmap for faster loads
        /// </summary>
        public bool use_mmap { get; set; }
        /// <summary>
        /// Use mlock to keep model in memory
        /// </summary>
        public bool use_mlock { get; set; }
        /// <summary>
        /// Compute perplexity over the prompt
        /// </summary>
        public bool perplexity { get; set; }
        /// <summary>
        /// Get only sentence embedding
        /// </summary>
        public bool embedding { get; set; }
        /// <summary>
        /// Model path
        /// </summary>
        public string model { get; set; }
        /// <summary>
        /// lora adapter path
        /// </summary>
        public string lora_adapter { get; set; }
        /// <summary>
        /// base model path for the lora adapter
        /// </summary>
        public string lora_base { get; set; }
        /// <summary>
        /// Number of threads (-1 = autodetect)
        /// </summary>
        public int n_threads { get; set; }
        /// <summary>
        /// batch size for prompt processing (must be >=32 to use BLAS)
        /// </summary>
        public int n_batch { get; set; }

        /// <summary>
        /// Treat EOS token as a newline instead of end of text
        /// </summary>
        public bool eos_to_newline { get; set; }
    }
}