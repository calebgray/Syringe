using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SyringeInjection {
	/// <summary>Easily distribute data throughout the app.</summary>
	public static class Syringe<T> {
		private static readonly HashSet<Action<T>> _actions = new HashSet<Action<T>>();
		private static readonly HashSet<Action<T>> _onces = new HashSet<Action<T>>();
		private static readonly Queue<T> _sets = new Queue<T>();
		private static T _data;
		private static bool _dataIsSet;

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="T"/> changes.</summary>
		public static void Subscribe(Action<T> action, bool getValueNow = true) {
			// Trigger the action right now, if the value was already set.
			if (getValueNow && HasValue()) action(_data);

			// Add the Listener
			_actions.Add(action);
		}

		/// <summary>Fire <paramref name="action"/> the next time, and only the next time, <seealso cref="Set(T)"/> is called... Unless <paramref name="getValueNow"/> is true, then execute right now.</summary>
		public static void Once(Action<T> action, bool getValueNow = true) {
			// Trigger the action right now, if the value was already set.
			if (getValueNow && HasValue()) {
				action(_data);
				return;
			}

			// Add the One-Time Listener
			_onces.Add(action);
		}

		/// <summary>Remove <paramref name="action"/> from <seealso cref="T"/>.</summary>
		public static void Unsubscribe(Action<T> action) {
			// Remove the Listener
			_actions.Remove(action);
			_onces.Remove(action);
		}

		/// <summary>Convenience method for calling Set(Get()) <seealso cref="T"/>.</summary>
		public static void Fire() {
			Set(Get());
		}

		/// <summary>Set and store the value of <seealso cref="T"/>.</summary>
		public static void Set(T value) {
			// Add Set to the Queue
			_sets.Enqueue(value);

			// Prevent Nested Set Calls; Only Execute Set Serially
			if (_sets.Count > 1) return;

			// Execute Sets (fifo)
			Exception firstException = null;
			while (_sets.Count > 0) {
				// Set the Value
				_data = _sets.Dequeue();
				_dataIsSet = !EqualityComparer<T>.Default.Equals(_data, default);

				// Trigger the list of actions and onces for this type.
				var actions = new Action<T>[_actions.Count + _onces.Count];
				_actions.CopyTo(actions);
				_onces.CopyTo(actions, _actions.Count);
				_onces.Clear();
				foreach (var action in actions) {
					try {
						action(_data);
					} catch (Exception e) {
						if (firstException == null) {
							firstException = new Exception(action.Method.DeclaringType.FullName + "." + action.Method.Name, e);
						}
					}
				}
			}
			_sets.Clear();
			if (firstException != null) throw firstException;
		}

		/// <summary>Update the value of <seealso cref="T"/>.</summary>
		/// <param name="getAndSet">Execute <paramref name="getAndSet"/> with the value of <seealso cref="T"/> and the <see cref="HasValue"/> result. Returns the updated value.</param>
		public static void Update(Func<T, bool, T> getAndSet) {
			// Add/Overwrite Value
			Set(getAndSet(_data, _dataIsSet));
		}

		/// <summary>Get the state of <seealso cref="T"/>.</summary>
		public static bool HasValue() {
			// Return the State
			return _dataIsSet;
		}

		/// <summary>Get the last known value of <seealso cref="T"/>.</summary>
		public static T Get() {
			// Return the Value
			return _data;
		}

		/// <summary>Remove the last known value of <seealso cref="T"/>.</summary>
		public static void Remove() {
			// Remove the Value
			Set(default);
		}

		/// <summary>Reset <seealso cref="T"/>.</summary>
		public static void Reset() {
			// Reset the Data
			_actions.Clear();
			_onces.Clear();
			Remove();
		}
	}

	/// <summary>Easily distribute data throughout the app.</summary>
	public static class Syringe<T1, T2> {
		private static readonly HashSet<Action<T1, T2>> _actions = new HashSet<Action<T1, T2>>();

		// Simple Action Wrapper
		private static void _handler<T>(T _) {
			foreach (var action in _actions.ToArray()) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get());
			}
		}

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="T1"/> or <seealso cref="T2"/> changes.</summary>
		public static void Subscribe(Action<T1, T2> action, bool getValuesNow = true) {
			// Trigger the Action When Any Value Was Set
			if (getValuesNow && (Syringe<T1>.HasValue() || Syringe<T2>.HasValue())) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get());
			}

			// Add the Action
			_actions.Add(action);

			// Add the Listeners (when adding the first action)
			if (_actions.Count > 1) return;
			Syringe<T1>.Subscribe(_handler, false);
			Syringe<T2>.Subscribe(_handler, false);
		}

		/// <summary>Remove <paramref name="action"/> from <seealso cref="T1"/> and <seealso cref="T2"/>.</summary>
		public static void Unsubscribe(Action<T1, T2> action) {
			// Remove the Action
			_actions.Remove(action);

			// Remove the Listeners (when there are no actions left)
			if (_actions.Count > 0) return;
			Syringe<T1>.Unsubscribe(_handler);
			Syringe<T2>.Unsubscribe(_handler);
		}
	}

	/// <summary>Easily distribute data throughout the app.</summary>
	public static class Syringe<T1, T2, T3> {
		private static readonly HashSet<Action<T1, T2, T3>> _actions = new HashSet<Action<T1, T2, T3>>();

		// Simple Action Wrapper
		private static void _handler<T>(T _) {
			foreach (var action in _actions.ToArray()) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get(), Syringe<T3>.Get());
			}
		}

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="T1"/>, <seealso cref="T2"/>, or <seealso cref="T3"/> changes.</summary>
		public static void Subscribe(Action<T1, T2, T3> action, bool getValuesNow = true) {
			// Trigger the Action When Any Value Was Set
			if (getValuesNow && (Syringe<T1>.HasValue() || Syringe<T2>.HasValue() || Syringe<T3>.HasValue())) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get(), Syringe<T3>.Get());
			}

			// Add the Action
			_actions.Add(action);

			// Add the Listeners (when adding the first action)
			if (_actions.Count > 1) return;
			Syringe<T1>.Subscribe(_handler, false);
			Syringe<T2>.Subscribe(_handler, false);
			Syringe<T3>.Subscribe(_handler, false);
		}

		/// <summary>Remove <paramref name="action"/> from <seealso cref="T1"/>, <seealso cref="T2"/>, and <seealso cref="T3"/>.</summary>
		public static void Unsubscribe(Action<T1, T2, T3> action) {
			// Remove the Action
			_actions.Remove(action);

			// Remove the Listeners (when there are no actions left)
			if (_actions.Count > 0) return;
			Syringe<T1>.Unsubscribe(_handler);
			Syringe<T2>.Unsubscribe(_handler);
			Syringe<T3>.Unsubscribe(_handler);
		}
	}

	/// <summary>Easily distribute data throughout the app.</summary>
	public static class Syringe<T1, T2, T3, T4> {
		private static readonly HashSet<Action<T1, T2, T3, T4>> _actions = new HashSet<Action<T1, T2, T3, T4>>();

		// Simple Action Wrapper
		private static void _handler<T>(T _) {
			foreach (var action in _actions.ToArray()) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get(), Syringe<T3>.Get(), Syringe<T4>.Get());
			}
		}

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="T1"/>, <seealso cref="T2"/>, <seealso cref="T3"/>, or <seealso cref="T4"/> changes.</summary>
		public static void Subscribe(Action<T1, T2, T3, T4> action, bool getValuesNow = true) {
			// Trigger the Action When Any Value Was Set
			if (getValuesNow && (Syringe<T1>.HasValue() || Syringe<T2>.HasValue() || Syringe<T3>.HasValue() || Syringe<T4>.HasValue())) {
				action(Syringe<T1>.Get(), Syringe<T2>.Get(), Syringe<T3>.Get(), Syringe<T4>.Get());
			}

			// Add the Action
			_actions.Add(action);

			// Add the Listeners (when adding the first action)
			if (_actions.Count > 1) return;
			Syringe<T1>.Subscribe(_handler, false);
			Syringe<T2>.Subscribe(_handler, false);
			Syringe<T3>.Subscribe(_handler, false);
			Syringe<T4>.Subscribe(_handler, false);
		}

		/// <summary>Remove <paramref name="action"/> from <seealso cref="T1"/>, <seealso cref="T2"/>, <seealso cref="T3"/>, and <seealso cref="T4"/>.</summary>
		public static void Unsubscribe(Action<T1, T2, T3, T4> action) {
			// Remove the Action
			_actions.Remove(action);

			// Remove the Listeners (when there are no actions left)
			if (_actions.Count > 0) return;
			Syringe<T1>.Unsubscribe(_handler);
			Syringe<T2>.Unsubscribe(_handler);
			Syringe<T3>.Unsubscribe(_handler);
			Syringe<T4>.Unsubscribe(_handler);
		}
	}

	/// <summary>Easily distribute data throughout the app, using the more expensive runtime version.</summary>
	[Serializable]
	public class Syringe : ISerializationCallbackReceiver {
		/******************************************************************************************************************/
		// Bare Minimum Serializable Implementation
		/******************************************************************************************************************/
		private Type _type;
		[SerializeField] private string _typeAssemblyQualifiedName = "";

		public Syringe(Type type) {
			_type = type;
		}

		public static implicit operator Syringe(Type type) {
			return new Syringe(type);
		}

		// Easy Access to Underlying Type
		public string FullName => _type == null ? null : _type.FullName;
		public string FriendlyName => FullName == null ? null : FullName.Substring(FullName.LastIndexOf('.') + 1);
		public Type Type => _type;
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Serialization Implementations
		/******************************************************************************************************************/
		public delegate void SubscribeMethod(Action<object> action, bool getValueNow = true);
		public SubscribeMethod Subscribe;
		public delegate void OnceMethod(Action<object> action, bool getValueNow = true);
		public OnceMethod Once;
		public delegate void UnsubscribeMethod(Action<object> action);
		public UnsubscribeMethod Unsubscribe;
		public delegate void FireMethod();
		public FireMethod Fire;
		public delegate void SetMethod(object value);
		public SetMethod Set;
		/*public delegate void UpdateMethod(Func<object, bool, object> getAndSet);
		public UpdateMethod Update;*/
		public delegate bool HasValueMethod();
		public HasValueMethod HasValue;
		public delegate object GetMethod();
		public GetMethod Get;
		public delegate void RemoveMethod();
		public RemoveMethod Remove;
		public delegate void ResetMethod();
		public ResetMethod Reset;

		/// <inheritdoc />
		public void OnBeforeSerialize() {
			_typeAssemblyQualifiedName = _type == null ? null : _type.AssemblyQualifiedName;
		}

		/// <inheritdoc />
		public void OnAfterDeserialize() {
			// Sanity Check
			if (_typeAssemblyQualifiedName == null) {
				_type = null;
				return;
			}

			try {
				// Find the Type
				_type = Type.GetType(_typeAssemblyQualifiedName);
				if (_type == null) return;

				// (Attempt to) Cache Runtime Reflections
				var staticSyringeType = typeof(Syringe<>).MakeGenericType(_type);
				Subscribe = (SubscribeMethod) Delegate.CreateDelegate(typeof(SubscribeMethod), staticSyringeType.GetMethod("Subscribe"));
				Once = (OnceMethod) Delegate.CreateDelegate(typeof(OnceMethod), staticSyringeType.GetMethod("Once"));
				Unsubscribe = (UnsubscribeMethod) Delegate.CreateDelegate(typeof(UnsubscribeMethod), staticSyringeType.GetMethod("Unsubscribe"));
				Fire = (FireMethod) Delegate.CreateDelegate(typeof(FireMethod), staticSyringeType.GetMethod("Fire"));
				// TODO: Is there a better way to cast T -> object than this?
				var set = staticSyringeType.GetMethod("Set");
				Set = value => set.Invoke(null, new[]{ value });
				//Update = (UpdateMethod) Delegate.CreateDelegate(typeof(UpdateMethod), staticSyringeType.GetMethod("HasValue"));
				HasValue = (HasValueMethod) Delegate.CreateDelegate(typeof(HasValueMethod), staticSyringeType.GetMethod("HasValue"));
				Get = (GetMethod) Delegate.CreateDelegate(typeof(GetMethod), staticSyringeType.GetMethod("Get"));
				Remove = (RemoveMethod) Delegate.CreateDelegate(typeof(RemoveMethod), staticSyringeType.GetMethod("Remove"));
				Reset = (ResetMethod) Delegate.CreateDelegate(typeof(ResetMethod), staticSyringeType.GetMethod("Reset"));
			} catch (Exception) {
				// (Fallback to) Runtime Version
				Subscribe = RuntimeSubscribe;
				Once = RuntimeOnce;
				Unsubscribe = RuntimeUnsubscribe;
				Fire = RuntimeFire;
				Set = RuntimeSet;
				//Update = RuntimeUpdate;
				HasValue = RuntimeHasValue;
				Get = RuntimeGet;
				Remove = RuntimeRemove;
				Reset = RuntimeReset;
			}
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Runtime Implementation
		/******************************************************************************************************************/
		private static readonly Dictionary<Type, HashSet<Action<object>>> _actions = new Dictionary<Type, HashSet<Action<object>>>();
		private static readonly Dictionary<Type, HashSet<Action<object>>> _onces = new Dictionary<Type, HashSet<Action<object>>>();
		private static readonly Dictionary<Type, Queue<object>> _sets = new Dictionary<Type, Queue<object>>();
		private static readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="Set(T)"/> is called.</summary>
		private void RuntimeSubscribe(Action<object> action, bool getValueNow = true) {
			// Trigger the action right now, if the value was already set.
			if (getValueNow && _data.ContainsKey(_type)) action(RuntimeGet());

			// Add the Listener (and create the List of listeners if it didn't exist.)
			HashSet<Action<object>> actionList;
			if (!_actions.TryGetValue(_type, out actionList)) {
				_actions.Add(_type, actionList = new HashSet<Action<object>>());
			}
			actionList.Add(action);
		}

		/// <summary>Fire <paramref name="action"/> the next time, and only the next time, <seealso cref="Set(T)"/> is called... Unless <paramref name="getValueNow"/> is true, then execute right now.</summary>
		private void RuntimeOnce(Action<object> action, bool getValueNow = true) {
			// Trigger the action right now, if the value was already set.
			if (getValueNow && RuntimeHasValue()) {
				action(RuntimeGet());
				return;
			}

			// Add the Listener (and create the List of listeners if it didn't exist.)
			HashSet<Action<object>> onceList;
			if (!_onces.TryGetValue(_type, out onceList)) {
				_onces.Add(_type, onceList = new HashSet<Action<object>>());
			}
			onceList.Add(action);
		}

		/// <summary>Remove <paramref name="action"/> from <seealso cref="T"/>.</summary>
		private void RuntimeUnsubscribe(Action<object> action) {
			// Remove the Listener
			HashSet<Action<object>> actionList;
			if (_actions.TryGetValue(_type, out actionList)) actionList.Remove(action);

			// Remove the Listener
			HashSet<Action<object>> onceList;
			if (_onces.TryGetValue(_type, out onceList)) onceList.Remove(action);
		}

		/// <summary>Convenience method for calling Set(Get()) <seealso cref="T"/>.</summary>
		private void RuntimeFire() {			
			// Return the State
			RuntimeSet(RuntimeGet());
		}

		/// <summary>Set and store the value of <seealso cref="T"/>.</summary>
		private void RuntimeSet(object value) {
			// Add Set to the Queue
			if (!_sets.TryGetValue(_type, out var sets)) {
				sets = new Queue<object>();
				_sets[_type] = sets;
			}
			sets.Enqueue(value);

			// Prevent Nested Set Calls; Only Execute Set Serially
			if (sets.Count > 1) return;

			// Execute Sets (fifo)
			Exception firstException = null;
			string firstExceptionName = null;
			int exceptionCounter = 0;
			while (sets.Count > 0) {
				// Set the Value
				value = sets.Dequeue();
				if (value == null) {
					_data.Remove(_type);
				} else {
					_data[_type] = value;
				}

				// Get the list of actions for this type.
				HashSet<Action<object>> onceList = null;
				if (!_actions.TryGetValue(_type, out var actionList) && !_onces.TryGetValue(_type, out onceList)) {
					// Nothing to do?
					sets.Clear();
					return;
				}

				// Trigger the list of actions for this type.
				var actions = new List<Action<object>>();
				if (actionList != null) actions.AddRange(actionList);
				if (onceList != null) {
					actions.AddRange(onceList);
					onceList.Clear();
				}
				foreach (var action in actions) {
					try {
						action(value);
					} catch (Exception e) {
						++exceptionCounter;
						if (firstException == null) {
							firstExceptionName = $"{action.Method.DeclaringType.FullName}.{action.Method.Name}";
							firstException = e;
						}
					}
				}
			}
			if (firstException != null) throw new Exception($"Encountered {exceptionCounter} Exceptions! Only Throwing the First Exception: {firstExceptionName}", firstException);
		}

		/*/// <summary>Update the value of <seealso cref="T"/>.</summary>
		/// <param name="getAndSet">Execute <paramref name="getAndSet"/> with the value of <seealso cref="T"/> and the <see cref="HasValue"/> result. Returns the updated value.</param>
		private void RuntimeUpdate(Func<object, bool, object> getAndSet) {
			// Add/Overwrite Value
			RuntimeSet(getAndSet(RuntimeGet(), RuntimeHasValue()));
		}*/

		/// <summary>Get the state of <seealso cref="T"/>.</summary>
		private bool RuntimeHasValue() {			
			// Return the State
			return _data.ContainsKey(_type);
		}

		/// <summary>Get the last known value of <seealso cref="T"/>.</summary>
		private object RuntimeGet() {			
			// Return the Value
			_data.TryGetValue(_type, out var value);
			return value;
		}

		/// <summary>Remove the last known value of <seealso cref="T"/>.</summary>
		private void RuntimeRemove() {
			// Remove the Value
			_data.Remove(_type);

			// Broadcast the Removed Value
			RuntimeSet(null);
		}

		/// <summary>Reset <seealso cref="T"/>.</summary>
		private void RuntimeReset() {
			// Reset the Data
			_actions[_type].Clear();
			_onces[_type].Clear();
			RuntimeRemove();
		}
		/******************************************************************************************************************/
	}
}
