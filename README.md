# UnityGenericPool
A generic pooling script for Unity 3D

# How To Use
This pool can only store objects that extend MonoBehaviour (i.e. components). You can create a new pool like this: 

```C#
// The pool
PoolScript<Transform> myPool;

// Template that will be used to initialize new objects if the pool is empty when a pop is requested
public Transform prefabObject;

void Start()
{
	// Create a new pool that uses prefabObject as its template
	myPool = new PoolScript<Transform>( prefabObject );
	
	// Populate (pre-warm) the pool with 5 instances of the template object
	myPool.Populate( 5 );
}

void DoStuffWithPool()
{
	// Fetch an object from the pool
	Transform anInstance = myPool.Pop();
	
	// ...
	// Use anInstance as you wish (you may want to reposition it first, though)
	// ...
	
	// If you are done with the object, push it back into the pool (object will be disabled)
	myPool.Push( anInstance );
}
```
