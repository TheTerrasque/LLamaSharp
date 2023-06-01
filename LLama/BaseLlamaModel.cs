using System;
using System.Collections.Generic;
using System.Linq;
using LLama.Exceptions;
using LLama.Native;
using LLama.Extensions;
using LLama.Interfaces;
using System.Threading;
using LLama.Models;
using Serilog;
using SerilogTimings.Extensions;

namespace LLama
{
    public class BaseLLamaModel : IDisposable, ILanguageModel
    {
        private ILogger _log;
        ILLamaParams _params;
        private string _encoding;
        SafeLLamaContextHandle _ctx;
        /// <summary>
        /// Model Context length
        /// </summary>
        public int ContextLength { get;  internal set; }
        List<Int32> _token_history = new();
        
        public void Dispose()
        {
            _ctx.Dispose();
        }

        public BaseLLamaModel(ILLamaParams Params, string encoding = "UTF-8") {
            _log = Log.ForContext<BaseLLamaModel>().ForContext("ModelParams", Params);
            _params = Params;
            _encoding = encoding;
            _log.Information("Initializing LLama model with params: {@modelParams}", _params);
            _ctx = Utils.llama_init_from_gpt_params(ref _params);
            ContextLength = NativeApi.llama_n_ctx(_ctx);
        }

        private void _process_tokens(List<Int32> tokens, CancellationToken? ct)
        {
            var log = _log.ForContext("Function","_process_tokens");

            // Find out which tokens are the same as in the previous call
            var skip_tokens = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i >= _token_history.Count)
                {
                    break;
                }
                if (tokens[i] == _token_history[i])
                {
                    skip_tokens++;
                }
            }
            log.Debug("Skipping {skipTokens} tokens out of {tokenCount}. TokenHistory length is {tokenHistoryCount}", 
                skip_tokens, tokens.Count, _token_history.Count);
            
