using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MCMicroLauncher.ApplicationState
{
    internal class StateMachine<TStates, TTriggers>
        : IDictionary<(TStates, TTriggers), TStates>, IDisposable
        where TStates : Enum
        where TTriggers : Enum
    {
        private TStates internalState;

        private readonly IDictionary<TStates, IList<Func<Task>>> onEntry
            = new Dictionary<TStates, IList<Func<Task>>>();

        private readonly IDictionary<TStates, IList<Func<Task>>> onLeave
            = new Dictionary<TStates, IList<Func<Task>>>();

        private readonly IDictionary<(TStates, TTriggers), TStates> transitions
            = new Dictionary<(TStates, TTriggers), TStates>();

        private readonly ChannelWriter<TTriggers> triggerQueue;

        public StateMachine()
        {
            this.internalState = default;

            var channel = Channel.CreateUnbounded<TTriggers>();
            var reader = channel.Reader;
            this.triggerQueue = channel.Writer;

            // fire and forget
            _ = this.RunWorkerLoopAsync(reader);
        }

        private async Task RunWorkerLoopAsync(
            ChannelReader<TTriggers> reader)
        {
            await foreach (var trigger in reader.ReadAllAsync())
            {
                try
                {
                    await HandleTrigger(trigger);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"Handler crashed, skipping trigger ({this.internalState}->{trigger})",
                        ex);
                }
            }
        }

        private async Task HandleTrigger(TTriggers trigger)
        {
            Log.Info("Processing trigger", $"{this.internalState} => {trigger}");

            if (!this.transitions
                .TryGetValue((this.internalState, trigger), out var newState))
            {
                Log.Warn("Transition not found", $"{this.internalState} => {trigger}");
                return;
            }

            Log.Info("Processing state change", $"{this.internalState} => {trigger} => {newState}");

            if (this.onLeave.TryGetValue(this.internalState, out var leaveActions))
            {
                Log.Info("Processing leave handlers", this.internalState.ToString());

                foreach (var action in leaveActions)
                {
                    await action();
                }
            }

            this.internalState = newState;

            if (this.onEntry.TryGetValue(newState, out var entryActions))
            {
                Log.Info("Processing entry handlers", this.internalState.ToString());

                foreach (var action in entryActions)
                {
                    await action();
                }
            }

            Log.Info("State change done", this.internalState.ToString());
        }

        internal void OnEntry(TStates state, Func<Task> action)
        {
            if (!this.onEntry.TryGetValue(state, out var list))
            {
                this.onEntry[state] = list = new List<Func<Task>>();
            }

            list.Add(action);
        }

        internal void OnEntry(TStates state, Action action)
        {
            OnEntry(state, () => { action(); return Task.CompletedTask; });
        }

        internal void OnLeave(TStates state, Func<Task> action)
        {
            if (!this.onLeave.TryGetValue(state, out var list))
            {
                this.onLeave[state] = list = new List<Func<Task>>();
            }

            list.Add(action);
        }

        internal void OnLeave(TStates state, Action action)
        {
            OnLeave(state, () => { action(); return Task.CompletedTask; });
        }

        internal void AddTransition(TStates state, TTriggers trigger, TStates newState)
        {
            this.transitions[(state, trigger)] = newState;
        }

        internal void Call(TTriggers trigger)
        {
            this.triggerQueue.TryWrite(trigger);
        }

        #region [ IDisposable implementation ]

        public void Dispose()
        {
            // close channel on dispose to end polling task
            this.triggerQueue.TryComplete();
        }

        #endregion

        #region [ IDictionary implementation ]

        public ICollection<(TStates, TTriggers)> Keys => transitions.Keys;

        public ICollection<TStates> Values => transitions.Values;

        public int Count => transitions.Count;

        public bool IsReadOnly => transitions.IsReadOnly;

        public TStates this[(TStates, TTriggers) key]
        {
            get => transitions[key];
            set => transitions[key] = value;
        }

        public void Add((TStates, TTriggers) key, TStates value)
        {
            transitions.Add(key, value);
        }

        public bool ContainsKey((TStates, TTriggers) key)
        {
            return transitions.ContainsKey(key);
        }

        public bool Remove((TStates, TTriggers) key)
        {
            return transitions.Remove(key);
        }

        public bool TryGetValue((TStates, TTriggers) key, [MaybeNullWhen(false)] out TStates value)
        {
            return transitions.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<(TStates, TTriggers), TStates> item)
        {
            transitions.Add(item);
        }

        public void Clear()
        {
            transitions.Clear();
        }

        public bool Contains(KeyValuePair<(TStates, TTriggers), TStates> item)
        {
            return transitions.Contains(item);
        }

        public void CopyTo(KeyValuePair<(TStates, TTriggers), TStates>[] array, int arrayIndex)
        {
            transitions.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<(TStates, TTriggers), TStates> item)
        {
            return transitions.Remove(item);
        }

        public IEnumerator<KeyValuePair<(TStates, TTriggers), TStates>> GetEnumerator()
        {
            return transitions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)transitions).GetEnumerator();
        }

        #endregion
    }
}
