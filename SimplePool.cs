using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// Generic pool class
// Author: Suleyman Yasir Kula
// Feel free to use/upgrade
public class SimplePool<T> where T : class
{
	// Objects stored in the pool
	private List<T> pool = null;

	// Blueprint to use for instantiation
	private T m_blueprint;
	private Object m_blueprintUnityObject;

	public T Blueprint {
		get
		{
			return m_blueprint;
		}
		set
		{
			m_blueprint = value;
			m_blueprintUnityObject = value as Object;
		}
	}

	// A function that can be used to override default NewObject( T ) function
	public Func<T, T> CreateFunction;

	// Actions that can be used to implement extra logic on pushed/popped objects
	public Action<T> OnPush, OnPop;

	public SimplePool( Func<T, T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null )
	{
		pool = new List<T>();

		this.CreateFunction = CreateFunction;
		this.OnPush = OnPush;
		this.OnPop = OnPop;
	}

	public SimplePool( T blueprint, Func<T, T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null )
							: this( CreateFunction, OnPush, OnPop )
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

		if( OnPop != null )
			OnPop( objToPop );

		return objToPop;
	}

	// Fetch multiple items at once from the pool
	public T[] Pop( int count )
	{
		if( count <= 0 )
			return new T[0];

		T[] result = new T[count];
		for( int i = 0; i < count; i++ )
			result[i] = Pop();

		return result;
	}

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;

		if( OnPush != null )
			OnPush( obj );

		pool.Add( obj );
	}

	// Pool multiple items at once
	public void Push( IEnumerable<T> objects )
	{
		if( objects == null ) return;

		foreach( T obj in objects )
			Push( obj );
	}

	// Clear the pool (either completely or only the null entries, if any)
	public void Clear( bool removeNullEntriesOnly = false, bool destroyRemovedObjects = true )
	{
		if( !removeNullEntriesOnly )
		{
			// Destroy the removed Objects
			if( destroyRemovedObjects )
				pool.ForEach( ( item ) => Object.Destroy( item as Object ) );

			pool.Clear();
		}
		else
			pool.RemoveAll( ( item ) => item == null );
	}

	// Create an instance of the blueprint and return it
	private T NewObject( T blueprint )
	{
		if( CreateFunction != null )
			return CreateFunction( blueprint );

		if( blueprint == null || m_blueprintUnityObject == null )
			return null;

		return Object.Instantiate( m_blueprintUnityObject ) as T;
	}
}