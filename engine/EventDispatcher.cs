using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.WPF
{
    // ══════════════════════════════════════════════════════════════════════════════════
    // EVENT DISPATCHER - Engine Heart ❤️
    // Responsible for:
    // 1. Event propagation (Capturing → AtTarget → Bubbling)
    // 2. Event delegation
    // 3. Event queuing and processing
    // ══════════════════════════════════════════════════════════════════════════════════

    public class EventDispatcher
    {
        // Event listeners: (elementId) → [(eventType) → [listeners]]
        private Dictionary<string, Dictionary<string, List<EventListener>>> _listeners 
            = new Dictionary<string, Dictionary<string, List<EventListener>>>();

        // Event queue for processing
        private Queue<NimbusEvent> _eventQueue = new Queue<NimbusEvent>();
        
        // Global event counter for debugging
        private ulong _eventCounter = 0;
        
        // Enable logging
        private bool _debugLogging = false;

        /// <summary>Constructor</summary>
        public EventDispatcher(bool debugLogging = false)
        {
            _debugLogging = debugLogging;
        }

        // ══════════════════════════════════════════════════════════════════════════════════
        // EVENT LISTENING
        // ══════════════════════════════════════════════════════════════════════════════════

        /// <summary>Add event listener to element</summary>
        public void AddEventListener(IUIModule element, string eventType, EventListener handler)
        {
            if (element == null || string.IsNullOrEmpty(eventType) || handler == null)
                return;

            if (!_listeners.ContainsKey(element.Id))
                _listeners[element.Id] = new Dictionary<string, List<EventListener>>();

            if (!_listeners[element.Id].ContainsKey(eventType))
                _listeners[element.Id][eventType] = new List<EventListener>();

            _listeners[element.Id][eventType].Add(handler);

            if (_debugLogging)
                Console.WriteLine($"[EVENT] Listener added: {element.Id} @ {eventType}");
        }

        /// <summary>Remove event listener from element</summary>
        public void RemoveEventListener(IUIModule element, string eventType, EventListener handler)
        {
            if (element == null || string.IsNullOrEmpty(eventType))
                return;

            if (_listeners.ContainsKey(element.Id) && _listeners[element.Id].ContainsKey(eventType))
            {
                _listeners[element.Id][eventType].Remove(handler);

                if (_listeners[element.Id][eventType].Count == 0)
                {
                    _listeners[element.Id].Remove(eventType);
                    if (_listeners[element.Id].Count == 0)
                        _listeners.Remove(element.Id);
                }
            }
        }

        /// <summary>Remove all listeners from element</summary>
        public void RemoveAllListeners(IUIModule element, string eventType = null)
        {
            if (element == null) return;

            if (eventType != null)
            {
                if (_listeners.ContainsKey(element.Id) && _listeners[element.Id].ContainsKey(eventType))
                {
                    _listeners[element.Id].Remove(eventType);
                }
            }
            else
            {
                if (_listeners.ContainsKey(element.Id))
                    _listeners.Remove(element.Id);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════════════
        // EVENT DISPATCHING & PROPAGATION
        // ══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Dispatch event with full propagation (capturing → target → bubbling)
        /// </summary>
        public void DispatchEvent(NimbusEvent evt, bool useCapturing = false)
        {
            if (evt == null || evt.Target == null)
                return;

            _eventCounter++;
            ulong eventId = _eventCounter;

            if (_debugLogging)
                Console.WriteLine($"\n[EVENT #{eventId}] START: {evt}");

            // Build the propagation path from target to root
            var path = BuildEventPath(evt.Target);

            // PHASE 1: CAPTURING (root → target)
            if (useCapturing && path.Count > 1)
            {
                if (_debugLogging) Console.WriteLine($"[EVENT #{eventId}] PHASE: Capturing (parent → target)");
                
                for (int i = path.Count - 1; i > 0; i--)
                {
                    if (evt.ImmediatePropagationStopped)
                        break;

                    evt.Phase = EventPhase.Capturing;
                    evt.CurrentTarget = path[i];
                    InvokeListeners(evt, path[i]);
                }
            }

            // PHASE 2: AT TARGET
            if (!evt.ImmediatePropagationStopped)
            {
                if (_debugLogging) Console.WriteLine($"[EVENT #{eventId}] PHASE: AtTarget");
                
                evt.Phase = EventPhase.AtTarget;
                evt.CurrentTarget = evt.Target;
                InvokeListeners(evt, evt.Target);
            }

            // PHASE 3: BUBBLING (target → root)
            if (!evt.ImmediatePropagationStopped)
            {
                if (_debugLogging) Console.WriteLine($"[EVENT #{eventId}] PHASE: Bubbling (target → parent)");
                
                evt.Phase = EventPhase.Bubbling;
                for (int i = 1; i < path.Count; i++)
                {
                    if (evt.ImmediatePropagationStopped)
                        break;

                    evt.CurrentTarget = path[i];
                    InvokeListeners(evt, path[i]);
                }
            }

            if (_debugLogging)
                Console.WriteLine($"[EVENT #{eventId}] END: {evt.Type}");
        }

        /// <summary>
        /// Dispatch event SYNCHRONOUSLY without queuing
        /// Use for immediate event handling
        /// </summary>
        public void DispatchEventImmediate(NimbusEvent evt, bool useCapturing = false)
        {
            DispatchEvent(evt, useCapturing);
        }

        /// <summary>
        /// Queue event for later processing
        /// Useful for batching high-frequency events (mousemove, etc.)
        /// </summary>
        public void QueueEvent(NimbusEvent evt)
        {
            if (evt != null)
                _eventQueue.Enqueue(evt);
        }

        /// <summary>
        /// Process all queued events
        /// </summary>
        public void ProcessQueuedEvents()
        {
            while (_eventQueue.Count > 0)
            {
                var evt = _eventQueue.Dequeue();
                DispatchEvent(evt);
            }
        }

        /// <summary>
        /// Get count of queued events
        /// </summary>
        public int GetQueuedEventCount()
        {
            return _eventQueue.Count;
        }

        // ══════════════════════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ══════════════════════════════════════════════════════════════════════════════════

        /// <summary>Build event propagation path (target to root)</summary>
        private List<IUIModule> BuildEventPath(IUIModule element)
        {
            var path = new List<IUIModule>();
            IUIModule current = element;

            while (current != null)
            {
                path.Insert(0, current);  // Insert at beginning to get root-first order
                current = current.Parent;
            }

            return path;
        }

        /// <summary>Invoke all listeners for an event at a specific element</summary>
        private void InvokeListeners(NimbusEvent evt, IUIModule element)
        {
            if (element == null || string.IsNullOrEmpty(element.Id))
                return;

            if (!_listeners.ContainsKey(element.Id))
                return;

            if (!_listeners[element.Id].ContainsKey(evt.Type))
                return;

            // Get listeners (make a copy in case listener list is modified during invocation)
            var listeners = new List<EventListener>(_listeners[element.Id][evt.Type]);

            foreach (var listener in listeners)
            {
                try
                {
                    if (_debugLogging)
                        Console.WriteLine($"  [EVENT] Invoking listener for {element.Id}.{evt.Type} (phase={evt.Phase})");

                    listener?.Invoke(evt);
                }
                catch (Exception ex)
                {
                    if (_debugLogging)
                        Console.WriteLine($"  [ERROR] Listener exception: {ex.Message}");
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════════════════
        // DEBUGGING & STATISTICS
        // ══════════════════════════════════════════════════════════════════════════════════

        /// <summary>Get total number of listeners registered</summary>
        public int GetTotalListenerCount()
        {
            return _listeners.Values.Sum(dict => dict.Values.Sum(list => list.Count));
        }

        /// <summary>Get listeners for specific element and event type</summary>
        public int GetListenerCount(IUIModule element, string eventType)
        {
            if (element == null || string.IsNullOrEmpty(eventType))
                return 0;

            if (!_listeners.ContainsKey(element.Id) || !_listeners[element.Id].ContainsKey(eventType))
                return 0;

            return _listeners[element.Id][eventType].Count;
        }

        /// <summary>Print debug info about all listeners</summary>
        public void PrintDebugInfo()
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("EVENT DISPATCHER DEBUG INFO");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"Total Events Dispatched: {_eventCounter}");
            Console.WriteLine($"Queued Events: {_eventQueue.Count}");
            Console.WriteLine($"Total Listeners Registered: {GetTotalListenerCount()}");
            Console.WriteLine("\nListeners by Element:");

            foreach (var kvp in _listeners)
            {
                Console.WriteLine($"\n  [{kvp.Key}]");
                foreach (var typeKvp in kvp.Value)
                {
                    Console.WriteLine($"    - {typeKvp.Key}: {typeKvp.Value.Count} listener(s)");
                }
            }

            Console.WriteLine("\n═══════════════════════════════════════════════════════════\n");
        }

        /// <summary>Enable/disable debug logging</summary>
        public void SetDebugLogging(bool enabled)
        {
            _debugLogging = enabled;
        }
    }
}