            while (skip_tokens < tokens.Count && (ct == null || ct.Value.IsCancellationRequested == false)) {
                
                log.Debug("Processing batch of max {n_batch} tokens. Tokens: {tokenCount}, skip_tokens: {skipTokens}", 
                    _params.n_batch, tokens.Count, skip_tokens);

                var tokens_to_process = tokens.Skip(skip_tokens).Take(_params.n_batch).ToArray();
                
                using (log.OperationAt(Serilog.Events.LogEventLevel.Debug).Time("Process {tokens} tokens", tokens_to_process.Length))
                {
                    var eval_result = NativeApi.llama_eval(_ctx, tokens_to_process, tokens_to_process.Length, skip_tokens, _params.n_threads);
                    if (eval_result != 0)
                    {
                        log.Error("llama_eval returned non-zero: {llama_eval_return}. Failed to eval.", eval_result);
                        throw new RuntimeError("Failed to eval.");
                    }
                    skip_tokens += tokens_to_process.Length;
                }
            }
            _token_history = tokens.ToList();
        }

        public byte[] SaveState()
        {
            var stateSize = NativeApi.llama_get_state_size(_ctx);
            byte[] stateMemory = new byte[stateSize];
            NativeApi.llama_copy_state_data(_ctx, stateMemory);
            return stateMemory;
        }

        public void LoadState(byte[] stateMemory)
        {
            int stateSize = (int)NativeApi.llama_get_state_size(_ctx);
            if (stateMemory.Length != stateSize)
            {
                throw new RuntimeError("Failed to validate state size.");
            }
            NativeApi.llama_set_state_data(_ctx, stateMemory);
        }

        Int32 _predict_next_token(ILlamaSamplingParams samplingParams) {
            var repeat_last_n = samplingParams.repeat_last_n < 0 ? ContextLength : samplingParams.repeat_last_n;
            var top_k = samplingParams.top_k <= 0 ? NativeApi.llama_n_vocab(_ctx) : samplingParams.top_k;

            Int32 id = 0;

            var model_vocabulary = NativeApi.llama_n_vocab(_ctx);
            var logits = Utils.llama_get_logits(_ctx, model_vocabulary);

            // Apply params.logit_bias map
            foreach (var (key, value) in samplingParams.logit_bias) logits[key] += value;

            // Create candidates
            var candidates = new List<LLamaTokenData>();
            candidates.Capacity = model_vocabulary;
            for (Int32 token_id = 0; token_id < model_vocabulary; token_id++)
            {
                candidates.Add(new LLamaTokenData(token_id, logits[token_id], 0.0f));
            }

            LLamaTokenDataArray candidates_p = new LLamaTokenDataArray(candidates.ToArray(), (ulong)candidates.Count, false);

            // Apply penalties
            float nl_logit = logits[NativeApi.llama_token_nl()];
            var last_n_repeat = Math.Min(Math.Min(_token_history.Count, repeat_last_n), ContextLength);

            var sampling_window = _token_history.Skip(_token_history.Count - last_n_repeat).ToArray();

            SamplingApi.llama_sample_repetition_penalty(_ctx, candidates_p,
                sampling_window, (ulong)last_n_repeat, samplingParams.repeat_penalty);

            SamplingApi.llama_sample_frequency_and_presence_penalties(_ctx, candidates_p,
                sampling_window, (ulong)last_n_repeat, samplingParams.frequency_penalty, samplingParams.presence_penalty);
            
            if (!samplingParams.penalize_nl)
            {
                logits[NativeApi.llama_token_nl()] = nl_logit;
            }

            if (samplingParams.temp <= 0)
            {
                // Greedy sampling
                id = SamplingApi.llama_sample_token_greedy(_ctx, candidates_p);
            }
            else
            {
                if (samplingParams.mirostat == 1)
                {
                    float mirostat_mu = 2.0f * samplingParams.mirostat_tau;
                    const int mirostat_m = 100;
                    SamplingApi.llama_sample_temperature(_ctx, candidates_p, samplingParams.temp);
                    id = SamplingApi.llama_sample_token_mirostat(_ctx, candidates_p, 
                        samplingParams.mirostat_tau, samplingParams.mirostat_eta, 
                        mirostat_m, ref mirostat_mu);
                }
                else if (samplingParams.mirostat == 2)
                {
                    float mirostat_mu = 2.0f * samplingParams.mirostat_tau;
                    SamplingApi.llama_sample_temperature(_ctx, candidates_p, samplingParams.temp);
                    id = SamplingApi.llama_sample_token_mirostat_v2(_ctx, candidates_p, 
                        samplingParams.mirostat_tau, samplingParams.mirostat_eta, ref mirostat_mu);
                }
                else
                {
                    // Temperature sampling
                    SamplingApi.llama_sample_top_k(_ctx, candidates_p, top_k, 1);
                    SamplingApi.llama_sample_tail_free(_ctx, candidates_p, samplingParams.tfs_z, 1);
                    SamplingApi.llama_sample_typical(_ctx, candidates_p, samplingParams.typical_p, 1);
                    SamplingApi.llama_sample_top_p(_ctx, candidates_p, samplingParams.top_p, 1);
                    SamplingApi.llama_sample_temperature(_ctx, candidates_p, samplingParams.temp);
                    id = SamplingApi.llama_sample_token(_ctx, candidates_p);
                }
            }
            return id;
        }

        public IEnumerable<string> Generate(string text, CancellationToken? ct, ILlamaSamplingParams? samplingParams = null) {
            var tokens = Utils.llama_tokenize(_ctx, text, true, _encoding);

            if (samplingParams == null) samplingParams = new LLamaSamplingParams();

            _log.Information("Generating text with params: {@samplingParams}", samplingParams);

            while (ct == null || ct.Value.IsCancellationRequested == false)
            {
                _process_tokens(tokens, ct);
                var next_token = _predict_next_token(samplingParams);
                if (next_token == NativeApi.llama_token_eos()) break;

                tokens.Add(next_token);
                var next_token_text = Utils.PtrToStringUTF8(NativeApi.llama_token_to_str(_ctx, next_token));
                yield return next_token_text;
            }
        }
    }
}