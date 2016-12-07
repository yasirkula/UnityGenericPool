using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// Generic pool class
// Author: Suleyman Yasir Kula
// Feel free to use/upgrade
public class PoolScript<T> where T : class
{
	private enum ObjectType { GameObject, Component, UnityObject, Object };

	// Head of the linked list
	private List<T> pool = null;

	// Blueprint to use for instantiation
	public T Blueprint { get; set; }

	// Some awkward place to teleport the pooled GameObject's to
	// (in addition to deactivating the GameObject)
	private Vector3 hidePos = new Vector3( -10000f, 10000f, 0f );

	// Generic type
	private ObjectType objectType;

	// A function that can be used to override default NewObject( T ) function
	private Func<T, T> createFunction;

	// Actions that can be used to implement extra logic on pushed/popped objects
	private Action<T> onPush, onPop;

	public PoolScript( Func<T, T> createFunction = null, Action<T> onPush = null, Action<T> onPop = null )
	{
		pool = new List<T>();

		this.createFunction = createFunction;
		this.onPush = onPush;
		this.onPop = onPop;

		// Simply determine the generic type:
		// it can be a Component, a GameObject, a UnityEngine.Object or a plain object
		if( typeof( T ).IsSubclassOf( typeof( Component ) ) || typeof( T ) == typeof( Component ) )
			objectType = ObjectType.Component;
		else if( typeof( T ).IsSubclassOf( typeof( GameObject ) ) || typeof( T ) == typeof( GameObject ) )
			objectType = ObjectType.GameObject;
		else if( typeof( T ).IsSubclassOf( typeof( Object ) ) || typeof( T ) == typeof( Object ) )
			objectType = ObjectType.UnityObject;
		else
			objectType = ObjectType.Object;
	}

	public PoolScript( T blueprint, Func<T, T> createFunction = null, Action<T> onPush = null, Action<T> onPop = null ) 
							: this( createFunction, onPush, onPop )
	{
		// Set the blueprint at creation
		Blueprint = blueprint;
	}

	// Populate the pool with the default blueprint
	public bool Populate( int count )
	{
		return Populate( Blueprint, count );
	}

	// Populate the pool with a specific blueprint
	public bool Populate( T blueprint, int count )
	{
		if( count <= 0 )
			return true;

		// Create a single object first to see if everything works fine
		// If not, return false
		T obj = NewObject( blueprint );
		if( obj == null )
			return false;

		Push( obj );

		// Everything works fine, populate the pool with the remaining items
		for( int i = 1; i < count; i++ )
		{
			Push( NewObject( blueprint ) );
		}

		return true;
	}

	// Fetch an item from the pool
	public T Pop()
	{
		T objToPop;

		if( pool.Count == 0 )
		{
			// Pool is empty, instantiate the blueprint

			objToPop = NewObject( Blueprint );
		}
		else
		{
			// Pool is not empty, fetch the first item in the pool

			int index = pool.Count - 1;
			objToPop = pool[index];
			pool.RemoveAt( index );
		}
		
		// If generic type is related with a GameObject, activate the gameObject
		if( objectType == ObjectType.Component )
			( objToPop as Component ).gameObject.SetActive( true );
		else if( objectType == ObjectType.GameObject )
			( objToPop as GameObject ).SetActive( true );

		if( onPop != null )
			onPop( objToPop );

		return objToPop;
	}

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;
		
		// If generic type is related with a GameObject,
		// deactivate the gameObject and teleport it to hidePos
		if( objectType == ObjectType.Component )
		{
			Component comp = obj as Component;
			comp.transform.position = hidePos;
			comp.gameObject.SetActive( false );
		}
		else if( objectType == ObjectType.GameObject )
		{
			GameObject gameObj = obj as GameObject;
			gameObj.transform.position = hidePos;
			gameObj.SetActive( false );
		}

		if( onPush != null )
			onPush( obj );

		pool.Add( obj );
	}

	// Create an instance of the blueprint and return it
	private T NewObject( T blueprint )
	{
		if( createFunction != null )
			return createFunction( blueprint );

		if( blueprint == null )
			return null;
		else if( objectType == ObjectType.Object )
		{
			if( blueprint is ICloneable )
				return (T) ( (ICloneable) blueprint ).Clone();

			return null;
		}

		T createdObj = Object.Instantiate( blueprint as Object ) as T;

		// If generic type is related with a GameObject,
		// deactivate the gameObject and teleport it to hidePos
		if( objectType == ObjectType.Component )
		{
			Component comp = createdObj as Component;
			comp.transform.position = hidePos;
			comp.gameObject.SetActive( false );
		}
		else if( objectType == ObjectType.GameObject )
		{
			GameObject gameObj = createdObj as GameObject;
			gameObj.transform.position = hidePos;
			gameObj.SetActive( false );
		}

		return createdObj;
	}
}
