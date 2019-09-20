using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
				_dataIsSet = !EqualityComparer<T>.Default.Equals(_data, default(T));

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
			Set(default(T));
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

		public string FullName {
			get { return _type == null ? null : _type.FullName; }
		}

		public string FriendlyName {
			get { return FullName == null ? null : FullName.Substring(FullName.LastIndexOf('.') + 1); }
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Serialization Implementations
		/******************************************************************************************************************/
		// Implementation Pointers (Static or Runtime)
		private Action<Action<object>, bool> _subscribe;
		private Action<Action<object>, bool> _once;
		public Action<Action<object>> Unsubscribe { get; private set; }
		public Action Fire { get; private set; }
		public Action<object> Set { get; private set; }
		public Action<Func<object, bool, object>> Update { get; private set; }
		public Func<bool> HasValue { get; private set; }
		public Func<object> Get { get; private set; }
		public Func<bool> Remove { get; private set; }
		public Action Reset { get; private set; }

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

				// (Attempt to) Cache Reflections
				var staticSyringeType = typeof(Syringe<>).MakeGenericType(_type);
				_staticActionType = typeof(Action<>).MakeGenericType(_type);
				_staticUpdateType = typeof(Func<,,>).MakeGenericType(_type, typeof(bool), _type);
				_staticMethodSubscribe = staticSyringeType.GetMethod("Subscribe");
				_staticMethodOnce = staticSyringeType.GetMethod("Once");
				_staticMethodUnsubscribe = staticSyringeType.GetMethod("Unsubscribe");
				_staticMethodFire = staticSyringeType.GetMethod("Fire");
				_staticMethodSet = staticSyringeType.GetMethod("Set");
				_staticMethodUpdate = staticSyringeType.GetMethod("Update");
				_staticMethodHasValue = staticSyringeType.GetMethod("HasValue");
				_staticMethodGet = staticSyringeType.GetMethod("Get");
				_staticMethodRemove = staticSyringeType.GetMethod("Remove");
				_staticMethodReset = staticSyringeType.GetMethod("Reset");
				_subscribe = StaticSubscribe;
				_once = StaticOnce;
				Unsubscribe = StaticUnsubscribe;
				Fire = StaticFire;
				Set = StaticSet;
				Update = StaticUpdate;
				HasValue = StaticHasValue;
				Get = StaticGet;
				Remove = StaticRemove;
				Reset = StaticReset;
			} catch (Exception e) {
				Debug.LogException(e);
				_subscribe = RuntimeSubscribe;
				_once = RuntimeOnce;
				Unsubscribe = RuntimeUnsubscribe;
				Fire = RuntimeFire;
				Set = RuntimeSet;
				Update = RuntimeUpdate;
				HasValue = RuntimeHasValue;
				Get = RuntimeGet;
				Remove = RuntimeRemove;
				Reset = RuntimeReset;
			}
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Method Overload Helpers
		/******************************************************************************************************************/
		/// <summary>Fire <paramref name="action"/> every time <seealso cref="Set(T)"/> is called.</summary>
		public void Subscribe(Action<object> action, bool getValueNow = true) {
			_subscribe(action, getValueNow);
		}

		/// <summary>Fire <paramref name="action"/> the next time, and only the next time, <seealso cref="Set(T)"/> is called... Unless <paramref name="getValueNow"/> is true, then execute right now.</summary>
		public void Once(Action<object> action, bool getValueNow = true) {
			_once(action, getValueNow);
		}
		/******************************************************************************************************************/

		/******************************************************************************************************************/
		// Mixed Static/Runtime Implementations
		/******************************************************************************************************************/
		// Reflection Cache
		private Type _staticActionType;
		private Type _staticUpdateType;
		private MethodInfo _staticMethodSubscribe;
		private MethodInfo _staticMethodOnce;
		private MethodInfo _staticMethodUnsubscribe;
		private MethodInfo _staticMethodFire;
		private MethodInfo _staticMethodSet;
		private MethodInfo _staticMethodUpdate;
		private MethodInfo _staticMethodHasValue;
		private MethodInfo _staticMethodGet;
		private MethodInfo _staticMethodRemove;
		private MethodInfo _staticMethodReset;

		/// <summary>Fire <paramref name="action"/> every time <seealso cref="Set"/> is called.</summary>
		private void StaticSubscribe(Action<object> action, bool getValueNow = true) {
			_staticMethodSubscribe.Invoke(null, new[] { (object) Delegate.CreateDelegate(_staticActionType, action.Target, action.Method), (object) getValueNow });
		}

		/// <summary>Fire <paramref name="action"/> the next time, and only the next time, <seealso cref="Set"/> is called... Unless <paramref name="getValueNow"/> is true, then execute right now.</summary>
		private void StaticOnce(Action<object> action, bool getValueNow = true) {
			_staticMethodOnce.Invoke(null, new[] { (object) Delegate.CreateDelegate(_staticActionType, action.Target, action.Method), (object) getValueNow });
		}

		/// <summary>Remove <paramref name="action"/>.</summary>
		private void StaticUnsubscribe(Action<object> action) {
			_staticMethodUnsubscribe.Invoke(null, new[] { (object) Delegate.CreateDelegate(_staticActionType, action.Target, action.Method) });
		}

		/// <summary>Convenience method for calling Set(Get()) <seealso cref="T"/>.</summary>
		private void StaticFire() {
			_staticMethodFire.Invoke(null, null);
		}

		/// <summary>Set and store a value.</summary>
		private void StaticSet(object value) {
			_staticMethodSet.Invoke(null, new[] { (object) value });
		}

		/// <summary>Update the value of <seealso cref="T"/>.</summary>
		/// <param name="getAndSet">Execute <paramref name="getAndSet"/> with the value of <seealso cref="T"/> and the <see cref="HasValue"/> result. Returns the updated value.</param>
		private void StaticUpdate(Func<object, bool, object> getAndSet) {
			_staticMethodUpdate.Invoke(null, new[] { (object) Delegate.CreateDelegate(_staticUpdateType, getAndSet.Target, getAndSet.Method) });
		}

		/// <summary>Get the last state.</summary>
		private bool StaticHasValue() {
			return (bool) _staticMethodHasValue.Invoke(null, null);
		}

		/// <summary>Get the last known value.</summary>
		private object StaticGet() {
			return _staticMethodGet.Invoke(null, null);
		}

		/// <summary>Remove the last known value.</summary>
		private bool StaticRemove() {
			return (bool) _staticMethodRemove.Invoke(null, null);
		}

		/// <summary>Reset actions and value.</summary>
		private void StaticReset() {
			_staticMethodReset.Invoke(null, null);
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
			Queue<object> sets;
			if (!_sets.TryGetValue(_type, out sets)) {
				sets = new Queue<object>();
				_sets[_type] = sets;
			}
			sets.Enqueue(value);

			// Prevent Nested Set Calls; Only Execute Set Serially
			if (sets.Count > 1) return;

			// Execute Sets (fifo)
			Exception firstException = null;
			while (sets.Count > 0) {
				// Set the Value
				value = sets.Dequeue();
				if (value == null) {
					_data.Remove(_type);
				} else {
					_data[_type] = value;
				}

				// Get the list of actions for this type.
				HashSet<Action<object>> actionList = null;
				HashSet<Action<object>> onceList = null;
				if (!_actions.TryGetValue(_type, out actionList) && !_onces.TryGetValue(_type, out onceList)) {
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
						if (firstException == null) {
							firstException = new Exception(action.Method.DeclaringType.FullName + "." + action.Method.Name, e);
						}
					}
				}
			}
			sets.Clear();
			if (firstException != null) throw firstException;
		}

		/// <summary>Update the value of <seealso cref="T"/>.</summary>
		/// <param name="getAndSet">Execute <paramref name="getAndSet"/> with the value of <seealso cref="T"/> and the <see cref="HasValue"/> result. Returns the updated value.</param>
		private void RuntimeUpdate(Func<object, bool, object> getAndSet) {
			// Add/Overwrite Value
			RuntimeSet(getAndSet(RuntimeGet(), RuntimeHasValue()));
		}

		/// <summary>Get the state of <seealso cref="T"/>.</summary>
		private bool RuntimeHasValue() {			
			// Return the State
			return _data.ContainsKey(_type);
		}

		/// <summary>Get the last known value of <seealso cref="T"/>.</summary>
		private object RuntimeGet() {			
			// Return the Value
			object value;
			_data.TryGetValue(_type, out value);
			return value;
		}

		/// <summary>Remove the last known value of <seealso cref="T"/>.</summary>
		private bool RuntimeRemove() {
			// Remove the Value
			var removed = _data.Remove(_type);

			// Broadcast the Removed Value
			RuntimeSet(null);

			// Return the Status
			return removed;
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
