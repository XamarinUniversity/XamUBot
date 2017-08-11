using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XamUBot.Dialogs
{
    [Serializable]
    public class FuzzyPromptOptions<T> : PromptOptions<T>
    {
        public IReadOnlyList<T> ValidOptions { get; private set; }

        public static FuzzyPromptOptions<T> Create(
            string prompt, string retry = "", string tooManyAttempts = "",
            IReadOnlyList<T> options = null, IReadOnlyList<T> validOptions = null,
            int attempts = 3, PromptStyler promptStyler = null)
        {
            return new FuzzyPromptOptions<T>(prompt, retry,
                tooManyAttempts, options, validOptions, attempts, promptStyler);
        }

        public FuzzyPromptOptions(string prompt, string retry = "", string tooManyAttempts = "", 
            IReadOnlyList<T> options = null, IReadOnlyList<T> validOptions = null,
            int attempts = 3, PromptStyler promptStyler = null)
        : base(prompt, retry, tooManyAttempts, options, attempts, promptStyler)
        {
            this.ValidOptions = validOptions ?? options;
        }
    }

    [Serializable]
    public class FuzzyPromptDialog<T> : PromptDialog.PromptChoice<T>
    {
        protected readonly FuzzyPromptOptions<T> PromptOptions;

        public FuzzyPromptDialog(FuzzyPromptOptions<T> promptOptions)
            : base(promptOptions)
        {
            this.PromptOptions = promptOptions;
        }

        public FuzzyPromptDialog(IEnumerable<T> displayOptions, IEnumerable<T> validOptions,
            string prompt, string retry, string tooManyAttempts, int attempts, 
            PromptStyle promptStyle = PromptStyle.Auto)
            : this(new FuzzyPromptOptions<T>(prompt, retry, tooManyAttempts,
                    options: displayOptions.ToList(), 
                    attempts: attempts, 
                    validOptions: validOptions.ToList(),
                    promptStyler: new PromptStyler(promptStyle)))
        {
        }

        public static void Choice(IDialogContext context, ResumeAfter<T> resume, 
            IEnumerable<T> options, IEnumerable<T> validOptions,
            string prompt, string retry = null, string tooManyAttempts = null, 
            int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto)
        {
            Choice(context, resume, new FuzzyPromptOptions<T>(
                prompt, retry, tooManyAttempts,
                attempts: attempts, options: options.ToList(), 
                validOptions: validOptions.ToList(),
                promptStyler: new PromptStyler(promptStyle)));
        }

        public static void Choice(IDialogContext context, ResumeAfter<T> resume, FuzzyPromptOptions<T> promptOptions)
        {
            var child = new FuzzyPromptDialog<T>(promptOptions);
            context.Call(child, resume);
        }

        protected override bool TryParse(IMessageActivity message, out T result)
        {
            if (PromptOptions.ValidOptions != null && message?.Text != null)
            {
                var entry = PromptOptions.ValidOptions.FirstOrDefault(term =>
                    string.Equals(term.ToString(), message.Text.Replace(" ", ""),
                        StringComparison.CurrentCultureIgnoreCase));
                if (entry != null)
                {
                    result = entry;
                    return true;
                }
            }

            return base.TryParse(message, out result);
        }
    }
}