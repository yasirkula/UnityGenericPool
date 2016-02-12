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
	// Create a new pool
	myPool = new PoolScript<Transform>();
	
	// Give the prefabObject as template to the pool but do not initially populate the pool (0)
	// if you change 0 to 5, pool will be initialized with 5 instances of the template object available
	myPool.Populate( prefabObject, 0 );
}

void DoStuffWithPool()
{
	// Fetch an object from the pool
	Transform anInstance = myPool.Pop();
	
	// ...
	// Reposition anInstance to where you want it to be and do stuff with it
	// ...
	
	// If you are done with the object, push the object back into the pool (object will be disabled)
	myPool.Push( anInstance );
}
```
