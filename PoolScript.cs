using UnityEngine;

public class Node<T> where T : MonoBehaviour
{
	public T obj;
	public Node<T> next;
	
	public Node( T go )
	{
		obj = go;
		next = null;
	}
}

[System.Serializable]
public class PoolScript<T> where T : MonoBehaviour
{
	private Node<T> head = null;
	private T blueprint;
	private Vector3 hidePos = new Vector3( -1000f, 1000f, 0f );
	
	public void Populate( T blueprint, int count )
	{
		this.blueprint = blueprint;
		
        if ( count > 0 )
		{
			head = new Node<T>( NewObject() );
			
			for( int i = 1; i < count; i++ )
			{
				Node<T> curr = head;
				head = new Node<T>( NewObject() );
				head.next = curr;
			}
		}
	}
	
	public T Pop()
	{
		if( head == null )
		{
			T obj = NewObject();
			obj.gameObject.SetActive( true );
			return obj;
		}
		
		Node<T> curr = head;
		head = head.next;
		curr.obj.gameObject.SetActive( true );
		
		return curr.obj;
	}
	
	public void Push( T obj )
	{
		if( obj == null ) return;
		
		obj.transform.position = hidePos;
		obj.gameObject.SetActive( false );
		
		Node<T> newHead = new Node<T>( obj );
		newHead.next = head;
		head = newHead;
	}
	
	private T NewObject()
	{
		T obj = UnityEngine.Object.Instantiate( blueprint ) as T;
		obj.transform.position = hidePos;
		obj.gameObject.SetActive( false );
		return obj;
	}
}
