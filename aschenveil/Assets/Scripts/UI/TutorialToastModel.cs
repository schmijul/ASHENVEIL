using System;
using System.Collections.Generic;

namespace Ashenveil.UI
{
    /// <summary>
    /// Pure queue for one-shot German tutorial hints. Each hint id shows at most once
    /// (dedupe), hints display for a fixed duration, and only one shows at a time.
    /// Referenced GDD section: Demo-Ablauf tutorials.
    /// </summary>
    public sealed class TutorialToastModel
    {
        private readonly float _displayDuration;
        private readonly Queue<Toast> _pending = new Queue<Toast>();
        private readonly HashSet<string> _shown = new HashSet<string>();
        private Toast? _current;
        private float _remaining;

        /// <summary>
        /// Raised when a new toast becomes visible. Args: German text.
        /// </summary>
        public event Action<string> ToastShown;

        /// <summary>
        /// Raised when the current toast expires.
        /// </summary>
        public event Action ToastHidden;

        /// <summary>
        /// Creates a toast model.
        /// </summary>
        public TutorialToastModel(float displayDuration)
        {
            _displayDuration = displayDuration > 0f ? displayDuration : 4f;
        }

        /// <summary>
        /// Currently visible toast text, or null.
        /// </summary>
        public string CurrentText => _current?.Text;

        /// <summary>
        /// Enqueues a hint. Ignored if that id was already shown or is already queued.
        /// </summary>
        public void Enqueue(string id, string germanText)
        {
            if (string.IsNullOrEmpty(id) || _shown.Contains(id))
            {
                return;
            }

            foreach (Toast t in _pending)
            {
                if (t.Id == id)
                {
                    return;
                }
            }

            if (_current.HasValue && _current.Value.Id == id)
            {
                return;
            }

            _pending.Enqueue(new Toast(id, germanText));
            TryPromote();
        }

        /// <summary>
        /// Advances the display timer, expiring the current toast and promoting the next.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!_current.HasValue)
            {
                TryPromote();
                return;
            }

            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                _current = null;
                ToastHidden?.Invoke();
                TryPromote();
            }
        }

        private void TryPromote()
        {
            if (_current.HasValue || _pending.Count == 0)
            {
                return;
            }

            Toast next = _pending.Dequeue();
            _current = next;
            _shown.Add(next.Id);
            _remaining = _displayDuration;
            ToastShown?.Invoke(next.Text);
        }

        private readonly struct Toast
        {
            public Toast(string id, string text)
            {
                Id = id;
                Text = text;
            }

            public string Id { get; }
            public string Text { get; }
        }
    }
}
