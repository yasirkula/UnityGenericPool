using UnityEngine;

// A linked list node
public class Node<T> where T : Object
{
	public T obj;
	public Node<T> next;

	public Node( T go )
	{
		obj = go;
		next = null;
	}
}

// Generic pool class
// Author: Suleyman Yasir Kula
// Feel free to use/upgrade
public class PoolScript<T> where T : Object
{
	// Head of the linked list
	private Node<T> head = null;

	// Blueprint to use for instantiation
	private T blueprint = null;
	
	// Some awkward place to teleport the pooled GameObject's to
	// (in addition to deactivating the GameObject)
	private Vector3 hidePos = new Vector3( -10000f, 10000f, 0f );

	// Flags to determine the generic type
	private bool isTComponent = false;
	private bool isTGameObject = false;

	public PoolScript()
	{
		// Simply determine the generic type:
		// it can be a component, a GameObject or neither
		if( typeof( T ).IsSubclassOf( typeof( Component ) ) || typeof( T ) == typeof( Component ) )
			isTComponent = true;

		if( typeof( T ).IsSubclassOf( typeof( GameObject ) ) || typeof( T ) == typeof( GameObject ) )
			isTGameObject = true;
	}

	public PoolScript( T blueprint ) : this()
	{
		// Set the blueprint at creation
		this.blueprint = blueprint;
	}

	// Mutator for blueprint
	public void SetBlueprint( T blueprint )
	{
		this.blueprint = blueprint;
	}

	// Accessor for blueprint
	public T GetBlueprint()
	{
		return blueprint;
	}

	// Populate the pool with the default blueprint
	public void Populate( int count )
	{
		if( blueprint != null )
		{
			Populate( blueprint, count );
		}
		else
		{
			Debug.LogError( "Can't populate, no blueprint is set!" );
		}
	}

	// Populate the pool with a specific blueprint
	public void Populate( T blueprint, int count )
	{
		if( count > 0 )
		{
			head = new Node<T>( NewObject( blueprint ) );

			for( int i = 1; i < count; i++ )
			{
				Node<T> curr = head;
				head = new Node<T>( NewObject( blueprint ) );
				head.next = curr;
			}
		}
	}

	// Fetch an item from the pool
	public T Pop()
	{
		T objToPop;

		if( head == null )
		{
			// Pool is empty, instantiate the blueprint

			if( blueprint == null )
			{
				Debug.LogError( "Can't pop object: no blueprint is set and the pool is empty!" );
				return null;
			}

			objToPop = NewObject( blueprint );
		}
		else
		{
			// Pool is not empty, fetch the first item in the pool

			Node<T> curr = head;
			head = head.next;
			objToPop = curr.obj;
		}

		// If generic type is related with a GameObject, activate the gameObject
		if( isTComponent )
			( objToPop as Component ).gameObject.SetActive( true );
		else if( isTGameObject )
			( objToPop as GameObject ).SetActive( true );

		return objToPop;
	}

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;

		// If generic type is related with a GameObject,
		// deactivate the gameObject and teleport it to hidePos
		if( isTComponent )
		{
			Component comp = obj as Component;
			comp.transform.position = hidePos;
			comp.gameObject.SetActive( false );
		}
		else if( isTGameObject )
		{
			GameObject gameObj = obj as GameObject;
			gameObj.transform.position = hidePos;
			gameObj.SetActive( false );
		}

		Node<T> newHead = new Node<T>( obj );
		newHead.next = head;
		head = newHead;
	}

	// Create an instance of the blueprint and return it
	private T NewObject( T blueprint )
	{
		T createdObj = Object.Instantiate( blueprint ) as T;

		// If generic type is related with a GameObject,
		// deactivate the gameObject and teleport it to hidePos
		if( isTComponent )
		{
			Component comp = createdObj as Component;
			comp.transform.position = hidePos;
			comp.gameObject.SetActive( false );
		}
		else if( isTGameObject )
		{
			GameObject gameObj = createdObj as GameObject;
			gameObj.transform.position = hidePos;
			gameObj.SetActive( false );
		}

		return createdObj;
	}
}
